# DddEventCore

> Lightweight DDD toolkit for .NET - Focus on domain logic, not infrastructure

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue.svg)](https://www.nuget.org/packages/DddEventCore)

**DddEventCore** is a lightweight, focused library for .NET developers building domain-driven applications. It provides essential DDD building blocks without the complexity of heavyweight frameworks.

## ✨ Features

- **🎯 Domain Events** - Event-driven architecture with async handlers
- **🏗️ Core DDD Building Blocks** - Entity, Value Object, Aggregate Root, Domain Service
- **🔄 Async/Await Support** - Modern async patterns throughout
- **💉 Simple DI Integration** - Works seamlessly with Microsoft.Extensions.DependencyInjection
- **🎨 SOLID Principles** - Clean, extensible architecture
- **📦 Minimal Dependencies** - Zero magic, just clean abstractions
- **🧪 Fully Tested** - Comprehensive unit test coverage
- **📚 Well Documented** - Extensive documentation and examples

Perfect for teams adopting DDD without heavyweight frameworks.

## 📦 Installation

```bash
dotnet add package Neominal.DddEventCore
```

Or via NuGet Package Manager:

```powershell
Install-Package Neominal.DddEventCore
```

## 🚀 Quick Start

### 1. Define Your Domain Event

```csharp
using DddEventCore.Events;

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
```

### 2. Create Your Aggregate Root

```csharp
using DddEventCore.Domain;

public sealed class Order : AggregateRoot<OrderId>
{
    public Guid CustomerId { get; private set; }
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    private Order(OrderId id, Guid customerId, Money totalAmount) : base(id)
    {
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
    }

    public static Order Place(Guid customerId, Money totalAmount)
    {
        if (totalAmount.Amount <= 0)
            throw new DomainException("Order total must be greater than zero");

        var orderId = OrderId.CreateNew();
        var order = new Order(orderId, customerId, totalAmount);

        // Raise domain event
        order.RaiseEvent(new OrderPlacedEvent(orderId, customerId, totalAmount));

        return order;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot confirm order");

        Status = OrderStatus.Confirmed;
        RaiseEvent(new OrderConfirmedEvent(Id));
    }
}
```

### 3. Create Event Handler

```csharp
using DddEventCore.Events;

public class SendOrderConfirmationEmailHandler : IDomainEventHandler<OrderPlacedEvent>
{
    private readonly IEmailService _emailService;

    public SendOrderConfirmationEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _emailService.SendOrderConfirmationAsync(
            domainEvent.CustomerId,
            domainEvent.OrderId,
            cancellationToken);
    }
}
```

### 4. Configure DI Container

```csharp
using DddEventCore;

var builder = WebApplication.CreateBuilder(args);

// Add DddEventCore and scan for handlers
builder.Services.AddDddEventCore(typeof(Program).Assembly);

// Register your domain services
builder.Services.AddScoped<OrderPricingService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();
```

### 5. Dispatch Events

```csharp
public class OrderService
{
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly IOrderRepository _repository;

    public OrderService(IDomainEventDispatcher dispatcher, IOrderRepository repository)
    {
        _dispatcher = dispatcher;
        _repository = repository;
    }

    public async Task PlaceOrderAsync(Guid customerId, Money amount)
    {
        // Create aggregate
        var order = Order.Place(customerId, amount);

        // Save to repository
        await _repository.AddAsync(order);

        // Dispatch domain events
        await _dispatcher.DispatchAsync(order.GetDomainEvents());

        // Clear events
        order.ClearDomainEvents();
    }
}
```

## 📚 Core Concepts

### Entity

Entities are defined by their identity, not their attributes:

```csharp
public class Customer : Entity<CustomerId>
{
    public string Name { get; private set; }
    public Email Email { get; private set; }

    public Customer(CustomerId id, string name, Email email) : base(id)
    {
        Name = name;
        Email = email;
    }
}
```

**Key Points:**

- Identity-based equality
- Mutable state allowed
- Lifecycle matters

### Value Object

Value Objects are defined by their attributes, not identity:

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

**Key Points:**

- Structural equality
- Immutable
- Side-effect free operations

### Aggregate Root

Aggregate Roots maintain consistency boundaries and collect domain events:

```csharp
public class ShoppingCart : AggregateRoot<CartId>
{
    private readonly List<CartItem> _items = new();

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public void AddItem(ProductId productId, int quantity)
    {
        // Business logic
        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(productId, quantity));
        }

        // Raise event
        RaiseEvent(new ItemAddedToCartEvent(Id, productId, quantity));
    }
}
```

**Key Points:**

- Consistency boundary
- Collects domain events
- Controls access to entities within

### Domain Service

Domain Services encapsulate business logic that doesn't belong to a single entity:

```csharp
public class OrderPricingService : IDomainService
{
    public Money CalculateTotalPrice(
        IEnumerable<OrderItem> items,
        Guid customerId,
        Money shippingCost)
    {
        // Coordinates multiple aggregates:
        // - OrderItems (Order aggregate)
        // - Customer loyalty level (Customer aggregate)
        // - Active promotions (Promotion aggregate)

        var subtotal = items.Aggregate(
            Money.Zero("USD"),
            (sum, item) => sum.Add(item.Price.Multiply(item.Quantity)));

        var discount = CalculateDiscount(customerId, subtotal);

        return subtotal.Add(shippingCost).Subtract(discount);
    }

    private Money CalculateDiscount(Guid customerId, Money subtotal)
    {
        // Business logic involving multiple aggregates
        return Money.Zero(subtotal.Currency);
    }
}
```

**When to Use Domain Services:**

- ✅ Coordinates multiple aggregates
- ✅ Business logic requiring external dependencies (repositories)
- ✅ Complex calculations not belonging to any single entity
- ❌ Don't use for orchestration (that's Application Services)
- ❌ Don't use for single-aggregate operations

