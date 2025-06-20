using System.Linq.Expressions;

namespace UCI.Middleware.Repositories.Interfaces
{
    /// <summary>
    /// Base repository interface providing common CRUD operations for entities.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(object id);

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
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Adds multiple entities.
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <returns>The added entities</returns>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates an existing entity.
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
        Task DeleteAsync(object id);

        /// <summary>
        /// Checks if an entity exists by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsAsync(object id);

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
    }
}