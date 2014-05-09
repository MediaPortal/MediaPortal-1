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

using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  /// <summary>
  /// A class which implements TV and radio service scanning for ATSC and SCTE (ITU-T annex B North
  /// American cable) tuners with BDA drivers.
  /// </summary>
  internal class ScannerMpeg2TsAtsc : ScannerMpeg2TsBase
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="ScannerMpeg2TsAtsc"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="analyser">The stream analyser instance to use for scanning.</param>
    public ScannerMpeg2TsAtsc(ITVCard tuner, ITsChannelScan analyser)
      : base(tuner, analyser)
    {
    }

    /// <summary>
    /// Set the service type for services which do not supply a service type.
    /// </summary>
    /// <param name="serviceType">The service type to check/update.</param>
    /// <param name="videoStreamCount">The number of video streams associated with the service.</param>
    /// <param name="audioStreamCount">The number of audio streams associated with the service.</param>
    /// <returns>the updated service type</returns>
    protected override int SetMissingServiceType(int serviceType, int videoStreamCount, int audioStreamCount)
    {
      if (serviceType <= 0)
      {
        if (videoStreamCount != 0)
        {
          return (int)AtscServiceType.DigitalTelevision;
        }
        else if (audioStreamCount != 0)
        {
          return (int)AtscServiceType.Audio;
        }
      }
      return serviceType;
    }

    /// <summary>
    /// Determine whether a service type is a radio service type.
    /// </summary>
    /// <param name="serviceType">The service type to check.</param>
    /// <returns><c>true</c> if the service type is a radio service type, otherwise <c>false</c></returns>
    protected override bool IsRadioService(int serviceType)
    {
      return serviceType == (int)AtscServiceType.Audio;
    }

    /// <summary>
    /// Determine whether a service type is a television service type.
    /// </summary>
    /// <param name="serviceType">The service type to check.</param>
    /// <returns><c>true</c> if the service type is a television service type, otherwise <c>false</c></returns>
    protected override bool IsTvService(int serviceType)
    {
      return serviceType == (int)AtscServiceType.AnalogTelevision ||
             serviceType == (int)AtscServiceType.DigitalTelevision;
    }

    /// <summary>
    /// Set the name for services which do not supply a name.
    /// </summary>
    /// <param name="channel">The service details.</param>
    protected override void SetMissingServiceName(IChannel channel)
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
}