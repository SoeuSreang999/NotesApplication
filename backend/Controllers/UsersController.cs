using System.Data;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApplication.DTOs;
using NotesApplication.Models;

namespace NotesApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IDbConnection _db;

    public UsersController(IDbConnection db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _db.QueryAsync<User>("SELECT id, username, email, created_at FROM Users");
            return Ok(users);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Database Error");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            const string sql = "SELECT id, username, email, created_at FROM Users WHERE id = @Id";
            var user = await _db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Database Error");
        }
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        try
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(req.Username) && string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { message = "Username or password must be provided." });
            }

            if (!string.IsNullOrWhiteSpace(req.Username))
            {
                const string checkSql = "SELECT COUNT(1) FROM Users WHERE username = @Username AND id != @UserId";
                var exists = await _db.ExecuteScalarAsync<int>(checkSql, new { Username = req.Username.Trim(), UserId = userId });
                if (exists > 0)
                {
                    return Conflict(new { message = "Username is already taken." });
                }
            }

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            var setClauses = new List<string>();

            if (!string.IsNullOrWhiteSpace(req.Username))
            {
                setClauses.Add("username = @Username");
                parameters.Add("Username", req.Username.Trim());
            }

            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                if (req.Password.Length < 6)
                {
                    return BadRequest(new { message = "Password must be at least 6 characters." });
                }
                var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
                setClauses.Add("password_hash = @PasswordHash");
                parameters.Add("PasswordHash", hash);
            }

            var sql = $@"
                UPDATE Users
                SET {string.Join(", ", setClauses)}
                OUTPUT INSERTED.id, INSERTED.username, INSERTED.email, INSERTED.created_at
                WHERE id = @UserId";

            var updatedUser = await _db.QuerySingleAsync<User>(sql, parameters);

            return Ok(new
            {
                message = "Profile updated successfully.",
                user = new
                {
                    updatedUser.Id,
                    updatedUser.Username,
                    updatedUser.Email,
                    updatedUser.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Update Profile Error");
        }
    }
}
