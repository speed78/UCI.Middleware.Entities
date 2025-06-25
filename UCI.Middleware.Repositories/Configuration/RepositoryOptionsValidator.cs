using Microsoft.Extensions.Options;

namespace UCI.Middleware.Repositories.Configuration
{
    /// <summary>
    /// Validates configuration options for repository connections.
    /// Implements IValidateOptions to provide validation during application startup.
    /// </summary>
    public class RepositoryOptionsValidator : IValidateOptions<RepositoryOptions>
    {
        /// <summary>
        /// Validates the repository configuration options.
        /// </summary>
        /// <param name="name">The name of the options instance being validated.</param>
        /// <param name="options">The options instance to validate.</param>
        /// <returns>
        /// A <see cref="ValidateOptionsResult"/> containing the validation result:
        /// - Success if all validation checks pass.
        /// - Failure with a list of validation errors if any checks fail.
        /// </returns>
        public ValidateOptionsResult Validate(string? name, RepositoryOptions options)
        {
            var failures = new List<string>();

            // ConnectionString
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                failures.Add("Repository ConnectionString is required");
            }

            // Provider
            var validProviders = new[] { "SqlServer" };
            if (string.IsNullOrEmpty(options.Provider) || !validProviders.Contains(options.Provider))
            {
                failures.Add($"Repository Provider must be one of: {string.Join(", ", validProviders)}");
            }

            // Command timeout
            if (options.CommandTimeout < 0 || options.CommandTimeout > 300)
            {
                failures.Add("CommandTimeout must be between 0 and 300 seconds");
            }
            
            if (failures.Any())
            {
                return ValidateOptionsResult.Fail(failures);
            }

            return ValidateOptionsResult.Success;
        }
    }
}
