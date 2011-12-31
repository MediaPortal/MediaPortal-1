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

    public ChannelRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<Channel> GetAllChannelsByGroupIdAndMediaType(int groupId, MediaTypeEnum mediaType)
    {
      IOrderedQueryable<Channel> channels = GetQuery<Channel>().Where(c => c.visibleInGuide && c.mediaType == (int)mediaType && c.GroupMaps.Count > 0 && c.GroupMaps.Any(gm => gm.mediaType == (int)mediaType && gm.ChannelGroup.idGroup == groupId)).OrderBy(c => c.sortOrder);
      return channels;  
    }

    public IQueryable<Channel> GetAllChannelsByGroupId(int groupId)
    {
      IOrderedQueryable<Channel> channels = GetQuery<Channel>().Where(c => c.visibleInGuide && c.GroupMaps.Count > 0 && c.GroupMaps.Any(gm => gm.ChannelGroup.idGroup == groupId)).OrderBy(c => c.sortOrder);
      return channels;     
    }

   

    public IQueryable<TuningDetail> IncludeAllRelations(IQueryable<TuningDetail> query)
    {
      IQueryable<TuningDetail> includeRelations =
       query.
         Include(c => c.Channel);

      return includeRelations;
    }
    

    public IQueryable<Channel> IncludeSelectRelations<T>(IQueryable<Channel> query,
                                                      Expression<Func<Channel, IEnumerable<T>>> inc)
    {
      IQueryable<Channel> includeRelations =
        query.
          Include(inc)
        ;
    

      //calling example
      Expression<Func<Channel, IEnumerable<TuningDetail>>> tuningDetail = c => c.TuningDetails;
      includeRelations = IncludeSelectRelations<TuningDetail>(includeRelations, tuningDetail);

      Expression<Func<Channel, IEnumerable<ChannelMap>>> channelMaps = c => c.ChannelMaps;
      includeRelations = IncludeSelectRelations<ChannelMap>(includeRelations, channelMaps);
      

      //Expression<Func<Channel, IEnumerable<ChannelGroup>>> expression = c => c.GroupMaps.Select(g => g.ChannelGroup);

      return includeRelations;
    }

    public IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query, ChannelIncludeRelationEnum includeRelations)
    {
      bool channelLinkMapsChannelLink = (includeRelations & ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink) == ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink;
      bool channelLinkMapsChannelPortal = (includeRelations & ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal) == ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal;
      bool channelMaps = (includeRelations & ChannelIncludeRelationEnum.ChannelMaps) == ChannelIncludeRelationEnum.ChannelMaps;
      bool channelMapsCard = (includeRelations & ChannelIncludeRelationEnum.ChannelMapsCard) == ChannelIncludeRelationEnum.ChannelMapsCard;
      bool groupMaps = (includeRelations & ChannelIncludeRelationEnum.GroupMaps) == ChannelIncludeRelationEnum.GroupMaps;
      bool groupMapsChannelGroup = (includeRelations & ChannelIncludeRelationEnum.GroupMapsChannelGroup) == ChannelIncludeRelationEnum.GroupMapsChannelGroup;
      bool tuningDetails = (includeRelations & ChannelIncludeRelationEnum.TuningDetails) == ChannelIncludeRelationEnum.TuningDetails;      
      
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
      if (tuningDetails)
      {
        query = query.Include(c => c.TuningDetails);
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
  }  
}
