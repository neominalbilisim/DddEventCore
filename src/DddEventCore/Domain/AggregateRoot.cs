using Neominal.DddEventCore.Events;

namespace Neominal.DddEventCore.Domain;

/// <summary>
/// Base abstract class for aggregate roots.
/// Collects and manages domain events.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root ID</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot() : base()
    {
    }

    /// <summary>
    /// Used to raise domain events from within the aggregate.
    /// Events are not dispatched immediately, but collected.
    /// </summary>
    protected void RaiseEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Returns domain events that have not been dispatched yet
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Clears all domain events (typically after dispatch)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
