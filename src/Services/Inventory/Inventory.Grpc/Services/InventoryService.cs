using Grpc.Core;
using Inventory.Grpc.Repositories.Interfaces;
using ILogger = Serilog.ILogger;

namespace Inventory.Grpc.Services;
using Inventory.Grpc.Protos;

public class InventoryService : StockProtoService.StockProtoServiceBase
{
    private readonly Serilog.ILogger _logger;
    private IInventoryRepository _repository;

    public InventoryService(ILogger logger, IInventoryRepository repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public override async Task<StockModel> GetStock(GetStockRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN Get Stock of ItemNo: {request.ItemNo}");
        var stockQuantity = await _repository.GetStockQuantity(request.ItemNo);
        var result = new StockModel()
        {
            Quantity = stockQuantity
        };
        _logger.Information($"END Get Stock of ItemNo: {request.ItemNo} - Quantity: {stockQuantity}");
        
        return result;
    }
}