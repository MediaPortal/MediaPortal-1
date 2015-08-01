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
    #region tuners

    public static IList<Tuner> ListAllTuners()
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        IQueryable<Tuner> query = tunerRepository.GetAll<Tuner>().OrderBy(t => t.Priority);
        query = tunerRepository.IncludeAllRelations(query);
        return query.ToList(); 
      }
    }

    public static IList<Tuner> ListAllTuners(TunerIncludeRelationEnum includeRelations)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        IQueryable<Tuner> query = tunerRepository.GetAll<Tuner>().OrderBy(t => t.Priority);
        query = tunerRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static Tuner GetTuner(int idTuner)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        IQueryable<Tuner> query = tunerRepository.GetQuery<Tuner>(t => t.IdTuner == idTuner);
        query = tunerRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }

    public static Tuner GetTuner(int idTuner, TunerIncludeRelationEnum includeRelations)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        IQueryable<Tuner> query = tunerRepository.GetQuery<Tuner>(t => t.IdTuner == idTuner);
        query = tunerRepository.IncludeAllRelations(query, includeRelations);
        return query.FirstOrDefault();
      }
    }

    public static Tuner GetTunerByExternalId(string externalId)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        var query = tunerRepository.GetQuery<Tuner>(t => t.ExternalId == externalId);
        query = tunerRepository.IncludeAllRelations(query);
        return query.FirstOrDefault(); 
      }
    }

    public static Tuner GetTunerByExternalId(string externalId, TunerIncludeRelationEnum includeRelations)
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

    #endregion

    #region satellites

    public static IList<Satellite> ListAllSatellites()
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        return tunerRepository.GetAll<Satellite>().ToList();
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

    #endregion

    public static DiseqcMotor SaveDiseqcMotor(DiseqcMotor motor)
    {
      using (ITunerRepository tunerRepository = new TunerRepository())
      {
        tunerRepository.AttachEntityIfChangeTrackingDisabled(tunerRepository.ObjectContext.DiseqcMotors, motor);
        tunerRepository.ApplyChanges(tunerRepository.ObjectContext.DiseqcMotors, motor);
        tunerRepository.UnitOfWork.SaveChanges();
        motor.AcceptChanges();
        return motor;
      }
    }

    /// <summary>
    /// Checks if a tuner can tune a specific channel
    /// </summary>
    /// <param name="tuner"></param>
    /// <param name="channelId">Channel id</param>
    /// <returns>true/false</returns>
    public static bool CanTuneChannel(Tuner tuner, int channelId)
    {
      IList<ChannelMap> tunerChannels = tuner.ChannelMaps;
      return !tunerChannels.Any(cmap => channelId == cmap.IdChannel);
    }
  }
}