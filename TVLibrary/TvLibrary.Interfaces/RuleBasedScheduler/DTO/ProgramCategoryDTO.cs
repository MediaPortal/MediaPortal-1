using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class ProgramCategoryDTO
  {
    public int IdCategory { get; set; }
    public string Category { get; set; }

    public override string ToString()
    {
      return Category;
    }
  }
}
