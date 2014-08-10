using System.Collections.Generic;
using System.Linq;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Channel
  {
    public override string ToString()
    {
      return DisplayName;
    }
  }
}