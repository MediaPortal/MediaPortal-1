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

using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel
{
  /// <summary>
  /// An implementation of <see cref="T:IChannel"/> for DVB-C2 channels.
  /// </summary>  
  [DataContract]
  public class ChannelDvbC2 : ChannelDvbBase, IChannelOfdm
  {
    #region variables

    [DataMember]
    private int _frequency = -1;

    [DataMember]
    private int _bandwidth = -1;

    [DataMember]
    private short _plpId = -1;

    #endregion

    #region properties

    /// <summary>
    /// Get/set the channel transmitter's carrier frequency. The frequency unit is kilo-Hertz (kHz).
    /// </summary>
    public int Frequency
    {
      get
      {
        return _frequency;
      }
      set
      {
        _frequency = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's bandwidth. The bandwidth unit is kilo-Hertz (kHz).
    /// </summary>
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

    /// <summary>
    /// Get/set the identifier of the physical layer pipe that the channel is multiplexed in.
    /// </summary>
    public short PlpId
    {
      get
      {
        return _plpId;
      }
      set
      {
        _plpId = value;
      }
    }

    #endregion

    #region IChannel members

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public override bool IsDifferentTransmitter(IChannel channel)
    {
      ChannelDvbC2 dvbc2Channel = channel as ChannelDvbC2;
      if (
        dvbc2Channel == null ||
        Frequency != dvbc2Channel.Frequency ||
        Bandwidth != dvbc2Channel.Bandwidth ||
        PlpId != dvbc2Channel.PlpId
      )
      {
        return true;
      }
      return false;
    }

    #endregion

    #region object overrides

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>, otherwise <c>false</c></returns>
    public override bool Equals(object obj)
    {
      ChannelDvbC2 channel = obj as ChannelDvbC2;
      if (
        channel == null ||
        !base.Equals(obj) ||
        Frequency != channel.Frequency ||
        Bandwidth != channel.Bandwidth ||
        PlpId != channel.PlpId
      )
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// A hash function for this type.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"/></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ Frequency.GetHashCode() ^
              Bandwidth.GetHashCode() ^ PlpId.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("DVB-C2, {0}, frequency = {1} kHz, bandwidth = {2} kHz, PLP ID = {3}",
                            base.ToString(), Frequency, Bandwidth, PlpId);
    }

    #endregion
  }
}