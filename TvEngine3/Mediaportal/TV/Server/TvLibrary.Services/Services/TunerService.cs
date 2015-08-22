using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class TunerService : ITunerService
  {
    #region tuners

    public IList<Tuner> ListAllTuners()
    {
      return TunerManagement.ListAllTuners();
    }

    public IList<Tuner> ListAllTuners(TunerIncludeRelationEnum includeRelations)
    {
      return TunerManagement.ListAllTuners(includeRelations);
    }

    public Tuner GetTuner(int idTuner)
    {
      return TunerManagement.GetTuner(idTuner);
    }

    public Tuner GetTuner(int idTuner, TunerIncludeRelationEnum includeRelations)
    {
      return TunerManagement.GetTuner(idTuner, includeRelations);
    }

    public Tuner GetTunerByExternalId(string externalId)
    {
      return TunerManagement.GetTunerByExternalId(externalId);
    }

    public Tuner GetTunerByExternalId(string externalId, TunerIncludeRelationEnum includeRelations)
    {
      return TunerManagement.GetTunerByExternalId(externalId, includeRelations);
    }

    public Tuner SaveTuner(Tuner tuner)
    {
      return TunerManagement.SaveTuner(tuner);
    }

    public IList<Tuner> SaveTuners(IEnumerable<Tuner> tuners)
    {
      return TunerManagement.SaveTuners(tuners);
    }

    public void DeleteTuner(int idTuner)
    {
      TunerManagement.DeleteTuner(idTuner);
    }

    #endregion

    #region tuner properties

    public IList<TunerProperty> ListAllTunerPropertiesByTuner(int idTuner)
    {
      return TunerPropertyManagement.ListAllTunerPropertiesByTuner(idTuner);
    }

    public IList<TunerProperty> SaveTunerProperties(IEnumerable<TunerProperty> properties)
    {
      return TunerPropertyManagement.SaveTunerProperties(properties);
    }

    #endregion

    #region tuner groups

    public IList<TunerGroup> ListAllTunerGroups()
    {
      return TunerGroupManagement.ListAllTunerGroups();
    }

    public TunerGroup GetTunerGroup(int idTunerGroup)
    {
      return TunerGroupManagement.GetTunerGroup(idTunerGroup);
    }

    public TunerGroup SaveTunerGroup(TunerGroup group)
    {
      return TunerGroupManagement.SaveTunerGroup(group);
    }

    public void DeleteTunerGroup(int idTunerGroup)
    {
      TunerGroupManagement.DeleteTunerGroup(idTunerGroup);
    }

    #endregion

    #region analog tuner settings

    public AnalogTunerSettings GetAnalogTunerSettings(int idAnalogTunerSettings)
    {
      return AnalogTunerSettingsManagement.GetAnalogTunerSettings(idAnalogTunerSettings);
    }

    public AnalogTunerSettings SaveAnalogTunerSettings(AnalogTunerSettings settings)
    {
      return AnalogTunerSettingsManagement.SaveAnalogTunerSettings(settings);
    }

    #endregion

    #region software encoders

    public IList<VideoEncoder> ListAvailableSoftwareEncodersVideo()
    {
      return SystemInformation.ListAvailableSoftwareEncodersVideo();
    }

    public IList<AudioEncoder> ListAvailableSoftwareEncodersAudio()
    {
      return SystemInformation.ListAvailableSoftwareEncodersAudio();
    }

    #endregion

    #region BDA network providers

    public IList<BdaNetworkProvider> ListAvailableBdaNetworkProviders()
    {
      return SystemInformation.ListAvailableBdaNetworkProviders();
    }

    #endregion

    #region satellites

    public IList<Satellite> ListAllSatellites()
    {
      return TunerManagement.ListAllSatellites();
    }

    public Satellite SaveSatellite(Satellite satellite)
    {
      return TunerManagement.SaveSatellite(satellite);
    }

    #endregion

    #region LNB types

    public IList<LnbType> ListAllLnbTypes()
    {
      return LnbTypeManagement.ListAllLnbTypes();
    }
    
    public LnbType GetLnbType(int idLnbType)
    {
      return LnbTypeManagement.GetLnbType(idLnbType);
    }

    #endregion

    public DiseqcMotor SaveDiseqcMotor(DiseqcMotor motor)
    {
      return TunerManagement.SaveDiseqcMotor(motor);
    }
  }
}