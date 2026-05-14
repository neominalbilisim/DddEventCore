using Neominal.DddEventCore.Events;
using Neominal.DddEventCore.Sample.Domain.Orders.Events;

namespace Neominal.DddEventCore.Sample.Application.Orders.EventHandlers;

/// <summary>
/// Handler for OrderPlacedEvent - Sends email
/// </summary>
public class SendOrderConfirmationEmailHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // In real application, email service would be used
        Console.WriteLine($"📧 [Email] Order confirmation email sent for Order: {domainEvent.OrderId}");
        Console.WriteLine($"   Customer: {domainEvent.CustomerId}");
        Console.WriteLine($"   Amount: {domainEvent.TotalAmount}");
        
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for OrderPlacedEvent - Updates inventory
/// </summary>
public class UpdateInventoryHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // In real application, inventory service would be called
        Console.WriteLine($"📦 [Inventory] Stock updated for Order: {domainEvent.OrderId}");
        
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for OrderConfirmedEvent
/// </summary>
public class NotifyWarehouseHandler : IDomainEventHandler<OrderConfirmedEvent>
{
    public async Task HandleAsync(OrderConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🏭 [Warehouse] Notified for Order: {domainEvent.OrderId}");
        Console.WriteLine($"   Confirmed at: {domainEvent.ConfirmedAt}");
        
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for OrderCancelledEvent
/// </summary>
public class RefundPaymentHandler : IDomainEventHandler<OrderCancelledEvent>
{
    public async Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"💰 [Payment] Refund initiated for Order: {domainEvent.OrderId}");
        Console.WriteLine($"   Reason: {domainEvent.Reason}");
        
        await Task.CompletedTask;
    }
}
