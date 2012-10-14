using System;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class StartingBetweenCondition : IScheduleCondition
  {
    private readonly DateTime? _startTimeInterval;
    private readonly DateTime? _endTimeInterval;

    public StartingBetweenCondition(DateTime? startTimeInterval, DateTime? endTimeInterval)
    {
      _startTimeInterval = startTimeInterval;
      _endTimeInterval = endTimeInterval;
    }

    public StartingBetweenCondition()
    {
    }

    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      if (_startTimeInterval.HasValue && _endTimeInterval.HasValue)
      {
        return baseQuery.Where(program => program.StartTime >= _startTimeInterval.GetValueOrDefault() && program.StartTime <= _endTimeInterval.GetValueOrDefault()); 
      }
      return baseQuery;
    }

  }
}
