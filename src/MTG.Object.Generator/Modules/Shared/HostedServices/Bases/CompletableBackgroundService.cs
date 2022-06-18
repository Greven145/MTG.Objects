using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;

namespace MTG.Object.Generator.Modules.Shared.HostedServices.Bases;

internal abstract class CompletableBackgroundService : BackgroundService {
    private static readonly List<CompletableBackgroundService> Services = new();
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private bool _isComplete;

    protected CompletableBackgroundService(IHostApplicationLifetime hostApplicationLifetime) {
        _hostApplicationLifetime = Guard.Against.Null(hostApplicationLifetime);
    }

    public override Task StartAsync(CancellationToken cancellationToken) {
        Services.Add(this);
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _isComplete = true;
        while (!stoppingToken.IsCancellationRequested) {
            await StopOrWait(stoppingToken);
        }
    }

    private async Task StopOrWait(CancellationToken stoppingToken) {
        if (Services.All(s => s._isComplete)) {
            _hostApplicationLifetime.StopApplication();
        }

        await Task.Delay(250, stoppingToken);
    }
}