using System.Security.Claims;
using BenchApp.Data;
using BenchApp.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BenchApp.Controllers;

// BROJ4: User Profile Page - "My Profile" data
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

        var user = await _db.Users
            .Include(u => u.Specialty)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "Profile not found.", statusCode: 404);
        }

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.Direction,
            user.Bio,
            user.IsProfileComplete,
            Role = user.Role?.Name,
            Specialty = user.Specialty == null ? null : new
            {
                user.Specialty.Id,
                user.Specialty.Name,
                user.Specialty.MentorName,
                user.Specialty.MentorEmail
            }
        });
    }
}
