namespace MassTransitDemo.Worker.StateMachines;

using MassTransit;
using MassTransitDemo.Contracts;
using Microsoft.Extensions.Logging;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    // States
    public State Submitted { get; private set; } = default!;
    public State Accepted { get; private set; } = default!;
    public State Cancelled { get; private set; } = default!;

    // Events
    public Event<SubmitOrder> SubmitOrderEvent { get; private set; } = default!;
    public Event<OrderPaymentCompleted> PaymentCompletedEvent { get; private set; } = default!;
    public Event<OrderPaymentFailed> PaymentFailedEvent { get; private set; } = default!;

    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
       
        InstanceState(x => x.CurrentState);

        // Correlate incoming events to the Saga instance by OrderId
        Event(() => SubmitOrderEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentCompletedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));
      
        Initially(
            When(SubmitOrderEvent)
                .Then(context =>
                {
                    context.Saga.CustomerNumber = context.Message.CustomerNumber;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.CreatedAt = context.Message.CreatedAt;

                    logger.LogInformation(
                        "[Saga: Order {OrderId}] Initialized for Customer: {Customer}, Amount: ${Amount}",
                        context.Saga.CorrelationId,
                        context.Saga.CustomerNumber,
                        context.Saga.Amount);
                })
                .Publish(context => new ProcessPayment(
                    context.Saga.CorrelationId,
                    context.Saga.CustomerNumber,
                    context.Saga.Amount,
                    DateTime.UtcNow))
                .TransitionTo(Submitted)
        );

        During(Submitted, 
            When(PaymentCompletedEvent)
                .Then(context =>
                {
                    context.Saga.PaymentTransactionId = context.Message.PaymentTransactionId;
                    context.Saga.CompletedAt = context.Message.ProcessedAt;

                    logger.LogInformation(
                        "[Saga: Order {OrderId}] Payment completed with Transaction ID: {TransactionId}",
                        context.Saga.CorrelationId,
                        context.Saga.PaymentTransactionId);
                })
                .TransitionTo(Accepted),

            When(PaymentFailedEvent)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;

                    logger.LogInformation(
                        "[Saga: Order {OrderId}] Payment failed due to: {Reason}",
                        context.Saga.CorrelationId,
                        context.Saga.FailureReason);
                })
                .TransitionTo(Cancelled)
        );
    }
}