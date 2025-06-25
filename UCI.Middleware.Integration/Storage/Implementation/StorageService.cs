using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UCI.Middleware.Integration.Storage.Configuration;
using UCI.Middleware.Integration.Storage.Interfaces;
using UCI.Middleware.Integration.Storage.Models;
using BlobInfo = UCI.Middleware.Integration.Storage.Models.BlobInfo;

namespace UCI.Middleware.Integration.Storage.Implementation
{
    public class StorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly StorageOptions _options;
        private readonly ILogger<StorageService> _logger;

        public StorageService(BlobServiceClient blobServiceClient, IOptions<StorageOptions> options, ILogger<StorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<StorageResult<string>> UploadAsync(string blobName, Stream content, UploadOptions? options = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerName = options?.Container ?? _options.DefaultContainer;
                var containerClient = await GetContainerClientAsync(containerName, cancellationToken);

                var blobClient = containerClient.GetBlobClient(blobName);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = options?.ContentType ?? "application/octet-stream"
                    },
                    Metadata = options?.Metadata,
                    Conditions = options?.Overwrite == false ? new BlobRequestConditions { IfNoneMatch = ETag.All } : null
                };

                var response = await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

                _logger.LogInformation("Successfully uploaded blob {BlobName} to container {Container}", blobName, containerName);

                return StorageResult<string>.Ok(blobClient.Uri.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload blob {BlobName}", blobName);
                return StorageResult<string>.Fail($"Failed to upload blob: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<string>> UploadAsync(string blobName, byte[] content, UploadOptions? options = null, CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream(content);
            return await UploadAsync(blobName, stream, options, cancellationToken);
        }

        public async Task<StorageResult<string>> UploadTextAsync(string blobName, string content, UploadOptions? options = null, CancellationToken cancellationToken = default)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var uploadOptions = options ?? new UploadOptions();
            uploadOptions.ContentType ??= "text/plain; charset=utf-8";

            return await UploadAsync(blobName, bytes, uploadOptions, cancellationToken);
        }

