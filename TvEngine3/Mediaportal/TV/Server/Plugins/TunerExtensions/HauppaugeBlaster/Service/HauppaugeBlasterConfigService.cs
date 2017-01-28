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
using System.IO;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using MediaPortal.Common.Utils;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster.Service
{
  internal class HauppaugeBlasterConfigService : IHauppaugeBlasterConfigService
  {
    public delegate void OnBlasterConfigChange(string tunerExternalIdPort1, string tunerExternalIdPort2);
    public event OnBlasterConfigChange OnConfigChange;

    private Blaster _blaster = new Blaster();
    private SystemChangeNotifier _systemChangeNotifier = null;

    ~HauppaugeBlasterConfigService()
    {
      Stop();
    }

    public void Start()
    {
      if (_systemChangeNotifier == null)
      {
        _systemChangeNotifier = new SystemChangeNotifier();
        _systemChangeNotifier.OnPowerBroadcast += OnPowerBroadcast;
      }
      _blaster.OpenInterface();
    }

    public void Stop()
    {
      if (_systemChangeNotifier != null)
      {
        _systemChangeNotifier.OnPowerBroadcast -= OnPowerBroadcast;
        _systemChangeNotifier.Dispose();
        _systemChangeNotifier = null;
      }
      if (_blaster != null)
      {
        _blaster.CloseInterface();
      }
    }

    private void OnPowerBroadcast(NativeMethods.PBT_MANAGEMENT_EVENT eventType)
    {
      if (eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMSUSPEND)
      {
        _blaster.CloseInterface();
      }
      else if (
        eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMEAUTOMATIC ||
        eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMECRITICAL ||
        eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMESUSPEND
      )
      {
        _blaster.OpenInterface();
      }
    }

    #region IHauppaugeBlasterConfigService members

    public void GetBlasterTunerExternalIds(out string tunerExternalIdPort1, out string tunerExternalIdPort2)
    {
      tunerExternalIdPort1 = SettingsManagement.GetValue("hauppaugeBlasterTunerPort1", string.Empty);
      tunerExternalIdPort2 = SettingsManagement.GetValue("hauppaugeBlasterTunerPort2", string.Empty);
    }

    public void SaveBlasterTunerExternalIds(string tunerExternalIdPort1, string tunerExternalIdPort2)
    {
      SettingsManagement.SaveValue("hauppaugeBlasterTunerPort1", tunerExternalIdPort1);
      SettingsManagement.SaveValue("hauppaugeBlasterTunerPort2", tunerExternalIdPort2);
      var onConfigChangeSubscribers = OnConfigChange;
      if (onConfigChangeSubscribers != null)
      {
        onConfigChangeSubscribers(tunerExternalIdPort1, tunerExternalIdPort2);
      }
    }

    public void GetBlasterInstallDetails(out string irBlastVersion, out string blastCfgLocation, out bool isHcwIrBlastDllPresent, out string blasterVersion, out int blasterPortCount)
    {
      irBlastVersion = null;
      blastCfgLocation = null;
      isHcwIrBlastDllPresent = false;
      blasterVersion = _blaster.Version;
      blasterPortCount = _blaster.PortCount;

      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hauppauge WinTV IR Blaster"))
      {
        if (key != null)
        {
          try
          {
            object value = key.GetValue("DisplayVersion");
            if (value != null)
            {
              irBlastVersion = value.ToString();
            }

            value = key.GetValue("InstallLocation");
            if (value != null)
            {
              blastCfgLocation = Path.Combine(value.ToString(), "BlastCfg.exe");
              if (!File.Exists(blastCfgLocation))
              {
                blastCfgLocation = null;
              }
            }
          }
          finally
          {
            key.Close();
          }
        }
      }

      try
      {
        IntPtr handle = NativeMethods.LoadLibrary("hcwIRblast.dll");
        if (handle != IntPtr.Zero)
        {
          NativeMethods.FreeLibrary(handle);
          isHcwIrBlastDllPresent = true;
        }
      }
      catch
      {
      }
    }

    public bool BlastChannelNumber(string channelNumber, int port)
    {
      return _blaster.BlastChannelNumber(channelNumber, port);
    }

    #endregion
  }
}