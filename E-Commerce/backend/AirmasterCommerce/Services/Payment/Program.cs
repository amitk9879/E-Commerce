using EventBus;
using Microsoft.EntityFrameworkCore;
using Payment.Consumers;
using Payment.Data;
using SharedKernel.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.AddSharedSerilogLogging("Payment");

// Register isolated database infrastructure
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inject our shared asynchronous single-connection EventBus engine
builder.Services.AddSingleton<RabbitMQEventBus>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
    var connectionString = builder.Configuration["RabbitMQ:ConnectionString"];
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("RabbitMQ ConnectionString is missing from appsettings.json!");
    }
    return new RabbitMQEventBus(connectionString, logger);
});

// Register resilient HTTP client for simulated external payment gateway
builder.Services.AddHttpClient("PaymentGateway", client =>
{
    // Fetch endpoint from configuration
    var baseUrl = builder.Configuration["PaymentGateway:BaseUrl"]
                  ?? throw new InvalidOperationException("PaymentGateway:BaseUrl is missing in configuration.");
    client.BaseAddress = new Uri(baseUrl);
}).AddStandardResilienceHandler();

// Register the asynchronous Background Worker listener service
builder.Services.AddHostedService<OrderCreatedConsumer>();

var app = builder.Build();

app.MapPost("/payment", () =>
{
    return Results.Ok(new
    {
        Success = true,
        TransactionId = Guid.NewGuid()
    });
});

// Run database migrations on application startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

await app.RunAsync();