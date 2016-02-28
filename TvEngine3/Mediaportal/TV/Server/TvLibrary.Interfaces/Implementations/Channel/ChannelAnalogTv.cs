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
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel
{
  /// <summary>
  /// An implementation of <see cref="T:IChannel"/> for analog TV channels.
  /// </summary>
  [DataContract]
  public class ChannelAnalogTv : ChannelBase, IChannelPhysical
  {
    #region variables

    [DataMember]
    private short _physicalChannelNumber = -1;

    [DataMember]
    private int _frequency = -1;

    [DataMember]
    private Country _country = CountryCollection.Instance.GetCountryByIsoCode("NL");

    [DataMember]
    private AnalogTunerSource _tunerSource = AnalogTunerSource.Cable;

    #endregion

    #region properties

    /// <summary>
    /// Get/set the number of the physical (broadcast) channel that the channel is transmitted in.
    /// </summary>
    /// <remarks>
    /// This number is converted to a frequency for tuning using per-country mapping tables.
    /// </remarks>
    public short PhysicalChannelNumber
    {
      get
      {
        return _physicalChannelNumber;
      }
      set
      {
        _physicalChannelNumber = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's carrier frequency. The frequency unit is kilo-Hertz (kHz).
    /// </summary>
    /// <remarks>
    /// Used as an override for the default per-country mapping table frequency associated with the physical channel number.
    /// </remarks>
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
    /// Get/set the country that the channel is broadcast in.
    /// </summary>
    /// <remarks>
    /// The country's identifier is used to convert the physical channel number to a frequency (using per-country mapping tables) when tuning analog TV.
    /// </remarks>
    public Country Country
    {
      get
      {
        return _country;
      }
      set
      {
        _country = value;
      }
    }

    /// <summary>
    /// Get/set the channel's tuner source.
    /// </summary>
    public AnalogTunerSource TunerSource
    {
      get
      {
        return _tunerSource;
      }
      set
      {
        _tunerSource = value;
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
        return PhysicalChannelNumber.ToString();
      }
    }

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <param name="checkLogicalChannelNumber"><c>True</c> to check the logical channel number. The logical channel number is only a tuning parameter for blasters.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public bool IsDifferentTransmitter(IChannel channel, bool checkLogicalChannelNumber)
    {
      ChannelAnalogTv analogChannel = channel as ChannelAnalogTv;
      if (
        analogChannel == null ||
        (checkLogicalChannelNumber && LogicalChannelNumber != analogChannel.LogicalChannelNumber) ||
        PhysicalChannelNumber != analogChannel.PhysicalChannelNumber ||
        Frequency != analogChannel.Frequency ||
        (Country == null && analogChannel.Country != null) ||
        (Country != null && analogChannel.Country == null) ||
        (Country != null && analogChannel.Country != null && Country.Id != analogChannel.Country.Id) ||
        TunerSource != analogChannel.TunerSource
      )
      {
        return true;
      }
      return false;
    }

    public override bool IsDifferentTransmitter(IChannel channel)
    {
      return IsDifferentTransmitter(channel, true);
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
      ChannelAnalogTv channel = obj as ChannelAnalogTv;
      if (
        channel == null ||
        !base.Equals(obj) ||
        PhysicalChannelNumber != channel.PhysicalChannelNumber ||
        Frequency != channel.Frequency ||
        (Country == null && channel.Country != null) ||
        (Country != null && channel.Country == null) ||
        (Country != null && channel.Country != null && Country.Id != channel.Country.Id) ||
        TunerSource != channel.TunerSource
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
      return base.GetHashCode() ^ PhysicalChannelNumber.GetHashCode() ^
              Frequency.GetHashCode() ^
              (Country == null ? -1 : Country.Id).GetHashCode() ^
              TunerSource.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("analog, {0}, physical channel number = {1}, frequency = {2} kHz, country = {3}, tuner source = {4}",
                            base.ToString(), PhysicalChannelNumber, Frequency,
                            Country, TunerSource);
    }

    #endregion
  }
}