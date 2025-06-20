namespace UCI.Middleware.Storage.Configuration
{
    /// <summary>
    /// XML file upload result
    /// </summary>
    public class XmlUploadResult
    {
        public bool Success { get; set; }
        public string? BlobName { get; set; }
        public string? ContainerName { get; set; }
        public string? PermanentUrl { get; set; } // Always accessible URL
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
