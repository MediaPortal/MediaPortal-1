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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC, PID filtering and remote
  /// controls for Twinhan tuners, including clones from TerraTec, TechniSat,
  /// Digital Rise and Elgato.
  /// </summary>
  public class Twinhan : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, ICustomTuner, IDiseqcDevice, IDisposable, IMpeg2PidFilter, IPowerDevice, IRemoteControlListener
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
      AnnexB = 0x0010,        // North American cable ITU-T annex B (AKA SCTE, OpenCable, clear QAM)
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

    private enum TwinhanTone22kState : byte
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
      public string Name;                                     // Example: VP1020, VP3020C, VP7045...

      public TwinhanDeviceType Type;                          // Values are bitwise AND'ed together to produce the final device type.
      public TwinhanDeviceSpeed Speed;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
      public byte[] MacAddress;
      public TwinhanCiSupport CiSupport;

      public int TsPacketLength;                              // 188 or 204

      // mm1352000: The following two bytes don't appear to always be set correctly.
      // Maybe these fields are only present for certain tuners or driver versions.
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
      public string Date;                                     // Example: "2004-12-20 18:30:00" or  "DEC 20 2004 10:22:10"  with compiler __DATE__ and __TIME__  definition s
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
      public string Company;                                  // Example: "TWINHAN" 
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string HardwareInfo;                             // Example: "PCI DVB CX-878 with MCU series", "PCI ATSC CX-878 with MCU series", "7020/7021 USB-Sat", "7045/7046 USB-Ter" etc.
      public byte CiMmiFlags;                                 // Bit 0 = event mode support (0 => not supported, 1 => supported)
      private byte Reserved;
      public TwinhanSimulatorType SimType;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 184)]
      private byte[] Reserved2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PidFilterParams    // PID_FILTER_INFO
    {
      public TwinhanPidFilterMode FilterMode;
      public byte MaxPids;                                    // Max number of PIDs supported by the PID filter (HW/FW limit, always <= MAX_PID_FILTER_PIDS).
      private ushort Padding;
      public uint ValidPidMask;                               // A bit mask specifying the current valid PIDs. If the bit is 0 then the PID is ignored. Example: if ValidPidMask = 0x00000005 then there are 2 valid PIDs at indexes 0 and 2.
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PID_FILTER_PID_COUNT)]
      public ushort[] FilterPids;                             // Filter PID list.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LnbParams  // LNB_DATA
    {
      [MarshalAs(UnmanagedType.I1)]
      public bool PowerOn;
      public TwinhanToneBurst ToneBurst;
      private ushort Padding;

      public uint LowBandLof;                               // unit = kHz
      public uint HighBandLof;                              // unit = kHz
      public uint SwitchFrequency;                          // unit = kHz
      public TwinhanTone22kState Tone22kState;
      public TwinhanDiseqcPort DiseqcPort;
      private ushort Padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DiseqcMessage
    {
      public int MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] Message;
    }

    // New CI/MMI state info structure - CI API v2.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CiStateInfo    // THCIState
    {
      public TwinhanCiState CiState;
      public TwinhanMmiState MmiState;
      public uint PmtState;
      public uint EventMessage;                             // Current event status.
      public TwinhanRawCommandState RawCmdState;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
      private uint[] Reserved;
    }

    // Old CI/MMI state info structure - CI API v1.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CiStateInfoOld   // THCIStateOld
    {
      public TwinhanCiState CiState;
      public TwinhanMmiState MmiState;
    }

    #region MMI data class

    // A private class to help us handle the two MMI data formats cleanly and easily.
    private class MmiData
    {
      public string Title = string.Empty;
      public string SubTitle = string.Empty;
      public string Footer = string.Empty;
      public List<string> Entries = new List<string>(15);
      public int EntryCount = 0;
      public bool IsEnquiry = false;
      public bool IsBlindAnswer = false;
      public int AnswerLength = 0;
      public string Prompt = string.Empty;
      public int ChoiceIndex = 0;
      public string Answer = string.Empty;
      public int Type = 0;

      public void WriteToBuffer(IntPtr buffer, bool isExtendedFormat)
      {
        if (isExtendedFormat)
        {
          ExtendedMmiData mmiData = new ExtendedMmiData();
          mmiData.AnswerLength = AnswerLength;
          mmiData.ChoiceIndex = ChoiceIndex;
          mmiData.Answer = Answer;
          mmiData.Type = Type;
          Marshal.StructureToPtr(mmiData, buffer, false);
        }
        else
        {
          DefaultMmiData mmiData = new DefaultMmiData();
          mmiData.AnswerLength = AnswerLength;
          mmiData.ChoiceIndex = ChoiceIndex;
          mmiData.Answer = Answer;
          mmiData.Type = Type;
          Marshal.StructureToPtr(mmiData, buffer, false);
        }
      }

      public void ReadFromBuffer(IntPtr buffer, bool isExtendedFormat)
      {
        if (isExtendedFormat)
        {
          ExtendedMmiData mmiData = (ExtendedMmiData)Marshal.PtrToStructure(buffer, typeof(ExtendedMmiData));
          Title = DvbTextConverter.Convert(mmiData.Title);
          SubTitle = DvbTextConverter.Convert(mmiData.SubTitle);
          Footer = DvbTextConverter.Convert(mmiData.Footer);
          EntryCount = mmiData.EntryCount;
          if (EntryCount > EXTENDED_MAX_CAM_MENU_ENTRIES)
          {
            EntryCount = EXTENDED_MAX_CAM_MENU_ENTRIES;
          }
          foreach (ExtendedMmiMenuEntry entry in mmiData.Entries)
          {
            Entries.Add(DvbTextConverter.Convert(entry.Text));
          }
          IsEnquiry = mmiData.IsEnquiry;
          IsBlindAnswer = mmiData.IsBlindAnswer;
          AnswerLength = mmiData.AnswerLength;
          Prompt = DvbTextConverter.Convert(mmiData.Prompt);
          ChoiceIndex = mmiData.ChoiceIndex;
          Answer = mmiData.Answer;
          Type = mmiData.Type;
        }
        else
        {
          DefaultMmiData mmiData = (DefaultMmiData)Marshal.PtrToStructure(buffer, typeof(DefaultMmiData));
          Title = DvbTextConverter.Convert(mmiData.Title);
          SubTitle = DvbTextConverter.Convert(mmiData.SubTitle);
          Footer = DvbTextConverter.Convert(mmiData.Footer);
          EntryCount = mmiData.EntryCount;
          if (EntryCount > DEFAULT_MAX_CAM_MENU_ENTRIES)
          {
            EntryCount = DEFAULT_MAX_CAM_MENU_ENTRIES;
          }
          foreach (DefaultMmiMenuEntry entry in mmiData.Entries)
          {
            Entries.Add(DvbTextConverter.Convert(entry.Text));
          }
          IsEnquiry = mmiData.IsEnquiry;
          IsBlindAnswer = mmiData.IsBlindAnswer;
          AnswerLength = mmiData.AnswerLength;
          Prompt = DvbTextConverter.Convert(mmiData.Prompt);
          ChoiceIndex = mmiData.ChoiceIndex;
          Answer = mmiData.Answer;
          Type = mmiData.Type;
        }
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DefaultMmiMenuEntry
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
      public byte[] Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DefaultMmiData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Title;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] SubTitle;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEFAULT_MAX_CAM_MENU_ENTRIES)]
      public DefaultMmiMenuEntry[] Entries;
      private ushort Padding1;
      public int EntryCount;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsEnquiry;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsBlindAnswer;
      public int AnswerLength;      // enquiry: expected answer length, enquiry answer: actual answer length
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Prompt;

      public int ChoiceIndex;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_STRING_LENGTH)]
      public string Answer;

      public int Type;              // 1, 2 (menu/list, select entry) or 3 (enquiry, enquiry answer)
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExtendedMmiMenuEntry
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
      public byte[] Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ExtendedMmiData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Title;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] SubTitle;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = EXTENDED_MAX_CAM_MENU_ENTRIES)]
      public ExtendedMmiMenuEntry[] Entries;
      public int EntryCount;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsEnquiry;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsBlindAnswer;
      public int AnswerLength;      // enquiry: expected answer length, enquiry answer: actual answer length
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Prompt;

      public int ChoiceIndex;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_STRING_LENGTH)]
      public string Answer;

      public int Type;              // 1, 2 (menu/list, select entry) or 3 (enquiry, enquiry answer)
    }

    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ApplicationInfo    // THAppInfo
    {
      public uint ApplicationType;
      public uint Manufacturer;
      public uint ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] RootMenuTitle;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct TuningParams   // TURNER_VALUE
    {
      [FieldOffset(0)]
      public int Frequency;                                 // unit = kHz

      // Note: these two fields are unioned - they are never both required in
      // a single tune request so the bytes are reused.
      [FieldOffset(4)]
      public int SymbolRate;                                // unit = ks/s
      [FieldOffset(4)]
      public int Bandwidth;                                 // unit = MHz

      [FieldOffset(8)]
      public ModulationType Modulation;                     // or polarisation???
      [FieldOffset(12), MarshalAs(UnmanagedType.I1)]
      public bool LockWaitForResult;
      [FieldOffset(13)]
      private byte Padding1;
      [FieldOffset(14)]
      private ushort Padding2;
    }

    // Many of these parameters appear to be obsolete.
    [StructLayout(LayoutKind.Sequential)]
    private struct RegistryParams   // THBDAREGPARAMS
    {
      public uint MceFrequencyTranslate;
      public uint FixedBandwidth;
      [MarshalAs(UnmanagedType.Bool)]
      public bool EnableOffFrequencyScan;   // Mantis drivers support this.
      [MarshalAs(UnmanagedType.Bool)]
      public bool EnableRelockMonitor;
      public uint LnbLof;
      public uint LnbLowLof;
      public uint LnbHighLof;
      public TwinhanDiseqcPort Diseqc;
      private byte Padding1;
      private ushort Padding2;
      [MarshalAs(UnmanagedType.Bool)]
      public bool LnbPower;
      public TwinhanTone22kState Tone22k;
      private byte Padding3;
      private ushort Padding4;
      public uint AtscFrequencyShift;       // Mantis drivers support this.
    }

    #endregion

    #region constants

    private const int MAX_STRING_LENGTH = 256;

    private static readonly int DEVICE_INFO_SIZE = Marshal.SizeOf(typeof(DeviceInfo));            // 240
    private static readonly int DRIVER_INFO_SIZE = Marshal.SizeOf(typeof(DriverInfo));            // 256
    private static readonly int PID_FILTER_PARAMS_SIZE = Marshal.SizeOf(typeof(PidFilterParams)); // 72
    private const int MAX_PID_FILTER_PID_COUNT = 32;
    private static readonly int LNB_PARAMS_SIZE = Marshal.SizeOf(typeof(LnbParams));              // 20
    private static readonly int DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(DiseqcMessage));      // 16
    private const int MAX_DISEQC_MESSAGE_LENGTH = 12;
    private static readonly int CI_STATE_INFO_SIZE = Marshal.SizeOf(typeof(CiStateInfo));         // 48
    private static readonly int OLD_CI_STATE_INFO_SIZE = Marshal.SizeOf(typeof(CiStateInfoOld));  // 8
    private static readonly int APPLICATION_INFO_SIZE = Marshal.SizeOf(typeof(ApplicationInfo));  // 76
    private static readonly int TUNING_PARAMS_SIZE = Marshal.SizeOf(typeof(TuningParams));        // 16
    private static readonly int REGISTRY_PARAMS_SIZE = Marshal.SizeOf(typeof(RegistryParams));    // 44

    private static readonly int GENERAL_BUFFER_SIZE = new int[]
      {
        DEVICE_INFO_SIZE, DISEQC_MESSAGE_SIZE, DRIVER_INFO_SIZE, LNB_PARAMS_SIZE,
        PID_FILTER_PARAMS_SIZE, TUNING_PARAMS_SIZE, REGISTRY_PARAMS_SIZE
      }.Max();

    private static readonly TimeSpan MMI_HANDLER_THREAD_WAIT_TIME = new TimeSpan(0, 0, 0, 0, 500);

    // Elgato and TerraTec have entended the length and number of possible CAM
    // menu entries in their drivers' MMI data struct.
    private const int DEFAULT_MMI_DATA_SIZE = 1684;
    private const int DEFAULT_MAX_CAM_MENU_ENTRIES = 9;

    private const int EXTENDED_MMI_DATA_SIZE = 33944;
    private const int EXTENDED_MAX_CAM_MENU_ENTRIES = 255;

    #endregion

    #region variables

    private bool _isTwinhan = false;
    private bool _isElgatoDriver = false;
    private bool _isElgatoPbdaDriver = false;
    private bool _isTerraTecDriver = false;
    private bool _isCaInterfaceOpen = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;

    // Satellite tuner LNB parameter cache.
    private bool _isPowerOn = false;
    private TwinhanDiseqcPort _diseqcPort = TwinhanDiseqcPort.Null;
    private TwinhanToneBurst _toneBurstCommand = TwinhanToneBurst.Off;

    // PID filter variables.
    private TwinhanDeviceSpeed _connection = TwinhanDeviceSpeed.Pcie;
    private bool _isPidFilterSupported = false;
    private bool _isPidFilterBypassSupported = true;
    private int _maxPidFilterPidCount = MAX_PID_FILTER_PID_COUNT;
    private HashSet<ushort> _pidFilterPids = new HashSet<ushort>();

    // Functions that are called from both the main TV service threads as well
    // as the MMI handler thread use their own local buffer to avoid buffer
    // data corruption. Otherwise functions called exclusively by:
    // - the MMI handler thread => MMI buffer
    // - other => general buffer.
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;
    private IoControl _ioControl = null;
    private BroadcastStandard _tunerSupportedBroadcastStandards = BroadcastStandard.Unknown;
    private string _tunerProductInstanceId = string.Empty;
    private string _tunerExternalId = string.Empty;

    private TwinhanCiSupport _ciApiVersion = TwinhanCiSupport.Unsupported;
    private int _mmiDataSize = DEFAULT_MMI_DATA_SIZE;

    private Thread _mmiHandlerThread = null;
    private ManualResetEvent _mmiHandlerThreadStopEvent = null;
    private object _mmiLock = new object();
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();

    private bool _isRemoteControlInterfaceOpen = false;
    private ITwinhanRemoteControl _remoteControlInterface = null;

    private IDiseqcDevice _elgatoPbdaDiseqcInterface = null;

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
      int bufferSize = CI_STATE_INFO_SIZE;
      if (_ciApiVersion == TwinhanCiSupport.Unsupported)
      {
        return 1; // Fail...
      }
      if (_ciApiVersion == TwinhanCiSupport.ApiVersion1)
      {
        ciState = TwinhanCiState.Empty_Old;
        bufferSize = OLD_CI_STATE_INFO_SIZE;
      }

      lock (_mmiLock)
      {
        for (int i = 0; i < bufferSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        int returnedByteCount;
        int hr = _ioControl.Get(IoControlCode.CiGetState, _mmiBuffer, bufferSize, out returnedByteCount);
        if (hr == (int)NativeMethods.HResult.S_OK && (returnedByteCount == OLD_CI_STATE_INFO_SIZE || returnedByteCount == CI_STATE_INFO_SIZE))
        {
          ciState = (TwinhanCiState)Marshal.ReadInt32(_mmiBuffer, 0);
          mmiState = (TwinhanMmiState)Marshal.ReadInt32(_mmiBuffer, 4);
          //Dump.DumpBinary(_mmiBuffer, returnedByteCount);
        }
        return hr;
      }
    }

    /// <summary>
    /// Determine the product instance identifier for the tuner.
    /// </summary>
    /// <param name="tunerExternalId">The tuner's external identifier.</param>
    /// <returns>the tuner's product instance identifier</returns>
    private string GetProductInstanceId(string tunerExternalId)
    {
      string productInstanceId = null;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in devices)
      {
        if (productInstanceId == null)
        {
          string devicePath = device.DevicePath;
          if (devicePath != null && tunerExternalId.Contains(devicePath))
          {
            productInstanceId = device.ProductInstanceIdentifier;
          }
        }
        device.Dispose();
      }
      return productInstanceId;
    }

    #region hardware/software information

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      this.LogDebug("Twinhan: read device information");
      for (int i = 0; i < DEVICE_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _ioControl.Get(IoControlCode.GetDeviceInfo, _generalBuffer, DEVICE_INFO_SIZE, out returnedByteCount);
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != DEVICE_INFO_SIZE)
      {
        this.LogWarn("Twinhan: failed to read device information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      //this.LogDebug("Twinhan: number of DeviceInfo bytes returned is {0}", returnedByteCount);
      //Dump.DumpBinary(_generalBuffer, returnedByteCount);
      DeviceInfo deviceInfo = (DeviceInfo)Marshal.PtrToStructure(_generalBuffer, typeof(DeviceInfo));
      this.LogDebug("  name                        = {0}", deviceInfo.Name);
      this.LogDebug("  supported modes             = {0}", deviceInfo.Type);
      this.LogDebug("  speed/interface             = {0}", deviceInfo.Speed);
      this.LogDebug("  MAC address                 = {0}", BitConverter.ToString(deviceInfo.MacAddress).ToLowerInvariant());
      this.LogDebug("  CI support                  = {0}", deviceInfo.CiSupport);
      this.LogDebug("  TS packet length            = {0}", deviceInfo.TsPacketLength);
      // Handle the PID filter paramter bytes carefully - not all drivers actually return
      // meaningful values for them.
      if (deviceInfo.IsPidFilterPresent == 0x01)
      {
        _isPidFilterSupported = true;
        if (deviceInfo.IsPidFilterBypassSupported == 0)
        {
          _isPidFilterBypassSupported = false;
        }
      }
      this.LogDebug("  PID filter supported        = {0}", _isPidFilterSupported);
      this.LogDebug("  PID filter bypass supported = {0}", _isPidFilterBypassSupported);

      _connection = deviceInfo.Speed;
      _ciApiVersion = deviceInfo.CiSupport;
    }

    /// <summary>
    /// Attempt to read the PID filter implementation details from the tuner.
    /// </summary>
    private void ReadPidFilterInfo()
    {
      this.LogDebug("Twinhan: read PID filter information");
      for (int i = 0; i < PID_FILTER_PARAMS_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _ioControl.Get(IoControlCode.GetPidFilterInfo, _generalBuffer, PID_FILTER_PARAMS_SIZE, out returnedByteCount);
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != PID_FILTER_PARAMS_SIZE)
      {
        this.LogWarn("Twinhan: failed to read PID filter information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      //this.LogDebug("Twinhan: number of PidFilterParams bytes returned is {0}", returnedByteCount);
      //Dump.DumpBinary(_generalBuffer, returnedByteCount);
      PidFilterParams pidFilterInfo = (PidFilterParams)Marshal.PtrToStructure(_generalBuffer, typeof(PidFilterParams));
      this.LogDebug("  current mode                = {0}", pidFilterInfo.FilterMode);
      this.LogDebug("  maximum PIDs                = {0}", pidFilterInfo.MaxPids);
      _maxPidFilterPidCount = pidFilterInfo.MaxPids;
    }

    /// <summary>
    /// Attempt to read the driver information from the tuner.
    /// </summary>
    /// <returns><c>true</c> if the driver information is read successfully, otherwise <c>false</c></returns>
    private bool ReadDriverInfo()
    {
      this.LogDebug("Twinhan: read driver information");
      for (int i = 0; i < DRIVER_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _ioControl.Get(IoControlCode.GetDriverInfo, _generalBuffer, DRIVER_INFO_SIZE, out returnedByteCount);
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != DRIVER_INFO_SIZE)
      {
        this.LogWarn("Twinhan: failed to read driver information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return false;
      }

      //this.LogDebug("Twinhan: number of DriverInfo bytes returned is {0}", returnedByteCount);
      //Dump.DumpBinary(_generalBuffer, returnedByteCount);
      DriverInfo driverInfo = (DriverInfo)Marshal.PtrToStructure(_generalBuffer, typeof(DriverInfo));
      char[] majorVersion = string.Format("{0:x2}", driverInfo.DriverMajorVersion).ToCharArray();
      char[] minorVersion = string.Format("{0:x2}", driverInfo.DriverMinorVersion).ToCharArray();
      this.LogDebug("  driver version              = {0}.{1}.{2}.{3}", majorVersion[0], majorVersion[1], minorVersion[0], minorVersion[1]);
      majorVersion = string.Format("{0:x2}", driverInfo.FirmwareMajorVersion).ToCharArray();
      minorVersion = string.Format("{0:x2}", driverInfo.FirmwareMinorVersion).ToCharArray();
      this.LogDebug("  firmware version            = {0}.{1}.{2}.{3}", majorVersion[0], majorVersion[1], minorVersion[0], minorVersion[1]);
      this.LogDebug("  date                        = {0}", driverInfo.Date);
      this.LogDebug("  company                     = {0}", driverInfo.Company);
      this.LogDebug("  hardware info               = {0}", driverInfo.HardwareInfo);
      this.LogDebug("  CI event mode supported     = {0}", (driverInfo.CiMmiFlags & 0x01) != 0);
      this.LogDebug("  simulator mode              = {0}", driverInfo.SimType);
      return true;
    }

    /// <summary>
    /// Attempt to read the device registry parameters.
    /// </summary>
    private void ReadRegistryParams()
    {
      this.LogDebug("Twinhan: read registry parameters");
      for (int i = 0; i < REGISTRY_PARAMS_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      RegistryParams registryParams = new RegistryParams();
      int returnedByteCount;
      int hr = _ioControl.Get(IoControlCode.GetRegistryParams, _generalBuffer, REGISTRY_PARAMS_SIZE, out returnedByteCount);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        //Dump.DumpBinary(_generalBuffer, returnedByteCount);
        registryParams = (RegistryParams)Marshal.PtrToStructure(_generalBuffer, typeof(RegistryParams));
        //this.LogDebug("  MCE frequency translation   = {0}", registryParams.MceFrequencyTranslate);
        //this.LogDebug("  fixed bandwidth             = {0}", registryParams.FixedBandwidth);
        this.LogDebug("  enable off frequency scan   = {0}", registryParams.EnableOffFrequencyScan);
        this.LogDebug("  enable relock monitor       = {0}", registryParams.EnableRelockMonitor);
        //this.LogDebug("  LNB LO frequency            = {0}", registryParams.LnbLof);
        //this.LogDebug("  LNB low band LO frequency   = {0}", registryParams.LnbLowLof);
        //this.LogDebug("  LNB high band LO frequency  = {0}", registryParams.LnbHighLof);
        //this.LogDebug("  DiSEqC                      = {0}", registryParams.Diseqc);
        //this.LogDebug("  LNB power                   = {0}", registryParams.LnbPower);
        //this.LogDebug("  22 kHz tone                 = {0}", registryParams.Tone22k);
        this.LogDebug("  ATSC frequency shift        = {0} kHz", registryParams.AtscFrequencyShift);
      }
      else
      {
        // The Mantis driver doesn't seem to support get, but it supports set - strange!
        this.LogWarn("Twinhan: failed to read registry parameters, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }

      if (registryParams.EnableOffFrequencyScan || !registryParams.EnableRelockMonitor || registryParams.AtscFrequencyShift != 1750)
      {
        // Note that the drivers that I tested with only updated EnableRelockMonitor and AtscFrequencyShift.
        this.LogDebug("Twinhan: updating registry parameters");
        registryParams.MceFrequencyTranslate = 0;
        registryParams.FixedBandwidth = 0;
        registryParams.EnableOffFrequencyScan = false;  // for faster DVB-T tuning
        registryParams.EnableRelockMonitor = true;      // for more robust tuner behaviour
        registryParams.LnbPower = true;
        registryParams.Tone22k = TwinhanTone22kState.Auto;
        registryParams.Diseqc = TwinhanDiseqcPort.Null;
        registryParams.AtscFrequencyShift = 1750;       // required for BDA ATSC tuning
        Marshal.StructureToPtr(registryParams, _generalBuffer, false);
        hr = _ioControl.Set(IoControlCode.SetRegistryParams, _generalBuffer, REGISTRY_PARAMS_SIZE);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("Twinhan: failed to update registry parameters, hr = 0x{0:x}", hr);
          return;
        }

        this.LogDebug("Twinhan: result = success");
      }
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread to receive MMI messages from the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isCaInterfaceOpen)
      {
        return;
      }

      lock (_mmiLock)
      {
        // Kill the existing thread if it is in "zombie" state.
        if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
        {
          StopMmiHandlerThread();
        }

        if (_mmiHandlerThread == null)
        {
          this.LogDebug("Twinhan: starting new MMI handler thread");
          _mmiHandlerThreadStopEvent = new ManualResetEvent(false);
          _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
          _mmiHandlerThread.Name = "Twinhan MMI handler";
          _mmiHandlerThread.IsBackground = true;
          _mmiHandlerThread.Priority = ThreadPriority.Lowest;
          _mmiHandlerThread.Start();
        }
      }
    }

    /// <summary>
    /// Stop the thread that receives MMI messages from the CAM.
    /// </summary>
    private void StopMmiHandlerThread()
    {
      lock (_mmiLock)
      {
        if (_mmiHandlerThread != null)
        {
          if (!_mmiHandlerThread.IsAlive)
          {
            this.LogWarn("Twinhan: aborting old MMI handler thread");
            _mmiHandlerThread.Abort();
          }
          else
          {
            _mmiHandlerThreadStopEvent.Set();
            if (!_mmiHandlerThread.Join((int)MMI_HANDLER_THREAD_WAIT_TIME.TotalMilliseconds * 2))
            {
              this.LogWarn("Twinhan: failed to join MMI handler thread, aborting thread");
              _mmiHandlerThread.Abort();
            }
          }
          _mmiHandlerThread = null;
          if (_mmiHandlerThreadStopEvent != null)
          {
            _mmiHandlerThreadStopEvent.Close();
            _mmiHandlerThreadStopEvent = null;
          }
        }
      }
    }

    /// <summary>
    /// Thread function for receiving MMI messages from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      this.LogDebug("Twinhan: MMI handler thread start polling");
      TwinhanCiState ciState = TwinhanCiState.Empty_Old;
      TwinhanMmiState mmiState = TwinhanMmiState.Idle;
      TwinhanCiState prevCiState = TwinhanCiState.Empty_Old;
      TwinhanMmiState prevMmiState = TwinhanMmiState.Idle;
      try
      {
        while (!_mmiHandlerThreadStopEvent.WaitOne(MMI_HANDLER_THREAD_WAIT_TIME))
        {
          int hr = GetCiStatus(out ciState, out mmiState);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("Twinhan: failed to get CI status, hr = 0x{0:x}", hr);
            continue;
          }

          // Handle CI slot state changes.
          if (ciState != prevCiState)
          {
            this.LogInfo("Twinhan: CI state change, old state = {0}, new state = {1}", prevCiState, ciState);
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
            this.LogInfo("Twinhan: MMI state change, old state = {0}, new state = {1}", prevMmiState, mmiState);
          }

          // If there is no CAM present or the CAM is not ready for interaction
          // then don't attempt to communicate with the CI.
          if (!_isCamReady)
          {
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
                this.LogDebug("Twinhan: enquiry");
                this.LogDebug("  blind     = {0}", mmi.IsBlindAnswer);
                this.LogDebug("  length    = {0}", mmi.AnswerLength);
                this.LogDebug("  prompt    = {0}", mmi.Prompt);
                this.LogDebug("  type      = {0}", mmi.Type);
                lock (_caMenuCallBackLock)
                {
                  if (_caMenuCallBack != null)
                  {
                    _caMenuCallBack.OnCiRequest(mmi.IsBlindAnswer, (uint)mmi.AnswerLength, mmi.Prompt);
                  }
                  else
                  {
                    this.LogDebug("Twinhan: menu call back not set");
                  }
                }
              }
              else
              {
                this.LogDebug("Twinhan: menu");

                lock (_caMenuCallBackLock)
                {
                  if (_caMenuCallBack == null)
                  {
                    this.LogDebug("Twinhan: menu call back not set");
                  }

                  this.LogDebug("  title     = {0}", mmi.Title);
                  this.LogDebug("  sub-title = {0}", mmi.SubTitle);
                  this.LogDebug("  footer    = {0}", mmi.Footer);
                  this.LogDebug("  # entries = {0}", mmi.EntryCount);
                  if (_caMenuCallBack != null)
                  {
                    _caMenuCallBack.OnCiMenu(mmi.Title, mmi.SubTitle, mmi.Footer, mmi.EntryCount);
                  }
                  for (int i = 0; i < mmi.EntryCount; i++)
                  {
                    this.LogDebug("    {0, -7} = {1}", i + 1, mmi.Entries[i]);
                    if (_caMenuCallBack != null)
                    {
                      _caMenuCallBack.OnCiMenuChoice(i, mmi.Entries[i]);
                    }
                  }
                }
                this.LogDebug("  type      = {0}", mmi.Type);
              }
            }
            else
            {
              (this as IConditionalAccessMenuActions).Close();
            }
          }
          else if (
            // CI API v1
            mmiState == TwinhanMmiState.Menu1Close_Old ||
            mmiState == TwinhanMmiState.Menu2Close_Old ||
            // CI API v2
            mmiState == TwinhanMmiState.MenuClose)
          {
            this.LogDebug("Twinhan: menu close request");
            lock (_caMenuCallBackLock)
            {
              if (_caMenuCallBack != null)
              {
                _caMenuCallBack.OnCiCloseDisplay(0);
              }
              else
              {
                this.LogDebug("Twinhan: menu call back not set");
              }
            }
            (this as IConditionalAccessMenuActions).Close();
          }
          prevMmiState = mmiState;
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Twinhan: MMI handler thread exception");
        return;
      }
      this.LogDebug("Twinhan: MMI handler thread stop polling");
    }

    /// <summary>
    /// Build and send an MMI response from the user to the CAM.
    /// </summary>
    /// <param name="mmi">The response object from the user.</param>
    /// <returns><c>true</c> if the message was successfully sent, otherwise <c>false</c></returns>
    private bool SendMmi(MmiData mmi)
    {
      this.LogDebug("Twinhan: send MMI message");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamReady)
      {
        this.LogError("Twinhan: failed to send MMI message, the CAM is not ready");
        return false;
      }

      int hr;
      lock (_mmiLock)
      {
        mmi.WriteToBuffer(_mmiBuffer, _mmiDataSize == EXTENDED_MMI_DATA_SIZE);
        //Dump.DumpBinary(_mmiBuffer, _mmiDataSize);
        int returnedByteCount;
        hr = _ioControl.Get(IoControlCode.CiAnswer, _mmiBuffer, _mmiDataSize, out returnedByteCount);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to send MMI message, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Read and parse an MMI response from the CAM into an MmiData object.
    /// </summary>
    /// <param name="mmi">The parsed response from the CAM.</param>
    /// <returns><c>true</c> if the response from the CAM was successfully parsed, otherwise <c>false</c></returns>
    private bool ReadMmi(out MmiData mmi)
    {
      this.LogDebug("Twinhan: read MMI response");
      mmi = new MmiData();
      int hr;
      lock (_mmiLock)
      {
        for (int i = 0; i < _mmiDataSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        int returnedByteCount;
        hr = _ioControl.Get(IoControlCode.CiGetMmi, _mmiBuffer, _mmiDataSize, out returnedByteCount);
        if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == _mmiDataSize)
        {
          this.LogDebug("Twinhan: result = success");
          //Dump.DumpBinary(_mmiBuffer, returnedByteCount);
          mmi.ReadFromBuffer(_mmiBuffer, _mmiDataSize == EXTENDED_MMI_DATA_SIZE);
          return true;
        }
      }

      this.LogError("Twinhan: failed to read MMI response, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Twinhan";
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Twinhan: initialising");

      if (_isTwinhan)
      {
        this.LogWarn("Twinhan: extension already initialised");
        return true;
      }

      IBaseFilter tunerFilter = context as IBaseFilter;
      if (tunerFilter == null)
      {
        this.LogDebug("Twinhan: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Twinhan: pin is not a property set");
        Release.ComObject("Twinhan filter input pin", ref pin);
        return false;
      }

      _ioControl = new IoControl(_propertySet);
      int hr = _ioControl.Set(IoControlCode.CheckInterface, IntPtr.Zero, 0);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: property set not supported, hr = 0x{0:x}", hr);
        Release.ComObject("Twinhan property set", ref _propertySet);
        return false;
      }

      this.LogInfo("Twinhan: extension supported");
      _isTwinhan = true;
      _tunerSupportedBroadcastStandards = tunerSupportedBroadcastStandards;
      _tunerExternalId = tunerExternalId;
      if (_tunerExternalId != null)
      {
        _tunerProductInstanceId = GetProductInstanceId(_tunerExternalId);
      }
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);

      FilterInfo tunerFilterInfo;
      hr = tunerFilter.QueryFilterInfo(out tunerFilterInfo);
      string tunerFilterName = tunerFilterInfo.achName;
      Release.FilterInfo(ref tunerFilterInfo);
      if (hr != (int)NativeMethods.HResult.S_OK || tunerFilterName == null)
      {
        this.LogError("Twinhan: failed to get the tuner filter name, hr = 0x{0:x}", hr);
      }
      else
      {
        tunerFilterName = tunerFilterName.ToLowerInvariant();
        if (tunerFilterName.Contains("eyetv"))
        {
          // Elgato EyeTV Sat and Elgato EyeTV Sat Free tuners expose the
          // Twinhan property set. Older drivers work fine. At some point the
          // driver seems to have been rewritten or heavily reworked. All new
          // drivers do not lock on signal if the LNB and/or DiSEqC IOCTLs are
          // used. Refer to:
          // http://forum.team-mediaportal.com/threads/tbs-5990-q-box-s2-hdtv-kein-signal.126540/
          // http://forum.team-mediaportal.com/threads/after-restart-tv-stops-playing-after-first-attempt.132016/
          this.LogDebug("Twinhan: this tuner has an Elgato driver");
          _isElgatoDriver = true;
        }
        else if (tunerFilterName.Contains("terratec") || tunerFilterName.Contains("cinergy"))
        {
          this.LogDebug("Twinhan: this tuner has a TerraTec driver");
          _isTerraTecDriver = true;
        }
      }

      ReadDeviceInfo();
      if (_isPidFilterSupported)
      {
        ReadPidFilterInfo();
      }
      bool isDriverInfoReadable = ReadDriverInfo();
      _isElgatoPbdaDriver = _isElgatoDriver && !isDriverInfoReadable;
      ReadRegistryParams();

      // Elgato EyeTV tuners with PBDA drivers must use the Microsoft BDA
      // DiSEqC interface.
      if (_isElgatoPbdaDriver && (tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) != 0)
      {
        _elgatoPbdaDiseqcInterface = new MicrosoftBdaDiseqc.MicrosoftBdaDiseqc();
        if (!_elgatoPbdaDiseqcInterface.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context))
        {
          this.LogWarn("Twinhan: failed to initialise Elgato PBDA DiSEqC interface");
          IDisposable d = _elgatoPbdaDiseqcInterface as IDisposable;
          if (d != null)
          {
            d.Dispose();
          }
          _elgatoPbdaDiseqcInterface = null;
        }
      }
      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnLoaded(ITuner tuner, out TunerAction action)
    {
      // The TerraTec H7 and TechniSat CableStar Combo HD CI require *very*
      // careful graph management. If the graph is left idle for any length of
      // time (a few minutes) they will fail to (re)start streaming. In
      // addition, they require the graph to be restarted if tuning fails,
      // otherwise they don't seem to behave properly in subsequent tune
      // requests.
      // We start the graph immediately to prevent the graph from being left
      // idle.
      action = TunerAction.Start;
    }

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Twinhan: on before tune call back");
      action = TunerAction.Default;

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return;
      }

      if (_isElgatoPbdaDriver)
      {
        return;
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      if (channel is ChannelDvbS2)
      {
        // Modulation for DVB-S2 QPSK and 8 PSK should be Mod8Vsb. As far as I
        // know, none of the hardware supported by this extension support 16 or
        // 32 APSK.
        ModulationType bdaModulation = ModulationType.ModNotSet;
        if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4 || satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk8)
        {
          bdaModulation = ModulationType.Mod8Vsb;
        }
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk16)
        {
          bdaModulation = ModulationType.Mod16Vsb;
        }
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk32)
        {
          bdaModulation = ModulationType.ModOqpsk;
        }

        if (bdaModulation != ModulationType.ModNotSet)
        {
          this.LogDebug("  modulation = {0}", bdaModulation);
          satelliteChannel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
        }
      }

      // Reset DiSEqC switch settings. We don't send commands unless we're
      // asked to during the tune request.
      _diseqcPort = TwinhanDiseqcPort.Null;
      _toneBurstCommand = TwinhanToneBurst.Off;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnStarted(ITuner tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that TV Server wants to take; as an output, the action to take.</param>
    public override void OnStop(ITuner tuner, ref TunerAction action)
    {
      // The TerraTec H7 and TechniSat CableStar Combo HD CI require *very*
      // careful graph management. If the graph is left idle for any length of
      // time (a few minutes) they will fail to (re)start streaming. In
      // addition, they require the graph to be restarted if tuning fails,
      // otherwise they don't seem to behave properly in subsequent tune
      // requests.
      if (action == TunerAction.Stop || action == TunerAction.Pause)
      {
        action = TunerAction.Restart;
      }
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("Twinhan: set power state, state = {0}", state);

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

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
      if ((_tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Twinhan: function disabled for safety");
        return true;    // Don't retry...
      }

      if (state == PowerState.On)
      {
        Marshal.WriteByte(_generalBuffer, 0, 0x01);
        _isPowerOn = true;
      }
      else
      {
        Marshal.WriteByte(_generalBuffer, 0, 0x00);
        _isPowerOn = false;
      }
      int hr = _ioControl.Set(IoControlCode.SetTunerPower, _generalBuffer, 1);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to set power state, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the extension implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the extension supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      if (
        (channel is ChannelAtsc && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc)) ||
        (channel is ChannelScte && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte)) ||
        (channel is ChannelDvbC && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbC)) ||
        (channel is IChannelSatellite && (_tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) != 0) ||
        (channel is ChannelDvbT && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT)) ||
        (channel is ChannelDvbT2 && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT2))
      )
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
    /// DigitalNow Quattro S-T (OEM 6090 2xDVB-S + 2xDVB-T/analog). It returned HRESULT 0x8007001f
    /// for the THBDA_IOCTL_LOCK_TUNER call, probably indicating that the driver does not support
    /// this IOCTL. It is possible that the IOCTLs are only implemented for certain models.
    /// </remarks>
    /// <param name="channel">The channel to tune.</param>
    /// <param name="parameters">Tuning time restriction settings.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("Twinhan: tune to channel");

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

      int hr;
      TuningParams tuningParams = new TuningParams();
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc))
      {
        tuningParams.Frequency = atscChannel.Frequency;
      }
      else
      {
        ChannelScte scteChannel = channel as ChannelScte;
        if (scteChannel != null && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte))
        {
          tuningParams.Frequency = scteChannel.Frequency;
        }
        else
        {
          ChannelDvbC dvbcChannel = channel as ChannelDvbC;
          if (dvbcChannel != null && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbC))
          {
            tuningParams.Frequency = dvbcChannel.Frequency;
            tuningParams.SymbolRate = dvbcChannel.SymbolRate;
            switch (dvbcChannel.ModulationScheme)
            {
              case ModulationSchemeQam.Qam16:
                tuningParams.Modulation = ModulationType.Mod16Qam;
                break;
              case ModulationSchemeQam.Qam32:
                tuningParams.Modulation = ModulationType.Mod32Qam;
                break;
              case ModulationSchemeQam.Qam64:
                tuningParams.Modulation = ModulationType.Mod64Qam;
                break;
              case ModulationSchemeQam.Qam128:
                tuningParams.Modulation = ModulationType.Mod128Qam;
                break;
              case ModulationSchemeQam.Qam256:
                tuningParams.Modulation = ModulationType.Mod256Qam;
                break;
              case ModulationSchemeQam.Qam512:
                tuningParams.Modulation = ModulationType.Mod512Qam;
                break;
              case ModulationSchemeQam.Qam1024:
                tuningParams.Modulation = ModulationType.Mod1024Qam;
                break;
            }
          }
          else
          {
            IChannelSatellite satelliteChannel = channel as IChannelSatellite;
            if (satelliteChannel != null)
            {
              tuningParams.Frequency = satelliteChannel.Frequency;
              tuningParams.SymbolRate = satelliteChannel.SymbolRate;
              if (SatelliteLnbHandler.IsHighVoltage(satelliteChannel.Polarisation))
              {
                tuningParams.Modulation = (ModulationType)2;
              }
              else
              {
                tuningParams.Modulation = (ModulationType)1;
              }
            }
            else
            {
              IChannelOfdm ofdmChannel = channel as IChannelOfdm;
              if (ofdmChannel == null || (!_tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT) && !_tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT2)))
              {
                this.LogError("Twinhan: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
                return false;
              }

              tuningParams.Frequency = ofdmChannel.Frequency;
              tuningParams.Bandwidth = ofdmChannel.Bandwidth / 1000;
              tuningParams.Modulation = 0;
            }
          }
        }
      }

      tuningParams.LockWaitForResult = true;

      Marshal.StructureToPtr(tuningParams, _generalBuffer, false);
      Dump.DumpBinary(_generalBuffer, TUNING_PARAMS_SIZE);

      hr = _ioControl.Set(IoControlCode.LockTuner, _generalBuffer, TUNING_PARAMS_SIZE);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to tune, hr = 0x{0:x}{1}{2}", hr, Environment.NewLine, channel);
      return false;
    }

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for a given transmitter.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ShouldEnable(IChannel tuningDetail)
    {
      // As far as I'm aware, PID filtering is only supported by the VP-7021
      // (Starbox) and VP-7041 (Magicbox) models. One or both of these are
      // connected using USB 1.x. The filter should always be enabled for
      // such tuners.
      return _isPidFilterSupported && (!_isPidFilterBypassSupported || _connection == TwinhanDeviceSpeed.UsbLow || _connection == TwinhanDeviceSpeed.UsbFull);
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      if (!_isPidFilterSupported)
      {
        return true;
      }

      this.LogDebug("Twinhan: disable PID filter");
      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      _pidFilterPids.Clear();

      PidFilterParams pidFilterParams = new PidFilterParams();
      pidFilterParams.FilterPids = new ushort[MAX_PID_FILTER_PID_COUNT];
      pidFilterParams.FilterMode = TwinhanPidFilterMode.Disabled;
      pidFilterParams.ValidPidMask = 0;
      Marshal.StructureToPtr(pidFilterParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, PID_FILTER_PARAMS_SIZE);
      int hr = _ioControl.Set(IoControlCode.SetPidFilterInfo, _generalBuffer, PID_FILTER_PARAMS_SIZE);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to disable PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return _maxPidFilterPidCount;
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPids.UnionWith(pids);
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPids.ExceptWith(pids);
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ApplyConfiguration()
    {
      if (!_isPidFilterSupported)
      {
        return true;
      }

      this.LogDebug("Twinhan: apply PID filter configuration");
      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

      PidFilterParams pidFilterParams = new PidFilterParams();
      pidFilterParams.FilterPids = new ushort[MAX_PID_FILTER_PID_COUNT];
      pidFilterParams.FilterMode = TwinhanPidFilterMode.Whitelist;
      _pidFilterPids.CopyTo(pidFilterParams.FilterPids, 0, Math.Min(_maxPidFilterPidCount, _pidFilterPids.Count));
      ulong mask = (ulong)(1 << _pidFilterPids.Count) - 1;
      pidFilterParams.ValidPidMask = (uint)mask;
      Marshal.StructureToPtr(pidFilterParams, _generalBuffer, false);
      Dump.DumpBinary(_generalBuffer, PID_FILTER_PARAMS_SIZE);
      int hr = _ioControl.Set(IoControlCode.SetPidFilterInfo, _generalBuffer, PID_FILTER_PARAMS_SIZE);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to apply PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("Twinhan: open conditional access interface");

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("Twinhan: conditional access interface is already open");
        return true;
      }

      // We can't tell whether a CI slot is actually connected, but we can tell if this tuner supports
      // a CI slot.
      if (_ciApiVersion == TwinhanCiSupport.Unsupported)
      {
        this.LogDebug("Twinhan: tuner doesn't have a CI slot");
        return false;
      }

      if (_isElgatoDriver || _isTerraTecDriver)
      {
        _mmiDataSize = EXTENDED_MMI_DATA_SIZE;
      }
      else
      {
        _mmiDataSize = DEFAULT_MMI_DATA_SIZE;
      }
      _mmiBuffer = Marshal.AllocCoTaskMem(_mmiDataSize);

      this.LogDebug("Twinhan: update CI/CAM state");
      _isCamPresent = false;
      _isCamReady = false;
      TwinhanCiState ciState;
      TwinhanMmiState mmiState;
      int hr = GetCiStatus(out ciState, out mmiState);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Twinhan: failed to get CI status, hr = 0x{0:x}", hr);
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

      _isCaInterfaceOpen = true;
      StartMmiHandlerThread();

      this.LogDebug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Determine if the conditional access interface is open.
    /// </summary>
    /// <value><c>true</c> if the conditional access interface is open, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsOpen
    {
      get
      {
        return _isCaInterfaceOpen;
      }
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Close()
    {
      return CloseConditionalAccessInterface(true);
    }

    private bool CloseConditionalAccessInterface(bool isDisposing)
    {
      this.LogDebug("Twinhan: close conditional access interface");

      if (isDisposing)
      {
        StopMmiHandlerThread();
      }

      if (_mmiBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiBuffer);
        _mmiBuffer = IntPtr.Zero;
      }

      _isCamPresent = false;
      _isCamReady = false;
      _isCaInterfaceOpen = false;

      this.LogDebug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      this.LogDebug("Twinhan: reset conditional access interface");

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

      bool success = (this as IConditionalAccessProvider).Close();

      // This IOCTL does not seem to be implemented at least for the VP-1041 - the HRESULT returned seems
      // to always be 0x8007001f (ERROR_GEN_FAILURE). Either that or special conditions (graph stopped etc.)
      // are required for the reset to work. Our strategy here is to attempt the reset, and if it fails
      // request a graph rebuild.
      int hr = _ioControl.Set(IoControlCode.ResetDevice, IntPtr.Zero, 0);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Twinhan: failed to reset device, hr = 0x{0:x}", hr);
        success = false;
      }

      return success && (this as IConditionalAccessProvider).Open();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("Twinhan: is conditional access interface ready");
      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

      // The CAM state is updated by the MMI handler thread.
      this.LogDebug("Twinhan: result = {0}", _isCamReady);
      return _isCamReady;
    }

    /// <summary>
    /// Determine whether the conditional access interface requires access to
    /// the MPEG 2 conditional access table in order to successfully decrypt
    /// programs.
    /// </summary>
    /// <value><c>true</c> if access to the MPEG 2 conditional access table is required in order to successfully decrypt programs, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsConditionalAccessTableRequiredForDecryption
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more programs
    ///   simultaneously. This parameter gives the interface an indication of the number of programs that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program's map table.</param>
    /// <param name="cat">The conditional access table for the program's transport stream.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.SendCommand(CaPmtListManagementAction listAction, CaPmtCommand command, TableProgramMap pmt, TableConditionalAccess cat, string programProvider)
    {
      this.LogDebug("Twinhan: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("Twinhan: failed to send conditional access command, the CAM is not ready");
        return false;
      }
      if (pmt == null)
      {
        this.LogError("Twinhan: failed to send conditional access command, PMT not supplied");
        return true;
      }

      // Twinhan supports the standard CA PMT format.
      byte[] caPmt = pmt.GetCaPmt(listAction, command);

      // Send the data to the CAM. Use local buffers since PMT updates are asynchronous.
      IntPtr pmtBuffer = Marshal.AllocCoTaskMem(caPmt.Length);
      try
      {
        Marshal.Copy(caPmt, 0, pmtBuffer, caPmt.Length);
        //Dump.DumpBinary(pmtBuffer, caPmt.Length);
        int hr = _ioControl.Set(IoControlCode.CiSendPmt, pmtBuffer, caPmt.Length);
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("Twinhan: result = success");
          return true;
        }

        this.LogError("Twinhan: failed to send PMT to CAM, hr = 0x{0:x}", hr);
        return false;
      }
      finally
      {
        Marshal.FreeCoTaskMem(pmtBuffer);
        pmtBuffer = IntPtr.Zero;
      }
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBack = callBack;
      }
      StartMmiHandlerThread();
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      this.LogDebug("Twinhan: enter menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamReady)
      {
        this.LogError("Twinhan: failed to enter menu, the CAM is not ready");
        return false;
      }

      int hr;
      lock (_mmiLock)
      {
        this.LogDebug("Twinhan: application information");
        for (int i = 0; i < APPLICATION_INFO_SIZE; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        int returnedByteCount;
        hr = _ioControl.Get(IoControlCode.CiGetApplicationInfo, _mmiBuffer, APPLICATION_INFO_SIZE, out returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != APPLICATION_INFO_SIZE)
        {
          this.LogError("Twinhan: failed to read application information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }

        ApplicationInfo info = (ApplicationInfo)Marshal.PtrToStructure(_mmiBuffer, typeof(ApplicationInfo));
        this.LogDebug("  type         = {0}", (MmiApplicationType)info.ApplicationType);
        this.LogDebug("  manufacturer = 0x{0:x4}", info.Manufacturer);
        this.LogDebug("  code         = 0x{0:x4}", info.ManufacturerCode);
        this.LogDebug("  menu title   = {0}", DvbTextConverter.Convert(info.RootMenuTitle));

        hr = _ioControl.Set(IoControlCode.CiInitialiseMmi, IntPtr.Zero, 0);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to enter menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("Twinhan: close menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamReady)
      {
        this.LogError("Twinhan: failed to close menu, the CAM is not ready");
        return false;
      }

      int hr;
      lock (_mmiLock)
      {
        hr = _ioControl.Set(IoControlCode.CiCloseMmi, IntPtr.Zero, 0);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to close menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("Twinhan: select menu entry, choice = {0}", choice);
      MmiData mmi = new MmiData();
      mmi.ChoiceIndex = (int)choice;
      mmi.Type = 1;
      return SendMmi(mmi);
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("Twinhan: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      if (cancel)
      {
        return (this as IConditionalAccessMenuActions).SelectEntry(0); // 0 means "go back to the previous menu level"
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
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      if (_isElgatoPbdaDriver)
      {
        if (_elgatoPbdaDiseqcInterface != null)
        {
          return _elgatoPbdaDiseqcInterface.SendCommand(command);
        }
        return false;
      }

      this.LogDebug("Twinhan: send DiSEqC command");

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Twinhan: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Twinhan: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      // There is interaction between the THBDA_IOCTL_SET_LNB_DATA
      // and THBDA_IOCTL_SET_DiSEqC IOCTLs.
      // THBDA_IOCTL_SET_LNB_DATA sets variables which are used when the next
      // tune request is applied. It doesn't actually cause anything to happen
      // immediately.
      // It looks like THBDA_IOCTL_SET_DiSEqC may do the same. If a tuner only
      // supports DiSEqC 1.0 commands, THBDA_IOCTL_SET_DiSEqC will set the LNB
      // power variable, set the DiSEqC port variable, and set the 22 kHz tone
      // state according to the command bytes.
      // This interaction means it is important to have consistency between
      // SendCommand() and SetToneState(). If you don't, strange things will
      // happen.
      // Note we must invoke THBDA_IOCTL_SET_LNB_DATA. The TerraTec S7 will
      // not seem to switch between low and high band if you don't.

      // If this is a DiSEqC 1.0 command, figure out which port is being
      // selected and cache for SetToneState().
      if (_diseqcPort == TwinhanDiseqcPort.Null && command.Length == 4 &&
        (command[0] == (byte)DiseqcFrame.CommandFirstTransmissionNoReply ||
        command[0] == (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) &&
        command[1] == (byte)DiseqcAddress.AnySwitch &&
        command[2] == (byte)DiseqcCommand.WriteN0)
      {
        _diseqcPort = (TwinhanDiseqcPort)(((command[3] & 0xc) >> 2) + 1);
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = command.Length;
      message.Message = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);

      Marshal.StructureToPtr(message, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, DISEQC_MESSAGE_SIZE);

      int hr = _ioControl.Set(IoControlCode.SetDiseqc, _generalBuffer, DISEQC_MESSAGE_SIZE);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogWarn("Twinhan: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      if (_isElgatoPbdaDriver)
      {
        if (_elgatoPbdaDiseqcInterface != null)
        {
          return _elgatoPbdaDiseqcInterface.SendCommand(command);
        }
        return false;
      }

      this.LogDebug("Twinhan: send tone burst command, command = {0}", command);

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

      if (command == ToneBurst.DataBurst)
      {
        _toneBurstCommand = TwinhanToneBurst.DataBurst;
      }
      else if (command == ToneBurst.ToneBurst)
      {
        _toneBurstCommand = TwinhanToneBurst.ToneBurst;
      }
      else
      {
        _toneBurstCommand = TwinhanToneBurst.Off;
      }
      this.LogDebug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      if (_isElgatoPbdaDriver)
      {
        if (_elgatoPbdaDiseqcInterface != null)
        {
          return _elgatoPbdaDiseqcInterface.SetToneState(state);
        }
        return false;
      }

      this.LogDebug("Twinhan: set tone state, state = {0}", state);

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

      LnbParams lnbParams = new LnbParams();
      lnbParams.PowerOn = _isPowerOn;
      lnbParams.ToneBurst = _toneBurstCommand;
      if (state == Tone22kState.On)
      {
        lnbParams.Tone22kState = TwinhanTone22kState.On;
      }
      else
      {
        lnbParams.Tone22kState = TwinhanTone22kState.Off;
      }

      lnbParams.LowBandLof = (uint)SatelliteLnbHandler.LOW_BAND_LOF / 1000;
      lnbParams.HighBandLof = (uint)SatelliteLnbHandler.HIGH_BAND_LOF / 1000;
      lnbParams.SwitchFrequency = (uint)SatelliteLnbHandler.SWITCH_FREQUENCY / 1000;
      lnbParams.DiseqcPort = _diseqcPort;

      // Reset - don't resend commands in subsequent tune requests.
      _diseqcPort = TwinhanDiseqcPort.Null;
      _toneBurstCommand = TwinhanToneBurst.Off;

      Marshal.StructureToPtr(lnbParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, LNB_PARAMS_SIZE);

      int hr = _ioControl.Set(IoControlCode.SetLnbData, _generalBuffer, LNB_PARAMS_SIZE);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Twinhan: result = success");
        return true;
      }

      this.LogError("Twinhan: failed to set tone state, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      response = null;

      if (_isElgatoPbdaDriver)
      {
        if (_elgatoPbdaDiseqcInterface != null)
        {
          return _elgatoPbdaDiseqcInterface.ReadResponse(out response);
        }
        return false;
      }

      this.LogDebug("Twinhan: read DiSEqC response");

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }

      for (int i = 0; i < DISEQC_MESSAGE_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _ioControl.Get(IoControlCode.GetDiseqc, _generalBuffer, DISEQC_MESSAGE_SIZE, out returnedByteCount);
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == DISEQC_MESSAGE_SIZE)
      {
        this.LogDebug("Twinhan: result = success");
        DiseqcMessage message = (DiseqcMessage)Marshal.PtrToStructure(_generalBuffer, typeof(DiseqcMessage));
        response = new byte[message.MessageLength];
        Buffer.BlockCopy(message.Message, 0, response, 0, message.MessageLength);

        Dump.DumpBinary(_generalBuffer, DISEQC_MESSAGE_SIZE);
        return true;
      }

      this.LogError("Twinhan: failed to read DiSEqC response, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      return false;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("Twinhan: open remote control interface");

      if (!_isTwinhan)
      {
        this.LogWarn("Twinhan: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Twinhan: remote control interface is already open");
        return true;
      }

      _remoteControlInterface = new RemoteControlHid(_tunerProductInstanceId, _tunerExternalId, _propertySet, _isTerraTecDriver);
      if (!_remoteControlInterface.Open())
      {
        _remoteControlInterface = new RemoteControlLegacy(_tunerProductInstanceId, _propertySet);
        if (!_remoteControlInterface.Open())
        {
          return false;
        }
      }

      _isRemoteControlInterfaceOpen = true;
      this.LogDebug("Twinhan: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Close()
    {
      return CloseRemoteControlListenerInterface(true);
    }

    private bool CloseRemoteControlListenerInterface(bool isDisposing)
    {
      this.LogDebug("Twinhan: close remote control interface");

      if (isDisposing && _remoteControlInterface != null)
      {
        _remoteControlInterface.Close();
        _remoteControlInterface = null;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Twinhan: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Twinhan()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isTwinhan)
      {
        CloseConditionalAccessInterface(isDisposing);
        CloseRemoteControlListenerInterface(isDisposing);

        if (isDisposing)
        {
          IDisposable d = _elgatoPbdaDiseqcInterface as IDisposable;
          if (d != null)
          {
            d.Dispose();
          }
          _elgatoPbdaDiseqcInterface = null;
        }
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (isDisposing)
      {
        _ioControl = null;
        Release.ComObject("Twinhan property set", ref _propertySet);
      }
      _isTwinhan = false;
    }

    #endregion
  }
}