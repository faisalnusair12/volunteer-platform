using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class ReviewsApi
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        // GET /reviews/organization/{orgId}
        app.MapGet("/reviews/organization/{orgId}", async (int orgId, AppDbContext db) =>
            Results.Ok(await db.OrganizationReviews
                .Include(r => r.User)
                .Where(r => r.OrganizationId == orgId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.OrganizationId,
                    r.UserId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    UserName = r.User.FullName,
                    UserPicture = r.User.ProfilePictureUrl
                })
                .ToListAsync()));

        // GET /reviews/organization/{orgId}/average
        app.MapGet("/reviews/organization/{orgId}/average", async (int orgId, AppDbContext db) =>
        {
            var reviews = await db.OrganizationReviews
                .Where(r => r.OrganizationId == orgId)
                .ToListAsync();

            if (reviews.Count == 0) return Results.Ok(new { average = 0.0, count = 0 });

            return Results.Ok(new
            {
                average = Math.Round(reviews.Average(r => r.Rating), 1),
                count = reviews.Count
            });
        });

        // POST /reviews
        app.MapPost("/reviews", async (OrganizationReview review, AppDbContext db) =>
        {
            if (review.OrganizationId <= 0) return Results.BadRequest(new { message = "OrganizationId is required" });
            if (review.UserId <= 0) return Results.BadRequest(new { message = "UserId is required" });
            if (review.Rating < 1 || review.Rating > 5) return Results.BadRequest(new { message = "Rating must be between 1 and 5" });

            // Check if user already reviewed this organization
            var existing = await db.OrganizationReviews
                .FirstOrDefaultAsync(r => r.OrganizationId == review.OrganizationId && r.UserId == review.UserId);
            if (existing != null)
                return Results.BadRequest(new { message = "You have already reviewed this organization" });

            db.OrganizationReviews.Add(review);
            await db.SaveChangesAsync();
            return Results.Created($"/reviews/{review.Id}", review);
        });

        // DELETE /reviews/{id}
        app.MapDelete("/reviews/{id}", async (int id, AppDbContext db) =>
        {
            var review = await db.OrganizationReviews.FindAsync(id);
            if (review is null) return Results.NotFound(new { message = "Review not found" });

            db.OrganizationReviews.Remove(review);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
