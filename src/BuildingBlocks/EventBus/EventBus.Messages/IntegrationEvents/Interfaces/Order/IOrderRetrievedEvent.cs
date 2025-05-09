using Shared.DTOs.Order;

namespace EventBus.Messages.IntegrationEvents.Events;

public interface IOrderRetrievedEvent : IIntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public OrderDto Order { get; init; }
    public bool Success { get; init; }
}