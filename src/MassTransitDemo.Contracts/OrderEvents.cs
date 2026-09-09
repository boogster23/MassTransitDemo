namespace MassTransitDemo.Contracts;

public record SubmitOrder
{
    public Guid OrderId { get; set; }
    public string CustomerNumber { get; init; } = String.Empty;
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record OrderSubmitted
{
    public Guid OrderId { get; set; }
    public string CustomerNumber { get; init; } = String.Empty;
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}