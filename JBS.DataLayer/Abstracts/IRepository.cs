using JBS.DataLayer.Entities.Base;
using JBS.Model.JqDataTableModels;
using System.Linq.Expressions;

namespace JBS.DataLayer.Abstracts;
public interface IRepository<TEntity, TKey> where TEntity : class, IBaseEntity<TKey>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="trackEntity"></param>
    /// <returns></returns>
    IQueryable<TEntity> Entity(bool trackEntity = false);
    /// <summary>
    /// Gets objects from database by filter.
    /// </summary>
    /// <param name="predicate">Specified a filter</param>
    /// <param name="includes"></param>
    IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate, params string[] includes);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TEntity> GetAsync(TKey id);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> AnyAsync();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    TEntity Insert(TEntity t);
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IQueryable<TEntity> GetQueryable();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<JqDataTableResponse<TEntity>> GetDataTableResponse(IQueryable<TEntity> query, JqDataTableRequest model);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<JqDataTableResponse<T>> GetDataTableResponse<T>(IQueryable<T> query, JqDataTableRequest model) where T : class;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// 
    IEnumerable<TEntity> GetListWithAsNoTracking();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IEnumerable<TEntity> GetListWithAsNoTracking(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerable<TEntity> GetList();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetListAsync();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<TEntity> InsertAsync(TEntity entity);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    IEnumerable<TEntity> InsertMany(ICollection<TEntity> entities);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> InsertManyAsync(ICollection<TEntity> entities);

    /// <summary>
    /// Update object changes and save to database.
    /// </summary>
    /// <param name="t">Specified the object to save.</param>
    void Update(TEntity t);

    /// <summary>
    /// It soft deletion the entity with <see cref="IEntity{TKey}.Id"/> by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.TrueString"/>
    /// </summary>
    /// <param name="id"></param>
    void Delete(TKey id);
    /// <summary>
    /// It soft deletion the entity by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.TrueString"/>
    /// </summary>
    /// <param name="entity"></param>
    void Delete(TEntity entity);
    /// <summary>
    /// It soft deletion the enities by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.TrueString"/>
    /// </summary>
    /// <param name="entities"></param>
    void DeleteRange(IEnumerable<TEntity> entities);
    /// <summary>
    /// It soft deletion the enities by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.TrueString"/>
    /// </summary>
    /// <param name="entities"></param>
    void DeleteRange(IEnumerable<TKey> ids);
    /// <summary>
    /// It soft undeletion the entity with <see cref="IEntity{TKey}.Id"/> by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.FalseString"/>
    /// </summary>
    /// <param name="id"></param>
    void UnDelete(TKey id);
    /// <summary>
    /// It soft deletion the entity by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.FalseString"/>
    /// </summary>
    /// <param name="entity"></param>
    void UnDelete(TEntity entity);
    /// <summary>
    /// It removes the entity permanently, should be used wisely.
    /// 
    /// Review the use case and use <see cref="IRepository{TEntity, TKey}.Delete(TEntity)" /> method.
    /// </summary>
    /// <param name="entity"></param>
    void Remove(TEntity entity);

    /// <summary>
    /// It removes the entity permanently, should be used wisely.
    /// 
    /// Review the use case and use <see cref="IRepository{TEntity, TKey}.DeleteRange(TEntity)" /> method.
    /// </summary>
    /// <param name="entity"></param>
    void RemoveRange(IEnumerable<TEntity> entities);

}
