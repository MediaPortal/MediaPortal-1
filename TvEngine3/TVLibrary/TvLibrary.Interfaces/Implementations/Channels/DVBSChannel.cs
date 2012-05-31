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
  public enum LnbType
  {
    /// <summary>
    /// Ku-Linear - LOF1 9750, LOF2 10600, SW 11700
    /// "Universal" LNB, common in Europe.
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
    /// DishPro Ku High (DBS) - LOF1 11250, LOF2 14350
    /// </summary>
    DpBandStackedHigh = 3,
    /// <summary>
    /// DishPro Ku Low (FSS) - LOF1 10750, LOF2 13850
    /// </summary>
    DpBandStackedLow = 4,
    /// <summary>
    /// North American Bandstacked Ku High (DBS) - LOF1 11250, LOF2 10675
    /// </summary>
    NaBandStackedHigh = 5,
    /// <summary>
    /// North American Bandstacked Ku Low (FSS) - LOF1 10750, LOF2 10175
    /// </summary>
    NaBandStackedLow = 6,
    /// <summary>
    /// Bandstacked C-Band LOF1 5150, LOF2 5750
    /// http://www.sadoun.com/Sat/Products/Eagle-Aspen/B1-SAT-Stack-C-Band-LNBF.htm
    /// </summary>
    BandStackedC = 7,
    /// <summary>
    /// Sadoun Bandstacked Ku Low (FSS) - LOF1 10100, LOF2 10750
    /// http://www.sadoun.com/Sat/Products/S/KBSL1-Sadoun-Bandstacked-Single-LNBF.htm
    /// </summary>
    SadounBandstackedLow = 8,
    /// <summary>
    /// Single oscillator 10750 MHz - LOF1 10750
    /// </summary>
    Lnb10750ToneOff = 9,
    /// <summary>
    /// Single oscillator 10750 MHz, 22 kHz tone on - LOF1 10750, LOF2 10750, SW 11700
    /// </summary>
    Lnb10750ToneOn = 10,
    /// <summary>
    /// Single oscillator 11250 MHz - LOF1 11250
    /// </summary>
    Lnb11250ToneOff = 11,
    /// <summary>
    /// Single oscillator 11250 MHz ("Legacy"), 22 kHz tone on - LOF1 11250, LOF2 11250, SW 12200
    /// </summary>
    Lnb11250ToneOn = 12,
    /// <summary>
    /// Single oscillator 11300 MHz - LOF1 11300
    /// </summary>
    Lnb11300ToneOff = 13,
    /// <summary>
    /// Single oscillator 11300 MHz, 22 kHz tone on - LOF1 11300, LOF2 11300, SW 12250
    /// </summary>
    Lnb11300ToneOn = 14
  }

  /// <summary>
  /// Enum listing DiSEqC switch commands for DiSEqC 1.0 and 1.1 compatible switches.
  /// </summary>
  public enum DiseqcPort
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
  /// Helper class for dealing with various aspects of LNB settings.
  /// </summary>
  public class BandTypeConverter
  {
    /// <summary>
    /// Get the switch port number (or LNB number) for a given DiSEqC switch command.
    /// </summary>
    /// <param name="command">The DiSEqC switch command.</param>
    /// <returns>the switch port number associated with the command</returns>
    public static int GetPortNumber(DiseqcPort command)
    {
      switch (command)
      {
        case DiseqcPort.None:
          return 0;   // no DiSEqC
        case DiseqcPort.SimpleA:
          return 1;
        case DiseqcPort.SimpleB:
          return 2;
        case DiseqcPort.PortA:
          return 1;
        case DiseqcPort.PortB:
          return 2;
        case DiseqcPort.PortC:
          return 3;
        case DiseqcPort.PortD:
          return 4;
      }
      // DiSEqC 1.1 commands...
      return ((int)command - 6);
    }

    /// <summary>
    /// Determine if tuning a given channel requires the LNB to be in low (22 kHz off) or high (22 kHz on)
    /// band mode.
    /// </summary>
    /// <param name="channel">Tuning details for the channel.</param>
    /// <param name="parameters">Manually defined LNB oscillator frequency parameters.</param>
    /// <returns><c>true</c> if the channel is broadcast from a high band transponder, otherwise <c>false</c></returns>
    public static bool IsHighBand(DVBSChannel channel, ScanParameters parameters)
    {
      uint lof1, lof2, sw;
      GetDefaultLnbSetup(parameters, channel.LnbType, out lof1, out lof2, out sw);

      if (sw == 0)
      {
        return false;
      }
      return channel.Frequency >= (sw * 1000);
    }

    /// <summary>
    /// Get the local oscillator frequencies corresponding with a particular LNB type. This function also
    /// considers the case when the frequencies are overriden.
    /// </summary>
    /// <param name="parameters">Manually defined LNB oscillator frequency parameters.</param>
    /// <param name="lnbType">The LNB type.</param>
    /// <param name="lof1">The LNB low local oscillator frequency in MHz.</param>
    /// <param name="lof2">The LNB high local oscillator frequency in MHz.</param>
    /// <param name="sw">The LNB switch frequency in MHz.</param>
    public static void GetDefaultLnbSetup(ScanParameters parameters, LnbType lnbType, out uint lof1, out uint lof2,
                                          out uint sw)
    {
      lof1 = lof2 = sw = 0;
      if (parameters.UseDefaultLnbFrequencies == false)
      {
        lof1 = (uint)parameters.LnbLowFrequency;
        lof2 = (uint)parameters.LnbHighFrequency;
        sw = (uint)parameters.LnbSwitchFrequency;
        return;
      }
      switch (lnbType)
      {
        case LnbType.Universal:
          lof1 = 9750;
          lof2 = 10600;
          sw = 11700;
          break;
        case LnbType.Circular:
          lof1 = 10750;
          lof2 = 0;
          sw = 0;
          break;
        case LnbType.CBand:
          lof1 = 5150;
          lof2 = 0;
          sw = 0;
          break;
        case LnbType.DpBandStackedHigh:
          lof1 = 11250;
          lof2 = 14350;
          sw = 0;
          break;
        case LnbType.DpBandStackedLow:
          lof1 = 10750;
          lof2 = 13850;
          sw = 0;
          break;
        case LnbType.NaBandStackedHigh:
          lof1 = 11250;
          lof2 = 10675;
          sw = 0;
          break;
        case LnbType.NaBandStackedLow:
          lof1 = 10750;
          lof2 = 10175;
          sw = 0;
          break;
        case LnbType.BandStackedC:
          lof1 = 5150;
          lof2 = 5750;
          sw = 0;
          break;
        case LnbType.SadounBandstackedLow:
          lof1 = 10100;
          lof2 = 10750;
          sw = 0;
          break;
        case LnbType.Lnb10750ToneOff:
          lof1 = 10750;
          lof2 = 0;
          sw = 0;
          break;
        case LnbType.Lnb10750ToneOn:
          lof1 = 10750;
          lof2 = 10750;
          sw = 11700;
          break;
        case LnbType.Lnb11250ToneOff:
          lof1 = 11250;
          lof2 = 0;
          sw = 0;
          break;
        case LnbType.Lnb11250ToneOn:
          lof1 = 11250;
          lof2 = 11250;
          sw = 12200;
          break;
        case LnbType.Lnb11300ToneOff:
          lof1 = 11300;
          lof2 = 0;
          sw = 0;
          break;
        case LnbType.Lnb11300ToneOn:
          lof1 = 11300;
          lof2 = 11300;
          sw = 12250;
          break;
      }
    }

    /// <summary>
    /// Determine whether a particular type of LNB is bandstacked.
    /// </summary>
    /// <remarks>
    /// Unlike regular LNBs, bandstacked LNBs pass all transponders in a single cable at all times. This is
    /// as opposed with a regular LNB which only passes the transponders with a particular polarity, depending
    /// on the voltage supplied. Bandstacking greatly improves the ability to split the signal to more than
    /// one receiver without complicated or expensive switch equipement. The cost is that the LNB is not able
    /// to receive as wide a range of transponders as a regular LNB. This is due to the fact that the LNB
    /// shifts the frequencies of each transponder in such a way that they don't overlap... but this
    /// requires the use of bandwidth that would otherwise be used for receiving a wider frequency range.
    /// The net result is that all the transponders of a particular polarity (usually vertical or circular
    /// right) end up with a lower frequency than the other transponders.
    /// </remarks>
    /// <param name="lnbType">The type of LNB to check.</param>
    /// <returns><c>true</c> if the LNB type is a bandstacked type, otherwise <c>false</c></returns>
    public static bool IsBandstackedLnb(LnbType lnbType)
    {
      if (lnbType == LnbType.BandStackedC || lnbType == LnbType.DpBandStackedHigh ||
        lnbType == LnbType.DpBandStackedLow || lnbType == LnbType.NaBandStackedHigh ||
        lnbType == LnbType.NaBandStackedLow || lnbType == LnbType.SadounBandstackedLow)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Get the appropriate LNB settings parameters to tune a given channel.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <param name="parameters">Manually defined LNB oscillator frequency parameters.</param>
    /// <param name="lnbLof">The LNB local oscillator frequency to use in MHz.</param>
    /// <param name="lnbSwitchFrequency">The LNB switch frequency to use in MHz.</param>
    /// <param name="polarisation">The LNB voltage (signal polarity) to set.</param>
    public static void GetLnbTuningParameters(DVBSChannel channel, ScanParameters parameters,
                          out uint lnbLof, out uint lnbSwitchFrequency, out Polarisation polarisation
    )
    {
      // 1: Get the default frequency settings for the LNB.
      uint lowLof;
      uint highLof;
      BandTypeConverter.GetDefaultLnbSetup(parameters, channel.LnbType, out lowLof, out highLof, out lnbSwitchFrequency);
      Log.Log.Debug("TvCardDvbS: LNB settings, low = {0} MHz, high = {1} MHz, switch = {2} MHz, polarisation = {3}", lowLof, highLof, lnbSwitchFrequency, channel.Polarisation);

      // 2: Switch frequency adjustment.
      // Setting the switch frequency to zero is the equivalent of saying that the switch frequency is
      // irrelevant - that the 22 kHz tone state shouldn't depend on the transponder frequency.
      if (lnbSwitchFrequency == 0)
      {
        lnbSwitchFrequency = 18000;
      }

      // 3: Local oscillator frequency selection and other miscellenaeous settings.
      // Some tuners (eg. Prof USB series) don't handle multiple LOFs correctly. We need to pass either
      // the low or high frequency with the switch frequency. How do we select the correct local oscillator
      // frequency? Well first assume that we should use the low frequency, then:
      // 1. For non-bandstacked LNBs, if the transponder frequency is higher than the switch frequency then
      //    we should use the high oscillator frequency.
      // 2. For bandstacked LNBs, if the transponder polarisation is horizontal or circular left then we
      //    should use the high oscillator frequency.
      // In addition, we should always supply bandstacked LNBs with 18 V.
      lnbLof = lowLof;
      polarisation = channel.Polarisation;
      bool isBandstackedLnb = BandTypeConverter.IsBandstackedLnb(channel.LnbType);
      if (!isBandstackedLnb)
      {
        if (channel.Frequency > (lnbSwitchFrequency * 1000))
        {
          lnbLof = highLof;
        }
      }
      else
      {
        if (channel.Polarisation == Polarisation.LinearH || channel.Polarisation == Polarisation.CircularL)
        {
          lnbLof = highLof;
        }
        polarisation = Polarisation.LinearH;
      }
      Log.Log.Debug("TvCardDvbS: LNB translated settings, oscillator = {0} MHz, switch = {1} MHz, polarisation = {2}", lnbLof, lnbSwitchFrequency, polarisation);
    }
  }

  /// <summary>
  /// A class capable of holding the tuning parameter details required to tune a DVB-S or DVB-S2 channel.
  /// </summary>
  [Serializable]
  public class DVBSChannel : DVBBaseChannel
  {
    #region variables

    private DiseqcPort _diseqc = DiseqcPort.None;
    private LnbType _lnbType = LnbType.Universal;
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
    /// Initialise a new instance of the <see cref="DVBSChannel"/> class.
    /// </summary>
    public DVBSChannel()
      : base()
    {
      _diseqc = DiseqcPort.None;
      _lnbType = LnbType.Universal;
      _satelliteIndex = -1;
      _polarisation = DirectShowLib.BDA.Polarisation.NotSet;
      _symbolRate = -1;
      _modulation = ModulationType.ModQpsk;
      _innerFecRate = BinaryConvolutionCodeRate.RateNotSet;
      _pilot = Pilot.NotSet;
      _rollOff = RollOff.NotSet;
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="DVBSChannel"/> class using an existing instance.
    /// </summary>
    /// <param name="channel">The existing channel instance.</param>
    public DVBSChannel(DVBSChannel channel)
      : base(channel)
    {
      _diseqc = channel.Diseqc;
      _lnbType = channel.LnbType;
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
    public DiseqcPort Diseqc
    {
      get { return _diseqc; }
      set { _diseqc = value; }
    }

    /// <summary>
    /// Get/set the type of LNB used to receive the channel.
    /// </summary>
    public LnbType LnbType
    {
      get { return _lnbType; }
      set { _lnbType = value; }
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
          base.ToString(), SymbolRate, ModulationType, Polarisation, InnerFecRate, Diseqc, LnbType, Pilot, RollOff);
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
      if (ch.LnbType != _lnbType)
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
      return base.GetHashCode() ^ _diseqc.GetHashCode() ^ _lnbType.GetHashCode() ^
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