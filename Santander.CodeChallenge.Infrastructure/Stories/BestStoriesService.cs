using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Santander.CodeChallenge.Application.Common.Abstractions;
using Santander.CodeChallenge.Application.Common.Notifications;
using Santander.CodeChallenge.Application.Stories.Models;
using Santander.CodeChallenge.Application.Stories.Services;
using Santander.CodeChallenge.Infrastructure.Configuration;

namespace Santander.CodeChallenge.Infrastructure.Stories;

public sealed class BestStoriesService : IBestStoriesService
{
    private readonly IHackerNewsClient _hackerNewsClient;
    private readonly IStoryCache _storyCache;
    private readonly INotificationContext _notificationContext;
    private readonly StoriesCacheOptions _cacheOptions;
    private readonly ILogger<BestStoriesService> _logger;

    public BestStoriesService(
        IHackerNewsClient hackerNewsClient,
        IStoryCache storyCache,
        INotificationContext notificationContext,
        IOptions<StoriesCacheOptions> cacheOptions,
        ILogger<BestStoriesService> logger)
    {
        _hackerNewsClient = hackerNewsClient;
        _storyCache = storyCache;
        _notificationContext = notificationContext;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StoryResponse>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
    {
        var snapshot = await _storyCache.GetBestStoriesSnapshotAsync(cancellationToken);
        if (snapshot is not null && snapshot.Count >= count)
        {
            return snapshot
                .OrderByDescending(static story => story.Score)
                .Take(count)
                .ToArray();
        }

        await RefreshSnapshotAsync(Math.Max(count, _cacheOptions.WarmupStoryCount), cancellationToken);

        var refreshedSnapshot = await _storyCache.GetBestStoriesSnapshotAsync(cancellationToken);
        if (refreshedSnapshot is not null && refreshedSnapshot.Count > 0)
        {
            return refreshedSnapshot
                .OrderByDescending(static story => story.Score)
                .Take(count)
                .ToArray();
        }

        _notificationContext.Add("Unable to retrieve stories at this time.");
        return Array.Empty<StoryResponse>();
    }

    public Task WarmupAsync(CancellationToken cancellationToken)
        => RefreshSnapshotAsync(_cacheOptions.WarmupStoryCount, cancellationToken);

    private async Task RefreshSnapshotAsync(int requestedCount, CancellationToken cancellationToken)
    {
        try
        {
            var storyIds = await _storyCache.GetBestStoryIdsAsync(cancellationToken);
            if (storyIds is null || storyIds.Count == 0)
            {
                storyIds = await _hackerNewsClient.GetBestStoryIdsAsync(cancellationToken);
                if (storyIds.Count > 0)
                {
                    await _storyCache.SetBestStoryIdsAsync(
                        storyIds,
                        TimeSpan.FromHours(_cacheOptions.BestIdsTtlHours),
                        cancellationToken);
                }
            }

            if (storyIds is null || storyIds.Count == 0)
            {
                _notificationContext.Add("Hacker News best stories were not available.");
                return;
            }

            var targetCount = Math.Min(
                _cacheOptions.MaxFetchStories,
                Math.Max(requestedCount * 3, requestedCount));
            var targetIds = storyIds.Take(targetCount).ToArray();

            var stories = new ConcurrentBag<StoryResponse>();

            await Parallel.ForEachAsync(
                targetIds,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = _cacheOptions.MaxFetchConcurrency,
                    CancellationToken = cancellationToken
                },
                async (storyId, token) =>
                {
                    var cached = await _storyCache.GetStoryAsync(storyId, token);
                    if (cached is not null)
                    {
                        stories.Add(cached);
                        return;
                    }

                    var item = await _hackerNewsClient.GetStoryByIdAsync(storyId, token);
                    if (item is null)
                    {
                        return;
                    }

                    var mapped = Map(item);
                    await _storyCache.SetStoryAsync(storyId, mapped, TimeSpan.FromHours(_cacheOptions.StoryTtlHours), token);
                    stories.Add(mapped);
                });

            var snapshot = stories
                .OrderByDescending(static story => story.Score)
                .ToArray();

            if (snapshot.Length == 0)
            {
                _notificationContext.Add("No stories available right now.");
                return;
            }

            await _storyCache.SetBestStoriesSnapshotAsync(
                snapshot,
                TimeSpan.FromHours(_cacheOptions.SnapshotTtlHours),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh best stories snapshot.");
            _notificationContext.Add("Failed to load stories from upstream source.");
        }
    }

    private static StoryResponse Map(HackerNewsStoryItem item)
    {
        var unixTime = item.Time <= 0 ? DateTimeOffset.UtcNow : DateTimeOffset.FromUnixTimeSeconds(item.Time);
        var uri = string.IsNullOrWhiteSpace(item.Url) ? string.Empty : item.Url;
        var title = string.IsNullOrWhiteSpace(item.Title) ? "Untitled" : item.Title;
        var postedBy = string.IsNullOrWhiteSpace(item.By) ? "unknown" : item.By;

        return new StoryResponse(title, uri, postedBy, unixTime, item.Score, item.Descendants);
    }
}
