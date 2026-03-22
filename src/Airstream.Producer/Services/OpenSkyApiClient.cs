using System.Text.Json;
using Airstream.Producer.Models;

namespace Airstream.Producer.Services;

public class OpenSkyApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://opensky-network.org";

    public OpenSkyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Get all current aircraft states
    /// </summary>
    public async Task<OpenSkyResponse?> GetAllStatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/states/all", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"OpenSky API error {(int)response.StatusCode} ({response.StatusCode}): {content}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("Note: OpenSky may be blocking anonymous requests. Consider:");
                    Console.WriteLine("  1. Waiting a few minutes (rate limited)");
                    Console.WriteLine("  2. Using a smaller bounding box instead of /states/all");
                    Console.WriteLine("  3. Registering for free OpenSky account and adding authentication");
                }
                
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<OpenSkyResponse>(json);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP error fetching OpenSky data: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            return null;
        }
    }
}
