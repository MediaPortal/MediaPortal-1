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
using System.Security;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.TechnoTrend
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for TechnoTrend Budget and Connect series devices.
  /// </summary>
  public class TechnoTrend : BaseCustomDevice, ICustomTuner, IPowerDevice, IConditionalAccessProvider, ICiMenuActions, IDiseqcDevice
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
      Psi                 // programme specific information
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
      SwitchProgrammeReply,
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
      [MarshalAs(UnmanagedType.LPStr)]
      public String CamMenuTitle;
      public IntPtr CaSystemIds;        // array of UInt16
      public UInt16 NumberOfCaSystemIds;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct ConnectionDescription  // TYPE_CONNECT_DESCR
    {
      public TtConnectionType ConnectionType;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String DialIn;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String ClientIpAddress;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String ServerIpAddress;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String TcpPort;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String ConnectionAuthenticationId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String LoginUserName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String LoginPassword;
      public byte RetryCount;
      public byte Timeout;      // unit = 10 ms
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct FilterNames // TS_FilterNames
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String TunerFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String TunerFilter2Name;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String CaptureFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String AnalogTunerFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String AnalogCaptureFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String StbCaptureFilterName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxWindowsPathLength)]
      public String ProductName;
      public TtFrontEndType FrontEndType;
    }

    /// <summary>
    /// A structure for holding the full set of callback function and context pointers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtFullCiCallbacks
    {
      public OnTtSlotStatus OnSlotStatus;
      public IntPtr OnSlotStatusContext;

      public OnTtCaStatus OnCaStatus;
      public IntPtr OnCaStatusContext;

      public OnTtDisplayString OnDisplayString;
      public IntPtr OnDisplayStringContext;

      public OnTtDisplayMenuOrList OnDisplayMenu;
      public IntPtr OnDisplayMenuContext;

      public OnTtDisplayMenuOrList OnDisplayList;
      public IntPtr OnDisplayListContext;

      public OnTtSwitchOsdOff OnSwitchOsdOff;
      public IntPtr OnSwitchOsdOffContext;

      public OnTtInputRequest OnInputRequest;
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
    /// A structure for holding a minimal ("slim") set of callback function and context pointers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtSlimCiCallbacks
    {
      public OnTtSlotStatus OnSlotStatus;
      public IntPtr OnSlotStatusContext;
      public OnTtCaStatus OnCaStatus;
      public IntPtr OnCAStatusContext;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtDvbcTuneRequest
    {
      public TtDeviceType DeviceType;
      public UInt32 Frequency;                // unit = kHz

      public ModulationType Modulation;       // expected to be QAM 16, 32, 64, 128, or 256
      private FECMethod InnerFecMethod;

      private BinaryConvolutionCodeRate InnerFecRate;
      private FECMethod OuterFecMethod;

      private BinaryConvolutionCodeRate OuterFecRate;
      public UInt32 SymbolRate;               // unit = ks/s

      public SpectralInversion SpectralInversion;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      private byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtDvbsTuneRequest
    {
      public TtDeviceType DeviceType;
      public UInt32 Frequency;                // unit = kHz

      public UInt32 FrequencyMultiplier;
      public Polarisation Polarisation;

      private UInt32 Bandwidth;
      public TtDiseqcPort Diseqc;

      private UInt32 Transponder;
      public ModulationType Modulation;       // expected to be QPSK for DVB-S, 8 VSB for DVB-S2

      private FECMethod InnerFecMethod;
      private BinaryConvolutionCodeRate InnerFecRate;

      private FECMethod OuterFecMethod;
      private BinaryConvolutionCodeRate OuterFecRate;

      public UInt32 SymbolRate;               // unit = ks/s
      public SpectralInversion SpectralInversion;

      public UInt32 LnbHighBandLof;           // unit = kHz
      public UInt32 LnbLowBandLof;            // unit = kHz

      public UInt32 LnbSwitchFrequency;       // unit = kHz
      [MarshalAs(UnmanagedType.Bool)]
      public bool UseToneBurst;               // This field turns tone burst on/off; use the Diseqc field to specify the tone state.

      private UInt32 Command;
      private UInt32 CommandCount;

      private UInt32 LnbSource;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      private byte[] RawDiseqc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TtDvbtTuneRequest
    {
      public TtDeviceType DeviceType;
      public UInt32 Frequency;                // unit = kHz

      public UInt32 FrequencyMultiplier;
      public UInt32 Bandwidth;                // unit = kHz

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
    private static extern IntPtr bdaapiOpenHWIdx(TtDeviceCategory category, UInt32 index);

    /// <summary>
    /// Close access to a specific device and relinquish control over it.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern void bdaapiClose(IntPtr device);

    #endregion

    #region automatic offset scanning

    /// <summary>
    /// Turn automatic 125/166 kHz offset scanning on or off.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="autoOffsetModeOn"><c>True</c> to turn offset scanning on.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetDVBTAutoOffsetMode(IntPtr device, [MarshalAs(UnmanagedType.Bool)] bool autoOffsetModeOn);

    /// <summary>
    /// Determine whether automatic 125/166 kHz offset scanning is on or off.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="autoOffsetModeOn"><c>True</c> if automatic offset scanning is on, otherwise <c>false</c>.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDVBTAutoOffsetMode(IntPtr device, [MarshalAs(UnmanagedType.Bool)] ref bool autoOffsetModeOn);

    #endregion

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="command">The command.</param>
    /// <param name="length">The length of the command.</param>
    /// <param name="repeats">The number of times to repeat the command.</param>
    /// <param name="toneBurst">An optional tone burst command that can be sent after the DiSEqC command has been sent.</param>
    /// <param name="polarisation">The polarisation to use when sending the command. This is used as a way to specify the voltage (ie. either 13 or 18 volts).</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetDiSEqCMsg(IntPtr device, IntPtr command, byte length, byte repeats,
                                                        TtToneBurst toneBurst, Polarisation polarisation);

    /// <summary>
    /// Switch the videoport between CI and FTA mode for devices that support a conditional access interface.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="useCiVideoPort"><c>True</c> to use the CI videoport.</param>
    /// <param name="effectiveCiVideoPort"><c>True</c> if the CI videoport is *actually* being used.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetVideoport(IntPtr device, [MarshalAs(UnmanagedType.Bool)] bool useCiVideoPort, [MarshalAs(UnmanagedType.Bool)] ref bool effectiveCiVideoPort);

    #region antenna power

    /// <summary>
    /// Turn the DVB-T antenna 5 volt power supply on or off for USB devices.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="antennaPowerOn"><c>True</c> to turn the antenna power supply on.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiSetDVBTAntPwr(IntPtr device, [MarshalAs(UnmanagedType.Bool)] bool antennaPowerOn);

    /// <summary>
    /// Determine whether the DVB-T antenna 5 volt power supply is on or off.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="antennaPowerOn"><c>True</c> if the antenna power supply is on, otherwise <c>false</c>.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDVBTAntPwr(IntPtr device, [MarshalAs(UnmanagedType.Bool)] ref bool antennaPowerOn);

    #endregion

    #region device information

    /// <summary>
    /// Get the driver version details.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="v1">Part 1 of the driver version number.</param>
    /// <param name="v2">Part 2 of the driver version number.</param>
    /// <param name="v3">Part 3 of the driver version number.</param>
    /// <param name="v4">Part 4 of the driver version number.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDrvVersion(IntPtr device, ref byte v1, ref byte v2, ref byte v3, ref byte v4);

    /// <summary>
    /// Get the hardware MAC address.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="highPart">The high part of the address.</param>
    /// <param name="lowPart">The low part of the address.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetMAC(IntPtr device, ref UInt32 highPart, ref UInt32 lowPart);

    /// <summary>
    /// Get the PCI/USB device IDs. The sub identifiers are only valid for PCI/PCIe devices.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="vendorId">The vendor identifier.</param>
    /// <param name="subVendorId">The sub-vendor identifier.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="subDeviceId">The sub-device identifier.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDeviceIDs(IntPtr device, ref UInt16 vendorId, ref UInt16 subVendorId, ref UInt16 deviceId, ref UInt16 subDeviceId);

    /// <summary>
    /// Determine whether the device really supports USB 2 "high speed" mode.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="usbHighSpeedSupported"><c>True</c> if USB 2 speeds are supported, otherwise <c>false</c>.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetUSBHighspeedMode(IntPtr device, [MarshalAs(UnmanagedType.Bool)] ref bool usbHighSpeedSupported);

    /// <summary>
    /// Retrieve details about the device filters and front end.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="filterNames">A pointer to a FilterNames struct.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDevNameAndFEType(IntPtr device, IntPtr filterNames);

    /// <summary>
    /// Get the device's hardware index.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="index">The hardware index.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetHwIdx(IntPtr device, ref Int32 index);

    /// <summary>
    /// Get the Windows device path.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="devicePath">A buffer containing the device path string.</param>
    /// <param name="devicePathLength">The length of the device path in bytes.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetDevicePath(IntPtr device, [MarshalAs(UnmanagedType.LPStr)] StringBuilder devicePath, ref Int32 devicePathLength);

    /// <summary>
    /// Get the product seller identifier.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiGetProductSellerID(IntPtr device, ref TtProductSeller sellerId);

    #endregion

    #region conditional access interface

    #region open/close the conditional access interface

    /// <summary>
    /// Initialise the conditional access interface. In this case the full set of callback delegates are specified.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="callbacks">Full callback structure pointer.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCI(IntPtr device, TtFullCiCallbacks callbacks);

    /// <summary>
    /// Initialise the conditional access interface. In this case the full set of callback delegates are specified.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="callbacks">Full callback structure pointer.</param>
    /// <param name="ciMessageHandler">A delegate for handling raw messages from the interface.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCIext(IntPtr device, TtFullCiCallbacks callbacks, OnTtCiMessage ciMessageHandler);

    /// <summary>
    /// Initialise the conditional access interface. In this case a minimal set of callback delegates are specified.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="callbacks">Minimal callback structure pointer.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCISlim(IntPtr device, TtSlimCiCallbacks callbacks);

    /// <summary>
    /// Initialise the conditional access interface. In this case callback delegates are not specified.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <returns><c>TtApiResult.Success</c> if a CI slot is present/connected, otherwise <c>TtApiResult.Error</c></returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiOpenCIWithoutPointer(IntPtr device);

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern void bdaapiCloseCI(IntPtr device);

    #endregion

    /// <summary>
    /// Send a message to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="pmt">The PMT for the service that should be descrambled.</param>
    /// <param name="pmtLength">The length of the PMT in bytes.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIReadPSIFastWithPMT(IntPtr device, IntPtr pmt, UInt16 pmtLength);

    /// <summary>
    /// Send a message to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="serviceId">The service ID of the service that should be descrambled.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIReadPSIFastDrvDemux(IntPtr device, UInt16 serviceId);

    /// <summary>
    /// Send a message to the CAM to request that one or more services be descrambled.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="sidList">A list of service IDs, one for each service that should be descrambled.</param>
    /// <param name="serviceCount">The number of services for the CAM to descramble.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIMultiDecode(IntPtr device, IntPtr sidList, Int32 serviceCount);

    /// <summary>
    /// Enter the CAM menu.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIEnterModuleMenu(IntPtr device, byte slotIndex);

    /// <summary>
    /// Request a CI status update. This request will be answered via the OnCiStatus() callback.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIGetSlotStatus(IntPtr device, byte slotIndex);

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="menuAnswer">The user's response.</param>
    /// <param name="answerLength">The length of the user's response in bytes.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIAnswer(IntPtr device, byte slotIndex,
                                                    [MarshalAs(UnmanagedType.LPStr)] String menuAnswer, byte answerLength);

    /// <summary>
    /// Select an entry in the CAM menu.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="choice">The index (0..n) of the menu choice selected by the user.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiCIMenuAnswer(IntPtr device, byte slotIndex, byte choice);

    #endregion

    /// <summary>
    /// Tune to a transponder/multiplex.
    /// </summary>
    /// <param name="device">The handle allocated to this device.</param>
    /// <param name="tuneRequest">A buffer containing the tune request.</param>
    /// <returns>a TechnoTrend API result to indicate success or failure reason</returns>
    [DllImport("Resources\\ttBdaDrvApi_Dll.dll", CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TtApiResult bdaapiTune(IntPtr device, IntPtr tuneRequest);

    #endregion

    #region callback definitions

    /// <summary>
    /// Called when a signal from the remote is detected by the IR receiver.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="code">A buffer containing the remote code. If the code is an RC5 code then it can be found in the lower 2 bytes. RC6 codes use 4 bytes.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtIrCode(IntPtr context, IntPtr code);

    /// <summary>
    /// Called by the tuner driver when the state of the CI slot changes.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <param name="slotInfo">A pointer to a CiSlotInfo struct containing extended information about the interface state.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtSlotStatus(IntPtr context, byte slotIndex, TtCiState state, IntPtr slotInfo);

    /// <summary>
    /// Called by the tuner driver when the result of an interaction with the CAM is known.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="reply">A reply message from the CAM.</param>
    /// <param name="error">An error message from the CAM.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCaStatus(IntPtr context, byte slotIndex, TtMmiMessage reply, TtCiError error);

    /// <summary>
    /// Called by the tuner driver when the CAM requests input from the user. This delegate is called
    /// immediately before OnTtInputRequest().
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="text">The request context text from the CAM.</param>
    /// <param name="textLength">The length of the context text in bytes.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtDisplayString(IntPtr context, byte slotIndex, IntPtr text, Int16 textLength);

    /// <summary>
    /// Called by the tuner driver when a menu or list from the CAM is available.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="numEntries">The number of entries in the menu/list.</param>
    /// <param name="entries">The menu/list entries. Each entry is NULL terminated.</param>
    /// <param name="totalMenuLength">The length of the menu (ie. the sum of the lengths of all entries) in bytes.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtDisplayMenuOrList(IntPtr context, byte slotIndex, Int16 numEntries, IntPtr entries, Int16 totalMenuLength);

    /// <summary>
    /// Called by the tuner driver when the CAM wants to close the menu.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtSwitchOsdOff(IntPtr context, byte slotIndex);

    /// <summary>
    /// Called by the tuner driver when the CAM requests input from the user.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="blind"><c>True</c> if the input should be hidden (eg. password).</param>
    /// <param name="answerLength">The expected answer length.</param>
    /// <param name="keyMask"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtInputRequest(IntPtr context, byte slotIndex, [MarshalAs(UnmanagedType.Bool)] bool blind, byte answerLength, Int16 keyMask);

    /// <summary>
    /// Called by the tuner driver when a message is received from the CAM. This delegate receives the raw
    /// message/response data from the CAM.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="tag">The type of message received.</param>
    /// <param name="buffer">A buffer containing the CAM message/response.</param>
    /// <param name="bufferSize">The size of the buffer.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtCiMessage(byte slotIndex, TtCiMessageHandlerTag tag, IntPtr buffer, Int16 bufferSize);

    #region low speed communication interface callback definitions

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
    /// <param name="timeout">A timeout in units of ten milliseconds.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTtLscSetParams(IntPtr context, byte slotIndex, byte bufferSize, byte timeout);

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
    private delegate void OnTtLscTransmitBuffer(IntPtr context, byte slotIndex, byte phaseId, IntPtr buffer, Int16 bufferSize);

    #endregion

    #endregion

    #region constants

    private const int MaxWindowsPathLength = 260;
    private const int MaxDiseqcCommandLength = 64;    // This is arbitrary - the hardware/interface limit is not known.
    private const int TuneRequestSize = 100;

    private static readonly string[] ValidBudget2DeviceNames = new string[]
    {
      "TechnoTrend BDA/DVB-C Tuner",
      "TechnoTrend BDA/DVB-S Tuner",
      "TechnoTrend BDA/DVB-T Tuner",
      "ttBudget2 BDA DVB-C Tuner",
      "ttBudget2 BDA DVB-S Tuner",
      "ttBudget2 BDA DVB-T Tuner"
    };

    private static readonly string[] ValidBudget3DeviceNames = new string[]
    {
      "TTHybridTV BDA DVBT Tuner",
      "TTHybridTV BDA ATSC Tuner"
    };

    private static readonly string[] ValidUsb2DeviceNames = new string[]
    {
      "USB 2.0 BDA DVB-C Tuner",
      "USB 2.0 BDA DVB-S Tuner",
      "USB 2.0 BDA (DVB-T Fake) DVB-T Tuner",
      "USB 2.0 BDA DVB-T Tuner"
    };

    private static readonly string[] ValidPinnacleDeviceNames = new string[]
    {
      "Pinnacle PCTV 4XXe Tuner"
    };

    private static readonly string[] ValidDssDeviceNames = new string[]
    {
      "USB 2.0 BDA DSS Tuner"
    };

    #endregion

    #region variables

    private bool _isTechnoTrend = false;
    private bool _isCiSlotPresent = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private byte _slotIndex = 0;
    private TtCiState _ciState = TtCiState.Unknown;

    private CardType _tunerType = CardType.Unknown;
    private TtDeviceCategory _deviceCategory = TtDeviceCategory.Unknown;
    private String _name = "TechnoTrend";

    private IntPtr _deviceHandle = IntPtr.Zero;
    private IntPtr _serviceBuffer = IntPtr.Zero;
    private IntPtr _generalBuffer = IntPtr.Zero;

    private TtFullCiCallbacks _callbacks;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    // When the CAM asks for input (via callback), the input request context (description) is provided
    // by a separate callback. We use this variable to cache it.
    private String _camInputRequestContext = String.Empty;

    private HashSet<UInt16> _descrambledServices = null;

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
      if (filterInfo.pGraph != null)
      {
        DsUtils.ReleaseComObject(filterInfo.pGraph);
        filterInfo.pGraph = null;
      }
      if (hr != 0 || String.IsNullOrEmpty(filterInfo.achName))
      {
        return TtDeviceCategory.Unknown;
      }

      foreach (String name in ValidBudget2DeviceNames)
      {
        if (filterInfo.achName.Equals(name))
        {
          return TtDeviceCategory.Budget2;
        }
      }
      foreach (String name in ValidBudget3DeviceNames)
      {
        if (filterInfo.achName.Equals(name))
        {
          return TtDeviceCategory.Budget3;
        }
      }
      foreach (String name in ValidUsb2DeviceNames)
      {
        if (filterInfo.achName.Equals(name))
        {
          return TtDeviceCategory.Usb2;
        }
      }
      foreach (String name in ValidPinnacleDeviceNames)
      {
        if (filterInfo.achName.Equals(name))
        {
          return TtDeviceCategory.Usb2Pinnacle;
        }
      }
      foreach (String name in ValidDssDeviceNames)
      {
        if (filterInfo.achName.Equals(name))
        {
          return TtDeviceCategory.Usb2Dss;
        }
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

      // Request the raw medium data.
      IntPtr raw;
      int hr = pin.KsQueryMediums(out raw);
      if (hr != 0 || raw == IntPtr.Zero)
      {
        if (raw != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(raw);
          raw = IntPtr.Zero;
        }
        DsUtils.ReleaseComObject(pin);
        pin = null;
        return deviceId;
      }

      try
      {
        // Read the number of mediums.
        int countMediums = Marshal.ReadInt32(raw, 4);
        if (countMediums == 0)
        {
          return deviceId;
        }

        // Calculate the address of the first medium.
        IntPtr addr = new IntPtr(raw.ToInt64() + 8);
        // Marshal the data into an RPM structure.
        RegPinMedium rpm = (RegPinMedium)Marshal.PtrToStructure(addr, typeof(RegPinMedium));
        return rpm.dw1;
      }
      finally
      {
        Marshal.FreeCoTaskMem(raw);
        raw = IntPtr.Zero;
        DsUtils.ReleaseComObject(pin);
        pin = null;
      }
    }

    /// <summary>
    /// Attempt to read the device information from the device.
    /// </summary>
    private void ReadDeviceInfo()
    {
      this.LogDebug("TechnoTrend: read device information");

      // General product details.
      IntPtr info = Marshal.AllocCoTaskMem(1824);
      for (int i = 0; i < 1824; i++)
      {
        Marshal.WriteByte(info, i, 0);
      }
      TtApiResult result = bdaapiGetDevNameAndFEType(_deviceHandle, info);
      if (result == TtApiResult.Success)
      {
        FilterNames names = (FilterNames)Marshal.PtrToStructure(info, typeof(FilterNames));
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
        this.LogDebug("TechnoTrend: failed to read the device details, result = {0}", result);
      }
      Marshal.FreeCoTaskMem(info);

      // Product seller.
      TtProductSeller seller = TtProductSeller.Unknown;
      result = bdaapiGetProductSellerID(_deviceHandle, ref seller);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("  product (re)seller  = {0}", seller);
      }
      else
      {
        this.LogDebug("TechnoTrend: failed to determine the product (re)seller, result = {0}", result);
      }

      // Driver version.
      byte v1 = 0;
      byte v2 = 0;
      byte v3 = 0;
      byte v4 = 0;
      result = bdaapiGetDrvVersion(_deviceHandle, ref v1, ref v2, ref v3, ref v4);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("  driver version      = {0}.{1}.{2}.{3}", v1, v2, v3, v4);
      }
      else
      {
        this.LogDebug("TechnoTrend: failed to read the driver version, result = {0}", result);
      }

      // MAC address.
      UInt32 lowPart = 0;
      UInt32 highPart = 0;
      result = bdaapiGetMAC(_deviceHandle, ref highPart, ref lowPart);
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
        this.LogDebug("TechnoTrend: failed to read the MAC address, result = {0}", result);
      }

      // USB speed.
      if (_deviceCategory == TtDeviceCategory.Usb2 || _deviceCategory == TtDeviceCategory.Usb2Pinnacle || _deviceCategory == TtDeviceCategory.Usb2Dss)
      {
        bool highSpeed = false;
        result = bdaapiGetUSBHighspeedMode(_deviceHandle, ref highSpeed);
        if (result == TtApiResult.Success)
        {
          this.LogDebug("  USB 2 speed support = {0}", highSpeed);
        }
        else
        {
          this.LogDebug("TechnoTrend: failed to determine whether USB high speed is supported, result = {0}", result);
        }
      }
    }

    /// <summary>
    /// Trigger the driver to send a conditional access interface status update (call OnCiStatus()). Note
    /// that I don't recommend using this function as it seems to mess up the driver state.
    /// </summary>
    private void GetCiSlotStatus()
    {
      TtApiResult result = bdaapiCIGetSlotStatus(_deviceHandle, _slotIndex);
      if (result != TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: bdaapiCIGetSlotStatus failed, result = {0}", result);
      }
    }

    #region callback handlers

    /// <summary>
    /// Called by the tuner driver when the state of the CI slot changes.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <param name="slotInfo">A pointer to a CiSlotInfo struct containing extended information about the interface state.</param>
    private void OnSlotStatus(IntPtr context, byte slotIndex, TtCiState state, IntPtr slotInfo)
    {
      this.LogDebug("TechnoTrend: CI slot status callback, slot = {0}", slotIndex);
      if (state == _ciState)
      {
        // Don't be too verbose - we don't need to print the CAS IDs all the time.
        this.LogDebug("TechnoTrend: CI state = {0}", _ciState);
        return;
      }

      this.LogDebug("TechnoTrend: CI state change, old state = {0}, new state = {1}", _ciState, state);
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
        if (info.CamMenuTitle.Equals(String.Empty))
        {
          this.LogDebug("  menu title = (not available)");
        }
        else
        {
          this.LogDebug("  menu title = {0} ", info.CamMenuTitle);
        }
        this.LogDebug("  # CAS IDs  = {0}", info.NumberOfCaSystemIds);
        for (int i = 0; i < info.NumberOfCaSystemIds; i++)
        {
          this.LogDebug("  {0,-2}         = 0x{1:x4}", i + 1, Marshal.ReadInt16(info.CaSystemIds, i * 2));
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TechnoTrend: CI slot status callback exception");
      }
    }

    /// <summary>
    /// Called by the tuner driver when the result of an interaction with the CAM is known.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="reply">A reply message from the CAM.</param>
    /// <param name="error">An error message from the CAM.</param>
    private void OnCaStatus(IntPtr context, byte slotIndex, TtMmiMessage reply, TtCiError error)
    {
      this.LogDebug("TechnoTrend: CA status callback, slot = {0}, reply = {1}, error = {2}", slotIndex, reply, error);
      try
      {
        // NoCaResource generally seems to indicate a smartcard or CAM error. The TechnoTrend
        // API doesn't seem to support removal and reinsertion of the smartcard on the fly
        // (no CAM state change reported). Closing and re-opening the conditional access
        // seems to resolve the problem.
        if (error == TtCiError.NoCaResource)
        {
          bool rebuildGraph;
          ResetInterface(out rebuildGraph);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TechnoTrend: CA status callback exception");
      }
    }

    /// <summary>
    /// Called by the tuner driver when the CAM requests input from the user. This delegate is called
    /// immediately before OnTtInputRequest().
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="text">The request context text from the CAM.</param>
    /// <param name="textLength">The length of the context text in bytes.</param>
    private void OnDisplayString(IntPtr context, byte slotIndex, IntPtr text, Int16 textLength)
    {
      try
      {
        _camInputRequestContext = Marshal.PtrToStringAnsi(text, textLength);
        this.LogDebug("TechnoTrend: display string callback, slot = {0}, string = {1}", slotIndex, _camInputRequestContext);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TechnoTrend: display string callback exception");
      }
    }

    /// <summary>
    /// Called by the tuner driver when a menu or list from the CAM is available.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="numEntries">The number of entries in the menu/list.</param>
    /// <param name="entries">The menu/list entries. Each entry is NULL terminated.</param>
    /// <param name="totalMenuLength">The length of the menu (ie. the sum of the lengths of all entries) in bytes.</param>
    private void OnDisplayMenuOrList(IntPtr context, byte slotIndex, Int16 numEntries, IntPtr entries, Int16 totalMenuLength)
    {
      try
      {
        this.LogDebug("TechnoTrend: display menu/list callback, slot = {0}, total menu length = {1}", slotIndex, totalMenuLength);

        if (_ciMenuCallbacks == null)
        {
          this.LogDebug("TechnoTrend: menu callbacks are not set");
        }

        // Construct menu/list strings for callback.
        StringBuilder[] strings = new StringBuilder[numEntries];
        int idx = 0;
        byte charChode;
        for (int i = 0; i < totalMenuLength - 1; i++)   // There is an extra NULL character to indicate the end of the menu.
        {
          charChode = Marshal.ReadByte((IntPtr)(entries.ToInt64() + i));
          // Start of a new entry?
          if (strings[idx] == null)
          {
            strings[idx] = new StringBuilder();
          }

          if (charChode != 0)
          {
            strings[idx].Append((char)charChode);
            continue;
          }

          // End of an entry. Is the meta-data complete?
          if (idx == 2)
          {
            this.LogDebug("  title     = {0}", strings[0].ToString());
            this.LogDebug("  sub-title = {0}", strings[1].ToString());
            this.LogDebug("  footer    = {0}", strings[2].ToString());
            this.LogDebug("  # entries = {0}", numEntries - 3);
            if (_ciMenuCallbacks != null)
            {
              _ciMenuCallbacks.OnCiMenu(strings[0].ToString(), strings[1].ToString(), strings[2].ToString(), numEntries - 3);
            }
          }
          else if (idx > 2)
          {
            this.LogDebug("  entry {0,-2}  = {1}", idx - 2, strings[idx].ToString());
            if (_ciMenuCallbacks != null)
            {
              _ciMenuCallbacks.OnCiMenuChoice(idx - 3, strings[idx].ToString());
            }
          }

          // Start new entry.
          idx++;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TechnoTrend: display menu/list callback exception");
      }
    }

    /// <summary>
    /// Called by the tuner driver when the CAM wants to close the menu.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnSwitchOsdOff(IntPtr context, byte slotIndex)
    {
      this.LogDebug("TechnoTrend: switch OSD off callback, slot = {0}", slotIndex);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiCloseDisplay(0);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "TechnoTrend: switch OSD off callback exception");
        }
      }
      else
      {
        this.LogDebug("TechnoTrend: menu callbacks are not set");
      }
    }

    /// <summary>
    /// Called by the tuner driver when the CAM requests input from the user.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="blind"><c>True</c> if the input should be hidden (eg. password).</param>
    /// <param name="answerLength">The expected answer length.</param>
    /// <param name="keyMask"></param>
    private void OnInputRequest(IntPtr context, byte slotIndex, bool blind, byte answerLength, Int16 keyMask)
    {
      this.LogDebug("TechnoTrend: input request callback, slot = {0}", slotIndex);
      this.LogDebug("  length   = {0}", answerLength);
      this.LogDebug("  blind    = {0}", blind);
      this.LogDebug("  key mask = {0:x4}", keyMask);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiRequest(blind, answerLength, _camInputRequestContext);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "TechnoTrend: input request callback exception");
        }
      }
      else
      {
        this.LogDebug("TechnoTrend: menu callbacks are not set");
      }
    }

    #region low speed communication callbacks

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="descriptor">???</param>
    private void OnLscSetDescriptor(IntPtr context, byte slotIndex, IntPtr descriptor)
    {
      this.LogDebug("TechnoTrend: OnLscSetDescriptor callback, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnLscConnect(IntPtr context, byte slotIndex)
    {
      this.LogDebug("TechnoTrend: OnLscConnect callback, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnLscDisconnect(IntPtr context, byte slotIndex)
    {
      this.LogDebug("TechnoTrend: OnLscDisconnect callback, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="bufferSize"></param>
    /// <param name="timeout">A timeout in units of ten milliseconds.</param>
    private void OnLscSetParams(IntPtr context, byte slotIndex, byte bufferSize, byte timeout)
    {
      this.LogDebug("TechnoTrend: OnLscSetParams callback, slot = {0}, buffer size = {1}, timeout = {2}", slotIndex, bufferSize, timeout);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    private void OnLscEnquireStatus(IntPtr context, byte slotIndex)
    {
      this.LogDebug("TechnoTrend: OnLscEnquireStatus callback, slot = {0}", slotIndex);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="phaseId"></param>
    private void OnLscGetNextBuffer(IntPtr context, byte slotIndex, byte phaseId)
    {
      this.LogDebug("TechnoTrend: OnLscGetNextBuffer callback, slot = {0}, phase = {1}", slotIndex, phaseId);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="phaseId"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferSize"></param>
    private void OnLscTransmitBuffer(IntPtr context, byte slotIndex, byte phaseId, IntPtr buffer, Int16 bufferSize)
    {
      this.LogDebug("TechnoTrend: OnLscTransmitBuffer callback, slot = {0}, phase = {1}", slotIndex, phaseId);
      DVB_MMI.DumpBinary(buffer, 0, bufferSize);
    }

    #endregion

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
        return _name;
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
      this.LogDebug("TechnoTrend: initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("TechnoTrend: tuner filter is null");
        return false;
      }
      if (_isTechnoTrend)
      {
        this.LogDebug("TechnoTrend: device is already initialised");
        return true;
      }

      _deviceCategory = GetDeviceCategory(tunerFilter);
      if (_deviceCategory == TtDeviceCategory.Unknown)
      {
        this.LogDebug("TechnoTrend: device category is unknown");
        return false;
      }

      int deviceId = GetDeviceId(tunerFilter);
      if (deviceId == -1)
      {
        this.LogDebug("TechnoTrend: failed to determine device ID");
        return false;
      }

      _deviceHandle = bdaapiOpenHWIdx(_deviceCategory, (uint)deviceId);
      if (_deviceHandle == IntPtr.Zero || _deviceHandle.ToInt64() == -1)
      {
        this.LogDebug("TechnoTrend: hardware interface could not be opened");
        return false;
      }

      this.LogDebug("TechnoTrend: supported device detected, category = {0}, id = {1}", _deviceCategory, deviceId);
      _isTechnoTrend = true;
      _tunerType = tunerType;
      _generalBuffer = Marshal.AllocCoTaskMem(TuneRequestSize);
      ReadDeviceInfo();
      if (_tunerType == CardType.DvbT)
      {
        TtApiResult result = bdaapiSetDVBTAutoOffsetMode(_deviceHandle, false);
        if (result != TtApiResult.Success)
        {
          this.LogDebug("TechnoTrend: failed to turn off auto offset mode, result = {0}", result);
        }
      }
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out DeviceAction action)
    {
      this.LogDebug("TechnoTrend: on before tune callback");
      action = DeviceAction.Default;

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return;
      }

      // We only need to tweak the modulation for DVB-S/S2 channels.
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }

      if (ch.ModulationType == ModulationType.ModQpsk || ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.Mod8Vsb;
      }
      else if (ch.ModulationType == ModulationType.ModNotSet)
      {
        ch.ModulationType = ModulationType.ModQpsk;
      }
      this.LogDebug("  modulation = {0}", ch.ModulationType);
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      this.LogDebug("TechnoTrend: set power state, on = {0}", powerOn);

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }
      if (_tunerType != CardType.DvbT)
      {
        this.LogDebug("TechnoTrend: power control is not supported for this device");
        return false;
      }

      TtApiResult result = bdaapiSetDVBTAntPwr(_deviceHandle, powerOn);
      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
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
      // Tuning of DVB-C, DVB-S/2 and DVB-T/2 channels is supported with an appropriate tuner.
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
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("TechnoTrend: tune to channel");

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }

      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel != null && _tunerType == CardType.DvbC)
      {
        TtDvbcTuneRequest tuneRequest = new TtDvbcTuneRequest();
        tuneRequest.DeviceType = TtDeviceType.DvbC;
        tuneRequest.Frequency = (uint)dvbcChannel.Frequency;
        tuneRequest.Modulation = dvbcChannel.ModulationType;
        tuneRequest.SymbolRate = (uint)dvbcChannel.SymbolRate;
        tuneRequest.SpectralInversion = SpectralInversion.Automatic;

        Marshal.StructureToPtr(tuneRequest, _generalBuffer, true);
      }
      else
      {
        DVBSChannel dvbsChannel = channel as DVBSChannel;
        if (dvbsChannel != null && _tunerType == CardType.DvbS)
        {
          TtDvbsTuneRequest tuneRequest = new TtDvbsTuneRequest();
          tuneRequest.DeviceType = TtDeviceType.DvbS;
          // Frequency is already specified in kHz (the base unit) so the
          // multiplier is set to 1.
          tuneRequest.Frequency = (uint)dvbsChannel.Frequency;
          tuneRequest.FrequencyMultiplier = 1;
          tuneRequest.Polarisation = dvbsChannel.Polarisation;
          tuneRequest.Diseqc = TtDiseqcPort.Null;
          tuneRequest.UseToneBurst = false;
          tuneRequest.Modulation = dvbsChannel.ModulationType;
          tuneRequest.SymbolRate = (uint)dvbsChannel.SymbolRate;
          tuneRequest.SpectralInversion = SpectralInversion.Automatic;
          tuneRequest.LnbLowBandLof = (uint)dvbsChannel.LnbType.LowBandFrequency;
          tuneRequest.LnbHighBandLof = (uint)dvbsChannel.LnbType.HighBandFrequency;
          tuneRequest.LnbSwitchFrequency = (uint)dvbsChannel.LnbType.SwitchFrequency;

          Marshal.StructureToPtr(tuneRequest, _generalBuffer, true);
        }
        else
        {
          DVBTChannel dvbtChannel = channel as DVBTChannel;
          if (dvbtChannel != null && _tunerType == CardType.DvbT)
          {
            TtDvbtTuneRequest tuneRequest = new TtDvbtTuneRequest();
            tuneRequest.DeviceType = TtDeviceType.DvbT;
            tuneRequest.Frequency = (uint)dvbtChannel.Frequency;
            // Frequency is already specified in kHz (the base unit) so the
            // multiplier is set to 1.
            tuneRequest.FrequencyMultiplier = 1;
            tuneRequest.Bandwidth = (uint)dvbtChannel.Bandwidth;
            tuneRequest.Modulation = ModulationType.ModNotSet;
            tuneRequest.SpectralInversion = SpectralInversion.Automatic;

            Marshal.StructureToPtr(tuneRequest, _generalBuffer, true);
          }
          else
          {
            this.LogDebug("TechnoTrend: tuning is not supported for this channel");
            return false;
          }
        }
      }

      //DVB_MMI.DumpBinary(_generalBuffer, 0, TuneRequestSize);
      TtApiResult result = bdaapiTune(_deviceHandle, _generalBuffer);
      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
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
      this.LogDebug("TechnoTrend: open conditional access interface");

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }
      if (_descrambledServices != null || _serviceBuffer != IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: interface is already open");
        return false;
      }

      // Callback contexts.
      _callbacks.OnSlotStatusContext = _deviceHandle;
      _callbacks.OnCaStatusContext = _deviceHandle;
      _callbacks.OnDisplayStringContext = _deviceHandle;
      _callbacks.OnDisplayMenuContext = _deviceHandle;
      _callbacks.OnDisplayListContext = _deviceHandle;
      _callbacks.OnSwitchOsdOffContext = _deviceHandle;
      _callbacks.OnInputRequestContext = _deviceHandle;
      _callbacks.OnLscSetDescriptorContext = _deviceHandle;
      _callbacks.OnLscConnectContext = _deviceHandle;
      _callbacks.OnLscDisconnectContext = _deviceHandle;
      _callbacks.OnLscSetParamsContext = _deviceHandle;
      _callbacks.OnLscEnquireStatusContext = _deviceHandle;
      _callbacks.OnLscGetNextBufferContext = _deviceHandle;
      _callbacks.OnLscTransmitBufferContext = _deviceHandle;

      // Callback functions.
      _callbacks.OnSlotStatus = OnSlotStatus;
      _callbacks.OnCaStatus = OnCaStatus;
      _callbacks.OnDisplayString = OnDisplayString;
      _callbacks.OnDisplayMenu = OnDisplayMenuOrList;
      _callbacks.OnDisplayList = OnDisplayMenuOrList;
      _callbacks.OnSwitchOsdOff = OnSwitchOsdOff;
      _callbacks.OnInputRequest = OnInputRequest;
      _callbacks.OnLscSetDescriptor = OnLscSetDescriptor;
      _callbacks.OnLscConnect = OnLscConnect;
      _callbacks.OnLscDisconnect = OnLscDisconnect;
      _callbacks.OnLscSetParams = OnLscSetParams;
      _callbacks.OnLscEnquireStatus = OnLscEnquireStatus;
      _callbacks.OnLscGetNextBuffer = OnLscGetNextBuffer;
      _callbacks.OnLscTransmitBuffer = OnLscTransmitBuffer;

      TtApiResult result = bdaapiOpenCI(_deviceHandle, _callbacks);
      if (result == TtApiResult.Success)
      {
        this.LogDebug("TechnoTrend: result = {0}", result);
        _isCiSlotPresent = true;
        _serviceBuffer = Marshal.AllocCoTaskMem(200);
        _descrambledServices = new HashSet<UInt16>();
      }
      else
      {
        // bdaapiOpenCI() returns "success" when a CI slot is present/connected, otherwise "error".
        this.LogDebug("TechnoTrend: CI slot not present, result = {0}", result);
        _isCiSlotPresent = false;
      }
      return _isCiSlotPresent;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      this.LogDebug("TechnoTrend: close conditional access interface");

      if (_isCiSlotPresent)
      {
        bdaapiCloseCI(_deviceHandle);
      }
      if (_serviceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_serviceBuffer);
        _serviceBuffer = IntPtr.Zero;
      }
      _descrambledServices = null;
      _camInputRequestContext = String.Empty;
      _isCiSlotPresent = false;
      _isCamPresent = false;
      _isCamReady = false;
      this.LogDebug("TechnoTrend: result = success");
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
      rebuildGraph = false;
      return CloseInterface() && OpenInterface();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      this.LogDebug("TechnoTrend: is conditional access interface ready");

      // The API accurately invokes the OnSlotStatus() delegate when the CI or CAM state changes so there
      // is no need to do anything other than report the current state.

      this.LogDebug("TechnoTrend: result = {0}", _isCamReady);
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
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      this.LogDebug("TechnoTrend: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }
      if (!_isCiSlotPresent)
      {
        this.LogDebug("TechnoTrend: CI slot not present");
        // Don't retry - a restart is required for the CI slot to be connected.
        return true;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        this.LogDebug("TechnoTrend: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null)
      {
        this.LogDebug("TechnoTrend: PMT not supplied");
        return true;
      }

      this.LogDebug("TechnoTrend: service ID is {0} (0x{0:x})", pmt.ProgramNumber);
      if (pmt.ProgramNumber == 0)
      {
        this.LogDebug("TechnoTrend: service 0 cannot be descrambled");
        return false;
      }

      // "Not selected" commands don't actually stop the CAM from decrypting the channel. We want to minimise
      // the number of interactions with the CAM as they can cause glitches in recordings etc.
      if (command == CaPmtCommand.NotSelected)
      {
        _descrambledServices.Remove(pmt.ProgramNumber);
        return true;
      }

      // We're dealing with a "descrambling" command. Search our list and ensure that the SID is present;
      // add it if it is not present.
      if (listAction == CaPmtListManagementAction.Add ||
        listAction == CaPmtListManagementAction.Update ||
        listAction == CaPmtListManagementAction.More ||
        listAction == CaPmtListManagementAction.Last)
      {
        if (!_descrambledServices.Contains(pmt.ProgramNumber))
        {
          _descrambledServices.Add(pmt.ProgramNumber);
        }
      }
      else if (listAction == CaPmtListManagementAction.Only ||
        listAction == CaPmtListManagementAction.First)
      {
        // "Only" and "first" actions mean start a new list.
        _descrambledServices = new HashSet<UInt16>();
        _descrambledServices.Add(pmt.ProgramNumber);
      }

      // Wait until we have the full list before we send any commands to the CAM.
      if (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.More)
      {
        return true;
      }

      // Send the updated list to the CAM.
      this.LogDebug("TechnoTrend: service list");
      int i = 0;
      HashSet<UInt16>.Enumerator en = _descrambledServices.GetEnumerator();
      while (en.MoveNext())
      {
        this.LogDebug("  {0} = {1} (0x{1:x4})", i + 1, en.Current);
        Marshal.WriteInt16(_serviceBuffer, 2 * i, (Int16)en.Current);
        i++;
      }

      TtApiResult result = bdaapiCIMultiDecode(_deviceHandle, _serviceBuffer, i);
      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
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
      this.LogDebug("TechnoTrend: enter menu");

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogDebug("TechnoTrend: the CAM is not ready");
        return false;
      }

      TtApiResult result = bdaapiCIEnterModuleMenu(_deviceHandle, _slotIndex);
      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      this.LogDebug("TechnoTrend: close menu (not implemented)");
      return true;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      this.LogDebug("TechnoTrend: select menu entry, choice = {0}", choice);

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogDebug("TechnoTrend: the CAM is not ready");
        return false;
      }

      TtApiResult result = bdaapiCIMenuAnswer(_deviceHandle, _slotIndex, choice);
      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
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
      this.LogDebug("TechnoTrend: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogDebug("TechnoTrend: the CAM is not ready");
        return false;
      }
      if (answer.Length > 255)
      {
        this.LogDebug("TechnoTrend: answer too long, length = {0}", answer.Length);
        return false;
      }

      TtApiResult result = bdaapiCIAnswer(_deviceHandle, _slotIndex, answer, (byte)answer.Length);
      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The TechnoTrend interface does not support directly setting the 22 kHz tone state. The
    /// tuning request LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      // TODO: this function needs to be tested. I'm uncertain whether the driver will accept commands with no DiSEqC messages.
      this.LogDebug("TechnoTrend: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }

      TtToneBurst tone = TtToneBurst.Off;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        tone = TtToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        tone = TtToneBurst.DataBurst;
      }
      TtApiResult result = bdaapiSetDiSEqCMsg(_deviceHandle, IntPtr.Zero, 0, 0, tone, Polarisation.LinearH);

      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      this.LogDebug("TechnoTrend: send DiSEqC command");

      if (!_isTechnoTrend || _deviceHandle == IntPtr.Zero)
      {
        this.LogDebug("TechnoTrend: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("TechnoTrend: command not supplied");
        return true;
      }

      int length = command.Length;
      if (length > MaxDiseqcCommandLength)
      {
        this.LogDebug("TechnoTrend: command too long, length = {0}", command.Length);
        return false;
      }
      Marshal.Copy(command, 0, _generalBuffer, length);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, length);

      // It is okay to use any polarisation. We chose one that will supply 18 Volts to the LNB because it
      // moves dish motors faster.
      TtApiResult result = bdaapiSetDiSEqCMsg(_deviceHandle, _generalBuffer, (byte)length, 0, TtToneBurst.Off, Polarisation.LinearH);
      this.LogDebug("TechnoTrend: result = {0}", result);
      return (result == TtApiResult.Success);
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
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _deviceHandle = IntPtr.Zero;
      _isTechnoTrend = false;
    }

    #endregion
  }
}