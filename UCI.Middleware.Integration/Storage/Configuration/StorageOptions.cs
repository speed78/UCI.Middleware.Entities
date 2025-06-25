namespace UCI.Middleware.Integration.Storage.Configuration
{
    public class StorageOptions
    {
        public const string SectionName = "Storage";

        public string ConnectionString { get; set; } = string.Empty;
        public string DefaultContainer { get; set; } = "default";
        public int TimeoutSeconds { get; set; } = 30;
        public bool CreateContainerIfNotExists { get; set; } = true;
    }
}
