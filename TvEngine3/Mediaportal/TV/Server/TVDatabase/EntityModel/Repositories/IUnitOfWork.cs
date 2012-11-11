using System;
using System.Data;
using System.Data.Objects;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        bool IsInTransaction { get; }
        void SaveChanges();
        void SaveChanges(SaveOptions saveOptions);
        void BeginTransaction();
        void BeginTransaction(IsolationLevel isolationLevel);
        void RollBackTransaction();
        void CommitTransaction();
    }
}
