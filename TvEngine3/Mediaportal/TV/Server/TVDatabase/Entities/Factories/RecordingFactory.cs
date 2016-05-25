using System;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class RecordingFactory
  {
    public static Recording CreateRecording(string fileName, MediaType mediaType, DateTime startTime, DateTime endTime, Schedule schedule, Program program)
    {
      var recording = new Recording
      {
        FileName = fileName,
        MediaType = (int)mediaType,
        StartTime = startTime,
        EndTime = endTime,
        IsRecording = true,
        WatchedCount = 0,
        StopTime = 0
      };

      if (schedule != null)
      {
        recording.IdSchedule = schedule.IdSchedule;
        recording.KeepMethod = schedule.KeepMethod;
        recording.KeepUntilDate = schedule.KeepDate;
      }
      else
      {
        recording.IdSchedule = null;
        recording.KeepMethod = (int)RecordingKeepMethod.UntilSpaceNeeded;
        recording.KeepUntilDate = null;
      }

      if (program != null)
      {
        recording.IdChannel = program.IdChannel;
        recording.Title = program.Title;
        recording.Description = program.Description;
        recording.EpisodeName = program.EpisodeName;
        recording.SeriesId = program.SeriesId;
        recording.SeasonNumber = program.SeasonNumber;
        recording.EpisodeId = program.EpisodeId;
        recording.EpisodeNumber = program.EpisodeNumber;
        recording.EpisodePartNumber = program.EpisodePartNumber;
        recording.IsPreviouslyShown = program.IsPreviouslyShown;
        recording.OriginalAirDate = program.OriginalAirDate;
        recording.IdProgramCategory = program.IdProgramCategory;
        recording.Classification = program.Classification;
        recording.Advisories = program.Advisories;
        recording.IsHighDefinition = program.IsHighDefinition;
        recording.IsThreeDimensional = program.IsThreeDimensional;
        recording.IsLive = program.IsLive;
        recording.ProductionYear = program.ProductionYear;
        recording.ProductionCountry = program.ProductionCountry;
        recording.StarRating = program.StarRating;
        recording.StarRatingMaximum = program.StarRatingMaximum;
        foreach (ProgramCredit credit in program.ProgramCredits)
        {
          recording.RecordingCredits.Add(new RecordingCredit { Person = credit.Person, Role = credit.Role });
        }
      }
      else
      {
        recording.IdChannel = null;
        recording.Title = string.Empty;
        recording.Description = string.Empty;
        recording.EpisodeName = null;
        recording.SeriesId = null;
        recording.SeasonNumber = null;
        recording.EpisodeId = null;
        recording.EpisodeNumber = null;
        recording.EpisodePartNumber = null;
        recording.IsPreviouslyShown = null;
        recording.OriginalAirDate = null;
        recording.IdProgramCategory = null;
        recording.Classification = null;
        recording.Advisories = (int)ContentAdvisory.None;
        recording.IsHighDefinition = null;
        recording.IsThreeDimensional = null;
        recording.IsLive = null;
        recording.ProductionYear = null;
        recording.ProductionCountry = null;
        recording.StarRating = null;
        recording.StarRatingMaximum = null;
      }
      return recording;
    }

    public static Recording CreateRecording(string fileName, MediaType mediaType, DateTime startTime, DateTime endTime, string title)
    {
      var recording = new Recording
      {
        FileName = fileName,
        MediaType = (int)mediaType,
        StartTime = startTime,
        EndTime = endTime,
        IdChannel = null,
        Title = string.Empty,
        Description = string.Empty,
        EpisodeName = null,
        SeriesId = null,
        SeasonNumber = null,
        EpisodeId = null,
        EpisodeNumber = null,
        EpisodePartNumber = null,
        IsPreviouslyShown = null,
        OriginalAirDate = null,
        IdProgramCategory = null,
        Classification = null,
        Advisories = (int)ContentAdvisory.None,
        IsHighDefinition = null,
        IsThreeDimensional = null,
        IsLive = null,
        ProductionYear = null,
        ProductionCountry = null,
        StarRating = null,
        StarRatingMaximum = null,
        IsRecording = false,
        IdSchedule = null,
        KeepMethod = (int)RecordingKeepMethod.UntilSpaceNeeded,
        KeepUntilDate = null,
        WatchedCount = 0,
        StopTime = 0
      };
      return recording;
    }
  }
}