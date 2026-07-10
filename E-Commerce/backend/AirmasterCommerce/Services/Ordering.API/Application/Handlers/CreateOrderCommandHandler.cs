using EventBus.Events;
using MediatR;
using Ordering.API.Application.DTOs;
using Ordering.API.Domain.Entities;
using Ordering.API.Infrastructure.Data;
using SharedKernel.Outbox;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.API.Application.Handlers
{
    public record OrderCreatedIntegrationEvent(Guid OrderId, Guid UserId, decimal TotalAmount) : IntegrationEvent;

    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly OrderingDbContext _context;

        public CreateOrderCommandHandler(OrderingDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = request.ShippingAddress,
                TotalAmount = request.Items.Sum(x => x.Quantity * x.UnitPrice)
            };

            foreach (var itemInput in request.Items)
            {
                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = itemInput.ProductId,
                    ProductName = itemInput.ProductName,
                    Quantity = itemInput.Quantity,
                    UnitPrice = itemInput.UnitPrice
                });
            }

            // 1. Stage order aggregate entities
            _context.Orders.Add(order);

            // 2. Stage outbox integration content within the exact same buffer
            var integrationEvent = new OrderCreatedIntegrationEvent(order.Id, order.UserId, order.TotalAmount);

            var outboxMessage = new OutboxMessage
            {
                Id = integrationEvent.Id,
                OccurredOnUtc = integrationEvent.CreationDate,
                Type = typeof(OrderCreatedIntegrationEvent).FullName ?? nameof(OrderCreatedIntegrationEvent),
                Content = JsonSerializer.Serialize(integrationEvent)
            };

            _context.OutboxMessages.Add(outboxMessage);

            // 3. Atomically commit both blocks to SQL Server
            await _context.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
