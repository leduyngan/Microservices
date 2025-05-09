namespace EventBus.Messages.IntegrationEvents.Events;

public interface IOrderDeletedEvent : IIntegrationEvent
{
    public bool Success { get; init; }
}