using FluentValidation;

namespace Santander.CodeChallenge.Application.Stories.Queries.GetBestStories;

public sealed class GetBestStoriesQueryValidator : AbstractValidator<GetBestStoriesQuery>
{
    public GetBestStoriesQueryValidator()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithMessage("n must be greater than zero.")
            .LessThanOrEqualTo(200)
            .WithMessage("n must be less than or equal to 200.");
    }
}
