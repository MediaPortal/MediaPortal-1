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
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Anysee
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for Anysee tuners. Smart card slots are not
  /// supported.
  /// </summary>
  public class Anysee : BaseCustomDevice, IConditionalAccessProvider, ICiMenuActions, IDiseqcDevice 
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
    	STV0297J_DNOS881Zx121A,			// ST (QAM demod)/???
    	DNQS441PH261A,					    // Samsung
    	TDA10023HT_DTOS203IH102A,		// Philips/NXP/Trident (QAM demod)/??? (DVB-C tuner)
    	DNBU10321IRT,
    	FakeHW,                     // (simulation hardware)
    	BS2N10WCC01,					      // "DVB-S2, Cosy NIM" - Conexant CX24116/CX24118 (QPSK, DVB-S2 QPSK/8PSK demod)
    	ZL10353_XC5000,					    // Zarlink (COFDM demod)/Xceive (hybrid digital/analog tuner)
    	TDA10023HT_XC5000,				  // Philips/NXP/Trident (QAM demod)/Xceive (hybrid digital/analog tuner)
    	ZL10353_DTOS203IH102A,      // Zarlink (COFDM demod)/???
    	ZL10353_DTOS403Ix102x,			// Zarlink (COFDM demod)/???
    	TDA10023HT_DTOS403Ix102x,		// Philips/NXP/Trident (QAM demod)
    	DNBU10512IST,					      // ST NIM - STV0903 (QPSK, DC II, DVB-S2 QPSK/8PSK demod), STV6110
    	M88DS3002_M88TS2020,			  // LG Montage NIM - M88DS3002 (QPSK, DVB-S2 QPSK/8PSK/16APSK/32APSK demod), M88TS2020 (digital tuner)
    	ZL10353_EN4020,					    // Zarlink (COFDM demod)/Entropic (hybrid digital/analog terrestrial/cable tuner)
    	TDA10023HT_EN4020,				  // Philips/NXP/Trident (QAM demod)/Entropic (hybrid digital/analog terrestrial/cable tuner)
    	ZL10353_TDA18212,				    // Zarlink (COFDM demod)/NXP DNOD44CDV086A (hybrid digital/analog terrestrial/cable tuner)
    	TDA10023HT_TDA18212,			  // Philips/NXP/Trident (QAM demod)/NXP DNOD44CDV086A (hybrid digital/analog terrestrial/cable tuner)
    	CXD2820_TDA18272,				    // ??? (DVB-T2 demod)/NXP DNOD44CDV086A (hybrid digital/analog terrestrial/cable tuner)
    	DNOQ44QCV106A,					    // Samsung NIM - DVB-T2/C
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
      IsOpenSetCallbacks = 2000 // CI_CONTROL_IS_PLUG_OPEN - check whether the CI API is open and set callback functions
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

    [StructLayout(LayoutKind.Sequential)]
    private struct IrData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.Bool)]
      public bool Enable;
      public Int32 Key;         // bit 8 = repeat flag (0 = repeat), bits 7-0 = key code
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PlatformInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
      public byte[] FirmwareVersion;    // [0x04, 0x00] -> 0.4
      public AnyseePlatform Platform;
      private Int32 Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StatusInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      public AnyseeBroadcastSystem CurrentBroadcastSystem;
      public AnyseeNimMode CurrentNimMode;

      public Int32 CurrentFrequency;    // unit = kHz
      public Int32 Unknown1;

      public AnyseeNim NimType;
      private Int32 Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DriverVersion
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public byte[] Version;            // [0x58, 0x20, 0x06, 0x01] -> 1.6.20.58
      private Int32 Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NimConfig
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

	    public Int32 SymbolRate;      // unit = s/s (Baud)
      public Int32 SweepRate;       // unit = Hz/s

      public Int32 Frequency;       // unit = kHz
      public Int32 CarrierOffset;   // unit = kHz

      public byte Bandwidth;        // unit = Mhz
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding;
      public AnyseeNim NimType;

      public AnyseeNimMode AnalogNimMode;
      public AnyseeNimMode DigitalNimMode;

      public AnyseeInversionMode SignalInversion;
      public AnyseeScanDirection ScanDirection;
	  }

    [StructLayout(LayoutKind.Sequential)]
    private struct LnbInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      public Int32 UnknownFlags;
      public Int32 SwitchFrequency; // unit = MHz

      public Int32 HighLof;         // unit = MHz
      public Int32 LowLof;          // unit = MHz

      public Int32 EffectiveLof;    // unit = MHz
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
      private byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Capabilities
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      public Int32 MinFrequency;    // unit = kHz
      public Int32 MaxFrequency;    // unit = kHz

      public Int32 MinSymbolRate;   // unit = s/s (Baud)
      public Int32 MaxSymbolRate;   // unit = s/s (Baud)

      public Int32 MinSearchStep;   // unit = Hz
      public Int32 MaxSearchStep;   // unit = Hz

      public AnyseeNimCapability NimCapabilities;
      public AnyseeBroadcastSystem PrimaryBroadcastSystem;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BoardInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public byte[] FirmwareVersion;    // [0x04, 0x00, 0x00, 0x00] -> 0.4
      public AnyseeBusType BusType;

      public AnyseeBoardType BoardType;
      public AnyseeBoardProperty BoardProperties;

      public AnyseeBoardMode BoardMode;
      private Int32 Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DiseqcMessage
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;

      public Int32 MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;

      public AnyseeToneBurst ToneBurst;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ApiString
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxApiStringLength)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct CiStateInfo    // tagCIStatus
    {
      public Int32 Size;
      public Int32 DeviceIndex;

      public ApiString Message;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MmiMenu  // MMIStrsBlock
    {
      public Int32 StringCount;
      public Int32 MenuIndex;
      public IntPtr Entries;                // This is a pointer to an array of pointers.
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MmiMessage   // tagCIMsgs
    {
      public Int32 DeviceIndex;
      public Int32 SlotIndex;

      public Int32 HeaderCount;
      public Int32 EntryCount;

      public AnyseeMmiMessageType Type;
      public Int32 ExpectedAnswerLength;

      public Int32 KeyCount;
      public ApiString RootMenuTitle;

      public IntPtr Menu;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ApiCallbacks
    {
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnAnyseeCiState OnCiState;
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnAnyseeMmiMessage OnMmiMessage;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PmtData    // DTVCIPMT
    {
      public byte PmtByte6;                     // Byte 6 from the PMT section (PMT version, current next indicator). 
      private byte Padding1;
      public UInt16 PcrPid;
      public UInt16 ServiceId;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDescriptorDataLength)]
      public byte[] ProgramCaDescriptorData;    // The first two bytes should specify the length of the descriptor data.
      private UInt16 Padding2;

      public UInt32 EsCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPmtElementaryStreams)]
      public EsPmtData[] EsPmt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct EsPmtData
    {
      public UInt16 Pid;
      public AnyseeEsType EsType;
      public byte StreamType;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDescriptorDataLength)]
      public byte[] DescriptorData;           // The first two bytes should specify the length of the descriptor data.
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

      [StructLayout(LayoutKind.Sequential)]
      private struct CiDeviceInfo   // ANYSEECIDEVICESINFO
      {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public ApiString[] DevicePaths;      // A list of the capture device paths for all Anysee devices connected to the system.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public Int32[] DevicePathLengths;     // The length of the corresponding device path in DevicePaths.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public Int32[] DevicePathIndices;     // The index of the corresponding capture device in the set of KSCATEGORY_BDA_RECEIVER_COMPONENT devices returned by the system enumerator.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public Int32[] DeviceIndices;         // The Anysee device index for the corresponding device.
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
      private delegate Int32 CreateDtvCIAPI(IntPtr ciApiInstance);

      /// <summary>
      /// Destroy a previously created common interface API instance.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance to destroy. The memory must be released by TV Server after calling this function.</param>
      /// <returns><c>one</c> if the instance is successfully destroyed, otherwise <c>zero</c></returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 DestroyDtvCIAPI(IntPtr ciApiInstance);

      /// <summary>
      /// Get the number of Anysee devices connected to the system with corresponding
      /// device path and index detail to enable opening a common interface API instance.
      /// </summary>
      /// <param name="deviceInfo">A buffer containing device path and index information for all Anysee devices connected to the system.</param>
      /// <returns>the number of Anysee devices connected to the system</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 GetanyseeNumberofDevicesEx(IntPtr deviceInfo);

      #endregion

      #region CIAPI unmanaged class

      /// <summary>
      /// Open the common interface API for a specific Anysee device.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance to open.</param>
      /// <param name="windowHandle">A reference to a window to use as an alternative to callbacks (CIAPI.dll sends custom messages to the window).</param>
      /// <param name="deviceIndex">The Anysee index for the device to open.</param>
      /// <returns>an HRESULT indicating whether the API was successfully opened</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 OpenCILib(IntPtr ciApiInstance, IntPtr windowHandle, Int32 deviceIndex);

      /// <summary>
      /// Execute a command on an open common interface instance.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance.</param>
      /// <param name="command">The command to execute.</param>
      /// <param name="inputParams">A reference to a buffer containing the appropriate input parameters for the command.</param>
      /// <param name="outputParams">A reference to a buffer that will be filled with the command's output parameters.</param>
      /// <returns>an HRESULT indicating whether the command was successfully executed</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 CI_Control(IntPtr ciApiInstance, AnyseeCiCommand command, IntPtr inputParams, IntPtr outputParams);

      #endregion

      #endregion

      #region constants

      private const int ApiInstanceSize = 76;
      private const int MaxDeviceCount = 32;
      private const int CiDeviceInfoSize = MaxDeviceCount * (MaxApiStringLength + 12);
      private const int ApiAccessThreadSleepTime = 500;   // unit = ms

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
      private String _devicePath = String.Empty;

      private IntPtr _ciApiInstance = IntPtr.Zero;
      private IntPtr _windowHandle = IntPtr.Zero;
      private IntPtr _libHandle = IntPtr.Zero;

      private Thread _apiAccessThread = null;
      private bool _stopApiAccessThread = false;

      #endregion

      /// <summary>
      /// Create a new CI API instance.
      /// </summary>
      public AnyseeCiApi()
      {
        _apiCount++;
        _apiIndex = _apiCount;
        Log.Debug("Anysee: loading API, API index = {0}", _apiIndex);
        if (!File.Exists("Plugins\\CustomDevices\\Resources\\CIAPI" + _apiIndex + ".dll"))
        {
          try
          {
            File.Copy("Plugins\\CustomDevices\\Resources\\CIAPI.dll", "Plugins\\CustomDevices\\Resources\\CIAPI" + _apiIndex + ".dll");
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to copy CIAPI.dll\r\n{0}", ex.ToString());
            return;
          }
        }
        _libHandle = NativeMethods.LoadLibrary("Plugins\\CustomDevices\\Resources\\CIAPI" + _apiIndex + ".dll");
        if (_libHandle == IntPtr.Zero || _libHandle == null)
        {
          Log.Debug("Anysee: failed to load the DLL");
          return;
        }

        try
        {
          IntPtr function = NativeMethods.GetProcAddress(_libHandle, "CreateDtvCIAPI");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the CreateDtvCIAPI function");
            return;
          }
          try
          {
            _createApi = (CreateDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(CreateDtvCIAPI));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the CreateDtvCIAPI function\r\n{0}", ex.ToString());
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "DestroyDtvCIAPI");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the DestroyDtvCIAPI function");
            return;
          }
          try
          {
            _destroyApi = (DestroyDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(DestroyDtvCIAPI));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the DestroyDtvCIAPI function\r\n{0}", ex.ToString());
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "GetanyseeNumberofDevicesEx");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the GetanyseeNumberofDevicesEx function");
            return;
          }
          try
          {
            _getAnyseeDeviceCount = (GetanyseeNumberofDevicesEx)Marshal.GetDelegateForFunctionPointer(function, typeof(GetanyseeNumberofDevicesEx));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the GetanyseeNumberofDevicesEx function\r\n", ex.ToString());
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "?OpenCILib@CCIAPI@@UAGJPAUHWND__@@H@Z");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the OpenCILib function");
            return;
          }
          try
          {
            _openApi = (OpenCILib)Marshal.GetDelegateForFunctionPointer(function, typeof(OpenCILib));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the OpenCILib function\r\n{0}", ex.ToString());
            return;
          }

          function = NativeMethods.GetProcAddress(_libHandle, "?CI_Control@CCIAPI@@UAGJKPAJ0@Z");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the CI_Control function");
            return;
          }
          try
          {
            _ciControl = (CI_Control)Marshal.GetDelegateForFunctionPointer(function, typeof(CI_Control));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the CI_Control function\r\n{0}", ex.ToString());
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
      public bool OpenApi(String tunerDevicePath)
      {
        Log.Debug("Anysee: opening API, API index = {0}", _apiIndex);

        if (!_dllLoaded)
        {
          Log.Debug("Anysee: the CIAPI.dll functions were not successfully loaded");
          return false;
        }
        if (_apiAccessThread != null && _apiAccessThread.IsAlive)
        {
          Log.Debug("Anysee: API access thread is already running");
          return false;
        }

        // We only care about the device instance part of the device path
        // because the API provides capture device paths - matching the full
        // device path would not work.
        _devicePath = tunerDevicePath.Split('{')[0];

        _ciApiInstance = Marshal.AllocCoTaskMem(ApiInstanceSize);
        for (int i = 0; i < ApiInstanceSize; i++)
        {
          Marshal.WriteByte(_ciApiInstance, i, 0);
        }
        _windowHandle = Marshal.AllocCoTaskMem(4);

        // Technically all access to the CI API functions should be made
        // from a separate thread because the CIAPI DLL only supports single
        // thread apartment access. TV Server threads use MTA by default.
        // Those two threading models are incompatible. In practise I have
        // determined that it is only necessary to call the "static" DLL
        // functions from an STA thread. That effectively means that we
        // only need an STA thread to open the API and hold it open until
        // it is no longer needed.
        Log.Debug("Anysee: starting API access thread");
        _stopApiAccessThread = false;
        _apiAccessThread = new Thread(new ThreadStart(AccessThread));
        _apiAccessThread.Name = String.Format("Anysee API {0} Access", _apiCount);
        _apiAccessThread.IsBackground = true;
        _apiAccessThread.SetApartmentState(ApartmentState.STA);
        _apiAccessThread.Start();

        Thread.Sleep(500);

        // The thread will terminate almost immediately if there is any
        // problem. If the thread is alive after half a second then
        // everything should be okay.
        if (_apiAccessThread.IsAlive)
        {
          Log.Debug("Anysee: API access thread running");
          return true;
        }
        Log.Debug("Anysee: API access thread self-terminated");
        return false;
      }

      /// <summary>
      /// This is a thread function that actually creates and opens
      /// an Anysee API instance. The API is held open until the thread
      /// is stopped.
      /// </summary>
      private void AccessThread()
      {
        Log.Debug("Anysee: creating new CI API instance");
        int result = _createApi(_ciApiInstance);
        if (result != 1)
        {
          Log.Debug("Anysee: failed to create instance, result = {0}", result);
          return;
        }
        Log.Debug("Anysee: created instance successfully");

        // We have an API instance, but now we need to open it by linking it with hardware.
        Log.Debug("Anysee: determining instance index");
        IntPtr infoBuffer = Marshal.AllocCoTaskMem(CiDeviceInfoSize);
        for (int i = 0; i < CiDeviceInfoSize; i++)
        {
          Marshal.WriteByte(infoBuffer, i, 0);
        }

        int numDevices = _getAnyseeDeviceCount(infoBuffer);
        Log.Debug("Anysee: number of devices = {0}", numDevices);
        if (numDevices == 0)
        {
          Marshal.FreeCoTaskMem(infoBuffer);
          return;
        }
        CiDeviceInfo deviceInfo = (CiDeviceInfo)Marshal.PtrToStructure(infoBuffer, typeof(CiDeviceInfo));
        Marshal.FreeCoTaskMem(infoBuffer);

        String captureDevicePath;
        int index = -1;
        for (int i = 0; i < numDevices; i++)
        {
          captureDevicePath = deviceInfo.DevicePaths[i].Text.Substring(0, deviceInfo.DevicePathLengths[i]);
          Log.Debug("Anysee: device {0}", i + 1);
          Log.Debug("  device path  = {0}", captureDevicePath);
          Log.Debug("  index        = {0}", deviceInfo.DevicePathIndices[i]);
          Log.Debug("  Anysee index = {0}", deviceInfo.DeviceIndices[i]);

          if (captureDevicePath.StartsWith(_devicePath))
          {
            Log.Debug("Anysee: found correct instance");
            index = deviceInfo.DeviceIndices[i];
            break;
          }
        }

        // If we have a valid device index then we can attempt to open the CI API.
        if (index != -1)
        {
          Log.Debug("Anysee: opening CI API");
          result = _openApi(_ciApiInstance, _windowHandle, index);
          if (result == 0)
          {
            Log.Debug("Anysee: result = success");
            // Hold the API open until it is no longer needed.
            while (!_stopApiAccessThread)
            {
              Thread.Sleep(ApiAccessThreadSleepTime);
            }
          }
          else
          {
            Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", result, HResult.GetDXErrorString(result));
          }
        }

        // When this thread is stopped, we automatically destroy the API instance.
        Log.Debug("Anysee: destroying CI API instance");
        result = _destroyApi(_ciApiInstance);
        Log.Debug("Anysee: result = {0}", result);
      }

      /// <summary>
      /// Close the API.
      /// </summary>
      /// <returns><c>true</c> if the API is successfully closed, otherwise <c>false</c></returns>
      public bool CloseApi()
      {
        Log.Debug("Anysee: closing API");

        if (!_dllLoaded)
        {
          Log.Debug("Anysee: the CIAPI.dll functions have not been loaded");
          return true;
        }

        // Stop the API access thread.
        if (_apiAccessThread == null)
        {
          Log.Debug("Anysee: API access thread is null");
        }
        else
        {
          Log.Debug("Anysee: API access thread state = {0}", _apiAccessThread.ThreadState);
        }
        _stopApiAccessThread = true;
        // In the worst case scenario it should take approximately
        // twice the thread sleep time to cleanly stop the thread.
        Thread.Sleep(ApiAccessThreadSleepTime * 2);

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
        Log.Debug("Anysee: execute API command");

        if (_apiAccessThread == null)
        {
          Log.Debug("Anysee: API access thread is null");
          return false;
        }
        if (!_apiAccessThread.IsAlive)
        {
          Log.Debug("Anysee: the API is not open");
          return false;
        }

        int hr;
        lock (this)
        {
          hr = _ciControl(_ciApiInstance, command, inputParams, outputParams);
        }
        if (hr == 0)
        {
          Log.Debug("Anysee: result = success");
          return true;
        }

        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
    }

    #region callback definitions

    /// <summary>
    /// Called by the tuner driver when the common interface slot state changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot.</param>
    /// <param name="state">The new CI state.</param>
    /// <param name="message">A short description of the CI state.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Int32 OnAnyseeCiState(Int32 slotIndex, AnyseeCiState state, [MarshalAs(UnmanagedType.LPStr)] String message);

    /// <summary>
    /// Called by the tuner driver when MMI information is ready to be processed.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="message">The message from the CAM.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully processed</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Int32 OnAnyseeMmiMessage(Int32 slotIndex, IntPtr message);

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xb8e78938, 0x899d, 0x41bd, 0xb5, 0xb4, 0x62, 0x69, 0xf2, 0x80, 0x18, 0x99);

    private const int KsPropertySize = 24;
    private const int RssiSize = KsPropertySize + 8;
    private const int CnrSize = KsPropertySize + 8;
    private const int IrDataSize = KsPropertySize + 8;
    private const int PlatformInfoSize = KsPropertySize + 8;
    private const int LockedSize = KsPropertySize + 8;
    private const int NimModeSize = KsPropertySize + 8;
    private const int StatusInfoSize = KsPropertySize + 24;
    private const int DriverVersionSize = KsPropertySize + 8;
    private const int NimConfigSize = KsPropertySize + 40;
    private const int LnbInfoSize = KsPropertySize + 40;
    private const int CapabilitiesSize = KsPropertySize + 32;
    private const int BoardInfoSize = KsPropertySize + 24;
    private const int DiseqcMessageSize = KsPropertySize + MaxDiseqcMessageLength + 8;
    private const int MaxDiseqcMessageLength = 16;

    private const int MaxApiStringLength = 256;
    private const int MaxCamMenuEntries = 32;
    private const int CiStateInfoSize = 8 + MaxApiStringLength;
    private const int MmiMenuSize = (MaxCamMenuEntries * MaxApiStringLength) + 8;
    private const int MmiMessageSize = 32 + MaxApiStringLength;
    private const int ApiCallbackSize = 8;
    private const int MaxDescriptorDataLength = 256;
    private const int MaxPmtElementaryStreams = 50;
    private const int EsPmtDataSize = 260;
    private const int PmtDataSize = 12 + MaxDescriptorDataLength + (MaxPmtElementaryStreams * EsPmtDataSize);

    #endregion

    #region variables

    private bool _isAnysee = false;
    private bool _isCiSlotPresent = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private AnyseeCiState _ciState = AnyseeCiState.Empty;

    private IKsPropertySet _propertySet = null;
    private AnyseeCiApi _ciApi = null;

    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _callbackBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;

    private String _tunerDevicePath = String.Empty;
    private ApiCallbacks _apiCallbacks;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    #region hardware/software information

    /// <summary>
    /// Attempt to read the NIM configuration information from the tuner.
    /// </summary>
    private void ReadNimConfig()
    {
      Log.Debug("Anysee: read NIM configuration");

      for (int i = 0; i < NimConfigSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.NimConfig,
        _generalBuffer, NimConfigSize,
        _generalBuffer, NimConfigSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != NimConfigSize)
      {
        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      // Most of the info here is not very relevant.
      NimConfig info = (NimConfig)Marshal.PtrToStructure(_generalBuffer, typeof(NimConfig));
      Log.Debug("  symbol rate      = {0} s/s", info.SymbolRate);
      Log.Debug("  sweep rate       = {0} Hz/s", info.SweepRate);
      Log.Debug("  frequency        = {0} kHz", info.Frequency);
      Log.Debug("  carrier offset   = {0} kHz", info.CarrierOffset);
      Log.Debug("  bandwidth        = {0} MHz", info.Bandwidth);
      Log.Debug("  NIM type         = {0}", info.NimType);
      Log.Debug("  analog mode      = {0}", info.AnalogNimMode);
      Log.Debug("  digital mode     = {0}", info.DigitalNimMode);
      Log.Debug("  inversion        = {0}", info.SignalInversion);
      Log.Debug("  scan direction   = {0}", info.ScanDirection);
    }

    /// <summary>
    /// Attempt to read the driver information from the tuner.
    /// </summary>
    private void ReadDriverVersion()
    {
      Log.Debug("Anysee: read driver version");

      for (int i = 0; i < DriverVersionSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DriverVersion,
        _generalBuffer, DriverVersionSize,
        _generalBuffer, DriverVersionSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != DriverVersionSize)
      {
        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      DriverVersion version = (DriverVersion)Marshal.PtrToStructure(_generalBuffer, typeof(DriverVersion));
      Log.Debug("  version          = {0:x}.{1:x}.{2:x}.{3:x}", version.Version[3], version.Version[2], version.Version[1], version.Version[0]);
    }

    /// <summary>
    /// Attempt to read the platform (chipset/product) information from the tuner.
    /// </summary>
    private void ReadPlatformInfo()
    {
      Log.Debug("Anysee: read platform information");

      for (int i = 0; i < PlatformInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.PlatformInfo,
        _generalBuffer, PlatformInfoSize,
        _generalBuffer, PlatformInfoSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != PlatformInfoSize)
      {
        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      PlatformInfo info = (PlatformInfo)Marshal.PtrToStructure(_generalBuffer, typeof(PlatformInfo));
      Log.Debug("  platform         = {0}", info.Platform);
      Log.Debug("  firmware version = {0}.{1}", info.FirmwareVersion[1], info.FirmwareVersion[0]);

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
      Log.Debug("Anysee: read board information");

      for (int i = 0; i < BoardInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.BoardInfo,
        _generalBuffer, BoardInfoSize,
        _generalBuffer, BoardInfoSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != BoardInfoSize)
      {
        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      DVB_MMI.DumpBinary(_generalBuffer, 0, BoardInfoSize);
      BoardInfo info = (BoardInfo)Marshal.PtrToStructure(_generalBuffer, typeof(BoardInfo));
      Log.Debug("  bus type         = {0}", info.BusType);
      Log.Debug("  board type       = {0}", info.BoardType);
      Log.Debug("  board properties = {0}", info.BoardProperties.ToString());
      Log.Debug("  board mode       = {0}", info.BoardMode);
    }

    /// <summary>
    /// Attempt to read the tuner capabilities.
    /// </summary>
    private void ReadCapabilities()
    {
      Log.Debug("Anysee: read capabilities");

      for (int i = 0; i < CapabilitiesSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.Capabilities,
        _generalBuffer, CapabilitiesSize,
        _generalBuffer, CapabilitiesSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != CapabilitiesSize)
      {
        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Capabilities capabilities = (Capabilities)Marshal.PtrToStructure(_generalBuffer, typeof(Capabilities));
      Log.Debug("  min frequency    = {0} kHz", capabilities.MinFrequency);
      Log.Debug("  max frequency    = {0} kHz", capabilities.MaxFrequency);
      Log.Debug("  min symbol rate  = {0} s/s", capabilities.MinSymbolRate);
      Log.Debug("  max symbol rate  = {0} s/s", capabilities.MaxSymbolRate);
      Log.Debug("  min search step  = {0} Hz", capabilities.MinSearchStep);
      Log.Debug("  max search step  = {0} Hz", capabilities.MaxSearchStep);
      Log.Debug("  capabilities     = {0}", capabilities.NimCapabilities.ToString());
      Log.Debug("  broadcast system = {0}", capabilities.PrimaryBroadcastSystem);
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
      Log.Debug("Anysee: send key, key = {0}", key);
      if (_ciApi == null)
      {
        Log.Debug("Anysee: the conditional access interface is not open");
        return false;
      }
      if (_isCamReady == false)
      {
        Log.Debug("Anysee: the CAM is not ready");
        return false;
      }

      lock (this)
      {
        Marshal.WriteInt32(_generalBuffer, (Int32)key);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetKey, _generalBuffer, IntPtr.Zero))
        {
          Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

    #region callback handlers

    /// <summary>
    /// Called by the tuner driver when the common interface slot state changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot.</param>
    /// <param name="state">The new CI state.</param>
    /// <param name="message">A short description of the CI state.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    private Int32 OnCiState(Int32 slotIndex, AnyseeCiState state, String message)
    {
      // If a CAM is inserted the API seems to only invoke this callback when the CAM state
      // changes. However, if a CAM is *not* inserted then this callback is invoked every
      // time the API polls the CI state. We don't want to log the polling - it would swamp
      // the logs.
      if (state == _ciState)
      {
        return 0;
      }

      Log.Debug("Anysee: CI state change callback, slot = {0}", slotIndex);

      // Update the CI state variables.
      lock (this)
      {
        Log.Debug("  old state = {0}", _ciState);
        Log.Debug("  new state = {0}", state);
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
      }

      if (String.IsNullOrEmpty(message))
      {
        message = "(no message)";
      }
      Log.Debug("  message   = {0}", message);

      return 0;
    }

    /// <summary>
    /// Called by the tuner driver when MMI information is ready to be processed.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="message">The message from the CAM.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully processed</returns>
    private Int32 OnMmiMessage(Int32 slotIndex, IntPtr message)
    {
      Log.Debug("Anysee: MMI message callback, slot = {0}", slotIndex);

      MmiMessage msg = (MmiMessage)Marshal.PtrToStructure(message, typeof(MmiMessage));
      Log.Debug("  device index  = {0}", msg.DeviceIndex);
      Log.Debug("  slot index    = {0}", msg.SlotIndex);
      Log.Debug("  menu title    = {0}", msg.RootMenuTitle.Text);
      Log.Debug("  message type  = {0}", msg.Type);
      MmiMenu menu = (MmiMenu)Marshal.PtrToStructure(msg.Menu, typeof(MmiMenu));
      Log.Debug("  string count  = {0}", menu.StringCount);
      Log.Debug("  menu index    = {0}", menu.MenuIndex);


      // Enquiry
      if (msg.Type == AnyseeMmiMessageType.InputRequest)
      {
        Log.Debug("Anysee: enquiry");
        if (msg.HeaderCount != 1)
        {
          Log.Debug("Anysee: unexpected header count, count = {0}", msg.HeaderCount);
          return 1;
        }
        if (_ciMenuCallbacks == null)
        {
          Log.Debug("Anysee: menu callbacks are not set");
        }

        String prompt = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 0));
        Log.Debug("  prompt    = {0}", prompt);
        Log.Debug("  length    = {0}", msg.ExpectedAnswerLength);
        Log.Debug("  key count = {0}", msg.KeyCount);
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiRequest(false, (uint)msg.ExpectedAnswerLength, prompt);
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: MMI callback enquiry exception\r\n{0}", ex.ToString());
            return 1;
          }
        }
        return 0;
      }

      // Menu or list
      Log.Debug("Anysee: menu");
      if (msg.HeaderCount != 3)
      {
        Log.Debug("Anysee: unexpected header count, count = {0}", msg.HeaderCount);
        return 1;
      }
      if (_ciMenuCallbacks == null)
      {
        Log.Debug("Anysee: menu callbacks are not set");
      }

      String title = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 0));
      String subTitle = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 4));
      String footer = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 8));
      Log.Debug("  title     = {0}", title);
      Log.Debug("  sub-title = {0}", subTitle);
      Log.Debug("  footer    = {0}", footer);
      Log.Debug("  # entries = {0}", msg.EntryCount);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiMenu(title, subTitle, footer, msg.EntryCount);
        }
        catch (Exception ex)
        {
          Log.Debug("Anysee: MMI callback header exception\r\n{0}", ex.ToString());
          return 1;
        }
      }

      String entry;
      int offset = 4 * msg.HeaderCount;
      for (int i = 0; i < msg.EntryCount; i++)
      {
        entry = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, offset + (i * 4)));
        Log.Debug("  entry {0,-2}  = {1}", i + 1, entry);
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiMenuChoice(i, entry);
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: MMI callback entry exception\r\n{0}", ex.ToString());
            return 1;
          }
        }
      }
      return 0;
    }

    #endregion

    #region ICustomDevice members

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
      Log.Debug("Anysee: initialising device");

      if (tunerFilter == null)
      {
        Log.Debug("Anysee: tuner filter is null");
        return false;
      }
      if (String.IsNullOrEmpty(tunerDevicePath))
      {
        Log.Debug("Anysee: tuner device path is not set");
        return false;
      }
      if (_isAnysee)
      {
        Log.Debug("Anysee: device is already initialised");
        return true;
      }

      // We need a reference to the capture filter because that is the filter which
      // actually implements the important property sets.
      IPin captureInputPin;
      IPin tunerOutputPin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      if (tunerOutputPin == null)
      {
        Log.Debug("Anysee: failed to find the tuner filter output pin");
        return false;
      }
      int hr = tunerOutputPin.ConnectedTo(out captureInputPin);
      DsUtils.ReleaseComObject(tunerOutputPin);
      tunerOutputPin = null;
      if (hr != 0 || captureInputPin == null)
      {
        Log.Debug("Anysee: failed to get the capture filter input pin, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      PinInfo captureInfo;
      hr = captureInputPin.QueryPinInfo(out captureInfo);
      DsUtils.ReleaseComObject(captureInputPin);
      captureInputPin = null;
      if (hr != 0)
      {
        Log.Debug("Anysee: failed to get the capture filter input pin info, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      // Check if the filter supports the property set.
      _propertySet = captureInfo.filter as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Debug("Anysee: capture filter is not a property set");
        return false;
      }

      KSPropertySupport support;
      hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Ir, out support);
      if (hr != 0 || support == 0)
      {
        Log.Debug("Anysee: device does not support the Anysee property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        DsUtils.ReleaseComObject(captureInfo.filter);
        captureInfo.filter = null;
        _propertySet = null;
        return false;
      }

      Log.Debug("Anysee: supported device detected");
      _isAnysee = true;
      _tunerDevicePath = tunerDevicePath;
      _generalBuffer = Marshal.AllocCoTaskMem(NimConfigSize);

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
    public bool OpenInterface()
    {
      Log.Debug("Anysee: open conditional access interface");

      if (!_isAnysee)
      {
        Log.Debug("Anysee: device not initialised or interface not supported");
        return false;
      }
      if (_ciApi != null)
      {
        Log.Debug("Anysee: previous interface instance is still open");
        return false;
      }

      // Is a CI slot present? If not, there is no point in opening the interface.
      ReadPlatformInfo();
      if (!_isCiSlotPresent)
      {
        Log.Debug("Anysee: CI slot not present");
        return false;
      }

      _ciApi = new AnyseeCiApi();
      if (!_ciApi.OpenApi(_tunerDevicePath))
      {
        Log.Debug("Anysee: open API failed");
        _ciApi.CloseApi();
        _ciApi = null;
        return false;
      }

      _pmtBuffer = Marshal.AllocCoTaskMem(PmtDataSize);

      Log.Debug("Anysee: setting callbacks");
      // We need to pass the addresses of our callback functions to
      // the API but C# makes that awkward. The workaround is to set
      // up a callback structure instance, marshal the instance into
      // a block of memory, and then read the addresses from the memory.
      _apiCallbacks = new ApiCallbacks();
      _apiCallbacks.OnCiState = OnCiState;
      _apiCallbacks.OnMmiMessage = OnMmiMessage;
      lock (this)
      {
        _callbackBuffer = Marshal.AllocCoTaskMem(ApiCallbackSize);
        Marshal.StructureToPtr(_apiCallbacks, _callbackBuffer, true);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.IsOpenSetCallbacks, (IntPtr)Marshal.ReadInt32(_callbackBuffer, 0), (IntPtr)Marshal.ReadInt32(_callbackBuffer, 4)))
        {
          Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      Log.Debug("Anysee: close conditional access interface");

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
      if (_callbackBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_callbackBuffer);
        _callbackBuffer = IntPtr.Zero;
      }

      if (result)
      {
        Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool rebuildGraph)
    {
      rebuildGraph = false;
      return CloseInterface() && OpenInterface();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      Log.Debug("Anysee: is conditional access interface ready");
      if (!_isCiSlotPresent)
      {
        Log.Debug("Anysee: CI slot not present");
        return false;
      }

      // The CAM state is automatically updated in the OnCiState() callback.
      Log.Debug("Anysee: result = {0}", _isCamReady);
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
    /// <param name="pmt">The programme map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    //public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      Log.Debug("Anysee: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (_ciApi == null)
      {
        Log.Debug("Anysee: the conditional access interface is not open");
        return false;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        Log.Debug("Anysee: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null)
      {
        Log.Debug("Anysee: PMT not supplied");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CaPmtCommand.NotSelected)
      {
        Log.Debug("Anysee: result = success");
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
      pmtData.ProgramCaDescriptorData = new byte[MaxDescriptorDataLength];
      foreach (IDescriptor d in pmt.ProgramCaDescriptors)
      {
        ReadOnlyCollection<byte> descriptorData = d.GetRawData();
        if (offset + descriptorData.Count >= MaxDescriptorDataLength)
        {
          Log.Debug("Anysee: PMT program CA descriptor data is too long");
          return false;
        }
        descriptorData.CopyTo(pmtData.ProgramCaDescriptorData, offset);
        offset += descriptorData.Count;
      }
      pmtData.ProgramCaDescriptorData[0] = (byte)((offset - 2) & 0xff);
      pmtData.ProgramCaDescriptorData[1] = (byte)(((offset - 2) >> 8) & 0xff);

      // Elementary streams.
      Log.Debug("Anysee: elementary streams");
      pmtData.EsPmt = new EsPmtData[MaxPmtElementaryStreams];
      UInt16 esCount = 0;
      foreach (PmtElementaryStream es in pmt.ElementaryStreams)
      {
        // We want to add each video, audio, subtitle and teletext stream with their corresponding
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
          Log.Debug("  excluding PID {0} (0x{0:x}), stream type = {1})", es.Pid, es.StreamType);
          continue;
        }

        // Yes!!!
        Log.Debug("  including PID {0} (0x{0:x}), stream type = {1}, category = {2}", es.Pid, es.StreamType, esType);
        EsPmtData esToKeep = new EsPmtData();
        esToKeep.Pid = es.Pid;
        esToKeep.EsType = esType;
        esToKeep.StreamType = (byte)es.StreamType;

        // Elementary stream CA descriptor data.
        offset = 2;   // 2 bytes reserved for the data length
        esToKeep.DescriptorData = new byte[MaxDescriptorDataLength];
        foreach (IDescriptor d in es.CaDescriptors)
        {
          ReadOnlyCollection<byte> descriptorData = d.GetRawData();
          if (offset + descriptorData.Count >= MaxDescriptorDataLength)
          {
            Log.Debug("Anysee: PMT elementary stream {0} (0x{0:x}) CA data is too long", es.Pid);
            return false;
          }
          descriptorData.CopyTo(esToKeep.DescriptorData, offset);
          offset += descriptorData.Count;
        }
        esToKeep.DescriptorData[0] = (byte)((offset - 2) & 0xff);
        esToKeep.DescriptorData[1] = (byte)(((offset - 2) >> 8) & 0xff);
        pmtData.EsPmt[esCount++] = esToKeep;

        if (esCount == MaxPmtElementaryStreams)
        {
          Log.Debug("Anysee: reached maximum number of included PIDs");
          break;
        }
      }

      Log.Debug("Anysee: total included PIDs = {0}", esCount);
      pmtData.EsCount = esCount;

      lock (this)
      {
        // Pass the PMT structure to the API.
        Marshal.StructureToPtr(pmtData, _pmtBuffer, true);
        //DVB_MMI.DumpBinary(_pmtBuffer, 0, PmtDataSize);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetPmt, _pmtBuffer, IntPtr.Zero))
        {
          Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Set the CAM menu callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
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
      Log.Debug("Anysee: enter menu");
      bool result = SendKey(AnyseeCamMenuKey.Exit);
      return result && SendKey(AnyseeCamMenuKey.Menu);
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Debug("Anysee: close menu");
      return SendKey(AnyseeCamMenuKey.Exit);
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Debug("Anysee: select menu entry, choice = {0}", choice);
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
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Debug("Anysee: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      for (int i = 0; i < answer.Length; i++)
      {
        // We can't send anything other than numbers through the Anysee interface.
        int digit;
        if (!Int32.TryParse(answer[i].ToString(), out digit))
        {
          Log.Debug("Anysee: answer may only contain numeric digits");
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
      Log.Debug("Anysee: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isAnysee || _propertySet == null)
      {
        Log.Debug("Anysee: device not initialised or interface not supported");
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

      Marshal.StructureToPtr(message, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcMessageSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _generalBuffer, DiseqcMessageSize,
        _generalBuffer, DiseqcMessageSize
      );
      if (hr == 0)
      {
        Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      Log.Debug("Anysee: send DiSEqC command");

      if (!_isAnysee || _propertySet == null)
      {
        Log.Debug("Anysee: interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("Anysee: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Debug("Anysee: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.Message = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);
      message.MessageLength = command.Length;
      message.ToneBurst = AnyseeToneBurst.Off;

      int hr;
      lock (this)
      {
        Marshal.StructureToPtr(message, _generalBuffer, true);
        //DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcMessageSize);

        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
          _generalBuffer, DiseqcMessageSize,
          _generalBuffer, DiseqcMessageSize
        );
      }
      if (hr == 0)
      {
        Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      // Not supported.
      response = null;
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
      if (_propertySet != null)
      {
        DsUtils.ReleaseComObject(_propertySet);
        _propertySet = null;
      }
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
