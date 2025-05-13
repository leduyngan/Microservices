using MassTransit;
using Shared.DTOs.Basket;
using Shared.DTOs.Order;

namespace Saga.Orchestrator.OrderManager;

public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string UserName { get; set; }
    public CartDto Cart { get; set; }
    public long OrderId { get; set; }
    public OrderDto Order { get; set; }
    public string InventoryDocumentNo { get; set; }
}