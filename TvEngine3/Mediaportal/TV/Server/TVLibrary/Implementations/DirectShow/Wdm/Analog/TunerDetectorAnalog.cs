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
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects analog tuners and
  /// capture devices with WDM/DirectShow drivers.
  /// </summary>
  internal class TunerDetectorAnalog : ITunerDetectorSystem
  {
    #region constants

    private static readonly HashSet<string> CAPTURE_DEVICE_BLACKLIST = new HashSet<string>
    {
      // Hauppauge WinTV CI and TerraTec Cinergy CI USB
      // Attempting to load these as capture sources causes a BSOD.
      "WinTVCIUSBBDA Source",
      "WinTVCIUSB",
      "Cinergy CI USB Capture",
      "US2CIBDA"
    };

    #endregion

    #region variables

    private IDictionary<string, ITuner> _knownTuners = new Dictionary<string, ITuner>();    // key = device path
    private HashSet<string> _crossbarProductIds = new HashSet<string>();

    #endregion

    #region ITunerDetectorSystem members

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "WDM";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a system device
    /// interface.
    /// </summary>
    /// <param name="classGuid">The identifier for the interface's class.</param>
    /// <param name="devicePath">The interface's device path.</param>
    /// <returns>the compatible tuners exposed by the interface</returns>
    public ICollection<ITuner> DetectTuners(Guid classGuid, string devicePath)
    {
      ICollection<ITuner> tuners = new List<ITuner>(1);

      // Is the interface a WDM crossbar or capture interface?
      if ((classGuid != FilterCategory.AMKSCapture && classGuid != FilterCategory.AMKSCrossbar) || string.IsNullOrEmpty(devicePath))
      {
        return tuners;
      }

      // If the interface is a capture interface, delay for long enough to
      // ensure any associated crossbar is detected first.
      if (classGuid != FilterCategory.AMKSCapture)
      {
        System.Threading.Thread.Sleep(2000);
      }

      // Detect the tuners associated with the device interface.
      DsDevice device = DsDevice.FromDevicePath(devicePath);
      if (device == null)
      {
        return tuners;
      }
      ITuner tuner = DetectTunerForDevice(device, classGuid, _crossbarProductIds);
      if (tuners == null || tuners.Count == 0)
      {
        device.Dispose();
        return tuners;
      }

      this.LogInfo("WDM detector: tuner added");
      _knownTuners[devicePath] = tuner;
      return tuners;
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITuner> DetectTuners()
    {
      this.LogDebug("WDM detector: detect tuners");
      List<ITuner> tuners = new List<ITuner>();
      IDictionary<string, ITuner> knownTuners = new Dictionary<string, ITuner>();
      HashSet<string> crossbarProductIds = new HashSet<string>();

      Guid[] categories = new Guid[2] { FilterCategory.AMKSCrossbar, FilterCategory.AMKSCapture };  // order important
      foreach (Guid category in categories)
      {
        DsDevice[] devices = DsDevice.GetDevicesOfCat(category);
        foreach (DsDevice device in devices)
        {
          // Is this a new device?
          string devicePath = device.DevicePath;
          ITuner tuner = null;
          if (!string.IsNullOrEmpty(devicePath) && _knownTuners.TryGetValue(devicePath, out tuner))
          {
            // No. Reuse the tuner instance we've previously created.
            device.Dispose();
            if (tuner != null)
            {
              tuners.Add(tuner);
            }
            knownTuners.Add(devicePath, tuner);

            if (category == FilterCategory.AMKSCrossbar && tuner.ProductInstanceId != null)
            {
              crossbarProductIds.Add(tuner.ProductInstanceId);
            }
            continue;
          }

          tuner = DetectTunerForDevice(device, category, crossbarProductIds);
          knownTuners.Add(devicePath, tuner);
          if (tuner != null)
          {
            tuners.Add(tuner);
          }
          else
          {
            device.Dispose();
          }
        }
      }

      _knownTuners = knownTuners;
      _crossbarProductIds = crossbarProductIds;
      return tuners;
    }

    #endregion

    private static ITuner DetectTunerForDevice(DsDevice device, Guid category, HashSet<string> crossbarProductIds)
    {
      string name = device.Name;
      string devicePath = device.DevicePath;
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath) || (category == FilterCategory.AMKSCapture && CAPTURE_DEVICE_BLACKLIST.Contains(name)))
      {
        return null;
      }

      string productInstanceId;
      string tunerInstanceId;
      BroadcastStandard supportedBroadcastStandards;
      if (category == FilterCategory.AMKSCrossbar)
      {
        // We have to do some graph work to figure out identifiers and
        // supported broadcast standards.
        Log.Debug("WDM detector: crossbar, name = {0}, device path = {1}", name, devicePath);
        GetCrossbarInfo(device, out productInstanceId, out tunerInstanceId, out supportedBroadcastStandards);
        if (productInstanceId != null)
        {
          crossbarProductIds.Add(productInstanceId);
        }
      }
      else
      {
        // We don't want to add duplicate entries for multi-input capture
        // devices (already detected via crossbar).
        Log.Debug("WDM detector: capture, name = {0}, device path = {1}", name, devicePath);
        GetIdentifiersForDevice(device, out productInstanceId, out tunerInstanceId);
        if (productInstanceId != null && crossbarProductIds.Contains(productInstanceId))
        {
          // This source has a crossbar, so don't create a capture tuner too.
          Log.Debug("  already detected crossbar");
          return null;
        }
        supportedBroadcastStandards = BroadcastStandard.ExternalInput;
      }

      return new TunerAnalog(device, category, tunerInstanceId, productInstanceId, supportedBroadcastStandards);
    }

    private static void GetCrossbarInfo(DsDevice device, out string productInstanceId, out string tunerInstanceId, out BroadcastStandard supportedBroadcastStandards)
    {
      productInstanceId = null;
      tunerInstanceId = null;
      supportedBroadcastStandards = BroadcastStandard.Unknown;

      IFilterGraph2 graph = (IFilterGraph2)new FilterGraph();
      Crossbar crossbar = new Crossbar(device);
      try
      {
        crossbar.PerformLoading(graph);
        if (
          crossbar.SupportedVideoSources != CaptureSourceVideo.None ||
          crossbar.SupportedAudioSources != CaptureSourceAudio.None
        )
        {
          // Tuner inputs count as external inputs as well because an STB can
          // be connected via RF/coax.
          supportedBroadcastStandards |= BroadcastStandard.ExternalInput;
        }

        // In my experience the tuner instance ID is only stored with the tuner
        // component moniker... but try the crossbar if there is no tuner.
        if (crossbar.PinIndexInputTunerVideo < 0 && crossbar.PinIndexInputTunerAudio < 0)
        {
          GetIdentifiersForDevice(device, out productInstanceId, out tunerInstanceId);
          return;
        }

        Tuner t = new Tuner();
        try
        {
          t.PerformLoading(graph, device.ProductInstanceIdentifier, crossbar);
          AMTunerModeType supportedModes = t.SupportedTuningModes;
          if (supportedModes.HasFlag(AMTunerModeType.TV))
          {
            supportedBroadcastStandards |= BroadcastStandard.AnalogTelevision;
          }
          if (supportedModes.HasFlag(AMTunerModeType.AMRadio))
          {
            supportedBroadcastStandards |= BroadcastStandard.AmRadio;
          }
          if (supportedModes.HasFlag(AMTunerModeType.FMRadio))
          {
            supportedBroadcastStandards |= BroadcastStandard.FmRadio;
          }

          DsDevice tunerDevice = t.Device;
          if ((supportedModes & ~(AMTunerModeType.TV | AMTunerModeType.AMRadio | AMTunerModeType.FMRadio)) != 0)
          {
            Log.Warn("WDM detector: tuner supports unsupported modes, name = {0}, device path = {1}, modes = [{2}]", tunerDevice.Name, tunerDevice.DevicePath, supportedModes);
          }
          GetIdentifiersForDevice(tunerDevice, out productInstanceId, out tunerInstanceId);
        }
        finally
        {
          t.PerformUnloading(graph);
        }
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "WDM detector: failed to get crossbar information, name = {0}, device path = {1}", device.Name, device.DevicePath);
      }
      finally
      {
        crossbar.PerformUnloading(graph);
        Release.ComObject("WDM detector information graph", ref graph);
      }
    }

    private static void GetIdentifiersForDevice(DsDevice device, out string productInstanceId, out string tunerInstanceId)
    {
      productInstanceId = device.ProductInstanceIdentifier;
      if (device.TunerInstanceIdentifier >= 0)
      {
        tunerInstanceId = device.TunerInstanceIdentifier.ToString();
      }
      else
      {
        tunerInstanceId = null;
      }
    }
  }
}