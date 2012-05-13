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
using DirectShowLib.BDA;
using TvLibrary.Interfaces;

namespace TvLibrary.Channels
{
  #region enums

  /// <summary>
  /// enum describing the DVBS band
  /// </summary>
  public enum BandType
  {
    /// <summary>
    /// Ku-Linear - LOF1 9750, LOF2 10600, SW 11700
    /// Universal LNB - common in Europe
    /// </summary>
    Universal = 0,
    /// <summary>
    /// Ku-Circular - LOF1 10750
    /// </summary>
    Circular = 1,
    /// <summary>
    /// C-Band - LOF1 5150
    /// </summary>
    CBand = 2,
    /// <summary>
    /// North American Bandstacked
    /// DishPro Ku-Linear Hi(DBS) - LOF1 11250, LOF2 14350
    /// </summary>
    NaBandStackedDpKuHi = 3,
    /// <summary>
    /// North American Bandstacked
    /// DishPro Ku-Linear Lo(FSS) - LOF1 10750, LOF2 13850
    /// </summary>
    NaBandStackedDpKuLo = 4,
    /// <summary>
    /// North American Bandstacked
    /// Ku-Linear Hi(DBS) - LOF1 11250, LOF2 10675
    /// </summary>
    NaBandStackedKuHi = 5,
    /// <summary>
    /// North American Bandstacked
    /// Ku-Linear Lo(FSS) - LOF1 10750, LOF2 10175
    /// </summary>
    NaBandStackedKuLo = 6,
    /// <summary>
    /// North American Bandstacked
    /// C-Band LOF1 5150, LOF2 5750
    /// </summary>
    NaBandStackedC = 7,
    /// <summary>
    /// North American Legacy
    /// LOF1 11250
    /// </summary>
    NaLegacy = 8,
    /// <summary>
    /// North American Custom1
    /// LOF1 11250, LOF2 11250, SW 12700
    /// </summary>
    NaCustom1 = 9,
    /// <summary>
    /// North American Custom2
    /// LOF1 11250, LOF2 11250, SW 12200
    /// </summary>
    NaCustom2 = 10,
  }

  /// <summary>
  /// Enum listing DiSEqC switch commands for DiSEqC 1.0 and 1.1 compatible switches.
  /// </summary>
  public enum DiseqcSwitchCommand
  {
    /// <summary>
    /// DiSEqC not used.
    /// </summary>
    None = 0,
    /// <summary>
    /// Simple A (tone burst).
    /// </summary>
    SimpleA = 1,
    /// <summary>
    /// Simple B (data burst).
    /// </summary>
    SimpleB = 2,
    /// <summary>
    /// DiSEqC 1.0 port A (option A, position A)
    /// </summary>
    PortA = 3,
    /// <summary>
    /// DiSEqC 1.0 port B (option A, position B)
    /// </summary>
    PortB = 4,
    /// <summary>
    /// DiSEqC 1.0 port C (option B, position A)
    /// </summary>
    PortC = 5,
    /// <summary>
    /// DiSEqC 1.0 port D (option B, position B)
    /// </summary>
    PortD = 6,
    /// <summary>
    /// DiSEqC 1.1 port 1
    /// </summary>
    Port1 = 7,
    /// <summary>
    /// DiSEqC 1.1 port 2
    /// </summary>
    Port2 = 8,
    /// <summary>
    /// DiSEqC 1.1 port 3
    /// </summary>
    Port3 = 9,
    /// <summary>
    /// DiSEqC 1.1 port 4
    /// </summary>
    Port4 = 10,
    /// <summary>
    /// DiSEqC 1.1 port 5
    /// </summary>
    Port5 = 11,
    /// <summary>
    /// DiSEqC 1.1 port 6
    /// </summary>
    Port6 = 12,
    /// <summary>
    /// DiSEqC 1.1 port 7
    /// </summary>
    Port7 = 13,
    /// <summary>
    /// DiSEqC 1.1 port 8
    /// </summary>
    Port8 = 14,
    /// <summary>
    /// DiSEqC 1.1 port 9
    /// </summary>
    Port9 = 15,
    /// <summary>
    /// DiSEqC 1.1 port 10
    /// </summary>
    Port10 = 16,
    /// <summary>
    /// DiSEqC 1.1 port 11
    /// </summary>
    Port11 = 17,
    /// <summary>
    /// DiSEqC 1.1 port 12
    /// </summary>
    Port12 = 18,
    /// <summary>
    /// DiSEqC 1.1 port 13
    /// </summary>
    Port13 = 19,
    /// <summary>
    /// DiSEqC 1.1 port 14
    /// </summary>
    Port14 = 20,
    /// <summary>
    /// DiSEqC 1.1 port 15
    /// </summary>
    Port15 = 21,
    /// <summary>
    /// DiSEqC 1.1 port 16
    /// </summary>
    Port16 = 22
  }

