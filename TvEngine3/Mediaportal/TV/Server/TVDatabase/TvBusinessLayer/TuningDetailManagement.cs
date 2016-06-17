using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
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

    public static IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.IdChannel == idChannel).OrderBy(td => td.Priority);
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> ListAllDigitalTransmitterTuningDetails()
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IList<Satellite> satellites = tuningDetailRepository.GetAll<Satellite>().ToList();
        Dictionary<int, Satellite> satellitesById = new Dictionary<int, Satellite>(satellites.Count);
        foreach (Satellite satellite in satellites)
        {
          satellitesById[satellite.IdSatellite] = satellite;
        }

        var data = tuningDetailRepository.GetQuery<TuningDetail>().Where(td => (td.BroadcastStandard & (int)BroadcastStandard.MaskAnalog) == 0)
                      .GroupBy(
                        group => new TuningDetail
                        {
                          Bandwidth = group.Bandwidth,
                          BroadcastStandard = group.BroadcastStandard,
                          FecCodeRate = group.FecCodeRate,
                          Frequency = group.Frequency,
                          IdSatellite = group.IdSatellite,
                          Modulation = group.Modulation,
                          PilotTonesState = group.PilotTonesState,
                          Polarisation = group.Polarisation,
                          RollOffFactor = group.RollOffFactor,
                          StreamId = group.StreamId,
                          SymbolRate = group.SymbolRate,
                          Url = group.Url
                        }
                      ).Select(
                        select => new
                        {
                          Bandwidth = select.Key.Bandwidth,
                          BroadcastStandard = select.Key.BroadcastStandard,
                          FecCodeRate = select.Key.FecCodeRate,
                          Frequency = select.Key.Frequency,
                          IdSatellite = select.Key.IdSatellite,
                          Modulation = select.Key.Modulation,
                          PilotTonesState = select.Key.PilotTonesState,
                          Polarisation = select.Key.Polarisation,
                          RollOffFactor = select.Key.RollOffFactor,
                          StreamId = select.Key.StreamId,
                          SymbolRate = select.Key.SymbolRate,
                          Url = select.Key.Url,

                          Ids = select.Select(s => s.IdTuning),
                          Names = select.Select(s => s.Name),
                          GrabEpgFlags = select.Select(s => s.GrabEpg),
                          LastEpgGrabTimes = select.Select(s => s.LastEpgGrabTime)
                        }
                      ).ToList();

        IList<TuningDetail> transmitterTuningDetails = new List<TuningDetail>(data.Count);
        foreach (var d in data)
        {
          Satellite satellite = null;
          if (d.IdSatellite.HasValue)
          {
            satellitesById.TryGetValue(d.IdSatellite.Value, out satellite);
          }

          transmitterTuningDetails.Add(new TuningDetail
          {
            Bandwidth = d.Bandwidth,
            BroadcastStandard = d.BroadcastStandard,
            FecCodeRate = d.FecCodeRate,
            Frequency = d.Frequency,
            GrabEpg = !d.GrabEpgFlags.Contains(false),
            IdSatellite = d.IdSatellite,
            LastEpgGrabTime = d.LastEpgGrabTimes.Min(),
            Modulation = d.Modulation,
            Name = string.Join(", ", d.Names.OrderBy(n => n)),
            PilotTonesState = d.PilotTonesState,
            Polarisation = d.Polarisation,
            Provider = string.Join(", ", d.Ids.Select(id => id.ToString())),
            RollOffFactor = d.RollOffFactor,
            Satellite = satellite,
            StreamId = d.StreamId,
            SymbolRate = d.SymbolRate,
            Url = d.Url,

            AudioSource = (int)CaptureSourceAudio.None,
            CountryId = -1,
            EpgOriginalNetworkId = -1,
            EpgServiceId = -1,
            EpgTransportStreamId = -1,
            FreesatChannelId = -1,
            IdChannel = -1,
            IdTuning = -1,
            IsEncrypted = false,
            IsHighDefinition = false,
            IsThreeDimensional = false,
            IsVcrSignal = false,
            LogicalChannelNumber = string.Empty,
            MediaType = (int)MediaType.Television,
            OpenTvChannelId = -1,
            OriginalNetworkId = -1,
            PhysicalChannelNumber = -1,
            PmtPid = -1,
            Priority = -1,
            ServiceId = -1,
            SourceId = -1,
            TransportStreamId = -1,
            TuningSource = (int)AnalogTunerSource.Cable,
            VideoSource = (int)CaptureSourceVideo.None
          });
        }

        return transmitterTuningDetails;
      }
    }

    public static TuningDetail GetTuningDetail(int idTuningDetail, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.IdTuning == idTuningDetail);
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.FirstOrDefault();
      }
    }

    public static IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.AnalogTelevision && td.PhysicalChannelNumber == physicalChannelNumber);
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandard, string logicalChannelNumber, TuningDetailRelation includeRelations, int? frequency = null)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)broadcastStandard && string.Equals(td.LogicalChannelNumber, logicalChannelNumber));
        if (frequency.HasValue)
        {
          query = query.Where(td => td.Frequency == frequency.Value);
        }
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetCaptureTuningDetails(string name, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.ExternalInput && string.Equals(td.Name, name));
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandard, int originalNetworkId, int serviceId, TuningDetailRelation includeRelations, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
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
          query = query.Where(td => td.IdSatellite == satelliteId.Value);
        }
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetFmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.FmRadio && td.Frequency == frequency);
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetFreesatTuningDetails(int channelId, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.FreesatChannelId == channelId);
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandard, int programNumber, TuningDetailRelation includeRelations, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return GetDvbTuningDetails(broadcastStandard, -1, programNumber, includeRelations, transportStreamId, frequency, satelliteId);
    }

    public static IList<TuningDetail> GetOpenTvTuningDetails(int channelId, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.OpenTvChannelId == channelId);
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
    }

    public static IList<TuningDetail> GetStreamTuningDetails(string url, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.DvbIp && string.Equals(td.Url, url));
        query = tuningDetailRepository.IncludeAllRelations(query, includeRelations);
        return query.ToList();
      }
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
          TuningDetail originalTuningDetail = GetTuningDetail(tuningDetail.IdTuning, TuningDetailRelation.None);
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
        tuningDetail = GetTuningDetail(idTuningDetail, TuningDetailRelation.None);
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

    // TODO This is one of the worst hacks I've ever written. Please remove if possible (!!!).
    [Obsolete("Do not use this function. It is a huge and terrible hack!")]
    public static TuningDetail GetClosestMatchTuningDetailWithoutId(IChannel channel, TuningDetailRelation includeRelations)
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
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
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
        if (detail.IdSatellite.HasValue)
        {
          if (detail.Satellite != null)
          {
            satelliteChannel.Longitude = detail.Satellite.Longitude;
          }
          else
          {
            // This is bad for performance. We've tried to avoid this, but just
            // in case...
            Log.Warn("channel management: forced to manually load satellite");
            using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
            {
              Satellite satellite = tuningDetailRepository.GetQuery<Satellite>(s => s.IdSatellite == detail.IdSatellite.Value).FirstOrDefault();
              if (satellite == null)
              {
                Log.Error("channel management: failed to load satellite, ID = {0}", detail.IdSatellite.Value);
                satelliteChannel.Longitude = 0;
              }
              else
              {
                satelliteChannel.Longitude = detail.Satellite.Longitude;
              }
            }
          }
        }

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