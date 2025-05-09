using Shared.DTOs.Order;

namespace EventBus.Messages.IntegrationEvents.Events;

public interface ICreateOrderEvent : IIntegrationEvent
{
    public CreateOrderDto Order { get; init; }
}