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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.TwinhanHidRemote
{
  /// <summary>
  /// A class for handling HID remote controls for Twinhan tuners, including clones from TerraTec,
  /// TechniSat and Digital Rise.
  /// </summary>
  public class TwinhanHidRemote : BaseCustomDevice, IRemoteControlListener
  {
    #region enums

    // http://uploads.bettershopping.eu/ART00784-1-web-big.jpg
    // variant: http://digitalnow.com.au/images/ProRemote.jpg (differences: home, second row, labels under bottom row)
    private enum TwinhanRemoteScanCodeNew
    {
      Power = 0,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine,
      Cancel,
      Clear,
      Zero,
      Favourites, // 13

      Up = 16,
      Down,
      Left,
      Right,
      Okay,
      Back,
      Tab,  // 22

      VolumeUp = 26,
      VolumeDown,
      ChannelDown,
      ChannelUp,

      Record = 64,
      Play,
      Pause,
      Stop,
      Rewind,
      FastForward,
      SkipBack, // 70
      SkipForward,
      Screenshot,
      Audio,              // text: SAP
      PictureInPicture,
      FullScreen,
      Subtitles,
      Mute,
      StereoMono,         // text: L/R
      Sleep,              // text: hibernate
      Source, // 80       // text: A/V
      Recall,
      ZoomIn,
      ZoomOut,
      Red,
      Green,
      Yellow,
      Blue, // 87

      Recordings = 92,    // text: record list
      Epg,                // text: info/EPG
      Preview,
      Teletext
    }

    // http://www.sadoun.com/Sat/Products/TwinHan/StarBox-2.jpg
    private enum TwinhanRemoteScanCodeOld
    {
      Tab = 0,
      Two,
      Down,               // overlay: channel down
      One,
      Recordings,         // text: record list
      Up,                 // overlay: channel up
      Three,  // 6

      Four = 9,
      Left = 10,          // overlay: volume down

      Cancel = 12,
      Seven,
      Recall,             // icon: circular arrow
      Teletext,
      Mute,
      Record,
      FastForward,  // 18

      Zero = 21,
      Power,
      Favourites,

      Eight = 25,
      Stop,
      Nine,
      Epg,
      Five,
      Right,              // overlay: volume up
      Six,  // 31

      Rewind = 64,
      Preview = 72,
      Pause = 76,         // text: time shift/pause
      FullScreen = 77,    // [q: German keyboard Y swapped with Z]
      Play = 79,          // overlay: okay
      Screenshot = 84     // text: capture
    }

    private enum TwinhanIoControlCode
    {
      CheckInterface = 121,

      HidRemoteControlEnable = 152,
      SetHidRemoteConfig = 153,
      GetHidRemoteConfig = 154
    }

    private enum TwinhanIrStandard : uint
    {
      Rc5 = 0,
      Nec
    }

    private enum TwinhanRemoteControlType
    {
      Old,
      New
    }

    private enum TwinhanRemoteControlMapping : uint
    {
      DtvDvb = 0,
      Cyberlink,
      InterVideo,
      Mce,
      DtvDvbWmInput,
      Custom,

      DigitalNow = 0x10,

      TerraTec110msRepeat = 0xfb,
      TerraTec220msRepeat,
      TerraTec110msSelectiveRepeat,   // selective = ch+/ch-/vol+/vol-/up/down/left/right
      TerraTec220msSelectiveRepeat,
      Disabled = 0xffff
    }

    [Flags]
    private enum UsageType
    {
      Keyboard = 0x1000000,
      Consumer = 0x2000000,
      Mce = 0x4000000,
      Raw = 0x8000000,
      Ascii = 0x10000000
    }

    private enum VirtualKeyModifier
    {
      Control = 0x10000,
      Shift = 0x20000,
      Alt = 0x40000
    }

    /// <summary>
    /// From USB HID usage tables.
    /// http://www.usb.org/developers/hidpage#HID_Usage
    /// http://www.usb.org/developers/devclass_docs/Hut1_12v2.pdf
    /// </summary>
    private enum HidUsagePage : ushort
    {
      Undefined = 0,
      GenericDesktopControl,
      SimulationControl,
      VirtualRealityControl,
      SportControl,
      GameControl,
      GenericDeviceControl,
      Keyboard,
      LightEmittingDiode,
      Button,
      Ordinal,
      Telephony,
      Consumer,
      Digitiser,

      PhysicalInterfaceDevice = 0x0f,
      Unicode = 0x10,
      AlphaNumericDisplay = 0x14,
      MedicalInstruments = 0x40,

      MonitorPage0 = 0x80,
      MonitorPage1,
      MonitorPage2,
      MonitorPage3,
      PowerPage0,
      PowerPage1,
      PowerPage2,
      PowerPage3,

      BarCodeScanner = 0x8c,
      Scale,
      MagneticStripeReader,
      ReservedPointOfSale,
      CameraControl,
      Arcade,

      // http://msdn.microsoft.com/en-us/library/windows/desktop/bb417079.aspx
      MceRemote = 0xffbc,
      TerraTecRemote = 0xffcc
    }

    // usage page 0x0c, usage 1
    private enum HidConsumerUsage
    {
      NumericKeyPad = 0x2,
      MediaSelectProgramGuide = 0x8d,
      ChannelIncrement = 0x9c,
      ChannelDecrement = 0x9d,

      Play = 0xb0,
      Pause,
      Record,
      FastForward,
      Rewind,
      ScanNextTrack,
      ScanPreviousTrack,
      Stop, // 0xb7

      Mute = 0xe2,
      VolumeIncrement = 0xe9,
      VolumeDecrement = 0xea,
      ApplicationControlProperties = 0x209
    }

    // usage page 0xffbc, usage 0x88
    // http://msdn.microsoft.com/en-us/library/windows/desktop/bb417079.aspx
    // http://download.microsoft.com/download/0/7/E/07EF37BB-33EA-4454-856A-7D663BC109DF/Windows-Media-Center-RC-IR-Collection-Green-Button-Specification-03-08-2011-V2.pdf
    private enum MceRemoteUsage
    {
      Undefined = 0,
      GreenButton = 0x0d,

      DvdMenu = 0x24,
      LiveTv,         // TV/jump

      Zoom = 0x27,
      Eject = 0x28,
      ClosedCaptioning = 0x2b,
      NetworkSelection = 0x2c,
      SubAudio = 0x2d,

      Ext0 = 0x32,
      Ext1,
      Ext2,
      Ext3,
      Ext4,
      Ext5,
      Ext6,
      Ext7,
      Ext8,

      Extras = 0x3c,
      ExtrasApp,
      Ten,
      Eleven,
      Twelve,
      ChannelInformation,
      ChannelInput,    // 3 digit input
      DvdTopMenu, // 0x43

      MyTv = 0x46,
      MyMusic,
      RecordedTv,
      MyPictures,
      MyVideos,
      DvdAngle,
      DvdAudio,
      DvdSubtitle,  // 0x4d

      Display = 0x4f,
      FmRadio = 0x50,

      TeletextOnOff = 0x5a,
      Red,
      Green,
      Yellow,
      Blue,

      Kiosk = 0x6a,
      Ext11 = 0x6f,

      BdTool = 0x78,

      Oem1 = 0x80,  // Ext9
      Oem2,         // Ext10
    }

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

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    private struct HidRemoteControlConfig   // RC_CONFIG
    {
      public TwinhanIrStandard IrStandard;
      public uint IrSysCodeCheck1;
      public uint IrSysCodeCheck2;
      public TwinhanRemoteControlMapping Mapping;
    }

    #endregion

    private class TwinhanHid
    {
      public IntPtr Handle = IntPtr.Zero;
      public NativeMethods.RawInputDeviceType Type = NativeMethods.RawInputDeviceType.RIM_TYPEKEYBOARD;
      public string Name = string.Empty;

      public HidUsagePage UsagePage = HidUsagePage.Consumer;
      public ushort Usage = 1;
      public IntPtr PreParsedData = IntPtr.Zero;

      public string ParentHidId = string.Empty;

      public void Dispose()
      {
        if (PreParsedData != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(PreParsedData);
          PreParsedData = IntPtr.Zero;
        }
      }
    }

    private class TwinhanHidDriver
    {
      #region constants

      private static readonly string[] TERRATEC_HID_DEVICE_NAMES = new string[]
      {
        "Cinergy_x_PCI_HID",
        "TT_x7_HID"
      };

      // GUID_THBDA_TUNER
      private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xe5644cc4, 0x17a1, 0x4eed, 0xbd, 0x90, 0x74, 0xfd, 0xa1, 0xd6, 0x54, 0x23);
      // GUID_THBDA_CMD
      private static readonly Guid COMMAND_GUID = new Guid(0x255e0082, 0x2017, 0x4b03, 0x90, 0xf8, 0x85, 0x6a, 0x62, 0xcb, 0x3d, 0x67);

      private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
      private const int COMMAND_SIZE = 40;

      private static readonly int HID_REMOTE_CONTROL_CONFIG_SIZE = Marshal.SizeOf(typeof(HidRemoteControlConfig));  // 16
      private static readonly Regex REGEX_SCANCODE_REGVAL = new Regex(@"^RC_Scancode_([0-9A-Fa-f]{2})$", RegexOptions.IgnoreCase);

      #endregion

      #region variables

      public string HidId = string.Empty;
      public string TunerId = string.Empty;
      private DsDevice TunerDevice = null;
      private IKsPropertySet TunerPropertySet = null;

      public TwinhanRemoteControlType RemoteControlType;
      public TwinhanRemoteControlMapping Mapping;
      public bool IsTerraTec = false;

      // virtual key combination => scan code
      public IDictionary<int, byte> CustomMapping = new Dictionary<int, byte>(64);

      #endregion

      public TwinhanHidDriver(string hidId, string tunerId)
      {
        HidId = hidId;
        TunerId = tunerId;
        try
        {
          FindTunerDevice();
          FindPropertySet();
          LoadConfig();
          LoadCustomMapping();
        }
        catch
        {
          Dispose();
          throw;
        }
        foreach (string terraTecId in TERRATEC_HID_DEVICE_NAMES)
        {
          if (HidId.ToLowerInvariant().Contains(terraTecId.ToLowerInvariant()))
          {
            IsTerraTec = true;
            break;
          }
        }
      }

      public void Enable()
      {
        IntPtr buffer = Marshal.AllocCoTaskMem(1);
        try
        {
          Marshal.WriteByte(buffer, 0, 1);
          int hr = SetIoctl(TwinhanIoControlCode.HidRemoteControlEnable, buffer, 1);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("Twinhan HID remote: failed to enable HID remote control, hr = 0x{0:x}", hr);
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(buffer);
        }
      }

      public void Disable()
      {
        IntPtr buffer = Marshal.AllocCoTaskMem(1);
        try
        {
          Marshal.WriteByte(buffer, 0, 0);
          int hr = SetIoctl(TwinhanIoControlCode.HidRemoteControlEnable, buffer, 1);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("Twinhan HID remote: failed to enable HID remote control, hr = 0x{0:x}", hr);
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(buffer);
        }
      }

      private void FindTunerDevice()
      {
        // Find the corresponding DsDevice. Note multi-tuner hardware will have
        // multiple matching devices. Any would be sufficient for our purposes.
        string tunerId = TunerId.ToLowerInvariant().Replace('\\', '#');
        DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
        foreach (DsDevice d in devices)
        {
          string devicePath = d.DevicePath;
          if (TunerDevice == null && devicePath != null && devicePath.Contains(tunerId))
          {
            TunerDevice = d;
          }
          else
          {
            d.Dispose();
          }
        }
        if (TunerDevice == null)
        {
          throw new TvException("Failed to find tuner device for ID {0}.", TunerId);
        }
      }

      private void FindPropertySet()
      {
        Guid filterClsid = typeof(IBaseFilter).GUID;
        object obj;
        try
        {
          TunerDevice.Mon.BindToObject(null, null, ref filterClsid, out obj);
        }
        catch (Exception ex)
        {
          throw new TvException("Failed to create tuner filter.", ex);
        }

        TunerPropertySet = obj as IKsPropertySet;
        if (TunerPropertySet == null)
        {
          Release.ComObject("Twinhan HID remote tuner filter", ref obj);
          throw new TvException("Tuner filter is not a property set.");
        }
        int hr = SetIoctl(TwinhanIoControlCode.CheckInterface, IntPtr.Zero, 0);
        if (hr != (int)HResult.Severity.Success)
        {
          Release.ComObject("Twinhan HID remote tuner filter", ref TunerPropertySet);
          throw new TvException("Property set not supported on tuner filter, hr = 0x{0:x}.", hr);
        }
      }

      private void LoadConfig()
      {
        IntPtr buffer = Marshal.AllocCoTaskMem(HID_REMOTE_CONTROL_CONFIG_SIZE);
        try
        {
          for (int i = 0; i < HID_REMOTE_CONTROL_CONFIG_SIZE; i++)
          {
            Marshal.WriteByte(buffer, i, 0);
          }
          int returnedByteCount;
          int hr = GetIoctl(TwinhanIoControlCode.GetHidRemoteConfig, buffer, HID_REMOTE_CONTROL_CONFIG_SIZE, out returnedByteCount);
          // The Mantis driver does not return the number of bytes populated,
          // so we can only check the HRESULT.
          if (hr != (int)HResult.Severity.Success)  // || returnedByteCount != HID_REMOTE_CONTROL_CONFIG_SIZE)
          {
            this.LogWarn("Twinhan HID remote: failed to read HID config, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
            RemoteControlType = TwinhanRemoteControlType.Old;
            Mapping = TwinhanRemoteControlMapping.DtvDvb;
          }
          else
          {
            HidRemoteControlConfig config = (HidRemoteControlConfig)Marshal.PtrToStructure(buffer, typeof(HidRemoteControlConfig));
            if (config.IrSysCodeCheck1 == 0x1b)
            {
              // DigitalNow QuattroS remote
              RemoteControlType = TwinhanRemoteControlType.New;
            }
            else if (config.IrSysCodeCheck1 == 0xff00)
            {
              RemoteControlType = TwinhanRemoteControlType.Old;
            }
            /*else if (config.IrSysCodeCheck1 == 0xeb14)
            {
              // TerraTec Cinergy remote - scan code mapping not known
            }
            else if (config.IrSysCodeCheck1 == 0x04eb)
            {
              // TerraTec x7 remote - scan code mapping not known
            }
            else if (config.IrSysCodeCheck1 == 0x0af5)
            {
              // TechniSat USB remote - scan code mapping not known
            }*/
            else
            {
              this.LogWarn("Twinhan HID remote: unrecognised IRSYSCODECHECK value 0x{0:x}, defaulting to old remote control type", config.IrSysCodeCheck1);
              RemoteControlType = TwinhanRemoteControlType.Old;
            }
            Mapping = config.Mapping;
            if (!Enum.IsDefined(typeof(TwinhanRemoteControlMapping), config.Mapping))
            {
              this.LogWarn("Twinhan HID remote: unrecognised RC_Configuration value {0}", config.Mapping);
            }
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(buffer);
        }
      }

      private void LoadCustomMapping()
      {
        RegistryView view = RegistryView.Registry64;
        if (!OSInfo.OSInfo.Is64BitOs())
        {
          view = RegistryView.Registry32;
        }

        // Read the HID properties.
        using (RegistryKey key1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}"))
        {
          if (key1 != null)
          {
            int skCount = key1.SubKeyCount;
            int checkedCount = 0;
            int i = 0;
            // Find the correct HID driver.
            while (checkedCount < skCount)
            {
              using (RegistryKey key2 = key1.OpenSubKey(string.Format("{0:D4}", i++)))
              {
                // HidId looks like AVSTREAM\AZUREWAVEPCIHID.VIRTUAL\5&1560A6BC&1&0.
                // MatchingDeviceId looks like avstream\azurewavepcihid.virtual.
                object deviceId = key2.GetValue("MatchingDeviceId");
                if (deviceId != null && HidId.ToLowerInvariant().Contains(deviceId.ToString()))
                {
                  using (RegistryKey key3 = key2.OpenSubKey("DriverData"))
                  {
                    if (key3 != null)
                    {
                      foreach (string name in key3.GetValueNames())
                      {
                        if (name.Equals("RC_Report"))
                        {
                          if ((int)key3.GetValue(name) == 0)
                          {
                            // When RC_Report is 0 the virtual HID devices won't
                            // be loaded by the driver; only the HID keyboard
                            // will be loaded. This means any scan codes which
                            // are mapped to MCE, consumer, raw or ASCII HID
                            // actions/codes won't work. Changing the value
                            // would not have any effect until the driver is
                            // reloaded (eg. reboot).
                            this.LogWarn("Twinhan HID remote: RC_Report is 0, buttons mapped to MCE, consumer, raw or ASCII HID won't work");
                          }
                        }
                        else
                        {
                          Match m = REGEX_SCANCODE_REGVAL.Match(name);
                          if (m.Success)
                          {
                            byte scancode = Convert.ToByte(m.Groups[1].Captures[0].Value, 16);
                            int value = (int)key3.GetValue(name);
                            int key = (int)UsageType.Keyboard | (value & 0xff);
                            if ((value & 0xff00) != 0)
                            {
                              key |= (int)VirtualKeyModifier.Control;
                            }
                            if ((value & 0xff0000) != 0)
                            {
                              key |= (int)VirtualKeyModifier.Shift;
                            }
                            if ((value & 0xff000000) != 0)
                            {
                              key |= (int)VirtualKeyModifier.Alt;
                            }
                            CustomMapping[key] = scancode;
                          }
                        }
                      }
                    }
                  }
                  break;
                }
              }
            }
          }
        }
      }

      #region IOCTL

      private int SetIoctl(TwinhanIoControlCode controlCode, IntPtr inBuffer, int inBufferSize)
      {
        int returnedByteCount;
        return ExecuteIoctl(controlCode, inBuffer, inBufferSize, IntPtr.Zero, 0, out returnedByteCount);
      }

      private int GetIoctl(TwinhanIoControlCode controlCode, IntPtr outBuffer, int outBufferSize, out int returnedByteCount)
      {
        return ExecuteIoctl(controlCode, IntPtr.Zero, 0, outBuffer, outBufferSize, out returnedByteCount);
      }

      private int ExecuteIoctl(TwinhanIoControlCode controlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int returnedByteCount)
      {
        returnedByteCount = 0;
        int hr = (int)HResult.Severity.Error;
        if (TunerPropertySet == null)
        {
          this.LogError("Twinhan HID remote: attempted to execute IOCTL when property set is NULL");
          return hr;
        }

        IntPtr instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
        IntPtr commandBuffer = Marshal.AllocCoTaskMem(COMMAND_SIZE);
        IntPtr returnedByteCountBuffer = Marshal.AllocCoTaskMem(sizeof(int));
        try
        {
          // Clear buffers. This is probably not actually needed, but better
          // to be safe than sorry!
          for (int i = 0; i < INSTANCE_SIZE; i++)
          {
            Marshal.WriteByte(instanceBuffer, i, 0);
          }
          Marshal.WriteInt32(returnedByteCountBuffer, 0);

          // Fill the command buffer.
          Marshal.Copy(COMMAND_GUID.ToByteArray(), 0, commandBuffer, 16);
          Marshal.WriteInt32(commandBuffer, 16, (int)NativeMethods.CTL_CODE((NativeMethods.FileDevice)0xaa00, (uint)controlCode, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_ANY_ACCESS));
          Marshal.WriteInt32(commandBuffer, 20, inBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 24, inBufferSize);
          Marshal.WriteInt32(commandBuffer, 28, outBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 32, outBufferSize);
          Marshal.WriteInt32(commandBuffer, 36, returnedByteCountBuffer.ToInt32());

          hr = TunerPropertySet.Set(BDA_EXTENSION_PROPERTY_SET, 0, instanceBuffer, INSTANCE_SIZE, commandBuffer, COMMAND_SIZE);
          if (hr == (int)HResult.Severity.Success)
          {
            returnedByteCount = Marshal.ReadInt32(returnedByteCountBuffer);
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(instanceBuffer);
          Marshal.FreeCoTaskMem(commandBuffer);
          Marshal.FreeCoTaskMem(returnedByteCountBuffer);
        }
        return hr;
      }

      #endregion

      public void Dispose()
      {
        if (TunerDevice != null)
        {
          TunerDevice.Dispose();
          TunerDevice = null;
        }
        Release.ComObject("Twinhan HID remote property set", ref TunerPropertySet);
      }
    }

    #region constants

    private static readonly string[] VALID_HID_DEVICE_NAMES = new string[]
    {
      // DigitalTV 3.4 driver cache
      //----------------------------------
      // These drivers use the standard Twinhan mapping tables.

      // Mantis: 1041 [AD-SP400 DVB-S2 PCI], 2040 [AD-CP400 DVB-C PCI], generic (TechniSat, TerraTec)
      "AzureWavePciHID",    

      // Vast array of DVB-C/T or DVB-S/S2 USB 2.0 designs, including TechniSat, TerraTec and Elgato clones...
      // 14f7:0001/2 [TechniSat SkyStar USB 2 HD CI r1/2]
      // 14f7:0003 [TechniSat CableStar Combo HD CI]
      // 0fd9:0025/2a/36 [Elgato EyeTV Sat CI]
      // 0fd9:0026 [6007 Elgato]
      // 0ccd:00a6 [6017 TerraTec]
      // 0ccd:00a9 [6027 TerraTec]
      // 13d3:3209/29/44/45/46/47/68/75 [AzureWave/Twinhan???]
      // 13d3:3285 [6033 DVB-S2]
      // 13d3:3290 [6011 Siliconlab DVB-C/T]
      // 13d3:3292 [6035 wireless DVB-S]
      // 13d3:3343 [6038 DVB-S2]
      // 13d3:3245/6 [7050 AD-SB300 DVB-S2 USB 2.0]
      "AzureWaveUsbHid",
      "UDST7000HID",          // (alternate driver for most of the above)

      // Vast array of hybrid ATSC/analog or DVB-T/analog USB 2.0 designs
      // 0543:1e60 [ViewSonic TA310 ATSC/analog]
      // 13d3:3212/3/4/5
      // 13d3:3239/42
      // 6000:0001 [Beholder]
      "UDXTTM6000HID",
      // 0ccd:0086 [TerraTec Hybrid XE]
      // 13d3:3235/40/41/43/64/67 [Twinhan TU501]
      // 2040:6600/10 [Hauppauge HVR-900H]
      // 5654:5254 [GOTVIEW USB 2.0 Hybrid MasterStick]
      // 6000:0002 [Beholder]
      "UDXTTM6010HID",

      // 04b4:8613/4, 13D3:3203/4/7/8 [7020/1A Twinhan/DigitalRise/TYPHOON StarBox DVB-S USB 2.0]
      "UDST7021HID",

      // 13D3:3219 [7049A DVB-T USB]
      // 1498:9206 [Dposh DVB-T USB]
      "UDTT7049HID",

      // 13D3:3205/6 [DigitalNow tinyUSB2 DVB-T]
      "UDTT2HID",

      // SAA7133: Tiger reference, 3056 [AD-TP500 DVB-T/analog PCI], 3252 [AD-AP500/TU500 ATSC/analog PCI]
      "3056HID",
      "3xHID",

      // SAA7160: 1028 [AD-SE200 DVB-S PCIe], 3071
      // SAA7162: 6050/1 [dual PCIe], 6090/1 [2xDVB-S, 2xDVB-T/analog], DigitalNow DNTV Live! QuattroS [2xDVB-S, 2xDVB-T/analog]
      "716xHID",

      // VP-7243 ATSC
      "7243HID",


      // TechniSat drivers
      //----------------------------------
      // These drivers use the standard Twinhan mapping tables.

      // TechniSat SkyStar HD2 (generic Mantis)
      "MtsHID",


      // TerraTec drivers
      //----------------------------------
      // These drivers do NOT use the standard Twinhan mapping tables.

      // Cinergy C PCI HD, Cinergy S2 PCI HD
      "Cinergy_x_PCI_HID",

      // 0ccd:10a3/b4 [TerraTec H7 r1/2/3]
      // 0ccd:10a4/ac/b0 [TerraTec S7 r1/2/3]
      "TT_x7_HID"
    };

    #region mappings

    private static readonly IDictionary<int, byte> MAPPING_DTV_DVB = new Dictionary<int, byte>(62)
    {
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,                                                             0x00 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                                                               0x01 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_NEXT,                                                            0x02 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                                                               0x03 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_L,                                                               0x04 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_PRIOR,                                                           0x05 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                                                               0x06 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)VirtualKeyModifier.Control,                             0x07 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x08 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                                                               0x09 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN | (int)VirtualKeyModifier.Shift,                            0x0a },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_ESCAPE,                                                          0x0c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                                                               0x0d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_C,                                                               0x0e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_A,                                                               0x0f },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_M,                                                               0x10 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_HOME,                                                            0x11 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_N,                                                               0x12 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,                                                            0x13 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_N | (int)VirtualKeyModifier.Control,                             0x14 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                                                               0x15 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F6 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt), 0x16 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_V,                                                               0x17 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Shift,                               0x18 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                                                               0x19 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_END,                                                             0x1a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                                                               0x1b },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_E,                                                               0x1c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                                                               0x1d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP | (int)VirtualKeyModifier.Shift,                              0x1e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                                                               0x1f },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_I,                                                               0x40 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                             0x41 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_K | (int)VirtualKeyModifier.Control,                             0x42 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T | (int)VirtualKeyModifier.Control,                             0x43 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_OEM_PLUS | (int)VirtualKeyModifier.Control,                      0x45 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_OEM_MINUS | (int)VirtualKeyModifier.Control,                     0x46 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)VirtualKeyModifier.Control,                             0x47 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_K,                                                               0x48 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_V | (int)VirtualKeyModifier.Control,                             0x49 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DELETE,                                                          0x4a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                                                              0x4b },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T,                                                               0x4c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z,                                                               0x4d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,                                                            0x4e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,                                                          0x4f },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_L | (int)VirtualKeyModifier.Control,                             0x50 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,                                                            0x51 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,                                                           0x52 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_G | (int)VirtualKeyModifier.Shift,                               0x53 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P,                                                               0x54 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x55 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x56 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_M | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x57 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x58 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x59 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_H | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5b },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_V | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Y | (int)VirtualKeyModifier.Shift,                               0x5e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_B | (int)VirtualKeyModifier.Shift,                               0x5f }
    };

    private static readonly IDictionary<int, byte> MAPPING_CYBERLINK = new Dictionary<int, byte>(51)
    {
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,               0x00 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                 0x01 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ChannelDecrement,             0x02 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                 0x03 },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.ChannelIncrement,             0x05 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                 0x06 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                 0x09 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_ESCAPE,            0x0c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                 0x0d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_L | (int)VirtualKeyModifier.Control, 0x0e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x0f },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Record,                       0x11 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.FastForward,                  0x12 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,              0x13 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Play,                         0x14 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                 0x15 },
      { (int)UsageType.Raw      | 0x16,                                               0x16 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                 0x19 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Stop,                         0x1a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                 0x1b },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_G | (int)VirtualKeyModifier.Control, 0x1c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                 0x1d },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                 0x1f },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.Rewind,                       0x40 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x43 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_PRIOR,             0x45 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_NEXT,              0x46 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DELETE,            0x4a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                0x4b },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Pause,                        0x4c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN | (int)VirtualKeyModifier.Alt, 0x4d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,              0x4e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,            0x4f },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_C | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x50 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,              0x51 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,             0x52 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x54 },
      { (int)UsageType.Raw      | 0x55,                                               0x55 },
      { (int)UsageType.Raw      | 0x56,                                               0x56 },
      { (int)UsageType.Raw      | 0x57,                                               0x57 },

      { (int)UsageType.Raw      | 0x59,                                               0x59 },
      { (int)UsageType.Mce      | (int)MceRemoteUsage.Undefined,                      0x5a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_HOME | (int)VirtualKeyModifier.Alt, 0x5b },
      { (int)UsageType.Raw      | 0x5c,                                               0x5c },
      { (int)UsageType.Raw      | 0x5d,                                               0x5d }
    };

    private static readonly IDictionary<int, byte> MAPPING_INTERVIDEO = new Dictionary<int, byte>(30)
    {
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,               0x00 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                 0x01 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_NEXT,              0x02 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                 0x03 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_PRIOR,             0x05 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                 0x06 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                 0x09 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                 0x0d },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                               0x11 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift),  0x12 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)VirtualKeyModifier.Control,                               0x14 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                 0x15 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F4 | (int)VirtualKeyModifier.Alt,                                  0x16 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                 0x19 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)VirtualKeyModifier.Control,                               0x1a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                 0x1b },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                 0x1d },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                 0x1f },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,            0x4f },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x55 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x57 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x59 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_HOME | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),  0x5b },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x5c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x5d }
    };

    private static readonly IDictionary<int, byte> MAPPING_MCE = new Dictionary<int, byte>(44)
    {
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                 0x01 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ChannelDecrement,             0x02 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                 0x03 },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.ChannelIncrement,             0x05 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                 0x06 },

      { (int)UsageType.Mce      | (int)MceRemoteUsage.DvdAudio,                       0x08 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                 0x09 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                 0x0d },
      { (int)UsageType.Mce      | (int)MceRemoteUsage.DvdAngle,                       0x0e },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Record,                       0x11 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.FastForward,                  0x12 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,              0x13 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Play,                         0x14 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                 0x15 },
      { (int)UsageType.Raw      | 0x16,                                               0x16 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                 0x19 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Stop,                         0x1a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                 0x1b },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                 0x1d },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                 0x1f },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.Rewind,                       0x40 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },
      { (int)UsageType.Raw      | 0x43,                                               0x43 },

      { (int)UsageType.Mce      | (int)MceRemoteUsage.RecordedTv,                     0x47 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_ESCAPE,            0x4a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                0x4b },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Pause,                        0x4c },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.MediaSelectProgramGuide,      0x4d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,              0x4e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,            0x4f },
      { (int)UsageType.Mce      | (int)MceRemoteUsage.LiveTv,                         0x50 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,              0x51 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,             0x52 },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.ApplicationControlProperties, 0x54 },
      { (int)UsageType.Raw      | 0x55,                                               0x55 },

      { (int)UsageType.Raw      | 0x57,                                               0x57 },

      { (int)UsageType.Raw      | 0x59,                                               0x59 },

      { (int)UsageType.Raw      | 0x5c,                                               0x5c },
      { (int)UsageType.Raw      | 0x5d,                                               0x5d }
    };

    private static readonly IDictionary<int, byte> MAPPING_DTV_DVB_WM_INPUT = new Dictionary<int, byte>(62)
    {
      { (int)UsageType.Ascii    | 0x3a,                                               0x00 },
      { (int)UsageType.Ascii    | 0x32,                                               0x01 },
      { (int)UsageType.Raw      | 0x02,                                               0x02 },
      { (int)UsageType.Ascii    | 0x31,                                               0x03 },
      { (int)UsageType.Ascii    | 0x0d,                                               0x04 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ChannelIncrement,             0x05 },
      { (int)UsageType.Ascii    | 0x33,                                               0x06 },

      { (int)UsageType.Mce      | (int)MceRemoteUsage.DvdAudio,                       0x08 },
      { (int)UsageType.Ascii    | 0x34,                                               0x09 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)UsageType.Ascii    | 0x41,                                               0x0c },
      { (int)UsageType.Ascii    | 0x37,                                               0x0d },
      { (int)UsageType.Mce      | (int)MceRemoteUsage.DvdAngle,                       0x0e },
      { (int)UsageType.Ascii    | 0x0f,                                               0x0f },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Record,                       0x11 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.FastForward,                  0x12 },
      { (int)UsageType.Ascii    | 0x43,                                               0x13 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Play,                         0x14 },
      { (int)UsageType.Ascii    | 0x30,                                               0x15 },
      { (int)UsageType.Ascii    | 0x13,                                               0x16 },
      { (int)UsageType.Ascii    | 0x0c,                                               0x17 },
      { (int)UsageType.Raw      | 0x18,                                               0x18 },
      { (int)UsageType.Ascii    | 0x38,                                               0x19 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Stop,                         0x1a },
      { (int)UsageType.Ascii    | 0x39,                                               0x1b },
      { (int)UsageType.Ascii    | 0x11,                                               0x1c },
      { (int)UsageType.Ascii    | 0x35,                                               0x1d },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)UsageType.Ascii    | 0x36,                                               0x1f },

      { (int)UsageType.Consumer | (int)HidConsumerUsage.Rewind,                       0x40 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },
      { (int)UsageType.Raw      | 0x43,                                               0x43 },

      { (int)UsageType.Ascii    | 0x07,                                               0x45 },
      { (int)UsageType.Ascii    | 0x0a,                                               0x46 },
      { (int)UsageType.Mce      | (int)MceRemoteUsage.RecordedTv,                     0x47 },
      { (int)UsageType.Ascii    | 0x0e,                                               0x48 },
      { (int)UsageType.Ascii    | 0x0b,                                               0x49 },
      { (int)UsageType.Ascii    | 0x3b,                                               0x4a },
      { (int)UsageType.Ascii    | 0x3c,                                               0x4b },
      { (int)UsageType.Consumer | (int)HidConsumerUsage.Pause,                        0x4c },
      { (int)UsageType.Ascii    | 0x06,                                               0x4d },
      { (int)UsageType.Ascii    | 0x3f,                                               0x4e },
      { (int)UsageType.Ascii    | 0x42,                                               0x4f },
      { (int)UsageType.Ascii    | 0x05,                                               0x50 },
      { (int)UsageType.Ascii    | 0x3d,                                               0x51 },
      { (int)UsageType.Ascii    | 0x3e,                                               0x52 },
      { (int)UsageType.Raw      | 0x53,                                               0x53 },
      { (int)UsageType.Ascii    | 0x03,                                               0x54 },
      { (int)UsageType.Raw      | 0x55,                                               0x55 },
      { (int)UsageType.Raw      | 0x56,                                               0x56 },
      { (int)UsageType.Raw      | 0x57,                                               0x57 },
      { (int)UsageType.Ascii    | 0x12,                                               0x58 },
      { (int)UsageType.Raw      | 0x59,                                               0x59 },
      { (int)UsageType.Mce      | (int)MceRemoteUsage.Undefined,                      0x5a },
      { (int)UsageType.Raw      | 0x5b,                                               0x5b },
      { (int)UsageType.Raw      | 0x5c,                                               0x5c },
      { (int)UsageType.Raw      | 0x5d,                                               0x5d },
      { (int)UsageType.Raw      | 0x5e,                                               0x5e },
      { (int)UsageType.Raw      | 0x5f,                                               0x5f }
    };

    private static readonly IDictionary<int, byte> MAPPING_DNTV = new Dictionary<int, byte>(48)
    {
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,                                                             0x00 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                                                               0x01 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x02 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                                                               0x03 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_U | (int)VirtualKeyModifier.Control,                             0x05 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                                                               0x06 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                                                               0x09 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_X | (int)VirtualKeyModifier.Control,                             0x0a },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F4 | (int)VirtualKeyModifier.Alt,                                0x0c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                                                               0x0d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_B | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x0e },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Y | (int)VirtualKeyModifier.Control,                             0x10 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                             0x11 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x12 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,                                                            0x13 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)VirtualKeyModifier.Control,                             0x14 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                                                               0x15 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_H | (int)VirtualKeyModifier.Control,                             0x17 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                                                               0x19 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)VirtualKeyModifier.Control,                             0x1a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                                                               0x1b },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_C | (int)VirtualKeyModifier.Control,                             0x1c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                                                               0x1d },
      // 0x1e is mapped to the same as 0x05 and 0x42
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_U | (int)VirtualKeyModifier.Control,                             0x1e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                                                               0x1f },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x40 },
      // 0x40 is mapped to the same as 0x02 and 0x56
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x41 },
      // 0x42 is mapped to the same as 0x05 and 0x1e
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_U | (int)VirtualKeyModifier.Control,                             0x42 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x45 },
      // 0x46 is mapped to the same as 0x45
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x46 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F6,                                                              0x47 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_O | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x48 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DELETE,                                                          0x4a },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                                                              0x4b },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_E | (int)VirtualKeyModifier.Control, 0x4c },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_E | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x4d },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,                                                            0x4e },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,                                                          0x4f },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F8,                                                              0x50 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,                                                            0x51 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,                                                           0x52 },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F7,                                                              0x54 },
      // 0x55 is mapped to the same as 0x17
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_H | (int)VirtualKeyModifier.Control,                             0x55 },
      // 0x56 is mapped to the same as 0x02 and 0x41
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x56 },
      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_J | (int)VirtualKeyModifier.Control,                             0x57 },

      // 0x59 is mapped to the same as 0x11
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                             0x59 },

      // 0x5b is mapped to the same as 0x0a
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_X | (int)VirtualKeyModifier.Control,                             0x5b },

      { (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)VirtualKeyModifier.Control,                             0x5d }
    };

    #endregion

    private static readonly int HID_INPUT_DATA_OFFSET = Marshal.OffsetOf(typeof(NativeMethods.RAWINPUT), "data").ToInt32() + Marshal.SizeOf(typeof(NativeMethods.RAWHID));

    private static readonly Regex REGEX_HID_DEVICE_NAME_TO_ID = new Regex(@"^\\\?\?\\([^\{]*)\#\{");

    #endregion

    #region variables

    // This plugin is not tuner-specific. We use this variable to restrict to
    // one instance.
    private static bool _isLoaded = false;
    private static object _instanceLock = new object();

    private bool _isTwinhanHidRemote = false;

    private IDictionary<IntPtr, TwinhanHid> _devices = null;
    private IDictionary<string, TwinhanHidDriver> _drivers = null;
    private VirtualKeyModifier _modifiers = 0;

    private bool _isRemoteControlInterfaceOpen = false;
    private uint _remoteControlListenerThreadId = 0;
    private Thread _remoteControlListenerThread = null;

    #endregion

    #region loading

    private void FindHids(out IDictionary<IntPtr, TwinhanHid> devices, out IDictionary<string, TwinhanHidDriver> drivers)
    {
      devices = new Dictionary<IntPtr, TwinhanHid>(15);
      drivers = new Dictionary<string, TwinhanHidDriver>(4);

      // Get the device list size.
      int hr;
      uint deviceCount = 0;
      uint listDeviceSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICELIST));
      int result = NativeMethods.GetRawInputDeviceList(null, ref deviceCount, listDeviceSize);
      if (result != 0)
      {
        hr = Marshal.GetLastWin32Error();
        this.LogError("Twinhan HID remote: failed to get raw input device list size, result = {0}, hr = 0x{1:x}", result, hr);
        return;
      }
      this.LogDebug("Twinhan HID remote: raw input device count = {0}", deviceCount);

      // Get the device list.
      NativeMethods.RAWINPUTDEVICELIST[] deviceList = new NativeMethods.RAWINPUTDEVICELIST[deviceCount];
      uint deviceInfoSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RID_DEVICE_INFO));
      IntPtr deviceInfo = Marshal.AllocHGlobal((int)deviceInfoSize);
      try
      {
        result = NativeMethods.GetRawInputDeviceList(deviceList, ref deviceCount, listDeviceSize);
        if (result == -1)
        {
          hr = Marshal.GetLastWin32Error();
          this.LogError("Twinhan HID remote: failed to get raw input device list, result = {0}, hr = 0x{1:x}", result, hr);
          return;
        }

        // For each device...
        foreach (NativeMethods.RAWINPUTDEVICELIST d in deviceList)
        {
          string name = ReadRawDeviceName(d.hDevice);
          this.LogDebug("Twinhan HID remote: {0} {1}", d.dwType, name);

          // Do we recognise the name?
          bool found = false;
          foreach (string hidName in VALID_HID_DEVICE_NAMES)
          {
            if (name.Contains(hidName))
            {
              found = true;
              break;
            }
          }
          if (!found)
          {
            continue;
          }

          string parentHidId;
          string parentTunerId;
          if (!FindParents(name, out parentHidId, out parentTunerId))
          {
            this.LogError("Twinhan HID remote: failed to find parents for HID {0}", name);
            continue;
          }
          this.LogDebug("  HID ID       = {0}", parentHidId);
          this.LogDebug("  tuner ID     = {0}", parentTunerId);

          TwinhanHidDriver driver = null;
          if (!drivers.TryGetValue(parentHidId, out driver))
          {
            try
            {
              driver = new TwinhanHidDriver(parentHidId, parentTunerId);
              drivers.Add(parentHidId, driver);
            }
            catch (Exception ex)
            {
              this.LogError(ex, "Twinhan HID remote: failed to load tuner/driver information, HID ID = {0}, tuner ID = {1}", parentHidId, parentTunerId);
              continue;
            }
          }
          this.LogDebug("  RC type      = {0}", driver.RemoteControlType);
          this.LogDebug("  mapping      = {0}", driver.Mapping);

          NativeMethods.RID_DEVICE_INFO info = new NativeMethods.RID_DEVICE_INFO();
          info.cbSize = deviceInfoSize;
          Marshal.StructureToPtr(info, deviceInfo, false);
          result = NativeMethods.GetRawInputDeviceInfoW(d.hDevice, NativeMethods.RawInputInfoCommand.RIDI_DEVICEINFO, deviceInfo, ref deviceInfoSize);
          if (result <= 0)
          {
            hr = Marshal.GetLastWin32Error();
            this.LogWarn("Twinhan HID remote: failed to get raw input device info, result = {0}, hr = 0x{1:x}, required size = {2}", result, hr, deviceInfoSize);
            continue;
          }

          IntPtr ppData = IntPtr.Zero;
          if (d.dwType == NativeMethods.RawInputDeviceType.RIM_TYPEHID)
          {
            ppData = GetPreParsedData(d.hDevice);
            if (ppData == IntPtr.Zero)
            {
              continue;
            }
          }

          TwinhanHid device = new TwinhanHid();
          device.Handle = d.hDevice;
          device.Type = d.dwType;
          device.Name = name;
          device.ParentHidId = parentHidId;
          device.PreParsedData = ppData;

          info = (NativeMethods.RID_DEVICE_INFO)Marshal.PtrToStructure(deviceInfo, typeof(NativeMethods.RID_DEVICE_INFO));
          if (d.dwType == NativeMethods.RawInputDeviceType.RIM_TYPEMOUSE)
          {
            this.LogDebug("  ID           = {0}", info.mouse.dwId);
            this.LogDebug("  # buttons    = {0}", info.mouse.dwNumberOfButtons);
            this.LogDebug("  sample rate  = {0}", info.mouse.dwSampleRate);
            this.LogDebug("  hor. wheel?  = {0}", info.mouse.fHasHorizontalWheel);
            device.UsagePage = HidUsagePage.GenericDesktopControl;
            device.Usage = 2;
          }
          else if (d.dwType == NativeMethods.RawInputDeviceType.RIM_TYPEKEYBOARD)
          {
            this.LogDebug("  KB type      = {0}", (KeyboardType)info.keyboard.dwType);
            this.LogDebug("  sub-type     = {0}", info.keyboard.dwSubType);
            this.LogDebug("  mode         = {0}", info.keyboard.dwKeyboardMode);
            this.LogDebug("  # func. keys = {0}", info.keyboard.dwNumberOfFunctionKeys);
            this.LogDebug("  # indicators = {0}", info.keyboard.dwNumberOfIndicators);
            this.LogDebug("  # keys total = {0}", info.keyboard.dwNumberOfKeysTotal);
            device.UsagePage = HidUsagePage.GenericDesktopControl;
            device.Usage = 6;
          }
          else if (d.dwType == NativeMethods.RawInputDeviceType.RIM_TYPEHID)
          {
            HidUsagePage p = (HidUsagePage)info.hid.usUsagePage;
            this.LogDebug("  vendor ID    = 0x{0:x}", info.hid.dwVendorId);
            this.LogDebug("  product ID   = 0x{0:x}", info.hid.dwProductId);
            this.LogDebug("  version      = 0x{0:x}", info.hid.dwVersionNumber);
            this.LogDebug("  usage page   = {0}", p);
            this.LogDebug("  usage        = 0x{0:x2}", info.hid.usUsage);
            device.UsagePage = p;
            device.Usage = info.hid.usUsage;
            DebugCapabilities(device.PreParsedData);
          }
          else
          {
            this.LogWarn("Twinhan HID remote: unrecognised raw input device type {0}", d.dwType);
          }

          devices.Add(d.hDevice, device);
        }
      }
      finally
      {
        Marshal.FreeHGlobal(deviceInfo);
      }
    }

    private string ReadRawDeviceName(IntPtr device)
    {
      int hr;
      uint deviceNameSize = 256;
      int result = NativeMethods.GetRawInputDeviceInfoW(device, NativeMethods.RawInputInfoCommand.RIDI_DEVICENAME, IntPtr.Zero, ref deviceNameSize);
      if (result != 0)
      {
        hr = Marshal.GetLastWin32Error();
        this.LogError("Twinhan HID remote: failed to get raw input device name size, result = {0}, hr = 0x{1:x}", result, hr);
        return string.Empty;
      }

      IntPtr deviceName = Marshal.AllocHGlobal((int)deviceNameSize * 2);  // size is the character count not byte count
      try
      {
        result = NativeMethods.GetRawInputDeviceInfoW(device, NativeMethods.RawInputInfoCommand.RIDI_DEVICENAME, deviceName, ref deviceNameSize);
        if (result > 0)
        {
          return Marshal.PtrToStringUni(deviceName, result - 1); // -1 for NULL termination
        }

        hr = Marshal.GetLastWin32Error();
        this.LogError("Twinhan HID remote: failed to get raw input device name, result = {0}, hr = 0x{1:x}", result, hr);
        return string.Empty;
      }
      finally
      {
        Marshal.FreeHGlobal(deviceName);
      }
    }

    private IntPtr GetPreParsedData(IntPtr device)
    {
      int hr;
      uint ppDataSize = 256;
      int result = NativeMethods.GetRawInputDeviceInfoW(device, NativeMethods.RawInputInfoCommand.RIDI_PREPARSEDDATA, IntPtr.Zero, ref ppDataSize);
      if (result != 0)
      {
        hr = Marshal.GetLastWin32Error();
        this.LogError("Twinhan HID remote: failed to get raw input pre-parsed data size, result = {0}, hr = 0x{1:x}", result, hr);
        return IntPtr.Zero;
      }

      IntPtr ppData = Marshal.AllocHGlobal((int)ppDataSize);
      result = NativeMethods.GetRawInputDeviceInfoW(device, NativeMethods.RawInputInfoCommand.RIDI_PREPARSEDDATA, ppData, ref ppDataSize);
      if (result <= 0)
      {
        hr = Marshal.GetLastWin32Error();
        this.LogError("Twinhan HID remote: failed to get raw input pre-parsed data, result = {0}, hr = 0x{1:x}", result, hr);
        return IntPtr.Zero;
      }
      return ppData;
    }

    private bool FindParents(string deviceName, out string hidId, out string tunerId)
    {
      hidId = string.Empty;
      tunerId = string.Empty;

      // Convert the device name into an ID.
      // \??\HID#AzureWavePciHID.VIRTUAL&Col02#6&32a2cb67&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030}
      // HID\AZUREWAVEPCIHID.VIRTUAL&COL02\6&32A2CB67&0&0001
      Match m = REGEX_HID_DEVICE_NAME_TO_ID.Match(deviceName);
      if (!m.Success)
      {
        this.LogError("Twinhan HID remote: failed to find parents, can't derive device ID from name {0}", deviceName);
        return false;
      }
      string targetDeviceId = m.Groups[1].Captures[0].Value.Replace('#', '\\').ToUpperInvariant();

      // Enumerate installed and present devices. We're looking for devices in
      // the HID and keyboard categories.
      IntPtr devInfoSet = NativeMethods.SetupDiGetClassDevsW(IntPtr.Zero, null, IntPtr.Zero, NativeMethods.DiGetClassFlags.DIGCF_PRESENT | NativeMethods.DiGetClassFlags.DIGCF_ALLCLASSES);
      if (devInfoSet != IntPtr.Zero)
      {
        try
        {
          StringBuilder deviceId = new StringBuilder((int)NativeMethods.MAX_DEVICE_ID_LEN);
          uint index = 0;
          NativeMethods.SP_DEVINFO_DATA devInfo = new NativeMethods.SP_DEVINFO_DATA();
          devInfo.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVINFO_DATA));
          while (NativeMethods.SetupDiEnumDeviceInfo(devInfoSet, index++, ref devInfo))
          {
            // Get the device ID for the HID.
            uint requiredSize;
            if (NativeMethods.SetupDiGetDeviceInstanceIdW(devInfoSet, ref devInfo, deviceId, NativeMethods.MAX_DEVICE_ID_LEN, out requiredSize))
            {

              // Is this the same device as represented by the target device ID?
              if (string.Equals(deviceId.ToString(), targetDeviceId, StringComparison.InvariantCultureIgnoreCase))
              {
                // Yes, same device. Find the first PCI parent of this device.
                uint parentDevInst;
                uint childDevInst = devInfo.DevInst;
                while (NativeMethods.CM_Get_Parent(out parentDevInst, childDevInst, 0) == 0 && NativeMethods.CM_Get_Device_IDW(parentDevInst, deviceId, NativeMethods.MAX_DEVICE_ID_LEN, 0) == 0)
                {
                  // The parent tuner device ID we're looking for should be something like:
                  // PCI\VEN_1822&DEV_4E35&SUBSYS_00031AE4&REV_01\4&CF81C54&0&00F0
                  string id = deviceId.ToString();
                  if (id.StartsWith("PCI"))
                  {
                    tunerId = id;
                    return true;
                  }
                  else
                  {
                    // Assumption: the last intermediate parent is the HID. ID should look like:
                    // AVSTREAM\AZUREWAVEPCIHID.VIRTUAL\5&1560A6BC&1&0
                    hidId = id;
                  }
                  childDevInst = parentDevInst;
                }
                break;
              }
            }
          }
        }
        finally
        {
          NativeMethods.SetupDiDestroyDeviceInfoList(devInfoSet);
        }
      }
      return false;
    }

    private void DebugCapabilities(IntPtr ppData)
    {
      NativeMethods.HIDP_CAPS capabilities;
      NativeMethods.HidStatus status = NativeMethods.HidP_GetCaps(ppData, out capabilities);
      if (status != NativeMethods.HidStatus.HIDP_STATUS_SUCCESS)
      {
        this.LogError("Twinhan HID remote: failed to get raw input capabilities, result = {0}", status);
        return;
      }

      this.LogDebug("debug: usage                   = 0x{0:x}", capabilities.Usage);
      this.LogDebug("debug: usage page              = {0}", (HidUsagePage)capabilities.UsagePage);
      this.LogDebug("debug: input report length     = {0}", capabilities.InputReportByteLength);
      this.LogDebug("debug: output report length    = {0}", capabilities.OutputReportByteLength);
      this.LogDebug("debug: feature report length   = {0}", capabilities.FeatureReportByteLength);
      this.LogDebug("debug: # link collection nodes = {0}", capabilities.NumberLinkCollectionNodes);
      this.LogDebug("debug: # input button caps.    = {0}", capabilities.NumberInputButtonCaps);
      this.LogDebug("debug: # input value caps.     = {0}", capabilities.NumberInputValueCaps);
      this.LogDebug("debug: # input data indices    = {0}", capabilities.NumberInputDataIndices);
      this.LogDebug("debug: # output button caps.   = {0}", capabilities.NumberOutputButtonCaps);
      this.LogDebug("debug: # output value caps.    = {0}", capabilities.NumberOutputValueCaps);
      this.LogDebug("debug: # output data indices   = {0}", capabilities.NumberOutputDataIndices);
      this.LogDebug("debug: # feature button caps.  = {0}", capabilities.NumberFeatureButtonCaps);
      this.LogDebug("debug: # feature value caps.   = {0}", capabilities.NumberFeatureValueCaps);
      this.LogDebug("debug: # feature data indices  = {0}", capabilities.NumberFeatureDataIndices);

      if (capabilities.NumberInputButtonCaps == 0)
      {
        return;
      }

      NativeMethods.HIDP_BUTTON_CAPS[] buttonCapabilities = new NativeMethods.HIDP_BUTTON_CAPS[capabilities.NumberInputButtonCaps];
      status = NativeMethods.HidP_GetButtonCaps(NativeMethods.HIDP_REPORT_TYPE.HidP_Input, buttonCapabilities, ref capabilities.NumberInputButtonCaps, ppData);
      if (status != NativeMethods.HidStatus.HIDP_STATUS_SUCCESS)
      {
        this.LogError("Twinhan HID remote: failed to get raw input input button capabilities, result = {0}", status);
        return;
      }

      foreach (NativeMethods.HIDP_BUTTON_CAPS bc in buttonCapabilities)
      {
        this.LogDebug("debug: button...");
        this.LogDebug("debug:   usage page = {0}", (HidUsagePage)bc.UsagePage);
        this.LogDebug("debug:   report ID  = {0}", bc.ReportID);
        this.LogDebug("debug:   is alias?  = {0}", bc.IsAlias);
        this.LogDebug("debug:   bit field  = {0}", bc.BitField);
        this.LogDebug("debug:   link coll. = {0}", bc.LinkCollection);
        this.LogDebug("debug:   link usage = 0x{0:x}", bc.LinkUsage);
        this.LogDebug("debug:   link UP    = {0}", (HidUsagePage)bc.LinkUsagePage);
        this.LogDebug("debug:   is range?  = {0}", bc.IsRange);
        this.LogDebug("debug:   is s rng?  = {0}", bc.IsStringRange);
        this.LogDebug("debug:   is d rng?  = {0}", bc.IsDesignatorRange);
        this.LogDebug("debug:   is abs?    = {0}", bc.IsAbsolute);
        if (bc.IsRange)
        {
          this.LogDebug("debug:   usage min  = {0}", bc.UsageMin);
          this.LogDebug("debug:   usage max  = {0}", bc.UsageMax);
          this.LogDebug("debug:   string min = {0}", bc.StringMin);
          this.LogDebug("debug:   string max = {0}", bc.StringMax);
          this.LogDebug("debug:   d min      = {0}", bc.DesignatorMin);
          this.LogDebug("debug:   d max      = {0}", bc.DesignatorMax);
          this.LogDebug("debug:   data i min = {0}", bc.DataIndexMin);
          this.LogDebug("debug:   data i max = {0}", bc.DataIndexMax);
        }
        else
        {
          this.LogDebug("debug:   usage      = {0}", bc.UsageMin);
          this.LogDebug("debug:   string ind = {0}", bc.StringMin);
          this.LogDebug("debug:   desig ind  = {0}", bc.DesignatorMin);
          this.LogDebug("debug:   data ind   = {0}", bc.DataIndexMin);
        }
      }
    }

    #endregion

    #region remote control listener thread

    /// <summary>
    /// Assemble an array containing the distinct set of usage page + usage
    /// pairs exposed by all supported HIDs.
    /// </summary>
    private NativeMethods.RAWINPUTDEVICE[] GetRegistrations()
    {
      int count = 0;
      Dictionary<ushort, HashSet<ushort>> usagePairs = new Dictionary<ushort, HashSet<ushort>>(6);
      foreach (TwinhanHid d in _devices.Values)
      {
        if (d.UsagePage == 0)
        {
          continue;
        }
        HashSet<ushort> usages;
        if (!usagePairs.TryGetValue((ushort)d.UsagePage, out usages))
        {
          usages = new HashSet<ushort>();
          usages.Add(d.Usage);
          usagePairs.Add((ushort)d.UsagePage, usages);
          count++;
        }
        else
        {
          if (usages.Add(d.Usage))
          {
            count++;
          }
        }
      }

      NativeMethods.RAWINPUTDEVICE[] registrations = new NativeMethods.RAWINPUTDEVICE[count];
      count = 0;
      foreach (KeyValuePair<ushort, HashSet<ushort>> pair in usagePairs)
      {
        foreach (ushort usage in pair.Value)
        {
          NativeMethods.RAWINPUTDEVICE r = new NativeMethods.RAWINPUTDEVICE();
          r.dwFlags = NativeMethods.RawInputDeviceFlag.RIDEV_INPUTSINK;
          r.usUsagePage = pair.Key;
          r.usUsage = usage;
          registrations[count++] = r;
        }
      }
      return registrations;
    }

    private void HidRemoteControlListener(object eventParam)
    {
      this.LogDebug("Twinhan HID remote: starting remote control listener thread");
      Thread.BeginThreadAffinity();
      string className = "TwinhanHidRemoteListenerThreadWindowClass";
      IntPtr processHandle = Process.GetCurrentProcess().Handle;
      int hr;
      try
      {
        NativeMethods.RAWINPUTDEVICE[] registrations = GetRegistrations();
        IntPtr handle;
        try
        {
          // We need a window handle to receive messages from the driver.
          NativeMethods.WNDCLASS wndclass;
          wndclass.style = 0;
          wndclass.lpfnWndProc = RemoteControlListenerWndProc;
          wndclass.cbClsExtra = 0;
          wndclass.cbWndExtra = 0;
          wndclass.hInstance = processHandle;
          wndclass.hIcon = IntPtr.Zero;
          wndclass.hCursor = IntPtr.Zero;
          wndclass.hbrBackground = IntPtr.Zero;
          wndclass.lpszMenuName = null;
          wndclass.lpszClassName = className;

          int atom = NativeMethods.RegisterClass(ref wndclass);
          if (atom == 0)
          {
            hr = Marshal.GetLastWin32Error();
            this.LogError("Twinhan HID remote: failed to register window class, hr = 0x{0:x}", hr);
            return;
          }

          // Create a window that won't show in the taskbar or alt+tab list etc. with size 0x0.
          handle = NativeMethods.CreateWindowEx(NativeMethods.WindowStyleEx.WS_EX_TOOLWINDOW,
                                                className, string.Empty, NativeMethods.WindowStyle.WS_POPUP,
                                                0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, processHandle, IntPtr.Zero);
          if (handle.Equals(IntPtr.Zero))
          {
            hr = Marshal.GetLastWin32Error();
            this.LogError("Twinhan HID remote: failed to create receive window, hr = 0x{0:x}", hr);
            return;
          }

          // Now register the window to start receiving key press events.
          for (int i = 0; i < registrations.Length; i++)
          {
            registrations[i].hwndTarget = handle;
          }
          if (!NativeMethods.RegisterRawInputDevices(registrations, (uint)registrations.Length, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE))))
          {
            hr = Marshal.GetLastWin32Error();
            this.LogError("Twinhan HID remote: failed to register for keypress events, hr = 0x{0:x}", hr);
            return;
          }

          this.LogDebug("Twinhan HID remote: remote control listener thread is running");
          _remoteControlListenerThreadId = NativeMethods.GetCurrentThreadId();
          _isRemoteControlInterfaceOpen = true;
        }
        finally
        {
          ((ManualResetEvent)eventParam).Set();
        }

        // This thread needs a message loop to pump messages to our window
        // procedure.
        while (true)
        {
          NativeMethods.MSG msg = new NativeMethods.MSG();
          try
          {
            // This call will block until a message is received. It returns
            // false (0) if the message is WM_QUIT.
            int result = NativeMethods.GetMessage(ref msg, IntPtr.Zero, 0, 0);
            if (result == 0)
            {
              // Unregister.
              for (int i = 0; i < registrations.Length; i++)
              {
                registrations[i].dwFlags = NativeMethods.RawInputDeviceFlag.RIDEV_REMOVE;
                registrations[i].hwndTarget = IntPtr.Zero;
              }
              if (!NativeMethods.RegisterRawInputDevices(registrations, (uint)registrations.Length, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE))))
              {
                hr = Marshal.GetLastWin32Error();
                this.LogWarn("Twinhan HID remote: failed to unregister for keypress events, hr = 0x{0:x}", hr);
              }
              if (!NativeMethods.DestroyWindow(handle))
              {
                hr = Marshal.GetLastWin32Error();
                this.LogWarn("Twinhan HID remote: failed to destroy receive window, hr = 0x{0:x}", hr);
              }
              else if (!NativeMethods.UnregisterClass(className, processHandle))
              {
                hr = Marshal.GetLastWin32Error();
                this.LogWarn("Twinhan HID remote: failed to unregister listener window class, hr = 0x{0:x}", hr);
              }
              return;
            }
            else if (result == -1)
            {
              hr = Marshal.GetLastWin32Error();
              this.LogError("Twinhan HID remote: failed to get window message, hr = 0x{0:x}", hr);
            }
            else
            {
              NativeMethods.TranslateMessage(ref msg);
              NativeMethods.DispatchMessage(ref msg);
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Twinhan HID remote: remote control listener thread exception");
          }
        }
      }
      finally
      {
        Thread.EndThreadAffinity();
        this.LogDebug("Twinhan HID remote: stopping remote control listener thread");
      }
    }

    private IntPtr RemoteControlListenerWndProc(IntPtr hWnd, NativeMethods.WindowsMessage msg, IntPtr wParam, IntPtr lParam)
    {
      if (msg != NativeMethods.WindowsMessage.WM_INPUT || lParam == null || lParam == IntPtr.Zero)   // usable key press event
      {
        return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
      }

      int hr;
      uint dataSize = 0;
      uint headerSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER));
      int result = NativeMethods.GetRawInputData(lParam, NativeMethods.RawInputDataCommand.RID_INPUT, IntPtr.Zero, ref dataSize, headerSize);
      if (result != 0)
      {
        hr = Marshal.GetLastWin32Error();
        this.LogError("Twinhan HID remote: failed to get raw input data size, result = {0}, hr = 0x{1:x}", result, hr);
        return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
      }

      IntPtr dataPtr = Marshal.AllocHGlobal((int)dataSize);
      try
      {
        result = NativeMethods.GetRawInputData(lParam, NativeMethods.RawInputDataCommand.RID_INPUT, dataPtr, ref dataSize, headerSize);
        if (result <= 0)
        {
          hr = Marshal.GetLastWin32Error();
          this.LogError("Twinhan HID remote: failed to get raw input data, result = {0}, hr = 0x{1:x}, required size = {2}", result, hr, dataSize);
          return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        // Convert device specific raw input to device independent "usages".
        NativeMethods.RAWINPUT input = (NativeMethods.RAWINPUT)Marshal.PtrToStructure(dataPtr, typeof(NativeMethods.RAWINPUT));
        TwinhanHid device;
        if (!_devices.TryGetValue(input.header.hDevice, out device))
        {
          return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }
        TwinhanHidDriver driver;
        if (!_drivers.TryGetValue(device.ParentHidId, out driver))
        {
          this.LogError("Twinhan HID remote: failed to find driver details for HID {0}", device.ParentHidId);
          return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        UsageType usageType;
        int usage;
        string usageName;
        if (!GetUsageFromRawInput(driver, device, input, dataPtr, out usageType, out usage, out usageName))
        {
          return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        // Reverse-convert device independent usages back to remote control buttons.
        byte scanCode;
        if (GetScanCodeFromUsage(driver, usageType, usage, out scanCode))
        {
          if (driver.RemoteControlType == TwinhanRemoteControlType.New)
          {
            this.LogDebug("Twinhan HID remote: remote control {0} key press, usage = {1}, button = {2}, device = {3}", usageType, usageName, (TwinhanRemoteScanCodeNew)scanCode, device.Name);
          }
          else
          {
            this.LogDebug("Twinhan HID remote: remote control {0} key press, usage = {1}, button = {2}, device = {3}", usageType, usageName, (TwinhanRemoteScanCodeOld)scanCode, device.Name);
          }
        }
        else
        {
          this.LogWarn("Twinhan HID remote: failed to find scan code, device = {0}, usage = {1}, usage type = {2}", device.Name, usageType, usageName);
        }
      }
      finally
      {
        Marshal.FreeHGlobal(dataPtr);
      }
      return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private bool GetUsageFromRawInput(TwinhanHidDriver driver, TwinhanHid device, NativeMethods.RAWINPUT input, IntPtr rawInput, out UsageType usageType, out int usage, out string usageName)
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

        usageType = UsageType.Keyboard;
        usage = (int)vk | (int)_modifiers;
        usageName = vk.ToString();
        if (_modifiers != 0)
        {
          usageName += string.Format(", modifiers = {0}", _modifiers);
        }
      }
      else if (input.header.dwType == NativeMethods.RawInputDeviceType.RIM_TYPEHID)
      {
        if ((!driver.IsTerraTec && device.Name.Contains("Col03")) || (driver.IsTerraTec && device.Name.Contains("Col02")))
        {
          usageType = UsageType.Raw;
          usage = Marshal.ReadByte(rawInput, HID_INPUT_DATA_OFFSET + 1);
          usageName = string.Format("0x{0:x2}", usage);
        }
        else if (device.Name.Contains("Col05"))
        {
          usageType = UsageType.Ascii;
          usage = Marshal.ReadByte(rawInput, HID_INPUT_DATA_OFFSET + 1);
          usageName = string.Format("0x{0:x2}", usage);
        }
        else
        {
          byte[] report = new byte[input.data.hid.dwSizeHid];
          Marshal.Copy(IntPtr.Add(rawInput, HID_INPUT_DATA_OFFSET), report, 0, report.Length);
          uint usageCount = input.data.hid.dwCount;
          NativeMethods.USAGE_AND_PAGE[] usages = new NativeMethods.USAGE_AND_PAGE[usageCount];
          NativeMethods.HidStatus status = NativeMethods.HidP_GetUsagesEx(NativeMethods.HIDP_REPORT_TYPE.HidP_Input, 0, usages, ref usageCount, device.PreParsedData, report, (uint)report.Length);
          if (status != NativeMethods.HidStatus.HIDP_STATUS_SUCCESS)
          {
            this.LogError("Twinhan HID remote: failed to get raw input HID usages, device = {0}, status = {1}", device.Name, status);
            Dump.DumpBinary(rawInput, (int)input.header.dwSize);
            return false;
          }
          if (usageCount > 1)
          {
            this.LogWarn("Twinhan HID remote: multiple simultaneous HID usages not supported");
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
            usageType = UsageType.Consumer;
            usageName = Enum.GetName(typeof(HidConsumerUsage), up.Usage);
          }
          else if (page == HidUsagePage.MceRemote)
          {
            usageType = UsageType.Mce;
            usageName = Enum.GetName(typeof(MceRemoteUsage), up.Usage);
          }
          else
          {
            this.LogError("Twinhan HID remote: unexpected usage page, device = {0}, page = {1}, usage = {2}", device.Name, page, up.Usage);
            return false;
          }
        }
      }
      else
      {
        this.LogError("Twinhan HID remote: received input from unsupported input device type, device = {0}, type = {1}", device.Name, input.header.dwType);
        return false;
      }
      return true;
    }

    private bool GetScanCodeFromUsage(TwinhanHidDriver driver, UsageType usageType, int usage, out byte scanCode)
    {
      scanCode = 0;
      if (driver.IsTerraTec)
      {
        // Assume raw input. This could be incorrect. We do know that the
        // TerraTec drivers don't use the internal mapping tables. It is
        // possible that TerraTec uses their "Remote Control Editor" software
        // to translate to configurable key press events outside the driver.
        scanCode = (byte)(usage & 0xff);
        return true;
      }

      int keyCode = (int)usageType | usage;
      if (driver.Mapping == TwinhanRemoteControlMapping.DtvDvb)
      {
        return MAPPING_DTV_DVB.TryGetValue(keyCode, out scanCode);
      }
      else if (driver.Mapping == TwinhanRemoteControlMapping.Cyberlink)
      {
        return MAPPING_CYBERLINK.TryGetValue(keyCode, out scanCode);
      }
      else if (driver.Mapping == TwinhanRemoteControlMapping.InterVideo)
      {
        return  MAPPING_INTERVIDEO.TryGetValue(keyCode, out scanCode);
      }
      else if (driver.Mapping == TwinhanRemoteControlMapping.Mce)
      {
        return  MAPPING_MCE.TryGetValue(keyCode, out scanCode);
      }
      else if (driver.Mapping == TwinhanRemoteControlMapping.DtvDvbWmInput)
      {
        return  MAPPING_DTV_DVB_WM_INPUT.TryGetValue(keyCode, out scanCode);
      }
      else if (driver.Mapping == TwinhanRemoteControlMapping.Custom)
      {
        return  driver.CustomMapping.TryGetValue(keyCode, out scanCode);
      }
      else if (driver.Mapping == TwinhanRemoteControlMapping.DigitalNow)
      {
        return  MAPPING_DNTV.TryGetValue(keyCode, out scanCode);
      }
      return false;
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Twinhan HID remote";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("Twinhan HID remote: initialising");

      lock (_instanceLock)
      {
        if (_isLoaded)
        {
          this.LogDebug("Twinhan HID remote: already loaded");
          return false;
        }

        // Check if any Twinhan HIDs are installed.
        FindHids(out _devices, out _drivers);
        if (_devices.Count == 0)
        {
          this.LogDebug("Twinhan HID remote: no devices detected");
          return false;
        }
        _isLoaded = true;
      }

      this.LogInfo("Twinhan HID remote: extension supported, {0} device(s), {1} driver(s)", _devices.Count, _drivers.Count);
      _isTwinhanHidRemote = true;
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenRemoteControlInterface()
    {
      this.LogDebug("Twinhan HID remote: open remote control interface");

      if (!_isTwinhanHidRemote)
      {
        this.LogWarn("Twinhan HID remote: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Twinhan HID remote: remote control interface is already open");
        return true;
      }

      foreach (TwinhanHidDriver d in _drivers.Values)
      {
        d.Enable();
      }

      ManualResetEvent startEvent = new ManualResetEvent(false);
      _remoteControlListenerThread = new Thread(new ParameterizedThreadStart(HidRemoteControlListener));
      _remoteControlListenerThread.Name = "Twinhan HID remote control listener";
      _remoteControlListenerThread.IsBackground = true;
      _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
      _remoteControlListenerThread.Start(startEvent);
      startEvent.WaitOne();
      startEvent.Close();

      if (_isRemoteControlInterfaceOpen)
      {
        this.LogDebug("Twinhan HID remote: result = success");
        return true;
      }
      return false;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseRemoteControlInterface()
    {
      this.LogDebug("Twinhan HID remote: close remote control interface");
      if (_remoteControlListenerThread != null && _remoteControlListenerThreadId > 0)
      {
        NativeMethods.PostThreadMessage(_remoteControlListenerThreadId, NativeMethods.WindowsMessage.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        _remoteControlListenerThread.Join();
        _remoteControlListenerThreadId = 0;
        _remoteControlListenerThread = null;
      }

      foreach (TwinhanHidDriver d in _drivers.Values)
      {
        d.Disable();
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Twinhan HID remote: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_isTwinhanHidRemote)
      {
        CloseRemoteControlInterface();
        lock (_instanceLock)
        {
          _isLoaded = false;
        }

        foreach (TwinhanHid d in _devices.Values)
        {
          d.Dispose();
        }
        _devices.Clear();

        foreach (TwinhanHidDriver d in _drivers.Values)
        {
          d.Dispose();
        }
        _drivers.Clear();

        _isTwinhanHidRemote = false;
      }
    }

    #endregion
  }
}