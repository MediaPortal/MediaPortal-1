using System.Collections.Generic;
using System.Linq;
using Mediaportal.Common.Utils;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

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
      TuningDetailManagement.OnStateChangedTuningDetailEvent += new TuningDetailManagement.OnStateChangedTuningDetailDelegate(ChannelManagement_OnStateChangedTuningDetailEvent);
      ChannelManagement.OnStateChangedChannelMapEvent += new ChannelManagement.OnStateChangedChannelMapDelegate(ChannelManagement_OnStateChangedChannelMapEvent);
      
      var allCardIds = new List<int>();
      IList<Channel> channels = null;
      ThreadHelper.ParallelInvoke(
        () =>
          {
            IList<Tuner> cards = TVDatabase.TVBusinessLayer.TunerManagement.ListAllTuners(TunerIncludeRelationEnum.None);
            allCardIds.AddRange(cards.Select(card => card.IdTuner));
          },
          () =>
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
            IOrderedEnumerable<TuningDetail> tuningDetails = channel.TuningDetails.OrderBy(td => td.Priority);
            IList<IChannel> tuningChannels = new List<IChannel>(10);
            foreach (TuningDetail tuningDetail in tuningDetails)
            {
              IChannel tuningChannel = TuningDetailManagement.GetTuningChannel(tuningDetail);
              IChannelSatellite satelliteChannel = tuningChannel as IChannelSatellite;
              if (satelliteChannel != null)
              {
                satelliteChannel.LnbType = new LnbTypeBLL(tuningDetail.LnbType);
              }
              tuningChannels.Add(tuningChannel);
            }
            _tuningChannelMapping[channel.IdChannel] = tuningChannels;

            IDictionary<int, bool> mapDict = new Dictionary<int, bool>();
            var copyAllCardIds = new List<int>(allCardIds);
            foreach (ChannelMap map in channel.ChannelMaps)
            {
              mapDict[map.IdTuner] = false;
              copyAllCardIds.Remove(map.IdTuner);
            }

            foreach (int cardIdMapped in copyAllCardIds)
            {
              mapDict[cardIdMapped] = true;
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
        UpdateCacheWithChannelMapForChannel(map.IdChannel, map.IdTuner, true);
      }
      else if (state == ObjectState.Added)
      {
        UpdateCacheWithChannelMapForChannel(map.IdChannel, map.IdTuner, false);
      }
    }

    private static void ChannelManagement_OnStateChangedTuningDetailEvent(int channelId)
    {
      if (channelId > 0)
      {
        UpdateCacheWithTuningDetailsForChannel(channelId);
      }
    }

    private static IList<IChannel> UpdateCacheWithTuningDetailsForChannel(int channelId)
    {
      IOrderedEnumerable<TuningDetail> tuningDetails = ChannelManagement.GetChannel(channelId, ChannelIncludeRelationEnum.TuningDetails).TuningDetails.OrderBy(td => td.Priority);
      IList<IChannel> tuningChannels = new List<IChannel>(10);
      foreach (TuningDetail tuningDetail in tuningDetails)
      {
        IChannel tuningChannel = TuningDetailManagement.GetTuningChannel(tuningDetail);
        IChannelSatellite satelliteChannel = tuningChannel as IChannelSatellite;
        if (satelliteChannel != null)
        {
          satelliteChannel.LnbType = new LnbTypeBLL(tuningDetail.LnbType);
        }
        tuningChannels.Add(tuningChannel);
      }
      lock (_tuningChannelMappingLock)
      {
        _tuningChannelMapping[channelId] = tuningChannels;
      }
      return tuningChannels;
    }

    public static IList<IChannel> GetTuningDetailsByChannelId(int channelId)
    {
      IList<IChannel> tuningDetails;
      bool tuningChannelMappingFound;

      lock (_tuningChannelMappingLock)
      {
        tuningChannelMappingFound = _tuningChannelMapping.TryGetValue(channelId, out tuningDetails);
      }

      if (!tuningChannelMappingFound)
      {
        tuningDetails = UpdateCacheWithTuningDetailsForChannel(channelId);        
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
          //check if channel is mapped to this card
          if (cardIds == null)
          {
            cardIds = new Dictionary<int, bool>();
          }
          if (!isChannelMappedToCard.HasValue)
          {
            isChannelMappedToCard = ChannelManagement.IsChannelMappedToTuner(idChannel, idCard);
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