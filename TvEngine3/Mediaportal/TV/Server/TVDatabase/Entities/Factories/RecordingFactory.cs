using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class RecordingFactory
  {
    public static Recording CreateRecording(int idChannel, int? idSchedule, bool isRecording, DateTime startTime, DateTime endTime, string title,
                     string description, ProgramCategory programCategory,
                     string fileName, int keepUntil, DateTime keepUntilDate, int timesWatched,
                     string episodeName,
                     string seriesNum, string episodeNum, string episodePart)
    {
      var recording = new Recording
                          {
                            IdChannel = idChannel,
                            IdSchedule = idSchedule,
                            IsRecording = isRecording,
                            StartTime = startTime,
                            EndTime = endTime,
                            Title = title,
                            Description = description,
                            ProgramCategory = programCategory,
                            FileName = fileName,
                            KeepUntil = keepUntil,
                            KeepUntilDate = keepUntilDate,
                            TimesWatched = timesWatched,
                            EpisodeName = episodeName,
                            SeriesNum = seriesNum,
                            EpisodeNum = episodeNum,
                            EpisodePart = episodePart

                          };
      return recording;
    }
               
       
     
      
  }
}
