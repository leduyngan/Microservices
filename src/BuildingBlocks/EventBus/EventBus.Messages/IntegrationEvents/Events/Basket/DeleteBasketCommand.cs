namespace EventBus.Messages.IntegrationEvents.Events;

public record DeleteBasketCommand
{
    public Guid CorrelationId { get; init; }
    public string UserName { get; init; }
}