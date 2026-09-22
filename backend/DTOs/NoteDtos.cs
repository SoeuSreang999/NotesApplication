using System.ComponentModel.DataAnnotations;

namespace NotesApplication.DTOs;

public record CreateNoteRequest(
    [Required(ErrorMessage = "Title is mandatory.")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
    string Title, 
    string? Content
);

public record UpdateNoteRequest(
    [Required(ErrorMessage = "Title is mandatory.")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
    string Title, 
    string? Content
);

public class NoteDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}

public class PaginatedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasMore => Page < TotalPages;
}
