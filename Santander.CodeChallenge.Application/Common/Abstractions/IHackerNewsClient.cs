namespace Santander.CodeChallenge.Application.Common.Abstractions;

public sealed record HackerNewsStoryItem(
    long Id,
    string? Title,
    string? Url,
    string? By,
    long Time,
    int Score,
    int Descendants,
    string? Type);

public interface IHackerNewsClient
{
    Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken);
    Task<HackerNewsStoryItem?> GetStoryByIdAsync(long id, CancellationToken cancellationToken);
}
