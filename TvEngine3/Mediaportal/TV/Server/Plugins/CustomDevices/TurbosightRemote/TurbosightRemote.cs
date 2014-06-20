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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.TurbosightRemote
{
  #region COM interfaces

  /// <summary>
  /// MediaPortal's wrapper class for the TBS NXP infra red remote control
  /// receiver.
  /// </summary>
  [Guid("fa698ab4-c8fe-4823-a638-4d1f2f2bf8bf")]
  internal class MpTbsNxpIrRcReceiver
  {
  }

  /// <summary>
  /// A call back interface for receiving remote control key press events from
  /// the TBS NXP IR RC receiver wrapper.
  /// </summary>
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IRemoteControlKeyPressCallBack
  {
    /// <summary>
    /// Invoked by the wrapper when a remote control key press event is fired
    /// by the underlying driver.
    /// </summary>
    /// <param name="keyCode">The remote control key's unique code.</param>
    /// <param name="protocol">The protocol/format used to detect and process the code.</param>
    /// <param name="context">The optional context passed to the wrapper when the interface was initialised.</param>
    /// <returns>an HRESULT indicating whether the event was handled successfully</returns>
    [PreserveSig]
    int OnKeyPress(uint keyCode, TbsNxpRcProtocol protocol, IntPtr context);
  }

  /// <summary>
  /// TBS NXP supported infra red remote control protocols.
  /// </summary>
  public enum TbsNxpRcProtocol : int
  {
    /// <summary>
    /// Philips RC-5.
    /// </summary>
    Rc5 = 0,
    /// <summary>
    /// Philips RC-6.
    /// </summary>
    Rc6,
    /// <summary>
    /// NEC 32 bit.
    /// </summary>
    Nec32,
    /// <summary>
    /// NEC 40 bit.
    /// </summary>
    Nec40
  }

  /// <summary>
  /// The main interface on the TBS NXP IR RC receiver wrapper class.
  /// </summary>
  [Guid("1cde3f2e-c771-4e42-9b9b-c32bd2467fcb"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMpTbsNxpIrRcReceiver
  {
    [PreserveSig]
    int Initialise(IKsControl control, TbsNxpRcProtocol protocol, IRemoteControlKeyPressCallBack callBack, IntPtr callBackContext);

    [PreserveSig]
    int Dispose();
  }

  #endregion

  /// <summary>
  /// A class for handling remote controls for Turbosight PCI and PCIe tuners
  /// based on Conexant and NXP chipsets.
  /// </summary>
  public class TurbosightRemote : BaseCustomDevice, IRemoteControlListener, IRemoteControlKeyPressCallBack
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Code = 0,
      Command
    }

    private enum TbsIrCommand : byte
    {
      Start = 1,
      Stop,
      Flush
    }

    /// <remarks>
    /// Image:
    ///   v1 = http://www.tbsdtv.com/products/images/tbs6981/tbs6981_4.jpg
    ///   v2 = http://kubik-digital.com/wp-content/uploads/2013/10/41zlbmefDGL4.jpg
    /// Testing: v1 (TBS5980 CI), v2 (TBS5980 CI, TBS6991)
    /// </remarks>
    private enum TbsRemoteCodeBig : byte
    {
      Recall = 128,       // text [v1]: recall, text [v2]: back
      Up,
      Right,
      Record,
      Power,
      Three,
      Two,
      One,
      Down,
      Six,
      Five,
      Four,
      VolumeDown, // 140  // overlay [v2]: blue
      Nine,
      Eight,
      Seven,
      Left,
      ChannelDown,        // overlay [v2]: yellow
      Zero,
      VolumeUp,           // overlay [v2]: green
      Mute,
      Favourites,         // overlay [v1]: green
      ChannelUp,  // 150  // overlay [v2]: red
      Subtitles,
      Pause,
      Okay,
      Screenshot,
      Mode,
      Epg,
      Zoom,               // overlay [v1]: yellow
      Menu,               // overlay [v1]: red
      Exit, // 159        // overlay [v1]: blue

      Asterix = 209,
      Hash = 210,
      Clear = 212,

      SkipForward = 216,
      SkipBack,
      FastForward,
      Rewind,
      Stop,
      Tv,
      Play  // 222
    }

    /// <remarks>
    /// Image: [none]
    /// Testing: untested, based on old SDK.
    /// </remarks>
    private enum TbsRemoteCodeSmall : byte
    {
      Mute = 1,
      Left,
      Down,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,  // 10

      FullScreen = 12,
      Okay = 15,
      Exit = 18,
      Right = 26,
      Eight = 27,
      Up = 30,
      Nine = 31
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct IrData
    {
      public uint Address;
      public uint Command;
    }

    private class ReceiverConexant
    {
      public string Name = string.Empty;
      public string DevicePath = string.Empty;
      public IKsPropertySet PropertySet = null;
    }

    private class ReceiverNxp
    {
      public string Name = string.Empty;
      public string DevicePath = string.Empty;
      public IKsControl Control = null;
      public IMpTbsNxpIrRcReceiver Receiver = null;
      public IntPtr Context = IntPtr.Zero;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xb51c4994, 0x0054, 0x4749, 0x82, 0x43, 0x02, 0x9a, 0x66, 0x86, 0x36, 0x36);

    private static readonly int KS_PROPERTY_SIZE = Marshal.SizeOf(typeof(KsProperty));    // 24
    private static readonly int IR_DATA_SIZE = Marshal.SizeOf(typeof(IrData));            // 8

    private const byte MIN_BIG_REMOTE_CODE = 128;
    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    // This plugin is not tuner-specific. We use this variable to restrict to
    // one instance.
    private static bool _isLoaded = false;
    private static object _instanceLock = new object();

    private bool _isTurbosightRemote = false;
    private List<ReceiverConexant> _receiversConexant = new List<ReceiverConexant>();
    private List<ReceiverNxp> _receiversNxp = new List<ReceiverNxp>();
    private IntPtr _commandBuffer = IntPtr.Zero;
    private IntPtr _codeBuffer = IntPtr.Zero;

    private bool _isRemoteControlInterfaceOpen = false;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

    #endregion

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for Conexant remote control commands.
    /// </summary>
    private void StartRemoteControlListenerThread()
    {
      // Don't start a thread if the interface has not been opened or we have
      // no Conexant receivers.
      if (!_isRemoteControlInterfaceOpen || _receiversConexant.Count == 0)
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
        this.LogDebug("Turbosight remote: starting new Conexant remote control listener thread");
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Turbosight remote Conexant remote control listener";
        _remoteControlListenerThread.IsBackground = true;
        _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
        _remoteControlListenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for Conexant remote control commands.
    /// </summary>
    private void StopRemoteControlListenerThread()
    {
      if (_remoteControlListenerThread != null)
      {
        if (!_remoteControlListenerThread.IsAlive)
        {
          this.LogWarn("Turbosight remote: aborting old Conexant remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Turbosight remote: failed to join Conexant remote control listener thread, aborting thread");
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
    /// Thread function for receiving Conexant remote control commands.
    /// </summary>
    private void RemoteControlListener()
    {
      this.LogDebug("Turbosight remote: Conexant remote control listener thread start polling");

      for (int i = 0; i < KS_PROPERTY_SIZE; i++)
      {
        Marshal.WriteByte(_commandBuffer, i, 0);
      }
      int hr;
      int returnedByteCount;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          foreach (ReceiverConexant r in _receiversConexant)
          {
            hr = r.PropertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Code,
              _commandBuffer, KS_PROPERTY_SIZE,
              _codeBuffer, IR_DATA_SIZE,
              out returnedByteCount
            );
            if (hr != (int)HResult.Severity.Success || returnedByteCount != IR_DATA_SIZE)
            {
              this.LogError("Turbosight remote: failed to read Conexant remote code, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
            }
            else
            {
              IrData data = (IrData)Marshal.PtrToStructure(_codeBuffer, typeof(IrData));
              if (data.Address != 0xffff && data.Command != 0xffff && data.Command != 0xff && data.Command != 0)
              {
                this.LogDebug("Turbosight remote: Conexant remote control key press, address = 0x{0:x8}, command = 0x{1:x8}", data.Address, data.Command);
              }
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Turbosight remote: Conexant remote control listener thread exception");
        return;
      }
      this.LogDebug("Turbosight remote: Conexant remote control listener thread stop polling");
    }

    #endregion

    private List<ReceiverConexant> FindConexantReceivers()
    {
      this.LogDebug("Turbosight remote: find Conexant receivers");
      List<ReceiverConexant> receivers = new List<ReceiverConexant>();
      int hr;
      bool isTbsIrFilter = false;
      Guid filterClsid = typeof(IBaseFilter).GUID;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
      try
      {
        foreach (DsDevice device in devices)
        {
          if (string.IsNullOrEmpty(device.Name) || string.IsNullOrEmpty(device.DevicePath))
          {
            continue;
          }

          this.LogDebug("Turbosight remote:   check {0} {1}", device.Name, device.DevicePath);
          object obj = null;
          try
          {
            device.Mon.BindToObject(null, null, ref filterClsid, out obj);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Turbosight remote: failed to create filter instance for {0} {1}", device.Name, device.DevicePath);
            continue;
          }

          isTbsIrFilter = false;
          try
          {
            IKsPropertySet propertySet = obj as IKsPropertySet;
            if (propertySet == null)
            {
              this.LogDebug("Turbosight remote:     filter is not a property set");
              continue;
            }

            KSPropertySupport support;
            hr = propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, out support);
            if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
            {
              this.LogDebug("Turbosight remote:     property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              continue;
            }

            this.LogDebug("Turbosight remote:     property set supported");
            isTbsIrFilter = true;
            ReceiverConexant r = new ReceiverConexant();
            r.Name = device.Name;
            r.DevicePath = device.DevicePath;
            r.PropertySet = propertySet;
            receivers.Add(r);
          }
          finally
          {
            if (!isTbsIrFilter)
            {
              Release.ComObject("Turbosight remote Conexant IR filter candidate", ref obj);
            }
          }
        }
      }
      finally
      {
        foreach (DsDevice device in devices)
        {
          device.Dispose();
        }
      }
      return receivers;
    }

    private List<ReceiverNxp> FindNxpReceivers()
    {
      this.LogDebug("Turbosight remote: find NXP receivers");
      List<ReceiverNxp> receivers = new List<ReceiverNxp>();
      bool isTbsIrFilter = false;
      Guid filterClsid = typeof(IBaseFilter).GUID;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.DeviceControlCategory);
      try
      {
        foreach (DsDevice device in devices)
        {
          if (string.IsNullOrEmpty(device.Name) || string.IsNullOrEmpty(device.DevicePath))
          {
            continue;
          }

          this.LogDebug("Turbosight remote:   check {0} {1}", device.Name, device.DevicePath);
          if (!device.Name.Contains("TBS") && !device.Name.Contains("TT-budget"))
          {
            this.LogDebug("Turbosight remote:     name doesn't match patterns");
            continue;
          }

          object obj = null;
          try
          {
            device.Mon.BindToObject(null, null, ref filterClsid, out obj);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Turbosight remote: failed to create filter instance for {0} {1}", device.Name, device.DevicePath);
            continue;
          }

          isTbsIrFilter = false;
          try
          {
            IKsControl control = obj as IKsControl;
            if (control == null)
            {
              this.LogDebug("Turbosight remote:     filter is not a control");
              Release.ComObject("Turbosight remote NXP IR filter candidate", ref obj);
              continue;
            }

            this.LogDebug("Turbosight remote:     control supported");
            isTbsIrFilter = true;
            ReceiverNxp r = new ReceiverNxp();
            r.Name = device.Name;
            r.DevicePath = device.DevicePath;
            r.Control = control;
            r.Context = Marshal.AllocCoTaskMem(1);
            Marshal.WriteByte(r.Context, 0, (byte)receivers.Count);
            receivers.Add(r);
          }
          finally
          {
            if (!isTbsIrFilter)
            {
              Release.ComObject("Turbosight remote NXP IR filter candidate", ref obj);
            }
          }
        }
      }
      finally
      {
        foreach (DsDevice device in devices)
        {
          device.Dispose();
        }
      }
      return receivers;
    }

    #region IRemoteControlKeyPressCallBack member

    /// <summary>
    /// Invoked by the wrapper when a remote control key press event is fired
    /// by the underlying driver.
    /// </summary>
    /// <param name="keyCode">The remote control key's unique code.</param>
    /// <param name="protocol">The protocol/format used to detect and process the code.</param>
    /// <param name="context">The optional context passed to the wrapper when the interface was initialised.</param>
    /// <returns>an HRESULT indicating whether the event was handled successfully</returns>
    public int OnKeyPress(uint keyCode, TbsNxpRcProtocol protocol, IntPtr context)
    {
      byte receiverIndex = 0xff;
      if (context != IntPtr.Zero)
      {
        receiverIndex = Marshal.ReadByte(context, 0);
      }

      if (protocol == TbsNxpRcProtocol.Rc5)
      {
        uint fieldBit = (keyCode & 0x1000) >> 12;
        uint toggleBit = (keyCode & 0x800) >> 11;
        uint systemAddress = (keyCode & 0x7c0) >> 6;
        uint rc5Code = keyCode & 0x3f;
        this.LogDebug("Turbosight remote: NXP RC-5 remote control key press, receiver = {0}, field bit = {1}, toggle bit = {2} system address = {3}, code = {4}", receiverIndex, fieldBit, toggleBit, systemAddress, rc5Code);
      }
      else if (protocol == TbsNxpRcProtocol.Rc6)
      {
        uint header = keyCode >> 16;
        uint control = ((keyCode >> 8) & 0xff);
        uint information = keyCode & 0xff;
        this.LogDebug("Turbosight remote: NXP RC-6 remote control key press, receiver = {0}, header = {1}, control = {2}, information = {3}", receiverIndex, header, control, information);
      }
      else if (protocol == TbsNxpRcProtocol.Nec32 || protocol == TbsNxpRcProtocol.Nec40)
      {
        uint address = keyCode >> 8;
        uint command = keyCode & 0xff;
        if (command < MIN_BIG_REMOTE_CODE)
        {
          this.LogDebug("Turbosight remote: NXP NEC small remote control key press, receiver = {0}, address = {1}, command = {2}", receiverIndex, address, (TbsRemoteCodeSmall)command);
        }
        else
        {
          this.LogDebug("Turbosight remote: NXP NEC big remote control key press, receiver = {0}, address = {1}, command = {2}", receiverIndex, address, (TbsRemoteCodeBig)command);
        }
      }
      return (int)HResult.Severity.Success;
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Turbosight remote";
      }
    }

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
      this.LogDebug("Turbosight remote: initialising");

      if (_isTurbosightRemote)
      {
        this.LogWarn("Turbosight remote: extension already initialised");
        return true;
      }

      lock (_instanceLock)
      {
        if (_isLoaded)
        {
          this.LogDebug("Turbosight remote: already loaded");
          return false;
        }

        _receiversConexant = FindConexantReceivers();
        _receiversNxp = FindNxpReceivers();
        _isLoaded = true;
      }

      if (_receiversConexant.Count == 0 && _receiversNxp.Count == 0)
      {
        this.LogDebug("Turbosight remote: no supported receivers detected");
        return false;
      }

      this.LogInfo("Turbosight remote: extension supported, {0} Conexant receiver(s), {1} NXP receiver(s)", _receiversConexant.Count, _receiversNxp.Count);
      _isTurbosightRemote = true;
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenRemoteControlInterface()
    {
      this.LogDebug("Turbosight remote: open remote control interface");

      if (!_isTurbosightRemote)
      {
        this.LogWarn("Turbosight remote: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Turbosight remote: remote control interface is already open");
        return true;
      }

      int hr;
      int i;
      if (_receiversConexant.Count > 0)
      {
        this.LogDebug("Turbosight remote: starting {0} Conexant receiver(s)", _receiversConexant.Count);
        _commandBuffer = Marshal.AllocCoTaskMem(KS_PROPERTY_SIZE);
        _codeBuffer = Marshal.AllocCoTaskMem(IR_DATA_SIZE);
        Marshal.WriteByte(_commandBuffer, 0, (byte)TbsIrCommand.Start);
        i = 1;
        foreach (ReceiverConexant r in _receiversConexant)
        {
          hr = r.PropertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, _commandBuffer, 1, _commandBuffer, 1);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("Turbosight remote: failed to start Conexant receiver {0} {1} {2}, hr = 0x{3:x} ({4})", i, r.Name, r.DevicePath, hr, HResult.GetDXErrorString(hr));
          }
          i++;
        }

        StartRemoteControlListenerThread();
      }

      if (_receiversNxp.Count > 0)
      {
        this.LogDebug("Turbosight remote: starting {0} NXP receiver(s)", _receiversNxp.Count);
        i = 0;
        foreach (ReceiverNxp r in _receiversNxp)
        {
          IMpTbsNxpIrRcReceiver receiver = null;
          try
          {
            string file = Path.Combine(PathManager.BuildAssemblyRelativePath("Resources"), "TbsNxpIrRcReceiver.dll");
            receiver = ComHelper.LoadComObjectFromFile(file, typeof(MpTbsNxpIrRcReceiver).GUID, typeof(IMpTbsNxpIrRcReceiver).GUID, true) as IMpTbsNxpIrRcReceiver;
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Turbosight remote: failed to load NXP receiver interface");
            break;
          }
          hr = receiver.Initialise(r.Control, TbsNxpRcProtocol.Nec32, this, r.Context);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("Turbosight remote: failed to start NXP receiver {0} {1} {2}, hr = 0x{3:x} ({4})", i, r.Name, r.DevicePath, hr, HResult.GetDXErrorString(hr));
          }
          else
          {
            r.Receiver = receiver;
          }
        }
      }

      _isRemoteControlInterfaceOpen = true;
      this.LogDebug("Turbosight remote: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseRemoteControlInterface()
    {
      this.LogDebug("Turbosight remote: close remote control interface");

      if (_isRemoteControlInterfaceOpen)
      {
        if (_receiversConexant.Count > 0)
        {
          this.LogDebug("Turbosight remote: stopping {0} Conexant receiver(s)", _receiversConexant.Count);
          StopRemoteControlListenerThread();

          Marshal.WriteByte(_commandBuffer, 0, (byte)TbsIrCommand.Stop);
          int i = 1;
          int hr;
          foreach (ReceiverConexant r in _receiversConexant)
          {
            hr = r.PropertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, _commandBuffer, 1, _commandBuffer, 1);
            if (hr != (int)HResult.Severity.Success)
            {
              this.LogWarn("Turbosight remote: failed to stop Conexant receiver {0} {1} {2}, hr = 0x{3:x} ({4})", i, r.Name, r.DevicePath, hr, HResult.GetDXErrorString(hr));
            }
            i++;
          }

          if (_commandBuffer != IntPtr.Zero)
          {
            Marshal.FreeCoTaskMem(_commandBuffer);
            _commandBuffer = IntPtr.Zero;
          }
          if (_codeBuffer != IntPtr.Zero)
          {
            Marshal.FreeCoTaskMem(_codeBuffer);
            _codeBuffer = IntPtr.Zero;
          }
        }

        if (_receiversNxp.Count > 0)
        {
          this.LogDebug("Turbosight remote: stopping {0} NXP receiver(s)", _receiversNxp.Count);
          int i = 1;
          foreach (ReceiverNxp r in _receiversNxp)
          {
            if (r.Receiver != null)
            {
              r.Receiver.Dispose();
              Release.ComObject(string.Format("Turbosight remote NXP receiver interface {0}", i), ref r.Receiver);
            }
            i++;
          }
        }

        _isRemoteControlInterfaceOpen = false;
      }
      this.LogDebug("Turbosight remote: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_isTurbosightRemote)
      {
        CloseRemoteControlInterface();
      }

      int i = 1;
      foreach (ReceiverConexant r in _receiversConexant)
      {
        Release.ComObject(string.Format("Turbosight remote Conexant receiver {0} property set", i++), ref r.PropertySet);
      }
      _receiversConexant.Clear();

      i = 1;
      foreach (ReceiverNxp r in _receiversNxp)
      {
        Release.ComObject(string.Format("Turbosight remote NXP receiver {0} control", i++), ref r.Control);
        Marshal.FreeCoTaskMem(r.Context);
      }
      _receiversNxp.Clear();

      lock (_instanceLock)
      {
        _isLoaded = false;
      }
      _isTurbosightRemote = false;
    }

    #endregion
  }
}