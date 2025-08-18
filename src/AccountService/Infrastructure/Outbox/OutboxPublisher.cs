using System.Text.Json;
using AccountService.Data;
using AccountService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Outbox
{
    public class OutboxPublisher(IServiceProvider sp, EventPublisher publisher, ILogger<OutboxPublisher> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AccountServiceContext>();

                    var messages = await db.OutboxMessages
                        .Where(m => m.ProcessedAt == null)
                        .OrderBy(m => m.OccurredAt)
                        .Take(50)
                        .ToListAsync(stoppingToken);

                    foreach (var msg in messages)
                    {
                        try
                        {
                            var type = Type.GetType($"Accounts.Domain.Events.{msg.Type}");
                            if (type == null)
                            {
                                logger.LogWarning("Unknown event type {Type}", msg.Type);
                                msg.ProcessedAt = DateTime.UtcNow;
                                continue;
                            }

                            var evt = JsonSerializer.Deserialize(msg.Payload, type);

                            await publisher.PublishDomainEventAsync(evt!);

                            msg.ProcessedAt = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            msg.RetryCount++;
                            logger.LogError(ex, "Failed to publish outbox message {Id}", msg.Id);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "OutboxPublisher loop error");
                }

                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}