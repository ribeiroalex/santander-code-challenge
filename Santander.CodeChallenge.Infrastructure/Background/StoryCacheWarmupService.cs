using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Santander.CodeChallenge.Application.Stories.Services;
using Santander.CodeChallenge.Infrastructure.Configuration;

namespace Santander.CodeChallenge.Infrastructure.Background;

public sealed class StoryCacheWarmupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly StoriesCacheOptions _cacheOptions;
    private readonly ILogger<StoryCacheWarmupService> _logger;

    public StoryCacheWarmupService(
        IServiceScopeFactory scopeFactory,
        IOptions<StoriesCacheOptions> cacheOptions,
        ILogger<StoryCacheWarmupService> logger)
    {
        _scopeFactory = scopeFactory;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WarmupOnce(stoppingToken);

        var interval = TimeSpan.FromMinutes(Math.Max(1, _cacheOptions.RefreshIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await WarmupOnce(stoppingToken);
        }
    }

    private async Task WarmupOnce(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bestStoriesService = scope.ServiceProvider.GetRequiredService<IBestStoriesService>();
            await bestStoriesService.WarmupAsync(cancellationToken);
            _logger.LogInformation("Hacker News cache warmup finished.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hacker News cache warmup failed.");
        }
    }
}
