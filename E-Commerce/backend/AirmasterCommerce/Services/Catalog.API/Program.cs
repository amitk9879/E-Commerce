using Catalog.API.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Behaviors;
using SharedKernel.Caching;
using SharedKernel.Interfaces;
using SharedKernel.Middleware;
using SharedKernel.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.AddSharedSerilogLogging("Catalog.API");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Data context config
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Native Microsoft MemoryCache engine registration
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Register MediatR validation pipeline interceptions
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Singleton initialization for EventBus
builder.Services.AddSingleton<EventBus.RabbitMQEventBus>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<EventBus.RabbitMQEventBus>>();
    var connectionString = builder.Configuration["RabbitMQ:ConnectionString"] ?? "amqps://yxcobcwd:msbIdDrx5pZn18UXuFYxyvc6inhz0Msh@fly.rmq.cloudamqp.com/yxcobcwd";
    return new EventBus.RabbitMQEventBus(connectionString, logger);
});

// Register background consumers
builder.Services.AddHostedService<Catalog.API.Infrastructure.BackgroundWorkers.OrderCreatedConsumer>();
builder.Services.AddHostedService<Catalog.API.Infrastructure.BackgroundWorkers.TransactionFailedConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    // Make sure to apply migrations on startup (user still needs to generate the migration file first)
    db.Database.Migrate();
}

app.Run();