using Microsoft.EntityFrameworkCore.Storage;
using UCI.Middleware.Entities.Context;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Repositories.Interfaces;

namespace UCI.Middleware.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly UciDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    private IClaimsSubmissionRepository? _claimsSubmissions;
    private ICorrespondentRepository? _correspondents;
    private IRepository<SubmissionStatus>? _submissionStatuses;
    private IRepository<ErrorType>? _errorTypes;
    private IRepository<FlowErrorResponse>? _flowErrors;
    private IRepository<ClaimErrorResponse>? _claimErrors;
    private IRepository<ClaimDetailErrorResponse>? _claimDetailErrors;

    public UnitOfWork(UciDbContext context)
    {
        _context = context;
    }

    // Repository properties with lazy initialization
    public IClaimsSubmissionRepository ClaimsSubmissions =>
        _claimsSubmissions ??= new ClaimsSubmissionRepository(_context);

    public ICorrespondentRepository Correspondents =>
        _correspondents ??= new CorrespondentRepository(_context);

    public IRepository<SubmissionStatus> SubmissionStatuses =>
        _submissionStatuses ??= new Repository<SubmissionStatus>(_context);

    public IRepository<ErrorType> ErrorTypes =>
        _errorTypes ??= new Repository<ErrorType>(_context);

    public IRepository<FlowErrorResponse> FlowErrors =>
        _flowErrors ??= new Repository<FlowErrorResponse>(_context);

    public IRepository<ClaimErrorResponse> ClaimErrors =>
        _claimErrors ??= new Repository<ClaimErrorResponse>(_context);

    public IRepository<ClaimDetailErrorResponse> ClaimDetailErrors =>
        _claimDetailErrors ??= new Repository<ClaimDetailErrorResponse>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}