using MassTransit;
using MassTransit.Testing;
using MassTransitDemo.Contracts;
using MassTransitDemo.Worker.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MassTransitDemo.Tests;

public class ProcessPaymentConsumerTests
{
    [Fact]
    public async Task Should_Publish_OrderPaymentCompleted_When_Amount_Is_Under_Threshold()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();

        // Act
        await harness.Bus.Publish(new ProcessPayment(
            orderId,
            "CUST-TEST-001",
            250.00m,
            DateTime.UtcNow));

        // Assert
        var consumerHarness = harness.GetConsumerHarness<ProcessPaymentConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<ProcessPayment>(x => x.Context.Message.OrderId == orderId));
        Assert.True(await harness.Published.Any<OrderPaymentCompleted>(x => x.Context.Message.OrderId == orderId));
        Assert.False(await harness.Published.Any<OrderPaymentFailed>(x => x.Context.Message.OrderId == orderId));
    }

    [Fact]
    public async Task Should_Publish_OrderPaymentFailed_When_Amount_Exceeds_Threshold()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ProcessPaymentConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();

        // Act
        await harness.Bus.Publish(new ProcessPayment(
            orderId,
            "CUST-TEST-002",
            1500.00m,
            DateTime.UtcNow));

        // Assert
        var consumerHarness = harness.GetConsumerHarness<ProcessPaymentConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<ProcessPayment>(x => x.Context.Message.OrderId == orderId));
        Assert.True(await harness.Published.Any<OrderPaymentFailed>(x => x.Context.Message.OrderId == orderId));
        Assert.False(await harness.Published.Any<OrderPaymentCompleted>(x => x.Context.Message.OrderId == orderId));
    }
}
