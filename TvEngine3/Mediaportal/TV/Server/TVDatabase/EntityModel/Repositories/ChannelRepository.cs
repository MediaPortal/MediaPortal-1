using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

    public IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query, ChannelRelation includeRelations)
    {
      if (includeRelations.HasFlag(ChannelRelation.Recordings))
      {
        query = query.Include(c => c.Recordings);
      }

      //todo: move to LoadNavigationProperties for performance improvement
      if (includeRelations.HasFlag(ChannelRelation.ChannelLinkMapsChannelLink))
      {
        query = query.Include(c => c.ChannelLinkMaps.Select(l => l.ChannelLink));
      }

      //todo: move to LoadNavigationProperties for performance improvement
      if (includeRelations.HasFlag(ChannelRelation.ChannelLinkMapsChannelPortal))
      {
        query = query.Include(c => c.ChannelLinkMaps.Select(l => l.ChannelPortal));
      }

      if (includeRelations.HasFlag(ChannelRelation.ChannelMaps))
      {
        query = query.Include(c => c.ChannelMaps);
      }

      //too slow, handle in LoadNavigationProperties instead
      //if (channelMapsTuner)
      //{      
      //query = query.Include(c => c.ChannelMaps.Select(tuner => tuner.Tuner));
      //}
      if (includeRelations.HasFlag(ChannelRelation.GroupMaps))
      {
        query = query.Include(c => c.GroupMaps);
      }

      //too slow, handle in LoadNavigationProperties instead
      //if (groupMapsChannelGroup)
      //{
      //  query = query.Include(c => c.GroupMaps.Select(g => g.ChannelGroup));
      //}

      if (includeRelations.HasFlag(ChannelRelation.TuningDetails))
      {
        query = query.Include(c => c.TuningDetails);
      }

      return query;
    }

    public IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels, ChannelRelation includeRelations)
    {
      bool channelMapsTuner = includeRelations.HasFlag(ChannelRelation.ChannelMapsTuner);
      bool groupMapsChannelGroup = includeRelations.HasFlag(ChannelRelation.GroupMapsChannelGroup);
      bool tuningDetails = includeRelations.HasFlag(ChannelRelation.TuningDetails);

      IList<Channel> list = channels.ToList(); //fetch the basic/incomplete result from DB now.

      IDictionary<int, Tuner> tuners = null;
      IDictionary<int, ChannelGroup> channelGroups = null;
      IDictionary<int, Satellite> satellites = null;

      if (channelMapsTuner)
      {
        List<Tuner> tempTuners = GetAll<Tuner>().ToList();
        tuners = new Dictionary<int, Tuner>();
        foreach (Tuner tuner in tempTuners)
        {
          tuners.Add(tuner.IdTuner, tuner);
        }
      }
      if (groupMapsChannelGroup)
      {
        List<ChannelGroup> tempChannelGroups = GetAll<ChannelGroup>().ToList();
        channelGroups = new Dictionary<int, ChannelGroup>();
        foreach (ChannelGroup group in tempChannelGroups)
        {
          channelGroups.Add(group.IdGroup, group);
        }
      }
      if (tuningDetails)
      {
        List<Satellite> tempSatellites = GetAll<Satellite>().ToList();
        satellites = new Dictionary<int, Satellite>();
        foreach (Satellite satellite in tempSatellites)
        {
          satellites.Add(satellite.IdSatellite, satellite);
        }
      }

      // Attach missing relations.
      // TODO further speed improvements by using Parallel/ThreadHelper?
      foreach (Channel channel in list)
      {
        if (channelMapsTuner)
        {
          foreach (ChannelMap channelMap in channel.ChannelMaps)
          {
            Tuner tuner;
            if (tuners.TryGetValue(channelMap.IdTuner, out tuner))
            {
              channelMap.Tuner = tuner;
            }
          }
        }
        if (groupMapsChannelGroup)
        {
          foreach (GroupMap groupMap in channel.GroupMaps)
          {
            ChannelGroup channelGroup;
            if (channelGroups.TryGetValue(groupMap.IdGroup, out channelGroup))
            {
              groupMap.ChannelGroup = channelGroup;
            }
          }
        }
        if (tuningDetails)
        {
          foreach (TuningDetail tuningDetail in channel.TuningDetails)
          {
            if (tuningDetail.IdSatellite.HasValue)
            {
              Satellite satellite;
              if (satellites.TryGetValue(tuningDetail.IdSatellite.Value, out satellite))
              {
                tuningDetail.Satellite = satellite;
              }
            }
          }
        }
      }
      return list;
    }

    public Channel LoadNavigationProperties(Channel channel, ChannelRelation includeRelations)
    {
      if (channel == null)
      {
        return null;
      }
      return LoadNavigationProperties(new Channel[1] { channel }, includeRelations)[0];
    }
  }
}