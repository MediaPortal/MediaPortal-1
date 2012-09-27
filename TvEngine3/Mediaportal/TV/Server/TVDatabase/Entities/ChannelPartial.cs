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

    public bool IsWebstream()
    {
      IList<TuningDetail> details = TuningDetails;
      if (details == null)
      {
        return false;
      }
      return details.Any(detail => detail.ChannelType == 5);
    }
  }
}
