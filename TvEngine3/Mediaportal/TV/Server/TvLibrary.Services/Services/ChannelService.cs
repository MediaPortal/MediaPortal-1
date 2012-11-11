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
    public IList<Channel> GetAllChannelsByGroupIdAndMediaType(int groupId, MediaTypeEnum mediatype)
    {
      var allChannelsByGroupIdAndMediaType = ChannelManagement.GetAllChannelsByGroupIdAndMediaType(groupId, mediatype);
      return allChannelsByGroupIdAndMediaType;
    }

    public IList<Channel> GetAllChannelsByGroupId(int groupId)
    {
      var allChannelsByGroupId = ChannelManagement.GetAllChannelsByGroupId(groupId);
      return allChannelsByGroupId;
    }

    public IList<Channel> ListAllChannels()
    {
      var listAllChannels = ChannelManagement.ListAllChannels();
      return listAllChannels;
    }

    public IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      var listAllChannels = ChannelManagement.ListAllChannels(includeRelations);
      return listAllChannels;
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaTypeEnum mediaType)
    {
      var listAllVisisbleChannelsByMediaType = ChannelManagement.ListAllVisibleChannelsByMediaType(mediaType);
      return listAllVisisbleChannelsByMediaType;
    }    

    public IList<Channel> SaveChannels(IEnumerable<Channel> channels)
    {
      return ChannelManagement.SaveChannels(channels);
    }

    public IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps)
    {
      return ChannelManagement.SaveChannelGroupMaps(groupMaps);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType)
    {
      var listAllChannelsByMediaType = ChannelManagement.ListAllChannelsByMediaType(mediaType);
      return listAllChannelsByMediaType;
    }    

    public IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      var listAllChannelsByMediaType = ChannelManagement.ListAllChannelsByMediaType(mediaType, includeRelations);
      return listAllChannelsByMediaType;
    }

    public IList<Channel> GetAllChannelsByGroupIdAndMediaType(int idGroup, MediaTypeEnum mediaType, ChannelIncludeRelationEnum include)
    {
      var allChannelsByGroupIdAndMediaType = ChannelManagement.GetAllChannelsByGroupIdAndMediaType(idGroup, mediaType, include);
      return allChannelsByGroupIdAndMediaType;
    }    

    public IList<Channel> GetChannelsByName(string channelName)
    {
      var channelsByName = ChannelManagement.GetChannelsByName(channelName).ToList();
      return channelsByName;
    }

    public Channel SaveChannel(Channel channel)
    {
      return ChannelManagement.SaveChannel(channel);
    }

    public Channel GetChannel(int idChannel)
    {
      var channel = ChannelManagement.GetChannel(idChannel);
      return channel;
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
      var tuningDetail = ChannelManagement.GetTuningDetail(dvbChannel);
      return tuningDetail;
    }

    public TuningDetail GetTuningDetailCustom(DVBBaseChannel dvbChannel, TuningDetailSearchEnum tuningDetailSearchEnum)
    {
      var tuningDetail = ChannelManagement.GetTuningDetail(dvbChannel, tuningDetailSearchEnum);
      return tuningDetail;
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
