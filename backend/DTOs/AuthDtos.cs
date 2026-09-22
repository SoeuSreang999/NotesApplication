namespace NotesApplication.DTOs;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string Username, string Password);

public record UpdateProfileRequest(string? Username, string? Password);
