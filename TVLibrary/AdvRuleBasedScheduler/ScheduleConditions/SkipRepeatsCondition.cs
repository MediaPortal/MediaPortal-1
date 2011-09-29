using System;
using System.Linq;
using TVDatabaseEntities;

namespace RuleBasedScheduler.ScheduleConditions
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
