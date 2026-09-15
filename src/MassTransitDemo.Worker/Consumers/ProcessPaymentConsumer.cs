namespace MassTransitDemo.Worker.Consumers;

using MassTransit;
using MassTransitDemo.Contracts;
using Microsoft.Extensions.Logging;

public class ProcessPaymentConsumer(ILogger<ProcessPaymentConsumer> logger) : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "--> [Payment Gateway] Charging ${Amount} to Customer: {Customer} for Order: {OrderId}...",
            msg.Amount,
            msg.CustomerNumber,
            msg.OrderId);

        await Task.Delay(300);

        if (msg.Amount > 1000m)
        {
            logger.LogWarning(
                "--> [Payment Gateway] DECLINED Order: {OrderId}. Reason: Exceeds $1000 threshold.",
                msg.OrderId);
            await context.Publish(new OrderPaymentFailed(
                msg.OrderId,
                "Credit limit exceeded (max $1,000)."));
            return;
        }

        var transactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        logger.LogInformation(
            "--> [Payment Gateway] APPROVED Order: {OrderId}. Transaction: {TxId}",
            msg.OrderId,
            transactionId);
        await context.Publish(new OrderPaymentCompleted(
            msg.OrderId,
            transactionId,
            DateTime.UtcNow));
    }
}