using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class NotOnCategoryCondition : IScheduleCondition
  {
    private IList<ProgramCategoryDTO> _categories;
    public IList<ProgramCategoryDTO> Categories
    {
      get { return _categories; }
      set { _categories = value; }
    }
    public NotOnCategoryCondition(IList<ProgramCategoryDTO> categories)
    {
      _categories = categories;
    }
    public NotOnCategoryCondition()
    {
    }
    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => !(_categories.Any(ch => ch.IdCategory == program.ReferencedProgramCategory.IdCategory)));
    }
  }
}
