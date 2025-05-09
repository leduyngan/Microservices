namespace EventBus.Messages.IntegrationEvents.Interfaces;

public interface IDeleteBasketCommand : IIntegrationEvent
{
    public string UserName { get; init; }
}