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
using System.Text.RegularExpressions;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog
{
  /// <summary>
  /// An implementation of <see cref="IChannelScanner"/> for analog streams.
  /// </summary>
  /// <remarks>
  /// TVE encodes analog streams as MPEG 2 transport streams with DVB service
  /// information.
  /// </remarks>
  internal class ChannelScannerAnalog : ChannelScannerDvb
  {
    private TunerAnalog _tuner = null;

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerAnalog"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="grabberMpeg">The MPEG 2 transport stream analyser instance to use for scanning.</param>
    /// <param name="grabberDvb">The DVB stream analyser instance to use for scanning.</param>
    public ChannelScannerAnalog(ITuner tuner, IGrabberSiMpeg grabberMpeg, IGrabberSiDvb grabberDvb)
      : base(tuner, grabberMpeg, grabberDvb, null)
    {
      _tuner = tuner as TunerAnalog;
    }

    #endregion

    #region IChannelScanner members

    /// <summary>
    /// Tune to a specified channel and scan for channel information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="isFastNetworkScan"><c>True</c> to do a fast network scan.</param>
    /// <param name="channels">The channel information found.</param>
    /// <param name="groupNames">The names of the groups referenced in <paramref name="channels"/>.</param>
    public override void Scan(IChannel channel, bool isFastNetworkScan, out IList<ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      // External input "scanning".
      if (_tuner != null)
      {
        ChannelCapture captureChannel = channel as ChannelCapture;
        if (captureChannel != null)
        {
          channels = new List<ScannedChannel>(100);
          groupNames = new Dictionary<ChannelGroupType, IDictionary<ulong, string>>(2);
          groupNames.Add(ChannelGroupType.BroadcastStandard, new Dictionary<ulong, string>(1) { { (ulong)BroadcastStandard.ExternalInput, BroadcastStandard.ExternalInput.GetDescription() } });
          IDictionary<ulong, string> providerGroupNames = new Dictionary<ulong, string>(30);
          foreach (IChannel sourceChannel in _tuner.GetSourceChannels())
          {
            ScannedChannel scannedChannel = new ScannedChannel(sourceChannel);
            scannedChannel.IsVisibleInGuide = true;
            scannedChannel.Groups.Add(ChannelGroupType.BroadcastStandard, new List<ulong>(1) { (ulong)BroadcastStandard.ExternalInput });

            ulong hashCode = (ulong)sourceChannel.Provider.GetHashCode();
            scannedChannel.Groups.Add(ChannelGroupType.ChannelProvider, new List<ulong>(1) { hashCode });
            providerGroupNames[hashCode] = sourceChannel.Provider;

            channels.Add(scannedChannel);
          }
          groupNames.Add(ChannelGroupType.ChannelProvider, providerGroupNames);
          return;
        }
      }

      base.Scan(channel, isFastNetworkScan, out channels, out groupNames);
      if (channels != null)
      {
        foreach (ScannedChannel scannedChannel in channels)
        {
          // Names pulled from German teletext often end with "text" (because
          // we are actually getting the teletext service name). Remove the
          // suffix.
          Match m = Regex.Match(scannedChannel.Channel.Name, @"(.*?)\s*text$", RegexOptions.IgnoreCase);
          if (m.Success)
          {
            scannedChannel.Channel.Name = m.Groups[1].Captures[0].Value;
          }
        }
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for transmitter tuning details
    /// within the available network information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the tuning details found</returns>
    public override IList<TuningDetail> ScanNetworkInformation(IChannel channel)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region function overrides

    /// <summary>
    /// Get a name for a channel that would otherwise be nameless.
    /// </summary>
    /// <param name="channel">The nameless channel.</param>
    /// <returns>a name for the channel</returns>
    protected override string GetNameForChannel(IChannel channel)
    {
      if (channel is ChannelAnalogTv)
      {
        return string.Format("Analog TV {0}", channel.LogicalChannelNumber);
      }
      if (channel is ChannelFmRadio)
      {
        return string.Format("FM {0}", channel.LogicalChannelNumber);
      }

      return base.GetNameForChannel(channel);
    }

    #endregion
  }
}