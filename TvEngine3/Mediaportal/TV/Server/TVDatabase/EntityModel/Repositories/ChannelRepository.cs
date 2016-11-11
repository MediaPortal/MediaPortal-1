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

      if (includeRelations.HasFlag(ChannelRelation.ChannelGroupMappings))
      {
        query = query.Include(c => c.ChannelGroupMappings);
      }
      //too slow, handle in LoadNavigationProperties instead
      //if (channelGroupMappingsChannelGroup)
      //{
      //  query = query.Include(c => c.ChannelGroupMappings.Select(m => m.ChannelGroup));
      //}

      if (includeRelations.HasFlag(ChannelRelation.TuningDetails))
      {
        query = query.Include(c => c.TuningDetails);
      }

      return query;
    }

    public IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels, ChannelRelation includeRelations)
    {
      bool channelGroupMappingsChannelGroup = includeRelations.HasFlag(ChannelRelation.ChannelGroupMappingsChannelGroup);
      bool tuningDetails = includeRelations.HasFlag(ChannelRelation.TuningDetails);

      IList<Channel> list = channels.ToList(); //fetch the basic/incomplete result from DB now.
      if (!channelGroupMappingsChannelGroup && !tuningDetails)
      {
        return list;
      }

      IDictionary<int, ChannelGroup> channelGroups = null;
      IDictionary<int, Satellite> satellites = null;

      if (channelGroupMappingsChannelGroup)
      {
        List<ChannelGroup> tempChannelGroups = GetAll<ChannelGroup>().ToList();
        channelGroups = new Dictionary<int, ChannelGroup>();
        foreach (ChannelGroup channelGroup in tempChannelGroups)
        {
          channelGroups.Add(channelGroup.IdChannelGroup, channelGroup);
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
        if (channelGroupMappingsChannelGroup)
        {
          foreach (ChannelGroupChannelMapping mapping in channel.ChannelGroupMappings)
          {
            ChannelGroup channelGroup;
            if (channelGroups.TryGetValue(mapping.IdChannelGroup, out channelGroup))
            {
              mapping.ChannelGroup = channelGroup;
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