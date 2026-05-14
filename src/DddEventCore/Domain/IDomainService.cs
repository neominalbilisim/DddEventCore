namespace Neominal.DddEventCore.Domain;

/// <summary>
/// Marker interface that domain services must implement.
/// Domain services coordinate multiple aggregates or
/// contain business logic that doesn't belong to a single aggregate.
/// </summary>
/// <remarks>
/// Domain Service Usage Scenarios:
/// - Business logic involving two or more aggregates
/// - Operations that don't naturally fit into an Entity or Value Object
/// - Domain logic requiring external system integration
/// - Complex calculations or validations
/// </remarks>
public interface IDomainService
{
    // Marker interface - no implementation required
}
