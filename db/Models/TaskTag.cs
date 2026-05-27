namespace backend.db.Models;

public class TaskTag
{
    public int TaskId { get; set; }
    public int TagId { get; set; }

    // Navigation
    public Task Task { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
