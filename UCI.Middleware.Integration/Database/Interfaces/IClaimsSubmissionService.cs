using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Entities.Enums.Ivass;
using UCI.Middleware.Integration.Database.Implementation;
using UCI.Middleware.Services.Interfaces;

namespace UCI.Middleware.Integration.Database.Interfaces
{
    /// <summary>
    /// Service interface for ClaimsSubmission business operations.
    /// </summary>
    public interface IClaimsSubmissionService : IBaseService<ClaimsSubmission, Guid>
    {
        Task<ClaimsSubmission?> GetSubmission(Guid id);

        /// <summary>
        /// Creates a new submission from file upload with business validation.
        /// </summary>
        /// <param name="inputFileName">The name of the input file</param>
        /// <param name="inputFileFullPath">The full path to the input file</param>
        /// <param name="correspondentId">The correspondent identifier (optional)</param>
        /// <returns>The created submission</returns>
        Task<ClaimsSubmission> CreateSubmissionFromFileAsync(string inputFileName, string inputFileFullPath, Guid correspondentId);

        /// <summary>
        /// Updates the submission status with validation.
        /// </summary>
        /// <param name="submissionId">The submission identifier</param>
        /// <param name="status"></param>
        /// <returns>The updated submission</returns>
        Task<ClaimsSubmission> UpdateStatusAsync(Guid submissionId, SubmissionStatusType status);

        /// <summary>
        /// Marks a submission as sent with protocol and timestamp.
        /// </summary>
        /// <param name="submissionId">The submission identifier</param>
        /// <param name="protocol">The protocol number assigned by IVASS</param>
        /// <returns>The updated submission</returns>
        Task<ClaimsSubmission> MarkAsSentAsync(Guid submissionId, string protocol);

        /// <summary>
        /// Records the response from IVASS for a submission.
        /// </summary>
        /// <param name="submissionId">The submission identifier</param>
        /// <param name="outputFileName">The response file name (optional)</param>
        /// <param name="outputFileFullPath">The response file path (optional)</param>
        /// <returns>The updated submission</returns>
        Task<ClaimsSubmission> CreateResponseAsync(Guid submissionId, string? outputFileName = null, string? outputFileFullPath = null);

        /// <summary>
        /// Gets submissions by their status with pagination.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated submissions with the specified status</returns>
        Task<PaginatedResult<ClaimsSubmission>> GetByStatusPagedAsync(SubmissionStatusType status, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// Gets submissions by correspondent with optional date filtering.
        /// </summary>
        /// <param name="correspondentId">The correspondent identifier</param>
        /// <param name="fromDate">Start date filter (optional)</param>
        /// <param name="toDate">End date filter (optional)</param>
        /// <returns>Collection of submissions for the correspondent</returns>
        Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Gets a submission with all related data (claims, errors, etc.).
        /// </summary>
        /// <param name="id">The submission identifier</param>
        /// <returns>The complete submission data if found</returns>
        Task<ClaimsSubmission?> GetSubmissionWithErrorsAsync(Guid id);

        /// <summary>
        /// Gets submissions that are pending response from IVASS.
        /// </summary>
        /// <param name="hoursInterval">Filter submissions older than specified hours (optional)</param>
        /// <returns>Collection of submissions pending response</returns>
        Task<IEnumerable<ClaimsSubmission>> GetPendingResponseAsync(int hoursInterval);

        /// <summary>
        /// Retrieves a submission by its IVASS protocol number.
        /// </summary>
        /// <param name="protocol">The protocol number assigned by IVASS</param>
        /// <returns>The submission associated with the protocol if found</returns>
        Task<ClaimsSubmission> GetByProtocolAsync(string protocol);

    }

}