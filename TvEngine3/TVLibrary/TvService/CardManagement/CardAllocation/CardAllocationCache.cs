using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvDatabase;
using TvLibrary.Interfaces;

namespace TvService
{
  public static class CardAllocationCache
  {
    private static readonly TvBusinessLayer _businessLayer = new TvBusinessLayer();

    public static IList<IChannel> GetTuningDetailsByChannelId(Channel channel)
    {
      var tuningDetails = _businessLayer.GetTuningChannelsByDbChannel(channel);
      return tuningDetails;
    }

    public static bool IsChannelMappedToCard(Channel dbChannel, Card card)
    {
      bool isChannelMappedToCard = _businessLayer.IsChannelMappedToCard(dbChannel, card, false);
      return isChannelMappedToCard;
    }
  }
}
