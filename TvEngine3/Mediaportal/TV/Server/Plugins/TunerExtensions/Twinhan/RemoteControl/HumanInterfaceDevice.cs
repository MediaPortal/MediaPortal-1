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
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl
{
  internal class HumanInterfaceDevice : IDisposable
  {
    #region enums

    // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724336%28v=vs.85%29.aspx
    private enum KeyboardType : uint
    {
      IbmPcXt83Key = 1,
      OlivettiIco102Key,
      IbmPcAt84Key,
      IbmEnhanced102Key,
      Nokia1050,
      Nokia9140,
      Japanese,
      Usb = 81
    }

    #endregion

    #region constants

    private static readonly int HID_INPUT_DATA_OFFSET = Marshal.OffsetOf(typeof(NativeMethods.RAWINPUT), "data").ToInt32() + Marshal.SizeOf(typeof(NativeMethods.RAWHID));

    #endregion

    #region variables

    private string _id = string.Empty;
    private string _name = string.Empty;
    private NativeMethods.RawInputDeviceType _type = NativeMethods.RawInputDeviceType.RIM_TYPEKEYBOARD;
    private bool _isTerraTecDriver = false;
    private IntPtr _handle = IntPtr.Zero;
    private IntPtr _preParsedData = IntPtr.Zero;
    private HidUsagePage _usagePage = HidUsagePage.Consumer;
    private ushort _usageCollection = 1;
    private VirtualKeyModifier _modifiers = 0;
    private bool _isOpen = false;

    #endregion

    public HumanInterfaceDevice(string id, string name, NativeMethods.RawInputDeviceType type, bool isTerraTecDriver, IntPtr handle)
    {
      _id = id;
      _name = name;
      _type = type;
      _isTerraTecDriver = isTerraTecDriver;
      _handle = handle;
    }

    ~HumanInterfaceDevice()
    {
      Dispose(false);
    }

    #region IDisposable member

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
      Close();
    }

    #endregion

    #region properties

    public string Id
    {
      get
      {
        return _id;
      }
    }

    public NativeMethods.RawInputDeviceType DeviceType
    {
      get
      {
        return _type;
      }
    }

    public IntPtr Handle
    {
      get
      {
        return _handle;
      }
    }

    public HidUsagePage UsagePage
    {
      get
      {
        return _usagePage;
      }
    }

    public ushort UsageCollection
    {
      get
      {
        return _usageCollection;
      }
    }

    public bool IsOpen
    {
      get
      {
        return _isOpen;
      }
    }

    #endregion

    public bool Open()
    {
      if (_isOpen)
      {
        return true;
      }

      this.LogDebug("Twinhan HID: open");
      this.LogDebug("  ID           = {0}", _id);
      this.LogDebug("  name         = {0}", _name);
      this.LogDebug("  type         = {0}", _type);

      uint deviceInfoSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RID_DEVICE_INFO));
      IntPtr deviceInfo = Marshal.AllocHGlobal((int)deviceInfoSize);

      try
      {
        NativeMethods.RID_DEVICE_INFO info = new NativeMethods.RID_DEVICE_INFO();
        info.cbSize = deviceInfoSize;
        Marshal.StructureToPtr(info, deviceInfo, false);
        int result = NativeMethods.GetRawInputDeviceInfo(_handle, NativeMethods.RawInputInfoCommand.RIDI_DEVICEINFO, deviceInfo, ref deviceInfoSize);
        if (result <= 0)
        {
          this.LogWarn("Twinhan HID: failed to get raw input device info, result = {0}, error = {1}, required size = {2}, HID = {3}", result, Marshal.GetLastWin32Error(), deviceInfoSize, _id);
          return false;
        }

        if (_type == NativeMethods.RawInputDeviceType.RIM_TYPEHID)
        {
          _preParsedData = LoadPreParsedData();
          if (_preParsedData == IntPtr.Zero)
          {
            return false;
          }
        }

        info = (NativeMethods.RID_DEVICE_INFO)Marshal.PtrToStructure(deviceInfo, typeof(NativeMethods.RID_DEVICE_INFO));
        if (_type == NativeMethods.RawInputDeviceType.RIM_TYPEMOUSE)
        {
          this.LogDebug("  mouse ID     = {0}", info.mouse.dwId);
          this.LogDebug("  # buttons    = {0}", info.mouse.dwNumberOfButtons);
          this.LogDebug("  sample rate  = {0}", info.mouse.dwSampleRate);
          this.LogDebug("  hor. wheel?  = {0}", info.mouse.fHasHorizontalWheel);
          _usagePage = HidUsagePage.GenericDesktopControl;
          _usageCollection = 2;
        }
        else if (_type == NativeMethods.RawInputDeviceType.RIM_TYPEKEYBOARD)
        {
          this.LogDebug("  KB type      = {0}", (KeyboardType)info.keyboard.dwType);
          this.LogDebug("  sub-type     = {0}", info.keyboard.dwSubType);
          this.LogDebug("  mode         = {0}", info.keyboard.dwKeyboardMode);
          this.LogDebug("  # func. keys = {0}", info.keyboard.dwNumberOfFunctionKeys);
          this.LogDebug("  # indicators = {0}", info.keyboard.dwNumberOfIndicators);
          this.LogDebug("  # keys total = {0}", info.keyboard.dwNumberOfKeysTotal);
          _usagePage = HidUsagePage.GenericDesktopControl;
          _usageCollection = 6;
        }
        else if (_type == NativeMethods.RawInputDeviceType.RIM_TYPEHID)
        {
          HidUsagePage p = (HidUsagePage)info.hid.usUsagePage;
          this.LogDebug("  vendor ID    = 0x{0:x}", info.hid.dwVendorId);
          this.LogDebug("  product ID   = 0x{0:x}", info.hid.dwProductId);
          this.LogDebug("  version      = 0x{0:x}", info.hid.dwVersionNumber);
          this.LogDebug("  page         = {0}", p);
          this.LogDebug("  collection   = 0x{0:x2}", info.hid.usUsage);
          _usagePage = p;
          _usageCollection = info.hid.usUsage;
          //DebugCapabilities();
        }
        else
        {
          this.LogWarn("Twinhan HID: unrecognised raw input device type, HID = {0}, type = {1}", _id, _type);
          return false;
        }

        _isOpen = true;
        return true;
      }
      finally
      {
        Marshal.FreeHGlobal(deviceInfo);
      }
    }

    public void Close()
    {
      if (_isOpen)
      {
        this.LogDebug("Twinhan HID: close {0}", _id);
        if (_preParsedData != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(_preParsedData);
          _preParsedData = IntPtr.Zero;
        }
        _isOpen = false;
      }
    }

    public bool GetUsageFromRawInput(NativeMethods.RAWINPUT input, IntPtr rawInput, out TwinhanUsageType usageType, out int usage, out string usageName)
    {
      usageType = 0;
      usage = 0;
      usageName = string.Empty;

      if (input.header.dwType == NativeMethods.RawInputDeviceType.RIM_TYPEKEYBOARD)
      {
        if (input.data.keyboard.Flags == NativeMethods.RawInputKeyboardFlag.RI_KEY_BREAK)
        {
          _modifiers = 0;
          // Key up event. We don't handle repeats, so ignore this.
          return false;
        }

        NativeMethods.VirtualKey vk = input.data.keyboard.VKey;
        if (vk == NativeMethods.VirtualKey.VK_CONTROL)
        {
          _modifiers |= VirtualKeyModifier.Control;
          return false;
        }
        if (vk == NativeMethods.VirtualKey.VK_SHIFT)
        {
          _modifiers |= VirtualKeyModifier.Shift;
          return false;
        }
        if (vk == NativeMethods.VirtualKey.VK_MENU)
        {
          _modifiers |= VirtualKeyModifier.Alt;
          return false;
        }

        usageType = TwinhanUsageType.Keyboard;
        usage = (int)vk | (int)_modifiers;
        usageName = vk.ToString();
        if (_modifiers != 0)
        {
          usageName += string.Format(", modifiers = {0}", _modifiers);
        }
      }
      else if (input.header.dwType == NativeMethods.RawInputDeviceType.RIM_TYPEHID)
      {
        if ((!_isTerraTecDriver && _name.Contains("Col03")) || (_isTerraTecDriver && _name.Contains("Col02")))
        {
          usageType = TwinhanUsageType.Raw;
          usage = Marshal.ReadByte(rawInput, HID_INPUT_DATA_OFFSET + 1);
          usageName = string.Format("0x{0:x2}", usage);
        }
        else if (_name.Contains("Col05"))
        {
          usageType = TwinhanUsageType.Ascii;
          usage = Marshal.ReadByte(rawInput, HID_INPUT_DATA_OFFSET + 1);
          usageName = string.Format("0x{0:x2}", usage);
        }
        else
        {
          byte[] report = new byte[input.data.hid.dwSizeHid];
          Marshal.Copy(IntPtr.Add(rawInput, HID_INPUT_DATA_OFFSET), report, 0, report.Length);
          uint usageCount = input.data.hid.dwCount;
          NativeMethods.USAGE_AND_PAGE[] usages = new NativeMethods.USAGE_AND_PAGE[usageCount];
          NativeMethods.HidStatus status = NativeMethods.HidP_GetUsagesEx(NativeMethods.HIDP_REPORT_TYPE.HidP_Input, 0, usages, ref usageCount, _preParsedData, report, (uint)report.Length);
          if (status != NativeMethods.HidStatus.HIDP_STATUS_SUCCESS)
          {
            this.LogError("Twinhan HID: failed to get raw input usages, status = {0}, HID = {1}", status, _id);
            Dump.DumpBinary(rawInput, (int)input.header.dwSize);
            return false;
          }
          if (usageCount > 1)
          {
            this.LogWarn("Twinhan HID: multiple simultaneous usages not supported, HID = {0}, usage count = {1}", _id, usageCount);
            foreach (NativeMethods.USAGE_AND_PAGE tempUsage in usages)
            {
              this.LogWarn("  page = {0}, usage = {1}", (HidUsagePage)tempUsage.UsagePage, tempUsage.Usage);
            }
          }

          NativeMethods.USAGE_AND_PAGE up = usages[0];
          HidUsagePage page = (HidUsagePage)up.UsagePage;
          usage = up.Usage;
          if (page != HidUsagePage.MceRemote && usage == 0)
          {
            // Key up event. We don't handle repeats, so ignore this.
            return false;
          }
          if (page == HidUsagePage.Consumer)
          {
            usageType = TwinhanUsageType.Consumer;
            usageName = System.Enum.GetName(typeof(HidConsumerUsage), up.Usage);
          }
          else if (page == HidUsagePage.MceRemote)
          {
            usageType = TwinhanUsageType.Mce;
            usageName = System.Enum.GetName(typeof(MceRemoteUsage), up.Usage);
          }
          else
          {
            this.LogError("Twinhan HID: unexpected usage page, HID = {0}, page = {1}, usage = {2}", _id, page, up.Usage);
            return false;
          }
        }
      }
      else
      {
        this.LogError("Twinhan HID: received input from unsupported input device type, HID = {0}, type = {1}", _id, input.header.dwType);
        return false;
      }
      return true;
    }

    private IntPtr LoadPreParsedData()
    {
      uint ppDataSize = 256;
      int result = NativeMethods.GetRawInputDeviceInfo(_handle, NativeMethods.RawInputInfoCommand.RIDI_PREPARSEDDATA, IntPtr.Zero, ref ppDataSize);
      if (result != 0)
      {
        this.LogError("Twinhan HID: failed to get raw input pre-parsed data size, result = {0}, error = {1}, HID = {2}", result, Marshal.GetLastWin32Error(), _name);
        return IntPtr.Zero;
      }

      IntPtr ppData = Marshal.AllocHGlobal((int)ppDataSize);
      result = NativeMethods.GetRawInputDeviceInfo(_handle, NativeMethods.RawInputInfoCommand.RIDI_PREPARSEDDATA, ppData, ref ppDataSize);
      if (result <= 0)
      {
        this.LogError("Twinhan HID: failed to get raw input pre-parsed data, result = {0}, error = {1}, HID = {2}", result, Marshal.GetLastWin32Error(), _name);
        Marshal.FreeHGlobal(ppData);
        return IntPtr.Zero;
      }
      return ppData;
    }

    private void DebugCapabilities()
    {
      this.LogDebug("  capabilities...");
      NativeMethods.HIDP_CAPS capabilities;
      NativeMethods.HidStatus status = NativeMethods.HidP_GetCaps(_preParsedData, out capabilities);
      if (status != NativeMethods.HidStatus.HIDP_STATUS_SUCCESS)
      {
        this.LogError("Twinhan HID: failed to get raw input capabilities, status = {0}, HID = {1}", status, _id);
        return;
      }

      this.LogDebug("    usage                   = 0x{0:x}", capabilities.Usage);
      this.LogDebug("    usage page              = {0}", (HidUsagePage)capabilities.UsagePage);
      this.LogDebug("    input report length     = {0}", capabilities.InputReportByteLength);
      this.LogDebug("    output report length    = {0}", capabilities.OutputReportByteLength);
      this.LogDebug("    feature report length   = {0}", capabilities.FeatureReportByteLength);
      this.LogDebug("    # link collection nodes = {0}", capabilities.NumberLinkCollectionNodes);
      this.LogDebug("    # input button caps.    = {0}", capabilities.NumberInputButtonCaps);
      this.LogDebug("    # input value caps.     = {0}", capabilities.NumberInputValueCaps);
      this.LogDebug("    # input data indices    = {0}", capabilities.NumberInputDataIndices);
      this.LogDebug("    # output button caps.   = {0}", capabilities.NumberOutputButtonCaps);
      this.LogDebug("    # output value caps.    = {0}", capabilities.NumberOutputValueCaps);
      this.LogDebug("    # output data indices   = {0}", capabilities.NumberOutputDataIndices);
      this.LogDebug("    # feature button caps.  = {0}", capabilities.NumberFeatureButtonCaps);
      this.LogDebug("    # feature value caps.   = {0}", capabilities.NumberFeatureValueCaps);
      this.LogDebug("    # feature data indices  = {0}", capabilities.NumberFeatureDataIndices);

      if (capabilities.NumberInputButtonCaps == 0)
      {
        return;
      }

      NativeMethods.HIDP_BUTTON_CAPS[] buttonCapabilities = new NativeMethods.HIDP_BUTTON_CAPS[capabilities.NumberInputButtonCaps];
      status = NativeMethods.HidP_GetButtonCaps(NativeMethods.HIDP_REPORT_TYPE.HidP_Input, buttonCapabilities, ref capabilities.NumberInputButtonCaps, _preParsedData);
      if (status != NativeMethods.HidStatus.HIDP_STATUS_SUCCESS)
      {
        this.LogError("Twinhan HID: failed to get raw input input button capabilities, status = {0}, HID = {1}", status, _id);
        return;
      }

      this.LogDebug("  buttons...");
      foreach (NativeMethods.HIDP_BUTTON_CAPS bc in buttonCapabilities)
      {
        this.LogDebug("    button...");
        this.LogDebug("      usage page = {0}", (HidUsagePage)bc.UsagePage);
        this.LogDebug("      report ID  = {0}", bc.ReportID);
        this.LogDebug("      is alias?  = {0}", bc.IsAlias);
        this.LogDebug("      bit field  = {0}", bc.BitField);
        this.LogDebug("      link coll. = {0}", bc.LinkCollection);
        this.LogDebug("      link usage = 0x{0:x}", bc.LinkUsage);
        this.LogDebug("      link UP    = {0}", (HidUsagePage)bc.LinkUsagePage);
        this.LogDebug("      is range?  = {0}", bc.IsRange);
        this.LogDebug("      is s rng?  = {0}", bc.IsStringRange);
        this.LogDebug("      is d rng?  = {0}", bc.IsDesignatorRange);
        this.LogDebug("      is abs?    = {0}", bc.IsAbsolute);
        if (bc.IsRange)
        {
          this.LogDebug("      usage min  = {0}", bc.UsageMin);
          this.LogDebug("      usage max  = {0}", bc.UsageMax);
          this.LogDebug("      string min = {0}", bc.StringMin);
          this.LogDebug("      string max = {0}", bc.StringMax);
          this.LogDebug("      d min      = {0}", bc.DesignatorMin);
          this.LogDebug("      d max      = {0}", bc.DesignatorMax);
          this.LogDebug("      data i min = {0}", bc.DataIndexMin);
          this.LogDebug("      data i max = {0}", bc.DataIndexMax);
        }
        else
        {
          this.LogDebug("      usage      = {0}", bc.UsageMin);
          this.LogDebug("      string ind = {0}", bc.StringMin);
          this.LogDebug("      desig ind  = {0}", bc.DesignatorMin);
          this.LogDebug("      data ind   = {0}", bc.DataIndexMin);
        }
      }
    }
  }
}