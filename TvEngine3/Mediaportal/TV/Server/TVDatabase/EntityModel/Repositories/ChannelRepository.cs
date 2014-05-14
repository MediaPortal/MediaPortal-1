using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Mediaportal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{

  public class ChannelRepository : GenericRepository<Model>, IChannelRepository
  {
    public ChannelRepository()
    {
    }

    public ChannelRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public ChannelRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<Channel> GetAllChannelsByGroupIdAndMediaType(int groupId, MediaTypeEnum mediaType)
    {
      IQueryable<Channel> channels = GetQuery<GroupMap>().Where(gm => gm.IdGroup == groupId && gm.Channel.VisibleInGuide && gm.Channel.MediaType == (int)mediaType).OrderBy(gm => gm.SortOrder).Select(gm => gm.Channel);
      return channels;
    }

    public IQueryable<Channel> GetAllChannelsByGroupId(int groupId)
    {
      IOrderedQueryable<Channel> channels = GetQuery<Channel>().Where(c => c.VisibleInGuide && c.GroupMaps.Count > 0 && c.GroupMaps.Any(gm => gm.ChannelGroup.IdGroup == groupId)).OrderBy(c => c.SortOrder);
      return channels;
    }

    public IQueryable<TuningDetail> IncludeAllRelations(IQueryable<TuningDetail> query)
    {
      IQueryable<TuningDetail> includeRelations = query.Include(c => c.Channel).Include(c => c.Channel.GroupMaps);
      return includeRelations;
    }

    public IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query, ChannelIncludeRelationEnum includeRelations)
    {
      bool channelLinkMapsChannelLink = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink);
      bool channelLinkMapsChannelPortal = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal);
      bool channelMaps = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMaps);
      bool groupMaps = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMaps);
      bool tuningDetails = includeRelations.HasFlag(ChannelIncludeRelationEnum.TuningDetails);
      bool recordings = includeRelations.HasFlag(ChannelIncludeRelationEnum.Recordings);

      if (recordings)
      {
        query = query.Include(c => c.Recordings);
      }

      //todo: move to LoadNavigationProperties for performance improvement
      if (channelLinkMapsChannelLink)
      {
        query = query.Include(c => c.ChannelLinkMaps.Select(l => l.ChannelLink));
      }

      //todo: move to LoadNavigationProperties for performance improvement
      if (channelLinkMapsChannelPortal)
      {
        query = query.Include(c => c.ChannelLinkMaps.Select(l => l.ChannelPortal));
      }

      if (channelMaps)
      {
        query = query.Include(c => c.ChannelMaps);
      }

      //too slow, handle in LoadNavigationProperties instead
      //if (channelMapsCard)
      //{      
      //query = query.Include(c => c.ChannelMaps.Select(card => card.Card));
      //}
      if (groupMaps)
      {
        query = query.Include(c => c.GroupMaps);
      }

      //too slow, handle in LoadNavigationProperties instead
      //if (groupMapsChannelGroup)
      //{
      //  query = query.Include(c => c.GroupMaps.Select(g => g.ChannelGroup));
      //}

      if (tuningDetails)
      {
        query = query.Include(c => c.TuningDetails);
      }

      return query;
    }

    private IDictionary<int, ChannelGroup> GetChannelGroupsDictionary()
    {
      List<ChannelGroup> groups = GetAll<ChannelGroup>().ToList();
      IDictionary<int, ChannelGroup> groupsDict = new Dictionary<int, ChannelGroup>();
      foreach (ChannelGroup group in groups)
      {
        groupsDict.Add(group.IdGroup, group);
      }
      return groupsDict;
    }

    private IDictionary<int, Card> GetCardsDictionary()
    {
      List<Card> cards = GetAll<Card>().ToList();
      IDictionary<int, Card> cardsDict = new Dictionary<int, Card>();
      foreach (Card card in cards)
      {
        cardsDict.Add(card.IdCard, card);
      }
      return cardsDict;
    }

    private IDictionary<int, LnbType> GetLnbTypesDictionary()
    {
      List<LnbType> lnbTypes = GetAll<LnbType>().ToList();
      IDictionary<int, LnbType> lnbTypesDict = new Dictionary<int, LnbType>();
      foreach (LnbType lnbType in lnbTypes)
      {
        lnbTypesDict.Add(lnbType.IdLnbType, lnbType);
      }
      return lnbTypesDict;
    }

    public Channel LoadNavigationProperties(Channel channel)
    {
      ChannelIncludeRelationEnum includeRelations = GetAllRelationsForChannel();
      return LoadNavigationProperties(channel, includeRelations);
    }

    public IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels)
    {
      ChannelIncludeRelationEnum includeRelations = GetAllRelationsForChannel();
      return LoadNavigationProperties(channels, includeRelations);
    }

    public IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels, ChannelIncludeRelationEnum includeRelations)
    {
      bool tuningDetails = includeRelations.HasFlag(ChannelIncludeRelationEnum.TuningDetails);
      bool channelMapsCard = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMapsCard);
      bool groupMapsChannelGroup = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMapsChannelGroup);

      IList<Channel> list = channels.ToList(); //fetch the basic/incomplete result from DB now.

      IDictionary<int, LnbType> lnbTypesDict = null;
      IDictionary<int, Card> cardDict = null;
      IDictionary<int, ChannelGroup> groupDict = null;

      if (tuningDetails)
      {
        lnbTypesDict = GetLnbTypesDictionary();
      }
      if (channelMapsCard)
      {
        cardDict = GetCardsDictionary();
      }

      if (groupMapsChannelGroup)
      {
        groupDict = GetChannelGroupsDictionary();
      }

      //now attach missing relations for tuningdetail - done in order to speed up query                    
      foreach (Channel channel in list)
      {
        if (tuningDetails)
        {
          foreach (var tuningDetail in channel.TuningDetails)
          {
            LoadTuningDetail(lnbTypesDict, tuningDetail);
          }
        }
        if (channelMapsCard)
        {
          foreach (var channelMap in channel.ChannelMaps)
          {
            LoadChannelMap(cardDict, channelMap);
          }
        }

        if (groupMapsChannelGroup)
        {
          foreach (GroupMap groupMap in channel.GroupMaps)
          {
            LoadGroupMap(groupDict, groupMap);
          }
        }
      }
      return list;
    }

    public Channel LoadNavigationProperties(Channel channel, ChannelIncludeRelationEnum includeRelations)
    {
      bool tuningDetails = includeRelations.HasFlag(ChannelIncludeRelationEnum.TuningDetails);
      bool channelMapsCard = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMapsCard);
      bool groupMapsChannelGroup = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMapsChannelGroup);

      IDictionary<int, LnbType> lnbTypesDict = null;
      IDictionary<int, Card> cardDict = null;
      IDictionary<int, ChannelGroup> groupDict = null;

      if (tuningDetails)
      {
        lnbTypesDict = GetLnbTypesDictionary();
      }
      if (channelMapsCard)
      {
        cardDict = GetCardsDictionary();
      }

      if (groupMapsChannelGroup)
      {
        groupDict = GetChannelGroupsDictionary();
      }

      ThreadHelper.ParallelInvoke(
        () =>
        {
          if (tuningDetails)
          {
            //now attach missing relations for tuningdetail - done in order to speed up query                    
            foreach (TuningDetail tuningDetail in channel.TuningDetails)
            {
              LoadTuningDetail(lnbTypesDict, tuningDetail);
            }
            //Parallel.ForEach(channel.TuningDetails, (tuningDetail) => LoadTuningDetail(lnbTypesDict, tuningDetail));
          }
        }
          ,
          () =>
          {
            if (channelMapsCard)
            {
              foreach (ChannelMap channelMap in channel.ChannelMaps)
              {
                LoadChannelMap(cardDict, channelMap);
              }
              //Parallel.ForEach(channel.ChannelMaps, (channelMap) => LoadChannelMap(cardDict, channelMap));
            }
          }
          ,
          () =>
          {
            if (groupMapsChannelGroup)
            {
              foreach (GroupMap groupMap in channel.GroupMaps)
              {
                LoadGroupMap(groupDict, groupMap);
              }
            }
          }
        );
      return channel;
    }

    private static void LoadGroupMap(IDictionary<int, ChannelGroup> groupDict, GroupMap groupMap)
    {
      if (groupMap.IdGroup > 0)
      {
        ChannelGroup group;
        if (groupDict.TryGetValue(groupMap.IdGroup, out group))
        {
          groupMap.ChannelGroup = group;
        }
      }
    }

    private static void LoadChannelMap(IDictionary<int, Card> cardDict, ChannelMap channelmap)
    {
      if (channelmap.IdCard > 0)
      {
        Card card;
        if (cardDict.TryGetValue(channelmap.IdCard, out card))
        {
          channelmap.Card = card;
        }
      }
    }

    private static void LoadTuningDetail(IDictionary<int, LnbType> lnbTypesDict, TuningDetail tuningDetail)
    {
      if (tuningDetail.IdLnbType.HasValue)
      {
        LnbType lnbType;
        if (
          lnbTypesDict.TryGetValue(
            tuningDetail.IdLnbType.Value,
            out lnbType))
        {
          tuningDetail.LnbType = lnbType;
        }
      }
    }

    public IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query)
    {
      ChannelIncludeRelationEnum include = GetAllRelationsForChannel();
      return IncludeAllRelations(query, include);
    }

    private static ChannelIncludeRelationEnum GetAllRelationsForChannel()
    {
      ChannelIncludeRelationEnum include = ChannelIncludeRelationEnum.TuningDetails;
      include |= ChannelIncludeRelationEnum.ChannelMapsCard;
      include |= ChannelIncludeRelationEnum.GroupMaps;
      include |= ChannelIncludeRelationEnum.GroupMapsChannelGroup;
      include |= ChannelIncludeRelationEnum.ChannelMaps;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal;
      return include;
    }

    public IQueryable<ChannelMap> IncludeAllRelations(IQueryable<ChannelMap> query)
    {
      IQueryable<ChannelMap> includeRelations =
        query.
          Include(c => c.Channel).
          Include(c => c.Card);

      return includeRelations;
    }
  }
}