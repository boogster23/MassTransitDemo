using MassTransit;
using MassTransitDemo.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

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

app.MapPost("/orders", async (SubmitOrder order, IPublishEndpoint publishEndpoint) =>
{
    var orderToSubmit = order with
    {
        OrderId = order.OrderId == Guid.Empty ? Guid.NewGuid() : order.OrderId,
        CreatedAt = DateTime.UtcNow
    };

    await publishEndpoint.Publish(orderToSubmit);

    return Results.Accepted($"/orders/{orderToSubmit.OrderId}", orderToSubmit);
})
.WithName("SubmitOrder");

app.Run();