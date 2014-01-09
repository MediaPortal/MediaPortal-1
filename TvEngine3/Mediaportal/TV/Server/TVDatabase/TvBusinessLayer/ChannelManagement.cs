using System;
using System.Collections.Generic;
using System.Linq;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Countries;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ChannelManagement
  {

    public delegate void OnStateChangedTuningDetailDelegate(TuningDetail tuningDetail, ObjectState state);
    public static event OnStateChangedTuningDetailDelegate OnStateChangedTuningDetailEvent;

    public delegate void OnStateChangedChannelMapDelegate(ChannelMap map, ObjectState state);
    public static event OnStateChangedChannelMapDelegate OnStateChangedChannelMapEvent;

    /*public delegate void OnAddTuningDetailDelegate(TuningDetail tuningDetail);
    public static event OnAddTuningDetailDelegate OnAddTuningDetailEvent;

    public delegate void OnDeleteTuningDetailDelegate(TuningDetail tuningDetail);
    public static event OnDeleteTuningDetailDelegate OnDeleteTuningDetailEvent;

    public delegate void OnAddChannelMapDelegate(ChannelMap map);
    public static event OnAddChannelMapDelegate OnAddChannelMapEvent;

    public delegate void OnDeleteChannelMapDelegate(ChannelMap map);
    public static event OnDeleteChannelMapDelegate OnDeleteChannelMapEvent;
    */

    public static IList<Channel> GetAllChannelsByGroupId(int idGroup)
    {
      try
      {
        using (IChannelRepository channelRepository = new ChannelRepository())
        {
          IQueryable<Channel> query = channelRepository.GetAllChannelsByGroupId(idGroup);
          query = channelRepository.IncludeAllRelations(query);
          IList<Channel> channels = channelRepository.LoadNavigationProperties(query);
          return channels;
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
          query = channelRepository.IncludeAllRelations(query);
          IList<Channel> channels = channelRepository.LoadNavigationProperties(query);
          return channels;
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
        IQueryable<Channel> query = channelRepository.GetAll<Channel>().OrderBy(c => c.SortOrder);
        query = channelRepository.IncludeAllRelations(query);
        IList<Channel> channels = channelRepository.LoadNavigationProperties(query);
        return channels;
      }
    }

    public static IList<Channel> ListAllVisibleChannelsByMediaType(MediaTypeEnum mediaType)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.VisibleInGuide && c.MediaType == (int)mediaType).OrderBy(c => c.SortOrder).OrderBy(c => c.DisplayName);
        query = channelRepository.IncludeAllRelations(query);
        IList<Channel> channels = channelRepository.LoadNavigationProperties(query);
        return channels;
      }
    }

    public static IList<Channel> GetAllChannelsWithExternalId()
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query =
          channelRepository.GetQuery<Channel>(c => c.ExternalId != null && c.ExternalId != "").OrderBy(
            c => c.ExternalId);
        query = channelRepository.IncludeAllRelations(query);
        IList<Channel> channels = channelRepository.LoadNavigationProperties(query);
        return channels;
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
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.MediaType == (int)mediaType).OrderBy(c => c.SortOrder);
        query = channelRepository.IncludeAllRelations(query);
        IList<Channel> channels = channelRepository.LoadNavigationProperties(query);
        return channels;
      }
    }

    public static IList<Channel> GetChannelsByName(string channelName)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.DisplayName == channelName);
        query = channelRepository.IncludeAllRelations(query);
        IList<Channel> channels = channelRepository.LoadNavigationProperties(query);
        return channels;
      }
    }

    public static Channel SaveChannel(Channel channel)
    {
      if (OnStateChangedChannelMapEvent != null || OnStateChangedTuningDetailEvent != null)
      {
        Dictionary<string, ObjectList>.ValueCollection deletedProperties = channel.ChangeTracker.ObjectsRemovedFromCollectionProperties.Values;
        foreach (ObjectList deletedProperty in deletedProperties)
        {
          if (OnStateChangedTuningDetailEvent != null)
          {
            IEnumerable<TuningDetail> deletedTuningDetails = deletedProperty.OfType<TuningDetail>();
            foreach (TuningDetail deletedTuningDetail in deletedTuningDetails)
            {
              OnStateChangedTuningDetailEvent(deletedTuningDetail, ObjectState.Deleted);
            }
          }

          if (OnStateChangedChannelMapEvent != null)
          {
            IEnumerable<ChannelMap> deletedChannelMaps = deletedProperty.OfType<ChannelMap>();
            foreach (ChannelMap deletedChannelMap in deletedChannelMaps)
            {
              OnStateChangedChannelMapEvent(deletedChannelMap, ObjectState.Deleted);
            }
          }
        }
      }

      using (IChannelRepository channelRepository = new ChannelRepository())
      {

        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.Channels, channel);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.Channels, channel);
        channelRepository.UnitOfWork.SaveChanges();

        IList<Action> events = new List<Action>();

        if (OnStateChangedChannelMapEvent != null || OnStateChangedTuningDetailEvent != null)
        {
          Dictionary<string, ObjectList>.ValueCollection addedProperties =
            channel.ChangeTracker.ObjectsAddedToCollectionProperties.Values;
          foreach (ObjectList addedProperty in addedProperties)
          {
            if (OnStateChangedTuningDetailEvent != null)
            {
              IEnumerable<TuningDetail> addedTuningDetails = addedProperty.OfType<TuningDetail>();
              foreach (TuningDetail addedTuningDetail in addedTuningDetails)
              {
                TuningDetail detail = addedTuningDetail;
                //events.Add(() => OnStateChangedTuningDetailEvent(detail, ObjectState.Added));
                OnStateChangedTuningDetailEvent(detail, ObjectState.Added);
              }
            }

            if (OnStateChangedChannelMapEvent != null)
            {
              IEnumerable<ChannelMap> addedChannelMaps = addedProperty.OfType<ChannelMap>();
              foreach (ChannelMap addedChannelMap in addedChannelMaps)
              {
                ChannelMap map = addedChannelMap;
                //events.Add(() => OnStateChangedChannelMapEvent(map, ObjectState.Added));
                OnStateChangedChannelMapEvent(map, ObjectState.Added);
              }
            }
          }
        }

        channel.AcceptChanges();
        Channel updatedChannel = GetChannel(channel.IdChannel);

        /*foreach (Action action in events)
        {
          action();
        }*/

        return updatedChannel;
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

    public static Channel GetChannelByTuningDetail(int networkId, int transportId, int serviceId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.TuningDetails.Any(t => t.NetworkId == networkId && t.TransportId == transportId && t.ServiceId == serviceId));
        Channel channel = channelRepository.IncludeAllRelations(query).FirstOrDefault();
        channel = channelRepository.LoadNavigationProperties(channel);
        return channel;
      }
    }

    public static bool IsChannelMappedToCard(int idChannel, int idCard, bool forEpg)
    {
      bool isChannelMappedToCard;
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        if (forEpg)
        {
          isChannelMappedToCard = channelRepository.Count<ChannelMap>(c => c.IdCard == idCard && c.IdChannel == idChannel && c.EpgOnly) > 0;
        }
        else
        {
          isChannelMappedToCard = channelRepository.Count<ChannelMap>(c => c.IdCard == idCard && c.IdChannel == idChannel) > 0;
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
        var query = channelRepository.GetQuery<TuningDetail>(t => t.ChannelType == channelType);

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.NetworkId)))
        {
          query = query.Where(t => t.NetworkId == dvbChannel.NetworkId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.ServiceId)))
        {
          query = query.Where(t => t.ServiceId == dvbChannel.ServiceId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.TransportId)))
        {
          query = query.Where(t => t.TransportId == dvbChannel.TransportId);
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
      switch (detail.ChannelType)
      {
        case 0: //AnalogChannel
          AnalogChannel analogChannel = new AnalogChannel();
          analogChannel.ChannelNumber = detail.ChannelNumber;
          CountryCollection collection = new CountryCollection();
          analogChannel.Country = collection.Countries[detail.CountryId];
          analogChannel.Frequency = detail.Frequency;
          analogChannel.MediaType = (MediaTypeEnum)detail.MediaType;
          analogChannel.Name = detail.Name;
          analogChannel.TunerSource = (TunerInputType)detail.TuningSource;
          analogChannel.VideoSource = (AnalogChannel.VideoInputType)detail.VideoSource;
          analogChannel.AudioSource = (AnalogChannel.AudioInputType)detail.AudioSource;
          return analogChannel;
        case 1: //ATSCChannel
          ATSCChannel atscChannel = new ATSCChannel();
          atscChannel.MajorChannel = detail.MajorChannel;
          atscChannel.MinorChannel = detail.MinorChannel;
          atscChannel.PhysicalChannel = detail.ChannelNumber;
          atscChannel.FreeToAir = detail.FreeToAir;
          atscChannel.Frequency = detail.Frequency;
          atscChannel.MediaType = (MediaTypeEnum)detail.MediaType;
          atscChannel.Name = detail.Name;
          atscChannel.NetworkId = detail.NetworkId;
          atscChannel.PmtPid = detail.PmtPid;
          atscChannel.Provider = detail.Provider;
          atscChannel.ServiceId = detail.ServiceId;
          //atscChannel.SymbolRate = detail.Symbolrate;
          atscChannel.TransportId = detail.TransportId;
          atscChannel.ModulationType = (ModulationType)detail.Modulation;
          return atscChannel;
        case 2: //DVBCChannel
          DVBCChannel dvbcChannel = new DVBCChannel();
          dvbcChannel.ModulationType = (ModulationType)detail.Modulation;
          dvbcChannel.FreeToAir = detail.FreeToAir;
          dvbcChannel.Frequency = detail.Frequency;
          dvbcChannel.MediaType = (MediaTypeEnum)detail.MediaType;
          dvbcChannel.Name = detail.Name;
          dvbcChannel.NetworkId = detail.NetworkId;
          dvbcChannel.PmtPid = detail.PmtPid;
          dvbcChannel.Provider = detail.Provider;
          dvbcChannel.ServiceId = detail.ServiceId;
          dvbcChannel.SymbolRate = detail.Symbolrate;
          dvbcChannel.TransportId = detail.TransportId;
          dvbcChannel.LogicalChannelNumber = detail.ChannelNumber;
          return dvbcChannel;
        case 3: //DVBSChannel
          DVBSChannel dvbsChannel = new DVBSChannel();
          dvbsChannel.Diseqc = (DiseqcPort)detail.DiSEqC;
          dvbsChannel.Polarisation = (Polarisation)detail.Polarisation;
          dvbsChannel.FreeToAir = detail.FreeToAir;
          dvbsChannel.Frequency = detail.Frequency;
          dvbsChannel.MediaType = (MediaTypeEnum)detail.MediaType;
          dvbsChannel.Name = detail.Name;
          dvbsChannel.NetworkId = detail.NetworkId;
          dvbsChannel.PmtPid = detail.PmtPid;
          dvbsChannel.Provider = detail.Provider;
          dvbsChannel.ServiceId = detail.ServiceId;
          dvbsChannel.SymbolRate = detail.Symbolrate;
          dvbsChannel.TransportId = detail.TransportId;
          dvbsChannel.SatelliteIndex = detail.SatIndex;
          dvbsChannel.ModulationType = (ModulationType)detail.Modulation;
          dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate)detail.InnerFecRate;
          dvbsChannel.Pilot = (Pilot)detail.Pilot;
          dvbsChannel.RollOff = (RollOff)detail.RollOff;
          dvbsChannel.LogicalChannelNumber = detail.ChannelNumber;
          dvbsChannel.LnbType = detail.LnbType;
          return dvbsChannel;
        case 4: //DVBTChannel
          DVBTChannel dvbtChannel = new DVBTChannel();
          dvbtChannel.Bandwidth = detail.Bandwidth;
          dvbtChannel.FreeToAir = detail.FreeToAir;
          dvbtChannel.Frequency = detail.Frequency;
          dvbtChannel.MediaType = (MediaTypeEnum)detail.MediaType;
          dvbtChannel.Name = detail.Name;
          dvbtChannel.NetworkId = detail.NetworkId;
          dvbtChannel.PmtPid = detail.PmtPid;
          dvbtChannel.Provider = detail.Provider;
          dvbtChannel.ServiceId = detail.ServiceId;
          dvbtChannel.TransportId = detail.TransportId;
          dvbtChannel.LogicalChannelNumber = detail.ChannelNumber;
          return dvbtChannel;
        case 7: //DVBIPChannel
          DVBIPChannel dvbipChannel = new DVBIPChannel();
          dvbipChannel.FreeToAir = detail.FreeToAir;
          dvbipChannel.MediaType = (MediaTypeEnum)detail.MediaType;
          dvbipChannel.Name = detail.Name;
          dvbipChannel.NetworkId = detail.NetworkId;
          dvbipChannel.PmtPid = detail.PmtPid;
          dvbipChannel.Provider = detail.Provider;
          dvbipChannel.ServiceId = detail.ServiceId;
          dvbipChannel.TransportId = detail.TransportId;
          dvbipChannel.LogicalChannelNumber = detail.ChannelNumber;
          dvbipChannel.Url = detail.Url;
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
        TuningDetail tuningDetail = channelRepository.GetQuery<TuningDetail>()
                .Include(t => t.LnbType)
                .FirstOrDefault(t => t.ChannelType == channelType && t.IdChannel == channel.IdChannel);

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
      IQueryable<Channel> channels = channelRepository.GetQuery<Channel>(s => s.IdChannel == idChannel);

      channels = channelRepository.IncludeAllRelations(channels, ChannelIncludeRelationEnum.Recordings);
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
        var query = channelRepository.GetQuery<TuningDetail>(t => t.ChannelType == channelType && t.Url == url);
        query = channelRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }

    public static void AddTuningDetail(int idChannel, IChannel channel)
    {
      TuningDetail tuningDetail = new TuningDetail();
      TuningDetail detail = UpdateTuningDetailWithChannelData(idChannel, channel, tuningDetail);
      tuningDetail.IdChannel = idChannel;
      SaveTuningDetail(detail);
      if (OnStateChangedTuningDetailEvent != null)
      {
        OnStateChangedTuningDetailEvent(tuningDetail, ObjectState.Added);
      }
    }

    public static void UpdateTuningDetail(int idChannel, int idTuning, IChannel channel)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<TuningDetail>(t => t.IdTuning == idTuning && t.IdChannel == idChannel);
        TuningDetail tuningDetail = query.FirstOrDefault();

        TuningDetail detail = UpdateTuningDetailWithChannelData(idChannel, channel, tuningDetail);
        SaveTuningDetail(detail);

        if (OnStateChangedTuningDetailEvent != null)
        {
          OnStateChangedTuningDetailEvent(detail, ObjectState.Modified);
        }
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
      LnbType lnbType = null;

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
        isVCRSignal = analogChannel.IsVcrSignal;
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
        lnbType = dvbsChannel.LnbType;
        symbolRate = dvbsChannel.SymbolRate;
        polarisation = (int)dvbsChannel.Polarisation;
        diseqc = (int)dvbsChannel.Diseqc;
        satIndex = dvbsChannel.SatelliteIndex;
        modulation = (int)dvbsChannel.ModulationType;
        innerFecRate = (int)dvbsChannel.InnerFecRate;
        pilot = (int)dvbsChannel.Pilot;
        rollOff = (int)dvbsChannel.RollOff;
        channelNumber = dvbsChannel.LogicalChannelNumber > 999 ? idChannel : dvbsChannel.LogicalChannelNumber;
        channelType = 3;
      }

      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.Bandwidth;
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

      tuningDetail.Name = channelName;
      tuningDetail.Provider = provider;
      tuningDetail.ChannelType = channelType;
      tuningDetail.ChannelNumber = channelNumber;
      tuningDetail.Frequency = (int)channelFrequency;
      tuningDetail.CountryId = country;
      tuningDetail.MediaType = (int)mediaType;
      tuningDetail.NetworkId = networkId;
      tuningDetail.TransportId = transportId;
      tuningDetail.ServiceId = serviceId;
      tuningDetail.PmtPid = pmtPid;
      tuningDetail.FreeToAir = freeToAir;
      tuningDetail.Modulation = modulation;
      tuningDetail.Polarisation = polarisation;
      tuningDetail.Symbolrate = symbolRate;
      tuningDetail.DiSEqC = diseqc;
      tuningDetail.Bandwidth = bandwidth;
      tuningDetail.MajorChannel = majorChannel;
      tuningDetail.MinorChannel = minorChannel;
      tuningDetail.VideoSource = videoInputType;
      tuningDetail.AudioSource = audioInputType;
      tuningDetail.IsVCRSignal = isVCRSignal;
      tuningDetail.TuningSource = tunerSource;
      tuningDetail.Band = band;
      tuningDetail.SatIndex = satIndex;
      tuningDetail.InnerFecRate = innerFecRate;
      tuningDetail.Pilot = pilot;
      tuningDetail.RollOff = rollOff;
      tuningDetail.Url = url;
      tuningDetail.Bitrate = 0;
      if (lnbType != null)
      {
        tuningDetail.IdLnbType = lnbType.IdLnbType;
      }

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
        IQueryable<TuningDetail> query = channelRepository.GetQuery<TuningDetail>(t => t.Name == channelName && t.ChannelType == channelType);
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

        channel = channelRepository.LoadNavigationProperties(channel, includeRelations);

      }
      return channel;
    }

    public static void DeleteTuningDetail(int idTuning)
    {
      if (OnStateChangedTuningDetailEvent != null)
      {
        TuningDetail tuningDetail = GetTuningDetail(idTuning);
        if (tuningDetail != null)
        {
          OnStateChangedTuningDetailEvent(tuningDetail, ObjectState.Deleted);
        }
      }

      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<TuningDetail>(p => p.IdTuning == idTuning);
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
        channelRepository.Delete<ChannelMap>(p => p.IdChannelMap == idChannelMap);
        channelRepository.UnitOfWork.SaveChanges();
      }
    }

    public static ChannelMap GetChannelMap(int idChannelMap)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<ChannelMap> query = channelRepository.GetQuery<ChannelMap>(c => c.IdChannelMap == idChannelMap);
        return channelRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static ChannelMap SaveChannelMap(ChannelMap map)
    {
      if (OnStateChangedChannelMapEvent != null)
      {
        OnStateChangedChannelMapEvent(map, map.ChangeTracker.State);
      }

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
        Channel channel = channelRepository.IncludeAllRelations(query).FirstOrDefault();
        channel = channelRepository.LoadNavigationProperties(channel);
        return channel;
      }
    }

    public static IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.MediaType == (int)mediaType).OrderBy(c => c.SortOrder);
        query = channelRepository.IncludeAllRelations(query, includeRelations);

        IList<Channel> channels = channelRepository.LoadNavigationProperties(query, includeRelations);
        // Log.Debug("ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations) SQL = {0}", query.ToTraceString());
        return channels;
      }
    }

    public static IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<Channel> query = channelRepository.GetAll<Channel>().OrderBy(c => c.SortOrder);
        query = channelRepository.IncludeAllRelations(query, includeRelations);
        IList<Channel> channels = channelRepository.LoadNavigationProperties(query, includeRelations);
        // Log.Debug("ListAllChannels(ChannelIncludeRelationEnum) SQL = {0}", query.ToTraceString());
        return channels;
      }
    }

    public static IList<Channel> GetAllChannelsByGroupIdAndMediaType(int idGroup, MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      try
      {
        using (IChannelRepository channelRepository = new ChannelRepository())
        {
          IQueryable<Channel> query = channelRepository.GetAllChannelsByGroupIdAndMediaType(idGroup, mediaType);
          query = channelRepository.IncludeAllRelations(query, includeRelations);
          IList<Channel> channels = channelRepository.LoadNavigationProperties(query, includeRelations);
          return channels;
        }
      }
      catch (Exception ex)
      {
        Log.Error("ChannelManagement.GetAllChannelsByGroupIdAndMediaType ex={0}", ex);
        throw;
      }
    }

    public static TuningDetail GetTuningDetail(int tuningDetailId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<TuningDetail> query = channelRepository.GetQuery<TuningDetail>(t => t.IdTuning == tuningDetailId);
        return channelRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static IList<Channel> ListAllChannelsForEpgGrabbing(ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetAll<Channel>().Where(c => (c.MediaType == (int)MediaTypeEnum.TV || c.MediaType == (int)MediaTypeEnum.Radio) 
          && c.GrabEpg && !c.TuningDetails.Any(t => t.ChannelType == 0 || t.ChannelType == 5)).OrderBy(c => c.SortOrder);
        return channelRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }
  }
}
