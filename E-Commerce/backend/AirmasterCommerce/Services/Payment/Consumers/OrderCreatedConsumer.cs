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
using Serilog.Context;
using Polly;

namespace Payment.Consumers
{
    public record InventoryReservedIntegrationEvent(Guid OrderId, Guid UserId, decimal TotalAmount) : IntegrationEvent;
    public record PaymentCompletedIntegrationEvent(Guid OrderId, Guid UserId, string TransactionId) : IntegrationEvent;
    public record PaymentFailedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;

    public sealed class InventoryReservedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InventoryReservedConsumer> _logger;
        private readonly RabbitMQEventBus _eventBus;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "payment_inventory_reserved_queue";
        private const string RoutingKey = "inventory.reserved";

        public InventoryReservedConsumer(
            IServiceProvider serviceProvider,
            ILogger<InventoryReservedConsumer> logger,
            IConfiguration configuration,
            RabbitMQEventBus eventBus,
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _eventBus = eventBus;
            _httpClientFactory = httpClientFactory;
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

            // Configure Dead-Letter Exchange (DLX)
            var dlxName = "AirmasterCentralExchange.DLX";
            var dlqName = QueueName + ".dlq";
            await _channel.ExchangeDeclareAsync(dlxName, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(dlqName, dlxName, "", cancellationToken: stoppingToken);
            // Configure main queue to route to DLX on failure
            var queueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", dlxName }
            };


            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

            _logger.LogInformation("Payment service listening on queue: {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                InventoryReservedIntegrationEvent? @event = null;
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    @event = JsonSerializer.Deserialize<InventoryReservedIntegrationEvent>(json);
                    
                    var correlationId = eventArgs.BasicProperties.CorrelationId ?? "Unknown-CorrelationId";

                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        if (@event != null)
                        {
                            var retryPolicy = Policy
                                .Handle<Exception>()
                                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                                (exception, timeSpan, retryCount, context) =>
                                {
                                    _logger.LogWarning(exception, "Transient error processing order payment. Retrying {RetryCount}/3 after {DelaySeconds}s delay...", retryCount, timeSpan.TotalSeconds);
                                });

                            await retryPolicy.ExecuteAsync(async () =>
                            {
                                await ProcessPaymentSimulationAsync(@event);
                            });
                        }
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order payment simulation. Publishing payment.failed event...");
                    if (@event != null)
                    {
                        var failedEvent = new PaymentFailedIntegrationEvent(@event.OrderId, ex.Message);
                        await _eventBus.PublishAsync(failedEvent, "payment.failed");
                    }
                    
                    // requeue: false tells RabbitMQ to send it to the Dead-Letter Exchange
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessPaymentSimulationAsync(InventoryReservedIntegrationEvent @event)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

            // Idempotency Check
            if (dbContext.IdempotencyRecords.Any(r => r.EventId == @event.Id))
            {
                _logger.LogInformation("Event {EventId} already processed. Skipping.", @event.Id);
                return;
            }

            _logger.LogInformation("Processing payment simulation gateway authorization for Order: {OrderId} totaling ${Amount}", @event.OrderId, @event.TotalAmount);

            // Use the resilient HttpClient to simulate the third-party gateway call
            // Polly will automatically retry and apply circuit breakers here if the endpoint fails
            var client = _httpClientFactory.CreateClient("PaymentGateway");

            // Making a dummy request to sandbox to trigger HTTP resilience
            var dummyPayload = new StringContent(JsonSerializer.Serialize(new { @event.TotalAmount, Currency = "USD" }), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("post", dummyPayload);

            // If it fails after all retries, this throws and routes to DLQ
            response.EnsureSuccessStatusCode();
            string mockTransactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}";

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
            
            var idempotencyRecord = new IdempotencyRecord 
            { 
                EventId = @event.Id, 
                EventType = nameof(InventoryReservedIntegrationEvent), 
                ProcessedAtUtc = DateTime.UtcNow 
            };

            dbContext.PaymentTransactions.Add(tx);
            dbContext.IdempotencyRecords.Add(idempotencyRecord);
            await dbContext.SaveChangesAsync();

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
