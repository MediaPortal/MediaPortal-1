using System.Collections.Generic;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardAllocation
{
  public static class CardAllocationCache
  {
    private static readonly IDictionary<int, IList<IChannel>> _tuningChannelMapping = new Dictionary<int, IList<IChannel>>();
    private static readonly IDictionary<int, IDictionary<int, bool>> _channelMapping = new Dictionary<int, IDictionary<int, bool>>();

    public static IList<IChannel> GetTuningDetailsByChannelId(Channel channel)
    {
      IList<IChannel> tuningDetails;
      bool tuningChannelMappingFound = _tuningChannelMapping.TryGetValue(channel.idChannel, out tuningDetails);

      if (!tuningChannelMappingFound)
      {
        tuningDetails = ChannelManagement.GetTuningChannelsByDbChannel(channel);        
        _tuningChannelMapping.Add(channel.idChannel, tuningDetails);
      }
      return tuningDetails;
    }

    public static bool IsChannelMappedToCard(Channel dbChannel, Card card)
    {      
      bool isChannelMappedToCard = false;

      IDictionary<int, bool> cardIds;

      bool isChannelFound = _channelMapping.TryGetValue(dbChannel.idChannel, out cardIds);

      bool channelMappingFound = false;
      if (isChannelFound)
      {
        channelMappingFound = cardIds.TryGetValue(card.IdCard, out isChannelMappedToCard);        
      }      

      if (!channelMappingFound)
      {
        //check if channel is mapped to this card and that the mapping is not for "Epg Only"
        isChannelMappedToCard = ChannelManagement.IsChannelMappedToCard(dbChannel, card, false);

        if (cardIds == null)
        {
          cardIds = new Dictionary<int, bool>();          
        }        
        cardIds.Add(card.IdCard, isChannelMappedToCard);
      }

      _channelMapping[dbChannel.idChannel] = cardIds;
      return isChannelMappedToCard;
    }

  }
}
