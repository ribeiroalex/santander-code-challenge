# Santander - Developer Coding Test

## Objective
Implement a REST API in ASP.NET Core that returns the best `n` stories from Hacker News, sorted by score descending, with the response contract:

```json
[
	{
		"title": "A uBlock Origin update was rejected from the Chrome Web Store",
		"uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
		"postedBy": "ismaildonmez",
		"time": "2019-10-12T13:43:01+00:00",
		"score": 1716,
		"commentCount": 572
	}
]
```

## Solution Structure

- `Santander.CodeChallenge.Api`:
	- HTTP surface (`GET /v1/stories/best?n={n}`)
	- Health endpoints (`/health/live`, `/health/ready`)
	- Startup and DI wiring
- `Santander.CodeChallenge.Application`:
	- MediatR request/handler (`GetBestStoriesQuery`)
	- FluentValidation request validation
	- Notification context for error collection
	- Response contracts
- `Santander.CodeChallenge.Infrastructure`:
	- Typed Hacker News client
	- Polly retry + circuit breaker
	- Redis cache adapter
	- Cache warmup hosted service
- `Santander.CodeChallenge.Tests`:
	- Unit tests for request validation and handler behavior

## Architecture Notes

### CQRS + Vertical Slice
The API is implemented as a single vertical slice for the read scenario:

- Query: `GetBestStoriesQuery`
- Handler: `GetBestStoriesQueryHandler`
- Service orchestration: cache-first retrieval with fallback refresh

This keeps the scope simple and aligned to the challenge requirements.

### MediatR + Notifications

- MediatR handles request dispatch and handlers.
- Validation is applied through a MediatR pipeline behavior.
- Notification context aggregates request errors and infrastructure failures.
- The controller maps notifications to `400` (validation) or `503` (service unavailable).

### Resilience (Polly)

The Hacker News client uses:

- Retry with exponential backoff + jitter for transient failures.
- Circuit breaker to stop hammering upstream during unstable periods.

### Cache Strategy (Redis)

- Cache-first reads are used to keep responses fast and stable.
- Best IDs, story items, and a pre-ranked snapshot are cached.
- Default TTL is 24 hours (configurable).
- On startup, a background service primes and periodically refreshes cache.

This reduces direct dependency on Hacker News during normal traffic and helps keep response times under 1 second for warm-cache requests.

## API Contract

### Endpoint

- `GET /v1/stories/best?n={n}`


### Status Codes

- `200 OK`: successful retrieval.
- `400 Bad Request`: validation/notification errors.
- `503 Service Unavailable`: upstream/cache fetch problems.

## Configuration

`Santander.CodeChallenge.Api/appsettings.json`:

- `ConnectionStrings:Redis`
- `HackerNews:BaseUrl`
- `HackerNews:TimeoutSeconds`
- `StoriesCache:*`

Key defaults:

- Story/snapshot/ids TTL: 24 hours
- Warmup story count: 200
- Max story fetch per refresh: 500
- Refresh interval: 720 minutes

## Run Locally (without Docker)

1. Start a local Redis instance on `localhost:6379`.
2. From repository root:

```bash
dotnet restore Santander.CodeChallenge.sln
dotnet build Santander.CodeChallenge.sln -c Release
dotnet run --project Santander.CodeChallenge.Api/Santander.CodeChallenge.Api.csproj
```

3. Call:

```bash
curl "http://localhost:5206/v1/stories/best?n=10"
```

Use the actual port shown in startup logs or `launchSettings.json`.
Swagger UI is available at `/swagger`.

## Run with Docker + nginx + Redis

From the repository root:

```bash
docker compose up --build
```

Services added for the challenge:

- `santander.api1`
- `santander.api2`
- `redis`
- `nginx` (reverse proxy + load balancing)

Call through nginx:

```bash
curl "http://localhost:8088/v1/stories/best?n=10"
```

## Tests

```bash
dotnet test Santander.CodeChallenge.Tests/Santander.CodeChallenge.Tests.csproj -c Release
```

Current tests cover query validation and MediatR handler success/failure paths.

## Assumptions

- The 1-second response target is for warm-cache requests.
- Long-lived cache (12-24h acceptable) is preferred over frequent upstream refresh.
- Hacker News can be intermittently unavailable; API should still serve from cache when data exists.