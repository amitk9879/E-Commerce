using Microsoft.AspNetCore.SignalR;
using Notification.API.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Notification.API.Consumers
{
    // Event models matching your existing infrastructure contracts
    public record OrderCreatedNotificationEvent(Guid OrderId, Guid UserId, decimal TotalAmount);
    public record PaymentCompletedNotificationEvent(Guid OrderId, Guid UserId, string TransactionId);
    public record ShipmentCreatedNotificationEvent(Guid OrderId, string TrackingNumber, string Carrier);

    public sealed class NotificationEventConsumer : BackgroundService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationEventConsumer> _logger;
        private readonly string _connectionString;

        private IConnection? _connection;
        private IChannel? _channel;

        private const string ExchangeName = "AirmasterCentralExchange";
        private const string QueueName = "notification_realtime_stream_queue";

        public NotificationEventConsumer(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationEventConsumer> logger,
            IConfiguration configuration)
        {
            _hubContext = hubContext;
            _logger = logger;
            _connectionString = configuration["RabbitMQ:ConnectionString"] ?? "amqp://guest:guest@localhost:5672";
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

            // Declare the central core exchange
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

            // Declare a dedicated queue for notifications
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            // Bind to multiple routing keys to catch the entire life cycle of an order!
            await _channel.QueueBindAsync(QueueName, ExchangeName, "order.created", cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, "payment.completed", cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, "shipping.created", cancellationToken: stoppingToken);

            _logger.LogInformation("SignalR Notification Hub Background Engine listening to CloudAMQP...");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var routingKey = eventArgs.RoutingKey;

                    _logger.LogInformation("Notification hub intercepted event message matching: {RoutingKey}", routingKey);

                    // Direct message broadcast based on what phase just completed
                    switch (routingKey)
                    {
                        case "order.created":
                            var orderData = JsonSerializer.Deserialize<OrderCreatedNotificationEvent>(json);
                            if (orderData != null)
                            {
                                await _hubContext.Clients.Group(orderData.UserId.ToString())
                                    .SendAsync("ReceiveNotification", new { status = "Order Placed", orderId = orderData.OrderId });
                            }
                            break;

                        case "payment.completed":
                            var paymentData = JsonSerializer.Deserialize<PaymentCompletedNotificationEvent>(json);
                            if (paymentData != null)
                            {
                                await _hubContext.Clients.Group(paymentData.UserId.ToString())
                                    .SendAsync("ReceiveNotification", new { status = "Payment Processed", orderId = paymentData.OrderId, transactionId = paymentData.TransactionId });
                            }
                            break;

                        case "shipping.created":
                            // Since Shipping context doesn't carry UserId naturally, we broadcast or resolve info. 
                            // For validation, we can look up order state or alert clients assigned to groups.
                            var shipData = JsonSerializer.Deserialize<ShipmentCreatedNotificationEvent>(json);
                            if (shipData != null)
                            {
                                await _hubContext.Clients.All
                                    .SendAsync("ReceiveNotification", new { status = "Shipped", orderId = shipData.OrderId, tracking = shipData.TrackingNumber, carrier = shipData.Carrier });
                            }
                            break;
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error passing execution event streaming payload out to WebSockets layer.");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
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
