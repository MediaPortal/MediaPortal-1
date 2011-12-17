using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class ChannelGroupRepository : GenericRepository<Model>, IChannelGroupRepository
  {
    public ChannelGroupRepository()    
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
  }
}
