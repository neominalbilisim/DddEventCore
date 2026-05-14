namespace Neominal.DddEventCore.Events;

/// <summary>
/// Base interface that all domain events must implement.
/// Domain events should be immutable and represent 
/// important business events that occur within an aggregate.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Timestamp when the event occurred (UTC)
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Unique identifier of the event
    /// </summary>
    Guid EventId { get; }
}
