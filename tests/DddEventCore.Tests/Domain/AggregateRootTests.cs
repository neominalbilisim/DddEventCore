using Neominal.DddEventCore.Domain;
using Neominal.DddEventCore.Events;
using FluentAssertions;
using Xunit;

namespace Neominal.DddEventCore.Tests.Domain;

public class AggregateRootTests
{
    private record TestDomainEvent(string Message) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid EventId { get; } = Guid.NewGuid();
    }

    private class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id) : base(id) { }

        public void DoSomething()
        {
            RaiseEvent(new TestDomainEvent("Something happened"));
        }

        public void DoMultipleThings()
        {
            RaiseEvent(new TestDomainEvent("First thing"));
            RaiseEvent(new TestDomainEvent("Second thing"));
        }
    }

    [Fact]
    public void AggregateRoot_ShouldRaiseEvent()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());

        // Act
        aggregate.DoSomething();
        var events = aggregate.GetDomainEvents();

        // Assert
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<TestDomainEvent>();
    }

    [Fact]
    public void AggregateRoot_ShouldRaiseMultipleEvents()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());

        // Act
        aggregate.DoMultipleThings();
        var events = aggregate.GetDomainEvents();

        // Assert
        events.Should().HaveCount(2);
        events.Should().AllBeOfType<TestDomainEvent>();
    }

    [Fact]
    public void AggregateRoot_ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DoMultipleThings();

        // Act
        aggregate.ClearDomainEvents();
        var events = aggregate.GetDomainEvents();

        // Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public void AggregateRoot_GetDomainEvents_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DoSomething();

        // Act
        var events = aggregate.GetDomainEvents();

        // Assert
        events.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }
}
