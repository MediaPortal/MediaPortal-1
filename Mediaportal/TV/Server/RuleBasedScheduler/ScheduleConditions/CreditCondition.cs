using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class CreditCondition : IScheduleCondition
  {
    private IList<ProgramCredit> _credits;
    public CreditCondition()
    {
    }
    public CreditCondition(IList<ProgramCredit> credits)
    {
      _credits = credits;
    }

    public IList<ProgramCredit> Credits
    {
      get { return _credits; }
      set { _credits = value; }
    }

    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      return
        baseQuery.Where(
          program =>  
          program.ProgramCredits != null &&
          program.ProgramCredits.Any(
            c =>
            (_credits.Any(
              d =>
              d.role.ToUpperInvariant().Contains(c.role.ToUpperInvariant()) &&
              d.person.ToUpperInvariant().Contains(c.person.ToUpperInvariant())))));
    }   
  }
}
