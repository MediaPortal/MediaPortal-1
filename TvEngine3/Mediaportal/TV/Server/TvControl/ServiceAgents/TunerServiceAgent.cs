using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class TunerServiceAgent : ServiceAgent<ITunerService>, ITunerService
  {
    public TunerServiceAgent(string hostname) : base(hostname)
    {
    }

    #region tuners

    public IList<Tuner> ListAllTuners(TunerRelation includeRelations)
    {
      return _channel.ListAllTuners(includeRelations);
    }

    public Tuner GetTuner(int idTuner, TunerRelation includeRelations)
    {
      return _channel.GetTuner(idTuner, includeRelations);
    }

    public Tuner GetTunerByExternalId(string externalId, TunerRelation includeRelations)
    {
      return _channel.GetTunerByExternalId(externalId, includeRelations);
    }

    public Tuner SaveTuner(Tuner tuner)
    {
      tuner.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveTuner(tuner);
    }

    public IList<Tuner> SaveTuners(IEnumerable<Tuner> tuners)
    {
      foreach (Tuner tuner in tuners)
      {
        tuner.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveTuners(tuners);
    }

    public void DeleteTuner(int idTuner)
    {
      _channel.DeleteTuner(idTuner);
    }

    #endregion

    #region tuner properties

    public IList<TunerProperty> ListAllTunerPropertiesByTuner(int idTuner)
    {
      return _channel.ListAllTunerPropertiesByTuner(idTuner);
    }

    public IList<TunerProperty> SaveTunerProperties(IEnumerable<TunerProperty> properties)
    {
      foreach (TunerProperty property in properties)
      {
        property.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveTunerProperties(properties);
    }

    #endregion

    #region tuner groups

    public IList<TunerGroup> ListAllTunerGroups()
    {
      return _channel.ListAllTunerGroups();
    }

    public TunerGroup GetTunerGroup(int idTunerGroup)
    {
      return _channel.GetTunerGroup(idTunerGroup);
    }

    public TunerGroup SaveTunerGroup(TunerGroup group)
    {
      group.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveTunerGroup(group);
    }

    public void DeleteTunerGroup(int idTunerGroup)
    {
      _channel.DeleteTunerGroup(idTunerGroup);
    }

    #endregion

    #region analog tuner settings

    public AnalogTunerSettings GetAnalogTunerSettings(int idAnalogTunerSettings)
    {
      return _channel.GetAnalogTunerSettings(idAnalogTunerSettings);
    }

    public AnalogTunerSettings SaveAnalogTunerSettings(AnalogTunerSettings settings)
    {
      settings.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveAnalogTunerSettings(settings);
    }

    #endregion

    #region stream tuner settings

    public StreamTunerSettings GetStreamTunerSettings(int idStreamTunerSettings)
    {
      return _channel.GetStreamTunerSettings(idStreamTunerSettings);
    }

    public StreamTunerSettings SaveStreamTunerSettings(StreamTunerSettings settings)
    {
      settings.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveStreamTunerSettings(settings);
    }

    #endregion

    #region tuner satellites

    public IList<TunerSatellite> ListAllTunerSatellites(TunerSatelliteRelation includeRelations)
    {
      return _channel.ListAllTunerSatellites(includeRelations);
    }

    public IList<TunerSatellite> ListAllTunerSatellitesByTuner(int idTuner, TunerSatelliteRelation includeRelations)
    {
      return _channel.ListAllTunerSatellitesByTuner(idTuner, includeRelations);
    }

    public TunerSatellite GetTunerSatellite(int idTunerSatellite, TunerSatelliteRelation includeRelations)
    {
      return _channel.GetTunerSatellite(idTunerSatellite, includeRelations);
    }

    public TunerSatellite SaveTunerSatellite(TunerSatellite tunerSatellite)
    {
      tunerSatellite.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveTunerSatellite(tunerSatellite);
    }

    public IList<TunerSatellite> SaveTunerSatellites(IEnumerable<TunerSatellite> tunerSatellites)
    {
      foreach (TunerSatellite tunerSatellite in tunerSatellites)
      {
        tunerSatellite.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveTunerSatellites(tunerSatellites);
    }

    public void DeleteTunerSatellite(int idTunerSatellite)
    {
      _channel.DeleteTunerSatellite(idTunerSatellite);
    }

    #endregion

    #region software encoders

    public IList<VideoEncoder> ListAvailableSoftwareEncodersVideo()
    {
      return _channel.ListAvailableSoftwareEncodersVideo();
    }

    public IList<AudioEncoder> ListAvailableSoftwareEncodersAudio()
    {
      return _channel.ListAvailableSoftwareEncodersAudio();
    }

    #endregion

    #region BDA network providers

    public IList<BdaNetworkProvider> ListAvailableBdaNetworkProviders()
    {
      return _channel.ListAvailableBdaNetworkProviders();
    }

    #endregion

    #region network interface names

    public IList<string> ListAvailableNetworkInterfaceNames()
    {
      return _channel.ListAvailableNetworkInterfaceNames();
    }

    #endregion

    #region satellites

    public IList<Satellite> ListAllSatellites()
    {
      return _channel.ListAllSatellites();
    }

    public IList<Satellite> ListAllReferencedSatellites()
    {
      return _channel.ListAllReferencedSatellites();
    }

    public Satellite GetSatellite(int idSatellite)
    {
      return _channel.GetSatellite(idSatellite);
    }

    public Satellite GetSatelliteByLongitude(int longitude)
    {
      return _channel.GetSatelliteByLongitude(longitude);
    }

    public Satellite SaveSatellite(Satellite satellite)
    {
      satellite.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveSatellite(satellite);
    }

    public IList<Satellite> SaveSatellites(IEnumerable<Satellite> satellites)
    {
      foreach (Satellite satellite in satellites)
      {
        satellite.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveSatellites(satellites);
    }

    public void DeleteSatellite(int idSatellite)
    {
      _channel.DeleteSatellite(idSatellite);
    }

    #endregion

    #region LNB types

    public IList<LnbType> ListAllLnbTypes()
    {
      return _channel.ListAllLnbTypes();
    }

    public LnbType GetLnbType(int idLnbType)
    {
      return _channel.GetLnbType(idLnbType);
    }

    #endregion
  }
}