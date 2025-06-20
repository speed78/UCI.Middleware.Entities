namespace UCI.Middleware.Storage.Configuration
{
    /// <summary>
    /// XML file download result
    /// </summary>
    public class XmlDownloadResult
    {
        public bool Success { get; set; }
        public Stream? Content { get; set; }
        public string? XmlContent { get; set; } // Direct XML string
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
