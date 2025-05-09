using MassTransit;

namespace Saga.Orchestrator.Extensions;

public class BusHostedService : IHostedService
{
    private readonly IBusControl _busControl;

    public BusHostedService(IBusControl busControl)
    {
        _busControl = busControl;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _busControl.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _busControl.StopAsync(cancellationToken);
    }
}