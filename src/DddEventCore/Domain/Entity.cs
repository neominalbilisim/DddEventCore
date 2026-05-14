namespace Neominal.DddEventCore.Domain;

/// <summary>
/// Base abstract class for entities.
/// Provides identity-based equality comparison.
/// </summary>
/// <typeparam name="TId">The type of the entity ID</typeparam>
public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
{
    public TId Id { get; protected set; }

    protected Entity(TId id)
    {
        if (id == null || id.Equals(default(TId)))
            throw new ArgumentException("Entity ID cannot be null or default value", nameof(id));
        
        Id = id;
    }

    protected Entity()
    {
        // Parameterless constructor for EF Core
        Id = default!;
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity<TId>);
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
