using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
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

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => !(_notOndays.Any(d => d == program.StartTime.DayOfWeek)));
    }
  }
}
