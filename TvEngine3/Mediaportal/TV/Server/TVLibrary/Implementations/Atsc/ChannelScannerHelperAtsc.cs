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

using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Atsc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Atsc
{
  /// <summary>
  /// An implementation of <see cref="IChannelScannerHelper"/> for ATSC channel scan logic.
  /// </summary>
  internal class ChannelScannerHelperAtsc : IChannelScannerHelper
  {
    /// <summary>
    /// Set or modify channel detail.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public virtual void UpdateChannel(ref IChannel channel)
    {
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      ChannelScte scteChannel = channel as ChannelScte;
      if (atscChannel == null && scteChannel == null)
      {
        return;
      }

      if (string.IsNullOrWhiteSpace(channel.LogicalChannelNumber))
      {
        if (atscChannel != null)
        {
          channel.LogicalChannelNumber = string.Format("{0}.{1}", atscChannel.PhysicalChannelNumber, atscChannel.ProgramNumber);
        }
        else
        {
          channel.LogicalChannelNumber = string.Format("{0}.{1}", scteChannel.PhysicalChannelNumber, scteChannel.ProgramNumber);
        }
      }

      if (string.IsNullOrWhiteSpace(channel.Name))
      {
        channel.Name = string.Format("Unknown {0}", channel.LogicalChannelNumber);
      }
    }

    /// <summary>
    /// Get the correct media type for a channel.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="videoStreamCount">The number of video streams associated with the service.</param>
    /// <param name="audioStreamCount">The number of audio streams associated with the service.</param>
    public virtual MediaType? GetMediaType(int serviceType, int videoStreamCount, int audioStreamCount)
    {
      if (serviceType <= 0)
      {
        if (videoStreamCount != 0)
        {
          return MediaType.Television;
        }
        else if (audioStreamCount != 0)
        {
          return MediaType.Radio;
        }
        return null;
      }

      if (serviceType == (int)ServiceType.Audio)
      {
        return MediaType.Radio;
      }

      if (
        serviceType == (int)ServiceType.AnalogTelevision ||
        serviceType == (int)ServiceType.DigitalTelevision)
      {
        return MediaType.Television;
      }

      return null;
    }
  }
}