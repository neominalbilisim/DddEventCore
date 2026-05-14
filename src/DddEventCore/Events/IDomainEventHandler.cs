namespace Neominal.DddEventCore.Events;

/// <summary>
/// Interface that handlers processing domain events must implement.
/// Multiple handlers can exist for each event type.
/// </summary>
/// <typeparam name="TEvent">The type of domain event to be processed</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Processes the domain event asynchronously
    /// </summary>
    /// <param name="domainEvent">The event to be processed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
