using MediatR;
using Santander.CodeChallenge.Application.Common.Models;
using Santander.CodeChallenge.Application.Stories.Models;

namespace Santander.CodeChallenge.Application.Stories.Queries.GetBestStories;

public sealed record GetBestStoriesQuery(int Count) : IRequest<ApiResult<IReadOnlyList<StoryResponse>>>;
