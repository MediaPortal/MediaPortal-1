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
using DirectShowLib;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;
using TvLibrary.Log;

namespace TvEngine
{
  /// <summary>
  /// A class for handling conditional access with the Hauppauge WinTV-CI.
  /// </summary>
  public class WinTvCi : BaseCustomDevice, IAddOnDevice, IConditionalAccessProvider, ICiMenuActions, ITvServerPlugin
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
    [DllImport("Resources\\hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
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
    [DllImport("Resources\\hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
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
    [DllImport("Resources\\hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_SendAPDU(IBaseFilter ciFilter,
                                                [In, MarshalAs(UnmanagedType.LPArray)] byte[] apdu, Int32 apduLength);

    /// <summary>
    /// Open an MMI session with the CAM.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether a session was successfully opened</returns>
    [DllImport("Resources\\hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_OpenMMI(IBaseFilter ciFilter);

    /// <summary>
    /// Enable the WinTV-CI tray icon. The tray application implements application-independent
    /// CAM menu access.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether the tray application was successfully started</returns>
    [DllImport("Resources\\hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_EnableTrayIcon(IBaseFilter ciFilter);

    /// <summary>
    /// Close the WinTV-CI interface.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether the interface was successfully closed</returns>
    [DllImport("Resources\\hcwWinTVCI.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 WinTVCI_Shutdown(IBaseFilter ciFilter);

    #endregion

    #region callback definitions

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
      IntPtr context, MmiApplicationType applicationType, UInt16 manufacturer, UInt16 code,
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

    #region variables

    // We use this list to keep track of whether the WinTV-CI is already in use.
    private static List<String> _devicesInUse = new List<string>();

    private bool _isWinTvCi = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
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
    private DvbMmiHandler _mmiHandler;

    #endregion

    #region callback handlers

    /// <summary>
    /// Called by the WinTV-CI driver when the CI slot state changes.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <returns>an HRESULT indicating whether the state change was successfully handled</returns>
    private Int32 OnCiState(IBaseFilter ciFilter, WinTvCiState state)
    {
      Log.Debug("WinTV-CI: CI state change callback");
      Log.Debug("  old state  = {0}", _ciState);
      Log.Debug("  new state  = {0}", state);
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
      IntPtr context, MmiApplicationType applicationType, UInt16 manufacturer, UInt16 code,
      [MarshalAs(UnmanagedType.LPStr)] String menuTitle)
    {
      Log.Debug("WinTV-CI: CAM info callback");
      Log.Debug("  type         = {0}", applicationType);
      Log.Debug("  manufacturer = 0x{0:x}", manufacturer);
      Log.Debug("  code         = 0x{0:x}", code);
      Log.Debug("  menu title   = {0}", menuTitle);

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
      Log.Info("WinTV-CI: APDU callback");

      //DVB_MMI.DumpBinary(apdu, 0, apduLength);
      byte[] apduBytes = new byte[apduLength];
      Marshal.Copy(apdu, apduBytes, 0, apduLength);
      _mmiHandler.HandleMMI(apduBytes);
      return 0;
    }

    /// <summary>
    /// Called by the WinTV-CI driver when a CAM wants to close an MMI session.
    /// </summary>
    /// <param name="ciFilter">The WinTV-CI filter.</param>
    /// <returns>an HRESULT indicating whether the MMI session was successfully closed</returns>
    private Int32 OnCloseMmi(IBaseFilter ciFilter)
    {
      Log.Debug("WinTV-CI: close MMI callback");
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiCloseDisplay(0);
        }
        catch (Exception ex)
        {
          Log.Debug("WinTV-CI: close MMI callback exception\r\n{0}", ex.ToString());
        }
      }
      else
      {
        Log.Debug("WinTV-CI: menu callbacks are not set");
      }
      return 0;
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Log.Debug("WinTV-CI: initialising device");

      if (String.IsNullOrEmpty(tunerDevicePath))
      {
        Log.Debug("WinTV-CI: tuner device path is not set");
        return false;
      }
      if (_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device is already initialised");
        return true;
      }

      // The TV Server plugin allows the WinTV-CI to be linked to one tuner. We need to know whether
      // this tuner (ie. the one associated with the tuner filter and tuner device path) is the one
      // that is currently linked to the WinTV-CI.
      TvBusinessLayer layer = new TvBusinessLayer();
      Card tuner = layer.GetCardByDevicePath(tunerDevicePath);
      if (tuner == null)
      {
        Log.Debug("WinTV-CI: device not found in database");
        return false;
      }

      if (layer.GetSetting("winTvCiTuner", "-1").Value != tuner.IdCard.ToString())
      {
        Log.Debug("WinTV-CI: the WinTV-CI is not linked to this device");
        return false;
      }

      Log.Debug("WinTV-CI: supported device detected");
      _isWinTvCi = true;
      return true;
    }

    #endregion

    #region IAddOnDevice member

    /// <summary>
    /// Insert and connect the device's additional filter(s) into the BDA graph.
    /// [network provider]->[tuner]->[capture]->[...device filter(s)]->[infinite tee]->[MPEG 2 demultiplexer]->[transport information filter]->[transport stream writer]
    /// </summary>
    /// <param name="graphBuilder">The graph builder to use to insert the device filter(s).</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter)
    {
      Log.Debug("WinTV-CI: add to graph");

      if (!_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device not initialised or interface not supported");
        return false;
      }
      if (graphBuilder == null)
      {
        Log.Debug("WinTV-CI: graph builder is null");
        return false;
      }
      if (lastFilter == null)
      {
        Log.Debug("WinTV-CI: upstream filter is null");
        return false;
      }
      if (_winTvCiFilter != null)
      {
        Log.Debug("WinTV-CI: device filter already in graph");
        return true;
      }

      // Check if the WinTV-CI is actually installed in this system.
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      foreach (DsDevice captureDevice in captureDevices)
      {
        if (captureDevice.Name != null && captureDevice.Name.ToLowerInvariant().Equals("wintvciusbbda source"))
        {
          lock (_devicesInUse)
          {
            List<String>.Enumerator en = _devicesInUse.GetEnumerator();
            while (en.MoveNext())
            {
              if (en.Current.Equals(captureDevice.DevicePath))
              {
                Log.Debug("WinTV-CI: the WinTV-CI is already in use");
                return false;
              }
            }
            _devicesInUse.Add(captureDevice.DevicePath);
          }
          Log.Debug("WinTV-CI: found WinTV-CI device");
          _winTvCiDevice = captureDevice;
          break;
        }
      }
      if (_winTvCiDevice == null)
      {
        Log.Info("WinTV-CI: WinTV-CI device not found");
        return false;
      }

      // We found the device. Now we need a reference to the graph builder's graph.
      bool success = false;
      try
      {
        IGraphBuilder tmpGraph = null;
        int hr = graphBuilder.GetFiltergraph(out tmpGraph);
        if (hr != 0)
        {
          Log.Debug("WinTV-CI: failed to get graph reference, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }
        _graph = tmpGraph as IFilterGraph2;
        if (_graph == null)
        {
          Log.Debug("WinTV-CI: failed to get graph reference");
          return false;
        }

        // Add the WinTV-CI filter to the graph.
        hr = _graph.AddSourceFilterForMoniker(_winTvCiDevice.Mon, null, _winTvCiDevice.Name, out _winTvCiFilter);
        if (hr != 0)
        {
          Log.Debug("WinTV-CI: failed to add filter to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        // Connect the filter into the graph.
        hr = graphBuilder.RenderStream(null, null, lastFilter, null, _winTvCiFilter);
        if (hr != 0)
        {
          Log.Debug("WinTV-CI: failed to render stream through filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        success = true;
      }
      finally
      {
        if (!success)
        {
          if (_winTvCiFilter != null)
          {
            _graph.RemoveFilter(_winTvCiFilter);
            DsUtils.ReleaseComObject(_winTvCiFilter);
            _winTvCiFilter = null;
          }
          lock (_devicesInUse)
          {
            _devicesInUse.Remove(_winTvCiDevice.DevicePath);
            _winTvCiDevice = null;
          }
        }
      }

      // Success!
      Log.Debug("WinTV-CI: result = success");
      lastFilter = _winTvCiFilter;
      return true;
    }

    #endregion

    #region ITvServerPlugin members

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "WinTV-CI";
      }
    }

    /// <summary>
    /// The version of this TV Server plugin.
    /// </summary>
    public string Version
    {
      get
      {
        return "1.0.0.0";
      }
    }

    /// <summary>
    /// The author of this TV Server plugin.
    /// </summary>
    public string Author
    {
      get
      {
        return "mm1352000";
      }
    }

    /// <summary>
    /// Determine whether this TV Server plugin should only run on the master server, or if it can also
    /// run on slave servers.
    /// </summary>
    /// <remarks>
    /// This property is obsolete. Master-slave configurations are not supported.
    /// </remarks>
    public bool MasterOnly
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Get an instance of the configuration section for use in TV Server configuration (SetupTv).
    /// </summary>
    public SetupTv.SectionSettings Setup
    {
      get { return new SetupTv.Sections.WinTvCiConfig(); }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IController controller)
    {
    }

    /// <summary>
    /// Stop this TV Server plugin.
    /// </summary>
    public void Stop()
    {
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
      Log.Debug("WinTV-CI: open conditional access interface");

      if (!_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device not initialised or interface not supported");
        return false;
      }
      if (_winTvCiFilter == null)
      {
        Log.Debug("WinTV-CI: device filter not added to the BDA filter graph");
        return false;
      }
      if (_mmiHandler != null)
      {
        Log.Debug("WinTV-CI: interface is already open");
        return false;
      }

      // This handler will deal with MMI messages from the CAM.
      _mmiHandler = new DvbMmiHandler("WinTV-CI");

      // Set up callbacks and open the interface.
      _ciStateCallback = new OnWinTvCiState(OnCiState);
      _camInfoCallback = new OnWinTvCiCamInfo(OnCamInfo);
      _apduCallback = new OnWinTvCiApdu(OnApdu);
      _closeMmiCallback = new OnWinTvCiCloseMmi(OnCloseMmi);
      int hr = WinTVCI_Init(_winTvCiFilter, _ciStateCallback, _camInfoCallback, _apduCallback, _closeMmiCallback);
      if (hr == 0)
      {
        Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      Log.Debug("WinTV-CI: close conditional access interface");

      if (_winTvCiFilter != null && _mmiHandler != null)
      {
        int hr = WinTVCI_Shutdown(_winTvCiFilter);
        if (hr != 0)
        {
          Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }
      }
      _isCamReady = false;
      _mmiHandler = null;
      _ciStateCallback = null;
      _camInfoCallback = null;
      _apduCallback = null;
      _closeMmiCallback = null;

      Log.Debug("WinTV-CI: result = success");
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
      Log.Debug("WinTV-CI: is conditional access interface ready");

      // The CI/CAM state is automatically updated in the OnCiState() callback.
      Log.Debug("WinTV-CI: result = {0}", _isCamReady);
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
      Log.Debug("WinTV-CI: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device not initialised or interface not supported");
        return false;
      }
      if (_winTvCiFilter == null)
      {
        Log.Debug("WinTV-CI: device filter not added to the BDA filter graph");
        return false;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        Log.Debug("WinTV-CI: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null)
      {
        Log.Debug("WinTV-CI: PMT not supplied");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CaPmtCommand.NotSelected)
      {
        Log.Debug("WinTV-CI: result = success");
        return true;
      }

      // The WinTV-CI can only decrypt one channel at a time. During development
      // of this class I did actually try to assemble a fake CA PMT structure that
      // would trick the module into decrypting multiple channels, however it didn't
      // work. So we'll just send this PMT to the CAM regardless of the list management
      // action.
      byte[] rawPmt = pmt.GetRawPmt();
      int hr = WinTVCI_SendPMT(_winTvCiFilter, rawPmt, rawPmt.Length);
      if (hr == 0)
      {
        Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
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
        _mmiHandler.SetCiMenuHandler(ref _ciMenuCallbacks);
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
      Log.Debug("WinTV-CI: enter menu");

      if (!_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device not initialised or interface not supported");
        return false;
      }
      if (_winTvCiFilter == null)
      {
        Log.Debug("WinTV-CI: device filter not added to the BDA filter graph");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("WinTV-CI: the CAM is not ready");
        return false;
      }

      int hr = WinTVCI_OpenMMI(_winTvCiFilter);
      if (hr == 0)
      {
        Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Debug("WinTV-CI: close menu");

      if (!_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device not initialised or interface not supported");
        return false;
      }
      if (_winTvCiFilter == null)
      {
        Log.Debug("WinTV-CI: device filter not added to the BDA filter graph");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("WinTV-CI: the CAM is not ready");
        return false;
      }

      byte[] apdu = DVB_MMI.CreateMMIClose();
      int hr = WinTVCI_SendAPDU(_winTvCiFilter, apdu, apdu.Length);
      if (hr == 0)
      {
        Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Debug("WinTV-CI: select menu entry, choice = {0}", choice);

      if (!_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device not initialised or interface not supported");
        return false;
      }
      if (_winTvCiFilter == null)
      {
        Log.Debug("WinTV-CI: device filter not added to the BDA filter graph");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("WinTV-CI: the CAM is not ready");
        return false;
      }

      byte[] apdu = DVB_MMI.CreateMMISelect(choice);
      int hr = WinTVCI_SendAPDU(_winTvCiFilter, apdu, apdu.Length);
      if (hr == 0)
      {
        Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
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
      Log.Debug("WinTV-CI: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isWinTvCi)
      {
        Log.Debug("WinTV-CI: device not initialised or interface not supported");
        return false;
      }
      if (_winTvCiFilter == null)
      {
        Log.Debug("WinTV-CI: device filter not added to the BDA filter graph");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("WinTV-CI: the CAM is not ready");
        return false;
      }

      MmiResponseType responseType = MmiResponseType.Answer;
      if (cancel)
      {
        responseType = MmiResponseType.Cancel;
      }
      byte[] apdu = DVB_MMI.CreateMMIAnswer(responseType, answer);
      int hr = WinTVCI_SendAPDU(_winTvCiFilter, apdu, apdu.Length);
      if (hr == 0)
      {
        Log.Debug("WinTV-CI: result = success");
        return true;
      }

      Log.Debug("WinTV-CI: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      if (_winTvCiFilter != null)
      {
        _graph.RemoveFilter(_winTvCiFilter);
        DsUtils.ReleaseComObject(_winTvCiFilter);
        _winTvCiFilter = null;
      }
      if (_winTvCiDevice != null)
      {
        lock (_devicesInUse)
        {
          _devicesInUse.Remove(_winTvCiDevice.DevicePath);
        }
        _winTvCiDevice = null;
      }
      _graph = null;
      _isWinTvCi = false;
    }

    #endregion
  }
}