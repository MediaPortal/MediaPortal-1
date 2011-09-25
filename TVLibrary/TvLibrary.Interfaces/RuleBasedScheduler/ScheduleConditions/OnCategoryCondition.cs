using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class OnCategoryCondition : IScheduleCondition
  {
    private IList<ProgramCategoryDTO> _categories;
    public IList<ProgramCategoryDTO> Categories
    {
      get { return _categories; }
      set { _categories = value; }
    }
    public OnCategoryCondition(IList<ProgramCategoryDTO> categories)
    {
      _categories = categories;
    }
    public OnCategoryCondition()
    {
    }
    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => (_categories.Any(categoryDto => categoryDto.IdCategory == program.ReferencedProgramCategory.IdCategory)));
    }
  }
}
