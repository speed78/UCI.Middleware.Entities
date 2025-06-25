using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Console.Services;
using UCI.Middleware.Entities.Context;
using UCI.Middleware.Integration.Database.Implementation;
using UCI.Middleware.Integration.Database.Interfaces;
using UCI.Middleware.Repositories.Extensions;


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

            // Configurazione del database - SOSTITUISCI "YourDbContext" con il nome reale del tuo DbContext
            // Ad esempio: UCIMiddlewareDbContext, IvassDbContext, etc.
            services.AddDbContext<UciDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Usa l'extension method del tuo progetto per registrare i repository
            services.AddUciRepositories<UciDbContext>();

            // Service layer
            services.AddScoped<IClaimsSubmissionService, ClaimsSubmissionService>();

            // Application services
            services.AddScoped<ClaimsTestService>();

            return services;
        }
    }
}