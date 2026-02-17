using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderTracker.Api.Data;
using OrderTracker.Api.Domain.Entities;
using OrderTracker.Api.Domain.Enums;
using OrderTracker.Api.Hubs;

namespace OrderTracker.Api.Infrastructure.Kafka;

public sealed class KafkaConsumerService(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaSettings> kafkaOptions,
    IHubContext<OrderHub> hubContext,
    ILogger<KafkaConsumerService> logger)
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = kafkaOptions.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.ConsumerGroup,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(settings.Topic);

        logger.LogInformation("Kafka consumer started. Topic: {Topic}", settings.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result = null;
                try
                {
                    result = consumer.Consume(stoppingToken);
                    if (result?.Message?.Value is null) continue;

                    await ProcessMessageAsync(result.Message.Value, stoppingToken);
                    consumer.Commit(result);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing Kafka message. Offset will not be committed.");
                }
            }
        }
        finally
        {
            consumer.Close();
            logger.LogInformation("Kafka consumer stopped.");
        }
    }

    private async Task ProcessMessageAsync(string messageJson, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<KafkaEventPayload>(messageJson, JsonOptions);
        if (payload is null)
        {
            logger.LogWarning("Received null or invalid Kafka payload.");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var isDuplicate = await db.OrderEvents
            .AnyAsync(e => e.IdempotencyKey == payload.IdempotencyKey, ct);

        if (isDuplicate)
        {
            logger.LogInformation(
                "Duplicate event skipped. IdempotencyKey: {Key}", payload.IdempotencyKey);
            return;
        }

        if (!Enum.TryParse<OrderEventType>(payload.EventType, out var eventType))
        {
            logger.LogWarning("Unknown event type: {EventType}", payload.EventType);
            return;
        }

        var orderEvent = new OrderEvent
        {
            OrderId = payload.OrderId,
            EventType = eventType,
            EventData = JsonSerializer.Serialize(payload.Data, JsonOptions),
            IdempotencyKey = payload.IdempotencyKey,
            CreatedAt = payload.Timestamp
        };

        db.OrderEvents.Add(orderEvent);
        await db.SaveChangesAsync(ct);

        await PushSignalRNotificationAsync(payload, ct);

        logger.LogInformation(
            "Processed event {EventType} for order {OrderId}", payload.EventType, payload.OrderId);
    }

    private async Task PushSignalRNotificationAsync(KafkaEventPayload payload, CancellationToken ct)
    {
        var group = $"user-{payload.UserId}";
        var notification = new
        {
            payload.OrderId,
            payload.EventType,
            payload.Data,
            payload.Timestamp
        };

        var method = payload.EventType == OrderEventType.OrderPlaced.ToString()
            ? "NewOrderPlaced"
            : "OrderStatusUpdated";

        await hubContext.Clients.Group(group).SendAsync(method, notification, ct);
    }
}
