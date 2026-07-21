using BenchApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BenchApp.Controllers;

// BE Provide controlled reference-data endpoints (Broj4 subtask 2)
[ApiController]
[Route("api/reference-data")]
public class ReferenceDataController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReferenceDataController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("specialties")]
    public async Task<IActionResult> GetSpecialties()
    {
        var specialties = await _db.Specialties
            .Select(s => new { s.Id, s.Name, s.MentorName, s.MentorEmail })
            .ToListAsync();

        return Ok(specialties);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
        return Ok(roles);
    }
}
