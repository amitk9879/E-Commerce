using EventBus;
using Notification.API.Consumers;
using Notification.API.Hubs;
using SharedKernel.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.AddSharedSerilogLogging("Notification.API");

// 1. Add SignalR Services
builder.Services.AddSignalR();

// 2. Register your CloudAMQP Event Bus Singleton cleanly from configuration
builder.Services.AddSingleton<RabbitMQEventBus>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
    var connectionString = builder.Configuration["RabbitMQ:ConnectionString"];

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("RabbitMQ ConnectionString is missing!");
    }
    return new RabbitMQEventBus(connectionString, logger);
});

// 3. Allow your frontend local ports to connect via CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000") // Angular / React default ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // CRITICAL FOR SIGNALR WEBSOCKETS
    });
});

builder.Services.AddHostedService<NotificationEventConsumer>();

var app = builder.Build();

app.UseCors();

// 4. Map the Hub endpoint
app.MapHub<NotificationHub>("/hub/notifications");

app.Run();