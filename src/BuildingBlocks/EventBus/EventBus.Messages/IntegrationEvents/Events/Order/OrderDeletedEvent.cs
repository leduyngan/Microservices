namespace EventBus.Messages.IntegrationEvents.Events;

public record OrderDeletedEvent
{
    public Guid CorrelationId { get; init; }
    public bool Success { get; init; }
}