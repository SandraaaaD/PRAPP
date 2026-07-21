namespace BenchApp.Models;

// Design Role Based Access Control system - roles seeded in DB
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Admin, Mentor, Practicant
    public List<string> Permissions { get; set; } = new();
}
