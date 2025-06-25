namespace UCI.Middleware.Repositories.Configuration
{
    /// <summary>
    /// Opzioni di configurazione per il database
    /// </summary>
    public class RepositoryOptions
    {
        public const string SectionName = "Database";

        public string ConnectionString { get; set; } = string.Empty;
        public string Provider { get; set; } = "SqlServer";
        public int CommandTimeout { get; set; } = 30;

    }
}
