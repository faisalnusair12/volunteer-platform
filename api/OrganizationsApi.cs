using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class OrganizationsApi
{
    public static void MapOrganizationEndpoints(this WebApplication app)
    {
        // GET /organizations
        app.MapGet("/organizations", async (AppDbContext db) =>
        {
            var orgs = await db.Organizations
                .Include(o => o.Tasks)
                .Include(o => o.Reviews)
                .ToListAsync();

            return Results.Ok(orgs.Select(o => new
            {
                o.Id,
                o.Name,
                o.Email,
                o.PhoneNumber,
                o.LogoUrl,
                o.BannerUrl,
                o.Description,
                o.CreatedAt,
                TaskCount = o.Tasks.Count,
                AverageRating = o.Reviews.Count > 0 ? Math.Round(o.Reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = o.Reviews.Count
            }));
        });

        // GET /organizations/{id}
        app.MapGet("/organizations/{id}", async (int id, AppDbContext db) =>
        {
            var org = await db.Organizations
                .Include(o => o.Tasks)
                .Include(o => o.Reviews)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (org is null) return Results.NotFound(new { message = "Organization not found" });

            return Results.Ok(new
            {
                org.Id,
                org.Name,
                org.Email,
                org.PhoneNumber,
                org.LogoUrl,
                org.BannerUrl,
                org.Description,
                org.CreatedAt,
                TaskCount = org.Tasks.Count,
                AverageRating = org.Reviews.Count > 0 ? Math.Round(org.Reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = org.Reviews.Count,
                Tasks = org.Tasks.Select(t => new { t.Id, t.Title, t.Description, t.Status, t.StartDate, t.EndDate, t.MaxVolunteers })
            });
        });

        // PUT /organizations/{id}
        app.MapPut("/organizations/{id}", async (int id, Organization input, AppDbContext db) =>
        {
            var org = await db.Organizations.FindAsync(id);
            if (org is null) return Results.NotFound(new { message = "Organization not found" });

            org.Name = input.Name;
            org.PhoneNumber = input.PhoneNumber;
            org.LogoUrl = input.LogoUrl;
            org.BannerUrl = input.BannerUrl;
            org.Description = input.Description;

            await db.SaveChangesAsync();
            return Results.Ok(org);
        });

        // DELETE /organizations/{id}
        app.MapDelete("/organizations/{id}", async (int id, AppDbContext db) =>
        {
            var org = await db.Organizations.FindAsync(id);
            if (org is null) return Results.NotFound(new { message = "Organization not found" });

            db.Organizations.Remove(org);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
