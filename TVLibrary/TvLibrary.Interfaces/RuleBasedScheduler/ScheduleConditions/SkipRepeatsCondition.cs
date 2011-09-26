using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class SkipRepeatsCondition : IScheduleCondition
  {    
    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
       return baseQuery.Where(program => !program.PreviouslyShown);
    }
  }
}
