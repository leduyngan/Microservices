namespace EventBus.Messages.IntegrationEvents.Events;

public interface IOrderCreatedEvent : IIntegrationEvent
{
    public long OrderId { get; init; }
    public bool Success { get; init; }
}