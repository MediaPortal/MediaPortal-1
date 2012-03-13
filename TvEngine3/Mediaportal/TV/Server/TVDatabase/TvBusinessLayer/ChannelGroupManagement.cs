using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Channel = Mediaportal.TV.Server.TVDatabase.Entities.Channel;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ChannelGroupManagement
  {
    public static IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetAll<ChannelGroup>();
        var listAllChannelGroups = channelGroupRepository.IncludeAllRelations(query, includeRelations).ToList();              
        return listAllChannelGroups;
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroups()
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetAll<ChannelGroup>();
        var listAllChannelGroups = channelGroupRepository.IncludeAllRelations(query).ToList();
        return listAllChannelGroups;
      }
    }   

    public static IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query =
          channelGroupRepository.GetQuery<ChannelGroup>(g => g.GroupMaps.Any(gm => gm.mediaType == (int)mediaType));

        var listAllChannelGroupsByMediaType = channelGroupRepository.IncludeAllRelations(query).ToList();
        return listAllChannelGroupsByMediaType;
      }
    }

    public static IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType, ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query =
          channelGroupRepository.GetQuery<ChannelGroup>(g => g.GroupMaps.Any(gm => gm.mediaType == (int)mediaType));

        var listAllChannelGroupsByMediaType = channelGroupRepository.IncludeAllRelations(query, includeRelations).ToList();
        return listAllChannelGroupsByMediaType;
      }
    }

    public static ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaTypeEnum mediaType)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        var query = channelGroupRepository.GetQuery<ChannelGroup>(
          g => g.groupName == groupName && g.GroupMaps.Any(gm => gm.mediaType == (int) mediaType));

        ChannelGroup channelGroupByNameAndMediaType = channelGroupRepository.IncludeAllRelations(query).FirstOrDefault();
        return channelGroupByNameAndMediaType;
      }
    }    
    
    public static ChannelGroup GetOrCreateGroup(string groupName)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {
        ChannelGroup group = channelGroupRepository.Single<ChannelGroup>(g => g.groupName == groupName);
        if (group == null)
        {
          group = new ChannelGroup {groupName = groupName, sortOrder = 9999};
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
        channelGroupRepository.Delete<GroupMap>(g => g.idMap == idMap);
        channelGroupRepository.UnitOfWork.SaveChanges();
      }
    }

    public static ChannelGroup GetChannelGroup(int idGroup)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository())
      {        
        IQueryable<ChannelGroup> query = channelGroupRepository.GetQuery<ChannelGroup>(g => g.idGroup == idGroup);
        ChannelGroup group = channelGroupRepository.IncludeAllRelations(query).FirstOrDefault();
        return group;
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
        channelGroupRepository.Delete<ChannelGroup>(g => g.idGroup == idGroup);
        channelGroupRepository.UnitOfWork.SaveChanges();
      }      
    }

    public static IList<ChannelGroup> ListAllCustomChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      using (IChannelGroupRepository channelGroupRepository = new ChannelGroupRepository(true))
      {
        var query = channelGroupRepository.GetQuery<ChannelGroup>(g => g.groupName != TvConstants.TvGroupNames.AllChannels && g.groupName != TvConstants.RadioGroupNames.AllChannels);
        var listAllChannelGroups = channelGroupRepository.IncludeAllRelations(query, includeRelations).ToList();
        return listAllChannelGroups;
      }
    }
  }
}
