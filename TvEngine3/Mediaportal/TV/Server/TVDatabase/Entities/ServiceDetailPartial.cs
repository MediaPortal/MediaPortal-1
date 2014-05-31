using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ServiceDetail
  {
    public void SetLogicalChannelNumberBasedOnMinorAndMajorChannel (int minorChannel, int majorChannel)
    {
      LogicalChannelNumber = string.Format("{0}.{1}", majorChannel, minorChannel);
    }

    public int MajorChannel
    {
      get
      {
        string majorChannel = LogicalChannelNumber.Split('.').FirstOrDefault();
        int majorChannelAsInt = Convert.ToInt32(majorChannel);
        return majorChannelAsInt;
      }
    }

    public int MinorChannel
    {
      get
      {
        string minorChannel = LogicalChannelNumber.Split('.').LastOrDefault();
        int majorChannelAsInt = Convert.ToInt32(minorChannel);
        return majorChannelAsInt;
      }
    }
  }
}
