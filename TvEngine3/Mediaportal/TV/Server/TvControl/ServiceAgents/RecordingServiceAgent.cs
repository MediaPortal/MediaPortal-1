using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class RecordingServiceAgent : ServiceAgent<IRecordingService>, IRecordingService 
  {
    public RecordingServiceAgent(string hostname) : base(hostname)
    {
    }

    public Recording GetRecording(int idRecording)
    {
      return _channel.GetRecording(idRecording);
    }

    public IList<Recording> ListAllRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      return _channel.ListAllRecordingsByMediaType(mediaType);
    }

    public Recording SaveRecording(Recording recording)
    {
      recording.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveRecording(recording);
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

    public IList<Recording> ListAllActiveRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      return _channel.ListAllActiveRecordingsByMediaType(mediaType);
    }

    public void DeleteRecording(int idRecording)
    {
      _channel.DeleteRecording(idRecording);
    }
  }
}
