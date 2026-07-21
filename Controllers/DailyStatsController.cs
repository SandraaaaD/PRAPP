using System.Security.Claims;
using BenchApp.Data;
using BenchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BenchApp.Controllers;

// Dashboard layout - daily stats per user
[ApiController]
[Route("api/daily-stats")]
[Authorize]
public class DailyStatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DailyStatsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyStats()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

        var stats = await _db.DailyStats
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return Ok(stats);
    }

    [HttpPost]
    public async Task<IActionResult> AddStat(DailyStatDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

        var stat = new DailyStat
        {
            UserId = userId,
            Date = dto.Date,
            ActivitiesCompleted = dto.ActivitiesCompleted,
            HoursLogged = dto.HoursLogged,
            Note = dto.Note
        };

        _db.DailyStats.Add(stat);
        await _db.SaveChangesAsync();

        return Ok(stat);
    }
}

public class DailyStatDto
{
    public DateOnly Date { get; set; }
    public int ActivitiesCompleted { get; set; }
    public int HoursLogged { get; set; }
    public string? Note { get; set; }
}
