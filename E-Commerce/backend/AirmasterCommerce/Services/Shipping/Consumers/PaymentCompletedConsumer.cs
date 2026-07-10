using EventBus;
using EventBus.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shipping.Data;
using Shipping.Data.Entities;
using System.Text;
using System.Text.Json;

namespace Shipping.Consumers
{
    // Inbound Integration Event Contract matching Payment service output
    public record PaymentCompletedIntegrationEvent(Guid OrderId, Guid UserId, string TransactionId) : IntegrationEvent;

    // Outbound Integration Event Contract signaling shipping execution maps
    public record ShipmentCreatedIntegrationEvent(Guid OrderId, string TrackingNumber, string Carrier, string TransactionId) : IntegrationEvent;

    public sealed class PaymentCompletedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentCompletedConsumer> _logger;
        private readonly RabbitMQEventBus _eventBus;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "shipping_payment_completed_queue";
        private const string RoutingKey = "payment.completed";

        public PaymentCompletedConsumer(
            IServiceProvider serviceProvider,
            ILogger<PaymentCompletedConsumer> logger,
            IConfiguration configuration,
            RabbitMQEventBus eventBus)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _eventBus = eventBus;
            _connectionString = configuration["RabbitMQ:ConnectionString"] ?? "amqps://yxcobcwd:msbIdDrx5pZn18UXuFYxyvc6inhz0Msh@fly.rmq.cloudamqp.com/yxcobcwd";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_connectionString) // Connects directly via the CloudAMQP endpoint
            };
            if (factory.Ssl.Enabled)
            {
                factory.Ssl.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

            _logger.LogInformation("Shipping consumer listening on cloud queue: {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var @event = JsonSerializer.Deserialize<PaymentCompletedIntegrationEvent>(json);
                    if (@event != null)
                    {
                        await ProcessShippingAllocationAsync(@event);
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing cloud routing message tracking points.");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessShippingAllocationAsync(PaymentCompletedIntegrationEvent @event)
        {
            if (string.IsNullOrEmpty(@event.TransactionId))
            {
                _logger.LogWarning("Execution halted: Cannot generate Tracking ID because Payment Transaction ID is missing.");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();

            _logger.LogInformation("Payment verified for Order: {OrderId}. Generating logistics allocation record...", @event.OrderId);

            string trackingNumber = $"AMR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var shipment = new Shipment
            {
                Id = Guid.NewGuid(),
                OrderId = @event.OrderId,
                UserId = @event.UserId,
                TrackingNumber = trackingNumber,
                Carrier = "FedEx",
                ShippingStatus = "LabelCreated",
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Shipments.Add(shipment);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Logistics Tracking generated: {Tracking}", trackingNumber);

            // Notify downstream consumers of generation, passing the Transaction ID forward!
            var shippingEvent = new ShipmentCreatedIntegrationEvent(shipment.OrderId, shipment.TrackingNumber, shipment.Carrier, @event.TransactionId);
            await _eventBus.PublishAsync(shippingEvent, "shipping.created");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
