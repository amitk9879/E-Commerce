using MediatR;

namespace Catalog.API.Application.DTOs
{
    // Unified, clean immutable record transport transfer states
    public record ProductDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int StockQuantity,
        string ImageUrl,
        Guid CategoryId
    );

    // Paginated result response wrapper
    public record PaginatedResultDto<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
    );

    // Write Pathway Command Contract
    public record CreateProductCommand(
        string Name,
        string Description,
        decimal Price,
        int StockQuantity,
        string ImageUrl,
        Guid CategoryId
    ) : IRequest<ProductDto>;

    // Read Pathway Query Contract
    public record GetProductsQuery : IRequest<IEnumerable<ProductDto>>;
    public record GetProductsPaginatedQuery(int Page, int PageSize) : IRequest<PaginatedResultDto<ProductDto>>;
    public record GetLowStockProductsQuery(int Limit = 5) : IRequest<IEnumerable<ProductDto>>;
}
