using Identity.API.Data;
using Identity.API.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Identity.API.Application.DTOs.AuthDtos;

namespace Identity.API.Application.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, bool>
    {
        private readonly IdentityDbContext _context;

        public RegisterCommandHandler(IdentityDbContext context) => _context = context;

        public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Guard clause: Prevent duplicate identity creations
            if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Blowfish work-factor hashing
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
