using System;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class OnDateCondition : IScheduleCondition
  {
    private DateTime? _onDate;

    public DateTime? OnDate
    {
      get { return _onDate; }
      set { _onDate = value; }
    }

    public OnDateCondition(DateTime? onDate)
    {
      _onDate = onDate;
    }

    public OnDateCondition()
    {
    }

    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      if (_onDate.HasValue)
      {
        return baseQuery.Where(program => (program.startTime.Equals(_onDate)));  
      }
      return baseQuery;
    }
  }
}
