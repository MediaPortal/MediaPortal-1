using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public enum SortOrder { Ascending, Descending }

  public interface IRepository<T> : IDisposable where T : ObjectContext, IDisposable
  {
    /// <summary>
    /// Gets entity by key.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="keyValue">The key value.</param>
    /// <returns></returns>
    TEntity GetByKey<TEntity>(object keyValue) where TEntity : class;

    /// <summary>
    /// Gets the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    IQueryable<TEntity> GetQuery<TEntity>() where TEntity : class;

    /// <summary>
    /// Gets the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    IQueryable<TEntity> GetQuery<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;

    /// <summary>
    /// Gets one entity based on matching criteria
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    TEntity Single<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;

    /// <summary>
    /// Firsts the specified predicate.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    TEntity First<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;

    /// <summary>
    /// Adds the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity.</param>
    void Add<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Attaches the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity.</param>
    void Attach<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Deletes the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity.</param>
    void Delete<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Deletes one or many entities matching the specified criteria
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="criteria">The criteria.</param>
    void Delete<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;

    /// <summary>
    /// Updates changes of the existing entity. 
    /// The caller must later call SaveChanges() on the repository explicitly to save the entity to database
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity.</param>
    void Update<TEntity>(TEntity entity) where TEntity : class;

    void UpdateList<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;    

    /// <summary>
    /// Finds entities based on provided criteria.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    IQueryable<TEntity> Find<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;

    /// <summary>
    /// Finds one entity based on provided criteria.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    TEntity FindOne<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;

    /// <summary>
    /// Gets all.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

    /// <summary>
    /// Gets a collection of entity with paging support
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="orderBy">The order by.</param>
    /// <param name="pageIndex">Index of the page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="sortOrder">The sort order.</param>
    /// <returns></returns>
    IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, string>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class;

    /// <summary>
    /// Gets a collection of entity base on criteria with paging support
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="criteria">The criteria.</param>
    /// <param name="orderBy">The order by.</param>
    /// <param name="pageIndex">Index of the page.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="sortOrder">The sort order.</param>
    /// <returns></returns>
    IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, string>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class;

    /// <summary>
    /// Counts the specified entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    int Count<TEntity>() where TEntity : class;

    /// <summary>
    /// Counts entities with the specified criteria.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    int Count<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;

    /// <summary>
    /// Gets the unit of work.
    /// </summary>
    /// <value>The unit of work.</value>
    IUnitOfWork UnitOfWork { get; }

    T ObjectContext { get; }

    void AddList<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    void DeleteList<TEntity>(IList<TEntity> entities) where TEntity : class;

    Expression<Func<TElement, bool>> BuildContainsExpression<TElement, TValue>(
    Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values);

    void ApplyChanges<TEntity>(ObjectSet<TEntity> objectSet, TEntity entity) where TEntity : class, IObjectWithChangeTracker;
    void ApplyChanges<TEntity>(string entitySetName, TEntity entity) where TEntity : class, IObjectWithChangeTracker;
    void ApplyChanges<TEntity>(ObjectSet<TEntity> entitySetName, IEnumerable<TEntity> entities) where TEntity : class, IObjectWithChangeTracker;
    void AttachEntityIfChangeTrackingDisabled<TEntity>(ObjectSet<TEntity> objectSet, TEntity entity) where TEntity : class, IObjectWithChangeTracker;
    void AttachEntityIfChangeTrackingDisabled<TEntity>(ObjectSet<TEntity> objectSet, IEnumerable<TEntity> entities) where TEntity : class, IObjectWithChangeTracker;
  }    
}