using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using MediaPortal.Common.Utils.ExtensionMethods;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Specification;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  /// <summary>
  /// Generic repository
  /// </summary>
  public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : ObjectContext
  {
    private IUnitOfWork _unitOfWork;
    private readonly string _connectionStringName;
    private TEntity _objectContext;
    private readonly PluralizationService _pluralizer = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en"));
    private bool _disposed;

    private readonly bool _trackingEnabled = false;

    ~GenericRepository()
    {
      Dispose();
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepository&lt;TEntity&gt;"/> class.
    /// </summary>
    public GenericRepository()
      : this(false)
    { }

    public GenericRepository(bool trackingEnabled)
    {
      _trackingEnabled = trackingEnabled;
      _objectContext = ObjectContextManager.CreateDbContext() as TEntity;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepository&lt;TEntity&gt;"/> class.
    /// </summary>
    /// <param name="objectContext">The object context.</param>
    public GenericRepository(TEntity objectContext)
    {
      if (objectContext == null)
        throw new ArgumentNullException("objectContext");
      _objectContext = objectContext;
    }

    public TEntity GetByKey<TEntity>(object keyValue) where TEntity : class
    {
      EntityKey key = GetEntityKey<TEntity>(keyValue);

      object originalItem;
      if (ObjectContext.TryGetObjectByKey(key, out originalItem))
      {
        return (TEntity)originalItem;
      }
      return default(TEntity);
    }

    public IQueryable<TEntity> GetQuery<TEntity>() where TEntity : class
    {
      var entityName = GetEntityName<TEntity>();
      if (_trackingEnabled)
      {
        return ObjectContext.CreateQuery<TEntity>(entityName);
      }
      return ObjectContext.CreateQuery<TEntity>(entityName).AsNoTracking();
    }

    public IQueryable<TEntity> GetQuery<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
      return GetQuery<TEntity>().Where(predicate);
    }

    public IQueryable<TEntity> GetQuery<TEntity>(ISpecification<TEntity> specification) where TEntity : class
    {
      return specification.SatisfyingEntitiesFrom(GetQuery<TEntity>());
    }

    public IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, string>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
    {
      if (sortOrder == SortOrder.Ascending)
      {
        return GetQuery<TEntity>().OrderBy(orderBy).Skip(pageIndex).Take(pageSize);//.AsEnumerable();
      }
      return GetQuery<TEntity>().OrderByDescending(orderBy).Skip(pageIndex).Take(pageSize);//.AsEnumerable();
    }

    public IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, string>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
    {
      if (sortOrder == SortOrder.Ascending)
      {
        return GetQuery<TEntity>().Where(predicate).OrderBy(orderBy).Skip(pageIndex).Take(pageSize);//.AsEnumerable();
      }
      return GetQuery<TEntity>().Where(predicate).OrderByDescending(orderBy).Skip(pageIndex).Take(pageSize);//.AsEnumerable();
    }

    public IQueryable<TEntity> Get<TEntity>(ISpecification<TEntity> specification, Expression<Func<TEntity, string>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
    {
      if (sortOrder == SortOrder.Ascending)
      {
        return specification.SatisfyingEntitiesFrom(GetQuery<TEntity>()).OrderBy(orderBy).Skip(pageIndex).Take(pageSize);//.AsEnumerable();
      }
      return specification.SatisfyingEntitiesFrom(GetQuery<TEntity>()).OrderByDescending(orderBy).Skip(pageIndex).Take(pageSize);//.AsEnumerable();
    }

    public TEntity Single<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
    {
      return GetQuery<TEntity>().SingleOrDefault<TEntity>(criteria);
    }

    public TEntity Single<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
    {
      return criteria.SatisfyingEntityFrom(GetQuery<TEntity>());
    }

    public TEntity First<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
      return GetQuery<TEntity>().FirstOrDefault(predicate);
    }

    public TEntity First<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
    {
      return criteria.SatisfyingEntitiesFrom(GetQuery<TEntity>()).FirstOrDefault();
    }

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
      if (entity == null)
      {
        throw new ArgumentNullException("entity");
      }
      ObjectContext.AddObject(GetEntityName<TEntity>(), entity);
    }

    public void AddList<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
      foreach (var entity in entities)
      {
        Add(entity);
      }
    }

    public void Attach<TEntity>(TEntity entity) where TEntity : class
    {
      if (entity == null)
      {
        throw new ArgumentNullException("entity");
      }

      ObjectContext.AttachTo(GetEntityName<TEntity>(), entity);
    }

    public void ApplyChanges<TEntity>(ObjectSet<TEntity> objectSet, TEntity entity) where TEntity : class, IObjectWithChangeTracker
    {
      if (entity == null)
      {
        throw new ArgumentNullException("entity");
      }
      objectSet.ApplyChanges(entity);
    }

    public void ApplyChanges<TEntity>(ObjectSet<TEntity> objectSet, IEnumerable<TEntity> entities) where TEntity : class, IObjectWithChangeTracker
    {
      foreach (var entity in entities)
      {
        ApplyChanges(objectSet, entity);
      }
    }

    public void ApplyChanges<TEntity>(string entitySetName, TEntity entity) where TEntity : class, IObjectWithChangeTracker
    {
      if (entity == null)
      {
        throw new ArgumentNullException("entity");
      }
      ObjectContext.ApplyChanges(entitySetName, entity);
    }

    private string GetEntitySetFullName<TEntity>(TEntity entity) where TEntity : class
    {
      // If the EntityKey exists, simply get the Entity Set name from the key
      /*if (entity.EntityKey != null)
      {
         return entity.EntityKey.EntitySetName;
      }
      else*/
      {
        string entityTypeName = entity.GetType().Name;
        var container = ObjectContext.MetadataWorkspace.GetEntityContainer(ObjectContext.DefaultContainerName, DataSpace.CSpace);
        string entitySetName = (from meta in container.BaseEntitySets
                                where meta.ElementType.Name == entityTypeName
                                select meta.Name).First();

        return entitySetName;
      }
    }

    private bool IsAttached<TEntity>(string entitySetFullName, TEntity entity) where TEntity : class
    {
      EntityKey key = ObjectContext.CreateEntityKey(entitySetFullName, entity);
      if (key == null)
      {
        throw new ArgumentNullException("key");
      }
      ObjectStateEntry entry;
      if (ObjectContext.ObjectStateManager.TryGetObjectStateEntry(key, out entry))
      {
        return (entry.State != EntityState.Detached);
      }
      return false;
    }

    public void Delete<TEntity>(TEntity entity) where TEntity : class
    {
      if (entity == null)
      {
        throw new ArgumentNullException("entity");
      }

      string entitySetFullName = GetEntitySetFullName(entity);
      if (!IsAttached(entitySetFullName, entity))
      {
        ObjectContext.AttachTo(entitySetFullName, entity);
      }

      //IObjectWithChangeTracker e = entity as IObjectWithChangeTracker;
      //e.ChangeTracker.State = ObjectState.Deleted;
      ObjectContext.DeleteObject(entity);
    }

    public void DeleteList<TEntity>(IList<TEntity> entities) where TEntity : class
    {
      for (int i = entities.Count(); i-- > 0; )
      {
        TEntity entity = entities[i];
        Delete(entity);
      }
    }

    public void Delete<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
    {
      IEnumerable<TEntity> records = Find(criteria);

      foreach (TEntity record in records)
      {
        Delete(record);
      }
    }

    public void Delete<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
    {
      IEnumerable<TEntity> records = Find(criteria);
      foreach (TEntity record in records)
      {
        Delete(record);
      }
    }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
    {
      return GetQuery<TEntity>();
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
      var fqen = GetEntityName<TEntity>();

      object originalItem;
      EntityKey key = ObjectContext.CreateEntityKey(fqen, entity);
      if (ObjectContext.TryGetObjectByKey(key, out originalItem))
      {
        ObjectContext.ApplyCurrentValues(key.EntitySetName, entity);
      }
    }

    public void UpdateList<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
      foreach (var entity in entities)
      {
        Update(entity);
      }
    }

    public IQueryable<TEntity> Find<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
    {
      return GetQuery<TEntity>().Where(criteria);
    }

    public TEntity FindOne<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
    {
      return GetQuery<TEntity>().Where(criteria).FirstOrDefault();
    }

    public TEntity FindOne<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
    {
      return criteria.SatisfyingEntityFrom(GetQuery<TEntity>());
    }

    public IQueryable<TEntity> Find<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
    {
      return criteria.SatisfyingEntitiesFrom(GetQuery<TEntity>());
    }

    public int Count<TEntity>() where TEntity : class
    {
      return GetQuery<TEntity>().Count();
    }

    public int Count<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
    {
      return GetQuery<TEntity>().Count(criteria);
    }

    public int Count<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
    {
      return criteria.SatisfyingEntitiesFrom(GetQuery<TEntity>()).Count();
    }

    public IUnitOfWork UnitOfWork
    {
      get { return _unitOfWork ?? (_unitOfWork = new UnitOfWork(ObjectContext)); }
    }

    public TEntity ObjectContext
    {
      get { return _objectContext ?? (_objectContext = ObjectContextManager.CreateDbContext() as TEntity); }
    }

    private EntityKey GetEntityKey<TEntity>(object keyValue) where TEntity : class
    {
      var entitySetName = GetEntityName<TEntity>();
      var objectSet = ObjectContext.CreateObjectSet<TEntity>();
      var keyPropertyName = objectSet.EntitySet.ElementType.KeyMembers[0].ToString();
      var entityKey = new EntityKey(entitySetName, new[] { new EntityKeyMember(keyPropertyName, keyValue) });
      return entityKey;
    }

    private string GetEntityName<TEntity>() where TEntity : class
    {
      return string.Format("{0}.{1}", ObjectContext.DefaultContainerName, _pluralizer.Pluralize(typeof(TEntity).Name));
    }

    public Expression<Func<TElement, bool>> BuildContainsExpression<TElement, TValue>(
      Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values)
    {
      if (null == valueSelector) { throw new ArgumentNullException("valueSelector"); }
      if (null == values) { throw new ArgumentNullException("values"); }
      ParameterExpression p = valueSelector.Parameters.Single();
      // p => valueSelector(p) == values[0] || valueSelector(p) == ...
      if (!values.Any())
      {
        return e => false;
      }
      var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));
      var body = @equals.Aggregate(Expression.Or);
      return Expression.Lambda<Func<TElement, bool>>(body, p);
    }

    private bool Exists<TEntity>(TEntity entity) where TEntity : class
    {
      var objSet = ObjectContext.CreateObjectSet<TEntity>();
      var entityKey = ObjectContext.CreateEntityKey(objSet.EntitySet.Name, entity);

      Object foundEntity;
      var exists = ObjectContext.TryGetObjectByKey(entityKey, out foundEntity);
      // TryGetObjectByKey attaches a found entity
      return (exists);
    }

    public void AttachEntityIfChangeTrackingDisabled<TEntity>(ObjectSet<TEntity> objectSet, TEntity entity) where TEntity : class, IObjectWithChangeTracker
    {
      if (entity.ChangeTracker.State == ObjectState.Modified || entity.ChangeTracker.State == ObjectState.Unchanged)
      {
        if (!entity.ChangeTracker.ChangeTrackingEnabled)
        {
          //if (Exists(entity))
          //{
          // Detach it here to prevent side-effects
          //ObjectContext.Detach(entity);
          //}
          objectSet.Attach(entity);
          ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
          ObjectContext.Refresh(RefreshMode.ClientWins, entity);
        }
      }
    }

    public void AttachEntityIfChangeTrackingDisabled<TEntity>(ObjectSet<TEntity> objectSet, IEnumerable<TEntity> entities) where TEntity : class, IObjectWithChangeTracker
    {
      foreach (var entity in entities)
      {
        AttachEntityIfChangeTrackingDisabled(objectSet, entity);
      }
    }

    public void Dispose()
    {
      if (!_disposed && _objectContext != null)
      {
        try
        {
          ObjectContext.SafeDispose();
          _unitOfWork.SafeDispose();
        }
        finally
        {
          _disposed = true;
        }
      }
    }
  }
}
