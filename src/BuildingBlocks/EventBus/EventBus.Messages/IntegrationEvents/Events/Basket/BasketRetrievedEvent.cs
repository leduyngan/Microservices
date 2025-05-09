using Shared.DTOs.Basket;

namespace EventBus.Messages.IntegrationEvents.Events;

public record BasketRetrievedEvent
{
    public Guid CorrelationId { get; init; }
    public CartDto Cart { get; init; }
    public bool Success { get; init; }
}