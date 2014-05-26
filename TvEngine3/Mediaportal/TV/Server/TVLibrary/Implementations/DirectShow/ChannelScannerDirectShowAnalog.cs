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

using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.IChannelScanner"/> for WDM analog tuners.
  /// </summary>
  internal class ChannelScannerDirectShowAnalog : ChannelScannerDirectShowBase
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerDirectShowAnalog"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="analyser">The stream analyser instance to use for scanning.</param>
    public ChannelScannerDirectShowAnalog(ITVCard tuner, ITsChannelScan analyser)
      : base(tuner, new ChannelScannerHelperAnalog(), analyser)
    {
    }

    /// <summary>
    /// Scans the specified transponder.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public override List<IChannel> Scan(IChannel channel)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel != null && analogChannel.VideoSource != CaptureSourceVideo.Tuner)
      {
        TunerAnalog analogTuner = _tuner as TunerAnalog;
        if (analogTuner != null)
        {
          return (List<IChannel>)analogTuner.GetSourceChannels();
        }
        return null;
      }
      return base.Scan(channel);
    }
  }
}