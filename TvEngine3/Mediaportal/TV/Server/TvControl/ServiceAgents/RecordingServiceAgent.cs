using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class RecordingServiceAgent : ServiceAgent<IRecordingService>, IRecordingService 
  {
    public RecordingServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<Recording> ListAllRecordings()
    {
      return _channel.ListAllRecordings();
    }

    public IList<Recording> ListAllRecordingsByMediaType(MediaType mediaType)
    {
      return _channel.ListAllRecordingsByMediaType(mediaType);
    }

    public IList<Recording> ListAllActiveRecordingsByMediaType(MediaType mediaType)
    {
      return _channel.ListAllActiveRecordingsByMediaType(mediaType);
    }

    public Recording GetRecording(int idRecording)
    {
      return _channel.GetRecording(idRecording);
    }

    public Recording GetRecordingByFileName(string fileName)
    {
      return _channel.GetRecordingByFileName(fileName);
    }

    public Recording GetActiveRecording(int scheduleId)
    {
      return _channel.GetActiveRecording(scheduleId);
    }

    public Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel)
    {
      return _channel.GetActiveRecordingByTitleAndChannel(title, idChannel);
    }

    public Recording SaveRecording(Recording recording)
    {
      recording.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveRecording(recording);
    }
  }
}