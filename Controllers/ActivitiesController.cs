using System.Security.Claims;
using BenchApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BenchApp.Controllers;

// Activity Entry Page - activities are scoped to the logged in user's specialty
[ApiController]
[Route("api/activities")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ActivitiesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyAreaActivities()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var user = await _db.Users.FindAsync(userId);

        if (user?.SpecialtyId == null)
        {
            return Ok(Array.Empty<object>());
        }

        var activities = await _db.Activities
            .Where(a => a.SpecialtyId == user.SpecialtyId)
            .Select(a => new { a.Id, a.Title, a.Description })
            .ToListAsync();

        return Ok(activities);
    }
}
