using System.ComponentModel.DataAnnotations;

namespace NotesApplication.DTOs;

public record RegisterRequest(
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(15, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 15 characters.")]
    string Username,

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]
    string Email,

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    string Password
);

public record LoginRequest(string Username, string Password);

public record UpdateProfileRequest(
    [StringLength(15, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 15 characters.")]
    string? Username,

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    string? Password,

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]
    string? Email = null
);
