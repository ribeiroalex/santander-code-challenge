using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Santander.CodeChallenge.Application.Common.Abstractions;
using Santander.CodeChallenge.Application.Stories.Models;

namespace Santander.CodeChallenge.Infrastructure.Caching;

public sealed class RedisStoryCache : IStoryCache
{
    private const string BestIdsKey = "hn:best:ids";
    private const string BestSnapshotKey = "hn:best:snapshot";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache _distributedCache;

    public RedisStoryCache(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<IReadOnlyList<long>?> GetBestStoryIdsAsync(CancellationToken cancellationToken)
        => await GetAsync<IReadOnlyList<long>>(BestIdsKey, cancellationToken);

    public async Task SetBestStoryIdsAsync(IReadOnlyList<long> ids, TimeSpan ttl, CancellationToken cancellationToken)
        => await SetAsync(BestIdsKey, ids, ttl, cancellationToken);

    public async Task<StoryResponse?> GetStoryAsync(long id, CancellationToken cancellationToken)
        => await GetAsync<StoryResponse>(StoryKey(id), cancellationToken);

    public async Task SetStoryAsync(long id, StoryResponse story, TimeSpan ttl, CancellationToken cancellationToken)
        => await SetAsync(StoryKey(id), story, ttl, cancellationToken);

    public async Task<IReadOnlyList<StoryResponse>?> GetBestStoriesSnapshotAsync(CancellationToken cancellationToken)
        => await GetAsync<IReadOnlyList<StoryResponse>>(BestSnapshotKey, cancellationToken);

    public async Task SetBestStoriesSnapshotAsync(IReadOnlyList<StoryResponse> stories, TimeSpan ttl, CancellationToken cancellationToken)
        => await SetAsync(BestSnapshotKey, stories, ttl, cancellationToken);

    private static string StoryKey(long id) => $"hn:story:{id}";

    private async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var payload = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }

    private async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(value, SerializerOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await _distributedCache.SetStringAsync(key, payload, options, cancellationToken);
    }
}
