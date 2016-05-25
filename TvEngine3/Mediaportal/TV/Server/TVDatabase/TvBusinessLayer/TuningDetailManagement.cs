using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class TuningDetailManagement
  {
    public delegate void OnStateChangedTuningDetailDelegate(int channelId);
    public static event OnStateChangedTuningDetailDelegate OnStateChangedTuningDetailEvent;

    public static IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.IdChannel == idChannel).OrderBy(td => td.Priority);
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static TuningDetail GetTuningDetail(int idTuningDetail)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.IdTuning == idTuningDetail);
        return tuningDetailRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.AnalogTelevision && td.PhysicalChannelNumber == physicalChannelNumber);
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandard, string logicalChannelNumber, int? frequency = null)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)broadcastStandard && string.Equals(td.LogicalChannelNumber, logicalChannelNumber));
        if (frequency.HasValue)
        {
          query = query.Where(td => td.Frequency == frequency.Value);
        }
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetCaptureTuningDetails(string name)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.ExternalInput && string.Equals(td.Name, name));
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandard, int originalNetworkId, int serviceId, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => (td.BroadcastStandard & (int)broadcastStandard) != 0 && td.OriginalNetworkId == originalNetworkId && td.ServiceId == serviceId);
        if (transportStreamId.HasValue)
        {
          query = query.Where(td => td.TransportStreamId == transportStreamId.Value);
        }
        if (frequency.HasValue)
        {
          query = query.Where(td => td.Frequency == frequency.Value);
        }
        if (satelliteId.HasValue)
        {
          query = query.Where(td => td.SatIndex == satelliteId.Value);
        }
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetFmRadioTuningDetails(int frequency)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.FmRadio && td.Frequency == frequency);
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetFreesatTuningDetails(int channelId)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.FreesatChannelId == channelId);
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandard, int programNumber, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return GetDvbTuningDetails(broadcastStandard, 0, programNumber, transportStreamId, frequency, satelliteId);
    }

    public static IList<TuningDetail> GetOpenTvTuningDetails(int channelId)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.OpenTvChannelId == channelId);
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetStreamTuningDetails(string url)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.DvbIp && string.Equals(td.Url, url));
        query = tuningDetailRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    // TODO This is one of the worst hacks I've ever written. Please remove if possible (!!!).
    [Obsolete("Do not use this function. It is a huge and terrible hack!")]
    public static TuningDetail GetClosestMatchTuningDetailWithoutId(IChannel channel)
    {
      int broadcastStandard = (int)GetBroadcastStandardFromChannelInstance(channel);
      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query;
        if (physicalChannel != null)
        {
          query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == broadcastStandard && string.Equals(td.Name, channel.Name) && td.Frequency == physicalChannel.Frequency);
        }
        else
        {
          query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == broadcastStandard && string.Equals(td.Name, channel.Name));
        }
        return tuningDetailRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static void AddTuningDetail(int idChannel, IChannel channel)
    {
      TuningDetail tuningDetail = new TuningDetail();
      TuningDetail detail = UpdateTuningDetailWithChannelData(idChannel, channel, tuningDetail);
      detail.Priority = 1;
      tuningDetail.IdChannel = idChannel;
      SaveTuningDetail(detail);
    }

    public static void UpdateTuningDetail(int idChannel, int idTuningDetail, IChannel channel)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(t => t.IdTuning == idTuningDetail && t.IdChannel == idChannel);
        TuningDetail tuningDetail = UpdateTuningDetailWithChannelData(idChannel, channel, query.FirstOrDefault());
        SaveTuningDetail(tuningDetail);
      }
    }

    private static TuningDetail UpdateTuningDetailWithChannelData(int idChannel, IChannel channel, TuningDetail tuningDetail)
    {
      tuningDetail.Name = channel.Name;
      tuningDetail.Provider = channel.Provider;
      tuningDetail.LogicalChannelNumber = channel.LogicalChannelNumber;
      tuningDetail.MediaType = (int)channel.MediaType;
      tuningDetail.IsEncrypted = channel.IsEncrypted;
      tuningDetail.IsHighDefinition = channel.IsHighDefinition;
      tuningDetail.IsThreeDimensional = channel.IsThreeDimensional;

      IChannelOfdm ofdmChannel = channel as IChannelOfdm;
      if (ofdmChannel != null)
      {
        tuningDetail.Bandwidth = ofdmChannel.Bandwidth;
      }

      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      if (physicalChannel != null)
      {
        tuningDetail.Frequency = physicalChannel.Frequency;
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel != null)
      {
        tuningDetail.SatIndex = satelliteChannel.DiseqcPositionerSatelliteIndex;
        tuningDetail.DiSEqC = (int)satelliteChannel.DiseqcSwitchPort;
        tuningDetail.IdLnbType = satelliteChannel.LnbType.Id;
        tuningDetail.Polarisation = (int)satelliteChannel.Polarisation;
        tuningDetail.Modulation = (int)satelliteChannel.ModulationScheme;
        tuningDetail.SymbolRate = satelliteChannel.SymbolRate;
        tuningDetail.FecCodeRate = (int)satelliteChannel.FecCodeRate;
      }

      ChannelDvbBase dvbChannel = channel as ChannelDvbBase;
      if (dvbChannel != null)
      {
        tuningDetail.OriginalNetworkId = dvbChannel.OriginalNetworkId;
        tuningDetail.OpenTvChannelId = dvbChannel.OpenTvChannelId;
        tuningDetail.EpgOriginalNetworkId = dvbChannel.EpgOriginalNetworkId;
        tuningDetail.EpgTransportStreamId = dvbChannel.EpgTransportStreamId;
        tuningDetail.EpgServiceId = dvbChannel.EpgServiceId;
      }
      ChannelMpeg2Base mpeg2Channel = channel as ChannelMpeg2Base;
      if (mpeg2Channel != null)
      {
        tuningDetail.PmtPid = mpeg2Channel.PmtPid;
        tuningDetail.ServiceId = mpeg2Channel.ProgramNumber;
        tuningDetail.TransportStreamId = mpeg2Channel.TransportStreamId;
      }

      ChannelAnalogTv analogTvChannel = channel as ChannelAnalogTv;
      if (analogTvChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.AnalogTelevision;
        tuningDetail.PhysicalChannelNumber = analogTvChannel.PhysicalChannelNumber;
        tuningDetail.CountryId = analogTvChannel.Country.Id;
        tuningDetail.TuningSource = (int)analogTvChannel.TunerSource;
      }
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.Atsc;
        tuningDetail.Modulation = (int)atscChannel.ModulationScheme;
        tuningDetail.SourceId = atscChannel.SourceId;
      }
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.ExternalInput;
        tuningDetail.VideoSource = (int)captureChannel.VideoSource;
        tuningDetail.AudioSource = (int)captureChannel.AudioSource;
        tuningDetail.IsVcrSignal = captureChannel.IsVcrSignal;
      }
      if (channel is ChannelDigiCipher2)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DigiCipher2;
      }
      ChannelDvbC dvbcChannel = channel as ChannelDvbC;
      if (dvbcChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbC;
        tuningDetail.Modulation = (int)dvbcChannel.ModulationScheme;
        tuningDetail.SymbolRate = dvbcChannel.SymbolRate;
      }
      ChannelDvbC2 dvbc2Channel = channel as ChannelDvbC2;
      if (dvbcChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbC2;
        tuningDetail.StreamId = dvbc2Channel.PlpId;
      }
      ChannelDvbS dvbsChannel = channel as ChannelDvbS;
      if (dvbsChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbS;
        tuningDetail.FreesatChannelId = dvbsChannel.FreesatChannelId;
      }
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (dvbs2Channel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbS2;
        tuningDetail.FreesatChannelId = dvbs2Channel.FreesatChannelId;
        tuningDetail.PilotTonesState = (int)dvbs2Channel.PilotTonesState;
        tuningDetail.RollOffFactor = (int)dvbs2Channel.RollOffFactor;
        tuningDetail.StreamId = dvbs2Channel.StreamId;
      }
      if (channel is ChannelDvbT)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbT;
      }
      ChannelDvbT2 dvbt2Channel = channel as ChannelDvbT2;
      if (dvbt2Channel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbT2;
        tuningDetail.StreamId = dvbt2Channel.PlpId;
      }
      if (channel is ChannelFmRadio)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.FmRadio;
      }
      if (channel is ChannelSatelliteTurboFec)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.SatelliteTurboFec;
      }
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.Scte;
        tuningDetail.Modulation = (int)scteChannel.ModulationScheme;
        tuningDetail.SourceId = scteChannel.SourceId;
      }
      ChannelStream streamChannel = channel as ChannelStream;
      if (streamChannel != null)
      {
        tuningDetail.BroadcastStandard = (int)BroadcastStandard.DvbIp;
        tuningDetail.Url = streamChannel.Url;
      }
      else
      {
        // URL can't be null. Set it empty for non-stream tuning details.
        tuningDetail.Url = string.Empty;
      }
      return tuningDetail;
    }

    public static TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        // Detect when a tuning detail is moved to a different channel. That
        // scenario requires two events.
        int originalChannelId = -1;
        if (OnStateChangedTuningDetailEvent != null && tuningDetail.IdTuning > 0)
        {
          TuningDetail originalTuningDetail = GetTuningDetail(tuningDetail.IdTuning);
          if (originalTuningDetail != null && tuningDetail.IdChannel != originalTuningDetail.IdChannel)
          {
            originalChannelId = originalTuningDetail.IdChannel;
          }
        }

        tuningDetailRepository.AttachEntityIfChangeTrackingDisabled(tuningDetailRepository.ObjectContext.TuningDetails, tuningDetail);
        tuningDetailRepository.ApplyChanges(tuningDetailRepository.ObjectContext.TuningDetails, tuningDetail);
        tuningDetailRepository.UnitOfWork.SaveChanges();
        tuningDetail.AcceptChanges();

        if (OnStateChangedTuningDetailEvent != null)
        {
          if (originalChannelId != -1)
          {
            OnStateChangedTuningDetailEvent(originalChannelId);
          }
          OnStateChangedTuningDetailEvent(tuningDetail.IdChannel);
        }
        return tuningDetail;
      }
    }

    public static void DeleteTuningDetail(int idTuningDetail)
    {
      TuningDetail tuningDetail = null;
      if (OnStateChangedTuningDetailEvent != null)
      {
        tuningDetail = GetTuningDetail(idTuningDetail);
      }

      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository(true))
      {
        tuningDetailRepository.Delete<TuningDetail>(p => p.IdTuning == idTuningDetail);
        tuningDetailRepository.UnitOfWork.SaveChanges();
      }

      if (OnStateChangedTuningDetailEvent != null && tuningDetail != null)
      {
        OnStateChangedTuningDetailEvent(tuningDetail.IdChannel);
      }
    }

    public static BroadcastStandard GetBroadcastStandardFromChannelInstance(IChannel channel)
    {
      if (channel is ChannelAnalogTv)
      {
        return BroadcastStandard.AnalogTelevision;
      }
      if (channel is ChannelAtsc)
      {
        return BroadcastStandard.Atsc;
      }
      if (channel is ChannelCapture)
      {
        return BroadcastStandard.ExternalInput;
      }
      if (channel is ChannelDigiCipher2)
      {
        return BroadcastStandard.DigiCipher2;
      }
      if (channel is ChannelDvbC)
      {
        return BroadcastStandard.DvbC;
      }
      if (channel is ChannelDvbC2)
      {
        return BroadcastStandard.DvbC2;
      }
      if (channel is ChannelDvbS)
      {
        return BroadcastStandard.DvbS;
      }
      if (channel is ChannelDvbS2)
      {
        return BroadcastStandard.DvbS2;
      }
      if (channel is ChannelDvbT)
      {
        return BroadcastStandard.DvbT;
      }
      if (channel is ChannelDvbT2)
      {
        return BroadcastStandard.DvbT2;
      }
      if (channel is ChannelFmRadio)
      {
        return BroadcastStandard.FmRadio;
      }
      if (channel is ChannelSatelliteTurboFec)
      {
        return BroadcastStandard.SatelliteTurboFec;
      }
      if (channel is ChannelScte)
      {
        return BroadcastStandard.Scte;
      }
      if (channel is ChannelStream)
      {
        return BroadcastStandard.DvbIp;
      }
      return BroadcastStandard.Unknown;
    }

    public static IChannel GetTuningChannel(TuningDetail detail)
    {
      IChannel channel = null;
      switch ((BroadcastStandard)detail.BroadcastStandard)
      {
        case BroadcastStandard.AnalogTelevision:
          ChannelAnalogTv analogTvChannel = new ChannelAnalogTv();
          analogTvChannel.PhysicalChannelNumber = (short)detail.PhysicalChannelNumber;
          analogTvChannel.Country = CountryCollection.Instance.GetCountryById(detail.CountryId);
          analogTvChannel.TunerSource = (AnalogTunerSource)detail.TuningSource;
          channel = analogTvChannel;
          break;
        case BroadcastStandard.Atsc:
          ChannelAtsc atscChannel = new ChannelAtsc();
          atscChannel.ModulationScheme = (ModulationSchemeVsb)detail.Modulation;
          atscChannel.SourceId = detail.SourceId;
          channel = atscChannel;
          break;
        case BroadcastStandard.ExternalInput:
          ChannelCapture captureChannel = new ChannelCapture();
          captureChannel.VideoSource = (CaptureSourceVideo)detail.VideoSource;
          captureChannel.AudioSource = (CaptureSourceAudio)detail.AudioSource;
          captureChannel.IsVcrSignal = detail.IsVcrSignal;
          channel = captureChannel;
          break;
        case BroadcastStandard.DigiCipher2:
          channel = new ChannelDigiCipher2();
          break;
        case BroadcastStandard.DvbC:
          ChannelDvbC dvbcChannel = new ChannelDvbC();
          dvbcChannel.ModulationScheme = (ModulationSchemeQam)detail.Modulation;
          dvbcChannel.SymbolRate = detail.SymbolRate;
          channel = dvbcChannel;
          break;
        case BroadcastStandard.DvbC2:
          ChannelDvbC2 dvbc2Channel = new ChannelDvbC2();
          dvbc2Channel.PlpId = (short)detail.StreamId;
          channel = dvbc2Channel;
          break;
        case BroadcastStandard.DvbS:
          ChannelDvbS dvbsChannel = new ChannelDvbS();
          dvbsChannel.FreesatChannelId = detail.FreesatChannelId;
          channel = dvbsChannel;
          break;
        case BroadcastStandard.DvbS2:
          ChannelDvbS2 dvbs2Channel = new ChannelDvbS2();
          dvbs2Channel.FreesatChannelId = detail.FreesatChannelId;
          dvbs2Channel.PilotTonesState = (PilotTonesState)detail.PilotTonesState;
          dvbs2Channel.RollOffFactor = (RollOffFactor)detail.RollOffFactor;
          dvbs2Channel.StreamId = (short)detail.StreamId;
          channel = dvbs2Channel;
          break;
        case BroadcastStandard.DvbT:
          channel = new ChannelDvbT();
          break;
        case BroadcastStandard.DvbT2:
          ChannelDvbT2 dvbt2Channel = new ChannelDvbT2();
          dvbt2Channel.PlpId = (short)detail.StreamId;
          channel = dvbt2Channel;
          break;
        case BroadcastStandard.FmRadio:
          channel = new ChannelFmRadio();
          break;
        case BroadcastStandard.SatelliteTurboFec:
          channel = new ChannelSatelliteTurboFec();
          break;
        case BroadcastStandard.Scte:
          ChannelScte scteChannel = new ChannelScte();
          scteChannel.ModulationScheme = (ModulationSchemeQam)detail.Modulation;
          scteChannel.SourceId = detail.SourceId;
          channel = scteChannel;
          break;
        case BroadcastStandard.DvbIp:
          ChannelStream streamChannel = new ChannelStream();
          streamChannel.Url = detail.Url;
          channel = streamChannel;
          break;
        default:
          Log.Error("channel management: failed to handle broadcast standard {0} in GetTuningChannel()", (BroadcastStandard)detail.BroadcastStandard);
          return null;
      }

      IChannelOfdm ofdmChannel = channel as IChannelOfdm;
      if (ofdmChannel != null)
      {
        ofdmChannel.Bandwidth = detail.Bandwidth;
      }

      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      if (physicalChannel != null)
      {
        physicalChannel.Frequency = detail.Frequency;
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel != null)
      {
        satelliteChannel.DiseqcPositionerSatelliteIndex = detail.SatIndex;
        satelliteChannel.DiseqcSwitchPort = (DiseqcPort)detail.DiSEqC;
        satelliteChannel.Polarisation = (Polarisation)detail.Polarisation;
        satelliteChannel.ModulationScheme = (ModulationSchemePsk)detail.Modulation;
        satelliteChannel.SymbolRate = detail.SymbolRate;
        satelliteChannel.FecCodeRate = (FecCodeRate)detail.FecCodeRate;
      }

      ChannelDvbBase dvbChannel = channel as ChannelDvbBase;
      if (dvbChannel != null)
      {
        dvbChannel.OriginalNetworkId = detail.OriginalNetworkId;
        dvbChannel.OpenTvChannelId = detail.OpenTvChannelId;
        dvbChannel.EpgOriginalNetworkId = detail.EpgOriginalNetworkId;
        dvbChannel.EpgTransportStreamId = detail.EpgTransportStreamId;
        dvbChannel.EpgServiceId = detail.EpgServiceId;
      }

      ChannelMpeg2Base mpeg2Channel = channel as ChannelMpeg2Base;
      if (mpeg2Channel != null)
      {
        mpeg2Channel.TransportStreamId = detail.TransportStreamId;
        mpeg2Channel.ProgramNumber = detail.ServiceId;
        mpeg2Channel.PmtPid = detail.PmtPid;
      }

      channel.Name = detail.Name;
      channel.Provider = detail.Provider;
      channel.LogicalChannelNumber = detail.LogicalChannelNumber;
      channel.MediaType = (MediaType)detail.MediaType;
      channel.IsEncrypted = detail.IsEncrypted;
      channel.IsHighDefinition = detail.IsHighDefinition;
      channel.IsThreeDimensional = detail.IsThreeDimensional;
      return channel;
    }
  }
}