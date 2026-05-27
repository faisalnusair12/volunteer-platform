namespace backend.db.Models;

public class VolunteerHour
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; }

    // Navigation
    public Task Task { get; set; } = null!;
    public User User { get; set; } = null!;
}
