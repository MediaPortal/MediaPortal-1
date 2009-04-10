/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using DirectShowLib.BDA;

namespace TvLibrary.Channels
{
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
  /// Class for LNB setup and LNB number
  /// Helps determin the DVB-S band type and subsequent LNB frequencies
  /// Also determins if hi band tuning is required
  /// </summary>
  public class BandTypeConverter
  {
    /// <summary>
    /// Gets the Antenna Number (or LNB number)
    /// </summary>
    /// <param name="channel">holds tuning details for DVB-S</param>
    /// <returns></returns>
    public static int GetAntennaNr(DVBSChannel channel)
    {
      byte disEqcPort = 0;

      switch (channel.DisEqc)
      {
        case DisEqcType.None:
          disEqcPort = 0; //no diseqc
          break;
        case DisEqcType.SimpleA: //simple A
          disEqcPort = 1;
          break;
        case DisEqcType.SimpleB: //simple B
          disEqcPort = 2;
          break;
        case DisEqcType.Level1AA: //Level 1 A/A
          disEqcPort = 1;
          break;
        case DisEqcType.Level1AB: //Level 1 A/B
          disEqcPort = 2;
          break;
        case DisEqcType.Level1BA: //Level 1 B/A
          disEqcPort = 3;
          break;
        case DisEqcType.Level1BB: //Level 1 B/B
          disEqcPort = 4;
          break;
      }
      return disEqcPort;
    }

