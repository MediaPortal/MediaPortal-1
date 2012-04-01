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
using System.Text;
using DirectShowLib;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Interfaces;
using System.Runtime.InteropServices;
using TvLibrary.Implementations.DVB.Structures;
using TvDatabase;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access with the Hauppauge WinTV-CI.
  /// </summary>
  public class WinTvCiModule : ICiMenuActions, IDisposable
  {
    #region enums

    private enum WinTvCiState : int
    {
      Empty = 1,
      CamPresent
    }

    #endregion

    #region DLL imports

    /// <summary>
    /// Initialise the WinTV-CI interface. The callback delegate parameters are all optional.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="onState">A delegate that will be called by the WinTV-CI driver when the CI slot state changes.</param>
    /// <param name="onCamInfo">A delegate that will be called by the WinTV-CI driver when CA application information is received from a CAM.</param>
    /// <param name="onApdu">A delegate that will be called by the WinTV-CI driver when an application protocol data unit is received from a CAM.</param>
    /// <param name="onCloseMmi">A delegate that will be called by the WinTV-CI driver when a CAM wants to close an MMI session.</param>
    /// <returns>an HRESULT indicating whether the interface was successfully opened</returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_Init(IBaseFilter ciFilter, OnWinTvCiState onState,
                                            OnWinTvCiCamInfo onCamInfo, OnWinTvCiApdu onApdu,
                                            OnWinTvCiCloseMmi onCloseMmi);

    /// <summary>
    /// Send PMT data to the CAM to request a service be descrambled. It is not possible to descramble
    /// more than one service at a time.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="pmtLength">The length of the PMT in bytes.</param>
    /// <returns>an HRESULT indicating whether the PMT was successfully received by the CAM</returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_SendPMT(IBaseFilter ciFilter,
                                               [In, MarshalAs(UnmanagedType.LPArray)] byte[] pmt, Int32 pmtLength);

    /// <summary>
    /// Send an APDU to the CAM. This function can be used communicate with the CAM
    /// after an MMI session has been opened.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="apdu">The APDU.</param>
    /// <param name="apduLength">The length of the APDU in bytes.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully received by the CAM</returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_SendAPDU(IBaseFilter ciFilter,
                                                [In, MarshalAs(UnmanagedType.LPArray)] byte[] apdu, Int32 apduLength);

    /// <summary>
    /// Open an MMI session with the CAM.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether a session was successfully opened</returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_OpenMMI(IBaseFilter ciFilter);

    /// <summary>
    /// Enable the WinTV-CI tray icon. The tray application implements application-independent
    /// CAM menu access.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether the tray application was successfully started</returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_EnableTrayIcon(IBaseFilter ciFilter);

    /// <summary>
    /// Close the WinTV-CI interface.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether the interface was successfully closed</returns>
    [DllImport("hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_Shutdown(IBaseFilter ciFilter);

    #endregion

    #region callback delegate definitions

    /// <summary>
    /// Called by the WinTV CI driver when the CI slot state changes.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <returns>an HRESULT indicating whether the state change was successfully handled</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate Int32 OnWinTvCiState(IBaseFilter ciFilter, WinTvCiState state);

    /// <summary>
    /// Called by the WinTV-CI driver when CA application information is received from a CAM.
    /// </summary>
    /// <param name="context">???</param>
    /// <param name="applicationType">The conditional access application type.</param>
    /// <param name="manufacturer">The conditional access application manufacturer.</param>
    /// <param name="code">A code set by the conditional access application manufacturer.</param>
    /// <param name="menuTitle">The CAM root menu title.</param>
    /// <returns>an HRESULT indicating whether the application information was successfully processed</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private delegate Int32 OnWinTvCiCamInfo(
      IntPtr context, DVB_MMI.ApplicationType applicationType, UInt16 manufacturer, UInt16 code,
      [MarshalAs(UnmanagedType.LPStr)] String menuTitle);

    /// <summary>
    /// Called by the WinTV-CI driver when an application protocol data unit is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="apdu">A buffer containing the APDU.</param>
    /// <param name="apduLength">The length of the APDU buffer in bytes.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully processed</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate Int32 OnWinTvCiApdu(IBaseFilter ciFilter, IntPtr apdu, Int32 apduLength);

    /// <summary>
    /// Called by the WinTV-CI driver when a CAM wants to close an MMI session.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether the MMI session was successfully closed</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate Int32 OnWinTvCiCloseMmi(IBaseFilter ciFilter);

    #endregion

    private bool _isWinTvCi = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;
    private WinTvCiState _ciState = WinTvCiState.Empty;

    private IBaseFilter _winTvCiFilter = null;
    private DsDevice _winTvCiDevice = null;
    private IFilterGraph2 _graph = null;

    // Callbacks
    private OnWinTvCiState _ciStateCallback;
    private OnWinTvCiCamInfo _camInfoCallback;
    private OnWinTvCiApdu _apduCallback;
    private OnWinTvCiCloseMmi _closeMmiCallback;

    private ICiMenuCallbacks _ciMenuCallbacks;
    private DVB_MMI_Handler _mmiHandler;

    ///<summary>
    /// Initialises a new instance of the <see cref="WinTvCiModule"/> class.
    ///</summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="tunerDevicePath">The tuner device path.</param>
    public WinTvCiModule(IBaseFilter tunerFilter, String tunerDevicePath)
    {
      // TV Server configuration allows the WinTV-CI to be linked to one
      // tuner. We need to know whether this tuner is the one that is
      // linked to the WinTV-CI.
      TvBusinessLayer layer = new TvBusinessLayer();
      Card tuner = layer.GetCardByDevicePath(tunerDevicePath);
      if (tuner == null)
      {
        Log.Log.Debug("WinTV-CI: tuner not found in database");
        return;
      }

      int winTvCiTunerId = Int32.Parse(layer.GetSetting("winTvCiTuner", "-1").Value);
      if (winTvCiTunerId != tuner.IdCard)
      {
        Log.Log.Info("WinTV-CI: WinTV-CI module not assigned to tuner");
        return;
      }

      Log.Log.Debug("WinTV-CI: supported tuner detected");
      _isWinTvCi = true;
      _mmiHandler = new DVB_MMI_Handler("WinTV-CI");
    }

    /// <summary>
    /// Insert and connect the add-on device into the graph.
    /// </summary>
    /// <param name="graphBuilder">The graph builder to use to insert the device.</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture filter) to connect the device to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(ref ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter)
    {
      Log.Log.Debug("WinTV-CI: add filter to graph");
      if (graphBuilder == null)
      {
        Log.Log.Debug("WinTV-CI: graph builder is null");
        return false;
      }
      if (lastFilter == null)
      {
        Log.Log.Debug("WinTV-CI: upstream filter is null");
        return false;
      }

      // Check if the WinTV-CI is actually installed in this system.
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      for (int i = 0; i < captureDevices.Length; i++)
      {
        if (captureDevices[i].Name != null && captureDevices[i].Name.ToLowerInvariant().Equals("wintvciusbbda source"))
        {
          if (DevicesInUse.Instance.IsUsed(captureDevices[i]))
          {
            Log.Log.Debug("WinTV-CI: the WinTV-CI is already in use");
            return false;
          }
          Log.Log.Debug("WinTV-CI: found WinTV-CI device");
          _winTvCiDevice = captureDevices[i];
          break;
        }
      }
      if (_winTvCiDevice == null)
      {
        Log.Log.Info("WinTV-CI: WinTV-CI device not found");
        return false;
      }

      // We found the device. Now we need a reference to the graph
      // builder's graph.
      IGraphBuilder tmpGraph = null;
      int hr = graphBuilder.GetFiltergraph(out tmpGraph);
      if (hr != 0)
      {
        Log.Log.Debug("WinTV-CI: failed to get graph reference, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      _graph = tmpGraph as IFilterGraph2;
      if (_graph == null)
      {
        Log.Log.Debug("WinTV-CI: failed to get graph reference");
        _winTvCiDevice = null;
        return false;
      }

      // Add the WinTV-CI filter to the graph.
      hr = _graph.AddSourceFilterForMoniker(_winTvCiDevice.Mon, null, _winTvCiDevice.Name, out _winTvCiFilter);
      if (hr != 0)
      {
        Log.Log.Debug("WinTV-CI: failed to add filter to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        _winTvCiDevice = null;
        if (_winTvCiFilter != null)
        {
          _graph.RemoveFilter(_winTvCiFilter);
          Release.ComObject(_winTvCiFilter);
          _winTvCiFilter = null;
        }
        return false;
      }

      // Connect the filter into the graph.
      hr = graphBuilder.RenderStream(null, null, lastFilter, null, _winTvCiFilter);
      if (hr != 0)
      {
        Log.Log.Debug("WinTV-CI: failed to render stream through filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        _graph.RemoveFilter(_winTvCiFilter);
        Release.ComObject(_winTvCiFilter);
        _winTvCiFilter = null;
        return false;
      }

      // Success!
      DevicesInUse.Instance.Add(_winTvCiDevice);
      _isCiSlotPresent = true;
      lastFilter = _winTvCiFilter;
      return true;
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is linked to the WinTV-CI.
    /// </summary>
    /// <value><c>true</c> if this tuner is linked to the WinTV-CI, otherwise <c>false</c></value>
    public bool IsWinTvCi
    {
      get
      {
        return _isWinTvCi;
      }
    }

    #region conditional access

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenCi()
    {
      Log.Log.Debug("WinTV-CI: open conditional access interface");

      _ciStateCallback = new OnWinTvCiState(OnCiState);
      _camInfoCallback = new OnWinTvCiCamInfo(OnCamInfo);
      _apduCallback = new OnWinTvCiApdu(OnApdu);
      _closeMmiCallback = new OnWinTvCiCloseMmi(OnCloseMmi);
      int hr = WinTVCI_Init(_winTvCiFilter, _ciStateCallback, _camInfoCallback, _apduCallback, _closeMmiCallback);
      if (hr == 0)
      {
        Log.Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseCi()
    {
      Log.Log.Debug("WinTV-CI: close conditional access interface");
      _isCamPresent = false;
      _isCamReady = false;
      int hr = WinTVCI_Shutdown(_winTvCiFilter);
      if (hr == 0)
      {
        Log.Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph"><c>True</c> if the DirectShow filter graph should be rebuilt after calling this function.</param>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    public bool ResetCi(out bool rebuildGraph)
    {
      rebuildGraph = false;
      return CloseCi() && OpenCi();
    }

    /// <summary>
    /// Determines whether a CI slot is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      Log.Log.Debug("WinTV-CI: is CI slot present");

      // The WinTV-CI is a CI slot - of course a CI slot is present!
      Log.Log.Debug("WinTV-CI: result = {0}", true);
      return true;
    }

    /// <summary>
    /// Determines whether a CAM is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present, otherwise <c>false</c></returns>
    public bool IsCamPresent()
    {
      Log.Log.Debug("WinTV-CI: is CAM present");

      // The CAM state is automatically updated in the OnCiState() callback.
      Log.Log.Debug("WinTV-CI: result = {0}", _isCamPresent);
      return _isCamPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present and ready for interaction.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present and ready, otherwise <c>false</c></returns>
    public bool IsCamReady()
    {
      Log.Log.Debug("WinTV-CI: is CAM ready");

      // The CAM state is automatically updated in the OnCiState() callback.
      Log.Log.Debug("WinTV-CI: result = {0}", _isCamReady);
      return _isCamReady;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the request is sent successfully, otherwise <c>false</c></returns>
    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("WinTV-CI: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCamPresent)
      {
        Log.Log.Debug("WinTV-CI: CAM not available");
        return true;    // Don't retry.
      }
      if (command == CommandIdType.MMI || command == CommandIdType.Query)
      {
        Log.Log.Debug("WinTV-CI: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null || pmt.Length == 0)
      {
        Log.Log.Debug("WinTV-CI: no PMT");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CommandIdType.NotSelected)
      {
        return true;
      }

      // The WinTV-CI can only decrypt one channel at a time. During development
      // of this class I did actually try to assemble a fake CA PMT structure that
      // would trick the module into decrypting multiple channels, however it didn't
      // work. So we'll just send this PMT to the CAM regardless of the list management
      // action.
      int hr = WinTVCI_SendPMT(_winTvCiFilter, pmt, length);
      if (hr == 0)
      {
        Log.Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region callback handlers

    /// <summary>
    /// Called by the WinTV CI driver when the CI slot state changes.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <returns>an HRESULT indicating whether the state change was successfully handled</returns>
    private Int32 OnCiState(IBaseFilter ciFilter, WinTvCiState state)
    {
      Log.Log.Debug("WinTV-CI: CI state change callback");
      Log.Log.Debug("  old state  = {0}", _ciState);
      Log.Log.Debug("  new state  = {0}", state);
      _ciState = state;

      _isCamReady = false;
      if (state == WinTvCiState.Empty)
      {
        _isCamPresent = false;
      }
      else if (state == WinTvCiState.CamPresent)
      {
        _isCamPresent = true;
      }
      return 0;
    }

    /// <summary>
    /// Called by the WinTV-CI driver when CA application information is received from a CAM.
    /// </summary>
    /// <param name="context">???</param>
    /// <param name="applicationType">The conditional access application type.</param>
    /// <param name="manufacturer">The conditional access application manufacturer.</param>
    /// <param name="code">A code set by the conditional access application manufacturer.</param>
    /// <param name="menuTitle">The CAM root menu title.</param>
    /// <returns>an HRESULT indicating whether the application information was successfully processed</returns>
    private Int32 OnCamInfo(
      IntPtr context, DVB_MMI.ApplicationType applicationType, UInt16 manufacturer, UInt16 code,
      [MarshalAs(UnmanagedType.LPStr)] String menuTitle)
    {
      Log.Log.Debug("WinTV-CI: CAM info callback");
      Log.Log.Debug("  type         = {0}", applicationType);
      Log.Log.Debug("  manufacturer = 0x{0:x}", manufacturer);
      Log.Log.Debug("  code         = 0x{0:x}", code);
      Log.Log.Debug("  menu title   = {0}", menuTitle);

      // Receiving application information indicates that the CAM is ready
      // for interaction.
      _isCamPresent = true;
      _isCamReady = true;
      return 0;
    }

    /// <summary>
    /// Called by the WinTV-CI driver when an application protocol data unit is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="apdu">A buffer containing the APDU.</param>
    /// <param name="apduLength">The length of the APDU buffer in bytes.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully processed</returns>
    private Int32 OnApdu(IBaseFilter ciFilter, IntPtr apdu, Int32 apduLength)
    {
      Log.Log.Info("WinTV-CI: APDU callback");

      //DVB_MMI.DumpBinary(apdu, 0, apduLength);
      byte[] apduBytes = new byte[apduLength];
      Marshal.Copy(apdu, apduBytes, 0, apduLength);
      _mmiHandler.HandleMMI(apduBytes, apduLength);
      return 0;
    }

    /// <summary>
    /// Called by the WinTV-CI driver when a CAM wants to close an MMI session.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether the MMI session was successfully closed</returns>
    private Int32 OnCloseMmi(IBaseFilter ciFilter)
    {
      Log.Log.Debug("WinTV-CI: close _mmiHandler callback");
      try
      {
        if (_ciMenuCallbacks != null)
        {
          _ciMenuCallbacks.OnCiCloseDisplay(0);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("WinTV-CI: close _mmiHandler callback exception\r\n{0}", ex.ToString());
      }
      return 0;
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
        _mmiHandler.SetCiMenuHandler(ref _ciMenuCallbacks);
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
      Log.Log.Debug("WinTV-CI: enter menu");
      int hr = WinTVCI_OpenMMI(_winTvCiFilter);
      if (hr == 0)
      {
        Log.Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      Log.Log.Debug("WinTV-CI: close menu");
      byte[] apdu = DVB_MMI.CreateMMIClose();
      int hr = WinTVCI_SendAPDU(_winTvCiFilter, apdu, apdu.Length);
      if (hr == 0)
      {
        Log.Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      Log.Log.Debug("WinTV-CI: select menu entry, choice = {0}", choice);
      byte[] apdu = DVB_MMI.CreateMMISelect(choice);
      int hr = WinTVCI_SendAPDU(_winTvCiFilter, apdu, apdu.Length);
      if (hr == 0)
      {
        Log.Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Log.Debug("WinTV-CI: send menu answer, answer = {0}, cancel = {1}", answer, cancel);
      DVB_MMI.ResponseType responseType = DVB_MMI.ResponseType.Answer;
      if (cancel)
      {
        responseType = DVB_MMI.ResponseType.Cancel;
      }
      byte[] apdu = DVB_MMI.CreateMMIAnswer(responseType, answer);
      int hr = WinTVCI_SendAPDU(_winTvCiFilter, apdu, apdu.Length);
      if (hr == 0)
      {
        Log.Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    public void Dispose()
    {
      if (_winTvCiFilter != null)
      {
        CloseCi();
        _graph.RemoveFilter(_winTvCiFilter);
        Release.ComObject(_winTvCiFilter);
        _winTvCiFilter = null;
      }
      if (_winTvCiDevice != null)
      {
        DevicesInUse.Instance.Remove(_winTvCiDevice);
        _winTvCiDevice = null;
      }
      _isWinTvCi = false;
    }

    #endregion
  }
}