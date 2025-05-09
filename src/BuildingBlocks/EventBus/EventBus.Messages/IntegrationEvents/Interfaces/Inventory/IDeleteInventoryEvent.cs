namespace EventBus.Messages.IntegrationEvents.Events;

public interface IDeleteInventoryEvent : IIntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public string DocumentNo { get; init; }
}