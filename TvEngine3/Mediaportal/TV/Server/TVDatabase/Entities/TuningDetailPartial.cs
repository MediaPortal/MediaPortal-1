#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Data.SqlTypes;
using Mediaportal.TV.Server.Common.Types.Enum;
using MediaPortal.Common.Utils.ExtensionMethods;
using BroadcastStandardEnum = Mediaportal.TV.Server.Common.Types.Enum.BroadcastStandard;
using MediaTypeEnum = Mediaportal.TV.Server.Common.Types.Enum.MediaType;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class TuningDetail
  {
    public TuningDetail()
    {
      _mediaType = (int)Mediaportal.TV.Server.Common.Types.Enum.MediaType.Television;
      _priority = 1;
      _broadcastStandard = (int)BroadcastStandardEnum.Unknown;
      _name = string.Empty;
      _provider = string.Empty;
      _logicalChannelNumber = Mediaportal.TV.Server.Common.Types.Channel.LogicalChannelNumber.GLOBAL_DEFAULT;
      _isEncrypted = false;
      _isHighDefinition = false;
      _isThreeDimensional = false;
      _originalNetworkId = -1;
      _transportStreamId = -1;
      _serviceId = -1;
      _freesatChannelId = -1;
      _openTvChannelId = -1;
      _epgOriginalNetworkId = -1;
      _epgTransportStreamId = -1;
      _epgServiceId = -1;
      _sourceId = -1;
      _pmtPid = -1;
      _physicalChannelNumber = -1;
      _frequency = -1;
      _countryId = -1;
      _modulation = -1;
      _polarisation = (int)Mediaportal.TV.Server.Common.Types.Enum.Polarisation.Automatic;
      _symbolRate = -1;
      _bandwidth = -1;
      _videoSource = (int)CaptureSourceVideo.None;
      _audioSource = (int)CaptureSourceAudio.None;
      _tuningSource = (int)AnalogTunerSource.Cable;
      _fecCodeRate = (int)Mediaportal.TV.Server.Common.Types.Enum.FecCodeRate.Automatic;
      _pilotTonesState = (int)Mediaportal.TV.Server.Common.Types.Enum.PilotTonesState.Automatic;
      _rollOffFactor = (int)Mediaportal.TV.Server.Common.Types.Enum.RollOffFactor.Automatic;
      _streamId = -1;
      _url = string.Empty;
      _isVcrSignal = false;
      _idSatellite = null;
      _grabEpg = true;
      _lastEpgGrabTime = SqlDateTime.MinValue.Value;
    }

    public override string ToString()
    {
      return Name;
    }

    public string GetDescription()
    {
      return string.Format("ID = {0}, channel ID = {1}, {2}, {3}", IdTuningDetail, IdChannel, GetServiceDescription(), GetTuningDescription());
    }

    public string GetServiceDescription()
    {
      BroadcastStandardEnum broadcastStandard = (BroadcastStandardEnum)BroadcastStandard;
      string description = string.Format("name = {0}, provider = {1}, type = {2}, standard = {3}, LCN = {4}, is encrypted = {5}", Name, Provider, (MediaTypeEnum)MediaType, broadcastStandard, LogicalChannelNumber, IsEncrypted);
      if (MediaType != (int)MediaTypeEnum.Radio)
      {
        description = string.Format("{0}, is HD = {1}, is 3D = {2}", description, IsHighDefinition, IsThreeDimensional);
      }
      if (BroadcastStandardEnum.MaskDvbSi.HasFlag(broadcastStandard))
      {
        description = string.Format("{0}, DVB ID = {1}/{2}/{3}, PMT PID = {4}, EPG DVB ID = {5}/{6}/{7}", description, OriginalNetworkId, TransportStreamId, ServiceId, PmtPid, EpgOriginalNetworkId, EpgTransportStreamId, EpgServiceId);
      }
      else if (BroadcastStandardEnum.MaskMpeg2TsSi.HasFlag(broadcastStandard))
      {
        description = string.Format("{0}, TSID = {1}, program # = {2}, PMT PID = {3}", description, TransportStreamId, ServiceId, PmtPid);
      }
      if (BroadcastStandardEnum.MaskAtscScteSi.HasFlag(broadcastStandard))
      {
        description = string.Format("{0}, source ID = {1}", description, SourceId);
      }
      if (BroadcastStandardEnum.MaskOpenTvSi.HasFlag(broadcastStandard))
      {
        description = string.Format("{0}, OpenTV CID = {1}", description, OpenTvChannelId);
      }
      if (BroadcastStandardEnum.MaskFreesatSi.HasFlag(broadcastStandard))
      {
        description = string.Format("{0}, Freesat CID = {1}", description, FreesatChannelId);
      }
      return description;
    }

    public string GetTuningDescription()
    {
      BroadcastStandardEnum broadcastStandard = (BroadcastStandardEnum)BroadcastStandard;
      switch (broadcastStandard)
      {
        case BroadcastStandardEnum.DvbIp:
          return Url;
        case BroadcastStandardEnum.ExternalInput:
          return string.Format("video source = {0}, audio source = {1}, is VCR signal = {2}", ((CaptureSourceVideo)VideoSource).GetDescription(), ((CaptureSourceAudio)AudioSource).GetDescription(), IsVcrSignal);
        case BroadcastStandardEnum.Atsc:
          return string.Format("frequency = {0} kHz, modulation = {1}", Frequency, ((ModulationSchemeVsb)Modulation).GetDescription());
        case BroadcastStandardEnum.Scte:
          return string.Format("frequency = {0} kHz, modulation = {1}", Frequency, ((ModulationSchemeQam)Modulation).GetDescription());
        case BroadcastStandardEnum.DvbC:
          return string.Format("frequency = {0} kHz, modulation = {1}, symbol rate = {2} ks/s", Frequency, ((ModulationSchemeQam)Modulation).GetDescription(), SymbolRate);
        case BroadcastStandardEnum.AnalogTelevision:
          return string.Format("country ID = {0}, source = {1}, physical channel number = {2}, frequency = {3} kHz", CountryId, TuningSource, PhysicalChannelNumber, Frequency);
        case BroadcastStandardEnum.AmRadio:
        case BroadcastStandardEnum.FmRadio:
        case BroadcastStandardEnum.IsdbC:
          return string.Format("frequency = {0} kHz", Frequency);
      }

      string description;
      if (BroadcastStandardEnum.MaskOfdm.HasFlag(broadcastStandard))
      {
        description = string.Format("frequency = {0} kHz, bandwidth = {1} kHz", Frequency, Bandwidth);
        if (!BroadcastStandardEnum.MaskDvb2.HasFlag((BroadcastStandardEnum)BroadcastStandard))
        {
          return description;
        }
        return string.Format("{0}, PLP ID = {1}", description, StreamId);
      }
      if (BroadcastStandardEnum.MaskSatellite.HasFlag(broadcastStandard))
      {
        description = string.Format("satellite ID = {0}, frequency = {1} kHz, polarisation = {2}, modulation = {3}, symbol rate = {4} ks/s, FEC code rate = {5}", IdSatellite, Frequency, ((Polarisation)Polarisation).GetDescription(), ((ModulationSchemePsk)Modulation).GetDescription(), SymbolRate, ((FecCodeRate)FecCodeRate).GetDescription());
        if (broadcastStandard == BroadcastStandardEnum.DvbDsng)
        {
          return string.Format("{0}, roll-off factor = {1}", description, ((RollOffFactor)RollOffFactor).GetDescription());
        }
        if (BroadcastStandardEnum.MaskDvbS2.HasFlag(broadcastStandard))
        {
          return string.Format("{0}, roll-off factor = {1}, pilot tones state = {2}, IS ID = {3}", description, ((RollOffFactor)RollOffFactor).GetDescription(), ((PilotTonesState)PilotTonesState).GetDescription(), StreamId);
        }
        return description;
      }
      return string.Empty;
    }

    public string GetTerseTuningDescription()
    {
      string frequencyMhz = string.Format("{0:#.##}", (float)Frequency / 1000);
      switch ((BroadcastStandardEnum)BroadcastStandard)
      {
        case BroadcastStandardEnum.ExternalInput:
          return string.Format("#{0} {1}, {2}", LogicalChannelNumber, ((CaptureSourceVideo)VideoSource).GetDescription(), ((CaptureSourceAudio)AudioSource).GetDescription());
        case BroadcastStandardEnum.AnalogTelevision:
          if (Frequency > 0)
          {
            return string.Format("#{0} {1} MHz", PhysicalChannelNumber, frequencyMhz);
          }
          return string.Format("#{0}", PhysicalChannelNumber);
        case BroadcastStandardEnum.AmRadio:
          return string.Format("{0} kHz", Frequency);
        case BroadcastStandardEnum.FmRadio:
        case BroadcastStandardEnum.IsdbC:
          return string.Format("{0} MHz", frequencyMhz);
        case BroadcastStandardEnum.DvbC:
          return string.Format("{0} MHz, {1}, {2} ks/s", frequencyMhz, ((ModulationSchemeQam)Modulation).GetDescription(), SymbolRate);
        case BroadcastStandardEnum.DvbC2:
        case BroadcastStandardEnum.DvbT:
        case BroadcastStandardEnum.DvbT2:
        case BroadcastStandardEnum.IsdbT:
          if (BroadcastStandardEnum.MaskDvb2.HasFlag((BroadcastStandardEnum)BroadcastStandard) && StreamId >= 0)
          {
            return string.Format("{0} MHz, BW {1:#.##} MHz, PLP {2}", frequencyMhz, (float)Bandwidth / 1000, StreamId);
          }
          return string.Format("{0} MHz, BW {1:#.##} MHz", frequencyMhz, (float)Bandwidth / 1000);
        case BroadcastStandardEnum.DvbDsng:
        case BroadcastStandardEnum.DvbS:
        case BroadcastStandardEnum.DvbS2:
        case BroadcastStandardEnum.DvbS2Pro:
        case BroadcastStandardEnum.DvbS2X:
        case BroadcastStandardEnum.IsdbS:
        case BroadcastStandardEnum.SatelliteTurboFec:
        case BroadcastStandardEnum.DigiCipher2:
        case BroadcastStandardEnum.DirecTvDss:
          string satellite = string.Format("sat {0}", IdSatellite);
          if (Satellite != null)
          {
            satellite = Satellite.LongitudeString();
          }
          // Don't bother with FEC code rate, pilot tones state or roll-off
          // factor. They won't help to differentiate. Remember that we're
          // trying to be terse.
          string description = string.Format("{0} {1} MHz, {2}, {3}, {4} ks/s", satellite, frequencyMhz, ((Polarisation)Polarisation).GetDescription(), ((ModulationSchemePsk)Modulation).GetDescription(), SymbolRate);
          if (BroadcastStandardEnum.MaskDvbS2.HasFlag((BroadcastStandardEnum)BroadcastStandard) && StreamId >= 0)
          {
            return string.Format("{0}, IS {1}", description, StreamId);
          }
          return description;
        case BroadcastStandardEnum.DvbIp:
          return Url;
        case BroadcastStandardEnum.Atsc:
          return string.Format("{0} MHz, {1}", frequencyMhz, ((ModulationSchemeVsb)Modulation).GetDescription());
        case BroadcastStandardEnum.Scte:
          return string.Format("{0} MHz, {1}", frequencyMhz, ((ModulationSchemeQam)Modulation).GetDescription());

        // Not implemented.
        case BroadcastStandardEnum.Dab:
        default:
          return string.Empty;
      }
    }
  }
}