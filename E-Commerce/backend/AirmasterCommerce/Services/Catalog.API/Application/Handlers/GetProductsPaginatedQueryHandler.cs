using Catalog.API.Application.DTOs;
using Catalog.API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Application.Handlers
{
    public sealed class GetProductsPaginatedQueryHandler : IRequestHandler<GetProductsPaginatedQuery, PaginatedResultDto<ProductDto>>
    {
        private readonly CatalogDbContext _context;

        public GetProductsPaginatedQueryHandler(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResultDto<ProductDto>> Handle(GetProductsPaginatedQuery request, CancellationToken cancellationToken)
        {
            int page = request.Page <= 0 ? 1 : request.Page;
            int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.Products.AsNoTracking();

            int totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.ImageUrl, p.CategoryId))
                .ToListAsync(cancellationToken);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResultDto<ProductDto>(items, totalCount, page, pageSize, totalPages);
        }
    }
}
