using UCI.Middleware.Entities.Entities.Ivass;

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
        /// <param name="statusId">The status identifier</param>
        /// <returns>Collection of submissions with the specified status</returns>
        Task<IEnumerable<ClaimsSubmission>> GetByStatusAsync(int statusId);

        /// <summary>
        /// Gets submissions by correspondent.
        /// </summary>
        /// <param name="correspondentId">The correspondent identifier</param>
        /// <returns>Collection of submissions for the specified correspondent</returns>
        Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId);

        /// <summary>
        /// Gets submissions within a date range.
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Collection of submissions within the date range</returns>
        Task<IEnumerable<ClaimsSubmission>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Gets a submission with its related claims and errors.
        /// </summary>
        /// <param name="id">The submission identifier</param>
        /// <returns>The submission with claims data if found</returns>
        Task<ClaimsSubmission?> GetWithClaimsAsync(Guid id);

        /// <summary>
        /// Gets a submission with its flow errors.
        /// </summary>
        /// <param name="id">The submission identifier</param>
        /// <returns>The submission with flow errors if found</returns>
        Task<ClaimsSubmission?> GetWithFlowErrorsAsync(Guid id);

        /// <summary>
        /// Gets a submission with all related data (claims, errors, etc.).
        /// </summary>
        /// <param name="id">The submission identifier</param>
        /// <returns>The complete submission data if found</returns>
        Task<ClaimsSubmission?> GetCompleteAsync(Guid id);

        /// <summary>
        /// Gets submissions that are pending response from IVASS.
        /// </summary>
        /// <returns>Collection of submissions pending response</returns>
        Task<IEnumerable<ClaimsSubmission>> GetPendingResponseAsync();

        /// <summary>
        /// Gets submissions by protocol number.
        /// </summary>
        /// <param name="protocol">The protocol number</param>
        /// <returns>Collection of submissions with the specified protocol</returns>
        Task<IEnumerable<ClaimsSubmission>> GetByProtocolAsync(string protocol);
    }
}
