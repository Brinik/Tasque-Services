using TasqueManager.Abstractions.RepositoryAbstractions;
using Microsoft.EntityFrameworkCore;
using TasqueManager.Domain;

namespace TasqueManager.Infrastructure.Repositories
{
    public abstract class Repository<T, TPrimaryKey> : IRepository<T, TPrimaryKey> where T
        : class, IEntity<TPrimaryKey>
    {
        protected readonly DbContext Context;
        private readonly DbSet<T> _entitySet;

        protected Repository(DbContext context)
        {
            Context = context;
            _entitySet = Context.Set<T>();
        }

        #region Get
        public virtual T Get(TPrimaryKey id)
        {
            var entity = _entitySet.Find(id) ?? throw new KeyNotFoundException(nameof(id));
            return entity;
        }

        public virtual async Task<T> GetAsync(TPrimaryKey id, CancellationToken cancellationToken)
        {
            var entity = await _entitySet.FindAsync(id, cancellationToken) ?? throw new KeyNotFoundException(nameof(id));
            return entity;
        }

        #endregion

        #region GetAll
        public virtual IQueryable<T> GetAll(
            Func<T, bool>? predicate = null,
            bool asNoTracking = false)
        {
            IQueryable<T> query = (predicate != null) ? _entitySet.Where((x) => predicate(x)) : _entitySet;
            return asNoTracking ? query.AsNoTracking() : query;
        }

        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken, bool asNoTracking = false)
        {
            return await GetAll().ToListAsync(cancellationToken);
        }

        #endregion

        #region Create
        public virtual T Add(T entity)
        {
            var objToReturn = _entitySet.Add(entity);
            return objToReturn.Entity;
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            return (await _entitySet.AddAsync(entity)).Entity;
        }

        #endregion

        #region Update
        public virtual void Update(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }
        #endregion

        #region Delete
        public virtual bool Delete(TPrimaryKey id)
        {
            var obj = _entitySet.Find(id);
            if (obj == null)
            {
                return false;
            }
            _entitySet.Remove(obj);
            return true;
        }
        public virtual bool Delete(T entity)
        {
            if (entity == null)
            {
                return false;
            }
            Context.Entry(entity).State = EntityState.Deleted;
            return true;
        }
        public virtual bool DeleteRange(ICollection<T> entities)
        {
            if (entities == null || entities.Count != 0)
            {
                return false;
            }
            _entitySet.RemoveRange(entities);
            return true;
        }

        #endregion

        #region SaveChanges
        public virtual void SaveChanges()
        {
            Context.SaveChanges();
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        #endregion
    }
}