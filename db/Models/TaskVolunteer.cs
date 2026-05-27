namespace backend.db.Models;

public class TaskVolunteer
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = "pending"; // "pending" | "approved" | "rejected"
    public DateTime JoinedAt { get; set; }

    // Navigation
    public Task Task { get; set; } = null!;
    public User User { get; set; } = null!;
}
