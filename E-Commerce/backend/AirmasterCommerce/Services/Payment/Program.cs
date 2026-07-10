using EventBus;
using Microsoft.EntityFrameworkCore;
using Payment.Consumers;
using Payment.Data;
using SharedKernel.Logging;

var builder = Host.CreateApplicationBuilder(args);
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

// Register the asynchronous Background Worker listener service
builder.Services.AddHostedService<OrderCreatedConsumer>();

var _host = builder.Build();

// Run database migrations on application startup
using (var scope = _host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

await _host.RunAsync();