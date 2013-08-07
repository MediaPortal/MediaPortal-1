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

using System;
using System.Collections.Generic;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for DVB-T BDA cards
  /// </summary>
  public class AnalogScanning : DvbBaseScanning
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalogScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public AnalogScanning(ITVCard card, ITsChannelScan analyzer)
      : base(card, analyzer)
    {
    }

    protected override void SetNameForUnknownChannel(IChannel channel, ChannelInfo info)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel != null)
      {
        if (analogChannel.IsTv)
        {
          info.service_name = String.Format("Analog TV {0}", analogChannel.ChannelNumber);
        }
        else
        {
          info.service_name = String.Format("FM {0}", ((float)analogChannel.Frequency / 1000000).ToString("F1"));
        }
      }
    }

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="channel">The high level tuning detail.</param>
    /// <param name="info">The subchannel detail.</param>
    /// <returns>The new channel.</returns>
    protected override IChannel CreateNewChannel(IChannel channel, ChannelInfo info)
    {
      AnalogChannel tuningChannel = (AnalogChannel)channel;
      AnalogChannel analogChannel = new AnalogChannel();
      analogChannel.Name = info.service_name;
      analogChannel.ChannelNumber = tuningChannel.ChannelNumber;
      analogChannel.Frequency = tuningChannel.Frequency;
      analogChannel.TunerSource = tuningChannel.TunerSource;
      analogChannel.VideoSource = tuningChannel.VideoSource;
      analogChannel.AudioSource = tuningChannel.AudioSource;
      analogChannel.Country = tuningChannel.Country;
      analogChannel.IsVCRSignal = tuningChannel.IsVCRSignal;
      analogChannel.IsTv = IsTvService(info.serviceType);
      analogChannel.IsRadio = IsRadioService(info.serviceType);
      Log.Log.Write("Found: {0}", analogChannel);
      return analogChannel;
    }
  }
}