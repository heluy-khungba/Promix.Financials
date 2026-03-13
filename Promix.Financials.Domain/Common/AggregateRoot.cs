using System;
using System.Collections.Generic;

namespace Promix.Financials.Domain.Common;

public abstract class AggregateRoot<TKey> : Entity<TKey> where TKey : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

/// <summary>
/// Base record to standardize timestamps.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}