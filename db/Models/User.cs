namespace backend.db.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "student"; // "student" | "admin"
    public string? UniversityId { get; set; }
    public bool TakingVolunteeringCourse { get; set; } = false;
    public string? ProfilePictureUrl { get; set; }
    public bool IsBlocked { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<TaskVolunteer> TaskVolunteers { get; set; } = new List<TaskVolunteer>();
    public ICollection<VolunteerHour> VolunteerHours { get; set; } = new List<VolunteerHour>();
    public ICollection<OrganizationReview> OrganizationReviews { get; set; } = new List<OrganizationReview>();
}
