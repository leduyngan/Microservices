namespace EventBus.Messages.IntegrationEvents.Events;

public record GetOrderEvent : IntegrationEvent, IGetOrderEvent
{
    public long OrderId { get; init; }
}