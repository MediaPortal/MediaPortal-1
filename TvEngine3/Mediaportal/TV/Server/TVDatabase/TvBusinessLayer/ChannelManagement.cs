using System.Collections.Generic;
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
    public static IList<Channel> ListAllChannels(ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetAll<Channel>().OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static IList<Channel> ListAllVisibleChannels(ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>().Where(c => c.VisibleInGuide).OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static IList<Channel> ListAllChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        includeRelations |= ChannelRelation.ChannelGroupMappings;
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>().Where(c => c.ChannelGroupMappings.Count > 0 && c.ChannelGroupMappings.Any(m => m.IdChannelGroup == idChannelGroup));
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations).OrderBy(c => c.ChannelGroupMappings.FirstOrDefault(m => m.IdChannelGroup == idChannelGroup).SortOrder).ToList();
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        includeRelations |= ChannelRelation.ChannelGroupMappings;
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>().Where(c => c.VisibleInGuide && c.ChannelGroupMappings.Count > 0 && c.ChannelGroupMappings.Any(m => m.IdChannelGroup == idChannelGroup));
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations).OrderBy(c => c.ChannelGroupMappings.FirstOrDefault(m => m.IdChannelGroup == idChannelGroup).SortOrder).ToList();
      }
    }

    public static IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.MediaType == (int)mediaType).OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.VisibleInGuide && c.MediaType == (int)mediaType).OrderBy(c => c.Name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static Channel GetChannel(int idChannel, ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.IdChannel == idChannel);
        Channel channel = channelRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
        return channelRepository.LoadNavigationProperties(channel, includeRelations);
      }
    }

    public static IList<Channel> GetChannelsByName(string name, ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.Name == name);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        return channelRepository.LoadNavigationProperties(query, includeRelations);
      }
    }

    public static Channel SaveChannel(Channel channel)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        // Currently we don't trigger tuning detail change events here because
        // tuning detail changes are all handled through tuning detail
        // management.
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
        // Currently we don't trigger tuning detail change events here because
        // tuning detail changes are all handled through tuning detail
        // management.
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

        // Need to manually delete tuning details to trigger events as required.
        Channel channel = GetChannel(idChannel, ChannelRelation.TuningDetails);
        foreach (TuningDetail tuningDetail in channel.TuningDetails)
        {
          TuningDetailManagement.DeleteTuningDetail(tuningDetail.IdTuningDetail);
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

    public static void DeleteOrphanedChannels(IEnumerable<int> channelIds = null)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.TuningDetails.Count == 0);
        if (channelIds != null)
        {
          query.Where(c => channelIds.Contains(c.IdChannel));
        }
        IList<Channel> channels = query.ToList();
        for (int i = 0; i < channels.Count; i++)
        {
          DeleteChannel(channels[i].IdChannel);
        }
      }
    }

    public static Channel MergeChannels(IEnumerable<Channel> channels, ChannelRelation includeRelations)
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

      bestChannel = GetChannel(bestChannel.IdChannel, ChannelRelation.ChannelGroupMappings | ChannelRelation.TuningDetails);
      int nextTuningDetailPriority = bestChannel.TuningDetails.Count + 1;
      HashSet<int> existingChannelGroupMappingChannelGroupIds = new HashSet<int>();
      foreach (ChannelGroupChannelMapping mapping in bestChannel.ChannelGroupMappings)
      {
        existingChannelGroupMappingChannelGroupIds.Add(mapping.IdChannelGroup);
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
          TuningDetailManagement.SaveTuningDetail(tuningDetail);
        }

        IList<ChannelGroupChannelMapping> mappings = channelRepository.GetQuery<ChannelGroupChannelMapping>().Where(m => channelIds.Contains(m.IdChannel) && !existingChannelGroupMappingChannelGroupIds.Contains(m.IdChannelGroup)).ToList();
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelGroupChannelMappings, mappings);
        foreach (ChannelGroupChannelMapping mapping in mappings)
        {
          mapping.IdChannel = bestChannel.IdChannel;
        }
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelGroupChannelMappings, mappings);

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
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.ExternalId != null && c.ExternalId != "").OrderBy(c => c.ExternalId);
        query = channelRepository.IncludeAllRelations(query, ChannelRelation.None);
        return channelRepository.LoadNavigationProperties(query, ChannelRelation.None);
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

      channels = channelRepository.IncludeAllRelations(channels, ChannelRelation.Recordings);
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

    #region channel-to-channel-group mappings

    public static ChannelGroupChannelMapping SaveChannelGroupMapping(ChannelGroupChannelMapping mapping)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelGroupChannelMappings, mapping);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelGroupChannelMappings, mapping);
        channelRepository.UnitOfWork.SaveChanges();
        mapping.AcceptChanges();
        return mapping;
      }
    }

    public static IList<ChannelGroupChannelMapping> SaveChannelGroupMappings(IEnumerable<ChannelGroupChannelMapping> mappings)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelGroupChannelMappings, mappings);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelGroupChannelMappings, mappings);
        channelRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //channelRepository.ObjectContext.AcceptAllChanges();
        foreach (ChannelGroupChannelMapping mapping in mappings)
        {
          mapping.AcceptChanges();
        }
        return mappings.ToList();
      }
    }

    public static void DeleteChannelGroupMapping(int idChannelGroupMapping)
    {
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<ChannelGroupChannelMapping>(m => m.IdChannelGroupChannelMapping == idChannelGroupMapping);
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    public static void DeleteChannelGroupMaps(IEnumerable<int> channelGroupMappingIds)
    {
      HashSet<int> ids = new HashSet<int>(channelGroupMappingIds);
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<ChannelGroupChannelMapping>(m => ids.Contains(m.IdChannelGroupChannelMapping));
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    #endregion

    public static Channel GetChannelByExternalId(string externalId, ChannelRelation includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<Channel>(c => c.ExternalId == externalId);
        Channel channel = channelRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
        return channelRepository.LoadNavigationProperties(channel, includeRelations);
      }
    }
  }
}