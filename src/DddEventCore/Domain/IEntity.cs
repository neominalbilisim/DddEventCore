namespace Neominal.DddEventCore.Domain;

/// <summary>
/// Base interface that all entities must implement.
/// Entities are defined by their identity.
/// </summary>
/// <typeparam name="TId">The type of the entity ID</typeparam>
public interface IEntity<out TId>
{
    /// <summary>
    /// Unique identifier of the entity
    /// </summary>
    TId Id { get; }
}
