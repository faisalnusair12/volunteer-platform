namespace backend.db.Models;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }

    // Navigation
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
    public ICollection<OrganizationReview> Reviews { get; set; } = new List<OrganizationReview>();
}