**Usage from Aggregate:**

```csharp
public static Order Place(
    IEnumerable<OrderItem> items,
    Guid customerId,
    OrderPricingService pricingService)
{
    var total = pricingService.CalculateTotalPrice(items, customerId, shipping);
    var order = new Order(OrderId.CreateNew(), total);
    order.RaiseEvent(new OrderPlacedEvent(...));
    return order;
}
```

📖 **[Read the complete Domain Services Guide →](DOMAIN-SERVICES.md)**

### Domain Events

Domain Events represent something important that happened in the domain:

```csharp
// Define event (use past tense)
public sealed record OrderPlacedEvent : IDomainEvent
{
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }
    public OrderId OrderId { get; }
    public Guid CustomerId { get; }

    public OrderPlacedEvent(OrderId orderId, Guid customerId)
    {
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
    }
}

// Handle event (multiple handlers allowed)
public class SendEmailHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken ct)
    {
        // Send confirmation email
    }
}

public class UpdateInventoryHandler : IDomainEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken ct)
    {
        // Update stock levels
    }
}
```

**Key Points:**

- Named in past tense (something happened)
- Immutable (use records)
- Can have multiple handlers
- Dispatched after successful transaction

### Domain Exception

Domain Exceptions represent business rule violations:

```csharp
public class InsufficientStockException : DomainException
{
    public InsufficientStockException(ProductId productId, int requested, int available)
        : base($"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}")
    {
    }
}

// Usage
if (stock.Quantity < requestedQuantity)
    throw new InsufficientStockException(productId, requestedQuantity, stock.Quantity);
```

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│        Application Layer                │
│  ┌──────────────────────────────────┐   │
│  │   Domain Event Handlers          │   │
│  │   Application Services           │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘
                  ▼
┌─────────────────────────────────────────┐
│         Domain Layer                    │
│  ┌──────────────────────────────────┐   │
│  │   Aggregates (with Events)       │   │
│  │   Entities                       │   │
│  │   Value Objects                  │   │
│  │   Domain Services                │   │
│  │   Domain Events                  │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘
                  ▼
┌─────────────────────────────────────────┐
│      DddEventCore Library               │
│  ┌──────────────────────────────────┐   │
│  │   IDomainEventDispatcher         │   │
│  │   AggregateRoot<T>               │   │
│  │   Entity<T>                      │   │
│  │   ValueObject                    │   │
│  │   IDomainService                 │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

## ⚡ Best Practices

### 1. Event Naming

Event names should be in **past tense** (something has happened):

- ✅ `OrderPlacedEvent`
- ✅ `PaymentReceivedEvent`
- ✅ `InventoryReservedEvent`
- ❌ `PlaceOrderEvent`
- ❌ `ReceivePaymentEvent`

### 2. Immutable Events

Always use records or readonly properties:

```csharp
// ✅ GOOD - Using record
public sealed record OrderPlacedEvent(OrderId OrderId, Guid CustomerId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

// ✅ GOOD - Readonly properties
public class OrderPlacedEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public Guid CustomerId { get; }

    public OrderPlacedEvent(OrderId orderId, Guid customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}

// ❌ BAD - Mutable
public class OrderPlacedEvent : IDomainEvent
{
    public OrderId OrderId { get; set; } // NO!
}
```

### 3. Rich Domain Events

Events should contain all information handlers need:

