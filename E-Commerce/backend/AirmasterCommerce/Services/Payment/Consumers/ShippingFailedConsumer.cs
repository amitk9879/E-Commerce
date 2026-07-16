using EventBus;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Payment.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Context;

namespace Payment.Consumers
{
    public record ShippingFailedIntegrationEvent(Guid OrderId, string TransactionId, string Reason) : IntegrationEvent;

    public sealed class ShippingFailedConsumer : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShippingFailedConsumer> _logger;
        private readonly RabbitMQEventBus _eventBus;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "payment_shipping_failed_queue";
        private const string RoutingKey = "shipping.failed";

        public ShippingFailedConsumer(
            IServiceProvider serviceProvider,
            ILogger<ShippingFailedConsumer> logger,
            IConfiguration configuration,
            RabbitMQEventBus eventBus,
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _eventBus = eventBus;
            _httpClientFactory = httpClientFactory;
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

            var dlxName = QueueName + ".dlx";
            var dlqName = QueueName + ".dlq";
            await _channel.ExchangeDeclareAsync(dlxName, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(dlqName, dlxName, "", cancellationToken: stoppingToken);
            
            var queueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", dlxName }
            };

            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

            _logger.LogInformation("Payment service listening for refunds on queue: {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<ShippingFailedIntegrationEvent>(json);

                    var correlationId = eventArgs.BasicProperties.CorrelationId ?? "Unknown-CorrelationId";

                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        if (@event != null)
                        {
                            await ProcessRefundAsync(@event);
                        }
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing refund. Routing to DLQ...");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessRefundAsync(ShippingFailedIntegrationEvent @event)
        {
            _logger.LogInformation("Processing refund for Order: {OrderId}, TransactionId: {TxnId} due to: {Reason}", @event.OrderId, @event.TransactionId, @event.Reason);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

            // Idempotency Check
            if (dbContext.IdempotencyRecords.Any(r => r.EventId == @event.Id))
            {
                _logger.LogInformation("Refund event {EventId} already processed. Skipping.", @event.Id);
                return;
            }

            var transaction = await dbContext.PaymentTransactions.FirstOrDefaultAsync(t => t.GatewayReference == @event.TransactionId);
            if (transaction != null && transaction.TransactionStatus == "Success")
            {
                // Simulate Refund API Call
                var client = _httpClientFactory.CreateClient("PaymentGateway");
                var dummyPayload = new StringContent(JsonSerializer.Serialize(new { transaction.Amount, Action = "Refund" }), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("post", dummyPayload);
                response.EnsureSuccessStatusCode();

                transaction.TransactionStatus = "Refunded";
                
                var idempotencyRecord = new Payment.Data.Entities.IdempotencyRecord 
                { 
                    EventId = @event.Id, 
                    EventType = nameof(ShippingFailedIntegrationEvent), 
                    ProcessedAtUtc = DateTime.UtcNow 
                };

                dbContext.IdempotencyRecords.Add(idempotencyRecord);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Payment refunded successfully for Transaction ID: {TxnId}", @event.TransactionId);
            }
            else
            {
                _logger.LogWarning("Transaction not found or not in Success state for refund. TxnId: {TxnId}", @event.TransactionId);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            base.StopAsync(cancellationToken).GetAwaiter().GetResult();
        }
    }
}
