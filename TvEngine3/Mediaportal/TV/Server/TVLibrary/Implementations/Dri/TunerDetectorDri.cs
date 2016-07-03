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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.SSDP;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorUpnp"/> which detects DRI tuners.
  /// </summary>
  internal class TunerDetectorDri : ITunerDetectorUpnp
  {
    private static readonly HashSet<TunerModulation> MANDATORY_MODULATION_SCHEMES = new HashSet<TunerModulation> { TunerModulation.Qam64, TunerModulation.Qam256 };

    #region ITunerDetectorUpnp members

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "DRI";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a UPnP device.
    /// </summary>
    /// <param name="descriptor">The UPnP device's root descriptor.</param>
    /// <param name="controlPoint">The control point that the device is attached to.</param>
    /// <returns>the compatible tuners exposed by the device</returns>
    public ICollection<ITuner> DetectTuners(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
    {
      List<ITuner> tuners = new List<ITuner>();

      IEnumerator<DeviceEntry> childDeviceEn = descriptor.RootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      while (childDeviceEn.MoveNext())
      {
        // Have to check for the tuner service to avoid Ceton tuning adaptor devices.
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            string uuid = childDeviceEn.Current.UUID;
            DeviceDescriptor deviceDescriptor = descriptor.FindDevice(uuid);

            ICollection<TunerModulation> supportedModulationSchemes;
            GetDriDeviceSupportedModulationSchemes(deviceDescriptor, controlPoint, out supportedModulationSchemes);

            BroadcastStandard supportedBroadcastStandards = BroadcastStandard.Unknown;
            if (supportedModulationSchemes.Contains(TunerModulation.Vsb8))
            {
              supportedBroadcastStandards |= BroadcastStandard.Atsc;
            }
            if (supportedModulationSchemes.Contains(TunerModulation.Qam64) || supportedModulationSchemes.Contains(TunerModulation.Qam256))
            {
              supportedBroadcastStandards |= BroadcastStandard.Scte;
            }

            if (supportedBroadcastStandards == BroadcastStandard.Unknown)
            {
              this.LogWarn("DRI detector: tuner {0} does not support any supported modulation schemes", uuid);
              break;
            }

            string tunerInstanceId = null;
            string productInstanceId = null;
            Match m = null;
            if (deviceDescriptor.FriendlyName.StartsWith("ATI"))
            {
              // Example: ATI TV Wonder OpenCable Receiver (37F0), Unit #1
              m = Regex.Match(descriptor.FriendlyName, @"\(([^\s]+)\),\sUnit\s\#(\d+)$", RegexOptions.IgnoreCase);
            }
            else if (deviceDescriptor.FriendlyName.StartsWith("Ceton"))
            {
              // Example: Ceton InfiniTV PCIe (00-80-75-05) Tuner 1 (00-00-22-00-00-80-75-05)
              m = Regex.Match(descriptor.FriendlyName, @"\s+\(([^\s]+)\)\s+Tuner\s+(\d+)", RegexOptions.IgnoreCase);
            }
            else
            {
              // Examples:
              // HDHomeRun Prime Tuner 1316890F-1
              // Hauppauge OpenCable Receiver 201200AA-1
              m = Regex.Match(descriptor.FriendlyName, @"\s+([^\s]+)-(\d)$", RegexOptions.IgnoreCase);
            }
            if (m != null && m.Success)
            {
              productInstanceId = m.Groups[1].Captures[0].Value;
              tunerInstanceId = m.Groups[2].Captures[0].Value;
            }

            tuners.Add(new TunerDri(deviceDescriptor, tunerInstanceId, productInstanceId, supportedBroadcastStandards, supportedModulationSchemes, controlPoint, new TunerStream(string.Format("MediaPortal DRI {0} Stream Source", uuid), 1)));
            break;
          }
        }
      }
      if (tuners.Count > 0)
      {
        this.LogInfo("DRI detector: tuner added");
      }
      return tuners;
    }

    #endregion

    private static void GetDriDeviceSupportedModulationSchemes(DeviceDescriptor deviceDescriptor, UPnPControlPoint controlPoint, out ICollection<TunerModulation> supportedModulationSchemes)
    {
      supportedModulationSchemes = new HashSet<TunerModulation>();

      try
      {
        DeviceConnection connection = controlPoint.Connect(deviceDescriptor.RootDescriptor, deviceDescriptor.DeviceUUID, null, true);
        try
        {
          int connectionId = 0;
          ServiceTuner serviceTuner = new ServiceTuner(connection.Device);
          ServiceConnectionManager serviceConnectionManager = new ServiceConnectionManager(connection.Device);
          try
          {
            int avTransportId;
            int rcsId;
            serviceConnectionManager.PrepareForConnection(string.Empty, string.Empty, -1, ConnectionDirection.Output, out connectionId, out avTransportId, out rcsId);

            string csvTunerModulations = (string)serviceTuner.QueryStateVariable("ModulationList");
            if (string.IsNullOrEmpty(csvTunerModulations))
            {
              supportedModulationSchemes = new HashSet<TunerModulation>(MANDATORY_MODULATION_SCHEMES);
            }
            else
            {
              foreach (string modulation in csvTunerModulations.Split(','))
              {
                TunerModulation tm = (TunerModulation)modulation.Trim();
                if (tm == null)
                {
                  Log.Warn("DRI detector: tuner supports unrecognised modulation scheme {0}", modulation);
                }
                else if (tm == TunerModulation.Qam64 || tm == TunerModulation.Qam64_2)
                {
                  supportedModulationSchemes.Add(TunerModulation.Qam64);
                }
                else if (tm == TunerModulation.Qam256 || tm == TunerModulation.Qam256_2)
                {
                  supportedModulationSchemes.Add(TunerModulation.Qam256);
                }
                else if (tm == TunerModulation.Vsb8 || tm == TunerModulation.Vsb8_2)
                {
                  supportedModulationSchemes.Add(TunerModulation.Vsb8);
                }
                else if (tm == TunerModulation.All)
                {
                  supportedModulationSchemes.Add(TunerModulation.Qam64);
                  supportedModulationSchemes.Add(TunerModulation.Qam256);
                  supportedModulationSchemes.Add(TunerModulation.Vsb8);
                }
                else
                {
                  Log.Warn("DRI detector: tuner supports unsupported modulation scheme {0}", tm);
                }
              }
            }
          }
          finally
          {
            serviceTuner.Dispose();
            if (connectionId > 0)
            {
              serviceConnectionManager.ConnectionComplete(connectionId);
            }
            serviceConnectionManager.Dispose();
          }
        }
        finally
        {
          connection.Disconnect();
          connection.Dispose();
        }
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "DRI detector: failed to determine supported modulation schemes for tuner {0}, assuming only mandatory schemes supported", deviceDescriptor.DeviceUUID);
        supportedModulationSchemes = new HashSet<TunerModulation>(MANDATORY_MODULATION_SCHEMES);
      }
    }
  }
}