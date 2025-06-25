namespace UCI.Middleware.Integration.Storage.Models
{
    public class BlobInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string ETag { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
