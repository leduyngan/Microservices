using Shared.DTOs.Inventory;

namespace EventBus.Messages.IntegrationEvents.Events;

public interface IUpdateInventoryEvent : IIntegrationEvent
{
    public SalesOrderDto SalesOrder { get; init; }
}