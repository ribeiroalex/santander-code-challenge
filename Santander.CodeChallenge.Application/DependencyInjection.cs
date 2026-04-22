using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Santander.CodeChallenge.Application.Common.Behaviors;
using Santander.CodeChallenge.Application.Common.Notifications;

namespace Santander.CodeChallenge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationContext, NotificationContext>();

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
