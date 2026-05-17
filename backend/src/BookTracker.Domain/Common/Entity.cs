namespace BookTracker.Domain.Common;

public abstract class Entity(Guid id) : IEquatable<Entity>
{
    public Guid Id { get; } = id;

    public bool Equals(Entity? other) => other is not null && other.GetType() == GetType() && other.Id == Id;
    public override bool Equals(object? obj) => obj is Entity e && Equals(e);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}

public abstract class AggregateRoot(Guid id) : Entity(id);
