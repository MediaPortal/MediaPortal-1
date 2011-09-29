using System;
using System.Collections.Generic;
using System.Linq;
using TVDatabaseEntities;

namespace RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class NotOnCategoryCondition : IScheduleCondition
  {
    private IList<ProgramCategory> _categories;
    public IList<ProgramCategory> Categories
    {
      get { return _categories; }
      set { _categories = value; }
    }
    public NotOnCategoryCondition(IList<ProgramCategory> categories)
    {
      _categories = categories;
    }
    public NotOnCategoryCondition()
    {
    }
    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      return baseQuery.Where(program => !(_categories.Any(ch => ch.idProgramCategory  == program.ProgramCategory.idProgramCategory)));
    }
  }
}
