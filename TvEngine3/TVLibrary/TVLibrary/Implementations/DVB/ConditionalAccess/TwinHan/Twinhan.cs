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
using System.Text;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using TvLibrary.Hardware;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for Twinhan tuners, including
  /// clones from TerraTec, TechniSat and Digital Rise.
  /// </summary>
  public class Twinhan : /*ICustomTuning,*/ IDiSEqCController, ICiMenuActions, IHardwareProvider, IDisposable
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
      Fm = 0x2000
    }

    private enum TwinhanDeviceSpeed : byte
    {
      Pci = 0xff,
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
      // Old messages
      Empty_Old = 0,          // CI_STATUS_EMPTY_OLD - NON_CI_INFO
      CamOkay1_Old,           // CI_STATUS_CAM_OK1_OLD - ME0
      CamOkay2_Old,           // CI_STATUS_CAM_OK2_OLD - ME1

      // New messages
      Empty = 10,             // CI_STATUS_EMPTY
      CamInserted,            // CI_STATUS_INSERTED
      CamOkay,                // CI_STATUS_CAM_OK
      CamUnknown              // CI_STATUS_CAM_UNKNOW
    }

    private enum TwinhanMmiState : uint   // CIMessage
    {
      Uninitialised = 0,

      // Old messages
      GetMenuOkay1_Old = 3,   // MMI_STATUS_GET_MENU_OK1_OLD - MMI0
      GetMenuOkay2_Old,       // MMI_STATUS_GET_MENU_OK2_OLD - MMI1
      GetMenuClose1_Old,      // MMI_STATUS_GET_MENU_CLOSE1_OLD - MMI0_ClOSE
      GetMenuClose2_Old,      // MMI_STATUS_GET_MENU_CLOSE2_OLD - MMI1_ClOSE

      // New messages
      SendMenuAnswer = 14,    // MMI_STATUS_ANSWER_SEND - communicating with CAM 
      MenuOkay,               // MMI_STATUS_GET_MENU_OK - full menu successfully received from the CAM and ready to be retrieved
      MenuFail,               // MMI_STATUS_GET_MENU_FAIL - menu not successfully received from the CAM
      MenuInit,               // MMI_STATUS_GET_MENU_INIT - menu still being received from the CAM
      MenuClose,              // MMI_STATUS_GET_MENU_CLOSE - CAM requests that the menu be closed
      MenuClosed,             // MMI_STATUS_GET_MENU_CLOSED - CAM menu state is closed
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
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 189)]
      private byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct PidFilterParams    // PID_FILTER_INFO
    {
      public TwinhanPidFilterMode FilterMode;
      public byte MaxPids;                                    // Max number of PIDs supported by the PID filter (HW/FW limit, always <= MaxPidFilterPids).
      private byte Padding1;
      private byte Padding2;
      public UInt32 ValidPidMask;                             // A bit mask specifying the current valid PIDs. If the bit is 0 then the PID is ignored. Example: if ValidPidMask = 0x00000005 then there are 2 valid PIDs at indexes 0 and 2.
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPidFilterPids)]
      public UInt16[] FilterPids;                             // Filter PID list.
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct LnbParams  // LNB_DATA
    {
      public bool PowerOn;
      public TwinhanToneBurst ToneBurst;
      private byte Padding1;
      private byte Padding2;
      public UInt32 LowBandLof;                               // unit = kHz
      public UInt32 HighBandLof;                              // unit = kHz
      public UInt32 SwitchFrequency;                          // unit = kHz
      public Twinhan22k Tone22k;
      public TwinhanDiseqcPort DiseqcPort;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct DiseqcMessage
    {
      public Int32 MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
    }

    // New CI/MMI state info structure - CI API v2.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct CiStateInfoOld   // THCIStateOld
    {
      public TwinhanCiState CiState;
      public TwinhanMmiState MmiState;
    }

    private struct MmiMenuChoice
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct MmiData
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Title;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String SubTitle;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCamMenuChoices)]
      public MmiMenuChoice[] Choices;
      private byte Padding1;
      private byte Padding2;
      public Int32 ChoiceCount;

      public Int32 IsEnquiry;

      public Int32 IsBlindAnswer;
      public Int32 ExpectedAnswerLength;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Prompt;

      public Int32 ChoiceIndex;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public String Answer;

      public Int32 Type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct ApplicationInfo    // THAppInfo
    {
      public UInt32 ApplicationType;
      public UInt32 Manufacturer;
      public UInt32 ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public String Info;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct TuningParams   // TURNER_VALUE
    {
      [FieldOffset(0)]
      public UInt32 Frequency;                                // unit = kHz

      // Note: these two fields are unioned - they are never both required in
      // a single tune request so the bytes are reused.
      [FieldOffset(4)]
      public UInt32 SymbolRate;                               // unit = ksps
      [FieldOffset(4)]
      public UInt32 Bandwidth;                                // unit = kHz

      [FieldOffset(8)]
      public UInt32 Modulation;                               // Number of bits per symbol.
      [FieldOffset(12)]
      public bool LockWaitForResult;
    }

    #endregion

    #region constants

    // GUID_THBDA_TUNER
    private static readonly Guid BdaExtensionPropertySet = new Guid(0xe5644cc4, 0x17a1, 0x4eed, 0xbd, 0x90, 0x74, 0xfd, 0xa1, 0xd6, 0x54, 0x23);
    // GUID_THBDA_CMD
    private static readonly Guid CommandGuid = new Guid(0x255e0082, 0x2017, 0x4b03, 0x90, 0xf8, 0x85, 0x6a, 0x62, 0xcb, 0x3d, 0x67);

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
    private const int MmiDataSize = 1684;
    private const int MaxCamMenuChoices = 9;
    private const int ApplicationInfoSize = 76;
    private const int TuningParamsSize = 16;

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

      public TwinhanCommand(UInt32 controlCode, IntPtr inBuffer, Int32 inBufferSize, IntPtr outBuffer,
                               Int32 outBufferSize)
      {
        _controlCode = controlCode;
        _inBuffer = inBuffer;
        _inBufferSize = inBufferSize;
        _outBuffer = outBuffer;
        _outBufferSize = outBufferSize;
      }

      public int Execute(IKsPropertySet ps, IntPtr buffer, out int returnedByteCount)
      {
        returnedByteCount = 0;
        int hr = 1; // fail
        if (ps == null || buffer == IntPtr.Zero)
        {
          return hr;
        }

        IntPtr returnedByteCountBuffer = Marshal.AllocCoTaskMem(sizeof(int));
        try
        {
          byte[] guidAsBytes = CommandGuid.ToByteArray();
          for (int i = 0; i < guidAsBytes.Length; i++)
          {
            Marshal.WriteByte(buffer, i, guidAsBytes[i]);
          }

          Marshal.WriteInt32(buffer, 16, (Int32)_controlCode);
          Marshal.WriteInt32(buffer, 20, _inBuffer.ToInt32());
          Marshal.WriteInt32(buffer, 24, _inBufferSize);
          Marshal.WriteInt32(buffer, 28, _outBuffer.ToInt32());
          Marshal.WriteInt32(buffer, 32, _outBufferSize);
          Marshal.WriteInt32(buffer, 36, returnedByteCountBuffer.ToInt32());

          hr = ps.Set(BdaExtensionPropertySet, 0, buffer, CommandSize, buffer, CommandSize);
          if (hr == 0)
          {
            returnedByteCount = Marshal.ReadInt32(returnedByteCountBuffer);
          }
        }
        finally
        {
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

    #region variables

    private bool _isTwinhan = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;
    private bool _isPidFilterSupported = false;
    private bool _isPidFilterBypassSupported = true;

    // General use buffers. Functions that are called from both the main
    // TV service threads as well as the MMI handler thread use their own
    // local buffers to avoid buffer data corruption. Otherwise functions
    // called exclusively by the MMI handler thread use the MMI buffers
    // and other functions use the non-MMI buffers.
    private IntPtr _commandBuffer = IntPtr.Zero;
    private IntPtr _responseBuffer = IntPtr.Zero;
    private IntPtr _mmiCommandBuffer = IntPtr.Zero;
    private IntPtr _mmiResponseBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;

    private TwinhanCiSupport _ciApiVersion = TwinhanCiSupport.Unsupported;
    private int _maxPidFilterPids = MaxPidFilterPids;

    private Thread _mmiHandlerThread = null;
    private bool _stopMmiHandlerThread;
    private ICiMenuCallbacks _ciMenuCallbacks;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Twinhan"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Twinhan(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        return;
      }
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      _commandBuffer = Marshal.AllocCoTaskMem(CommandSize);
      _responseBuffer = Marshal.AllocCoTaskMem(DriverInfoSize);
      _mmiCommandBuffer = Marshal.AllocCoTaskMem(CommandSize);
      _mmiResponseBuffer = Marshal.AllocCoTaskMem(MmiDataSize);
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CHECK_INTERFACE, IntPtr.Zero, 0, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: supported tuner detected");
        _isTwinhan = true;
        ReadDeviceInfo();

        _isCiSlotPresent = IsCiSlotPresent();
        if (_isCiSlotPresent)
        {
          _isCamPresent = IsCamPresent();
          if (_isCamPresent)
          {
            _isCamReady = IsCamReady();
          }
        }

        SetLnbPowerState(true);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Twinhan-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Twinhan-compatible tuner, otherwise <c>false</c></value>
    public bool IsTwinhan
    {
      get
      {
        return _isTwinhan;
      }
    }

    /// <summary>
    /// Attempt to read the device and driver information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      Log.Log.Debug("Twinhan: read device information");
      for (int i = 0; i < DeviceInfoSize; i++)
      {
        Marshal.WriteByte(_responseBuffer, 0);
      }
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_GET_DEVICE_INFO, IntPtr.Zero, 0, _responseBuffer, DeviceInfoSize);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr != 0)
      {
        Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      //Log.Log.Debug("Twinhan: number of DeviceInfo bytes returned is {0}", returnedByteCount);
      //DVB_MMI.DumpBinary(_responseBuffer, 0, returnedByteCount);
      DeviceInfo deviceInfo = (DeviceInfo)Marshal.PtrToStructure(_responseBuffer, typeof(DeviceInfo));
      Log.Log.Debug("  name                        = {0}", deviceInfo.Name);
      Array deviceTypes = Enum.GetValues(typeof(TwinhanDeviceType));
      String supportedModes = "";
      for (int i = 0; i < deviceTypes.Length; i++)
      {
        if (((int)deviceInfo.Type & (UInt32)deviceTypes.GetValue(i)) != 0)
        {
          if (supportedModes.Length != 0)
          {
            supportedModes += ", ";
          }
          String typeName = Enum.GetName(typeof(TwinhanDeviceType), deviceTypes.GetValue(i));
          supportedModes += typeName;
        }
      }
      Log.Log.Debug("  supported modes             = {0}", supportedModes);
      Log.Log.Debug("  speed/interface             = {0}", deviceInfo.Speed);
      String macAddress = String.Empty;
      for (int i = 0; i < deviceInfo.MacAddress.Length; i++)
      {
        if (i != 0)
        {
          macAddress += "-";
        }
        macAddress += String.Format("{0:x2}", deviceInfo.MacAddress[i]);
      }
      Log.Log.Debug("  MAC address                 = 0x{0}", macAddress);
      Log.Log.Debug("  CI support                  = {0}", deviceInfo.CiSupport);
      Log.Log.Debug("  TS packet length            = {0}", deviceInfo.TsPacketLength);
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
      Log.Log.Debug("  PID filter supported        = {0}", _isPidFilterSupported);
      Log.Log.Debug("  PID filter bypass supported = {0}", _isPidFilterBypassSupported);

      _ciApiVersion = deviceInfo.CiSupport;

      if (_isPidFilterSupported)
      {
        Log.Log.Debug("Twinhan: read PID filter information");
        for (int i = 0; i < PidFilterParamsSize; i++)
        {
          Marshal.WriteByte(_responseBuffer, 0);
        }
        command = new TwinhanCommand(THBDA_IOCTL_GET_PID_FILTER_INFO, IntPtr.Zero, 0, _responseBuffer, PidFilterParamsSize);
        hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
        if (hr != 0)
        {
          Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return;
        }

        //Log.Log.Debug("Twinhan: number of PidFilterParams bytes returned is {0}", returnedByteCount);
        //DVB_MMI.DumpBinary(_responseBuffer, 0, returnedByteCount);
        PidFilterParams pidFilterInfo = (PidFilterParams)Marshal.PtrToStructure(_responseBuffer, typeof(PidFilterParams));
        Log.Log.Debug("  current mode                = {0}", pidFilterInfo.FilterMode);
        Log.Log.Debug("  maximum PIDs                = {0}", pidFilterInfo.MaxPids);

        if (pidFilterInfo.MaxPids <= MaxPidFilterPids)
        {
          _maxPidFilterPids = pidFilterInfo.MaxPids;
        }
      }

      Log.Log.Debug("Twinhan: read driver information");
      for (int i = 0; i < DriverInfoSize; i++)
      {
        Marshal.WriteByte(_responseBuffer, 0);
      }
      command = new TwinhanCommand(THBDA_IOCTL_GET_DRIVER_INFO, IntPtr.Zero, 0, _responseBuffer, DriverInfoSize);
      hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr != 0)
      {
        Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      //Log.Log.Debug("Twinhan: number of DriverInfo bytes returned is {0}", returnedByteCount);
      //DVB_MMI.DumpBinary(_responseBuffer, 0, returnedByteCount);
      DriverInfo driverInfo = (DriverInfo)Marshal.PtrToStructure(_responseBuffer, typeof(DriverInfo));
      char[] majorVersion = String.Format("{0:x2}", driverInfo.DriverMajorVersion).ToCharArray();
      char[] minorVersion = String.Format("{0:x2}", driverInfo.DriverMinorVersion).ToCharArray();
      Log.Log.Debug("  driver version              = {0}.{1}.{2}.{3}", majorVersion[0], majorVersion[1], minorVersion[0], minorVersion[1]);
      majorVersion = String.Format("{0:x2}", driverInfo.FirmwareMajorVersion).ToCharArray();
      minorVersion = String.Format("{0:x2}", driverInfo.FirmwareMinorVersion).ToCharArray();
      Log.Log.Debug("  firmware version            = {0}.{1}.{2}.{3}", majorVersion[0], majorVersion[1], minorVersion[0], minorVersion[1]);
      Log.Log.Debug("  date                        = {0}", driverInfo.Date);
      Log.Log.Debug("  company                     = {0}", driverInfo.Company);
      Log.Log.Debug("  hardware info               = {0}", driverInfo.HardwareInfo);
      Log.Log.Debug("  CI event mode supported     = {0}", (driverInfo.CiMmiFlags & 0x01) != 0);
    }

    /// <summary>
    /// Turn the LNB power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn power supply on, otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if the power supply state is set successfully, otherwise <c>false</c></returns>
    public bool SetLnbPowerState(bool powerOn)
    {
      Log.Log.Debug("Twinhan: set LNB power state, on = {0}", powerOn);
      if (powerOn)
      {
        Marshal.WriteByte(_responseBuffer, 0, 0x01);
      }
      else
      {
        Marshal.WriteByte(_responseBuffer, 0, 0x00);
      }
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_SET_TUNER_POWER, _responseBuffer, 1, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    private bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Twinhan: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      LnbParams lnbParams = new LnbParams();
      lnbParams.PowerOn = true;
      lnbParams.ToneBurst = TwinhanToneBurst.Off;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        lnbParams.ToneBurst = TwinhanToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        lnbParams.ToneBurst = TwinhanToneBurst.DataBurst;
      }
      // It is not critical to set the LNB frequencies as these are set
      // on the tuning space in the tuning request. Even when attempting
      // to use the custom tuning method you specify the intermediate
      // frequency.
      lnbParams.LowBandLof = 0;
      lnbParams.HighBandLof = 0;
      lnbParams.SwitchFrequency = 0;
      lnbParams.Tone22k = Twinhan22k.Off;
      if (tone22kState == Tone22k.On)
      {
        lnbParams.Tone22k = Twinhan22k.On;
      }
      lnbParams.DiseqcPort = TwinhanDiseqcPort.Null;

      Marshal.StructureToPtr(lnbParams, _responseBuffer, true);
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_SET_LNB_DATA, _responseBuffer, LnbParamsSize, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Reset the device. This IOCTL does not seem to be implemented at least for the
    /// VP-1041 - the HRESULT returned seems to always be 0x8007001f (ERROR_GEN_FAILURE).
    /// Either that or special conditions (graph stopped etc.) are required for the
    /// reset to work.
    /// </summary>
    public void ResetDevice()
    {
      Log.Log.Debug("Twinhan: reset device");

      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_RESET_DEVICE, IntPtr.Zero, 0, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr != 0)
      {
        Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Log.Log.Debug("Twinhan: result = success");
      _isCamPresent = IsCamPresent();
      if (_isCamPresent)
      {
        _isCamReady = IsCamReady();
      }
      else
      {
        _isCamReady = false;
      }
    }

    /// <summary>
    /// Set DVB-S2 tuning parameters that could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with DVB-S2 parameters set.</returns>
    public DVBSChannel SetTuningParameters(DVBSChannel channel)
    {
      Log.Log.Debug("Twinhan: set tuning parameters");
      if (channel.ModulationType == ModulationType.ModQpsk || channel.ModulationType == ModulationType.Mod8Psk)
      {
        channel.ModulationType = ModulationType.Mod8Vsb;
      }
      // I don't think any Twinhan tuners or clones support demodulating anything
      // higher than 8 PSK. Nevertheless...
      else if (channel.ModulationType == ModulationType.Mod16Apsk)
      {
        channel.ModulationType = ModulationType.Mod16Vsb;
      }
      else if (channel.ModulationType == ModulationType.Mod32Apsk)
      {
        channel.ModulationType = ModulationType.ModOqpsk;
      }
      Log.Log.Debug("  modulation = {0}", channel.ModulationType);
      return channel;
    }

    /// <summary>
    /// Set the PIDs for hardware pid filtering. This function is untested. As far as
    /// I'm aware PID filtering is only supported by the VP-7021 (Starbox) and
    /// VP-7041 (Magicbox) models.
    /// </summary>
    /// <param name="pids">The pids to filter</param>
    public void SetHardwareFilterPids(List<ushort> pids)
    {
      Log.Log.Debug("Twinhan: set hardware filter PIDs");
      if (!_isPidFilterSupported)
      {
        Log.Log.Debug("Twinhan: PID filtering not supported");
        return;
      }

      PidFilterParams pidFilterParams = new PidFilterParams();
      pidFilterParams.FilterPids = new ushort[MaxPidFilterPids];
      if (_isPidFilterBypassSupported && (pids == null || pids.Count == 0))
      {
        Log.Log.Debug("Twinhan: disabling PID filter");
        pidFilterParams.FilterMode = TwinhanPidFilterMode.Disabled;
        pidFilterParams.ValidPidMask = 0;
      }
      else
      {
        Log.Log.Debug("Twinhan: enabling PID filter");
        pidFilterParams.FilterMode = TwinhanPidFilterMode.Whitelist;
        pidFilterParams.ValidPidMask = 0;
        for (int i = 0; i < pids.Count; i++)
        {
          if (i == MaxPidFilterPids)
          {
            Log.Log.Debug("Twinhan: too many PIDs, hardware limit = {0}, actual count = {1}", MaxPidFilterPids, pids.Count);
            break;
          }
          Log.Log.Debug("  {0}", pids[i]);
          pidFilterParams.FilterPids[i] = pids[i];
          pidFilterParams.ValidPidMask = (pidFilterParams.ValidPidMask << 1) | 0x01;
        }
      }

      Marshal.StructureToPtr(pidFilterParams, _responseBuffer, true);
      DVB_MMI.DumpBinary(_responseBuffer, 0, PidFilterParamsSize);
      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_SET_PID_FILTER_INFO, _responseBuffer, PidFilterParamsSize, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: result = success");
      }
      else
      {
        Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
    }

    #region conditional access

    /// <summary>
    /// Gets the conditional access interface status.
    /// </summary>
    /// <param name="ciState">State of the CI slot.</param>
    /// <param name="mmiState">State of the MMI.</param>
    /// <returns>an HRESULT indicating whether the CI status was successfully retrieved</returns>
    private int GetCiStatus(out TwinhanCiState ciState, out TwinhanMmiState mmiState)
    {
      ciState = TwinhanCiState.Empty;
      mmiState = TwinhanMmiState.Uninitialised;
      int bufferSize = CiStateInfoSize;
      if (_ciApiVersion == TwinhanCiSupport.Unsupported)
      {
        return 1;
      }
      if (_ciApiVersion == TwinhanCiSupport.ApiVersion1)
      {
        ciState = TwinhanCiState.Empty_Old;
        bufferSize = OldCiStateInfoSize;
      }

      // Use local buffers here because this function is called from the MMI
      // polling thread as well as indirectly from the main TV service thread.
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
        int hr = command.Execute(_propertySet, commandBuffer, out returnedByteCount);
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
        Marshal.FreeCoTaskMem(responseBuffer);
      }
    }

    /// <summary>
    /// Determines whether a CI slot is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      Log.Log.Debug("Twinhan: is CI slot present");
      bool ciPresent = false;
      if (_ciApiVersion != TwinhanCiSupport.Unsupported)
      {
        // We can't tell whether the CI slot is actually connected, but
        // we can tell that this tuner supports a CI slot - that is good
        // enough.
        ciPresent = true;
      }
      Log.Log.Debug("Twinhan: result = {0}", ciPresent);
      return ciPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present, otherwise <c>false</c></returns>
    public bool IsCamPresent()
    {
      Log.Log.Debug("Twinhan: is CAM present");
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("Twinhan: CI slot not present");
        return false;
      }

      TwinhanCiState ciState;
      TwinhanMmiState mmiState;
      int hr = GetCiStatus(out ciState, out mmiState);
      if (hr != 0)
      {
        Log.Log.Debug("Twinhan: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      Log.Log.Debug("Twinhan: CI state = {0}, MMI state = {1}", ciState, mmiState);
      bool camPresent = false;
      if (ciState != TwinhanCiState.Empty_Old && ciState != TwinhanCiState.Empty)
      {
        camPresent = true;
      }
      Log.Log.Debug("Twinhan: result = {0}", camPresent);
      return camPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present and ready for interaction.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present and ready, otherwise <c>false</c></returns>
    public bool IsCamReady()
    {
      Log.Log.Debug("Twinhan: is CAM ready");
      if (!_isCamPresent)
      {
        Log.Log.Debug("Twinhan: CAM not present");
        return false;
      }

      TwinhanCiState ciState;
      TwinhanMmiState mmiState;
      int hr = GetCiStatus(out ciState, out mmiState);
      if (hr != 0)
      {
        Log.Log.Debug("Twinhan: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      Log.Log.Debug("Twinhan: CI state = {0}, MMI state = {1}", ciState, mmiState);
      bool camReady = false;
      if (ciState == TwinhanCiState.CamOkay1_Old ||
        ciState == TwinhanCiState.CamOkay2_Old ||
        ciState == TwinhanCiState.CamOkay)
      {
        camReady = true;
      }
      Log.Log.Debug("Twinhan: result = {0}", camReady);
      return camReady;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the service is successfully descrambled, otherwise <c>false</c></returns>
    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("Twinhan: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCamPresent)
      {
        Log.Log.Debug("Twinhan: CAM not available");
        return true;
      }
      if (pmt == null || pmt.Length == 0)
      {
        Log.Log.Debug("Twinhan: no PMT");
        return true;
      }

      // Twinhan supports the standard CA PMT format.
      ChannelInfo info = new ChannelInfo();
      info.DecodePmt(pmt);
      info.caPMT.CommandId = command;
      info.caPMT.CAPmt_Listmanagement = listAction;
      foreach (CaPmtEs es in info.caPMT.CaPmtEsList)
      {
        es.CommandId = command;
      }
      int caPmtLength;
      byte[] caPmt = info.caPMT.CaPmtStruct(out caPmtLength);

      // Send the data to the CAM. Use local buffers since PMT updates are asynchronous.
      IntPtr commandBuffer = Marshal.AllocCoTaskMem(CommandSize);
      IntPtr responseBuffer = Marshal.AllocCoTaskMem(caPmtLength);
      try
      {
        Marshal.Copy(caPmt, 0, responseBuffer, caPmtLength);
        DVB_MMI.DumpBinary(responseBuffer, 0, caPmtLength);
        TwinhanCommand tcommand = new TwinhanCommand(THBDA_IOCTL_CI_SEND_PMT, responseBuffer, caPmtLength, IntPtr.Zero, 0);
        int returnedByteCount;
        int hr = tcommand.Execute(_propertySet, commandBuffer, out returnedByteCount);
        if (hr == 0)
        {
          Log.Log.Debug("Twinhan: result = success");
          return true;
        }

        Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      finally
      {
        Marshal.FreeCoTaskMem(commandBuffer);
        Marshal.FreeCoTaskMem(responseBuffer);
      }
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Log.Debug("Twinhan: starting new MMI handler thread");
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
      Log.Log.Debug("Twinhan: MMI handler thread start polling");
      TwinhanCiState ciState = TwinhanCiState.Empty_Old;
      TwinhanMmiState mmiState = TwinhanMmiState.Uninitialised;
      TwinhanCiState prevCiState = TwinhanCiState.Empty_Old;
      TwinhanMmiState prevMmiState = TwinhanMmiState.Uninitialised;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          int hr = GetCiStatus(out ciState, out mmiState);
          if (hr != 0)
          {
            Log.Log.Debug("Twinhan: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            Thread.Sleep(500);
            continue;
          }

          // Handle CI slot state changes.
          if (ciState != prevCiState)
          {
            Log.Log.Debug("Twinhan: CI state change, old state = {0}, new state = {1}", prevCiState, ciState);
            prevCiState = ciState;
            if (ciState == TwinhanCiState.CamInserted ||
              ciState == TwinhanCiState.CamUnknown)
            {
              _isCamPresent = true;
              _isCamReady = false;
            }
            else if (ciState == TwinhanCiState.CamOkay ||
              ciState == TwinhanCiState.CamOkay1_Old ||
              ciState == TwinhanCiState.CamOkay2_Old)
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
            Log.Log.Debug("Twinhan: MMI state change, old state = {0}, new state = {1}", prevMmiState, mmiState);
            prevMmiState = mmiState;
          }

          switch (mmiState)
          {
            case TwinhanMmiState.GetMenuOkay1_Old:
            case TwinhanMmiState.GetMenuOkay2_Old:
            case TwinhanMmiState.MenuOkay:
              MmiData mmi;
              if (ReadMmi(out mmi))
              {
                if (mmi.IsEnquiry != 0)
                {
                  Log.Log.Debug("Twinhan: enquiry");
                  Log.Log.Debug("  blind     = {0}", mmi.IsBlindAnswer);
                  Log.Log.Debug("  length    = {0}", mmi.ExpectedAnswerLength);
                  Log.Log.Debug("  text      = {0}", mmi.Prompt);
                  Log.Log.Debug("  type      = {0}", mmi.Type);
                  if (_ciMenuCallbacks != null)
                  {
                    _ciMenuCallbacks.OnCiRequest(mmi.IsBlindAnswer != 0, (uint)mmi.ExpectedAnswerLength, mmi.Prompt);
                  }
                }
                else
                {
                  Log.Log.Debug("Twinhan: menu");
                  Log.Log.Debug("  title     = {0}", mmi.Title);
                  Log.Log.Debug("  sub-title = {0}", mmi.SubTitle);
                  Log.Log.Debug("  footer    = {0}", mmi.Footer);
                  Log.Log.Debug("  # choices = {0}", mmi.ChoiceCount);
                  if (mmi.ChoiceCount > MaxCamMenuChoices)
                  {
                    mmi.ChoiceCount = MaxCamMenuChoices;
                  }
                  if (_ciMenuCallbacks != null)
                  {
                    _ciMenuCallbacks.OnCiMenu(mmi.Title, mmi.SubTitle, mmi.Footer, mmi.ChoiceCount);
                  }
                  for (int i = 0; i < mmi.ChoiceCount; i++)
                  {
                    Log.Log.Debug("  choice {0}  = {1}", i + 1, mmi.Choices[i].Text);
                    if (_ciMenuCallbacks != null)
                    {
                      _ciMenuCallbacks.OnCiMenuChoice(i, mmi.Choices[i].Text);
                    }
                  }
                  Log.Log.Debug("  type      = {0}", mmi.Type);
                }
              }
              else
              {
                CloseCIMenu();
              }
              break;
            case TwinhanMmiState.GetMenuClose1_Old:
            case TwinhanMmiState.GetMenuClose2_Old:
            case TwinhanMmiState.MenuClose:
              Log.Log.Debug("Twinhan: menu close request");
              if (_ciMenuCallbacks != null)
              {
                try
                {
                  _ciMenuCallbacks.OnCiCloseDisplay(0);
                  CloseCIMenu();
                }
                catch (Exception ex)
                {
                  Log.Log.Debug("Twinhan: close CI menu error in MMI handler thread\r\n{0}", ex.ToString());
                }
              }
              break;
            default:
              break;
          }
          Thread.Sleep(500);
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Twinhan: error in MMI handler thread\r\n{0}", ex.ToString());
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
      if (!_isCamPresent)
      {
        return false;
      }

      Log.Log.Debug("Twinhan: send MMI message");
      int hr;
      lock (this)
      {
        Marshal.StructureToPtr(mmi, _mmiResponseBuffer, true);
        //DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, MmiDataSize);
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_ANSWER, IntPtr.Zero, 0, _mmiResponseBuffer, MmiDataSize);
        int returnedByteCount;
        hr = command.Execute(_propertySet, _mmiCommandBuffer, out returnedByteCount);
        if (hr == 0)
        {
          Log.Log.Debug("Twinhan: result = success");
          return true;
        }
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Read and parse an MMI response from the CAM into an MmiData object.
    /// </summary>
    /// <param name="mmi">The parsed response from the CAM.</param>
    /// <returns><c>true</c> if the response from the CAM was successfully parsed, otherwise <c>false</c></returns>
    private bool ReadMmi(out MmiData mmi)
    {
      mmi = new MmiData();

      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Twinhan: read MMI response");
      int hr;
      lock (this)
      {
        for (int i = 0; i < MmiDataSize; i++)
        {
          Marshal.WriteByte(_mmiResponseBuffer, i, 0);
        }
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_GET_MMI, IntPtr.Zero, 0, _mmiResponseBuffer, MmiDataSize);
        int returnedByteCount;
        hr = command.Execute(_propertySet, _mmiCommandBuffer, out returnedByteCount);
        if (hr == 0 && returnedByteCount == MmiDataSize)
        {
          Log.Log.Debug("Twinhan: result = success");
          //DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, returnedByteCount);
          mmi = (MmiData)Marshal.PtrToStructure(_mmiResponseBuffer, typeof(MmiData));
          return true;
        }
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Sets the CAM callback handler functions.
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
    /// Sends a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Twinhan: enter menu");
      int hr;
      lock (this)
      {
        Log.Log.Debug("Twinhan: application information");
        for (int i = 0; i < ApplicationInfoSize; i++)
        {
          Marshal.WriteByte(_mmiResponseBuffer, i, 0);
        }
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_GET_APP_INFO, IntPtr.Zero, 0, _mmiResponseBuffer, ApplicationInfoSize);
        int returnedByteCount;
        hr = command.Execute(_propertySet, _mmiCommandBuffer, out returnedByteCount);
        if (hr != 0)
        {
          Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        ApplicationInfo info = (ApplicationInfo)Marshal.PtrToStructure(_mmiResponseBuffer, typeof(ApplicationInfo));
        Log.Log.Debug("  type         = {0}", (DVB_MMI.ApplicationType)info.ApplicationType);
        Log.Log.Debug("  manufacturer = 0x{0:x}", info.Manufacturer);
        Log.Log.Debug("  code         = 0x{0:x}", info.ManufacturerCode);
        Log.Log.Debug("  information  = {0}", info.Info);

        command = new TwinhanCommand(THBDA_IOCTL_CI_INIT_MMI, IntPtr.Zero, 0, IntPtr.Zero, 0);
        hr = command.Execute(_propertySet, _mmiCommandBuffer, out returnedByteCount);
      }
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Twinhan: close menu");
      int hr;
      lock (this)
      {
        TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_CI_CLOSE_MMI, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(_propertySet, _mmiCommandBuffer, out returnedByteCount);
      }
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Twinhan: select menu entry, choice = {0}", (int)choice);
      MmiData mmi = new MmiData();
      mmi.ChoiceIndex = (int)choice;
      mmi.Type = 1;
      return SendMmi(mmi);
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, string answer)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = "";
      }
      Log.Log.Debug("Twinhan: send menu answer, answer = {0}, cancel = {1}", answer, cancel);
      MmiData mmi = new MmiData();
      if (cancel == true)
      {
        SelectMenu(0); // 0 means back
      }
      else
      {
        mmi.Answer = answer;
        mmi.Type = 3;
      }
      return SendMmi(mmi);
    }

    #endregion

    #region ICustomTuning members

    /// <summary>
    /// Check if the custom tune method supports tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the custom tune method supports tuning the channel, otherwise <c>false</c></returns>
    public bool SupportsTuningForChannel(IChannel channel)
    {
      // Tuning is supported for DVB-C, DVB-S and DVB-T channels. The interface
      // probably supports ATSC as well, however I'm not sure how to implement that
      // since we tune ATSC by channel number rather than frequency.
      if (channel is DVBCChannel || channel is DVBSChannel || channel is DVBTChannel)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a channel using the custom tune method. This interface has only been
    /// tested for satellite tuning. It does not return an error, however it appears
    /// to have absolutely no effect (meaning tuner lock is not achieved). It is
    /// possible that the IOCTLs are only implemented for certain models or that
    /// lock and signal strength/quality must be tested using the IOCTL interface
    /// in order to trigger the driver to apply the tune request.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scan parameters.</param>
    /// <returns><c>true</c> if tuning is successful, otherwise <c>false</c></returns>
    public bool CustomTune(IChannel channel, ScanParameters parameters)
    {
      Log.Log.Debug("Twinhan: tune to channel");
      if (!SupportsTuningForChannel(channel))
      {
        Log.Log.Debug("Twinhan: custom tuning not supported for this channel");
        return false;
      }

      TuningParams tuningParams = new TuningParams();
      if (channel is DVBCChannel)
      {
        DVBCChannel ch = channel as DVBCChannel;
        tuningParams.Frequency = (uint)ch.Frequency;
        tuningParams.SymbolRate = (uint)ch.SymbolRate;
        uint modulation = 0;
        if (ch.ModulationType == ModulationType.Mod16Qam)
        {
          modulation = 16;
        }
        else if (ch.ModulationType == ModulationType.Mod32Qam)
        {
          modulation = 32;
        }
        else if (ch.ModulationType == ModulationType.Mod64Qam)
        {
          modulation = 64;
        }
        else if (ch.ModulationType == ModulationType.Mod128Qam)
        {
          modulation = 128;
        }
        else if (ch.ModulationType == ModulationType.Mod256Qam)
        {
          modulation = 256;
        }
        tuningParams.Modulation = modulation;
      }
      else if (channel is DVBSChannel)
      {
        DVBSChannel ch = channel as DVBSChannel;
        int lowLof;
        int highLof;
        int switchFrequency;
        BandTypeConverter.GetDefaultLnbSetup(parameters, ch.BandType, out lowLof, out highLof, out switchFrequency);
        if (BandTypeConverter.IsHiBand(ch, parameters))
        {
          tuningParams.Frequency = (uint)(ch.Frequency - (highLof * 1000));
        }
        else
        {
          tuningParams.Frequency = (uint)(ch.Frequency - (lowLof * 1000));
        }
        tuningParams.SymbolRate = (uint)ch.SymbolRate;
        tuningParams.Modulation = 0;  // ???
      }
      else if (channel is DVBTChannel)
      {
        DVBTChannel ch = channel as DVBTChannel;
        tuningParams.Frequency = (uint)ch.Frequency;
        tuningParams.Bandwidth = (uint)(ch.BandWidth * 1000);
        tuningParams.Modulation = 0;  // ???
      }
      tuningParams.LockWaitForResult = true;

      Log.Log.Debug("  frequency   = {0} kHz", tuningParams.Frequency);
      if (channel is DVBTChannel)
      {
        Log.Log.Debug("  bandwidth   = {0} kHz", tuningParams.Bandwidth);
      }
      else
      {
        Log.Log.Debug("  symbol rate = {0} ksps", tuningParams.SymbolRate);
      }
      Log.Log.Debug("  modulation  = {0}", tuningParams.Modulation);

      Marshal.StructureToPtr(tuningParams, _responseBuffer, true);
      //DVB_MMI.DumpBinary(_responseBuffer, 0, TuningParamsSize);

      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_LOCK_TUNER, _responseBuffer, TuningParamsSize, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: result = success");
        return true;
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      bool successDiseqc = true;
      bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst toneBurst = ToneBurst.Off;
      if (channel.DisEqc == DisEqcType.SimpleA)
      {
        toneBurst = ToneBurst.ToneBurst;
      }
      else if (channel.DisEqc == DisEqcType.SimpleB)
      {
        toneBurst = ToneBurst.DataBurst;
      }

      Tone22k tone22k = Tone22k.Off;
      if (isHighBand)
      {
        tone22k = Tone22k.On;
      }

      // TODO: it is important to call SetToneState() before SendDiSEqCCommand()
      // but this may be problematic when it comes to moving this code.
      bool successTone = SetToneState(toneBurst, tone22k);

      if (channel.DisEqc != DisEqcType.None && channel.DisEqc != DisEqcType.SimpleA && channel.DisEqc != DisEqcType.SimpleB)
      {
        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                              (channel.Polarisation == Polarisation.CircularL));
        byte command = 0xf0;
        command |= (byte)(isHighBand ? 1 : 0);
        command |= (byte)((isHorizontal) ? 2 : 0);
        command |= (byte)((antennaNr - 1) << 2);
        successDiseqc = SendDiSEqCCommand(new byte[4] { 0xe0, 0x10, 0x38, command });
      }

      return (successDiseqc && successTone);
    }

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Twinhan: send DiSEqC command");

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Twinhan: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = command.Length;
      message.Message = new byte[MaxDiseqcMessageLength];
      for (int i = 0; i < command.Length; i++)
      {
        message.Message[i] = command[i];
      }

      Marshal.StructureToPtr(message, _responseBuffer, true);
      //DVB_MMI.DumpBinary(_responseBuffer, 0, DiseqcMessageSize);

      TwinhanCommand tcommand = new TwinhanCommand(THBDA_IOCTL_SET_DiSEqC, _responseBuffer, DiseqcMessageSize, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = tcommand.Execute(_propertySet, _commandBuffer, out returnedByteCount);

      // The above command seems to return HRESULT 0x8007001f (ERROR_GEN_FAILURE)
      // regardless of whether or not it was actually successful. I tested using
      // a TechniSat SkyStar HD2 (AKA Mantis, VP-1041, Cinergy S2 PCI HD) with
      // driver versions 1.1.1.502 (July 2009) and 1.1.2.700 (July 2010).
      // --mm1352000, 16-12-2011
      Log.Log.Debug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command. This function is untested.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      Log.Log.Debug("Twinhan: read DiSEqC command");

      for (int i = 0; i < DiseqcMessageSize; i++)
      {
        Marshal.WriteByte(_responseBuffer, i, 0);
      }

      TwinhanCommand command = new TwinhanCommand(THBDA_IOCTL_GET_DiSEqC, IntPtr.Zero, 0, _responseBuffer, DiseqcMessageSize);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, _commandBuffer, out returnedByteCount);
      if (hr == 0)
      {
        Log.Log.Debug("Twinhan: result = success");
        DiseqcMessage message = (DiseqcMessage)Marshal.PtrToStructure(_responseBuffer, typeof(DiseqcMessage));
        reply = new byte[message.MessageLength];
        for (int i = 0; i < message.MessageLength; i++)
        {
          reply[i] = message.Message[i];
        }

        DVB_MMI.DumpBinary(_responseBuffer, 0, DiseqcMessageSize);
        return true;
      }

      Log.Log.Debug("Twinhan: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      reply = new byte[1];
      reply[0] = 0;
      return false;
    }

    #endregion

    #region IHardwareProvider members

    /// <summary>
    /// Initialise the hardware provider.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public void Init(IBaseFilter tunerFilter)
    {
      // Not implemented.
    }

    /// <summary>
    /// Get or set a custom device index. Not applicable for Twinhan tuners.
    /// </summary>
    public int DeviceIndex
    {
      get
      {
        return 0;
      }
      set
      {
      }
    }

    /// <summary>
    /// Get or set the tuner device path. Not applicable for Twinhan tuners.
    /// </summary>
    public String DevicePath
    {
      get
      {
        return "";
      }
      set
      {
      }
    }

    /// <summary>
    /// Get the provider loading priority.
    /// </summary>
    public int Priority
    {
      get
      {
        return 10;
      }
    }

    /// <summary>
    /// Checks if hardware is supported and open the device.
    /// </summary>
    public void CheckAndOpen()
    {
      // Not implemented.
    }

    /// <summary>
    /// Returns the name of the provider.
    /// </summary>
    public String Provider
    {
      get
      {
        return "Twinhan";
      }
    }

    /// <summary>
    /// Returns the result of detection. If false the provider should be disposed.
    /// </summary>
    public bool IsSupported
    {
      get
      {
        return _isTwinhan;
      }
    }

    /// <summary>
    /// Returns the provider capabilities.
    /// </summary>
    public CapabilitiesType Capabilities
    {
      get
      {
        return CapabilitiesType.None;
      }
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close the conditional access interface and free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (_mmiHandlerThread != null)
      {
        _stopMmiHandlerThread = true;
        Thread.Sleep(1000);
      }
      if (_propertySet != null)
      {
        Release.ComObject(_propertySet);
        Marshal.FreeCoTaskMem(_commandBuffer);
        Marshal.FreeCoTaskMem(_responseBuffer);
        Marshal.FreeCoTaskMem(_mmiCommandBuffer);
        Marshal.FreeCoTaskMem(_mmiResponseBuffer);
      }
    }

    #endregion
  }
}