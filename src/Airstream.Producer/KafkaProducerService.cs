using Confluent.Kafka;

namespace Airstream.Producer;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic = "adsb-raw";
    private bool _disposed;

    public KafkaProducerService(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "airstream-producer",
            Acks = Acks.All, // Required when EnableIdempotence is true
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Airstream Kafka Producer - Starting...\n");

        try
        {
            await SendTestMessagesAsync(10);
            Console.WriteLine("\n✓ All messages sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            throw;
        }
    }

    public async Task<DeliveryResult<string, string>> SendMessageAsync(string key, string value)
    {
        var message = new Message<string, string>
        {
            Key = key,
            Value = value
        };

        try
        {
            var result = await _producer.ProduceAsync(_topic, message);
            return result;
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine($"✗ Failed to deliver message: {ex.Error.Reason}");
            throw;
        }
    }

    private async Task SendTestMessagesAsync(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            var key = $"flight-{i}";
            var value = CreateTestMessage(i);

            var result = await SendMessageAsync(key, value);
            
            Console.WriteLine($"✓ Message {i} delivered to {result.TopicPartitionOffset}");
            
            await Task.Delay(500); // Small delay between messages
        }
    }

    private string CreateTestMessage(int flightNumber)
    {
        return $@"{{
            ""flightId"": ""TEST{flightNumber:D4}"",
            ""timestamp"": ""{DateTime.UtcNow:O}"",
            ""altitude"": {30000 + flightNumber * 100},
            ""latitude"": {37.5 + flightNumber * 0.1},
            ""longitude"": {-122.4 + flightNumber * 0.1}
        }}";
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _disposed = true;
    }
}
