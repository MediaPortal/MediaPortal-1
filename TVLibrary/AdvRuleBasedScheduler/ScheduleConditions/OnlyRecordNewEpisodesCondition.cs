using System;
using System.Collections.Generic;
using System.Linq;
using TVDatabaseEntities;

namespace RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class OnlyRecordNewEpisodesCondition : IScheduleCondition
  {
    private readonly IList<int> _skipEpisodes;

    public OnlyRecordNewEpisodesCondition(IList<int> skipEpisodes)
    {
      _skipEpisodes = skipEpisodes;
    }

    public OnlyRecordNewEpisodesCondition()
    {
    }

    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {      
      
      return baseQuery.Where(program => !(_skipEpisodes.Any(e => e.Equals(program.episodeNum))));      
      //return baseQuery.Where(program => !DB.Recordings.Any(r => r.Title == program.Title && t.SeriesNum == program.SeriesNum && t.EpisodeNum == program.EpisodeNum));
    }
   
  }
}
