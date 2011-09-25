using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
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

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => (_ondays.Any(d => d == program.StartTime.DayOfWeek)));
    }
    
  }
}
