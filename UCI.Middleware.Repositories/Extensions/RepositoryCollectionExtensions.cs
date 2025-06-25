using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Repositories.Interfaces;
using UCI.Middleware.Repositories.Interfaces.Specific;
using UCI.Middleware.Repositories.Implementations;
using UCI.Middleware.Repositories.Implementations.Specific;

namespace UCI.Middleware.Repositories.Extensions
{
    /// <summary>
    /// Extension methods for configuring repository services in the dependency injection container.
    /// </summary>
    public static class RepositoryCollectionExtensions
    {
        /// <summary>
        /// Adds UCI Middleware repository services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddUciRepositories(this IServiceCollection services)
        {
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register specific repositories
            services.AddScoped<IClaimsSubmissionRepository, ClaimsSubmissionRepository>();

            // Register generic repository (if needed independently)
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }

        /// <summary>
        /// Adds UCI Middleware repository services with a specific DbContext type.
        /// </summary>
        /// <typeparam name="TContext">The DbContext type to use</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddUciRepositories<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            // Register Unit of Work with specific context
            services.AddScoped<IUnitOfWork>(provider =>
                new UnitOfWork(
                    provider.GetRequiredService<TContext>(),
                    provider.GetRequiredService<ILogger<UnitOfWork>>(),
                    provider.GetRequiredService<ILoggerFactory>()
                ));

            // Register specific repositories with specific context
            services.AddScoped<IClaimsSubmissionRepository>(provider =>
                new ClaimsSubmissionRepository(
                    provider.GetRequiredService<TContext>(),
                    provider.GetRequiredService<ILogger<ClaimsSubmissionRepository>>()
                ));

            // Register generic repository with specific context
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}