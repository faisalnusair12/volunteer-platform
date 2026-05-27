using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class HoursApi
{
    public static void MapHoursEndpoints(this WebApplication app)
    {
        // GET /hours
        app.MapGet("/hours", async (AppDbContext db) =>
            Results.Ok(await db.VolunteerHours
                .Include(h => h.Task)
                .Include(h => h.User)
                .ToListAsync()));

        // GET /hours/{id}
        app.MapGet("/hours/{id}", async (int id, AppDbContext db) =>
            await db.VolunteerHours
                .Include(h => h.Task)
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.Id == id) is VolunteerHour hour
                ? Results.Ok(hour)
                : Results.NotFound(new { message = "VolunteerHour not found" }));

        // GET /hours/task/{taskId}
        app.MapGet("/hours/task/{taskId}", async (int taskId, AppDbContext db) =>
            Results.Ok(await db.VolunteerHours
                .Include(h => h.User)
                .Where(h => h.TaskId == taskId)
                .ToListAsync()));
                
        // GET /hours/user/{userId}
        app.MapGet("/hours/user/{userId}", async (int userId, AppDbContext db) =>
            Results.Ok(await db.VolunteerHours
                .Include(h => h.Task)
                .Where(h => h.UserId == userId)
                .ToListAsync()));

        // POST /hours
        app.MapPost("/hours", async (VolunteerHour hour, AppDbContext db) =>
        {
            if (hour.TaskId <= 0) return Results.BadRequest(new { message = "TaskId is required" });
            if (hour.UserId <= 0) return Results.BadRequest(new { message = "UserId is required" });
            if (hour.HoursWorked <= 0) return Results.BadRequest(new { message = "HoursWorked must be greater than 0" });

            db.VolunteerHours.Add(hour);
            await db.SaveChangesAsync();
            return Results.Created($"/hours/{hour.Id}", hour);
        });

        // POST /hours/org — organization logs hours for a volunteer on their task
        app.MapPost("/hours/org", async (OrgLogHoursRequest req, AppDbContext db) =>
        {
            if (req.OrganizationId <= 0) return Results.BadRequest(new { message = "OrganizationId is required" });
            if (req.TaskId <= 0) return Results.BadRequest(new { message = "TaskId is required" });
            if (req.UserId <= 0) return Results.BadRequest(new { message = "UserId is required" });
            if (req.HoursWorked <= 0) return Results.BadRequest(new { message = "HoursWorked must be greater than 0" });

            // Verify the org owns the task
            var task = await db.Tasks.FindAsync(req.TaskId);
            if (task is null) return Results.NotFound(new { message = "Task not found" });
            if (task.OrganizationId != req.OrganizationId)
                return Results.Json(new { message = "You can only log hours for your own tasks" }, statusCode: 403);

            // Verify the user is an approved volunteer on the task
            var volunteer = await db.TaskVolunteers
                .FirstOrDefaultAsync(v => v.TaskId == req.TaskId && v.UserId == req.UserId && v.Status == "approved");
            if (volunteer is null)
                return Results.BadRequest(new { message = "User is not an approved volunteer on this task" });

            var hour = new VolunteerHour
            {
                TaskId = req.TaskId,
                UserId = req.UserId,
                HoursWorked = req.HoursWorked,
                Notes = req.Notes
            };

            db.VolunteerHours.Add(hour);
            await db.SaveChangesAsync();
            return Results.Created($"/hours/{hour.Id}", hour);
        });

        // PUT /hours/{id}
        app.MapPut("/hours/{id}", async (int id, VolunteerHour input, AppDbContext db) =>
        {
            var hour = await db.VolunteerHours.FindAsync(id);
            if (hour is null) return Results.NotFound(new { message = "VolunteerHour not found" });

            hour.HoursWorked = input.HoursWorked;
            hour.Notes = input.Notes;

            await db.SaveChangesAsync();
            return Results.Ok(hour);
        });

        // DELETE /hours/{id}
        app.MapDelete("/hours/{id}", async (int id, AppDbContext db) =>
        {
            var hour = await db.VolunteerHours.FindAsync(id);
            if (hour is null) return Results.NotFound(new { message = "VolunteerHour not found" });

            db.VolunteerHours.Remove(hour);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public class OrgLogHoursRequest
{
    public int OrganizationId { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Notes { get; set; }
}
