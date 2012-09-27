using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class OnDayCondition : IScheduleCondition
  {
    private IList<DayOfWeek> _ondays;
    public OnDayCondition(IList<DayOfWeek> ondays)
    {
      _ondays = ondays;
    }

    public OnDayCondition()
    {
    }

    public IList<DayOfWeek> Ondays
    {
      get { return _ondays; }
      set { _ondays = value; }
    }

    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      return baseQuery.Where(program => (_ondays.Any(d => d == program.StartTime.DayOfWeek)));
    }
    
  }
}
