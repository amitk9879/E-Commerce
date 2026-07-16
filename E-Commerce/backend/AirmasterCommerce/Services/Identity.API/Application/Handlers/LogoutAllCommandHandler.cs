using Identity.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Identity.API.Application.DTOs.AuthDtos;

namespace Identity.API.Application.Handlers
{
    public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand, bool>
    {
        private readonly IdentityDbContext _context;

        public LogoutAllCommandHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            // Change the SecurityStamp to invalidate all existing tokens globally
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
