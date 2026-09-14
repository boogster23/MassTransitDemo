using System.Threading.RateLimiting;
using MassTransit;
using MassTransitDemo.ApiService.Configurations;
using MassTransitDemo.ApiService.Endpoints;
using MassTransitDemo.ApiService.Helpers;
using MassTransitDemo.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.Configure<DebounceOptions>(builder.Configuration.GetSection("Debounce"));
builder.Services.AddScoped<OrderService>();
builder.Services.AddSingleton<DebounceGuard>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("orders-fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ??
                          httpContext.Connection.RemoteIpAddress?.ToString() ??
                          "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var connString = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(connString);
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.MapOrderEndpoints();

app.Run();