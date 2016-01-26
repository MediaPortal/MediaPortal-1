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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper
{
  #region event delegates

  /// <summary>
  /// Delegate for a device interface change event.
  /// </summary>
  /// <param name="eventType">The type of change event that has occurred.</param>
  /// <param name="classGuid">The identifier for the interface's class.</param>
  /// <param name="devicePath">The interface's device path.</param>
  public delegate void OnDeviceInterfaceChangeDelegate(NativeMethods.DBT_MANAGEMENT_EVENT eventType, Guid classGuid, string devicePath);

  /// <summary>
  /// Delegate for a power broadcast event.
  /// </summary>
  /// <param name="eventType">The type of broadcast event that has occurred.</param>
  public delegate void OnPowerBroadcastDelegate(NativeMethods.PBT_MANAGEMENT_EVENT eventType);

  #endregion

  public class SystemChangeNotifier
  {
    private class NotifierWindow : NativeWindow, IDisposable
    {
      private IntPtr _deviceChangeNotificationHandle;
      private OnDeviceInterfaceChangeDelegate _deviceChangeDelegate = null;
      private OnPowerBroadcastDelegate _powerBroadcastDelegate = null;

      public NotifierWindow(OnDeviceInterfaceChangeDelegate deviceChangeDelegate, OnPowerBroadcastDelegate powerBroadcastDelegate)
      {
        _deviceChangeDelegate = deviceChangeDelegate;
        _powerBroadcastDelegate = powerBroadcastDelegate;

        CreateParams windowParameters = new CreateParams();
        windowParameters.Style = unchecked((int)NativeMethods.WindowStyle.WS_POPUP);
        windowParameters.ExStyle = (int)NativeMethods.WindowStyleEx.WS_EX_TOOLWINDOW;
        CreateHandle(windowParameters);

        if (deviceChangeDelegate != null)
        {
          NativeMethods.DEV_BROADCAST_DEVICEINTERFACE dbi = new NativeMethods.DEV_BROADCAST_DEVICEINTERFACE();
          dbi.dbcc_size = Marshal.SizeOf(dbi);
          dbi.dbcc_devicetype = NativeMethods.DBT_DEVICE_TYPE.DBT_DEVTYP_DEVICEINTERFACE;
          dbi.dbcc_classguid = Guid.Empty;

          _deviceChangeNotificationHandle = NativeMethods.RegisterDeviceNotification(Handle, ref dbi, NativeMethods.DEVICE_NOTIFY_FLAGS.DEVICE_NOTIFY_WINDOW_HANDLE | NativeMethods.DEVICE_NOTIFY_FLAGS.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES);
          if (_deviceChangeNotificationHandle == IntPtr.Zero)
          {
            this.LogError("system change notifier: failed to register for device notification, error = {0}", Marshal.GetLastWin32Error());
          }
        }
      }

      ~NotifierWindow()
      {
        Dispose(false);
      }

      #region IDisposable member

      /// <summary>
      /// Release and dispose all resources.
      /// </summary>
      public void Dispose()
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }

      /// <summary>
      /// Release and dispose all resources.
      /// </summary>
      /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
      private void Dispose(bool isDisposing)
      {
        if (_deviceChangeNotificationHandle != IntPtr.Zero)
        {
          NativeMethods.UnregisterDeviceNotification(_deviceChangeNotificationHandle);
          _deviceChangeNotificationHandle = IntPtr.Zero;
        }

        DestroyHandle();
      }

      #endregion

      protected override void WndProc(ref Message m)
      {
        if (m.Msg == (int)NativeMethods.WindowsMessage.WM_DEVICECHANGE)
        {
          if (_deviceChangeDelegate != null && m.LParam != IntPtr.Zero)
          {
            NativeMethods.DEV_BROADCAST_HDR dbh = (NativeMethods.DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.DEV_BROADCAST_HDR));
            if (dbh.dbch_devicetype == NativeMethods.DBT_DEVICE_TYPE.DBT_DEVTYP_DEVICEINTERFACE)
            {
              NativeMethods.DEV_BROADCAST_DEVICEINTERFACE dbhInterface = (NativeMethods.DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.DEV_BROADCAST_DEVICEINTERFACE));
              _deviceChangeDelegate((NativeMethods.DBT_MANAGEMENT_EVENT)m.WParam.ToInt32(), dbhInterface.dbcc_classguid, dbhInterface.dbcc_name);
            }
          }
          m.Result = new IntPtr(1);
        }
        else if (m.Msg == (int)NativeMethods.WindowsMessage.WM_POWERBROADCAST)
        {
          if (_powerBroadcastDelegate != null)
          {
            _powerBroadcastDelegate((NativeMethods.PBT_MANAGEMENT_EVENT)m.WParam.ToInt32());
          }
          m.Result = new IntPtr(1);
        }

        base.WndProc(ref m);
      }
    }

    private Thread _notifierThread = null;
    private uint _notifierThreadId = 0;

    public SystemChangeNotifier()
    {
      // Start the listener thread.
      ManualResetEvent startEvent = new ManualResetEvent(false);
      try
      {
        _notifierThread = new Thread(new ParameterizedThreadStart(Notifier));
        _notifierThread.Name = "system change notifier";
        _notifierThread.IsBackground = true;
        _notifierThread.Priority = ThreadPriority.Lowest;
        _notifierThread.Start(startEvent);
        if (!startEvent.WaitOne(5000))
        {
          this.LogWarn("system change notifier: failed to receive notifier thread start event, assuming error occurred");
        }
      }
      finally
      {
        startEvent.Close();
        startEvent.Dispose();
      }
    }

    ~SystemChangeNotifier()
    {
      Dispose(false);
    }

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing && _notifierThread != null && _notifierThreadId > 0)
      {
        NativeMethods.PostThreadMessage(_notifierThreadId, NativeMethods.WindowsMessage.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        if (!_notifierThread.Join(500))
        {
          this.LogWarn("system change notifier: failed to join notifier thread, aborting thread");
          _notifierThread.Abort();
        }
        _notifierThreadId = 0;
        _notifierThread = null;
      }
    }

    #endregion

    public event OnDeviceInterfaceChangeDelegate OnDeviceInterfaceChange = null;
    public event OnPowerBroadcastDelegate OnPowerBroadcastDelegate = null;

    private void Notifier(object eventParam)
    {
      this.LogDebug("system change notifier: starting notifier thread");

      // Be ***very*** careful if you modify this code. For more info about how
      // NativeWindow should be used:
      // http://stackoverflow.com/questions/2443867/message-pump-in-net-windows-service
      Thread.BeginThreadAffinity();
      NotifierWindow notifierWindow = null;
      try
      {
        try
        {
          notifierWindow = new NotifierWindow(OnDeviceInterfaceChange, OnPowerBroadcastDelegate);
        }
        catch (System.Exception ex)
        {
          this.LogError(ex, "system change notifier: failed to create notifier window");
          notifierWindow = null;
          return;
        }
        finally
        {
          ((ManualResetEvent)eventParam).Set();
        }

        _notifierThreadId = NativeMethods.GetCurrentThreadId();
        // This call will block and pump messages to the notifier window until
        // the notifier window is closed. Without this, the window won't
        // receive messages.
        Application.Run();

        this.LogDebug("system change notifier: stopping notifier thread");
      }
      finally
      {
        if (notifierWindow != null)
        {
          notifierWindow.Dispose();
        }
        Thread.EndThreadAffinity();
      }
    }
  }
}