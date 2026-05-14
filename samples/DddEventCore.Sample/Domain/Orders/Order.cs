using Neominal.DddEventCore.Domain;
using Neominal.DddEventCore.Sample.Domain.Orders.Events;

namespace Neominal.DddEventCore.Sample.Domain.Orders;

/// <summary>
/// Order aggregate root
/// </summary>
public sealed class Order : AggregateRoot<OrderId>
{
    public Guid CustomerId { get; private set; }
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    // For EF Core
    private Order() : base()
    {
        TotalAmount = null!;
    }

    private Order(OrderId id, Guid customerId, Money totalAmount) : base(id)
    {
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    public static Order Place(Guid customerId, Money totalAmount)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty");

        if (totalAmount.Amount <= 0)
            throw new DomainException("Order total amount must be greater than zero");

        var orderId = OrderId.CreateNew();
        var order = new Order(orderId, customerId, totalAmount);

        // Raise domain event
        order.RaiseEvent(new OrderPlacedEvent(orderId, customerId, totalAmount));

        return order;
    }

    /// <summary>
    /// Confirms the order
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Cannot confirm order in {Status} status");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        // Raise domain event
        RaiseEvent(new OrderConfirmedEvent(Id, ConfirmedAt.Value));
    }

    /// <summary>
    /// Cancels the order
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled");
        
        if (Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel shipped order");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required", nameof(reason));

        Status = OrderStatus.Cancelled;

        // Raise domain event
        RaiseEvent(new OrderCancelledEvent(Id, reason));
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Cancelled
}
