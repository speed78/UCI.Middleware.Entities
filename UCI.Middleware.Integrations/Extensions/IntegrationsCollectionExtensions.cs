using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UCI.Middleware.Integrations.Storage.Configuration;
using UCI.Middleware.Integrations.Storage.Implementation;
using UCI.Middleware.Integrations.Storage.Interfaces;
using UCI.Middleware.Repositories.Configuration;
using UCI.Middleware.Repositories.Implementations;
using UCI.Middleware.Repositories.Implementations.Specific;
using UCI.Middleware.Repositories.Interfaces;
using UCI.Middleware.Repositories.Interfaces.Specific;

namespace UCI.Middleware.Integrations.Extensions
{
    public static class IntegrationsCollectionExtensions
    {
        public static IServiceCollection AddUciIntegrationsServices<TContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            services.AddAzureStorageServices(configuration);
            services.AddUciRepositoryServicesWithDatabase<TContext>(configuration, contextLifetime);
            return services;
        }

        public static IServiceCollection AddAzureStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
            services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();

            services.AddSingleton<BlobServiceClient>(serviceProvider =>
            {
                var storageOptions = serviceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;
                return new BlobServiceClient(storageOptions.ConnectionString);
            });

            services.AddScoped<IStorageService, StorageService>();

            return services;
        }

        public static IServiceCollection AddUciRepositoryServices<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            services.Configure<RepositoryOptions>(configuration.GetSection(RepositoryOptions.SectionName));
            services.AddSingleton<IValidateOptions<RepositoryOptions>, RepositoryOptionsValidator>();

            services.AddScoped<IUnitOfWork>(provider =>
                new UnitOfWork(
                    provider.GetRequiredService<TContext>(),
                    provider.GetRequiredService<ILogger<UnitOfWork>>(),
                    provider.GetRequiredService<ILoggerFactory>()
                ));

            services.AddScoped<IClaimsSubmissionRepository>(provider =>
                new ClaimsSubmissionRepository(
                    provider.GetRequiredService<TContext>(),
                    provider.GetRequiredService<ILogger<ClaimsSubmissionRepository>>()
                ));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }

        public static IServiceCollection AddUciRepositoryServicesWithDatabase<TContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            services.AddUciRepositoryServices<TContext>(configuration);

            services.AddDbContext<TContext>((serviceProvider, options) =>
            {
                var repositoryOptions = serviceProvider.GetRequiredService<IOptions<RepositoryOptions>>().Value;
                ConfigureDatabaseProvider(options, repositoryOptions);
            }, contextLifetime);

            return services;
        }

        private static void ConfigureDatabaseProvider(DbContextOptionsBuilder options, RepositoryOptions repositoryOptions)
        {
            options.UseSqlServer(repositoryOptions.ConnectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(repositoryOptions.CommandTimeout);
            });
        }
    }
}
