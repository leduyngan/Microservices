using Shared.DTOs.Order;

namespace EventBus.Messages.IntegrationEvents.Events;

public record CreateOrderEvent : IntegrationEvent, ICreateOrderEvent
{
    public CreateOrderDto Order { get; init; }
}