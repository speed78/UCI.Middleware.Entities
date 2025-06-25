namespace UCI.Middleware.Integration.Storage.Models
{
    public class UploadOptions
    {
        public string? ContentType { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public bool Overwrite { get; set; } = true;
        public string? Container { get; set; }
    }
}
