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
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// KNC CI control class
  /// </summary>
  public class KNCAPI : IDisposable, ICiMenuActions
  {
    #region Dll Imports

    /// <summary>
    /// KNC: Enable CI
    /// </summary>
    /// <param name="m_iDeviceIndex">device index 0..n</param>
    /// <param name="tunerFilter">tuner filter</param>
    /// <param name="callbacks">callback pointer struct</param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_Enable", CharSet=CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_Enable(uint m_iDeviceIndex, IBaseFilter tunerFilter,  IntPtr callbacks);
    /// <summary>
    /// KNC: Disable CI
    /// </summary>
    /// <param name="m_iDeviceIndex">device index 0..n</param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_Disable", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_Disable(uint m_iDeviceIndex);
    /// <summary>
    /// KNC: Detect if CI is available
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_IsAvailable", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_IsAvailable(uint m_iDeviceIndex);
    /// <summary>
    /// KNC: Detect if CI is ready
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_IsReady", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_IsReady(uint m_iDeviceIndex);
    /// <summary>
    /// KNC: Enable CI Hardware ???
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_HW_Enable", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_HW_Enable(uint m_iDeviceIndex, bool param);
    /// <summary>
    /// KNC: Query CAM name
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="Name"></param>
    /// <param name="BufferSize"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_GetName", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_GetName(uint m_iDeviceIndex, [MarshalAs(UnmanagedType.LPStr)] StringBuilder Name, uint BufferSize);
    /// <summary>
    /// KNC: Send caPMT to CAM
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="caPMT"></param>
    /// <param name="caPmtLen"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_SendPMTCommand", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_SendPMTCommand(uint m_iDeviceIndex, IntPtr caPMT, uint caPmtLen);
    /// <summary>
    /// KNC: Enter CI menu
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="nSlot"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_EnterMenu", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_EnterMenu(uint m_iDeviceIndex, byte nSlot);
    /// <summary>
    /// KNC: Select CI menu choice
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="nSlot"></param>
    /// <param name="nChoice"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_SelectMenu", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_SelectMenu(uint m_iDeviceIndex, byte nSlot, byte nChoice);
    /// <summary>
    /// KNC: Close CI menu
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="nSlot"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_CloseMenu", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_CloseMenu(uint m_iDeviceIndex, byte nSlot);
    /// <summary>
    /// KNC: Send CI menu answer
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="nSlot"></param>
    /// <param name="cancel"></param>
    /// <param name="MenuAnswer"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_SendMenuAnswer", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint KNCBDA_CI_SendMenuAnswer(uint m_iDeviceIndex, byte nSlot, bool cancel, [In, MarshalAs(UnmanagedType.LPStr)] String MenuAnswer);
    /// <summary>
    /// KNC: Enable hardware ???
    /// </summary>
    /// <param name="m_iDeviceIndex"></param>
    /// <param name="tunerFilter"></param>
    /// <returns></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_HW_Enable", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern uint KNCBDA_HW_Enable(uint m_iDeviceIndex, IBaseFilter tunerFilter);

    #endregion

    #region enums
    /// <summary>
    /// Status for CI Slot
    /// </summary>
    public enum KNCCiSlotStatus 
    {
      /// Initializing
      Initializing      = 0,
      /// Transport
      Transport         = 1,
      /// Resource
      Resource          = 2,
      /// Application
      Application       = 3,
      /// ConditionalAccess
      ConditionalAccess =	4,
      /// Ready
      Ready             = 5,
      /// OpenService
      OpenService       = 6,
      /// Releasing
      Releasing         = 7,
      /// CloseMMI
      CloseMMI          = 8,
      /// Request
      Request           = 9,
      /// Menu
      Menu              = 10,
      /// MenuChoice
      MenuChoice        = 11,
      /// OpenDisplay
      OpenDisplay       = 12,
      /// CloseDisplay
      CloseDisplay      = 13,
      /// None
      None              = 99
    }
    #endregion

    #region constants

    const string KNC1DVBSTuner = "KNC BDA DVB-S";
    const string KNC1DVBS2Tuner = "KNC BDA DVB-S2";
    const string KNC1DVBCTuner = "KNC BDA DVB-C";
    const string KNC1DVBTTuner = "KNC BDA DVB-T";

    #endregion

    #region callbacks

    /// <summary>
    /// CI MENU CALLBACK STRUCT 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public unsafe struct KNCCiCallbacks
    {
      /// context 
      public UInt32 pParam;
      /// delegate for CI state callback
      [MarshalAs(UnmanagedType.FunctionPtr)] public OnKncCiState         onCiState;
      /// delegate for opening display
      [MarshalAs(UnmanagedType.FunctionPtr)] public OnKncCiOpenDisplay   onOpenDisplay;
      /// delegate for CI menu
      [MarshalAs(UnmanagedType.FunctionPtr)] public OnKncCiMenu          onCiMenu;
      /// delegate for CI menu choices
      [MarshalAs(UnmanagedType.FunctionPtr)] public OnKncCiMenuChoice    onCiMenuChoice;
      /// deletgate for CI requests
      [MarshalAs(UnmanagedType.FunctionPtr)] public OnKncCiRequest       onRequest;
      /// delegate for closing CI
      [MarshalAs(UnmanagedType.FunctionPtr)] public OnKncCiCloseDisplay  onCloseDisplay;	
    };
    /// <summary>
    /// KNC: Callbacks from CI
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="State"></param>
    /// <param name="lpszMessage"></param>
    /// <param name="pParam"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnKncCiState(byte slot, KNCCiSlotStatus State, [MarshalAs(UnmanagedType.LPStr)] String lpszMessage, IntPtr pParam);
    
    /// <summary>
    /// KNC: Callbacks from CI
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="pParam"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnKncCiOpenDisplay(byte slot, IntPtr pParam);
    
    /// <summary>
    /// KNC: Callbacks from CI
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="lpszTitle"></param>
    /// <param name="lpszSubTitle"></param>
    /// <param name="lpszBottom"></param>
    /// <param name="nNumChoices"></param>
    /// <param name="pParam"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnKncCiMenu(byte slot, [MarshalAs(UnmanagedType.LPStr)] String lpszTitle,
      [MarshalAs(UnmanagedType.LPStr)] String lpszSubTitle,
      [MarshalAs(UnmanagedType.LPStr)] String lpszBottom, 
      uint nNumChoices, IntPtr pParam);
    /// <summary>
    /// KNC: Callbacks from CI
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="nChoice"></param>
    /// <param name="lpszText"></param>
    /// <param name="pParam"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnKncCiMenuChoice(byte slot, uint nChoice, [MarshalAs(UnmanagedType.LPStr)] String lpszText, IntPtr pParam);
    
    /// <summary>
    /// KNC: Callbacks from CI
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="bBlind"></param>
    /// <param name="nAnswerLength"></param>
    /// <param name="lpszText"></param>
    /// <param name="pParam"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnKncCiRequest(byte slot, bool bBlind, uint nAnswerLength, [MarshalAs(UnmanagedType.LPStr)] String lpszText, IntPtr pParam);
    
    /// <summary>
    /// KNC: Callbacks from CI
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="nDelay"></param>
    /// <param name="pParam"></param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void OnKncCiCloseDisplay(byte slot, uint nDelay, IntPtr pParam);
    #endregion

    #region variables

    readonly IntPtr ptrPmt;
    readonly IntPtr ptrCallback;
    readonly IntPtr _ptrDataInstance;
    private   IBaseFilter   m_tunerFilter;
    uint m_iDeviceIndex;
    bool m_bIsKNC           = false;
    bool m_bCAM_present     = false;
    byte m_nSlot            = 0;
    int  m_waitTimeout      = 0;
    private KNCCiSlotStatus  m_ciState;
    private KNCCiCallbacks   m_callbacks;
    private ICiMenuCallbacks m_ciMenuCallback;

    #endregion  
    /// <summary>
    /// Initializes a new instance of the <see cref="KNCAPI"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="DeviceIndex">The KNC1 card hardware index (0 based)</param>
    public KNCAPI(IBaseFilter tunerFilter, uint DeviceIndex)
    {

      ptrPmt            = Marshal.AllocCoTaskMem(1024);
      ptrCallback       = Marshal.AllocCoTaskMem(7*4); // 7*Int32
      _ptrDataInstance  = Marshal.AllocCoTaskMem(1024);

      m_tunerFilter     = tunerFilter;

      FilterInfo info;
      tunerFilter.QueryFilterInfo(out info);
      if ((info.achName == KNC1DVBSTuner) ||
          (info.achName == KNC1DVBS2Tuner) ||
          (info.achName == KNC1DVBTTuner) ||
          (info.achName == KNC1DVBCTuner))
      {  
        m_bIsKNC=true;
      }
      else
      {
        return;
      }

      // iDeviceIndex passed by TvLibrary ! Enumerated by DevicePath
      m_iDeviceIndex = DeviceIndex;
      Log.Log.Debug("KNC: card {0} detected: {1}", m_iDeviceIndex, info.achName);

      OpenCI();
    }

    /// <summary>
    /// Opens the CI API
    /// </summary>
    public void OpenCI()
    {
      // HW Enable (always?) succeeds
      if (KNCBDA_HW_Enable(m_iDeviceIndex, m_tunerFilter) == 0)
      {
        Log.Log.Debug("KNC: card {0} HW Enable failed", m_iDeviceIndex);
      }

      m_callbacks                 = new KNCCiCallbacks();
      m_callbacks.onCiMenu        = OnCiMenu;
      m_callbacks.onCiMenuChoice  = OnCiMenuChoice;
      m_callbacks.onCiState       = OnCiState;
      m_callbacks.onCloseDisplay  = OnCiCloseDisplay;
      m_callbacks.onOpenDisplay   = OnCiOpenDisplay;
      m_callbacks.onRequest       = OnCiRequest;
      m_callbacks.pParam          = 0;

      unsafe
      {
        // prepare structure to be passed as pointer
        Marshal.StructureToPtr(m_callbacks, ptrCallback, false);
        if (KNCBDA_CI_Enable(m_iDeviceIndex, m_tunerFilter, ptrCallback) != 0)
        {
          ////CI_HW_Enable seems to always succeed ?!
          //if (KNCBDA_CI_HW_Enable(m_iDeviceIndex, true) == 0)
          //  Log.Log.Debug("KNC: card {0} CI HW enable FAILED!", m_iDeviceIndex);

          // CI Enable succeeds always, only if CAM isReady there is a slot/CAM
          if (KNCBDA_CI_IsReady(m_iDeviceIndex) != 0)
          {
            Log.Log.Debug("KNC: card {0} CI slot enabled successfully", m_iDeviceIndex);

            // remember CAM is present
            m_bCAM_present = true;
            // init state to ready (callbacks are only done when changing, so expect it's ready first)
            m_ciState = KNCCiSlotStatus.Ready;

            StringBuilder nameBuffer = new StringBuilder(100);
            if (KNCBDA_CI_GetName(m_iDeviceIndex, nameBuffer, (uint)nameBuffer.MaxCapacity) != 0)
              Log.Log.Debug("KNC: card {0} CAM Type: {1}", m_iDeviceIndex, nameBuffer);
            else
              Log.Log.Debug("KNC: CI_GetName failed.");
          }
          else
          {
            Log.Log.Debug("KNC: card {0} detected without CAM", m_iDeviceIndex);
          }
        }
      }
    }

    /// <summary>
    /// Closes the CI API
    /// </summary>
    public void CloseCI()
    {
      Log.Log.Debug("KNC: Disable CI");
      KNCBDA_CI_Disable(m_iDeviceIndex);
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
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      CloseCI();
      Log.Log.Debug("KNC: Disposing CI handler");
      Marshal.FreeCoTaskMem(ptrPmt);
      Marshal.FreeCoTaskMem(ptrCallback);
      Marshal.FreeCoTaskMem(_ptrDataInstance);
    }

    /// <summary>
    /// Determines whether cam is ready.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam ready]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      bool yesNo = (KNCBDA_CI_IsReady(m_iDeviceIndex) != 0);
      Log.Log.Info("KNC: IsCAMReady {0}", yesNo);
      return yesNo;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a KNC card.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is a KNC card; otherwise, <c>false</c>.
    /// </value>
    public bool IsKNC
    {
      get
      {
        Log.Log.Info("KNC: IsKNC {0}", m_bIsKNC);
        return m_bIsKNC;
      }
    }

    /// <summary>
    /// Sends the PMT.
    /// </summary>
    /// <param name="pmt">The PMT.</param>
    /// <param name="PMTlength">The PM tlength.</param>
    /// <returns></returns>
    public bool SendPMT(byte[] pmt, int PMTlength)
    {
      bool succeeded = false;
      for (int i = 0; i < PMTlength; ++i)
      {
        Marshal.WriteByte(ptrPmt, i, pmt[i]);
      }
      succeeded = KNCBDA_CI_SendPMTCommand(m_iDeviceIndex, ptrPmt, (uint) PMTlength) != 0;
      Log.Log.Info("KNC: SendPMT success = {0}", succeeded);
      
      if (!succeeded && m_ciState != KNCCiSlotStatus.Ready)
      {
        if (m_waitTimeout != 0)
        {
          succeeded = true; // if there is no CAM inserted, don't try to resend, as it would lead to "cannot run graph" after timeout
        }
        else
        {
          m_waitTimeout++; // increase to check it has once failed
        }
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
      switch (channel.DisEqc)
      {
        case DisEqcType.Level1AA:
          Log.Log.Info("KNC:  Level1AA - SendDiSEqCCommand(0x00)");
          SendDiSEqCCommand(0x00);
          break;
        case DisEqcType.Level1AB:
          Log.Log.Info("KNC:  Level1AB - SendDiSEqCCommand(0x01)");
          SendDiSEqCCommand(0x01);
          break;
        case DisEqcType.Level1BA:
          Log.Log.Info("KNC:  Level1BA - SendDiSEqCCommand(0x0100)");
          SendDiSEqCCommand(0x0100);
          break;
        case DisEqcType.Level1BB:
          Log.Log.Info("KNC:  Level1BB - SendDiSEqCCommand(0x0101)");
          SendDiSEqCCommand(0x0101);
          break;
        case DisEqcType.SimpleA:
          Log.Log.Info("KNC:  SimpleA - SendDiSEqCCommand(0x00)");
          SendDiSEqCCommand(0x00);
          break;
        case DisEqcType.SimpleB:
          Log.Log.Info("KNC:  SimpleB - SendDiSEqCCommand(0x01)");
          SendDiSEqCCommand(0x01);
          break;
        default:
          return;
      }
    }

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="ulRange">The DisEqCPort</param>
    /// <returns>true if succeeded, otherwise false</returns>
    protected bool SendDiSEqCCommand(ulong ulRange)
    {
      Log.Log.Info("KNC:  SendDiSEqC Command {0}", ulRange);
      // get ControlNode of tuner control node
      object ControlNode;
      int hr = ((IBDA_Topology)m_tunerFilter).GetControlNode(0, 1, 0, out ControlNode);
      if (hr == 0)
      // retrieve the BDA_DeviceControl interface 
      {
        IBDA_DeviceControl DecviceControl = (IBDA_DeviceControl)m_tunerFilter;
        if (DecviceControl != null)
        {
          if (ControlNode != null)
          {
            IBDA_FrequencyFilter FrequencyFilter = ControlNode as IBDA_FrequencyFilter;
            hr = DecviceControl.StartChanges();
            if (hr == 0)
            {
              if (FrequencyFilter != null)
              {
                hr = FrequencyFilter.put_Range(ulRange);
                Log.Log.Info("KNC:  put_Range:{0} success:{1}", ulRange, hr);
                if (hr == 0)
                {
                  // did it accept the changes? 
                  hr = DecviceControl.CheckChanges();
                  if (hr == 0)
                  {
                    hr = DecviceControl.CommitChanges();
                    if (hr == 0)
                    {
                      Log.Log.Info("KNC:  CommitChanges() Succeeded");
                      return true;
                    }
                    // reset configuration
                    Log.Log.Info("KNC:  CommitChanges() Failed!");
                    DecviceControl.StartChanges();
                    DecviceControl.CommitChanges();
                    return false;
                  }
                  Log.Log.Info("KNC:  CheckChanges() Failed!");
                  return false;
                }
                Log.Log.Info("KNC:  put_Range Failed!");
                return false;
              }
            }
          }
        }
      }
      Log.Log.Info("KNC:  GetControlNode Failed!");
      return false;
    } //end SendDiSEqCCommand

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      bool yesNo = false;
      if (m_bIsKNC)
      {
        if (KNCBDA_CI_IsAvailable(m_iDeviceIndex) != 0)
        {
          yesNo = true;
        }
      }
      Log.Log.Info("KNC: IsCIAvailable {0}", yesNo);
      return yesNo;
    }

    #region Callback handler 
    /// <summary>
    /// Callback from driver when CI status changes
    /// </summary>
    /// <param name="slot">Slot</param>
    /// <param name="State">Status</param>
    /// <param name="lpszMessage">Message</param>
    /// <param name="pParam">Context pointer</param>
    public void OnCiState(byte slot, KNCCiSlotStatus State, String lpszMessage, IntPtr pParam)
    {
      lock (this)
      {
        m_ciState = State; // remember in instance
        if (m_ciState == KNCCiSlotStatus.Ready)
        {
          m_waitTimeout = 0; // allow one new retry
        }
        Log.Log.Debug("KNC: card {0} CI State: {1} {2}", m_iDeviceIndex, lpszMessage, State);
      }
    }
    /// <summary>
    /// Callback from driver on opening CI menu
    /// </summary>
    /// <param name="slot">Slot</param>
    /// <param name="pParam">Context pointer</param>
    public void OnCiOpenDisplay(byte slot, IntPtr pParam)
    {
      lock (this)
      {
        Log.Log.Debug("OnKncCiOpenDisplay slot: {0}", slot);
      }
    }
    /// <summary>
    /// Callback from driver, returning CI menu headers
    /// </summary>
    /// <param name="slot">Slot</param>
    /// <param name="lpszTitle">Title</param>
    /// <param name="lpszSubTitle">Subtitle</param>
    /// <param name="lpszBottom">Bottom text</param>
    /// <param name="nNumChoices">Number of choices</param>
    /// <param name="pParam">Context pointer</param>
    public void OnCiMenu(byte slot, String lpszTitle, String lpszSubTitle, String lpszBottom, uint nNumChoices, IntPtr pParam)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("OnKncCiMenu slot:       {0}", slot);
          Log.Log.Debug("OnKncCiMenu title:      {0}", lpszTitle);
          Log.Log.Debug("OnKncCiMenu subtitle:   {0}", lpszSubTitle);
          Log.Log.Debug("OnKncCiMenu bottom:     {0}", lpszSubTitle);
          Log.Log.Debug("OnKncCiMenu lpszBottom: {0}", lpszBottom);
          Log.Log.Debug("OnKncCiMenu nNumChoices:{0}", nNumChoices);
          if (m_ciMenuCallback != null)
          {
            m_ciMenuCallback.OnCiMenu(lpszTitle.ToString(), lpszSubTitle.ToString(), lpszBottom.ToString(), (int)nNumChoices);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("OnKncCiMenu exception: {0}", ex.ToString());
      }
    }
    /// <summary>
    /// Callback from driver for every choice in menu
    /// </summary>
    /// <param name="slot">Slot</param>
    /// <param name="nChoice">current choice number</param>
    /// <param name="lpszText">choice text</param>
    /// <param name="pParam">Context pointer</param>
    public void OnCiMenuChoice(byte slot, uint nChoice, String lpszText, IntPtr pParam)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("OnKncCiMenuChoice slot:{0} choice:{1} text:{2}", slot, nChoice, lpszText);
          if (m_ciMenuCallback != null)
          {
            m_ciMenuCallback.OnCiMenuChoice((int)nChoice, lpszText.ToString());
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("OnKncCiMenuChoice exception: {0}", ex.ToString());
      }
    }
    /// <summary>
    /// Callback from driver for requesting user input
    /// </summary>
    /// <param name="slot">Slot</param>
    /// <param name="bBlind">Do blind input (like password)</param>
    /// <param name="nAnswerLength">Expected answer length</param>
    /// <param name="lpszText">Request text</param>
    /// <param name="pParam">Context pointer</param>
    public void OnCiRequest(byte slot, bool bBlind, uint nAnswerLength, String lpszText, IntPtr pParam)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("OnKncCiRequest slot:{0} bBlind:{1} nAnswerLength:{2} text:{3}", slot, bBlind, nAnswerLength, lpszText);
          if (m_ciMenuCallback != null)
          {
            m_ciMenuCallback.OnCiRequest(bBlind, nAnswerLength, lpszText.ToString());
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("OnKncCiRequest exception: {0}", ex.ToString());
      }
    }

    /// <summary>
    /// Callback from driver to close display
    /// </summary>
    /// <param name="slot">Slot</param>
    /// <param name="nDelay">delay in ms</param>
    /// <param name="pParam">Context pointer</param>
    public void OnCiCloseDisplay(byte slot, uint nDelay, IntPtr pParam)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("OnKncCiCloseDisplay slot:{0} nDelay:{1}", slot, nDelay);
          if (m_ciMenuCallback != null)
          {
            m_ciMenuCallback.OnCiCloseDisplay((int)nDelay);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("KncCiCloseDisplay exception: {0}", ex.ToString());
      }
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
        m_ciMenuCallback=ciMenuHandler;
        return true;
      }
      return false;
    }


    /// <summary>
    /// Enters the CI menu of KNC1 card
    /// </summary>
    /// <returns></returns>
    public bool EnterCIMenu()
    {
      if (!m_bCAM_present)
        return false;
      Log.Log.Debug("KNC: Enter CI Menu");
      KNCBDA_CI_EnterMenu(m_iDeviceIndex, m_nSlot);
      return true;
    }

    /// <summary>
    /// Closes the CI menu of KNC1 card
    /// </summary>
    /// <returns></returns>
    public bool CloseCIMenu()
    {
      if (!m_bCAM_present)
        return false;
      Log.Log.Debug("KNC: Close CI Menu");
      KNCBDA_CI_CloseMenu(m_iDeviceIndex, m_nSlot);
      return true;
    }

    /// <summary>
    /// Selects a CI menu entry
    /// </summary>
    /// <param name="choice"></param>
    /// <returns></returns>
    public bool SelectMenu(byte choice)
    {
      if (!m_bCAM_present)
        return false;
      Log.Log.Debug("KNC: Select CI Menu entry {0}", choice);
      KNCBDA_CI_SelectMenu(m_iDeviceIndex, m_nSlot, choice);
      return true;
    }

    /// <summary>
    /// Sends an answer after CI request
    /// </summary>
    /// <param name="Cancel"></param>
    /// <param name="Answer"></param>
    /// <returns></returns>
    public bool SendMenuAnswer(bool Cancel, String Answer)
    {
      if (!m_bCAM_present)
        return false;
      if (Answer == null) Answer = "";
      Log.Log.Debug("KNC: Send Menu Answer: {0}, Cancel: {1}", Answer, Cancel);
      KNCBDA_CI_SendMenuAnswer(m_iDeviceIndex, m_nSlot, Cancel, Answer);
      return true;
    }
    #endregion
  }
}
