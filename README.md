# airstream
Playing with ADSB data

A toy application for streaming ADSB (Automatic Dependent Surveillance-Broadcast) data from OpenSky Network, processing it through Kafka, detecting go-arounds using Flink, and storing anomalies in PostgreSQL with a simple API.

## Architecture
- **Data Ingestion**: C# service streaming ADSB data from OpenSky Network
- **Message Broker**: Apache Kafka for data streaming
- **Stream Processing**: Apache Flink for go-around detection
- **Database**: PostgreSQL for storing detected anomalies
- **API**: C# .NET 10 API for serving anomaly data

## Prerequisites
- Docker
- Docker Compose
- .NET 10 SDK (for local development)

## Getting Started with Kafka

### 1. Start the Infrastructure
```bash
docker-compose up -d
```

This starts:
- **Kafka** on `localhost:9092` (KRaft mode - no Zookeeper needed)
- **Kafka UI** on `http://localhost:8080` (web interface)
- **PostgreSQL** on `localhost:5432`

### 2. Verify Kafka is Running
Check the logs:
```bash
docker-compose logs kafka
```

Or visit the Kafka UI at: http://localhost:8080

### 3. Test Kafka Connection
You can use the Kafka UI to create topics, send messages, and view consumers.

Alternatively, use the Kafka CLI tools inside the container:
```bash
# List topics
docker exec -it airstream-kafka kafka-topics --list --bootstrap-server localhost:9092

# Create a test topic
docker exec -it airstream-kafka kafka-topics --create --topic adsb-raw --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1

# Send a test message
echo "test message" | docker exec -i airstream-kafka kafka-console-producer --topic adsb-raw --bootstrap-server localhost:9092

# Consume messages
docker exec -it airstream-kafka kafka-console-consumer --topic adsb-raw --from-beginning --bootstrap-server localhost:9092
```

### 4. Connection Details for C# Applications

**Kafka Bootstrap Server**: `localhost:9092`

**PostgreSQL Connection String**: 
```
Host=localhost;Port=5432;Database=airstream;Username=airstream;Password=airstream_dev
```

## Next Steps
1. Create a C# producer to fetch and stream ADSB data from OpenSky
2. Set up Flink for stream processing
3. Implement go-around detection logic
4. Build the anomaly storage service
5. Create the API endpoints

## Stopping the Services
```bash
docker-compose down
```

To also remove volumes (clear all data):
```bash
docker-compose down -v
```
