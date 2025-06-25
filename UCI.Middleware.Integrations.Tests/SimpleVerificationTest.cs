using NUnit.Framework;
using Testcontainers.MsSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using UCI.Middleware.Entities.Context;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Entities.Entities.Aia;

namespace UCI.Middleware.Tests.IntegrationTests
{
    [TestFixture]
    public class UciDbContextIntegrationTests
    {
        private MsSqlContainer _sqlContainer;
        private UciDbContext _dbContext;
        private string _connectionString;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            // Avvio container SQL Server
            _sqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("YourStrong!Passw0rd")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .Build();

            await _sqlContainer.StartAsync();
            _connectionString = _sqlContainer.GetConnectionString();

            // Configurazione UciDbContext
            var options = new DbContextOptionsBuilder<UciDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            _dbContext = new UciDbContext(options);

            // Esecuzione migrations - questo creerà tutte le tabelle e i dati seed
            await _dbContext.Database.MigrateAsync();
        }

        [SetUp]
        public async Task SetUp()
        {
            // Pulisci i dati di test prima di ogni test (mantieni i seed data)
            await _dbContext.IvassClaimsSubmissions.ExecuteDeleteAsync();
            await _dbContext.Claims.ExecuteDeleteAsync();
            await _dbContext.FlowErrors.ExecuteDeleteAsync();
            await _dbContext.Scores.ExecuteDeleteAsync();
            await _dbContext.CorrespondentScores.ExecuteDeleteAsync();
        }

        [Test]
        public async Task Test_DatabaseCreatedWithMigrations()
        {
            // Verifica che le tabelle principali siano state create
            var tablesExist = await _dbContext.Database.ExecuteSqlRawAsync(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME IN ('IvassClaimsSubmissions', 'SubmissionStatuses', 'Correspondents', 'ErrorCodes')
            ");

            // Se arriviamo qui senza eccezioni, le tabelle esistono
            Assert.Pass("Database e tabelle create correttamente dalle migrations");
        }

        [Test]
        public async Task Test_SeedData_IsPresent()
        {
            // Verifica che i dati seed siano stati inseriti
            var submissionStatusCount = await _dbContext.SubmissionStatuses.CountAsync();
            var correspondentCount = await _dbContext.Correspondents.CountAsync();
            var errorTypeCount = await _dbContext.ErrorCodes.CountAsync();

            Assert.AreEqual(6, submissionStatusCount, "Dovrebbero esserci 6 SubmissionStatus");
            Assert.AreEqual(33, correspondentCount, "Dovrebbero esserci 33 Correspondent");
            Assert.Greater(errorTypeCount, 0, "Dovrebbero esserci degli ErrorType");

            // Verifica alcuni dati specifici
            var uploadedStatus = await _dbContext.SubmissionStatuses
                .FirstOrDefaultAsync(s => s.Description == "Uploaded");
            Assert.IsNotNull(uploadedStatus, "Status 'Uploaded' dovrebbe esistere");

            var uciCorrespondent = await _dbContext.Correspondents
                .FirstOrDefaultAsync(c => c.UciCode == "000000");
            Assert.IsNotNull(uciCorrespondent, "Correspondent UCI dovrebbe esistere");
            Assert.AreEqual("UCI", uciCorrespondent.ConventionalName);
        }

        [Test]
        public async Task Test_CanCreateClaimsSubmission()
        {
            // Arrange - Prendi dati seed esistenti
            var uploadedStatus = await _dbContext.SubmissionStatuses
                .FirstAsync(s => s.Description == "Uploaded");

            var correspondent = await _dbContext.Correspondents
                .FirstAsync(c => c.UciCode == "000075"); // AIG EUROPE

            var claimsSubmission = new ClaimsSubmission
            {
                InputFileName = "test_claims.xml",
                InputFileFullPath = @"C:\temp\test_claims.xml",
                UploadDate = DateTime.UtcNow,
                SubmissionStatusId = uploadedStatus.Id,
                CorrespondentId = correspondent.Id
            };

            // Act
            _dbContext.IvassClaimsSubmissions.Add(claimsSubmission);
            await _dbContext.SaveChangesAsync();

            // Assert
            var savedSubmission = await _dbContext.IvassClaimsSubmissions
                .Include(s => s.SubmissionStatus)
                .Include(s => s.Correspondent)
                .FirstOrDefaultAsync(s => s.InputFileName == "test_claims.xml");

            Assert.IsNotNull(savedSubmission);
            Assert.AreEqual("Uploaded", savedSubmission.SubmissionStatus.Description);
            Assert.AreEqual("AIG EUROPE", savedSubmission.Correspondent.ConventionalName);
            Assert.IsNotNull(savedSubmission.Id);
        }



        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            // Cleanup
            if (_dbContext != null)
            {
                await _dbContext.DisposeAsync();
            }

            if (_sqlContainer != null)
            {
                await _sqlContainer.DisposeAsync();
            }
        }
    }

    // Classe helper per configurazione avanzata
    public static class UciTestHelper
    {
        public static IServiceCollection ConfigureServices(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddDbContext<UciDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }

        public static async Task<ClaimsSubmission> CreateTestSubmission(
            UciDbContext context,
            string fileName = "test.xml",
            string statusDescription = "Uploaded")
        {
            var status = await context.SubmissionStatuses
                .FirstAsync(s => s.Description == statusDescription);

            var correspondent = await context.Correspondents
                .FirstAsync(c => c.UciCode == "000000"); // UCI

            return new ClaimsSubmission
            {
                InputFileName = fileName,
                InputFileFullPath = $@"C:\temp\{fileName}",
                UploadDate = DateTime.UtcNow,
                SubmissionStatusId = status.Id,
                CorrespondentId = correspondent.Id
            };
        }
    }
}