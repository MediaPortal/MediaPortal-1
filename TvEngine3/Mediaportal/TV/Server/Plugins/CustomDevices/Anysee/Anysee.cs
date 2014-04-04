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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Anysee
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC and remote controls for Anysee tuners. Smart
  /// card slots are not supported.
  /// </summary>
  public class Anysee : BaseCustomDevice, IConditionalAccessProvider, IConditionalAccessMenuActions, IDiseqcDevice, IRemoteControlListener
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Rssi = 1,                       // signal strength metric
      Cnr = 2,                        // carrier to noise ratio
      Ir = 4,
      PlatformInfo = 6,
      Locked = 7,
      NimMode = 8,
      StatusInfo = 15,
      DriverVersion = 18,
      NimConfig = 19,
      LnbInfo = 20,
      Capabilities = 22,
      BoardInfo = 23,
      Diseqc = 24
    }

    // PCB/product/revision
    private enum AnyseePlatform : ushort
    {
      Pcb507T = 2,              // Anysee E30 - DVB-T
      Pcb507H,                  // Anysee K30/K50 - External ATSC + NTSC
      Pcb507S,                  // Anysee E30 S Plus (v1) - External DVB-S + Smartcard Interface
      Pcb507C,                  // Anysee E30 C Plus (v1) - External DVB-C + analog PAL Smartcard Interface
      Pcb507CD,                 // Anysee E30 Plus - DVB-T + Smartcard Interface
      Pcb507HI,                 // Anysee K70 - ATSC + NTSC + DMB
      Pcb507D,                  // Anysee E30 Combo Plus (v1) - External DVB-T + DVB-C + Smartcard Interface
      Pcb507E,                  // Anysee E30 C Plus (v2) - External DVB-C + Smartcard Interface
      Pcb507DC,                 // Anysee E30 C Plus (v3) - External DVB-C + Smartcard Interface
      Pcb507S2,                 // Anysee E30 S2 Plus - External DVB-S2 + Smartcard Interface
      Pcb507SI,                 // Anysee E30 S Plus (v2) - External DVB-S + Smartcard Interface
      Pcb507PS,                 // Anysee E30P S - Internal DVB-S + Smartcard Interface [Optional]
      Pcb507PS2,                // Anysee E30P S2 - Internal DVB-S2 + Smartcard Interface
      Pcb507FA = 15,            // Anysee E30 TC Plus (v2)/E30 Plus/E30 C Plus - DVB-T + DVB-C + Smartcard Interface [Optional]
      Pcb5M01G = 16,            // SLC 1GB NAND flash memory
      Pcb508TC = 18,            // Anysee E7 TC - External DVB-T + DVB-C + CI + Smartcard Interface [Optional]
      Pcb508S2,                 // Anysee E7 S2 - External DVB-S2 + CI + Smartcard Interface [Optional]
      Pcb508T2C,                // Anysee E7 T2C - External DVB-T2 + DVB-C + CI + Smartcard Interface [Optional]
      Pcb508PTC,                // Anysee E7P TC - Internal DVB-T + DVB-C + CI + Smartcard Interface [Optional]
      Pcb508PS2,                // Anysee E7P S2 - Internal DVB-S2 + CI + Smartcard Interface [Optional]
      Pcb508PT2C                // Anysee E7P T2C - Internal DVB-T2 + DVB-C + CI + Smartcard Interface [Optional]
    }

    // chip combinations
    private enum AnyseeNim
    {
      Unknown = 0,
      DNOS404Zx101x,
      DNOS404Zx102x,              // Samsung NIM - Zarlink MT352 (COFDM demod)
      DNOS404Zx103x,              // Samsung NIM - Zarlink ZL10353 (COFDM demod)
      TDVSH062P,                  // LG/Innotek NIM - LG DT3303 (8 VSB + 256/64 QAM demod), Philips TDA9887 (analog demod), Infineon TUA6034 (hybrid digital/analog terrestrial tuner)
      MT352_FMD1216ME,            // Zarlink (COFDM demod)/Philips (hybrid DVB-T/analog TV/FM tuner)
      ZL10353_FMD1216ME,          // Zarlink (COFDM demod)/Philips (hybrid DVB-T/analog TV/FM tuner)
      PN3030_ITD3010,             // ??? (DAB/DAB+/DMB/FM demod)/Integrant Technologies (DMB/FM tuner)
      DNOS881Zx121A,              // Samsung NIM
      STV0297J_DNOS881Zx121A,     // ST (QAM demod)/???
      DNQS441PH261A,              // Samsung
      TDA10023HT_DTOS203IH102A,   // Philips/NXP/Trident (QAM demod)/??? (DVB-C tuner)
      DNBU10321IRT,
      FakeHW,                     // (simulation hardware)
      BS2N10WCC01,                // "DVB-S2, Cosy NIM" - Conexant CX24116/CX24118 (QPSK, DVB-S2 QPSK/8PSK demod)
      ZL10353_XC5000,             // Zarlink (COFDM demod)/Xceive (hybrid digital/analog tuner)
      TDA10023HT_XC5000,          // Philips/NXP/Trident (QAM demod)/Xceive (hybrid digital/analog tuner)
      ZL10353_DTOS203IH102A,      // Zarlink (COFDM demod)/???
      ZL10353_DTOS403Ix102x,      // Zarlink (COFDM demod)/???
      TDA10023HT_DTOS403Ix102x,   // Philips/NXP/Trident (QAM demod)
      DNBU10512IST,               // ST NIM - STV0903 (QPSK, DC II, DVB-S2 QPSK/8PSK demod), STV6110
      M88DS3002_M88TS2020,        // LG Montage NIM - M88DS3002 (QPSK, DVB-S2 QPSK/8PSK/16APSK/32APSK demod), M88TS2020 (digital tuner)
      ZL10353_EN4020,             // Zarlink (COFDM demod)/Entropic (hybrid digital/analog terrestrial/cable tuner)
      TDA10023HT_EN4020,          // Philips/NXP/Trident (QAM demod)/Entropic (hybrid digital/analog terrestrial/cable tuner)
      ZL10353_TDA18212,           // Zarlink (COFDM demod)/NXP DNOD44CDV086A (hybrid digital/analog terrestrial/cable tuner)
      TDA10023HT_TDA18212,        // Philips/NXP/Trident (QAM demod)/NXP DNOD44CDV086A (hybrid digital/analog terrestrial/cable tuner)
      CXD2820_TDA18272,           // ??? (DVB-T2 demod)/NXP DNOD44CDV086A (hybrid digital/analog terrestrial/cable tuner)
      DNOQ44QCV106A,              // Samsung NIM - DVB-T2/C
    }

    private enum AnyseeNimMode
    {
      None = 0,
      DvbS_Qpsk,
      Qam16,
      Qam32,
      Qam64,
      Qam128,
      Qam256,
      Vsb,
      DvbT_Ofdm,
      Ntsc,
      PalBg,
      PalI,
      PalDk,
      SecamL,
      SecamLp,
      Fm,
      Dmb,
      Dab,
      Qam4,
      Bpsk,
      Am,
      DvbS_Qam16Fec3_4,
      DvbS_Qam16Fec7_8,
      DvbS_8PskFec2_3,
      DvbS_8PskFec5_6,
      DvbS_8PskFec8_9,
      DvbS_QpskFec1_2,
      DvbS_QpskFec2_3,
      DvbS_QpskFec3_4,
      DvbS_QpskFec5_6,
      DvbS_QpskFec6_7,
      DvbS_QpskFec7_8,
      DvbS2_QpskFec1_2,
      DvbS2_QpskFec3_5,
      DvbS2_QpskFec2_3,
      DvbS2_QpskFec3_4,
      DvbS2_QpskFec4_5,
      DvbS2_QpskFec5_6,
      DvbS2_QpskFec8_9,
      DvbS2_QpskFec9_10,
      DvbS2_8PskFec3_5,
      DvbS2_8PskFec2_3,
      DvbS2_8PskFec3_4,
      DvbS2_8PskFec5_6,
      DvbS2_8PskFec8_9,
      DvbS2_8PskFec9_10,
      DvbS2_16ApskFec2_3,
      DvbS2_16ApskFec3_4,
      DvbS2_16ApskFec4_5,
      DvbS2_16ApskFec5_6,
      DvbS2_16ApskFec8_9,
      DvbS2_16ApskFec9_10,
      DvbS2_32ApskFec3_4,
      DvbS2_32ApskFec4_5,
      DvbS2_32ApskFec5_6,
      DvbS2_32ApskFec8_9,
      DvbS2_32ApskFec9_10,
      DtvFec1_2,
      DtvFec2_3,
      DtvFec6_7,
      DvbS_8Psk,
      DvbS2_Qpsk,
      DvbS2_8Psk,
      DvbS2_16Apsk,
      DvbS2_32Apsk,
      DvbS2_QpskFec1_3,
      DvbS2_QpskFec1_4,
      DvbS2_QpskFec2_5,
      DvbT2_Ofdm,
      AutoQam,
      AutoOfdm                // DVB-T or DVB-T2
    }

    private enum AnyseeInversionMode
    {
      None = 0,         // Normal
      Inverted,
      Auto,
      AutoNormalFirst   // Auto-detect, try normal inversion first
    }

    private enum AnyseeScanDirection
    {
      Up = 0,
      Down
    }

    [Flags]
    private enum AnyseeNimCapability
    {
      SymbolRate = 1,
      SearchStep = 2,
      Lnb = 4,
      RollOff = 8,
      Pilot = 16
    }

    private enum AnyseeBroadcastSystem
    {
      Unknown = 0,
      Ntsc,
      NtscM,
      Atsc,
      Dmb,
      DirecTv,
      Dab,
      Pal,          // PAL and/or PAL-BG
      Secam,        // SECAM and/or SECAM-L
      DvbT,
      DvbC,
      DvbS,
      DvbS2,
      DvbH,
      DvbHd,
      IsdbT,
      IsdbC,
      IsdbS,
      DmbTh,
      NtscN,
      NtscJ,
      PalDk,
      PalI,
      PalM,
      PalN,
      SecamLp,
      SecamDk,
      Am,
      Fm,
      Dtv6MHz,
      Dtv7MHz,
      Dtv8MHz,
      Dtv7_8MHz,    // 7 and/or 8 MHz
      ClearQam,
      DvbT2,
      Mcns
    }

    private enum AnyseeBusType
    {
      Usb = 0,
      Pci,
      I2c,          // "I squared C"
      VirtualUsb,
      VirtualPci,
      VirtualI2c
    }

    private enum AnyseeBoardType
    {
      Unknown = 0,
      Analog,
      Digital,
      Hybrid,
      TsEquipment,
      NandFlashMemory
    }

    [Flags]
    private enum AnyseeBoardProperty
    {
      None = 0,
      HighSpeedUsb = 1,
      MultiNim = 2,
      PowerDown = 4
    }

    private enum AnyseeBoardMode
    {
      // USB
      UsbTsBypass = 0,
      UsbTsInput,
      UsbTsOutput,

      // asynchronous
      AsyncFifoInput,
      AsyncFifoOutput,

      // synchronous
      SyncFifoInput,
      SyncFifoOutput,

      Analog = 11,
      Digital,
      TransportStream,
      AvCapture,
      Unknown,              // "no setup"
      GetFailure,
      Gpio,                 // general purpose input/output
      Gpif
    }

    private enum AnyseeToneBurst : byte
    {
      Off = 0,
      ToneBurst,
      DataBurst
    }

    private enum AnyseeCamMenuKey   // CI_KEY_MAP
    {
      // Numeric keys
      Zero = 1,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine,

      // Navigation keys
      Menu = 20,
      Exit,
      Up,
      Down,
      Left,
      Right,
      Select,

      Clear = 30
    }

    private enum AnyseeCiState    // CI_MESSAGE_COMMAND
    {
      Empty = 2002,             // CI_MSG_EXTRACTED_CAM - CAM not present or not initialised
      Clear = 2100,             // CI_MSG_CLEAR - seems to indicate that there are no encrypted PIDs or that the CAM is not decrypting any channels
      CamInserted = 2101,       // CI_MSG_INITIALIZATION_CAM - CAM initialising
      CamOkay = 2102,           // CI_MSG_INSERT_CAM - CAM initialisation finishing
      SendPmtComplete = 2103,   // CI_SEND_PMT_COMPLET - PMT sent to the CAM
      CamReady = 2105           // CI_USE_MENU - CAM menu can be accessed
    }

    private enum AnyseeCiCommand : uint   // CI_CONTROL_COMMAND
    {
      GetDeviceIndex = 1100,    // CI_CONTROL_GET_DEVICE_NUM - get the Anysee device index
      SetPmtOld = 1101,         // Send PMT to the CAM using old PMT format (PMTInfo).
      IsOpen = 1104,            // CI_CONTROL_IS_OPEN - check whether the CI API is open
      SetKey = 1105,            // CI_CONTROL_SET_KEY - send a key press to the CAM
      SetTdt = 1106,            // CI_CONTROL_SET_TDT - send TDT to the CAM
      ResetHardware = 1107,
      SetTsBypass = 1109,       // Not documented - do *not* use!
      SetPmt = 1110,            // CI_CONTROL_SET_PMT - send PMT to the CAM
      IsOpenSetCallBacks = 2000 // CI_CONTROL_IS_PLUG_OPEN - check whether the CI API is open and set call back functions
    }

    private enum AnyseeMmiMessageType
    {
      Menu = 0,
      InputRequest,
    }

    private enum AnyseeEsType : byte
    {
      Unknown = 0,
      Audio,
      Video,
      Teletext,
      Subtitle,
      Private
    }

    #endregion

    #region structs
    // These structs are all aligned to 8 byte boundaries.

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct IrData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.Bool)]
      public bool Enable;
      public int Key;           // bit 8 = repeat flag (0 = repeat), bits 7-0 = key code
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlatformInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
      public byte[] FirmwareVersion;    // [0x04, 0x00] -> 0.4
      public AnyseePlatform Platform;
      private int Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct StatusInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      public AnyseeBroadcastSystem CurrentBroadcastSystem;
      public AnyseeNimMode CurrentNimMode;

      public int CurrentFrequency;      // unit = kHz
      public int Unknown1;

      public AnyseeNim NimType;
      private int Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DriverVersion
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public byte[] Version;        // [0x58, 0x20, 0x06, 0x01] -> 1.6.20.58
      private int Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NimConfig
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      public int SymbolRate;        // unit = s/s (Baud)
      public int SweepRate;         // unit = Hz/s

      public int Frequency;         // unit = kHz
      public int CarrierOffset;     // unit = kHz

      public byte Bandwidth;        // unit = Mhz
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding;
      public AnyseeNim NimType;

      public AnyseeNimMode AnalogNimMode;
      public AnyseeNimMode DigitalNimMode;

      public AnyseeInversionMode SignalInversion;
      public AnyseeScanDirection ScanDirection;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LnbInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      public int UnknownFlags;
      public int SwitchFrequency;   // unit = MHz

      public int HighLof;           // unit = MHz
      public int LowLof;            // unit = MHz

      public int EffectiveLof;      // unit = MHz
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
      private byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Capabilities
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      public int MinFrequency;      // unit = kHz
      public int MaxFrequency;      // unit = kHz

      public int MinSymbolRate;     // unit = s/s (Baud)
      public int MaxSymbolRate;     // unit = s/s (Baud)

      public int MinSearchStep;     // unit = Hz
      public int MaxSearchStep;     // unit = Hz

      public AnyseeNimCapability NimCapabilities;
      public AnyseeBroadcastSystem PrimaryBroadcastSystem;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BoardInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public byte[] FirmwareVersion;    // [0x04, 0x00, 0x00, 0x00] -> 0.4
      public AnyseeBusType BusType;

      public AnyseeBoardType BoardType;
      public AnyseeBoardProperty BoardProperties;

      public AnyseeBoardMode BoardMode;
      private int Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessage
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KS_PROPERTY_SIZE)]
      public byte[] KsProperty;

      public int MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] Message;

      public AnyseeToneBurst ToneBurst;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
    private struct ApiString
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_API_STRING_LENGTH)]
      public string Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MmiMenu  // MMIStrsBlock
    {
      public int StringCount;
      public int MenuIndex;
      // This is a pointer to an array of pointers. The maximum size of the
      // array is MAX_CAM_MENU_ENTRIES. Each of the pointers points to a
      // block of memory with size MAX_API_STRING_LENGTH containing a
      // string encoded according to EN 300 468 Annex A.
      public IntPtr Entries;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MmiMessage   // tagCIMsgs
    {
      public int DeviceIndex;
      public int SlotIndex;

      public int HeaderCount;
      public int EntryCount;

      public AnyseeMmiMessageType Type;
      public int ExpectedAnswerLength;

      public int KeyCount;
      // Encoded according to EN 300 468 Annex A.
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_API_STRING_LENGTH)]
      public byte[] RootMenuTitle;

      public IntPtr Menu;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PmtData    // DTVCIPMT
    {
      public byte PmtByte6;                     // Byte 6 from the PMT section (PMT version, current next indicator). 
      private byte Padding1;
      public ushort PcrPid;
      public ushort ServiceId;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESCRIPTOR_DATA_LENGTH)]
      public byte[] ProgramCaDescriptorData;    // The first two bytes should specify the length of the descriptor data.
      private ushort Padding2;

      public uint EsCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PMT_ELEMENTARY_STREAMS)]
      public EsPmtData[] EsPmt;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct EsPmtData
    {
      public ushort Pid;
      public AnyseeEsType EsType;
      public byte StreamType;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESCRIPTOR_DATA_LENGTH)]
      public byte[] DescriptorData;             // The first two bytes should specify the length of the descriptor data.
    }

    #endregion

    /// <summary>
    /// This class is used to "hide" awkward aspects of using the Anysee API
    /// like the requirement for one copy of the CIAPI DLL per device and STA
    /// thread access.
    /// </summary>
    private class AnyseeCiApi
    {
      #region structs

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct CiDeviceInfo   // ANYSEECIDEVICESINFO
      {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICE_COUNT)]
        public ApiString[] DevicePaths;       // A list of the capture device paths for all Anysee devices connected to the system.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICE_COUNT)]
        public int[] DevicePathLengths;       // The length of the corresponding device path in DevicePaths.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICE_COUNT)]
        public int[] DevicePathIndices;       // The index of the corresponding capture device in the set of KSCATEGORY_BDA_RECEIVER_COMPONENT devices returned by the system enumerator.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICE_COUNT)]
        public int[] DeviceIndices;           // The Anysee device index for the corresponding device.
      }

      #endregion

      #region delegates

      #region static DLL functions

      /// <summary>
      /// Create a new common interface API instance. One instance is required for each Anysee device.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance created. The memory must be allocated by TV Server before calling this function.</param>
      /// <returns><c>one</c> if an instance is successfully created, otherwise <c>zero</c></returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate int CreateDtvCIAPI(IntPtr ciApiInstance);

      /// <summary>
      /// Destroy a previously created common interface API instance.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance to destroy. The memory must be released by TV Server after calling this function.</param>
      /// <returns><c>one</c> if the instance is successfully destroyed, otherwise <c>zero</c></returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate int DestroyDtvCIAPI(IntPtr ciApiInstance);

      /// <summary>
      /// Get the number of Anysee devices connected to the system with corresponding
      /// device path and index detail to enable opening a common interface API instance.
      /// </summary>
      /// <param name="deviceInfo">A buffer containing device path and index information for all Anysee devices connected to the system.</param>
      /// <returns>the number of Anysee devices connected to the system</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate int GetanyseeNumberofDevicesEx(IntPtr deviceInfo);

      #endregion

      #region CIAPI unmanaged class

      /// <summary>
      /// Open the common interface API for a specific Anysee device.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance to open.</param>
      /// <param name="windowHandle">A reference to a window to use as an alternative to call backs (CIAPI.dll sends custom messages to the window).</param>
      /// <param name="deviceIndex">The Anysee index for the device to open.</param>
      /// <returns>an HRESULT indicating whether the API was successfully opened</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate int OpenCILib(IntPtr ciApiInstance, IntPtr windowHandle, int deviceIndex);

      /// <summary>
      /// Execute a command on an open common interface instance.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance.</param>
      /// <param name="command">The command to execute.</param>
      /// <param name="inputParams">A reference to a buffer containing the appropriate input parameters for the command.</param>
      /// <param name="outputParams">A reference to a buffer that will be filled with the command's output parameters.</param>
      /// <returns>an HRESULT indicating whether the command was successfully executed</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate int CI_Control(IntPtr ciApiInstance, AnyseeCiCommand command, IntPtr inputParams, IntPtr outputParams);

      #endregion

      #endregion

      #region constants

      private const int API_INSTANCE_SIZE = 76;
      private const int MAX_DEVICE_COUNT = 32;
      private static readonly int CI_DEVICE_INFO_SIZE = Marshal.SizeOf(typeof(CiDeviceInfo));   // 8576
      private const int API_ACCESS_THREAD_WAIT_TIME = 500;    // unit = ms

      #endregion

      #region variables

      // This variable tracks the number of open API instances which corresponds with used DLL indices.
      private static int _apiCount = 0;

      // Delegate instances for each API DLL function.
      private CreateDtvCIAPI _createApi = null;
      private DestroyDtvCIAPI _destroyApi = null;
      private GetanyseeNumberofDevicesEx _getAnyseeDeviceCount = null;
      private OpenCILib _openApi = null;
      private CI_Control _ciControl = null;

      private int _apiIndex = 0;
      private bool _dllLoaded = false;
      private string _devicePath = string.Empty;

      private IntPtr _ciApiInstance = IntPtr.Zero;
      private IntPtr _windowHandle = IntPtr.Zero;
      private IntPtr _libHandle = IntPtr.Zero;

      private Thread _apiAccessThread = null;
      private AutoResetEvent _apiAccessThreadStopEvent = null;

      #endregion

      /// <summary>
      /// Create a new CI API instance.
      /// </summary>
      public AnyseeCiApi()
      {
        _apiCount++;
        _apiIndex = _apiCount;
        this.LogDebug("Anysee: loading API, API index = {0}", _apiIndex);
        string resourcesFolder = PathManager.BuildAssemblyRelativePath("Resources"); // It's called already inside plugins\CustomDevices !
        string sourceFilename = Path.Combine(resourcesFolder, "CIAPI.dll");
        string targetFilename = Path.Combine(resourcesFolder, "CIAPI" + _apiIndex + ".dll");
        if (!File.Exists(targetFilename))
        {
          try
          {
            File.Copy(sourceFilename, targetFilename);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Anysee: failed to copy CIAPI.dll");
            return;
          }
        }
        _libHandle = NativeMethods.LoadLibraryA(targetFilename);
        if (_libHandle == IntPtr.Zero)
        {
          this.LogError("Anysee: failed to load the CI API DLL");
          return;
        }

        try
        {
          IntPtr function = NativeMethods.GetProcAddress(_libHandle, "CreateDtvCIAPI");
          if (function == IntPtr.Zero)
          {
            this.LogError("Anysee: failed to locate the CreateDtvCIAPI function");
            return;
          }
          try
          {
            _createApi = (CreateDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(CreateDtvCIAPI));
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Anysee: failed to load the CreateDtvCIAPI function");
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "DestroyDtvCIAPI");
          if (function == IntPtr.Zero)
          {
            this.LogError("Anysee: failed to locate the DestroyDtvCIAPI function");
            return;
          }
          try
          {
            _destroyApi = (DestroyDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(DestroyDtvCIAPI));
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Anysee: failed to load the DestroyDtvCIAPI function");
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "GetanyseeNumberofDevicesEx");
          if (function == IntPtr.Zero)
          {
            this.LogError("Anysee: failed to locate the GetanyseeNumberofDevicesEx function");
            return;
          }
          try
          {
            _getAnyseeDeviceCount = (GetanyseeNumberofDevicesEx)Marshal.GetDelegateForFunctionPointer(function, typeof(GetanyseeNumberofDevicesEx));
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Anysee: failed to load the GetanyseeNumberofDevicesEx function");
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "?OpenCILib@CCIAPI@@UAGJPAUHWND__@@H@Z");
          if (function == IntPtr.Zero)
          {
            this.LogError("Anysee: failed to locate the OpenCILib function");
            return;
          }
          try
          {
            _openApi = (OpenCILib)Marshal.GetDelegateForFunctionPointer(function, typeof(OpenCILib));
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Anysee: failed to load the OpenCILib function");
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "?CI_Control@CCIAPI@@UAGJKPAJ0@Z");
          if (function == IntPtr.Zero)
          {
            this.LogError("Anysee: failed to locate the CI_Control function");
            return;
          }
          try
          {
            _ciControl = (CI_Control)Marshal.GetDelegateForFunctionPointer(function, typeof(CI_Control));
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Anysee: failed to load the CI_Control function");
            return;
          }

          _dllLoaded = true;
        }
        finally
        {
          if (!_dllLoaded)
          {
            NativeMethods.FreeLibrary(_libHandle);
            _libHandle = IntPtr.Zero;
          }
        }
      }

      /// <summary>
      /// Open the API.
      /// </summary>
      /// <param name="tunerDevicePath">The tuner device path.</param>
      /// <returns><c>true</c> if the API is successfully opened, otherwise <c>false</c></returns>
      public bool OpenApi(string tunerDevicePath)
      {
        this.LogDebug("Anysee: opening API, API index = {0}", _apiIndex);

        if (!_dllLoaded)
        {
          this.LogError("Anysee: the CIAPI.dll functions were not successfully loaded");
          return false;
        }
        if (_apiAccessThread != null && _apiAccessThread.IsAlive)
        {
          this.LogWarn("Anysee: API access thread is already running");
          return false;
        }

        // We only care about the device instance part of the device path
        // because the API provides capture device paths - matching the full
        // device path would not work.
        _devicePath = tunerDevicePath.Split('{')[0];

        _ciApiInstance = Marshal.AllocCoTaskMem(API_INSTANCE_SIZE);
        for (int i = 0; i < API_INSTANCE_SIZE; i++)
        {
          Marshal.WriteByte(_ciApiInstance, i, 0);
        }
        _windowHandle = Marshal.AllocCoTaskMem(IntPtr.Size);

        // Technically all access to the CI API functions should be made
        // from a separate thread because the CIAPI DLL only supports single
        // thread apartment access. TV Server threads use MTA by default.
        // Those two threading models are incompatible. In practise I have
        // determined that it is only necessary to call the "static" DLL
        // functions from an STA thread. That effectively means that we
        // only need an STA thread to open the API and hold it open until
        // it is no longer needed.
        this.LogDebug("Anysee: starting API access thread");
        _apiAccessThreadStopEvent = new AutoResetEvent(false);
        _apiAccessThread = new Thread(new ThreadStart(AccessThread));
        _apiAccessThread.Name = string.Format("Anysee API {0} Access", _apiCount);
        _apiAccessThread.IsBackground = true;
        _apiAccessThread.Priority = ThreadPriority.Lowest;
        _apiAccessThread.SetApartmentState(ApartmentState.STA);   // Critical!!!
        _apiAccessThread.Start();

        Thread.Sleep(500);

        // The thread will terminate almost immediately if there is any
        // problem. If the thread is alive after half a second then
        // everything should be okay.
        if (_apiAccessThread.IsAlive)
        {
          this.LogDebug("Anysee: API access thread running");
          return true;
        }
        this.LogError("Anysee: CI API access thread self-terminated");
        return false;
      }

      /// <summary>
      /// This is a thread function that actually creates and opens
      /// an Anysee API instance. The API is held open until the thread
      /// is stopped.
      /// </summary>
      private void AccessThread()
      {
        this.LogDebug("Anysee: creating new CI API instance");
        int result = _createApi(_ciApiInstance);
        if (result != 1)
        {
          this.LogError("Anysee: failed to create CI API instance, result = {0}", result);
          return;
        }
        this.LogDebug("Anysee: created instance successfully");

        // We have an API instance, but now we need to open it by linking it with hardware.
        this.LogDebug("Anysee: determining instance index");
        IntPtr infoBuffer = Marshal.AllocCoTaskMem(CI_DEVICE_INFO_SIZE);
        for (int i = 0; i < CI_DEVICE_INFO_SIZE; i++)
        {
          Marshal.WriteByte(infoBuffer, i, 0);
        }

        int numDevices = _getAnyseeDeviceCount(infoBuffer);
        this.LogDebug("Anysee: number of devices = {0}", numDevices);
        if (numDevices == 0)
        {
          Marshal.FreeCoTaskMem(infoBuffer);
          return;
        }
        CiDeviceInfo deviceInfo = (CiDeviceInfo)Marshal.PtrToStructure(infoBuffer, typeof(CiDeviceInfo));
        Marshal.FreeCoTaskMem(infoBuffer);

        string captureDevicePath;
        int index = -1;
        for (int i = 0; i < numDevices; i++)
        {
          captureDevicePath = deviceInfo.DevicePaths[i].Text.Substring(0, deviceInfo.DevicePathLengths[i]);
          this.LogDebug("Anysee: device {0}", i + 1);
          this.LogDebug("  device path  = {0}", captureDevicePath);
          this.LogDebug("  index        = {0}", deviceInfo.DevicePathIndices[i]);
          this.LogDebug("  Anysee index = {0}", deviceInfo.DeviceIndices[i]);

          if (captureDevicePath.StartsWith(_devicePath))
          {
            this.LogDebug("Anysee: found correct instance");
            index = deviceInfo.DeviceIndices[i];
            break;
          }
        }

        // If we have a valid device index then we can attempt to open the CI API.
        if (index != -1)
        {
          this.LogDebug("Anysee: opening CI API");
          result = _openApi(_ciApiInstance, _windowHandle, index);
          if (result == (int)HResult.Severity.Success)
          {
            this.LogDebug("Anysee: result = success");
            while (!_apiAccessThreadStopEvent.WaitOne(API_ACCESS_THREAD_WAIT_TIME))
            {
              // Nothing to do except hold the API open until it is no longer needed.
            }
          }
          else
          {
            this.LogError("Anysee: failed to open CI API, hr = 0x{0:x} ({1})", result, HResult.GetDXErrorString(result));
          }
        }

        // When this thread is stopped, we automatically destroy the API instance.
        this.LogDebug("Anysee: destroying CI API instance");
        result = _destroyApi(_ciApiInstance);
        this.LogDebug("Anysee: result = {0}", result);
      }

      /// <summary>
      /// Close the API.
      /// </summary>
      /// <returns><c>true</c> if the API is successfully closed, otherwise <c>false</c></returns>
      public bool CloseApi()
      {
        this.LogDebug("Anysee: closing API");

        if (!_dllLoaded)
        {
          this.LogWarn("Anysee: the CI API DLL has not been successfully loaded");
          return true;
        }

        // Stop the API access thread.
        if (_apiAccessThread == null)
        {
          this.LogWarn("Anysee: CI API access thread is null, earlier failure?");
        }
        else
        {
          this.LogDebug("Anysee: API access thread state = {0}", _apiAccessThread.ThreadState);
          _apiAccessThreadStopEvent.Set();
          if (!_apiAccessThread.Join(API_ACCESS_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Anysee: failed to join CI API access thread, aborting thread");
            _apiAccessThread.Abort();
          }
          _apiAccessThread = null;
        }
        if (_apiAccessThreadStopEvent != null)
        {
          _apiAccessThreadStopEvent.Close();
          _apiAccessThreadStopEvent = null;
        }

        // Free memory and close the library.
        if (_ciApiInstance != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(_ciApiInstance);
          _ciApiInstance = IntPtr.Zero;
        }
        if (_windowHandle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(_windowHandle);
          _windowHandle = IntPtr.Zero;
        }
        if (_libHandle != IntPtr.Zero)
        {
          NativeMethods.FreeLibrary(_libHandle);
          _libHandle = IntPtr.Zero;
        }
        return true;
      }

      /// <summary>
      /// Execute a command on an open API instance.
      /// </summary>
      /// <param name="command">The command to execute.</param>
      /// <param name="inputParams">A reference to a buffer containing the appropriate input parameters for the command.</param>
      /// <param name="outputParams">A reference to a buffer that will be filled with the command's output parameters.</param>
      /// <returns><c>true</c> if the command is successfully executed, otherwise <c>false</c></returns>
      public bool ExecuteCommand(AnyseeCiCommand command, IntPtr inputParams, IntPtr outputParams)
      {
        this.LogDebug("Anysee: execute API command");

        if (_apiAccessThread == null)
        {
          this.LogError("Anysee: failed to execute CI control command, API access thread is null");
          return false;
        }
        if (!_apiAccessThread.IsAlive)
        {
          this.LogError("Anysee: failed to execute CI control command, the API is not open");
          return false;
        }

        int hr;
        lock (_ciControl)
        {
          hr = _ciControl(_ciApiInstance, command, inputParams, outputParams);
        }
        if (hr == (int)HResult.Severity.Success)
        {
          this.LogDebug("Anysee: result = success");
          return true;
        }

        this.LogError("Anysee: failed to execute CI control command {0}, hr = 0x{1:x} ({2})", command, hr, HResult.GetDXErrorString(hr));
        return false;
      }
    }

    #region call back definitions

    /// <summary>
    /// Called by the tuner driver when the common interface slot state changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot.</param>
    /// <param name="state">The new CI state.</param>
    /// <param name="message">A short description of the CI state.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int OnAnyseeCiState(int slotIndex, AnyseeCiState state, [MarshalAs(UnmanagedType.LPStr)] string message);

    /// <summary>
    /// Called by the tuner driver when MMI information is ready to be processed.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="message">The message from the CAM.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully processed</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int OnAnyseeMmiMessage(int slotIndex, IntPtr message);

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xb8e78938, 0x899d, 0x41bd, 0xb5, 0xb4, 0x62, 0x69, 0xf2, 0x80, 0x18, 0x99);

    private const int KS_PROPERTY_SIZE = 24;                                                  // Marshal.SizeOf(typeof(KsProperty))
    private const int RSSI_SIZE = KS_PROPERTY_SIZE + 8;
    private const int CNR_SIZE = KS_PROPERTY_SIZE + 8;
    private static readonly int IR_DATA_SIZE = Marshal.SizeOf(typeof(IrData));                // 32
    private static readonly int PLATFORM_INFO_SIZE = Marshal.SizeOf(typeof(PlatformInfo));    // 32
    private const int LOCKED_SIZE = KS_PROPERTY_SIZE + 8;
    private const int NIM_MODE_SIZE = KS_PROPERTY_SIZE + 8;
    private static readonly int STATUS_INFO_SIZE = Marshal.SizeOf(typeof(StatusInfo));        // 48
    private static readonly int DRIVER_VERSION_SIZE = Marshal.SizeOf(typeof(DriverVersion));  // 32
    private static readonly int NIM_CONFIG_SIZE = Marshal.SizeOf(typeof(NimConfig));          // 64
    private static readonly int LNB_INFO_SIZE = Marshal.SizeOf(typeof(LnbInfo));              // 64
    private static readonly int CAPABILITIES_SIZE = Marshal.SizeOf(typeof(Capabilities));     // 56
    private static readonly int BOARD_INFO_SIZE = Marshal.SizeOf(typeof(BoardInfo));          // 48
    private static readonly int DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(DiseqcMessage));  // 48
    private const int MAX_DISEQC_MESSAGE_LENGTH = 16;

    private const int MAX_API_STRING_LENGTH = 256;
    private const int MAX_CAM_MENU_ENTRIES = 32;
    private static readonly int MMI_MENU_SIZE = Marshal.SizeOf(typeof(MmiMenu));              // 8
    private static readonly int MMI_MESSAGE_SIZE = Marshal.SizeOf(typeof(MmiMessage));        // 32
    private const int MAX_DESCRIPTOR_DATA_LENGTH = 256;
    private const int MAX_PMT_ELEMENTARY_STREAMS = 50;
    private static readonly int ES_PMT_DATA_SIZE = Marshal.SizeOf(typeof(EsPmtData));         // 260
    private static readonly int PMT_DATA_SIZE = Marshal.SizeOf(typeof(PmtData));              // 13268

    private static readonly int GENERAL_BUFFER_SIZE = new int[] { BOARD_INFO_SIZE, CAPABILITIES_SIZE, DISEQC_MESSAGE_SIZE, DRIVER_VERSION_SIZE, NIM_CONFIG_SIZE, PLATFORM_INFO_SIZE }.Max();

    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    private bool _isAnysee = false;
    private bool _isCaInterfaceOpen = false;
    private bool _isCiSlotPresent = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private AnyseeCiState _ciState = AnyseeCiState.Empty;
    private string _tunerDevicePath = string.Empty;

    private IKsPropertySet _propertySet = null;
    private AnyseeCiApi _ciApi = null;

    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;
    private IntPtr _remoteControlBuffer = IntPtr.Zero;

    private OnAnyseeCiState _ciStateChangeDelegate = null;
    private IntPtr _ciStateChangeCallBackPtr = IntPtr.Zero;
    private OnAnyseeMmiMessage _mmiMessageDelegate = null;
    private IntPtr _mmiMessageCallBackPtr = IntPtr.Zero;
    private IConditionalAccessMenuCallBacks _caMenuCallBacks = null;
    private object _caMenuCallBackLock = new object();

    private bool _isRemoteControlInterfaceOpen = false;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

    #endregion

    #region hardware/software information

    /// <summary>
    /// Attempt to read the NIM configuration information from the tuner.
    /// </summary>
    private void ReadNimConfig()
    {
      this.LogDebug("Anysee: read NIM configuration");

      for (int i = 0; i < NIM_CONFIG_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.NimConfig,
        _generalBuffer, NIM_CONFIG_SIZE,
        _generalBuffer, NIM_CONFIG_SIZE,
        out returnedByteCount
      );
      if (hr != (int)HResult.Severity.Success || returnedByteCount != NIM_CONFIG_SIZE)
      {
        this.LogWarn("Anysee: failed to read NIM configuration, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
      }

      // Most of the info here is not very relevant.
      NimConfig info = (NimConfig)Marshal.PtrToStructure(_generalBuffer, typeof(NimConfig));
      this.LogDebug("  symbol rate      = {0} s/s", info.SymbolRate);
      this.LogDebug("  sweep rate       = {0} Hz/s", info.SweepRate);
      this.LogDebug("  frequency        = {0} kHz", info.Frequency);
      this.LogDebug("  carrier offset   = {0} kHz", info.CarrierOffset);
      this.LogDebug("  bandwidth        = {0} MHz", info.Bandwidth);
      this.LogDebug("  NIM type         = {0}", info.NimType);
      this.LogDebug("  analog mode      = {0}", info.AnalogNimMode);
      this.LogDebug("  digital mode     = {0}", info.DigitalNimMode);
      this.LogDebug("  inversion        = {0}", info.SignalInversion);
      this.LogDebug("  scan direction   = {0}", info.ScanDirection);
    }

    /// <summary>
    /// Attempt to read the driver information from the tuner.
    /// </summary>
    private void ReadDriverVersion()
    {
      this.LogDebug("Anysee: read driver version");

      for (int i = 0; i < DRIVER_VERSION_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DriverVersion,
        _generalBuffer, DRIVER_VERSION_SIZE,
        _generalBuffer, DRIVER_VERSION_SIZE,
        out returnedByteCount
      );
      if (hr != (int)HResult.Severity.Success || returnedByteCount != DRIVER_VERSION_SIZE)
      {
        this.LogWarn("Anysee: failed to read driver version, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
        return;
      }

      DriverVersion version = (DriverVersion)Marshal.PtrToStructure(_generalBuffer, typeof(DriverVersion));
      this.LogDebug("  version          = {0:x}.{1:x}.{2:x}.{3:x}", version.Version[3], version.Version[2], version.Version[1], version.Version[0]);
    }

    /// <summary>
    /// Attempt to read the platform (chipset/product) information from the tuner.
    /// </summary>
    private void ReadPlatformInfo()
    {
      this.LogDebug("Anysee: read platform information");

      for (int i = 0; i < PLATFORM_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PlatformInfo,
        _generalBuffer, PLATFORM_INFO_SIZE,
        _generalBuffer, PLATFORM_INFO_SIZE,
        out returnedByteCount
      );
      if (hr != (int)HResult.Severity.Success || returnedByteCount != PLATFORM_INFO_SIZE)
      {
        this.LogWarn("Anysee: failed to read platform information, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
        return;
      }

      PlatformInfo info = (PlatformInfo)Marshal.PtrToStructure(_generalBuffer, typeof(PlatformInfo));
      this.LogDebug("  platform         = {0}", info.Platform);
      this.LogDebug("  firmware version = {0}.{1}", info.FirmwareVersion[1], info.FirmwareVersion[0]);

      if (info.Platform == AnyseePlatform.Pcb508S2 ||
        info.Platform == AnyseePlatform.Pcb508TC ||
        info.Platform == AnyseePlatform.Pcb508T2C ||
        info.Platform == AnyseePlatform.Pcb508PS2 ||
        info.Platform == AnyseePlatform.Pcb508PTC ||
        info.Platform == AnyseePlatform.Pcb508PT2C)
      {
        _isCiSlotPresent = true;
      }
    }

    /// <summary>
    /// Attempt to read the board (USB etc.) information from the tuner.
    /// </summary>
    private void ReadBoardInfo()
    {
      this.LogDebug("Anysee: read board information");

      for (int i = 0; i < BOARD_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.BoardInfo,
        _generalBuffer, BOARD_INFO_SIZE,
        _generalBuffer, BOARD_INFO_SIZE,
        out returnedByteCount
      );
      if (hr != (int)HResult.Severity.Success || returnedByteCount != BOARD_INFO_SIZE)
      {
        this.LogWarn("Anysee: failed to read board information, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
        return;
      }

      //Dump.DumpBinary(_generalBuffer, BOARD_INFO_SIZE);
      BoardInfo info = (BoardInfo)Marshal.PtrToStructure(_generalBuffer, typeof(BoardInfo));
      this.LogDebug("  bus type         = {0}", info.BusType);
      this.LogDebug("  board type       = {0}", info.BoardType);
      this.LogDebug("  board properties = {0}", info.BoardProperties);
      this.LogDebug("  board mode       = {0}", info.BoardMode);
    }

    /// <summary>
    /// Attempt to read the tuner capabilities.
    /// </summary>
    private void ReadCapabilities()
    {
      this.LogDebug("Anysee: read capabilities");

      for (int i = 0; i < CAPABILITIES_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Capabilities,
        _generalBuffer, CAPABILITIES_SIZE,
        _generalBuffer, CAPABILITIES_SIZE,
        out returnedByteCount
      );
      if (hr != (int)HResult.Severity.Success || returnedByteCount != CAPABILITIES_SIZE)
      {
        this.LogWarn("Anysee: failed to read capabilities, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
        return;
      }

      Capabilities capabilities = (Capabilities)Marshal.PtrToStructure(_generalBuffer, typeof(Capabilities));
      this.LogDebug("  min frequency    = {0} kHz", capabilities.MinFrequency);
      this.LogDebug("  max frequency    = {0} kHz", capabilities.MaxFrequency);
      this.LogDebug("  min symbol rate  = {0} s/s", capabilities.MinSymbolRate);
      this.LogDebug("  max symbol rate  = {0} s/s", capabilities.MaxSymbolRate);
      this.LogDebug("  min search step  = {0} Hz", capabilities.MinSearchStep);
      this.LogDebug("  max search step  = {0} Hz", capabilities.MaxSearchStep);
      this.LogDebug("  capabilities     = {0}", capabilities.NimCapabilities);
      this.LogDebug("  broadcast system = {0}", capabilities.PrimaryBroadcastSystem);
    }

    #endregion

    /// <summary>
    /// Send key press event information to the CAM. This is the mechanism that
    /// is used for interaction within the CAM menu.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns><c>true</c> if the key code is passed to the CAM successfully, otherwise <c>false</c></returns>
    private bool SendKey(AnyseeCamMenuKey key)
    {
      this.LogDebug("Anysee: send key, key = {0}", key);
      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Anysee: not initialised or interface not supported");
        return false;
      }
      if (_isCamReady == false)
      {
        this.LogError("Anysee: failed to send key press to CAM, the CAM is not ready");
        return false;
      }

      IntPtr keyBuffer = Marshal.AllocCoTaskMem(sizeof(AnyseeCamMenuKey));
      try
      {
        Marshal.WriteInt32(keyBuffer, (int)key);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetKey, keyBuffer, IntPtr.Zero))
        {
          this.LogDebug("Anysee: result = success");
          return true;
        }

        this.LogError("Anysee: failed to send key press {0} to CAM", key);
        return false;
      }
      finally
      {
        Marshal.FreeCoTaskMem(keyBuffer);
      }
    }

    #region call back handlers

    /// <summary>
    /// Called by the tuner driver when the common interface slot state changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot.</param>
    /// <param name="state">The new CI state.</param>
    /// <param name="message">A short description of the CI state.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    private int OnCiState(int slotIndex, AnyseeCiState state, string message)
    {
      // If a CAM is inserted the API seems to only invoke this call back when the CAM state
      // changes. However, if a CAM is *not* inserted then this call back is invoked every
      // time the API polls the CI state. We don't want to log the polling - it would swamp
      // the logs.
      if (state == _ciState)
      {
        return 0;
      }

      this.LogInfo("Anysee: CI state change call back, slot = {0}", slotIndex);

      // Update the CI state variables.
      this.LogInfo("  old state = {0}", _ciState);
      this.LogInfo("  new state = {0}", state);
      _ciState = state;
      if (state == AnyseeCiState.CamInserted || state == AnyseeCiState.CamOkay)
      {
        _isCamPresent = true;
        _isCamReady = false;
      }
      else if (state == AnyseeCiState.CamReady || state == AnyseeCiState.SendPmtComplete || state == AnyseeCiState.Clear)
      {
        _isCamPresent = true;
        _isCamReady = true;
      }
      else
      {
        _isCamPresent = false;
        _isCamReady = false;
      }

      if (string.IsNullOrEmpty(message))
      {
        message = "(no message)";
      }
      this.LogInfo("  message   = {0}", message.Trim());

      return 0;
    }

    /// <summary>
    /// Called by the tuner driver when MMI information is ready to be processed.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="message">The message from the CAM.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully processed</returns>
    private int OnMmiMessage(int slotIndex, IntPtr message)
    {
      this.LogInfo("Anysee: MMI message call back, slot = {0}", slotIndex);

      MmiMessage msg = (MmiMessage)Marshal.PtrToStructure(message, typeof(MmiMessage));
      this.LogDebug("  device index  = {0}", msg.DeviceIndex);
      this.LogDebug("  slot index    = {0}", msg.SlotIndex);
      this.LogDebug("  menu title    = {0}", DvbTextConverter.Convert(msg.RootMenuTitle));
      this.LogInfo("  message type  = {0}", msg.Type);
      MmiMenu menu = (MmiMenu)Marshal.PtrToStructure(msg.Menu, typeof(MmiMenu));
      this.LogDebug("  string count  = {0}", menu.StringCount);
      this.LogDebug("  menu index    = {0}", menu.MenuIndex);


      // Enquiry
      if (msg.Type == AnyseeMmiMessageType.InputRequest)
      {
        this.LogDebug("Anysee: enquiry");
        if (msg.HeaderCount != 1)
        {
          this.LogError("Anysee: unexpected MMI input request header count, count = {0}", msg.HeaderCount);
          Dump.DumpBinary(message, MMI_MESSAGE_SIZE);
          return 1;
        }
        lock (_caMenuCallBackLock)
        {
          if (_caMenuCallBacks == null)
          {
            this.LogDebug("Anysee: menu call backs are not set");
          }

          string prompt = DvbTextConverter.Convert(Marshal.ReadIntPtr(menu.Entries, 0));
          this.LogDebug("  prompt    = {0}", prompt);
          this.LogDebug("  length    = {0}", msg.ExpectedAnswerLength);
          this.LogDebug("  key count = {0}", msg.KeyCount);
          if (_caMenuCallBacks != null)
          {
            _caMenuCallBacks.OnCiRequest(false, (uint)msg.ExpectedAnswerLength, prompt);
          }
        }
        return 0;
      }

      // Menu or list
      this.LogDebug("Anysee: menu");
      if (msg.HeaderCount != 3)
      {
        this.LogError("Anysee: unexpected MMI menu or list header count, count = {0}", msg.HeaderCount);
        Dump.DumpBinary(message, MMI_MESSAGE_SIZE);
        return 1;
      }
      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBacks == null)
        {
          this.LogDebug("Anysee: menu call backs are not set");
        }

        string title = DvbTextConverter.Convert(Marshal.ReadIntPtr(menu.Entries, 0));
        string subTitle = DvbTextConverter.Convert(Marshal.ReadIntPtr(menu.Entries, IntPtr.Size));
        string footer = DvbTextConverter.Convert(Marshal.ReadIntPtr(menu.Entries, IntPtr.Size * 2));
        this.LogDebug("  title     = {0}", title);
        this.LogDebug("  sub-title = {0}", subTitle);
        this.LogDebug("  footer    = {0}", footer);
        this.LogDebug("  # entries = {0}", msg.EntryCount);
        if (msg.EntryCount > MAX_CAM_MENU_ENTRIES - 3)
        {
          this.LogError("Anysee: MMI menu or list entry count {0} exceeds the maximum supported entry count {1}", msg.EntryCount, MAX_CAM_MENU_ENTRIES - 3);
          Dump.DumpBinary(message, MMI_MESSAGE_SIZE);
          return 1;
        }
        if (_caMenuCallBacks != null)
        {
          _caMenuCallBacks.OnCiMenu(title, subTitle, footer, msg.EntryCount);
        }

        string entry;
        IntPtr entryPtr = IntPtr.Add(menu.Entries, IntPtr.Size * 2);
        for (int i = 0; i < msg.EntryCount; i++)
        {
          entry = DvbTextConverter.Convert(Marshal.ReadIntPtr(entryPtr, 0));
          entryPtr = IntPtr.Add(entryPtr, IntPtr.Size);
          this.LogDebug("    {0, -7} = {1}", i + 1, entry);
          if (_caMenuCallBacks != null)
          {
            _caMenuCallBacks.OnCiMenuChoice(i, entry);
          }
        }
      }
      return 0;
    }

    #endregion

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartRemoteControlListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isRemoteControlInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_remoteControlListenerThread != null && !_remoteControlListenerThread.IsAlive)
      {
        StopRemoteControlListenerThread();
      }
      if (_remoteControlListenerThread == null)
      {
        this.LogDebug("Anysee: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Anysee remote control listener";
        _remoteControlListenerThread.IsBackground = true;
        _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
        _remoteControlListenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopRemoteControlListenerThread()
    {
      if (_remoteControlListenerThread != null)
      {
        if (!_remoteControlListenerThread.IsAlive)
        {
          this.LogWarn("Anysee: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Anysee: failed to join remote control listener thread, aborting thread");
            _remoteControlListenerThread.Abort();
          }
        }
        _remoteControlListenerThread = null;
        if (_remoteControlListenerThreadStopEvent != null)
        {
          _remoteControlListenerThreadStopEvent.Close();
          _remoteControlListenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void RemoteControlListener()
    {
      this.LogDebug("Anysee: remote control listener thread start polling");
      int hr;
      int returnedByteCount;
      int previousCode = 0;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          for (int i = 0; i < IR_DATA_SIZE; i++)
          {
            Marshal.WriteByte(_remoteControlBuffer, i, 0);
          }
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir,
            _remoteControlBuffer, IR_DATA_SIZE,
            _remoteControlBuffer, IR_DATA_SIZE,
            out returnedByteCount
          );
          if (hr != (int)HResult.Severity.Success || returnedByteCount != IR_DATA_SIZE)
          {
            this.LogError("Anysee: failed to read IR data, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
          }
          else
          {
            IrData data = (IrData)Marshal.PtrToStructure(_remoteControlBuffer, typeof(IrData));
            if (data.Key != previousCode)
            {
              this.LogDebug("Anysee: remote control key press = {0}", data.Key);
              previousCode = data.Key;
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Anysee: remote control listener thread exception");
        return;
      }
      this.LogDebug("Anysee: remote control listener thread stop polling");
    }

    #endregion

    #region ICustomDevice members

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
      this.LogDebug("Anysee: initialising");

      if (_isAnysee)
      {
        this.LogWarn("Anysee: extension already initialised");
        return true;
      }

      IBaseFilter tunerFilter = context as IBaseFilter;
      if (tunerFilter == null)
      {
        this.LogDebug("Anysee: context is not a filter");
        return false;
      }
      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("Anysee: tuner external identifier is not set");
        return false;
      }

      // We need a reference to the capture filter because that is the filter which
      // actually implements the important property sets.
      IPin captureInputPin;
      IPin tunerOutputPin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      if (tunerOutputPin == null)
      {
        this.LogDebug("Anysee: failed to find the tuner filter output pin");
        return false;
      }
      int hr = tunerOutputPin.ConnectedTo(out captureInputPin);
      Release.ComObject("Anysee tuner filter output pin", ref tunerOutputPin);
      if (hr != (int)HResult.Severity.Success || captureInputPin == null)
      {
        this.LogDebug("Anysee: failed to get the capture filter input pin, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      PinInfo captureInfo;
      hr = captureInputPin.QueryPinInfo(out captureInfo);
      Release.ComObject("Anysee capture filter input pin", ref captureInputPin);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("Anysee: failed to get the capture filter input pin info, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      // Check if the filter supports the property set.
      _propertySet = captureInfo.filter as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Anysee: capture filter is not a property set");
        Release.PinInfo(ref captureInfo);
        return false;
      }

      KSPropertySupport support;
      hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir, out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Anysee: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        Release.PinInfo(ref captureInfo);
        _propertySet = null;
        return false;
      }

      this.LogInfo("Anysee: extension supported");
      _isAnysee = true;
      _tunerDevicePath = tunerExternalId;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in devices)
      {
        if (!string.IsNullOrEmpty(device.DevicePath) && tunerExternalId.Contains(device.DevicePath))
        {
          _tunerDevicePath = device.DevicePath;
          break;
        }
      }
      foreach (DsDevice device in devices)
      {
        device.Dispose();
      }
      _generalBuffer = Marshal.AllocCoTaskMem(NIM_CONFIG_SIZE);

      ReadNimConfig();
      ReadDriverVersion();
      ReadPlatformInfo();
      ReadBoardInfo();
      ReadCapabilities();
      return true;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenConditionalAccessInterface()
    {
      this.LogDebug("Anysee: open conditional access interface");

      if (!_isAnysee)
      {
        this.LogWarn("Anysee: not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("Anysee: conditional access interface is already open");
        return true;
      }

      // Is a CI slot present? If not, there is no point in opening the interface.
      ReadPlatformInfo();
      if (!_isCiSlotPresent)
      {
        this.LogDebug("Anysee: CI slot not present");
        return false;
      }

      _ciApi = new AnyseeCiApi();
      if (!_ciApi.OpenApi(_tunerDevicePath))
      {
        this.LogError("Anysee: failed to open CI API");
        _ciApi.CloseApi();
        _ciApi = null;
        return false;
      }

      _pmtBuffer = Marshal.AllocCoTaskMem(PMT_DATA_SIZE);

      this.LogDebug("Anysee: setting call backs");
      _ciStateChangeDelegate = OnCiState;
      _ciStateChangeCallBackPtr = Marshal.GetFunctionPointerForDelegate(_ciStateChangeDelegate);
      _mmiMessageDelegate = OnMmiMessage;
      _mmiMessageCallBackPtr = Marshal.GetFunctionPointerForDelegate(_mmiMessageDelegate);
      if (_ciApi.ExecuteCommand(AnyseeCiCommand.IsOpenSetCallBacks, _ciStateChangeCallBackPtr, _mmiMessageCallBackPtr))
      {
        this.LogDebug("Anysee: result = success");
        _isCaInterfaceOpen = true;
        return true;
      }

      this.LogError("Anysee: failed to open conditional access interface");
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseConditionalAccessInterface()
    {
      this.LogDebug("Anysee: close conditional access interface");

      bool result = true;
      if (_ciApi != null)
      {
        result = _ciApi.CloseApi();
        _ciApi = null;
      }
      _isCiSlotPresent = false;
      _isCamPresent = false;
      _isCamReady = false;

      if (_pmtBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_pmtBuffer);
        _pmtBuffer = IntPtr.Zero;
      }
      _ciStateChangeCallBackPtr = IntPtr.Zero;
      _mmiMessageCallBackPtr = IntPtr.Zero;

      if (result)
      {
        this.LogDebug("Anysee: result = success");
        _isCaInterfaceOpen = false;
        return true;
      }

      this.LogError("Anysee: failed to close conditional access interface");
      return false;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="resetTuner">This parameter will be set to <c>true</c> if the tuner must be reset
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    public bool ResetConditionalAccessInterface(out bool resetTuner)
    {
      resetTuner = false;
      return CloseConditionalAccessInterface() && OpenConditionalAccessInterface();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsConditionalAccessInterfaceReady()
    {
      this.LogDebug("Anysee: is conditional access interface ready");
      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Anysee: not initialised or interface not supported");
        return false;
      }

      // The CAM state is automatically updated in the OnCiState() call back.
      this.LogDebug("Anysee: result = {0}", _isCamReady);
      return _isCamReady;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    //public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    public bool SendConditionalAccessCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      this.LogDebug("Anysee: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Anysee: not initialised or interface not supported");
        return false;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        this.LogError("Anysee: conditional access command type {0} is not supported", command);
        return true;
      }
      if (pmt == null)
      {
        this.LogError("Anysee: failed to send conditional access command, PMT not supplied");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CaPmtCommand.NotSelected)
      {
        this.LogDebug("Anysee: result = success");
        return true;
      }

      // Anysee tuners only support decrypting one channel at a time. We'll just send this PMT to the CAM
      // regardless of the list management action.
      PmtData pmtData = new PmtData();
      pmtData.PmtByte6 = (byte)((pmt.Version << 1) | pmt.CurrentNextIndicator);
      pmtData.PcrPid = pmt.PcrPid;
      pmtData.ServiceId = pmt.ProgramNumber;

      // Program CA descriptor data.
      int offset = 2;   // 2 bytes reserved for the data length
      pmtData.ProgramCaDescriptorData = new byte[MAX_DESCRIPTOR_DATA_LENGTH];
      foreach (IDescriptor d in pmt.ProgramCaDescriptors)
      {
        ReadOnlyCollection<byte> descriptorData = d.GetRawData();
        if (offset + descriptorData.Count >= MAX_DESCRIPTOR_DATA_LENGTH)
        {
          this.LogError("Anysee: PMT program CA descriptor data is too long");
          return false;
        }
        descriptorData.CopyTo(pmtData.ProgramCaDescriptorData, offset);
        offset += descriptorData.Count;
      }
      pmtData.ProgramCaDescriptorData[0] = (byte)((offset - 2) & 0xff);
      pmtData.ProgramCaDescriptorData[1] = (byte)(((offset - 2) >> 8) & 0xff);

      // Elementary streams.
      this.LogDebug("Anysee: elementary streams");
      pmtData.EsPmt = new EsPmtData[MAX_PMT_ELEMENTARY_STREAMS];
      ushort esCount = 0;
      foreach (PmtElementaryStream es in pmt.ElementaryStreams)
      {
        // We want to add each video, audio, sub-title and teletext stream with their corresponding
        // conditional access descriptors.
        AnyseeEsType esType = AnyseeEsType.Unknown;

        if (StreamTypeHelper.IsVideoStream(es.LogicalStreamType))
        {
          esType = AnyseeEsType.Video;
        }
        else if (StreamTypeHelper.IsAudioStream(es.LogicalStreamType))
        {
          esType = AnyseeEsType.Audio;
        }
        else if (es.LogicalStreamType == LogicalStreamType.Subtitles)
        {
          esType = AnyseeEsType.Subtitle;
        }
        else if (es.LogicalStreamType == LogicalStreamType.Teletext)
        {
          esType = AnyseeEsType.Teletext;
        }

        // So do we actually want to keep this stream?
        if (esType == AnyseeEsType.Unknown)
        {
          // Nope.
          this.LogDebug("  excluding PID {0}, stream type = {1})", es.Pid, es.StreamType);
          continue;
        }

        // Yes!!!
        this.LogDebug("  including PID {0}, stream type = {1}, category = {2}", es.Pid, es.StreamType, esType);
        EsPmtData esToKeep = new EsPmtData();
        esToKeep.Pid = es.Pid;
        esToKeep.EsType = esType;
        esToKeep.StreamType = (byte)es.StreamType;

        // Elementary stream CA descriptor data.
        offset = 2;   // 2 bytes reserved for the data length
        esToKeep.DescriptorData = new byte[MAX_DESCRIPTOR_DATA_LENGTH];
        foreach (IDescriptor d in es.CaDescriptors)
        {
          ReadOnlyCollection<byte> descriptorData = d.GetRawData();
          if (offset + descriptorData.Count >= MAX_DESCRIPTOR_DATA_LENGTH)
          {
            this.LogError("Anysee: PMT elementary stream {0} CA data is too long", es.Pid);
            return false;
          }
          descriptorData.CopyTo(esToKeep.DescriptorData, offset);
          offset += descriptorData.Count;
        }
        esToKeep.DescriptorData[0] = (byte)((offset - 2) & 0xff);
        esToKeep.DescriptorData[1] = (byte)(((offset - 2) >> 8) & 0xff);
        pmtData.EsPmt[esCount++] = esToKeep;

        if (esCount == MAX_PMT_ELEMENTARY_STREAMS)
        {
          this.LogWarn("Anysee: reached maximum number of included PIDs");
          break;
        }
      }

      this.LogDebug("Anysee: total included PIDs = {0}", esCount);
      pmtData.EsCount = esCount;

      // Pass the PMT structure to the API.
      Marshal.StructureToPtr(pmtData, _pmtBuffer, false);
      //Dump.DumpBinary(_pmtBuffer, PMT_DATA_SIZE);
      if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetPmt, _pmtBuffer, IntPtr.Zero))
      {
        this.LogDebug("Anysee: result = success");
        return true;
      }

      this.LogError("Anysee: failed to send conditional access command");
      return false;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBacks">The call back delegate.</param>
    public void SetCallBacks(IConditionalAccessMenuCallBacks callBacks)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBacks = callBacks;
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterMenu()
    {
      this.LogDebug("Anysee: enter menu");
      bool result = SendKey(AnyseeCamMenuKey.Exit);
      return result && SendKey(AnyseeCamMenuKey.Menu);
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseMenu()
    {
      this.LogDebug("Anysee: close menu");
      return SendKey(AnyseeCamMenuKey.Exit);
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenuEntry(byte choice)
    {
      this.LogDebug("Anysee: select menu entry, choice = {0}", choice);
      if (choice == 0)
      {
        // Going back to the previous menu is not supported.
        return SendKey(AnyseeCamMenuKey.Exit);
      }
      while (choice > 1)
      {
        if (!SendKey(AnyseeCamMenuKey.Down))
        {
          return false;
        }
        choice--;
      }
      return SendKey(AnyseeCamMenuKey.Select);
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("Anysee: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      for (int i = 0; i < answer.Length; i++)
      {
        // We can't send anything other than numbers through the Anysee interface.
        int digit;
        if (!int.TryParse(answer[i].ToString(), out digit))
        {
          this.LogError("Anysee: answer may only contain numeric digits");
          return false;
        }
        if (!SendKey((AnyseeCamMenuKey)(digit + 1)))
        {
          return false;
        }
      }
      return SendKey(AnyseeCamMenuKey.Select);
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The Anysee interface does not support directly setting the 22 kHz tone state. The tuning request
    /// LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Anysee: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isAnysee)
      {
        this.LogWarn("Anysee: not initialised or interface not supported");
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = 0;
      message.ToneBurst = AnyseeToneBurst.Off;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        message.ToneBurst = AnyseeToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        message.ToneBurst = AnyseeToneBurst.DataBurst;
      }

      Marshal.StructureToPtr(message, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, DISEQC_MESSAGE_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc,
        _generalBuffer, DISEQC_MESSAGE_SIZE,
        _generalBuffer, DISEQC_MESSAGE_SIZE
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Anysee: result = success");
        return true;
      }

      this.LogError("Anysee: failed to set tone state, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
    {
      this.LogDebug("Anysee: send DiSEqC command");

      if (!_isAnysee)
      {
        this.LogWarn("Anysee: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Anysee: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Anysee: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.Message = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);
      message.MessageLength = command.Length;
      message.ToneBurst = AnyseeToneBurst.Off;

      Marshal.StructureToPtr(message, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, DISEQC_MESSAGE_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc,
        _generalBuffer, DISEQC_MESSAGE_SIZE,
        _generalBuffer, DISEQC_MESSAGE_SIZE
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Anysee: result = success");
        return true;
      }

      this.LogError("Anysee: failed to send DiSEqC command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadDiseqcResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenRemoteControlInterface()
    {
      this.LogDebug("Anysee: open remote control interface");

      if (!_isAnysee)
      {
        this.LogWarn("Anysee: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Anysee: remote control interface is already open");
        return true;
      }

      _remoteControlBuffer = Marshal.AllocCoTaskMem(IR_DATA_SIZE);
      IrData command = new IrData();
      command.Enable = true;
      Marshal.StructureToPtr(command, _remoteControlBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir,
        _remoteControlBuffer, IR_DATA_SIZE,
        _remoteControlBuffer, IR_DATA_SIZE
      );
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("Anysee: failed to enable IR commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      _isRemoteControlInterfaceOpen = true;
      StartRemoteControlListenerThread();

      this.LogDebug("Anysee: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseRemoteControlInterface()
    {
      this.LogDebug("Anysee: close remote control interface");

      StopRemoteControlListenerThread();
      if (_isRemoteControlInterfaceOpen && _propertySet != null)
      {
        IrData command = new IrData();
        command.Enable = false;
        Marshal.StructureToPtr(command, _remoteControlBuffer, false);
        int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir,
          _remoteControlBuffer, IR_DATA_SIZE,
          _remoteControlBuffer, IR_DATA_SIZE
        );

        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Anysee: failed to disable IR commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }
      }

      if (_remoteControlBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_remoteControlBuffer);
        _remoteControlBuffer = IntPtr.Zero;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Anysee: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_isAnysee)
      {
        CloseConditionalAccessInterface();
        CloseRemoteControlInterface();
      }
      Release.ComObject("Anysee property set", ref _propertySet);
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isAnysee = false;
    }

    #endregion
  }
}