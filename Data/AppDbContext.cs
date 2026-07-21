using BenchApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BenchApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<DailyStat> DailyStats => Set<DailyStat>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
