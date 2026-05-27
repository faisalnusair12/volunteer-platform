namespace backend.db.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
