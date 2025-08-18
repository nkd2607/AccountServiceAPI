using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace AccountService.Infrastructure.Messaging
{
    public class RabbitMqConnection : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly ILogger<RabbitMqConnection> _logger;

        public RabbitMqConnection(IConfiguration config, ILogger<RabbitMqConnection> logger)
        {
            _logger = logger;

            _factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:HostName"] ?? "localhost",
                UserName = config["RabbitMQ:UserName"] ?? "guest",
                Password = config["RabbitMQ:Password"] ?? "guest",
                Port = int.TryParse(config["RabbitMQ:Port"], out var port) ? port : 5672
            };
        }

        public async Task<IChannel> CreateChannelAsync(CancellationToken ct)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                try
                {
                    _connection = await _factory.CreateConnectionAsync(ct);
                    _logger.LogInformation("RabbitMQ connection established.");
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogError(ex, "Unable to connect to RabbitMQ");
                    throw;
                }
            }

            return await _connection.CreateChannelAsync(cancellationToken: ct);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _logger.LogInformation("RabbitMQ connection closed.");
            }
        }
    }
}