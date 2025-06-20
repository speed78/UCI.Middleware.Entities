using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCI.Middleware.Storage.Configuration;

namespace UCI.Middleware.Storage.Interfaces
{
    /// <summary>
    /// Interface for XML file storage operations with permanent links
    /// </summary>
    public interface IXmlStorageManager
    {
        // Upload operations
        Task<XmlUploadResult> UploadXmlAsync(Stream xmlStream, string fileName, string? containerName = null, Dictionary<string, string>? metadata = null);
        Task<XmlUploadResult> UploadXmlAsync(string xmlContent, string fileName, string? containerName = null, Dictionary<string, string>? metadata = null);
        Task<XmlUploadResult> UploadXmlFromPathAsync(string xmlFilePath, string? blobName = null, string? containerName = null, Dictionary<string, string>? metadata = null);

        // Download operations
        Task<XmlDownloadResult> DownloadXmlAsync(string fileName, string? containerName = null);
        Task<string?> GetXmlContentAsync(string fileName, string? containerName = null);
        Task<bool> DownloadXmlToPathAsync(string fileName, string downloadPath, string? containerName = null);

        // File management
        Task<bool> DeleteXmlAsync(string fileName, string? containerName = null);
        Task<bool> XmlExistsAsync(string fileName, string? containerName = null);
        Task<XmlFileInfo?> GetXmlInfoAsync(string fileName, string? containerName = null);
        Task<IEnumerable<XmlFileInfo>> ListXmlFilesAsync(string? containerName = null, string? prefix = null);

        // XML-specific operations
        Task<bool> ValidateXmlAsync(string fileName, string? containerName = null);
        Task<string?> GetXmlRootElementAsync(string fileName, string? containerName = null);
        Task<XmlUploadResult> UploadValidatedXmlAsync(string xmlContent, string fileName, string? containerName = null, Dictionary<string, string>? metadata = null);

        // Permanent URL operations (never expire)
        string GetPermanentUrl(string fileName, string? containerName = null);
        Task<string?> GetPermanentUrlAsync(string fileName, string? containerName = null);

        // Container management
        Task<bool> CreateContainerAsync(string containerName);
        Task<bool> DeleteContainerAsync(string containerName);
        Task<IEnumerable<string>> ListContainersAsync();

        // Utility methods
        Task<long> GetContainerSizeAsync(string? containerName = null);
        Task<int> GetXmlFileCountAsync(string? containerName = null);
    }
}
