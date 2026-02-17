using System.Text.Json;
using Confluent.Kafka;

namespace OrderTracker.Api.Infrastructure.Kafka;

public sealed class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public KafkaProducerService(string bootstrapServers, string topic)
    {
        _topic = topic;
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(KafkaEventPayload payload, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var message = new Message<string, string>
        {
            Key = payload.OrderId.ToString(),
            Value = json
        };

        await _producer.ProduceAsync(_topic, message, ct);
    }

    public void Dispose() => _producer.Dispose();
}
