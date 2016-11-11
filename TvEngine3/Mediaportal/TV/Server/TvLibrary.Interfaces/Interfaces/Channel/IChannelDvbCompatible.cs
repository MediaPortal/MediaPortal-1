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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Channel
{
  /// <summary>
  /// Interface which describes a DVB-compatible (digital video broadcast)
  /// channel.
  /// </summary>
  public interface IChannelDvbCompatible : IChannelMpeg2Ts
  {
    /// <summary>
    /// Get/set the channel's DVB original network identifier.
    /// </summary>
    int OriginalNetworkId
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the channel's DVB service identifier.
    /// </summary>
    /// <remarks>
    /// This is 100% equivalent to the MPEG 2 transport stream program number.
    /// </remarks>
    int ServiceId
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the DVB original network identifier of the service that the channel's electronic programme guide data is sourced from.
    /// </summary>
    int EpgOriginalNetworkId
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the DVB transport stream identifier of the service that the channel's electronic programme guide data is sourced from.
    /// </summary>
    int EpgTransportStreamId
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the DVB service identifier of the service that the channel's electronic programme guide data is sourced from.
    /// </summary>
    int EpgServiceId
    {
      get;
      set;
    }
  }
}