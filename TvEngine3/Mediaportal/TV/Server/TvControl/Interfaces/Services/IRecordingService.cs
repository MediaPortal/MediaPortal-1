using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  // Define a service contract.
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IRecordingService
  {
    [OperationContract]
    Recording GetRecording(int idRecording);

    [OperationContract]
    IList<Recording> ListAllRecordingsByMediaType(MediaType mediaType);

    [OperationContract]
    Recording SaveRecording(Recording recording);

    [OperationContract]
    Recording GetRecordingByFileName(string fileName);

    [OperationContract]
    Recording GetActiveRecording(int scheduleId);

    [OperationContract]
    Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel);

    [OperationContract]
    IList<Recording> ListAllActiveRecordingsByMediaType(MediaType mediaType);

    [OperationContract]
    void DeleteRecording(int idRecording);
  }
}