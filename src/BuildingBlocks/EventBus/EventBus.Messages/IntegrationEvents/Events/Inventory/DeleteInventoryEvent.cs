namespace EventBus.Messages.IntegrationEvents.Events;

public record DeleteInventoryEvent : IntegrationEvent, IDeleteInventoryCommand
{
    public Guid CorrelationId { get; init; }
    public string DocumentNo { get; init; }
}