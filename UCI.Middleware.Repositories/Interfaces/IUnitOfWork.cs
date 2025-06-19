using UCI.Middleware.Entities.Entities.Ivass;

namespace UCI.Middleware.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern to manage transactions across multiple repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IClaimsSubmissionRepository ClaimsSubmissions { get; }
    ICorrespondentRepository Correspondents { get; }
    IRepository<SubmissionStatus> SubmissionStatuses { get; }
    IRepository<ErrorType> ErrorTypes { get; }
    IRepository<FlowErrorResponse> FlowErrors { get; }
    IRepository<ClaimErrorResponse> ClaimErrors { get; }
    IRepository<ClaimDetailErrorResponse> ClaimDetailErrors { get; }

    // Transaction management
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}