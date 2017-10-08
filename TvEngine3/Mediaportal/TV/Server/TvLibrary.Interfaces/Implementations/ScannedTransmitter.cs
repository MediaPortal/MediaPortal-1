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
using System.Runtime.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations
{
  [DataContract]
  public class ScannedTransmitter
  {
    #region variables

    [DataMember]
    private ushort _originalNetworkId = 0;

    [DataMember]
    private ushort _transportStreamId = 0;

    [DataMember]
    private BroadcastStandard _broadcastStandard = BroadcastStandard.Unknown;

    [DataMember]
    private int _longitude = 0;

    // The set of possible frequencies. This list often only contains one
    // frequency (the correct one), but may contain additional alternative
    // and/or regional frequencies. Only one frequency is expected to be
    // receivable.
    [DataMember]
    private IList<int> _frequencies = new List<int>(50);

    [DataMember]
    private int _bandwidth = -1;    // unit = kHz

    [DataMember]
    private Polarisation _polarisation = Polarisation.Automatic;

    [DataMember]
    private ModulationSchemePsk _modulationSchemePsk = ModulationSchemePsk.Automatic;

    [DataMember]
    private ModulationSchemeQam _modulationSchemeQam = ModulationSchemeQam.Automatic;

    [DataMember]
    private int _symbolRate = -1;   // unit = ks/s

    [DataMember]
    private FecCodeRate _fecCodeRate = FecCodeRate.Automatic;

    [DataMember]
    private RollOffFactor _rollOffFactor = RollOffFactor.Automatic;

    [DataMember]
    private int _streamId = -1;

    #endregion

    #region properties

    public ushort OriginalNetworkId
    {
      get
      {
        return _originalNetworkId;
      }
      set
      {
        _originalNetworkId = value;
      }
    }

    public ushort TransportStreamId
    {
      get
      {
        return _transportStreamId;
      }
      set
      {
        _transportStreamId = value;
      }
    }

    public BroadcastStandard BroadcastStandard
    {
      get
      {
        return _broadcastStandard;
      }
      set
      {
        _broadcastStandard = value;
      }
    }

    public int Longitude
    {
      get
      {
        return _longitude;
      }
      set
      {
        _longitude = value;
      }
    }

    public IList<int> Frequencies
    {
      get
      {
        return _frequencies;
      }
      set
      {
        _frequencies = value;
      }
    }

    public int Bandwidth
    {
      get
      {
        return _bandwidth;
      }
      set
      {
        _bandwidth = value;
      }
    }

    public Polarisation Polarisation
    {
      get
      {
        return _polarisation;
      }
      set
      {
        _polarisation = value;
      }
    }

    public ModulationSchemePsk ModulationSchemePsk
    {
      get
      {
        return _modulationSchemePsk;
      }
      set
      {
        _modulationSchemePsk = value;
      }
    }

    public ModulationSchemeQam ModulationSchemeQam
    {
      get
      {
        return _modulationSchemeQam;
      }
      set
      {
        _modulationSchemeQam = value;
      }
    }

    public int SymbolRate
    {
      get
      {
        return _symbolRate;
      }
      set
      {
        _symbolRate = value;
      }
    }

    public FecCodeRate FecCodeRate
    {
      get
      {
        return _fecCodeRate;
      }
      set
      {
        _fecCodeRate = value;
      }
    }

    public RollOffFactor RollOffFactor
    {
      get
      {
        return _rollOffFactor;
      }
      set
      {
        _rollOffFactor = value;
      }
    }

    public int StreamId
    {
      get
      {
        return _streamId;
      }
      set
      {
        _streamId = value;
      }
    }

    #endregion

    public IChannel GetTuningChannel()
    {
      IChannel channel = null;
      switch (BroadcastStandard)
      {
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
        case BroadcastStandard.DvbS:
          channel = new ChannelDvbS();
          break;
        case BroadcastStandard.DvbS2:
        case BroadcastStandard.DvbS2Pro:
        case BroadcastStandard.DvbS2X:
          ChannelDvbS2 dvbs2Channel = new ChannelDvbS2(BroadcastStandard);
          dvbs2Channel.RollOffFactor = RollOffFactor;
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

        // These broadcast standards can be supported with the existing
        // properties but don't have network information.
        case BroadcastStandard.AmRadio:
          channel = new ChannelAmRadio();
          break;
        case BroadcastStandard.FmRadio:
          channel = new ChannelFmRadio();
          break;
        case BroadcastStandard.DigiCipher2:
          channel = new ChannelDigiCipher2();
          break;
        case BroadcastStandard.Scte:
          channel = new ChannelScte();
          break;
        default:
          this.LogError("scanned transmitter: failed to handle broadcast standard {0} in GetTuningChannel()", BroadcastStandard);
          return null;
      }

      IChannelPhysical physicalChannel = channel as IChannelPhysical;
      if (physicalChannel != null)
      {
        physicalChannel.Frequency = Frequencies[0];

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
      return channel;
    }

    public override string ToString()
    {
      string frequencyMhz = "0";
      if (Frequencies != null && Frequencies.Count > 0)
      {
        frequencyMhz = string.Format("{0:#.##}", (float)Frequencies[0] / 1000);
      }
      switch ((BroadcastStandard)BroadcastStandard)
      {
        case BroadcastStandard.IsdbC:
          return string.Format("{0} MHz", frequencyMhz);
        case BroadcastStandard.DvbC:
          return string.Format("{0} MHz, {1}, {2} ks/s", frequencyMhz, ModulationSchemeQam.GetDescription(), SymbolRate);
        case BroadcastStandard.DvbC2:
        case BroadcastStandard.DvbT:
        case BroadcastStandard.DvbT2:
        case BroadcastStandard.IsdbT:
          if (BroadcastStandard.MaskDvb2.HasFlag(BroadcastStandard) && StreamId >= 0)
          {
            return string.Format("{0} MHz, BW {1:#.##} MHz, PLP {2}", frequencyMhz, (float)Bandwidth / 1000, StreamId);
          }
          return string.Format("{0} MHz, BW {1:#.##} MHz", frequencyMhz, (float)Bandwidth / 1000);
        case BroadcastStandard.DvbDsng:
        case BroadcastStandard.DvbS:
        case BroadcastStandard.DvbS2:
        case BroadcastStandard.DvbS2Pro:
        case BroadcastStandard.DvbS2X:
        case BroadcastStandard.IsdbS:
        case BroadcastStandard.SatelliteTurboFec:
        case BroadcastStandard.DigiCipher2:
        case BroadcastStandard.DirecTvDss:
          string description = string.Format("{0:#.#}° {1}", Math.Abs(Longitude / 10), Longitude < 0 ? "W" : "E");
          description += string.Format(" {0} MHz, {1}, {2}, {3} ks/s", frequencyMhz, Polarisation.GetDescription(), ModulationSchemePsk.GetDescription(), SymbolRate);
          if (BroadcastStandard.MaskDvbS2.HasFlag(BroadcastStandard))
          {
            description += ", RO " + RollOffFactor.GetDescription();
            if (StreamId >= 0)
            {
              description += ", IS " + StreamId;
            }
          }
          return description;
      }
      this.LogError("scanned transmitter: failed to handle broadcast standard {0} in ToString()", BroadcastStandard);
      return string.Empty;
    }
  }
}