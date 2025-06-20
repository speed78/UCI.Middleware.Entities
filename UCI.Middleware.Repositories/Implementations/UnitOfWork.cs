using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Repositories.Interfaces;
using UCI.Middleware.Repositories.Interfaces.Specific;
using UCI.Middleware.Repositories.Implementations.Specific;

namespace UCI.Middleware.Repositories.Implementations
{
    /// <summary>
    /// Unit of Work implementation providing coordinated access to repositories
    /// and transaction management with comprehensive logging.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

        // Repository instances - lazy loaded
        private IClaimsSubmissionRepository? _claimsSubmissions;
        private IRepository<ClaimErrorResponse>? _claimErrors;
        private IRepository<ClaimDetailErrorResponse>? _claimDetailErrors;
        private IRepository<FlowErrorResponse>? _flowErrors;
        private IRepository<ErrorType>? _errorTypes;
        private IRepository<Correspondent>? _correspondents;
        private IRepository<SubmissionStatus>? _submissionStatuses;

        public UnitOfWork(DbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IClaimsSubmissionRepository ClaimsSubmissions =>
            _claimsSubmissions ??= new ClaimsSubmissionRepository(_context,
                _loggerFactory.CreateLogger<ClaimsSubmissionRepository>());

        public IRepository<ClaimErrorResponse> ClaimErrors =>
            _claimErrors ??= new Repository<ClaimErrorResponse>(_context,
                _loggerFactory.CreateLogger<Repository<ClaimErrorResponse>>());

        public IRepository<ClaimDetailErrorResponse> ClaimDetailErrors =>
            _claimDetailErrors ??= new Repository<ClaimDetailErrorResponse>(_context,
                _loggerFactory.CreateLogger<Repository<ClaimDetailErrorResponse>>());

        public IRepository<FlowErrorResponse> FlowErrors =>
            _flowErrors ??= new Repository<FlowErrorResponse>(_context,
                _loggerFactory.CreateLogger<Repository<FlowErrorResponse>>());

        public IRepository<ErrorType> ErrorTypes =>
            _errorTypes ??= new Repository<ErrorType>(_context,
                _loggerFactory.CreateLogger<Repository<ErrorType>>());

        public IRepository<Correspondent> Correspondents =>
            _correspondents ??= new Repository<Correspondent>(_context,
                _loggerFactory.CreateLogger<Repository<Correspondent>>());

        public IRepository<SubmissionStatus> SubmissionStatuses =>
            _submissionStatuses ??= new Repository<SubmissionStatus>(_context,
                _loggerFactory.CreateLogger<Repository<SubmissionStatus>>());

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger.LogDebug("Saving changes to database");
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved {ChangesCount} changes to database", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database. Transaction active: {HasTransaction}",
                    _transaction != null);
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    throw new InvalidOperationException("A transaction is already active.");
                }

                _logger.LogDebug("Beginning database transaction");
                _transaction = await _context.Database.BeginTransactionAsync();

                _logger.LogInformation("Database transaction started. Transaction ID: {TransactionId}",
                    _transaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning database transaction");
                throw;
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to commit.");
                }

                _logger.LogDebug("Committing transaction {TransactionId}", _transaction.TransactionId);
                await _transaction.CommitAsync();

                _logger.LogInformation("Transaction {TransactionId} committed successfully",
                    _transaction.TransactionId);
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to rollback.");
                }

                _logger.LogWarning("Rolling back transaction {TransactionId}", _transaction.TransactionId);
                await _transaction.RollbackAsync();

                _logger.LogInformation("Transaction {TransactionId} rolled back successfully",
                    _transaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction {TransactionId}",
                    _transaction?.TransactionId);
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _logger.LogDebug("Disposing UnitOfWork");
                        _transaction?.Dispose();
                        _context?.Dispose();
                        _logger.LogDebug("UnitOfWork disposed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disposing UnitOfWork");
                    }
                }
                _disposed = true;
            }
        }
    }
}