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
using System.Text.RegularExpressions;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel
{
  /// <summary>
  /// An implementation of <see cref="T:IChannel"/> for ATSC channels.
  /// </summary>
  [DataContract]
  public class ChannelAtsc : ChannelMpeg2Base, IChannelPhysical
  {
    #region variables

    [DataMember]
    private int _frequency = -1;

    [DataMember]
    private ModulationSchemeVsb _modulationScheme = ModulationSchemeVsb.Automatic;

    [DataMember]
    private int _sourceId = -1;

    #endregion

    #region properties

    /// <summary>
    /// Get the number of the physical (broadcast) channel that the channel is transmitted in.
    /// </summary>
    public short PhysicalChannelNumber
    {
      get
      {
        return GetPhysicalChannelNumberForFrequency(Frequency);
      }
    }

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
    public ModulationSchemeVsb ModulationScheme
    {
      get
      {
        return _modulationScheme;
      }
      set
      {
        _modulationScheme = value;
      }
    }

    /// <summary>
    /// Get/set the channel's source identifier.
    /// </summary>
    public int SourceId
    {
      get
      {
        return _sourceId;
      }
      set
      {
        _sourceId = value;
      }
    }

    /// <summary>
    /// Get the channel's major channel number.
    /// </summary>
    public int MajorChannelNumber
    {
      get
      {
        Match m = LOGICAL_CHANNEL_NUMBER_FORMAT.Match(LogicalChannelNumber);
        if (m.Success)
        {
          return int.Parse(m.Groups[1].Captures[0].Value);
        }
        return -1;
      }
    }

    /// <summary>
    /// Get the channel's minor channel number.
    /// </summary>
    public int MinorChannelNumber
    {
      get
      {
        Match m = LOGICAL_CHANNEL_NUMBER_FORMAT.Match(LogicalChannelNumber);
        if (m.Success && m.Groups[3].Captures.Count != 0)
        {
          return int.Parse(m.Groups[3].Captures[0].Value);
        }
        return -1;
      }
    }

    #endregion

    #region IChannel members

    /// <summary>
    /// Get the default logical number associated with the channel.
    /// </summary>
    public override string DefaultLogicalChannelNumber
    {
      get
      {
        // This format is analogous to the SCTE QAM channel number.
        return string.Format("{0}.{1}", PhysicalChannelNumber, ProgramNumber);
      }
    }

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public override bool IsDifferentTransmitter(IChannel channel)
    {
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (
        atscChannel == null ||
        Frequency != atscChannel.Frequency ||
        ModulationScheme != atscChannel.ModulationScheme
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
      ChannelAtsc channel = obj as ChannelAtsc;
      if (
        channel == null ||
        !base.Equals(obj) ||
        Frequency != channel.Frequency ||
        ModulationScheme != channel.ModulationScheme ||
        SourceId != channel.SourceId
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
              ModulationScheme.GetHashCode() ^ SourceId.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("ATSC, {0}, source ID = {1}, frequency = {2} kHz (#{3}), modulation scheme = {4}",
                            base.ToString(), SourceId, Frequency,
                            PhysicalChannelNumber, ModulationScheme);
    }

    #endregion

    /// <summary>
    /// Calculate the physical channel number corresponding with a centre or
    /// analog video carrier frequency in the US FCC broadcast frequency plan.
    /// </summary>
    /// <param name="carrierFrequency">The centre frequency. The unit is kilo-Hertz (kHz).</param>
    /// <returns>the physical channel number corresponding with <paramref name="carrierFrequency">the carrier frequency</paramref>></returns>
    public static short GetPhysicalChannelNumberForFrequency(int carrierFrequency)
    {
      // Between 2 and 69, or zero if invalid.
      if (carrierFrequency >= 470000)
      {
        short channelNumber = (short)(14 + ((carrierFrequency - 470000) / 6000));
        if (channelNumber > 69)
        {
          channelNumber = 0;
        }
        return channelNumber;
      }
      if (carrierFrequency >= 174000)
      {
        return (short)(7 + ((carrierFrequency - 174000) / 6000));
      }
      if (carrierFrequency >= 76000)
      {
        return (short)(5 + ((carrierFrequency - 76000) / 6000));
      }
      if (carrierFrequency >= 54000)
      {
        return (short)(2 + ((carrierFrequency - 54000) / 6000));
      }
      return 0;
    }
  }
}