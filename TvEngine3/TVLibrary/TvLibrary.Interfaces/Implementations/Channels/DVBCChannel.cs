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
  /// A class capable of holding the tuning parameter details required to tune a DVB-C channel.
  /// </summary>
  [Serializable]
  public class DVBCChannel : DVBBaseChannel
  {
    #region variables

    private int _symbolRate = -1;
    private ModulationType _modulation = ModulationType.Mod64Qam;

    #endregion

    #region constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBCChannel"/> class.
    /// </summary>
    public DVBCChannel()
      : base()
    {
      _symbolRate = 6875;
      _modulation = ModulationType.Mod64Qam;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBCChannel"/> class using an existing instance.
    /// </summary>
    /// <param name="channel">The existing channel instance.</param>
    public DVBCChannel(DVBCChannel channel)
      : base(channel)
    {
      _modulation = channel.ModulationType;
      _symbolRate = channel.SymbolRate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBCChannel"/> class using a <see cref="DVBCTuning"/>
    /// instance.
    /// </summary>
    /// <param name="tuningParameters">Core channel tuning parameters.</param>
    public DVBCChannel(DVBCTuning tuningParameters)
    {
      TuningInfo = tuningParameters;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/set the symbol rate for the channel's multiplex.
    /// </summary>
    public int SymbolRate
    {
      get { return _symbolRate; }
      set { _symbolRate = value; }
    }

    /// <summary>
    /// Get/set the modulation scheme for the channel's multiplex.
    /// </summary>
    public ModulationType ModulationType
    {
      get { return _modulation; }
      set { _modulation = value; }
    }

    /// <summary>
    /// Get/set the core tuning parameters for the channel's multiplex.
    /// </summary>
    public DVBCTuning TuningInfo
    {
      get
      {
        return new DVBCTuning(Frequency, _modulation, _symbolRate);
      }
      set
      {
        Frequency = value.Frequency;
        _symbolRate = value.SymbolRate;
        _modulation = value.ModulationType;
      }
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
      string line = String.Format("DVBC:{0} SymbolRate:{1} Modulation:{2}",
                                  base.ToString(), SymbolRate, ModulationType);
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
      DVBCChannel ch = obj as DVBCChannel;
      if (ch == null)
      {
        return false;
      }
      if (!base.Equals(obj))
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

      return true;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"></see></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _symbolRate.GetHashCode() ^ _modulation.GetHashCode();
    }

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    public override bool IsDifferentTransponder(IChannel channel)
    {
      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel == null)
      {
        return true;
      }
      return dvbcChannel.Frequency != Frequency ||
             dvbcChannel.SymbolRate != _symbolRate ||
             dvbcChannel.ModulationType != _modulation;
    }
  }
}