using System;
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
                            MediaType = (int)mediaType,
                            TimesWatched = timesWatched,
                            TotalTimeWatched = totalTimeWatched,
                            GrabEpg = grabEpg,
                            LastGrabTime = lastGrabTime,
                            SortOrder = sortOrder,
                            VisibleInGuide = visibleInGuide,
                            ExternalId = externalId,
                            DisplayName = displayName
                          };         
      return channel;
    }


    public static Channel CreateChannel(string name)
    {
      var newChannel = new Channel {VisibleInGuide = true, DisplayName = name};
      return newChannel;
    }
  }
}
