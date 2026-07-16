using EventBus;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Infrastructure.BackgroundWorkers;
using Ordering.API.Infrastructure.Data;
using SharedKernel.Behaviors;
using SharedKernel.Middleware;
using SharedKernel.Logging;
using SharedKernel.Auth;

var builder = WebApplication.CreateBuilder(args);
builder.AddSharedSerilogLogging("Ordering.API");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Data persistence config
builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Singleton initialization matching your custom IAsyncDisposable EventBus
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

// Register MediatR pipeline actions and open-generic validation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Background Worker registration
builder.Services.AddHostedService<OutboxProcessorWorker>();
builder.Services.AddHostedService<OrderStatusUpdateConsumer>();

builder.Services.AddAirmasterJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    db.Database.Migrate();
}

app.Run();