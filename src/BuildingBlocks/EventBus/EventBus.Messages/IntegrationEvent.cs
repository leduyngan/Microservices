namespace EventBus.Messages;

public record IntegrationEvent() : IIntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime CreationDate { get; } = DateTime.UtcNow;
    public Guid Id { get; set; }
}