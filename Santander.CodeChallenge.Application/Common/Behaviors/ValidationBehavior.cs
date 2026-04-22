using FluentValidation;
using MediatR;
using Santander.CodeChallenge.Application.Common.Models;
using Santander.CodeChallenge.Application.Common.Notifications;

namespace Santander.CodeChallenge.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly INotificationContext _notificationContext;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, INotificationContext notificationContext)
    {
        _validators = validators;
        _notificationContext = notificationContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var validationContext = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));
            var failures = validationResults
                .SelectMany(x => x.Errors)
                .Where(x => x is not null)
                .Select(x => x.ErrorMessage)
                .Distinct()
                .ToArray();

            foreach (var failure in failures)
            {
                _notificationContext.Add(failure);
            }

            if (failures.Length > 0)
            {
                var responseType = typeof(TResponse);
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ApiResult<>))
                {
                    var payloadType = responseType.GetGenericArguments()[0];
                    var failMethod = responseType.GetMethod(nameof(ApiResult<object>.Fail), new[] { typeof(IEnumerable<string>) });
                    if (failMethod is not null)
                    {
                        return (TResponse)failMethod.Invoke(null, new object[] { failures })!;
                    }

                    var fallback = typeof(ApiResult<>).MakeGenericType(payloadType)
                        .GetMethod(nameof(ApiResult<object>.Fail), new[] { typeof(string[]) });

                    if (fallback is not null)
                    {
                        return (TResponse)fallback.Invoke(null, new object[] { failures })!;
                    }
                }

                throw new ValidationException(string.Join("; ", failures));
            }
        }

        return await next();
    }
}
