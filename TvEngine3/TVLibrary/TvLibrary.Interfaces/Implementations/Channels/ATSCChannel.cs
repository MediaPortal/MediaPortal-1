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
  /// A class capable of holding the tuning parameter details required to tune an ATSC or QAM channel.
  /// </summary>
  [Serializable]
  public class ATSCChannel : DVBBaseChannel
  {
    #region variables

    private int _physicalChannel = -1;
    private int _majorChannel = -1;
    private int _minorChannel = -1;
    private ModulationType _modulation = ModulationType.ModNotSet;

    #endregion

    #region constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ATSCChannel"/> class.
    /// </summary>
    public ATSCChannel()
    {
      _physicalChannel = -1;
      _majorChannel = -1;
      _minorChannel = -1;
      _modulation = ModulationType.Mod8Vsb;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ATSCChannel"/> class using an existing instance.
    /// </summary>
    /// <param name="channel">The existing channel instance.</param>
    public ATSCChannel(ATSCChannel channel)
      : base(channel)
    {
      _physicalChannel = channel.PhysicalChannel;
      _majorChannel = channel.MajorChannel;
      _minorChannel = channel.MinorChannel;
      _modulation = channel.ModulationType;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/set the physical channel number for the channel's transmitter.
    /// </summary>
    public int PhysicalChannel
    {
      get { return _physicalChannel; }
      set { _physicalChannel = value; }
    }

    /// <summary>
    /// Get/set the channel's major channel number.
    /// </summary>
    public int MajorChannel
    {
      get { return _majorChannel; }
      set { _majorChannel = value; }
    }

    /// <summary>
    /// Get/set the channel's minor channel number.
    /// </summary>
    public int MinorChannel
    {
      get { return _minorChannel; }
      set { _minorChannel = value; }
    }

    /// <summary>
    /// Get/set the modulation scheme for the channel's transmitter.
    /// </summary>
    public ModulationType ModulationType
    {
      get { return _modulation; }
      set { _modulation = value; }
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
      return String.Format("ATSC:{0} phys:{1} maj:{2} min:{3} mod:{4}", base.ToString(), _physicalChannel, _majorChannel,
                           _minorChannel, _modulation);
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
      ATSCChannel ch = obj as ATSCChannel;
      if (ch == null)
      {
        return false;
      }
      if (!base.Equals(obj))
      {
        return false;
      }

      if (ch.PhysicalChannel != _physicalChannel)
      {
        return false;
      }
      if (ch.MajorChannel != _majorChannel)
      {
        return false;
      }
      if (ch.MinorChannel != _minorChannel)
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
      return base.GetHashCode() ^ _physicalChannel.GetHashCode() ^ _majorChannel.GetHashCode() ^
            _minorChannel.GetHashCode() ^ _modulation.GetHashCode();
    }

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    public override bool IsDifferentTransponder(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        return true;
      }

      // ATSC (over-the-air digital television).
      if (_modulation == ModulationType.Mod8Vsb)
      {
        return atscChannel.PhysicalChannel != _physicalChannel;
      }
      // QAM (cable television).
      else if (_modulation == ModulationType.Mod256Qam)
      {
        return atscChannel.Frequency != Frequency;
      }
      return true;
    }
  }
}