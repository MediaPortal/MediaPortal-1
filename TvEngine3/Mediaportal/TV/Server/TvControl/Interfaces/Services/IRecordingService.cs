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
    IList<Recording> ListAllRecordings();

    [OperationContract]
    IList<Recording> ListAllRecordingsByMediaType(MediaType mediaType);

    [OperationContract]
    IList<Recording> ListAllActiveRecordingsByMediaType(MediaType mediaType);

    [OperationContract]
    Recording GetRecording(int idRecording);

    [OperationContract]
    Recording GetRecordingByFileName(string fileName);

    [OperationContract]
    Recording GetActiveRecording(int scheduleId);

    [OperationContract]
    Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel);

    [OperationContract]
    Recording SaveRecording(Recording recording);
  }
}