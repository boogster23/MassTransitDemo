using MassTransit;
using MassTransit.Testing;
using MassTransitDemo.Contracts;
using MassTransitDemo.Worker.StateMachines;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MassTransitDemo.Tests;

public class OrderStateMachineTests
{
    [Fact]
    public async Task Should_Create_Saga_And_Transition_To_Submitted_When_Order_Is_Submitted()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        var orderId = Guid.NewGuid();

        // Act
        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            CustomerNumber = "CUST-SAGA-001",
            Amount = 300.00m,
            CreatedAt = DateTime.UtcNow
        });

        // Assert
        Assert.True(await sagaHarness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId));
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId));

        var sagaInstance = sagaHarness.Created.ContainsInState(orderId, sagaHarness.StateMachine, sagaHarness.StateMachine.Submitted);
        Assert.NotNull(sagaInstance);
        Assert.Equal("CUST-SAGA-001", sagaInstance.CustomerNumber);
        Assert.Equal(300.00m, sagaInstance.Amount);

        // Verify ProcessPayment was published by the saga
        Assert.True(await harness.Published.Any<ProcessPayment>(x => x.Context.Message.OrderId == orderId));
    }

    [Fact]
    public async Task Should_Transition_To_Accepted_When_Payment_Completes()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        var orderId = Guid.NewGuid();

        // Step 1: Submit Order
        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            CustomerNumber = "CUST-SAGA-002",
            Amount = 450.00m,
            CreatedAt = DateTime.UtcNow
        });

        Assert.True(await sagaHarness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId));

        // Step 2: Simulate Payment Completed
        await harness.Bus.Publish(new OrderPaymentCompleted(
            orderId,
            "TXN-ABC-123",
            DateTime.UtcNow));

        // Assert
        Assert.True(await sagaHarness.Consumed.Any<OrderPaymentCompleted>(x => x.Context.Message.OrderId == orderId));

        var sagaInstance = sagaHarness.Created.ContainsInState(orderId, sagaHarness.StateMachine, sagaHarness.StateMachine.Accepted);
        Assert.NotNull(sagaInstance);
        Assert.Equal("TXN-ABC-123", sagaInstance.PaymentTransactionId);
        Assert.NotNull(sagaInstance.CompletedAt);
    }

    [Fact]
    public async Task Should_Transition_To_Cancelled_When_Payment_Fails()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        var orderId = Guid.NewGuid();

        // Step 1: Submit Order
        await harness.Bus.Publish(new SubmitOrder
        {
            OrderId = orderId,
            CustomerNumber = "CUST-SAGA-003",
            Amount = 1500.00m,
            CreatedAt = DateTime.UtcNow
        });

        Assert.True(await sagaHarness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId));

        // Step 2: Simulate Payment Failed
        await harness.Bus.Publish(new OrderPaymentFailed(
            orderId,
            "Credit limit exceeded"));

        // Assert
        Assert.True(await sagaHarness.Consumed.Any<OrderPaymentFailed>(x => x.Context.Message.OrderId == orderId));

        var sagaInstance = sagaHarness.Created.ContainsInState(orderId, sagaHarness.StateMachine, sagaHarness.StateMachine.Cancelled);
        Assert.NotNull(sagaInstance);
        Assert.Equal("Credit limit exceeded", sagaInstance.FailureReason);
    }
}
