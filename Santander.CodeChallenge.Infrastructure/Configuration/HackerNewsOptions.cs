namespace Santander.CodeChallenge.Infrastructure.Configuration;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    public string BaseUrl { get; init; } = "https://hacker-news.firebaseio.com/";
    public int TimeoutSeconds { get; init; } = 2;
}
