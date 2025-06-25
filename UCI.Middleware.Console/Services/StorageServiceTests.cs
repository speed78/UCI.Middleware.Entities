using Microsoft.Extensions.Logging;
using UCI.Middleware.Integrations.Storage.Interfaces;
using UCI.Middleware.Integrations.Storage.Models;

namespace UCI.Middleware.Console.Services
{
    public class StorageServiceTests
    {
        private readonly IStorageService _storageService;
        private readonly ILogger<StorageServiceTests> _logger;
        private readonly TestData _testData;

        public StorageServiceTests(IStorageService storageService, ILogger<StorageServiceTests> logger, TestData testData)
        {
            _storageService = storageService;
            _logger = logger;
            _testData = testData;
        }

        public async Task RunAllTestsAsync()
        {
            _logger.LogInformation("=== INIZIO TEST STORAGE SERVICE ===");

            await TestUploadFile();
            await TestMoveFile();
            await TestFileOperations();
            await TestCleanup();

            _logger.LogInformation("=== FINE TEST STORAGE SERVICE ===");
        }

        private async Task TestUploadFile()
        {
            _logger.LogInformation("=== TEST UPLOAD FILE ===");
            try
            {
                var fileName = "test_document.xml";
                var localFilePath = @"C:\temp\claims\test_document.xml";

                // Verifica che il file locale esista
                if (!File.Exists(localFilePath))
                {
                    _logger.LogWarning("File locale non trovato, creazione file di test...");
                    await CreateTestFileAsync(localFilePath);
                }

                // Leggi il contenuto del file
                var fileContent = await File.ReadAllBytesAsync(localFilePath);

                // Upload nel container "incoming"
                var uploadOptions = new UploadOptions
                {
                    Container = "incoming",
                    ContentType = "application/xml",
                    Metadata = new Dictionary<string, string>
                {
                    { "OriginalFileName", fileName },
                    { "UploadedBy", "TestConsole" },
                    { "UploadDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "FileSize", fileContent.Length.ToString() }
                }
                };

                var uploadResult = await _storageService.UploadAsync(fileName, fileContent, uploadOptions);

                if (uploadResult.Success)
                {
                    _logger.LogInformation("File caricato con successo:");
                    _logger.LogInformation($"  Nome: {fileName}");
                    _logger.LogInformation($"  Container: incoming");
                    _logger.LogInformation($"  URL: {uploadResult.Data}");
                    _logger.LogInformation($"  Dimensione: {fileContent.Length} bytes");

                    // Salva per i test successivi
                    _testData.LastUploadedFileName = fileName;
                    _testData.LastUploadedFileUrl = uploadResult.Data!;
                }
                else
                {
                    _logger.LogError("Errore durante l'upload: {Error}", uploadResult.ErrorMessage);
                    throw new Exception($"Upload fallito: {uploadResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il test di upload");
                throw;
            }
        }

        private async Task TestMoveFile()
        {
            _logger.LogInformation("=== TEST MOVE FILE ===");
            try
            {
                if (string.IsNullOrEmpty(_testData.LastUploadedFileName))
                {
                    throw new InvalidOperationException("Nessun file precedentemente caricato trovato");
                }

                var sourceFileName = _testData.LastUploadedFileName;
                var destinationFileName = $"processed/{DateTime.UtcNow:yyyyMMdd_HHmmss}_{sourceFileName}";

                // Sposta da "incoming" a "processed"
                var moveResult = await _storageService.MoveAsync(
                    sourceFileName,
                    destinationFileName,
                    sourceContainer: "incoming",
                    destinationContainer: "processed"
                );

                if (moveResult.Success)
                {
                    _logger.LogInformation("File spostato con successo:");
                    _logger.LogInformation($"  Da: incoming/{sourceFileName}");
                    _logger.LogInformation($"  A: processed/{destinationFileName}");
                    _logger.LogInformation($"  Nuovo URL: {moveResult.Data}");

                    // Aggiorna i dati di test
                    _testData.LastMovedFileName = destinationFileName;
                    _testData.LastMovedFileUrl = moveResult.Data!;

                    // Verifica che il file sia stato effettivamente spostato
                    await VerifyMoveOperation(sourceFileName, destinationFileName);
                }
                else
                {
                    _logger.LogError("Errore durante lo spostamento: {Error}", moveResult.ErrorMessage);
                    throw new Exception($"Move fallito: {moveResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il test di move");
                throw;
            }
        }

        private async Task VerifyMoveOperation(string sourceFileName, string destinationFileName)
        {
            _logger.LogInformation("=== VERIFICA OPERAZIONE MOVE ===");

            // Verifica che il file sorgente non esista più
            var sourceExists = await _storageService.ExistsAsync(sourceFileName, "incoming");
            if (sourceExists.Success)
            {
                if (sourceExists.Data)
                {
                    _logger.LogWarning("ATTENZIONE: Il file sorgente esiste ancora in incoming/{FileName}", sourceFileName);
                }
                else
                {
                    _logger.LogInformation("✓ File sorgente correttamente rimosso da incoming");
                }
            }

            // Verifica che il file di destinazione esista
            var destExists = await _storageService.ExistsAsync(destinationFileName, "processed");
            if (destExists.Success && destExists.Data)
            {
                _logger.LogInformation("✓ File di destinazione trovato in processed");

                // Ottieni info sul file spostato
                var fileInfo = await _storageService.GetBlobInfoAsync(destinationFileName, "processed");
                if (fileInfo.Success)
                {
                    _logger.LogInformation("Dettagli file spostato:");
                    _logger.LogInformation($"  Dimensione: {fileInfo.Data!.Size} bytes");
                    _logger.LogInformation($"  Ultima modifica: {fileInfo.Data.LastModified}");
                    _logger.LogInformation($"  Content-Type: {fileInfo.Data.ContentType}");
                    _logger.LogInformation($"  Metadata: {string.Join(", ", fileInfo.Data.Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}");
                }
            }
            else
            {
                _logger.LogError("✗ File di destinazione NON trovato in processed");
            }
        }

        private async Task TestFileOperations()
        {
            _logger.LogInformation("=== TEST OPERAZIONI AGGIUNTIVE ===");
            try
            {
                // Test download del file spostato
                if (!string.IsNullOrEmpty(_testData.LastMovedFileName))
                {
                    var downloadResult = await _storageService.DownloadTextAsync(_testData.LastMovedFileName, "processed");
                    if (downloadResult.Success)
                    {
                        _logger.LogInformation("Download riuscito, contenuto: {Length} caratteri", downloadResult.Data!.Length);
                        _logger.LogInformation("Prime 100 caratteri: {Preview}",
                            downloadResult.Data.Length > 100 ? downloadResult.Data[..100] + "..." : downloadResult.Data);
                    }
                }

                // Test lista file nel container processed
                var listResult = await _storageService.ListBlobsAsync(container: "processed");
                if (listResult.Success)
                {
                    _logger.LogInformation("File nel container 'processed': {Count}", listResult.Data!.Count());
                    foreach (var blob in listResult.Data.Take(5)) // Mostra max 5 file
                    {
                        _logger.LogInformation($"  - {blob.Name} ({blob.Size} bytes, {blob.LastModified})");
                    }
                }

                // Test copia del file per backup
                if (!string.IsNullOrEmpty(_testData.LastMovedFileName))
                {
                    var backupFileName = $"backup/{_testData.LastMovedFileName}";
                    var copyResult = await _storageService.CopyAsync(
                        _testData.LastMovedFileName,
                        backupFileName,
                        sourceContainer: "processed",
                        destinationContainer: "backup"
                    );

                    if (copyResult.Success)
                    {
                        _logger.LogInformation("Backup creato: {BackupFile}", backupFileName);
                        _testData.LastBackupFileName = backupFileName;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante le operazioni aggiuntive");
                throw;
            }
        }

        private async Task TestCleanup()
        {
            _logger.LogInformation("=== TEST CLEANUP ===");
            try
            {
                var deletedCount = 0;

                // Elimina il file processato
                if (!string.IsNullOrEmpty(_testData.LastMovedFileName))
                {
                    var deleteResult = await _storageService.DeleteAsync(_testData.LastMovedFileName, "processed");
                    if (deleteResult.Success && deleteResult.Data)
                    {
                        deletedCount++;
                        _logger.LogInformation("File processato eliminato: {FileName}", _testData.LastMovedFileName);
                    }
                }

                // Elimina il backup
                if (!string.IsNullOrEmpty(_testData.LastBackupFileName))
                {
                    var deleteResult = await _storageService.DeleteAsync(_testData.LastBackupFileName, "backup");
                    if (deleteResult.Success && deleteResult.Data)
                    {
                        deletedCount++;
                        _logger.LogInformation("File backup eliminato: {FileName}", _testData.LastBackupFileName);
                    }
                }

                _logger.LogInformation("Cleanup completato: {Count} file eliminati", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cleanup");
                // Non rilanciamo l'eccezione per il cleanup
            }
        }

        private async Task CreateTestFileAsync(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var testContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestDocument>
    <Header>
        <CreatedBy>StorageServiceTest</CreatedBy>
        <CreatedAt>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</CreatedAt>
        <TestId>{Guid.NewGuid()}</TestId>
    </Header>
    <Content>
        <Message>Questo è un file di test per il servizio di storage</Message>
        <Data>
            <Item id=""1"">Test Item 1</Item>
            <Item id=""2"">Test Item 2</Item>
            <Item id=""3"">Test Item 3</Item>
        </Data>
    </Content>
</TestDocument>";

            await File.WriteAllTextAsync(filePath, testContent);
            _logger.LogInformation("File di test creato: {FilePath} ({Size} bytes)", filePath, testContent.Length);
        }
    }
}
