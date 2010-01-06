#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
  /// <summary>
  /// Tuning part of DVB-C required for scanning
  /// </summary>
  [Serializable]
  public struct DVBCTuning
  {
    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="p_Frequency">Frequency</param>
    /// <param name="p_ModulationType">ModulationType</param>
    /// <param name="p_SymbolRate">SymbolRate</param>
    public DVBCTuning(long p_Frequency, ModulationType p_ModulationType, int p_SymbolRate)
    {
      Frequency = p_Frequency;
      ModulationType = p_ModulationType;
      SymbolRate = p_SymbolRate;
    }

    /// <summary>
    /// Frequency
    /// </summary>
    public long Frequency;

    /// <summary>
    /// ModulationType
    /// </summary>
    public ModulationType ModulationType;

    /// <summary>
    /// SymbolRate
    /// </summary>
    public int SymbolRate;

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("freq:{0} mod:{1} symbolrate:{2}", Frequency, ModulationType, SymbolRate);
    }
  }

  /// <summary>
  /// class holding all tuning details for DVBC
  /// </summary>
  [Serializable]
  public class DVBCChannel : DVBBaseChannel
  {
    #region variables

    /// <summary>
    /// returns basic tuning info for current channel
    /// </summary>
    public DVBCTuning TuningInfo
    {
      get { return new DVBCTuning(Frequency, ModulationType, SymbolRate); }
      set
      {
        Frequency = value.Frequency;
        ModulationType = value.ModulationType;
        SymbolRate = value.SymbolRate;
      }
    }

    private ModulationType _modulation;
    private int _symbolRate;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBCChannel"/> class.
    /// </summary>
    public DVBCChannel()
    {
      ModulationType = ModulationType.Mod64Qam;
      SymbolRate = 6875;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBCChannel"/> class.
    /// </summary>
    /// <param name="tuning">Tuning detail</param>
    public DVBCChannel(DVBCTuning tuning)
    {
      TuningInfo = tuning;
    }

    #region properties

    /// <summary>
    /// gets/sets the symbolrate for this channel
    /// </summary>
    public int SymbolRate
    {
      get { return _symbolRate; }
      set { _symbolRate = value; }
    }

    /// <summary>
    /// gets/sets the ModulationType for this channel
    /// </summary>
    public ModulationType ModulationType
    {
      get { return _modulation; }
      set { _modulation = value; }
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
      string line = String.Format("DVBC:{0} SymbolRate:{1} Modulation:{2}",
                                  base.ToString(), SymbolRate, ModulationType);
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
      if ((obj as DVBCChannel) == null)
      {
        return false;
      }
      if (!base.Equals(obj))
      {
        return false;
      }
      DVBCChannel ch = obj as DVBCChannel;
      if (ch.ModulationType != ModulationType)
      {
        return false;
      }
      if (ch.SymbolRate != SymbolRate)
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
      return base.GetHashCode() ^ _modulation.GetHashCode() ^ _symbolRate.GetHashCode();
    }

    /// <summary>
    /// Checks if the given channel and this instance are on the different transponder
    /// </summary>
    /// <param name="channel">Channel to check</param>
    /// <returns>true, if the channels are on the same transponder</returns>
    public override bool IsDifferentTransponder(IChannel channel)
    {
      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel == null)
      {
        return true;
      }
      return dvbcChannel.Frequency != Frequency;
    }
  }
}