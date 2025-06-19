using UCI.Middleware.Entities.Entities.Ivass;

namespace UCI.Middleware.Repositories.Interfaces;

/// <summary>
/// Specific repository for ClaimsSubmission with business-specific methods
/// </summary>
public interface IClaimsSubmissionRepository : IRepository<ClaimsSubmission>
{
    // Business-specific queries
    Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId);
    Task<IEnumerable<ClaimsSubmission>> GetByStatusAsync(int statusId);
    Task<IEnumerable<ClaimsSubmission>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<ClaimsSubmission>> GetPendingSubmissionsAsync();
    Task<IEnumerable<ClaimsSubmission>> GetSubmissionsWithErrorsAsync();

    // Complex queries with includes
    Task<ClaimsSubmission?> GetWithDetailsAsync(Guid id);
    Task<IEnumerable<ClaimsSubmission>> GetWithCorrespondentAsync();
    Task<IEnumerable<ClaimsSubmission>> GetWithStatusAsync();

    // Statistics
    Task<int> GetCountByStatusAsync(int statusId);
    Task<Dictionary<int, int>> GetSubmissionCountsByStatusAsync();
    Task<IEnumerable<object>> GetSubmissionStatsByDateAsync(DateTime from, DateTime to);
}