using System.Net.Http.Json;
using Santander.CodeChallenge.Application.Common.Abstractions;

namespace Santander.CodeChallenge.Infrastructure.HackerNews;

public sealed class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;

    public HackerNewsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        var ids = await _httpClient.GetFromJsonAsync<IReadOnlyList<long>>("v0/beststories.json", cancellationToken);
        return ids ?? Array.Empty<long>();
    }

    public async Task<HackerNewsStoryItem?> GetStoryByIdAsync(long id, CancellationToken cancellationToken)
    {
        var story = await _httpClient.GetFromJsonAsync<HackerNewsStoryItem>($"v0/item/{id}.json", cancellationToken);
        if (story is null || !string.Equals(story.Type, "story", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return story;
    }
}
