using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class TaskImagesApi
{
    public static void MapTaskImageEndpoints(this WebApplication app)
    {
        // GET /tasks/{taskId}/images
        app.MapGet("/tasks/{taskId}/images", async (int taskId, AppDbContext db) =>
            Results.Ok(await db.TaskImages
                .Where(img => img.TaskId == taskId)
                .ToListAsync()));

        // POST /tasks/{taskId}/images
        app.MapPost("/tasks/{taskId}/images", async (int taskId, TaskImage img, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(img.ImageUrl)) return Results.BadRequest(new { message = "ImageUrl is required" });
            
            var task = await db.Tasks.FindAsync(taskId);
            if (task is null) return Results.NotFound(new { message = "Task not found" });

            img.TaskId = taskId;
            db.TaskImages.Add(img);
            await db.SaveChangesAsync();
            
            return Results.Created($"/tasks/{taskId}/images/{img.Id}", img);
        });

        // DELETE /tasks/images/{id}
        app.MapDelete("/tasks/images/{id}", async (int id, AppDbContext db) =>
        {
            var img = await db.TaskImages.FindAsync(id);
            if (img is null) return Results.NotFound(new { message = "Image not found" });

            db.TaskImages.Remove(img);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
