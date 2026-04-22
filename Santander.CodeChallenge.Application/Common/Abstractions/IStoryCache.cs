using Santander.CodeChallenge.Application.Stories.Models;

namespace Santander.CodeChallenge.Application.Common.Abstractions;

public interface IStoryCache
{
    Task<IReadOnlyList<long>?> GetBestStoryIdsAsync(CancellationToken cancellationToken);
    Task SetBestStoryIdsAsync(IReadOnlyList<long> ids, TimeSpan ttl, CancellationToken cancellationToken);

    Task<StoryResponse?> GetStoryAsync(long id, CancellationToken cancellationToken);
    Task SetStoryAsync(long id, StoryResponse story, TimeSpan ttl, CancellationToken cancellationToken);

    Task<IReadOnlyList<StoryResponse>?> GetBestStoriesSnapshotAsync(CancellationToken cancellationToken);
    Task SetBestStoriesSnapshotAsync(IReadOnlyList<StoryResponse> stories, TimeSpan ttl, CancellationToken cancellationToken);
}
