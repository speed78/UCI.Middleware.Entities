using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using UCI.Middleware.Repositories.Interfaces;

namespace UCI.Middleware.Repositories.Implementations
{
    /// <summary>
    /// Base repository implementation providing common CRUD operations with logging support.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;

        public Repository(DbContext context, ILogger<Repository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            try
            {
                _logger.LogDebug("Getting entity {EntityType} with ID {Id}", typeof(T).Name, id);
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Getting all entities of type {EntityType}", typeof(T).Name);
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Finding entities of type {EntityType} with predicate", typeof(T).Name);
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entities of type {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                _logger.LogDebug("Adding new entity of type {EntityType}", typeof(T).Name);
                var result = await _dbSet.AddAsync(entity);
                _logger.LogInformation("Successfully added entity of type {EntityType}", typeof(T).Name);
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                var entityList = entities.ToList();
                _logger.LogDebug("Adding {Count} entities of type {EntityType}", entityList.Count, typeof(T).Name);
                await _dbSet.AddRangeAsync(entityList);
                _logger.LogInformation("Successfully added {Count} entities of type {EntityType}", entityList.Count, typeof(T).Name);
                return entityList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding range of entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                _logger.LogDebug("Updating entity of type {EntityType}", typeof(T).Name);
                _dbSet.Update(entity);
                _logger.LogInformation("Successfully updated entity of type {EntityType}", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
                _logger.LogDebug("Deleting entity of type {EntityType}", typeof(T).Name);
                _dbSet.Remove(entity);
                _logger.LogInformation("Successfully deleted entity of type {EntityType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task DeleteAsync(object id)
        {
            try
            {
                _logger.LogDebug("Deleting entity {EntityType} with ID {Id}", typeof(T).Name, id);
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    await DeleteAsync(entity);
                }
                else
                {
                    _logger.LogWarning("Entity {EntityType} with ID {Id} not found for deletion", typeof(T).Name, id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(object id)
        {
            try
            {
                _logger.LogDebug("Checking if entity {EntityType} with ID {Id} exists", typeof(T).Name, id);
                var entity = await GetByIdAsync(id);
                var exists = entity != null;
                _logger.LogDebug("Entity {EntityType} with ID {Id} exists: {Exists}", typeof(T).Name, id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of entity {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                _logger.LogDebug("Counting all entities of type {EntityType}", typeof(T).Name);
                var count = await _dbSet.CountAsync();
                _logger.LogDebug("Count of entities {EntityType}: {Count}", typeof(T).Name, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Counting entities of type {EntityType} with predicate", typeof(T).Name);
                var count = await _dbSet.CountAsync(predicate);
                _logger.LogDebug("Count of entities {EntityType} with predicate: {Count}", typeof(T).Name, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }
    }
}