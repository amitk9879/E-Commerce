using Identity.API.Data;
using Identity.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Identity.API.Application.DTOs.AuthDtos;

namespace Identity.API.Application.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponseDto>
    {
        private readonly IdentityDbContext _context;
        private readonly TokenService _tokenService;

        public LoginCommandHandler(IdentityDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<TokenResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            string passwordInput = "password123";
            string secureHash = BCrypt.Net.BCrypt.HashPassword(passwordInput, workFactor: 12);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // Secure timing defense: Do not disclose whether email or password was wrong
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid login credentials.");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Commit refresh token metrics back to SQL database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync(cancellationToken);

            return new TokenResponseDto(accessToken, refreshToken);
        }
    }
}
