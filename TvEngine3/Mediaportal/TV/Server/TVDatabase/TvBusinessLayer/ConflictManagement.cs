using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ConflictManagement
  {

    public static IList<Conflict> ListAllConflicts()
    {
      using (var conflictRepository = new GenericRepository<Model>())
      {
        var listAllConflicts = conflictRepository.GetAll<Conflict>();
        return listAllConflicts.ToList();
      }
    }

    public static Conflict SaveConflict(Conflict conflict)
    {
      using (var conflictRepository = new GenericRepository<Model>())
      {
        conflictRepository.AttachEntityIfChangeTrackingDisabled(conflictRepository.ObjectContext.Conflicts, conflict);
        conflictRepository.ApplyChanges(conflictRepository.ObjectContext.Conflicts, conflict);
        conflictRepository.UnitOfWork.SaveChanges();
        conflict.AcceptChanges();
        return conflict;
      }            
    }

    public static Conflict GetConflict(int idConflict)
    {
      using (var conflictRepository = new GenericRepository<Model>())
      {
        return conflictRepository.Single<Conflict>(s => s.IdConflict == idConflict);
      }
    }

    public static void DeleteConflict(int idConflict)
    {
      using (var conflictRepository = new GenericRepository<Model>(true))
      {
        conflictRepository.Delete<Conflict>(p => p.IdConflict == idConflict);
        conflictRepository.UnitOfWork.SaveChanges();
      }
    }
  }
}
