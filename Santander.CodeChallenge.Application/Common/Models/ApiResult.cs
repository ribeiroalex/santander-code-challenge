namespace Santander.CodeChallenge.Application.Common.Models;

public sealed class ApiResult<T>
{
    private ApiResult(bool success, T? data, IReadOnlyCollection<string> errors)
    {
        Success = success;
        Data = data;
        Errors = errors;
    }

    public bool Success { get; }
    public T? Data { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static ApiResult<T> Ok(T data) => new(true, data, Array.Empty<string>());

    public static ApiResult<T> Fail(params string[] errors)
    {
        var sanitized = errors.Where(static e => !string.IsNullOrWhiteSpace(e)).ToArray();
        return new ApiResult<T>(false, default, sanitized);
    }

    public static ApiResult<T> Fail(IEnumerable<string> errors)
    {
        var sanitized = errors.Where(static e => !string.IsNullOrWhiteSpace(e)).ToArray();
        return new ApiResult<T>(false, default, sanitized);
    }
}
