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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  [Serializable]
  public class TuningDetail : ICloneable
  {
    public BroadcastStandard BroadcastStandard = BroadcastStandard.Unknown;
    public int Longitude = 0;
    public int CellId = -1;
    public int CellIdExtension = -1;
    public int Frequency = -1;      // unit = kHz
    public int FrequencyOffset = 0; // unit = kHz
    public short PhysicalChannelNumber = -1;
    public int Bandwidth = -1;      // unit = kHz
    public Polarisation Polarisation = Polarisation.Automatic;
    public string ModulationScheme = "Automatic";
    public int SymbolRate = -1;     // unit = ks/s
    public FecCodeRate FecCodeRate = FecCodeRate.Automatic;
    public RollOffFactor RollOffFactor = RollOffFactor.Automatic;
    public PilotTonesState PilotTonesState = PilotTonesState.Automatic;
    public int StreamId = -1;

    // The properties below are not loaded from or saved to tuning detail
    // files.

    // The set of possible alternative/regional frequencies. This list is only
    // populated for certain network information (NIT) scan results. When the
    // list is populated, the frequency property/variable will not be set. Only
    // one frequency is expected to be receivable.
    [XmlIgnore]
    public IList<int> Frequencies = new List<int>();

    // These properties are only populated for network information (NIT) scan
    // results. They're used to minimise the number of tuning details that need
    // to be tried.
    [XmlIgnore]
    public ushort OriginalNetworkId = 0;
    [XmlIgnore]
    public ushort TransportStreamId = 0;

    // These properties correspond with user interface settings.
    [XmlIgnore]
    public Country Country;
    [XmlIgnore]
    public AnalogTunerSource TunerSource;
    [XmlIgnore]
    public string Url;

    // These properties correspond with information from an M3U.
    [XmlIgnore]
    public string StreamName;
    [XmlIgnore]
    public string StreamLogicalChannelNumber;

    [XmlIgnore]
    public ModulationSchemeVsb ModulationSchemeVsb
    {
      get
      {
        ModulationSchemeVsb vsbModulation;
        if (ModulationScheme != null && System.Enum.TryParse<ModulationSchemeVsb>(ModulationScheme, out vsbModulation))
        {
          return vsbModulation;
        }
        return ModulationSchemeVsb.Automatic;
      }
    }

    [XmlIgnore]
    public ModulationSchemeQam ModulationSchemeQam
    {
      get
      {
        ModulationSchemeQam qamModulation;
        if (ModulationScheme != null && System.Enum.TryParse<ModulationSchemeQam>(ModulationScheme, out qamModulation))
        {
          return qamModulation;
        }
        return ModulationSchemeQam.Automatic;
      }
    }

    [XmlIgnore]
    public ModulationSchemePsk ModulationSchemePsk
    {
      get
      {
        ModulationSchemePsk pskModulation;
        if (ModulationScheme != null && System.Enum.TryParse<ModulationSchemePsk>(ModulationScheme, out pskModulation))
        {
          return pskModulation;
        }
        return ModulationSchemePsk.Automatic;
      }
    }

    public IChannel GetTuningChannel()
    {
      IChannel channel = null;
      switch (BroadcastStandard)
      {
        case BroadcastStandard.AmRadio:
          channel = new ChannelAmRadio();
          break;
        case BroadcastStandard.AnalogTelevision:
          ChannelAnalogTv analogTvChannel = new ChannelAnalogTv();
          analogTvChannel.Country = Country;
          analogTvChannel.PhysicalChannelNumber = PhysicalChannelNumber;
          analogTvChannel.TunerSource = TunerSource;
          channel = analogTvChannel;
          break;
        case BroadcastStandard.Atsc:
          ChannelAtsc atscChannel = new ChannelAtsc();
          atscChannel.ModulationScheme = ModulationSchemeVsb;
          channel = atscChannel;
          break;
        case BroadcastStandard.DigiCipher2:
          channel = new ChannelDigiCipher2();
          break;
        case BroadcastStandard.DvbC:
          channel = new ChannelDvbC();
          break;
        case BroadcastStandard.DvbC2:
          ChannelDvbC2 dvbc2Channel = new ChannelDvbC2();
          dvbc2Channel.PlpId = (short)StreamId;
          channel = dvbc2Channel;
          break;
        case BroadcastStandard.DvbDsng:
          ChannelDvbDsng dvbDsngChannel = new ChannelDvbDsng();
          dvbDsngChannel.RollOffFactor = RollOffFactor;
          break;
        case BroadcastStandard.DvbIp:
          ChannelStream streamChannel = new ChannelStream();
          if (!string.IsNullOrEmpty(StreamName))
          {
            streamChannel.Name = StreamName;
          }
          if (!string.IsNullOrEmpty(StreamLogicalChannelNumber))
          {
            streamChannel.LogicalChannelNumber = StreamLogicalChannelNumber;
          }
          streamChannel.Url = Url;
          channel = streamChannel;
          break;
        case BroadcastStandard.DvbS:
          channel = new ChannelDvbS();
          break;
        case BroadcastStandard.DvbS2:
        case BroadcastStandard.DvbS2Pro:
        case BroadcastStandard.DvbS2X:
          ChannelDvbS2 dvbs2Channel = new ChannelDvbS2(BroadcastStandard);
          dvbs2Channel.RollOffFactor = RollOffFactor;
          dvbs2Channel.PilotTonesState = PilotTonesState;
          dvbs2Channel.StreamId = (short)StreamId;
          channel = dvbs2Channel;
          break;
        case BroadcastStandard.DvbT:
          channel = new ChannelDvbT();
          break;
        case BroadcastStandard.DvbT2:
          ChannelDvbT2 dvbt2Channel = new ChannelDvbT2();
          dvbt2Channel.PlpId = (short)StreamId;
          channel = dvbt2Channel;
          break;
        case BroadcastStandard.FmRadio:
          channel = new ChannelFmRadio();
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
        case BroadcastStandard.SatelliteTurboFec:
          channel = new ChannelSatelliteTurboFec();
          break;
        case BroadcastStandard.Scte:
          channel = new ChannelScte();
          break;
        default:
          this.LogError("tuning detail: failed to handle broadcast standard {0} in GetTuningChannel()", BroadcastStandard);
          return null;
      }

      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      if (physicalChannel != null)
      {
        physicalChannel.Frequency = Frequency;

        IChannelOfdm ofdmChannel = channel as IChannelOfdm;
        if (ofdmChannel != null)
        {
          ofdmChannel.Bandwidth = Bandwidth;
        }
        else
        {
          IChannelQam qamChannel = channel as IChannelQam;
          if (qamChannel != null)
          {
            qamChannel.ModulationScheme = ModulationSchemeQam;
            qamChannel.SymbolRate = SymbolRate;
          }
          else
          {
            IChannelSatellite satelliteChannel = channel as IChannelSatellite;
            if (satelliteChannel != null)
            {
              satelliteChannel.Longitude = Longitude;
              satelliteChannel.Polarisation = Polarisation;
              satelliteChannel.ModulationScheme = ModulationSchemePsk;
              satelliteChannel.SymbolRate = SymbolRate;
              satelliteChannel.FecCodeRate = FecCodeRate;
            }
          }
        }
      }

      channel.GrabEpg = false;  // prevent attempts to grab EPG while scanning
      return channel;
    }

    #region object overrides

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>, otherwise <c>false</c></returns>
    public override bool Equals(object obj)
    {
      TuningDetail tuningDetail = obj as TuningDetail;
      if (
        tuningDetail == null ||
        BroadcastStandard != tuningDetail.BroadcastStandard ||
        Longitude != tuningDetail.Longitude ||
        CellId != tuningDetail.CellId ||
        CellIdExtension != tuningDetail.CellIdExtension ||
        Frequency != tuningDetail.Frequency ||
        FrequencyOffset != tuningDetail.FrequencyOffset ||
        Bandwidth != tuningDetail.Bandwidth ||
        Polarisation != tuningDetail.Polarisation ||
        !string.Equals(ModulationScheme, tuningDetail.ModulationScheme) ||
        SymbolRate != tuningDetail.SymbolRate ||
        FecCodeRate != tuningDetail.FecCodeRate ||
        RollOffFactor != tuningDetail.RollOffFactor ||
        PilotTonesState != tuningDetail.PilotTonesState ||
        StreamId != tuningDetail.StreamId ||
        (
          Frequencies == null &&
          tuningDetail.Frequencies != null &&
          tuningDetail.Frequencies.Count > 0
        ) ||
        (
          Frequencies != null &&
          Frequencies.Count > 0 &&
          tuningDetail.Frequencies == null
        )
      )
      {
        return false;
      }

      foreach (int frequency in Frequencies)
      {
        if (!tuningDetail.Frequencies.Contains(frequency))
        {
          return false;
        }
      }
      foreach (int frequency in tuningDetail.Frequencies)
      {
        if (!Frequencies.Contains(frequency))
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// A hash function for this type.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"/></returns>
    public override int GetHashCode()
    {
      return BroadcastStandard.GetHashCode() ^ Longitude.GetHashCode() ^
              CellId.GetHashCode() ^ CellIdExtension.GetHashCode() ^
              Frequency.GetHashCode() ^ FrequencyOffset.GetHashCode() ^
              Bandwidth.GetHashCode() ^ Polarisation.GetHashCode() ^
              ModulationScheme.GetHashCode() ^ SymbolRate.GetHashCode() ^
              FecCodeRate.GetHashCode() ^ RollOffFactor.GetHashCode() ^
              PilotTonesState.GetHashCode() ^ StreamId.GetHashCode() ^
              Frequencies.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      string frequencyMhz;
      if (Frequencies != null && Frequencies.Count > 0)
      {
        List<string> frequencyStrings = new List<string>(Frequencies.Count);
        foreach (int frequency in Frequencies)
        {
          frequencyStrings.Add(string.Format("{0:#.##}", (float)frequency / 1000));
        }
        frequencyMhz = string.Join("/", frequencyStrings);
      }
      else
      {
        frequencyMhz = string.Format("{0:#.##}", (float)Frequency / 1000);
      }
      switch (BroadcastStandard)
      {
        case BroadcastStandard.AmRadio:
          return string.Format("{0} kHz", Frequencies != null && Frequencies.Count > 0 ? string.Join("/", Frequencies) : Frequency.ToString());
        case BroadcastStandard.AnalogTelevision:
          return string.Format("#{0}", PhysicalChannelNumber);
        case BroadcastStandard.FmRadio:
        case BroadcastStandard.IsdbC:
          return string.Format("{0} MHz", frequencyMhz);
        case BroadcastStandard.ExternalInput:
          return "external inputs";
        case BroadcastStandard.DvbC:
          return string.Format("{0} MHz, {1}, {2} ks/s", frequencyMhz, ModulationSchemeQam.GetDescription(), SymbolRate);
        case BroadcastStandard.DvbC2:
        case BroadcastStandard.DvbT:
        case BroadcastStandard.DvbT2:
        case BroadcastStandard.IsdbT:
          string offsetDescription = string.Empty;
          if (FrequencyOffset > 0)
          {
            offsetDescription = string.Format(" (+/- {0} kHz)", FrequencyOffset);
          }
          string plpIdDescription = string.Empty;
          if ((BroadcastStandard == BroadcastStandard.DvbC2 || BroadcastStandard == BroadcastStandard.DvbT2) && StreamId >= 0)
          {
            plpIdDescription = string.Format(", PLP {0}", StreamId);
          }
          return string.Format("{0} {1} MHz{2}, BW {3:#.##} MHz{4}", BroadcastStandard.GetDescription(), frequencyMhz, offsetDescription, (float)Bandwidth / 1000, plpIdDescription);
        case BroadcastStandard.DvbIp:
          if (!string.IsNullOrEmpty(StreamName))
          {
            return string.Format("{0} - {1}", StreamName, Url);
          }
          return Url;
        case BroadcastStandard.DvbDsng:
        case BroadcastStandard.DvbS:
        case BroadcastStandard.DvbS2:
        case BroadcastStandard.DvbS2Pro:
        case BroadcastStandard.DvbS2X:
        case BroadcastStandard.IsdbS:
        case BroadcastStandard.SatelliteTurboFec:
        case BroadcastStandard.DigiCipher2:
        case BroadcastStandard.DirecTvDss:
          string extraDescription = string.Empty;
          if (BroadcastStandard == BroadcastStandard.DvbDsng || BroadcastStandard.MaskDvbS2.HasFlag(BroadcastStandard))
          {
            extraDescription = string.Format(", roll-off factor {0}", RollOffFactor.GetDescription());
            if (BroadcastStandard.MaskDvbS2.HasFlag(BroadcastStandard))
            {
              extraDescription = string.Format("{0}, pilot tones {1}", extraDescription, PilotTonesState.GetDescription().ToLowerInvariant());
              if (StreamId >= 0)
              {
                extraDescription = string.Format("{0}, IS {1}", extraDescription, StreamId);
              }
            }
          }
          return string.Format("{0} {1} MHz, {2}, {3}, {4} ks/s, {5}{6}", BroadcastStandard.GetDescription(), frequencyMhz, Polarisation.GetDescription(), ModulationSchemePsk.GetDescription(), SymbolRate, FecCodeRate.GetDescription(), extraDescription);
        case BroadcastStandard.Atsc:
          return string.Format("{0} MHz (#{1}), {2}", frequencyMhz, ChannelAtsc.GetPhysicalChannelNumberForFrequency(Frequency), ModulationSchemeVsb.GetDescription());
        case BroadcastStandard.Scte:
          if (Frequency <= 0)
          {
            return "CableCARD out-of-band SI";
          }
          return string.Format("{0} MHz (#{1}), {2}", frequencyMhz, ChannelScte.GetPhysicalChannelNumberForFrequency(Frequency), ModulationSchemeQam.GetDescription());

        // Not implemented.
        case BroadcastStandard.Dab:
        default:
          return string.Empty;
      }
    }

    #endregion

    #region ICloneable member

    /// <summary>
    /// Clone the tuning detail instance.
    /// </summary>
    /// <returns>a shallow clone of the tuning detail instance</returns>
    public object Clone()
    {
      return MemberwiseClone();
    }

    #endregion
  }
}