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

using System.Text.RegularExpressions;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog
{
  /// <summary>
  /// An implementation of <see cref="IChannelScannerHelper"/> for analog channel scan logic.
  /// </summary>
  internal class ChannelScannerHelperAnalog : IChannelScannerHelper
  {
    /// <summary>
    /// Set or modify channel detail.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public virtual void UpdateChannel(ref IChannel channel)
    {
      ChannelAnalogTv analogTvChannel = channel as ChannelAnalogTv;
      ChannelFmRadio fmRadioChannel = channel as ChannelFmRadio;
      if (analogTvChannel == null && fmRadioChannel == null)
      {
        return;
      }

      string logicalChannelNumber = channel.LogicalChannelNumber;
      if (string.IsNullOrWhiteSpace(logicalChannelNumber))
      {
        if (analogTvChannel != null)
        {
          logicalChannelNumber = analogTvChannel.PhysicalChannelNumber.ToString();
        }
        else
        {
          logicalChannelNumber = string.Format("{0:#.#}", (float)fmRadioChannel.Frequency / 1000);
        }
        channel.LogicalChannelNumber = logicalChannelNumber;
      }

      if (string.IsNullOrWhiteSpace(channel.Name))
      {
        if (analogTvChannel != null)
        {
          channel.Name = string.Format("Analog TV {0}", logicalChannelNumber);
        }
        else
        {
          channel.Name = string.Format("FM {0}", logicalChannelNumber);
        }
      }
      else
      {
        // Names pulled from German teletext often end with "text" (because we
        // are actually getting the teletext service name). Remove the suffix.
        Match m = Regex.Match(channel.Name, @"(.*?)\s*text$", RegexOptions.IgnoreCase);
        if (m.Success)
        {
          channel.Name = m.Groups[1].Captures[0].Value;
        }
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
      if (videoStreamCount > 0)
      {
        return MediaType.Television;
      }
      if (audioStreamCount > 0)
      {
        return MediaType.Radio;
      }
      return null;
    }
  }
}