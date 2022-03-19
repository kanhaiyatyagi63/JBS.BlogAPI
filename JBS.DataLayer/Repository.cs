
using JBS.DataLayer.Abstracts;
using JBS.DataLayer.Entities.Base;
using JBS.Model.JqDataTableModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace JBS.DataLayer;
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IBaseEntity<TKey>
{
    private readonly DataContext _dataContext;

    public Repository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    private DbSet<TEntity> DbSet()
    {
        return _dataContext.Set<TEntity>();
    }

    public IQueryable<TEntity> Entity(bool trackEntity = false)
    {
        return trackEntity ? DbSet() : DbSet().AsNoTracking();
    }

    public IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate, params string[] includes)
    {
        var dbSet = DbSet().Where(predicate).AsQueryable();

        if (includes == null || !includes.Any()) return dbSet;

        return includes.Aggregate(dbSet, (current, include) => current.Include(include));
    }

    public async Task<JqDataTableResponse<TEntity>> GetDataTableResponse(IQueryable<TEntity> query, JqDataTableRequest model)
    {
        var recordsCount = await query.CountAsync();
        var pagedResult = new JqDataTableResponse<TEntity>
        {
            RecordsTotal = recordsCount,
            RecordsFiltered = recordsCount,
            Data = await query.OrderBy(model.GetSortExpression())
                .Skip(model.Start)
                .Take(model.Length)
                .ToListAsync()
        };

        return pagedResult;
    }

    public async Task<JqDataTableResponse<T>> GetDataTableResponse<T>(IQueryable<T> query, JqDataTableRequest model) where T : class
    {
        var recordsCount = await query.CountAsync();
        var pagedResult = new JqDataTableResponse<T>
        {
            RecordsTotal = recordsCount,
            RecordsFiltered = recordsCount,
            PageLength = model.Length,
            Data = await query.OrderBy(model.GetSortExpression())
                .Skip(model.Start)
                .Take(model.Length)
                .ToListAsync()
        };

        return pagedResult;
    }

    public async Task<TEntity> GetAsync(TKey id)
    {
        return await DbSet().FirstOrDefaultAsync(x => x.Equals(id));
    }

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet().FirstOrDefaultAsync(predicate);
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return DbSet().AsQueryable<TEntity>();
    }

    public IEnumerable<TEntity> GetList()
    {
        return DbSet().AsEnumerable<TEntity>();
    }
    public IEnumerable<TEntity> GetListWithAsNoTracking()
    {
        return DbSet().AsNoTracking().AsEnumerable<TEntity>();
    }
    public IEnumerable<TEntity> GetListWithAsNoTracking(Expression<Func<TEntity, bool>> predicate)
    {
        return DbSet().AsNoTracking().Where(predicate);
    }
    public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
    {
        return DbSet().Where(predicate);
    }

    public async Task<IEnumerable<TEntity>> GetListAsync()
    {
        return await DbSet().ToListAsync();
    }
    public async Task<bool> AnyAsync()
    {
        return await DbSet().AnyAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet().AnyAsync(predicate);
    }

    public async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet().Where(predicate).ToListAsync();
    }

    public TEntity Insert(TEntity entity)
    {
        DbSet().Add(entity);

        return entity;
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        await DbSet().AddAsync(entity);

        return entity;
    }

    public IEnumerable<TEntity> InsertMany(ICollection<TEntity> entities)
    {
        DbSet().AddRange(entities);

        return entities;
    }

    public async Task<IEnumerable<TEntity>> InsertManyAsync(ICollection<TEntity> entities)
    {
        await DbSet().AddRangeAsync(entities);

        return entities;
    }


    public void Delete(TKey id)
    {
        TEntity entity = _dataContext.Set<TEntity>().FirstOrDefault(x => x.Id.Equals(id));
        if (entity != null)
        {
            Delete(entity);
        }
    }

    public void Delete(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException("Entity");

        entity.IsDeleted = true;
        _dataContext.Entry(entity).State = EntityState.Modified;
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Delete(entity);
        }
    }
    public void DeleteRange(IEnumerable<TKey> ids)
    {
        foreach (var id in ids)
        {
            Delete(id);
        }
    }
    public void UnDelete(TKey id)
    {
        TEntity entity = _dataContext.Set<TEntity>().FirstOrDefault(x => x.Id.Equals(id));
        if (entity != null)
        {
            UnDelete(entity);
        }
    }

    public void UnDelete(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException("Entity");

        entity.IsDeleted = false;
        _dataContext.Entry(entity).State = EntityState.Modified;
    }
    public void Update(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException("Entity");

        _dataContext.Entry(entity).State = EntityState.Modified;
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Update(entity);
        }
    }

    public void Remove(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException("Entity");

        DbSet().Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        DbSet().RemoveRange(entities);
    }

}

