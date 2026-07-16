using EventBus;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Domain.Enums;
using Ordering.API.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Serilog.Context;
using Polly;

namespace Ordering.API.Infrastructure.BackgroundWorkers
{
    public sealed class OrderStatusUpdateConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderStatusUpdateConsumer> _logger;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "ordering_status_update_queue_v2";

        public OrderStatusUpdateConsumer(
            IServiceProvider serviceProvider,
            ILogger<OrderStatusUpdateConsumer> logger,
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

            await _channel.QueueBindAsync(QueueName, ExchangeName, "payment.completed", cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, "payment.failed", cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, "shipping.created", cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, "inventory.failed", cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, "shipping.failed", cancellationToken: stoppingToken);

            _logger.LogInformation("Ordering order status update consumer listening on queue: {QueueName}...", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var routingKey = eventArgs.RoutingKey;
                    var correlationId = eventArgs.BasicProperties.CorrelationId ?? "Unknown-CorrelationId";

                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        var retryPolicy = Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                            (exception, timeSpan, retryCount, context) =>
                            {
                                _logger.LogWarning(exception, "Transient error updating order status. Retrying {RetryCount}/3 after {DelaySeconds}s delay...", retryCount, timeSpan.TotalSeconds);
                            });

                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

                            if (routingKey == "payment.completed")
                            {
                                var data = JsonSerializer.Deserialize<PaymentCompletedIntegrationEvent>(json);
                                if (data != null)
                                {
                                    var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == data.OrderId, stoppingToken);
                                    if (order != null)
                                    {
                                        order.Status = OrderStatus.Paid;
                                        order.TransactionId = data.TransactionId;
                                        order.PaidAtUtc = DateTime.UtcNow;
                                        await dbContext.SaveChangesAsync(stoppingToken);
                                        _logger.LogInformation("Order {OrderId} status updated to Paid.", data.OrderId);
                                    }
                                }
                            }
                            else if (routingKey == "shipping.created")
                            {
                                var data = JsonSerializer.Deserialize<ShipmentCreatedIntegrationEvent>(json);
                                if (data != null)
                                {
                                    var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == data.OrderId, stoppingToken);
                                    if (order != null)
                                    {
                                        order.Status = OrderStatus.Shipped;
                                        order.TrackingNumber = data.TrackingNumber;
                                        order.Carrier = data.Carrier;
                                        order.ShippedAtUtc = DateTime.UtcNow;
                                        await dbContext.SaveChangesAsync(stoppingToken);
                                        _logger.LogInformation("Order {OrderId} status updated to Shipped.", data.OrderId);
                                    }
                                }
                            }
                            else if (routingKey == "payment.failed")
                            {
                                var data = JsonSerializer.Deserialize<PaymentFailedIntegrationEvent>(json);
                                if (data != null)
                                {
                                    var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == data.OrderId, stoppingToken);
                                    if (order != null)
                                    {
                                        order.Status = OrderStatus.Cancelled; // Or a specific Failed state
                                        await dbContext.SaveChangesAsync(stoppingToken);
                                        _logger.LogInformation("Order {OrderId} status updated to Cancelled due to payment failure.", data.OrderId);
                                    }
                                }
                            }
                            else if (routingKey == "shipping.failed")
                            {
                                var data = JsonSerializer.Deserialize<ShippingFailedIntegrationEvent>(json);
                                if (data != null)
                                {
                                    var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == data.OrderId, stoppingToken);
                                    if (order != null)
                                    {
                                        // Order is marked as cancelled/shipping failed. Payment service handles refund directly
                                        order.Status = OrderStatus.Cancelled;
                                        await dbContext.SaveChangesAsync(stoppingToken);
                                        _logger.LogInformation("Order {OrderId} status updated to Cancelled due to shipping failure.", data.OrderId);
                                    }
                                }
                            }
                        });
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order status update. Routing to DLQ...");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
