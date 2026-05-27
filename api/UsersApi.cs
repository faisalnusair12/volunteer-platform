using Microsoft.EntityFrameworkCore;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class UsersApi
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // GET /users
        app.MapGet("/users", async (AppDbContext db) =>
            Results.Ok(await db.Users.ToListAsync()));

        // GET /users/{id}
        app.MapGet("/users/{id}", async (int id, AppDbContext db) =>
            await db.Users.FindAsync(id) is User user
                ? Results.Ok(user)
                : Results.NotFound(new { message = "User not found" }));

        // PUT /users/{id}
        app.MapPut("/users/{id}", async (int id, User input, AppDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound(new { message = "User not found" });

            user.FullName = input.FullName;
            user.PhoneNumber = input.PhoneNumber;
            user.Role = input.Role;
            user.UniversityId = input.UniversityId;
            user.TakingVolunteeringCourse = input.TakingVolunteeringCourse;
            user.ProfilePictureUrl = input.ProfilePictureUrl;

            await db.SaveChangesAsync();
            return Results.Ok(user);
        });

        // DELETE /users/{id}
        app.MapDelete("/users/{id}", async (int id, AppDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound(new { message = "User not found" });

            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
