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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb
{
  /// <summary>
  /// An implementation of <see cref="IChannelScannerHelper"/> for DVB channel scan logic.
  /// </summary>
  internal class ChannelScannerHelperDvb : IChannelScannerHelper
  {
    /// <summary>
    /// Set or modify channel detail.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public virtual void UpdateChannel(ref IChannel channel)
    {
      if (string.IsNullOrWhiteSpace(channel.Name))
      {
        ChannelStream streamChannel = channel as ChannelStream;
        if (streamChannel != null)
        {
          // Streams often don't have meaningful PSI. Just use the URL in those
          // cases.
          streamChannel.Name = streamChannel.Url;
        }
        else
        {
          // Try to use "Unknown <frequency>.<service ID>". At least that way
          // people can often tell which transmitter the service came from.
          ChannelMpeg2Base mpeg2Channel = channel as ChannelMpeg2Base;
          IChannelPhysical physicalChannel = channel as IChannelPhysical;
          if (mpeg2Channel != null && physicalChannel != null)
          {
            channel.Name = string.Format("Unknown {0}.{1}", (int)(physicalChannel.Frequency / 1000), mpeg2Channel.ProgramNumber);
          }
          else if (mpeg2Channel != null)
          {
            channel.Name = string.Format("Unknown {0}", mpeg2Channel.ProgramNumber);
          }
          else if (physicalChannel != null)
          {
            channel.Name = string.Format("Unknown {0}", (int)(physicalChannel.Frequency / 1000));
          }
          else
          {
            channel.Name = "Unknown Non MPEG 2";
          }
        }
      }

      // Logical channel number not available.
      // Assumption: such channels are not mainstream/popular.
      // If we use default value zero or empty string, sorting by
      // channel number will list the less popular channels first,
      // which is not what we want. Setting default channel number as
      // below ensures these channels are listed last.
      if (string.IsNullOrWhiteSpace(channel.LogicalChannelNumber))
      {
        channel.LogicalChannelNumber = "10000";
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

      if (
        serviceType == (int)ServiceType.DigitalRadio ||
        serviceType == (int)ServiceType.FmRadio ||
        serviceType == (int)ServiceType.AdvancedCodecDigitalRadio)
      {
        return MediaType.Radio;
      }

      if (
        serviceType == (int)ServiceType.DigitalTelevision ||
        serviceType == (int)ServiceType.NvodTimeShifted ||
        serviceType == (int)ServiceType.Mpeg2HdDigitalTelevision ||
        serviceType == (int)ServiceType.AdvancedCodecSdDigitalTelevision ||
        serviceType == (int)ServiceType.AdvancedCodecSdNvodTimeShifted ||
        serviceType == (int)ServiceType.AdvancedCodecHdDigitalTelevision ||
        serviceType == (int)ServiceType.AdvancedCodecHdNvodTimeShifted ||
        serviceType == (int)ServiceType.AdvancedCodecFrameCompatiblePlanoStereoscopicHdDigitalTelevision ||
        serviceType == (int)ServiceType.AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodTimeShifted ||
        serviceType == (int)ServiceType.SkyGermanyOptionChannel)
      {
        return MediaType.Television;
      }

      return null;
    }
  }
}