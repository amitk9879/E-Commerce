using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using SharedKernel.Logging;
using SharedKernel.Auth;

var builder = WebApplication.CreateBuilder(args);
builder.AddSharedSerilogLogging("Gateway");

// 1. Define the Rate Limiting Policy
builder.Services.AddRateLimiter(options =>
{
    // Policy A: Strict protection for sensitive compute/auth writes
    options.AddPolicy("StrictSecurePolicy", context =>
    {
        // Extract the remote client's IP address safely
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp, // <-- Creates a unique window for EACH separate IP!
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // Policy B: Moderate protection for read-heavy browsing
    options.AddPolicy("StandardReadPolicy", context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: clientIp, // <-- Creates a unique bucket for EACH separate IP!
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 20,
                ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                TokensPerPeriod = 15,
                QueueLimit = 5
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please slow down your requests.", token);
    };
});

// 2. Load YARP configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevShell", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Front-end server shell
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Crucial for SignalR Hub routing
    });
});

// Configure JWT Authentication
builder.Services.AddAirmasterJwtAuthentication(builder.Configuration);
// Tell YARP to forward the Authorization header correctly if needed
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AngularDevShell");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 3. Enable the Rate Limiter Middleware BEFORE the Reverse Proxy
app.UseRateLimiter();

// 4. Map the reverse proxy pipeline
app.MapReverseProxy();

app.Run();