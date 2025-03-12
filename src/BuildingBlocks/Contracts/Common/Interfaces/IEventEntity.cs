using Contracts.Common.Events;

namespace Contracts.Domains.Interfaces;

public interface IEventEntity
{
    void AddDomainEvent(BaseEvent domainEvent);
    void RemoveDomainEvent(BaseEvent domainEvent);
    void ClearDomainEvents();
    IReadOnlyCollection<BaseEvent> DomainEvents();
}

public interface IEventEntity<T> : IEntityBase<T>, IEventEntity
{

}