using System;

namespace Promix.Financials.Domain.Common;

public abstract class Entity<TKey> where TKey : notnull
{
    public TKey Id { get; protected set; } = default!;

    // ✅ Optimistic Concurrency — مُفعَّل
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (IsTransient(this) || IsTransient(other)) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        if (IsTransient(this)) return base.GetHashCode();
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity<TKey>? a, Entity<TKey>? b)
        => a is null ? b is null : a.Equals(b);

    public static bool operator !=(Entity<TKey>? a, Entity<TKey>? b)
        => !(a == b);

    private static bool IsTransient(Entity<TKey> entity)
        => EqualityComparer<TKey>.Default.Equals(entity.Id, default!);
}