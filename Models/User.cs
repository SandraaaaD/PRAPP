namespace BenchApp.Models;

// Broj1/Broj2 - Registration model (BE Registration model / User Profile API and Persistence)
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Step 2 of registration - specialty/area info
    public int? SpecialtyId { get; set; }
    public Specialty? Specialty { get; set; }
    public string? Direction { get; set; } // "nasoka" - e.g. Frontend/Backend/QA within a specialty
    public string? Bio { get; set; }

    // Role Based Access Control
    public int RoleId { get; set; } = 3; // default = Practicant
    public Role? Role { get; set; }

    public bool IsProfileComplete { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<DailyStat> DailyStats { get; set; } = new();
}
