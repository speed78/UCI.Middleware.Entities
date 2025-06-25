using Microsoft.Extensions.Options;

namespace UCI.Middleware.Integrations.Storage.Configuration
{
    // <summary>
    /// Validator per StorageOptions
    /// </summary>
    public class StorageOptionsValidator : IValidateOptions<StorageOptions>
    {
        public ValidateOptionsResult Validate(string? name, StorageOptions options)
        {
            var failures = new List<string>();

            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                failures.Add("Storage ConnectionString is required");
            }

            if (string.IsNullOrEmpty(options.DefaultContainer) || options.DefaultContainer.Length < 3)
            {
                failures.Add("DefaultContainer must be at least 3 characters");
            }

            if (options.TimeoutSeconds < 5 || options.TimeoutSeconds > 300)
            {
                failures.Add("TimeoutSeconds must be between 5 and 300");
            }

            if (failures.Any())
            {
                return ValidateOptionsResult.Fail(failures);
            }

            return ValidateOptionsResult.Success;
        }
    }

}
