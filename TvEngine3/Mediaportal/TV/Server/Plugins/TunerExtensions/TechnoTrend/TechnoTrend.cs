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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using BdaPolarisation = DirectShowLib.BDA.Polarisation;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;
using TvePolarisation = Mediaportal.TV.Server.Common.Types.Enum.Polarisation;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.TechnoTrend
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC and remote controls for TechnoTrend Budget
  /// and Connect series tuners.
  /// </summary>
  public class TechnoTrend : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, ICustomTuner, IDiseqcDevice, IDisposable, IPowerDevice, IRemoteControlListener
  {
    #region enums

    private enum TtDeviceType   // DVB_TYPE
    {
      DvbC = 0,
      DvbT,
      DvbS
    }

    private enum TtCiMessageHandlerTag // typ_CiMsgHandlerTag
    {
      DebugString = 0,
      Message,
      OutputMessage,
      Psi                 // program specific information
    }

    private enum TtDeviceCategory //DEVICE_CAT
    {
      Unknown = 0,
      Budget2,
      Budget3,
      Usb2,         // connect series
      Usb2Pinnacle, // Pinnacle OEM models
      Usb2Dss,
      Premium       // premium series
    }

    private enum TtCiState : byte
    {
      /// The common interface slot is empty.
      Empty = 0,
      /// A CAM is present in the common interface slot.
      CamInserted,
      /// The CAM hardware is initialised.
      CamOkay,
      /// The CAM and host software/firmware are initialised.
      ApplicationOk,
      /// A debug message from the interface is available.
      DebugMessage,
      /// The common interface slot state could not be determined.
      Unknown = 0xff
    }

    private enum TtMmiMessage : byte
    {
      None = 0,
      CiInfo,
      Menu,
      List,
      Text,
      RequestInput,
      InputComplete,
      ListMore,
      MenuMore,
      CloseMmiImmediate,
      SectionRequest,
      CloseFilter,
      PsiComplete,
      CamReady,
      SwitchProgramReply,
      TextMore
    }

    private enum TtCiError : short
    {
      None = 0,
      WrongFilterIndex,
      SetFilter,
      CloseFilter,
      InvalidData,
      NoCaResource        // Often indicates smartcard issue.
    }

    private enum TtFrontEndType // TYPE_FRONT_END
    {
      Unknown = 0,
      DvbC,
      DvbS,
      DvbS2,
      DvbT,
      Atsc,
      Dss,
      DvbCT,        // DVB-C and DVB-T
      DvbS2Premium
    }

    private enum TtApiResult  // TYPE_RET_VAL
    {
      /// <summary>
      /// operation finished successful
      /// </summary> 
      Success,
      /// <summary>
      /// operation is not implemented for the opened handle
      /// </summary> 
      NotImplemented,
      /// <summary>
      /// operation is not supported for the opened handle
      /// </summary> 
      NotSupported,
      /// <summary>
      /// the given HANDLE seems not to be correct
      /// </summary> 
      ErrorHandle,
      /// <summary>
      /// the internal IOCTL subsystem has no device handle
      /// </summary> 
      NoDeviceHandle,
      /// <summary>
      /// the internal IOCTL failed
      /// </summary> 
      Failed,
      /// <summary>
      /// the infra-red interface is already initialised
      /// </summary> 
      IrAlreadyOpen,
      /// <summary>
      /// the infra-red interface is not initialised
      /// </summary> 
      IrNotOpened,
      /// <summary>
      /// length exceeds maximum in EEPROM-userspace operation
      /// </summary> 
      TooManyBytes,
      /// <summary>
      /// common interface hardware error
      /// </summary> 
      CiHardwareError,
      /// <summary>
      /// common interface already opened
      /// </summary> 
      CiAlreadyOpen,
      /// <summary>
      /// operation timed out
      /// </summary> 
      Timeout,
      /// <summary>
      /// read PSI failed
      /// </summary> 
      ReadPsiFailed,
      /// <summary>
      /// not set
      /// </summary> 
      NotSet,
      /// <summary>
      /// operation finished with general error
      /// </summary> 
      Error,
      /// <summary>
      /// operation finished with illegal pointer
      /// </summary> 
      BadPointer,
      /// <summary>
      /// the input structure did not have the expected size
      /// </summary> 
      IncorrectSize,
      /// <summary>
      /// the tuner interface was not available
      /// </summary> 
      TunerInterfaceNotAvailable,
      /// <summary>
      /// an unknown DVB type has been specified for the tune request
      /// </summary> 
      UnknownDvbType,
      /// <summary>
      /// buffer size is too small
      /// </summary> 
      BufferTooSmall
    }

    private enum TtProductSeller  // PRODUCT_SELLER
    {
      Unknown = 0,
      TechnoTrend,
      TechniSat
    }

    private enum TtLedColour  // TYPE_LED_COLOR
    {
      Red = 0,
      Green
    }

    private enum TtConnectionType // TYPE_CONNECTION
    {
      Phone = 1,
      Cable,
      Internet,
      Serial
    }

    private enum TtFilterType // TYPE_FILTER
    {
      NotPresent = 0,
      Streaming,
      Piping,
      Pes,              // packetised elementary stream
      Es,               // elementary stream
      Section,
      MpeSection,
      Pid,              // packet ID
      MultiPid,
      Ts,               // transport stream
      MultiMpe
    }

    private enum TtDiseqcPort : uint
    {
      Null = 0xffffffff,
      ToneBurst = 0x00000000,
      DataBurst = 0x00000001,
      PortA = 0x00000000,
      PortB = 0x00000001,
      PortC = 0x00010000,
      PortD = 0x00010001
    }

    private enum TtToneBurst
    {
      Off = 0,
      ToneBurst,
      DataBurst
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CiSlotInfo   // TYP_SLOT_INFO
    {
      public TtCiState Status;
      private byte Padding1;
      private ushort Padding2;
      public IntPtr CamMenuTitle;
      public IntPtr CaSystemIds;        // array of ushort
      public ushort NumberOfCaSystemIds;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct ConnectionDescription  // TYPE_CONNECT_DESCR
    {
      public TtConnectionType ConnectionType;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string DialIn;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string ClientIpAddress;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string ServerIpAddress;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string TcpPort;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string ConnectionAuthenticationId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string LoginUserName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string LoginPassword;
      public byte RetryCount;
      public byte Timeout;      // unit = 10 ms
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct FilterNames // TS_FilterNames
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string TunerFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string TunerFilter2Name;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string CaptureFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string AnalogTunerFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string AnalogCaptureFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string StbCaptureFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_WINDOWS_PATH_LENGTH)]
      public string ProductName;
      public TtFrontEndType FrontEndType;
    }

    /// <summary>
    /// A structure for holding the full set of call back function and context pointers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtFullCiCallBack
    {
      public OnTtCiSlotStatus OnSlotStatus;
      public IntPtr OnSlotStatusContext;

      public OnTtCiCaStatus OnCaStatus;
      public IntPtr OnCaStatusContext;

      public OnTtCiDisplayString OnDisplayString;
      public IntPtr OnDisplayStringContext;

      public OnTtCiDisplayMenuOrList OnDisplayMenu;
      public IntPtr OnDisplayMenuContext;

      public OnTtCiDisplayMenuOrList OnDisplayList;
      public IntPtr OnDisplayListContext;

      public OnTtCiSwitchOsdOff OnSwitchOsdOff;
      public IntPtr OnSwitchOsdOffContext;

      public OnTtCiInputRequest OnInputRequest;
      public IntPtr OnInputRequestContext;

      public OnTtLscSetDescriptor OnLscSetDescriptor;
      public IntPtr OnLscSetDescriptorContext;

      public OnTtLscConnect OnLscConnect;
      public IntPtr OnLscConnectContext;

      public OnTtLscDisconnect OnLscDisconnect;
      public IntPtr OnLscDisconnectContext;

      public OnTtLscSetParams OnLscSetParams;
      public IntPtr OnLscSetParamsContext;

      public OnTtLscEnquireStatus OnLscEnquireStatus;
      public IntPtr OnLscEnquireStatusContext;

      public OnTtLscGetNextBuffer OnLscGetNextBuffer;
      public IntPtr OnLscGetNextBufferContext;

      public OnTtLscTransmitBuffer OnLscTransmitBuffer;
      public IntPtr OnLscTransmitBufferContext;
    }

    /// <summary>
    /// A structure for holding a minimal ("slim") set of call back function and context pointers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtSlimCiCallBack
    {
      public OnTtCiSlotStatus OnSlotStatus;
      public IntPtr OnSlotStatusContext;
      public OnTtCiCaStatus OnCaStatus;
      public IntPtr OnCAStatusContext;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtDvbcTuneRequest
    {
      public TtDeviceType DeviceType;
      public int Frequency;                   // unit = kHz

      public ModulationType Modulation;       // expected to be QAM 16, 32, 64, 128, or 256
      private FECMethod InnerFecMethod;

      private BinaryConvolutionCodeRate InnerFecRate;
      private FECMethod OuterFecMethod;

      private BinaryConvolutionCodeRate OuterFecRate;
      public int SymbolRate;                  // unit = ks/s

      public SpectralInversion SpectralInversion;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      private byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtDvbsTuneRequest
    {
      public TtDeviceType DeviceType;
      public int Frequency;                   // unit = kHz

      public int FrequencyMultiplier;
      public BdaPolarisation Polarisation;

      private uint Bandwidth;
      public TtDiseqcPort Diseqc;

      private uint Transponder;
      public ModulationType Modulation;       // expected to be QPSK for DVB-S, 8 VSB for DVB-S2

      private FECMethod InnerFecMethod;
      private BinaryConvolutionCodeRate InnerFecRate;

      private FECMethod OuterFecMethod;
      private BinaryConvolutionCodeRate OuterFecRate;

      public int SymbolRate;                  // unit = ks/s
      public SpectralInversion SpectralInversion;

      public int LnbHighBandLof;              // unit = kHz
      public int LnbLowBandLof;               // unit = kHz

      public int LnbSwitchFrequency;          // unit = kHz
      [MarshalAs(UnmanagedType.Bool)]
      public bool UseToneBurst;               // This field turns tone burst on/off; use the Diseqc field to specify the tone state.

      private uint Command;
      private uint CommandCount;

      private uint LnbSource;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      private byte[] RawDiseqc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtDvbtTuneRequest
    {
      public TtDeviceType DeviceType;
      public int Frequency;                   // unit = kHz

      public int FrequencyMultiplier;
      public int Bandwidth;                   // unit = kHz

      public ModulationType Modulation;       // expected to be QAM 16, QAM 64, QAM 256, or QPSK
      private FECMethod InnerFecMethod;

      private BinaryConvolutionCodeRate InnerFecRate;
      private FECMethod OuterFecMethod;

      private BinaryConvolutionCodeRate OuterFecRate;
      public SpectralInversion SpectralInversion;

      private GuardInterval GuardInterval;
      private TransmissionMode TransmissionMode;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
      private byte[] Padding;
    }

    #endregion

    #region DLL imports

    #region open/close device access

    /// <summary>
    /// Determine how many TechnoTrend devices in a particular category are available in the system.
    /// </summary>
    /// <param name="category">The category/type of devices to count.</param>
    /// <returns>the number of devices that are available</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern IntPtr bdaapiEnumerate(TtDeviceCategory category);

    /// <summary>
    /// Initialise access/control for a specific device.
    /// </summary>
    /// <param name="category">The category/type of device to initialise.</param>
    /// <param name="index">The device's hardware index.</param>
    /// <returns>a handle that the DLL can use to identify this device for future function calls</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern IntPtr bdaapiOpenHWIdx(TtDeviceCategory category, uint index);

    /// <summary>
    /// Close access to a specific device and relinquish control over it.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern void bdaapiClose(IntPtr handle);

    #endregion

    #region automatic offset scanning

    /// <summary>
    /// Turn automatic 125/166 kHz offset scanning on or off.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="autoOffsetModeOn"><c>True</c> to turn offset scanning on.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetDVBTAutoOffsetMode(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool autoOffsetModeOn);

    /// <summary>
    /// Determine whether automatic 125/166 kHz offset scanning is on or off.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="autoOffsetModeOn"><c>True</c> if automatic offset scanning is on, otherwise <c>false</c>.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDVBTAutoOffsetMode(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] ref bool autoOffsetModeOn);

    #endregion

    #region IR remote

    /// <summary>
    /// Initialise the infra red remote control interface.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="callBack">A delegate for the DLL to invoke when remote control key presses are detected.</param>
    /// <param name="context">Optional context that the DLL will pass as a parameter to the call back.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenIR(IntPtr handle, OnTtRemoteControlKeyPress callBack, IntPtr context);

    /// <summary>
    /// Close the infra red remote control interface.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCloseIR(IntPtr handle);

    #endregion

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="command">The command.</param>
    /// <param name="length">The length of the command.</param>
    /// <param name="repeats">The number of times to repeat the command.</param>
    /// <param name="toneBurst">An optional tone burst command that can be sent after the DiSEqC command has been sent.</param>
    /// <param name="polarisation">The polarisation to use when sending the command. This is used as a way to specify the voltage (ie. either 13 or 18 volts).</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetDiSEqCMsg(IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] byte[] command, byte length, byte repeats,
                                                        TtToneBurst toneBurst, BdaPolarisation polarisation);

    /// <summary>
    /// Switch the videoport between CI and FTA mode for devices that support a conditional access interface.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="useCiVideoPort"><c>True</c> to use the CI videoport.</param>
    /// <param name="effectiveCiVideoPort"><c>True</c> if the CI videoport is *actually* being used.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetVideoport(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool useCiVideoPort, [MarshalAs(UnmanagedType.Bool)] ref bool effectiveCiVideoPort);

    #region antenna power

    /// <summary>
    /// Turn the DVB-T antenna 5 volt power supply on or off for USB devices.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="antennaPowerOn"><c>True</c> to turn the antenna power supply on.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetDVBTAntPwr(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool antennaPowerOn);

    /// <summary>
    /// Determine whether the DVB-T antenna 5 volt power supply is on or off.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="antennaPowerOn"><c>True</c> if the antenna power supply is on, otherwise <c>false</c>.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDVBTAntPwr(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] ref bool antennaPowerOn);

    #endregion

    #region device information

    /// <summary>
    /// Get the driver version details.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="v1">Part 1 of the driver version number.</param>
    /// <param name="v2">Part 2 of the driver version number.</param>
    /// <param name="v3">Part 3 of the driver version number.</param>
    /// <param name="v4">Part 4 of the driver version number.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDrvVersion(IntPtr handle, ref byte v1, ref byte v2, ref byte v3, ref byte v4);

    /// <summary>
    /// Get the hardware MAC address.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="highPart">The high part of the address.</param>
    /// <param name="lowPart">The low part of the address.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetMAC(IntPtr handle, ref uint highPart, ref uint lowPart);

    /// <summary>
    /// Get the PCI/USB device IDs. The sub identifiers are only valid for PCI/PCIe devices.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="vendorId">The vendor identifier.</param>
    /// <param name="subVendorId">The sub-vendor identifier.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="subDeviceId">The sub-device identifier.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDeviceIDs(IntPtr handle, ref ushort vendorId, ref ushort subVendorId, ref ushort deviceId, ref ushort subDeviceId);

    /// <summary>
    /// Determine whether the device really supports USB 2 "high speed" mode.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="usbHighSpeedSupported"><c>True</c> if USB 2 speeds are supported, otherwise <c>false</c>.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetUSBHighspeedMode(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool usbHighSpeedSupported);

    /// <summary>
    /// Retrieve details about the device filters and front end.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="filterNames">The filter information.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDevNameAndFEType(IntPtr handle, out FilterNames filterNames);

    /// <summary>
    /// Get the device's hardware index.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="index">The hardware index.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetHwIdx(IntPtr handle, ref int index);

    /// <summary>
    /// Get the Windows device path.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="devicePath">A buffer containing the device path string.</param>
    /// <param name="devicePathLength">The length of the device path in bytes.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDevicePath(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder devicePath, ref int devicePathLength);

    /// <summary>
    /// Get the product seller identifier.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetProductSellerID(IntPtr handle, ref TtProductSeller sellerId);

    #endregion

    #region conditional access interface

    #region open/close the conditional access interface

    /// <summary>
    /// Initialise the conditional access interface. In this case the full set of call back delegates are specified.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="callBack">Full call back structure pointer.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCI(IntPtr handle, TtFullCiCallBack callBack);

    /// <summary>
    /// Initialise the conditional access interface. In this case the full set of call back delegates are specified.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="callBack">Full call back structure pointer.</param>
    /// <param name="ciMessageHandler">A delegate for handling raw messages from the interface.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCIext(IntPtr handle, TtFullCiCallBack callBack, OnTtCiMessage ciMessageHandler);

    /// <summary>
    /// Initialise the conditional access interface. In this case a minimal set of call back delegates are specified.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="callBack">Minimal call back structure pointer.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCISlim(IntPtr handle, TtSlimCiCallBack callBack);

    /// <summary>
    /// Initialise the conditional access interface. In this case call back delegates are not specified.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCIWithoutPointer(IntPtr handle);

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern void bdaapiCloseCI(IntPtr handle);

    #endregion

    /// <summary>
    /// Send a message to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="pmt">The PMT for the service that should be descrambled.</param>
    /// <param name="pmtLength">The length of the PMT in bytes.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIReadPSIFastWithPMT(IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] byte[] pmt, ushort pmtLength);

    /// <summary>
    /// Send a message to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="serviceId">The service ID of the service that should be descrambled.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIReadPSIFastDrvDemux(IntPtr handle, ushort serviceId);

    /// <summary>
    /// Send a message to the CAM to request that one or more services be descrambled.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="sidList">A list of service IDs, one for each service that should be descrambled.</param>
    /// <param name="serviceCount">The number of services for the CAM to descramble.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIMultiDecode(IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] ushort[] sidList, int serviceCount);

    /// <summary>
    /// Enter the CAM menu.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIEnterModuleMenu(IntPtr handle, byte slotIndex);

    /// <summary>
    /// Request a CI status update. This request will be answered via the OnCiStatus() call back.
    /// </summary>
    /// <remarks>
    /// Using this function is not recommended. It seems to mess up the driver state.
    /// </remarks>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIGetSlotStatus(IntPtr handle, byte slotIndex);

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="menuAnswer">The user's response.</param>
    /// <param name="answerLength">The length of the user's response in bytes.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIAnswer(IntPtr handle, byte slotIndex,
                                                    [MarshalAs(UnmanagedType.LPStr)] string menuAnswer, byte answerLength);

    /// <summary>
    /// Select an entry in the CAM menu.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="choice">The index (0..n) of the menu choice selected by the user.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIMenuAnswer(IntPtr handle, byte slotIndex, byte choice);

    #endregion

    /// <summary>
    /// Tune to a transponder/multiplex.
    /// </summary>
    /// <param name="handle">The handle allocated to a device when it was opened.</param>
    /// <param name="tuneRequest">A buffer containing the tune request.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiTune(IntPtr handle, IntPtr tuneRequest);

    #endregion

    #region delegate definitions

    /// <summary>
    /// Invoked by the tuner driver when a remote control key press is detected.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="code">The key code. If the code is an RC-5 code then it can be found in the lower 2 bytes. RC-6 codes use 4 bytes.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtRemoteControlKeyPress(IntPtr context, ref int code);

    /// <summary>
    /// Invoked by the tuner driver when the state of the CI slot changes.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <param name="slotInfo">A pointer to a CiSlotInfo struct containing extended information about the interface state.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiSlotStatus(IntPtr context, byte slotIndex, TtCiState state, IntPtr slotInfo);

    /// <summary>
    /// Invoked by the tuner driver when the result of an interaction with the CAM is known.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="reply">A reply message from the CAM.</param>
    /// <param name="error">An error message from the CAM.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiCaStatus(IntPtr context, byte slotIndex, TtMmiMessage reply, TtCiError error);

    /// <summary>
    /// Invoked by the tuner driver when the CAM requests input from the user. This delegate is called
    /// immediately before OnTtInputRequest().
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="prompt">A buffer containing the request prompt text from the CAM.</param>
    /// <param name="promptByteCount">The number of bytes in the prompt buffer.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiDisplayString(IntPtr context, byte slotIndex, IntPtr prompt, short promptByteCount);

    /// <summary>
    /// Invoked by the tuner driver when a menu or list from the CAM is available.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="stringCount">The number of strings in <paramref name="stringBuffer"/>.</param>
    /// <param name="stringBuffer">A buffer containing menu strings (title, sub-title, entries and footer). Each string is NULL terminated.</param>
    /// <param name="totalMenuLength">The length of the menu (ie. the size of <paramref name="stringBuffer"/>) in bytes.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiDisplayMenuOrList(IntPtr context, byte slotIndex, short stringCount, IntPtr stringBuffer, short totalMenuLength);

    /// <summary>
    /// Invoked by the tuner driver when the CAM wants to close the menu.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiSwitchOsdOff(IntPtr context, byte slotIndex);

    /// <summary>
    /// Invoked by the tuner driver when the CAM requests input from the user.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="blind"><c>True</c> if the input should be hidden (eg. password).</param>
    /// <param name="answerLength">The expected answer length.</param>
    /// <param name="keyMask"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiInputRequest(IntPtr context, byte slotIndex, [MarshalAs(UnmanagedType.Bool)] bool blind, byte answerLength, short keyMask);

    /// <summary>
    /// Invoked by the tuner driver when a message is received from the CAM. This delegate receives the raw
    /// message/response data from the CAM.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="tag">The type of message received.</param>
    /// <param name="buffer">A buffer containing the CAM message/response.</param>
    /// <param name="bufferSize">The size of the buffer.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiMessage(byte slotIndex, TtCiMessageHandlerTag tag, IntPtr buffer, short bufferSize);

    #region low speed communication interface call back definitions

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="descriptor">???</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscSetDescriptor(IntPtr context, byte slotIndex, IntPtr descriptor);

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscConnect(IntPtr context, byte slotIndex);

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscDisconnect(IntPtr context, byte slotIndex);

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="bufferSize"></param>
    /// <param name="timeOut">A time-out. The unit is ten milli-seconds.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscSetParams(IntPtr context, byte slotIndex, byte bufferSize, byte timeOut);

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscEnquireStatus(IntPtr context, byte slotIndex);

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="phaseId"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscGetNextBuffer(IntPtr context, byte slotIndex, byte phaseId);

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="phaseId"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferSize"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscTransmitBuffer(IntPtr context, byte slotIndex, byte phaseId, IntPtr buffer, short bufferSize);

    #endregion

    #endregion

    #region constants

    private const int MAX_WINDOWS_PATH_LENGTH = 260;
    private const int MAX_PROGRAM_COUNT = 100;          // This is arbitrary.
    private const int MAX_DISEQC_COMMAND_LENGTH = 64;   // This is arbitrary - the hardware/interface limit is not known.
    private static readonly int FILTER_NAMES_SIZE = Marshal.SizeOf(typeof(FilterNames));        // 1824
    private static readonly int TUNE_REQUEST_SIZE = Marshal.SizeOf(typeof(TtDvbsTuneRequest));  // 100 (the size is the same for all tune request structs)

    private const int SERVICE_BUFFER_SIZE = MAX_PROGRAM_COUNT * sizeof(ushort);
    private static readonly int GENERAL_BUFFER_SIZE = Math.Max(MAX_DISEQC_COMMAND_LENGTH, TUNE_REQUEST_SIZE);

    private static readonly HashSet<string> VALID_BUDGET2_DEVICE_NAMES = new HashSet<string>
    {
      "TechnoTrend BDA/DVB-C Tuner",
      "TechnoTrend BDA/DVB-S Tuner",
      "TechnoTrend BDA/DVB-T Tuner",
      "ttBudget2 BDA DVB-C Tuner",
      "ttBudget2 BDA DVB-S Tuner",
      "ttBudget2 BDA DVB-T Tuner"
    };

    private static readonly HashSet<string> VALID_BUDGET3_DEVICE_NAMES = new HashSet<string>
    {
      "TTHybridTV BDA DVBT Tuner",
      "TTHybridTV BDA ATSC Tuner"
    };

    private static readonly HashSet<string> VALID_USB2_DEVICE_NAMES = new HashSet<string>
    {
      "USB 2.0 BDA DVB-C Tuner",
      "USB 2.0 BDA DVB-S Tuner",
      "USB 2.0 BDA (DVB-T Fake) DVB-T Tuner",
      "USB 2.0 BDA DVB-T Tuner"
    };

    private const string VALID_PINNACLE_DEVICE_NAME = "Pinnacle PCTV 4XXe Tuner";
    private const string VALID_DSS_DEVICE_NAME = "USB 2.0 BDA DSS Tuner";

    #endregion

    #region variables

    private bool _isTechnoTrend = false;
    private bool _isRemoteControlInterfaceOpen = false;
    private bool _isCaInterfaceOpen = false;
    #pragma warning disable 0414
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private byte _slotIndex = 0;
    private TtCiState _ciState = TtCiState.Unknown;

    private bool _isTerrestrialTuner = false;
    private TtDeviceCategory _deviceCategory = TtDeviceCategory.Unknown;
    private string _name = "TechnoTrend";

    private IntPtr _tunerHandle = IntPtr.Zero;
    private IntPtr _generalBuffer = IntPtr.Zero;

    private TtFullCiCallBack _ciCallBack;
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();

    // When the CAM asks for input (via call back), the input request prompt is provided
    // by a separate call back. We use this variable to cache it.
    private string _camInputRequestPrompt = string.Empty;

    private HashSet<ushort> _descrambledPrograms = null;

    private OnTtRemoteControlKeyPress _remoteControlKeyPressDelegate = null;

    #endregion

    /// <summary>
    /// Determine the TechnoTrend device category.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <returns>the device category</returns>
    private TtDeviceCategory GetDeviceCategory(IBaseFilter tunerFilter)
    {
      // Get the tuner filter name.
      FilterInfo filterInfo;
      int hr = tunerFilter.QueryFilterInfo(out filterInfo);
      string filterName = filterInfo.achName;
      Release.FilterInfo(ref filterInfo);
      if (hr != (int)NativeMethods.HResult.S_OK || string.IsNullOrEmpty(filterName))
      {
        return TtDeviceCategory.Unknown;
      }

      if (VALID_BUDGET2_DEVICE_NAMES.Contains(filterName))
      {
        return TtDeviceCategory.Budget2;
      }
      if (VALID_BUDGET3_DEVICE_NAMES.Contains(filterName))
      {
        return TtDeviceCategory.Budget3;
      }
      if (VALID_USB2_DEVICE_NAMES.Contains(filterName))
      {
        return TtDeviceCategory.Usb2;
      }
      if (VALID_PINNACLE_DEVICE_NAME.Equals(filterName))
      {
        return TtDeviceCategory.Usb2Pinnacle;
      }
      if (VALID_DSS_DEVICE_NAME.Equals(filterName))
      {
        return TtDeviceCategory.Usb2Dss;
      }
      return TtDeviceCategory.Unknown;
    }

    /// <summary>
    /// Determine the TechnoTrend device identifier.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <returns><c>-1</c> if it is not possible to determine the device identifier, otherwise the device identifier</returns>
    private int GetDeviceId(IBaseFilter tunerFilter)
    {
      int deviceId = -1;

      IKsPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0) as IKsPin;
      if (pin == null)
      {
        return deviceId;
      }

      try
      {
        // Request the KS medium data. The TechnoTrend device identifier is the
        // ID in the first medium.
        IntPtr ksMultiplePtr;
        int hr = pin.KsQueryMediums(out ksMultiplePtr);
        if (hr != (int)NativeMethods.HResult.S_OK || ksMultiplePtr == IntPtr.Zero)
        {
          return deviceId;
        }
        try
        {
          KSMultipleItem ksMultipleItem = (KSMultipleItem)Marshal.PtrToStructure(ksMultiplePtr, typeof(KSMultipleItem));
          if (ksMultipleItem.Count == 0)
          {
            return deviceId;
          }

          // The first medium starts at the end of the KS multiple item.
          IntPtr mediumPtr = IntPtr.Add(ksMultiplePtr, Marshal.SizeOf(typeof(KSMultipleItem)));
          RegPinMedium medium = (RegPinMedium)Marshal.PtrToStructure(mediumPtr, typeof(RegPinMedium));
          return medium.dw1;
        }
        finally
        {
          Marshal.FreeCoTaskMem(ksMultiplePtr);
        }
      }
      finally
      {
        Release.ComObject("TechnoTrend tuner filter input pin", ref pin);
      }
    }

    /// <summary>
    /// Attempt to read the device information from the device.
    /// </summary>
    private void ReadDeviceInfo()
    {
      this.LogDebug("TechnoTrend: read device information");

      // General product details.
      FilterNames names = new FilterNames();
      TtApiResult result = bdaapiGetDevNameAndFEType(_tunerHandle, out names);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("  product name        = {0}", names.ProductName);
        this.LogDebug("  tuner type          = {0}", names.FrontEndType);
        this.LogDebug("  tuner filter name   = {0}", names.TunerFilterName);
        this.LogDebug("  capture filter name = {0}", names.CaptureFilterName);
        // These other filter names are not relevant for digital tuners (they will be blank).
        /*this.LogDebug("TechnoTrend: {0}", names.AnalogTunerFilterName);
        this.LogDebug("TechnoTrend: {0}", names.AnalogCaptureFilterName);
        this.LogDebug("TechnoTrend: {0}", names.StbCaptureFilterName);*/
        _name = names.ProductName;
      }
      else
      {
        this.LogWarn("TechnoTrend: failed to read the device details, result = {0}", result);
      }

      // Product seller.
      TtProductSeller seller = TtProductSeller.Unknown;
      result = bdaapiGetProductSellerID(_tunerHandle, ref seller);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("  product (re)seller  = {0}", seller);
      }
      else
      {
        this.LogWarn("TechnoTrend: failed to read the product (re)seller, result = {0}", result);
      }

      // Driver version.
      byte v1 = 0;
      byte v2 = 0;
      byte v3 = 0;
      byte v4 = 0;
      result = bdaapiGetDrvVersion(_tunerHandle, ref v1, ref v2, ref v3, ref v4);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("  driver version      = {0}.{1}.{2}.{3}", v1, v2, v3, v4);

        // Check for the WMC driver (not compatible).
        if ((_deviceCategory == TtDeviceCategory.Usb2 && v1 == 1 && v2 == 0 && v3 == 4 && v4 == 6) ||
          (_deviceCategory == TtDeviceCategory.Budget2 && v1 == 5 && v2 == 0 && v3 == 3 && v4 == 6))
        {
          this.LogError("TechnoTrend: detected incompatible Windows Media Center driver, you must install the standard BDA driver");
        }
      }
      else
      {
        this.LogWarn("TechnoTrend: failed to read the driver version, result = {0}", result);
      }

      // MAC address.
      uint lowPart = 0;
      uint highPart = 0;
      result = bdaapiGetMAC(_tunerHandle, ref highPart, ref lowPart);
      if (result == TtApiResult.Success)
      {
        Array lowBytes = BitConverter.GetBytes(lowPart);
        Array highBytes = BitConverter.GetBytes(highPart);
        this.LogDebug("  MAC address         = {0:x2}-{1:x2}-{2:x2}-{3:x2}-{4:x2}-{5:x2}",
          highBytes.GetValue(2), highBytes.GetValue(1), highBytes.GetValue(0),
          lowBytes.GetValue(2), lowBytes.GetValue(1), lowBytes.GetValue(0)
        );
      }
      else
      {
        this.LogWarn("TechnoTrend: failed to read the MAC address, result = {0}", result);
      }

      // USB speed.
      if (_deviceCategory == TtDeviceCategory.Usb2 || _deviceCategory == TtDeviceCategory.Usb2Pinnacle || _deviceCategory == TtDeviceCategory.Usb2Dss)
      {
        bool highSpeed = false;
        result = bdaapiGetUSBHighspeedMode(_tunerHandle, out highSpeed);
        if (result == TtApiResult.Success)
        {
          this.LogDebug("  USB 2 speed support = {0}", highSpeed);
        }
        else
        {
          this.LogWarn("TechnoTrend: failed to determine whether USB high speed is supported, result = {0}", result);
        }
      }
    }

    #region delegate implementations

    /// <summary>
    /// Invoked by the tuner driver when a remote control key press is detected.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="code">The key code. If the code is an RC-5 code then it can be found in the lower 2 bytes. RC-6 codes use 4 bytes.</param>
    private void OnRemoteControlKeyPress(IntPtr context, ref int code)
    {
      if ((code & 0xffffc000) == 0)
      {
        int fieldBit = (code & 0x1000) >> 12;
        int toggleBit = (code & 0x800) >> 11;
        int systemAddress = (code & 0x7c0) >> 6;
        int command = code & 0x3f;
        this.LogDebug("TechnoTrend: RC-5 remote control key press, field bit = {0}, toggle bit = {1} system address = {2}, command = {3}", fieldBit, toggleBit, systemAddress, command);
      }
      else
      {
        int header = code >> 16;
        int control = ((code >> 8) & 0xff);
        int information = code & 0xff;
        this.LogDebug("TechnoTrend: RC-6 remote control key press, header = {0}, control = {1}, information = {2}", header, control, information);
      }
    }

    /// <summary>
    /// Invoked by the tuner driver when the state of the CI slot changes.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <param name="slotInfo">A pointer to a CiSlotInfo struct containing extended information about the interface state.</param>
    private void OnCiSlotStatus(IntPtr context, byte slotIndex, TtCiState state, IntPtr slotInfo)
    {
      this.LogInfo("TechnoTrend: CI slot status call back, slot = {0}", slotIndex);
      if (state == _ciState)
      {
        // Don't be too verbose - we don't need to print the CAS IDs all the time.
        this.LogInfo("TechnoTrend: CI state = {0}", _ciState);
        return;
      }

      this.LogInfo("TechnoTrend: CI state change, old state = {0}, new state = {1}", _ciState, state);
      if (state == TtCiState.CamOkay || state == TtCiState.ApplicationOk)
      {
        _isCamPresent = true;
        _isCamReady = true;
      }
      else if (state == TtCiState.CamInserted)
      {
        _isCamPresent = true;
        _isCamReady = false;
      }
      else
      {
        _isCamPresent = false;
        _isCamReady = false;
      }
      _ciState = state;

      if (slotInfo == IntPtr.Zero)
      {
        if (state != TtCiState.Empty)
        {
          this.LogDebug("TechnoTrend: detailed slot info is not available [yet]");
        }
        return;
      }

      try
      {
        CiSlotInfo info = (CiSlotInfo)Marshal.PtrToStructure(slotInfo, typeof(CiSlotInfo));
        this.LogDebug("TechnoTrend: slot info");
        this.LogDebug("  status     = {0} ", info.Status);
        if (info.CamMenuTitle.Equals(string.Empty))
        {
          this.LogDebug("  menu title = (not available)");
        }
        else
        {
          this.LogDebug("  menu title = {0} ", DvbTextConverter.Convert(info.CamMenuTitle));
        }
        this.LogDebug("  # CAS IDs  = {0}", info.NumberOfCaSystemIds);
        for (int i = 0; i < info.NumberOfCaSystemIds; i++)
        {
          this.LogDebug("    {0, -8} = 0x{1:x4}", i + 1, Marshal.ReadInt16(info.CaSystemIds, i * 2));
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TechnoTrend: CI slot status call back exception");
      }
    }

    /// <summary>
    /// Invoked by the tuner driver when the result of an interaction with the CAM is known.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="reply">A reply message from the CAM.</param>
    /// <param name="error">An error message from the CAM.</param>
    private void OnCiCaStatus(IntPtr context, byte slotIndex, TtMmiMessage reply, TtCiError error)
    {
      this.LogInfo("TechnoTrend: CI CA status call back, slot = {0}, reply = {1}, error = {2}", slotIndex, reply, error);
      try
      {
        // NoCaResource generally seems to indicate a smartcard or CAM error. The TechnoTrend
        // API doesn't seem to support removal and reinsertion of the smartcard on the fly
        // (no CAM state change reported). Closing and re-opening the conditional access
        // seems to resolve the problem.
        if (error == TtCiError.NoCaResource)
        {
          (this as IConditionalAccessProvider).Reset();
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TechnoTrend: CA status call back exception");
      }
    }

    /// <summary>
    /// Invoked by the tuner driver when the CAM requests input from the user. This delegate is called
    /// immediately before OnTtInputRequest().
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="prompt">A buffer containing the request prompt text from the CAM.</param>
    /// <param name="promptByteCount">The number of bytes in the prompt buffer.</param>
    private void OnCiDisplayString(IntPtr context, byte slotIndex, IntPtr prompt, short promptByteCount)
    {
      try
      {
        _camInputRequestPrompt = DvbTextConverter.Convert(prompt, promptByteCount);
        this.LogDebug("TechnoTrend: CI display string call back, slot = {0}, prompt = {1}", slotIndex, _camInputRequestPrompt);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TechnoTrend: display string call back exception");
      }
    }

    /// <summary>
    /// Invoked by the tuner driver when a menu or list from the CAM is available.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="stringCount">The number of strings in <paramref name="stringBuffer"/>.</param>
    /// <param name="stringBuffer">A buffer containing menu strings (title, sub-title, entries and footer). Each string is NULL terminated.</param>
    /// <param name="totalMenuLength">The length of the menu (ie. the size of <paramref name="stringBuffer"/>) in bytes.</param>
    private void OnCiDisplayMenuOrList(IntPtr context, byte slotIndex, short stringCount, IntPtr stringBuffer, short totalMenuLength)
    {
      this.LogInfo("TechnoTrend: CI display menu/list call back, slot = {0}, total menu length = {1}", slotIndex, totalMenuLength);

      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack == null)
        {
          this.LogDebug("TechnoTrend: menu call back not set");
        }

        // Decode menu/list strings for call back.
        List<string> strings = new List<string>(15);
        IntPtr stringPtr = stringBuffer;
        string s;
        int decodedByteCount;
        int totalDecodedByteCount = 0;
        for (int i = 0; i < stringCount && totalDecodedByteCount < totalMenuLength; i++)
        {
          s = DvbTextConverter.Convert(stringPtr, -1, 0, out decodedByteCount);
          if (decodedByteCount == 0)
          {
            this.LogWarn("TechnoTrend: failed to decode menu/list string {0} of {1}", i + 1, stringCount);
            Dump.DumpBinary(stringBuffer, totalMenuLength);
            break;
          }
          totalDecodedByteCount += decodedByteCount;
          strings.Add(s);
          stringPtr = IntPtr.Add(stringPtr, decodedByteCount);
        }

        if (strings.Count < 3 || stringCount != strings.Count)
        {
          this.LogError("TechnoTrend: actual menu string count {0} does not match expected string count {1}", strings.Count, stringCount);
          Dump.DumpBinary(stringBuffer, totalMenuLength);
          return;
        }
        this.LogDebug("  title     = {0}", strings[0]);
        this.LogDebug("  sub-title = {0}", strings[1]);
        this.LogDebug("  footer    = {0}", strings[2]);
        this.LogDebug("  # entries = {0}", stringCount - 3);
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiMenu(strings[0], strings[1], strings[2], stringCount - 3);
        }

        for (int i = 3; i < stringCount; i++)
        {
          s = strings[i];
          this.LogDebug("    {0, -7} = {1}", i - 2, s);
          if (_caMenuCallBack != null)
          {
            _caMenuCallBack.OnCiMenuChoice(i - 3, s);
          }
        }
      }
    }

    /// <summary>
    /// Invoked by the tuner driver when the CAM wants to close the menu.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnCiSwitchOsdOff(IntPtr context, byte slotIndex)
    {
      this.LogInfo("TechnoTrend: CI switch OSD off call back, slot = {0}", slotIndex);
      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiCloseDisplay(0);
        }
        else
        {
          this.LogDebug("TechnoTrend: menu call back not set");
        }
      }
    }

    /// <summary>
    /// Invoked by the tuner driver when the CAM requests input from the user.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="blind"><c>True</c> if the input should be hidden (eg. password).</param>
    /// <param name="answerLength">The expected answer length.</param>
    /// <param name="keyMask"></param>
    private void OnCiInputRequest(IntPtr context, byte slotIndex, bool blind, byte answerLength, short keyMask)
    {
      this.LogInfo("TechnoTrend: CI input request call back, slot = {0}", slotIndex);
      this.LogDebug("  length   = {0}", answerLength);
      this.LogDebug("  blind    = {0}", blind);
      this.LogDebug("  key mask = 0x{0:x4}", keyMask);
      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiRequest(blind, answerLength, _camInputRequestPrompt);
        }
        else
        {
          this.LogDebug("TechnoTrend: menu call back not set");
        }
      }
    }

    #region low speed communication call-backs

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="descriptor">???</param>
    private void OnLscSetDescriptor(IntPtr context, byte slotIndex, IntPtr descriptor)
    {
      this.LogDebug("TechnoTrend: low speed communication set descriptor call back, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnLscConnect(IntPtr context, byte slotIndex)
    {
      this.LogDebug("TechnoTrend: low speed communication connect call back, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnLscDisconnect(IntPtr context, byte slotIndex)
    {
      this.LogDebug("TechnoTrend: low speed communication disconnect call back, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="bufferSize"></param>
    /// <param name="timeOut">A time-out. The unit is ten milli-seconds.</param>
    private void OnLscSetParams(IntPtr context, byte slotIndex, byte bufferSize, byte timeOut)
    {
      this.LogDebug("TechnoTrend: low speed communication set parameters call back, slot = {0}, buffer size = {1}, time-out = {2}", slotIndex, bufferSize, timeOut);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnLscEnquireStatus(IntPtr context, byte slotIndex)
    {
      this.LogDebug("TechnoTrend: low speed communication enquire status call back, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="phaseId"></param>
    private void OnLscGetNextBuffer(IntPtr context, byte slotIndex, byte phaseId)
    {
      this.LogDebug("TechnoTrend: low speed communication get next buffer call back, slot = {0}, phase = {1}", slotIndex, phaseId);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="phaseId"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferSize"></param>
    private void OnLscTransmitBuffer(IntPtr context, byte slotIndex, byte phaseId, IntPtr buffer, short bufferSize)
    {
      this.LogDebug("TechnoTrend: low speed communication transmit buffer call back, slot = {0}, phase = {1}", slotIndex, phaseId);
      Dump.DumpBinary(buffer, bufferSize);
    }

    #endregion

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return _name;
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
      this.LogDebug("TechnoTrend: initialising");

      if (_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: extension already initialised");
        return true;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("TechnoTrend: context is not a filter");
        return false;
      }

      _deviceCategory = GetDeviceCategory(filter);
      if (_deviceCategory == TtDeviceCategory.Unknown)
      {
        this.LogDebug("TechnoTrend: device category is unknown");
        return false;
      }

      int deviceId = GetDeviceId(filter);
      if (deviceId == -1)
      {
        this.LogDebug("TechnoTrend: failed to determine device ID");
        return false;
      }

      _tunerHandle = bdaapiOpenHWIdx(_deviceCategory, (uint)deviceId);
      if (_tunerHandle == IntPtr.Zero || _tunerHandle == NativeMethods.INVALID_HANDLE_VALUE)
      {
        this.LogError("TechnoTrend: hardware interface could not be opened");
        _tunerHandle = IntPtr.Zero;
        return false;
      }

      this.LogInfo("TechnoTrend: extension supported, category = {0}, id = {1}", _deviceCategory, deviceId);
      _isTechnoTrend = true;
      _isTerrestrialTuner = (tunerSupportedBroadcastStandards & BroadcastStandard.MaskDigital & BroadcastStandard.MaskTerrestrial) != 0;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
      ReadDeviceInfo();
      if (_isTerrestrialTuner)
      {
        TtApiResult result = bdaapiSetDVBTAutoOffsetMode(_tunerHandle, false);
        if (result != TtApiResult.Success)
        {
          this.LogWarn("TechnoTrend: failed to turn off auto offset mode, result = {0}", result);
        }
      }
      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("TechnoTrend: on before tune call back");
      action = TunerAction.Default;

      if (!_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return;
      }

      // We only need to tweak the modulation for DVB-S/S2 channels.
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      ModulationType bdaModulation = ModulationType.ModNotSet;
      if (channel is ChannelDvbS2)
      {
        bdaModulation = ModulationType.Mod8Vsb;
      }
      else if (channel is ChannelDvbS)
      {
        bdaModulation = ModulationType.ModQpsk;
      }
      else
      {
        this.LogWarn("TechnoTrend: tune request for unsupported satellite standard");
      }

      if (bdaModulation != ModulationType.ModNotSet)
      {
        this.LogDebug("  modulation = {0}", bdaModulation);
        satelliteChannel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
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
      this.LogDebug("TechnoTrend: set power state, state = {0}", state);

      if (!_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (!_isTerrestrialTuner)
      {
        this.LogDebug("TechnoTrend: power control is not supported for this device");
        return false;
      }

      TtApiResult result = bdaapiSetDVBTAntPwr(_tunerHandle, state == PowerState.On);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to set power state, result = {0}", result);
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
      // Tuning of OFDM, QAM and satellite channels is supported with an appropriate tuner.
      if (
        channel is IChannelOfdm ||
        channel is IChannelQam ||
        channel is IChannelSatellite
      )
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("TechnoTrend: tune to channel");

      if (!_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }

      IChannelOfdm ofdmChannel = channel as IChannelOfdm;
      if (ofdmChannel != null)
      {
        TtDvbtTuneRequest tuneRequest = new TtDvbtTuneRequest();
        tuneRequest.DeviceType = TtDeviceType.DvbT;
        tuneRequest.Frequency = ofdmChannel.Frequency;
        // Frequency is already specified in kHz (the base unit) so the
        // multiplier is set to 1.
        tuneRequest.FrequencyMultiplier = 1;
        tuneRequest.Bandwidth = ofdmChannel.Bandwidth;
        tuneRequest.Modulation = ModulationType.ModNotSet;
        tuneRequest.SpectralInversion = SpectralInversion.Automatic;

        Marshal.StructureToPtr(tuneRequest, _generalBuffer, false);
      }
      else
      {
        IChannelQam qamChannel = channel as IChannelQam;
        if (qamChannel != null)
        {
          TtDvbcTuneRequest tuneRequest = new TtDvbcTuneRequest();
          tuneRequest.DeviceType = TtDeviceType.DvbC;
          tuneRequest.Frequency = qamChannel.Frequency;

          switch (qamChannel.ModulationScheme)
          {
            case ModulationSchemeQam.Qam16:
              tuneRequest.Modulation = ModulationType.Mod16Qam;
              break;
            case ModulationSchemeQam.Qam32:
              tuneRequest.Modulation = ModulationType.Mod32Qam;
              break;
            case ModulationSchemeQam.Qam64:
              tuneRequest.Modulation = ModulationType.Mod64Qam;
              break;
            case ModulationSchemeQam.Qam128:
              tuneRequest.Modulation = ModulationType.Mod128Qam;
              break;
            case ModulationSchemeQam.Qam256:
              tuneRequest.Modulation = ModulationType.Mod256Qam;
              break;
            case ModulationSchemeQam.Qam512:
              tuneRequest.Modulation = ModulationType.Mod512Qam;
              break;
            case ModulationSchemeQam.Qam1024:
              tuneRequest.Modulation = ModulationType.Mod1024Qam;
              break;
            default:
              this.LogWarn("TechnoTrend: QAM tune request uses unsupported modulation {0}, falling back to automatic", qamChannel.ModulationScheme);
              tuneRequest.Modulation = ModulationType.ModNotSet;
              break;
          }

          tuneRequest.SymbolRate = qamChannel.SymbolRate;
          tuneRequest.SpectralInversion = SpectralInversion.Automatic;

          Marshal.StructureToPtr(tuneRequest, _generalBuffer, false);
        }
        else
        {
          IChannelSatellite satelliteChannel = channel as IChannelSatellite;
          if (satelliteChannel == null)
          {
            this.LogError("TechnoTrend: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
            return false;
          }

          TtDvbsTuneRequest tuneRequest = new TtDvbsTuneRequest();
          tuneRequest.DeviceType = TtDeviceType.DvbS;
          // Frequency is already specified in kHz (the base unit) so the
          // multiplier is set to 1.
          tuneRequest.Frequency = satelliteChannel.Frequency;
          tuneRequest.FrequencyMultiplier = 1;
          tuneRequest.LnbLowBandLof = SatelliteLnbHandler.LOW_BAND_LOF;
          tuneRequest.LnbHighBandLof = SatelliteLnbHandler.HIGH_BAND_LOF;
          tuneRequest.LnbSwitchFrequency = SatelliteLnbHandler.SWITCH_FREQUENCY;

          switch (satelliteChannel.Polarisation)
          {
            case TvePolarisation.CircularLeft:
              tuneRequest.Polarisation = BdaPolarisation.CircularL;
              break;
            case TvePolarisation.CircularRight:
              tuneRequest.Polarisation = BdaPolarisation.CircularL;
              break;
            case TvePolarisation.LinearHorizontal:
              tuneRequest.Polarisation = BdaPolarisation.LinearH;
              break;
            case TvePolarisation.LinearVertical:
              tuneRequest.Polarisation = BdaPolarisation.LinearV;
              break;
            default:
              this.LogWarn("TechnoTrend: satellite tune request uses unsupported polarisation {0}, falling back to automatic", satelliteChannel.Polarisation);
              tuneRequest.Polarisation = BdaPolarisation.NotSet;
              break;
          }

          tuneRequest.Diseqc = TtDiseqcPort.Null;
          tuneRequest.UseToneBurst = false;
          tuneRequest.Modulation = (ModulationType)satelliteChannel.ModulationScheme;
          tuneRequest.SymbolRate = satelliteChannel.SymbolRate;
          tuneRequest.SpectralInversion = SpectralInversion.Automatic;

          Marshal.StructureToPtr(tuneRequest, _generalBuffer, false);
        }
      }

      //Dump.DumpBinary(_generalBuffer, TUNE_REQUEST_SIZE);
      TtApiResult result = bdaapiTune(_tunerHandle, _generalBuffer);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to tune, result = {0}{1}{2}", result, Environment.NewLine, channel);
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
      this.LogDebug("TechnoTrend: open conditional access interface");

      if (!_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("TechnoTrend: conditional access interface is already open");
        return true;
      }

      // Call back contexts.
      _ciCallBack.OnSlotStatusContext = _tunerHandle;
      _ciCallBack.OnCaStatusContext = _tunerHandle;
      _ciCallBack.OnDisplayStringContext = _tunerHandle;
      _ciCallBack.OnDisplayMenuContext = _tunerHandle;
      _ciCallBack.OnDisplayListContext = _tunerHandle;
      _ciCallBack.OnSwitchOsdOffContext = _tunerHandle;
      _ciCallBack.OnInputRequestContext = _tunerHandle;
      _ciCallBack.OnLscSetDescriptorContext = _tunerHandle;
      _ciCallBack.OnLscConnectContext = _tunerHandle;
      _ciCallBack.OnLscDisconnectContext = _tunerHandle;
      _ciCallBack.OnLscSetParamsContext = _tunerHandle;
      _ciCallBack.OnLscEnquireStatusContext = _tunerHandle;
      _ciCallBack.OnLscGetNextBufferContext = _tunerHandle;
      _ciCallBack.OnLscTransmitBufferContext = _tunerHandle;

      // Call back functions.
      _ciCallBack.OnSlotStatus = OnCiSlotStatus;
      _ciCallBack.OnCaStatus = OnCiCaStatus;
      _ciCallBack.OnDisplayString = OnCiDisplayString;
      _ciCallBack.OnDisplayMenu = OnCiDisplayMenuOrList;
      _ciCallBack.OnDisplayList = OnCiDisplayMenuOrList;
      _ciCallBack.OnSwitchOsdOff = OnCiSwitchOsdOff;
      _ciCallBack.OnInputRequest = OnCiInputRequest;
      _ciCallBack.OnLscSetDescriptor = OnLscSetDescriptor;
      _ciCallBack.OnLscConnect = OnLscConnect;
      _ciCallBack.OnLscDisconnect = OnLscDisconnect;
      _ciCallBack.OnLscSetParams = OnLscSetParams;
      _ciCallBack.OnLscEnquireStatus = OnLscEnquireStatus;
      _ciCallBack.OnLscGetNextBuffer = OnLscGetNextBuffer;
      _ciCallBack.OnLscTransmitBuffer = OnLscTransmitBuffer;

      TtApiResult result = bdaapiOpenCI(_tunerHandle, _ciCallBack);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        _isCiSlotPresent = true;
        _descrambledPrograms = new HashSet<ushort>();
        _isCaInterfaceOpen = true;
      }
      else
      {
        // bdaapiOpenCI() returns "success" when a CI slot is present/connected, otherwise "error".
        this.LogDebug("TechnoTrend: CI slot not present, result = {0}", result);
        _isCiSlotPresent = false;
      }
      return _isCaInterfaceOpen;
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
      this.LogDebug("TechnoTrend: close conditional access interface");

      if (_isCaInterfaceOpen)
      {
        if (_tunerHandle != IntPtr.Zero && _tunerHandle != NativeMethods.INVALID_HANDLE_VALUE)
        {
          bdaapiCloseCI(_tunerHandle);
        }
        else
        {
          this.LogWarn("TechnoTrend: conditional access interfaces is open but hardware handle is null");
        }
      }

      if (isDisposing)
      {
        _descrambledPrograms = null;
        _camInputRequestPrompt = string.Empty;
      }

      _isCiSlotPresent = false;
      _isCamPresent = false;
      _isCamReady = false;
      _isCaInterfaceOpen = false;

      this.LogDebug("TechnoTrend: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      return (this as IConditionalAccessProvider).Close() && (this as IConditionalAccessProvider).Open();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("TechnoTrend: is conditional access interface ready");

      // The API accurately invokes the OnSlotStatus() delegate when the CI or CAM state changes so there
      // is no need to do anything other than report the current state.

      this.LogDebug("TechnoTrend: result = {0}", _isCamReady);
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
      this.LogDebug("TechnoTrend: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("TechnoTrend: the CAM is not ready");
        return false;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        this.LogError("TechnoTrend: conditional access command type {0} is not supported", command);
        return true;
      }
      if (pmt == null)
      {
        this.LogError("TechnoTrend: PMT not supplied");
        return true;
      }

      this.LogDebug("TechnoTrend: program number is {0}", pmt.ProgramNumber);
      if (pmt.ProgramNumber == 0)
      {
        this.LogError("TechnoTrend: program number 0 cannot be descrambled");
        return false;
      }

      // "Not selected" commands don't actually stop the CAM from decrypting the channel. We want to minimise
      // the number of interactions with the CAM as they can cause glitches in recordings etc.
      if (command == CaPmtCommand.NotSelected)
      {
        _descrambledPrograms.Remove(pmt.ProgramNumber);
        return true;
      }

      // We're dealing with a "descrambling" command. Search our list and ensure that the SID is present;
      // add it if it is not present.
      if (listAction == CaPmtListManagementAction.Add ||
        listAction == CaPmtListManagementAction.Update ||
        listAction == CaPmtListManagementAction.More ||
        listAction == CaPmtListManagementAction.Last)
      {
        _descrambledPrograms.Add(pmt.ProgramNumber);
      }
      else if (listAction == CaPmtListManagementAction.Only ||
        listAction == CaPmtListManagementAction.First)
      {
        // "Only" and "first" actions mean start a new list.
        _descrambledPrograms = new HashSet<ushort>();
        _descrambledPrograms.Add(pmt.ProgramNumber);
      }

      // Wait until we have the full list before we send any commands to the CAM.
      if (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.More)
      {
        return true;
      }

      TtApiResult result = TtApiResult.Success;
      if (_descrambledPrograms.Count == 1)
      {
        ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
        byte[] pmtBytes = new byte[rawPmt.Count];
        rawPmt.CopyTo(pmtBytes, 0);
        result = bdaapiCIReadPSIFastWithPMT(_tunerHandle, pmtBytes, (ushort)rawPmt.Count);
      }
      else
      {
        if (_descrambledPrograms.Count > MAX_PROGRAM_COUNT)
        {
          this.LogError("TechnoTrend: not able to descramble {0} programs, limit is {1}", _descrambledPrograms.Count, MAX_PROGRAM_COUNT);
          return false;
        }

        // Send the updated list to the CAM.
        ushort[] programNumbers = new ushort[_descrambledPrograms.Count];
        _descrambledPrograms.CopyTo(programNumbers);
        this.LogDebug("TechnoTrend: program list = {0}", string.Join(", ", programNumbers));
        result = bdaapiCIMultiDecode(_tunerHandle, programNumbers, programNumbers.Length);
      }

      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to send conditional access command, result = {0}", result);
      return false;
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
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      this.LogDebug("TechnoTrend: enter menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("TechnoTrend: failed to enter menu, the CAM is not ready");
        return false;
      }

      TtApiResult result = bdaapiCIEnterModuleMenu(_tunerHandle, _slotIndex);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to enter menu, result = {0}", result);
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("TechnoTrend: close menu (not implemented)");
      return true;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("TechnoTrend: select menu entry, choice = {0}", choice);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("TechnoTrend: failed to select menu entry, the CAM is not ready");
        return false;
      }

      TtApiResult result = bdaapiCIMenuAnswer(_tunerHandle, _slotIndex, choice);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to select menu entry, result = {0}", result);
      return false;
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
      this.LogDebug("TechnoTrend: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("TechnoTrend: failed to answer enquiry, the CAM is not ready");
        return false;
      }
      if (answer.Length > 255)
      {
        this.LogError("TechnoTrend: answer too long, length = {0}", answer.Length);
        return false;
      }

      TtApiResult result = bdaapiCIAnswer(_tunerHandle, _slotIndex, answer, (byte)answer.Length);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to answer enquiry, result = {0}", result);
      return false;
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
      this.LogDebug("TechnoTrend: send DiSEqC command");

      if (!_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("TechnoTrend: DiSEqC command not supplied");
        return true;
      }

      int length = command.Length;
      if (length > MAX_DISEQC_COMMAND_LENGTH)
      {
        this.LogError("TechnoTrend: DiSEqC command too long, length = {0}", length);
        return false;
      }

      // It is okay to use any polarisation. We chose one that will supply 18 Volts to the LNB because it
      // moves dish motors faster.
      TtApiResult result = bdaapiSetDiSEqCMsg(_tunerHandle, command, (byte)length, 0, TtToneBurst.Off, BdaPolarisation.LinearH);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to send DiSEqC command, result = {0}", result);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <remarks>
    /// Don't know whether the driver will send a tone burst command without a
    /// DiSEqC command.
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("TechnoTrend: send tone burst command, command = {0}", command);

      if (!_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }

      TtToneBurst tone = TtToneBurst.Off;
      if (command == ToneBurst.ToneBurst)
      {
        tone = TtToneBurst.ToneBurst;
      }
      else if (command == ToneBurst.DataBurst)
      {
        tone = TtToneBurst.DataBurst;
      }
      TtApiResult result = bdaapiSetDiSEqCMsg(_tunerHandle, null, 0, 0, tone, BdaPolarisation.LinearH);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = success");
        return true;
      }

      this.LogError("TechnoTrend: failed to send tone burst command, result = {0}", result);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      // Set by tune request LNB frequency parameters.
      return true;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
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
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("TechnoTrend: open remote control interface");

      if (!_isTechnoTrend)
      {
        this.LogWarn("TechnoTrend: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("TechnoTrend: remote control interface is already open");
        return true;
      }

      _remoteControlKeyPressDelegate = OnRemoteControlKeyPress;
      TtApiResult result = bdaapiOpenIR(_tunerHandle, _remoteControlKeyPressDelegate, IntPtr.Zero);
      if (result != TtApiResult.Success)
      {
        this.LogError("TechnoTrend: failed to open remote control interface, result = {0}", result);
        return false;
      }

      _isRemoteControlInterfaceOpen = true;
      this.LogDebug("TechnoTrend: result = success");
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
      this.LogDebug("TechnoTrend: close remote control interface");

      if (_isRemoteControlInterfaceOpen)
      {
        if (_tunerHandle != IntPtr.Zero && _tunerHandle != NativeMethods.INVALID_HANDLE_VALUE)
        {
          TtApiResult result = bdaapiCloseIR(_tunerHandle);
          if (result != TtApiResult.Success)
          {
            this.LogError("TechnoTrend: failed to close remote control interface, result = {0}", result);
            return false;
          }
        }
        else
        {
          this.LogWarn("TechnoTrend: remote control interfaces is open but hardware handle is null");
        }
      }
      if (isDisposing)
      {
        _remoteControlKeyPressDelegate = null;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("TechnoTrend: result = success");
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

    ~TechnoTrend()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isTechnoTrend)
      {
        CloseConditionalAccessInterface(isDisposing);
        CloseRemoteControlListenerInterface(isDisposing);
        if (_tunerHandle != IntPtr.Zero)
        {
          bdaapiClose(_tunerHandle);
          _tunerHandle = IntPtr.Zero;
        }
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isTechnoTrend = false;
    }

    #endregion
  }
}