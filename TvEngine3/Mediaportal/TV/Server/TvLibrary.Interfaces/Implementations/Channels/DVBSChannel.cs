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
using System.Runtime.Serialization;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels
{
  /// <summary>
  /// A class capable of holding the tuning parameter details required to tune a DVB-S or DVB-S2 channel.
  /// </summary>
  [DataContract]
  public class DVBSChannel : DVBBaseChannel
  {
    #region variables

    [DataMember]
    private DiseqcPort _diseqc = DiseqcPort.None;

    [DataMember]
    private LnbType _lnbType = null;

    [DataMember]
    private int _satelliteIndex = -1;

    [DataMember]
    private Polarisation _polarisation = Polarisation.NotSet;

    [DataMember]
    private int _symbolRate = -1;

    [DataMember]
    private ModulationType _modulation = ModulationType.ModQpsk;

    [DataMember]
    private BinaryConvolutionCodeRate _innerFecRate = BinaryConvolutionCodeRate.RateNotSet;

    [DataMember]
    private Pilot _pilot = Pilot.NotSet;

    [DataMember]
    private RollOff _rollOff = RollOff.NotSet;

    #endregion

    #region constructors

    /// <summary>
    /// Initialise a new instance of the <see cref="DVBSChannel"/> class.
    /// </summary>
    public DVBSChannel()
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
      DVBSChannel ch = (DVBSChannel)MemberwiseClone();
      if (LnbType != null)
      {
        ch.LnbType = LnbType.Clone();
      }
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

    /// <summary>
    /// Get a channel instance with properties set to enable tuning of this channel.
    /// </summary>
    /// <returns>a channel instance with parameters adjusted as necessary</returns>
    public override IChannel GetTuningChannel()
    {
      IChannel clone = (IChannel)Clone();
      DVBSChannel dvbsChannel = clone as DVBSChannel;
      if (dvbsChannel == null)
      {
        return clone;
      }

      // 1: Log the default frequency settings for the LNB.
      this.LogDebug("DvbsChannel: LNB settings, low = {0} kHz, high = {1} kHz, switch = {2} kHz, bandstacked = {3}, toroidal = {4}, polarisation = {5}",
          dvbsChannel.LnbType.LowBandFrequency, dvbsChannel.LnbType.HighBandFrequency, dvbsChannel.LnbType.SwitchFrequency,
          dvbsChannel.LnbType.IsBandStacked, dvbsChannel.LnbType.IsToroidal, dvbsChannel.Polarisation);

      // 2: Toroidal LNB handling.
      // LNBs mounted on a toroidal dish require circular polarities to be inverted. Note that it is important to do
      // this before the bandstacked LNB logic.
      if (dvbsChannel.LnbType.IsToroidal)
      {
        if (dvbsChannel.Polarisation == Polarisation.CircularL)
        {
          dvbsChannel.Polarisation = Polarisation.CircularR;
        }
        else if (dvbsChannel.Polarisation == Polarisation.CircularR)
        {
          dvbsChannel.Polarisation = Polarisation.CircularL;
        }
      }

      // 3: Bandstacked LNB handling.
      // For bandstacked LNBs, if the transponder polarisation is horizontal or circular left then we
      // should use the nominal high oscillator frequency. In addition, we should always supply bandstacked
      // LNBs with 18 V for reliable operation.
      if (dvbsChannel.LnbType.IsBandStacked)
      {
        if (dvbsChannel.Polarisation == Polarisation.LinearH || dvbsChannel.Polarisation == Polarisation.CircularL)
        {
          dvbsChannel.LnbType.LowBandFrequency = dvbsChannel.LnbType.HighBandFrequency;
        }
        dvbsChannel.LnbType.HighBandFrequency = dvbsChannel.LnbType.LowBandFrequency + 500000;
        dvbsChannel.Polarisation = Polarisation.LinearH;
      }

      // A note about LNB settings...
      // The LNB settings in the database have been set *very* intentionally based on the following assumptions and
      // information.
      // Golden rule: you should never pass zero to a BDA tuner driver for any LNB frequency setting.
      // Very often people who don't know any better will set the high oscillator frequency and/or switch frequency
      // for single oscillator LNBs to zero to indicate that they are irrelevant. Driver behaviour with respect to
      // the 22 kHz tone should be considered undefined in that situation. In some cases the driver behaviour wouldn't
      // matter, however consider:
      // - some single oscillator LNBs have been known to respond to the 22 kHz tone
      // - the tone may degrade the signal quality
      // - the 22 kHz tone state is still important in an environment with mixed LNB types or 22 kHz tone switches
      // 
      // In the database, we set the high oscillator frequency to [low LOF] + 500000 kHz for single oscillator LNBs.
      // Our intention is to ensure that the low and high oscillator frequencies are different as some drivers (for
      // example Anysee E7, SkyStar 2 [BDA]) don't turn the 22 kHz tone on or off (!!!) if the frequencies are not
      // different. Other drivers (for example KNC TV-Station PCI) require that the frequencies be the same in order
      // to turn the 22 kHz tone off - these drivers should be handled with plugins. Note that the 500000 kHz value
      // is arbitrary.
      // In the database, we also set the switch frequency to 18000000 kHz when the 22 kHz tone should be turned off.
      // This value is *not* arbitrary. Some drivers (for example Genpix SkyWalker) will treat 20 GHz as a signal to
      // always use high voltage (useful for bandstacked LNBs).

      this.LogDebug("DvbsChannel: translated LNB settings, low = {0} kHz, high = {1} kHz, switch = {2} kHz, polarisation = {3}",
          dvbsChannel.LnbType.LowBandFrequency, dvbsChannel.LnbType.HighBandFrequency, dvbsChannel.LnbType.SwitchFrequency,
          dvbsChannel.Polarisation);
      return dvbsChannel;
    }
  }
}