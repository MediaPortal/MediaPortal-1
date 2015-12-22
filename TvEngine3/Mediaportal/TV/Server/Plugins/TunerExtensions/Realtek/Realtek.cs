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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Realtek
{
  /// <summary>
  /// A class for handling PID filter and remote control support for various Realtek RTL283x based
  /// tuners.
  /// </summary>
  public class Realtek : BaseTunerExtension, IDisposable, IMpeg2PidFilter, IRemoteControlListener
  {
    #region enums

    private enum BdaExtensionPropertyDeviceControl
    {
      IrCode = 0,
      UsbMode,
      EnablePid,
      DisablePid,
      DeviceSuspendStatus,
      PidFilterStatus
    }

    private enum BdaExtensionPropertyFilterReadWrite
    {
      Ir = 11
    }

    private enum RtlIrType : int
    {
      Rc6 = 0,
      Rc5,
      Nec
    }

    // DigitalNow Dabby remote control codes.
    // I couldn't find a picture of the remote.
    private enum RtlNecIrCode : uint
    {
      Power = 0x866bc23d,
      Source = 0x866b807f,
      Zoom = 0x866bd02f,
      ShutDown = 0x866bc03f,
      One = 0x866b20df,
      Two = 0x866b10ef,
      Three = 0x866b40bf,
      Four = 0x866bf00f,
      Five = 0x866ba05f,
      Six = 0x866b609f,
      Seven = 0x866b30cf,
      Eight = 0x866bb04f,
      Nine = 0x866b50af,
      Zero = 0x866b8877,
      ChannelUp = 0x866b906f,
      ChannelDown = 0x866be01f,
      VolumeUp = 0x866b708f,
      VolumeDown = 0x866bc837,
      Back = 0x866b08f7,
      Okay = 0x866b48b7,    // enter
      Record = 0x866b28d7,
      Stop = 0x866ba857,
      Play = 0x866b6897,
      Mute = 0x866be817,
      Up = 0x866b18e7,
      Down = 0x866b9867,
      Left = 0x866b58a7,
      Right = 0x866bd827,
      Red = 0x866b38c7,
      Green = 0x866bb847,
      Yellow = 0x866b7887,
      Blue = 0x866bf807
    }

    #endregion

    #region delegates

    /// <summary>
    /// Initialise a Realtek BDA filter.
    /// </summary>
    /// <remarks>
    /// This initialisation is necessary to use the IR functions.
    /// </remarks>
    /// <param name="hwnd">An optional window handle for receiving device change notifications.</param>
    /// <returns><c>true</c> if initialisation is successful, otherwise <c>false</c></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool RTK_BDAFilterInit(IntPtr hwnd);

    /// <summary>
    /// Release (deinitialise) a Realtek BDA filter.
    /// </summary>
    /// <param name="hwnd">The window handle registered with RTK_BDAFilterInit(), if any.</param>
    /// <returns><c>true</c> if release is successful, otherwise <c>false</c></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool RTK_BDAFilterRelease(IntPtr hwnd);

    /// <summary>
    /// Initialise IR access.
    /// </summary>
    /// <returns><c>true</c> if initialisation is successful, otherwise <c>false</c></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool RTK_InitialAPModeIRParameter();

    /// <summary>
    /// Get the IR code type.
    /// </summary>
    /// <param name="irType">The IR code type.</param>
    /// <returns><c>true</c> if the IR code type is retrieved successfully, otherwise <c>false</c></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool RTK_GetAPModeIRCurrentIRType(out RtlIrType irType);

    /// <summary>
    /// Get an IR code, if available.
    /// </summary>
    /// <param name="isIrCodeAvailable"><c>True</c> if an IR code is available.</param>
    /// <param name="irCode">The IR code.</param>
    /// <param name="irCodeSize">The size of the IR code buffer in bytes.</param>
    /// <returns><c>true</c> if the IR code is retrieved successfully, otherwise <c>false</c></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool RTK_GetAPModeIRCode([MarshalAs(UnmanagedType.Bool)] out bool isIrCodeAvailable, out uint irCode, IntPtr irCodeSize);

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET_DEVICE_CONTROL = new Guid(0x1bfb70f7, 0xadfb, 0x4414, 0x9f, 0xd4, 0x60, 0xe9, 0xe5, 0x40, 0xa5, 0x59);
    private static readonly Guid BDA_EXTENSION_PROPERTY_SET_FILTER_READ_WRITE = new Guid(0xc8890094, 0x30e0, 0x4a9f, 0x9c, 0xda, 0x3a, 0x6e, 0xc8, 0x6e, 0xb8, 0xa8);

    private static readonly int KS_PROPERTY_SIZE = Marshal.SizeOf(typeof(KsProperty));    // 24

    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    private bool _isRealtek = false;
    private string _tunerName = null;
    private string _productInstanceId = null;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _generalBuffer = IntPtr.Zero;

    #region remote control

    private bool _isRemoteControlInterfaceOpen = false;
    private RtlIrType _irType = RtlIrType.Nec;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

    // This variable tracks the number of open remote control API instances which corresponds with used DLL indices.
    private static int _remoteControlApiCount = 0;
    private static HashSet<string> _openRemoteControlProducts = new HashSet<string>();

    // Remote control API instance variables.
    private int _remoteControlApiIndex = 0;
    private bool _isRemoteControlDllLoaded = false;
    private IntPtr _remoteControlApiLibHandle = IntPtr.Zero;

    // Delegate instances for the required IR API (RTL283XACCESS) functions.
    private RTK_BDAFilterInit _bdaFilterInit = null;
    private RTK_BDAFilterRelease _bdaFilterRelease = null;
    private RTK_InitialAPModeIRParameter _initialiseIr = null;
    private RTK_GetAPModeIRCurrentIRType _getIrType = null;
    private RTK_GetAPModeIRCode _getIrCode = null;

    #endregion

    private HashSet<ushort> _pidFilterPids = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToAdd = new HashSet<ushort>();
    private bool _isPidFilterDisabled = false;

    #endregion

    /// <summary>
    /// Set the tuner for the remote control API to use.
    /// </summary>
    /// <remarks>
    /// Normally the remote control API will bind to the first tuner which
    /// matches a list of friendly names located in the registry. We manipulate
    /// the registry list to ensure the API binds to a specific tuner/product.
    /// In theory this should enable us to receive remote control inputs from
    /// all Realtek products.
    /// </remarks>
    /// <param name="tunerName">The target tuner's name.</param>
    /// <returns>a token to enable calling ResetRcApiTuner() if successful, otherwise null</returns>
    private string SetRcApiTuner(string tunerName)
    {
      string originalListTunerName = null;
      List<RegistryView> views = new List<RegistryView>() { RegistryView.Default };
      if (OSInfo.OSInfo.Is64BitOs() && IntPtr.Size != 8)
      {
        views.Add(RegistryView.Registry64);
      }
      try
      {
        foreach (RegistryView view in views)
        {
          using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).CreateSubKey(@"SYSTEM\CurrentControlSet\Services\RTL2832UBDA"))
          {
            try
            {
              if (string.IsNullOrEmpty(originalListTunerName))
              {
                originalListTunerName = (string)key.GetValue("FilterName1");
              }
              key.SetValue("FilterName1", tunerName);
            }
            finally
            {
              key.Close();
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Realtek: failed to set remote control API tuner");
      }
      return originalListTunerName;
    }

    /// <summary>
    /// Unset the remote control API's tuner.
    /// </summary>
    /// <param name="token">The configuration token returned from SetRcApiTuner().</param>
    private void ResetRcApiTuner(string token)
    {
      if (string.IsNullOrEmpty(token))
      {
        return;
      }

      List<RegistryView> views = new List<RegistryView>() { RegistryView.Default };
      if (OSInfo.OSInfo.Is64BitOs() && IntPtr.Size != 8)
      {
        views.Add(RegistryView.Registry64);
      }
      try
      {
        foreach (RegistryView view in views)
        {
          using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).CreateSubKey(@"SYSTEM\CurrentControlSet\Services\RTL2832UBDA"))
          {
            try
            {
              key.SetValue("FilterName1", token);
            }
            finally
            {
              key.Close();
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Realtek: failed to reset remote control API tuner");
      }
    }

    /// <summary>
    /// Load a remote control API instance. This involves obtaining delegate
    /// instances for each of the required member functions.
    /// </summary>
    /// <returns><c>true</c> if the instance is successfully loaded, otherwise <c>false</c></returns>
    private bool LoadNewRcApiInstance()
    {
      _remoteControlApiCount++;
      _remoteControlApiIndex = _remoteControlApiCount;
      this.LogDebug("Realtek: loading API, API index = {0}", _remoteControlApiIndex);
      string resourcesFolder = PathManager.BuildAssemblyRelativePath("Resources");
      string fileNameSource = Path.Combine(resourcesFolder, "RTL283XACCESS.dll");
      string fileNameTarget = Path.Combine(resourcesFolder, string.Format("RTL283XACCESS{0}.dll", _remoteControlApiIndex));
      if (!File.Exists(fileNameTarget))
      {
        try
        {
          File.Copy(fileNameSource, fileNameTarget);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Realtek: failed to copy access DLL");
          return false;
        }
      }
      _remoteControlApiLibHandle = NativeMethods.LoadLibrary(fileNameTarget);
      if (_remoteControlApiLibHandle == IntPtr.Zero)
      {
        this.LogError("Realtek: failed to load access DLL");
        return false;
      }

      try
      {
        IntPtr function = NativeMethods.GetProcAddress(_remoteControlApiLibHandle, "RTK_BDAFilterInit");
        if (function == IntPtr.Zero)
        {
          this.LogError("Realtek: failed to locate the RTK_BDAFilterInit function");
          return false;
        }
        try
        {
          _bdaFilterInit = (RTK_BDAFilterInit)Marshal.GetDelegateForFunctionPointer(function, typeof(RTK_BDAFilterInit));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Realtek: failed to load the RTK_BDAFilterInit function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_remoteControlApiLibHandle, "RTK_BDAFilterRelease");
        if (function == IntPtr.Zero)
        {
          this.LogError("Realtek: failed to locate the RTK_BDAFilterRelease function");
          return false;
        }
        try
        {
          _bdaFilterRelease = (RTK_BDAFilterRelease)Marshal.GetDelegateForFunctionPointer(function, typeof(RTK_BDAFilterRelease));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Realtek: failed to load the RTK_BDAFilterRelease function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_remoteControlApiLibHandle, "RTK_InitialAPModeIRParameter");
        if (function == IntPtr.Zero)
        {
          this.LogError("Realtek: failed to locate the RTK_InitialAPModeIRParameter function");
          return false;
        }
        try
        {
          _initialiseIr = (RTK_InitialAPModeIRParameter)Marshal.GetDelegateForFunctionPointer(function, typeof(RTK_InitialAPModeIRParameter));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Realtek: failed to load the RTK_InitialAPModeIRParameter function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_remoteControlApiLibHandle, "RTK_GetAPModeIRCurrentIRType");
        if (function == IntPtr.Zero)
        {
          this.LogError("Realtek: failed to locate the RTK_GetAPModeIRCurrentIRType function");
          return false;
        }
        try
        {
          _getIrType = (RTK_GetAPModeIRCurrentIRType)Marshal.GetDelegateForFunctionPointer(function, typeof(RTK_GetAPModeIRCurrentIRType));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Realtek: failed to load the RTK_GetAPModeIRCurrentIRType function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_remoteControlApiLibHandle, "RTK_GetAPModeIRCode");
        if (function == IntPtr.Zero)
        {
          this.LogError("Realtek: failed to locate the RTK_GetAPModeIRCode function");
          return false;
        }
        try
        {
          _getIrCode = (RTK_GetAPModeIRCode)Marshal.GetDelegateForFunctionPointer(function, typeof(RTK_GetAPModeIRCode));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Realtek: failed to load the RTK_GetAPModeIRCode function");
          return false;
        }

        _isRemoteControlDllLoaded = true;
        return true;
      }
      finally
      {
        if (!_isRemoteControlDllLoaded)
        {
          NativeMethods.FreeLibrary(_remoteControlApiLibHandle);
          _remoteControlApiLibHandle = IntPtr.Zero;
        }
      }
    }

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
        this.LogDebug("Realtek: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Realtek remote control listener";
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
          this.LogWarn("Realtek: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Realtek: failed to join remote control listener thread, aborting thread");
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
      this.LogDebug("Realtek: remote control listener thread start polling");
      try
      {
        bool isIrCodeAvailable;
        uint irCode;
        IntPtr irCodeSize = new IntPtr(4);    // size of irCode
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          if (_getIrCode(out isIrCodeAvailable, out irCode, irCodeSize))
          {
            this.LogError("Realtek: failed to read remote code");
          }
          else if (isIrCodeAvailable)
          {
            this.LogDebug("Realtek: remote control key press, code = {0}", (RtlNecIrCode)irCode);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Realtek: remote control listener thread exception");
        return;
      }
      this.LogDebug("Realtek: remote control listener thread stop polling");
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Realtek: initialising");

      if (_isRealtek)
      {
        this.LogWarn("Realtek: extension already initialised");
        return true;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Realtek: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_FILTER_READ_WRITE, (int)BdaExtensionPropertyFilterReadWrite.Ir, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Realtek: filter read/write property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_DEVICE_CONTROL, (int)BdaExtensionPropertyDeviceControl.IrCode, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Realtek: device control property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      this.LogInfo("Realtek: extension supported");
      _isRealtek = true;

      if (tunerExternalId != null)
      {
        DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
        foreach (DsDevice device in devices)
        {
          if (device != null)
          {
            if (tunerExternalId.Contains(device.DevicePath))
            {
              _tunerName = device.Name;
              _productInstanceId = device.ProductInstanceIdentifier;
            }
            device.Dispose();
          }
        }
      }

      _instanceBuffer = Marshal.AllocCoTaskMem(KS_PROPERTY_SIZE);
      _generalBuffer = Marshal.AllocCoTaskMem(4);

      // We need to sync the state of the PID filter and our PID list. Because we can't query for
      // the current PID list, the only way to do this is to disable the filter (and assume that
      // clears the PID list).
      _isPidFilterDisabled = false;
      (this as IMpeg2PidFilter).Disable();
      return true;
    }

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for a given transmitter.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ShouldEnable(IChannel tuningDetail)
    {
      // Assume it is necessary/desirable to enable the filter. The hardware
      // does have limits... though the exact details are not known.
      return true;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      if (_isPidFilterDisabled)
      {
        _pidFilterPids.Clear();
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        return true;
      }

      this.LogDebug("Realtek: disable PID filter");
      if (!_isRealtek)
      {
        this.LogWarn("Realtek: not initialised or interface not supported");
        return false;
      }

      Marshal.WriteInt32(_generalBuffer, 0, 0);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_DEVICE_CONTROL, (int)BdaExtensionPropertyDeviceControl.PidFilterStatus,
          _instanceBuffer, KS_PROPERTY_SIZE,
          _generalBuffer, 4
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Realtek: result = success");
        _isPidFilterDisabled = true;
        _pidFilterPids.Clear();
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        return true;
      }

      this.LogError("Realtek: failed to disable PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return -1;  // maximum not known
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.UnionWith(pids);
      _pidFilterPidsToRemove.ExceptWith(pids);
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.ExceptWith(pids);
      _pidFilterPidsToRemove.UnionWith(pids);
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ApplyConfiguration()
    {
      if (_pidFilterPidsToAdd.Count == 0 && _pidFilterPidsToRemove.Count == 0)
      {
        return true;
      }

      this.LogDebug("Realtek: apply PID filter configuration");
      if (!_isRealtek)
      {
        this.LogWarn("Realtek: not initialised or interface not supported");
        return false;
      }

      int hr = (int)NativeMethods.HResult.S_OK;
      if (_isPidFilterDisabled)
      {
        this.LogDebug("  enable filter...");
        Marshal.WriteInt32(_generalBuffer, 0, 1);
        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_DEVICE_CONTROL, (int)BdaExtensionPropertyDeviceControl.PidFilterStatus,
            _instanceBuffer, KS_PROPERTY_SIZE,
            _generalBuffer, 4
        );
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Realtek: failed to enable PID filter, hr = 0x{0:x}", hr);
          return false;
        }

        this.LogDebug("Realtek: result = success");
        _isPidFilterDisabled = true;
      }

      if (_pidFilterPidsToRemove.Count > 0)
      {
        this.LogDebug("  disable {0} current PID(s)...", _pidFilterPidsToRemove.Count);
        foreach (ushort pid in _pidFilterPidsToRemove)
        {
          Marshal.WriteInt32(_generalBuffer, 0, pid);
          hr |= _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_DEVICE_CONTROL, (int)BdaExtensionPropertyDeviceControl.DisablePid,
              _instanceBuffer, KS_PROPERTY_SIZE,
              _generalBuffer, 4
          );
        }
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Realtek: failed to disable current PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        _pidFilterPids.ExceptWith(_pidFilterPidsToRemove);
        _pidFilterPidsToRemove.Clear();
      }

      if (_pidFilterPidsToAdd.Count > 0)
      {
        this.LogDebug("  enable {0} new PID(s)...", _pidFilterPidsToAdd.Count);
        foreach (ushort pid in _pidFilterPidsToAdd)
        {
          Marshal.WriteInt32(_generalBuffer, 0, pid);
          hr |= _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_DEVICE_CONTROL, (int)BdaExtensionPropertyDeviceControl.EnablePid,
              _instanceBuffer, KS_PROPERTY_SIZE,
              _generalBuffer, 4
          );
        }
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Realtek: failed to enable new PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        _pidFilterPids.UnionWith(_pidFilterPidsToAdd);
        _pidFilterPidsToAdd.Clear();
      }

      this.LogDebug("Realtek: result = success");
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("Realtek: open remote control interface");

      if (!_isRealtek)
      {
        this.LogWarn("Realtek: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Realtek: remote control interface is already open");
        return true;
      }
      if (string.IsNullOrEmpty(_tunerName) || string.IsNullOrEmpty(_productInstanceId))
      {
        this.LogDebug("Realtek: tuner identifiers are not set");
        return false;
      }
      if (_openRemoteControlProducts.Contains(_productInstanceId))
      {
        this.LogDebug("Realtek: multi-tuner product remote control opened for other tuner");
        return false;
      }

      // Realtek and/or DigitalNow distribute a utiltity that enables the
      // remote to work with popular media software. It seems that the utility
      // may cause RTK_GetAPModeIRCode() to fail sometimes (due to hardware
      // contention???). Therefore it would be desirable to automatically stop
      // and restart the utility. Unfortunately we currently have no way to
      // restart the utility, so we can only warn when we detect that it is
      // running.
      if (Process.GetProcessesByName("RTLRCtl").Length > 0)
      {
        this.LogWarn("Realtek: remote control utility (RTLRCtl.exe) is running");
      }

      if (!LoadNewRcApiInstance())
      {
        return false;
      }

      string resetToken = null;
      try
      {
        resetToken = SetRcApiTuner(_tunerName);

        if (!_bdaFilterInit(IntPtr.Zero))
        {
          this.LogError("Realtek: failed to initialise BDA filter");
          return false;
        }

        if (!_initialiseIr())
        {
          this.LogError("Realtek: failed to initialise remote control API");
          _bdaFilterRelease(IntPtr.Zero);
          return false;
        }

        if (_getIrType(out _irType))
        {
          this.LogDebug("Realtek: IR type = {0}", _irType);
        }
        else
        {
          this.LogWarn("Realtek: failed to get IR type, defaulting to NEC");
          _irType = RtlIrType.Nec;
        }

        _isRemoteControlInterfaceOpen = true;
      }
      finally
      {
        ResetRcApiTuner(resetToken);
        if (!_isRemoteControlInterfaceOpen)
        {
          NativeMethods.FreeLibrary(_remoteControlApiLibHandle);
          _remoteControlApiLibHandle = IntPtr.Zero;
          _isRemoteControlDllLoaded = false;
        }
      }

      _openRemoteControlProducts.Add(_productInstanceId);
      StartRemoteControlListenerThread();

      this.LogDebug("Realtek: result = success");
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
      this.LogDebug("Realtek: close remote control interface");

      if (isDisposing)
      {
        StopRemoteControlListenerThread();

        if (_isRemoteControlInterfaceOpen)
        {
          _bdaFilterRelease(IntPtr.Zero);
          _openRemoteControlProducts.Remove(_productInstanceId);
        }
      }

      if (_remoteControlApiLibHandle != IntPtr.Zero)
      {
        NativeMethods.FreeLibrary(_remoteControlApiLibHandle);
        _remoteControlApiLibHandle = IntPtr.Zero;
        _isRemoteControlDllLoaded = false;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Realtek: result = success");
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

    ~Realtek()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isRealtek)
      {
        CloseRemoteControlListenerInterface(isDisposing);
      }
      if (isDisposing)
      {
        _propertySet = null;
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isRealtek = false;
    }

    #endregion
  }
}