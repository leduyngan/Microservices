using Shared.DTOs.Basket;

namespace EventBus.Messages.IntegrationEvents.Interfaces;

public interface IBasketRetrievedEvent : IIntegrationEvent
{
    public CartDto Cart { get; init; }
    public bool Success { get; init; }
}