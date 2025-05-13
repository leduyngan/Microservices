using System.Collections.Concurrent;
using EventBus.Messages.IntegrationEvents.Events;

namespace Saga.Orchestrator.OrderManager;

public class SagaCompletedConsumer : MassTransit.IConsumer<SagaCompletedEvent>
{
    private readonly Serilog.ILogger _logger;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _sagaCompletions;

    public SagaCompletedConsumer(Serilog.ILogger logger, ConcurrentDictionary<Guid, TaskCompletionSource<bool>> sagaCompletions)
    {
        _logger = logger;
        _sagaCompletions = sagaCompletions;
    }

    public Task Consume(MassTransit.ConsumeContext<SagaCompletedEvent> context)
    {
        var correlationId = context.Message.CorrelationId;
        _logger.Information("Received SagaCompletedEvent for CorrelationId: {CorrelationId}, Success: {Success}",
            correlationId, context.Message.Success);
        var isGetValue = _sagaCompletions.TryGetValue(correlationId, out var tcs);
                
        if (isGetValue)
        {
            tcs.TrySetResult(context.Message.Success);
        }
        else
        {
            _logger.Warning("No TaskCompletionSource found for CorrelationId: {CorrelationId}", correlationId);
        }

        return Task.CompletedTask;
    }
}