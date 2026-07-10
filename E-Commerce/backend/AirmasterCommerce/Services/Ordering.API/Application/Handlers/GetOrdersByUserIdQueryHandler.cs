using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Application.DTOs;
using Ordering.API.Domain.Enums;
using Ordering.API.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.API.Application.Handlers
{
    public sealed class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, IEnumerable<OrderHistoryDto>>
    {
        private readonly OrderingDbContext _context;

        public GetOrdersByUserIdQueryHandler(OrderingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderHistoryDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .Where(o => o.UserId == request.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);

            return orders.Select(o => new OrderHistoryDto(
                Id: o.Id.ToString(),
                CustomerEmail: request.UserEmail,
                OrderDate: o.OrderDate,
                TotalAmount: o.TotalAmount,
                Status: MapOrderStatus(o.Status),
                ItemsCount: o.Items.Sum(i => i.Quantity),
                ShippingAddress: o.ShippingAddress,
                TransactionId: o.TransactionId,
                TrackingNumber: o.TrackingNumber,
                Carrier: o.Carrier,
                Items: o.Items.Select(i => new OrderItemDto(
                    ProductId: i.ProductId.ToString(),
                    ProductName: i.ProductName,
                    Quantity: i.Quantity,
                    UnitPrice: i.UnitPrice
                )).ToList()
            ));
        }

        private static string MapOrderStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Processing",
                OrderStatus.Paid => "Payment Approved",
                OrderStatus.PaymentFailed => "Failed",
                OrderStatus.Shipped => "Shipped",
                OrderStatus.Cancelled => "Failed",
                _ => "Processing"
            };
        }
    }
}
