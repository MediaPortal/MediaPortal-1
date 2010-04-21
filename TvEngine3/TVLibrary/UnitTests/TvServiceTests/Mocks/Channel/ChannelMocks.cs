using System;
using System.Collections.Generic;
using TvDatabase;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks.Channel
{
  public class ChannelMocks
  {
    public static TvDatabase.Channel GetFTAChannel(out ChannelMap channelMap)
    {
      channelMap = Isolate.Fake.Instance<ChannelMap>();
      List<ChannelMap> channelMaps = new List<ChannelMap>();
      channelMaps.Add(channelMap);
      TvDatabase.Channel channel = Isolate.Fake.Instance<TvDatabase.Channel>();
      Isolate.WhenCalled(() => channel.Name).WillReturn("Test FTA");
      Isolate.WhenCalled(() => channel.ReferringChannelMap()).WillReturn(channelMaps);
      Isolate.WhenCalled(() => channel.FreeToAir).WillReturn(true);
      return channel;
    }

    public static TvDatabase.Channel GetScrambledChannel(out ChannelMap channelMap)
    {
      channelMap = Isolate.Fake.Instance<ChannelMap>();
      List<ChannelMap> channelMaps = new List<ChannelMap>();
      channelMaps.Add(channelMap);
      TvDatabase.Channel channel = Isolate.Fake.Instance<TvDatabase.Channel>();
      Isolate.WhenCalled(() => channel.Name).WillReturn("Test Scrambled");
      Isolate.WhenCalled(() => channel.ReferringChannelMap()).WillReturn(channelMaps);
      Isolate.WhenCalled(() => channel.FreeToAir).WillReturn(false);
      return channel;
    }
  }
}
