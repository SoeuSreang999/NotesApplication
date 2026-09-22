using System.Data;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApplication.DTOs;
using NotesApplication.Models;

namespace NotesApplication.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly IDbConnection _db;

    public NotesController(IDbConnection db)
    {
        _db = db;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Invalid or missing user ID claim.");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            var baseWhere = "WHERE user_id = @UserId AND deleted_at IS NULL";
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                baseWhere += " AND (title LIKE @Search OR content LIKE @Search)";
                parameters.Add("Search", $"%{search.Trim()}%");
            }

            var countSql = $"SELECT COUNT(1) FROM Notes {baseWhere}";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql, parameters);

            var sql = $@"
                SELECT id, user_id, title, content, created_at, updated_at, updated_by
                FROM Notes
                {baseWhere}";

            sql += sort switch
            {
                "oldest" => " ORDER BY created_at ASC",
                "title_asc" => " ORDER BY title ASC",
                "title_desc" => " ORDER BY title DESC",
                "updated" => " ORDER BY COALESCE(updated_at, created_at) DESC",
                _ => " ORDER BY created_at DESC"
            };

            sql += @"
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var notes = await _db.QueryAsync<NoteDto>(sql, parameters);

            var result = new PaginatedResponse<NoteDto>
            {
                Data = notes,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Failed to fetch notes");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();

            const string sql = @"
                SELECT id, user_id, title, content, created_at, updated_at, updated_by
                FROM Notes
                WHERE id = @Id AND user_id = @UserId AND deleted_at IS NULL";

            var note = await _db.QueryFirstOrDefaultAsync<NoteDto>(sql, new { Id = id, UserId = userId });
            if (note == null)
            {
                return NotFound(new { message = "Note not found." });
            }

            return Ok(note);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Failed to fetch note");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
        {
            return BadRequest(new { message = "Title is mandatory." });
        }

        try
        {
            var userId = GetCurrentUserId();

            const string sql = @"
                INSERT INTO Notes (user_id, title, content, created_at)
                OUTPUT INSERTED.id, INSERTED.user_id, INSERTED.title, INSERTED.content, INSERTED.created_at, INSERTED.updated_at
                VALUES (@UserId, @Title, @Content, SYSDATETIMEOFFSET());";

            var newNote = await _db.QuerySingleAsync<NoteDto>(sql, new
            {
                UserId = userId,
                Title = req.Title.Trim(),
                Content = req.Content
            });

            return Created($"/api/notes/{newNote.Id}", newNote);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Failed to create note");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNoteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
        {
            return BadRequest(new { message = "Title is mandatory." });
        }

        try
        {
            var userId = GetCurrentUserId();

            const string checkSql = @"
                SELECT COUNT(1) FROM Notes 
                WHERE id = @Id AND user_id = @UserId AND deleted_at IS NULL";

            var exists = await _db.ExecuteScalarAsync<int>(checkSql, new { Id = id, UserId = userId });
            if (exists == 0)
            {
                return NotFound(new { message = "Note not found or you don't have permission to edit it." });
            }

            const string updateSql = @"
                UPDATE Notes
                SET title = @Title,
                    content = @Content,
                    updated_at = SYSDATETIMEOFFSET(),
                    updated_by = @UserId
                OUTPUT INSERTED.id, INSERTED.user_id, INSERTED.title, INSERTED.content, INSERTED.created_at, INSERTED.updated_at, INSERTED.updated_by
                WHERE id = @Id AND user_id = @UserId;";

            var updatedNote = await _db.QuerySingleAsync<NoteDto>(updateSql, new
            {
                Id = id,
                UserId = userId,
                Title = req.Title.Trim(),
                Content = req.Content
            });

            return Ok(updatedNote);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Failed to update note");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();

            const string checkSql = @"
                SELECT COUNT(1) FROM Notes 
                WHERE id = @Id AND user_id = @UserId AND deleted_at IS NULL";

            var exists = await _db.ExecuteScalarAsync<int>(checkSql, new { Id = id, UserId = userId });
            if (exists == 0)
            {
                return NotFound(new { message = "Note not found or you don't have permission to delete it." });
            }

            const string deleteSql = @"
                UPDATE Notes
                SET deleted_at = SYSDATETIMEOFFSET(),
                    deleted_by = @UserId
                WHERE id = @Id AND user_id = @UserId;";

            await _db.ExecuteAsync(deleteSql, new { Id = id, UserId = userId });

            return Ok(new { message = "Note deleted successfully.", id });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Failed to delete note");
        }
    }
}
