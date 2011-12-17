using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                            idChannel = idChannel,
                            idSchedule = idSchedule,
                            isRecording = isRecording,
                            startTime = startTime,
                            endTime = endTime,
                            title = title,
                            description = description,
                            ProgramCategory = programCategory,
                            fileName = fileName,
                            keepUntil = keepUntil,
                            keepUntilDate = keepUntilDate,
                            timesWatched = timesWatched,
                            episodeName = episodeName,
                            seriesNum = seriesNum,
                            episodeNum = episodeNum,
                            episodePart = episodePart

                          };
      return recording;
    }
               
       
     
      
  }
}
