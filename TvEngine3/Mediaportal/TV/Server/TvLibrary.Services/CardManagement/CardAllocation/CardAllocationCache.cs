using System.Collections.Generic;
using System.Linq;
using Mediaportal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardAllocation
{
  public static class CardAllocationCache
  {
    private static readonly IDictionary<int, IList<IChannel>> _tuningChannelMapping = new Dictionary<int, IList<IChannel>>();
    private static readonly IDictionary<int, IDictionary<int, bool>> _channelMapping = new Dictionary<int, IDictionary<int, bool>>();

    private static readonly object _tuningChannelMappingLock = new object();
    private static readonly object _channelMappingLock = new object();


    static CardAllocationCache()
    {
      ChannelManagement.OnStateChangedTuningDetailEvent += new ChannelManagement.OnStateChangedTuningDetailDelegate(ChannelManagement_OnStateChangedTuningDetailEvent);
      ChannelManagement.OnStateChangedChannelMapEvent += new ChannelManagement.OnStateChangedChannelMapDelegate(ChannelManagement_OnStateChangedChannelMapEvent);
      
      var allCardIds = new List<int>();
      IList<Channel> channels = null;
      ThreadHelper.ParallelInvoke(
        ()=>
          {
            IList<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None);
            allCardIds.AddRange(cards.Select(card => card.IdCard));
          },
          ()=>
            {
              ChannelIncludeRelationEnum include = ChannelIncludeRelationEnum.TuningDetails;
              include |= ChannelIncludeRelationEnum.ChannelMaps;
              include |= ChannelIncludeRelationEnum.GroupMaps;
              channels = ChannelManagement.ListAllChannels(include);
            }
          );
                  

      lock (_channelMappingLock)
      {
        lock (_tuningChannelMappingLock)
        {
          foreach (Channel channel in channels)
          {            
            IList<IChannel> tuningDetails = ChannelManagement.GetTuningChannelsByDbChannel(channel);
            _tuningChannelMapping[channel.IdChannel] = tuningDetails;

            IDictionary<int, bool> mapDict = new Dictionary<int, bool>();
            var copyAllCardIds = new List<int>(allCardIds);
            foreach (ChannelMap map in channel.ChannelMaps)
            {
              mapDict[map.IdCard] =  true;
              copyAllCardIds.Remove(map.IdCard);
            }

            foreach (int cardIdNotMapped in copyAllCardIds)
            {
              mapDict[cardIdNotMapped] = false;
            }
            _channelMapping.Add(channel.IdChannel, mapDict);
          }
        }        
      }

    }

    private static void ChannelManagement_OnStateChangedChannelMapEvent(ChannelMap map, ObjectState state)
    {      
      if (state == ObjectState.Deleted)
      {
        UpdateCacheWithChannelMapForChannel(map.IdChannel, map.IdCard, false);
      }
      else if (state == ObjectState.Added)
      {
        UpdateCacheWithChannelMapForChannel(map.IdChannel, map.IdCard, true);
      }
    }


    private static void ChannelManagement_OnStateChangedTuningDetailEvent(ServiceDetail serviceDetail, ObjectState state)
    {
      if (serviceDetail.IdChannel > 0)
      {
        Channel channel = ChannelManagement.GetChannel(serviceDetail.IdChannel);
        UpdateCacheWithTuningDetailsForChannel(channel);
      }
    }

    private static IList<IChannel> UpdateCacheWithTuningDetailsForChannel(Channel channel)
    {
      IList<IChannel> tuningDetails = new List<IChannel>();
      if (channel != null)
      {
        tuningDetails = ChannelManagement.GetTuningChannelsByDbChannel(channel);
        {
          lock (_tuningChannelMappingLock)
          {
            _tuningChannelMapping[channel.IdChannel] = tuningDetails;
          }
        }
      }
      return tuningDetails;
    }

    public static IList<IChannel> GetTuningDetailsByChannelId(Channel channel)
    {
      IList<IChannel> tuningDetails;
      bool tuningChannelMappingFound;

      lock (_tuningChannelMappingLock)
      {
        tuningChannelMappingFound = _tuningChannelMapping.TryGetValue(channel.IdChannel, out tuningDetails);
      }

      if (!tuningChannelMappingFound)
      {
        tuningDetails = UpdateCacheWithTuningDetailsForChannel(channel);        
      }
      return tuningDetails;
    }

    

    private static bool UpdateCacheWithChannelMapForChannel(int idChannel, int idCard, bool? isChannelMappedToCard = null)
    {
      lock (_channelMappingLock)
      {        
        IDictionary<int, bool> cardIds;
        bool isChannelFound = _channelMapping.TryGetValue(idChannel, out cardIds);

        bool channelMappingFound = false;
        bool existingIsMapped = false;
        bool updateNeeded;
        if (isChannelFound)
        {          
          channelMappingFound = cardIds.TryGetValue(idCard, out existingIsMapped);
        }

        if (!channelMappingFound)
        {
          updateNeeded = true;
          //check if channel is mapped to this card and that the mapping is not for "Epg Only"            
          if (cardIds == null)
          {
            cardIds = new Dictionary<int, bool>();
          }
          if (!isChannelMappedToCard.HasValue)
          {
            isChannelMappedToCard = ChannelManagement.IsChannelMappedToCard(idChannel, idCard, false);
          }
        }
        else
        {
          if (isChannelMappedToCard.HasValue)
          {
            updateNeeded = existingIsMapped != isChannelMappedToCard.GetValueOrDefault(); 
          }
          else
          {
            updateNeeded = false;
            isChannelMappedToCard = existingIsMapped;
          }          
        }                
        
        if (updateNeeded)
        {
          //make sure that we only set the dictionary cache, when actually needed
          cardIds[idCard] = isChannelMappedToCard.GetValueOrDefault();
          _channelMapping[idChannel] = cardIds;          
        }        
      }
      return isChannelMappedToCard.GetValueOrDefault();
    }



    public static bool IsChannelMappedToCard(int idChannel, int idCard)
    {
      bool isChannelMappedToCard = UpdateCacheWithChannelMapForChannel(idChannel, idCard);
      return isChannelMappedToCard;      
    }

  }
}
