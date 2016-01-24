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
using System.Windows.Forms;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl
{
  /// <summary>
  /// A class for centralised input registration, reception and unregistration.
  /// </summary>
  internal class HidListener
  {
    private Thread _listenerThread = null;
    private uint _listenerThreadId = 0;
    private ListenerWindow _listenerWindow = null;
    private IDictionary<HidUsagePage, IDictionary<ushort, HashSet<string>>> _registrations = new Dictionary<HidUsagePage, IDictionary<ushort, HashSet<string>>>();

    public event OnInputDelegate OnInput = null;

    public delegate void OnInputDelegate(IntPtr input);

    #region listener window class

    private class ListenerWindow : NativeWindow
    {
      private OnInputDelegate _inputDelegate = null;

      public ListenerWindow(OnInputDelegate inputDelegate)
      {
        _inputDelegate = inputDelegate;
      }

      protected override void WndProc(ref Message m)
      {
        if (
          m.Msg == (int)NativeMethods.WindowsMessage.WM_INPUT &&
          m.LParam != null &&
          m.LParam != IntPtr.Zero &&
          _inputDelegate != null
        )
        {
          _inputDelegate(m.LParam);
        }
        base.WndProc(ref m);
      }
    }

    #endregion

    ~HidListener()
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
      if (_listenerThread != null && _listenerThreadId > 0)
      {
        NativeMethods.PostThreadMessage(_listenerThreadId, NativeMethods.WindowsMessage.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        if (!_listenerThread.Join(500))
        {
          this.LogWarn("Twinhan HID listener: failed to join remote control listener thread, aborting thread");
          _listenerThread.Abort();
        }
        _listenerThreadId = 0;
        _listenerThread = null;
      }
    }

    #endregion

    public void RegisterHids(ICollection<HumanInterfaceDevice> devices)
    {
      this.LogDebug("Twinhan HID listener: register HIDs");

      // Start the listener thread which opens the listener window.
      if (_listenerThread == null)
      {
        ManualResetEvent startEvent = new ManualResetEvent(false);
        try
        {
          _listenerThread = new Thread(new ParameterizedThreadStart(Listener));
          _listenerThread.Name = "Twinhan HID remote control listener";
          _listenerThread.IsBackground = true;
          _listenerThread.Priority = ThreadPriority.Lowest;
          _listenerThread.Start(startEvent);
          if (!startEvent.WaitOne(5000) || _listenerWindow == null)
          {
            this.LogWarn("Twinhan HID listener: failed to receive remote control listener thread start event, assuming error occurred");
            _listenerThread.Abort();
            _listenerThread = null;
            return;
          }
        }
        finally
        {
          startEvent.Close();
          startEvent.Dispose();
        }
      }

      // Identify the new usage page/collection combinations which need to be
      // registered.
      Dictionary<HidUsagePage, IDictionary<ushort, HashSet<string>>> newRegistrationUsages = new Dictionary<HidUsagePage, IDictionary<ushort, HashSet<string>>>(6);
      List<NativeMethods.RAWINPUTDEVICE> newRegistrations = new List<NativeMethods.RAWINPUTDEVICE>(6);
      foreach (HumanInterfaceDevice d in devices)
      {
        if (d.UsagePage == HidUsagePage.Undefined)
        {
          continue;
        }

        IDictionary<ushort, HashSet<string>> usageCollections;
        HashSet<string> registeredDevices;
        if (!_registrations.TryGetValue(d.UsagePage, out usageCollections))
        {
          _registrations[d.UsagePage] = new Dictionary<ushort, HashSet<string>>();
        }
        else if (usageCollections.TryGetValue(d.UsageCollection, out registeredDevices) && registeredDevices.Count > 0)
        {
          // Nothing to do if the page/collection is already registered.
          usageCollections[d.UsageCollection].Add(d.Id);
          continue;
        }

        bool addRegistration = true;
        if (!newRegistrationUsages.TryGetValue(d.UsagePage, out usageCollections))
        {
          usageCollections = new Dictionary<ushort, HashSet<string>>();
          newRegistrationUsages.Add(d.UsagePage, usageCollections);
        }
        if (!usageCollections.TryGetValue(d.UsageCollection, out registeredDevices))
        {
          usageCollections.Add(d.UsageCollection, new HashSet<string> { d.Id });
        }
        else if (!registeredDevices.Add(d.Id))
        {
          addRegistration = false;
        }

        if (addRegistration)
        {
          this.LogDebug("  add registration, usage page = {0}, usage collection = {1}", d.UsagePage, d.UsageCollection);
          NativeMethods.RAWINPUTDEVICE r = new NativeMethods.RAWINPUTDEVICE();
          r.dwFlags = NativeMethods.RawInputDeviceFlag.RIDEV_INPUTSINK;
          r.usUsagePage = (ushort)d.UsagePage;
          r.usUsage = d.UsageCollection;
          r.hwndTarget = _listenerWindow.Handle;
          newRegistrations.Add(r);
        }
      }

      if (newRegistrations.Count == 0)
      {
        return;
      }

      if (!NativeMethods.RegisterRawInputDevices(newRegistrations.ToArray(), (uint)newRegistrations.Count, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE))))
      {
        this.LogError("Twinhan HID listener: failed to register for keypress events, error = {0}", Marshal.GetLastWin32Error());
        return;
      }

      // Update our registration collection.
      foreach (HidUsagePage page in newRegistrationUsages.Keys)
      {
        IDictionary<ushort, HashSet<string>> usageCollections = newRegistrationUsages[page];
        foreach (var collection in usageCollections)
        {
          _registrations[page].Add(collection.Key, collection.Value);
        }
      }
    }

    public void UnregisterHids(ICollection<HumanInterfaceDevice> devices)
    {
      this.LogDebug("Twinhan HID listener: unregister HIDs");

      // Identify the usage page/collection combinations which no longer need
      // to be registered.
      List<NativeMethods.RAWINPUTDEVICE> oldRegistrations = new List<NativeMethods.RAWINPUTDEVICE>(6);
      foreach (HumanInterfaceDevice d in devices)
      {
        if (d.UsagePage == HidUsagePage.Undefined)
        {
          continue;
        }

        IDictionary<ushort, HashSet<string>> usageCollections;
        HashSet<string> registeredDevices;
        if (!_registrations.TryGetValue(d.UsagePage, out usageCollections) || !usageCollections.TryGetValue(d.UsageCollection, out registeredDevices))
        {
          continue;
        }
        if (registeredDevices.Count == 1 && registeredDevices.Contains(d.Id))
        {
          this.LogDebug("  remove registration, usage page = {0}, usage collection = {1}", d.UsagePage, d.UsageCollection);
          NativeMethods.RAWINPUTDEVICE r = new NativeMethods.RAWINPUTDEVICE();
          r.dwFlags = NativeMethods.RawInputDeviceFlag.RIDEV_REMOVE;
          r.usUsagePage = (ushort)d.UsagePage;
          r.usUsage = d.UsageCollection;
          r.hwndTarget = IntPtr.Zero;
          oldRegistrations.Add(r);
        }
        else
        {
          registeredDevices.Remove(d.Id);
        }
      }

      if (oldRegistrations.Count == 0)
      {
        return;
      }

      if (!NativeMethods.RegisterRawInputDevices(oldRegistrations.ToArray(), (uint)oldRegistrations.Count, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE))))
      {
        this.LogError("Twinhan HID listener: failed to unregister for keypress events, error = {0}", Marshal.GetLastWin32Error());
        return;
      }

      // Update our registration collection.
      foreach (NativeMethods.RAWINPUTDEVICE registration in oldRegistrations)
      {
        _registrations[(HidUsagePage)registration.usUsagePage][registration.usUsage].Clear();
      }

      // If the registration collection is empty, stop the listener thread and
      // close the listener window.
      foreach (HidUsagePage page in _registrations.Keys)
      {
        IDictionary<ushort, HashSet<string>> usageCollections = _registrations[page];
        foreach (var collection in usageCollections)
        {
          if (collection.Value.Count > 0)
          {
            return;
          }
        }
      }

      Dispose();
    }

    private void Listener(object eventParam)
    {
      this.LogDebug("Twinhan HID listener: starting listener thread");

      // Be ***very*** careful if you modify this code. For more info about how
      // NativeWindow should be used:
      // http://stackoverflow.com/questions/2443867/message-pump-in-net-windows-service
      Thread.BeginThreadAffinity();
      _listenerWindow = null;
      _registrations.Clear();
      try
      {
        try
        {
          _listenerWindow = new ListenerWindow(OnInput);
          try
          {
            _listenerWindow.CreateHandle(new CreateParams()
            {
              Style = unchecked((int)NativeMethods.WindowStyle.WS_POPUP),
              ExStyle = (int)NativeMethods.WindowStyleEx.WS_EX_TOOLWINDOW,
            });
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Twinhan HID listener: failed to create keypress listener window");
            _listenerWindow = null;
            return;
          }
        }
        finally
        {
          ((ManualResetEvent)eventParam).Set();
        }

        _listenerThreadId = NativeMethods.GetCurrentThreadId();
        // This call will block and pump messages to the listener window
        // until the listener window is closed. Without this, the window won't
        // receive WM_INPUT.
        Application.Run();

        this.LogDebug("Twinhan HID listener: stopping listener thread");
      }
      finally
      {
        if (_listenerWindow != null)
        {
          List<NativeMethods.RAWINPUTDEVICE> oldRegistrations = new List<NativeMethods.RAWINPUTDEVICE>(6);
          foreach (HidUsagePage usagePage in _registrations.Keys)
          {
            IDictionary<ushort, HashSet<string>> usages = _registrations[usagePage];
            foreach (var usage in usages)
            {
              if (usage.Value.Count > 0)
              {
                NativeMethods.RAWINPUTDEVICE r = new NativeMethods.RAWINPUTDEVICE();
                r.dwFlags = NativeMethods.RawInputDeviceFlag.RIDEV_REMOVE;
                r.usUsagePage = (ushort)usagePage;
                r.usUsage = usage.Key;
                r.hwndTarget = IntPtr.Zero;
                oldRegistrations.Add(r);
              }
            }
          }

          if (oldRegistrations.Count > 0 && !NativeMethods.RegisterRawInputDevices(oldRegistrations.ToArray(), (uint)oldRegistrations.Count, (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE))))
          {
            this.LogWarn("Twinhan HID listener: failed to unregister for keypress events, error = {0}", Marshal.GetLastWin32Error());
          }

          _registrations.Clear();
          _listenerWindow.DestroyHandle();
        }
        Thread.EndThreadAffinity();
      }
    }
  }
}