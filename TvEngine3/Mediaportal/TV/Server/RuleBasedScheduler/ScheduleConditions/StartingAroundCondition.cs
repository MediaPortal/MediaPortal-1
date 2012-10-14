using System;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class StartingAroundCondition : IScheduleCondition
  {
    private DateTime? _aroundTime;
    private int? _deviationMins;

    public StartingAroundCondition(DateTime? aroundTime, int? deviationMins)
    {
      _aroundTime = aroundTime;
      _deviationMins = deviationMins;
    }

    public StartingAroundCondition()
    {
    }

    public DateTime? AroundTime
    {
      get { return _aroundTime; }
      set { _aroundTime = value; }
    }

    public int? DeviationMins
    {
      get { return _deviationMins; }
      set { _deviationMins = value; }
    }

    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      if (_aroundTime.HasValue && _deviationMins.HasValue)
      {
        DateTime from = _aroundTime.GetValueOrDefault().AddMinutes(-1 * _deviationMins.GetValueOrDefault());
        DateTime to = _aroundTime.GetValueOrDefault().AddMinutes(_deviationMins.GetValueOrDefault());
        return baseQuery.Where(program => (program.StartTime >= from && program.StartTime <= to));
      }
      return baseQuery;
      // eg prg @ 16.50
      // around 16.40
      // deviation +/- 10 mins
      // => starttime between 16.30 and 16.50
    }
 
  }
}
