using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;
using TaskModel = backend.db.Models.Task;

namespace backend.api;

public static class TasksApi
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        // GET /tasks
        app.MapGet("/tasks", async (AppDbContext db) =>
            Results.Ok(await db.Tasks
                .Include(t => t.Organization)
                .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
                .Include(t => t.TaskImages)
                .ToListAsync()));

        // GET /tasks/{id}
        app.MapGet("/tasks/{id}", async (int id, AppDbContext db) =>
            await db.Tasks
                .Include(t => t.Organization)
                .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
                .Include(t => t.TaskImages)
                .FirstOrDefaultAsync(t => t.Id == id) is TaskModel task
                ? Results.Ok(task)
                : Results.NotFound(new { message = "Task not found" }));

        // GET /tasks/open
        app.MapGet("/tasks/open", async (AppDbContext db) =>
            Results.Ok(await db.Tasks
                .Include(t => t.Organization)
                .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
                .Include(t => t.TaskImages)
                .Where(t => t.Status == "open")
                .ToListAsync()));

        // POST /tasks
        app.MapPost("/tasks", async (TaskCreateRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Title)) return Results.BadRequest(new { message = "Title is required" });
            if (req.OrganizationId <= 0) return Results.BadRequest(new { message = "OrganizationId is required" });

            // Date validation: start must precede end
            if (req.StartDate.HasValue && req.EndDate.HasValue && req.StartDate.Value >= req.EndDate.Value)
                return Results.BadRequest(new { message = "Start date must be before end date" });

            var task = new TaskModel
            {
                OrganizationId = req.OrganizationId,
                Title = req.Title,
                Description = req.Description,
                MaxVolunteers = req.MaxVolunteers,
                StartDate = req.StartDate,
                EndDate = req.EndDate
            };

            db.Tasks.Add(task);
            await db.SaveChangesAsync();

            if (req.TagIds != null && req.TagIds.Any())
            {
                foreach (var tagId in req.TagIds)
                {
                    if (await db.Tags.AnyAsync(t => t.Id == tagId))
                    {
                        db.TaskTags.Add(new TaskTag { TaskId = task.Id, TagId = tagId });
                    }
                }
                await db.SaveChangesAsync();
            }

            return Results.Created($"/tasks/{task.Id}", task);
        });

        // PUT /tasks/{id}
        app.MapPut("/tasks/{id}", async (int id, TaskModel input, AppDbContext db) =>
        {
            var task = await db.Tasks.FindAsync(id);
            if (task is null) return Results.NotFound(new { message = "Task not found" });

            // Date validation: start must precede end
            if (input.StartDate.HasValue && input.EndDate.HasValue && input.StartDate.Value >= input.EndDate.Value)
                return Results.BadRequest(new { message = "Start date must be before end date" });

            task.Title = input.Title;
            task.Description = input.Description;
            task.MaxVolunteers = input.MaxVolunteers;
            task.Status = input.Status;
            task.StartDate = input.StartDate;
            task.EndDate = input.EndDate;

            await db.SaveChangesAsync();
            return Results.Ok(task);
        });

        // DELETE /tasks/{id}
        app.MapDelete("/tasks/{id}", async (int id, AppDbContext db) =>
        {
            var task = await db.Tasks.FindAsync(id);
            if (task is null) return Results.NotFound(new { message = "Task not found" });

            db.Tasks.Remove(task);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public class TaskCreateRequest
{
    public int OrganizationId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? MaxVolunteers { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<int>? TagIds { get; set; }
}
