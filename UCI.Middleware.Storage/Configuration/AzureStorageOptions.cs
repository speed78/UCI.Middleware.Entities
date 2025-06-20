using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.AccessControl;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace UCI.Middleware.Storage.Configuration
{
    ///<summary>
    /// Configuration settings for Azure Storage
    /// </summary>
    public class AzureStorageOptions
    {
        public const string SectionName = "AzureStorage";

        public string ConnectionString { get; set; } = string.Empty;
        public string DefaultContainer { get; set; } = "uci-files";
        public int MaxFileSizeMB { get; set; } = 100;
        public string[] AllowedFileTypes { get; set; } = { ".xml" };
        public bool EnableLogging { get; set; } = true;
        public int UrlExpirationHours { get; set; } = 24;
        public bool EnablePublicAccess { get; set; } = false;
    }
}
