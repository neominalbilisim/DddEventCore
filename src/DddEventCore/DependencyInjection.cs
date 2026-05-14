using System.Reflection;
using Neominal.DddEventCore.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Neominal.DddEventCore;

/// <summary>
/// Extension methods for Dependency Injection
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds DddEventCore services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan for domain event handlers</param>
    /// <returns>Service collection (for fluent API)</returns>
    public static IServiceCollection AddDddEventCore(
        this IServiceCollection services, 
        params Assembly[] assemblies)
    {
        // Add event dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Use calling assembly if no assembly is specified
        var assembliesToScan = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        // Find and register all domain event handlers
        foreach (var assembly in assembliesToScan)
        {
            RegisterDomainEventHandlers(services, assembly);
        }

        return services;
    }

    private static void RegisterDomainEventHandlers(IServiceCollection services, Assembly assembly)
    {
        // Find all types implementing IDomainEventHandler<>
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .Select(i => new
                {
                    InterfaceType = i,
                    ImplementationType = t
                }))
            .ToList();

        // Register each handler in the DI container
        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.InterfaceType, handler.ImplementationType);
        }
    }
}
