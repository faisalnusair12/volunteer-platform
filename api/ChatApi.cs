using System.Text;
using System.Text.Json;

namespace backend.api;

public static class ChatApi
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat", async (ChatRequest req) =>
        {
            var apiKey = "YOUR_API_KEY"; // 🔥 حط المفتاح هون

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            // ✅ الهيدرز الصح
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            client.DefaultRequestHeaders.Add("X-Title", "chatbot-project");

            var url = "https://openrouter.ai/api/v1/chat/completions";

            var body = new
            {
                model = "openai/gpt-3.5-turbo", // موديل مجاني نسبياً
                messages = new[]
                {
                    new { role = "user", content = req.Message }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Results.Ok("API Error: " + response.StatusCode + " - " + responseString);
                }

                using var doc = JsonDocument.Parse(responseString);

                var reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return Results.Ok(reply);
            }
            catch (Exception ex)
            {
                return Results.Ok("Error: " + ex.Message);
            }
        });
    }
}

// ✅ مهم جداً
public record ChatRequest(string Message);
