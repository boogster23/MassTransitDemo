namespace MassTransitDemo.Worker.StateMachines;

using MassTransit;

public class OrderState : SagaStateMachineInstance
{
    // maps to OrderId
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;
    public string CustomerNumber { get; set; } = default!;
    public decimal Amount { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RowVersion { get; set; }
}