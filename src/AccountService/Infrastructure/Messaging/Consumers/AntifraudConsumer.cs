using AccountService.Data;
using AccountService.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Messaging.Consumers
{
    public class AntifraudConsumer(
        RabbitMqConnection connection,
        AccountServiceContext db,
        ILogger<EventConsumer<object>> logger)
    {
        public async Task StartAsync(CancellationToken ct)
        {
            var consumer = new EventConsumer<object>(connection, logger);

            await consumer.StartAsync(
                "account.events",
                "account.antifraud",
                "client.#",
                async message =>
                {
                    switch (message)
                    {
                        case ClientBlocked blocked:
                            logger.LogInformation("Client {ClientId} blocked, freezing accounts", blocked.ClientId);

                            var accounts = await db.Accounts
                                .Where(a => a.OwnerId == blocked.ClientId)
                                .ToListAsync(ct);

                            foreach (var acc in accounts)
                                acc.Frozen = true;

                            await db.SaveChangesAsync(ct);
                            break;

                        case ClientUnblocked unblocked:
                            logger.LogInformation("Client {ClientId} unblocked, unfreezing accounts", unblocked.ClientId);

                            var unfrozen = await db.Accounts
                                .Where(a => a.OwnerId == unblocked.ClientId)
                                .ToListAsync(ct);

                            foreach (var acc in unfrozen)
                                acc.Frozen = false;

                            await db.SaveChangesAsync(ct);
                            break;

                        default:
                            logger.LogWarning("Unknown message type: {Type}", message.GetType().Name);
                            break;
                    }
                },
                ct, 
               message => message switch
                {
                    ClientBlocked b => b.ClientId,
                    ClientUnblocked u => u.ClientId,
                    _ => Guid.NewGuid()
                }, 1, ct);
        }
    }
}