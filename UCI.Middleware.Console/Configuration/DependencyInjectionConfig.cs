using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Console.Services;
using UCI.Middleware.Entities.Context;
using UCI.Middleware.Integrations.Database.Implementation;
using UCI.Middleware.Integrations.Database.Interfaces;
using UCI.Middleware.Integrations.Extensions;


namespace UCI.Middleware.Console.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurazione del logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddUciIntegrationsServices<UciDbContext>(configuration);


            // Service layer
            services.AddScoped<IClaimsSubmissionService, ClaimsSubmissionService>();

            // Application services
            services.AddScoped<ClaimsTestService>();
            services.AddSingleton<TestData>();
            services.AddScoped<StorageServiceTests>();

            return services;
        }
    }
}