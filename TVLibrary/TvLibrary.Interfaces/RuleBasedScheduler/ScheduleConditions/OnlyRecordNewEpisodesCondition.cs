using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
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

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => !(_skipEpisodes.Any(e => e == program.EpisodeNum)));
    }
   
  }
}
