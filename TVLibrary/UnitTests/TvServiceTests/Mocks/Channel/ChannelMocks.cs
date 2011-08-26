using System;
using System.Collections.Generic;
using TvDatabase;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks.Channel
{
  public class ChannelMocks
  {
    public static TvDatabase.Channel GetChannel(out ChannelMap channelMap)
    {
      channelMap = Isolate.Fake.Instance<ChannelMap>();
      List<ChannelMap> channelMaps = new List<ChannelMap>();
      channelMaps.Add(channelMap);
      TvDatabase.Channel channel = Isolate.Fake.Instance<TvDatabase.Channel>();
      Isolate.WhenCalled(() => channel.DisplayName).WillReturn("Test Channel");
      Isolate.WhenCalled(() => channel.ReferringChannelMap()).WillReturn(channelMaps);
      return channel;
    }
  }
}
