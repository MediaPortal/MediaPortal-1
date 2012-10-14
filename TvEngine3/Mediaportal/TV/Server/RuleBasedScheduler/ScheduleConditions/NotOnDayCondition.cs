using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class NotOnDayCondition : IScheduleCondition
  {
    private IList<DayOfWeek> _notOndays;
    public NotOnDayCondition(IList<DayOfWeek> notOndays)
    {
      _notOndays = notOndays;
    }
    public NotOnDayCondition()
    {
    }

    public IList<DayOfWeek> NotOndays
    {
      get { return _notOndays; }
      set { _notOndays = value; }
    }

    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      return baseQuery.Where(program => !(_notOndays.Any(d => d == program.StartTime.DayOfWeek)));
    }
  }
}
