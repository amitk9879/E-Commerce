using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace Ordering.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderCommand command)
        {
            var orderId = await _mediator.Send(command);
            return Ok(new { OrderId = orderId, Status = "Accepted", Message = "Order received and saved to atomic processing stream." });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(Guid userId)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "guest@airmaster.com";
            var query = new GetOrdersByUserIdQuery(userId, userEmail);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAllOrdersQuery(page, pageSize));
            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 5)
        {
            var result = await _mediator.Send(new GetRecentOrdersQuery(limit));
            return Ok(result);
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetOrderMetrics()
        {
            var result = await _mediator.Send(new GetOrderMetricsQuery());
            return Ok(result);
        }
    }
}
