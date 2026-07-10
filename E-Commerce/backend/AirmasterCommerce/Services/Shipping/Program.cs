using EventBus;
using Microsoft.EntityFrameworkCore;
using Shipping.Consumers;
using Shipping.Data;
using SharedKernel.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.AddSharedSerilogLogging("Shipping");

builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddHostedService<PaymentCompletedConsumer>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
    db.Database.Migrate();
}

await host.RunAsync();