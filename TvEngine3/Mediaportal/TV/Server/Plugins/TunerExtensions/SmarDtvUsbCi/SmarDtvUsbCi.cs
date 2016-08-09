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
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Config;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product.Struct;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi
{
  /// <summary>
  /// A class for handling conditional access with the Hauppauge WinTV-CI and TerraTec USB CI. Both
  /// devices are based on an OEM design by SmarDTV.
  /// </summary>
  public class SmarDtvUsbCi : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, IDirectShowAddOnDevice, IDisposable, ITvServerPlugin, ITvServerPluginCommunication
  {
    #region variables

    private static bool _isPluginEnabled = false;
    private static SmarDtvUsbCiConfigService _service = new SmarDtvUsbCiConfigService();

    private bool _isSmarDtvUsbCi = false;
    private bool _isCaInterfaceOpen = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private CiState _ciState = CiState.Empty;

    private ISmarDtvUsbCiProduct _product = null;
    private IBaseFilter _ciFilter = null;
    private IFilterGraph2 _graph = null;
    private bool _isFilterInGraph = false;

    // Call backs
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();
    private CiCallBack _ciCallBack;

    #endregion

    #region delegate implementations

    /// <summary>
    /// Invoked by the driver when the CI slot state changes.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <returns>an HRESULT indicating whether the state change was successfully handled</returns>
    private int OnCiState(IBaseFilter ciFilter, CiState state)
    {
      this.LogInfo("SmarDTV USB CI: CI state change call back");
      this.LogInfo("  old state  = {0}", _ciState);
      this.LogInfo("  new state  = {0}", state);
      _ciState = state;

      _isCamReady = false;
      if (state == CiState.Empty)
      {
        _isCamPresent = false;
      }
      else if (state == CiState.CamPresent)
      {
        _isCamPresent = true;
      }
      return 0;
    }

    /// <summary>
    /// Invoked by the driver when application information is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="info">The application information.</param>
    /// <returns>an HRESULT indicating whether the application information was successfully processed</returns>
    private int OnApplicationInfo(IBaseFilter ciFilter, ref ApplicationInfo info)
    {
      this.LogInfo("SmarDTV USB CI: application information call back");
      this.LogDebug("  type         = {0}", info.ApplicationType);
      // Note: current drivers seem to have a bug that causes only the first byte in the manufacturer and code
      // fields to be available.
      this.LogDebug("  manufacturer = 0x{0:x4}", info.Manufacturer);
      this.LogDebug("  code         = 0x{0:x4}", info.Code);
      this.LogDebug("  menu title   = {0}", DvbTextConverter.Convert(info.MenuTitle, info.MenuTitleLength));

      // Receiving application information indicates that the CAM is ready for interaction.
      _isCamPresent = true;
      _isCamReady = true;
      return 0;
    }

    /// <summary>
    /// Invoked by the driver when a CAM wants to close an MMI session.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <returns>an HRESULT indicating whether the MMI session was successfully closed</returns>
    private int OnCloseMmi(IBaseFilter ciFilter)
    {
      this.LogInfo("SmarDTV USB CI: close MMI call back");
      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiCloseDisplay(0);
        }
        else
        {
          this.LogDebug("SmarDTV USB CI: menu call back not set");
        }
      }
      return 0;
    }

    /// <summary>
    /// Invoked by the driver when an application protocol data unit is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="apduLength">The length of the APDU buffer in bytes.</param>
    /// <param name="apdu">A buffer containing the APDU.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully processed</returns>
    private int OnApdu(IBaseFilter ciFilter, int apduLength, byte[] apdu)
    {
      this.LogInfo("SmarDTV USB CI: APDU call back");
      lock (_caMenuCallBackLock)
      {
        DvbMmiHandler.HandleMmiData(apdu, _caMenuCallBack);
      }
      return 0;
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// Does the extension control some part of the tuner hardware?
    /// </summary>
    public override bool ControlsTunerHardware
    {
      get
      {
        return false;
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
      this.LogDebug("SmarDTV USB CI: initialising");

      // If the plugin is disabled then don't load the extension.
      if (_isPluginEnabled)
      {
        this.LogDebug("SmarDTV USB CI: plugin disabled");
        return false;
      }

      if (_isSmarDtvUsbCi)
      {
        this.LogWarn("SmarDTV USB CI: extension already initialised");
        return true;
      }

      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("SmarDTV USB CI: tuner external identifier is not set");
        return false;
      }

      // A machine may only have one instance of each OEM product installed
      // - this is a driver limitation. It is unknown whether a single machine
      // may have multiple instances by connecting instances of different
      // products (we don't explicitly prevent this). The TV Server plugin
      // allows each OEM CI product to be linked to a single tuner. Here we
      // need to know whether this tuner (ie. the one referred to by the
      // external identifier) is currently linked to any of the products.
      ReadOnlyCollection<ISmarDtvUsbCiProduct> productList = SmarDtvUsbCiProductBase.GetProductList();
      foreach (ISmarDtvUsbCiProduct p in productList)
      {
        this.LogDebug("SmarDTV USB CI: {0}...", p.Name);
        if (!tunerExternalId.Equals(p.LinkedTuner))
        {
          this.LogDebug("SmarDTV USB CI:   not linked");
          continue;
        }
        if (p.IsInUse)
        {
          this.LogDebug("SmarDTV USB CI:   already in use");
          continue;
        }
        DriverInstallState installState = p.InstallState;
        if (installState != DriverInstallState.BdaDriver)
        {
          this.LogDebug("SmarDTV USB CI:   not usable, install state = {0}", installState);
          continue;
        }
        _ciFilter = p.Initialise();
        if (_ciFilter == null)
        {
          this.LogError("SmarDTV USB CI: failed to initialise product {0}", p.Name);
          continue;
        }

        this.LogInfo("SmarDTV USB CI: extension supported");
        _isSmarDtvUsbCi = true;
        _product = p;
        _isFilterInGraph = false;
        return true;
      }

      this.LogDebug("SmarDTV USB CI: tuner not linked to any CI products or otherwise not supported");
      _isSmarDtvUsbCi = false;
      return false;
    }

    #endregion

    #region IDirectShowAddOnDevice member

    /// <summary>
    /// Insert and connect additional filter(s) into the graph.
    /// </summary>
    /// <param name="graph">The tuner filter graph.</param>
    /// <param name="lastFilter">The source filter (usually either a capture/receiver or
    ///   multiplexer filter) to connect the [first] additional filter to.</param>
    /// <returns><c>true</c> if one or more additional filters were successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(IFilterGraph2 graph, ref IBaseFilter lastFilter)
    {
      this.LogDebug("SmarDTV USB CI: add to graph");

      if (!_isSmarDtvUsbCi)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (graph == null)
      {
        this.LogError("SmarDTV USB CI: failed to add the filter to the graph, graph is null");
        return false;
      }
      if (lastFilter == null)
      {
        this.LogError("SmarDTV USB CI: failed to add the filter to the graph, last filter is null");
        return false;
      }
      if (_isFilterInGraph)
      {
        this.LogWarn("SmarDTV USB CI: filter already in graph");
        return true;
      }

      bool success = false;
      _graph = graph;
      try
      {
        // Add the CI filter to the graph.
        int hr = _graph.AddFilter(_ciFilter, _product.Name);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("SmarDTV USB CI: failed to add the filter to the graph, hr = 0x{0:x}", hr);
          return false;
        }
        _isFilterInGraph = true;

        // Connect the filter into the graph.
        IPin lastFilterOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        IPin ciFilterInputPin = DsFindPin.ByDirection(_ciFilter, PinDirection.Input, 0);
        try
        {
          if (ciFilterInputPin == null || lastFilterOutputPin == null)
          {
            this.LogError("SmarDTV USB CI: failed to locate required pins");
            return false;
          }
          hr = _graph.ConnectDirect(lastFilterOutputPin, ciFilterInputPin, null);
        }
        finally
        {
          Release.ComObject("SmarDTV upstream filter output pin", ref lastFilterOutputPin);
          Release.ComObject("SmarDTV CI filter input pin", ref ciFilterInputPin);
        }
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("SmarDTV USB CI: failed to connect the filter into the graph, hr = 0x{0:x}", hr);
          return false;
        }

        success = true;
      }
      finally
      {
        if (!success && _isFilterInGraph)
        {
          _isFilterInGraph = false;
          _graph.RemoveFilter(_ciFilter);
        }
      }

      // Success!
      this.LogDebug("SmarDTV USB CI: result = success");
      lastFilter = _ciFilter;
      return true;
    }

    #endregion

    #region ITvServerPlugin members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "SmarDTV USB CI";
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
    /// Get an instance of the configuration section for use in TV Server configuration (SetupTv).
    /// </summary>
    public SectionSettings Setup
    {
      get
      {
        return new SmarDtvUsbCiConfig();
      }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("SmarDTV USB CI: plugin enabled");
      _isPluginEnabled = true;
    }

    /// <summary>
    /// Stop this TV Server plugin.
    /// </summary>
    public void Stop()
    {
      this.LogDebug("SmarDTV USB CI: plugin disabled");
      _isPluginEnabled = false;
    }

    #endregion

    #region ITvServerPluginCommunication members

    /// <summary>
    /// Supply a service class implementation for client-server plugin communication.
    /// </summary>
    public object GetServiceInstance
    {
      get
      {
        return _service;
      }
    }

    /// <summary>
    /// Supply a service class interface for client-server plugin communication.
    /// </summary>
    public Type GetServiceInterfaceForContractType
    {
      get
      {
        return typeof(ISmarDtvUsbCiConfigService);
      }
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("SmarDTV USB CI: open conditional access interface");

      if (!_isSmarDtvUsbCi)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (!_isFilterInGraph)
      {
        this.LogDebug("SmarDTV USB CI: filter not added to the BDA filter graph");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("SmarDTV USB CI: conditional access interface is already open");
        return true;
      }

      // Setup call backs and open the interface.
      _ciCallBack = new CiCallBack();
      _ciCallBack.OnApdu = new OnSmarDtvUsbCiApdu(OnApdu);
      _ciCallBack.OnApplicationInfo = new OnSmarDtvUsbCiApplicationInfo(OnApplicationInfo);
      _ciCallBack.OnCiState = new OnSmarDtvUsbCiState(OnCiState);
      _ciCallBack.OnCloseMmi = new OnSmarDtvUsbCiCloseMmi(OnCloseMmi);
      int hr = _product.OpenInterface(ref _ciCallBack);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("SmarDTV USB CI: failed to open conditional access interface, hr = 0x{0:x}", hr);
        return false;
      }

      VersionInfo versionInfo = new VersionInfo();
      hr = _product.GetVersionInfo(out versionInfo);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("  plugin version     = {0}", versionInfo.PluginVersion);
        this.LogDebug("  BDA driver version = {0}", versionInfo.BdaVersion);
        this.LogDebug("  USB driver version = {0}", versionInfo.UsbVersion);
        this.LogDebug("  firmware version   = {0}", versionInfo.FirmwareVersion);
        this.LogDebug("  FPGA version       = {0}", versionInfo.FpgaVersion);
      }
      else
      {
        this.LogWarn("SmarDTV USB CI: failed to retrieve version information, hr = 0x{0:x}", hr);
      }

      this.LogDebug("SmarDTV USB CI: result = success");
      _isCaInterfaceOpen = true;
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
      this.LogDebug("SmarDTV USB CI: close conditional access interface");

      // I don't know of any way to safely shutdown the interface (stop call
      // backs etc.) except to dispose the filter, which we can't do. It seems
      // that you'll get a BSOD if you invoke USB2CI_Init() with null call back
      // references or a null structure. So the best we can do is not close the
      // interface and warn to the log.
      this.LogWarn("SmarDTV USB CI: not possible to close conditional access interface without destroying graph");
      return false;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      this.LogDebug("SmarDTV USB CI: reset conditional access interface");

      if (!_isSmarDtvUsbCi)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (!_isCaInterfaceOpen)
      {
        this.LogError("SmarDTV USB CI: failed to reset conditional access interface, interface is not open");
        return false;
      }

      this.LogDebug("SmarDTV USB CI: get upstream connection");
      IPin pin = DsFindPin.ByDirection(_ciFilter, PinDirection.Input, 0);
      if (pin == null)
      {
        this.LogError("SmarDTV USB CI: failed to get the CI filter input pin");
        return false;
      }

      IPin connectedPin = null;
      int hr;
      try
      {
        hr = pin.ConnectedTo(out connectedPin);
        if (hr != (int)NativeMethods.HResult.S_OK || connectedPin == null)
        {
          this.LogError("SmarDTV USB CI: failed to get the pin connected to the CI filter input, hr = 0x{0:x}", hr);
          return false;
        }
      }
      finally
      {
        Release.ComObject("SmarDTV USB CI filter input pin", ref pin);
      }

      IBaseFilter connectedFilter = null;
      try
      {
        PinInfo pinInfo;
        hr = connectedPin.QueryPinInfo(out pinInfo);
        if (hr != (int)NativeMethods.HResult.S_OK || pinInfo.filter == null)
        {
          this.LogError("SmarDTV USB CI: failed to get the filter connected to the CI filter input, hr = 0x{0:x}", hr);
          return false;
        }
        connectedFilter = pinInfo.filter;
      }
      finally
      {
        Release.ComObject("SmarDTV USB CI filter input connected pin", ref connectedPin);
      }

      try
      {
        this.LogDebug("SmarDTV USB CI: get downstream connection");
        pin = DsFindPin.ByDirection(_ciFilter, PinDirection.Output, 0);
        if (pin == null)
        {
          this.LogError("SmarDTV USB CI: failed to get the CI filter output pin");
          return false;
        }

        try
        {
          hr = pin.ConnectedTo(out connectedPin);
          if (hr != (int)NativeMethods.HResult.S_OK || connectedPin == null)
          {
            this.LogError("SmarDTV USB CI: failed to get the pin connected to the CI filter output, hr = 0x{0:x}", hr);
            return false;
          }
        }
        finally
        {
          Release.ComObject("SmarDTV USB CI filter output pin", ref pin);
        }

        try
        {
          this.LogDebug("SmarDTV USB CI: reinitialise");
          _isFilterInGraph = false;
          _graph.RemoveFilter(_ciFilter);
          _product.Deinitialise();
          _ciFilter = _product.Initialise();

          IBaseFilter lastFilter = connectedFilter;
          if (!AddToGraph(_graph, ref lastFilter))
          {
            return false;
          }

          this.LogDebug("SmarDTV USB CI: reconnect downstream filter chain");
          pin = DsFindPin.ByDirection(_ciFilter, PinDirection.Output, 0);
          if (pin == null)
          {
            this.LogError("SmarDTV USB CI: failed to get the new CI filter output pin");
            return false;
          }

          try
          {
            hr = _graph.ConnectDirect(pin, connectedPin, null);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              this.LogError("SmarDTV USB CI: failed to reconnect downstream filter chain, hr = 0x{0:x}", hr);
              return false;
            }
            return (this as IConditionalAccessProvider).Open();
          }
          finally
          {
            Release.ComObject("SmarDTV USB CI new filter output pin", ref pin);
          }
        }
        finally
        {
          Release.ComObject("SmarDTV USB CI filter output connected pin", ref connectedPin);
        }
      }
      finally
      {
        Release.ComObject("SmarDTV USB CI filter input connected filter", ref connectedFilter);
      }
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("SmarDTV USB CI: is conditional access interface ready");
      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }

      // The CI/CAM state is automatically updated in the OnCiState() call back.
      this.LogDebug("SmarDTV USB CI: result = {0}", _isCamReady);
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
      this.LogDebug("SmarDTV USB CI: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return true;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        this.LogError("SmarDTV USB CI: conditional access command type {0} is not supported", command);
        return true;
      }
      if (pmt == null)
      {
        this.LogError("SmarDTV USB CI: failed to send conditional access command, PMT not supplied");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CaPmtCommand.NotSelected)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      // During development of this class I tried a couple of tricks to get the WinTV-CI to decrypt
      // more than one channel at a time. I came to the conclusion that it is not possible. I tried:
      // - assemble a fake CA PMT structure and pass it to USB2CI_GuiSendPMT
      // - send a CA PMT APDU using USB2CI_APDUToCAM
      // We'll just send this PMT to the CAM regardless of the list management action.
      ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
      byte[] rawPmtCopy = new byte[rawPmt.Count];
      rawPmt.CopyTo(rawPmtCopy, 0);
      int hr = _product.SendPmt(rawPmtCopy, (short)rawPmt.Count);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to send conditional access command, hr = 0x{0:x}", hr);
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
      this.LogDebug("SmarDTV USB CI: enter menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("SmarDTV USB CI: failed to open menu, the CAM is not ready");
        return false;
      }

      int hr = _product.OpenMmiSession();
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to open menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("SmarDTV USB CI: close menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("SmarDTV USB CI: failed to close menu, the CAM is not ready");
        return false;
      }

      byte[] apdu = DvbMmiHandler.CreateMmiClose(0);
      int hr = _product.SendMmiApdu(apdu.Length, apdu);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to close menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("SmarDTV USB CI: select menu entry, choice = {0}", choice);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("SmarDTV USB CI: failed to select menu entry, the CAM is not ready");
        return false;
      }

      byte[] apdu = DvbMmiHandler.CreateMmiMenuAnswer(choice);
      int hr = _product.SendMmiApdu(apdu.Length, apdu);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to select menu entry, hr = 0x{0:x}", hr);
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
      this.LogDebug("SmarDTV USB CI: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("SmarDTV USB CI: failed to answer enquiry, the CAM is not ready");
        return false;
      }

      MmiResponseType responseType = MmiResponseType.Answer;
      if (cancel)
      {
        responseType = MmiResponseType.Cancel;
      }
      byte[] apdu = DvbMmiHandler.CreateMmiEnquiryAnswer(responseType, answer);
      int hr = _product.SendMmiApdu(apdu.Length, apdu);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to answer enquiry, hr = 0x{0:x}", hr);
      return false;
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

    ~SmarDtvUsbCi()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing && _product != null)
      {
        if (_graph != null)
        {
          if (_isFilterInGraph)
          {
            _isFilterInGraph = false;
            _graph.RemoveFilter(_ciFilter);
          }
          _graph = null;
        }
        if (_ciFilter != null)
        {
          _product.Deinitialise();
          _ciFilter = null;
        }
      }

      _isSmarDtvUsbCi = false;
    }

    #endregion
  }
}