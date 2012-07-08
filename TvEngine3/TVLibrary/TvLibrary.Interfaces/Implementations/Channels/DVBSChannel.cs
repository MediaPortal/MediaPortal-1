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
using TvLibrary.Interfaces.Device;

namespace TvLibrary.Channels
{
  /// <summary>
  /// Helper class for dealing with various aspects of LNB settings.
  /// </summary>
  public class LnbTypeConverter
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
    /// Get the appropriate LNB settings parameters to tune a given channel.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <param name="lnbLowLof">The LNB low band local oscillator frequency (in kHz) to use.</param>
    /// <param name="lnbHighLof">The LNB high band local oscillator frequency (in kHz) to use.</param>
    /// <param name="lnbSwitchFrequency">The LNB switch frequency (in kHz) to use.</param>
    /// <param name="polarisation">The LNB voltage (signal polarity) to use.</param>
    public static void GetLnbTuningParameters(DVBSChannel channel, out uint lnbLowLof, out uint lnbHighLof, out uint lnbSwitchFrequency, out Polarisation polarisation)
    {
      // 1: Log the default frequency settings for the LNB.
      Log.Log.Debug("DvbsChannel: LNB settings, low = {0} kHz, high = {1} kHz, switch = {2} kHz, bandstacked = {3}, toroidal = {4}, polarisation = {5}",
          channel.LnbType.LowBandFrequency, channel.LnbType.HighBandFrequency, channel.LnbType.SwitchFrequency,
          channel.LnbType.IsBandStacked, channel.LnbType.IsToroidal, channel.Polarisation);

      // 2: Toroidal LNB handling.
      // LNBs mounted on a toroidal dish require circular polarities to be inverted.
      polarisation = channel.Polarisation;
      if (channel.LnbType.IsToroidal)
      {
        if (channel.Polarisation == Polarisation.CircularL)
        {
          polarisation = Polarisation.CircularR;
        }
        else if (channel.Polarisation == Polarisation.CircularR)
        {
          polarisation = Polarisation.CircularL;
        }
      }

      // 2: Switch frequency adjustment.
      // Setting the switch frequency to zero is the equivalent of saying that the switch frequency is
      // irrelevant - that the 22 kHz tone state shouldn't depend on the transponder frequency.
      lnbSwitchFrequency = (uint)channel.LnbType.SwitchFrequency;
      if (lnbSwitchFrequency == 0)
      {
        // Note: do not think this is random! Some drivers such as the Genpix SkyWalker driver will treat
        // 20 GHz as a signal to always use high voltage (useful for bandstacked LNBs).
        lnbSwitchFrequency = 18000000;
      }

      // 3: Local oscillator frequency selection and polarisation.
      // Drivers are frustrating! Most tuners rely on the LNB frequency settings to determine the intermediate
      // frequency (the frequency in the cable that the tuner should tune to) and whether the 22 kHz tone
      // should be on or off. Some tuner drivers (eg. Prof USB) don't seem to understand what to do with the
      // three frequencies; others (eg. Anysee E7, SkyStar 2 [BDA], KNC PCI) don't turn the 22 kHz tone on or
      // or off unless certain conditions are met; others can't handle negative intermediate frequencies which
      // are standard for C-band LNB calculations.
      // Our approach is to:
      // - calculate the actual intermediate frequency, then back-calculate a "safe" positive LOF from that
      // - always ensure that the low and high LOF values are different when the 22 kHz tone should be on, and
      //   the same when the 22 kHz tone should be off
      long lof = channel.LnbType.LowBandFrequency;
      bool toneOn = false;
      if (channel.LnbType.IsBandStacked)
      {
        // For bandstacked LNBs, if the transponder polarisation is horizontal or circular left then we
        // should use the nominal high oscillator frequency. In addition, we should always supply
        // bandstacked LNBs with 18 V.
        if (channel.Polarisation == Polarisation.LinearH || channel.Polarisation == Polarisation.CircularL)
        {
          lof = channel.LnbType.HighBandFrequency;
        }
        polarisation = Polarisation.LinearH;
        if (channel.Frequency > lnbSwitchFrequency)
        {
          toneOn = true;
        }
      }
      else
      {
        if (channel.Frequency > lnbSwitchFrequency)
        {
          lof = channel.LnbType.HighBandFrequency;
          toneOn = true;
        }
      }

      long intermediateFrequency = Math.Abs(channel.Frequency - lof);
      lof = channel.Frequency - intermediateFrequency;
      if (toneOn)
      {
        lnbLowLof = (uint)lof - 500000;
        lnbHighLof = (uint)lof;
      }
      else
      {
        lnbLowLof = (uint)lof;
        lnbHighLof = (uint)lof;
      }

      Log.Log.Debug("DvbsChannel: translated LNB settings, low = {0} kHz, high = {1} kHz, switch = {2} kHz, polarisation = {3}",
          lnbLowLof, lnbHighLof, lnbSwitchFrequency, polarisation);
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
    private ILnbType _lnbType = null;
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
      _lnbType = null;
      _satelliteIndex = -1;
      _polarisation = Polarisation.NotSet;
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
    public ILnbType LnbType
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
    /// Get/set the symbol rate for the channel's transponder. The symbol rate unit is ks/s.
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

    #region object overrides

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

    #endregion

    #region ICloneable member

    /// <summary>
    /// Clone the channel instance.
    /// </summary>
    /// <returns>a shallow clone of the channel instance</returns>
    public override object Clone()
    {
      DVBSChannel ch = (DVBSChannel)this.MemberwiseClone();
      ch.LnbType = (ILnbType)ch.LnbType.Clone();
      return ch;
    }

    #endregion

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
             dvbsChannel.LnbType != _lnbType ||
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