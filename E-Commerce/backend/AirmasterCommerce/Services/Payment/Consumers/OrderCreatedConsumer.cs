using EventBus;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Payment.Data;
using Payment.Data.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

namespace Payment.Consumers
{
    public record OrderCreatedIntegrationEvent(Guid OrderId, Guid UserId, decimal TotalAmount) : IntegrationEvent;
    public record PaymentCompletedIntegrationEvent(Guid OrderId, Guid UserId, string TransactionId) : IntegrationEvent;

    public sealed class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCreatedConsumer> _logger;
        private readonly RabbitMQEventBus _eventBus;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "payment_order_created_queue";
        private const string RoutingKey = "order.created";

        public OrderCreatedConsumer(
            IServiceProvider serviceProvider,
            ILogger<OrderCreatedConsumer> logger,
            IConfiguration configuration,
            RabbitMQEventBus eventBus)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _eventBus = eventBus;
            // Fallback default allows clean local checking configurations
            _connectionString = configuration["RabbitMQ:ConnectionString"] ?? "amqps://yxcobcwd:msbIdDrx5pZn18UXuFYxyvc6inhz0Msh@fly.rmq.cloudamqp.com/yxcobcwd";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
            if (factory.Ssl.Enabled)
            {
                factory.Ssl.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

            _logger.LogInformation("Payment service listening on queue: {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(json);

                    if (@event != null)
                    {
                        await ProcessPaymentSimulationAsync(@event);
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order payment simulation.");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessPaymentSimulationAsync(OrderCreatedIntegrationEvent @event)
        {
            _logger.LogInformation("Processing payment simulation gateway authorization for Order: {OrderId} totaling ${Amount}", @event.OrderId, @event.TotalAmount);

            // Simulate transactional processing time delay out-of-band
            await Task.Delay(2000);

            string mockTransactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            
            // Save transaction to DB context
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                
                var tx = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    OrderId = @event.OrderId,
                    UserId = @event.UserId,
                    Amount = @event.TotalAmount,
                    TransactionStatus = "Success",
                    GatewayReference = mockTransactionId,
                    ProcessedAtUtc = DateTime.UtcNow
                };
                
                dbContext.PaymentTransactions.Add(tx);
                await dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Payment cleared successfully. Transaction ID: {TxnId}", mockTransactionId);

            // Broadcast downstream message via shared kernel event bus
            var paymentSuccessEvent = new PaymentCompletedIntegrationEvent(@event.OrderId, @event.UserId, mockTransactionId);
            await _eventBus.PublishAsync(paymentSuccessEvent, "payment.completed");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
