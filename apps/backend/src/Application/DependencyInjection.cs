using Microsoft.Extensions.DependencyInjection;

namespace Kcow.Application;

/// <summary>
/// Extension methods for configuring Application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application services to the DI container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add application services here as they are created
        return services;
    }
}
