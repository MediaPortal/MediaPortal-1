using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ChannelGroupManagement
  {
    public static IList<ChannelGroup> ListAllChannelGroups()
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetAll<ChannelGroup>();
        return channelGroupRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetAll<ChannelGroup>();
        return channelGroupRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetQuery<ChannelGroup>(g => g.MediaType == (int)mediaType);
        return channelGroupRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType, ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetQuery<ChannelGroup>(g => g.MediaType == (int)mediaType);
        return channelGroupRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaTypeEnum mediaType)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetQuery<ChannelGroup>(
          g => g.GroupName == groupName && g.MediaType == (int)mediaType);
        return channelGroupRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }    
    
    public static ChannelGroup GetOrCreateGroup(string groupName, MediaTypeEnum mediaType)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        ChannelGroup group = channelGroupRepository.Single<ChannelGroup>(g => g.GroupName == groupName && g.MediaType == (int)mediaType);
        if (group == null)
        {
          group = new ChannelGroup {GroupName = groupName, SortOrder = 9999, MediaType = (int)mediaType};
          channelGroupRepository.Add(group);
          channelGroupRepository.UnitOfWork.SaveChanges();
        }
        return group;
      }
    }

    public static void DeleteChannelGroupMap(int idMap)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository(true))
      {
        channelGroupRepository.Delete<GroupMap>(g => g.IdMap == idMap);
        channelGroupRepository.UnitOfWork.SaveChanges();
      }
    }

    public static ChannelGroup GetChannelGroup(int idGroup)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {        
        IQueryable<ChannelGroup> query = channelGroupRepository.GetQuery<ChannelGroup>(g => g.IdGroup == idGroup);
        return channelGroupRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static ChannelGroup SaveGroup(ChannelGroup group)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        channelGroupRepository.AttachEntityIfChangeTrackingDisabled(channelGroupRepository.ObjectContext.ChannelGroups, group);
        channelGroupRepository.ApplyChanges(channelGroupRepository.ObjectContext.ChannelGroups, group);
        channelGroupRepository.UnitOfWork.SaveChanges();
        group.AcceptChanges();
        return group;
      }
    }

    public static void DeleteChannelGroup(int idGroup)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository(true))
      {
        IQueryable <ChannelGroup> query = channelGroupRepository.GetQuery<ChannelGroup>(g => g.IdGroup == idGroup);
        ChannelGroup group = channelGroupRepository.IncludeAllRelations(query, ChannelGroupIncludeRelationEnum.GroupMaps).FirstOrDefault();
        if (group.GroupMaps.Count > 0)
        {
          foreach (GroupMap groupmap in group.GroupMaps)
          {
            groupmap.ChangeTracker.State = ObjectState.Deleted;
          }
          channelGroupRepository.ApplyChanges(channelGroupRepository.ObjectContext.ChannelGroups, group);
        }
        channelGroupRepository.Delete<ChannelGroup>(g => g.IdGroup == idGroup);
        channelGroupRepository.UnitOfWork.SaveChanges();
      }
    }
  }
}