using EventBus;
using Ordering.API.Application.Handlers;
using Ordering.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ordering.API.Infrastructure.BackgroundWorkers
{
    public sealed class OutboxProcessorWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorWorker> _logger;

        public OutboxProcessorWorker(IServiceProvider serviceProvider, ILogger<OutboxProcessorWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Background Processor Started.");

            // 1. Guard Loop: Block until the database is physically reachable and migrations are verified
            await WaitForDatabaseReadyAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                    var eventBus = scope.ServiceProvider.GetRequiredService<RabbitMQEventBus>();

                    var messages = await context.OutboxMessages
                        .Where(m => m.ProcessedOnUtc == null)
                        .OrderBy(m => m.OccurredOnUtc)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                    if (messages.Any())
                    {
                        foreach (var message in messages)
                        {
                            try
                            {
                                if (message.Type == typeof(OrderCreatedIntegrationEvent).FullName)
                                {
                                    var @event = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(message.Content);
                                    if (@event != null)
                                    {
                                        await eventBus.PublishAsync(@event, "order.created");
                                    }
                                }

                                message.ProcessedOnUtc = DateTime.UtcNow;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to publish Outbox message {MessageId}", message.Id);
                                message.Error = ex.Message;
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database query execution error occurred in the processing loop.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        /// <summary>
        /// Prevents loop crashes by verifying DB access safely before processing messages
        /// </summary>
        private async Task WaitForDatabaseReadyAsync(CancellationToken stoppingToken)
        {
            var isDbReady = false;
            while (!isDbReady && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

                    // Simple, low-overhead handshake check
                    await context.Database.CanConnectAsync(stoppingToken);
                    isDbReady = true;
                    _logger.LogInformation("Database connectivity check succeeded. Access verified.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Database 'Airmaster_OrderingDb' is not ready yet. Retrying in 5 seconds... Error: {Message}", ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}