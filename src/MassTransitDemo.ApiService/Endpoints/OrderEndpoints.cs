using MassTransitDemo.ApiService.Configurations;
using MassTransitDemo.ApiService.Helpers;
using MassTransitDemo.ApiService.Services;
using MassTransitDemo.Contracts;
using Microsoft.Extensions.Options;

namespace MassTransitDemo.ApiService.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapPost("/rate-limited", async (SubmitOrder order, OrderService orderService) =>
        {
            var orderToSubmit = await orderService.SubmitAsync(order);
            return Results.Accepted($"/api/orders/{orderToSubmit.OrderId}", orderToSubmit);
        })
        .WithName("SubmitOrderLimited")
        .RequireRateLimiting("orders-fixed");

        group.MapPost("/", async (SubmitOrder order, OrderService orderService) =>
        {
            var orderToSubmit = await orderService.SubmitAsync(order);
            return Results.Accepted($"/api/orders/{orderToSubmit.OrderId}", orderToSubmit);
        })
        .WithName("SubmitOrder");

        group.MapPost("/debounced", async (
            SubmitOrder order,
            OrderService orderService,
            DebounceGuard debounceGuard,
            IOptions<DebounceOptions> options) =>
        {
            var customerKey = string.IsNullOrWhiteSpace(order.CustomerNumber) ? "anonymous" : order.CustomerNumber;
            var key = order.OrderId != Guid.Empty
                ? $"debounce:{customerKey}:{order.OrderId}"
                : $"debounce:{customerKey}:{order.Amount}";

            if (!debounceGuard.TryAcquire(key, options.Value.DefaultInterval))
            {
                return Results.Accepted(uri: null, value: new { message = "Request debounced and suppressed." });
            }

            var orderToSubmit = await orderService.SubmitAsync(order);
            return Results.Accepted($"/api/orders/{orderToSubmit.OrderId}", orderToSubmit);
        })
        .WithName("SubmitOrderDebounced");
    }
}