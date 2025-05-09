namespace EventBus.Messages;

public interface IIntegrationEvent
{
    public Guid CorrelationId { get; init; }
    DateTime CreationDate { get; }
    Guid Id { get; set; }
}