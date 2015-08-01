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
using System.ComponentModel;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Rtl283x
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects the supported
  /// proprietary modes for BDA tuners based on Realtek's RTL283x chipset.
  /// </summary>
  internal class TunerDetectorRtl283x : ITunerDetectorSystem
  {
    #region enums

    private enum BdaExtensionFilterControlProperty
    {
      Frequency = 2,
      Bandwidth = 3,
      Modulation = 10,
      SymbolRate = 11
    }

    private enum BdaExtensionFilterModeProperty
    {
      GetDemodSupportedModes = 2,
      SetDemodMode = 3
    }

    private enum BdaExtensionDeviceControlProperty
    {
      EnablePid = 2,
      DisablePid = 3,
      PidFilterStatus = 5
    }

    [Flags]
    private enum Rtl283xBroadcastStandard
    {
      None = 0,
      [Description("DVB-T")]
      Dvbt = 0x0001,
      [Description("DTMB")]
      Dtmb = 0x0002,
      [Description("FM")]
      Fm = 0x0004,
      [Description("DAB/ISDB-T 1 Seg")]
      DabIsdbt1seg = 0x0008,
      [Description("DVB-H")]
      Dvbh = 0x0010,
      [Description("DVB-C")]
      Dvbc = 0x0020,
      [Description("ATSC")]
      Atsc = 0x0040,
      [Description("ATSC-MH")]
      AtscMh = 0x0080,
      [Description("ISDB-T Full Seg")]
      IsdbtFseg = 0x0100,
      [Description("ISDB-T SB")]
      IsdbtSb = 0x0200,
      [Description("DVB-S")]
      Dvbs = 0x0400,
      [Description("CMMB")]
      Cmmb = 0x0800
    }

    private enum Rtl283xDemodulatorMode
    {
      [Description("DVB-T")]
      Dvbt = 0,
      [Description("DTMB")]
      Dtmb,
      [Description("DVB-C")]
      Dvbc,
      [Description("ATSC")]
      Atsc,
      [Description("Clear QAM")]
      ClearQam
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_FILTER_CONTROL_PROPERTY_SET = new Guid(0xf25c6bc9, 0xdbd7, 0x4a0c, 0xb2, 0x28, 0x9a, 0x58, 0x78, 0x9f, 0x2e, 0x1d);
    private static readonly Guid BDA_EXTENSION_FILTER_MODE_PROPERTY_SET = new Guid(0xbb992b31, 0x931d, 0x41b1, 0x85, 0xea, 0xa0, 0x1b, 0x4e, 0x30, 0x6c, 0xa5);
    private static readonly Guid BDA_EXTENSION_DEVICE_CONTROL_PROPERTY_SET = new Guid(0x1bfb70f7, 0xadfb, 0x4414, 0x9f, 0xd4, 0x60, 0xe9, 0xe5, 0x40, 0xa5, 0x59);

    #endregion

    #region variables

    // key = device path
    private IDictionary<string, ICollection<ITuner>> _knownTuners = new Dictionary<string, ICollection<ITuner>>();

    #endregion

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "RTL283x";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITuner> DetectTuners()
    {
      this.LogDebug("RTL283x detector: detect tuners");
      List<ITuner> tuners = new List<ITuner>();
      IDictionary<string, ICollection<ITuner>> knownTuners = new Dictionary<string, ICollection<ITuner>>();

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
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
        ICollection<ITuner> deviceTuners;
        if (_knownTuners.TryGetValue(devicePath, out deviceTuners))
        {
          device.Dispose();
          tuners.AddRange(deviceTuners);
          knownTuners.Add(devicePath, deviceTuners);
          continue;
        }
        deviceTuners = new List<ITuner>(4);

        // Check if we have an RTL283x based tuner.
        this.LogDebug("RTL283x detector: check tuner, name = {0}, device path = {1}", name, devicePath);
        Guid filterClsid = typeof(IBaseFilter).GUID;
        object obj = null;
        try
        {
          device.Mon.BindToObject(null, null, ref filterClsid, out obj);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "RTL283x detector: failed to create filter instance, name = {0}, device path = {1}", name, devicePath);
          device.Dispose();
          continue;
        }

        try
        {
          IKsPropertySet ps = obj as IKsPropertySet;
          if (ps == null)
          {
            this.LogDebug("RTL283x detector: filter is not a property set");
            device.Dispose();
            continue;
          }
          KSPropertySupport support;
          int hr = ps.QuerySupported(BDA_EXTENSION_FILTER_MODE_PROPERTY_SET, (int)BdaExtensionFilterModeProperty.GetDemodSupportedModes, out support);
          if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
          {
            this.LogDebug("RTL283x detector: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
            device.Dispose();
            continue;
          }

          this.LogDebug("RTL283x detector: property set supported, checking supported modes");
          IntPtr buffer = Marshal.AllocCoTaskMem(4);
          Rtl283xBroadcastStandard supportedModes;
          try
          {
            int returnedByteCount;
            hr = ps.Get(BDA_EXTENSION_FILTER_MODE_PROPERTY_SET, (int)BdaExtensionFilterModeProperty.GetDemodSupportedModes, IntPtr.Zero, 0, buffer, 4, out returnedByteCount);
            if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != 4)
            {
              this.LogError("RTL283x detector: failed to read supported modes, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
              device.Dispose();
              continue;
            }
            supportedModes = (Rtl283xBroadcastStandard)Marshal.ReadInt32(buffer, 0);
          }
          finally
          {
            Marshal.FreeCoTaskMem(buffer);
          }

          this.LogDebug("  modes = {0}", supportedModes);

          if (supportedModes.HasFlag(Rtl283xBroadcastStandard.Fm))
          {
            deviceTuners.Add(new TunerRtl283xFm(device));
          }
        }
        finally
        {
          Release.ComObject("RTL283x detection BDA source filter", ref obj);
          tuners.AddRange(deviceTuners);
          knownTuners.Add(devicePath, deviceTuners);
        }
      }

      _knownTuners = knownTuners;
      return tuners;
    }
  }
}