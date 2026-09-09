namespace MassTransitDemo.Worker;

using MassTransit;
using MassTransitDemo.Contracts;
using Microsoft.Extensions.Logging;

public class SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger) : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        logger.LogInformation(
            "===> Received Order: {OrderId} for Customer: {Customer}, Amount: ${Amount}",
            context.Message.OrderId,
            context.Message.CustomerNumber,
            context.Message.Amount);

        await Task.Delay(100);

        await context.Publish<OrderSubmitted>(new
        {
           context.Message.OrderId,
           context.Message.CustomerNumber,
           context.Message.Amount,
           CreatedAt = DateTime.UtcNow 
        });

        logger.LogInformation("===> Order {OrderId} processed and OrderSubmitted published!", context.Message.OrderId);
    }
}