```csharp
// ✅ GOOD - Contains all necessary information
public record ProductReservedEvent(
    ProductId ProductId,
    int Quantity,
    OrderId OrderId,
    DateTime ReservedUntil
);

// ❌ BAD - Missing context
public record ProductReservedEvent(ProductId ProductId);
```

### 4. Respect Aggregate Boundaries

One aggregate should not directly modify another. Use events instead:

```csharp
public class Order : AggregateRoot<OrderId>
{
    public void Confirm()
    {
        Status = OrderStatus.Confirmed;

        // ✅ GOOD - Raise event, let handler update inventory
        RaiseEvent(new OrderConfirmedEvent(Id, Items));
    }
}

// Handler updates different aggregate
public class ReserveInventoryHandler : IDomainEventHandler<OrderConfirmedEvent>
{
    public async Task HandleAsync(OrderConfirmedEvent e, CancellationToken ct)
    {
        // Update Inventory aggregate
        await _inventoryService.ReserveAsync(e.Items, ct);
    }
}
```

### 5. Unit of Work Pattern

Dispatch events after successful SaveChanges:

```csharp
public async Task SaveChangesAsync()
{
    // Within transaction
    await _dbContext.SaveChangesAsync();

    // Only dispatch if transaction succeeded
    var aggregates = _dbContext.ChangeTracker
        .Entries<IAggregateRoot>()
        .Select(e => e.Entity)
        .ToList();

    foreach (var aggregate in aggregates)
    {
        await _dispatcher.DispatchAsync(aggregate.GetDomainEvents());
        aggregate.ClearDomainEvents();
    }
}
```

## 🧪 Testing

### Testing Aggregates

```csharp
[Fact]
public void Order_Place_ShouldRaiseOrderPlacedEvent()
{
    // Arrange
    var customerId = Guid.NewGuid();
    var amount = new Money(100m, "USD");

    // Act
    var order = Order.Place(customerId, amount);
    var events = order.GetDomainEvents();

    // Assert
    events.Should().ContainSingle();
    var @event = events.First().Should().BeOfType<OrderPlacedEvent>().Subject;
    @event.CustomerId.Should().Be(customerId);
    @event.TotalAmount.Should().Be(amount);
}
```

### Testing Event Handlers

```csharp
[Fact]
public async Task SendEmailHandler_ShouldSendEmail_WhenOrderPlaced()
{
    // Arrange
    var emailService = Substitute.For<IEmailService>();
    var handler = new SendOrderConfirmationEmailHandler(emailService);
    var @event = new OrderPlacedEvent(OrderId.CreateNew(), Guid.NewGuid(), new Money(100, "USD"));

    // Act
    await handler.HandleAsync(@event);

    // Assert
    await emailService.Received(1).SendOrderConfirmationAsync(
        @event.CustomerId,
        @event.OrderId,
        Arg.Any<CancellationToken>());
}
```

### Testing Event Dispatcher

```csharp
[Fact]
public async Task Dispatcher_ShouldCallAllHandlers()
{
    // Arrange
    var services = new ServiceCollection();
    var handler1 = new TestEventHandler1();
    var handler2 = new TestEventHandler2();

    services.AddSingleton<IDomainEventHandler<TestEvent>>(handler1);
    services.AddSingleton<IDomainEventHandler<TestEvent>>(handler2);
    services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

    var provider = services.BuildServiceProvider();
    var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();

    var @event = new TestEvent("test");

    // Act
    await dispatcher.DispatchAsync(@event);

    // Assert
    handler1.WasCalled.Should().BeTrue();
    handler2.WasCalled.Should().BeTrue();
}
```

## 📖 Documentation

- **[Domain Services Guide](DOMAIN-SERVICES.md)** - Comprehensive guide on when and how to use Domain Services
- **[Version 1.0 Summary](VERSION-1.0-SUMMARY.md)** - Detailed version summary and design decisions
- **[Examples](samples/)** - Sample applications demonstrating usage

## 🗺️ Roadmap

### Version 2.0 (Planned)

- **Integration Events** - Communication between bounded contexts
- **Outbox Pattern** - Reliable event delivery with transactional guarantees
- **Event Versioning** - Schema evolution and backward compatibility
- **Specification Pattern** - Reusable business rule composition

### Future Versions

- Saga Pattern support
- Event Sourcing capabilities
- Snapshot support
- Pipeline behaviors

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Eric Evans - [Domain-Driven Design](https://www.domainlanguage.com/ddd/)
- Vaughn Vernon - [Implementing Domain-Driven Design](https://vaughnvernon.com/)
- Microsoft - [.NET Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/neominalbilisim/DddEventCore/issues)
- **Discussions**: [GitHub Discussions](https://github.com/neominalbilisim/DddEventCore/discussions)

---

Made with ❤️ for the .NET DDD community
