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
    public record OrderItemEventDto(Guid ProductId, int Quantity);
    public record OrderCreatedIntegrationEvent(Guid OrderId, Guid UserId, decimal TotalAmount, List<OrderItemEventDto> Items) : IntegrationEvent;
    
    public record InventoryReservedIntegrationEvent(Guid OrderId, Guid UserId, decimal TotalAmount) : IntegrationEvent;
    public record InventoryFailedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;

    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCreatedConsumer> _logger;
        private readonly RabbitMQEventBus _eventBus;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "catalog_order_created_queue_v2";
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
            await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

            _logger.LogInformation("Catalog service listening for inventory reservation on: {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                OrderCreatedIntegrationEvent? @event = null;
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    @event = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(json);
                    
                    var correlationId = eventArgs.BasicProperties.CorrelationId ?? "Unknown-CorrelationId";

                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        if (@event != null)
                        {
                            var retryPolicy = Policy
                                .Handle<Exception>()
                                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                            await retryPolicy.ExecuteAsync(async () =>
                            {
                                await ReserveInventoryAsync(@event);
                            });
                        }
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reserving inventory. Publishing inventory.failed event...");
                    if (@event != null)
                    {
                        var failedEvent = new InventoryFailedIntegrationEvent(@event.OrderId, ex.Message);
                        await _eventBus.PublishAsync(failedEvent, "inventory.failed");
                    }
                    
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ReserveInventoryAsync(OrderCreatedIntegrationEvent @event)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            if (await dbContext.IdempotencyRecords.AnyAsync(r => r.EventId == @event.Id))
            {
                _logger.LogInformation("Event {EventId} already processed. Skipping.", @event.Id);
                return;
            }

            _logger.LogInformation("Attempting to reserve inventory for Order: {OrderId}", @event.OrderId);
            
            // Start Transaction to ensure consistency
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try 
            {
                foreach(var item in @event.Items)
                {
                    var product = await dbContext.Products.FindAsync(item.ProductId);
                    if(product == null) 
                    {
                        throw new Exception($"Product {item.ProductId} not found.");
                    }

                    if(product.StockQuantity < item.Quantity)
                    {
                        throw new Exception($"Insufficient stock for {product.Name}. Requested: {item.Quantity}, Available: {product.StockQuantity}");
                    }

                    product.StockQuantity -= item.Quantity;
                }

                var reservation = new OrderReservation
                {
                    OrderId = @event.OrderId,
                    UserId = @event.UserId,
                    TotalAmount = @event.TotalAmount,
                    ReservedItemsJson = JsonSerializer.Serialize(@event.Items),
                    ReservedAtUtc = DateTime.UtcNow
                };

                var idempotencyRecord = new IdempotencyRecord 
                { 
                    EventId = @event.Id, 
                    EventType = nameof(OrderCreatedIntegrationEvent), 
                    ProcessedAtUtc = DateTime.UtcNow 
                };

                dbContext.OrderReservations.Add(reservation);
                dbContext.IdempotencyRecords.Add(idempotencyRecord);
                
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Inventory reserved successfully for Order: {OrderId}", @event.OrderId);

                var successEvent = new InventoryReservedIntegrationEvent(@event.OrderId, @event.UserId, @event.TotalAmount);
                await _eventBus.PublishAsync(successEvent, "inventory.reserved");
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
