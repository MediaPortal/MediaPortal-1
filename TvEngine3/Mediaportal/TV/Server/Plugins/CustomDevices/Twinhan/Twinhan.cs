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
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Twinhan
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for Twinhan devices, including clones from TerraTec,
  /// TechniSat and Digital Rise.
  /// </summary>
  public class Twinhan : BaseCustomDevice, /*IAddOnDevice, ICustomTuner,*/ IPowerDevice, IPidFilterController, IConditionalAccessProvider, ICiMenuActions, IDiseqcDevice
  {
    #region enums

    [Flags]
    private enum TwinhanDeviceType : uint
    {
      // Digital types
      DvbS = 0x0001,
      DvbT = 0x0002,
      DvbC = 0x0004,
      Atsc = 0x0008,
      AnnexC = 0x0010,        // US OpenCable (clear QAM)
      IsdbT = 0x0020,
      IsdbS = 0x0040,

      // Analog types
      Pal = 0x0100,
      Ntsc = 0x0200,
      Secam = 0x0400,
      Svideo = 0x0800,
      Composite = 0x1000,
      Fm = 0x2000,

      RemoteControlSupported = 0x80000000
    }

    private enum TwinhanDeviceSpeed : byte
    {
      Pci = 0xff,             // PCI
      Pcie = 0xfe,            // PCI-express
      UsbLow = 0,             // USB 1.x
      UsbFull = 1,            // USB 1.x
      UsbHigh = 2             // USB 2.0
    }

    private enum TwinhanCiSupport : byte
    {
      Unsupported = 0,
      ApiVersion1,
      ApiVersion2
    }

    [Flags]
    private enum TwinhanSimulatorType : uint
    {
      Physical = 0,   // ie. not a simulator
      VirtualDvbS = 1,
      VirtualDvbT = 2,
      VirtualDvbC = 4,
      VirtualAtsc = 8,
      MceVirtualDvbT = 16
    }

    private enum TwinhanCamType   // CAM_TYPE_ENUM
    {
      Unknown = 0,
      Default = 1,            // Viaccess
      Astoncrypt,
      Conax,
      Cryptoworks
    }

    private enum TwinhanCiState : uint    // CIMessage
    {
      // Old states - CI API v1.
      Empty_Old = 0,          // CI_STATUS_EMPTY_OLD - there is no CAM present
      Cam1Okay_Old,           // CI_STATUS_CAM_OK1_OLD (ME0) - the first slot has a CAM
      Cam2Okay_Old,           // CI_STATUS_CAM_OK2_OLD (ME1) - the second slot has a CAM

      // New states - CI API v2.
      Empty = 10,             // CI_STATUS_EMPTY - there is no CAM present
      CamInserted,            // CI_STATUS_INSERTED - a CAM is present but is still being initialised
      CamOkay,                // CI_STATUS_CAM_OK - a CAM is present, initialised and ready for interaction
      CamUnknown              // CI_STATUS_CAM_UNKNOW
    }

    private enum TwinhanMmiState : uint   // CIMessage
    {
      Idle = 0,               // NON_CI_INFO - there are no new MMI objects available

      // Old states - CI API v1.
      Menu1Okay_Old = 3,      // MMI_STATUS_GET_MENU_OK1_OLD (MMI0) - the CAM in the first slot has an MMI object waiting
      Menu2Okay_Old,          // MMI_STATUS_GET_MENU_OK2_OLD (MMI1) - the CAM in the second slot has an MMI object waiting
      Menu1Close_Old,         // MMI_STATUS_GET_MENU_CLOSE1_OLD (MMI0_ClOSE) - the CAM in the first slot requests that the MMI session be closed
      Menu2Close_Old,         // MMI_STATUS_GET_MENU_CLOSE2_OLD (MMI1_ClOSE) - the CAM in the second slot requests that the MMI session be closed

      // New states - CI API v2.
      SendMenuAnswer = 14,    // MMI_STATUS_ANSWER_SEND 
      MenuOkay,               // MMI_STATUS_GET_MENU_OK - the CAM has an MMI object waiting
      MenuFail,               // MMI_STATUS_GET_MENU_FAIL - the CAM failed to assemble or send and MMI object
      MenuInit,               // MMI_STATUS_GET_MENU_INIT - the CAM is assembling an MMI object
      MenuClose,              // MMI_STATUS_GET_MENU_CLOSE - the CAM requests that the MMI session be closed
      MenuClosed,             // MMI_STATUS_GET_MENU_CLOSED - there is no open MMI session
    }

    private enum TwinhanRawCommandState : uint    // CIMessage
    {
      SendCommand = 30,       // RAW_CMD_STATUS_SEND
      DataOkay                // RAW_CMD_STATUS_GET_DATA_OK
    }

    private enum TwinhanPidFilterMode : byte
    {
      Whitelist = 0,          // PID_FILTER_MODE_PASS - PID filter list contains PIDs that pass through
      Disabled,               // PID_FILTER_MODE_DISABLE - PID filter disabled and all PIDs pass through
      Blacklist               // PID_FILTER_MODE_FILTER - PID filter list contains PIDs that *don't* pass through
    }

    private enum TwinhanToneBurst : byte
    {
      Off = 0,
      ToneBurst,
      DataBurst
    }

    private enum Twinhan22k : byte
    {
      Auto = 0,               // Based on transponder frequency and LNB switch frequency...
      Off,
      On
    }

    private enum TwinhanDiseqcPort : byte
    {
      Null = 0,
      PortA,
      PortB,
      PortC,
      PortD
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DeviceInfo   // DEVICE_INFO
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public String Name;                                     // Example: VP1020, VP3020C, VP7045...

      public TwinhanDeviceType Type;                          // Values are bitwise AND'ed together to produce the final device type.
      public TwinhanDeviceSpeed Speed;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
      public byte[] MacAddress;
      public TwinhanCiSupport CiSupport;

      public Int32 TsPacketLength;                            // 188 or 204

      // mm1352000: The following two bytes don't appear to always be set correctly.
      // Maybe these fields are only present for certain devices or driver versions.
      public byte IsPidFilterPresent;
      public byte IsPidFilterBypassSupported;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 190)]
      private byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DriverInfo   // DriverInfo
    {
      public byte DriverMajorVersion;                         // BCD encoding eg. 0x32 -> 3.2
      public byte DriverMinorVersion;                         // BCD encoding eg. 0x21 -> 2.1
      public byte FirmwareMajorVersion;                       // BCD encoding eg. 0x10 -> 1.0
      public byte FirmwareMinorVersion;                       // BCD encoding eg. 0x05 -> 0.5  ==> 1.0b05
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
      public String Date;                                     // Example: "2004-12-20 18:30:00" or  "DEC 20 2004 10:22:10"  with compiler __DATE__ and __TIME__  definition s
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
      public String Company;                                  // Example: "TWINHAN" 
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public String HardwareInfo;                             // Example: "PCI DVB CX-878 with MCU series", "PCI ATSC CX-878 with MCU series", "7020/7021 USB-Sat", "7045/7046 USB-Ter" etc.
      public byte CiMmiFlags;                                 // Bit 0 = event mode support (0 => not supported, 1 => supported)
      private byte Reserved;
      public TwinhanSimulatorType SimType;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 184)]
      private byte[] Reserved2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PidFilterParams    // PID_FILTER_INFO
    {
      public TwinhanPidFilterMode FilterMode;
      public byte MaxPids;                                    // Max number of PIDs supported by the PID filter (HW/FW limit, always <= MaxPidFilterPids).
      private UInt16 Padding;
      public UInt32 ValidPidMask;                             // A bit mask specifying the current valid PIDs. If the bit is 0 then the PID is ignored. Example: if ValidPidMask = 0x00000005 then there are 2 valid PIDs at indexes 0 and 2.
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPidFilterPids)]
      public UInt16[] FilterPids;                             // Filter PID list.
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LnbParams  // LNB_DATA
    {
      [MarshalAs(UnmanagedType.I1)]
      public bool PowerOn;
      public TwinhanToneBurst ToneBurst;
      private UInt16 Padding;

      public UInt32 LowBandLof;                               // unit = kHz
      public UInt32 HighBandLof;                              // unit = kHz
      public UInt32 SwitchFrequency;                          // unit = kHz
      public Twinhan22k Tone22k;
      public TwinhanDiseqcPort DiseqcPort;
      private UInt16 Padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DiseqcMessage
    {
      public Int32 MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
    }

    // New CI/MMI state info structure - CI API v2.
    [StructLayout(LayoutKind.Sequential)]
    private struct CiStateInfo    // THCIState
    {
      public TwinhanCiState CiState;
      public TwinhanMmiState MmiState;
      public UInt32 PmtState;
      public UInt32 EventMessage;                             // Current event status.
      public TwinhanRawCommandState RawCmdState;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
      private UInt32[] Reserved;
    }

    // Old CI/MMI state info structure - CI API v1.
    [StructLayout(LayoutKind.Sequential)]
    private struct CiStateInfoOld   // THCIStateOld
    {
      public TwinhanCiState CiState;
      public TwinhanMmiState MmiState;
    }

    #region MMI data class

    // A private class to help us handle the two MMI data formats cleanly and easily.
    private class MmiData
    {
      public String Title = String.Empty;
      public String SubTitle = String.Empty;
      public String Footer = String.Empty;
      public List<String> Entries = new List<string>();
      public Int32 EntryCount = 0;
      public bool IsEnquiry = false;
      public bool IsBlindAnswer = false;
      public Int32 AnswerLength = 0;
      public String Prompt = String.Empty;
      public Int32 ChoiceIndex = 0;
      public String Answer = String.Empty;
      public Int32 Type = 0;

      public void WriteToBuffer(IntPtr buffer, bool isTerraTecFormat)
      {
        if (isTerraTecFormat)
        {
          TerraTecMmiData mmiData = new TerraTecMmiData();
          mmiData.AnswerLength = AnswerLength;
          mmiData.ChoiceIndex = ChoiceIndex;
          mmiData.Answer = Answer;
          mmiData.Type = Type;
          Marshal.StructureToPtr(mmiData, buffer, true);
        }
        else
        {
          DefaultMmiData mmiData = new DefaultMmiData();
          mmiData.AnswerLength = AnswerLength;
          mmiData.ChoiceIndex = ChoiceIndex;
          mmiData.Answer = Answer;
          mmiData.Type = Type;
          Marshal.StructureToPtr(mmiData, buffer, true);
        }
      }

      public void ReadFromBuffer(IntPtr buffer, bool isTerraTecFormat)
      {
        if (isTerraTecFormat)
        {
          TerraTecMmiData mmiData = (TerraTecMmiData)Marshal.PtrToStructure(buffer, typeof(TerraTecMmiData));
          Title = mmiData.Title;
          SubTitle = mmiData.SubTitle;
          Footer = mmiData.Footer;
          EntryCount = mmiData.EntryCount;
          if (EntryCount > TerraTecMaxCamMenuEntries)
          {
            EntryCount = TerraTecMaxCamMenuEntries;
          }
          foreach (TerraTecMmiMenuEntry entry in mmiData.Entries)
          {
            Entries.Add(entry.Text);
          }
          IsEnquiry = mmiData.IsEnquiry;
          IsBlindAnswer = mmiData.IsBlindAnswer;
          AnswerLength = mmiData.AnswerLength;
          Prompt = mmiData.Prompt;
          ChoiceIndex = mmiData.ChoiceIndex;
          Answer = mmiData.Answer;
          Type = mmiData.Type;
        }
        else
        {
          DefaultMmiData mmiData = (DefaultMmiData)Marshal.PtrToStructure(buffer, typeof(DefaultMmiData));
          Title = mmiData.Title;
          SubTitle = mmiData.SubTitle;
          Footer = mmiData.Footer;
          EntryCount = mmiData.EntryCount;
          if (EntryCount > DefaultMaxCamMenuEntries)
          {
            EntryCount = DefaultMaxCamMenuEntries;
          }
          foreach (DefaultMmiMenuEntry entry in mmiData.Entries)
          {
            Entries.Add(entry.Text);
          }
          IsEnquiry = mmiData.IsEnquiry;
          IsBlindAnswer = mmiData.IsBlindAnswer;
          AnswerLength = mmiData.AnswerLength;
          Prompt = mmiData.Prompt;
          ChoiceIndex = mmiData.ChoiceIndex;
          Answer = mmiData.Answer;
          Type = mmiData.Type;
        }
      }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DefaultMmiMenuEntry
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DefaultMmiData
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Title;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String SubTitle;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = DefaultMaxCamMenuEntries)]
      public DefaultMmiMenuEntry[] Entries;
      private UInt16 Padding1;
      public Int32 EntryCount;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsEnquiry;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsBlindAnswer;
      public Int32 AnswerLength;    // enquiry: expected answer length, enquiry answer: actual answer length
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Prompt;

      public Int32 ChoiceIndex;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Answer;

      public Int32 Type;            // 1, 2 (menu/list, select entry) or 3 (enquiry, enquiry answer)
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct TerraTecMmiMenuEntry
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct TerraTecMmiData
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Title;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String SubTitle;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = TerraTecMaxCamMenuEntries)]
      public TerraTecMmiMenuEntry[] Entries;
      public Int32 EntryCount;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsEnquiry;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsBlindAnswer;
      public Int32 AnswerLength;    // enquiry: expected answer length, enquiry answer: actual answer length
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Prompt;

      public Int32 ChoiceIndex;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Answer;

      public Int32 Type;            // 1, 2 (menu/list, select entry) or 3 (enquiry, enquiry answer)
    }

    #endregion

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ApplicationInfo    // THAppInfo
    {
      public UInt32 ApplicationType;
      public UInt32 Manufacturer;
      public UInt32 ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public String RootMenuTitle;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct TuningParams   // TURNER_VALUE
    {
      [FieldOffset(0)]
      public UInt32 Frequency;                                // unit = kHz

      // Note: these two fields are unioned - they are never both required in
      // a single tune request so the bytes are reused.
      [FieldOffset(4)]
      public UInt32 SymbolRate;                               // unit = ks/s
      [FieldOffset(4)]
      public UInt32 Bandwidth;                                // unit = MHz

      [FieldOffset(8)]
      public ModulationType Modulation;
      [FieldOffset(12), MarshalAs(UnmanagedType.I1)]
      public bool LockWaitForResult;
      [FieldOffset(13)]
      private byte Padding1;
      [FieldOffset(14)]
      private UInt16 Padding2;
    }

    #endregion

    #region Twinhan IO controls

    // Initialise a buffer with a new command, ready to pass to the tuner filter.
    private class TwinhanCommand
    {
      private UInt32 _controlCode;
      private IntPtr _inBuffer;
      private Int32 _inBufferSize;
      private IntPtr _outBuffer;
      private Int32 _outBufferSize;

      public TwinhanCommand(UInt32 controlCode, IntPtr inBuffer, Int32 inBufferSize, IntPtr outBuffer, Int32 outBufferSize)
      {
        _controlCode = controlCode;
        _inBuffer = inBuffer;
        _inBufferSize = inBufferSize;
        _outBuffer = outBuffer;
        _outBufferSize = outBufferSize;
      }

      public int Execute(IKsPropertySet ps, out int returnedByteCount)
      {
        returnedByteCount = 0;
        int hr = 1; // fail
        if (ps == null)
        {
          return hr;
        }

        IntPtr instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
        IntPtr commandBuffer = Marshal.AllocCoTaskMem(CommandSize);
        IntPtr returnedByteCountBuffer = Marshal.AllocCoTaskMem(sizeof(int));
        try
        {
          // Clear buffers. This is probably not actually needed, but better
          // to be safe than sorry!
          for (int i = 0; i < InstanceSize; i++)
          {
            Marshal.WriteByte(instanceBuffer, i, 0);
          }
          Marshal.WriteInt32(returnedByteCountBuffer, 0);

          // Fill the command buffer.
          byte[] guidAsBytes = CommandGuid.ToByteArray();
          Marshal.Copy(guidAsBytes, 0, commandBuffer, guidAsBytes.Length);
          Marshal.WriteInt32(commandBuffer, 16, (Int32)_controlCode);
          Marshal.WriteInt32(commandBuffer, 20, _inBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 24, _inBufferSize);
          Marshal.WriteInt32(commandBuffer, 28, _outBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 32, _outBufferSize);
          Marshal.WriteInt32(commandBuffer, 36, returnedByteCountBuffer.ToInt32());

          hr = ps.Set(BdaExtensionPropertySet, 0, instanceBuffer, InstanceSize, commandBuffer, CommandSize);
          if (hr == 0)
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
    }

    private const uint THBDA_IO_INDEX = 0xaa00;
    private const uint METHOD_BUFFERED = 0x0000;
    private const uint FILE_ANY_ACCESS = 0x0000;

    // Assemble a control code.
    private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
    {
      return ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);
    }

    //*******************************************************************************************************
    //Functionality : Check BDA driver if support IOCTL interface
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CHECK_INTERFACE = CTL_CODE(THBDA_IO_INDEX, 121, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get device info
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct DEVICE_INFO
    //OutBufferSize : sizeof(DEVICE_INFO) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_DEVICE_INFO = CTL_CODE(THBDA_IO_INDEX, 124, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get driver info
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct DriverInfo
    //OutBufferSize : sizeof(DriverInfo) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_DRIVER_INFO = CTL_CODE(THBDA_IO_INDEX, 125, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Reset USB or PCI controller
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_RESET_DEVICE = CTL_CODE(THBDA_IO_INDEX, 120, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #region tuner power

    //*******************************************************************************************************
    //Functionality : Set turner power
    //InBuffer      : Tuner_Power_ON | Tuner_Power_OFF
    //InBufferSize  : 1 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_SET_TUNER_POWER = CTL_CODE(THBDA_IO_INDEX, 100, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get turner power status
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : Tuner_Power_ON | Tuner_Power_OFF
    //OutBufferSize : 1 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_TUNER_POWER = CTL_CODE(THBDA_IO_INDEX, 101, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region tuning

    //*******************************************************************************************************
    //Functionality : Set turner frequency and symbol rate
    //InBuffer      : struct TURNER_VALUE
    //InBufferSize  : sizeof(TURNER_VALUE) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_LOCK_TUNER = CTL_CODE(THBDA_IO_INDEX, 106, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get turner frequency and symbol rate
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct TURNER_VALUE
    //OutBufferSize : sizeof(TURNER_VALUE) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_TUNER_VALUE = CTL_CODE(THBDA_IO_INDEX, 107, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get signal quality & strength
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct SIGNAL_DATA
    //OutBufferSize : sizeof(SIGNAL_DATA) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_SIGNAL_Q_S = CTL_CODE(THBDA_IO_INDEX, 108, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region DiSEqC and LNB parameters

    //*******************************************************************************************************
    //Functionality : Send DiSEqC command
    //InBuffer      : struct DiSEqC_DATA
    //InBufferSize  : sizeof(DiSEqC_DATA) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_SET_DiSEqC = CTL_CODE(THBDA_IO_INDEX, 104, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get DiSEqC command
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct DiSEqC_DATA
    //OutBufferSize : sizeof(DiSEqC_DATA) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_DiSEqC = CTL_CODE(THBDA_IO_INDEX, 105, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Set LNB parameters
    //InBuffer      : struct LNB_DATA
    //InBufferSize  : sizeof(LNB_DATA) bytes
    //OutBuffer     : 0
    //OutBufferSize : 0
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_SET_LNB_DATA = CTL_CODE(THBDA_IO_INDEX, 128, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : GET LNB parameters
    //InBuffer      : NULL
    //InBufferSize  : 0
    //OutBuffer     : struct LNB_DATA
    //OutBufferSize : sizeof(LNB_DATA) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_LNB_DATA = CTL_CODE(THBDA_IO_INDEX, 129, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region PID filter

    //*******************************************************************************************************
    //Functionality : Set PID filter mode and Pids to PID filter
    //InBuffer      : struct PID_FILTER_INFO
    //InBufferSize  : sizeof(PID_FILTER_INFO) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_SET_PID_FILTER_INFO = CTL_CODE(THBDA_IO_INDEX, 113, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get Pids, PLD mode and available max number Pids
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct PID_FILTER_INFO
    //OutBufferSize : sizeof(PID_FILTER_INFO) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_PID_FILTER_INFO = CTL_CODE(THBDA_IO_INDEX, 114, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region CI handling

    #region MMI

    //*******************************************************************************************************
    //Functionality : Get CI state
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct THCIState
    //OutBufferSize : sizeof(THCIState) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_GET_STATE = CTL_CODE(THBDA_IO_INDEX, 200, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get APP info.
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct THAppInfo
    //OutBufferSize : sizeof(THAppInfo) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_GET_APP_INFO = CTL_CODE(THBDA_IO_INDEX, 201, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Init MMI
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_INIT_MMI = CTL_CODE(THBDA_IO_INDEX, 202, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get MMI
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct THMMIInfo
    //OutBufferSize : sizeof(THMMIInfo) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_GET_MMI = CTL_CODE(THBDA_IO_INDEX, 203, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Answer
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct THMMIInfo
    //OutBufferSize : sizeof(THMMIInfo) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_ANSWER = CTL_CODE(THBDA_IO_INDEX, 204, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Close MMI
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_CLOSE_MMI = CTL_CODE(THBDA_IO_INDEX, 205, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region PMT

    //*******************************************************************************************************
    //Functionality : Send PMT
    //InBuffer      : PMT data buffer
    //InBufferSize  : PMT data buffer size bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //Comment       : CA_PMT data format
    //1: ca pmt list management;(8 bit);
    //2: program number (16 bit);
    //3: reserved (2 bit);
    //4: version number (5 bit);
    //5: current next indicator (I bit);
    //6: reserved (4 bit);
    //7: program information length (12 bit);
    //8: if (7!=0)
    //	    ca pmt command id (program level); (8 bit);
    //	    ca descriptor at program level; (n * 8bit);
    //9:  stream type (8 bit);
    //10: reserved (3 bit);
    //11: elementary stream PID (bit 13);
    //12: reserved (4 bit);
    //13: ES information length (12 bit);
    //14: if (ES information length ! =0)
    //       ca pmt command id ( elementary stream level) (8 bit);
    //	     ca descriptor at elementary stream level; ( n * 8bit)
    //* more detail, please refer to EN 50221 (8,4,3,4 CA_PMT); 
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_SEND_PMT = CTL_CODE(THBDA_IO_INDEX, 206, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get PMT Reply
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : PMT Reply Buffer
    //OutBufferSize : sizeof(PMT Reply Buffer) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_GET_PMT_REPLY = CTL_CODE(THBDA_IO_INDEX, 210, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #endregion

    #region not used/implemented

    #region ring buffer TS data capture

    //*******************************************************************************************************
    //Functionality : START TS capture (from Tuner to driver Ring buffer)
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_START_CAPTURE = CTL_CODE(THBDA_IO_INDEX, 109, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Stop TS capture
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_STOP_CAPTURE = CTL_CODE(THBDA_IO_INDEX, 110, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get Driver ring buffer status
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct RING_BUF_STATUS 
    //OutBufferSize : sizeof(RING_BUF_STATUS) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_RINGBUFFER_STATUS = CTL_CODE(THBDA_IO_INDEX, 111, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get TS from driver's ring buffer to local  buffer 
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct CAPTURE_DATA
    //OutBufferSize : sizeof(CAPTURE_DATA) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_CAPTURE_DATA = CTL_CODE(THBDA_IO_INDEX, 112, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region remote control

    //*******************************************************************************************************
    //Functionality : Start RC(Remote Controller receiving) thread
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_START_REMOTE_CONTROL = CTL_CODE(THBDA_IO_INDEX, 115, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Stop RC thread, and remove all RC event
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_STOP_REMOTE_CONTROL = CTL_CODE(THBDA_IO_INDEX, 116, METHOD_BUFFERED, FILE_ANY_ACCESS);


    //*******************************************************************************************************
    //Functionality : Add RC_Event to driver
    //InBuffer      : REMOTE_EVENT
    //InBufferSize  : sizeof(REMOTE_EVENT) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_ADD_RC_EVENT = CTL_CODE(THBDA_IO_INDEX, 117, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Remove RC_Event 
    //InBuffer      : REMOTE_EVENT
    //InBufferSize  : sizeof(REMOTE_EVENT) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_REMOVE_RC_EVENT = CTL_CODE(THBDA_IO_INDEX, 118, METHOD_BUFFERED, FILE_ANY_ACCESS);


    //*******************************************************************************************************
    //Functionality : Get Remote Controller key
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : BYTE
    //OutBufferSize : 1 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_REMOTE_CONTROL_VALUE = CTL_CODE(THBDA_IO_INDEX, 119, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Set Remote control,HID function enable or disable
    //InBuffer      : 1 0 for OFF,others for ON.
    //InBufferSize  : 1 bytes
    //OutBuffer     : 0 registers value
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_HID_RC_ENABLE = CTL_CODE(THBDA_IO_INDEX, 152, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region registry parameters

    //*******************************************************************************************************
    //Functionality : Set Twinhan BDA driver configuration
    //InBuffer      : struct THBDAREGPARAMS
    //InBufferSize  : sizeof(THBDAREGPARAMS) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_SET_REG_PARAMS = CTL_CODE(THBDA_IO_INDEX, 122, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get Twinhan BDA driver configuration
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct THBDAREGPARAMS
    //OutBufferSize : struct THBDAREGPARAMS
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_REG_PARAMS = CTL_CODE(THBDA_IO_INDEX, 123, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region EEPROM access

    //*******************************************************************************************************
    //Functionality : Write EEPROM value
    //InBuffer      : struct EE_IO_DATA
    //InBufferSize  : sizeof(EE_IO_DATA) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_SET_EE_VAL = CTL_CODE(THBDA_IO_INDEX, 126, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Read EEPROM value      
    //InBuffer      : struct EE_IO_DATA
    //InBufferSize  : sizeof(EE_IO_DATA) bytes
    //OutBuffer     : struct EE_IO_DATA
    //OutBufferSize : sizeof(EE_IO_DATA) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_EE_VAL = CTL_CODE(THBDA_IO_INDEX, 127, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region virtual DVB-T device control

    //*******************************************************************************************************
    //Functionality : Enable virtual DVBT interface for DVB-S card
    //InBuffer      : 1 0 for OFF,others for ON.
    //InBufferSize  : 1 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_ENABLE_VIRTUAL_DVBT = CTL_CODE(THBDA_IO_INDEX, 300, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Reset (Clear) DVB-S Transponder mapping table entry for virtual DVB-T interface
    //InBuffer      : NULL
    //InBufferSize  : 0
    //OutBuffer     : NULL
    //OutBufferSize : 0
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_RESET_T2S_MAPPING = CTL_CODE(THBDA_IO_INDEX, 301, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Set DVB-S Transponder mapping table entry for virtual DVB-T interface
    //InBuffer      : struct DVB-T2S_MAPPING_ENTRY
    //InBufferSize  : sizeof(struct DVB-T2S_MAPPING_ENTRY) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_SET_T2S_MAPPING = CTL_CODE(THBDA_IO_INDEX, 302, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : GET DVB-S Transponder mapping table entry
    //InBuffer      : &(Table_Index)
    //InBufferSize  : sizeof(ULONG)
    //OutBuffer     : struct DVB-T2S_MAPPING_ENTRY
    //OutBufferSize : sizeof(struct DVB-T2S_MAPPING_ENTRY) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_GET_T2S_MAPPING = CTL_CODE(THBDA_IO_INDEX, 303, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region tuner firmware download

    //*******************************************************************************************************
    //Functionality : Download tuner firmware, 704C
    //InBuffer      : 1 byte buffer,  0:Downlaod analog TV firmware, 1:download DVB-T firmware
    //InBufferSize  : 1:byte
    //OutBuffer     :1 byte buffer,  0-99: download percentage, 100:download complete, 255:Fail 
    //OutBufferSize : 1 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_DOWNLOAD_TUNER_FIRMWARE = CTL_CODE(THBDA_IO_INDEX, 400, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get tuner firmware download progress
    //InBuffer      : NULL
    //InBufferSize  : 0:byte
    //OutBuffer     :1 byte buffer,  0-99: download percentage, 100:download complete, 255:Fail 
    //OutBufferSize : 1 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_DOWNLOAD_TUNER_FIRMWARE_STAUS = CTL_CODE(THBDA_IO_INDEX, 401, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region CI event handling

    //*******************************************************************************************************
    //Functionality : Create CI event
    //InBuffer      : hEventHandle (The event handle that is created by AP)
    //InBufferSize  : sizeof(HANDLE)
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_EVENT_CREATE = CTL_CODE(THBDA_IO_INDEX, 208, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Close CI event
    //InBuffer      : hEventHandle (The event handle that is sended by create CI event)
    //InBufferSize  : sizeof(HANDLE)
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_EVENT_CLOSE = CTL_CODE(THBDA_IO_INDEX, 209, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region CI raw command messaging

    //*******************************************************************************************************
    //Functionality : Send CI raw command
    //InBuffer      : RAW_CMD_INFO
    //InBufferSize  : sizeof(RAW_CMD_INFO) bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_SEND_RAW_CMD = CTL_CODE(THBDA_IO_INDEX, 211, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get CI raw command data
    //InBuffer      : NULL
    //InBufferSize  : 0
    //OutBuffer     : Raw command data buffer
    //OutBufferSize : Max 1024 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_GET_RAW_CMD_DATA = CTL_CODE(THBDA_IO_INDEX, 212, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #endregion

    #endregion

    #region constants

    // GUID_THBDA_TUNER
    private static readonly Guid BdaExtensionPropertySet = new Guid(0xe5644cc4, 0x17a1, 0x4eed, 0xbd, 0x90, 0x74, 0xfd, 0xa1, 0xd6, 0x54, 0x23);
    // GUID_THBDA_CMD
    private static readonly Guid CommandGuid = new Guid(0x255e0082, 0x2017, 0x4b03, 0x90, 0xf8, 0x85, 0x6a, 0x62, 0xcb, 0x3d, 0x67);

    private const int InstanceSize = 32;    // The size of a property instance (KSP_NODE) parameter.
    private const int CommandSize = 40;

    private const int DeviceInfoSize = 240;
    private const int DriverInfoSize = 256;
    private const int PidFilterParamsSize = 72;
    private const int MaxPidFilterPids = 32;
    private const int LnbParamsSize = 20;
    private const int DiseqcMessageSize = 16;
    private const int MaxDiseqcMessageLength = 12;
    private const int CiStateInfoSize = 48;
    private const int OldCiStateInfoSize = 8;
    private const int ApplicationInfoSize = 76;
    private const int TuningParamsSize = 16;

    private const int MmiHandlerThreadSleepTime = 500;    // unit = ms

    // TerraTec have entended the length and number of
    // possible CAM menu entries in the MMI data struct
    // returned by their drivers.
    private const int DefaultMmiDataSize = 1684;
    private const int DefaultMaxCamMenuEntries = 9;

    private const int TerraTecMmiDataSize = 33944;
    private const int TerraTecMaxCamMenuEntries = 255;

    #endregion

    #region variables

    private bool _isTwinhan = false;
    private bool _isTerraTec = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private bool _isPidFilterSupported = false;
    private bool _isPidFilterBypassSupported = true;

    // Functions that are called from both the main TV service threads
    // as well as the MMI handler thread use their own local buffer to
    // avoid buffer data corruption. Otherwise functions called exclusively
    // by the MMI handler thread use the MMI buffer and other functions
    // use the general buffer.
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;
    private CardType _tunerType = CardType.Unknown;
    private String _tunerFilterName = String.Empty;

    private TwinhanCiSupport _ciApiVersion = TwinhanCiSupport.Unsupported;
    private int _maxPidFilterPids = MaxPidFilterPids;
    private int _mmiDataSize = DefaultMmiDataSize;

    private Thread _mmiHandlerThread = null;
    private bool _stopMmiHandlerThread = false;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Get the conditional access interface status.
    /// </summary>
    /// <param name="ciState">State of the CI slot.</param>
    /// <param name="mmiState">State of the MMI.</param>
    /// <returns>an HRESULT indicating whether the CI status was successfully retrieved</returns>
    private int GetCiStatus(out TwinhanCiState ciState, out TwinhanMmiState mmiState)
    {
      ciState = TwinhanCiState.Empty;
      mmiState = TwinhanMmiState.Idle;
      int bufferSize = CiStateInfoSize;
      if (_ciApiVersion == TwinhanCiSupport.Unsupported)
      {
        return 1; // Fail...
      }
      if (_ciApiVersion == TwinhanCiSupport.ApiVersion1)
      {
        ciState = TwinhanCiState.Empty_Old;
        bufferSize = OldCiStateInfoSize;
      }

      // Use local buffers here because this function is called from the MMI polling thread as well as
      // indirectly from the main TV service thread.
      IntPtr commandBuffer = Marshal.AllocCoTaskMem(CommandSize);
      IntPtr responseBuffer = Marshal.AllocCoTaskMem(bufferSize);
      for (int i = 0; i < bufferSize; i++)
      {
        Marshal.WriteByte(responseBuffer, i, 0);
      }
      try
      {
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_GET_STATE, IntPtr.Zero, 0, responseBuffer, bufferSize);
        int returnedByteCount;
        int hr = command.Execute(_propertySet, out returnedByteCount);
        if (hr == 0)
        {
          ciState = (TwinhanCiState)Marshal.ReadInt32(responseBuffer, 0);
          mmiState = (TwinhanMmiState)Marshal.ReadInt32(responseBuffer, 4);
          //DVB_MMI.DumpBinary(responseBuffer, 0, returnedByteCount);
        }
        return hr;
      }
      finally
      {
        Marshal.FreeCoTaskMem(commandBuffer);
        commandBuffer = IntPtr.Zero;
        Marshal.FreeCoTaskMem(responseBuffer);
        responseBuffer = IntPtr.Zero;
      }
    }

    #region hardware/software information

    /// <summary>
    /// Attempt to read the device information from the device.
    /// </summary>
    private void ReadDeviceInfo()
    {
      Log.Debug("Twinhan: read device information");
      for (int i = 0; i < DeviceInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, 0);
      }
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_GET_DEVICE_INFO, IntPtr.Zero, 0, _generalBuffer, DeviceInfoSize);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr != 0)
      {
        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      //Log.Debug("Twinhan: number of DeviceInfo bytes returned is {0}", returnedByteCount);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);
      DeviceInfo deviceInfo = (DeviceInfo)Marshal.PtrToStructure(_generalBuffer, typeof(DeviceInfo));
      Log.Debug("  name                        = {0}", deviceInfo.Name);
      Log.Debug("  supported modes             = {0}", deviceInfo.Type.ToString());
      Log.Debug("  speed/interface             = {0}", deviceInfo.Speed);
      Log.Debug("  MAC address                 = {0}", BitConverter.ToString(deviceInfo.MacAddress));
      Log.Debug("  CI support                  = {0}", deviceInfo.CiSupport);
      Log.Debug("  TS packet length            = {0}", deviceInfo.TsPacketLength);
      // Handle the PID filter paramter bytes carefully - not all drivers actually return
      // meaningful values for them.
      if (deviceInfo.IsPidFilterBypassSupported == 0x01)
      {
        _isPidFilterSupported = true;
        if (deviceInfo.IsPidFilterBypassSupported == 0)
        {
          _isPidFilterBypassSupported = false;
        }
      }
      Log.Debug("  PID filter supported        = {0}", _isPidFilterSupported);
      Log.Debug("  PID filter bypass supported = {0}", _isPidFilterBypassSupported);

      _ciApiVersion = deviceInfo.CiSupport;
    }

    /// <summary>
    /// Attempt to read the PID filter implementation details from the device.
    /// </summary>
    private void ReadPidFilterInfo()
    {
      Log.Debug("Twinhan: read PID filter information");
      for (int i = 0; i < PidFilterParamsSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, 0);
      }
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_GET_PID_FILTER_INFO, IntPtr.Zero, 0, _generalBuffer, PidFilterParamsSize);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr != 0)
      {
        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      //Log.Debug("Twinhan: number of PidFilterParams bytes returned is {0}", returnedByteCount);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);
      PidFilterParams pidFilterInfo = (PidFilterParams)Marshal.PtrToStructure(_generalBuffer, typeof(PidFilterParams));
      Log.Debug("  current mode                = {0}", pidFilterInfo.FilterMode);
      Log.Debug("  maximum PIDs                = {0}", pidFilterInfo.MaxPids);
      _maxPidFilterPids = pidFilterInfo.MaxPids;
    }

    /// <summary>
    /// Attempt to read the driver information from the device.
    /// </summary>
    private void ReadDriverInfo()
    {
      Log.Debug("Twinhan: read driver information");
      for (int i = 0; i < DriverInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, 0);
      }
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_GET_DRIVER_INFO, IntPtr.Zero, 0, _generalBuffer, DriverInfoSize);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr != 0)
      {
        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      //Log.Debug("Twinhan: number of DriverInfo bytes returned is {0}", returnedByteCount);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);
      DriverInfo driverInfo = (DriverInfo)Marshal.PtrToStructure(_generalBuffer, typeof(DriverInfo));
      char[] majorVersion = String.Format("{0:x2}", driverInfo.DriverMajorVersion).ToCharArray();
      char[] minorVersion = String.Format("{0:x2}", driverInfo.DriverMinorVersion).ToCharArray();
      Log.Debug("  driver version              = {0}.{1}.{2}.{3}", majorVersion[0], majorVersion[1], minorVersion[0], minorVersion[1]);
      majorVersion = String.Format("{0:x2}", driverInfo.FirmwareMajorVersion).ToCharArray();
      minorVersion = String.Format("{0:x2}", driverInfo.FirmwareMinorVersion).ToCharArray();
      Log.Debug("  firmware version            = {0}.{1}.{2}.{3}", majorVersion[0], majorVersion[1], minorVersion[0], minorVersion[1]);
      Log.Debug("  date                        = {0}", driverInfo.Date);
      Log.Debug("  company                     = {0}", driverInfo.Company);
      Log.Debug("  hardware info               = {0}", driverInfo.HardwareInfo);
      Log.Debug("  CI event mode supported     = {0}", (driverInfo.CiMmiFlags & 0x01) != 0);
      Log.Debug("  simulator mode              = {0}", driverInfo.SimType);
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if there is no purpose for it.
      if (!_isTwinhan || _propertySet == null)
      {
        return;
      }

      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        Log.Debug("Twinhan: aborting old MMI handler thread");
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Debug("Twinhan: starting new MMI handler thread");
        _stopMmiHandlerThread = false;
        _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
        _mmiHandlerThread.Name = "Twinhan MMI handler";
        _mmiHandlerThread.IsBackground = true;
        _mmiHandlerThread.Priority = ThreadPriority.Lowest;
        _mmiHandlerThread.Start();
      }
    }

    /// <summary>
    /// Thread function for handling MMI responses from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      Log.Debug("Twinhan: MMI handler thread start polling");
      TwinhanCiState ciState = TwinhanCiState.Empty_Old;
      TwinhanMmiState mmiState = TwinhanMmiState.Idle;
      TwinhanCiState prevCiState = TwinhanCiState.Empty_Old;
      TwinhanMmiState prevMmiState = TwinhanMmiState.Idle;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          int hr = GetCiStatus(out ciState, out mmiState);
          if (hr != 0)
          {
            Log.Debug("Twinhan: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            Thread.Sleep(MmiHandlerThreadSleepTime);
            continue;
          }

          // Handle CI slot state changes.
          if (ciState != prevCiState)
          {
            Log.Debug("Twinhan: CI state change, old state = {0}, new state = {1}", prevCiState, ciState);
            prevCiState = ciState;
            if (ciState == TwinhanCiState.CamInserted ||
              ciState == TwinhanCiState.CamUnknown)
            {
              _isCamPresent = true;
              _isCamReady = false;
            }
            else if (ciState == TwinhanCiState.CamOkay ||
              ciState == TwinhanCiState.Cam1Okay_Old ||
              ciState == TwinhanCiState.Cam2Okay_Old)
            {
              _isCamPresent = true;
              _isCamReady = true;
            }
            else
            {
              _isCamPresent = false;
              _isCamReady = false;
            }
          }

          // Log MMI state changes.
          if (mmiState != prevMmiState)
          {
            Log.Debug("Twinhan: MMI state change, old state = {0}, new state = {1}", prevMmiState, mmiState);
          }

          // If there is no CAM present or the CAM is not ready for interaction
          // then don't attempt to communicate with the CI.
          if (!_isCamReady)
          {
            Thread.Sleep(MmiHandlerThreadSleepTime);
            continue;
          }

          if (
            // CI API v1
            mmiState == TwinhanMmiState.Menu1Okay_Old ||
            mmiState == TwinhanMmiState.Menu2Okay_Old ||
            // CI API v2
            (prevMmiState != mmiState && mmiState == TwinhanMmiState.MenuOkay)
          )
          {
            MmiData mmi;
            if (ReadMmi(out mmi))
            {
              if (mmi.IsEnquiry)
              {
                Log.Debug("Twinhan: enquiry");
                Log.Debug("  blind     = {0}", mmi.IsBlindAnswer);
                Log.Debug("  length    = {0}", mmi.AnswerLength);
                Log.Debug("  text      = {0}", mmi.Prompt);
                Log.Debug("  type      = {0}", mmi.Type);
                if (_ciMenuCallbacks != null)
                {
                  try
                  {
                    _ciMenuCallbacks.OnCiRequest(mmi.IsBlindAnswer, (uint)mmi.AnswerLength, mmi.Prompt);
                  }
                  catch (Exception ex)
                  {
                    Log.Debug("Twinhan: menu request callback exception\r\n{0}", ex.ToString());
                  }
                }
                else
                {
                  Log.Debug("Twinhan: menu callbacks are not set");
                }
              }
              else
              {
                Log.Debug("Twinhan: menu");

                if (_ciMenuCallbacks == null)
                {
                  Log.Debug("Twinhan: menu callbacks are not set");
                }

                Log.Debug("  title     = {0}", mmi.Title);
                Log.Debug("  sub-title = {0}", mmi.SubTitle);
                Log.Debug("  footer    = {0}", mmi.Footer);
                Log.Debug("  # entries = {0}", mmi.EntryCount);
                if (_ciMenuCallbacks != null)
                {
                  try
                  {
                    _ciMenuCallbacks.OnCiMenu(mmi.Title, mmi.SubTitle, mmi.Footer, mmi.EntryCount);
                  }
                  catch (Exception ex)
                  {
                    Log.Debug("Twinhan: menu header callback exception\r\n{0}", ex.ToString());
                  }
                }
                for (int i = 0; i < mmi.EntryCount; i++)
                {
                  Log.Debug("  entry {0,-2}  = {1}", i + 1, mmi.Entries[i]);
                  if (_ciMenuCallbacks != null)
                  {
                    try
                    {
                      _ciMenuCallbacks.OnCiMenuChoice(i, mmi.Entries[i]);
                    }
                    catch (Exception ex)
                    {
                      Log.Debug("Twinhan: menu choice callback exception\r\n{0}", ex.ToString());
                    }
                  }
                }
                Log.Debug("  type      = {0}", mmi.Type);
              }
            }
            else
            {
              CloseCIMenu();
            }
          }
          else if (
            // CI API v1
            mmiState == TwinhanMmiState.Menu1Close_Old ||
            mmiState == TwinhanMmiState.Menu2Close_Old ||
            // CI API v2
            mmiState == TwinhanMmiState.MenuClose)
          {
            Log.Debug("Twinhan: menu close request");
            if (_ciMenuCallbacks != null)
            {
              try
              {
                _ciMenuCallbacks.OnCiCloseDisplay(0);
              }
              catch (Exception ex)
              {
                Log.Debug("Twinhan: close menu callback exception\r\n{0}", ex.ToString());
              }
            }
            else
            {
              Log.Debug("Twinhan: menu callbacks are not set");
            }
            CloseCIMenu();
          }
          prevMmiState = mmiState;

          Thread.Sleep(MmiHandlerThreadSleepTime);
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Debug("Twinhan: exception in MMI handler thread\r\n{0}", ex.ToString());
        return;
      }
    }

    /// <summary>
    /// Build and send an MMI response from the user to the CAM.
    /// </summary>
    /// <param name="mmi">The response object from the user.</param>
    /// <returns><c>true</c> if the message was successfully sent, otherwise <c>false</c></returns>
    private bool SendMmi(MmiData mmi)
    {
      Log.Debug("Twinhan: send MMI message");

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("Twinhan: the CAM is not ready");
        return false;
      }

      int hr;
      lock (this)
      {
        mmi.WriteToBuffer(_mmiBuffer, _isTerraTec);
        //DVB_MMI.DumpBinary(_mmiBuffer, 0, _mmiDataSize);
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_ANSWER, IntPtr.Zero, 0, _mmiBuffer, _mmiDataSize);
        int returnedByteCount;
        hr = command.Execute(_propertySet, out returnedByteCount);
        if (hr == 0)
        {
          Log.Debug("Twinhan: result = success");
          return true;
        }
      }

      Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Read and parse an MMI response from the CAM into an MmiData object.
    /// </summary>
    /// <param name="mmi">The parsed response from the CAM.</param>
    /// <returns><c>true</c> if the response from the CAM was successfully parsed, otherwise <c>false</c></returns>
    private bool ReadMmi(out MmiData mmi)
    {
      Log.Debug("Twinhan: read MMI response");
      mmi = new MmiData();
      int hr;
      lock (this)
      {
        for (int i = 0; i < _mmiDataSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_GET_MMI, IntPtr.Zero, 0, _mmiBuffer, _mmiDataSize);
        int returnedByteCount;
        hr = command.Execute(_propertySet, out returnedByteCount);
        if (hr == 0 && returnedByteCount == _mmiDataSize)
        {
          Log.Debug("Twinhan: result = success");
          //DVB_MMI.DumpBinary(_mmiBuffer, 0, returnedByteCount);
          mmi.ReadFromBuffer(_mmiBuffer, _isTerraTec);
          return true;
        }
      }

      Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        if (_isTerraTec)
        {
          return "TerraTec";
        }
        return "Twinhan";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Log.Debug("Twinhan: initialising device");

      if (tunerFilter == null)
      {
        Log.Debug("Twinhan: tuner filter is null");
        return false;
      }
      if (_isTwinhan)
      {
        Log.Debug("Twinhan: device is already initialised");
        return true;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Debug("Twinhan: pin is not a property set");
        if (pin != null)
        {
          DsUtils.ReleaseComObject(pin);
          pin = null;
        }
        return false;
      }

      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CHECK_INTERFACE, IntPtr.Zero, 0, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr != 0)
      {
        Log.Debug("Twinhan: device does not support the Twinhan property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      Log.Debug("Twinhan: supported device detected");
      _isTwinhan = true;
      _tunerType = tunerType;

      FilterInfo tunerFilterInfo;
      hr = tunerFilter.QueryFilterInfo(out tunerFilterInfo);
      if (tunerFilterInfo.pGraph != null)
      {
        DsUtils.ReleaseComObject(tunerFilterInfo.pGraph);
        tunerFilterInfo.pGraph = null;
      }
      if (hr != 0 || tunerFilterInfo.achName == null)
      {
        Log.Debug("Twinhan: failed to get the tuner filter name, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        _tunerFilterName = tunerFilterInfo.achName;
        if (_tunerFilterName.ToLowerInvariant().Contains("terratec") || _tunerFilterName.ToLowerInvariant().Contains("cinergy"))
        {
          Log.Debug("Twinhan: this device is using a TerraTec driver");
          _isTerraTec = true;
        }
      }
      _generalBuffer = Marshal.AllocCoTaskMem(DriverInfoSize);

      ReadDeviceInfo();
      if (_isPidFilterSupported)
      {
        ReadPidFilterInfo();
      }
      ReadDriverInfo();
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked when device initialisation is complete.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnInitialised(ITVCard tuner, out DeviceAction action)
    {
      // The TerraTec H7 and TechniSat CableStar Combo HD CI require *very* careful graph management. If the graph
      // is left idle for any length of time (a few minutes) they will fail to (re)start streaming. In addition,
      // they require the graph to be restarted if tuning fails, otherwise they don't seem to behave properly in
      // subsequent tune requests.
      // We start the graph immediately to prevent the graph from being left idle.
      action = DeviceAction.Start;
    }

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out DeviceAction action)
    {
      Log.Debug("Twinhan: on before tune callback");
      action = DeviceAction.Default;

      if (!_isTwinhan)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return;
      }

      // We only need to tweak the modulation for DVB-S2 channels.
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }

      // Modulation for DVB-S2 QPSK should be ModQpsk... which is what we already use to indicate DVB-S2 QPSK
      // in our tuning parameter files.
      if (ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.Mod8Vsb;
      }
      else if (ch.ModulationType == ModulationType.Mod16Apsk)
      {
        ch.ModulationType = ModulationType.Mod16Vsb;
      }
      else if (ch.ModulationType == ModulationType.Mod32Apsk)
      {
        ch.ModulationType = ModulationType.ModOqpsk;
      }
      Log.Debug("  modulation = {0}", ch.ModulationType);
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted, when the device is running but before
    /// signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnRunning(ITVCard tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    /// <summary>
    /// This callback is invoked before the device is stopped.
    /// </summary>
    /// <param name="tuner">The device instance that this device instance is associated with.</param>
    /// <param name="action">As an input, the action that TV Server wants to take; as an output, the action to take.</param>
    public override void OnStop(ITVCard tuner, ref DeviceAction action)
    {
      // The TerraTec H7 and TechniSat CableStar Combo HD CI require *very* careful graph management. If the graph
      // is left idle for any length of time (a few minutes) they will fail to (re)start streaming. In addition,
      // they require the graph to be restarted if tuning fails, otherwise they don't seem to behave properly in
      // subsequent tune requests.
      if (action == DeviceAction.Stop || action == DeviceAction.Pause)
      {
        action = DeviceAction.Restart;
      }
    }

    #endregion

    #endregion

    #region IAddOnDevice member

    /// <summary>
    /// Insert and connect the device's additional filter(s) into the BDA graph.
    /// [network provider]->[tuner]->[capture]->[...device filter(s)]->[infinite tee]->[MPEG 2 demultiplexer]->[transport information filter]->[transport stream writer]
    /// </summary>
    /// <remarks>
    /// This implementation exists only to provide a work around driver issues with the TerraTec H7 and TechniSat
    /// CableStar Combo HD CI. The short summary is that older drivers wrongly indicate that the tuner filter is
    /// also the capture filter, and this causes graph building to fail because TV Server does not attempt to add
    /// a capture filter. According to user reports, the older drivers are more stable with respect to CI
    /// performance when multiple devices are connected. See the following thread for more details:
    /// http://forum.team-mediaportal.com/threads/tv-server-channel-scan-no-channels-detected.95913/
    /// </remarks>
    /// <param name="graphBuilder">The graph builder to use to insert the device filter(s).</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter)
    {
      Log.Debug("Twinhan: add to graph");

      String lowerFilterName = _tunerFilterName.ToLowerInvariant();
      if (
        !lowerFilterName.Equals("technisat udst7000bda dvb-c ctuner") &&
        !lowerFilterName.Equals("technisat udst7000bda dvb-t vtuner") &&
        !lowerFilterName.Equals("terratec h7 digital tuner (dvb-c)") &&
        !lowerFilterName.Equals("terratec h7 digital tuner (dvb-t)")
      )
      {
        // No problem with non-H7 devices.
        return true;
      }

      Log.Debug("Twinhan: TerraTec H7 compatible device detected");
      bool captureFilterRequired = false;
      IKsTopologyInfo topologyInfo = lastFilter as IKsTopologyInfo;
      if (topologyInfo != null)
      {
        int categoryCount;
        topologyInfo.get_NumCategories(out categoryCount);
        for (int i = 0; i < categoryCount; i++)
        {
          Guid guid;
          topologyInfo.get_Category(i, out guid);
          if (guid == FilterCategory.BDASourceFiltersCategory)
          {
            captureFilterRequired = true;
            break;
          }
        }
      }
      else
      {
        Log.Debug("Twinhan: filter does not have topology information, assuming filter not required");
        return true;
      }

      if (!captureFilterRequired)
      {
        Log.Debug("Twinhan: filter is not a tuner filter so a capture filter is not required");
        return true;
      }
      // TODO - add the capture/receiver filter... or maybe we can ask people who want to use the old drivers that
      // exhibit this bug to mod their registry.
      return true;
    }

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      Log.Debug("Twinhan: set power state, on = {0}", powerOn);

      // It is not known for certain whether any Twinhan DVB-T tuners are able to
      // supply power to the aerial, however the FAQs on TerraTec's website suggest
      // that none are able.
      // In practise it seems that there is no problem attempting to enable power
      // for certain DVB-T tuners however attempting to execute this function with
      // a TerraTec H7 causes a hard crash.
      // Unfortunately there is no way to check whether this property is actually
      // supported because of the way the Twinhan API has been implemented (ie. one
      // KsProperty with the actual property codes encoded in the property data).
      // For that reason and for safety's sake we only execute this function for
      // satellite tuners.
      if (_tunerType != CardType.DvbS)
      {
        Log.Debug("Twinhan: function disabled for safety");
        return true;    // Don't retry...
      }

      if (powerOn)
      {
        Marshal.WriteByte(_generalBuffer, 0, 0x01);
      }
      else
      {
        Marshal.WriteByte(_generalBuffer, 0, 0x00);
      }
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_SET_TUNER_POWER, _generalBuffer, 1, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the device implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      // Tuning of DVB-C, DVB-S/2 and DVB-T/2 channels is supported with an appropriate tuner. The interface
      // probably supports ATSC as well, however I'm not sure how to implement that since we tune ATSC by
      // channel number rather than frequency.
      if ((channel is DVBCChannel && _tunerType == CardType.DvbC) ||
        (channel is DVBSChannel && _tunerType == CardType.DvbS) ||
        (channel is DVBTChannel && _tunerType == CardType.DvbT))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <remarks>
    /// This interface has been tested unsuccessfully for satellite and terrestrial tuning with a
    /// DigitalNow Quattro S-T (OEM 6090 2xDVB-S + 2xDVB-T/analog). It returned HRESULT 0x8007001f for the
    /// THBDA_IOCTL_LOCK_TUNER call, probably indicating that the driver does not support does not support
    /// this IOCTL. It is possible that the IOCTLs are only implemented for certain models.
    /// </remarks>
    /// <param name="channel">The channel to tune.</param>
    /// <param name="parameters">Tuning time restriction settings.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      Log.Debug("Twinhan: tune to channel");

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }

      int returnedByteCount;
      int hr;
      TuningParams tuningParams = new TuningParams();
      if (channel is DVBCChannel)
      {
        DVBCChannel ch = channel as DVBCChannel;
        tuningParams.Frequency = (UInt32)ch.Frequency;
        tuningParams.SymbolRate = (UInt32)ch.SymbolRate;
        tuningParams.Modulation = ch.ModulationType;
      }
      else if (channel is DVBSChannel)
      {
        DVBSChannel dvbsChannel = channel as DVBSChannel;
        LnbParams lnbParams = new LnbParams();
        lnbParams.PowerOn = true;
        lnbParams.ToneBurst = TwinhanToneBurst.Off;
        lnbParams.LowBandLof = (UInt32)dvbsChannel.LnbType.LowBandFrequency;
        lnbParams.HighBandLof = (UInt32)dvbsChannel.LnbType.HighBandFrequency;
        lnbParams.SwitchFrequency = (UInt32)dvbsChannel.LnbType.SwitchFrequency;
        lnbParams.Tone22k = Twinhan22k.Off;
        if (dvbsChannel.Frequency > dvbsChannel.LnbType.SwitchFrequency)
        {
          lnbParams.Tone22k = Twinhan22k.On;
        }
        lnbParams.DiseqcPort = TwinhanDiseqcPort.PortA;

        Marshal.StructureToPtr(lnbParams, _generalBuffer, true);
        TwinhanCommand lcommand = new TwinhanCommand(THBDA_IOCTL_SET_LNB_DATA, _generalBuffer, LnbParamsSize, IntPtr.Zero, 0);
        hr = lcommand.Execute(_propertySet, out returnedByteCount);
        if (hr == 0)
        {
          Log.Debug("Twinhan: result = success");
        }
        else
        {
          Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }

        tuningParams.Frequency = (UInt32)dvbsChannel.Frequency;
        tuningParams.SymbolRate = (UInt32)dvbsChannel.SymbolRate;
        tuningParams.Modulation = (ModulationType)1;
      }
      else if (channel is DVBTChannel)
      {
        DVBTChannel ch = channel as DVBTChannel;
        tuningParams.Frequency = (UInt32)ch.Frequency;
        tuningParams.Bandwidth = (UInt32)ch.BandWidth;
        tuningParams.Modulation = 0;  // ???
      }
      else
      {
        Log.Debug("Twinhan: tuning not supported");
        return false;
      }
      tuningParams.LockWaitForResult = true;

      Marshal.StructureToPtr(tuningParams, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, TuningParamsSize);

      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_LOCK_TUNER, _generalBuffer, TuningParamsSize, IntPtr.Zero, 0);
      hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
      }
      else
      {
        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      for (int i = 0; i < LnbParamsSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      command = new TwinhanCommand(THBDA_IOCTL_GET_LNB_DATA, IntPtr.Zero, 0, _generalBuffer, LnbParamsSize);
      hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
      }
      else
      {
        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      DVB_MMI.DumpBinary(_generalBuffer, 0, LnbParamsSize);


      Marshal.WriteInt32(_generalBuffer, 0);
      command = new TwinhanCommand(THBDA_IOCTL_GET_SIGNAL_Q_S, IntPtr.Zero, 0, _generalBuffer, 4);
      hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
        DVB_MMI.DumpBinary(_generalBuffer, 0, 4);
      }
      else
      {
        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return true;
    }

    #endregion

    #region IPidFilterController member

    /// <summary>
    /// Configure the PID filter.
    /// </summary>
    /// <param name="pids">The PIDs to allow through the filter.</param>
    /// <param name="modulation">The current multiplex/transponder modulation scheme.</param>
    /// <param name="forceEnable">Set this parameter to <c>true</c> to force the filter to be enabled.</param>
    /// <returns><c>true</c> if the PID filter is configured successfully, otherwise <c>false</c></returns>
    public bool SetFilterPids(HashSet<UInt16> pids, ModulationType modulation, bool forceEnable)
    {
      Log.Debug("Twinhan: set PID filter PIDs, modulation = {0}, force enable = {1}", modulation, forceEnable);

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }
      // This function is untested but as far as I'm aware, PID filtering is only supported by the VP-7021
      // (Starbox) and VP-7041 (Magicbox) models.
      if (!_isPidFilterSupported)
      {
        Log.Debug("Twinhan: PID filtering not supported");
        return true;    // Don't retry...
      }

      // It is not ideal to have to enable PID filtering because doing so can limit the number of channels
      // that can be viewed/recorded simultaneously. One or both of the Starbox and Magicbox are USB 1
      // devices which means that they are not capable of even carrying full DVB-S transponders.
      PidFilterParams pidFilterParams = new PidFilterParams();
      pidFilterParams.FilterPids = new UInt16[MaxPidFilterPids];
      pidFilterParams.FilterMode = TwinhanPidFilterMode.Disabled;
      pidFilterParams.ValidPidMask = 0;
      if (pids == null || pids.Count == 0 || (_isPidFilterBypassSupported && !forceEnable))
      {
        Log.Debug("Twinhan: disabling PID filter");
      }
      else
      {
        // If we get to here then the default approach is to enable the filter, but there is one other
        // constraint that applies: the filter PID limit.
        pidFilterParams.FilterMode = TwinhanPidFilterMode.Whitelist;
        if (pids.Count > _maxPidFilterPids)
        {
          Log.Debug("Twinhan: too many PIDs, hardware limit = {0}, actual count = {1}", _maxPidFilterPids, pids.Count);
          // When the forceEnable flag is set, we just set as many PIDs as possible.
          if (_isPidFilterBypassSupported && !forceEnable)
          {
            Log.Debug("Twinhan: disabling PID filter");
            pidFilterParams.FilterMode = TwinhanPidFilterMode.Disabled;
          }
        }

        if (pidFilterParams.FilterMode != TwinhanPidFilterMode.Disabled)
        {
          Log.Debug("Twinhan: enabling PID filter");
          HashSet<UInt16>.Enumerator en = pids.GetEnumerator();
          byte pidCount = 0;
          while (en.MoveNext() && pidCount < _maxPidFilterPids)
          {
            pidFilterParams.FilterPids[pidCount++] = en.Current;
            pidFilterParams.ValidPidMask = (pidFilterParams.ValidPidMask << 1) | 0x01;
          }
        }
      }

      Marshal.StructureToPtr(pidFilterParams, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, PidFilterParamsSize);
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_SET_PID_FILTER_INFO, _generalBuffer, PidFilterParamsSize, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenInterface()
    {
      Log.Debug("Twinhan: open conditional access interface");

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }
      if (_mmiBuffer != IntPtr.Zero)
      {
        Log.Debug("Twinhan: interface is already open");
        return false;
      }

      // We can't tell whether a CI slot is actually connected, but we can tell if this device supports
      // a CI slot.
      if (_ciApiVersion == TwinhanCiSupport.Unsupported)
      {
        Log.Debug("Twinhan: device doesn't have a CI slot");
        return false;
      }

      if (_isTerraTec)
      {
        _mmiDataSize = TerraTecMmiDataSize;
      }
      else
      {
        _mmiDataSize = DefaultMmiDataSize;
      }
      _mmiBuffer = Marshal.AllocCoTaskMem(_mmiDataSize);

      Log.Debug("Twinhan: update CI/CAM state");
      _isCamPresent = false;
      _isCamReady = false;
      TwinhanCiState ciState;
      TwinhanMmiState mmiState;
      int hr = GetCiStatus(out ciState, out mmiState);
      if (hr != 0)
      {
        Log.Debug("Twinhan: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        if (ciState == TwinhanCiState.CamInserted ||
          ciState == TwinhanCiState.CamUnknown)
        {
          _isCamPresent = true;
          _isCamReady = false;
        }
        else if (ciState == TwinhanCiState.CamOkay ||
          ciState == TwinhanCiState.Cam1Okay_Old ||
          ciState == TwinhanCiState.Cam2Okay_Old)
        {
          _isCamPresent = true;
          _isCamReady = true;
        }
      }

      StartMmiHandlerThread();

      Log.Debug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      Log.Debug("Twinhan: close conditional access interface");

      if (_mmiHandlerThread != null && _mmiHandlerThread.IsAlive)
      {
        _stopMmiHandlerThread = true;
        // In the worst case scenario it should take approximately
        // twice the thread sleep time to cleanly stop the thread.
        Thread.Sleep(MmiHandlerThreadSleepTime * 2);
        _mmiHandlerThread = null;
      }

      if (_mmiBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiBuffer);
        _mmiBuffer = IntPtr.Zero;
      }

      _isCamPresent = false;
      _isCamReady = false;

      Log.Debug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool rebuildGraph)
    {
      Log.Debug("Twinhan: reset conditional access interface");
      rebuildGraph = false;

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }

      CloseInterface();

      // This IOCTL does not seem to be implemented at least for the VP-1041 - the HRESULT returned seems
      // to always be 0x8007001f (ERROR_GEN_FAILURE). Either that or special conditions (graph stopped etc.)
      // are required for the reset to work. Our strategy here is to attempt the reset, and if it fails
      // request a graph rebuild.
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_RESET_DEVICE, IntPtr.Zero, 0, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr != 0)
      {
        Log.Debug("Twinhan: reset property failed, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        rebuildGraph = true;
        return true;
      }

      bool result = OpenInterface();
      Log.Debug("Twinhan: result = {0}", result);
      return result;
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      Log.Debug("Twinhan: is conditional access interface ready");

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }

      TwinhanCiState ciState;
      TwinhanMmiState mmiState;
      int hr = GetCiStatus(out ciState, out mmiState);
      if (hr != 0)
      {
        Log.Debug("Twinhan: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      Log.Debug("Twinhan: CI state = {0}, MMI state = {1}", ciState, mmiState);
      bool camReady = false;
      if (ciState == TwinhanCiState.Cam1Okay_Old ||
        ciState == TwinhanCiState.Cam2Okay_Old ||
        ciState == TwinhanCiState.CamOkay)
      {
        camReady = true;
      }
      Log.Debug("Twinhan: result = {0}", camReady);
      return camReady;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The programme map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      Log.Debug("Twinhan: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }
      if (pmt == null)
      {
        Log.Debug("Twinhan: PMT not supplied");
        return true;
      }

      // Twinhan supports the standard CA PMT format.
      byte[] caPmt = pmt.GetCaPmt(listAction, command);

      // Send the data to the CAM. Use local buffers since PMT updates are asynchronous.
      IntPtr commandBuffer = Marshal.AllocCoTaskMem(CommandSize);
      IntPtr pmtBuffer = Marshal.AllocCoTaskMem(caPmt.Length);
      try
      {
        Marshal.Copy(caPmt, 0, pmtBuffer, caPmt.Length);
        //DVB_MMI.DumpBinary(pmtBuffer, 0, caPmt.Length);
        TwinhanCommand tcommand = new TwinhanCommand(THBDA_IOCTL_CI_SEND_PMT, pmtBuffer, caPmt.Length, IntPtr.Zero, 0);
        int returnedByteCount;
        int hr = tcommand.Execute(_propertySet, out returnedByteCount);
        if (hr == 0)
        {
          Log.Debug("Twinhan: result = success");
          return true;
        }

        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      finally
      {
        Marshal.FreeCoTaskMem(commandBuffer);
        commandBuffer = IntPtr.Zero;
        Marshal.FreeCoTaskMem(pmtBuffer);
        pmtBuffer = IntPtr.Zero;
      }
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Set the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        StartMmiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      Log.Debug("Twinhan: enter menu");

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("Twinhan: the CAM is not ready");
        return false;
      }

      int hr;
      lock (this)
      {
        Log.Debug("Twinhan: application information");
        for (int i = 0; i < ApplicationInfoSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_GET_APP_INFO, IntPtr.Zero, 0, _mmiBuffer, ApplicationInfoSize);
        int returnedByteCount;
        hr = command.Execute(_propertySet, out returnedByteCount);
        if (hr != 0)
        {
          Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        ApplicationInfo info = (ApplicationInfo)Marshal.PtrToStructure(_mmiBuffer, typeof(ApplicationInfo));
        Log.Debug("  type         = {0}", (MmiApplicationType)info.ApplicationType);
        Log.Debug("  manufacturer = 0x{0:x}", info.Manufacturer);
        Log.Debug("  code         = 0x{0:x}", info.ManufacturerCode);
        Log.Debug("  menu title   = {0}", info.RootMenuTitle);

        command = new TwinhanCommand(THBDA_IOCTL_CI_INIT_MMI, IntPtr.Zero, 0, IntPtr.Zero, 0);
        hr = command.Execute(_propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Debug("Twinhan: close menu");

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("Twinhan: the CAM is not ready");
        return false;
      }

      int hr;
      lock (this)
      {
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_CLOSE_MMI, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(_propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Debug("Twinhan: select menu entry, choice = {0}", (int)choice);
      MmiData mmi = new MmiData();
      mmi.ChoiceIndex = (int)choice;
      mmi.Type = 1;
      return SendMmi(mmi);
    }

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Debug("Twinhan: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      if (cancel)
      {
        return SelectMenu(0); // 0 means "go back to the previous menu level"
      }

      MmiData mmi = new MmiData();
      mmi.AnswerLength = answer.Length;
      mmi.Answer = answer;
      mmi.Type = 3;
      return SendMmi(mmi);
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      // Not implemented.
      // Note: as tempting as it may be, do *not* implement this function using the THBDA_IOCTL_SET_LNB_DATA
      // IOCTL. There is some kind of interaction between the THBDA_IOCTL_SET_LNB_DATA and THBDA_IOCTL_SET_DiSEqC
      // IOCTLs that I was not able to work out fully. The driver seems to get confused about DiSEqC, 22k or LNB
      // settings when both IOCTLs are used.
      return true;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      Log.Debug("Twinhan: send DiSEqC command");

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("Twinhan: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Debug("Twinhan: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = command.Length;
      message.Message = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);

      Marshal.StructureToPtr(message, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcMessageSize);

      TwinhanCommand tcommand = new TwinhanCommand(THBDA_IOCTL_SET_DiSEqC, _generalBuffer, DiseqcMessageSize, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = tcommand.Execute(_propertySet, out returnedByteCount);
      /*if (hr != 0)
      {
        Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }*/

      // The above command seems to return HRESULT 0x8007001f (ERROR_GEN_FAILURE)
      // regardless of whether or not it was actually successful. I tested using
      // a TechniSat SkyStar HD2 (AKA Mantis, VP-1041, Cinergy S2 PCI HD) with
      // driver versions 1.1.1.502 (July 2009) and 1.1.2.700 (July 2010).
      // --mm1352000, 16-12-2011
      Log.Debug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      Log.Debug("Twinhan: read DiSEqC response");
      response = null;

      if (!_isTwinhan || _propertySet == null)
      {
        Log.Debug("Twinhan: device not initialised or interface not supported");
        return false;
      }

      for (int i = 0; i < DiseqcMessageSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_GET_DiSEqC, IntPtr.Zero, 0, _generalBuffer, DiseqcMessageSize);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        Log.Debug("Twinhan: result = success");
        DiseqcMessage message = (DiseqcMessage)Marshal.PtrToStructure(_generalBuffer, typeof(DiseqcMessage));
        response = new byte[message.MessageLength];
        Buffer.BlockCopy(message.Message, 0, response, 0, message.MessageLength);

        DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcMessageSize);
        return true;
      }

      Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      CloseInterface();
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (_propertySet != null)
      {
        DsUtils.ReleaseComObject(_propertySet);
        _propertySet = null;
      }
      _isTwinhan = false;
    }

    #endregion
  }
}