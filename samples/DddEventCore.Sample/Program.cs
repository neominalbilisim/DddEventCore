using Neominal.DddEventCore;
using Neominal.DddEventCore.Events;
using Neominal.DddEventCore.Sample.Domain.Orders;
using Neominal.DddEventCore.Sample.Domain.Orders.Services;
using Neominal.DddEventCore.Sample.Domain.Customers.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Neominal.DddEventCore.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        // Create DI container with host builder
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add DddEventCore and scan handlers
                services.AddDddEventCore(typeof(Program).Assembly);

                // Register Domain Services
                services.AddScoped<OrderPricingService>();
                services.AddScoped<CustomerUniquenessService>();
                services.AddScoped<ICustomerRepository, InMemoryCustomerRepository>();
            })
            .Build();

        Console.WriteLine("=== DddEventCore Sample Application ===\n");

        // Get service from DI container
        using var scope = host.Services.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var pricingService = scope.ServiceProvider.GetRequiredService<OrderPricingService>();

        await DemoOrderLifecycle(dispatcher);
        
        Console.WriteLine();
        
        await DemoDomainServices(pricingService);

        Console.WriteLine("\n=== Application Completed ===");
    }

    static async Task DemoOrderLifecycle(IDomainEventDispatcher dispatcher)
    {
        Console.WriteLine("--- Scenario 1: Successful Order Flow ---\n");

        // 1. Create order
        var customerId = Guid.NewGuid();
        var amount = new Money(150.50m, "TRY");
        var order = Order.Place(customerId, amount);
        
        Console.WriteLine($"✅ Order created: {order.Id}");
        Console.WriteLine($"   Status: {order.Status}");
        Console.WriteLine($"   Total: {order.TotalAmount}\n");

        // Dispatch domain events
        await dispatcher.DispatchAsync(order.GetDomainEvents());
        order.ClearDomainEvents();

        Console.WriteLine();

        // 2. Confirm order
        order.Confirm();
        Console.WriteLine($"✅ Order confirmed: {order.Id}");
        Console.WriteLine($"   Status: {order.Status}\n");

        await dispatcher.DispatchAsync(order.GetDomainEvents());
        order.ClearDomainEvents();

        Console.WriteLine("\n--- Scenario 2: Cancelled Order Flow ---\n");

        // 3. Create new order and cancel it
        var order2 = Order.Place(Guid.NewGuid(), new Money(75.00m, "USD"));
        Console.WriteLine($"✅ Order created: {order2.Id}");
        
        await dispatcher.DispatchAsync(order2.GetDomainEvents());
        order2.ClearDomainEvents();

        Console.WriteLine();

        order2.Cancel("Customer requested cancellation");
        Console.WriteLine($"❌ Order cancelled: {order2.Id}");
        Console.WriteLine($"   Status: {order2.Status}\n");

        await dispatcher.DispatchAsync(order2.GetDomainEvents());
        order2.ClearDomainEvents();

        Console.WriteLine("\n--- Scenario 3: Domain Exception Example ---\n");

        try
        {
            // Try to create order with invalid amount
            var invalidOrder = Order.Place(Guid.NewGuid(), new Money(0, "EUR"));
        }
        catch (Neominal.DddEventCore.Domain.DomainException ex)
        {
            Console.WriteLine($"❌ Domain Exception: {ex.Message}");
        }

        try
        {
            // Try to cancel already cancelled order
            order2.Cancel("Second attempt");
        }
        catch (Neominal.DddEventCore.Domain.DomainException ex)
        {
            Console.WriteLine($"❌ Domain Exception: {ex.Message}");
        }
    }

    static async Task DemoDomainServices(OrderPricingService pricingService)
    {
        Console.WriteLine("--- Scenario 4: Domain Service Usage ---\n");

        // Create order items
        var items = new List<OrderItem>
        {
            new OrderItem(Guid.NewGuid(), "Laptop", new Money(5000m, "TRY"), 1),
            new OrderItem(Guid.NewGuid(), "Mouse", new Money(250m, "TRY"), 2),
            new OrderItem(Guid.NewGuid(), "Keyboard", new Money(750m, "TRY"), 1)
        };

        var customerId = Guid.Parse("a1234567-89ab-cdef-0123-456789abcdef"); // VIP customer (starts with a)
        var shippingCost = new Money(50m, "TRY");

        Console.WriteLine("📦 Order Items:");
        foreach (var item in items)
        {
            Console.WriteLine($"   - {item.ProductName}: {item.Price} x {item.Quantity}");
        }
        Console.WriteLine($"   - Shipping: {shippingCost}\n");

        // Calculate price with Domain Service
        var totalPrice = pricingService.CalculateTotalPrice(items, customerId, shippingCost);
        var discount = pricingService.CalculateDiscount(customerId, new Money(6000m, "TRY"));

        Console.WriteLine($"💰 Pricing Calculation (via Domain Service):");
        Console.WriteLine($"   Subtotal: 6,000.00 TRY");
        Console.WriteLine($"   Discount (VIP 10%): {discount}");
        Console.WriteLine($"   Shipping: {shippingCost}");
        Console.WriteLine($"   Total: {totalPrice}");
        
        Console.WriteLine($"\n✅ Domain Service successfully calculated complex pricing logic");
        Console.WriteLine($"   (involving Customer aggregate + Pricing policies)");
        
        await Task.CompletedTask;
    }
}
