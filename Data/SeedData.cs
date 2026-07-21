using BenchApp.Models;

namespace BenchApp.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (!db.Roles.Any())
        {
            // Populate roles in database - RBAC
            db.Roles.AddRange(
                new Role { Id = 1, Name = "Admin", Permissions = new() { "*" } },
                new Role { Id = 2, Name = "Mentor", Permissions = new() { "view_team_stats", "manage_activities" } },
                new Role { Id = 3, Name = "Practicant", Permissions = new() { "view_own_stats", "submit_activity" } }
            );
        }

        if (!db.Specialties.Any())
        {
            var fe = new Specialty { Id = 1, Name = "Frontend Development", MentorName = "Ana Petrovska", MentorEmail = "ana.petrovska@bench.mk" };
            var be = new Specialty { Id = 2, Name = "Backend Development", MentorName = "Marko Ilievski", MentorEmail = "marko.ilievski@bench.mk" };
            var qa = new Specialty { Id = 3, Name = "Quality Assurance", MentorName = "Elena Trajkova", MentorEmail = "elena.trajkova@bench.mk" };
            var devops = new Specialty { Id = 4, Name = "DevOps", MentorName = "Filip Naumov", MentorEmail = "filip.naumov@bench.mk" };

            db.Specialties.AddRange(fe, be, qa, devops);

            db.Activities.AddRange(
                new Activity { Id = 1, Title = "React component basics", Description = "Build reusable components", SpecialtyId = 1 },
                new Activity { Id = 2, Title = "State management workshop", Description = "Context API and hooks", SpecialtyId = 1 },
                new Activity { Id = 3, Title = "REST API design", Description = "Design a clean REST contract", SpecialtyId = 2 },
                new Activity { Id = 4, Title = "EF Core migrations", Description = "Practice schema migrations", SpecialtyId = 2 },
                new Activity { Id = 5, Title = "Manual test case design", Description = "Write test cases for a login flow", SpecialtyId = 3 },
                new Activity { Id = 6, Title = "CI pipeline basics", Description = "Set up a build pipeline", SpecialtyId = 4 }
            );
        }

        db.SaveChanges();
    }
}
