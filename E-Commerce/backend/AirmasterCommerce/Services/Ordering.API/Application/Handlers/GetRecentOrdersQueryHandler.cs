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
    public sealed class GetRecentOrdersQueryHandler : IRequestHandler<GetRecentOrdersQuery, IEnumerable<OrderHistoryDto>>
    {
        private readonly OrderingDbContext _context;

        public GetRecentOrdersQueryHandler(OrderingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderHistoryDto>> Handle(GetRecentOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            return orders.Select(o => new OrderHistoryDto(
                Id: o.Id.ToString(),
                CustomerEmail: o.UserId.ToString(),
                OrderDate: o.OrderDate,
                TotalAmount: o.TotalAmount,
                Status: MapOrderStatus(o.Status),
                ItemsCount: o.Items.Sum(i => i.Quantity),
                ShippingAddress: o.ShippingAddress,
                TransactionId: o.TransactionId,
                TrackingNumber: o.TrackingNumber,
                Carrier: o.Carrier,
                PaidAtUtc: o.PaidAtUtc,
                ShippedAtUtc: o.ShippedAtUtc,
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
