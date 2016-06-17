using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface ITunerService
  {
    #region tuners

    [OperationContract]
    IList<Tuner> ListAllTuners(TunerRelation includeRelations);

    [OperationContract]
    Tuner GetTuner(int idTuner, TunerRelation includeRelations);

    [OperationContract]
    Tuner GetTunerByExternalId(string externalId, TunerRelation includeRelations);

    [OperationContract]
    Tuner SaveTuner(Tuner tuner);

    [OperationContract]
    IList<Tuner> SaveTuners(IEnumerable<Tuner> tuners);

    [OperationContract]
    void DeleteTuner(int idTuner);

    #endregion

    #region tuner properties

    [OperationContract]
    IList<TunerProperty> ListAllTunerPropertiesByTuner(int idTuner);

    [OperationContract]
    IList<TunerProperty> SaveTunerProperties(IEnumerable<TunerProperty> properties);

    #endregion

    #region tuner groups

    [OperationContract]
    IList<TunerGroup> ListAllTunerGroups();

    [OperationContract]
    TunerGroup GetTunerGroup(int idTunerGroup);

    [OperationContract]
    TunerGroup SaveTunerGroup(TunerGroup group);

    [OperationContract]
    void DeleteTunerGroup(int idTunerGroup);

    #endregion

    #region analog tuner settings

    [OperationContract]
    AnalogTunerSettings GetAnalogTunerSettings(int idAnalogTunerSettings);

    [OperationContract]
    AnalogTunerSettings SaveAnalogTunerSettings(AnalogTunerSettings settings);

    #endregion

    #region tuner satellites

    [OperationContract]
    IList<TunerSatellite> ListAllTunerSatellites(TunerSatelliteRelation includeRelations);

    [OperationContract]
    IList<TunerSatellite> ListAllTunerSatellitesByTuner(int idTuner, TunerSatelliteRelation includeRelations);

    [OperationContract]
    TunerSatellite GetTunerSatellite(int idTunerSatellite, TunerSatelliteRelation includeRelations);

    [OperationContract]
    TunerSatellite SaveTunerSatellite(TunerSatellite tunerSatellite);

    [OperationContract]
    IList<TunerSatellite> SaveTunerSatellites(IEnumerable<TunerSatellite> tunerSatellites);

    [OperationContract]
    void DeleteTunerSatellite(int idTunerSatellite);

    #endregion

    #region software encoders

    [OperationContract]
    IList<VideoEncoder> ListAvailableSoftwareEncodersVideo();

    [OperationContract]
    IList<AudioEncoder> ListAvailableSoftwareEncodersAudio();

    #endregion

    #region BDA network providers

    [OperationContract]
    IList<BdaNetworkProvider> ListAvailableBdaNetworkProviders();

    #endregion

    #region satellites

    [OperationContract]
    IList<Satellite> ListAllSatellites();

    [OperationContract]
    IList<Satellite> ListAllReferencedSatellites();

    [OperationContract]
    Satellite SaveSatellite(Satellite satellite);

    [OperationContract]
    IList<Satellite> SaveSatellites(IEnumerable<Satellite> satellites);

    [OperationContract]
    void DeleteSatellite(int idSatellite);

    #endregion

    #region LNB types

    [OperationContract]
    IList<LnbType> ListAllLnbTypes();

    [OperationContract]
    LnbType GetLnbType(int idLnbType);

    #endregion
  }
}