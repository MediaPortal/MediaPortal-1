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
using System.Globalization;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi
{
  /// <summary>
  /// A class for handling conditional access with the Hauppauge WinTV-CI and TerraTec USB CI. Both
  /// devices are based on an OEM design by SmarDTV.
  /// </summary>
  public class SmarDtvUsbCi : BaseCustomDevice, IDirectShowAddOnDevice, IConditionalAccessProvider, IConditionalAccessMenuActions, ITvServerPlugin
  {
    #region enums

    private enum SmarDtvCiState : int
    {
      Unplugged = 0,
      Empty,
      CamPresent
    }

    #endregion

    #region call back definitions

    /// <summary>
    /// Called by the driver when the CI slot state changes.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <returns>an HRESULT indicating whether the state change was successfully handled</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int OnSmarDtvUsbCiState(IBaseFilter ciFilter, SmarDtvCiState state);

    /// <summary>
    /// Called by the driver when application information is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="info">A buffer containing the application information.</param>
    /// <returns>an HRESULT indicating whether the application information was successfully processed</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int OnSmarDtvUsbCiApplicationInfo(IBaseFilter ciFilter, IntPtr info);

    /// <summary>
    /// Called by the driver when a CAM wants to close an MMI session.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <returns>an HRESULT indicating whether the MMI session was successfully closed</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int OnSmarDtvUsbCiCloseMmi(IBaseFilter ciFilter);

    /// <summary>
    /// Called by the driver when an application protocol data unit is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="apduLength">The length of the APDU buffer in bytes.</param>
    /// <param name="apdu">A buffer containing the APDU.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully processed</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int OnSmarDtvUsbCiApdu(IBaseFilter ciFilter, int apduLength, IntPtr apdu);

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SmarDtvUsbCiCallBacks
    {
      public IntPtr Context;  // Optional context that the interface will pass back as a parameter when the delegates are executed.
      public OnSmarDtvUsbCiState OnCiState;
      public OnSmarDtvUsbCiApplicationInfo OnApplicationInfo;
      public OnSmarDtvUsbCiCloseMmi OnCloseMmi;
      public OnSmarDtvUsbCiApdu OnApdu;
    }

    #pragma warning disable 0649, 0169
    // These structs are used - they are marshaled from the COM interface.

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct VersionInfo
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
      public string PluginVersion;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
      public string BdaVersion;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
      public string UsbVersion;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
      public string FirmwareVersion;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
      public string FpgaVersion;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ApplicationInfo
    {
      public MmiApplicationType ApplicationType;
      private byte Padding;
      public ushort Manufacturer;
      public ushort Code;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 352)]
      public byte[] MenuTitle;
    }

    #pragma warning restore 0649, 0169

    #endregion

    #region constants

    //private static readonly int APPLICATION_INFO_SIZE = Marshal.SizeOf(typeof(ApplicationInfo));    // 358
    private static readonly int VERSION_INFO_SIZE = Marshal.SizeOf(typeof(VersionInfo));            // 52
    private static readonly int CI_CALLBACKS_SIZE = Marshal.SizeOf(typeof(SmarDtvUsbCiCallBacks));  // 20

    #endregion

    #region variables

    // We use this hash to keep track of the devices that are in use. Each CI device can only be
    // used with one tuner at any given time, and only one CI device of each brand may be connected
    // to a single system.
    private static HashSet<string> _devicesInUse = new HashSet<string>();

    private bool _isSmarDtvUsbCi = false;
    private bool _isCaInterfaceOpen = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private SmarDtvCiState _ciState = SmarDtvCiState.Empty;

    private IBaseFilter _ciFilter = null;
    private Type _ciType = null;
    private DsDevice _ciDevice = null;
    private IFilterGraph2 _graph = null;

    // Call backs
    private IConditionalAccessMenuCallBacks _caMenuCallBacks = null;
    private object _caMenuCallBackLock = new object();
    private SmarDtvUsbCiCallBacks _ciCallBacks;
    private IntPtr _ciCallBackBuffer = IntPtr.Zero;

    #endregion

    #region call back handlers

    /// <summary>
    /// Called by the driver when the CI slot state changes.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <returns>an HRESULT indicating whether the state change was successfully handled</returns>
    private int OnCiState(IBaseFilter ciFilter, SmarDtvCiState state)
    {
      this.LogInfo("SmarDTV USB CI: CI state change call back");
      this.LogInfo("  old state  = {0}", _ciState);
      this.LogInfo("  new state  = {0}", state);
      _ciState = state;

      _isCamReady = false;
      if (state == SmarDtvCiState.Empty)
      {
        _isCamPresent = false;
      }
      else if (state == SmarDtvCiState.CamPresent)
      {
        _isCamPresent = true;
      }
      return 0;
    }

    /// <summary>
    /// Called by the driver when application information is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="info">A buffer containing the application information.</param>
    /// <returns>an HRESULT indicating whether the application information was successfully processed</returns>
    private int OnApplicationInfo(IBaseFilter ciFilter, IntPtr info)
    {
      this.LogInfo("SmarDTV USB CI: application information call back");
      //Dump.DumpBinary(info, APPLICATION_INFO_SIZE);
      ApplicationInfo appInfo = (ApplicationInfo)Marshal.PtrToStructure(info, typeof(ApplicationInfo));
      this.LogDebug("  type         = {0}", appInfo.ApplicationType);
      // Note: current drivers seem to have a bug that causes only the first byte in the manufacturer and code
      // fields to be available.
      this.LogDebug("  manufacturer = 0x{0:x4}", appInfo.Manufacturer);
      this.LogDebug("  code         = 0x{0:x4}", appInfo.Code);
      this.LogDebug("  menu title   = {0}", DvbTextConverter.Convert(appInfo.MenuTitle));

      // Receiving application information indicates that the CAM is ready for interaction.
      _isCamPresent = true;
      _isCamReady = true;
      return 0;
    }

    /// <summary>
    /// Called by the driver when a CAM wants to close an MMI session.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <returns>an HRESULT indicating whether the MMI session was successfully closed</returns>
    private int OnCloseMmi(IBaseFilter ciFilter)
    {
      this.LogInfo("SmarDTV USB CI: close MMI call back");
      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBacks != null)
        {
          _caMenuCallBacks.OnCiCloseDisplay(0);
        }
        else
        {
          this.LogDebug("SmarDTV USB CI: menu call backs are not set");
        }
      }
      return 0;
    }

    /// <summary>
    /// Called by the driver when an application protocol data unit is received from a CAM.
    /// </summary>
    /// <param name="ciFilter">The CI filter.</param>
    /// <param name="apduLength">The length of the APDU buffer in bytes.</param>
    /// <param name="apdu">A buffer containing the APDU.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully processed</returns>
    private int OnApdu(IBaseFilter ciFilter, int apduLength, IntPtr apdu)
    {
      this.LogInfo("SmarDTV USB CI: APDU call back");

      //Dump.DumpBinary(apdu, apduLength);
      byte[] apduBytes = new byte[apduLength];
      Marshal.Copy(apdu, apduBytes, 0, apduLength);
      lock (_caMenuCallBackLock)
      {
        DvbMmiHandler.HandleMmiData(apduBytes, _caMenuCallBacks);
      }
      return 0;
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
      this.LogDebug("SmarDTV USB CI: initialising");

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

      // A machine may only have one instance of each OEM product installed - this is a driver limitation. It
      // is unknown whether a single machine may have multiple instances by connecting instances of different
      // products (we don't explicitly prevent this). The TV Server plugin allows each OEM CI product to be
      // linked to a single tuner. Here we need to know whether this tuner (ie. the one referred to by the
      // external identifier) is currently linked to any of the products.
      Card tuner = CardManagement.GetCardByDevicePath(tunerExternalId, CardIncludeRelationEnum.None);
      if (tuner == null)
      {
        this.LogError("SmarDTV USB CI: tuner external identifier {0} not found in database", tunerExternalId);
        return false;
      }

      string tunerIdAsString = tuner.IdCard.ToString(CultureInfo.InvariantCulture);
      ReadOnlyCollection<SmarDtvUsbCiProduct> productList = SmarDtvUsbCiProduct.GetProductList();
      foreach (SmarDtvUsbCiProduct p in productList)
      {
        if (!SettingsManagement.GetValue(p.DbSettingName, "-1").Equals(tunerIdAsString))
        {
          continue;
        }

        this.LogDebug("SmarDTV USB CI: this is the preferred tuner for CI product \"{0}\"", p.ProductName);
        lock (_devicesInUse)
        {
          // Check if the CI device is actually installed in this system.
          DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
          try
          {
            foreach (DsDevice captureDevice in captureDevices)
            {
              if (captureDevice.Name != null && captureDevice.Name.Equals(p.BdaDeviceName))
              {
                this.LogDebug("SmarDTV USB CI: found corresponding CI device");
                if (_devicesInUse.Contains(captureDevice.DevicePath))
                {
                  this.LogDebug("SmarDTV USB CI: the CI device is already in use");
                  continue;
                }
                this.LogInfo("SmarDTV USB CI: extension supported");
                _isSmarDtvUsbCi = true;
                _ciType = p.ComInterface;
                _ciDevice = captureDevice;
                _devicesInUse.Add(_ciDevice.DevicePath);
                return true;
              }
            }
          }
          finally
          {
            foreach (DsDevice d in captureDevices)
            {
              if (d != _ciDevice)
              {
                d.Dispose();
              }
            }
          }
        }
        this.LogDebug("SmarDTV USB CI: CI device not found");
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

      if (!_isSmarDtvUsbCi || _ciDevice == null)
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
      if (_ciFilter != null)
      {
        this.LogWarn("SmarDTV USB CI: filter already in graph");
        return true;
      }

      bool success = false;
      _graph = graph;
      try
      {
        // Add the CI filter to the graph.
        int hr = _graph.AddSourceFilterForMoniker(_ciDevice.Mon, null, _ciDevice.Name, out _ciFilter);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("SmarDTV USB CI: failed to add the filter to the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        // Connect the filter into the graph.
        IPin tmpOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        IPin tmpInputPin = DsFindPin.ByDirection(_ciFilter, PinDirection.Input, 0);
        try
        {
          if (tmpInputPin == null || tmpOutputPin == null)
          {
            this.LogError("SmarDTV USB CI: failed to locate required pins");
            return false;
          }
          hr = _graph.ConnectDirect(tmpOutputPin, tmpInputPin, null);
        }
        finally
        {
          Release.ComObject("SmarDTV upstream filter output pin", ref tmpOutputPin);
          Release.ComObject("SmarDTV CI filter input pin", ref tmpInputPin);
        }
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("SmarDTV USB CI: failed to connect the CI filter into the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        success = true;
      }
      finally
      {
        if (!success)
        {
          // We're not too worried about cleanup here as Dispose() will be called shortly. We just want to make
          // sure that we don't leave the CI filter in the graph if it can't be used.
          _graph.RemoveFilter(_ciFilter);
          Release.ComObject("SmarDTV CI filter", ref _ciFilter);
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
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
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
    public Mediaportal.TV.Server.SetupControls.SectionSettings Setup
    {
      get { return new SmarDtvUsbCiConfig(); }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
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
    public bool OpenConditionalAccessInterface()
    {
      this.LogDebug("SmarDTV USB CI: open conditional access interface");

      if (!_isSmarDtvUsbCi)
      {
        this.LogWarn("SmarDTV USB CI: not initialised or interface not supported");
        return false;
      }
      if (_ciFilter == null)
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
      _ciCallBacks = new SmarDtvUsbCiCallBacks();
      _ciCallBacks.OnApdu = new OnSmarDtvUsbCiApdu(OnApdu);
      _ciCallBacks.OnApplicationInfo = new OnSmarDtvUsbCiApplicationInfo(OnApplicationInfo);
      _ciCallBacks.OnCiState = new OnSmarDtvUsbCiState(OnCiState);
      _ciCallBacks.OnCloseMmi = new OnSmarDtvUsbCiCloseMmi(OnCloseMmi);
      _ciCallBackBuffer = Marshal.AllocCoTaskMem(CI_CALLBACKS_SIZE);
      Marshal.StructureToPtr(_ciCallBacks, _ciCallBackBuffer, false);
      int hr = (int)_ciType.GetMethod("USB2CI_Init").Invoke(_ciFilter, new object[] { _ciCallBackBuffer });
      if (hr == (int)HResult.Severity.Success)
      {
        IntPtr versionInfoBuffer = Marshal.AllocCoTaskMem(VERSION_INFO_SIZE);
        try
        {
          for (byte i = 0; i < VERSION_INFO_SIZE; i++)
          {
            Marshal.WriteByte(versionInfoBuffer, i, 0);
          }
          hr = (int)_ciType.GetMethod("USB2CI_GetVersion").Invoke(_ciFilter, new object[] { versionInfoBuffer });
          if (hr == (int)HResult.Severity.Success)
          {
            //Dump.DumpBinary(versionBuffer, VERSION_INFO_SIZE);
            VersionInfo versionInfo = (VersionInfo)Marshal.PtrToStructure(versionInfoBuffer, typeof(VersionInfo));
            this.LogDebug("  plugin version     = {0}", versionInfo.PluginVersion);
            this.LogDebug("  BDA driver version = {0}", versionInfo.BdaVersion);
            this.LogDebug("  USB driver version = {0}", versionInfo.UsbVersion);
            this.LogDebug("  firmware version   = {0}", versionInfo.FirmwareVersion);
            this.LogDebug("  FPGA version       = {0}", versionInfo.FpgaVersion);
          }
          else
          {
            this.LogWarn("SmarDTV USB CI: failed to retrieve version information, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(versionInfoBuffer);
        }

        this.LogDebug("SmarDTV USB CI: result = success");
        _isCaInterfaceOpen = true;
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to open conditional access interface, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseConditionalAccessInterface()
    {
      this.LogDebug("SmarDTV USB CI: close conditional access interface");

      _isCamPresent = false;
      _isCamReady = false;
      if (_ciCallBackBuffer != IntPtr.Zero)
      {
        Marshal.Release(_ciCallBackBuffer);
        _ciCallBackBuffer = IntPtr.Zero;
      }

      this.LogDebug("SmarDTV USB CI: result = success");
      _isCaInterfaceOpen = false;
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="resetTuner">This parameter will be set to <c>true</c> if the tuner must be reset
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    public bool ResetConditionalAccessInterface(out bool resetTuner)
    {
      resetTuner = true;
      return true;
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsConditionalAccessInterfaceReady()
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
    public bool SendConditionalAccessCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
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
      int hr = (int)_ciType.GetMethod("USB2CI_GuiSendPMT").Invoke(_ciFilter, new object[] { rawPmtCopy, (short)rawPmt.Count });
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to send conditional access command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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

      int hr = (int)_ciType.GetMethod("USB2CI_OpenMMI").Invoke(_ciFilter, null);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to open menu, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseMenu()
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
      int hr = (int)_ciType.GetMethod("USB2CI_APDUToCAM").Invoke(_ciFilter, new object[] { apdu.Length, apdu });
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to close menu, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenuEntry(byte choice)
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
      int hr = (int)_ciType.GetMethod("USB2CI_APDUToCAM").Invoke(_ciFilter, new object[] { apdu.Length, apdu });
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to select menu entry, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
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
      int hr = (int)_ciType.GetMethod("USB2CI_APDUToCAM").Invoke(_ciFilter, new object[] { apdu.Length, apdu });
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("SmarDTV USB CI: result = success");
        return true;
      }

      this.LogError("SmarDTV USB CI: failed to answer enquiry, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_isSmarDtvUsbCi)
      {
        CloseConditionalAccessInterface();
      }
      if (_graph != null)
      {
        _graph.RemoveFilter(_ciFilter);
        Release.ComObject("SmarDTV CI graph", ref _graph);
      }
      Release.ComObject("SmarDTV CI filter", ref _ciFilter);

      if (_ciDevice != null)
      {
        lock (_devicesInUse)
        {
          _devicesInUse.Remove(_ciDevice.DevicePath);
        }
        _ciDevice.Dispose();
        _ciDevice = null;
      }

      _isSmarDtvUsbCi = false;
    }

    #endregion
  }
}