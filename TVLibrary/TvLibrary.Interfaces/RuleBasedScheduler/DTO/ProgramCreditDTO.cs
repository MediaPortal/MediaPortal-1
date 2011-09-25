using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class ProgramCreditDTO
  {
      public int IdCredit { get; set; }      
      public string Person { get; set; }
      public string Role { get; set; }
  }
}
