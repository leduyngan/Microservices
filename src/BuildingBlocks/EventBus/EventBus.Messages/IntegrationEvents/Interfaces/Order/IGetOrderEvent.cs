namespace EventBus.Messages.IntegrationEvents.Events;

public interface IGetOrderEvent : IIntegrationEvent
{
    public long OrderId { get; init; }
}