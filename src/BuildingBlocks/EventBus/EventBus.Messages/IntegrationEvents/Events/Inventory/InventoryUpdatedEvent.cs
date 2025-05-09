namespace EventBus.Messages.IntegrationEvents.Events;

public record InventoryUpdatedEvent : IntegrationEvent, IInventoryUpdatedEvent
{
    public Guid CorrelationId { get; init; }
    public string DocumentNo { get; init; }
    public bool Success { get; init; }
}