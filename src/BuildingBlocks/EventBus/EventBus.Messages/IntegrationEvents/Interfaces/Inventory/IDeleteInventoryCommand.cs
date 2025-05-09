namespace EventBus.Messages.IntegrationEvents.Events;

public interface IDeleteInventoryCommand : IIntegrationEvent
{
    public string DocumentNo { get; init; }
}