/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{

  #region API Enums

  #region TTApiResult

  /// <summary>
  /// Return values of TT API
  /// </summary>
  public enum TTApiResult
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
    /// the infrared interface is already initialised
    /// </summary> 
    IRAlreadyOpen,
    /// <summary>
    /// the infrared interface is not initialised
    /// </summary> 
    IRNotOpened,
    /// <summary>
    /// length exceeds maximum in EEPROM-Userspace operation
    /// </summary> 
    TooManyBytes,
    /// <summary>
    /// common interface hardware error
    /// </summary> 
    CIHardwareError,
    /// <summary>
    /// common interface already opened
    /// </summary> 
    CIAlreadyOpen,
    /// <summary>
    /// operation finished with timeout
    /// </summary> 
    TimeOut,
    /// <summary>
    /// read psi failed
    /// </summary> 
    ReadPSIFailed,
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
    /// the tunerequest structure did not have the expected size
    /// </summary> 
    IncorrectSize,
    /// <summary>
    /// the tuner interface was not available
    /// </summary> 
    TunerIFNotAvailable,
    /// <summary>
    /// an unknown DVB type has been specified for the tune request
    /// </summary> 
    UnknownDVBType,
    /// <summary>
    /// length of buffer is too small
    /// </summary> 
    BufferTooSmall
  }

  #endregion

  #region TTApiDeviceCat

  /// <summary>
  /// API Device Catagory
  /// </summary>
  public enum TTApiDeviceCat
  {
    /// <summary> 
    /// not set
    /// </summary> 
    UNKNOWN = 0,
    /// <summary> 
    /// Budget 2
    /// </summary> 
    BUDGET_2,
    /// <summary> 
    /// Budget 3 aka TT-budget T-3000
    /// </summary> 
    BUDGET_3,
    /// <summary> 
    /// USB 2.0
    /// </summary> 
    USB_2,
    /// <summary> 
    /// USB 2.0 Pinnacle
    /// </summary> 
    USB_2_PINNACLE,
    /// <summary> 
    /// USB 2.0 DSS
    /// </summary> 
    USB_2_DSS,
    /// <summary> 
    /// Premium
    /// </summary> 
    PREMIUM
  }

  #endregion

  #region TTCiSlotStatus

  /// <summary>
  /// Status for CI Slot
  /// </summary>
  public enum TTCiSlotStatus
  {
    /// Common interface slot is empty.
    SlotEmpty = 0,
    /// A CAM is inserted into the common interface.
    SlotModuleInserted = 1,
    /// CAM initialisation ready.
    SlotModuleOk = 2,
    /// CAM initialisation ready.
    SlotCaOk = 3,
    /// CAM initialisation ready.
    SlotDbgMsg = 4,
    /// Slot state could not be determined.
    SlotUnknownState = 0xFF
  }

  #endregion

  #endregion

  /// <summary>
  /// Technotrend BDA API wrapper and CI handler
  /// </summary>
  public unsafe class TechnoTrendAPI : IDisposable, IDiSEqCController, ICiMenuActions
  {
    #region Structs for callback and arguments

    /// <summary>
    /// Technotrend: Callback structures
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct TTCallbackStruct
    {
      public OnSlotStatusCallback p01;

      /// Context pointer for PCBFCN_CI_OnSlotStatus
      public IntPtr p01Context;

      /// PCBFCN_CI_OnCAStatus
      public OnCaChangeCallback p02;

      /// Context pointer for PCBFCN_CI_OnCAStatus
      public IntPtr p02Context;

      /// PCBFCN_CI_OnDisplayString
      public OnDisplayString p03;

      /// Context pointer for PCBFCN_CI_OnDisplayString
      public IntPtr p03Context;

      /// PCBFCN_CI_OnDisplayMenu
      public OnDisplayMenuAndLists p04; // use same type of delegate

      /// Context pointer for PCBFCN_CI_OnDisplayMenu
      public IntPtr p04Context;

      /// PCBFCN_CI_OnDisplayList
      public OnDisplayMenuAndLists p05; // use same type of delegate

      /// Context pointer for PCBFCN_CI_OnDisplayList
      public IntPtr p05Context;

      /// PCBFCN_CI_OnSwitchOsdOff
      public OnSwitchOsdOff p06;

      /// Context pointer for PCBFCN_CI_OnSwitchOsdOff
      public IntPtr p06Context;

      /// PCBFCN_CI_OnInputRequest
      public OnInputRequest p07;

      /// Context pointer for PCBFCN_CI_OnInputRequest
      public IntPtr p07Context;

      /// PCBFCN_CI_OnLscSetDescriptor
      public OnLscSetDescriptor p08;

      /// Context pointer for PCBFCN_CI_OnLscSetDescriptor
      public IntPtr p08Context;

      /// PCBFCN_CI_OnLscConnect
      public OnLscConnect p09;

      /// Context pointer for PCBFCN_CI_OnLscConnect
      public IntPtr p09Context;

      /// PCBFCN_CI_OnLscDisconnect
      public OnLscDisconnect p10;

      /// Context pointer for PCBFCN_CI_OnLscDisconnect
      public IntPtr p10Context;

      /// PCBFCN_CI_OnLscSetParams
      public OnLscSetParams p11;

      /// Context pointer for PCBFCN_CI_OnLscSetParams
      public IntPtr p11Context;

      /// PCBFCN_CI_OnLscEnquireStatus
      public OnLscEnquireStatus p12;

      /// Context pointer for PCBFCN_CI_OnLscEnquireStatus
      public IntPtr p12Context;

      /// PCBFCN_CI_OnLscGetNextBuffer
      public OnLscGetNextBuffer p13;

      /// Context pointer for PCBFCN_CI_OnLscGetNextBuffer
      public IntPtr p13Context;

      /// PCBFCN_CI_OnLscTransmitBuffer
      public OnLscTransmitBuffer p14;

      /// Context pointer for PCBFCN_CI_OnLscTransmitBuffer
      public IntPtr p14Context;
    } ;

    /// <summary>
    /// Technotrend: Callback structures
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private unsafe struct TTSlimCallbackStruct
    {
      public OnSlotStatusCallback OnSlotStatus;
      public IntPtr OnSlotStatusContext;
      public OnCaChangeCallback OnCAStatus;
      public IntPtr OnCAStatusContext;
    }

    /// <summary>
    /// Technotrend: Slot info structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public unsafe struct SlotInfo
    {
      /// CI status
      public Byte nStatus;

      /// menu title string
      public IntPtr pMenuTitleString;

      /// cam system ID's
      public UInt16* pCaSystemIDs;

      /// number of cam system ID's
      public UInt16 wNoOfCaSystemIDs;
    }

    #endregion

    #region DLL Imports

    /// <summary>
    /// Technotrend: Open hardware
    /// </summary>
    /// <param name="deviceType"></param>
    /// <param name="deviceIdentifier"></param>
    /// <returns>handle to opened device</returns>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern IntPtr bdaapiOpenHWIdx(TTApiDeviceCat deviceType, UInt32 deviceIdentifier);

    /// <summary>
    /// Technotrend: Open CI
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TTApiResult bdaapiOpenCIWithoutPointer(IntPtr device);

    /// <summary>
    /// Technotrend: Open CI
    /// </summary>
    /// <param name="device"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TTApiResult bdaapiOpenCISlim(IntPtr device, TTSlimCallbackStruct callback);

    /// <summary>
    /// Technotrend: Open CI
    /// </summary>
    /// <param name="device"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TTApiResult bdaapiOpenCI(IntPtr device, TTCallbackStruct callback);

    /// <summary>
    /// Technotrend: Close API
    /// </summary>
    /// <param name="device"></param>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern void bdaapiClose(IntPtr device);

    /// <summary>
    /// Technotrend: Close CI
    /// </summary>
    /// <param name="device"></param>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern void bdaapiCloseCI(IntPtr device);

    /// <summary>
    /// Technotrend: Query slot status
    /// </summary>
    /// <param name="device"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TTApiResult bdaapiCIGetSlotStatus(IntPtr device, byte slot);

    /// <summary>
    /// Technotrend: get driver version
    /// </summary>
    /// <param name="device"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="v4"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TTApiResult bdaapiGetDrvVersion(IntPtr device, ref byte v1, ref byte v2, ref byte v3,
                                                          ref byte v4);

    /// <summary>
    /// Technotrend: decodes PMT
    /// </summary>
    /// <param name="device"></param>
    /// <param name="pNrs"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    [SuppressUnmanagedCodeSecurity]
    private static extern TTApiResult bdaapiCIMultiDecode(IntPtr device, IntPtr pNrs, Int32 count);

    /// <summary>
    /// Technotrend: Diseqc
    /// </summary>
    /// <param name="device"></param>
    /// <param name="data"></param>
    /// <param name="length"></param>
    /// <param name="repeat"></param>
    /// <param name="toneburst"></param>
    /// <param name="polarity"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiSetDiSEqCMsg", CallingConvention = CallingConvention.Cdecl)]
    public static extern TTApiResult bdaapiSetDiSEqCMsg(IntPtr device, IntPtr data, byte length, byte repeat,
                                                        byte toneburst, int polarity);

    /// <summary>
    /// Technotrend: set antenna power
    /// </summary>
    /// <param name="device"></param>
    /// <param name="bAntPwrOnOff"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiSetDVBTAntPwr", CallingConvention = CallingConvention.Cdecl)]
    public static extern TTApiResult bdaapiSetDVBTAntPwr(IntPtr device, bool bAntPwrOnOff);

    /// <summary>
    /// Technotrend: get antenna power
    /// </summary>
    /// <param name="device"></param>
    /// <param name="uiAntPwrOnOff"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiGetDVBTAntPwr", CallingConvention = CallingConvention.Cdecl)]
    public static extern TTApiResult bdaapiGetDVBTAntPwr(IntPtr device, ref int uiAntPwrOnOff);

    /// <summary>
    /// Technotrend: Enter CI menu
    /// </summary>
    /// <param name="device"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiCIEnterModuleMenu",
      CallingConvention = CallingConvention.Cdecl)]
    public static extern TTApiResult bdaapiCIEnterModuleMenu(IntPtr device, byte slot);

    /// <summary>
    /// Technotrend: Select CI menu choice
    /// </summary>
    /// <param name="device"></param>
    /// <param name="slot"></param>
    /// <param name="selection"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiCIMenuAnswer", CallingConvention = CallingConvention.Cdecl)]
    public static extern TTApiResult bdaapiCIMenuAnswer(IntPtr device, byte slot, byte selection);

    /// <summary>
    /// Technotrend: Send CI menu answer
    /// </summary>
    /// <param name="device"></param>
    /// <param name="slot"></param>
    /// <param name="pKeyBuffer"></param>
    /// <param name="nLength"></param>
    /// <returns></returns>
    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiCIAnswer", CallingConvention = CallingConvention.Cdecl)]
    public static extern TTApiResult bdaapiCIAnswer(IntPtr device, byte slot,
                                                    [MarshalAs(UnmanagedType.LPStr)] String pKeyBuffer, byte nLength);

    #endregion

    #region callback delegate definitions

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="nStatus"></param>
    /// <param name="csInfo"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnSlotStatusCallback(IntPtr Context, byte nSlot, byte nStatus, SlotInfo* csInfo);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="nReplyTag"></param>
    /// <param name="wStatus"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnCaChangeCallback(IntPtr Context, byte nSlot, byte nReplyTag, Int16 wStatus);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="pString"></param>
    /// <param name="wLength"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnDisplayString(IntPtr Context, byte nSlot, IntPtr pString, Int16 wLength);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="wItems"></param>
    /// <param name="pStringArray"></param>
    /// <param name="wLength"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnDisplayMenuAndLists(
      IntPtr Context, byte nSlot, Int16 wItems, IntPtr pStringArray, Int16 wLength);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnSwitchOsdOff(IntPtr Context, byte nSlot);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="bBlindAnswer"></param>
    /// <param name="nExpectedLength"></param>
    /// <param name="dwKeyMask"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnInputRequest(
      IntPtr Context, byte nSlot, bool bBlindAnswer, byte nExpectedLength, Int16 dwKeyMask);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="pDescriptor"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnLscSetDescriptor(IntPtr Context, byte nSlot, IntPtr pDescriptor);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnLscConnect(IntPtr Context, byte nSlot);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnLscDisconnect(IntPtr Context, byte nSlot);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="BufferSize"></param>
    /// <param name="Timeout10Ms"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnLscSetParams(IntPtr Context, byte nSlot, byte BufferSize, byte Timeout10Ms);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnLscEnquireStatus(IntPtr Context, byte nSlot);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="PhaseID"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnLscGetNextBuffer(IntPtr Context, byte nSlot, byte PhaseID);

    /// <summary>
    /// Technotrend: Callbacks from CI
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="nSlot"></param>
    /// <param name="PhaseID"></param>
    /// <param name="pData"></param>
    /// <param name="nLength"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnLscTransmitBuffer(
      IntPtr Context, byte nSlot, byte PhaseID, IntPtr pData, Int16 nLength);

    #endregion

    #region Constants

    private const string LBDG2_NAME = "TechnoTrend BDA/DVB Capture";
    private const string LBDG2_NAME_C_TUNER = "TechnoTrend BDA/DVB-C Tuner";
    private const string LBDG2_NAME_S_TUNER = "TechnoTrend BDA/DVB-S Tuner";
    private const string LBDG2_NAME_T_TUNER = "TechnoTrend BDA/DVB-T Tuner";
    private const string LBDG2_NAME_NEW = "ttBudget2 BDA DVB Capture";
    private const string LBDG2_NAME_C_TUNER_NEW = "ttBudget2 BDA DVB-C Tuner";
    private const string LBDG2_NAME_S_TUNER_NEW = "ttBudget2 BDA DVB-S Tuner";
    private const string LBDG2_NAME_T_TUNER_NEW = "ttBudget2 BDA DVB-T Tuner";
    private const string LBUDGET3NAME = "TTHybridTV BDA Digital Capture";
    private const string LBUDGET3NAME_TUNER = "TTHybridTV BDA DVBT Tuner";
    private const string LBUDGET3NAME_ATSC_TUNER = "TTHybridTV BDA ATSC Tuner";
    private const string LBUDGET3NAME_TUNER_ANLG = "TTHybridTV BDA Analog TV Tuner";
    private const string LBUDGET3NAME_ANLG = "TTHybridTV BDA Analog Capture";
    private const string LUSB2BDA_DVB_NAME = "USB 2.0 BDA DVB Capture";
    private const string LUSB2BDA_DSS_NAME = "USB 2.0 BDA DSS Capture";
    private const string LUSB2BDA_DSS_NAME_TUNER = "USB 2.0 BDA DSS Tuner";
    private const string LUSB2BDA_DVB_NAME_C_TUNER = "USB 2.0 BDA DVB-C Tuner";
    private const string LUSB2BDA_DVB_NAME_S_TUNER = "USB 2.0 BDA DVB-S Tuner";
    private const string LUSB2BDA_DVB_NAME_S_TUNER_FAKE = "USB 2.0 BDA (DVB-T Fake) DVB-T Tuner";
    private const string LUSB2BDA_DVB_NAME_T_TUNER = "USB 2.0 BDA DVB-T Tuner";
    private const string LUSB2BDA_DVBS_NAME_PIN = "Pinnacle PCTV 4XXe Capture";
    private const string LUSB2BDA_DVBS_NAME_PIN_TUNER = "Pinnacle PCTV 4XXe Tuner";

    #endregion

    #region Member variables

    private uint m_deviceID;
    private IntPtr m_hBdaApi;
    private byte m_slot;
    private bool m_verboseLogging = false;
    private bool m_ciSlotAvailable = false;
    private int m_ciStatus; // set to -1 when no ca resource
    private int m_caErrorCount; // number of failures to setProgramm
    private int m_waitTimeout;
    private String m_ciDisplayString;
    private TTCiSlotStatus m_slotStatus;
    private TTApiDeviceCat m_deviceType;
    private IBaseFilter m_tunerFilter;
    private DVBSChannel _previousChannel;
    private ICiMenuCallbacks ciMenuCallbacks;
    private readonly IntPtr ptrPmt;
    private readonly IntPtr _ptrDataInstance;


    //TTSlimCallbackStruct slimCallback;
    private TTCallbackStruct fullCallback;

    #endregion

    /// <summary>
    /// constructor for enabling technotrend ci 
    /// </summary>
    /// <param name="tunerFilter">tunerfilter</param>
    public TechnoTrendAPI(IBaseFilter tunerFilter)
    {
      m_deviceID = 0;
      m_slot = 0;
      m_ciStatus = 0;
      m_caErrorCount = 0;
      m_waitTimeout = 0;
      m_ciSlotAvailable = false;
      m_slotStatus = TTCiSlotStatus.SlotUnknownState;
      m_tunerFilter = tunerFilter;
      ptrPmt = Marshal.AllocCoTaskMem(1024); // buffer for handling pmt
      _ptrDataInstance = Marshal.AllocCoTaskMem(1024); // buffer for diseqc messages

      // detect card type
      DetectCardType();
      // if unknown exit
      if (m_deviceType == TTApiDeviceCat.UNKNOWN) return;

      // enumerate device id for opening hw
      GetDeviceID();

      // OpenCI
      OpenCI();
    }

    /// <summary>
    /// Opens the CI API
    /// </summary>
    public void OpenCI()
    {
      m_hBdaApi = bdaapiOpenHWIdx(m_deviceType, m_deviceID);
      if (m_hBdaApi.ToInt32() == -1)
      {
        Log.Log.Debug("TechnoTrend: unable to open the device");
        return;
      }

      if (m_verboseLogging) Log.Log.Debug("TechnoTrend: OpenHWIdx succeeded");
      GetDrvVersion();

      // assign callback functions
      fullCallback.p01Context = fullCallback.p02Context = fullCallback.p03Context = fullCallback.p04Context =
                                                                                    fullCallback.p05Context =
                                                                                    fullCallback.p06Context =
                                                                                    fullCallback.p07Context =
                                                                                    fullCallback.p08Context =
                                                                                    fullCallback.p09Context =
                                                                                    fullCallback.p10Context =
                                                                                    fullCallback.p11Context =
                                                                                    fullCallback.p12Context =
                                                                                    fullCallback.p13Context =
                                                                                    fullCallback.p14Context = m_hBdaApi;
      fullCallback.p01 = onSlotChange;
      fullCallback.p02 = onCaChange;
      fullCallback.p03 = onDisplayString;
      fullCallback.p04 = onDisplayMenuOrList;
      fullCallback.p05 = onDisplayMenuOrList; // same function for list/menu
      fullCallback.p06 = onSwitchOsdOff;
      fullCallback.p07 = onInputRequest;
      fullCallback.p08 = onLscSetDescriptor;
      fullCallback.p09 = onLscConnect;
      fullCallback.p10 = onLscDisconnect;
      fullCallback.p11 = onLscSetParams;
      fullCallback.p12 = onLscEnquireStatus;
      fullCallback.p13 = onLscGetNextBuffer;
      fullCallback.p14 = onLscTransmitBuffer;

      // open ci hardware 
      TTApiResult result = bdaapiOpenCI(m_hBdaApi, fullCallback);
      if (result == TTApiResult.Success)
      {
        m_ciSlotAvailable = true;
        m_caErrorCount = 0; // (re)set error counter
        Log.Log.Debug("TechnoTrend: OpenCI succeeded");
      }
      else
      {
        Log.Log.Debug("TechnoTrend: no CI detected: {0}", result);
      }
    }

    /// <summary>
    /// Closes the CI API
    /// </summary>
    public void CloseCI()
    {
      // if hw was opened before, close it now
      if (m_hBdaApi.ToInt32() != -1)
      {
        if (m_ciSlotAvailable)
        {
          Log.Log.Debug("TechnoTrend: Closing CI");
          bdaapiCloseCI(m_hBdaApi);
        }
        Log.Log.Debug("TechnoTrend: Closing hardware");
        bdaapiClose(m_hBdaApi);
      }
    }

    /// <summary>
    /// Reset the CI
    /// </summary>
    public void ResetCI()
    {
      CloseCI();
      OpenCI();
    }

    /// <summary>
    /// Query slot status from card
    /// </summary>
    private void GetCISlotStatus()
    {
      TTApiResult result = bdaapiCIGetSlotStatus(m_hBdaApi, m_slot);
      if (result != TTApiResult.Success)
      {
        Log.Log.Debug("TechnoTrend: bdaapiCIGetSlotStatus failed {0}", result);
      }
    }

    /// <summary>
    /// returns driver version number
    /// </summary>
    /// <returns></returns>
    public TTApiResult GetDrvVersion()
    {
      TTApiResult hr;
      byte v1 = 0, v2 = 0, v3 = 0, v4 = 0;

      hr = bdaapiGetDrvVersion(m_hBdaApi, ref v1, ref v2, ref v3, ref v4);
      if (hr != TTApiResult.Success)
      {
        Log.Log.Debug("TechnoTrend: bdaapiGetDrvVersion failed {0}", hr);
      }
      else
      {
        Log.Log.Debug("TechnoTrend: initalized id:{0}, driver version:{1}.{2}.{3}.{4}", m_deviceID, v1, v2, v3, v4);
      }

      return TTApiResult.Success;
    }


    /// <summary>
    ///**************************************************************************************************
    ///* GetDeviceID()
    ///* Finds the technotrend device Id for the tuner filter
    ///* returns true if device id is found otherwise false
    ///**************************************************************************************************
    /// </summary>
    /// <returns></returns>
    private bool GetDeviceID()
    {
      m_deviceID = 0;

      IKsPin pKsPin = DsFindPin.ByDirection(m_tunerFilter, PinDirection.Output, 0) as IKsPin;
      if (pKsPin != null)
      {
        IntPtr raw;

        // Request the raw data
        int hr = pKsPin.KsQueryMediums(out raw);
        try
        {
          // Load the number of media
          int countRecPin = Marshal.ReadInt32(raw, 4);
          RegPinMedium rpm;

          // Load it all
          for (int i = 0, s = Marshal.SizeOf(typeof (RegPinMedium)); i < countRecPin; i++)
          {
            // Get the reference
            IntPtr addr = new IntPtr(raw.ToInt32() + 8 + s * i);
            // Reconstruct
            rpm = (RegPinMedium)Marshal.PtrToStructure(addr, typeof (RegPinMedium));
            m_deviceID = (uint)rpm.dw1;
          }
          // Report
          return true;
        }
        finally
        {
          // Cleanup native memory
          if (IntPtr.Zero != raw) Marshal.FreeCoTaskMem(raw);
        }
      }
      return true;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is TechnoTrend.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is TechnoTrend; otherwise, <c>false</c>.
    /// </value>
    public bool IsTechnoTrend
    {
      get { return (m_deviceType != TTApiDeviceCat.UNKNOWN); }
    }

    /// <summary>
    /// Determines whether cam is ready.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam ready]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      bool yesNo = false;
      if (m_ciSlotAvailable && (m_slotStatus == TTCiSlotStatus.SlotCaOk || m_slotStatus == TTCiSlotStatus.SlotModuleOk))
      {
        yesNo = true;
      }
      if (m_verboseLogging) Log.Log.Debug("TechnoTrend: IsCamReady: {0}", yesNo);
      return yesNo;
    }

    /// <summary>
    /// Determines whether cam is ready.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam ready]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      bool yesNo = true;
      if (m_ciSlotAvailable == false || m_slotStatus == TTCiSlotStatus.SlotEmpty ||
          m_slotStatus == TTCiSlotStatus.SlotUnknownState)
      {
        yesNo = false;
      }
      if (m_verboseLogging) Log.Log.Debug("TechnoTrend: IsCamPresent: {0}", yesNo);
      return yesNo;
    }

    /// <summary>
    /// detects card type based on tunerfilter info
    /// </summary>
    /// <returns></returns>
    private void DetectCardType()
    {
      FilterInfo info;
      if (m_tunerFilter.QueryFilterInfo(out info) == 0)
      {
        switch (info.achName)
        {
          case LBDG2_NAME_C_TUNER:
          case LBDG2_NAME_S_TUNER:
          case LBDG2_NAME_T_TUNER:
          case LBDG2_NAME_C_TUNER_NEW:
          case LBDG2_NAME_S_TUNER_NEW:
          case LBDG2_NAME_T_TUNER_NEW:
            m_deviceType = TTApiDeviceCat.BUDGET_2;
            break;
          case LBUDGET3NAME_TUNER:
          case LBUDGET3NAME_ATSC_TUNER:
            //case LBUDGET3NAME_TUNER_ANLG:
            m_deviceType = TTApiDeviceCat.BUDGET_3;
            break;
          case LUSB2BDA_DVB_NAME_C_TUNER:
          case LUSB2BDA_DVB_NAME_S_TUNER:
          case LUSB2BDA_DVB_NAME_T_TUNER:
            m_deviceType = TTApiDeviceCat.USB_2;
            break;
          case LUSB2BDA_DVBS_NAME_PIN_TUNER:
            m_deviceType = TTApiDeviceCat.USB_2_PINNACLE;
            break;
          default:
            m_deviceType = TTApiDeviceCat.UNKNOWN;
            break;
        }
      }
    }

    /// <summary>
    /// decodes multiple programs
    /// </summary>
    /// <param name="subChannels">list of subchannels</param>
    /// <returns>true is successful</returns>
    public bool DescrambleMultiple(Dictionary<int, ConditionalAccessContext> subChannels)
    {
      bool succeeded = false;
      TTApiResult result;

      // if OpenCI failed, there's no CI ergo no CAM
      if (m_ciSlotAvailable == false)
      {
        if (m_verboseLogging) Log.Log.Debug("TechnoTrend: DescrambleMultiple: no CI present");
        succeeded = true;
        return succeeded;
      }

      m_ciStatus = -1;
      GetCISlotStatus();

      List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();
      Dictionary<int, ConditionalAccessContext>.Enumerator en = subChannels.GetEnumerator();

      while (en.MoveNext())
      {
        bool exists = false;
        ConditionalAccessContext context = en.Current.Value;
        foreach (ConditionalAccessContext c in filteredChannels)
        {
          if (c.Channel.Equals(context.Channel))
            exists = true;
        }
        if (!exists && context.ServiceId != 0) // also check for sid != 0, otherwise TT API fails
        {
          filteredChannels.Add(context);
        }
      }

      Log.Log.Debug("TechnoTrend: DescrambleMultiple:({0})", filteredChannels.Count);
      for (int i = 0; i < filteredChannels.Count; ++i)
      {
        ConditionalAccessContext context = filteredChannels[i];
        Log.Log.Debug("TechnoTrend: DescrambleMultiple: serviceId:{0}", context.ServiceId);
        Marshal.WriteInt16(ptrPmt, 2 * i, (short)context.ServiceId);
      }

      if (m_slotStatus == TTCiSlotStatus.SlotCaOk || m_slotStatus == TTCiSlotStatus.SlotModuleOk)
        // || m_slotStatus==CI_SLOT_DBG_MSG)
      {
        result = bdaapiCIMultiDecode(m_hBdaApi, ptrPmt, (short)filteredChannels.Count);
        if (result == TTApiResult.Success)
        {
          if (m_ciStatus == 1)
          {
            succeeded = true;
            Log.Log.Debug("TechnoTrend: services decoded:{0} {1}", result, m_ciStatus);
          }
          else
          {
            succeeded = false;
            Log.Log.Debug("TechnoTrend: services not decoded:{0} ciStatus: {1}", result, m_ciStatus);
          }
        }
        else
        {
          Log.Log.Debug("TechnoTrend: services not decoded:{0}", result);
        }
      }
      else if (m_slotStatus == TTCiSlotStatus.SlotUnknownState)
      {
        if (m_waitTimeout == 0)
        {
          //no CAM inserted
          Log.Log.Debug("TechnoTrend: CI slot state unknown, allow one retry");
          succeeded = false; // to allow retry from ConditionalAccess
        }
        else
        {
          //still no valid state? don't try next time!
          Log.Log.Debug("TechnoTrend: CI slot state still unknown after one retry. Stop trying.");
          succeeded = true; // to allow retry from ConditionalAccess
        }
        m_waitTimeout++;
      }
      else if (m_slotStatus == TTCiSlotStatus.SlotModuleInserted)
      {
        Log.Log.Debug("TechnoTrend: CI module inserted but not yet ready");
        succeeded = false; // to allow retry from ConditionalAccess
      }
      else if (m_slotStatus == TTCiSlotStatus.SlotEmpty)
      {
        Log.Log.Debug("TechnoTrend: no cam detected, slot empty");
        succeeded = true; //no CAM inserted, no retry
      }
      return succeeded;
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scanparameters.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_previousChannel != null)
      {
        if (_previousChannel.Frequency == channel.Frequency &&
            _previousChannel.DisEqc == channel.DisEqc &&
            _previousChannel.Polarisation == channel.Polarisation)
        {
          Log.Log.WriteFile("TechnoTrend: already tuned to diseqc:{0}, frequency:{1}, polarisation:{2}", channel.DisEqc,
                            channel.Frequency, channel.Polarisation);
          return;
        }
      }
      _previousChannel = channel;
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      //"01,02,03,04,05,06,07,08,09,0a,0b,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,"	
      Marshal.WriteByte(_ptrDataInstance, 0, 0xE0); //diseqc command 1. uFraming=0xe0
      Marshal.WriteByte(_ptrDataInstance, 1, 0x10); //diseqc command 1. uAddress=0x10
      Marshal.WriteByte(_ptrDataInstance, 2, 0x38); //diseqc command 1. uCommand=0x38
      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);
      Log.Log.WriteFile(
        "TechnoTrend SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2}, polarisation:{3} hiband:{4}",
        channel.DisEqc, antennaNr, channel.Frequency, channel.Polarisation, hiBand);
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                           (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);
      Marshal.WriteByte(_ptrDataInstance, 3, cmd);
      bdaapiSetDiSEqCMsg(m_hBdaApi, _ptrDataInstance, 4, 1, 0, (short)channel.Polarisation);
      Log.Log.Info("TechnoTrend: Diseqc Command Send");
    }

    /// <summary>
    /// Here we turn on the USB DVB-T antennae
    /// </summary>
    /// <param name="onOff">true for on</param>
    public void EnableAntenna(bool onOff)
    {
      int uiAntPwrOnOff = 0;
      string Get5vAntennae = "Disabled";
      Log.Log.Info("Setting TechnoTrend DVB-T 5v Antennae Power enabled:{0}", onOff);
      bdaapiSetDVBTAntPwr(m_hBdaApi, onOff);
      bdaapiGetDVBTAntPwr(m_hBdaApi, ref uiAntPwrOnOff);
      if (uiAntPwrOnOff == 0)
      {
        Get5vAntennae = "Disabled";
      }
      if (uiAntPwrOnOff == 1)
      {
        Get5vAntennae = "Enabled";
      }
      if (uiAntPwrOnOff == 2)
      {
        Get5vAntennae = "Not Connected";
      }
      Log.Log.Info("TechnoTrend DVB-T 5v Antennae status:{0}", Get5vAntennae);
    }

    #region callback handlers

    /// <summary>
    /// callback from driver for CI slot status
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="nStatus"></param>
    /// <param name="csInfo"></param>
    public unsafe void onSlotChange(
      IntPtr Context,
      byte nSlot,
      byte nStatus,
      SlotInfo* csInfo
      )
    {
      try
      {
        m_slotStatus = (TTCiSlotStatus)nStatus;
        Log.Log.Debug("TechnoTrend: slot {0} changed", nSlot);
        if (csInfo != null)
        {
          Log.Log.Debug("TechnoTrend:    CI status:{0} ", m_slotStatus);
          if (csInfo->pMenuTitleString != null)
          {
            Log.Log.Debug("TechnoTrend:    CI text  :{0} ", Marshal.PtrToStringAnsi(csInfo->pMenuTitleString));
          }

          for (int i = 0; i < csInfo->wNoOfCaSystemIDs; ++i)
          {
            Log.Log.Debug("TechnoTrend:      ca system id  :{0:X} ", csInfo->pCaSystemIDs[i]);
          }
        }
      }
      catch (Exception)
      {
        Log.Log.Debug("TechnoTrend: OnSlotChange() exception");
      }
    }

    /// <summary>
    /// callback from driver for CA changes
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="nReplyTag"></param>
    /// <param name="wStatus"></param>
    public unsafe void onCaChange(IntPtr Context, byte nSlot, byte nReplyTag, Int16 wStatus)
    {
      try
      {
        Log.Log.Debug("$ OnCaChange slot:{0} reply:{1:X} status:{2}", nSlot, nReplyTag, wStatus);
        switch (nReplyTag)
        {
          case 0x0C: //CI_PSI_COMPLETE:
            Log.Log.Debug("$ CI: ### Number of programs : {0}", wStatus);
            break;

          case 0x0D: //CI_MODULE_READY:
            Log.Log.Debug("$ CI: CI_MODULE_READY in OnCAStatus not supported");
            break;
          case 0x0E: //CI_SWITCH_PRG_REPLY:
            {
              switch (wStatus)
              {
                case 4: //ERR_INVALID_DATA:
                  Log.Log.Debug("$ CI: ERROR::SetProgram failed !!! (invalid PNR)");
                  break;
                case 5: //ERR_NO_CA_RESOURCE:
                  Log.Log.Debug("$ CI: ERROR::SetProgram failed !!! (no CA resource available)");
                  m_ciStatus = -1; // not ready
                  m_caErrorCount++; // count the errors to allow reset
                  break;
                case 0: //ERR_NONE:
                  Log.Log.Debug("$ CI:    SetProgram OK");
                  m_ciStatus = 1;
                  m_caErrorCount = 0; // reset counter
                  break;
                default:
                  break;
              }
            }
            break;
          default:
            break;
        }
        // if setProgram failed twice, reset the CAM
        // ATTEBNTION: DOESN'T WORK and crashes graph / BaseFilter
        //if (m_ciStatus == -1 && m_caErrorCount >= 1)
        //{
        //  Log.Log.Info("SetProgram failed {0} times because of no CA resource. Resetting CI now.", m_caErrorCount);
        //  ResetCI();
        //}
      }
      catch (Exception e)
      {
        Log.Log.Debug("TechnoTrend: OnCaChange() exception: {0}", e.ToString());
      }
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="pString"></param>
    /// <param name="wLength"></param>
    public unsafe void onDisplayString(IntPtr Context, byte nSlot, IntPtr pString, Int16 wLength)
    {
      try
      {
        m_ciDisplayString = Marshal.PtrToStringAnsi(pString, wLength);
        Log.Log.Debug("TechnoTrend:OnDisplayString slot:{0} {1}", nSlot, m_ciDisplayString);
      }
      catch (Exception e)
      {
        Log.Log.Debug("TechnoTrend: OnDisplayString() exception: {0}", e.ToString());
      }
    }


    /// <summary>
    /// callback from driver to display the CI menu
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="wItems">Number of Items in the List</param>
    /// <param name="pStringArray">Contains all strings of the list.</param>
    /// <param name="wLength">Length of the string array.</param>
    public unsafe void onDisplayMenuOrList(IntPtr Context, byte nSlot, Int16 wItems, IntPtr pStringArray, Int16 wLength)
    {
      try
      {
        Log.Log.Debug("TechnoTrend: OnDisplayMenu/List; {0} items; wLength: {1}; pStringArray: {2:x} ", wItems, wLength,
                      pStringArray);
        // construct all strings for callback
        StringBuilder[] Entries = new StringBuilder[wItems];
        int idx = 0;
        byte charChode;
        for (int i = 0; i < wLength - 1; ++i) // wLength-1 --> last char is a 0, avoid one additional loop and callback
        {
          charChode = Marshal.ReadByte((IntPtr)(pStringArray.ToInt32() + i));
          if (Entries[idx] == null) Entries[idx] = new StringBuilder();
          if (charChode != 0) // we don't need \0 at end of string
          {
            Entries[idx].Append((char)charChode);
          }
          else // if string ends before maxlength
          {
            Log.Log.Debug("TechnoTrend: {0}: {1} ", idx, Entries[idx].ToString());
            // is title part finished?
            if (ciMenuCallbacks != null)
            {
              if (idx == 2)
              {
                ciMenuCallbacks.OnCiMenu(Entries[0].ToString(), Entries[1].ToString(), Entries[2].ToString(), wItems - 3);
                  //-3 header lines
              }
              if (idx > 2)
              {
                ciMenuCallbacks.OnCiMenuChoice(idx - 3, Entries[idx].ToString()); // current line as option
              }
            }
            idx++; // next line
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("TechnoTrend: OnDisplayMenu() exception: {0}", ex.ToString());
      }
    }


    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    public unsafe void onSwitchOsdOff(IntPtr Context, byte nSlot)
    {
      Log.Log.Debug("TechnoTrend:CI_OnSwitchOsdOff slot:{0}", nSlot);
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="bBlindAnswer">True if hidden input (*)</param>
    /// <param name="nExpectedLength">Expected max. answer length</param>
    /// <param name="dwKeyMask">Key mask</param>
    public unsafe void onInputRequest(IntPtr Context, byte nSlot, bool bBlindAnswer, byte nExpectedLength,
                                      Int16 dwKeyMask)
    {
      Log.Log.Debug("TechnoTrend: OnInputRequest; bBlindAnswer {0}, nExpectedLength: {1}; dwKeyMask: {2} ", bBlindAnswer,
                    nExpectedLength, dwKeyMask);
      if (ciMenuCallbacks != null)
      {
        ciMenuCallbacks.OnCiRequest(bBlindAnswer, nExpectedLength, m_ciDisplayString);
          // m_ciDisplayString from former callback!
      }
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="pDescriptor">Descriptor</param>
    public unsafe void onLscSetDescriptor(IntPtr Context, byte nSlot, IntPtr pDescriptor)
    {
      Log.Log.Debug("TechnoTrend:OnLscSetDescriptor slot:{0}", nSlot);
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    public unsafe void onLscConnect(IntPtr Context, byte nSlot)
    {
      Log.Log.Debug("TechnoTrend:OnLscConnect slot:{0}", nSlot);
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    public unsafe void onLscDisconnect(IntPtr Context, byte nSlot)
    {
      Log.Log.Debug("TechnoTrend:OnLscDisconnect slot:{0}", nSlot);
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="BufferSize">Buffer size</param>
    /// <param name="Timeout10Ms">Timeout</param>
    public unsafe void onLscSetParams(IntPtr Context, byte nSlot, byte BufferSize, byte Timeout10Ms)
    {
      Log.Log.Debug("TechnoTrend:OnLscSetParams slot:{0}", nSlot);
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    public unsafe void onLscEnquireStatus(IntPtr Context, byte nSlot)
    {
      Log.Log.Debug("TechnoTrend:OnLscEnquireStatus slot:{0}", nSlot);
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="PhaseID">Phase</param>
    public unsafe void onLscGetNextBuffer(IntPtr Context, byte nSlot, byte PhaseID)
    {
      Log.Log.Debug("TechnoTrend:OnLscGetNextBuffer slot:{0}", nSlot);
    }

    /// <summary>
    /// callback from driver
    /// </summary>
    /// <param name="Context">Can be used for a context pointer in the calling application. This parameter can be NULL.</param>
    /// <param name="nSlot">Is the Slot ID.</param>
    /// <param name="PhaseID">Phase</param>
    /// <param name="pData">Data</param>
    /// <param name="nLength">Length</param>
    public unsafe void onLscTransmitBuffer(IntPtr Context, byte nSlot, byte PhaseID, IntPtr pData, Int16 nLength)
    {
      Log.Log.Debug("TechnoTrend:OnLscTransmitBuffer slot:{0}", nSlot);
    }

    #endregion

    #region IDisposable Member

    /// <summary>
    /// Disposes TT API
    /// </summary>
    public void Dispose()
    {
      CloseCI();
      Marshal.FreeCoTaskMem(ptrPmt);
      Marshal.FreeCoTaskMem(_ptrDataInstance);
    }

    #endregion

    #region IDiSEqCController Members

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      if (!IsTechnoTrend) return false;

      for (int i = 0; i < diSEqC.Length; ++i)
        Marshal.WriteByte(_ptrDataInstance, i, diSEqC[i]);
      Polarisation pol = Polarisation.LinearV;
      if (_previousChannel != null)
        pol = _previousChannel.Polarisation;
      bdaapiSetDiSEqCMsg(m_hBdaApi, _ptrDataInstance, (byte)diSEqC.Length, 1, 0, (short)pol);
      return true;
    }

    /// <summary>
    /// gets the diseqc reply
    /// </summary>
    /// <param name="reply">The reply.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      reply = null;
      return false;
    }

    #endregion

    #region ICiMenuActions Member

    /// <summary>
    /// Sets the callback handler
    /// </summary>
    /// <param name="ciMenuHandler"></param>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        ciMenuCallbacks = ciMenuHandler;
        return true;
      }
      return false;
    }


    /// <summary>
    /// Enters the CI menu
    /// </summary>
    /// <returns></returns>
    public bool EnterCIMenu()
    {
      Log.Log.Debug("TechnoTrend: Enter CI Menu");
      if (bdaapiCIEnterModuleMenu(m_hBdaApi, 0) != TTApiResult.Success)
      {
        Log.Log.Debug("TechnoTrend: bdaapiCIEnterModuleMenu failed.");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Closes the CI menu
    /// </summary>
    /// <returns></returns>
    public bool CloseCIMenu()
    {
      Log.Log.Debug("TechnoTrend: Close CI Menu not yet implemented");
      return true;
    }

    /// <summary>
    /// Selects a CI menu entry
    /// </summary>
    /// <param name="choice"></param>
    /// <returns></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Log.Debug("TechnoTrend: Select CI Menu entry {0}", choice);
      if (bdaapiCIMenuAnswer(m_hBdaApi, m_slot, choice) != TTApiResult.Success)
      {
        Log.Log.Debug("TechnoTrend: bdaapiCIMenuAnswer  failed");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Sends an answer after CI request
    /// </summary>
    /// <param name="Cancel"></param>
    /// <param name="Answer"></param>
    /// <returns></returns>
    public bool SendMenuAnswer(bool Cancel, string Answer)
    {
      if (Answer == null) Answer = "";
      Log.Log.Debug("TechnoTrend: Send Menu Answer: {0}, Cancel: {1}", Answer, Cancel);
      if (bdaapiCIAnswer(m_hBdaApi, 0, Answer, (byte)Answer.Length) != TTApiResult.Success)
      {
        Log.Log.Debug("TechnoTrend: SendMenuAnswer failed.");
        return false;
      }
      return true;
    }

    #endregion
  }
}