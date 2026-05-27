namespace backend.db.Models;

public class TaskImage
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime UploadedAt { get; set; }

    // Navigation
    public Task Task { get; set; } = null!;
}
