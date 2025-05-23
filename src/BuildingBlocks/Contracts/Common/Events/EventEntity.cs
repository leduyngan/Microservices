using Contracts.Domains;
using Contracts.Domains.Interfaces;

namespace Contracts.Common.Events;

public class EventEntity<T> : EntityBase<T>, IEventEntity
{
    private readonly List<BaseEvent> _domainEvents = new();
    
    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public IReadOnlyCollection<BaseEvent> DomainEvents() => _domainEvents.AsReadOnly();
}