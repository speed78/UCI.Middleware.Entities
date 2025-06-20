using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Repositories.Interfaces.Specific;

namespace UCI.Middleware.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface providing coordinated access to all repositories
    /// and transaction management capabilities.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Repository for ClaimsSubmission entities with specific business operations.
        /// </summary>
        IClaimsSubmissionRepository ClaimsSubmissions { get; }

        /// <summary>
        /// Repository for ClaimErrorResponse entities.
        /// </summary>
        IRepository<ClaimErrorResponse> ClaimErrors { get; }

        /// <summary>
        /// Repository for ClaimDetailErrorResponse entities.
        /// </summary>
        IRepository<ClaimDetailErrorResponse> ClaimDetailErrors { get; }

        /// <summary>
        /// Repository for FlowErrorResponse entities.
        /// </summary>
        IRepository<FlowErrorResponse> FlowErrors { get; }

        /// <summary>
        /// Repository for ErrorType entities.
        /// </summary>
        IRepository<ErrorType> ErrorTypes { get; }

        /// <summary>
        /// Repository for Correspondent entities.
        /// </summary>
        IRepository<Correspondent> Correspondents { get; }

        /// <summary>
        /// Repository for SubmissionStatus entities.
        /// </summary>
        IRepository<SubmissionStatus> SubmissionStatuses { get; }

        /// <summary>
        /// Saves all pending changes to the database.
        /// </summary>
        /// <returns>The number of affected records</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}