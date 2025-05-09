namespace EventBus.Messages.IntegrationEvents.Events;

public interface IInventoryDeletedEvent : IIntegrationEvent
{
    public bool Success { get; init; }
}