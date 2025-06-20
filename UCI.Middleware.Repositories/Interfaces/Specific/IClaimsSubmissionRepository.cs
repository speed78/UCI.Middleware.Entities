
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Entities.Enums.Ivass;

namespace UCI.Middleware.Repositories.Interfaces.Specific
{
    /// <summary>
    /// Repository interface for ClaimsSubmission entity with specific business operations.
    /// </summary>
    public interface IClaimsSubmissionRepository : IRepository<ClaimsSubmission>
    {
        /// <summary>
        /// Gets submissions by their status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Collection of submissions with the specified status</returns>
        Task<IEnumerable<ClaimsSubmission>> GetByStatusAsync(SubmissionStatusType status);

        /// <summary>
        /// Gets submissions by correspondent.
        /// </summary>
        /// <param name="correspondentId">The correspondent identifier</param>
        /// <returns>Collection of submissions for the specified correspondent</returns>
        Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId);

        
        /// <summary>
        /// Gets a submission with all related data (claims, errors, etc.).
        /// </summary>
        /// <param name="id">The submission identifier</param>
        /// <returns>The complete submission data if found</returns>
        Task<ClaimsSubmission?> GetCompleteAsync(Guid id);

        /// <summary>
        /// Gets submissions that are pending response from IVASS.
        /// </summary>
        /// <param name="hoursInterval"></param>
        /// <returns>Collection of submissions pending response</returns>
        Task<IEnumerable<ClaimsSubmission>> GetPendingResponseAsync(int hoursInterval);


        Task<ClaimsSubmission> GetByProtocolAsync(string protocol);


    }
}