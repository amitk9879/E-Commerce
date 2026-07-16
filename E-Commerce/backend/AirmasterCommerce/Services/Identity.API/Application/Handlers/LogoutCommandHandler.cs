using Identity.API.Data;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Identity.API.Application.DTOs.AuthDtos;

namespace Identity.API.Application.Handlers
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly IMemoryCache _memoryCache;

        public LogoutCommandHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return Task.FromResult(false);
            }

            // Calculate how long the token is still valid for
            var expiryTime = request.Expiration;
            var timeToLive = expiryTime - DateTime.UtcNow;

            if (timeToLive > TimeSpan.Zero)
            {
                // Add the token to the blacklist (memory cache)
                _memoryCache.Set(request.Token, "blacklisted", timeToLive);
            }

            return Task.FromResult(true);
        }
    }
}
