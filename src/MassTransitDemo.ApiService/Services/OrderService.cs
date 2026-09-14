using MassTransit;
using MassTransitDemo.Contracts;

namespace MassTransitDemo.ApiService.Services;
public class OrderService(IPublishEndpoint publishEndpoint)
{
    public async Task<SubmitOrder> SubmitAsync(SubmitOrder order)
    {
        var orderToSubmit = order with
        {
            OrderId = order.OrderId == Guid.Empty ? Guid.NewGuid() : order.OrderId,
            CreatedAt = DateTime.UtcNow
        };

        await publishEndpoint.Publish(orderToSubmit);
        return orderToSubmit;
    }
}