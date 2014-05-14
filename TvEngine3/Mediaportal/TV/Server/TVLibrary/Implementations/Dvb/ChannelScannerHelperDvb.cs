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
      // Fill in missing names.
      if (string.IsNullOrWhiteSpace(channel.Name))
      {
        DVBIPChannel streamChannel = channel as DVBIPChannel;
        if (streamChannel != null)
        {
          // Streams often don't have meaningful PSI. Just use the URL in those cases.
          streamChannel.Name = streamChannel.Url;
        }
        else
        {
          DVBBaseChannel dvbChannel = channel as DVBBaseChannel;
          if (dvbChannel == null)
          {
            return;
          }
          // Default: use "Unknown <frequency>-<service ID>". At least that way people can often
          // tell which transponder the service came from.
          dvbChannel.Name = "Unknown " + (dvbChannel.Frequency / 1000) + "-" + dvbChannel.ServiceId;
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

      if (
        serviceType == (int)DvbServiceType.DigitalRadio ||
        serviceType == (int)DvbServiceType.FmRadio ||
        serviceType == (int)DvbServiceType.AdvancedCodecDigitalRadio)
      {
        return MediaTypeEnum.Radio;
      }

      if (
        serviceType == (int)DvbServiceType.DigitalTelevision ||
        serviceType == (int)DvbServiceType.Mpeg2HdDigitalTelevision ||
        serviceType == (int)DvbServiceType.AdvancedCodecSdDigitalTelevision ||
        serviceType == (int)DvbServiceType.AdvancedCodecHdDigitalTelevision ||
        serviceType == (int)DvbServiceType.AdvancedCodecFrameCompatiblePlanoStereoscopicHdDigitalTelevision ||
        serviceType == (int)DvbServiceType.SkyGermanyOptionChannel)
      {
        return MediaTypeEnum.TV;
      }

      return null;
    }
  }
}