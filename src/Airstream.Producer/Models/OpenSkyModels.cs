using System.Text.Json.Serialization;

namespace Airstream.Producer.Models;

/// <summary>
/// OpenSky Network API response containing aircraft states
/// </summary>
public class OpenSkyResponse
{
    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("states")]
    public List<List<object?>>? States { get; set; }

    public IEnumerable<AircraftState> GetAircraftStates()
    {
        if (States == null)
            yield break;

        foreach (var state in States)
        {
            yield return AircraftState.FromStateVector(state, Time);
        }
    }
}

/// <summary>
/// Represents a single aircraft state from OpenSky Network
/// Documentation: https://openskynetwork.github.io/opensky-api/rest.html#response
/// </summary>
public class AircraftState
{
    public string Icao24 { get; set; } = string.Empty;
    public string? Callsign { get; set; }
    public string? OriginCountry { get; set; }
    public long? TimePosition { get; set; }
    public long LastContact { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public double? BaroAltitude { get; set; }
    public bool OnGround { get; set; }
    public double? Velocity { get; set; }
    public double? TrueTrack { get; set; }
    public double? VerticalRate { get; set; }
    public int[]? Sensors { get; set; }
    public double? GeoAltitude { get; set; }
    public string? Squawk { get; set; }
    public bool Spi { get; set; }
    public int PositionSource { get; set; }
    public long ResponseTime { get; set; }

    public static AircraftState FromStateVector(List<object?> state, long responseTime)
    {
        return new AircraftState
        {
            Icao24 = GetString(state, 0) ?? string.Empty,
            Callsign = GetString(state, 1)?.Trim(),
            OriginCountry = GetString(state, 2),
            TimePosition = GetLong(state, 3),
            LastContact = GetLong(state, 4) ?? 0,
            Longitude = GetDouble(state, 5),
            Latitude = GetDouble(state, 6),
            BaroAltitude = GetDouble(state, 7),
            OnGround = GetBool(state, 8),
            Velocity = GetDouble(state, 9),
            TrueTrack = GetDouble(state, 10),
            VerticalRate = GetDouble(state, 11),
            GeoAltitude = GetDouble(state, 13),
            Squawk = GetString(state, 14),
            Spi = GetBool(state, 15),
            PositionSource = GetInt(state, 16),
            ResponseTime = responseTime
        };
    }

    private static string? GetString(List<object?> state, int index)
    {
        if (index >= state.Count || state[index] == null)
            return null;
        return state[index]?.ToString();
    }

    private static double? GetDouble(List<object?> state, int index)
    {
        if (index >= state.Count || state[index] == null)
            return null;
        
        var value = state[index];
        if (value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
            return jsonElement.GetDouble();
        
        if (double.TryParse(value?.ToString(), out var result))
            return result;
        
        return null;
    }

    private static long? GetLong(List<object?> state, int index)
    {
        if (index >= state.Count || state[index] == null)
            return null;
        
        var value = state[index];
        if (value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
            return jsonElement.GetInt64();
        
        if (long.TryParse(value?.ToString(), out var result))
            return result;
        
        return null;
    }

    private static int GetInt(List<object?> state, int index)
    {
        if (index >= state.Count || state[index] == null)
            return 0;
        
        var value = state[index];
        if (value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
            return jsonElement.GetInt32();
        
        if (int.TryParse(value?.ToString(), out var result))
            return result;
        
        return 0;
    }

    private static bool GetBool(List<object?> state, int index)
    {
        if (index >= state.Count || state[index] == null)
            return false;
        
        var value = state[index];
        if (value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.True)
            return true;
        
        if (bool.TryParse(value?.ToString(), out var result))
            return result;
        
        return false;
    }
}
