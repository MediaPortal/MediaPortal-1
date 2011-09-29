using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVDatabaseEntities
{
  public partial class ProgramCredit
  {
    public override string ToString()
    {
      return ("[" + role + "] = [" + person + "]");
    }
  }
}
