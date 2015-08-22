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
    IList<Tuner> ListAllTuners();

    [OperationContract(Name = "ListAllTunersWithSpecificRelations")]
    IList<Tuner> ListAllTuners(TunerIncludeRelationEnum includeRelations);

    [OperationContract]
    Tuner GetTuner(int idTuner);

    [OperationContract(Name = "GetTunerWithSpecificRelations")]
    Tuner GetTuner(int idTuner, TunerIncludeRelationEnum includeRelations);

    [OperationContract]
    Tuner GetTunerByExternalId(string externalId);

    [OperationContract(Name = "GetTunerByExternalIdWithSpecificRelations")]
    Tuner GetTunerByExternalId(string externalId, TunerIncludeRelationEnum includeRelations);

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
    Satellite SaveSatellite(Satellite satellite);

    #endregion

    #region LNB types

    [OperationContract]
    IList<LnbType> ListAllLnbTypes();

    [OperationContract]
    LnbType GetLnbType(int idLnbType);

    #endregion

    [OperationContract]
    DiseqcMotor SaveDiseqcMotor(DiseqcMotor motor);
  }
}