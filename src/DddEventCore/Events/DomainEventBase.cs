namespace Neominal.DddEventCore.Events;

/// <summary>
/// Base abstract class for domain events.
/// It is recommended to implement as a record, but can also be used as a class.
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    /// <summary>
    /// Timestamp when the event occurred (UTC)
    /// </summary>
    public DateTime OccurredAt { get; }
    
    /// <summary>
    /// Unique identifier of the event
    /// </summary>
    public Guid EventId { get; }

    protected DomainEventBase()
    {
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }

    protected DomainEventBase(DateTime occurredAt, Guid eventId)
    {
        OccurredAt = occurredAt;
        EventId = eventId;
    }
}
