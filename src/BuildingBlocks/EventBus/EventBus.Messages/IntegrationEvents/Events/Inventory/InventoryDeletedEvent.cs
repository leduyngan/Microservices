namespace EventBus.Messages.IntegrationEvents.Events;

public record InventoryDeletedEvent : IntegrationEvent, IInventoryDeletedEvent
{
    public Guid CorrelationId { get; init; }
    public bool Success { get; init; }
}