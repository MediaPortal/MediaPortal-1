using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ChannelManagement
  {
    public delegate void OnStateChangedChannelMapDelegate(ChannelMap map, ObjectState state);
    public static event OnStateChangedChannelMapDelegate OnStateChangedChannelMapEvent;

    public static IList<Channel> ListAllChannels()
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetAll<Channel>().OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query);
        return channelRepository.LoadNavigationProperties(query);
      }
    }

    public static IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetAll<Channel>().OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static IList<Channel> ListAllChannelsByGroupId(int idGroup)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>().Where(c => c.GroupMaps.Count > 0 && c.GroupMaps.Any(gm => gm.IdGroup == idGroup));
        query = channelRepository.IncludeAllRelations(query);
        return channelRepository.LoadNavigationProperties(query).OrderBy(c => c.GroupMaps.FirstOrDefault(gm => gm.IdGroup == idGroup).SortOrder).ToList();
      }
    }

    public static IList<Channel> ListAllChannelsByGroupId(int idGroup, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        includeRelations |= ChannelIncludeRelationEnum.GroupMaps;
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>().Where(c => c.GroupMaps.Count > 0 && c.GroupMaps.Any(gm => gm.IdGroup == idGroup));
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations).OrderBy(c => c.GroupMaps.FirstOrDefault(gm => gm.IdGroup == idGroup).SortOrder).ToList();
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByGroupId(int idGroup)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>().Where(c => c.VisibleInGuide && c.GroupMaps.Count > 0 && c.GroupMaps.Any(gm => gm.IdGroup == idGroup));
        query = channelRepository.IncludeAllRelations(query);
        return channelRepository.LoadNavigationProperties(query).OrderBy(c => c.GroupMaps.FirstOrDefault(gm => gm.IdGroup == idGroup).SortOrder).ToList();
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByGroupId(int idGroup, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        includeRelations |= ChannelIncludeRelationEnum.GroupMaps;
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>().Where(c => c.VisibleInGuide && c.GroupMaps.Count > 0 && c.GroupMaps.Any(gm => gm.IdGroup == idGroup));
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations).OrderBy(c => c.GroupMaps.FirstOrDefault(gm => gm.IdGroup == idGroup).SortOrder).ToList();
      }
    }

    public static IList<Channel> ListAllChannelsByMediaType(MediaType mediaType)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.MediaType == (int)mediaType).OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query);
        return channelRepository.LoadNavigationProperties(query);
      }
    }

    public static IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.MediaType == (int)mediaType).OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.VisibleInGuide && c.MediaType == (int)mediaType).OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query);
        return channelRepository.LoadNavigationProperties(query);
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.VisibleInGuide && c.MediaType == (int)mediaType).OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static Channel GetChannel(int idChannel)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.IdChannel == idChannel);
        Channel channel = channelRepository.IncludeAllRelations(query).FirstOrDefault();
        channel = channelRepository.LoadNavigationProperties(channel);
        return channel;
      }
    }

    public static Channel GetChannel(int idChannel, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.IdChannel == idChannel);
        Channel channel = channelRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
        channel = channelRepository.LoadNavigationProperties(channel, includeRelations);
        return channel;
      }
    }

    public static IList<Channel> GetChannelsByName(string channelName)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.Name == channelName);
        query = channelRepository.IncludeAllRelations(query);
        return channelRepository.LoadNavigationProperties(query);
      }
    }

    public static IList<Channel> GetChannelsByName(string channelName, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.Name == channelName);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static Channel SaveChannel(Channel channel)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        // TODO should channel map and tuning detail change events should be triggered here?
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Channels, channel);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Channels, channel);
        channelRepository.UnitOfWork.SaveChanges();
        channel.AcceptChanges();
        return channel;
      }
    }

    public static IList<Channel> SaveChannels(IEnumerable<Channel> channels)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        // TODO should channel map and tuning detail change events should be triggered here?
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Channels, channels);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Channels, channels);
        channelRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //channelRepository.ObjectContext.AcceptAllChanges();
        foreach (Channel channel in channels)
        {
          channel.AcceptChanges();
        }
        return channels.ToList();
      }
    }

    public static void DeleteChannel(int idChannel)
    {
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        ClearChannelRecordingForeignKeys(idChannel, channelRepository);

        // Need to manually delete tuning details and channel maps to trigger
        // events as required.
        Channel channel = GetChannel(idChannel, ChannelIncludeRelationEnum.ChannelMaps | ChannelIncludeRelationEnum.TuningDetails);
        foreach (TuningDetail td in channel.TuningDetails)
        {
          TuningDetailManagement.DeleteTuningDetail(td.IdTuning);
        }
        foreach (ChannelMap map in channel.ChannelMaps)
        {
          DeleteChannelMap(map.IdChannelMap);
        }

        channelRepository.Delete<Channel>(p => p.IdChannel == idChannel);

        /*Channel ch = new Channel();
        ch.idChannel = idChannel;

        channelRepository.ObjectContext.AttachTo("Channels", ch);
        channelRepository.ObjectContext.DeleteObject(ch);
        */
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    public static Channel MergeChannels(IEnumerable<Channel> channels, ChannelIncludeRelationEnum includeRelations)
    {
      // Merge into the channel with the most program records.
      int maxProgramCount = 0;
      Channel bestChannel = null;
      foreach (Channel channel in channels)
      {
        using (IChannelRepository channelRepository = new ChannelRepository())
        {
          int programCount = channelRepository.GetQuery<Program>().Where(p => p.IdChannel == channel.IdChannel).Count();
          if (bestChannel == null || programCount > maxProgramCount)
          {
            bestChannel = channel;
          }
        }
      }

      bestChannel = GetChannel(bestChannel.IdChannel, ChannelIncludeRelationEnum.ChannelMaps | ChannelIncludeRelationEnum.GroupMaps | ChannelIncludeRelationEnum.TuningDetails);
      int nextTuningDetailPriority = bestChannel.TuningDetails.Count + 1;
      HashSet<int> existingChannelMapTunerIds = new HashSet<int>();
      HashSet<int> existingGroupMapGroupIds = new HashSet<int>();
      foreach (ChannelMap channelMap in bestChannel.ChannelMaps)
      {
        existingChannelMapTunerIds.Add(channelMap.IdTuner);
      }
      foreach (GroupMap groupMap in bestChannel.GroupMaps)
      {
        existingGroupMapGroupIds.Add(groupMap.IdGroup);
      }

      HashSet<int> channelIds = new HashSet<int>();
      foreach (Channel channel in channels)
      {
        if (channel.IdChannel == bestChannel.IdChannel)
        {
          continue;
        }
        bestChannel.TimesWatched += channel.TimesWatched;
        channelIds.Add(channel.IdChannel);
      }

      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IList<Recording> recordings = channelRepository.GetQuery<Recording>().Where(r => r.IdChannel.HasValue && channelIds.Contains(r.IdChannel.Value)).ToList();
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Recordings, recordings);
        foreach (Recording recording in recordings)
        {
          recording.IdChannel = bestChannel.IdChannel;
        }
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Recordings, recordings);

        IList<Schedule> schedules = channelRepository.GetQuery<Schedule>().Where(s => channelIds.Contains(s.IdChannel)).ToList();
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Schedules, schedules);
        foreach (Schedule schedule in schedules)
        {
          // TODO This is practically like deleting a schedule then creating a
          // new one. I wonder if we ought to be firing some kind of event.
          schedule.IdChannel = bestChannel.IdChannel;
        }
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Schedules, schedules);

        IList<TuningDetail> tuningDetails = channelRepository.GetQuery<TuningDetail>().Where(td => channelIds.Contains(td.IdChannel)).ToList();
        foreach (TuningDetail tuningDetail in tuningDetails)
        {
          tuningDetail.IdChannel = bestChannel.IdChannel;
          tuningDetail.Priority = nextTuningDetailPriority++;
          TuningDetailManagement.SaveTuningDetail(tuningDetail);
        }

        IList<ChannelMap> channelMaps = channelRepository.GetQuery<ChannelMap>().Where(m => channelIds.Contains(m.IdChannel) && !existingChannelMapTunerIds.Contains(m.IdTuner)).ToList();
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelMaps, channelMaps);
        if (OnStateChangedChannelMapEvent != null)
        {
          foreach (ChannelMap channelMap in channelMaps)
          {
            OnStateChangedChannelMapEvent(channelMap, ObjectState.Deleted);
            channelMap.IdChannel = bestChannel.IdChannel;
            OnStateChangedChannelMapEvent(channelMap, ObjectState.Added);
          }
        }
        else
        {
          foreach (ChannelMap channelMap in channelMaps)
          {
            channelMap.IdChannel = bestChannel.IdChannel;
          }
        }
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelMaps, channelMaps);

        IList<GroupMap> groupMaps = channelRepository.GetQuery<GroupMap>().Where(m => channelIds.Contains(m.IdChannel) && !existingGroupMapGroupIds.Contains(m.IdGroup)).ToList();
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.GroupMaps, groupMaps);
        foreach (GroupMap groupMap in groupMaps)
        {
          groupMap.IdChannel = bestChannel.IdChannel;
        }
        channelRepository.ApplyChanges(channelRepository.ObjectContext.GroupMaps, groupMaps);

        channelRepository.UnitOfWork.SaveChanges();
      }

      foreach (int channelId in channelIds)
      {
        DeleteChannel(channelId);
      }

      return GetChannel(bestChannel.IdChannel, includeRelations);
    }

    public static IList<Channel> GetAllChannelsWithExternalId()
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query =
          channelRepository.GetQuery<Channel>(c => c.ExternalId != null && c.ExternalId != "").OrderBy(
            c => c.ExternalId);
        query = channelRepository.IncludeAllRelations(query, ChannelIncludeRelationEnum.TuningDetails);
        return channelRepository.LoadNavigationProperties(query, ChannelIncludeRelationEnum.TuningDetails);
      }
    }

    public static Channel GetChannelByTuningDetail(int networkId, int transportId, int serviceId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.TuningDetails.Any(t => t.OriginalNetworkId == networkId && t.TransportStreamId == transportId && t.ServiceId == serviceId));
        Channel channel = channelRepository.IncludeAllRelations(query).FirstOrDefault();
        channel = channelRepository.LoadNavigationProperties(channel);
        return channel;
      }
    }

    public static bool IsChannelMappedToTuner(int idChannel, int idTuner)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        return channelRepository.Count<ChannelMap>(m => m.IdTuner == idTuner && m.IdChannel == idChannel) == 0;
      }
    }

    public static ChannelLinkageMap SaveChannelLinkageMap(ChannelLinkageMap map)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelLinkageMaps, map);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelLinkageMaps, map);
        channelRepository.UnitOfWork.SaveChanges();
        map.AcceptChanges();
        return map;
      }
    }

    public static void DeleteAllChannelLinkageMaps(int idPortalChannel)
    {
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<ChannelLinkageMap>(p => p.IdPortalChannel == idPortalChannel);
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    public static History SaveChannelHistory(History history)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Histories, history);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Histories, history);
        channelRepository.UnitOfWork.SaveChanges();
        history.AcceptChanges();
        return history;
      }
    }

    private static void ClearChannelRecordingForeignKeys(int idChannel, IChannelRepository channelRepository)
    {
      // todo : since "on delete: set null" is not currently supported in EF, we have to do this manually - remove this ugly workaround once EF gets mature enough.
      IQueryable<Channel> channels = channelRepository.GetQuery<Channel>(s => s.IdChannel == idChannel);

      channels = channelRepository.IncludeAllRelations(channels, ChannelIncludeRelationEnum.Recordings);
      Channel channel = channels.FirstOrDefault();

      if (channel != null)
      {
        //channelRepository.DeleteList(channel.Recordings);

        for (int i = channel.Recordings.Count - 1; i >= 0; i--)
        {
          Recording recording = channel.Recordings[i];
          recording.IdChannel = null;
          recording.IdSchedule = null;
        }
        channelRepository.ApplyChanges<Channel>(channelRepository.ObjectContext.Channels, channel);
      }
    }

    #region channel-to-tuner maps

    private static ChannelMap GetChannelMap(int idChannelMap)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        return channelRepository.GetQuery<ChannelMap>(m => m.IdChannelMap == idChannelMap).FirstOrDefault();
      }
    }

    public static ChannelMap SaveChannelMap(ChannelMap channelMap)
    {
      if (OnStateChangedChannelMapEvent != null)
      {
        OnStateChangedChannelMapEvent(channelMap, channelMap.ChangeTracker.State);
      }

      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelMaps, channelMap);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelMaps, channelMap);
        channelRepository.UnitOfWork.SaveChanges();
        channelMap.AcceptChanges();
        return channelMap;
      }
    }

    public static IList<ChannelMap> SaveChannelMaps(IEnumerable<ChannelMap> channelMaps)
    {
      if (OnStateChangedChannelMapEvent != null)
      {
        foreach (ChannelMap channelMap in channelMaps)
        {
          OnStateChangedChannelMapEvent(channelMap, channelMap.ChangeTracker.State);
        }
      }

      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelMaps, channelMaps);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelMaps, channelMaps);
        channelRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //channelRepository.ObjectContext.AcceptAllChanges();
        foreach (ChannelMap map in channelMaps)
        {
          map.AcceptChanges();
        }
      }
      return channelMaps.ToList();
    }

    public static void DeleteChannelMap(int idChannelMap)
    {
      if (OnStateChangedChannelMapEvent != null)
      {
        ChannelMap channelMap = GetChannelMap(idChannelMap);
        if (channelMap != null)
        {
          OnStateChangedChannelMapEvent(channelMap, ObjectState.Deleted);
        }
      }

      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<ChannelMap>(m => m.IdChannelMap == idChannelMap);
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    public static void DeleteChannelMaps(IEnumerable<int> channelMapIds)
    {
      if (OnStateChangedChannelMapEvent != null)
      {
        foreach (int id in channelMapIds)
        {
          ChannelMap channelMap = GetChannelMap(id);
          if (channelMap != null)
          {
            OnStateChangedChannelMapEvent(channelMap, ObjectState.Deleted);
          }
        }
      }

      HashSet<int> ids = new HashSet<int>(channelMapIds);
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<ChannelMap>(m => ids.Contains(m.IdChannelMap));
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    #endregion

    #region channel-to-group maps

    public static GroupMap SaveChannelGroupMap(GroupMap groupMap)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.GroupMaps, groupMap);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.GroupMaps, groupMap);
        channelRepository.UnitOfWork.SaveChanges();
        groupMap.AcceptChanges();
        return groupMap;
      }
    }

    public static IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.GroupMaps, groupMaps);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.GroupMaps, groupMaps);
        channelRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //channelRepository.ObjectContext.AcceptAllChanges();
        foreach (GroupMap map in groupMaps)
        {
          map.AcceptChanges();
        }
        return groupMaps.ToList();
      }
    }

    public static void DeleteChannelGroupMap(int idGroupMap)
    {
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<GroupMap>(m => m.IdMap == idGroupMap);
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    public static void DeleteChannelGroupMaps(IEnumerable<int> groupMapIds)
    {
      HashSet<int> ids = new HashSet<int>(groupMapIds);
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<GroupMap>(m => ids.Contains(m.IdMap));
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    #endregion

    public static Channel GetChannelByExternalId(string externalId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<Channel>(c => c.ExternalId == externalId);
        Channel channel = channelRepository.IncludeAllRelations(query).FirstOrDefault();
        channel = channelRepository.LoadNavigationProperties(channel);
        return channel;
      }
    }

    public static IList<Channel> ListAllChannelsForEpgGrabbing(ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetAll<Channel>().Where(c => string.IsNullOrEmpty(c.ExternalId) && c.VisibleInGuide).OrderBy(c => c.LastGrabTime ?? SqlDateTime.MinValue.Value);
        return channelRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }
  }
}