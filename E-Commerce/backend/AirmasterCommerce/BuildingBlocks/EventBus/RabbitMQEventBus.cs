using EventBus.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EventBus
{
    public sealed class RabbitMQEventBus : IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly ILogger<RabbitMQEventBus> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        private readonly SemaphoreSlim _lock = new(1, 1);

        private const string ExchangeName = "AirmasterCentralExchange";

        public RabbitMQEventBus(
            string connectionString,
            ILogger<RabbitMQEventBus> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        private async Task InitializeAsync()
        {
            if (_channel != null)
                return;

            await _lock.WaitAsync();

            try
            {
                if (_channel != null)
                    return;

                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_connectionString)
                };

                if (factory.Ssl.Enabled)
                {
                    factory.Ssl.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                }

                _connection = await factory.CreateConnectionAsync();

                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation(
                    "RabbitMQ connection established.");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task PublishAsync<TEvent>(
            TEvent @event,
            string routingKey)
            where TEvent : IntegrationEvent
        {
            await InitializeAsync();

            var json = JsonSerializer.Serialize(@event);

            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await _channel!.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Integration event {EventId} published to {RoutingKey}",
                @event.Id,
                routingKey);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            _lock.Dispose();
        }
    }
}