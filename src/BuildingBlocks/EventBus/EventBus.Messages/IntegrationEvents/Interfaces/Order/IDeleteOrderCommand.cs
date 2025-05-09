namespace EventBus.Messages.IntegrationEvents.Events;

public interface IDeleteOrderCommand : IIntegrationEvent
{
    public long OrderId { get; init; }
}