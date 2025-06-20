using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCI.Middleware.Storage.Configuration
{
    /// <summary>
    /// XML file information
    /// </summary>
    public class XmlFileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PermanentUrl { get; set; } = string.Empty; // Never expires
        public Dictionary<string, string> Metadata { get; set; } = new();
        public string ETag { get; set; } = string.Empty;
        public bool IsValidXml { get; set; }
        public string? XmlRootElement { get; set; }
    }
}
