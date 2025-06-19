using Microsoft.Extensions.DependencyInjection;
using UCI.Middleware.Repositories.Implementations;
using UCI.Middleware.Repositories.Interfaces;

namespace UCI.Middleware.Repositories.Extensions
{
 
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register generic repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register specific repositories
            services.AddScoped<IClaimsSubmissionRepository, ClaimsSubmissionRepository>();
            services.AddScoped<ICorrespondentRepository, CorrespondentRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
   
}
