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
    private static readonly IDictionary<int, IList<IChannel>> _tuningChannelMapping = new Dictionary<int, IList<IChannel>>();
    private static readonly IDictionary<int, IDictionary<int, bool>> _channelMapping = new Dictionary<int, IDictionary<int, bool>>();
    private static readonly TvBusinessLayer _businessLayer = new TvBusinessLayer();

    public static IList<IChannel> GetTuningDetailsByChannelId(Channel channel)
    {
      IList<IChannel> tuningDetails;
      bool tuningChannelMappingFound = _tuningChannelMapping.TryGetValue(channel.IdChannel, out tuningDetails);

      if (!tuningChannelMappingFound)
      {        
        tuningDetails = _businessLayer.GetTuningChannelsByDbChannel(channel);
        _tuningChannelMapping.Add(channel.IdChannel, tuningDetails);
      }
      return tuningDetails;
    }

    public static bool IsChannelMappedToCard(Channel dbChannel, Card card)
    {
      bool isChannelMappedToCard = false;

      IDictionary<int, bool> cardIds;

      bool isChannelFound = _channelMapping.TryGetValue(dbChannel.IdChannel, out cardIds);

      bool channelMappingFound = false;
      if (isChannelFound)
      {
        channelMappingFound = cardIds.TryGetValue(card.IdCard, out isChannelMappedToCard);
      }

      if (!channelMappingFound)
      {
        //check if channel is mapped to this card and that the mapping is not for "Epg Only"        
        isChannelMappedToCard = _businessLayer.IsChannelMappedToCard(dbChannel, card, false);

        if (cardIds == null)
        {
          cardIds = new Dictionary<int, bool>();
        }
        cardIds.Add(card.IdCard, isChannelMappedToCard);
      }

      _channelMapping[dbChannel.IdChannel] = cardIds;
      return isChannelMappedToCard;
    }

  }
}
