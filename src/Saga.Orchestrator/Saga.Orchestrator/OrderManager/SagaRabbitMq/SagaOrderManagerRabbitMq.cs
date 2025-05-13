using MassTransit;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Sagas.OrderManager;
using EventBus.Messages.IntegrationEvents.Events;
using Saga.Orchestrator.OrderManager;
using Shared.DTOs.Basket;

public class SagaOrderManagerRabbitMq : ISagaOrderManagerRabbitMq<BasketCheckoutDto, OrderResponse>
{
    private readonly IBusControl _busControl;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;
    private readonly Serilog.ILogger _logger;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _sagaCompletions;

    public SagaOrderManagerRabbitMq(
        IBusControl busControl,
        IMapper mapper,
        Serilog.ILogger logger,
        IPublishEndpoint publishEndpoint, ConcurrentDictionary<Guid, TaskCompletionSource<bool>> sagaCompletions)
    {
        _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint;
        _sagaCompletions = sagaCompletions;
    }


    public async Task<OrderResponse> CreateOrder(BasketCheckoutDto input)
    {
        try
        {
            var correlationId = Guid.NewGuid();
            _logger.Information("Starting Saga for order creation with CorrelationId: {CorrelationId}", correlationId);

            // Thêm TaskCompletionSource vào dictionary
            var tcs = new TaskCompletionSource<bool>();
            _sagaCompletions.TryAdd(correlationId, tcs);

            await _busControl.Publish(new GetBasketCommand
            {
                CorrelationId = correlationId,
                UserName = input.UserName
            });

            // Chờ hoàn thành
            var result = await WaitForSagaCompletion(correlationId);
            return new OrderResponse(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initiate order creation saga");
            return new OrderResponse(false);
        }
    }

    public Task<OrderResponse> RollbackOrder(string username, string documentNo, long orderId)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> WaitForSagaCompletion(Guid correlationId)
    {
        var timeout = TimeSpan.FromMinutes(5);

        try
        {
            if (!_sagaCompletions.TryGetValue(correlationId, out var tcs))
            {
                _logger.Error("No TaskCompletionSource found for CorrelationId: {CorrelationId}", correlationId);
                return false;
            }

            using (var cts = new CancellationTokenSource(timeout))
            {
                var result = await tcs.Task.WaitAsync(cts.Token);
                _logger.Information("Saga completed with result {Result} for CorrelationId: {CorrelationId}", result, correlationId);
                return result;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Error("Saga timed out for CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error waiting for saga completion for CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
        finally
        {
            _sagaCompletions.TryRemove(correlationId, out _);
        }
    }
}