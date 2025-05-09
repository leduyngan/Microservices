namespace EventBus.Messages.IntegrationEvents.Events;

public interface IInventoryUpdatedEvent : IIntegrationEvent
{
    public string DocumentNo { get; init; }
    public bool Success { get; init; }
}