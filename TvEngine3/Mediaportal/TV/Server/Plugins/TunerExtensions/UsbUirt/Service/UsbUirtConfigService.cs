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
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Mediaportal.TV.Server.Common.Types.Channel;
using Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Service
{
  internal class UsbUirtConfigService : IUsbUirtConfigService
  {
    public delegate void OnTunerSetTopBoxConfigChange();
    public event OnTunerSetTopBoxConfigChange OnConfigChange;

    #region constants

    public const string CHANNEL_NUMBER_POWER_ON = "power on";
    public const string CHANNEL_NUMBER_POWER_OFF = "power off";

    private const string USB_UIRT_USB_ID = "usb#vid_0403&pid_f850";

    #endregion

    #region variables

    private SystemChangeNotifier _systemChangeNotifier = new SystemChangeNotifier();
    private object _lockDevices = new object();
    private IDictionary<uint, Driver> _devices = new Dictionary<uint, Driver>();
    private SortedList<string, string> _stbProfileNames = new SortedList<string, string>();

    #endregion

    public void Start()
    {
      this.LogDebug("USB-UIRT service: start");

      lock (_lockDevices)
      {
        for (uint i = 0; i <= Driver.MAXIMUM_DEVICE_INDEX; i++)
        {
          if (Driver.IsConnected(i))
          {
            Driver d = new Driver(i);
            d.Open();
            _devices.Add(i, d);
          }
        }
      }

      _systemChangeNotifier = new SystemChangeNotifier();
      _systemChangeNotifier.OnDeviceInterfaceChange += OnDeviceChange;
      _systemChangeNotifier.OnPowerBroadcast += OnPowerBroadcast;
    }

    public void Stop()
    {
      this.LogDebug("USB-UIRT service: stop");

      if (_systemChangeNotifier != null)
      {
        _systemChangeNotifier.OnDeviceInterfaceChange -= OnDeviceChange;
        _systemChangeNotifier.OnPowerBroadcast -= OnPowerBroadcast;
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

    #region IUsbUirtConfigService members

    public TunerSetTopBoxConfig GetSetTopBoxConfigurationForTuner(string tunerExternalId)
    {
      return TunerSetTopBoxConfig.Load(tunerExternalId);
    }

    public void SaveSetTopBoxConfiguration(ICollection<TunerSetTopBoxConfig> config)
    {
      foreach (TunerSetTopBoxConfig c in config)
      {
        c.Save();
      }
      var onConfigChangeSubscribers = OnConfigChange;
      if (onConfigChangeSubscribers != null)
      {
        onConfigChangeSubscribers();
      }
    }

    public ICollection<UsbUirtDetail> GetAllUsbUirtDetails()
    {
      lock (_lockDevices)
      {
        List<UsbUirtDetail> details = new List<UsbUirtDetail>(_devices.Count);
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

      this.LogDebug("USB-UIRT service: save set top box profile, name = {0}, is overwrite = {1}, file = {2}", profile.Name, isOverwrite, fileName);
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
        this.LogError(ex, "USB-UIRT config: failed to save set top box profile, file name = {0}", fileName);
      }
      return false;
    }

    public LearnResult Learn(uint usbUirtIndex, TimeSpan timeLimit, out string command)
    {
      command = string.Empty;
      Driver driver;
      lock (_lockDevices)
      {
        if (!_devices.TryGetValue(usbUirtIndex, out driver))
        {
          this.LogError("USB-UIRT service: failed to learn, the device is currently unavailable, index = {0}", usbUirtIndex);
          return LearnResult.Unavailable;
        }
      }

      return driver.Learn(timeLimit, out command);
    }

    public TransmitResult Transmit(uint usbUirtIndex, TransmitZone zone, string setTopBoxProfileName, string channelNumber)
    {
      if (
        usbUirtIndex > Driver.MAXIMUM_DEVICE_INDEX ||
        zone == TransmitZone.None ||
        string.IsNullOrEmpty(setTopBoxProfileName)
      )
      {
        this.LogError("USB-UIRT service: failed to transmit, invalid parameters, USB-UIRT index = {0}, zone(s) = [{1}], profile name = {2}", usbUirtIndex, zone, setTopBoxProfileName ?? string.Empty);
        return TransmitResult.Fail;
      }

      ushort majorChannelNumber;
      ushort? minorChannelNumber;
      if (!LogicalChannelNumber.Parse(channelNumber, out majorChannelNumber, out minorChannelNumber))
      {
        this.LogError("USB-UIRT service: failed to transmit, invalid channel number, number = {0}", channelNumber);
        return TransmitResult.Fail;
      }
      if (!minorChannelNumber.HasValue)
      {
        channelNumber = majorChannelNumber.ToString();
      }
      else
      {
        channelNumber = string.Format("{0}.{1}", majorChannelNumber, minorChannelNumber.Value);
      }

      // Load the STB profile.
      UpdateStbProfileNames();
      string setTopBoxProfileFileName;
      if (!_stbProfileNames.TryGetValue(setTopBoxProfileName, out setTopBoxProfileFileName))
      {
        this.LogError("USB-UIRT service: failed to transmit, can't locate the set top box profile, profile name = {0}", setTopBoxProfileName);
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
          this.LogError(ex, "USB-UIRT service: failed to transmit, invalid set top box profile, file name = {0}", setTopBoxProfileFileName);
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
        if (!_devices.TryGetValue(usbUirtIndex, out driver))
        {
          this.LogError("USB-UIRT service: failed to transmit, the device is currently unavailable, index = {0}", usbUirtIndex);
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
          result = driver.Transmit(commandName, commandString, zone);
          if (result == TransmitResult.Success && profile.PowerChangeDelay > 0)
          {
            Thread.Sleep(profile.PowerChangeDelay);
          }
          return result;
        }
        this.LogError("USB-UIRT service: failed to transmit, the {0} command has not been learned, profile name = {1}", commandName, setTopBoxProfileName);
        return TransmitResult.InvalidCommand;
      }

      // pre-change
      if (!string.IsNullOrEmpty(profile.PreChange))
      {
        result = driver.Transmit("pre-change", profile.PreChange, zone);
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
        if (minorChannelNumber.HasValue)
        {
          digitCount--;
        }
        for (int d = profile.DigitCount; d > digitCount; d--)
        {
          result = driver.Transmit("padding 0", profile.Digit0, zone);
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
          commandName = string.Format("separator");
          commandString = profile.Separator;
        }
        else
        {
          int d = (int)c - (int)'0';
          if (d < 0 || d >= digits.Length)
          {
            this.LogError("USB-UIRT service: failed to transmit, channel number contains unexpected content, number = {0}", channelNumber);
            return TransmitResult.Fail;
          }
          commandName = c.ToString();
          commandString = digits[d];
        }
        result = driver.Transmit(commandName, commandString, zone);
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
        return driver.Transmit("enter/select/OK", profile.Enter, zone);
      }
      return TransmitResult.Success;
    }

    #endregion

    private void OnDeviceChange(NativeMethods.DBT_MANAGEMENT_EVENT eventType, Guid classGuid, string devicePath)
    {
      if (string.IsNullOrEmpty(devicePath) || devicePath.IndexOf(USB_UIRT_USB_ID, StringComparison.InvariantCultureIgnoreCase) <= 0)
      {
        return;
      }

      this.LogInfo("USB-UIRT service: on device change, event type = {0}, GUID = {1}, device path = {2}", eventType, classGuid, devicePath);

      ThreadPool.QueueUserWorkItem(delegate
      {
        lock (_lockDevices)
        {
          if (eventType == NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEARRIVAL)
          {
            for (uint i = 0; i <= Driver.MAXIMUM_DEVICE_INDEX; i++)
            {
              if (!_devices.ContainsKey(i) && Driver.IsConnected(i))
              {
                Driver d = new Driver(i);
                d.Open();
                _devices[i] = d;
                return;
              }
            }
            this.LogWarn("USB-UIRT service: failed to identify connected device, device path = {0}", devicePath);
          }
          else if (eventType == NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEREMOVECOMPLETE)
          {
            uint? removedDeviceIndex = null;
            foreach (Driver device in _devices.Values)
            {
              if (!Driver.IsConnected(device.Index))
              {
                device.Close();
                removedDeviceIndex = device.Index;
                break;
              }
            }
            if (removedDeviceIndex.HasValue)
            {
              _devices.Remove(removedDeviceIndex.Value);
              return;
            }

            this.LogWarn("USB-UIRT service: failed to identify removed device, device path = {0}", devicePath);
          }
        }
      });
    }

    private void OnPowerBroadcast(NativeMethods.PBT_MANAGEMENT_EVENT eventType)
    {
      if (eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMSUSPEND)
      {
        lock (_lockDevices)
        {
          foreach (Driver device in _devices.Values)
          {
            device.Close();
          }
        }
      }
      else if (
        eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMEAUTOMATIC ||
        eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMECRITICAL ||
        eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMESUSPEND
      )
      {
        lock (_lockDevices)
        {
          foreach (Driver device in _devices.Values)
          {
            device.Open();
          }
        }
      }
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
            this.LogWarn(ex, "USB-UIRT: invalid set top box profile, file name = {0}", fileName);
          }
          xmlFile.Close();
        }
      }
      _stbProfileNames = stbProfileNames;
    }
  }
}