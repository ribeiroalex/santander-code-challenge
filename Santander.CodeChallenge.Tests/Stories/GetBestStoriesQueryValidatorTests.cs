using Santander.CodeChallenge.Application.Stories.Queries.GetBestStories;

namespace Santander.CodeChallenge.Tests.Stories;

[TestClass]
public sealed class GetBestStoriesQueryValidatorTests
{
    [TestMethod]
    public async Task Validate_ShouldFail_WhenCountIsZero()
    {
        var validator = new GetBestStoriesQueryValidator();

        var result = await validator.ValidateAsync(new GetBestStoriesQuery(0));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenCountExceedsLimit()
    {
        var validator = new GetBestStoriesQueryValidator();

        var result = await validator.ValidateAsync(new GetBestStoriesQuery(201));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldPass_WhenCountInRange()
    {
        var validator = new GetBestStoriesQueryValidator();

        var result = await validator.ValidateAsync(new GetBestStoriesQuery(50));

        Assert.IsTrue(result.IsValid);
    }
}
