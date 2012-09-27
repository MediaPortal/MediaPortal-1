using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.LinqKit;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Countries;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Channel = Mediaportal.TV.Server.TVDatabase.Entities.Channel;
using Mediaportal.TV.Server.TVDatabase.EntityModel;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ChannelManagement
  {
    public static IList<Channel> GetAllChannelsByGroupId(int idGroup)
    {
      try
      {
        using (IChannelRepository channelRepository = new ChannelRepository())
        {
          var query = channelRepository.GetAllChannelsByGroupId(idGroup);
          return channelRepository.IncludeAllRelations(query).ToList();
        }
      }
      catch (Exception ex)
      {
        Log.Error("ChannelManagement.GetAllChannelsByGroupIdAndMediaType ex={0}", ex);
        throw;
      }
    }

    public static IList<Channel> GetAllChannelsByGroupIdAndMediaType(int idGroup, MediaTypeEnum mediaType)
    {
      try
      {
        using (IChannelRepository channelRepository = new ChannelRepository())
        {
          var query = channelRepository.GetAllChannelsByGroupIdAndMediaType(idGroup, mediaType);
          return channelRepository.IncludeAllRelations(query).ToList();
        }
      }
      catch (Exception ex)
      {
        Log.Error("ChannelManagement.GetAllChannelsByGroupIdAndMediaType ex={0}", ex);
        throw;
      }
    }

    public static IList<Channel> ListAllChannels()
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetAll<Channel>().OrderBy(c=>c.SortOrder);
        return channelRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByMediaType(MediaTypeEnum mediaType)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IOrderedQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.VisibleInGuide && c.MediaType == (int)mediaType).OrderBy(c=>c.SortOrder).OrderBy(c=>c.DisplayName);
        return channelRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static IList<Channel> GetAllChannelsWithExternalId()
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IOrderedQueryable<Channel> query =
          channelRepository.GetQuery<Channel>(c => c.ExternalId != null && c.ExternalId != "").OrderBy(
            c => c.ExternalId);
        return channelRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static IList<Channel> SaveChannels(IEnumerable<Channel> channels)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Channels, channels);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Channels, channels);
        channelRepository.UnitOfWork.SaveChanges();        
        channelRepository.ObjectContext.AcceptAllChanges();
        return channels.ToList();        
      }
    }

    public static IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IOrderedQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.MediaType == (int)mediaType).OrderBy(c => c.SortOrder);
        return channelRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static IList<Channel> GetChannelsByName(string channelName)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<Channel>(c => c.DisplayName == channelName);
        return channelRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static Channel SaveChannel(Channel channel)
    {      
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Channels, channel);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Channels, channel);        
        channelRepository.UnitOfWork.SaveChanges();
        channel.AcceptChanges();
        return GetChannel(channel.IdChannel);
      }
    }

    public static Channel GetChannel(int idChannel)
    {
      //lazy loading verified ok
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.IdChannel == idChannel);
        return channelRepository.IncludeAllRelations(query).FirstOrDefault();        
      }
    }

    public static Channel GetChannelByTuningDetail(int networkId, int transportId, int serviceId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<Channel>(c => c.TuningDetails.Any(t => t.networkId == networkId && t.transportId == transportId && t.serviceId == serviceId));
        return channelRepository.IncludeAllRelations(query).FirstOrDefault();
      }     
    }

    public static bool IsChannelMappedToCard(Channel channel, Card card, bool forEpg)
    {
      bool isChannelMappedToCard;
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        if (forEpg)
        {
          isChannelMappedToCard = channelRepository.Count<ChannelMap>(c => c.idCard == card.IdCard && c.idChannel == channel.IdChannel && c.epgOnly) > 0;
        }
        else
        {
          isChannelMappedToCard = channelRepository.Count<ChannelMap>(c => c.idCard == card.IdCard && c.idChannel == channel.IdChannel) > 0;          
        }                
      }
      return isChannelMappedToCard;
    }

    public static TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel)
    {
      TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.ServiceId;
      tuningDetailSearchEnum |= TuningDetailSearchEnum.NetworkId;
      tuningDetailSearchEnum |= TuningDetailSearchEnum.TransportId;

      return GetTuningDetail(dvbChannel, tuningDetailSearchEnum);
    }

    public static TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel, TuningDetailSearchEnum tuningDetailSearchEnum)
    {      
      int channelType = GetChannelType(dvbChannel);
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<TuningDetail>(t => t.channelType == channelType);        

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.NetworkId)))
        {
          query = query.Where(t => t.networkId == dvbChannel.NetworkId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.ServiceId)))
        {
          query = query.Where(t => t.serviceId == dvbChannel.ServiceId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.TransportId)))
        {
          query = query.Where(t => t.transportId == dvbChannel.TransportId);
        }
               
        query = channelRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }
    

    public static int GetChannelType(DVBBaseChannel channel)
    {
      int channelType;

      if (channel is DVBTChannel)
      {
        channelType = 4;
      }
      else if (channel is DVBSChannel)
      {
        channelType = 3;
      }
      else if (channel is DVBCChannel)
      {
        channelType = 2;
      }
      else if (channel is DVBIPChannel)
      {
        channelType = 7;
      }
      else // must be ATSCChannel
      {
        channelType = 1;
      }
      return channelType;
    }

    public static IList<IChannel> GetTuningChannelsByDbChannel(Channel channel)
    {
      IList<TuningDetail> tuningDetails = channel.TuningDetails;
      return tuningDetails.Select(GetTuningChannel).ToList();
    }

    public static IChannel GetTuningChannel(TuningDetail detail)
    {
      switch (detail.channelType)
      {
        case 0: //AnalogChannel
          AnalogChannel analogChannel = new AnalogChannel();
          analogChannel.ChannelNumber = detail.channelNumber;
          CountryCollection collection = new CountryCollection();
          analogChannel.Country = collection.Countries[detail.countryId];
          analogChannel.Frequency = detail.frequency;
          analogChannel.MediaType = (MediaTypeEnum) detail.mediaType;
          analogChannel.Name = detail.name;
          analogChannel.TunerSource = (TunerInputType)detail.tuningSource;
          analogChannel.VideoSource = (AnalogChannel.VideoInputType)detail.videoSource;
          analogChannel.AudioSource = (AnalogChannel.AudioInputType)detail.audioSource;
          return analogChannel;
        case 1: //ATSCChannel
          ATSCChannel atscChannel = new ATSCChannel();
          atscChannel.MajorChannel = detail.majorChannel;
          atscChannel.MinorChannel = detail.minorChannel;
          atscChannel.PhysicalChannel = detail.channelNumber;
          atscChannel.FreeToAir = detail.freeToAir;
          atscChannel.Frequency = detail.frequency;
          atscChannel.MediaType = (MediaTypeEnum)detail.mediaType;
          atscChannel.Name = detail.name;
          atscChannel.NetworkId = detail.networkId;
          atscChannel.PmtPid = detail.pmtPid;
          atscChannel.Provider = detail.provider;
          atscChannel.ServiceId = detail.serviceId;
          //atscChannel.SymbolRate = detail.Symbolrate;
          atscChannel.TransportId = detail.transportId;
          atscChannel.ModulationType = (ModulationType)detail.modulation;
          return atscChannel;
        case 2: //DVBCChannel
          DVBCChannel dvbcChannel = new DVBCChannel();
          dvbcChannel.ModulationType = (ModulationType)detail.modulation;
          dvbcChannel.FreeToAir = detail.freeToAir;
          dvbcChannel.Frequency = detail.frequency;
          dvbcChannel.MediaType = (MediaTypeEnum)detail.mediaType;
          dvbcChannel.Name = detail.name;
          dvbcChannel.NetworkId = detail.networkId;
          dvbcChannel.PmtPid = detail.pmtPid;
          dvbcChannel.Provider = detail.provider;
          dvbcChannel.ServiceId = detail.serviceId;
          dvbcChannel.SymbolRate = detail.symbolrate;
          dvbcChannel.TransportId = detail.transportId;
          dvbcChannel.LogicalChannelNumber = detail.channelNumber;
          return dvbcChannel;
        case 3: //DVBSChannel
          DVBSChannel dvbsChannel = new DVBSChannel();
          dvbsChannel.DisEqc = (DisEqcType)detail.diseqc;
          dvbsChannel.Polarisation = (Polarisation)detail.polarisation;
          dvbsChannel.SwitchingFrequency = detail.switchingFrequency;
          dvbsChannel.FreeToAir = detail.freeToAir;
          dvbsChannel.Frequency = detail.frequency;
          dvbsChannel.MediaType = (MediaTypeEnum)detail.mediaType;
          dvbsChannel.Name = detail.name;
          dvbsChannel.NetworkId = detail.networkId;
          dvbsChannel.PmtPid = detail.pmtPid;
          dvbsChannel.Provider = detail.provider;
          dvbsChannel.ServiceId = detail.serviceId;
          dvbsChannel.SymbolRate = detail.symbolrate;
          dvbsChannel.TransportId = detail.transportId;
          dvbsChannel.BandType = (BandType)detail.band;
          dvbsChannel.SatelliteIndex = detail.satIndex;
          dvbsChannel.ModulationType = (ModulationType)detail.modulation;
          dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate)detail.innerFecRate;
          dvbsChannel.Pilot = (Pilot)detail.pilot;
          dvbsChannel.Rolloff = (RollOff)detail.rollOff;
          dvbsChannel.LogicalChannelNumber = detail.channelNumber;
          return dvbsChannel;
        case 4: //DVBTChannel
          DVBTChannel dvbtChannel = new DVBTChannel();
          dvbtChannel.BandWidth = detail.bandwidth;
          dvbtChannel.FreeToAir = detail.freeToAir;
          dvbtChannel.Frequency = detail.frequency;
          dvbtChannel.MediaType = (MediaTypeEnum)detail.mediaType;
          dvbtChannel.Name = detail.name;
          dvbtChannel.NetworkId = detail.networkId;
          dvbtChannel.PmtPid = detail.pmtPid;
          dvbtChannel.Provider = detail.provider;
          dvbtChannel.ServiceId = detail.serviceId;
          dvbtChannel.TransportId = detail.transportId;
          dvbtChannel.LogicalChannelNumber = detail.channelNumber;
          return dvbtChannel;
        case 7: //DVBIPChannel
          DVBIPChannel dvbipChannel = new DVBIPChannel();
          dvbipChannel.FreeToAir = detail.freeToAir;
          dvbipChannel.MediaType = (MediaTypeEnum)detail.mediaType;
          dvbipChannel.Name = detail.name;
          dvbipChannel.NetworkId = detail.networkId;
          dvbipChannel.PmtPid = detail.pmtPid;
          dvbipChannel.Provider = detail.provider;
          dvbipChannel.ServiceId = detail.serviceId;
          dvbipChannel.TransportId = detail.transportId;
          dvbipChannel.LogicalChannelNumber = detail.channelNumber;
          dvbipChannel.Url = detail.url;
          return dvbipChannel;
      }
      return null;
    }

    public static TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {        
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.TuningDetails, tuningDetail);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.TuningDetails, tuningDetail);        
        channelRepository.UnitOfWork.SaveChanges();
        tuningDetail.AcceptChanges();        
        return tuningDetail;
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

    public static IChannel GetTuningChannelByType(Channel channel, int channelType)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        TuningDetail tuningDetail = channelRepository.FindOne<TuningDetail>(t => t.channelType == channelType);

        if (tuningDetail != null)
        {
          return GetTuningChannel(tuningDetail);
        }
      }
      return null;
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

    public static void DeleteChannel(int idChannel)
    {
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        SetRelatedRecordingsToNull(idChannel, channelRepository);
        channelRepository.Delete<Channel>(p => p.IdChannel == idChannel);

        /*Channel ch = new Channel();
        ch.idChannel = idChannel;

        channelRepository.ObjectContext.AttachTo("Channels", ch);
        channelRepository.ObjectContext.DeleteObject(ch);
        */
        channelRepository.UnitOfWork.SaveChanges();
      }
    }


    private static void SetRelatedRecordingsToNull(int idChannel, IChannelRepository channelRepository)
    {
      // todo : since "on delete: set null" is not currently supported in EF, we have to do this manually - remove this ugly workaround once EF gets mature enough.
      var channels = channelRepository.GetQuery<Channel>(s => s.IdChannel == idChannel);
      channels = channelRepository.IncludeAllRelations(channels).Include(r => r.Recordings);
      Channel channel = channels.FirstOrDefault();      

      if (channel != null)
      {
        //channelRepository.DeleteList(channel.Recordings);

        for (int i = channel.Recordings.Count - 1; i >= 0; i--)
        {
          Recording recording = channel.Recordings[i];
          recording.Schedule = null;
        }
        channelRepository.ApplyChanges<Channel>(channelRepository.ObjectContext.Channels, channel);
      }
    }


    public static TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel, string url)
    {
      int channelType = GetChannelType(dvbChannel);
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<TuningDetail>(t => t.channelType == channelType && t.url == url);
        query = channelRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }

    public static void AddTuningDetail(int idChannel, IChannel channel)
    {
      TuningDetail tuningDetail = new TuningDetail();
      TuningDetail detail = UpdateTuningDetailWithChannelData(idChannel, channel, tuningDetail);
      tuningDetail.idChannel = idChannel;
      SaveTuningDetail(detail);  
    }
    public static void UpdateTuningDetail(int idChannel, int idTuning, IChannel channel)
    {

      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<TuningDetail>(t => t.idTuning == idTuning && t.idChannel == idChannel);        
        TuningDetail tuningDetail = query.FirstOrDefault();

        TuningDetail detail = UpdateTuningDetailWithChannelData(idChannel, channel, tuningDetail);        
        SaveTuningDetail(detail);
      }       
    }

    private static TuningDetail UpdateTuningDetailWithChannelData(int idChannel, IChannel channel, TuningDetail tuningDetail)
    {
      string channelName = "";
      long channelFrequency = 0;
      int channelNumber = 0;
      int country = 31;      
      int tunerSource = 0;
      int videoInputType = 0;
      int audioInputType = 0;
      bool isVCRSignal = false;
      int symbolRate = 0;
      int modulation = 0;
      int polarisation = 0;
      int switchFrequency = 0;
      int diseqc = 0;
      int bandwidth = 8;
      bool freeToAir = true;
      int pmtPid = -1;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int band = 0;
      int satIndex = -1;
      int innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      int pilot = (int)Pilot.NotSet;
      int rollOff = (int)RollOff.NotSet;
      string url = "";
      MediaTypeEnum mediaType = MediaTypeEnum.TV;

      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel != null)
      {
        channelName = analogChannel.Name;
        channelFrequency = analogChannel.Frequency;
        channelNumber = analogChannel.ChannelNumber;
        country = analogChannel.Country.Index;
        mediaType = analogChannel.MediaType;        
        tunerSource = (int)analogChannel.TunerSource;
        videoInputType = (int)analogChannel.VideoSource;
        audioInputType = (int)analogChannel.AudioSource;
        isVCRSignal = analogChannel.IsVCRSignal;
        channelType = 0;
      }

      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel != null)
      {
        majorChannel = atscChannel.MajorChannel;
        minorChannel = atscChannel.MinorChannel;
        channelNumber = atscChannel.PhysicalChannel;        
        modulation = (int)atscChannel.ModulationType;
        channelType = 1;
      }

      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel != null)
      {
        symbolRate = dvbcChannel.SymbolRate;
        modulation = (int)dvbcChannel.ModulationType;
        channelNumber = dvbcChannel.LogicalChannelNumber > 999 ? idChannel : dvbcChannel.LogicalChannelNumber;
        channelType = 2;
      }

      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel != null)
      {
        symbolRate = dvbsChannel.SymbolRate;
        polarisation = (int)dvbsChannel.Polarisation;
        switchFrequency = dvbsChannel.SwitchingFrequency;
        diseqc = (int)dvbsChannel.DisEqc;
        band = (int)dvbsChannel.BandType;
        satIndex = dvbsChannel.SatelliteIndex;
        modulation = (int)dvbsChannel.ModulationType;
        innerFecRate = (int)dvbsChannel.InnerFecRate;
        pilot = (int)dvbsChannel.Pilot;
        rollOff = (int)dvbsChannel.Rolloff;
        channelNumber = dvbsChannel.LogicalChannelNumber > 999 ? idChannel : dvbsChannel.LogicalChannelNumber;
        channelType = 3;
      }

      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.BandWidth;
        channelNumber = dvbtChannel.LogicalChannelNumber;
        channelType = 4;
      }

      DVBIPChannel dvbipChannel = channel as DVBIPChannel;
      if (dvbipChannel != null)
      {
        url = dvbipChannel.Url;
        channelNumber = dvbipChannel.LogicalChannelNumber;
        channelType = 7;
      }

      DVBBaseChannel dvbChannel = channel as DVBBaseChannel;
      if (dvbChannel != null)
      {
        pmtPid = dvbChannel.PmtPid;
        networkId = dvbChannel.NetworkId;
        serviceId = dvbChannel.ServiceId;
        transportId = dvbChannel.TransportId;
        channelName = dvbChannel.Name;
        provider = dvbChannel.Provider;
        channelFrequency = dvbChannel.Frequency;
        mediaType = dvbChannel.MediaType;        
        freeToAir = dvbChannel.FreeToAir;
      }

      tuningDetail.name = channelName;      
      tuningDetail.provider = provider;
      tuningDetail.channelType = channelType;
      tuningDetail.channelNumber = channelNumber;
      tuningDetail.frequency = (int)channelFrequency;
      tuningDetail.countryId = country;
      tuningDetail.mediaType = (int) mediaType;
      tuningDetail.networkId = networkId;
      tuningDetail.transportId = transportId;
      tuningDetail.serviceId = serviceId;
      tuningDetail.pmtPid = pmtPid;
      tuningDetail.freeToAir = freeToAir;
      tuningDetail.modulation = modulation;
      tuningDetail.polarisation = polarisation;
      tuningDetail.symbolrate = symbolRate;
      tuningDetail.diseqc = diseqc;
      tuningDetail.switchingFrequency = switchFrequency;
      tuningDetail.bandwidth = bandwidth;
      tuningDetail.majorChannel = majorChannel;
      tuningDetail.minorChannel = minorChannel;
      tuningDetail.videoSource = videoInputType;
      tuningDetail.audioSource = audioInputType;
      tuningDetail.isVCRSignal = isVCRSignal;
      tuningDetail.tuningSource = tunerSource;
      tuningDetail.band = band;
      tuningDetail.satIndex = satIndex;
      tuningDetail.innerFecRate = innerFecRate;
      tuningDetail.pilot = pilot;
      tuningDetail.rollOff = rollOff;
      tuningDetail.url = url;
      tuningDetail.bitrate = 0;      

      /*TuningDetail detail = TuningDetailFactory.CreateTuningDetail(idChannel, channelName, provider,
                                             channelType, channelNumber, (int)channelFrequency, country, mediaType,
                                             networkId, transportId, serviceId, pmtPid, freeToAir,
                                             modulation, polarisation, symbolRate, diseqc, switchFrequency,
                                             bandwidth, majorChannel, minorChannel, videoInputType,
                                             audioInputType, isVCRSignal, tunerSource, band,
                                             satIndex,
                                             innerFecRate, pilot, rollOff, url, 0);*/
      return tuningDetail;
    }



    public static IList<TuningDetail> GetTuningDetailsByName(string channelName, int channelType)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<TuningDetail> query = channelRepository.GetQuery<TuningDetail>(t => t.name == channelName && t.channelType == channelType);
        query = channelRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }    

    public static Channel GetChannelByName(string channelName, ChannelIncludeRelationEnum includeRelations)
    {
      Channel channel;
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<Channel>(c => c.DisplayName == channelName);
        channel = channelRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
        
        if (channel == null)
        {
          query = channelRepository.GetQuery<Channel>(c => c.DisplayName.Contains(channelName));
          channel = channelRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault(); 
        }        
      }
      return channel;
    }

    public static void DeleteTuningDetail(int idTuning)
    {
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<TuningDetail>(p => p.idTuning == idTuning);
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

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
        channelRepository.ObjectContext.AcceptAllChanges();        
        return groupMaps.ToList();        
      }
    }

    public static void DeleteChannelMap(int idChannelMap)
    {
      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<ChannelMap>(p => p.idChannelMap == idChannelMap);
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    public static ChannelMap SaveChannelMap(ChannelMap map)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ChannelMaps, map);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ChannelMaps, map);
        channelRepository.UnitOfWork.SaveChanges();
        map.AcceptChanges();
        return map;
      }
    }


    public static Channel GetChannelByExternalId(string externalId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<Channel>(c => c.ExternalId == externalId);
        return channelRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {        
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.MediaType == (int)mediaType).OrderBy(c => c.SortOrder);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        Log.Debug("ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations) SQL = {0}", query.ToTraceString());
        return query.ToList();        
      }
    }

    public static IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetAll<Channel>().OrderBy(c => c.SortOrder);
        return channelRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<Channel> GetAllChannelsByGroupIdAndMediaType(int idGroup, MediaTypeEnum mediaType, ChannelIncludeRelationEnum include)
    {
      try
      {
        using (IChannelRepository channelRepository = new ChannelRepository())
        {
          var query = channelRepository.GetAllChannelsByGroupIdAndMediaType(idGroup, mediaType);
          return channelRepository.IncludeAllRelations(query, include).ToList();
        }
      }
      catch (Exception ex)
      {
        Log.Error("ChannelManagement.GetAllChannelsByGroupIdAndMediaType ex={0}", ex);
        throw;
      }
    }    
  }
}
