using MediatR;
using Ordering.Application.Common.Interfaces;
using Serilog;
using Shared.SeedWork;

namespace Ordering.Application.Features.V1.Orders.Commands.DeleteOrderByDocumentNo;

public class DeleteOrderByDocumentNoCommandHandler : IRequestHandler<DeleteOrderByDocumentNoCommand, ApiResult<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public DeleteOrderByDocumentNoCommandHandler(IOrderRepository orderRepository, ILogger logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResult<bool>> Handle(DeleteOrderByDocumentNoCommand request, CancellationToken cancellationToken)
    {
        var orderEntity = await _orderRepository.GetOrderByDocumentNo(request.DocumentNo);
        if(orderEntity is null) return new ApiResult<bool>(true);
        
        _orderRepository.Delete(orderEntity);
        orderEntity.DeletedOrder();
        await _orderRepository.SaveChangesAsync();
        
        _logger.Information($"Order {orderEntity.Id} was successfully deleted.");
        
        return new ApiResult<bool>(true);
    }
}