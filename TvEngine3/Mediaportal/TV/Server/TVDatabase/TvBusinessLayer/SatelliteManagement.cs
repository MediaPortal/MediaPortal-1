using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class SatelliteManagement
  {
    public static IList<Satellite> ListAllSatellites()
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        return tunerRepository.GetAll<Satellite>().OrderBy(s => s.Longitude).ToList();
      }
    }

    public static IList<Satellite> ListAllReferencedSatellites()
    {
      IList<int> tuningDetailSatelliteIds;
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        tuningDetailSatelliteIds = tuningDetailRepository.GetQuery<TuningDetail>().Select(td => td.IdSatellite).Distinct().Where(id => id.HasValue).Select(id => id.Value).ToList();
      }
      IList<int> tunerSatelliteSatelliteIds;
      using (ITunerSatelliteRepository tunerSatelliteRepository = new TunerSatelliteRepository())
      {
        tunerSatelliteSatelliteIds = tunerSatelliteRepository.GetQuery<TunerSatellite>().Select(td => td.IdSatellite).Distinct().ToList();
      }
      HashSet<int> referencedSatelliteIds = new HashSet<int>(tunerSatelliteSatelliteIds);
      referencedSatelliteIds.UnionWith(tuningDetailSatelliteIds);
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        return tunerRepository.GetQuery<Satellite>().Where(s => referencedSatelliteIds.Contains(s.IdSatellite)).ToList();
      }
    }

    public static Satellite GetSatellite(int idSatellite)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        return tunerRepository.GetQuery<Satellite>(s => s.IdSatellite == idSatellite).FirstOrDefault();
      }
    }

    public static Satellite GetSatelliteByLongitude(int longitude)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        return tunerRepository.GetQuery<Satellite>(s => s.Longitude == longitude).FirstOrDefault();
      }
    }

    public static Satellite SaveSatellite(Satellite satellite)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        tunerRepository.AttachEntityIfChangeTrackingDisabled(tunerRepository.ObjectContext.Satellites, satellite);
        tunerRepository.ApplyChanges(tunerRepository.ObjectContext.Satellites, satellite);
        tunerRepository.UnitOfWork.SaveChanges();
        satellite.AcceptChanges();
        return satellite;
      }
    }

    public static IList<Satellite> SaveSatellites(IEnumerable<Satellite> satellites)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        tunerRepository.AttachEntityIfChangeTrackingDisabled(tunerRepository.ObjectContext.Satellites, satellites);
        tunerRepository.ApplyChanges(tunerRepository.ObjectContext.Satellites, satellites);
        tunerRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //tunerRepository.ObjectContext.AcceptAllChanges();
        foreach (Satellite satellite in satellites)
        {
          satellite.AcceptChanges();
        }
        return satellites.ToList();
      }
    }

    public static void DeleteSatellite(int idSatellite)
    {
      using (ITunerRepository tunerRepository = new TunerRepository(true))
      {
        tunerRepository.Delete<Satellite>(s => s.IdSatellite == idSatellite);
        tunerRepository.UnitOfWork.SaveChanges();
      }
    }
  }
}