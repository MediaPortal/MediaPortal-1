using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ChannelGroupManagement
  {
    public static IList<ChannelGroup> ListAllChannelGroups(ChannelGroupRelation includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetAll<ChannelGroup>().OrderBy(cg => cg.SortOrder);
        return channelGroupRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType, ChannelGroupRelation includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetQuery<ChannelGroup>(cg => cg.MediaType == (int)mediaType).OrderBy(g => g.SortOrder);
        return channelGroupRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static ChannelGroup GetChannelGroup(int idChannelGroup, ChannelGroupRelation includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        IQueryable<ChannelGroup> query = channelGroupRepository.GetQuery<ChannelGroup>(cg => cg.IdGroup == idChannelGroup);
        return channelGroupRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
      }
    }

    public static ChannelGroup GetOrCreateChannelGroup(string name, MediaType mediaType)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        ChannelGroup group = channelGroupRepository.Single<ChannelGroup>(cg => cg.GroupName == name && cg.MediaType == (int)mediaType);
        if (group == null)
        {
          group = new ChannelGroup { GroupName = name, SortOrder = 9999, MediaType = (int)mediaType };
          channelGroupRepository.Add(group);
          channelGroupRepository.UnitOfWork.SaveChanges();
        }
        return group;
      }
    }

    public static ChannelGroup SaveChannelGroup(ChannelGroup channelGroup)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        channelGroupRepository.AttachEntityIfChangeTrackingDisabled(channelGroupRepository.ObjectContext.ChannelGroups, channelGroup);
        channelGroupRepository.ApplyChanges(channelGroupRepository.ObjectContext.ChannelGroups, channelGroup);
        channelGroupRepository.UnitOfWork.SaveChanges();
        channelGroup.AcceptChanges();
        return channelGroup;
      }
    }

    public static void DeleteChannelGroup(int idChannelGroup)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository(true))
      {
        IQueryable <ChannelGroup> query = channelGroupRepository.GetQuery<ChannelGroup>(g => g.IdGroup == idChannelGroup);
        ChannelGroup group = channelGroupRepository.IncludeAllRelations(query, ChannelGroupRelation.GroupMaps).FirstOrDefault();
        if (group.GroupMaps.Count > 0)
        {
          foreach (GroupMap mapping in group.GroupMaps)
          {
            mapping.ChangeTracker.State = ObjectState.Deleted;
          }
          channelGroupRepository.ApplyChanges(channelGroupRepository.ObjectContext.ChannelGroups, group);
        }
        channelGroupRepository.Delete<ChannelGroup>(cg => cg.IdGroup == idChannelGroup);
        channelGroupRepository.UnitOfWork.SaveChanges();
      }
    }
  }
}