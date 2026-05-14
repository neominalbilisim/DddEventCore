using Neominal.DddEventCore.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Neominal.DddEventCore.Tests.Events;

public class DomainEventDispatcherTests
{
    private record TestEvent(string Message) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid EventId { get; } = Guid.NewGuid();
    }

    private class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public List<TestEvent> HandledEvents { get; } = new();

        public Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            HandledEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    private class AnotherTestEventHandler : IDomainEventHandler<TestEvent>
    {
        public List<TestEvent> HandledEvents { get; } = new();

        public Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            HandledEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_ShouldCallRegisteredHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = new TestEventHandler();
        services.AddSingleton<IDomainEventHandler<TestEvent>>(handler);
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();

        var domainEvent = new TestEvent("Test message");

        // Act
        await dispatcher.DispatchAsync(domainEvent);

        // Assert
        handler.HandledEvents.Should().HaveCount(1);
        handler.HandledEvents.First().Should().Be(domainEvent);
    }

    [Fact]
    public async Task DispatchAsync_ShouldCallAllRegisteredHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler1 = new TestEventHandler();
        var handler2 = new AnotherTestEventHandler();
        
        services.AddSingleton<IDomainEventHandler<TestEvent>>(handler1);
        services.AddSingleton<IDomainEventHandler<TestEvent>>(handler2);
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();

        var domainEvent = new TestEvent("Test message");

        // Act
        await dispatcher.DispatchAsync(domainEvent);

        // Assert
        handler1.HandledEvents.Should().HaveCount(1);
        handler2.HandledEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task DispatchAsync_WithMultipleEvents_ShouldDispatchAll()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = new TestEventHandler();
        services.AddSingleton<IDomainEventHandler<TestEvent>>(handler);
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();

        var events = new[]
        {
            new TestEvent("First"),
            new TestEvent("Second"),
            new TestEvent("Third")
        };

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        handler.HandledEvents.Should().HaveCount(3);
    }

    [Fact]
    public async Task DispatchAsync_WithNullEvent_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();

        // Act
        var act = async () => await dispatcher.DispatchAsync((IDomainEvent)null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
