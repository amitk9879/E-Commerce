using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ordering.API.Application.DTOs;
using Ordering.API.Domain.Enums;
using Ordering.API.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.API.Application.Handlers
{
    public sealed class GetOrderMetricsQueryHandler : IRequestHandler<GetOrderMetricsQuery, OrderMetricsDto>
    {
        private readonly OrderingDbContext _context;
        private readonly IMemoryCache _cache;

        public GetOrderMetricsQueryHandler(OrderingDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<OrderMetricsDto> Handle(GetOrderMetricsQuery request, CancellationToken cancellationToken)
        {
            const string cacheKey = "OrderMetricsCacheKey";

            if (!_cache.TryGetValue(cacheKey, out OrderMetricsDto? metrics) || metrics == null)
            {
                var totalOrders = await _context.Orders.CountAsync(cancellationToken);
                var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount, cancellationToken);
                var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Paid, cancellationToken);

                metrics = new OrderMetricsDto(totalOrders, totalRevenue, pendingOrders);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                
                _cache.Set(cacheKey, metrics, cacheOptions);
            }

            return metrics!;
        }
    }
}
