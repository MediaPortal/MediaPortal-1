using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class ChannelFactory
  {
    public static Channel Clone(Channel source)
    {
      return CloneHelper.DeepCopy<Channel>(source);      
    }
      
    public static Channel CreateChannel (MediaTypeEnum mediaType, int timesWatched, DateTime totalTimeWatched, bool grabEpg,
                   DateTime lastGrabTime, int sortOrder, bool visibleInGuide, string externalId,
                   string displayName)
    {
      var channel = new Channel
                          {
                            mediaType = (int)mediaType,
                            timesWatched = timesWatched,
                            totalTimeWatched = totalTimeWatched,
                            grabEpg = grabEpg,
                            lastGrabTime = lastGrabTime,
                            sortOrder = sortOrder,
                            visibleInGuide = visibleInGuide,
                            externalId = externalId,
                            displayName = displayName
                          };         
      return channel;
    }


    public static Channel CreateChannel(string name)
    {
      var newChannel = new Channel {visibleInGuide = true, displayName = name};
      return newChannel;
    }
  }
}
