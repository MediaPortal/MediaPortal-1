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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel
{
  /// <summary>
  /// An implementation of <see cref="T:IChannel"/> for ISDB-C channels.
  /// </summary>
  [DataContract]
  public class ChannelIsdbC : ChannelDvbBase, IChannelIsdb, IChannelQam
  {
    #region variables

    [DataMember]
    private int _frequency = -1;

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
    /// Get/set the channel transmitter's modulation scheme.
    /// </summary>
    /// <remarks>
    /// This parameter is typically not needed for tuning.
    /// </remarks>
    public ModulationSchemeQam ModulationScheme
    {
      get
      {
        return ModulationSchemeQam.Qam64;
      }
      set
      {
      }
    }

    /// <summary>
    /// Get the channel transmitter's symbol rate. The symbol rate unit is kilo-symbols per second (ks/s).
    /// </summary>
    /// <remarks>
    /// This parameter is typically not needed for tuning.
    /// </remarks>
    public int SymbolRate
    {
      get
      {
        return 5274;
      }
      set
      {
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
      ChannelIsdbC isdbcChannel = channel as ChannelIsdbC;
      if (isdbcChannel == null || Frequency != isdbcChannel.Frequency)
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
      ChannelScte channel = obj as ChannelScte;
      if (
        channel == null ||
        !base.Equals(obj) ||
        Frequency != channel.Frequency
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
      return base.GetHashCode() ^ Frequency.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("ISDB-C, {0}, frequency = {1} kHz", base.ToString(), Frequency);
    }

    #endregion
  }
}