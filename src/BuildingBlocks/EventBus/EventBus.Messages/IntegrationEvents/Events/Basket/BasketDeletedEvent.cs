namespace EventBus.Messages.IntegrationEvents.Events;

public record BasketDeletedEvent
{
    public Guid CorrelationId { get; init; }
    public bool Success { get; init; }
}