using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.db;
using backend.db.Models;

namespace backend.api;

public static class AuthApi
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // POST /auth/register
        app.MapPost("/auth/register", async (UserRegisterRequest req, AppDbContext db, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email)) return Results.BadRequest(new { message = "Email is required" });
            if (string.IsNullOrWhiteSpace(req.Password)) return Results.BadRequest(new { message = "Password is required" });
            if (string.IsNullOrWhiteSpace(req.FullName)) return Results.BadRequest(new { message = "FullName is required" });
            if (req.Role != "student") return Results.BadRequest(new { message = "You can only register as a student" });

            if (await db.Users.AnyAsync(u => u.Email == req.Email) || await db.Organizations.AnyAsync(o => o.Email == req.Email))
                return Results.BadRequest(new { message = "Email already registered" });

            var user = new User
            {
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                Role = req.Role,
                UniversityId = req.UniversityId,
                TakingVolunteeringCourse = req.TakingVolunteeringCourse
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/users/{user.Id}", new { user.Id, user.Email, user.Role, user.CreatedAt });
        });

        // POST /auth/register/org
        app.MapPost("/auth/register/org", async (OrgRegisterRequest req, AppDbContext db, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email)) return Results.BadRequest(new { message = "Email is required" });
            if (string.IsNullOrWhiteSpace(req.Password)) return Results.BadRequest(new { message = "Password is required" });
            if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { message = "Name is required" });

            if (await db.Users.AnyAsync(u => u.Email == req.Email) || await db.Organizations.AnyAsync(o => o.Email == req.Email))
                return Results.BadRequest(new { message = "Email already registered" });

            var org = new Organization
            {
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Name = req.Name,
                PhoneNumber = req.PhoneNumber,
                IsApproved = false
            };

            db.Organizations.Add(org);
            await db.SaveChangesAsync();

            return Results.Created($"/organizations/{org.Id}", new { org.Id, org.Email, Role = "organization", org.CreatedAt });
        });

        // POST /auth/login
        app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email)) return Results.BadRequest(new { message = "Email is required" });
            if (string.IsNullOrWhiteSpace(req.Password)) return Results.BadRequest(new { message = "Password is required" });

            // Try user
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            {
                if (user.IsBlocked)
                    return Results.Json(new { message = "Your account has been blocked. Contact an administrator." }, statusCode: 403);

                var token = GenerateJwtToken(user.Id.ToString(), user.Email, user.Role, config);
                return Results.Ok(new { token, user = new { user.Id, user.Email, user.Role, user.CreatedAt, user.FullName, user.ProfilePictureUrl, user.IsBlocked } });
            }

            // Try org
            var org = await db.Organizations.FirstOrDefaultAsync(o => o.Email == req.Email);
            if (org != null && BCrypt.Net.BCrypt.Verify(req.Password, org.PasswordHash))
            {
                if (!org.IsApproved)
                    return Results.Json(new { message = "Your organization account is pending admin approval." }, statusCode: 403);

                var token = GenerateJwtToken(org.Id.ToString(), org.Email, "organization", config);
                return Results.Ok(new { token, user = new { org.Id, org.Email, Role = "organization", org.CreatedAt, org.Name } });
            }

            return Results.Json(new { message = "Invalid email or password" }, statusCode: 401);
        });

        // POST /auth/forgot-password
        app.MapPost("/auth/forgot-password", async (ForgotPasswordRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email)) return Results.BadRequest(new { message = "Email is required" });
            if (string.IsNullOrWhiteSpace(req.PhoneNumber)) return Results.BadRequest(new { message = "Phone Number is required" });
            if (string.IsNullOrWhiteSpace(req.UniversityId)) return Results.BadRequest(new { message = "University ID is required" });
            if (string.IsNullOrWhiteSpace(req.NewPassword)) return Results.BadRequest(new { message = "New password is required" });

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.PhoneNumber == req.PhoneNumber && u.UniversityId == req.UniversityId);
            if (user == null)
            {
                return Results.BadRequest(new { message = "No matching user found with the provided details" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Password reset successfully. You can now login." });
        });
    }

    private static string GenerateJwtToken(string id, string email, string role, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class UserRegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = "student";
    public string? PhoneNumber { get; set; }
    public string? UniversityId { get; set; }
    public bool TakingVolunteeringCourse { get; set; }
}

public class OrgRegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string UniversityId { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
