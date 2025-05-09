using AutoMapper;
using EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using MediatR;
using Ordering.Application.Features.V1.Orders;
using Ordering.Application.Features.V1.Orders.Commands.CreateOrder;
using ILogger = Serilog.ILogger;


namespace Ordering.API.Application.IntegrationEvents.EventHandler;

public class CreateOrderConsumer : IConsumer<CreateOrderEvent>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public CreateOrderConsumer(IMediator mediator, IMapper mapper, ILogger logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<CreateOrderEvent> context)
    {
        // can call through mediator or directly(service or repository)
        var command = _mapper.Map<CreateOrderCommand>(context.Message.Order);
        var result = await _mediator.Send(command);
        
        await context.Publish(new OrderCreatedEvent
        {
            CorrelationId = context.Message.CorrelationId,
            OrderId = result.Data,
            Success = result != null
        });
        
        _logger.Information("BasketCheckoutEvent consumed successfully. " +
                            "Order is created with Id: {newOrderId}", result.Data);
    }
}