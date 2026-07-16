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

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Extract the token from the Authorization header
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return BadRequest("Invalid token.");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            
            // To properly calculate expiration, you could parse the JWT here, 
            // but for simplicity we'll just set it to 15 mins (the token lifetime).
            var expiration = DateTime.UtcNow.AddMinutes(15);
            
            await _mediator.Send(new LogoutCommand(token, expiration));
            return Ok(new { Message = "Successfully logged out." });
        }

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            // Need the UserId from the authenticated user
            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized();
            }

            await _mediator.Send(new LogoutAllCommand(userId));
            return Ok(new { Message = "Successfully logged out from all devices." });
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
