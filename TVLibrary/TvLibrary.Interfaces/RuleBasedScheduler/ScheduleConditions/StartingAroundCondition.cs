using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
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

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
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
