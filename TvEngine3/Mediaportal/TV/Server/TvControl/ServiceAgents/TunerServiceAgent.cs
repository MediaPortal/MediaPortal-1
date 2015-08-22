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

    public IList<Tuner> ListAllTuners()
    {
      return _channel.ListAllTuners();
    }

    public IList<Tuner> ListAllTuners(TunerIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllTuners(includeRelations);
    }

    public Tuner GetTuner(int idTuner)
    {
      return _channel.GetTuner(idTuner);
    }

    public Tuner GetTuner(int idTuner, TunerIncludeRelationEnum includeRelations)
    {
      return _channel.GetTuner(idTuner, includeRelations);
    }

    public Tuner GetTunerByExternalId(string externalId)
    {
      return _channel.GetTunerByExternalId(externalId);
    }

    public Tuner GetTunerByExternalId(string externalId, TunerIncludeRelationEnum includeRelations)
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

    #region satellites

    public IList<Satellite> ListAllSatellites()
    {
      return _channel.ListAllSatellites();
    }

    public Satellite SaveSatellite(Satellite satellite)
    {
      satellite.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveSatellite(satellite);
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

    public DiseqcMotor SaveDiseqcMotor(DiseqcMotor motor)
    {
      motor.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveDiseqcMotor(motor);
    }
  }
}