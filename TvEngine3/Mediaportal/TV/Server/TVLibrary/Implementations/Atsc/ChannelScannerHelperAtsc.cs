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

using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

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
      // Fill in missing names.
      if (string.IsNullOrWhiteSpace(channel.Name))
      {
        ATSCChannel atscChannel = channel as ATSCChannel;
        if (atscChannel == null)
        {
          return;
        }

        // North America has strange service naming conventions. Services have a callsign (eg. WXYZ) and/or name,
        // and a virtual channel number (eg. 21.2). The callsign is a historical thing - if available, it is usually
        // found in the short name field in the VCT. Virtual channel numbers were introduced in the analog (NTSC)
        // switch-off. They don't necessarily have any relationship to the physical channel number (6 MHz frequency
        // slot - in TVE, ATSCChannel.PhysicalChannel) that the service is transmitted in.
        if (atscChannel.MinorChannel >= 0)
        {
          atscChannel.Name = "Unknown " + atscChannel.MajorChannel + "-" + atscChannel.MinorChannel;
        }
        else
        {
          atscChannel.Name = "Unknown " + atscChannel.MajorChannel;
        }
      }
    }

    /// <summary>
    /// Get the correct media type for a channel.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="videoStreamCount">The number of video streams associated with the service.</param>
    /// <param name="audioStreamCount">The number of audio streams associated with the service.</param>
    public virtual MediaTypeEnum? GetMediaType(int serviceType, int videoStreamCount, int audioStreamCount)
    {
      if (serviceType <= 0)
      {
        if (videoStreamCount != 0)
        {
          return MediaTypeEnum.TV;
        }
        else if (audioStreamCount != 0)
        {
          return MediaTypeEnum.Radio;
        }
        return null;
      }

      if (serviceType == (int)AtscServiceType.Audio)
      {
        return MediaTypeEnum.Radio;
      }

      if (
        serviceType == (int)AtscServiceType.AnalogTelevision ||
        serviceType == (int)AtscServiceType.DigitalTelevision)
      {
        return MediaTypeEnum.TV;
      }

      return null;
    }
  }
}