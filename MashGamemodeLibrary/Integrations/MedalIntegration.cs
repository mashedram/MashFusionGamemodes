using System.Text;
using System.Text.Json;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Integrations;

public record struct MedalEvent {
    public string EventId { get; init; }
    public string EventName { get; init; }
}

public class MedalIntegration
{
    private const string MedalApiUrl = "http://localhost:12665";
    // It's a public API key aimed to mark the game type, it's fine to have it here.
    // https://docs.medal.tv/gameapi/index.html
    private const string ApiKey = "pub_ZKPrQhyrdUozjY077hrsPbiLxOr0EkKV";

    public static void SendEvent(MedalEvent medalEvent)
    {
        _ = SendEventAsync(medalEvent);
    }
    
    private static async Task SendEventAsync(MedalEvent medalEvent)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("publicKey", ApiKey);
            var payload = new
            {
                eventId = medalEvent.EventId,
                eventName = medalEvent.EventName,
                // contextTags = new
                // {
                //     GameMode = "MashGamemodeLibrary",  
                // },
                // triggerActions = new[] { "SaveClip", "SaveScreenshot" },
                // clipOptions = new {
                //     duration = 60,
                //     captureDelayMs = 1000,
                //     alertType = "SoundOnly"
                // }
            };

            var json = JsonSerializer.Serialize(payload);
            InternalLogger.Debug($"Sending event to Medal API: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{MedalApiUrl}/api/v1/event/invoke", content);
            InternalLogger.Debug($"Medal API response: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        } catch (Exception ex)
        {
            InternalLogger.Error($"Failed to send event to Medal API: {ex.Message}");
        }
    }
}