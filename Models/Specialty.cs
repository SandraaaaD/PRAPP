namespace BenchApp.Models;

// BE Provide controlled reference-data endpoints - oblast + mentor
public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Frontend Development"
    public string MentorName { get; set; } = string.Empty; // Sekoja oblast ima svoj mentor
    public string MentorEmail { get; set; } = string.Empty;
    public List<Activity> Activities { get; set; } = new();
}
