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

    public IList<Tuner> ListAllTuners(TunerRelation includeRelations)
    {
      return TunerManagement.ListAllTuners(includeRelations);
    }

    public Tuner GetTuner(int idTuner, TunerRelation includeRelations)
    {
      return TunerManagement.GetTuner(idTuner, includeRelations);
    }

    public Tuner GetTunerByExternalId(string externalId, TunerRelation includeRelations)
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

    #region stream tuner settings

    public StreamTunerSettings GetStreamTunerSettings(int idStreamTunerSettings)
    {
      return StreamTunerSettingsManagement.GetStreamTunerSettings(idStreamTunerSettings);
    }

    public StreamTunerSettings SaveStreamTunerSettings(StreamTunerSettings settings)
    {
      return StreamTunerSettingsManagement.SaveStreamTunerSettings(settings);
    }

    #endregion

    #region tuner satellites

    public IList<TunerSatellite> ListAllTunerSatellites(TunerSatelliteRelation includeRelations)
    {
      return TunerSatelliteManagement.ListAllTunerSatellites(includeRelations);
    }

    public IList<TunerSatellite> ListAllTunerSatellitesByTuner(int idTuner, TunerSatelliteRelation includeRelations)
    {
      return TunerSatelliteManagement.ListAllTunerSatellitesByTuner(idTuner, includeRelations);
    }

    public TunerSatellite GetTunerSatellite(int idTunerSatellite, TunerSatelliteRelation includeRelations)
    {
      return TunerSatelliteManagement.GetTunerSatellite(idTunerSatellite, includeRelations);
    }

    public TunerSatellite SaveTunerSatellite(TunerSatellite tunerSatellite)
    {
      return TunerSatelliteManagement.SaveTunerSatellite(tunerSatellite);
    }

    public IList<TunerSatellite> SaveTunerSatellites(IEnumerable<TunerSatellite> tunerSatellites)
    {
      return TunerSatelliteManagement.SaveTunerSatellites(tunerSatellites);
    }

    public void DeleteTunerSatellite(int idTunerSatellite)
    {
      TunerSatelliteManagement.DeleteTunerSatellite(idTunerSatellite);
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

    #region network interface names

    public IList<string> ListAvailableNetworkInterfaceNames()
    {
      return SystemInformation.ListAvailableNetworkInterfaceNames();
    }

    #endregion

    #region satellites

    public IList<Satellite> ListAllSatellites()
    {
      return SatelliteManagement.ListAllSatellites();
    }

    public IList<Satellite> ListAllReferencedSatellites()
    {
      return SatelliteManagement.ListAllReferencedSatellites();
    }

    public Satellite GetSatellite(int idSatellite)
    {
      return SatelliteManagement.GetSatellite(idSatellite);
    }

    public Satellite GetSatelliteByLongitude(int longitude)
    {
      return SatelliteManagement.GetSatelliteByLongitude(longitude);
    }

    public Satellite SaveSatellite(Satellite satellite)
    {
      return SatelliteManagement.SaveSatellite(satellite);
    }

    public IList<Satellite> SaveSatellites(IEnumerable<Satellite> satellites)
    {
      return SatelliteManagement.SaveSatellites(satellites);
    }

    public void DeleteSatellite(int idSatellite)
    {
      SatelliteManagement.DeleteSatellite(idSatellite);
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
  }
}