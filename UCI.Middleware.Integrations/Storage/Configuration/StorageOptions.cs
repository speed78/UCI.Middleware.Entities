namespace UCI.Middleware.Integrations.Storage.Configuration
{
    public class StorageOptions
    {
        public const string SectionName = "Storage";

        public string ConnectionString { get; set; } = string.Empty;
        public string DefaultContainer { get; set; } = "incoming";
        public int TimeoutSeconds { get; set; } = 30;
        public bool CreateContainerIfNotExists { get; set; } = true;

        /// <summary>
        /// Container names for different stages
        /// </summary>
        public ContainerNames Containers { get; set; } = new();
    }

    public class ContainerNames
    {
        public string Incoming { get; set; } = "incoming";
        public string Processed { get; set; } = "processed";
        public string Failed { get; set; } = "failed";
    }
}