  /// <summary>
  /// Enum listing tone burst (simple DiSEqC) states.
  /// </summary>
  public enum ToneBurst
  {
    /// Off
    Off = 0,
    /// Tone burst AKA unmodulated, simple A
    ToneBurst,
    /// Data burst AKA modulated, simple B
    DataBurst
  }

  /// <summary>
  /// Enum listing 22 kHz oscillator logical states.
  /// </summary>
  public enum Tone22k
  {
    /// Off
    Off = 0,
    /// On
    On,
    /// Auto - controlled by LNB frequency parameters.
    Auto
  }

  #endregion

  /// <summary>
  /// Class for LNB setup and LNB number
  /// Helps determin the DVB-S band type and subsequent LNB frequencies
  /// Also determins if hi band tuning is required
  /// </summary>
  public class BandTypeConverter
  {
    /// <summary>
    /// Get the switch port number (or LNB number) for a given DiSEqC switch command.
    /// </summary>
    /// <param name="command">The DiSEqC switch command.</param>
    /// <returns>the switch port number associated with the command</returns>
    public static int GetPortNumber(DiseqcSwitchCommand command)
    {
      switch (command)
      {
        case DiseqcSwitchCommand.None:
          return 0;   // no DiSEqC
        case DiseqcSwitchCommand.SimpleA:
          return 1;
        case DiseqcSwitchCommand.SimpleB:
          return 2;
        case DiseqcSwitchCommand.PortA:
          return 1;
        case DiseqcSwitchCommand.PortB:
          return 2;
        case DiseqcSwitchCommand.PortC:
          return 3;
        case DiseqcSwitchCommand.PortD:
          return 4;
      }
      // DiSEqC 1.1 commands...
      return ((int)command - 6);
    }

    /// <summary>
    /// Determine if the channel is received when the LNB is in low (22 kHz off) or high (22 kHz on) band mode.
    /// </summary>
    /// <param name="channel">Tuning details for a satellite channel.</param>
    /// <param name="parameters">LNB oscillator frequency parameters.</param>
    /// <returns><c>true</c> if the channel is broadcast from a high band transponder, otherwise <c>false</c></returns>
    public static bool IsHighBand(DVBSChannel channel, ScanParameters parameters)
    {
      int lof1, lof2, sw;
      GetDefaultLnbSetup(parameters, channel.BandType, out lof1, out lof2, out sw);

      if (sw == 0)
      {
        return false;
      }
      return channel.Frequency >= (sw * 1000);
    }

