using Grpc.Core;
using Inventory.Grpc.Client;
using Polly;
using Polly.Retry;
using ILogger = Serilog.ILogger;

namespace Basket.API.GrpcServices;

public class StockItemGrpcService
{
    private readonly StockProtoService.StockProtoServiceClient _stockProtoService;
    private readonly ILogger _logger;
    private readonly AsyncRetryPolicy<StockModel> _retryPolicy;

    public StockItemGrpcService(StockProtoService.StockProtoServiceClient stockProtoService, ILogger logger)
    {
        _stockProtoService = stockProtoService ?? throw new ArgumentNullException(nameof(stockProtoService));
        _logger = logger;
        _retryPolicy = Policy<StockModel>.Handle<RpcException>()
            .RetryAsync(3);
    }

    public async Task<StockModel> GetStock(string itemNo)
    {
        try
        {
            _logger.Information($"BEGIN: Get Stock StockItemGrpcService ItemNo: {itemNo}");
            var stockItemRequest = new GetStockRequest { ItemNo = itemNo };

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = await _stockProtoService.GetStockAsync(stockItemRequest);
                if(result != null)
                    _logger.Information($"END: Get Stock StockItemGrpcService ItemNo: {itemNo} - Stock value: {result.Quantity}");
                
                return result;
            });
        }
        catch (RpcException e)
        {
            _logger.Error(e, $"Grpc StockItemGrpcService failed: {e.Message}");
            return new StockModel
            {
                Quantity = -1
            };
        }
    }
}