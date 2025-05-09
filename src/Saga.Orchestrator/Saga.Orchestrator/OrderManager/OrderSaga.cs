using AutoMapper;
using EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using Shared.DTOs.Basket;
using Shared.DTOs.Inventory;
using Shared.DTOs.Order;

public class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    private readonly Serilog.ILogger _logger;
    private readonly IMapper _mapper;
    
    public State OrderCreation { get; private set; }
    public State OrderRetrieval { get; private set; }
    public State InventoryUpdate { get; private set; }
    public State BasketDeletion { get; private set; }
    public State Rollback { get; private set; }
    public State OrderDeletion { get; private set; }
    public State Failed { get; private set; }
    public State Completed { get; private set; }

    public MassTransit.Event<BasketRetrievedEvent> BasketRetrieved { get; private set; }
    public MassTransit.Event<OrderCreatedEvent> OrderCreated { get; private set; }
    public MassTransit.Event<OrderRetrievedEvent> OrderRetrieved { get; private set; }
    public MassTransit.Event<InventoryUpdatedEvent> InventoryUpdated { get; private set; }
    public MassTransit.Event<BasketDeletedEvent> BasketDeleted { get; private set; }
    public MassTransit.Event<InventoryDeletedEvent> InventoryDeleted { get; private set; }
    public MassTransit.Event<OrderDeletedEvent> OrderDeleted { get; private set; }

    
    public OrderSaga(Serilog.ILogger logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
        CartDto cartDto = null;
        long orderId = 0;

        InstanceState(x => x.CurrentState);

        Event(() => BasketRetrieved, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => OrderCreated, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => OrderRetrieved, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => InventoryUpdated, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => BasketDeleted, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => InventoryDeleted, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => OrderDeleted, x => x.CorrelateById(context => context.Message.CorrelationId));

        Initially(
            When(BasketRetrieved)
                .ThenAsync(async context =>
                {
                    if (context.Message.Success)
                    {
                        context.Instance.Cart = context.Message.Cart;
                        cartDto = context.Instance.Cart;
                        context.Instance.UserName = context.Message.Cart.Username; // Store UserName
                        
                        var order = _mapper.Map<CreateOrderDto>(context.Message.Cart);
                        order.TotalPrice = context.Message.Cart.TotalPrice;
                        order.FirstName = context.Message.Cart.Username;
                        order.LastName = context.Message.Cart.Username;
                        order.InvoiceAddress = context.Message.Cart.Username;
                        order.ShippingAddress = context.Message.Cart.Username;
                        
                        _logger.Information("Basket retrieved for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        await context.Publish(new CreateOrderEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            Order = order
                        });
                        context.Instance.CurrentState = OrderCreation;
                    }
                    else
                    {
                        _logger.Error("Basket retrieval failed for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        await context.Publish(new SagaCompletedEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            Success = false
                        });
                        context.Instance.CurrentState = Failed;
                    }
                })
        );

        During(OrderCreation, 
            When(OrderCreated)
                .ThenAsync(async context =>
                {
                    if (context.Message.Success)
                    {
                        context.Instance.OrderId = context.Message.OrderId;
                        orderId = context.Instance.OrderId;
                        _logger.Information("Order created with OrderId: {OrderId}", context.Message.OrderId);
                        await context.Publish(new GetOrderEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            OrderId = context.Message.OrderId
                        });
                        context.Instance.CurrentState = OrderRetrieval;
                    }
                    else
                    {
                        _logger.Error("Order creation failed for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        await context.Publish(new SagaCompletedEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            Success = false
                        });
                        context.Instance.CurrentState = Failed;
                    }
                })
        );

        During(OrderRetrieval,
            When(OrderRetrieved)
                .ThenAsync(async context =>
                {
                    if (context.Message.Success)
                    {
                        context.Instance.Order = context.Message.Order;
                        _logger.Information("Order retrieved for OrderId: {OrderId}", context.Instance.OrderId);
                        await context.Publish(new UpdateInventoryEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            SalesOrder = new SalesOrderDto
                            {
                                OrderNo = context.Message.Order.DocumentNo,
                                SaleItems = _mapper.Map<List<SaleItemDto>>(cartDto.Items)
                            }
                        });
                        context.Instance.CurrentState = InventoryUpdate;
                    }
                    else
                    {
                        _logger.Error("Order retrieval failed for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        await context.Publish(new DeleteOrderEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            OrderId = orderId
                        });
                        await context.Publish(new SagaCompletedEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            Success = false
                        });
                        context.Instance.CurrentState = Rollback;
                    }
                })
        );

        During(InventoryUpdate,
            When(InventoryUpdated)
                .ThenAsync(async context =>
                {
                    if (context.Message.Success)
                    {
                        context.Instance.InventoryDocumentNo = context.Message.DocumentNo;
                        _logger.Information("Inventory updated with DocumentNo: {DocumentNo}", context.Message.DocumentNo);
                        await context.Publish(new DeleteBasketCommand
                        {
                            CorrelationId = context.Message.CorrelationId,
                            UserName = context.Instance.UserName
                        });
                        context.Instance.CurrentState = BasketDeletion;
                    }
                    else
                    {
                        _logger.Error("Inventory update failed for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        await context.Publish(new DeleteOrderEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            OrderId = orderId
                        });
                        await context.Publish(new SagaCompletedEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            Success = false
                        });
                        context.Instance.CurrentState = Rollback;
                    }
                })
        );

        During(BasketDeletion,
            When(BasketDeleted)
                .ThenAsync(async context =>
                {
                    if (context.Message.Success)
                    {
                        _logger.Information("Basket deleted for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        await context.Publish(new SagaCompletedEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            Success = true
                        });
                        context.Instance.CurrentState = Completed;
                    }
                    else
                    {
                        _logger.Error("Basket deletion failed for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        await context.Publish(new DeleteInventoryEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            DocumentNo = context.Instance.InventoryDocumentNo
                        });
                        // await context.Publish(new SagaCompletedEvent
                        // {
                        //     CorrelationId = context.Message.CorrelationId,
                        //     Success = false
                        // });
                        context.Instance.CurrentState = Rollback;
                    }
                })
        );

        During(Rollback,
            When(InventoryDeleted)
                .ThenAsync(async context =>
                {
                    _logger.Information("Inventory rolled back for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                    await context.Publish(new DeleteOrderEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        OrderId = orderId
                    });
                    context.Instance.CurrentState = OrderDeletion;
                })
        );

        During(OrderDeletion,
            When(OrderDeleted)
                .ThenAsync(async context =>
                {
                    _logger.Information("Order rolled back for CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                    await context.Publish(new SagaCompletedEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Success = false
                    });
                    context.Instance.CurrentState = Failed;
                })
        );

        SetCompletedWhenFinalized();
        _logger.Information("SetCompletedWhenFinalized() called for OrderSaga");
    }

}

// Order Saga State
public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public State CurrentState { get; set; }
    public string UserName { get; set; }
    public CartDto Cart { get; set; }
    public long OrderId { get; set; }
    public OrderDto Order { get; set; }
    public string InventoryDocumentNo { get; set; }
}