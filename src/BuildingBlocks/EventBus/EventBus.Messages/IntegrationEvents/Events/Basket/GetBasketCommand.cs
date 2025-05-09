using EventBus.Messages.IntegrationEvents.Interfaces;

namespace EventBus.Messages.IntegrationEvents.Events;

public record GetBasketCommand : IntegrationEvent, IGetBasketCommand
{
    public string UserName { get; init; }
}