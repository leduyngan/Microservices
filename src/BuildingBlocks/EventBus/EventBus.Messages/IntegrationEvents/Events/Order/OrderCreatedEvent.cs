namespace EventBus.Messages.IntegrationEvents.Events;

public record OrderCreatedEvent : IntegrationEvent, IOrderCreatedEvent
{
    public Guid CorrelationId { get; init; }
    public long OrderId { get; init; }
    public bool Success { get; init; }
}