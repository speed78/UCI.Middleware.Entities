using Microsoft.EntityFrameworkCore;
using UCI.Middleware.Entities.Context;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Repositories.Interfaces;

namespace UCI.Middleware.Repositories.Implementations;

public class ClaimsSubmissionRepository : Repository<ClaimsSubmission>, IClaimsSubmissionRepository
{
    public ClaimsSubmissionRepository(UciDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId)
    {
        return await _dbSet
            .Where(cs => cs.CorrespondentId == correspondentId)
            .Include(cs => cs.Correspondent)
            .Include(cs => cs.SubmissionStatus)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClaimsSubmission>> GetByStatusAsync(int statusId)
    {
        return await _dbSet
            .Where(cs => cs.SubmissionStatusId == statusId)
            .Include(cs => cs.SubmissionStatus)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClaimsSubmission>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(cs => cs.UploadDate >= from && cs.UploadDate <= to)
            .OrderByDescending(cs => cs.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClaimsSubmission>> GetPendingSubmissionsAsync()
    {
        return await _dbSet
            .Where(cs => cs.SubmissionStatus.Description == "Uploaded" ||
                       cs.SubmissionStatus.Description == "SizeValidated" ||
                       cs.SubmissionStatus.Description == "SchemaValidated")
            .Include(cs => cs.SubmissionStatus)
            .Include(cs => cs.Correspondent)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClaimsSubmission>> GetSubmissionsWithErrorsAsync()
    {
        return await _dbSet
            .Where(cs => cs.FlowErrors.Any() || cs.Claims.Any(c => c.ClaimErrors.Any()))
            .Include(cs => cs.FlowErrors)
            .Include(cs => cs.Claims)
                .ThenInclude(c => c.ClaimErrors)
            .ToListAsync();
    }

    public async Task<ClaimsSubmission?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Where(cs => cs.Id == id)
            .Include(cs => cs.Correspondent)
            .Include(cs => cs.SubmissionStatus)
            .Include(cs => cs.FlowErrors)
                .ThenInclude(fe => fe.Error)
            .Include(cs => cs.Claims)
                .ThenInclude(c => c.ClaimErrors)
                    .ThenInclude(ce => ce.Error)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ClaimsSubmission>> GetWithCorrespondentAsync()
    {
        return await _dbSet
            .Include(cs => cs.Correspondent)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClaimsSubmission>> GetWithStatusAsync()
    {
        return await _dbSet
            .Include(cs => cs.SubmissionStatus)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(int statusId)
    {
        return await _dbSet
            .CountAsync(cs => cs.SubmissionStatusId == statusId);
    }

    public async Task<Dictionary<int, int>> GetSubmissionCountsByStatusAsync()
    {
        return await _dbSet
            .GroupBy(cs => cs.SubmissionStatusId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<object>> GetSubmissionStatsByDateAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(cs => cs.UploadDate >= from && cs.UploadDate <= to)
            .GroupBy(cs => cs.UploadDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                StatusCounts = g.GroupBy(cs => cs.SubmissionStatusId)
                               .ToDictionary(sg => sg.Key, sg => sg.Count())
            })
            .ToListAsync();
    }
}