    /// <summary>
    /// Determins if the tuning paramter is HiBand and if so involke the 22Khz switch.
    /// </summary>
    /// <param name="channel">tuning details for specific channel / frequency</param>
    /// <param name="parameters">holds the parameters needed for tuning channel </param>
    /// <returns></returns>
    public static bool IsHiBand(DVBSChannel channel, ScanParameters parameters)
    {
      int lof1, lof2, sw;
      GetDefaultLnbSetup(parameters, channel.BandType, out lof1, out lof2, out sw);

      if (sw == 0)
      {
        return false;
      }
      return channel.Frequency >= (sw*1000);
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
  /// enum describing the different DisEqc type
  /// </summary>
  public enum DisEqcType
  {
    /// <summary>
    /// diseqc not used
    /// </summary>
    None = 0,
    /// <summary>
    /// Simple A
    /// </summary>
    SimpleA = 1,
    /// <summary>
    /// Simple B
    /// </summary>
    SimpleB = 2,
    /// <summary>
    /// Level 1 A/A
    /// </summary>
    Level1AA = 3,
    /// <summary>
    /// Level 1 A/B
    /// </summary>
    Level1AB = 4,
    /// <summary>
    /// Level 1 B/A
    /// </summary>
    Level1BA = 5,
    /// <summary>
    /// Level 1 B/B
    /// </summary>
    Level1BB = 6,
  } ;

  /// <summary>
  /// class holding all tuning details for DVBS
  /// </summary>
  [Serializable]
  public class DVBSChannel : DVBBaseChannel
  {
    #region variables

    private Polarisation _polarisation;
    private int _symbolRate;
    private int _switchingFrequency;
    private DisEqcType _disEqc;
    private BandType _bandType;
    private ModulationType _modulation = ModulationType.ModQpsk;
    private BinaryConvolutionCodeRate _innerFecRate = BinaryConvolutionCodeRate.RateNotSet;
    private Pilot _pilot = Pilot.NotSet;
    private RollOff _rollOff = RollOff.NotSet;
    private int _satelliteIndex;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBSChannel"/> class.
    /// </summary>
    /// <param name="chan">The chan.</param>
    public DVBSChannel(DVBSChannel chan)
      : base(chan)
    {
      _polarisation = chan.Polarisation;
      _symbolRate = chan.SymbolRate;
      _switchingFrequency = chan.SwitchingFrequency;
      DisEqc = chan.DisEqc;
      _bandType = chan.BandType;
      _modulation = chan.ModulationType;
      _innerFecRate = chan.InnerFecRate;
      _pilot = chan.Pilot;
      _rollOff = chan.Rolloff;
      _satelliteIndex = chan.SatelliteIndex;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBSChannel"/> class.
    /// </summary>
    public DVBSChannel()
    {
      SwitchingFrequency = 0;
      DisEqc = DisEqcType.SimpleA;
      _bandType = BandType.Universal;
      _satelliteIndex = -1;
      _modulation = ModulationType.ModQpsk;
      _innerFecRate = BinaryConvolutionCodeRate.RateNotSet;
      _pilot = Pilot.NotSet;
      _rollOff = RollOff.NotSet;
    }

    #region properties

    /// <summary>
    /// gets/sets the InnerFEC Rate for this channel
    /// </summary>
    public BinaryConvolutionCodeRate InnerFecRate
    {
      get { return _innerFecRate; }
      set { _innerFecRate = value; }
    }

    /// <summary>
    /// gets/sets the Modulation type for this channel
    /// </summary>
    public ModulationType ModulationType
    {
      get { return _modulation; }
      set { _modulation = value; }
    }

    /// <summary>
    /// gets/sets the Satellite Index for this channel
    /// </summary>
    public int SatelliteIndex
    {
      get { return _satelliteIndex; }
      set { _satelliteIndex = value; }
    }

    /// <summary>
    /// gets/sets the BandType for this channel
    /// </summary>
    public BandType BandType
    {
      get { return _bandType; }
      set { _bandType = value; }
    }

    /// <summary>
    /// gets/sets the Polarisation for this channel
    /// </summary>
    public Polarisation Polarisation
    {
      get { return _polarisation; }
      set { _polarisation = value; }
    }

    /// <summary>
    /// gets/sets the SymbolRate for this channel
    /// </summary>
    public int SymbolRate
    {
      get { return _symbolRate; }
      set { _symbolRate = value; }
    }

    /// <summary>
    /// gets/sets the LNB Switch frequency for this channel
    /// </summary>
    public int SwitchingFrequency
    {
      get { return _switchingFrequency; }
      set { _switchingFrequency = value; }
    }

    /// <summary>
    /// gets/sets the DiSEqC setting for this channel
    /// </summary>
    public DisEqcType DisEqc
    {
      get { return _disEqc; }
      set { _disEqc = value; }
    }

    /// <summary>
    /// gets/sets the Pilot setting for this channel
    /// </summary>
    public Pilot Pilot
    {
      get { return _pilot; }
      set { _pilot = value; }
    }

    /// <summary>
    /// gets/sets the Roll-Off setting for this channel
    /// </summary>
    public RollOff Rolloff
    {
      get { return _rollOff; }
      set { _rollOff = value; }
    }

    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      string line =
        String.Format(
          "DVBS:{0} SymbolRate:{1} Modulation:{2} Polarisation:{3} InnerFecRate:{4} DisEqc:{5} band:{6} Pilot:{7} RollOff:{8}",
          base.ToString(), SymbolRate, ModulationType, Polarisation, InnerFecRate, DisEqc, BandType, Pilot, Rolloff);
      return line;
    }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      if ((obj as DVBSChannel) == null)
      {
        return false;
      }
      if (!base.Equals(obj))
      {
        return false;
      }
      DVBSChannel ch = obj as DVBSChannel;
      if (ch.Polarisation != Polarisation)
      {
        return false;
      }
      if (ch.SatelliteIndex != SatelliteIndex)
      {
        return false;
      }
      if (ch.SymbolRate != SymbolRate)
      {
        return false;
      }
      if (ch.SwitchingFrequency != SwitchingFrequency)
      {
        return false;
      }
      if (ch.DisEqc != DisEqc)
      {
        return false;
      }
      if (ch.BandType != BandType)
      {
        return false;
      }
      if (ch.ModulationType != ModulationType)
      {
        return false;
      }
      if (ch.InnerFecRate != InnerFecRate)
      {
        return false;
      }
      if (ch.Pilot != Pilot)
      {
        return false;
      }
      if (ch.Rolloff != Rolloff)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _polarisation.GetHashCode() ^ _symbolRate.GetHashCode() ^
             _switchingFrequency.GetHashCode() ^ _disEqc.GetHashCode() ^ _bandType.GetHashCode()
             ^ SatelliteIndex.GetHashCode() ^ _modulation.GetHashCode() ^ _innerFecRate.GetHashCode() ^
             _pilot.GetHashCode() ^ _rollOff.GetHashCode();
    }
  }
}