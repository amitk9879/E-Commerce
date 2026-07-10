using Catalog.API.Application.DTOs;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using MediatR;
using SharedKernel.Interfaces;

namespace Catalog.API.Application.Handlers
{
    public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly CatalogDbContext _context;
        private readonly ICacheService _cacheService;
        private const string CatalogCacheKey = "products_all_collection";

        public CreateProductCommandHandler(CatalogDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                ImageUrl = request.ImageUrl,
                CategoryId = request.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            // Evict stale catalog cache items to maintain data consistency
            await _cacheService.RemoveAsync(CatalogCacheKey);

            return new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.ImageUrl,
                product.CategoryId
            );
        }
    }
}
