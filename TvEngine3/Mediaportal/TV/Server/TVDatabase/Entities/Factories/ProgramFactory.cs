using System;
using Mediaportal.TV.Server.Common.Types.Enum;
using System.Data.SqlTypes;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ProgramFactory
  {
    public static Program Clone(Program source)
    {
      return CloneHelper.DeepCopy<Program>(source);
    }

    public static Program CreateProgram(int idChannel, DateTime startTime, DateTime endTime, string title)
    {
      var program = new Program
      {
        IdChannel = idChannel,
        StartTime = startTime,
        EndTime = endTime,
        Title = title ?? string.Empty,
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
        AudioLanguages = null,
        SubtitlesLanguages = null,
        IsLive = null,
        ProductionYear = null,
        ProductionCountry = null,
        StarRating = null,
        StarRatingMaximum = null,
        State = (int)ProgramState.None
      };
      return program;
    }
  }
}