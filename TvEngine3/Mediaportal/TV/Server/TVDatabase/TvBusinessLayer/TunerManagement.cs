using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class TunerManagement
  {
    public static IList<Tuner> ListAllTuners(TunerRelation includeRelations)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        IQueryable<Tuner> query = tunerRepository.GetAll<Tuner>().OrderBy(t => t.Priority);
        query = tunerRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static Tuner GetTuner(int idTuner, TunerRelation includeRelations)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        IQueryable<Tuner> query = tunerRepository.GetQuery<Tuner>(t => t.IdTuner == idTuner);
        query = tunerRepository.IncludeAllRelations(query, includeRelations);
        return query.FirstOrDefault();
      }
    }

    public static Tuner GetTunerByExternalId(string externalId, TunerRelation includeRelations)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        var query = tunerRepository.GetQuery<Tuner>(t => t.ExternalId == externalId);
        query = tunerRepository.IncludeAllRelations(query, includeRelations);
        return query.FirstOrDefault();
      }
    }

    public static Tuner SaveTuner(Tuner tuner)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        tunerRepository.AttachEntityIfChangeTrackingDisabled(tunerRepository.ObjectContext.Tuners, tuner);
        tunerRepository.ApplyChanges(tunerRepository.ObjectContext.Tuners, tuner);
        tunerRepository.UnitOfWork.SaveChanges();
        tuner.AcceptChanges();
        return tuner;
      }
    }

    public static IList<Tuner> SaveTuners(IEnumerable<Tuner> tuners)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        tunerRepository.AttachEntityIfChangeTrackingDisabled(tunerRepository.ObjectContext.Tuners, tuners);
        tunerRepository.ApplyChanges(tunerRepository.ObjectContext.Tuners, tuners);
        tunerRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //tunerRepository.ObjectContext.AcceptAllChanges();
        foreach (Tuner tuner in tuners)
        {
          tuner.AcceptChanges();
        }
        return tuners.ToList();
      }
    }

    public static void DeleteTuner(int idTuner)
    {
      using (ITunerRepository tunerRepository = new TunerRepository(true))
      {
        tunerRepository.Delete<Tuner>(t => t.IdTuner == idTuner);
        tunerRepository.UnitOfWork.SaveChanges();
      }
    }
  }
}