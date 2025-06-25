using Microsoft.Extensions.Logging;
using UCI.Middleware.Entities.Enums.Ivass;
using UCI.Middleware.Integration.Database.Interfaces;

namespace UCI.Middleware.Console.Services
{
    public class ClaimsTestService
    {
        private readonly IClaimsSubmissionService _claimsService;
        private readonly ILogger<ClaimsTestService> _logger;
        private readonly TestDataManager _testData;

        public ClaimsTestService(
            IClaimsSubmissionService claimsService,
            ILogger<ClaimsTestService> logger)
        {
            _claimsService = claimsService;
            _logger = logger;
            _testData = new TestDataManager();
        }

        public async Task RunAllTestsAsync()
        {
            await TestCreateSubmission();
            await TestReadSubmissions();
            await TestUpdateStatus();
        }

        private async Task TestCreateSubmission()
        {
            _logger.LogInformation("=== TEST CREAZIONE SUBMISSION ===");

            try
            {
                var correspondentId = new Guid("550e8400-e29b-41d4-a716-446655440000");
                var fileName = "test_claims_file.xml";
                var filePath = @"C:\temp\claims\test_claims_file.xml";

                var submission = await _claimsService.CreateSubmissionFromFileAsync(fileName, filePath, correspondentId);

                _logger.LogInformation($"Submission creata con successo:");
                _logger.LogInformation($"  ID: {submission.Id}");
                _logger.LogInformation($"  File: {submission.InputFileName}");
                _logger.LogInformation($"  Path: {submission.InputFileFullPath}");
                _logger.LogInformation($"  Status: {submission.SubmissionStatusId}");
                _logger.LogInformation($"  Upload Date: {submission.UploadDate}");
                _logger.LogInformation($"  Correspondent ID: {submission.CorrespondentId}");

                // Salva l'ID per i test successivi
                _testData.LastSubmissionId = submission.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della submission");
                throw;
            }
        }

        private async Task TestReadSubmissions()
        {
            _logger.LogInformation("=== TEST LETTURA SUBMISSIONS ===");

            try
            {
                await TestReadSubmissionById();
                await TestReadSubmissionsByStatus();
                await TestReadPendingSubmissions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la lettura delle submissions");
                throw;
            }
        }

        private async Task TestReadSubmissionById()
        {
            if (_testData.LastSubmissionId != Guid.Empty)
            {
                _logger.LogInformation($"Lettura submission per ID: {_testData.LastSubmissionId}");
                var submission = await _claimsService.GetSubmission(_testData.LastSubmissionId);

                if (submission != null)
                {
                    _logger.LogInformation($"Submission trovata:");
                    _logger.LogInformation($"  ID: {submission.Id}");
                    _logger.LogInformation($"  File: {submission.InputFileName}");
                    _logger.LogInformation($"  Status: {submission.SubmissionStatusId}");
                }
                else
                {
                    _logger.LogWarning("Submission non trovata");
                }
            }
        }

        private async Task TestReadSubmissionsByStatus()
        {
            _logger.LogInformation("Lettura submissions per status 'Uploaded'");
            var pagedResult = await _claimsService.GetByStatusPagedAsync(SubmissionStatusType.Uploaded, 1, 10);

            _logger.LogInformation($"Trovate {pagedResult.TotalCount} submissions con status 'Uploaded'");
            _logger.LogInformation($"Pagina {pagedResult.PageNumber} di {Math.Ceiling((double)pagedResult.TotalCount / pagedResult.PageSize)}");

            foreach (var item in pagedResult.Items)
            {
                _logger.LogInformation($"  - ID: {item.Id}, File: {item.InputFileName}, Date: {item.UploadDate}");
            }
        }

        private async Task TestReadPendingSubmissions()
        {
            _logger.LogInformation("Lettura submissions pending response (oltre 1 ora)");
            var pendingSubmissions = await _claimsService.GetPendingResponseAsync(1);
            _logger.LogInformation($"Trovate {pendingSubmissions.Count()} submissions pending");
        }

        private async Task TestUpdateStatus()
        {
            _logger.LogInformation("=== TEST AGGIORNAMENTO STATUS ===");

            try
            {
                if (_testData.LastSubmissionId != Guid.Empty)
                {
                    await TestUpdateToProcessing();
                    await TestMarkAsSent();
                    await TestCreateResponse();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dello status");
                throw;
            }
        }

        private async Task TestUpdateToProcessing()
        {
            _logger.LogInformation($"Aggiornamento status a 'Processing' per submission {_testData.LastSubmissionId}");
            var updatedSubmission = await _claimsService.UpdateStatusAsync(_testData.LastSubmissionId, SubmissionStatusType.Completed);
            _logger.LogInformation($"Status aggiornato a: {updatedSubmission.SubmissionStatusId}");
        }

        private async Task TestMarkAsSent()
        {
            _logger.LogInformation("Marking submission as sent con protocol");
            var protocol = $"PROT_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var sentSubmission = await _claimsService.MarkAsSentAsync(_testData.LastSubmissionId, protocol);
            _logger.LogInformation($"Submission marcata come sent con protocol: {sentSubmission.Protocol}");
            _logger.LogInformation($"Send Date: {sentSubmission.SendDate}");
        }

        private async Task TestCreateResponse()
        {
            _logger.LogInformation("Creazione response per submission");
            var responseSubmission = await _claimsService.CreateResponseAsync(
                _testData.LastSubmissionId,
                "response_file.xml",
                @"C:\temp\claims\responses\response_file.xml");

            _logger.LogInformation($"Response creata:");
            _logger.LogInformation($"  Response Date: {responseSubmission.ResponseDate}");
            _logger.LogInformation($"  Output File: {responseSubmission.OutputFileName}");
            _logger.LogInformation($"  Status: {responseSubmission.SubmissionStatusId}");
        }
    }
}