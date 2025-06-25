using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using UCI.Middleware.Integrations.Database.Interfaces;
using UCI.Middleware.Repositories.Interfaces;

namespace UCI.Middleware.Integrations.Database.Implementation
{
    /// <summary>
    /// Base service implementation providing common business operations with logging and validation.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TKey">The entity key type</typeparam>
    public abstract class BaseService<T, TKey> : IBaseService<T, TKey> where T : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger<BaseService<T, TKey>> _logger;

        protected BaseService(IUnitOfWork unitOfWork, ILogger<BaseService<T, TKey>> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the repository for the entity type T.
        /// Must be implemented by derived classes to return the appropriate repository.
        /// </summary>
        protected abstract IRepository<T> Repository { get; }

        public virtual async Task<T?> GetByIdAsync(TKey id)
        {
            try
            {
                _logger.LogDebug("Getting {EntityType} with ID {Id}", typeof(T).Name, id);
                var result = await Repository.GetByIdAsync(id);

                if (result != null)
                    _logger.LogDebug("Found {EntityType} with ID {Id}", typeof(T).Name, id);
                else
                    _logger.LogDebug("{EntityType} with ID {Id} not found", typeof(T).Name, id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Getting all {EntityType} entities", typeof(T).Name);
                var result = await Repository.GetAllAsync();
                _logger.LogInformation("Retrieved {Count} {EntityType} entities", result.Count(), typeof(T).Name);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Finding {EntityType} entities with predicate", typeof(T).Name);
                var result = await Repository.FindAsync(predicate);
                _logger.LogInformation("Found {Count} {EntityType} entities matching predicate", result.Count(), typeof(T).Name);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding {EntityType} entities with predicate", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                _logger.LogDebug("Creating new {EntityType}", typeof(T).Name);

                // Business validation
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    _logger.LogWarning("Validation failed for {EntityType}: {Errors}", typeof(T).Name, errors);
                    throw new ValidationException($"Validation failed: {errors}");
                }

                await _unitOfWork.BeginTransactionAsync();

                var result = await Repository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created {EntityType}", typeof(T).Name);
                return result;
            }
            catch (ValidationException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {EntityType}", typeof(T).Name);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                var entityList = entities.ToList();
                _logger.LogDebug("Creating {Count} new {EntityType} entities", entityList.Count, typeof(T).Name);

                // Validate all entities
                var validationTasks = entityList.Select(ValidateAsync);
                var validationResults = await Task.WhenAll(validationTasks);

                var invalidResults = validationResults.Where(r => !r.IsValid).ToList();
                if (invalidResults.Any())
                {
                    var allErrors = invalidResults.SelectMany(r => r.Errors);
                    var errors = string.Join(", ", allErrors);
                    _logger.LogWarning("Validation failed for {Count} {EntityType} entities: {Errors}",
                        invalidResults.Count, typeof(T).Name, errors);
                    throw new ValidationException($"Validation failed: {errors}");
                }

                await _unitOfWork.BeginTransactionAsync();

                var result = await Repository.AddRangeAsync(entityList);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
                return result;
            }
            catch (ValidationException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {Count} {EntityType} entities", entities.Count(), typeof(T).Name);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                _logger.LogDebug("Updating {EntityType}", typeof(T).Name);

                // Business validation
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    _logger.LogWarning("Validation failed for {EntityType}: {Errors}", typeof(T).Name, errors);
                    throw new ValidationException($"Validation failed: {errors}");
                }

                await _unitOfWork.BeginTransactionAsync();

                var result = await Repository.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated {EntityType}", typeof(T).Name);
                return result;
            }
            catch (ValidationException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType}", typeof(T).Name);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
                _logger.LogDebug("Deleting {EntityType}", typeof(T).Name);

                await _unitOfWork.BeginTransactionAsync();

                await Repository.DeleteAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deleted {EntityType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType}", typeof(T).Name);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            try
            {
                _logger.LogDebug("Deleting {EntityType} with ID {Id}", typeof(T).Name, id);

                var entity = await Repository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("{EntityType} with ID {Id} not found for deletion", typeof(T).Name, id);
                    throw new EntityNotFoundException($"{typeof(T).Name} with ID {id} not found");
                }

                await DeleteAsync(entity);
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(TKey id)
        {
            try
            {
                _logger.LogDebug("Checking if {EntityType} with ID {Id} exists", typeof(T).Name, id);
                var exists = await Repository.ExistsAsync(id);
                _logger.LogDebug("{EntityType} with ID {Id} exists: {Exists}", typeof(T).Name, id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                _logger.LogDebug("Counting all {EntityType} entities", typeof(T).Name);
                var count = await Repository.CountAsync();
                _logger.LogDebug("Count of {EntityType} entities: {Count}", typeof(T).Name, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting {EntityType} entities", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Counting {EntityType} entities with predicate", typeof(T).Name);
                var count = await Repository.CountAsync(predicate);
                _logger.LogDebug("Count of {EntityType} entities with predicate: {Count}", typeof(T).Name, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting {EntityType} entities with predicate", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Default validation implementation. Override in derived classes for specific business rules.
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <returns>Validation result</returns>
        public virtual async Task<ValidationResult> ValidateAsync(T entity)
        {
            await Task.CompletedTask; // Allow for async validation in derived classes

            if (entity == null)
            {
                return ValidationResult.Failure("Entity cannot be null");
            }

            return ValidationResult.Success();
        }
    }

    /// <summary>
    /// Exception thrown when validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an entity is not found.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message) { }
        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}