using Moq;
using Santander.CodeChallenge.Application.Common.Notifications;
using Santander.CodeChallenge.Application.Stories.Models;
using Santander.CodeChallenge.Application.Stories.Queries.GetBestStories;
using Santander.CodeChallenge.Application.Stories.Services;

namespace Santander.CodeChallenge.Tests.Stories;

[TestClass]
public sealed class GetBestStoriesQueryHandlerTests
{
    [TestMethod]
    public async Task Handle_ShouldReturnStories_WhenServiceSucceeds()
    {
        var serviceMock = new Mock<IBestStoriesService>();
        var notificationContext = new NotificationContext();

        serviceMock
            .Setup(x => x.GetBestStoriesAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new StoryResponse("Title 1", "https://example.com/1", "alice", DateTimeOffset.UtcNow, 200, 10),
                new StoryResponse("Title 2", "https://example.com/2", "bob", DateTimeOffset.UtcNow, 150, 2)
            });

        var handler = new GetBestStoriesQueryHandler(serviceMock.Object, notificationContext);

        var result = await handler.Handle(new GetBestStoriesQuery(2), CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnFailure_WhenNotificationsExist()
    {
        var serviceMock = new Mock<IBestStoriesService>();
        var notificationContext = new NotificationContext();
        notificationContext.Add("Failed to load stories from upstream source.");

        serviceMock
            .Setup(x => x.GetBestStoriesAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<StoryResponse>());

        var handler = new GetBestStoriesQueryHandler(serviceMock.Object, notificationContext);

        var result = await handler.Handle(new GetBestStoriesQuery(2), CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(1, result.Errors.Count);
    }
}
