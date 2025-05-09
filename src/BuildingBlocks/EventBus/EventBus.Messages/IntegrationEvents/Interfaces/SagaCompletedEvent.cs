namespace EventBus.Messages.IntegrationEvents.Events;

public interface ISagaCompletedEvent : IIntegrationEvent
{
    public bool Success { get; init; }
}