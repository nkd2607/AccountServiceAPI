using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AccountService.Infrastructure.Messaging
{
    public interface IInboxStore
    {
        Task<bool> TryBeginProcessingAsync(Guid messageId, string handler, CancellationToken ct);
        Task MarkProcessedAsync(Guid messageId, string handler, CancellationToken ct);
    }

    public class EventConsumer<T>(
        RabbitMqConnection connection,
        ILogger<EventConsumer<T>> logger,
        IInboxStore? inbox = null,
        JsonSerializerOptions? jsonOptions = null)
    {
        private readonly JsonSerializerOptions _json = jsonOptions ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task StartAsync(string exchange,
            string queue,
            string routingKey,
            Func<T, Task> handler,
            CancellationToken cancellationToken,
            Func<T, Guid>? getMessageId = null, 
            ushort prefetch = 16,
            CancellationToken ct = default
        )
        {
            var channel = await connection.CreateChannelAsync(ct);

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
            await channel.QueueBindAsync(queue, exchange, routingKey, cancellationToken: ct);

            await channel.BasicQosAsync(0, prefetch, global: false, cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    var msg = JsonSerializer.Deserialize<T>(json, _json);
                    if (msg == null)
                    {
                        logger.LogWarning("Deserialization to {Type} returned null; acknowledging to skip. Payload: {Payload}",
                            typeof(T).Name, json);
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                        return;
                    }

                    if (inbox != null && getMessageId != null)
                    {
                        var messageId = getMessageId(msg);
                        var handlerName = typeof(T).Name;

                        var canProcess = await inbox.TryBeginProcessingAsync(messageId, handlerName, ct);
                        if (!canProcess)
                        {
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                            return;
                        }

                        await handler(msg);
                        await inbox.MarkProcessedAsync(messageId, handlerName, ct);
                    }
                    else
                    {
                        await handler(msg);
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Failed to process delivery {DeliveryTag} on queue {Queue}. Will NACK & requeue.",
                        ea.DeliveryTag, queue);

                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: ct);
                }
            };

            var consumerTag = await channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: ct);
            logger.LogInformation("Subscribed consumer {ConsumerTag} to {Queue} (binding {Exchange}:{RoutingKey})",
                consumerTag, queue, exchange, routingKey);
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var reg = ct.Register(() =>
            {
                _ = channel.BasicCancelAsync(consumerTag, cancellationToken: ct); 
                tcs.TrySetResult(null);
            });

            await tcs.Task;
        }
    }
}