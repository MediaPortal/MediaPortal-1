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
  /// A base class for Digital Video Broadcast <see cref="T:IChannel"/> implementations.
  /// </summary>
  [DataContract]
  [KnownType(typeof(ChannelDvbC))]
  [KnownType(typeof(ChannelDvbC2))]
  [KnownType(typeof(ChannelDvbDsng))]
  [KnownType(typeof(ChannelDvbS))]
  [KnownType(typeof(ChannelDvbS2))]
  [KnownType(typeof(ChannelDvbT))]
  [KnownType(typeof(ChannelDvbT2))]
  [KnownType(typeof(ChannelIsdbC))]
  [KnownType(typeof(ChannelIsdbS))]
  [KnownType(typeof(ChannelIsdbT))]
  [KnownType(typeof(ChannelSatelliteTurboFec))]
  [KnownType(typeof(ChannelStream))]
  public abstract class ChannelDvbBase : ChannelMpeg2TsBase, IChannelDvbCompatible
  {
    #region variables

    [DataMember]
    protected int _originalNetworkId = -1;

    [DataMember]
    protected int _epgOriginalNetworkId = -1;

    [DataMember]
    protected int _epgTransportStreamId = -1;

    [DataMember]
    protected int _epgServiceId = -1;

    #endregion

    #region properties

    /// <summary>
    /// Get/set the channel's DVB original network identifier.
    /// </summary>
    public int OriginalNetworkId
    {
      get
      {
        return _originalNetworkId;
      }
      set
      {
        _originalNetworkId = value;
      }
    }

    /// <summary>
    /// Get/set the channel's DVB service identifier.
    /// </summary>
    /// <remarks>
    /// This is 100% equivalent to the MPEG 2 transport stream program number.
    /// </remarks>
    public int ServiceId
    {
      get
      {
        return ProgramNumber;
      }
      set
      {
        ProgramNumber = value;
      }
    }

    /// <summary>
    /// Get/set the DVB original network identifier of the service that the channel's electronic programme guide data is sourced from.
    /// </summary>
    public int EpgOriginalNetworkId
    {
      get
      {
        return _epgOriginalNetworkId;
      }
      set
      {
        _epgOriginalNetworkId = value;
      }
    }

    /// <summary>
    /// Get/set the DVB transport stream identifier of the service that the channel's electronic programme guide data is sourced from.
    /// </summary>
    public int EpgTransportStreamId
    {
      get
      {
        return _epgTransportStreamId;
      }
      set
      {
        _epgTransportStreamId = value;
      }
    }

    /// <summary>
    /// Get/set the DVB service identifier of the service that the channel's electronic programme guide data is sourced from.
    /// </summary>
    public int EpgServiceId
    {
      get
      {
        return _epgServiceId;
      }
      set
      {
        _epgServiceId = value;
      }
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
      ChannelDvbBase channel = obj as ChannelDvbBase;
      if (
        channel == null ||
        !base.Equals(obj) ||
        OriginalNetworkId != channel.OriginalNetworkId ||
        EpgOriginalNetworkId != channel.EpgOriginalNetworkId ||
        EpgTransportStreamId != channel.EpgTransportStreamId ||
        EpgServiceId != channel.EpgServiceId
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
      return base.GetHashCode() ^ OriginalNetworkId.GetHashCode() ^
              EpgOriginalNetworkId.GetHashCode() ^
              EpgTransportStreamId.GetHashCode() ^ EpgServiceId.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("{0}, ONID = {1}, EPG ONID = {2}, EPG TSID = {3}, EPG service ID = {4}",
                            base.ToString().Replace("program number", "service ID"),
                            OriginalNetworkId, EpgOriginalNetworkId,
                            EpgTransportStreamId, EpgServiceId);
    }

    #endregion
  }
}