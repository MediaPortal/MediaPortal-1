using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class RecordingService : IRecordingService
  {
    public IList<Recording> ListAllRecordings()
    {
      return RecordingManagement.ListAllRecordings();
    }

    public IList<Recording> ListAllRecordingsByMediaType(MediaType mediaType)
    {
      return RecordingManagement.ListAllRecordingsByMediaType(mediaType);
    }

    public IList<Recording> ListAllActiveRecordingsByMediaType(MediaType mediaType)
    {
      return RecordingManagement.ListAllActiveRecordingsByMediaType(mediaType);
    }

    public Recording GetRecording(int idRecording)
    {
      return RecordingManagement.GetRecording(idRecording);
    }

    public Recording GetRecordingByFileName(string fileName)
    {
      return RecordingManagement.GetRecordingByFileName(fileName);
    }

    public Recording GetActiveRecording(int scheduleId)
    {
      return RecordingManagement.GetActiveRecording(scheduleId);
    }

    public Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel)
    {
      return RecordingManagement.GetActiveRecordingByTitleAndChannel(title, idChannel);
    }

    public Recording SaveRecording(Recording recording)
    {
      return RecordingManagement.SaveRecording(recording);
    }
  }
}