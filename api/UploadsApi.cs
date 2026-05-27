using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.api;

public static class UploadsApi
{
    public static void MapUploadEndpoints(this WebApplication app)
    {
        app.MapPost("/upload", async (IFormFile file, HttpContext context) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { message = "No file uploaded." });
            }

            // Create uploads directory if it doesn't exist
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Generate a unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the full URL
            var request = context.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/uploads/{uniqueFileName}";

            return Results.Ok(new { url = fileUrl });
        })
        .DisableAntiforgery(); // Disable CSRF requirements for simplistic uploads
    }
}
