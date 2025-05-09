namespace EventBus.Messages.IntegrationEvents.Events;

public interface IDeleteOrderEvent : IIntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public string DocumentNo { get; init; }
}