using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class OnlyRecordNewTitlesCondition : IScheduleCondition
  {    
    private readonly IList<string> _skipTitles;

    public OnlyRecordNewTitlesCondition(IList<string> skipTitles)
    {
      _skipTitles = skipTitles;
    }
    public OnlyRecordNewTitlesCondition()
    {
    }

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => !(_skipTitles.Any(t => program.Title.ToUpperInvariant().Equals(t.ToUpperInvariant()))));
    }
   
  }
}
