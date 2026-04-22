using MediatR;
using Microsoft.AspNetCore.Mvc;
using Santander.CodeChallenge.Application.Stories.Queries.GetBestStories;

namespace Santander.CodeChallenge.Api.Controllers;

[ApiController]
[Route("v1/stories")]
public sealed class StoriesController : ControllerBase
{
    [HttpGet("best")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetBestStories([FromQuery(Name = "n")] int n, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBestStoriesQuery(n), cancellationToken);

        if (result.Success)
        {
            return Ok(result.Data);
        }

        var message = string.Join("; ", result.Errors);
        if (result.Errors.Any(static error => error.Contains("Unable", StringComparison.OrdinalIgnoreCase) || error.Contains("Failed", StringComparison.OrdinalIgnoreCase)))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = result.Errors, message });
        }

        return BadRequest(new { errors = result.Errors, message });
    }
}
