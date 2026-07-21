using BenchApp.Data;
using BenchApp.DTOs;
using BenchApp.Middleware;
using BenchApp.Models;
using BenchApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BenchApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthController(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    // BROJ1 step 1: create user profile and api persistence (full name, email, password)
    [HttpPost("register/step1")]
    public async Task<ActionResult<AuthResponseDto>> RegisterStep1(RegisterStep1Dto dto)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existing != null)
        {
            // SANDRA - domain-specific error mapped to the shared contract, field-level so the FE
            // highlights the email input directly (2.1)
            throw new ApiException(
                ErrorCodes.EmailAlreadyExists,
                "An account with this email already exists.",
                statusCode: 409,
                fieldErrors: new() { new FieldError { Field = "email", Message = "This email is already registered." } });
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = 3, // Practicant by default
            IsProfileComplete = false
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var role = await _db.Roles.FindAsync(user.RoleId);
        var token = _jwt.GenerateAccessToken(user, role?.Name ?? "Practicant");
        var refreshToken = await IssueRefreshToken(user.Id);

        return Ok(new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = user.FullName,
            IsProfileComplete = user.IsProfileComplete,
            Role = role?.Name ?? "Practicant"
        });
    }

    // BROJ1 step 2: specialty / oblast / nasoka, then redirect to home page (FE side)
    [HttpPost("register/step2")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> RegisterStep2(RegisterStep2Dto dto)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "User not found.", statusCode: 404);
        }

        var specialty = await _db.Specialties.FindAsync(dto.SpecialtyId);
        if (specialty == null)
        {
            // SANDRA - field error pointing at the specialty dropdown specifically (2.1)
            throw new ApiException(
                ErrorCodes.ValidationError,
                "Selected specialty does not exist.",
                statusCode: 400,
                fieldErrors: new() { new FieldError { Field = "specialtyId", Message = "Please choose a valid specialty." } });
        }

        user.SpecialtyId = dto.SpecialtyId;
        user.Direction = dto.Direction;
        user.Bio = dto.Bio;
        user.IsProfileComplete = true;

        await _db.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            IsProfileComplete = true,
            Role = user.Role?.Name ?? "Practicant"
        });
    }

    // BROJ2: User Login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            // SANDRA - deliberately generic (doesn't reveal whether email exists), not retryable (2.2/2.3)
            throw new ApiException(
                ErrorCodes.InvalidCredentials,
                "Email or password is incorrect.",
                statusCode: 401,
                retryable: false);
        }

        var token = _jwt.GenerateAccessToken(user, user.Role?.Name ?? "Practicant");
        var refreshToken = await IssueRefreshToken(user.Id);

        return Ok(new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = user.FullName,
            IsProfileComplete = user.IsProfileComplete,
            Role = user.Role?.Name ?? "Practicant"
        });
    }

    // BROJ3: User Logout - revoke refresh token, FE deletes access token + redirects to login
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);
        if (token != null)
        {
            token.Revoked = true;
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Logged out successfully." });
    }

    private async Task<string> IssueRefreshToken(int userId)
    {
        var refreshToken = _jwt.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await _db.SaveChangesAsync();
        return refreshToken;
    }
}

public class LogoutDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
