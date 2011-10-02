using System;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class SkipRepeatsCondition : IScheduleCondition
  {    
    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
       return baseQuery.Where(program => !program.previouslyShown);
    }
  }
}
