using UCI.Middleware.Integrations.Storage.Models;

namespace UCI.Middleware.Integrations.Storage.Interfaces
{
    public interface IStorageService
    {
        // Upload operations
        Task<StorageResult<string>> UploadAsync(string blobName, Stream content, UploadOptions? options = null, CancellationToken cancellationToken = default);
        Task<StorageResult<string>> UploadAsync(string blobName, byte[] content, UploadOptions? options = null, CancellationToken cancellationToken = default);
        Task<StorageResult<string>> UploadTextAsync(string blobName, string content, UploadOptions? options = null, CancellationToken cancellationToken = default);

        // Download operations
        Task<StorageResult<Stream>> DownloadAsync(string blobName, string? container = null, CancellationToken cancellationToken = default);
        Task<StorageResult<byte[]>> DownloadBytesAsync(string blobName, string? container = null, CancellationToken cancellationToken = default);
        Task<StorageResult<string>> DownloadTextAsync(string blobName, string? container = null, CancellationToken cancellationToken = default);

        // Metadata operations
        Task<StorageResult<BlobInfo>> GetBlobInfoAsync(string blobName, string? container = null, CancellationToken cancellationToken = default);
        Task<StorageResult<bool>> ExistsAsync(string blobName, string? container = null, CancellationToken cancellationToken = default);

        // List operations
        Task<StorageResult<IEnumerable<BlobInfo>>> ListBlobsAsync(string? prefix = null, string? container = null, CancellationToken cancellationToken = default);

        // Copy/Move operations
        Task<StorageResult<string>> CopyAsync(string sourceBlobName, string destinationBlobName, string? sourceContainer = null, string? destinationContainer = null, CancellationToken cancellationToken = default);
        Task<StorageResult<string>> MoveAsync(string sourceBlobName, string destinationBlobName, string? sourceContainer = null, string? destinationContainer = null, CancellationToken cancellationToken = default);

        // Delete operations
        Task<StorageResult<bool>> DeleteAsync(string blobName, string? container = null, CancellationToken cancellationToken = default);
        Task<StorageResult<int>> DeleteBatchAsync(IEnumerable<string> blobNames, string? container = null, CancellationToken cancellationToken = default);

        // Container operations
        Task<StorageResult<bool>> CreateContainerIfNotExistsAsync(string containerName, CancellationToken cancellationToken = default);
        Task<StorageResult<bool>> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);
    }
}