        public async Task<StorageResult<Stream>> DownloadAsync(string blobName, string? container = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerName = container ?? _options.DefaultContainer;
                var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully downloaded blob {BlobName} from container {Container}", blobName, containerName);

                return StorageResult<Stream>.Ok(response.Value.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download blob {BlobName}", blobName);
                return StorageResult<Stream>.Fail($"Failed to download blob: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<byte[]>> DownloadBytesAsync(string blobName, string? container = null, CancellationToken cancellationToken = default)
        {
            var streamResult = await DownloadAsync(blobName, container, cancellationToken);
            if (!streamResult.Success)
                return StorageResult<byte[]>.Fail(streamResult.ErrorMessage!, streamResult.Exception);

            try
            {
                using var stream = streamResult.Data!;
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, cancellationToken);
                return StorageResult<byte[]>.Ok(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert stream to bytes for blob {BlobName}", blobName);
                return StorageResult<byte[]>.Fail($"Failed to convert stream to bytes: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<string>> DownloadTextAsync(string blobName, string? container = null, CancellationToken cancellationToken = default)
        {
            var bytesResult = await DownloadBytesAsync(blobName, container, cancellationToken);
            if (!bytesResult.Success)
                return StorageResult<string>.Fail(bytesResult.ErrorMessage!, bytesResult.Exception);

            try
            {
                var text = System.Text.Encoding.UTF8.GetString(bytesResult.Data!);
                return StorageResult<string>.Ok(text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert bytes to text for blob {BlobName}", blobName);
                return StorageResult<string>.Fail($"Failed to convert bytes to text: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<BlobInfo>> GetBlobInfoAsync(string blobName, string? container = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerName = container ?? _options.DefaultContainer;
                var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                var properties = response.Value;

                var blobInfo = new BlobInfo
                {
                    Name = blobName,
                    Container = containerName,
                    Size = properties.ContentLength,
                    LastModified = properties.LastModified.DateTime,
                    ContentType = properties.ContentType ?? "application/octet-stream",
                    ETag = properties.ETag.ToString(),
                    Metadata = properties.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };

                return StorageResult<BlobInfo>.Ok(blobInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get blob info for {BlobName}", blobName);
                return StorageResult<BlobInfo>.Fail($"Failed to get blob info: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<bool>> ExistsAsync(string blobName, string? container = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerName = container ?? _options.DefaultContainer;
                var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.ExistsAsync(cancellationToken);
                return StorageResult<bool>.Ok(response.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if blob exists {BlobName}", blobName);
                return StorageResult<bool>.Fail($"Failed to check blob existence: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<IEnumerable<BlobInfo>>> ListBlobsAsync(string? prefix = null, string? container = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerName = container ?? _options.DefaultContainer;
                var containerClient = await GetContainerClientAsync(containerName, cancellationToken);

                var blobs = new List<BlobInfo>();

                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
                {
                    var blobInfo = new BlobInfo
                    {
                        Name = blobItem.Name,
                        Container = containerName,
                        Size = blobItem.Properties.ContentLength ?? 0,
                        LastModified = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue,
                        ContentType = blobItem.Properties.ContentType ?? "application/octet-stream",
                        ETag = blobItem.Properties.ETag?.ToString() ?? string.Empty,
                        Metadata = blobItem.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    };

                    blobs.Add(blobInfo);
                }

                _logger.LogInformation("Listed {Count} blobs from container {Container} with prefix {Prefix}",
                    blobs.Count, containerName, prefix ?? "none");

                return StorageResult<IEnumerable<BlobInfo>>.Ok(blobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list blobs in container {Container}", container ?? _options.DefaultContainer);
                return StorageResult<IEnumerable<BlobInfo>>.Fail($"Failed to list blobs: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<bool>> DeleteAsync(string blobName, string? container = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerName = container ?? _options.DefaultContainer;
                var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

                _logger.LogInformation("Deleted blob {BlobName} from container {Container}. Existed: {Existed}",
                    blobName, containerName, response.Value);

                return StorageResult<bool>.Ok(response.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blob {BlobName}", blobName);
                return StorageResult<bool>.Fail($"Failed to delete blob: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<int>> DeleteBatchAsync(IEnumerable<string> blobNames, string? container = null, CancellationToken cancellationToken = default)
        {
            var deletedCount = 0;
            var errors = new List<string>();

            foreach (var blobName in blobNames)
            {
                var result = await DeleteAsync(blobName, container, cancellationToken);
                if (result.Success && result.Data)
                {
                    deletedCount++;
                }
                else if (!result.Success)
                {
                    errors.Add($"{blobName}: {result.ErrorMessage}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Batch delete completed with {DeletedCount} successes and {ErrorCount} errors",
                    deletedCount, errors.Count);
            }

            return StorageResult<int>.Ok(deletedCount);
        }

        public async Task<StorageResult<string>> CopyAsync(string sourceBlobName, string destinationBlobName, string? sourceContainer = null, string? destinationContainer = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var sourceContainerName = sourceContainer ?? _options.DefaultContainer;
                var destinationContainerName = destinationContainer ?? _options.DefaultContainer;

                var sourceContainerClient = await GetContainerClientAsync(sourceContainerName, cancellationToken);
                var destinationContainerClient = await GetContainerClientAsync(destinationContainerName, cancellationToken);

                var sourceBlobClient = sourceContainerClient.GetBlobClient(sourceBlobName);
                var destinationBlobClient = destinationContainerClient.GetBlobClient(destinationBlobName);

                // Check if source blob exists
                var sourceExists = await sourceBlobClient.ExistsAsync(cancellationToken);
                if (!sourceExists.Value)
                {
                    return StorageResult<string>.Fail($"Source blob {sourceBlobName} does not exist in container {sourceContainerName}");
                }

                // Start copy operation
                var copyOperation = await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri, cancellationToken: cancellationToken);

                // Wait for copy to complete
                await copyOperation.WaitForCompletionAsync(cancellationToken);

                if (copyOperation.HasCompleted && !copyOperation.HasValue)
                {
                    return StorageResult<string>.Fail("Copy operation failed");
                }

                _logger.LogInformation("Successfully copied blob from {SourceContainer}/{SourceBlob} to {DestinationContainer}/{DestinationBlob}",
                    sourceContainerName, sourceBlobName, destinationContainerName, destinationBlobName);

                return StorageResult<string>.Ok(destinationBlobClient.Uri.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy blob from {SourceBlob} to {DestinationBlob}", sourceBlobName, destinationBlobName);
                return StorageResult<string>.Fail($"Failed to copy blob: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<string>> MoveAsync(string sourceBlobName, string destinationBlobName, string? sourceContainer = null, string? destinationContainer = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // First copy the blob
                var copyResult = await CopyAsync(sourceBlobName, destinationBlobName, sourceContainer, destinationContainer, cancellationToken);

                if (!copyResult.Success)
                {
                    return copyResult;
                }

                // Then delete the source blob
                var deleteResult = await DeleteAsync(sourceBlobName, sourceContainer, cancellationToken);

                if (!deleteResult.Success)
                {
                    _logger.LogWarning("Copy succeeded but delete failed during move operation. Source blob {SourceBlob} still exists", sourceBlobName);
                    // Note: We don't return failure here because the copy succeeded
                }

                _logger.LogInformation("Successfully moved blob from {SourceContainer}/{SourceBlob} to {DestinationContainer}/{DestinationBlob}",
                    sourceContainer ?? _options.DefaultContainer, sourceBlobName,
                    destinationContainer ?? _options.DefaultContainer, destinationBlobName);

                return copyResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move blob from {SourceBlob} to {DestinationBlob}", sourceBlobName, destinationBlobName);
                return StorageResult<string>.Fail($"Failed to move blob: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<bool>> CreateContainerIfNotExistsAsync(string containerName, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var response = await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

                var created = response != null;
                _logger.LogInformation("Container {Container} creation result: {Created}", containerName, created ? "Created" : "Already exists");

                return StorageResult<bool>.Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create container {Container}", containerName);
                return StorageResult<bool>.Fail($"Failed to create container: {ex.Message}", ex);
            }
        }

        public async Task<StorageResult<bool>> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var response = await containerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

                _logger.LogInformation("Container {Container} deletion result: {Deleted}", containerName, response.Value);

                return StorageResult<bool>.Ok(response.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete container {Container}", containerName);
                return StorageResult<bool>.Fail($"Failed to delete container: {ex.Message}", ex);
            }
        }

        private async Task<BlobContainerClient> GetContainerClientAsync(string containerName, CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (_options.CreateContainerIfNotExists)
            {
                await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            }

            return containerClient;
        }
    }
}
