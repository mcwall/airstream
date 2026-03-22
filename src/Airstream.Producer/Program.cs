using Airstream.Producer;
using Airstream.Producer.Services;

// Configuration
const string kafkaBootstrapServers = "localhost:9092";
var pollingInterval = TimeSpan.FromSeconds(30);

// Set up cancellation for graceful shutdown
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Create services
var httpClient = new HttpClient();
var openSkyClient = new OpenSkyApiClient(httpClient);
var kafkaProducer = new KafkaProducerService(kafkaBootstrapServers);
var producer = new OpenSkyKafkaProducer(openSkyClient, kafkaProducer, pollingInterval);

// Start polling
try
{
    await producer.StartAsync(cts.Token);
}
finally
{
    kafkaProducer.Dispose();
    httpClient.Dispose();
}

