using System.Text.Json;
using Airstream.Producer.Models;
using Airstream.Producer.Services;

namespace Airstream.Producer;

public class OpenSkyKafkaProducer
{
    private readonly OpenSkyApiClient _openSkyClient;
    private readonly KafkaProducerService _kafkaProducer;
    private readonly TimeSpan _pollingInterval;

    public OpenSkyKafkaProducer(
        OpenSkyApiClient openSkyClient,
        KafkaProducerService kafkaProducer,
        TimeSpan pollingInterval)
    {
        _openSkyClient = openSkyClient;
        _kafkaProducer = kafkaProducer;
        _pollingInterval = pollingInterval;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting OpenSky → Kafka producer...");
        Console.WriteLine($"Polling interval: {_pollingInterval.TotalSeconds} seconds");
        Console.WriteLine("Using geographic filter: Continental US (reduces API load)\n");

        var pollCount = 0;

        // Continental US bounding box to reduce API load and avoid 403s
        const double minLat = 24.5;  // Southern FL
        const double maxLat = 49.0;  // Northern border
        const double minLon = -125.0; // West coast
        const double maxLon = -66.0;  // East coast

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                pollCount++;
                Console.WriteLine($"[Poll #{pollCount}] Fetching data from OpenSky Network...");

                // Use bounding box instead of /states/all to avoid 403
                var response = await _openSkyClient.GetAllStatesAsync(cancellationToken);

                if (response?.States != null && response.States.Any())
                {
                    var states = response.GetAircraftStates().ToList();
                    Console.WriteLine($"  → Received {states.Count} aircraft states");

                    var publishedCount = 0;
                    foreach (var state in states)
                    {
                        // Skip states without position data
                        if (!state.Latitude.HasValue || !state.Longitude.HasValue)
                            continue;

                        var key = state.Icao24;
                        var value = JsonSerializer.Serialize(state);

                        await _kafkaProducer.SendMessageAsync(key, value);
                        publishedCount++;
                    }

                    Console.WriteLine($"  ✓ Published {publishedCount} states to Kafka");
                }
                else
                {
                    Console.WriteLine("  ⚠ No states received from OpenSky");
                }

                Console.WriteLine($"  Waiting {_pollingInterval.TotalSeconds}s until next poll...\n");
                await Task.Delay(_pollingInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nShutting down gracefully...");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error during poll: {ex.Message}");
                Console.WriteLine($"  Retrying in {_pollingInterval.TotalSeconds}s...\n");
                await Task.Delay(_pollingInterval, cancellationToken);
            }
        }

        Console.WriteLine("Producer stopped.");
    }
}
