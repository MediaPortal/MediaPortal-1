using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class TunerSatelliteManagement
  {
    public static IList<TunerSatellite> ListAllTunerSatellites(TunerSatelliteRelation includeRelations)
    {
      using (ITunerSatelliteRepository tunerSatelliteRepository = new TunerSatelliteRepository())
      {
        IQueryable<TunerSatellite> query = tunerSatelliteRepository.GetAll<TunerSatellite>();
        if (includeRelations.HasFlag(TunerSatelliteRelation.Satellite))
        {
          query = query.OrderBy(ts => ts.Satellite.Longitude).ThenBy(ts => ts.IdTuner.GetValueOrDefault(0));
        }
        else
        {
          query = query.OrderBy(ts => ts.IdTuner.GetValueOrDefault(0));
        }
        return tunerSatelliteRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TunerSatellite> ListAllTunerSatellitesByTuner(int idTuner, TunerSatelliteRelation includeRelations)
    {
      using (ITunerSatelliteRepository tunerSatelliteRepository = new TunerSatelliteRepository())
      {
        IQueryable<TunerSatellite> query = tunerSatelliteRepository.GetQuery<TunerSatellite>(ts => ts.IdTuner == null || ts.IdTuner == idTuner);
        if (includeRelations.HasFlag(TunerSatelliteRelation.Satellite))
        {
          query = query.OrderBy(ts => ts.Satellite.Longitude).ThenBy(ts => !ts.IdTuner.HasValue);
        }
        else
        {
          query = query.OrderBy(ts => !ts.IdTuner.HasValue);
        }
        return tunerSatelliteRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static TunerSatellite GetTunerSatellite(int idTunerSatellite, TunerSatelliteRelation includeRelations)
    {
      using (ITunerSatelliteRepository tunerSatelliteRepository = new TunerSatelliteRepository())
      {
        IQueryable<TunerSatellite> query = tunerSatelliteRepository.GetQuery<TunerSatellite>(ts => ts.IdTunerSatellite == idTunerSatellite);
        return tunerSatelliteRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
      }
    }

    public static TunerSatellite SaveTunerSatellite(TunerSatellite TunerSatellite)
    {
      using (ITunerSatelliteRepository tunerSatelliteRepository = new TunerSatelliteRepository())
      {
        tunerSatelliteRepository.AttachEntityIfChangeTrackingDisabled(tunerSatelliteRepository.ObjectContext.TunerSatellites, TunerSatellite);
        tunerSatelliteRepository.ApplyChanges(tunerSatelliteRepository.ObjectContext.TunerSatellites, TunerSatellite);
        tunerSatelliteRepository.UnitOfWork.SaveChanges();
        TunerSatellite.AcceptChanges();
        return TunerSatellite;
      }
    }

    public static IList<TunerSatellite> SaveTunerSatellites(IEnumerable<TunerSatellite> TunerSatellites)
    {
      using (ITunerSatelliteRepository tunerSatelliteRepository = new TunerSatelliteRepository())
      {
        tunerSatelliteRepository.AttachEntityIfChangeTrackingDisabled(tunerSatelliteRepository.ObjectContext.TunerSatellites, TunerSatellites);
        tunerSatelliteRepository.ApplyChanges(tunerSatelliteRepository.ObjectContext.TunerSatellites, TunerSatellites);
        tunerSatelliteRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //tunerSatelliteRepository.ObjectContext.AcceptAllChanges();
        foreach (TunerSatellite TunerSatellite in TunerSatellites)
        {
          TunerSatellite.AcceptChanges();
        }
        return TunerSatellites.ToList();
      }
    }

    public static void DeleteTunerSatellite(int idTunerSatellite)
    {
      using (ITunerSatelliteRepository tunerSatelliteRepository = new TunerSatelliteRepository(true))
      {
        tunerSatelliteRepository.Delete<TunerSatellite>(ts => ts.IdTunerSatellite == idTunerSatellite);
        tunerSatelliteRepository.UnitOfWork.SaveChanges();
      }
    }
  }
}