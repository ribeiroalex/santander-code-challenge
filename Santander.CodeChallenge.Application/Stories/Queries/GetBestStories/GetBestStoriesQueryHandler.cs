using MediatR;
using Santander.CodeChallenge.Application.Common.Models;
using Santander.CodeChallenge.Application.Common.Notifications;
using Santander.CodeChallenge.Application.Stories.Models;
using Santander.CodeChallenge.Application.Stories.Services;

namespace Santander.CodeChallenge.Application.Stories.Queries.GetBestStories;

public sealed class GetBestStoriesQueryHandler : IRequestHandler<GetBestStoriesQuery, ApiResult<IReadOnlyList<StoryResponse>>>
{
    private readonly IBestStoriesService _bestStoriesService;
    private readonly INotificationContext _notificationContext;

    public GetBestStoriesQueryHandler(IBestStoriesService bestStoriesService, INotificationContext notificationContext)
    {
        _bestStoriesService = bestStoriesService;
        _notificationContext = notificationContext;
    }

    public async Task<ApiResult<IReadOnlyList<StoryResponse>>> Handle(GetBestStoriesQuery request, CancellationToken cancellationToken)
    {
        var stories = await _bestStoriesService.GetBestStoriesAsync(request.Count, cancellationToken);

        if (_notificationContext.HasNotifications)
        {
            return ApiResult<IReadOnlyList<StoryResponse>>.Fail(_notificationContext.Notifications);
        }

        return ApiResult<IReadOnlyList<StoryResponse>>.Ok(stories);
    }
}
