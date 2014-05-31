using System;
using System.Collections.Generic;
using System.Linq;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel;
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

    public delegate void OnStateChangedTuningDetailDelegate(ServiceDetail serviceDetail, ObjectState state);
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
            IEnumerable<ServiceDetail> deletedServiceDetails = deletedProperty.OfType<ServiceDetail>();
            foreach (ServiceDetail deletedServiceDetail in deletedServiceDetails)
            {
              OnStateChangedTuningDetailEvent(deletedServiceDetail, ObjectState.Deleted);
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
              IEnumerable<ServiceDetail> addedServiceDetails = addedProperty.OfType<ServiceDetail>();
              foreach (ServiceDetail addedServiceDetail in addedServiceDetails)
              {
                ServiceDetail detail = addedServiceDetail;
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

    public static Channel GetChannelBy(int networkId, int transportId, int serviceId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {                
        IQueryable<Channel> query = channelRepository.GetQuery<Channel>(c => c.ServiceDetails.OfType<ServiceDvb>().Any(t => t.OriginalNetworkId == networkId 
            && t.TransportStreamId == transportId 
            && t.ServiceId == serviceId));
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

    public static ServiceDetail GetServiceDetail(DVBBaseChannel dvbChannel)
    {
      TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.ServiceId;
      tuningDetailSearchEnum |= TuningDetailSearchEnum.NetworkId;
      tuningDetailSearchEnum |= TuningDetailSearchEnum.TransportId;

      return GetServiceDetail(dvbChannel, tuningDetailSearchEnum);
    }

    public static ServiceDetail GetServiceDetail(DVBBaseChannel dvbChannel, TuningDetailSearchEnum tuningDetailSearchEnum)
    {      
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<ServiceDetail> query = GetServiceDetailQueryBasedOnTunerType(dvbChannel, channelRepository, tuningDetailSearchEnum);
        query = ApplyTuningDetailConstraintBasedOnTunerType(dvbChannel, query);      
        query = channelRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }


    private static IQueryable<ServiceDetail> ApplyTuningDetailConstraintBasedOnTunerType(DVBBaseChannel dvbChannel,
                                                             IQueryable<ServiceDetail> query)
    {      
      if (dvbChannel is DVBTChannel)
      {
        query = query.Where(s => s.TuningDetail is TuningDetailTerrestrial);
      }
      else if (dvbChannel is DVBSChannel)
      {
        query = query.Where(s => s.TuningDetail is TuningDetailSatellite);
      }
      else if (dvbChannel is DVBCChannel)
      {
        query = query.Where(s => s.TuningDetail is TuningDetailCable);        
      }
      else if (dvbChannel is DVBIPChannel)
      {
        query = query.Where(s => s.TuningDetail is TuningDetailStream);                
      }
      else // must be ATSCChannel
      {
        query = query.Where(s => s.TuningDetail is TuningDetailAtsc);                
      }
      return query;
    }
    

    private static IQueryable<ServiceDetail> GetServiceDetailQueryBasedOnTunerType(DVBBaseChannel dvbChannel,
                                                              IChannelRepository channelRepository, TuningDetailSearchEnum tuningDetailSearchEnum)
    {
      IQueryable<ServiceDetail> query = channelRepository.GetQuery<ServiceDetail>();
      if (dvbChannel is DVBTChannel || dvbChannel is DVBSChannel || dvbChannel is DVBCChannel || dvbChannel is DVBIPChannel)
      {
        IQueryable<ServiceDvb> queryDvb = query.OfType<ServiceDvb>();

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.NetworkId)))
        {
          queryDvb = queryDvb.Where(a => a.OriginalNetworkId == dvbChannel.NetworkId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.ServiceId)))
        {
          queryDvb = queryDvb.Where(b => b.ServiceId == dvbChannel.ServiceId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.TransportId)))
        {
          queryDvb = queryDvb.Where(c => c.TransportStreamId == dvbChannel.TransportId);
        }

        query = queryDvb;

      }      
      else // must be ATSCChannel
      {
        var queryAtsc = query.OfType<ServiceAtsc>();        
        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.ServiceId)))
        {
          queryAtsc = queryAtsc.Where(b => b.ServiceId == dvbChannel.ServiceId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.TransportId)))
        {
          queryAtsc = queryAtsc.Where(c => c.TransportStreamId == dvbChannel.TransportId);
        }
        query = queryAtsc;
      }
      return query;
    }


    /*
    public static TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel)
    {
      TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.ServiceId;
      tuningDetailSearchEnum |= TuningDetailSearchEnum.NetworkId;
      tuningDetailSearchEnum |= TuningDetailSearchEnum.TransportId;

      return GetTuningDetail(dvbChannel, tuningDetailSearchEnum);
    }

    public static TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel, TuningDetailSearchEnum tuningDetailSearchEnum) 
    {      
      using (IChannelRepository channelRepository = new ChannelRepository())
      {                                              
        IQueryable<TuningDetail> query = GetTuningDetailBasedOnTunerType(dvbChannel, channelRepository);        

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.NetworkId)))
        {          
          query = query.Where(a => a.ServiceDetails.OfType<ServiceDvb>().FirstOrDefault().OriginalNetworkId == dvbChannel.NetworkId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.ServiceId)))
        {
          query = query.Where(b => b.ServiceDetails.OfType<ServiceDvb>().FirstOrDefault().ServiceId == dvbChannel.ServiceId);
        }

        if ((tuningDetailSearchEnum.HasFlag(TuningDetailSearchEnum.TransportId)))
        {
          query = query.Where(c => c.ServiceDetails.OfType<ServiceDvb>().FirstOrDefault().TransportStreamId == dvbChannel.TransportId);
        }

        query = channelRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }                   
    }*/

    /*
    private static IQueryable<TuningDetail> GetTuningDetailBasedOnTunerType(DVBBaseChannel dvbChannel,
                                                              IChannelRepository channelRepository)
    {
      IQueryable<TuningDetail> query = channelRepository.GetQuery<TuningDetail>();
      if (dvbChannel is DVBTChannel)
      {
        query = query.OfType<TuningDetailTerrestrial>();
      }
      else if (dvbChannel is DVBSChannel)
      {
        query = query.OfType<TuningDetailSatellite>();        
      }
      else if (dvbChannel is DVBCChannel)
      {
        query = query.OfType<TuningDetailCable>();
      }
      else if (dvbChannel is DVBIPChannel)
      {
        query = query.OfType<TuningDetailStream>();
      }
      else // must be ATSCChannel
      {
        query = query.OfType<TuningDetailAtsc>();
      }
      return query;
    }
    */

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
      IList<ServiceDetail> serviceDetails = channel.ServiceDetails;
      return serviceDetails.Select(detail => GetTuningChannel(detail)).ToList();
    }

    public static IChannel GetTuningChannel(ServiceDetail serviceDetail, int? cardId = null)
    {
      IChannel channel = null;
      if (serviceDetail.TuningDetail is TuningDetailAnalog)
      {
        channel = GetAnalogTuningChannel(serviceDetail);        
      }

      else if (serviceDetail.TuningDetail is TuningDetailAtsc)
      {
        channel = GetAtscTuningChannel(serviceDetail);        
      }

      else if (serviceDetail.TuningDetail is TuningDetailCable)
      {
        channel = GetCableTuningChannel(serviceDetail);        
      }
                
      else if (serviceDetail.TuningDetail is TuningDetailSatellite)
      {
        channel = GetSatelliteTuningChannel(serviceDetail, cardId);        
      }

      else if (serviceDetail.TuningDetail is TuningDetailTerrestrial)
      {
        channel = GetTerrestrialTuningChannel(serviceDetail);        
      }

      else if (serviceDetail.TuningDetail is TuningDetailStream)
      {
        channel = GetStreamingTuningChannel(serviceDetail);         
      }

      return channel;
    }

    private static DVBIPChannel GetStreamingTuningChannel(ServiceDetail serviceDetail)
    {
      var tuningDetail = serviceDetail.TuningDetail as TuningDetailStream;
      if (tuningDetail == null)
      {
        throw new InvalidCastException("serviceDetail.TuningDetail was not of type TuningDetailStream");
      }
        
      var serviceDetailDvb = (serviceDetail as ServiceDvb);

      var dvbipChannel = new DVBIPChannel();

      if (serviceDetail.EncryptionScheme != null)
      {
        dvbipChannel.EncryptionScheme = (EncryptionSchemeEnum) serviceDetail.EncryptionScheme.Value;
      }

      dvbipChannel.MediaType = (MediaTypeEnum) serviceDetail.MediaType;
      dvbipChannel.Name = serviceDetail.Name;
      dvbipChannel.NetworkId = serviceDetailDvb.OriginalNetworkId.Value;

      if (serviceDetailDvb.PmtPid != null)
      {
        dvbipChannel.PmtPid = serviceDetailDvb.PmtPid.Value;
      }
      dvbipChannel.Provider = serviceDetailDvb.Provider;
      if (serviceDetailDvb.ServiceId != null)
      {
        dvbipChannel.ServiceId = serviceDetailDvb.ServiceId.Value;
      }
      if (serviceDetailDvb.TransportStreamId != null)
      {
        dvbipChannel.TransportId = serviceDetailDvb.TransportStreamId.Value;
      }

      int logicalChannelNumber;
      if (int.TryParse(serviceDetailDvb.LogicalChannelNumber, out logicalChannelNumber))
      {
        dvbipChannel.LogicalChannelNumber = logicalChannelNumber; 
      }      

      dvbipChannel.Url = tuningDetail.Url;
      return dvbipChannel;
    }

    private static DVBTChannel GetTerrestrialTuningChannel(ServiceDetail serviceDetail)
    {
      var tuningDetail = serviceDetail.TuningDetail as TuningDetailTerrestrial;

      if (tuningDetail == null)
      {
        throw new InvalidCastException("serviceDetail.TuningDetail was not of type TuningDetailTerrestrial");
      }

      var serviceDetailDvb = (serviceDetail as ServiceDvb);

      if (serviceDetailDvb == null)
      {
        throw new ArgumentException("was not of type ServiceDvb", "serviceDetail");
      }

      var dvbtChannel = new DVBTChannel();
      if (tuningDetail.Bandwidth != null)
      {
        dvbtChannel.Bandwidth = tuningDetail.Bandwidth.Value;
      }

      if (serviceDetail.EncryptionScheme != null)
      {
        dvbtChannel.EncryptionScheme = (EncryptionSchemeEnum) serviceDetail.EncryptionScheme.Value;
      }

      if (tuningDetail.Frequency != null) dvbtChannel.Frequency = tuningDetail.Frequency.Value;
      dvbtChannel.MediaType = (MediaTypeEnum) serviceDetail.MediaType;
      dvbtChannel.Name = serviceDetail.Name;
      dvbtChannel.NetworkId = serviceDetailDvb.OriginalNetworkId.Value;
      if (serviceDetailDvb.PmtPid != null)
      {
        dvbtChannel.PmtPid = serviceDetailDvb.PmtPid.Value;
      }
      dvbtChannel.Provider = serviceDetailDvb.Provider;
      if (serviceDetailDvb.ServiceId != null)
      {
        dvbtChannel.ServiceId = serviceDetailDvb.ServiceId.Value;
      }
      if (serviceDetailDvb.TransportStreamId != null)
      {
        dvbtChannel.TransportId = serviceDetailDvb.TransportStreamId.Value;
      }

      int logicalChannelNumber;
      if (int.TryParse(serviceDetailDvb.LogicalChannelNumber, out logicalChannelNumber))
      {
        dvbtChannel.LogicalChannelNumber = logicalChannelNumber;
      }      
     
      return dvbtChannel;
    }

    private static DVBSChannel GetSatelliteTuningChannel(ServiceDetail serviceDetail, int? cardId = null)
    {
      var tuningDetail = serviceDetail.TuningDetail as TuningDetailSatellite;

      if (tuningDetail == null)
      {
        throw new InvalidCastException("serviceDetail.TuningDetail was not of type TuningDetailSatellite");
      }

      var serviceDetailDvb = (serviceDetail as ServiceDvb);

      if (serviceDetailDvb == null)
      {
        throw new ArgumentException("was not of type ServiceDvb", "serviceDetail");
      }

      var dvbsChannel = new DVBSChannel();

      if (cardId.HasValue)
      {
        TunerSatellite tuner = tuningDetail.Satellite.TunerSatellites.FirstOrDefault(t => t.IdCard == cardId);
        if (tuner != null)
        {
          dvbsChannel.Diseqc = (DiseqcPort)tuner.DiseqcSwitchSetting;
          dvbsChannel.LnbType = tuner.LnbType;

          if (tuner.DiseqcMotorPosition != null)
          {
            dvbsChannel.SatelliteIndex = tuner.DiseqcMotorPosition.Value;
          }
        } 
      }  
    
      if (tuningDetail.Polarisation != null)
      {
        dvbsChannel.Polarisation = (Polarisation) tuningDetail.Polarisation.Value;
      }

      if (serviceDetail.EncryptionScheme != null)
      {
        dvbsChannel.EncryptionScheme = (EncryptionSchemeEnum) serviceDetail.EncryptionScheme.Value;
      }


      if (tuningDetail.Frequency != null)
      {
        dvbsChannel.Frequency = tuningDetail.Frequency.Value;
      }

      dvbsChannel.MediaType = (MediaTypeEnum) serviceDetail.MediaType;
      dvbsChannel.Name = serviceDetail.Name;
      if (serviceDetailDvb.OriginalNetworkId != null)
      {
        dvbsChannel.NetworkId = serviceDetailDvb.OriginalNetworkId.Value;
      }

      if (serviceDetailDvb.PmtPid != null)
      {
        dvbsChannel.PmtPid = serviceDetailDvb.PmtPid.Value;
      }


      dvbsChannel.Provider = serviceDetailDvb.Provider;
      if (serviceDetailDvb.ServiceId != null)
      {
        dvbsChannel.ServiceId = serviceDetailDvb.ServiceId.Value;
      }
      if (tuningDetail.SymbolRate != null)
      {
        dvbsChannel.SymbolRate = tuningDetail.SymbolRate.Value;
      }
      if (serviceDetailDvb.TransportStreamId != null)
      {
        dvbsChannel.TransportId = serviceDetailDvb.TransportStreamId.Value;
      }

      if (tuningDetail.Modulation != null)
      {
        dvbsChannel.ModulationType = (ModulationType) tuningDetail.Modulation.Value;
      }
      if (tuningDetail.FecRate != null)
      {
        dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate) tuningDetail.FecRate.Value;
      }

      if (serviceDetail.TuningDetail is TuningDetailDvbS2)
      {
        var tuningDetailDvbS2 = serviceDetail.TuningDetail as TuningDetailDvbS2;
        dvbsChannel.Pilot = (Pilot) tuningDetailDvbS2.Pilot.Value;
        dvbsChannel.RollOff = (RollOff) tuningDetailDvbS2.RollOff.Value;
      }      

      int logicalChannelNumber;
      if (int.TryParse(serviceDetailDvb.LogicalChannelNumber, out logicalChannelNumber))
      {
        dvbsChannel.LogicalChannelNumber = logicalChannelNumber;
      }      

      return dvbsChannel;
    }

    private static DVBCChannel GetCableTuningChannel(ServiceDetail serviceDetail)
    {
      var tuningDetail = serviceDetail.TuningDetail as TuningDetailCable;

      if (tuningDetail == null)
      {
        throw new InvalidCastException("serviceDetail.TuningDetail was not of type TuningDetailCable");
      }

      var serviceDetailDvb = (serviceDetail as ServiceDvb);

      if (serviceDetailDvb == null)
      {
        throw new ArgumentException("was not of type ServiceDvb", "serviceDetail");
      }

      var dvbcChannel = new DVBCChannel();
      if (tuningDetail.Modulation != null)
      {
        dvbcChannel.ModulationType = (ModulationType) tuningDetail.Modulation.Value;
      }

      if (serviceDetail.EncryptionScheme != null)
      {
        dvbcChannel.EncryptionScheme = (EncryptionSchemeEnum) serviceDetail.EncryptionScheme.Value;
      }

      if (tuningDetail.Frequency != null)
      {
        dvbcChannel.Frequency = tuningDetail.Frequency.Value;
      }

      dvbcChannel.MediaType = (MediaTypeEnum) serviceDetail.MediaType;
      dvbcChannel.Name = serviceDetail.Name;

      if (serviceDetailDvb.OriginalNetworkId.HasValue)
      {
        dvbcChannel.NetworkId = serviceDetailDvb.OriginalNetworkId.Value;
      }

      if (serviceDetailDvb.PmtPid.HasValue)
      {
        dvbcChannel.PmtPid = serviceDetailDvb.PmtPid.Value;
      }

      dvbcChannel.Provider = serviceDetailDvb.Provider;

      if (serviceDetailDvb.ServiceId.HasValue)
      {
        dvbcChannel.ServiceId = serviceDetailDvb.ServiceId.Value;
      }

      if (tuningDetail.SymbolRate != null)
      {
        dvbcChannel.SymbolRate = tuningDetail.SymbolRate.Value;
      }
      if (serviceDetailDvb.TransportStreamId != null)
      {
        dvbcChannel.TransportId = serviceDetailDvb.TransportStreamId.Value;
      }
   
      int logicalChannelNumber;
      if (int.TryParse(serviceDetailDvb.LogicalChannelNumber, out logicalChannelNumber))
      {
        dvbcChannel.LogicalChannelNumber = logicalChannelNumber;
      }      

      return dvbcChannel;
    }

    private static ATSCChannel GetAtscTuningChannel(ServiceDetail serviceDetail)
    {
      var tuningDetail = serviceDetail.TuningDetail as TuningDetailAtsc;

      if (tuningDetail == null)
      {
        throw new InvalidCastException("serviceDetail.TuningDetail was not of type TuningDetailAtsc");
      }

      var serviceDetailDvb = (serviceDetail as ServiceDvb);

      if (serviceDetailDvb == null)
      {
        throw new ArgumentException("was not of type ServiceDvb", "serviceDetail");
      }

      var atscChannel = new ATSCChannel();
      var atscChannels = serviceDetail.LogicalChannelNumber.Split('.');

      if (atscChannels.Length != 2)
      {
        throw new Exception(string.Format("atsc: could not determine Major and Minor channels from: {0}", serviceDetail.LogicalChannelNumber));
      }

      int majorCh;
      if (!int.TryParse(atscChannels[0], out majorCh))
      {
        throw new Exception(string.Format("atsc: could not parse Majorchannel from string: {0}", atscChannels[0]));
      }

      int minorCh;
      if (!int.TryParse(atscChannels[1], out minorCh))
      {
        throw new Exception(string.Format("atsc: could not parse Minorchannel from string: {0}", atscChannels[1]));
      }

      atscChannel.MajorChannel = majorCh;
      atscChannel.MinorChannel = minorCh;


      //serviceDetailDvb.LogicalChannelNumber

      if (tuningDetail.PhysicalChannel != null)
      {
        atscChannel.PhysicalChannel = tuningDetail.PhysicalChannel.Value;
      }

      if (serviceDetail.EncryptionScheme != null)
      {
        atscChannel.EncryptionScheme = (EncryptionSchemeEnum) serviceDetail.EncryptionScheme.Value;
      }

      if (tuningDetail.Frequency != null)
      {
        atscChannel.Frequency = tuningDetail.Frequency.Value;
      }
      atscChannel.MediaType = (MediaTypeEnum) serviceDetail.MediaType;
      atscChannel.Name = serviceDetail.Name;
      if (serviceDetailDvb.OriginalNetworkId != null)
      {
        atscChannel.NetworkId = serviceDetailDvb.OriginalNetworkId.Value;
      }


      if (serviceDetailDvb.PmtPid.HasValue)
      {
        atscChannel.PmtPid = serviceDetailDvb.PmtPid.Value;
      }

      atscChannel.Provider = serviceDetailDvb.Provider;
      if (serviceDetailDvb.ServiceId.HasValue)
      {
        atscChannel.ServiceId = serviceDetailDvb.ServiceId.Value;
      }
      //atscChannel.SymbolRate = detail.Symbolrate;
      if (serviceDetailDvb.TransportStreamId != null)
      {
        atscChannel.TransportId = serviceDetailDvb.TransportStreamId.Value;
      }

      if (tuningDetail.Modulation != null)
      {
        atscChannel.ModulationType = (ModulationType) tuningDetail.Modulation.Value;
      }
      return atscChannel;
    }

    private static AnalogChannel GetAnalogTuningChannel(ServiceDetail serviceDetail)
    {
      var tuningDetail = serviceDetail.TuningDetail as TuningDetailAnalog;

      if (tuningDetail == null)
      {
        throw new InvalidCastException("serviceDetail.TuningDetail was not of type TuningDetailAnalog");
      }

      var analogChannel = new AnalogChannel();
   
      int logicalChannelNumber;
      if (int.TryParse(serviceDetail.LogicalChannelNumber, out logicalChannelNumber))
      {
        analogChannel.ChannelNumber = logicalChannelNumber;
      }      

      var collection = new CountryCollection();

      if (tuningDetail.IdCountry != null)
      {
        analogChannel.Country = collection.Countries[tuningDetail.IdCountry.Value];
      }

      if (tuningDetail.Frequency != null)
      {
        analogChannel.Frequency = tuningDetail.Frequency.Value;
      }

      analogChannel.MediaType = (MediaTypeEnum) serviceDetail.MediaType;
      analogChannel.Name = serviceDetail.Name;

      if (tuningDetail.SignalSource != null)
      {
        analogChannel.TunerSource = (TunerInputType) tuningDetail.SignalSource.Value;
      }
      if (tuningDetail.VideoSource != null)
      {
        analogChannel.VideoSource = (AnalogChannel.VideoInputType) tuningDetail.VideoSource.Value;
      }
      if (tuningDetail.AudioSource != null)
      {
        analogChannel.AudioSource = (AnalogChannel.AudioInputType)tuningDetail.AudioSource.Value;
      }
      return analogChannel;
    }

    public static ServiceDetail SaveServiceDetail(ServiceDetail detail)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        channelRepository.AttachEntityIfChangeTrackingDisabled(channelRepository.ObjectContext.ServiceDetails, detail);
        channelRepository.ApplyChanges(channelRepository.ObjectContext.ServiceDetails, detail);
        channelRepository.UnitOfWork.SaveChanges();
        detail.AcceptChanges();
        return detail;
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

      ChannelIncludeRelationEnum include = ChannelIncludeRelationEnum.TuningDetails;
      include |= ChannelIncludeRelationEnum.ChannelMapsCard;
      include |= ChannelIncludeRelationEnum.GroupMaps;
      include |= ChannelIncludeRelationEnum.GroupMapsChannelGroup;
      include |= ChannelIncludeRelationEnum.ChannelMaps;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelLink;
      include |= ChannelIncludeRelationEnum.ChannelLinkMapsChannelPortal;
      include |= ChannelIncludeRelationEnum.Recordings;

      channels = channelRepository.IncludeAllRelations(channels, include);
      Channel channel = channels.FirstOrDefault();
      channelRepository.LoadNavigationProperties(channel, include);

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
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        if (dvbChannel is DVBIPChannel)
        {
          IQueryable<TuningDetail> query = channelRepository.GetQuery<TuningDetail>().OfType<TuningDetailStream>().Where(t => t.Url == url);          
          query = channelRepository.IncludeAllRelations(query);
          return query.FirstOrDefault();
        }
        else
        {
          return null;
        }        
      }
    }

    private static IQueryable<TuningDetail> GetQueryBasedOnChannelType(DVBBaseChannel dvbChannel, IChannelRepository channelRepository)
    {
      IQueryable<TuningDetail> basic = null;

      if (dvbChannel is DVBTChannel)
      {
        basic = channelRepository.GetQuery<TuningDetailTerrestrial>();
      }

      else if (dvbChannel is DVBSChannel)
      {
        basic = channelRepository.GetQuery<TuningDetailSatellite>();
      }

      else if (dvbChannel is DVBCChannel)
      {
        basic = channelRepository.GetQuery<TuningDetailCable>();
      }

      else if (dvbChannel is DVBIPChannel)
      {
        basic = channelRepository.GetQuery<TuningDetailStream>();
      }
      else if (dvbChannel is ATSCChannel)
      {
        basic = channelRepository.GetQuery<TuningDetailAtsc>();
      }
      return basic;
    }

    private static ServiceDetail CreateNewServiceDetailTypeBasedOnTuningChannel (IChannel channel)
    {
      ServiceDetail serviceDetail = null;
      TuningDetail tuningDetail = null;

      if (channel is DVBBaseChannel)
      {
        if (channel is ATSCChannel)
        {
          serviceDetail = new ServiceAtsc();
          tuningDetail = new TuningDetailAtsc();
        }
        else
        {
          serviceDetail = new ServiceDvb();

          if (channel is DVBSChannel)
          {
            tuningDetail = new TuningDetailSatellite();
          }
          else if (channel is DVBCChannel)
          {
            tuningDetail = new TuningDetailCable();
          }

          else if (channel is DVBTChannel)
          {
            tuningDetail = new TuningDetailTerrestrial();
          }
          else if (channel is DVBIPChannel)
          {
            tuningDetail = new TuningDetailStream();
          }          
        }        
      }

      
      serviceDetail.TuningDetail = tuningDetail;


      return serviceDetail;
    }

    public static void AddServiceDetail(int idChannel, IChannel channel, int idCard)
    {
      ServiceDetail serviceDetail = CreateNewServiceDetailTypeBasedOnTuningChannel(channel);
      serviceDetail = UpdateServiceDetailWithChannelData(idChannel, channel, serviceDetail, idCard);

      serviceDetail.IdChannel = idChannel;

      SaveServiceDetail(serviceDetail);

      if (OnStateChangedTuningDetailEvent != null)
      {
        OnStateChangedTuningDetailEvent(serviceDetail, ObjectState.Added);
      }
    }
    public static void UpdateServiceDetail(int idChannel, int idTuning, IChannel channel, int idCard)
    {

      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        var query = channelRepository.GetQuery<ServiceDetail>(t => t.IdChannel == idChannel && t.IdTuningDetail == idTuning);
        ServiceDetail detail = query.FirstOrDefault();

        detail = UpdateServiceDetailWithChannelData(idChannel, channel, detail, idCard);
        SaveServiceDetail(detail);

        if (OnStateChangedTuningDetailEvent != null)
        {
          OnStateChangedTuningDetailEvent(detail, ObjectState.Modified);
        }
      }
    }

    private static ServiceDetail UpdateServiceDetailWithChannelData(int idChannel, IChannel channel, ServiceDetail serviceDetail, int idCard)
    {
      var tuningDetail = serviceDetail.TuningDetail;

      EncryptionSchemeEnum encryptionScheme = EncryptionSchemeEnum.Free;
      MediaTypeEnum mediaType = MediaTypeEnum.TV;

      string channelName = "";
      string logicalChannelNumber = null;
            

      if (channel is DVBBaseChannel)
      {
        long channelFrequency = 0;
        UpdateDvbServiceDetailWithChannelData(channel, serviceDetail, out encryptionScheme, out channelName, out mediaType, out channelFrequency);

        if (channel is ATSCChannel)
        {
          UpdateAtscTuningDetailWithChannelData(channel, channelFrequency, tuningDetail, out logicalChannelNumber);
        }

        if (channel is DVBCChannel)
        {
          UpdateCableTuningDetailWithChannelData(idChannel, channel, channelFrequency, tuningDetail, out logicalChannelNumber);
        }

        if (channel is DVBSChannel)
        {
          UpdateSatelliteTuningDetailWithChannelData(idChannel, channel, idCard, channelFrequency, tuningDetail, out logicalChannelNumber);
        }


        if (channel is DVBTChannel)
        {
          UpdateTerrestrialTuningDetailWithChannelData(channel, channelFrequency, tuningDetail, out logicalChannelNumber);
        }

        if (channel is DVBIPChannel)
        {
          UpdateStreamingTuningDetailWithChannelData(channel, tuningDetail, out logicalChannelNumber);
        }

      }
      else
      {
        if (channel is AnalogChannel)
        {
          UpdateAnalogTuningDetailWithChannelData(channel, tuningDetail, out mediaType, out channelName, out logicalChannelNumber);
        }  
      }

      UpdateCommonServiceDetailWithChannelData(serviceDetail, logicalChannelNumber, encryptionScheme, channelName, mediaType);


      /*TuningDetail detail = TuningDetailFactory.CreateTuningDetail(idChannel, channelName, provider,
                                             channelType, channelNumber, (int)channelFrequency, country, mediaType,
                                             networkId, transportId, serviceId, pmtPid, freeToAir,
                                             modulation, polarisation, symbolRate, diseqc, switchFrequency,
                                             bandwidth, majorChannel, minorChannel, videoInputType,
                                             audioInputType, isVCRSignal, tunerSource, band,
                                             satIndex,
                                             innerFecRate, pilot, rollOff, url, 0);*/
      return serviceDetail;
    }

    private static void UpdateDvbServiceDetailWithChannelData(IChannel channel, ServiceDetail serviceDetail,
                                                              out EncryptionSchemeEnum encryptionScheme,
                                                              out string channelName, out MediaTypeEnum mediaType,
                                                              out long channelFrequency)
    {
      encryptionScheme = EncryptionSchemeEnum.Free;
      channelName = null;
      mediaType = MediaTypeEnum.TV;
      channelFrequency = 0;

      var dvbChannel = channel as DVBBaseChannel;
      if (dvbChannel != null)
      {
        channelName = dvbChannel.Name;
        channelFrequency = dvbChannel.Frequency;
        mediaType = dvbChannel.MediaType;
        encryptionScheme = dvbChannel.EncryptionScheme;

        if (serviceDetail is ServiceDvb)
        {
          var serviceDetailDvb = serviceDetail as ServiceDvb;
          serviceDetailDvb.Provider = dvbChannel.Provider;
          serviceDetailDvb.OriginalNetworkId = dvbChannel.NetworkId;
          serviceDetailDvb.TransportStreamId = dvbChannel.TransportId;
          serviceDetailDvb.ServiceId = dvbChannel.ServiceId;
          serviceDetailDvb.PmtPid = dvbChannel.PmtPid;
        }
        else
        {
          throw new Exception("DVBBaseChannel expected a servicedetail of type ServiceDvb");
        }
      }
      
    }

    private static void UpdateAnalogTuningDetailWithChannelData(IChannel channel, TuningDetail tuningDetail,
                                                                 out MediaTypeEnum mediaType, out string channelName,
                                                                 out string logicalChannelNumber)
    {
      //todo gibman- fetch tuningdetails from DB if they exist

      mediaType = MediaTypeEnum.TV;
      channelName = null;
      logicalChannelNumber = null;

      var analogChannel = channel as AnalogChannel;
      if (analogChannel != null)
      {
        channelName = analogChannel.Name;
        logicalChannelNumber = Convert.ToString(analogChannel.ChannelNumber);
        mediaType = analogChannel.MediaType;

        if (tuningDetail is TuningDetailAnalog)
        {
          var tuningDetailAnalog = ((TuningDetailAnalog) tuningDetail);
          tuningDetailAnalog.IdCountry = analogChannel.Country.Index;
          tuningDetailAnalog.VideoSource = (int) analogChannel.VideoSource;
          tuningDetailAnalog.AudioSource = (int) analogChannel.AudioSource;
          tuningDetailAnalog.IsVcrSignal = analogChannel.IsVcrSignal;
          tuningDetailAnalog.SignalSource = (int) analogChannel.TunerSource;
          tuningDetailAnalog.Frequency = (int) analogChannel.Frequency;
        }
        else
        {
          throw new Exception("AnalogChannel expected a tuningDetail of type TuningDetailAnalog");
        }
      }

    }

    private static void UpdateAtscTuningDetailWithChannelData(IChannel channel,
                                                               long channelFrequency, TuningDetail tuningDetail, out string logicalChannelNumber)
    {
      logicalChannelNumber = null;
      
      var atscChannel = channel as ATSCChannel;
      if (atscChannel != null)
      {
        int majorChannel = atscChannel.MajorChannel;
        int minorChannel = atscChannel.MinorChannel;

        logicalChannelNumber = string.Format("{0}.{1}", majorChannel, minorChannel);
        
        if (tuningDetail is TuningDetailAtsc)
        {
          var tuningDetailAtsc = ((TuningDetailAtsc) tuningDetail);
          tuningDetailAtsc.Modulation = (int) atscChannel.ModulationType;          
          tuningDetailAtsc.PhysicalChannel = atscChannel.PhysicalChannel;          
          tuningDetailAtsc.Frequency = (int) channelFrequency;
        }
        else
        {
          throw new Exception("ATSCChannel expected a tuningDetail of type TuningDetailAtsc");
        }
      }
      
    }

    private static void UpdateCableTuningDetailWithChannelData(int idChannel, IChannel channel, 
                                                                long channelFrequency, TuningDetail tuningDetail, out string logicalChannelNumber)
    {
      logicalChannelNumber = null;
      var dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel != null)
      {
        logicalChannelNumber = Convert.ToString(dvbcChannel.LogicalChannelNumber > 999 ? idChannel : dvbcChannel.LogicalChannelNumber);

        if (tuningDetail is TuningDetailCable)
        {
          int frequency = (int) channelFrequency;
          int modulationType = (int) dvbcChannel.ModulationType;
          int symbolRate = dvbcChannel.SymbolRate;

          using (IChannelRepository channelRepository = new ChannelRepository())
          {
            TuningDetailCable existingTuningDetail = channelRepository.GetQuery<TuningDetail>().OfType<TuningDetailCable>().FirstOrDefault(t => t.Frequency == frequency && t.Modulation == modulationType && t.SymbolRate == symbolRate);

            if (existingTuningDetail != null)
            {
              tuningDetail = existingTuningDetail;
            }
            else
            {
              var tuningDetailCable = ((TuningDetailCable)tuningDetail);
              tuningDetailCable.Frequency = frequency;
              tuningDetailCable.Modulation = modulationType;
              tuningDetailCable.SymbolRate = symbolRate;   
            }
          }                    
        }
        else
        {
          throw new Exception("DVBCChannel expected a tuningDetail of type TuningDetailCable");
        }
      }      
    }

    private static void UpdateSatelliteTuningDetailWithChannelData(int idChannel, IChannel channel, int idCard,
                                                                    long channelFrequency, TuningDetail tuningDetail,
                                                                    out string logicalChannelNumber)
    {

      //todo gibman- fetch tuningdetails from DB if they exist
      logicalChannelNumber = null;

      var dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel != null)
      {
        LnbType lnbType = dvbsChannel.LnbType;
        int diseqc = (int) dvbsChannel.Diseqc;
        int satIndex = dvbsChannel.SatelliteIndex;
        int modulation = (int) dvbsChannel.ModulationType;
        int innerFecRate = (int) dvbsChannel.InnerFecRate;
        int pilot = (int) dvbsChannel.Pilot;
        int rollOff = (int) dvbsChannel.RollOff;
        logicalChannelNumber = Convert.ToString(dvbsChannel.LogicalChannelNumber > 999 ? idChannel : dvbsChannel.LogicalChannelNumber);

        if (tuningDetail is TuningDetailDvbS2)
        {
          var tuningDetailDvbS2 = ((TuningDetailDvbS2) tuningDetail);
          tuningDetailDvbS2.Pilot = pilot;
          tuningDetailDvbS2.RollOff = rollOff;
        }

        if (tuningDetail is TuningDetailSatellite)
        {
          var tuningDetailSatellite = ((TuningDetailSatellite) tuningDetail);
          tuningDetailSatellite.Frequency = (int) channelFrequency;
          tuningDetailSatellite.Modulation = modulation;
          tuningDetailSatellite.Polarisation = (int) dvbsChannel.Polarisation;
          tuningDetailSatellite.SymbolRate = dvbsChannel.SymbolRate;
          tuningDetailSatellite.FecRate = innerFecRate;

          //serviceDetail.SatIndex = satIndex; //tunersat - motor position
          //serviceDetail.DiSEqC = diseqc;
          TunerSatellite tuner =
            tuningDetailSatellite.Satellite.TunerSatellites.FirstOrDefault(
              t => t.DiseqcSwitchSetting == diseqc && t.DiseqcMotorPosition == satIndex);
          if (tuner == null)
          {
            tuner = new TunerSatellite {DiseqcSwitchSetting = diseqc, DiseqcMotorPosition = satIndex, IdCard = idCard};

            if (lnbType != null && tuner.IdLnbType != lnbType.IdLnbType)
            {
              using (IChannelRepository channelRepository = new ChannelRepository())
              {
                var lnbExisting = channelRepository.FindOne<LnbType>(l => l.IdLnbType == lnbType.IdLnbType);
                if (lnbExisting == null)
                {
                  throw new Exception("lnb not found with id :" + lnbType.IdLnbType);
                }
                tuner.IdLnbType = lnbType.IdLnbType;
              }
            }

            tuner.IdSatellite = tuningDetailSatellite.Satellite.IdSatellite;
            using (IChannelRepository channelRepository = new ChannelRepository())
            {
              channelRepository.Add(tuner);
              channelRepository.UnitOfWork.SaveChanges();
            }
          }
          else
          {
            if (lnbType != null && tuner.IdLnbType != lnbType.IdLnbType)
            {
              //update with new lnb type
              using (IChannelRepository channelRepository = new ChannelRepository())
              {
                var lnbExisting = channelRepository.FindOne<LnbType>(l => l.IdLnbType == lnbType.IdLnbType);
                if (lnbExisting == null)
                {
                  throw new Exception("lnb not found with id :" + lnbType.IdLnbType);
                }
                tuner.IdLnbType = lnbType.IdLnbType;
              }

              tuner.IdLnbType = lnbType.IdLnbType;
            }
          }

          tuningDetailSatellite.Satellite.TunerSatellites.Add(tuner);
        }
        else
        {
          throw new Exception("DVBSChannel expected a tuningDetail of type TuningDetailSatellite");
        }
      }
      
    }

    private static void UpdateTerrestrialTuningDetailWithChannelData(IChannel channel, long channelFrequency,
                                                                      TuningDetail tuningDetail,
                                                                      out string logicalChannelNumber)
    {
      //todo gibman- fetch tuningdetails from DB if they exist

      logicalChannelNumber = null;
      var dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel != null)
      {
        logicalChannelNumber = Convert.ToString(dvbtChannel.LogicalChannelNumber);

        if (tuningDetail is TuningDetailTerrestrial)
        {
          var tuningDetailTerrestrial = ((TuningDetailTerrestrial) tuningDetail);
          tuningDetailTerrestrial.Frequency = (int) channelFrequency;
          tuningDetailTerrestrial.Bandwidth = dvbtChannel.Bandwidth;
        }
        else
        {
          throw new Exception("DVBTChannel expected a tuningDetail of type TuningDetailTerrestrial");
        }
      }
      
    }

    private static void UpdateCommonServiceDetailWithChannelData(ServiceDetail serviceDetail,
                                                                          string logicalChannelNumber,
                                                                          EncryptionSchemeEnum encryptionScheme,
                                                                          string channelName, MediaTypeEnum mediaType)
    {
      serviceDetail.Name = channelName;
      serviceDetail.MediaType = (int) mediaType;
      serviceDetail.LogicalChannelNumber = logicalChannelNumber;
      serviceDetail.EncryptionScheme = (int) encryptionScheme;
      serviceDetail.MediaType = (int) mediaType;
    }

    private static void UpdateStreamingTuningDetailWithChannelData(IChannel channel, TuningDetail tuningDetail,
                                                                    out string logicalChannelNumber)
    {
      //todo gibman- fetch tuningdetails from DB if they exist

      logicalChannelNumber = null;
      var dvbipChannel = channel as DVBIPChannel;
      if (dvbipChannel != null)
      {
        logicalChannelNumber = Convert.ToString(dvbipChannel.LogicalChannelNumber);
        if (tuningDetail is TuningDetailStream)
        {
          ((TuningDetailStream)tuningDetail).Url = dvbipChannel.Url;
        }
        else
        {
          throw new Exception("DVBTChannel expected a tuningDetail of type TuningDetailTerrestrial");
        }
      }      
    }

    /*
    public static IList<ServiceDetail> GetTuningDetailsByName<TTuningDetail>(string channelName)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<ServiceDetail> query = channelRepository.GetQuery<ServiceDetail>(t => t.Name == channelName && t.TuningDetail is TTuningDetail);
        query = channelRepository.IncludeAllRelations(query);        
        return query.ToList();
      }
    }*/



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
        ServiceDetail serviceDetail = GetServiceDetail(idTuning);
        if (serviceDetail != null)
        {
          OnStateChangedTuningDetailEvent(serviceDetail, ObjectState.Deleted);
        }
      }

      using (IChannelRepository channelRepository = new ChannelRepository(true))
      {
        channelRepository.Delete<TuningDetail>(p => p.IdTuningDetail == idTuning);
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

    public static ServiceDetail GetServiceDetail(int serviceDetailId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<ServiceDetail> query = channelRepository.GetQuery<ServiceDetail>(t => t.IdServiceDetail == serviceDetailId);
        return query.FirstOrDefault();
        //return channelRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static TuningDetail GetTuningDetail(int tuningDetailId)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {
        IQueryable<TuningDetail> query = channelRepository.GetQuery<TuningDetail>(t => t.IdTuningDetail == tuningDetailId);
        return channelRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static IList<Channel> ListAllChannelsForEpgGrabbing(ChannelIncludeRelationEnum includeRelations)
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {                 
        var query = channelRepository.GetAll<Channel>().Where(c => (c.MediaType == (int)MediaTypeEnum.TV || c.MediaType == (int)MediaTypeEnum.Radio)
          && c.ServiceDetails.Any(t => (t.TuningDetail is TuningDetailAnalog || 
            t.TuningDetail is TuningDetailAtsc || 
            t.TuningDetail is TuningDetailCable ||            
            t.TuningDetail is TuningDetailSatellite ||
            t.TuningDetail is TuningDetailTerrestrial) && t.TuningDetail.GrabEPG == true)).OrderBy(c => c.SortOrder);
        return channelRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static Channel GetChannelFromServiceDetailByName<T>(string channelName) where T : TuningDetail
    {
      using (IChannelRepository channelRepository = new ChannelRepository())
      {                
        IQueryable<ServiceDetail> query = channelRepository.GetQuery<ServiceDetail>();
        ServiceDetail a = query.FirstOrDefault(t => t.Name == channelName && t.TuningDetail is T);
        return a.Channel;
      }                   
    }
  }
}
