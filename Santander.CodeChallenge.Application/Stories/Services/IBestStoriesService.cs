using Santander.CodeChallenge.Application.Stories.Models;

namespace Santander.CodeChallenge.Application.Stories.Services;

public interface IBestStoriesService
{
    Task<IReadOnlyList<StoryResponse>> GetBestStoriesAsync(int count, CancellationToken cancellationToken);
    Task WarmupAsync(CancellationToken cancellationToken);
}
