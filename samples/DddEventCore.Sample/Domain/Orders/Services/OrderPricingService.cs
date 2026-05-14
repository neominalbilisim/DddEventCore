using Neominal.DddEventCore.Domain;

namespace Neominal.DddEventCore.Sample.Domain.Orders.Services;

/// <summary>
/// Order pricing domain service.
/// Used for pricing logic involving multiple aggregates.
/// </summary>
public class OrderPricingService : IDomainService
{
    /// <summary>
    /// Calculates customer-specific discount
    /// </summary>
    public Money CalculateDiscount(Guid customerId, Money subtotal)
    {
        // In real application:
        // - Read loyalty level from Customer aggregate
        // - Check active campaigns from Promotion aggregate
        // - Apply pricing policies

        // Example: 10% discount for VIP customers
        if (IsVipCustomer(customerId))
        {
            var discountAmount = subtotal.Amount * 0.10m;
            return new Money(discountAmount, subtotal.Currency);
        }

        return Money.Zero(subtotal.Currency);
    }

    /// <summary>
    /// Calculates total order amount (items + shipping - discount)
    /// </summary>
    public Money CalculateTotalPrice(
        IEnumerable<OrderItem> items, 
        Guid customerId,
        Money shippingCost)
    {
        if (!items.Any())
            throw new DomainException("Cannot calculate price for empty order");

        // Sum of products
        var currency = items.First().Price.Currency;
        var subtotal = items.Aggregate(
            Money.Zero(currency),
            (sum, item) => sum.Add(item.Price.Multiply(item.Quantity)));

        // Calculate discount
        var discount = CalculateDiscount(customerId, subtotal);

        // Add shipping
        var total = subtotal.Add(shippingCost);

        // Subtract discount
        return total.Subtract(discount);
    }

    /// <summary>
    /// Checks if the customer is VIP
    /// </summary>
    private bool IsVipCustomer(Guid customerId)
    {
        // In real application, check from Customer aggregate
        // or read model

        // Simple check for example
        return customerId.ToString().StartsWith("a") || 
               customerId.ToString().StartsWith("b");
    }
}

/// <summary>
/// Order item
/// </summary>
public class OrderItem : ValueObject
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public Money Price { get; }
    public int Quantity { get; }

    public OrderItem(Guid productId, string productName, Money price, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Order item quantity must be greater than zero");

        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return Price;
        yield return Quantity;
    }
}
