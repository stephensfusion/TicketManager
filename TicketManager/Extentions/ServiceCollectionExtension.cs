using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSCatch.Extensions;
using NSCatch.Interfaces;
using NSCore.DatabaseContext;
using NSCore.DbContextHelper;
using NSCore.Models;
using TicketManager.Data;
using TicketManager.Enumerations;
using TicketManager.Interfaces;

namespace TicketManager.Extentions;

/// <summary>
/// Extension methods for configuring TicketManager services in the DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds TicketManager services with its own dedicated DbContext using NSCore DbContextHelper.
    /// This is the recommended approach for services that need their own isolated DbContext.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="inputModel">The database configuration</param>
    /// <param name="dbOptions">Optional configuration for pooling,logging and advanced features</param>
    /// <param name="catchOption">Optional cache configuration</param>
    /// <param name="applyMigrationsAutomatically">Applies migrations automatically is needed</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="InvalidOperationException">Throws an invalid operation exception</exception>
    public static IServiceCollection AddTicketManager<TicketStatus, Priority,Tags>(
        this IServiceCollection services,
        IDatabaseConfig inputModel,
        DbSetupOptions? dbOptions = null,
        ICatchOption? catchOption = null,
        bool applyMigrationsAutomatically = true)
        where TicketStatus : struct ,Enum
        where Priority : struct, Enum
        where Tags : struct, Enum
    {
        // Register dedicated AppDbContext for Ticket Manager using NSCore DbContextHelper
        services.AddCustomDbContextFactory<AppDbContext>(inputModel, dbOptions);

        // Register NSCatch with "TicketService" cache key for service isolation
        services.AddNSCache(catchOption, "TicketService");

        // Register ManageTickets service with cache dependencies
        services.AddSingleton<ITicketManager<TicketStatus,Priority,Tags>>(provider =>
        {
            var contextFactory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();

            //Get cache services if configured
            ICacheManager? cacheManager = provider.GetKeyedService<ICacheManager>("TicketService");
            ICacheKeyBuilder? cacheKeyBuilder = provider.GetKeyedService<ICacheKeyBuilder>("TicketService");

            if (cacheManager is null || cacheKeyBuilder is null)
            {
                throw new InvalidOperationException("Cache services are not properly configured for Manager.");
            }

            return new ManageTickets<TicketStatus,Priority,Tags>(inputModel, contextFactory, cacheManager, cacheKeyBuilder, applyMigrationsAutomatically);
        });
        // Register as hosted service
        services.AddSingleton<IHostedService>(provider =>
        {
            var manageTicketsService = provider.GetRequiredService<ITicketManager<TicketStatus, Priority,Tags>>();
            return (ManageTickets<TicketStatus,Priority,Tags>)manageTicketsService;
        });
        return services;
    }
}


