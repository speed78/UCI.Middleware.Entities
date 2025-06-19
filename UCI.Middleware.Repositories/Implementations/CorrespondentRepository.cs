using Microsoft.EntityFrameworkCore;
using UCI.Middleware.Entities.Context;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Repositories.Interfaces;

namespace UCI.Middleware.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Correspondent entity with business-specific methods
    /// </summary>
    public class CorrespondentRepository : Repository<Correspondent>, ICorrespondentRepository
    {
        public CorrespondentRepository(UciDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get correspondent by business code (e.g., "001-000075")
        /// </summary>
        /// <param name="code">The business code</param>
        /// <returns>Correspondent or null if not found</returns>
        public async Task<Correspondent?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        /// <summary>
        /// Get correspondent by UCI code (e.g., "000075")
        /// </summary>
        /// <param name="uciCode">The UCI code</param>
        /// <returns>Correspondent or null if not found</returns>
        public async Task<Correspondent?> GetByUciCodeAsync(string uciCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UciCode == uciCode);
        }

        /// <summary>
        /// Get all correspondents by conventional name (supports partial matches)
        /// </summary>
        /// <param name="name">The conventional name or part of it</param>
        /// <returns>List of matching correspondents</returns>
        public async Task<IEnumerable<Correspondent>> GetByConventionalNameAsync(string name)
        {
            return await _dbSet
                .Where(c => c.ConventionalName.Contains(name))
                .OrderBy(c => c.ConventionalName)
                .ToListAsync();
        }

        /// <summary>
        /// Get all active correspondents (Type = true)
        /// </summary>
        /// <returns>List of active correspondents</returns>
        public async Task<IEnumerable<Correspondent>> GetActiveCorrespondentsAsync()
        {
            return await _dbSet
                .Where(c => c.Type == true)
                .OrderBy(c => c.ConventionalName)
                .ToListAsync();
        }

        /// <summary>
        /// Get correspondents that have notifications enabled
        /// </summary>
        /// <returns>List of correspondents with notifications enabled</returns>
        public async Task<IEnumerable<Correspondent>> GetNotificationEnabledAsync()
        {
            return await _dbSet
                .Where(c => c.ReceiveNotifications == true &&
                           c.NotificationEmail != null &&
                           c.NotificationEmail != "")
                .OrderBy(c => c.ConventionalName)
                .ToListAsync();
        }

        /// <summary>
        /// Get correspondents by BDS identifier
        /// </summary>
        /// <param name="bdsIdentifier">The BDS identifier</param>
        /// <returns>List of correspondents with the same BDS identifier</returns>
        public async Task<IEnumerable<Correspondent>> GetByBdsIdentifierAsync(string bdsIdentifier)
        {
            return await _dbSet
                .Where(c => c.BdsIdentifier == bdsIdentifier)
                .OrderBy(c => c.UciCode)
                .ToListAsync();
        }

        /// <summary>
        /// Search correspondents by multiple criteria
        /// </summary>
        /// <param name="searchTerm">Search term to match against Code, UciCode, or ConventionalName</param>
        /// <param name="isActive">Filter by active status (null = all)</param>
        /// <param name="hasNotifications">Filter by notification status (null = all)</param>
        /// <returns>List of matching correspondents</returns>
        public async Task<IEnumerable<Correspondent>> SearchAsync(
            string? searchTerm = null,
            bool? isActive = null,
            bool? hasNotifications = null)
        {
            IQueryable<Correspondent> query = _dbSet;

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Code.ToLower().Contains(term) ||
                    c.UciCode.ToLower().Contains(term) ||
                    c.ConventionalName.ToLower().Contains(term) ||
                    c.BdsIdentifier.ToLower().Contains(term));
            }

            // Apply active status filter
            if (isActive.HasValue)
            {
                query = query.Where(c => c.Type == isActive.Value);
            }

            // Apply notification status filter
            if (hasNotifications.HasValue)
            {
                if (hasNotifications.Value)
                {
                    query = query.Where(c => c.ReceiveNotifications == true &&
                                           c.NotificationEmail != null &&
                                           c.NotificationEmail != "");
                }
                else
                {
                    query = query.Where(c => c.ReceiveNotifications == false ||
                                           c.NotificationEmail == null ||
                                           c.NotificationEmail == "");
                }
            }

            return await query
                .OrderBy(c => c.ConventionalName)
                .ThenBy(c => c.UciCode)
                .ToListAsync();
        }

      
        /// <summary>
        /// Update notification settings for a correspondent
        /// </summary>
        /// <param name="correspondentId">The correspondent ID</param>
        /// <param name="receiveNotifications">Whether to receive notifications</param>
        /// <param name="notificationEmail">The notification email</param>
        /// <returns>True if updated successfully</returns>
        public async Task<bool> UpdateNotificationSettingsAsync(
            Guid correspondentId,
            bool receiveNotifications,
            string? notificationEmail)
        {
            var correspondent = await _dbSet.FindAsync(correspondentId);
            if (correspondent == null)
                return false;

            correspondent.ReceiveNotifications = receiveNotifications;
            correspondent.NotificationEmail = notificationEmail;

            return true; // Changes will be saved by UnitOfWork
        }
    }
}
