using Contracts.Common.Events;

namespace Ordering.Domain.OrderAggregate.Events;

public class OrderDeletedEvent : BaseEvent
{
    private long Id { get; }

    public OrderDeletedEvent(long id)
    {
        Id = id;
    }
}