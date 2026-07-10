using MediatR;

namespace Identity.API.Application.DTOs
{
    public class AuthDtos
    {
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

        public record TokenResponseDto(string AccessToken, string RefreshToken);

        public record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<bool>;

        public record LoginCommand(string Email, string Password) : IRequest<TokenResponseDto>;
        
        public record UserDto(string Id, string Email, string FirstName, string LastName, string Role, DateTime CreatedAt);
        public record GetAllUsersQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<UserDto>>;

        public record UserMetricsDto(long TotalUsers);
        public record GetUserMetricsQuery() : IRequest<UserMetricsDto>;
    }
}
