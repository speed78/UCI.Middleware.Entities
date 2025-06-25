using System.Linq.Expressions;

namespace UCI.Middleware.Integrations.Database.Interfaces
{
    /// <summary>
    /// Base service interface providing common business operations for entities.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TKey">The entity key type</typeparam>
    public interface IBaseService<T, TKey> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(TKey id);

        /// <summary>
        /// Gets all entities of type T.
        /// </summary>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Finds entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The search predicate</param>
        /// <returns>Collection of matching entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Creates a new entity with business validation.
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Creates multiple entities with business validation.
        /// </summary>
        /// <param name="entities">The entities to create</param>
        /// <returns>The created entities</returns>
        Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates an existing entity with business validation.
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Deletes an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        Task DeleteAsync(TKey id);

        /// <summary>
        /// Checks if an entity exists by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsAsync(TKey id);

        /// <summary>
        /// Gets the total count of entities.
        /// </summary>
        /// <returns>The total count</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Gets the count of entities matching the predicate.
        /// </summary>
        /// <param name="predicate">The search predicate</param>
        /// <returns>The count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Validates an entity before create/update operations.
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <returns>Validation result with any errors</returns>
        Task<ValidationResult> ValidateAsync(T entity);
    }

    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Failure(params string[] errors) => new()
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }
}