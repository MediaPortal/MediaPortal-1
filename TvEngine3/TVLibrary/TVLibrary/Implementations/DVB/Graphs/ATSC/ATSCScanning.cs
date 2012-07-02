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
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class which implements TV and radio service scanning for ATSC and annex-C (North American cable)
  /// tuners with BDA drivers.
  /// </summary>
  public class ATSCScanning : DvbBaseScanning
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="ATSCScanning"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    public ATSCScanning(TvCardDvbBase tuner)
      : base(tuner)
    {
      _enableWaitForVct = true;
    }

    /// <summary>
    /// Set the service type for services which do not supply a service type.
    /// </summary>
    /// <param name="serviceType">The service type to check/update.</param>
    /// <param name="hasVideo">Non-zero if the corresponding service has at least one video stream.</param>
    /// <param name="hasAudio">Non-zero if the corresponding service has at least one audio stream.</param>
    /// <returns>the updated service type</returns>
    protected override short SetMissingServiceType(short serviceType, short hasVideo, short hasAudio)
    {
      if (serviceType <= 0)
      {
        if (hasVideo != 0)
        {
          return (short)AtscServiceType.DigitalTelevision;
        }
        else if (hasAudio != 0)
        {
          return (short)AtscServiceType.Audio;
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
      // and a virtual channel number (eg. 21-2). The callsign is a historical thing - if available, it is usually
      // found in the short name field in the VCT. Virtual channel numbers were introduced in the analog (NTSC)
      // switch-off. They don't necessarily have any relationship to the physical channel number (6 MHz frequency
      // slot - in TV Server, ATSCChannel.PhysicalChannel) that the service is transmitted in.

      // If possible we use <major channel>-<minor channel> labelling.
      int majorChannel = atscChannel.MajorChannel;
      int minorChannel = atscChannel.MinorChannel;
      if (atscChannel.MajorChannel > 0)
      {
        Log.Log.Debug("AtscScanning: service name not set, translated with VCT info to {0}", atscChannel.Name);
      }
      else
      {
        // If we don't have the major and minor channel numbers or the name then use the physical channel number or
        // frequency for the major channel substitute.
        Log.Log.Debug("AtscScanning: service name not set, translated with other info to {0}", atscChannel.Name);
        if (atscChannel.PhysicalChannel > 0)
        {
          majorChannel = atscChannel.PhysicalChannel;
        }
        else
        {
          majorChannel = (int)atscChannel.Frequency;
        }

        // For minor channel number, use the SID or PMT PID. It is strange, but broadcasters seem to treat
        // SID/PMT PID 0x10 as service X.1, PID 0x20 as X.2, PID 0x30 as X.3... etc.
        if (atscChannel.PmtPid % 16 == 0)
        {
          minorChannel = atscChannel.PmtPid / 16;
        }
        else if (atscChannel.ServiceId % 16 == 0)
        {
          minorChannel = atscChannel.ServiceId / 16;
        }
        else
        {
          minorChannel = atscChannel.ServiceId;
        }
      }

      atscChannel.Name = majorChannel + "-" + minorChannel;
    }
  }
}