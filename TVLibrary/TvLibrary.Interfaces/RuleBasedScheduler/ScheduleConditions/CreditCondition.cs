using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class CreditCondition : IScheduleCondition
  {
    private IList<ProgramCreditDTO> _credits;
    public CreditCondition()
    {
    }
    public CreditCondition(IList<ProgramCreditDTO> credits)
    {
      _credits = credits;
    }

    public IList<ProgramCreditDTO> Credits
    {
      get { return _credits; }
      set { _credits = value; }
    }

    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return
        baseQuery.Where(
          program =>  
          program.ReferencedProgramCredits != null && 
          program.ReferencedProgramCredits.Any(
            c =>
            (_credits.Any(
              d =>
              d.Role.ToUpperInvariant().Contains(c.Role.ToUpperInvariant()) &&
              d.Person.ToUpperInvariant().Contains(c.Person.ToUpperInvariant())))));
    }   
  }
}
