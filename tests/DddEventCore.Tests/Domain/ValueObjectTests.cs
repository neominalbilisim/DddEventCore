using Neominal.DddEventCore.Domain;
using FluentAssertions;
using Xunit;

namespace Neominal.DddEventCore.Tests.Domain;

public class ValueObjectTests
{
    private class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string ZipCode { get; }

        public Address(string street, string city, string zipCode)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;
        }
    }

    [Fact]
    public void ValueObject_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Istanbul", "34000");
        var address2 = new Address("123 Main St", "Istanbul", "34000");

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void ValueObject_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Istanbul", "34000");
        var address2 = new Address("456 Oak Ave", "Ankara", "06000");

        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void ValueObject_SameReference_ShouldBeEqual()
    {
        // Arrange
        var address = new Address("123 Main St", "Istanbul", "34000");

        // Act & Assert
        address.Should().Be(address);
        (address == address).Should().BeTrue();
    }

    [Fact]
    public void ValueObject_ShouldHaveConsistentHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Istanbul", "34000");
        var address2 = new Address("123 Main St", "Istanbul", "34000");

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }
}