    /// <summary>
    /// Gets the default LNB Setup depending on the type chosen.
    /// </summary>
    /// <param name="parameters">Satelliet scan parameters</param>
    /// <param name="band">LNB type i.e. Uiniversal</param>
    /// <param name="lof1">LNB low frequency</param>
    /// <param name="lof2">LNB high frequency</param>
    /// <param name="sw">LNB switch frequency</param>
    public static void GetDefaultLnbSetup(ScanParameters parameters, BandType band, out int lof1, out int lof2,
                                          out int sw)
    {
      lof1 = lof2 = sw = 0;
      if (parameters.UseDefaultLnbFrequencies == false)
      {
        lof1 = parameters.LnbLowFrequency;
        lof2 = parameters.LnbHighFrequency;
        sw = parameters.LnbSwitchFrequency;
        return;
      }
      switch (band)
      {
        case BandType.Universal:
          lof1 = 9750;
          lof2 = 10600;
          sw = 11700;
          break;
        case BandType.Circular:
          lof1 = 10750;
          lof2 = 0;
          sw = 0;
          break;
        case BandType.CBand:
          lof1 = 5150;
          lof2 = 0;
          sw = 0;
          break;
        case BandType.NaBandStackedDpKuHi:
          lof1 = 11250;
          lof2 = 14350;
          sw = 0;
          break;
        case BandType.NaBandStackedDpKuLo:
          lof1 = 10750;
          lof2 = 13850;
          sw = 0;
          break;
        case BandType.NaBandStackedKuHi:
          lof1 = 11250;
          lof2 = 10675;
          sw = 0;
          break;
        case BandType.NaBandStackedKuLo:
          lof1 = 10750;
          lof2 = 10175;
          sw = 0;
          break;
        case BandType.NaBandStackedC:
          lof1 = 5150;
          lof2 = 5750;
          sw = 0;
          break;
        case BandType.NaLegacy:
          lof1 = 11250;
          lof2 = 0;
          sw = 0;
          break;
        case BandType.NaCustom1:
          lof1 = 11250;
          lof2 = 11250;
          sw = 12700;
          break;
        case BandType.NaCustom2:
          lof1 = 11250;
          lof2 = 11250;
          sw = 12200;
          break;
      }
    }
  }

  /// <summary>
  /// A class capable of holding the tuning parameter details required to tune a DVB-S or DVB-S2 channel.
  /// </summary>
  [Serializable]
  public class DVBSChannel : DVBBaseChannel
  {
    #region variables

    private DiseqcSwitchCommand _diseqc = DiseqcSwitchCommand.None;
    private BandType _bandType = BandType.Universal;
    private int _satelliteIndex = -1;
    private Polarisation _polarisation = Polarisation.NotSet;
    private int _symbolRate = -1;
    private ModulationType _modulation = ModulationType.ModQpsk;
    private BinaryConvolutionCodeRate _innerFecRate = BinaryConvolutionCodeRate.RateNotSet;
    private Pilot _pilot = Pilot.NotSet;
    private RollOff _rollOff = RollOff.NotSet;

    #endregion

