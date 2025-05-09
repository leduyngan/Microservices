using AutoMapper;
using EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using MediatR;
using Ordering.Application.Features.V1.Orders;
using Ordering.Application.Features.V1.Orders.Commands.CreateOrder;
using Ordering.Application.Features.V1.Orders.Commands.DeleteOrderByDocumentNo;
using Ordering.Application.Features.V1.Orders.Queries.GetOrderById;
using Shared.DTOs.Order;
using ILogger = Serilog.ILogger;


namespace Ordering.API.Application.IntegrationEvents.EventHandler;

// public class DeleteOrderByDocumentNoConsumer : IConsumer<DeleteOrderEvent>
// {
//     private readonly IMediator _mediator;
//     private readonly ILogger _logger;
//
//     public DeleteOrderByDocumentNoConsumer(IMediator mediator, ILogger logger)
//     {
//         _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//     }
//
//     public async Task Consume(ConsumeContext<DeleteOrderEvent> context)
//     {
//         var command = new DeleteOrderByDocumentNoCommand(context.Message.DocumentNo);
//         var result = await _mediator.Send(command);
//     }
// }

public class DeleteOrderConsumer : IConsumer<DeleteOrderEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public DeleteOrderConsumer(IMediator mediator, ILogger logger, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<DeleteOrderEvent> context)
    {
        _logger.Information("da vao deleteorder command handler");
        var command = new DeleteOrderCommand(context.Message.OrderId);
        var result = await _mediator.Send(command);
        await context.Publish(new OrderDeletedEvent
        {
            CorrelationId = context.Message.CorrelationId,
            Success = result.Data
        });
        
    }
}