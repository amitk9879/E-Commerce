using MediatR;
using System;
using System.Collections.Generic;

namespace Ordering.API.Application.DTOs
{
    public record OrderItemInput(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

    public record CreateOrderCommand(
        Guid UserId,
        string ShippingAddress,
        List<OrderItemInput> Items
    ) : IRequest<Guid>;

    public record OrderItemDto(
        string ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    );

    public record OrderHistoryDto(
        string Id,
        string CustomerEmail,
        DateTime OrderDate,
        decimal TotalAmount,
        string Status,
        int ItemsCount,
        string ShippingAddress,
        string? TransactionId,
        string? TrackingNumber,
        string? Carrier,
        List<OrderItemDto> Items
    );

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public PaginatedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            Items = items; TotalCount = totalCount; Page = page; PageSize = pageSize;
        }
    }

    public record GetOrdersByUserIdQuery(Guid UserId, string UserEmail) : IRequest<IEnumerable<OrderHistoryDto>>;

    public record GetAllOrdersQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<OrderHistoryDto>>;

    public record GetRecentOrdersQuery(int Limit = 5) : IRequest<IEnumerable<OrderHistoryDto>>;

    public record OrderMetricsDto(long TotalOrders, decimal TotalRevenue, long PendingOrders);
    public record GetOrderMetricsQuery() : IRequest<OrderMetricsDto>;
}
