namespace EventBus.Messages.IntegrationEvents.Events;

public record SagaCompletedEvent : IntegrationEvent, ISagaCompletedEvent
{
    public bool Success { get; init; }
}