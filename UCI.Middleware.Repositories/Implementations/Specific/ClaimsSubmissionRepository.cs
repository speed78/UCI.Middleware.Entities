using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Entities.Enums.Ivass;
using UCI.Middleware.Repositories.Interfaces.Specific;

namespace UCI.Middleware.Repositories.Implementations.Specific
{
    /// <summary>
    /// Specialized repository for ClaimsSubmission entities with business-specific operations.
    /// </summary>
    public class ClaimsSubmissionRepository(DbContext context, ILogger<ClaimsSubmissionRepository> logger)
        : Repository<ClaimsSubmission>(context, logger),
            IClaimsSubmissionRepository
    {
        public async Task<IEnumerable<ClaimsSubmission>> GetByStatusAsync(SubmissionStatusType status)
        {
            try
            {
                logger.LogDebug("Getting ClaimsSubmissions by status {StatusId}", status);
                var result = await _dbSet
                    .Where(x => x.SubmissionStatusId == status.GetId())
                    .Include(x => x.SubmissionStatus)
                    .ToListAsync();
                logger.LogInformation("Found {Count} ClaimsSubmissions with status {StatusId}", result.Count(), status);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting ClaimsSubmissions by status {StatusId}", status);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId)
        {
            try
            {
                logger.LogDebug("Getting ClaimsSubmissions by correspondent {CorrespondentId}", correspondentId);
                var result = await _dbSet
                    .Where(x => x.CorrespondentId == correspondentId)
                    .Include(x => x.Correspondent)
                    .Include(x => x.SubmissionStatus)
                    .ToListAsync();
                logger.LogInformation("Found {Count} ClaimsSubmissions for correspondent {CorrespondentId}", result.Count(), correspondentId);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting ClaimsSubmissions by correspondent {CorrespondentId}", correspondentId);
                throw;
            }
        }


        public async Task<ClaimsSubmission?> GetWithClaimsAsync(Guid id)
        {
            try
            {
                logger.LogDebug("Getting ClaimsSubmission {Id} with claims", id);
                var result = await _dbSet
                    .Include(x => x.Claims)
                        .ThenInclude(c => c.ClaimErrors)
                            .ThenInclude(ce => ce.Error)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (result != null)
                    logger.LogInformation("Found ClaimsSubmission {Id} with {ClaimsCount} claims", id, result.Claims.Count);
                else
                    logger.LogWarning("ClaimsSubmission {Id} not found", id);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting ClaimsSubmission {Id} with claims", id);
                throw;
            }
        }

        public async Task<ClaimsSubmission?> GetCompleteAsync(Guid id)
        {
            try
            {
                logger.LogDebug("Getting complete ClaimsSubmission {Id}", id);
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
                    logger.LogInformation("Found complete ClaimsSubmission {Id} with {ClaimsCount} claims and {ErrorsCount} flow errors",
                        id, result.Claims.Count, result.FlowErrors.Count);
                else
                    logger.LogWarning("ClaimsSubmission {Id} not found", id);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting complete ClaimsSubmission {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetPendingResponseAsync(int hoursInterval)
        {
            try
            {
                logger.LogDebug("Getting pending response ClaimsSubmissions older than {Hours} hours", hoursInterval);

                var query = _dbSet
                    .Where(x => x.SendDate.HasValue && x.SubmissionStatusId == SubmissionStatusType.Sent.GetId());


                var cutoffDate = DateTime.UtcNow.AddHours(-hoursInterval);
                query = query.Where(x =>
                    !x.LastResponseAttemptDate.HasValue ||
                    x.LastResponseAttemptDate.Value <= cutoffDate
                );


                var result = await query
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .OrderBy(x => x.SendDate)
                    .ToListAsync();

                logger.LogInformation("Found {Count} ClaimsSubmissions pending response", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting pending response ClaimsSubmissions");
                throw;
            }
        }

        public async Task<ClaimsSubmission> GetByProtocolAsync(string protocol)
        {
            try
            {
                logger.LogDebug("Getting ClaimsSubmissions by protocol {Protocol}", protocol);
                var result = await _dbSet
                    .Where(x => x.Protocol == protocol)
                    .Include(x => x.SubmissionStatus)
                    .Include(x => x.Correspondent)
                    .FirstOrDefaultAsync();

                if (result != null)
                {
                    logger.LogInformation("Found ClaimsSubmission with protocol {Protocol}", protocol);
                    return result;
                }

                logger.LogWarning("No ClaimsSubmission found with protocol {Protocol}", protocol);
                throw new KeyNotFoundException($"No ClaimsSubmission found with protocol {protocol}");
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting ClaimsSubmissions by protocol {Protocol}", protocol);
                throw new InvalidOperationException($"Failed to retrieve ClaimsSubmissions with protocol {protocol}", ex);
            }
        }
    }
}