using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Identity.API.Application.DTOs.AuthDtos;

namespace Identity.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Account successfully registered." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var tokens = await _mediator.Send(command);
            return Ok(tokens);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _mediator.Send(new GetAllUsersQuery(page, pageSize));
            return Ok(users);
        }

        [HttpGet("users/metrics")]
        public async Task<IActionResult> GetUserMetrics()
        {
            var metrics = await _mediator.Send(new GetUserMetricsQuery());
            return Ok(metrics);
        }
    }
}
