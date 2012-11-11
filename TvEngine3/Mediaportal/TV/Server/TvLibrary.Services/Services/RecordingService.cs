using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{ 
  public class RecordingService : IRecordingService
  {    

    public Recording GetRecording(int idRecording)
    {
      Recording recording = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(idRecording);
      return recording;
    }

    public IList<Recording> ListAllRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      IEnumerable<Recording> recordings = RecordingManagement.ListAllRecordingsByMediaType(mediaType);
      return recordings.ToList();
    }

    public Recording SaveRecording(Recording recording)
    {
      return RecordingManagement.SaveRecording(recording);
    }

    public Recording GetRecordingByFileName(string fileName)
    {
      var recordingByFileName = RecordingManagement.GetRecordingByFileName(fileName);
      return recordingByFileName;
    }

    public Recording GetActiveRecording(int scheduleId)
    {
      var activeRecording = RecordingManagement.GetActiveRecording(scheduleId);
      return activeRecording;
    }

    public Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel)
    {
      var activeRecordingByTitleAndChannel = RecordingManagement.GetActiveRecordingByTitleAndChannel(title, idChannel);
      return activeRecordingByTitleAndChannel;
    }

    public IList<Recording> ListAllActiveRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      var listAllActiveRecordingsByMediaType = RecordingManagement.ListAllActiveRecordingsByMediaType(mediaType);
      return listAllActiveRecordingsByMediaType;
    }

    public void DeleteRecording(int idRecording)
    {
      RecordingManagement.DeleteRecording(idRecording);
    }
  }
}
