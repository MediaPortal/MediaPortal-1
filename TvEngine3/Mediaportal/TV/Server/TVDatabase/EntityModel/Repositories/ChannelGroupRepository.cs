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
  public class ChannelGroupRepository : GenericRepository<Model>, IChannelGroupRepository
  {
    public ChannelGroupRepository()    
    {
    }

    public ChannelGroupRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {      
    }

    public ChannelGroupRepository(Model context)
      : base(context)
    {
    }


    public IQueryable<ChannelGroup> IncludeAllRelations(IQueryable<ChannelGroup> query)
    {
      var includeRelations = query.
        Include(r => r.GroupMaps).
        Include(r => r.KeywordMap).
        Include(r => r.GroupMaps.Select(c => c.Channel)).
        Include(r => r.GroupMaps.Select(c => c.Channel.TuningDetails));
      return includeRelations;
    }

    public IQueryable<ChannelGroup> IncludeAllRelations(IQueryable<ChannelGroup> query, ChannelGroupIncludeRelationEnum includeRelations)
    {
      bool groupMaps = (includeRelations & ChannelGroupIncludeRelationEnum.GroupMaps) == ChannelGroupIncludeRelationEnum.GroupMaps;
      bool groupMapsChannel = (includeRelations & ChannelGroupIncludeRelationEnum.GroupMapsChannel) == ChannelGroupIncludeRelationEnum.GroupMapsChannel;
      bool groupMapsTuningDetails = (includeRelations & ChannelGroupIncludeRelationEnum.GroupMapsTuningDetails) == ChannelGroupIncludeRelationEnum.GroupMapsTuningDetails;
      bool keywordMap = (includeRelations & ChannelGroupIncludeRelationEnum.KeywordMap) == ChannelGroupIncludeRelationEnum.KeywordMap;

      if (groupMaps)
      {
        query = query.Include(r => r.GroupMaps);
      }

      if (keywordMap)
      {
        query = query.Include(r => r.KeywordMap);
      }

      if (groupMapsChannel)
      {
        query = query.Include(r => r.GroupMaps.Select(c => c.Channel));
      }

      if (groupMapsTuningDetails)
      {
        query = query.Include(r => r.GroupMaps.Select(c => c.Channel.TuningDetails));
      }

      return query;
    }
  }
}
