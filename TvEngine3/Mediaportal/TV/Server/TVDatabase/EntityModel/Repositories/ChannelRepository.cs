using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
      IQueryable<TuningDetail> includeRelations = query.Include(c => c.Channel).
        Include(c => c.Channel.GroupMaps);      
      return includeRelations;
    }    

    public IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query, ChannelIncludeRelationEnum includeRelations)
    {      
      bool channelLinkMapsChannelLink = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink);
      bool channelLinkMapsChannelPortal = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal);
      bool channelMaps = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMaps);
      bool channelMapsCard = includeRelations.HasFlag(ChannelIncludeRelationEnum.ChannelMapsCard);
      bool groupMaps = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMaps);
      bool groupMapsChannelGroup = includeRelations.HasFlag(ChannelIncludeRelationEnum.GroupMapsChannelGroup);
      bool tuningDetails = includeRelations.HasFlag(ChannelIncludeRelationEnum.TuningDetails);

      if (tuningDetails)
      {
        query = query.Include(c => c.TuningDetails);
      }

      if (channelLinkMapsChannelLink)
      {
        query = query.Include(c => c.ChannelLinkMaps.Select(l => l.ChannelLink));
      }
      if (channelLinkMapsChannelPortal)
      {
        query = query.Include(c => c.ChannelLinkMaps.Select(l => l.ChannelPortal));
      }
      if (channelMaps)
      {
        query = query.Include(c => c.ChannelMaps);
      }
      if (channelMapsCard)
      {
        query = query.Include(c => c.ChannelMaps.Select(card => card.Card));
      }
      if (groupMaps)
      {
        query = query.Include(c => c.GroupMaps);
      }
      if (groupMapsChannelGroup)
      {
        query = query.Include(c => c.GroupMaps.Select(g => g.ChannelGroup));
      }      

      return query;
    }


    public IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query)
    {
      IQueryable<Channel> includeRelations =
        query.          
          Include(c => c.TuningDetails).
          Include(c => c.ChannelMaps).
          Include(c => c.ChannelMaps.Select(card => card.Card)).
          Include(c => c.GroupMaps).
          Include(c => c.GroupMaps.Select(g => g.ChannelGroup)).
          Include(c => c.ChannelLinkMaps.Select(l => l.ChannelLink)).
          Include(c => c.ChannelLinkMaps.Select(l => l.ChannelPortal))          
        ;   

      return includeRelations;
    }

    public IQueryable<ChannelMap> IncludeAllRelations(IQueryable<ChannelMap> query)
    {
      IQueryable<ChannelMap> includeRelations =
        query.
          Include(c => c.Channel).
          Include(c => c.Card)
        ;

      return includeRelations;
    }
  }  
}
