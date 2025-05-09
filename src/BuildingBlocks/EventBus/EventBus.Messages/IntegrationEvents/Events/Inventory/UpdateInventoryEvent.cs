using Shared.DTOs.Inventory;

namespace EventBus.Messages.IntegrationEvents.Events;

public record UpdateInventoryEvent : IntegrationEvent
{
    public SalesOrderDto SalesOrder { get; init; }
}