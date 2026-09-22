using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using NotesApplication.DTOs;
using NotesApplication.Models;
using NotesApplication.Services;

namespace NotesApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDbConnection _db;
    private readonly JwtService _jwtService;

    public AuthController(IDbConnection db, JwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register()
    {
        var (username, email, password) = await ExtractRegisterDataAsync();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(new { message = "Username, email, and password are required." });
        }

        username = username.Trim();
        email = email.Trim();

        if (username.Length < 3 || username.Length > 15)
        {
            return BadRequest(new { message = "Username must be between 3 and 15 characters." });
        }

        if (email.Length > 50)
        {
            return BadRequest(new { message = "Email cannot exceed 50 characters." });
        }

        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            return BadRequest(new { message = "Invalid email address format." });
        }

        if (password.Length < 6 || password.Length > 100)
        {
            return BadRequest(new { message = "Password must be between 6 and 100 characters." });
        }

        try
        {
            const string checkSql = "SELECT COUNT(1) FROM Users WHERE username = @Username OR email = @Email";
            var exists = await _db.ExecuteScalarAsync<int>(checkSql, new { Username = username, Email = email });
            if (exists > 0)
            {
                return Conflict(new { message = "Username or Email already exists." });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            const string insertSql = @"
                INSERT INTO Users (username, email, password_hash)
                OUTPUT INSERTED.id, INSERTED.username, INSERTED.email, INSERTED.created_at
                VALUES (@Username, @Email, @PasswordHash);";

            var newUser = await _db.QuerySingleAsync<User>(insertSql, new
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash
            });

            var token = _jwtService.GenerateToken(newUser);

            return Created($"/api/users/{newUser.Id}", new
            {
                message = "User registered successfully",
                token = token,
                user = new
                {
                    newUser.Id,
                    newUser.Username,
                    newUser.Email,
                    newUser.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Register Error");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var (username, password) = await ExtractLoginDataAsync();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(new { message = "username (or email) and password are required." });
        }

        try
        {
            const string sql = "SELECT * FROM Users WHERE username = @Username OR email = @Username";
            var user = await _db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username/email or password." });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500, title: "Login Error");
        }
    }

    private async Task<(string? Username, string? Email, string? Password)> ExtractRegisterDataAsync()
    {
        string? username = null;
        string? email = null;
        string? password = null;

        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync();
            username = form["username"].FirstOrDefault();
            email = form["email"].FirstOrDefault();
            password = form["password"].FirstOrDefault();
        }
        else if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                var body = await Request.ReadFromJsonAsync<RegisterRequest>();
                if (body != null)
                {
                    username = body.Username;
                    email = body.Email;
                    password = body.Password;
                }
            }
            catch { }
        }

        username ??= Request.Query["username"].FirstOrDefault();
        email ??= Request.Query["email"].FirstOrDefault();
        password ??= Request.Query["password"].FirstOrDefault();

        return (username, email, password);
    }

    private async Task<(string? Username, string? Password)> ExtractLoginDataAsync()
    {
        string? username = null;
        string? password = null;

        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync();
            username = form["username"].FirstOrDefault()
                       ?? form["name"].FirstOrDefault()
                       ?? form["email"].FirstOrDefault()
                       ?? form["usernameOrEmail"].FirstOrDefault();
            password = form["password"].FirstOrDefault();
        }
        else if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                var body = await Request.ReadFromJsonAsync<LoginRequest>();
                if (body != null)
                {
                    username = body.Username;
                    password = body.Password;
                }
            }
            catch { }
        }

        username ??= Request.Query["username"].FirstOrDefault()
                     ?? Request.Query["name"].FirstOrDefault()
                     ?? Request.Query["email"].FirstOrDefault()
                     ?? Request.Query["usernameOrEmail"].FirstOrDefault();
        password ??= Request.Query["password"].FirstOrDefault();

        return (username, password);
    }
}
