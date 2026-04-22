namespace Santander.CodeChallenge.Infrastructure.Configuration;

public sealed class StoriesCacheOptions
{
    public const string SectionName = "StoriesCache";

    public int StoryTtlHours { get; init; } = 24;
    public int SnapshotTtlHours { get; init; } = 24;
    public int BestIdsTtlHours { get; init; } = 24;
    public int WarmupStoryCount { get; init; } = 200;
    public int MaxFetchStories { get; init; } = 500;
    public int MaxFetchConcurrency { get; init; } = 20;
    public int RefreshIntervalMinutes { get; init; } = 720;
}
