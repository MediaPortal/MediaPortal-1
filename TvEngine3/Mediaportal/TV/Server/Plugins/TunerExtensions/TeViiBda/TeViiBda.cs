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
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.TeVii
{
  /// <summary>
  /// A class for handling DiSEqC and remote controls for TeVii tuners. This
  /// class is implemented using direct kernel streaming property set access
  /// rather than the official TeVii SDK/API.
  /// </summary>
  public class TeViiBda : BaseTunerExtension, IDiseqcDevice, IDisposable, IRemoteControlListener
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Diseqc = 2,
      IrGet,
      IrSet,
      MacAddress,

      DeviceId = 10
    }

    private enum TeViiIrCommand : byte
    {
      Start = 1,
      Stop
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessage
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] Message;
      public byte MessageLength;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLastMessage;
      public int Unknown;     // "power"
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct IrData
    {
      public uint Address;
      public uint Command;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0x16faac60, 0x9022, 0xd420, 0x15, 0x20, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);

    private static readonly int INSTANCE_SIZE = Marshal.SizeOf(typeof(KsProperty));             // 24
    private static readonly int DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(DiseqcMessage));    // 160
    private const int MAX_DISEQC_MESSAGE_LENGTH = 151;
    private static readonly int IR_DATA_SIZE = Marshal.SizeOf(typeof(IrData));                  // 8
    private const int MAC_ADDRESS_LENGTH = 6;
    private const int DEVICE_ID_LENGTH = 4;

    private static readonly int GENERAL_BUFFER_SIZE = DISEQC_MESSAGE_SIZE;

    private const string TEVIIRC_PROCESS_NAME = "TeViiRC";
    private static readonly TimeSpan REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = new TimeSpan(0, 0, 0, 0, 100);

    private static readonly HashSet<string> VALID_DEVICE_PATHS = new HashSet<string>
    {
      // [stream class drivers, BDA capture/receiver components]
      "pci#ven_14f1&dev_8802&subsys_9022d420",    // S420 (PCI DVB-S)
      "pci#ven_14f1&dev_8802&subsys_9022d460",    // S460 (PCI DVB-S2)
      "pci#ven_14f1&dev_8802&subsys_9022d464",    // S464 v1 (PCI DVB-S2)
      "pci#ven_14f1&dev_8802&subsys_9022d465",    // S464 v2 (PCI DVB-S2)
      "pci#ven_14f1&dev_8802&subsys_90223000",    // S464 v3 (PCI DVB-S2)

      // S421 (PCI DVB-S; actually S63* with PCI bridge)
      "usb#vid_9022&pid_d421",
      "usb#vid_9022&pid_1000",

      // S47* (PCIe DVB-S/S2)
      "pci#ven_14f1&dev_8852&subsys_9022d470",
      "pci#ven_14f1&dev_8852&subsys_9022d471",
      "pci#ven_14f1&dev_8852&subsys_9022d472",
      "pci#ven_14f1&dev_8852&subsys_9022d473",

      // S48* (PCIe dual DVB-S/S2; actually 2 x S66* with PCIe bridge)
      "usb#vid_9022&pid_d481",    // S660
      "usb#vid_9022&pid_d482",    // S660
      "usb#vid_9022&pid_d483",    // S662
      "usb#vid_9022&pid_d484",    // S662

      // S620 (USB ???)
      "usb#vid_9022&pid_d620",

      // S63* (USB DVB-S)
      "usb#vid_9022&pid_d630",
      "usb#vid_3344&pid_d630",
      "usb#vid_9022&pid_d632",
      "usb#vid_9022&pid_2000",

      // S650 (USB DVB-S2)
      "usb#vid_9022&pid_d650",

      // S66* (USB DVB-S/S2)
      "usb#vid_9022&pid_d660",
      "usb#vid_9022&pid_d662"
    };

    #endregion

    #region variables

    private bool _isTeViiBda = false;
    private string _tunerExternalId = string.Empty;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private bool _restartTeViiRcExe = false;

    private bool _isRemoteControlInterfaceOpen = false;
    private IntPtr _remoteControlBuffer = IntPtr.Zero;
    private Thread _remoteControlListenerThread = null;
    private ManualResetEvent _remoteControlListenerThreadStopEvent = null;

    #endregion

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      this.LogDebug("TeVii BDA: read device information");

      // Device ID.
      for (int i = 0; i < DEVICE_ID_LENGTH; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DeviceId,
        _instanceBuffer, INSTANCE_SIZE,
        _generalBuffer, DEVICE_ID_LENGTH,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != DEVICE_ID_LENGTH)
      {
        this.LogWarn("TeVii BDA: failed to read device ID, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        this.LogDebug("  vendor ID   = 0x{0:x4}", Marshal.ReadInt16(_generalBuffer, 0));
        this.LogDebug("  device ID   = 0x{0:x4}", Marshal.ReadInt16(_generalBuffer, 2));
      }

      // MAC address.
      for (int i = 0; i < MAC_ADDRESS_LENGTH; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MacAddress,
        _instanceBuffer, INSTANCE_SIZE,
        _generalBuffer, MAC_ADDRESS_LENGTH,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != MAC_ADDRESS_LENGTH)
      {
        this.LogWarn("TeVii BDA: failed to read MAC address, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        byte[] address = new byte[MAC_ADDRESS_LENGTH];
        Marshal.Copy(_generalBuffer, address, 0, MAC_ADDRESS_LENGTH);
        this.LogDebug("  MAC address = {0}", BitConverter.ToString(address).ToLowerInvariant());
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
        this.LogDebug("TeVii BDA: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new ManualResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "TeVii BDA remote control listener";
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
          this.LogWarn("TeVii BDA: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join((int)REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME.TotalMilliseconds * 2))
          {
            this.LogWarn("TeVii BDA: failed to join remote control listener thread, aborting thread");
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
      this.LogDebug("TeVii BDA: remote control listener thread start polling");
      int hr;
      int returnedByteCount;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          Marshal.WriteInt32(_remoteControlBuffer, 0, 1);
          Marshal.WriteInt32(_remoteControlBuffer, 4, 1);
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.IrGet,
            _instanceBuffer, INSTANCE_SIZE,
            _remoteControlBuffer, IR_DATA_SIZE,
            out returnedByteCount
          );
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != IR_DATA_SIZE)
          {
            this.LogError("TeVii BDA: failed to read remote code, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          }
          else
          {
            IrData data = (IrData)Marshal.PtrToStructure(_remoteControlBuffer, typeof(IrData));
            if (data.Address != 0xffff && data.Command != 0xffff)
            {
              this.LogDebug("TeVii BDA: key press, address = 0x{0:x8}, command = 0x{1:x8}", data.Address, data.Command);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TeVii BDA: remote control listener thread exception");
        return;
      }
      this.LogDebug("TeVii BDA: remote control listener thread stop polling");
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "TeVii BDA";
      }
    }

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 70;
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
      this.LogDebug("TeVii BDA: initialising");

      if (_isTeViiBda)
      {
        this.LogWarn("TeVii BDA: extension already initialised");
        return true;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("TeVii BDA: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("TeVii BDA: pin is not a property set");
        Release.ComObject("TeVii BDA filter input pin", ref pin);
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.IrGet, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("TeVii BDA: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        Release.ComObject("TeVii BDA property set", ref _propertySet);
        return false;
      }

      this.LogInfo("TeVii BDA: extension supported");
      _isTeViiBda = true;
      _tunerExternalId = tunerExternalId;
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
      ReadDeviceInfo();
      return true;
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
      this.LogDebug("TeVii BDA: send DiSEqC command");

      if (!_isTeViiBda)
      {
        this.LogWarn("TeVii BDA: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("TeVii BDA: DiSEqC command not supplied");
        return true;
      }

      int length = command.Length;
      if (length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("TeVii BDA: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.Message = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);
      message.MessageLength = (byte)length;
      message.IsLastMessage = true;   // It's entirely possible that this is wrong.
      message.Unknown = 0;

      Marshal.StructureToPtr(message, _generalBuffer, false);
      //Dump.DumpBinary(_diseqcBuffer, DISEQC_MESSAGE_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc,
        _generalBuffer, DISEQC_MESSAGE_SIZE,
        _generalBuffer, DISEQC_MESSAGE_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("TeVii BDA: result = success");
        return true;
      }

      this.LogError("TeVii BDA: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      // Not supported.
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
      return false;
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
      this.LogDebug("TeVii BDA: open remote control interface");

      if (!_isTeViiBda)
      {
        this.LogWarn("TeVii BDA: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("TeVii BDA: remote control interface is already open");
        return true;
      }

      if (_tunerExternalId.Contains("vid_9022&pid_d482") || _tunerExternalId.Contains("vid_9022&pid_d484"))
      {
        this.LogDebug("TeVii BDA: detected S480/482 tuner 2, remote control support disabled to avoid double key presses");
        return false;
      }

      _remoteControlBuffer = Marshal.AllocCoTaskMem(IR_DATA_SIZE);
      Marshal.WriteByte(_remoteControlBuffer, 0, (byte)TeViiIrCommand.Start);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.IrSet, _remoteControlBuffer, 1, _remoteControlBuffer, 1);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("TeVii BDA: failed to start remote control, hr = 0x{0:x}", hr);
        Marshal.FreeCoTaskMem(_remoteControlBuffer);
        _remoteControlBuffer = IntPtr.Zero;
        return false;
      }

      _restartTeViiRcExe = false;
      Process[] processes = Process.GetProcessesByName(TEVIIRC_PROCESS_NAME);
      if (processes.Length > 0)
      {
        // TeViiRC is running. We stop it to avoid unnecessary HID keypresses.
        // We'll restart it when we're done.
        this.LogWarn("TeVii BDA: attempt terminate {0} TeVii RC process(es)", processes.Length);
        foreach (Process proc in processes)
        {
          proc.Kill();
        }
        Thread.Sleep(400);
        processes = Process.GetProcessesByName(TEVIIRC_PROCESS_NAME);
        if (processes.Length != 0)
        {
          this.LogWarn("TeVii BDA: failed to terminate TeVii RC, still {0} process(es)", processes.Length);
        }
        else
        {
          _restartTeViiRcExe = true;
        }
      }

      StartRemoteControlListenerThread();
      _isRemoteControlInterfaceOpen = true;
      this.LogDebug("TeVii BDA: result = success");
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
      this.LogDebug("TeVii BDA: close remote control interface");

      if (isDisposing && _isRemoteControlInterfaceOpen)
      {
        StopRemoteControlListenerThread();

        Marshal.WriteByte(_remoteControlBuffer, 0, (byte)TeViiIrCommand.Stop);
        int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.IrSet, _remoteControlBuffer, 1, _remoteControlBuffer, 1);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("TeVii BDA: failed to stop remote control, hr = 0x{0:x}", hr);
        }

        Marshal.FreeCoTaskMem(_remoteControlBuffer);
        _remoteControlBuffer = IntPtr.Zero;
      }

      if (_restartTeViiRcExe)
      {
        this.LogDebug("TeVii BDA: restart TeVii RC process");
        try
        {
          int processCount = Process.GetProcessesByName(TEVIIRC_PROCESS_NAME).Length;
          if (processCount == 0)
          {
            Process.Start(@"c:\WINDOWS\TeViiRC.exe");
          }
          else
          {
            this.LogWarn("TeVii BDA: {0} TeVii RC process(es) already running", processCount);
          }
          _restartTeViiRcExe = false;
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "TeVii BDA: failed to restart TeViiRC process");
        }
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("TeVii BDA: result = success");
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

    ~TeViiBda()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isTeViiBda)
      {
        CloseRemoteControlListenerInterface(isDisposing);
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
      }
      _isTeViiBda = false;
    }

    #endregion
  }
}