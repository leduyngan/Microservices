namespace EventBus.Messages.IntegrationEvents.Events;

public record DeleteOrderEvent : IntegrationEvent, IDeleteOrderEvent
{
    public Guid CorrelationId { get; init; }
    public string DocumentNo { get; init; }
    public long OrderId { get; init; }
}