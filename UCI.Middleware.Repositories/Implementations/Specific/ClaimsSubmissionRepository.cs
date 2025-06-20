using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Repositories.Interfaces.Specific;

namespace UCI.Middleware.Repositories.Implementations.Specific
{
    /// <summary>
    /// Specialized repository for ClaimsSubmission entities with business-specific operations.
    /// </summary>
    public class ClaimsSubmissionRepository : Repository<ClaimsSubmission>, IClaimsSubmissionRepository
    {
        private readonly ILogger<ClaimsSubmissionRepository> _specificLogger;

        public ClaimsSubmissionRepository(DbContext context, ILogger<ClaimsSubmissionRepository> logger)
            : base(context, logger as ILogger<Repository<ClaimsSubmission>>)
        {
            _specificLogger = logger;
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetByStatusAsync(int statusId)
        {
            try
            {
                _specificLogger.LogDebug("Getting ClaimsSubmissions by status {StatusId}", statusId);
                var result = await _dbSet
                    .Where(x => x.SubmissionStatusId == statusId)
                    .Include(x => x.SubmissionStatus)
                    .ToListAsync();
                _specificLogger.LogInformation("Found {Count} ClaimsSubmissions with status {StatusId}", result.Count(), statusId);
                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting ClaimsSubmissions by status {StatusId}", statusId);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId)
        {
            try
            {
                _specificLogger.LogDebug("Getting ClaimsSubmissions by correspondent {CorrespondentId}", correspondentId);
                var result = await _dbSet
                    .Where(x => x.CorrespondentId == correspondentId)
                    .Include(x => x.Correspondent)
                    .Include(x => x.SubmissionStatus)
                    .ToListAsync();
                _specificLogger.LogInformation("Found {Count} ClaimsSubmissions for correspondent {CorrespondentId}", result.Count(), correspondentId);
                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting ClaimsSubmissions by correspondent {CorrespondentId}", correspondentId);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _specificLogger.LogDebug("Getting ClaimsSubmissions from {FromDate} to {ToDate}", fromDate, toDate);
                var result = await _dbSet
                    .Where(x => x.UploadDate >= fromDate && x.UploadDate <= toDate)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .OrderByDescending(x => x.UploadDate)
                    .ToListAsync();
                _specificLogger.LogInformation("Found {Count} ClaimsSubmissions in date range {FromDate} to {ToDate}", result.Count(), fromDate, toDate);
                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting ClaimsSubmissions by date range {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<ClaimsSubmission?> GetWithClaimsAsync(Guid id)
        {
            try
            {
                _specificLogger.LogDebug("Getting ClaimsSubmission {Id} with claims", id);
                var result = await _dbSet
                    .Include(x => x.Claims)
                        .ThenInclude(c => c.ClaimErrors)
                            .ThenInclude(ce => ce.Error)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (result != null)
                    _specificLogger.LogInformation("Found ClaimsSubmission {Id} with {ClaimsCount} claims", id, result.Claims.Count);
                else
                    _specificLogger.LogWarning("ClaimsSubmission {Id} not found", id);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting ClaimsSubmission {Id} with claims", id);
                throw;
            }
        }

        public async Task<ClaimsSubmission?> GetWithFlowErrorsAsync(Guid id)
        {
            try
            {
                _specificLogger.LogDebug("Getting ClaimsSubmission {Id} with flow errors", id);
                var result = await _dbSet
                    .Include(x => x.FlowErrors)
                        .ThenInclude(fe => fe.Error)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (result != null)
                    _specificLogger.LogInformation("Found ClaimsSubmission {Id} with {ErrorsCount} flow errors", id, result.FlowErrors.Count);
                else
                    _specificLogger.LogWarning("ClaimsSubmission {Id} not found", id);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting ClaimsSubmission {Id} with flow errors", id);
                throw;
            }
        }

        public async Task<ClaimsSubmission?> GetCompleteAsync(Guid id)
        {
            try
            {
                _specificLogger.LogDebug("Getting complete ClaimsSubmission {Id}", id);
                var result = await _dbSet
                    .Include(x => x.Claims)
                        .ThenInclude(c => c.ClaimErrors)
                            .ThenInclude(ce => ce.Error)
                    .Include(x => x.FlowErrors)
                        .ThenInclude(fe => fe.Error)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (result != null)
                    _specificLogger.LogInformation("Found complete ClaimsSubmission {Id} with {ClaimsCount} claims and {ErrorsCount} flow errors",
                        id, result.Claims.Count, result.FlowErrors.Count);
                else
                    _specificLogger.LogWarning("ClaimsSubmission {Id} not found", id);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting complete ClaimsSubmission {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetPendingResponseAsync()
        {
            try
            {
                _specificLogger.LogDebug("Getting pending response ClaimsSubmissions");
                var result = await _dbSet
                    .Where(x => x.SendDate.HasValue && !x.ResponseDate.HasValue)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .OrderBy(x => x.SendDate)
                    .ToListAsync();
                _specificLogger.LogInformation("Found {Count} ClaimsSubmissions pending response", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting pending response ClaimsSubmissions");
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetByProtocolAsync(string protocol)
        {
            try
            {
                _specificLogger.LogDebug("Getting ClaimsSubmissions by protocol {Protocol}", protocol);
                var result = await _dbSet
                    .Where(x => x.Protocol == protocol)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .ToListAsync();
                _specificLogger.LogInformation("Found {Count} ClaimsSubmissions with protocol {Protocol}", result.Count(), protocol);
                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting ClaimsSubmissions by protocol {Protocol}", protocol);
                throw;
            }
        }
    }
}
