using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VerdaVida.Shared.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds shared services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        // Add any shared services here
        // This is a placeholder for future shared services
        
        return services;
    }

    /// <summary>
    /// Registers a service as both its interface and implementation.
    /// </summary>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddService<TInterface, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.Add(new ServiceDescriptor(typeof(TInterface), typeof(TImplementation), lifetime));
        return services;
    }

    /// <summary>
    /// Registers a service as both its interface and implementation with a factory.
    /// </summary>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="factory">The factory function.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddService<TInterface>(
        this IServiceCollection services,
        Func<IServiceProvider, TInterface> factory,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class
    {
        services.Add(new ServiceDescriptor(typeof(TInterface), factory, lifetime));
        return services;
    }
}
