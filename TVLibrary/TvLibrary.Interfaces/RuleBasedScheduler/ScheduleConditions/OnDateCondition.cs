using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class OnDateCondition : IScheduleCondition
  {
    private DateTime _onDate;

    public DateTime OnDate
    {
      get { return _onDate; }
      set { _onDate = value; }
    }

    public OnDateCondition(DateTime onDate)
    {
      _onDate = onDate;
    }

    public OnDateCondition()
    {
    }

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => (program.StartTime.Equals(_onDate)));
    }
  }
}
