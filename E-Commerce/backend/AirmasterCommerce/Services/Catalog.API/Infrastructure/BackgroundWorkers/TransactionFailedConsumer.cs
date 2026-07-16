using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using EventBus;
using EventBus.Events;
using Microsoft.EntityFrameworkCore;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;
using System.Text;
using System.Text.Json;

namespace Catalog.API.Infrastructure.BackgroundWorkers
{
    public record PaymentFailedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
    public record ShippingFailedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;

    public class TransactionFailedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransactionFailedConsumer> _logger;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "catalog_transaction_failed_queue";

        public TransactionFailedConsumer(
            IServiceProvider serviceProvider,
            ILogger<TransactionFailedConsumer> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
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

            var dlxName = "AirmasterCentralExchange.DLX";
            var dlqName = QueueName + ".dlq";
            await _channel.ExchangeDeclareAsync(dlxName, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(dlqName, dlxName, "", cancellationToken: stoppingToken);
            
            var queueArgs = new Dictionary<string, object?> { { "x-dead-letter-exchange", dlxName } };

            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs, cancellationToken: stoppingToken);
            
            // Bind to both payment.failed and shipping.failed
            await _channel.QueueBindAsync(QueueName, ExchangeName, "payment.failed", cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, "shipping.failed", cancellationToken: stoppingToken);

            _logger.LogInformation("Catalog service listening for failures on: {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    
                    // We only need the OrderId and EventId to process compensation
                    // Both PaymentFailed and ShippingFailed have the same structure (OrderId, Reason)
                    // We can deserialize as a generic IntegrationEvent containing OrderId
                    using var document = JsonDocument.Parse(json);
                    var eventId = document.RootElement.GetProperty("Id").GetGuid();
                    var orderId = document.RootElement.GetProperty("OrderId").GetGuid();
                    
                    var correlationId = eventArgs.BasicProperties.CorrelationId ?? "Unknown-CorrelationId";

                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        var retryPolicy = Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            await RestoreInventoryAsync(eventId, orderId, eventArgs.RoutingKey);
                        });
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error restoring inventory.");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task RestoreInventoryAsync(Guid eventId, Guid orderId, string routingKey)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            if (await dbContext.IdempotencyRecords.AnyAsync(r => r.EventId == eventId))
            {
                _logger.LogInformation("Event {EventId} already processed. Skipping.", eventId);
                return;
            }

            _logger.LogInformation("Attempting to restore inventory for Order: {OrderId} due to {RoutingKey}", orderId, routingKey);
            
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try 
            {
                var reservation = await dbContext.OrderReservations.FirstOrDefaultAsync(r => r.OrderId == orderId);
                if (reservation != null)
                {
                    var items = System.Text.Json.JsonSerializer.Deserialize<List<OrderItemEventDto>>(reservation.ReservedItemsJson);
                    if (items != null)
                    {
                        foreach(var item in items)
                        {
                            var product = await dbContext.Products.FindAsync(item.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity += item.Quantity; // Restore stock
                            }
                        }
                    }

                    // Optional: We can delete the reservation or keep it for audit. Let's delete it.
                    dbContext.OrderReservations.Remove(reservation);
                    _logger.LogInformation("Inventory restored successfully for Order: {OrderId}", orderId);
                }
                else
                {
                    _logger.LogWarning("No inventory reservation found for Order: {OrderId}. Cannot restore.", orderId);
                }

                var idempotencyRecord = new IdempotencyRecord 
                { 
                    EventId = eventId, 
                    EventType = "TransactionFailedCompensation", 
                    ProcessedAtUtc = DateTime.UtcNow 
                };
                
                dbContext.IdempotencyRecords.Add(idempotencyRecord);
                
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
