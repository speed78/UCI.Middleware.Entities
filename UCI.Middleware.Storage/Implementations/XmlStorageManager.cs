using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UCI.Middleware.Storage.Configuration;
using UCI.Middleware.Storage.Exceptions;
using UCI.Middleware.Storage.Interfaces;

namespace UCI.Middleware.Storage.Implementations
{
    /// <summary>
    /// XML Storage Manager with permanent links (never expire)
    /// </summary>
    public class XmlStorageManager : IXmlStorageManager
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly AzureStorageOptions _options;
        private readonly ILogger<XmlStorageManager> _logger;
        private readonly TelemetryClient _telemetryClient;

        public XmlStorageManager(
            IOptions<AzureStorageOptions> options,
            ILogger<XmlStorageManager> logger,
            TelemetryClient telemetryClient)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));

            if (string.IsNullOrEmpty(_options.ConnectionString))
            {
                throw new ArgumentException("Azure Storage connection string is required", nameof(options));
            }

            _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
        }

        #region Upload Operations

        public async Task<XmlUploadResult> UploadXmlAsync(
            Stream xmlStream,
            string fileName,
            string? containerName = null,
            Dictionary<string, string>? metadata = null)
        {
            var startTime = DateTime.UtcNow;
            var container = containerName ?? _options.DefaultContainer;

            try
            {
                _logger.LogInformation("Starting XML upload: {FileName} to container: {Container}", fileName, container);

                // Validation
                ValidateXmlFileName(fileName);
                await ValidateXmlStreamAsync(xmlStream);

                // Ensure container exists with public access for permanent links
                await EnsureContainerExistsAsync(container);

                // Generate unique blob name
                var blobName = GenerateUniqueBlobName(fileName);
                var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blobName);

                // Prepare upload options for XML
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "application/xml"
                    },
                    Metadata = metadata ?? new Dictionary<string, string>()
                };

                // Add XML-specific metadata
                uploadOptions.Metadata["UploadedAt"] = DateTime.UtcNow.ToString("O");
                uploadOptions.Metadata["OriginalFileName"] = fileName;
                uploadOptions.Metadata["FileSize"] = xmlStream.Length.ToString();
                uploadOptions.Metadata["FileType"] = "XML";

                // Extract and store XML root element
                var rootElement = await ExtractXmlRootElementAsync(xmlStream);
                if (!string.IsNullOrEmpty(rootElement))
                {
                    uploadOptions.Metadata["XmlRootElement"] = rootElement;
                }

                // Reset stream position
                xmlStream.Position = 0;

                // Upload XML file
                var response = await blobClient.UploadAsync(xmlStream, uploadOptions);

                // Generate permanent URL (public access, never expires)
                var permanentUrl = blobClient.Uri.ToString();

                var result = new XmlUploadResult
                {
                    Success = true,
                    BlobName = blobName,
                    ContainerName = container,
                    PermanentUrl = permanentUrl,
                    FileSize = xmlStream.Length,
                    UploadedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, string>(uploadOptions.Metadata)
                };

                // Track telemetry
                TrackOperation("Upload", container, blobName, true, DateTime.UtcNow - startTime, xmlStream.Length);

                _logger.LogInformation("XML uploaded successfully: {BlobName} ({FileSize} bytes) - URL: {Url}",
                    blobName, xmlStream.Length, permanentUrl);

                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to upload XML {fileName}: {ex.Message}";
                _logger.LogError(ex, errorMessage);

                TrackOperation("Upload", container, fileName, false, DateTime.UtcNow - startTime, xmlStream?.Length ?? 0, ex);

                return new XmlUploadResult
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        public async Task<XmlUploadResult> UploadXmlAsync(
            string xmlContent,
            string fileName,
            string? containerName = null,
            Dictionary<string, string>? metadata = null)
        {
            // Validate XML content
            ValidateXmlContent(xmlContent);

            var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
            using var stream = new MemoryStream(xmlBytes);
            return await UploadXmlAsync(stream, fileName, containerName, metadata);
        }

        public async Task<XmlUploadResult> UploadXmlFromPathAsync(
            string xmlFilePath,
            string? blobName = null,
            string? containerName = null,
            Dictionary<string, string>? metadata = null)
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"XML file not found: {xmlFilePath}");
            }

            // Validate file extension
            if (!Path.GetExtension(xmlFilePath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                throw new XmlValidationException("File must have .xml extension");
            }

            var fileName = blobName ?? Path.GetFileName(xmlFilePath);

            using var fileStream = File.OpenRead(xmlFilePath);
            return await UploadXmlAsync(fileStream, fileName, containerName, metadata);
        }

        public async Task<XmlUploadResult> UploadValidatedXmlAsync(
            string xmlContent,
            string fileName,
            string? containerName = null,
            Dictionary<string, string>? metadata = null)
        {
            // Strict XML validation before upload
            ValidateXmlContent(xmlContent);

            // Add validation metadata
            var validationMetadata = metadata ?? new Dictionary<string, string>();
            validationMetadata["XmlValidated"] = "true";
            validationMetadata["ValidatedAt"] = DateTime.UtcNow.ToString("O");

            return await UploadXmlAsync(xmlContent, fileName, containerName, validationMetadata);
        }

        #endregion

        #region Download Operations

        public async Task<XmlDownloadResult> DownloadXmlAsync(string fileName, string? containerName = null)
        {
            var startTime = DateTime.UtcNow;
            var container = containerName ?? _options.DefaultContainer;

            try
            {
                _logger.LogInformation("Starting XML download: {FileName} from container: {Container}", fileName, container);

                var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);

                // Check if blob exists
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    throw new XmlStorageException($"XML file {fileName} not found in container {container}", "Download", container, fileName);
                }

                // Download blob
                var response = await blobClient.DownloadStreamingAsync();
                var properties = await blobClient.GetPropertiesAsync();

                // Read XML content as string
                using var reader = new StreamReader(response.Value.Content);
                var xmlContent = await reader.ReadToEndAsync();

                // Reset stream for return
                var contentBytes = Encoding.UTF8.GetBytes(xmlContent);
                var contentStream = new MemoryStream(contentBytes);

                var result = new XmlDownloadResult
                {
                    Success = true,
                    Content = contentStream,
                    XmlContent = xmlContent,
                    FileName = fileName,
                    FileSize = properties.Value.ContentLength,
                    LastModified = properties.Value.LastModified.DateTime,
                    Metadata = new Dictionary<string, string>(properties.Value.Metadata)
                };

                TrackOperation("Download", container, fileName, true, DateTime.UtcNow - startTime, properties.Value.ContentLength);

                _logger.LogInformation("XML downloaded successfully: {FileName} ({FileSize} bytes)", fileName, properties.Value.ContentLength);

                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to download XML {fileName}: {ex.Message}";
                _logger.LogError(ex, errorMessage);

                TrackOperation("Download", container, fileName, false, DateTime.UtcNow - startTime, 0, ex);

                return new XmlDownloadResult
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        public async Task<string?> GetXmlContentAsync(string fileName, string? containerName = null)
        {
            var result = await DownloadXmlAsync(fileName, containerName);
            return result.Success ? result.XmlContent : null;
        }

        public async Task<bool> DownloadXmlToPathAsync(string fileName, string downloadPath, string? containerName = null)
        {
            try
            {
                var result = await DownloadXmlAsync(fileName, containerName);

                if (!result.Success || string.IsNullOrEmpty(result.XmlContent))
                    return false;

                // Ensure directory exists
                var directory = Path.GetDirectoryName(downloadPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(downloadPath, result.XmlContent, Encoding.UTF8);

                _logger.LogInformation("XML downloaded to path: {DownloadPath}", downloadPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download XML to path: {DownloadPath}", downloadPath);
                return false;
            }
        }

        #endregion

        #region File Management

        public async Task<bool> DeleteXmlAsync(string fileName, string? containerName = null)
        {
            var startTime = DateTime.UtcNow;
            var container = containerName ?? _options.DefaultContainer;

            try
            {
                _logger.LogInformation("Deleting XML: {FileName} from container: {Container}", fileName, container);

                var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);
                var response = await blobClient.DeleteIfExistsAsync();

                TrackOperation("Delete", container, fileName, response.Value, DateTime.UtcNow - startTime, 0);

                if (response.Value)
                {
                    _logger.LogInformation("XML deleted successfully: {FileName}", fileName);
                }
                else
                {
                    _logger.LogWarning("XML not found for deletion: {FileName}", fileName);
                }

                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete XML: {FileName}", fileName);
                TrackOperation("Delete", container, fileName, false, DateTime.UtcNow - startTime, 0, ex);
                return false;
            }
        }

        public async Task<bool> XmlExistsAsync(string fileName, string? containerName = null)
        {
            try
            {
                var container = containerName ?? _options.DefaultContainer;
                var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);
                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking XML existence: {FileName}", fileName);
                return false;
            }
        }

        public async Task<XmlFileInfo?> GetXmlInfoAsync(string fileName, string? containerName = null)
        {
            try
            {
                var container = containerName ?? _options.DefaultContainer;
                var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);

                var properties = await blobClient.GetPropertiesAsync();

                // Check if it's valid XML
                var isValidXml = await ValidateXmlAsync(fileName, container);
                var rootElement = properties.Value.Metadata.TryGetValue("XmlRootElement", out var root) ? root : null;

                return new XmlFileInfo
                {
                    Name = fileName,
                    ContainerName = container,
                    Size = properties.Value.ContentLength,
                    LastModified = properties.Value.LastModified.DateTime,
                    CreatedOn = properties.Value.CreatedOn.DateTime,
                    PermanentUrl = blobClient.Uri.ToString(), // Never expires
                    Metadata = new Dictionary<string, string>(properties.Value.Metadata),
                    ETag = properties.Value.ETag.ToString(),
                    IsValidXml = isValidXml,
                    XmlRootElement = rootElement
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get XML info: {FileName}", fileName);
                return null;
            }
        }

        public async Task<IEnumerable<XmlFileInfo>> ListXmlFilesAsync(string? containerName = null, string? prefix = null)
        {
            try
            {
                var container = containerName ?? _options.DefaultContainer;
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);

                var xmlFiles = new List<XmlFileInfo>();

                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    // Filter only XML files
                    if (!blobItem.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var rootElement = blobItem.Metadata.TryGetValue("XmlRootElement", out var root) ? root : null;

                    xmlFiles.Add(new XmlFileInfo
                    {
                        Name = blobItem.Name,
                        ContainerName = container,
                        Size = blobItem.Properties.ContentLength ?? 0,
                        LastModified = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue,
                        CreatedOn = blobItem.Properties.CreatedOn?.DateTime ?? DateTime.MinValue,
                        PermanentUrl = containerClient.GetBlobClient(blobItem.Name).Uri.ToString(),
                        Metadata = new Dictionary<string, string>(blobItem.Metadata),
                        ETag = blobItem.Properties.ETag?.ToString() ?? string.Empty,
                        IsValidXml = blobItem.Metadata.ContainsKey("XmlValidated"),
                        XmlRootElement = rootElement
                    });
                }

                _logger.LogInformation("Listed {FileCount} XML files from container: {Container}", xmlFiles.Count, container);

                return xmlFiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list XML files from container: {Container}", containerName);
                return Enumerable.Empty<XmlFileInfo>();
            }
        }

        #endregion

        #region XML-Specific Operations

        public async Task<bool> ValidateXmlAsync(string fileName, string? containerName = null)
        {
            try
            {
                var xmlContent = await GetXmlContentAsync(fileName, containerName);
                if (string.IsNullOrEmpty(xmlContent))
                    return false;

                ValidateXmlContent(xmlContent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetXmlRootElementAsync(string fileName, string? containerName = null)
        {
            try
            {
                var xmlContent = await GetXmlContentAsync(fileName, containerName);
                if (string.IsNullOrEmpty(xmlContent))
                    return null;

                using var stringReader = new StringReader(xmlContent);
                using var xmlReader = XmlReader.Create(stringReader);

                if (xmlReader.MoveToContent() == XmlNodeType.Element)
                {
                    return xmlReader.Name;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get XML root element for: {FileName}", fileName);
                return null;
            }
        }

        #endregion

        #region Permanent URL Operations

        public string GetPermanentUrl(string fileName, string? containerName = null)
        {
            var container = containerName ?? _options.DefaultContainer;
            var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);
            return blobClient.Uri.ToString(); // Public URL, never expires
        }

        public async Task<string?> GetPermanentUrlAsync(string fileName, string? containerName = null)
        {
            try
            {
                var exists = await XmlExistsAsync(fileName, containerName);
                return exists ? GetPermanentUrl(fileName, containerName) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permanent URL for: {FileName}", fileName);
                return null;
            }
        }

        #endregion

        #region Container Management

        public async Task<bool> CreateContainerAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                // Create with public access for permanent links
                var accessType = PublicAccessType.None; // o PublicAccessType.Blob se vuoi accesso pubblico
                var response = await containerClient.CreateIfNotExistsAsync(accessType);
                _logger.LogInformation("Container created or already exists: {ContainerName}", containerName);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create container: {ContainerName}", containerName);
                return false;
            }
        }

        public async Task<bool> DeleteContainerAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var response = await containerClient.DeleteIfExistsAsync();

                if (response.Value)
                {
                    _logger.LogInformation("Container deleted successfully: {ContainerName}", containerName);
                }

                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete container: {ContainerName}", containerName);
                return false;
            }
        }

        public async Task<IEnumerable<string>> ListContainersAsync()
        {
            try
            {
                var containers = new List<string>();

                await foreach (var container in _blobServiceClient.GetBlobContainersAsync())
                {
                    containers.Add(container.Name);
                }

                return containers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list containers");
                return Enumerable.Empty<string>();
            }
        }

        #endregion

        #region Utility Methods

        public async Task<long> GetContainerSizeAsync(string? containerName = null)
        {
            try
            {
                var container = containerName ?? _options.DefaultContainer;
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);

                long totalSize = 0;

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    // Count only XML files
                    if (blobItem.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        totalSize += blobItem.Properties.ContentLength ?? 0;
                    }
                }

                return totalSize;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate container size: {Container}", containerName);
                return 0;
            }
        }

        public async Task<int> GetXmlFileCountAsync(string? containerName = null)
        {
            try
            {
                var container = containerName ?? _options.DefaultContainer;
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);

                int count = 0;

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    if (blobItem.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to count XML files in container: {Container}", containerName);
                return 0;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task EnsureContainerExistsAsync(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var accessType = _options.EnablePublicAccess ? PublicAccessType.Blob : PublicAccessType.None;
            await containerClient.CreateIfNotExistsAsync(accessType);
        }

        private void ValidateXmlFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new XmlValidationException("File name cannot be empty");

            if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                throw new XmlValidationException("File must have .xml extension");
        }

        private async Task ValidateXmlStreamAsync(Stream xmlStream)
        {
            if (xmlStream == null)
                throw new XmlValidationException("XML stream cannot be null");

            if (xmlStream.Length == 0)
                throw new XmlValidationException("XML file cannot be empty");

            var maxSizeBytes = _options.MaxFileSizeMB * 1024 * 1024;
            if (xmlStream.Length > maxSizeBytes)
                throw new XmlValidationException($"XML file size exceeds maximum allowed size of {_options.MaxFileSizeMB}MB");

            // Validate XML content
            var position = xmlStream.Position;
            try
            {
                using var reader = XmlReader.Create(xmlStream, new XmlReaderSettings { ValidationType = ValidationType.None });
                while (reader.Read()) { } // Parse entire document
            }
            catch (XmlException ex)
            {
                throw new XmlValidationException($"Invalid XML content: {ex.Message}", ex);
            }
            finally
            {
                xmlStream.Position = position; // Reset position
            }
        }

        private void ValidateXmlContent(string xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
                throw new XmlValidationException("XML content cannot be empty");

            try
            {
                using var stringReader = new StringReader(xmlContent);
                using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { ValidationType = ValidationType.None });
                while (xmlReader.Read()) { } // Parse entire document
            }
            catch (XmlException ex)
            {
                throw new XmlValidationException($"Invalid XML content: {ex.Message}", ex);
            }
        }

        private async Task<string?> ExtractXmlRootElementAsync(Stream xmlStream)
        {
            try
            {
                var position = xmlStream.Position;
                using var reader = XmlReader.Create(xmlStream);

                if (reader.MoveToContent() == XmlNodeType.Element)
                {
                    var rootElement = reader.Name;
                    xmlStream.Position = position; // Reset position
                    return rootElement;
                }

                xmlStream.Position = position; // Reset position
                return null;
            }
            catch
            {
                return null;
            }
        }

        private string GenerateUniqueBlobName(string fileName)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            return $"{nameWithoutExtension}_{timestamp}_{guid}.xml";
        }

        private void TrackOperation(string operation, string container, string fileName, bool success, TimeSpan duration, long fileSize, Exception? exception = null)
        {
            var properties = new Dictionary<string, string>
            {
                ["Operation"] = operation,
                ["Container"] = container,
                ["FileName"] = fileName,
                ["Success"] = success.ToString(),
                ["Layer"] = "XmlStorage",
                ["FileType"] = "XML"
            };

            var metrics = new Dictionary<string, double>
            {
                ["Duration"] = duration.TotalMilliseconds,
                ["FileSize"] = fileSize
            };

            if (exception != null)
            {
                properties["ExceptionType"] = exception.GetType().Name;
                properties["ExceptionMessage"] = exception.Message;
                _telemetryClient.TrackException(exception, properties, metrics);
            }

            _telemetryClient.TrackEvent($"XmlStorage{operation}", properties, metrics);
        }

        #endregion
    }
}


