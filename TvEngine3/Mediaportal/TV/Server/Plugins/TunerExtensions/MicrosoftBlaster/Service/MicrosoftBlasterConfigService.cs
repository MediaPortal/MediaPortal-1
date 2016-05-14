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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Service
{
  internal class MicrosoftBlasterConfigService : IMicrosoftBlasterConfigService
  {
    public delegate void OnBlasterConfigChange();
    public event OnBlasterConfigChange OnConfigChange;

    #region constants

    public const string CHANNEL_NUMBER_POWER_ON = "power on";
    public const string CHANNEL_NUMBER_POWER_OFF = "power off";

    private static readonly Guid DEVICE_INTERFACE_GUID_MS_EHOME = new Guid(0x7951772d, 0xcd50, 0x49b7, 0xb1, 0x03, 0x2b, 0xaa, 0xc4, 0x94, 0xfc, 0x57);
    private static readonly Guid DEVICE_INTERFACE_GUID_MS_EHOME_REPLACEMENT = new Guid(0x00873fdf, 0x61a8, 0x11d1, 0xaa, 0x5e, 0x00, 0xc0, 0x4f, 0xb1, 0x72, 0x8b);

    #endregion

    #region variables

    private SystemChangeNotifier _systemChangeNotifier = new SystemChangeNotifier();
    private object _lockDevices = new object();
    private IDictionary<string, Driver> _devices = new Dictionary<string, Driver>();
    private SortedList<string, string> _stbProfileNames = new SortedList<string, string>();

    #endregion

    public void Start()
    {
      this.LogDebug("Microsoft blaster service: start");
      List<Driver> devices = new List<Driver>();
      if (Environment.OSVersion.Version.Major >= 6) // Vista or later
      {
        foreach (string devicePath in FindDevices(DEVICE_INTERFACE_GUID_MS_EHOME))
        {
          devices.Add(new DriverPort(devicePath));
        }
      }
      else
      {
        foreach (string devicePath in FindDevices(DEVICE_INTERFACE_GUID_MS_EHOME))
        {
          devices.Add(new DriverEmulator(devicePath, false));
        }
      }

      foreach (string devicePath in FindDevices(DEVICE_INTERFACE_GUID_MS_EHOME_REPLACEMENT))
      {
        devices.Add(new DriverEmulator(devicePath, true));
      }

      lock (_lockDevices)
      {
        foreach (Driver device in devices)
        {
          device.Open();
          _devices.Add(device.DevicePath, device);
        }
      }

      _systemChangeNotifier = new SystemChangeNotifier();
      _systemChangeNotifier.OnDeviceInterfaceChange += OnDeviceChange;
    }

    public void Stop()
    {
      this.LogDebug("Microsoft blaster service: stop");
      if (_systemChangeNotifier != null)
      {
        _systemChangeNotifier.OnDeviceInterfaceChange -= OnDeviceChange;
        _systemChangeNotifier.Dispose();
        _systemChangeNotifier = null;
      }

      lock (_lockDevices)
      {
        foreach (Driver device in _devices.Values)
        {
          device.Close();
        }
        _devices.Clear();
      }
    }

    #region IMicrosoftBlasterConfigService members

    public SetTopBoxConfig GetSetTopBoxConfigurationForTuner(string tunerExternalId)
    {
      return SetTopBoxConfig.LoadSettings(tunerExternalId);
    }

    public void SaveSetTopBoxConfiguration(ICollection<SetTopBoxConfig> settings)
    {
      foreach (SetTopBoxConfig config in settings)
      {
        config.SaveSettings();
      }
      if (OnConfigChange != null)
      {
        OnConfigChange();
      }
    }

    public ICollection<TransceiverDetail> GetAllTransceiverDetails()
    {
      lock (_lockDevices)
      {
        List<TransceiverDetail> details = new List<TransceiverDetail>(_devices.Count);
        foreach (Driver device in _devices.Values)
        {
          details.Add(device.GetDetail());
        }
        return details;
      }
    }

    public IList<string> GetAllSetTopBoxProfileNames()
    {
      UpdateStbProfileNames();
      return _stbProfileNames.Keys;
    }

    public bool SaveSetTopBoxProfile(SetTopBoxProfile profile)
    {
      UpdateStbProfileNames();

      // If a profile with the same name already exists, overwrite it. Otherwise create a new profile.
      bool isOverwrite = true;
      string fileName;
      if (!_stbProfileNames.TryGetValue(profile.Name, out fileName))
      {
        fileName = profile.Name;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
          fileName = fileName.Replace(c, '_');
        }
        fileName = Path.Combine(PathManager.GetDataPath, "SetTopBoxes", fileName + ".xml");
        isOverwrite = false;
      }

      this.LogDebug("Microsoft blaster service: save set top box profile, name = {0}, is overwrite = {1}, file = {2}", profile.Name, isOverwrite, fileName);
      try
      {
        using (XmlWriter xmlFile = XmlWriter.Create(fileName, new XmlWriterSettings() { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine }))
        {
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(SetTopBoxProfile));
          xmlSerializer.Serialize(xmlFile, profile);
          xmlFile.Close();
        }
        if (!isOverwrite)
        {
          _stbProfileNames.Add(profile.Name, fileName);
        }
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Microsoft blaster config: failed to save set top box profile, file name = {0}", fileName);
      }
      return false;
    }

    public LearnResult Learn(string transceiverDevicePath, TimeSpan timeLimit, out string command)
    {
      command = string.Empty;
      Driver driver;
      lock (_lockDevices)
      {
        if (!_devices.TryGetValue(transceiverDevicePath, out driver))
        {
          this.LogError("Microsoft blaster service: failed to learn, the device is currently unavailable, device path = {0}", transceiverDevicePath);
          return LearnResult.Unavailable;
        }
      }

      return driver.Learn(timeLimit, out command);
    }

    public TransmitResult Transmit(string transceiverDevicePath, TransmitPort port, string setTopBoxProfileName, string channelNumber)
    {
      if (
        string.IsNullOrEmpty(transceiverDevicePath) ||
        port == TransmitPort.None ||
        string.IsNullOrEmpty(setTopBoxProfileName) ||
        string.IsNullOrEmpty(channelNumber)
      )
      {
        this.LogError("Microsoft blaster service: failed to transmit, invalid parameters");
        return TransmitResult.Fail;
      }

      // Load the STB profile.
      UpdateStbProfileNames();
      string setTopBoxProfileFileName;
      if (!_stbProfileNames.TryGetValue(setTopBoxProfileName, out setTopBoxProfileFileName))
      {
        this.LogError("Microsoft blaster service: failed to transmit, can't locate the set top box profile, profile name = {0}", setTopBoxProfileName);
        return TransmitResult.InvalidProfile;
      }
      SetTopBoxProfile profile;
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(SetTopBoxProfile));
      using (XmlReader xmlFile = XmlReader.Create(setTopBoxProfileFileName))
      {
        try
        {
          profile = (SetTopBoxProfile)xmlSerializer.Deserialize(xmlFile);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Microsoft blaster service: failed to transmit, invalid set top box profile, file name = {0}", setTopBoxProfileFileName);
          return TransmitResult.InvalidProfile;
        }
        finally
        {
          xmlFile.Close();
        }
      }

      Driver driver;
      lock (_lockDevices)
      {
        if (!_devices.TryGetValue(transceiverDevicePath, out driver))
        {
          this.LogError("Microsoft blaster service: failed to transmit, the device is currently unavailable, device path = {0}", transceiverDevicePath);
          return TransmitResult.Unavailable;
        }
      }

      // power off/on
      TransmitResult result;
      string commandName = null;
      string commandString = null;
      if (string.Equals(channelNumber, CHANNEL_NUMBER_POWER_OFF))
      {
        commandName = "power off";
        commandString = profile.PowerOff;
      }
      else if (string.Equals(channelNumber, CHANNEL_NUMBER_POWER_ON))
      {
        commandName = "power on";
        commandString = profile.PowerOn;
      }
      if (commandName != null)
      {
        if (string.IsNullOrEmpty(commandString))
        {
          commandName += " (toggle)";
          commandString = profile.PowerToggle;
        }
        if (!string.IsNullOrEmpty(commandString))
        {
          result = driver.Transmit(commandName, commandString, port);
          if (result == TransmitResult.Success && profile.PowerChangeDelay > 0)
          {
            Thread.Sleep(profile.PowerChangeDelay);
          }
          return result;
        }
        this.LogError("Microsoft blaster service: failed to transmit, the {0} command has not been learned, profile name = {1}", commandName, setTopBoxProfileName);
        return TransmitResult.InvalidCommand;
      }

      // pre-change
      if (!string.IsNullOrEmpty(profile.PreChange))
      {
        result = driver.Transmit("pre-change", profile.PreChange, port);
        if (result != TransmitResult.Success)
        {
          return result;
        }
        if (profile.CommandDelay > 0)
        {
          Thread.Sleep(profile.CommandDelay);
        }
      }

      // padding
      if (profile.DigitCount > 0)
      {
        int digitCount = channelNumber.Length;
        if (channelNumber.Contains("."))
        {
          digitCount--;
        }
        for (int d = profile.DigitCount; d > digitCount; d--)
        {
          result = driver.Transmit("padding 0", profile.Digit0, port);
          if (result != TransmitResult.Success)
          {
            return result;
          }
          if (profile.CommandDelay > 0)
          {
            Thread.Sleep(profile.CommandDelay);
          }
        }
      }

      // channel number
      bool isFirst = true;
      string[] digits = new string[10] { profile.Digit0, profile.Digit1, profile.Digit2, profile.Digit3, profile.Digit4, profile.Digit5, profile.Digit6, profile.Digit7, profile.Digit8, profile.Digit9 };
      foreach (char c in channelNumber.ToCharArray())
      {
        if (!isFirst && profile.CommandDelay > 0)
        {
          Thread.Sleep(profile.CommandDelay);
        }
        isFirst = false;

        if (c == '.')
        {
          commandName = "separator (.)";
          commandString = profile.Separator;
        }
        else
        {
          int d = (int)c - (int)'0';
          if (d < 0 || d >= digits.Length)
          {
            this.LogError("Microsoft blaster service: failed to transmit, channel number contains unexpected content, channel number = {0}", channelNumber);
            return TransmitResult.Fail;
          }
          commandName = c.ToString();
          commandString = digits[d];
        }
        result = driver.Transmit(commandName, commandString, port);
        if (result != TransmitResult.Success)
        {
          return result;
        }
      }

      // enter/select/OK
      if (!string.IsNullOrEmpty(profile.Enter))
      {
        if (profile.CommandDelay > 0)
        {
          Thread.Sleep(profile.CommandDelay);
        }
        return driver.Transmit("enter/select/OK", profile.Enter, port);
      }
      return TransmitResult.Success;
    }

    #endregion

    /// <summary>
    /// Find the devices which support a given interface.
    /// </summary>
    /// <param name="iid">A device interface identifier.</param>
    /// <returns>the device paths of each connected device which supports the interface</returns>
    private ICollection<string> FindDevices(Guid iid)
    {
      this.LogDebug("Microsoft blaster service: find devices, IID = {0}", iid);
      ICollection<string> devices = new List<string>();
      IntPtr devInfoSet = NativeMethods.SetupDiGetClassDevs(ref iid, null, IntPtr.Zero, NativeMethods.DiGetClassFlags.DIGCF_DEVICEINTERFACE | NativeMethods.DiGetClassFlags.DIGCF_PRESENT);
      if (devInfoSet == IntPtr.Zero || devInfoSet == NativeMethods.INVALID_HANDLE_VALUE)
      {
        this.LogError("Microsoft blaster service: failed to get device information set");
        return devices;
      }

      try
      {
        int error;
        uint index = 0;
        NativeMethods.SP_DEVINFO_DATA devInfo = new NativeMethods.SP_DEVINFO_DATA();
        devInfo.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVINFO_DATA));
        while (NativeMethods.SetupDiEnumDeviceInfo(devInfoSet, index++, ref devInfo))
        {
          NativeMethods.SP_DEVICE_INTERFACE_DATA devInterface = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
          devInterface.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVICE_INTERFACE_DATA));
          if (!NativeMethods.SetupDiEnumDeviceInterfaces(devInfoSet, ref devInfo, ref iid, 0, ref devInterface))
          {
            this.LogError("Microsoft blaster service: failed to get interface for device, error = {0}, dev inst = {1}, IID = {2}", Marshal.GetLastWin32Error(), devInfo.DevInst, iid);
            continue;
          }

          // Get the device path for the interface. First get the length, then
          // get the device path itself.
          uint deviceInterfaceDetailSize = 0;
          if (NativeMethods.SetupDiGetDeviceInterfaceDetail(devInfoSet, ref devInterface, IntPtr.Zero, 0, out deviceInterfaceDetailSize, IntPtr.Zero) || deviceInterfaceDetailSize == 0)
          {
            this.LogError("Microsoft blaster service: failed to get interface detail for device, error = {0}, required size = {1}, dev inst = {2}, IID = {3}", Marshal.GetLastWin32Error(), deviceInterfaceDetailSize, devInfo.DevInst, iid);
            continue;
          }
          error = Marshal.GetLastWin32Error();
          if (error != (int)NativeMethods.SystemErrorCode.ERROR_INSUFFICIENT_BUFFER)
          {
            // ERROR_INSUFFICIENT_BUFFER is the ***SUCCESS*** result when
            // retrieving the detail data length.
            this.LogError("Microsoft blaster service: failed to get device interface detail length for device, error = {0}, required size = {1}, dev inst = {2}, IID = {3}", Marshal.GetLastWin32Error(), deviceInterfaceDetailSize, devInfo.DevInst, iid);
            continue;
          }

          IntPtr buffer = Marshal.AllocHGlobal((int)deviceInterfaceDetailSize);
          try
          {
            // Stupid Windows - different packing for 32 and 64 bit. Ugh!
            // http://stackoverflow.com/questions/10728644/properly-declare-sp-device-interface-detail-data-for-pinvoke
            if (IntPtr.Size == 8)
            {
              Marshal.WriteInt32(buffer, 0, 8);   // 4 byte cbSize DWORD + 2 byte DevicePath TCHAR [wchar_t since we call the *W() version of the function] + 2 bytes padding [8 byte pack for 64 bit]
            }
            else
            {
              Marshal.WriteInt32(buffer, 0, 6);   // 4 byte cbSize DWORD + 2 byte DevicePath TCHAR [wchar_t since we call the *W() version of the function] + 0 bytes padding [1 byte pack for 32 bit]
            }
            if (!NativeMethods.SetupDiGetDeviceInterfaceDetail(devInfoSet, ref devInterface, buffer, deviceInterfaceDetailSize, IntPtr.Zero, IntPtr.Zero))
            {
              this.LogError("Microsoft blaster service: failed to get device interface detail length for device, error = {0}, required size = {1}, dev inst = {2}, IID = {2}", Marshal.GetLastWin32Error(), deviceInterfaceDetailSize, devInfo.DevInst, iid);
              continue;
            }

            string devicePath = Marshal.PtrToStringUni(IntPtr.Add(buffer, 4));
            if (string.IsNullOrEmpty(devicePath))
            {
              this.LogError("Microsoft blaster service: failed to get device path for device, dev inst = {0}, size = {1}, IID = {2}", devInfo.DevInst, deviceInterfaceDetailSize, iid);
              continue;
            }
            this.LogDebug("  dev inst = {0}, device path = {1}", devInfo.DevInst, devicePath);

            devices.Add(devicePath.ToLowerInvariant());
          }
          finally
          {
            Marshal.FreeHGlobal(buffer);
          }
        }

        error = Marshal.GetLastWin32Error();
        if (error != (int)NativeMethods.SystemErrorCode.ERROR_NO_MORE_ITEMS)
        {
          this.LogError("Microsoft blaster service: failed to get next device, error = {0}", error);
        }
      }
      finally
      {
        NativeMethods.SetupDiDestroyDeviceInfoList(devInfoSet);
      }

      return devices;
    }

    private void OnDeviceChange(NativeMethods.DBT_MANAGEMENT_EVENT eventType, Guid classGuid, string devicePath)
    {
      if (
        string.IsNullOrEmpty(devicePath) ||
        (
          classGuid != DEVICE_INTERFACE_GUID_MS_EHOME &&
          classGuid != DEVICE_INTERFACE_GUID_MS_EHOME_REPLACEMENT
        )
      )
      {
        return;
      }

      this.LogInfo("Microsoft blaster service: on device change, event type = {0}, GUID = {1}, device path = {2}", eventType, classGuid, devicePath);
      devicePath = devicePath.ToLowerInvariant();

      ThreadPool.QueueUserWorkItem(delegate
      {
        Driver device;
        lock (_lockDevices)
        {
          if (eventType == NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEARRIVAL)
          {
            if (classGuid == DEVICE_INTERFACE_GUID_MS_EHOME_REPLACEMENT)
            {
              device = new DriverEmulator(devicePath, true);
            }
            else if (Environment.OSVersion.Version.Major >= 6)  // Vista or later
            {
              device = new DriverPort(devicePath);
            }
            else
            {
              device = new DriverEmulator(devicePath, false);
            }
            device.Open();
            _devices.Add(devicePath, device);
          }
          else if (eventType == NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEREMOVECOMPLETE)
          {
            if (_devices.TryGetValue(devicePath, out device))
            {
              device.Close();
              _devices.Remove(devicePath);
            }
          }
        }
      });
    }

    private void UpdateStbProfileNames()
    {
      string[] stbProfileFileNames = Directory.GetFiles(Path.Combine(PathManager.GetDataPath, "SetTopBoxes"), "*.xml");
      if (stbProfileFileNames.Length == _stbProfileNames.Count)
      {
        return;
      }

      SortedList<string, string> stbProfileNames = new SortedList<string, string>(stbProfileFileNames.Length);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(SetTopBoxProfile));
      foreach (string fileName in stbProfileFileNames)
      {
        using (XmlReader xmlFile = XmlReader.Create(fileName))
        {
          try
          {
            SetTopBoxProfile profile = (SetTopBoxProfile)xmlSerializer.Deserialize(xmlFile);
            stbProfileNames.Add(profile.Name, fileName);
          }
          catch (Exception ex)
          {
            this.LogWarn(ex, "Microsoft blaster: invalid set top box profile, file name = {0}", fileName);
          }
          xmlFile.Close();
        }
      }
      _stbProfileNames = stbProfileNames;
    }
  }
}