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
using TvLibrary.Interfaces;

namespace TvLibrary.Channels
{
  /// <summary>
  /// Tuning part of DVB-T required for scanning
  /// </summary>
  [Serializable]
  public struct DVBTTuning
  {
    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="p_Frequency">Frequency</param>
    /// <param name="p_BandWidth">BandWidth</param>
    /// <param name="p_Offset">Offset</param>
    public DVBTTuning(long p_Frequency, int p_BandWidth, int p_Offset)
    {
      Frequency = p_Frequency;
      BandWidth = p_BandWidth;
      Offset = p_Offset;
    }

    /// <summary>
    /// Frequency
    /// </summary>
    public long Frequency;

    /// <summary>
    /// BandWidth
    /// </summary>
    public int BandWidth;

    /// <summary>
    /// Offset
    /// </summary>
    public int Offset;

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("freq:{0}/{2} bandwidth:{1}", Frequency, BandWidth, Offset);
    }
  }

  /// <summary>
  /// A class capable of holding the tuning parameter details required to tune a DVB-T channel.
  /// </summary>
  [Serializable]
  public class DVBTChannel : DVBBaseChannel
  {
    #region variables

    private int _bandwidth = 8;
    private int _offset = 0;

    #endregion

    #region constructors

    /// <summary>
    /// Initialise a new instance of the <see cref="DVBTChannel"/> class.
    /// </summary>
    public DVBTChannel()
      : base()
    {
      _bandwidth = 8;
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="DVBTChannel"/> class using an existing instance.
    /// </summary>
    /// <param name="channel">The existing channel instance.</param>
    public DVBTChannel(DVBTChannel channel)
      : base(channel)
    {
      _bandwidth = channel.Bandwidth;
      _offset = channel.Offset;
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="DVBTChannel"/> class using a <see cref="DVBTTuning"/>
    /// instance.
    /// </summary>
    /// <param name="tuningParameters">Core channel tuning parameters.</param>
    public DVBTChannel(DVBTTuning tuningParameters)
    {
      TuningInfo = tuningParameters;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/set the bandwidth for this channel's transmitter.
    /// </summary>
    public int Bandwidth
    {
      get { return _bandwidth; }
      set { _bandwidth = value; }
    }

    /// <summary>
    /// Get/set the frequency offset for this channel's transmitter.
    /// </summary>
    public int Offset
    {
      get { return _offset; }
      set { _offset = value; }
    }

    /// <summary>
    /// Get/set the core tuning parameters for the channel's transmitter.
    /// </summary>
    public DVBTTuning TuningInfo
    {
      get { return new DVBTTuning(Frequency, _bandwidth, _offset); }
      set
      {
        Frequency = value.Frequency;
        _bandwidth = value.BandWidth;
        _offset = value.Offset;
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
      string line = String.Format("DVBT:{0} Bandwidth:{1}", base.ToString(), Bandwidth);
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
      DVBTChannel ch = obj as DVBTChannel;
      if (ch == null)
      {
        return false;
      }
      if (!base.Equals(obj))
      {
        return false;
      }

      if (ch.Bandwidth != _bandwidth)
      {
        return false;
      }
      if (ch.Offset != _offset)
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
      return base.GetHashCode() ^ _bandwidth.GetHashCode() ^ _offset.GetHashCode();
    }

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    public override bool IsDifferentTransponder(IChannel channel)
    {
      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel == null)
      {
        return true;
      }
      return dvbtChannel.Frequency != Frequency ||
             dvbtChannel.Bandwidth != _bandwidth ||
             dvbtChannel.Offset != _offset;
    }
  }
}