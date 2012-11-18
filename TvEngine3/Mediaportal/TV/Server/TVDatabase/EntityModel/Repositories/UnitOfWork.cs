using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Objects;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  internal class UnitOfWork : IUnitOfWork
  {

    private static bool? _entityFrameworkEnableVerbosePersistenceLogging;

    private readonly ObjectContext _objectContext;
    private DbTransaction _transaction;

    public UnitOfWork(ObjectContext context)
    {
      if (!_entityFrameworkEnableVerbosePersistenceLogging.HasValue)
      {
        bool entityFrameworkEnableVerbosePersistenceLogging;
        bool parsed = Boolean.TryParse(ConfigurationManager.AppSettings["EntityFrameworkEnableVerbosePersistenceLogging"], out entityFrameworkEnableVerbosePersistenceLogging);
        if (parsed)
        {
          _entityFrameworkEnableVerbosePersistenceLogging = entityFrameworkEnableVerbosePersistenceLogging;
          this.LogDebug("EntityFrameworkEnableVerbosePersistenceLogging={0}", _entityFrameworkEnableVerbosePersistenceLogging);
        }
      }
      _objectContext = context;
    }

    public bool IsInTransaction
    {
      get { return _transaction != null; }
    }

    public void BeginTransaction()
    {
      BeginTransaction(IsolationLevel.ReadCommitted);
    }

    public void BeginTransaction(IsolationLevel isolationLevel)
    {
      if (IsInTransaction)
      {
        throw new ApplicationException("Cannot begin a new transaction while an existing transaction is still running. " +
                                        "Please commit or rollback the existing transaction before starting a new one.");
      }
      OpenConnection();
      _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
    }

    public void RollBackTransaction()
    {
      if (!IsInTransaction)
      {
        throw new ApplicationException("Cannot roll back a transaction while there is no transaction running.");
      }

      try
      {
        _transaction.Rollback();
      }
      catch
      {
        throw;
      }
      finally
      {
        ReleaseCurrentTransaction();
      }
    }

    public void CommitTransaction()
    {
      if (!IsInTransaction)
      {
        throw new ApplicationException("Cannot roll back a transaction while there is no transaction running.");
      }

      try
      {
        _objectContext.SaveChanges();
        _transaction.Commit();
      }
      catch
      {
        _transaction.Rollback();
        throw;
      }
      finally
      {
        ReleaseCurrentTransaction();
      }
    }

    public void SaveChanges()
    {
      if (IsInTransaction)
      {
        throw new ApplicationException("A transaction is running. Call BeginTransaction instead.");
      }

      if (_entityFrameworkEnableVerbosePersistenceLogging.HasValue && _entityFrameworkEnableVerbosePersistenceLogging.Value)
      {
        string traceString = _objectContext.ToTraceString();
        this.LogDebug("EF SaveChanges SQL = {0}", traceString); 
      }      

      _objectContext.SaveChanges();
    }

    public void SaveChanges(SaveOptions saveOptions)
    {
      if (IsInTransaction)
      {
        throw new ApplicationException("A transaction is running. Call BeginTransaction instead.");
      }
      _objectContext.SaveChanges(saveOptions);
    }

  

    /// <summary>
    /// Releases the current transaction
    /// </summary>
    private void ReleaseCurrentTransaction()
    {
      if (_transaction != null)
      {
        _transaction.Dispose();
        _transaction = null;
      }
    }

    private void OpenConnection()
    {
      if (_objectContext.Connection.State != ConnectionState.Open)
      {
        _objectContext.Connection.Open();
      }
    }



    #region Implementation of IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;

      if (_disposed)
        return;

      ReleaseCurrentTransaction();

      _disposed = true;
    }
    private bool _disposed;
    #endregion
  }
}