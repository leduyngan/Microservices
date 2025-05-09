using AutoMapper;
using EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using MediatR;
using Ordering.Application.Features.V1.Orders;
using Ordering.Application.Features.V1.Orders.Commands.CreateOrder;
using Ordering.Application.Features.V1.Orders.Queries.GetOrderById;
using Shared.DTOs.Order;
using ILogger = Serilog.ILogger;


namespace Ordering.API.Application.IntegrationEvents.EventHandler;

public class GetOrderConsumer : IConsumer<GetOrderEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetOrderConsumer(IMediator mediator, ILogger logger, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetOrderEvent> context)
    {
        var query = new GetOrderByIdQuery(context.Message.OrderId);
        var result = await _mediator.Send(query);
        var order = _mapper.Map<Shared.DTOs.Order.OrderDto>(result.Data);
        await context.Publish(new OrderRetrievedEvent
        {
            CorrelationId = context.Message.CorrelationId,
            Order = order,
            Success = result != null
        });
    }
}