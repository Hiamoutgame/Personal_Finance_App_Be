using Personal_Finance_Management.Service.Common.Constants;
using BroadcastService = Personal_Finance_Management.Service.broadcast;

namespace Personal_Finance_Management.Api.Jobs;

public class BroadcastDispatchBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BroadcastDispatchBackgroundService> _logger;

    public BroadcastDispatchBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<BroadcastDispatchBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _configuration.GetValue(ConfigKeys.BroadcastDispatch.IntervalSeconds, IntegrationDefaults.BroadcastDispatchIntervalSeconds);
        if (intervalSeconds <= 0)
        {
            intervalSeconds = IntegrationDefaults.BroadcastDispatchIntervalSeconds;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        await DispatchDueBroadcasts(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DispatchDueBroadcasts(stoppingToken);
        }
    }

    private async Task DispatchDueBroadcasts(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var broadcastService = scope.ServiceProvider.GetRequiredService<BroadcastService.IService>();
            var dispatchedCount = await broadcastService.DispatchDueBroadcastsAsync(cancellationToken);

            if (dispatchedCount > 0)
            {
                _logger.LogInformation("Dispatched {BroadcastCount} scheduled broadcasts.", dispatchedCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled broadcast dispatch failed.");
        }
    }
}
