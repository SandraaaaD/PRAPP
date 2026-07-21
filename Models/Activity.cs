namespace BenchApp.Models;

// Activities filtered by user's specialty area
public class Activity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Description { get; set; } = string.Empty;
    public string Description2 { get; set; } = string.Empty;
    public int SpecialtyId { get; set; }
    public Specialty? Specialty { get; set; }
}
