using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Console.Configuration;
using UCI.Middleware.Console.Services;

namespace UCI.Middleware.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configurazione del host con dependency injection
            var host = CreateHostBuilder(args).Build();

            // Ottenimento del service
            var testService = host.Services.GetRequiredService<ClaimsTestService>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Avvio dell'applicazione Claims Submission Console");

                await testService.RunAllTestsAsync();

                logger.LogInformation("Applicazione completata con successo");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante l'esecuzione dell'applicazione");
            }

            System.Console.WriteLine("Premi un tasto per uscire...");
            System.Console.ReadKey();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddApplicationServices(context.Configuration);
                });
    }
}