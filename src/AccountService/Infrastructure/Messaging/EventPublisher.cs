using AccountService.Domain.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AccountService.Infrastructure.Messaging
{
    public class EventPublisher(RabbitMqConnection connection, ILogger<EventPublisher> logger)
    {
        public async Task PublishAsync<T>(string exchange, string routingKey, T @event)
        {
            var channel = await connection.CreateChannelAsync(CancellationToken.None);

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = (DeliveryModes)2
            };

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: props,
                body: body
            );

            logger.LogInformation("Published {EventType} with routingKey {RoutingKey}", typeof(T).Name, routingKey);
        }
        public async Task PublishDomainEventAsync<T>(T @event)
        {
            var exchange = "account.events";
            string routingKey = @event switch
            {
                AccountOpened => "account.opened",
                MoneyCredited => "money.credited",
                MoneyDebited => "money.debited",
                TransferCompleted => "money.transfer.completed",
                InterestAccrued => "money.interest.accrued",
                _ => throw new InvalidOperationException($"Неизвестный тип события {@event!.GetType().Name}")
            };

            await PublishAsync(exchange, routingKey, @event);
        }
    }
}