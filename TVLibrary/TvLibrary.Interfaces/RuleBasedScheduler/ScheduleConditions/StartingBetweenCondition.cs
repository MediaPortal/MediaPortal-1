using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
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

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      if (_startTimeInterval.HasValue && _endTimeInterval.HasValue)
      {
        return baseQuery.Where(program => program.StartTime >= _startTimeInterval.GetValueOrDefault() && program.StartTime <= _endTimeInterval.GetValueOrDefault()); 
      }
      return baseQuery;
    }

  }
}
