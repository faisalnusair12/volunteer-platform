using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;
using TaskModel = backend.db.Models.Task;

namespace backend.api;

public static class AdminApi
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        // GET /admin/stats
        app.MapGet("/admin/stats", async (AppDbContext db) =>
        {
            var userCount = await db.Users.CountAsync();
            var orgCount = await db.Organizations.CountAsync();
            var taskCount = await db.Tasks.CountAsync();
            var volunteerCount = await db.TaskVolunteers.CountAsync();
            var totalHours = await db.VolunteerHours.SumAsync(h => h.HoursWorked);
            var tagCount = await db.Tags.CountAsync();

            return Results.Ok(new
            {
                userCount,
                orgCount,
                taskCount,
                volunteerCount,
                totalHours,
                tagCount
            });
        });

        // PUT /admin/users/{id}/block
        app.MapPut("/admin/users/{id}/block", async (int id, AppDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound(new { message = "User not found" });

            user.IsBlocked = !user.IsBlocked;
            await db.SaveChangesAsync();
            return Results.Ok(new { user.Id, user.IsBlocked });
        });

        // PUT /admin/users/{id}/role
        app.MapPut("/admin/users/{id}/role", async (int id, RoleUpdateRequest req, AppDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound(new { message = "User not found" });

            if (req.Role != "student" && req.Role != "admin")
                return Results.BadRequest(new { message = "Role must be 'student' or 'admin'" });

            user.Role = req.Role;
            await db.SaveChangesAsync();
            return Results.Ok(new { user.Id, user.Role });
        });

        // DELETE /admin/users/{id}
        app.MapDelete("/admin/users/{id}", async (int id, AppDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound(new { message = "User not found" });

            // Prevent deleting Adam (id=1)
            if (user.Id == 1) return Results.BadRequest(new { message = "Cannot delete the primary admin" });

            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PUT /admin/organizations/{id}/toggle-approval
        app.MapPut("/admin/organizations/{id}/toggle-approval", async (int id, AppDbContext db) =>
        {
            var org = await db.Organizations.FindAsync(id);
            if (org is null) return Results.NotFound(new { message = "Organization not found" });

            org.IsApproved = !org.IsApproved;
            await db.SaveChangesAsync();
            return Results.Ok(new { org.Id, org.IsApproved });
        });

        // DELETE /admin/organizations/{id}
        app.MapDelete("/admin/organizations/{id}", async (int id, AppDbContext db) =>
        {
            var org = await db.Organizations.FindAsync(id);
            if (org is null) return Results.NotFound(new { message = "Organization not found" });

            db.Organizations.Remove(org);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PUT /admin/tasks/{id}
        app.MapPut("/admin/tasks/{id}/status", async (int id, TaskStatusRequest req, AppDbContext db) =>
        {
            var task = await db.Tasks.FindAsync(id);
            if (task is null) return Results.NotFound(new { message = "Task not found" });

            task.Status = req.Status;
            await db.SaveChangesAsync();
            return Results.Ok(task);
        });

        // DELETE /admin/tasks/{id}
        app.MapDelete("/admin/tasks/{id}", async (int id, AppDbContext db) =>
        {
            var task = await db.Tasks.FindAsync(id);
            if (task is null) return Results.NotFound(new { message = "Task not found" });

            db.Tasks.Remove(task);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PUT /admin/volunteers/{id}/status
        app.MapPut("/admin/volunteers/{id}/status", async (int id, VolunteerStatusRequest req, AppDbContext db) =>
        {
            var volunteer = await db.TaskVolunteers.FindAsync(id);
            if (volunteer is null) return Results.NotFound(new { message = "Volunteer record not found" });

            volunteer.Status = req.Status;
            await db.SaveChangesAsync();
            return Results.Ok(volunteer);
        });

        // DELETE /admin/volunteers/{id}
        app.MapDelete("/admin/volunteers/{id}", async (int id, AppDbContext db) =>
        {
            var volunteer = await db.TaskVolunteers.FindAsync(id);
            if (volunteer is null) return Results.NotFound(new { message = "Volunteer record not found" });

            db.TaskVolunteers.Remove(volunteer);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public class RoleUpdateRequest
{
    public string Role { get; set; } = null!;
}

public class TaskStatusRequest
{
    public string Status { get; set; } = null!;
}

public class VolunteerStatusRequest
{
    public string Status { get; set; } = null!;
}
