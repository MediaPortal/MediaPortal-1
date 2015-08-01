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

    // key = device path
    private IDictionary<string, ITuner> _knownTuners = new Dictionary<string, ITuner>();

    #endregion

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
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITuner> DetectTuners()
    {
      this.LogDebug("WDM detector: detect tuners");
      List<ITuner> tuners = new List<ITuner>();
      IDictionary<string, ITuner> knownTuners = new Dictionary<string, ITuner>();
      HashSet<string> crossbarProductIds = new HashSet<string>();
      string productInstanceId;
      string tunerInstanceId;

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        // Is this a new device?
        ITuner tuner;
        if (_knownTuners.TryGetValue(devicePath, out tuner))
        {
          device.Dispose();
          tuners.Add(tuner);
          knownTuners.Add(devicePath, tuner);
          if (tuner.ProductInstanceId != null)
          {
            crossbarProductIds.Add(tuner.ProductInstanceId);
          }
          continue;
        }

        this.LogDebug("WDM detector: crossbar, name = {0}, device path = {1}", name, devicePath);

        // We have to do some graph work to figure out identifiers and
        // supported broadcast standards.
        BroadcastStandard supportedBroadcastStandards;
        GetCrossbarInfo(device, out productInstanceId, out tunerInstanceId, out supportedBroadcastStandards);

        tuner = new TunerAnalog(device, FilterCategory.AMKSCrossbar, tunerInstanceId, productInstanceId, supportedBroadcastStandards);
        tuners.Add(tuner);
        knownTuners.Add(devicePath, tuner);
        if (tuner.ProductInstanceId != null)
        {
          crossbarProductIds.Add(tuner.ProductInstanceId);
        }
      }

      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath) || CAPTURE_DEVICE_BLACKLIST.Contains(name))
        {
          device.Dispose();
          continue;
        }

        // Is this a new device?
        ITuner tuner;
        if (_knownTuners.TryGetValue(devicePath, out tuner))
        {
          device.Dispose();
          if (tuner != null)
          {
            tuners.Add(tuner);
          }
          knownTuners.Add(devicePath, tuner);
          continue;
        }

        // We don't want to add duplicate entries for multi-input capture
        // devices (already detected via crossbar).
        this.LogDebug("WDM detector: capture, name = {0}, device path = {1}", name, devicePath);
        GetIdentifiersForDevice(device, out productInstanceId, out tunerInstanceId);
        if (productInstanceId != null && crossbarProductIds.Contains(productInstanceId))
        {
          // This source has a crossbar, so don't create a capture tuner too.
          this.LogDebug("  already detected crossbar");
          device.Dispose();
          knownTuners.Add(devicePath, null);
          continue;
        }

        tuner = new TunerAnalog(device, FilterCategory.AMKSCapture, tunerInstanceId, productInstanceId, BroadcastStandard.ExternalInput);
        tuners.Add(tuner);
        knownTuners.Add(devicePath, tuner);
      }

      _knownTuners = knownTuners;
      return tuners;
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
        CaptureSourceVideo supportedVideoSources = crossbar.SupportedVideoSources;
        CaptureSourceAudio supportedAudioSources = crossbar.SupportedAudioSources;
        if (
          (supportedVideoSources != CaptureSourceVideo.None && supportedVideoSources != CaptureSourceVideo.Tuner) ||
          (supportedAudioSources != CaptureSourceAudio.None && supportedAudioSources != CaptureSourceAudio.Tuner)
        )
        {
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