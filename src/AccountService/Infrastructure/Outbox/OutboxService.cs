using System.Text.Json;
using AccountService.Data;
using AccountService.Domain.Outbox;

namespace AccountService.Infrastructure.Outbox
{
    public class OutboxService(AccountServiceContext db)
    {
        public async Task AddAsync<T>(T @event, CancellationToken ct)
        {
            var message = new OutboxMessage
            {
                Type = @event!.GetType().Name,
                Payload = JsonSerializer.Serialize(@event)
            };

            await db.OutboxMessages.AddAsync(message, ct);
        }
    }
}