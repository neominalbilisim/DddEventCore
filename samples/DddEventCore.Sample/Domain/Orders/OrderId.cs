using Neominal.DddEventCore.Domain;

namespace Neominal.DddEventCore.Sample.Domain.Orders;

/// <summary>
/// Order ID value object
/// </summary>
public sealed class OrderId : ValueObject
{
    public Guid Value { get; }

    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty", nameof(value));
        
        Value = value;
    }

    public static OrderId CreateNew() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
