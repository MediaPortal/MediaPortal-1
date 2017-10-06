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
    public delegate void OnTuningDetailTuningParametersChangedDelegate(int idChannel);
    public static event OnTuningDetailTuningParametersChangedDelegate OnTuningDetailTuningParametersChanged;

    public delegate void OnTuningDetailMappingsChangedDelegate(int idTuningDetail, int idTuner, ObjectState state);
    public static event OnTuningDetailMappingsChangedDelegate OnTuningDetailMappingsChanged;

    public static IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.IdChannel == idChannel).OrderBy(td => td.Priority);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> ListAllTuningDetailsByBroadcastStandard(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? idSatellite = null)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => (td.BroadcastStandard & (int)broadcastStandards) != 0).OrderBy(td => td.IdTuningDetail);
        if (idSatellite.HasValue)
        {
          query = query.Where(td => td.IdSatellite == idSatellite.Value);
        }
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> ListAllTuningDetailsByMediaType(MediaType mediaType, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.MediaType == (int)mediaType).OrderBy(td => td.Name);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> ListAllTuningDetailsByOriginalNetworkIds(IEnumerable<int> originalNetworkIds, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => originalNetworkIds.Contains(td.OriginalNetworkId)).OrderBy(td => td.IdTuningDetail);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
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

        var data = tuningDetailRepository.GetQuery<TuningDetail>().Where(td => (td.BroadcastStandard & (int)BroadcastStandard.MaskDigital) != 0)
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

                          Ids = select.Select(s => s.IdTuningDetail),
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
            GrabEpg = !d.GrabEpgFlags.Contains(false),  // TODO consider reversing this condition
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
            Url = d.Url
          });
        }

        return transmitterTuningDetails;
      }
    }

    public static TuningDetail GetTuningDetail(int idTuningDetail, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.IdTuningDetail == idTuningDetail);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).FirstOrDefault();
      }
    }

    public static IList<TuningDetail> GetAmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.AmRadio && td.Frequency == frequency);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.AnalogTelevision && td.PhysicalChannelNumber == physicalChannelNumber);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandards, string logicalChannelNumber, TuningDetailRelation includeRelations, int? frequency = null)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)broadcastStandards && string.Equals(td.LogicalChannelNumber, logicalChannelNumber));
        if (frequency.HasValue)
        {
          query = query.Where(td => td.Frequency == frequency.Value);
        }
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetCaptureTuningDetails(string name, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.ExternalInput && string.Equals(td.Name, name));
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetCaptureTuningDetails(int tunerId, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td =>
          td.BroadcastStandard == (int)BroadcastStandard.ExternalInput &&
          td.VideoSource != (int)CaptureSourceVideo.TunerDefault &&
          td.AudioSource != (int)CaptureSourceAudio.TunerDefault &&
          td.Name.StartsWith(string.Format("Tuner {0} ", tunerId)) &&
          td.Provider == "External Input"
        );
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandards, int originalNetworkId, TuningDetailRelation includeRelations, int? serviceId = null, int? transportStreamId = null, int? frequency = null, int? idSatellite = null)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => (td.BroadcastStandard & (int)broadcastStandards) != 0);
        if (originalNetworkId > 0)
        {
          query = query.Where(td => td.OriginalNetworkId == originalNetworkId);
        }
        if (serviceId.HasValue)
        {
          query = query.Where(td => td.ServiceId == serviceId.Value);
        }
        if (transportStreamId.HasValue)
        {
          query = query.Where(td => td.TransportStreamId == transportStreamId.Value);
        }
        if (frequency.HasValue)
        {
          query = query.Where(td => td.Frequency == frequency.Value);
        }
        if (idSatellite.HasValue)
        {
          query = query.Where(td => td.IdSatellite == idSatellite.Value);
        }
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetExternalTunerTuningDetails(TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td =>
          td.BroadcastStandard == (int)BroadcastStandard.ExternalInput &&
          td.VideoSource == (int)CaptureSourceVideo.TunerDefault &&
          td.AudioSource == (int)CaptureSourceAudio.TunerDefault &&
          td.Provider == "External Tuner"
        );
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetFmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.FmRadio && td.Frequency == frequency);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetFreesatTuningDetails(int channelId, TuningDetailRelation includeRelations)
    {
      BroadcastStandard broadcastStandards = BroadcastStandard.DvbS | BroadcastStandard.DvbS2;
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => (td.BroadcastStandard & (int)broadcastStandards) != 0 && td.FreesatChannelId == channelId);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetMpeg2TsTuningDetails(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? programNumber = null, int? transportStreamId = null, int? frequency = null, int? idSatellite = null)
    {
      return GetDvbTuningDetails(broadcastStandards, -1, includeRelations, programNumber, transportStreamId, frequency, idSatellite);
    }

    public static IList<TuningDetail> GetOpenTvTuningDetails(BroadcastStandard broadcastStandards, int channelId, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => (td.BroadcastStandard & (int)broadcastStandards) != 0 && td.OpenTvChannelId == channelId);
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static IList<TuningDetail> GetStreamTuningDetails(string url, TuningDetailRelation includeRelations)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IQueryable<TuningDetail> query = tuningDetailRepository.GetQuery<TuningDetail>(td => td.BroadcastStandard == (int)BroadcastStandard.DvbIp && string.Equals(td.Url, url));
        return tuningDetailRepository.IncludeAllRelations(query, includeRelations).ToList();
      }
    }

    public static void UpdateTuningDetailEpgInfo(TuningDetail transmitterTuningDetail)
    {
      string[] idStrings = transmitterTuningDetail.Provider.Split(',');
      HashSet<int> ids = new HashSet<int>();
      foreach (string id in idStrings)
      {
        int temp;
        if (int.TryParse(id, out temp))
        {
          ids.Add(temp);
        }
      }
      if (ids.Count == 0)
      {
        return;
      }

      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        IList<TuningDetail> tuningDetails = tuningDetailRepository.GetQuery<TuningDetail>(td => ids.Contains(td.IdTuningDetail) && (td.GrabEpg != transmitterTuningDetail.GrabEpg || td.LastEpgGrabTime != transmitterTuningDetail.LastEpgGrabTime)).ToList();
        if (tuningDetails.Count == 0)
        {
          return;
        }

        HashSet<int> channelIds = new HashSet<int>();
        foreach (TuningDetail tuningDetail in tuningDetails)
        {
          tuningDetail.GrabEpg = transmitterTuningDetail.GrabEpg;
          tuningDetail.LastEpgGrabTime = transmitterTuningDetail.LastEpgGrabTime;
          channelIds.Add(tuningDetail.IdChannel);
        }

        tuningDetailRepository.AttachEntityIfChangeTrackingDisabled(tuningDetailRepository.ObjectContext.TuningDetails, tuningDetails);
        tuningDetailRepository.ApplyChanges(tuningDetailRepository.ObjectContext.TuningDetails, tuningDetails);
        tuningDetailRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //tuningDetailRepository.ObjectContext.AcceptAllChanges();
        foreach (TuningDetail tuningDetail in tuningDetails)
        {
          tuningDetail.AcceptChanges();
        }

        if (OnTuningDetailTuningParametersChanged != null)
        {
          foreach (int channelId in channelIds)
          {
            OnTuningDetailTuningParametersChanged(channelId);
          }
        }
      }
    }

    public static TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        // We support eventing for tuning detail parameter changes and
        // movements between channels, but not indirect mapping modification.
        int originalChannelId = -1;
        if (OnTuningDetailTuningParametersChanged != null && tuningDetail.IdTuningDetail > 0)
        {
          TuningDetail originalTuningDetail = GetTuningDetail(tuningDetail.IdTuningDetail, TuningDetailRelation.None);
          if (originalTuningDetail != null && tuningDetail.IdChannel != originalTuningDetail.IdChannel)
          {
            originalChannelId = originalTuningDetail.IdChannel;
          }
        }

        tuningDetailRepository.AttachEntityIfChangeTrackingDisabled(tuningDetailRepository.ObjectContext.TuningDetails, tuningDetail);
        tuningDetailRepository.ApplyChanges(tuningDetailRepository.ObjectContext.TuningDetails, tuningDetail);
        tuningDetailRepository.UnitOfWork.SaveChanges();
        tuningDetail.AcceptChanges();

        if (OnTuningDetailTuningParametersChanged != null)
        {
          if (originalChannelId != -1)
          {
            OnTuningDetailTuningParametersChanged(originalChannelId);
          }
          OnTuningDetailTuningParametersChanged(tuningDetail.IdChannel);
        }
        return tuningDetail;
      }
    }

    public static void DeleteTuningDetail(int idTuningDetail)
    {
      TuningDetail tuningDetail = null;
      if (OnTuningDetailTuningParametersChanged != null || OnTuningDetailMappingsChanged != null)
      {
        tuningDetail = GetTuningDetail(idTuningDetail, TuningDetailRelation.TunerMappings);
        if (tuningDetail != null && OnTuningDetailMappingsChanged != null)
        {
          foreach (TunerTuningDetailMapping mapping in tuningDetail.TunerMappings)
          {
            OnTuningDetailMappingsChanged(mapping.IdTuningDetail, mapping.IdTuner, ObjectState.Deleted);
          }
        }
      }

      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository(true))
      {
        tuningDetailRepository.Delete<TuningDetail>(td => td.IdTuningDetail == idTuningDetail);
        tuningDetailRepository.UnitOfWork.SaveChanges();
      }

      if (OnTuningDetailTuningParametersChanged != null && tuningDetail != null)
      {
        OnTuningDetailTuningParametersChanged(tuningDetail.IdChannel);
      }
    }

    // TODO move this out of here; I don't think such business logic should be mixed into the database layer
    public static IChannel GetTuningChannel(TuningDetail detail)
    {
      IChannel channel = null;
      switch ((BroadcastStandard)detail.BroadcastStandard)
      {
        case BroadcastStandard.AmRadio:
          channel = new ChannelAmRadio();
          break;
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
          channel = new ChannelDvbC();
          break;
        case BroadcastStandard.DvbC2:
          ChannelDvbC2 dvbc2Channel = new ChannelDvbC2();
          dvbc2Channel.PlpId = (short)detail.StreamId;
          channel = dvbc2Channel;
          break;
        case BroadcastStandard.DvbDsng:
          ChannelDvbDsng dvbDsngChannel = new ChannelDvbDsng();
          dvbDsngChannel.RollOffFactor = (RollOffFactor)detail.RollOffFactor;
          channel = dvbDsngChannel;
          break;
        case BroadcastStandard.DvbS:
          channel = new ChannelDvbS();
          break;
        case BroadcastStandard.DvbS2:
        case BroadcastStandard.DvbS2Pro:
        case BroadcastStandard.DvbS2X:
          ChannelDvbS2 dvbs2Channel = new ChannelDvbS2((BroadcastStandard)detail.BroadcastStandard);
          dvbs2Channel.RollOffFactor = (RollOffFactor)detail.RollOffFactor;
          dvbs2Channel.PilotTonesState = (PilotTonesState)detail.PilotTonesState;
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
        case BroadcastStandard.IsdbC:
          channel = new ChannelIsdbC();
          break;
        case BroadcastStandard.IsdbS:
          channel = new ChannelIsdbS();
          break;
        case BroadcastStandard.IsdbT:
          channel = new ChannelIsdbT();
          break;
        case BroadcastStandard.FmRadio:
          channel = new ChannelFmRadio();
          break;
        case BroadcastStandard.SatelliteTurboFec:
          channel = new ChannelSatelliteTurboFec();
          break;
        case BroadcastStandard.Scte:
          channel = new ChannelScte();
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

      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      if (physicalChannel != null)
      {
        physicalChannel.Frequency = detail.Frequency;

        IChannelOfdm ofdmChannel = channel as IChannelOfdm;
        if (ofdmChannel != null)
        {
          ofdmChannel.Bandwidth = detail.Bandwidth;
        }
        else
        {
          IChannelQam qamChannel = channel as IChannelQam;
          if (qamChannel != null)
          {
            qamChannel.ModulationScheme = (ModulationSchemeQam)detail.Modulation;
            qamChannel.SymbolRate = detail.SymbolRate;
          }
          else
          {
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
          }
        }
      }

      IChannelMpeg2Ts mpeg2TsChannel = channel as IChannelMpeg2Ts;
      if (mpeg2TsChannel != null)
      {
        mpeg2TsChannel.TransportStreamId = detail.TransportStreamId;
        mpeg2TsChannel.ProgramNumber = detail.ServiceId;
        mpeg2TsChannel.PmtPid = detail.PmtPid;

        IChannelDvbCompatible dvbCompatibleChannel = channel as IChannelDvbCompatible;
        if (dvbCompatibleChannel != null)
        {
          dvbCompatibleChannel.OriginalNetworkId = detail.OriginalNetworkId;
          dvbCompatibleChannel.EpgOriginalNetworkId = detail.EpgOriginalNetworkId;
          dvbCompatibleChannel.EpgTransportStreamId = detail.EpgTransportStreamId;
          dvbCompatibleChannel.EpgServiceId = detail.EpgServiceId;

          IChannelFreesat freesatChannel = channel as IChannelFreesat;
          if (freesatChannel != null)
          {
            freesatChannel.FreesatChannelId = detail.FreesatChannelId;
          }

          IChannelOpenTv openTvChannel = channel as IChannelOpenTv;
          if (openTvChannel != null)
          {
            openTvChannel.OpenTvChannelId = detail.OpenTvChannelId;
          }
        }
      }

      channel.Name = detail.Name;
      channel.Provider = detail.Provider;
      channel.LogicalChannelNumber = detail.LogicalChannelNumber;
      channel.MediaType = (MediaType)detail.MediaType;
      channel.GrabEpg = detail.GrabEpg;
      channel.IsEncrypted = detail.IsEncrypted;
      channel.IsHighDefinition = detail.IsHighDefinition;
      channel.IsThreeDimensional = detail.IsThreeDimensional;
      return channel;
    }

    #region tuning-detail-to-tuner mappings

    public static IList<TunerTuningDetailMapping> ListAllTunerMappings()
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        return tuningDetailRepository.GetAll<TunerTuningDetailMapping>().ToList();
      }
    }

    private static TunerTuningDetailMapping GetTunerMapping(int idTunerMapping)
    {
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        return tuningDetailRepository.GetQuery<TunerTuningDetailMapping>(m => m.IdTunerTuningDetailMapping == idTunerMapping).FirstOrDefault();
      }
    }

    public static TunerTuningDetailMapping SaveTunerMapping(TunerTuningDetailMapping mapping)
    {
      // We assume tuner mappings are added or deleted; never updated.
      if (OnTuningDetailMappingsChanged != null)
      {
        OnTuningDetailMappingsChanged(mapping.IdTuningDetail, mapping.IdTuner, mapping.ChangeTracker.State);
      }

      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        tuningDetailRepository.AttachEntityIfChangeTrackingDisabled(tuningDetailRepository.ObjectContext.TunerTuningDetailMappings, mapping);
        tuningDetailRepository.ApplyChanges(tuningDetailRepository.ObjectContext.TunerTuningDetailMappings, mapping);
        tuningDetailRepository.UnitOfWork.SaveChanges();
        mapping.AcceptChanges();
        return mapping;
      }
    }

    public static IList<TunerTuningDetailMapping> SaveTunerMappings(IEnumerable<TunerTuningDetailMapping> mappings)
    {
      // We assume tuner mappings are added or deleted; never updated.
      if (OnTuningDetailMappingsChanged != null)
      {
        foreach (TunerTuningDetailMapping mapping in mappings)
        {
          OnTuningDetailMappingsChanged(mapping.IdTuningDetail, mapping.IdTuner, mapping.ChangeTracker.State);
        }
      }

      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository())
      {
        tuningDetailRepository.AttachEntityIfChangeTrackingDisabled(tuningDetailRepository.ObjectContext.TunerTuningDetailMappings, mappings);
        tuningDetailRepository.ApplyChanges(tuningDetailRepository.ObjectContext.TunerTuningDetailMappings, mappings);
        tuningDetailRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //tuningDetailRepository.ObjectContext.AcceptAllChanges();
        foreach (TunerTuningDetailMapping map in mappings)
        {
          map.AcceptChanges();
        }
      }
      return mappings.ToList();
    }

    public static void DeleteTunerMapping(int idTunerMapping)
    {
      if (OnTuningDetailMappingsChanged != null)
      {
        TunerTuningDetailMapping mapping = GetTunerMapping(idTunerMapping);
        if (mapping != null)
        {
          OnTuningDetailMappingsChanged(mapping.IdTuningDetail, mapping.IdTuner, ObjectState.Deleted);
        }
      }

      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository(true))
      {
        tuningDetailRepository.Delete<TunerTuningDetailMapping>(m => m.IdTunerTuningDetailMapping == idTunerMapping);
        tuningDetailRepository.UnitOfWork.SaveChanges();
      }
    }

    public static void DeleteTunerMappings(IEnumerable<int> tunerMappingIds)
    {
      if (OnTuningDetailMappingsChanged != null)
      {
        foreach (int id in tunerMappingIds)
        {
          TunerTuningDetailMapping mapping = GetTunerMapping(id);
          if (mapping != null)
          {
            OnTuningDetailMappingsChanged(mapping.IdTuningDetail, mapping.IdTuner, ObjectState.Deleted);
          }
        }
      }

      HashSet<int> ids = new HashSet<int>(tunerMappingIds);
      using (ITuningDetailRepository tuningDetailRepository = new TuningDetailRepository(true))
      {
        tuningDetailRepository.Delete<TunerTuningDetailMapping>(m => ids.Contains(m.IdTunerTuningDetailMapping));
        tuningDetailRepository.UnitOfWork.SaveChanges();
      }
    }

    #endregion
  }
}