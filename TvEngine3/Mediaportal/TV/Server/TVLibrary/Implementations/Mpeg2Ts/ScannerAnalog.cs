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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVScanning"/> for WDM analog tuners.
  /// </summary>
  public class ScannerAnalog : ScannerMpeg2TsBase
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="ScannerAnalog"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="analyser">The stream analyser instance to use for scanning.</param>
    public ScannerAnalog(ITVCard tuner, ITsChannelScan analyser)
      : base(tuner, analyser)
    {
    }

    /// <summary>
    /// Scans the specified transponder.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    public override List<IChannel> Scan(IChannel channel, ScanParameters settings)
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
      return base.Scan(channel, settings);
    }

    /// <summary>
    /// Set the name for services which do not supply a name.
    /// </summary>
    /// <param name="channel">The service details.</param>
    protected override void SetMissingServiceName(IChannel channel)
    {
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel != null)
      {
        if (analogChannel.MediaType == MediaTypeEnum.TV)
        {
          analogChannel.Name = string.Format("Analog TV {0}", analogChannel.ChannelNumber);
        }
        else if (analogChannel.MediaType == MediaTypeEnum.Radio)
        {
          analogChannel.Name = string.Format("FM {0}", ((float)analogChannel.Frequency / 1000000).ToString("F1"));
        }
      }
    }
  }
}