using Neominal.DddEventCore.Events;

namespace Neominal.DddEventCore.Sample.Domain.Orders.Events;

/// <summary>
/// Event raised when an order is placed
/// </summary>
public sealed record OrderPlacedEvent : IDomainEvent
{
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }
    
    public OrderId OrderId { get; }
    public Guid CustomerId { get; }
    public Money TotalAmount { get; }

    public OrderPlacedEvent(OrderId orderId, Guid customerId, Money totalAmount)
    {
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}

/// <summary>
/// Event raised when an order is confirmed
/// </summary>
public sealed record OrderConfirmedEvent : IDomainEvent
{
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }
    
    public OrderId OrderId { get; }
    public DateTime ConfirmedAt { get; }

    public OrderConfirmedEvent(OrderId orderId, DateTime confirmedAt)
    {
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
        OrderId = orderId;
        ConfirmedAt = confirmedAt;
    }
}

/// <summary>
/// Event raised when an order is cancelled
/// </summary>
public sealed record OrderCancelledEvent : IDomainEvent
{
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }
    
    public OrderId OrderId { get; }
    public string Reason { get; }

    public OrderCancelledEvent(OrderId orderId, string reason)
    {
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
        OrderId = orderId;
        Reason = reason;
    }
}
