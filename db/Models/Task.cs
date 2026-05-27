namespace backend.db.Models;

public class Task
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? MaxVolunteers { get; set; }
    public string Status { get; set; } = "open";        // "open" | "closed" | "done"
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<TaskVolunteer> TaskVolunteers { get; set; } = new List<TaskVolunteer>();
    public ICollection<VolunteerHour> VolunteerHours { get; set; } = new List<VolunteerHour>();
    public ICollection<TaskImage> TaskImages { get; set; } = new List<TaskImage>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
