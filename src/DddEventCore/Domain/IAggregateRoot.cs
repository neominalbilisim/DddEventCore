using Neominal.DddEventCore.Events;

namespace Neominal.DddEventCore.Domain;

/// <summary>
/// Interface that aggregate roots must implement.
/// Aggregate roots collect and manage domain events.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Returns domain events that have not been dispatched yet
    /// </summary>
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();

    /// <summary>
    /// Clears all domain events (after dispatch)
    /// </summary>
    void ClearDomainEvents();
}
