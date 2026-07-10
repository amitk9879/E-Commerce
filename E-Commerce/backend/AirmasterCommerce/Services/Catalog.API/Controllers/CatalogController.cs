using Catalog.API.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [Route("api/catalog")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CatalogController(IMediator mediator) => _mediator = mediator;

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            if (page.HasValue && pageSize.HasValue)
            {
                var result = await _mediator.Send(new GetProductsPaginatedQuery(page.Value, pageSize.Value));
                return Ok(result);
            }
            else
            {
                var result = await _mediator.Send(new GetProductsQuery());
                return Ok(result);
            }
        }

        [HttpGet("products/low-stock")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int limit = 5)
        {
            var result = await _mediator.Send(new GetLowStockProductsQuery(limit));
            return Ok(result);
        }

        [HttpPost("products")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllProducts), new { id = result.Id }, result);
        }
    }
}
