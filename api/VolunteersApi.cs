using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class VolunteersApi
{
    public static void MapVolunteerEndpoints(this WebApplication app)
    {
        // GET /volunteers
        app.MapGet("/volunteers", async (AppDbContext db) =>
            Results.Ok(await db.TaskVolunteers
                .Include(v => v.Task)
                .Include(v => v.User)
                .ToListAsync()));

        // GET /volunteers/{id}
        app.MapGet("/volunteers/{id}", async (int id, AppDbContext db) =>
            await db.TaskVolunteers
                .Include(v => v.Task)
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id) is TaskVolunteer volunteer
                ? Results.Ok(volunteer)
                : Results.NotFound(new { message = "Volunteer record not found" }));

        // GET /volunteers/task/{taskId}
        app.MapGet("/volunteers/task/{taskId}", async (int taskId, AppDbContext db) =>
            Results.Ok(await db.TaskVolunteers
                .Include(v => v.User)
                .Where(v => v.TaskId == taskId)
                .ToListAsync()));

        // GET /volunteers/user/{userId}
        app.MapGet("/volunteers/user/{userId}", async (int userId, AppDbContext db) =>
            Results.Ok(await db.TaskVolunteers
                .Include(v => v.Task)
                .Where(v => v.UserId == userId)
                .ToListAsync()));

        // POST /volunteers
        app.MapPost("/volunteers", async (TaskVolunteer volunteer, AppDbContext db) =>
        {
            if (volunteer.TaskId <= 0) return Results.BadRequest(new { message = "TaskId is required" });
            if (volunteer.UserId <= 0) return Results.BadRequest(new { message = "UserId is required" });

            db.TaskVolunteers.Add(volunteer);
            await db.SaveChangesAsync();
            return Results.Created($"/volunteers/{volunteer.Id}", volunteer);
        });

        // PUT /volunteers/{id} — update status
        app.MapPut("/volunteers/{id}", async (int id, TaskVolunteer input, AppDbContext db) =>
        {
            var volunteer = await db.TaskVolunteers.FindAsync(id);
            if (volunteer is null) return Results.NotFound(new { message = "Volunteer record not found" });

            volunteer.Status = input.Status;

            await db.SaveChangesAsync();
            return Results.Ok(volunteer);
        });

        // DELETE /volunteers/{id}
        app.MapDelete("/volunteers/{id}", async (int id, AppDbContext db) =>
        {
            var volunteer = await db.TaskVolunteers.FindAsync(id);
            if (volunteer is null) return Results.NotFound(new { message = "Volunteer record not found" });

            db.TaskVolunteers.Remove(volunteer);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
