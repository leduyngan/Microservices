namespace EventBus.Messages.IntegrationEvents.Interfaces;

public interface IBasketDeletedEvent : IIntegrationEvent
{
    public bool Success { get; init; }
}