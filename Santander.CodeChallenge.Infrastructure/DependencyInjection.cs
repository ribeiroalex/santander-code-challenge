using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Santander.CodeChallenge.Application.Common.Abstractions;
using Santander.CodeChallenge.Application.Stories.Services;
using Santander.CodeChallenge.Infrastructure.Background;
using Santander.CodeChallenge.Infrastructure.Caching;
using Santander.CodeChallenge.Infrastructure.Configuration;
using Santander.CodeChallenge.Infrastructure.HackerNews;
using Santander.CodeChallenge.Infrastructure.Stories;

namespace Santander.CodeChallenge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HackerNewsOptions>(configuration.GetSection(HackerNewsOptions.SectionName));
        services.Configure<StoriesCacheOptions>(configuration.GetSection(StoriesCacheOptions.SectionName));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "redis:6379";
            options.InstanceName = "santander-codechallenge";
        });

        services.AddHttpClient<IHackerNewsClient, HackerNewsClient>((serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<HackerNewsOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddScoped<IStoryCache, RedisStoryCache>();
        services.AddScoped<IBestStoriesService, BestStoriesService>();
        services.AddHostedService<StoryCacheWarmupService>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var jitter = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100 + jitter.Next(0, 100)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
