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
  /// A base class for <see cref="T:IChannel"/> implementations.
  /// </summary>
  [DataContract]
  [KnownType(typeof(ChannelAnalogTv))]
  [KnownType(typeof(ChannelAtsc))]
  [KnownType(typeof(ChannelCapture))]
  [KnownType(typeof(ChannelDigiCipher2))]
  [KnownType(typeof(ChannelDvbC))]
  [KnownType(typeof(ChannelDvbC2))]
  [KnownType(typeof(ChannelDvbS))]
  [KnownType(typeof(ChannelDvbS2))]
  [KnownType(typeof(ChannelDvbT))]
  [KnownType(typeof(ChannelDvbT2))]
  [KnownType(typeof(ChannelFmRadio))]
  [KnownType(typeof(ChannelSatelliteTurboFec))]
  [KnownType(typeof(ChannelScte))]
  [KnownType(typeof(ChannelStream))]
  public abstract class ChannelBase : IChannel
  {
    #region variables

    [DataMember]
    protected string _name = string.Empty;

    [DataMember]
    protected string _provider = string.Empty;

    /// <remarks>
    /// This property is a string in order to support ATSC and SCTE two-part
    /// channel numbers in the form X.Y (also written as X-Y).
    /// </remarks>
    [DataMember]
    protected string _logicalChannelNumber = string.Empty;

    [DataMember]
    protected MediaType _mediaType = MediaType.Television;

    [DataMember]
    protected bool _isEncrypted = false;

    [DataMember]
    protected bool _isHighDefinition = false;

    [DataMember]
    protected bool _isThreeDimensional = false;

    #endregion

    #region IChannel members

    #region properties

    /// <summary>
    /// Get/set the channel's name.
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        _name = value;
      }
    }

    /// <summary>
    /// Get/set the channel provider's name.
    /// </summary>
    public string Provider
    {
      get
      {
        return _provider;
      }
      set
      {
        _provider = value;
      }
    }

    /// <summary>
    /// Get/set the logical number associated with the channel.
    /// </summary>
    public string LogicalChannelNumber
    {
      get
      {
        return _logicalChannelNumber;
      }
      set
      {
        _logicalChannelNumber = value;
      }
    }

    /// <summary>
    /// Get the default logical number associated with the channel.
    /// </summary>
    public virtual string DefaultLogicalChannelNumber
    {
      get
      {
        // If we were to use zero or empty string, sorting channel groups by
        // number would place the channels with default numbers first. That
        // would be undesirable.
        return "10000";
      }
    }

    /// <summary>
    /// Get/set the channel's media type.
    /// </summary>
    public MediaType MediaType
    {
      get
      {
        return _mediaType;
      }
      set
      {
        _mediaType = value;
      }
    }

    /// <summary>
    /// Get/set whether the channel is encrypted.
    /// </summary>
    public bool IsEncrypted
    {
      get
      {
        return _isEncrypted;
      }
      set
      {
        _isEncrypted = value;
      }
    }

    /// <summary>
    /// Get/set whether the channel's video is high definition (HD).
    /// </summary>
    public bool IsHighDefinition
    {
      get
      {
        return _isHighDefinition;
      }
      set
      {
        _isHighDefinition = value;
      }
    }

    /// <summary>
    /// Get/set whether the channel's video is three dimensional (3D).
    /// </summary>
    public bool IsThreeDimensional
    {
      get
      {
        return _isThreeDimensional;
      }
      set
      {
        _isThreeDimensional = value;
      }
    }

    #endregion

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public abstract bool IsDifferentTransmitter(IChannel channel);

    #endregion

    #region object overrides

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>, otherwise <c>false</c></returns>
    public override bool Equals(object obj)
    {
      ChannelBase channel = obj as ChannelBase;
      if (
        channel == null ||
        !string.Equals(Name, channel.Name) ||
        !string.Equals(Provider, channel.Provider) ||
        !string.Equals(LogicalChannelNumber, channel.LogicalChannelNumber) ||
        MediaType != channel.MediaType ||
        IsEncrypted != channel.IsEncrypted ||
        IsHighDefinition != channel.IsHighDefinition ||
        IsThreeDimensional != channel.IsThreeDimensional
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
      return base.GetHashCode() ^ Name.GetHashCode() ^ Provider.GetHashCode() ^
              LogicalChannelNumber.GetHashCode() ^ MediaType.GetHashCode() ^
              IsEncrypted.GetHashCode() ^ IsHighDefinition.GetHashCode() ^
              IsThreeDimensional.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("name = {0}, provider = {1}, LCN = {2}, media type = {3}, is encrypted = {4}, is HD = {5}, is 3D = {6}",
                            Name, Provider, LogicalChannelNumber, MediaType,
                            IsEncrypted, IsHighDefinition, IsThreeDimensional);
    }

    #endregion

    #region ICloneable member

    /// <summary>
    /// Clone the channel instance.
    /// </summary>
    /// <returns>a shallow clone of the channel instance</returns>
    public object Clone()
    {
      return MemberwiseClone();
    }

    #endregion
  }
}