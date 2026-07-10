using Identity.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Identity.API.Application.DTOs.AuthDtos;

namespace Identity.API.Application.Handlers
{
    public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedResult<UserDto>>
    {
        private readonly IdentityDbContext _context;

        public GetAllUsersQueryHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users.AsNoTracking();
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(u => u.Email)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserDto(u.Id.ToString(), u.Email, u.FirstName, u.LastName, u.Role, System.DateTime.UtcNow))
                .ToListAsync(cancellationToken);

            return new PaginatedResult<UserDto>(items, totalCount, request.Page, request.PageSize);
        }
    }
}
