namespace EventBus.Messages.IntegrationEvents.Interfaces;

public interface IGetBasketCommand : IIntegrationEvent
{
    public string UserName { get; init; }
}