namespace BenchApp.Models;

// Dashboard layout - daily stats per user
public class DailyStat
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public int ActivitiesCompleted { get; set; }
    public int HoursLogged { get; set; }
    public string? Note { get; set; }
}

// JWT Token - refresh token storage, also used on Logout to revoke
public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; } = false;
}
