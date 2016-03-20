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
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl
{
  internal class RemoteControlCyprus : ITurbosightRemoteControl
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Reserved = 0,
      Ir = 1,             // Property for retrieving IR codes from the IR receiver.
      CiAccess = 8,       // Property for interacting with the CI slot.
      BlindScan = 9,      // Property for accessing and controlling the hardware blind scan capabilities.
      TbsAccess = 18      // TBS property for enabling control of the common properties in the TbsAccessMode enum.
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct IrCommand
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      private byte[] Reserved1;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
      public byte[] Codes;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 244)]
      private byte[] Reserved2;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 0xaa, 0x87, 0xb5, 0xe1, 0xdc, 0x41, 0x13);

    private static readonly int IR_COMMAND_SIZE = Marshal.SizeOf(typeof(IrCommand));          // 288
    private static readonly TimeSpan REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = new TimeSpan(0, 0, 0, 0, 100);

    #endregion

    #region variables

    private static HashSet<string> _openProducts = new HashSet<string>();

    private bool _isInterfaceOpen = false;
    private string _productInstanceId = null;
    private IKsPropertySet _propertySet = null;

    private Thread _listenerThread = null;
    private ManualResetEvent _listenerThreadStopEvent = null;

    #endregion

    public RemoteControlCyprus(string productInstanceId, IKsPropertySet propertySet)
    {
      _productInstanceId = productInstanceId;
      _propertySet = propertySet;
    }

    #region ITurbosightRemoteControl members

    /// <summary>
    /// Open the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool Open()
    {
      this.LogDebug("Turbosight Cyprus RC: open interface, product instance ID = {0}", _productInstanceId ?? "[null]");
      if (_isInterfaceOpen)
      {
        this.LogWarn("Turbosight Cyprus RC: interface is already open");
        return true;
      }
      if (_productInstanceId == null)
      {
        this.LogDebug("Turbosight Cyprus RC: product instance identifier is null");
        return false;
      }
      if (_openProducts.Contains(_productInstanceId))
      {
        this.LogDebug("Turbosight Cyprus RC: multi-tuner product remote control opened for other tuner");
        return true;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogError("Turbosight Cyprus RC: property set does not support IR property, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      _isInterfaceOpen = true;
      _openProducts.Add(_productInstanceId);
      StartListenerThread();
      return true;
    }

    /// <summary>
    /// Close the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool Close()
    {
      this.LogDebug("Turbosight Cyprus RC: close interface");

      StopListenerThread();

      if (_isInterfaceOpen)
      {
        if (_productInstanceId != null)
        {
          _openProducts.Remove(_productInstanceId);
        }
        _isInterfaceOpen = false;
      }

      return true;
    }

    #endregion

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_listenerThread != null && !_listenerThread.IsAlive)
      {
        StopListenerThread();
      }
      if (_listenerThread == null)
      {
        this.LogDebug("Turbosight Cyprus RC: starting new listener thread");
        _listenerThreadStopEvent = new ManualResetEvent(false);
        _listenerThread = new Thread(new ThreadStart(Listener));
        _listenerThread.Name = "Turbosight Cyprus remote control listener";
        _listenerThread.IsBackground = true;
        _listenerThread.Priority = ThreadPriority.Lowest;
        _listenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopListenerThread()
    {
      if (_listenerThread != null)
      {
        if (!_listenerThread.IsAlive)
        {
          this.LogWarn("Turbosight Cyprus RC: aborting old listener thread");
          _listenerThread.Abort();
        }
        else
        {
          _listenerThreadStopEvent.Set();
          if (!_listenerThread.Join((int)REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME.TotalMilliseconds * 2))
          {
            this.LogWarn("Turbosight Cyprus RC: failed to join listener thread, aborting thread");
            _listenerThread.Abort();
          }
        }
        _listenerThread = null;
        if (_listenerThreadStopEvent != null)
        {
          _listenerThreadStopEvent.Close();
          _listenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void Listener()
    {
      this.LogDebug("Turbosight Cyprus RC: listener thread start polling");

      IntPtr codeBuffer = Marshal.AllocCoTaskMem(IR_COMMAND_SIZE);
      int hr;
      int returnedByteCount;
      try
      {
        while (!_listenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir,
            codeBuffer, IR_COMMAND_SIZE,
            codeBuffer, IR_COMMAND_SIZE,
            out returnedByteCount
          );
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != IR_COMMAND_SIZE)
          {
            this.LogError("Turbosight Cyprus RC: failed to read remote code, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          }
          else
          {
            IrCommand command = (IrCommand)Marshal.PtrToStructure(codeBuffer, typeof(IrCommand));
            byte code = command.Codes[0];
            if (code != 0xff)
            {
              if (code < (int)RemoteCodeBig.MINIMUM_VALUE)
              {
                this.LogDebug("Turbosight Cyprus RC: small remote control key press, code = {0}", (RemoteCodeSmall)code);
              }
              else
              {
                this.LogDebug("Turbosight Cyprus RC: big remote control key press, code = {0}", (RemoteCodeBig)code);
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
        this.LogError(ex, "Turbosight Cyprus RC: listener thread exception");
        return;
      }
      finally
      {
        Marshal.FreeCoTaskMem(codeBuffer);
      }
      this.LogDebug("Turbosight Cyprus RC: listener thread stop polling");
    }

    #endregion
  }
}