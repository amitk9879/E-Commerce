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
    public sealed class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PaginatedResult<OrderHistoryDto>>
    {
        private readonly OrderingDbContext _context;

        public GetAllOrdersQueryHandler(OrderingDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<OrderHistoryDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Orders.AsNoTracking();
            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = orders.Select(o => new OrderHistoryDto(
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
                Items: o.Items.Select(i => new OrderItemDto(
                    ProductId: i.ProductId.ToString(),
                    ProductName: i.ProductName,
                    Quantity: i.Quantity,
                    UnitPrice: i.UnitPrice
                )).ToList()
            ));

            return new PaginatedResult<OrderHistoryDto>(items, totalCount, request.Page, request.PageSize);
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
