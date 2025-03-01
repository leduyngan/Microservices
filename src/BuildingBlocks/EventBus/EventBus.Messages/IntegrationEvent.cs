namespace EventBus.Messages;

public record IntegrationEvent() : IIntegrationEvent
{
    public DateTime CreationDate { get; } = DateTime.UtcNow;
    public Guid Id { get; set; }
}