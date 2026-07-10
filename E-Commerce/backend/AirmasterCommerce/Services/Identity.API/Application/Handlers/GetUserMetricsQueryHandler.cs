using MediatR;
using Microsoft.EntityFrameworkCore;
using Identity.API.Data;
using System.Threading;
using System.Threading.Tasks;
using static Identity.API.Application.DTOs.AuthDtos;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Identity.API.Application.Handlers
{
    public sealed class GetUserMetricsQueryHandler : IRequestHandler<GetUserMetricsQuery, UserMetricsDto>
    {
        private readonly IdentityDbContext _context;
        private readonly IMemoryCache _cache;

        public GetUserMetricsQueryHandler(IdentityDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<UserMetricsDto> Handle(GetUserMetricsQuery request, CancellationToken cancellationToken)
        {
            const string cacheKey = "UserMetricsCacheKey";

            if (!_cache.TryGetValue(cacheKey, out UserMetricsDto? metrics) || metrics == null)
            {
                var totalUsers = await _context.Users.CountAsync(cancellationToken);
                metrics = new UserMetricsDto(totalUsers);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                
                _cache.Set(cacheKey, metrics, cacheOptions);
            }

            return metrics!;
        }
    }
}
