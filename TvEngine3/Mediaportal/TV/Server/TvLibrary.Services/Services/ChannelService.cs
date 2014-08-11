using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class ChannelService : IChannelService
  {
    public IList<Channel> ListAllChannels()
    {
      return ChannelManagement.ListAllChannels();
    }

    public IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllChannels(includeRelations);
    }

    public IList<Channel> ListAllChannelsByGroupId(int groupId)
    {
      return ChannelManagement.ListAllChannelsByGroupId(groupId);
    }

    public IList<Channel> ListAllChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllChannelsByGroupId(groupId, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int groupId)
    {
      return ChannelManagement.ListAllVisibleChannelsByGroupId(groupId);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllVisibleChannelsByGroupId(groupId, includeRelations);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType)
    {
      return ChannelManagement.ListAllChannelsByMediaType(mediaType);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllChannelsByMediaType(mediaType, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaTypeEnum mediaType)
    {
      return ChannelManagement.ListAllVisibleChannelsByMediaType(mediaType);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllVisibleChannelsByMediaType(mediaType, includeRelations);
    }

    public IList<Channel> SaveChannels(IEnumerable<Channel> channels)
    {
      return ChannelManagement.SaveChannels(channels);
    }

    public IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps)
    {
      return ChannelManagement.SaveChannelGroupMaps(groupMaps);
    }

    public IList<Channel> GetChannelsByName(string channelName)
    {
      return ChannelManagement.GetChannelsByName(channelName).ToList();
    }

    public Channel SaveChannel(Channel channel)
    {
      return ChannelManagement.SaveChannel(channel);
    }

    public Channel GetChannel(int idChannel)
    {
      return ChannelManagement.GetChannel(idChannel);
    }

    public void DeleteChannel(int idChannel)
    {
      ChannelManagement.DeleteChannel(idChannel);
    }

    public TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      return ChannelManagement.SaveTuningDetail(tuningDetail);      
    }

    public TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel)
    {
      return ChannelManagement.GetTuningDetail(dvbChannel);
    }

    public TuningDetail GetTuningDetailCustom(DVBBaseChannel dvbChannel, TuningDetailSearchEnum tuningDetailSearchEnum)
    {
      return ChannelManagement.GetTuningDetail(dvbChannel, tuningDetailSearchEnum);
    }

    public TuningDetail GetTuningDetailByURL(DVBBaseChannel dvbChannel, string url)
    {
      return ChannelManagement.GetTuningDetail(dvbChannel, url);
    }

    public void AddTuningDetail(int idChannel, IChannel channel)
    {
      ChannelManagement.AddTuningDetail(idChannel, channel);
    }

    public IList<TuningDetail> GetTuningDetailsByName(string channelName, int channelType)
    {
      return ChannelManagement.GetTuningDetailsByName(channelName, channelType);
    }

    public void UpdateTuningDetail(int idChannel, int idTuning, IChannel channel)
    {
      ChannelManagement.UpdateTuningDetail(idChannel, idTuning, channel);
    }


    public Channel GetChannelByName(string channelName, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.GetChannelByName(channelName, includeRelations);
    }

    public void DeleteTuningDetail(int idTuning)
    {
      ChannelManagement.DeleteTuningDetail(idTuning);
    }

    public GroupMap SaveChannelGroupMap(GroupMap groupMap)
    {
      return ChannelManagement.SaveChannelGroupMap(groupMap);
    }

    public void DeleteChannelMap(int idChannelMap)
    {
      ChannelManagement.DeleteChannelMap(idChannelMap);
    }

    public ChannelMap SaveChannelMap(ChannelMap map)
    {
      return ChannelManagement.SaveChannelMap(map);
    }
  }
}