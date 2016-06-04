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
      //if (channelMapsTuner)
      //{      
      //query = query.Include(c => c.ChannelMaps.Select(tuner => tuner.Tuner));
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

    private IDictionary<int, Tuner> GetTunersDictionary()
    {
      List<Tuner> tuners = GetAll<Tuner>().ToList();
      IDictionary<int, Tuner> tunersDict = new Dictionary<int, Tuner>();
      foreach (Tuner tuner in tuners)
      {
        tunersDict.Add(tuner.IdTuner, tuner);
      }
      return tunersDict;
    }

    public Channel LoadNavigationProperties(Channel channel)
    {
      return LoadNavigationProperties(channel, GetAllRelationsForChannel());
    }

    public IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels)
    {
      return LoadNavigationProperties(channels, GetAllRelationsForChannel());
    }

    public IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels, ChannelIncludeRelationEnum includeRelations)
    {
      bool channelMapsTuner = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMapsTuner);
      bool groupMapsChannelGroup = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMapsChannelGroup);

      IList<Channel> list = channels.ToList(); //fetch the basic/incomplete result from DB now.

      IDictionary<int, Tuner> tunerDict = null;
      IDictionary<int, ChannelGroup> groupDict = null;

      if (channelMapsTuner)
      {
        tunerDict = GetTunersDictionary();
      }
      if (groupMapsChannelGroup)
      {
        groupDict = GetChannelGroupsDictionary();
      }

      //now attach missing relations for tuningdetail - done in order to speed up query                    
      foreach (Channel channel in list)
      {
        if (channelMapsTuner)
        {
          foreach (var channelMap in channel.ChannelMaps)
          {
            LoadChannelMap(tunerDict, channelMap);
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
      bool channelMapsTuner = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMapsTuner);
      bool groupMapsChannelGroup = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMapsChannelGroup);

      IDictionary<int, Tuner> tunerDict = null;
      IDictionary<int, ChannelGroup> groupDict = null;

      if (channelMapsTuner)
      {
        tunerDict = GetTunersDictionary();
      }
      if (groupMapsChannelGroup)
      {
        groupDict = GetChannelGroupsDictionary();
      }

      ThreadHelper.ParallelInvoke(
        () =>
        {
          if (channelMapsTuner)
          {
            foreach (ChannelMap channelMap in channel.ChannelMaps)
            {
              LoadChannelMap(tunerDict, channelMap);
            }
            //Parallel.ForEach(channel.ChannelMaps, (channelMap) => LoadChannelMap(tunerDict, channelMap));
          }
        },
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

    private static void LoadChannelMap(IDictionary<int, Tuner> tunerDict, ChannelMap channelmap)
    {
      if (channelmap.IdTuner > 0)
      {
        Tuner tuner;
        if (tunerDict.TryGetValue(channelmap.IdTuner, out tuner))
        {
          channelmap.Tuner = tuner;
        }
      }
    }

    public IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query)
    {
      return IncludeAllRelations(query, GetAllRelationsForChannel());
    }

    private static ChannelIncludeRelationEnum GetAllRelationsForChannel()
    {
      ChannelIncludeRelationEnum include = ChannelIncludeRelationEnum.TuningDetails;
      include |= ChannelIncludeRelationEnum.ChannelMapsTuner;
      include |= ChannelIncludeRelationEnum.GroupMaps;
      include |= ChannelIncludeRelationEnum.GroupMapsChannelGroup;
      include |= ChannelIncludeRelationEnum.ChannelMaps;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal;
      return include;
    }
  }
}