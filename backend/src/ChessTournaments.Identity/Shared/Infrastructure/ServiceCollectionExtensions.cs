using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChessTournaments.Identity.Shared.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequestHandlers(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var handlerTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
            .Where(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                    )
            )
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType
                .GetInterfaces()
                .First(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                );

            services.TryAddScoped(interfaceType, handlerType);
        }

        return services;
    }
}
