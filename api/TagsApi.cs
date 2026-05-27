using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class TagsApi
{
    public static void MapTagEndpoints(this WebApplication app)
    {
        // GET /tags
        app.MapGet("/tags", async (AppDbContext db) =>
            Results.Ok(await db.Tags.ToListAsync()));

        // POST /tags
        app.MapPost("/tags", async (Tag tag, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(tag.Name)) return Results.BadRequest(new { message = "Name is required" });

            if (await db.Tags.AnyAsync(t => t.Name == tag.Name))
                return Results.BadRequest(new { message = "Tag already exists" });

            db.Tags.Add(tag);
            await db.SaveChangesAsync();
            return Results.Created($"/tags/{tag.Id}", tag);
        });
        
        // DELETE /tags/{id}
        app.MapDelete("/tags/{id}", async (int id, AppDbContext db) =>
        {
            var tag = await db.Tags.FindAsync(id);
            if (tag is null) return Results.NotFound(new { message = "Tag not found" });

            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