    #region constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBSChannel"/> class.
    /// </summary>
    public DVBSChannel()
      : base()
    {
      _diseqc = DiseqcSwitchCommand.None;
      _bandType = BandType.Universal;
      _satelliteIndex = -1;
      _polarisation = DirectShowLib.BDA.Polarisation.NotSet;
      _symbolRate = -1;
      _modulation = ModulationType.ModQpsk;
      _innerFecRate = BinaryConvolutionCodeRate.RateNotSet;
      _pilot = Pilot.NotSet;
      _rollOff = RollOff.NotSet;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBSChannel"/> class using an existing instance.
    /// </summary>
    /// <param name="channel">The existing channel instance.</param>
    public DVBSChannel(DVBSChannel channel)
      : base(channel)
    {
      _diseqc = channel.Diseqc;
      _bandType = channel.BandType;
      _satelliteIndex = channel.SatelliteIndex;
      _polarisation = channel.Polarisation;
      _symbolRate = channel.SymbolRate;
      _modulation = channel.ModulationType;
      _innerFecRate = channel.InnerFecRate;
      _pilot = channel.Pilot;
      _rollOff = channel.RollOff;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/set the DiSEqC switch setting used to receive the channel.
    /// </summary>
    public DiseqcSwitchCommand Diseqc
    {
      get { return _diseqc; }
      set { _diseqc = value; }
    }

    /// <summary>
    /// Get/set the band (LNB) type for LNB used to receive the channel.
    /// </summary>
    public BandType BandType
    {
      get { return _bandType; }
      set { _bandType = value; }
    }

    /// <summary>
    /// Get/set the index for the channel's satellite.
    /// </summary>
    public int SatelliteIndex
    {
      get { return _satelliteIndex; }
      set { _satelliteIndex = value; }
    }

    /// <summary>
    /// Get/set the polarisation for the channel's transponder.
    /// </summary>
    public Polarisation Polarisation
    {
      get { return _polarisation; }
      set { _polarisation = value; }
    }

    /// <summary>
    /// Get/set the symbol rate for the channel's transponder.
    /// </summary>
    public int SymbolRate
    {
      get { return _symbolRate; }
      set { _symbolRate = value; }
    }

    /// <summary>
    /// Get/set the modulation scheme for the channel's transponder.
    /// </summary>
    public ModulationType ModulationType
    {
      get { return _modulation; }
      set { _modulation = value; }
    }

    /// <summary>
    /// Get/set the inner FEC rate for the channel's transponder.
    /// </summary>
    public BinaryConvolutionCodeRate InnerFecRate
    {
      get { return _innerFecRate; }
      set { _innerFecRate = value; }
    }

    /// <summary>
    /// Get/set the DVB-S2 pilot signal setting for the channel's transponder.
    /// </summary>
    public Pilot Pilot
    {
      get { return _pilot; }
      set { _pilot = value; }
    }

    /// <summary>
    /// Get/set the DVB-S2 roll-off setting for the channel's transponder.
    /// </summary>
    public RollOff RollOff
    {
      get { return _rollOff; }
      set { _rollOff = value; }
    }

    #endregion

    /// <summary>
    /// Get a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>
    /// </returns>
    public override string ToString()
    {
      string line =
        String.Format(
          "DVBS:{0} SymbolRate:{1} Modulation:{2} Polarisation:{3} InnerFecRate:{4} DiSEqC:{5} band:{6} Pilot:{7} RollOff:{8}",
          base.ToString(), SymbolRate, ModulationType, Polarisation, InnerFecRate, Diseqc, BandType, Pilot, RollOff);
      return line;
    }

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>, otherwise <c>false</c>
    /// </returns>
    public override bool Equals(object obj)
    {
      DVBSChannel ch = obj as DVBSChannel;
      if (ch == null)
      {
        return false;
      }
      if (!base.Equals(obj))
      {
        return false;
      }

      if (ch.Diseqc != _diseqc)
      {
        return false;
      }
      if (ch.BandType != _bandType)
      {
        return false;
      }
      if (ch.SatelliteIndex != _satelliteIndex)
      {
        return false;
      }
      if (ch.Polarisation != _polarisation)
      {
        return false;
      }
      if (ch.SymbolRate != _symbolRate)
      {
        return false;
      }
      if (ch.ModulationType != _modulation)
      {
        return false;
      }
      if (ch.InnerFecRate != _innerFecRate)
      {
        return false;
      }
      if (ch.Pilot != _pilot)
      {
        return false;
      }
      if (ch.RollOff != _rollOff)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"></see></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _diseqc.GetHashCode() ^ _bandType.GetHashCode() ^
            _satelliteIndex.GetHashCode() ^ _polarisation.GetHashCode() ^ _symbolRate.GetHashCode() ^
            _modulation.GetHashCode() ^ _innerFecRate.GetHashCode() ^ _pilot.GetHashCode() ^
            _rollOff.GetHashCode();
    }

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    public override bool IsDifferentTransponder(IChannel channel)
    {
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null)
      {
        return true;
      }
      return dvbsChannel.Diseqc != _diseqc ||
             dvbsChannel.SatelliteIndex != _satelliteIndex ||
             dvbsChannel.Frequency != Frequency ||
             dvbsChannel.Polarisation != _polarisation ||
             dvbsChannel.SymbolRate != _symbolRate ||
             dvbsChannel.ModulationType != _modulation ||
             dvbsChannel.InnerFecRate != _innerFecRate ||
             dvbsChannel.Pilot != _pilot ||
             dvbsChannel.RollOff != _rollOff;             
    }
  }
}