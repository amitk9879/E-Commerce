using Catalog.API.Application.DTOs;
using Catalog.API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Interfaces;

namespace Catalog.API.Application.Handlers
{
    public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly CatalogDbContext _context;
        private readonly ICacheService _cacheService;
        private const string CatalogCacheKey = "products_all_collection";

        public GetProductsQueryHandler(CatalogDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            // 1. Fetch from Cache Layer first
            var cachedData = await _cacheService.GetAsync<IEnumerable<ProductDto>>(CatalogCacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            // 2. Cache Miss: Query Database using AsNoTracking for fast read speeds
            var products = await _context.Products
                .AsNoTracking()
                .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.ImageUrl, p.CategoryId))
                .ToListAsync(cancellationToken);

            // 3. Hydrate cache with a 10-minute relative lifespan expiration
            await _cacheService.SetAsync(CatalogCacheKey, products, TimeSpan.FromMinutes(10));

            return products;
        }
    }
}
