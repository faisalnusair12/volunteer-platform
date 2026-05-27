namespace backend.db.Models;

public class OrganizationReview
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }          // 1–5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public User User { get; set; } = null!;
}
