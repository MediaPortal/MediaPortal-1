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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeRemote
{
  /// <summary>
  /// A class for receiving remote control keypresses from Hauppauge tuners.
  /// </summary>
  public class HauppaugeRemote : BaseTunerExtension, IDisposable, IRemoteControlListener
  {
    #region enums

    /// <remarks>
    /// Values are RC-5 system addresses.
    /// </remarks>
    private enum HcwRemoteType : int
    {
      Classic21Button = 0,
      Pctv = 7,

      /// <remarks>
      /// From c:\Windows\irremote.ini. Compatible with 35/36 and 45/46 button remotes.
      /// </remarks>
      Pvr2_Unknown = 0x1c,
      Pvr2_Unknown2 = 0x2040,

      Pvr2_35Button = 0x1d,
      Pvr2_45Button = 0x1e,
      Pvr_34Button = 0x1f,

      Mce = 0x800f
    }

    /// <remarks>
    /// Image: http://linuxtv.org/vdrwiki/images/a/ae/Remote_control%28Hauppauge_black%29.jpg
    /// Testing: untested, based on default c:\Windows\irremote.ini.
    /// </remarks>
    private enum HcwRemoteCodeClassic
    {
      Zero = 0,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine,

      Radio = 12,
      Mute,               // [code clash]

      Tv = 15,            // [code clash]
      VolumeUp,
      VolumeDown,

      Reserved = 30,

      ChannelUp = 32,
      ChannelDown,
      Source,             // [code clash]

      Minimise = 38,
      FullScreen = 46     // [code clash]
    }

    /// <remarks>
    /// Image:
    ///   v1 = http://www.hauppauge.com/site/press/pctv/pctv_presspictures/Remote_RC5_Mini_Black_PCTV-logo.jpg
    ///   v2 = http://www.hauppauge.com/site/press/pctv/pctv_presspictures/PCTV_170e_Pack-Contents_PCTV-branded.jpg
    /// Testing: untested, based on default c:\Windows\irremote.ini.
    /// </remarks>
    private enum HcwRemoteCodePctv
    {
      Mute = 0,
      Logo = 1,           // the button with the PCTV Systems logo on it
      VolumeUp = 3,       // [overlay: right]
      Okay = 5,
      ChannelUp = 6,      // [overlay: up]
      VolumeDown = 9,     // [overlay: left]
      ChannelDown = 12,   // [overlay: down]
      One = 15,
      Three = 16,
      Seven = 17,
      Nine = 18,
      Two = 21,
      Four = 24,
      Five = 27,
      Six = 30,
      Eight = 33,
      FullScreen = 36,
      Zero = 39,
      Teletext = 42,
      Rewind = 45,
      PlayPause = 48,
      Record = 54,
      Power = 57,
      Stop = 60,
      Info = 63
    }

    /// <remarks>
    /// Image: http://linuxtv.org/vdrwiki/images/7/7b/Remote_control%28Hauppauge_nova-t%29.jpg
    /// http://i.ebayimg.com/00/s/OTM1WDExMDA=/z/upgAAOxyNwNSLLeM/$%28KGrHqYOKp0FIpQ,5db%29BSLLeMDF0Q~~60_57.JPG
    /// Testing: untested, based on default c:\Windows\irremote.ini.
    /// </remarks>
    private enum HcwRemoteCodePvr1
    {
      Zero = 0,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine, // 9

      Red = 11,
      Function,           // [unlabeled, code clash with HcwRemoteCodePvr2 "radio" button]
      Menu,

      Mute = 15,
      VolumeUp,
      VolumeDown,

      SkipForward = 30,
      BackExit,
      ChannelUp,
      ChannelDown,

      SkipBack = 36,      // replay
      Okay,

      Blue = 41,
      Green = 46,
      Pause = 48,
      Rewind = 50,

      FastForward = 52,
      Play,
      Stop,
      Record,
      Yellow, // 56

      Go = 59,
      FullScreen,
      Power
    }

    /// <remarks>
    /// Image:
    ///   standard (A415) = http://www.hauppauge.com/site/press/presspictures/remote_front.jpg
    ///   credit card 35 (DSR-0112) = http://www.hauppauge.com/site/press/presspictures/remote_creditcard.jpg
    ///   credit card 36 = http://www.hauppauge.com/site/press/presspictures/remote_creditcard-black_front.png
    ///   black 46 (DSR-0101) = http://i.ebayimg.com/t/HAUPPAUGE-DSR-0101-REMOTE-CONTROL-A415-HPG-WE-A-/00/s/MTMwOVgxNjAw/z/uHwAAOxymwBSP5YU/$T2eC16FHJIIFHJG5sli0BSP5YT1POQ~~60_57.JPG
    /// Testing: standard (Nova S Plus), DSR-0101 (HVR-4400)
    /// Comments are labels above the buttons.
    /// "New" is as compared with HcwRemoteCodePvr1.
    /// </remarks>
    private enum HcwRemoteCodePvr2
    {
      Zero = 0,                 // space
      One,
      Two,                      // ABC
      Three,                    // DEF
      Four,                     // GHI
      Five,                     // JKL
      Six,                      // MNO
      Seven,                    // PQRS
      Eight,                    // TUV
      Nine,                     // WXYZ
      Teletext, // 10           // [new, text: *]
      Red,
      Radio,                    // [code clash with HcwRemoteCodePvr1 unlabeled function button]
      Menu,
      Subtitles,                // sub/CC [new, text: #]
      Mute,
      VolumeUp,
      VolumeDown,
      ChannelPrevious,  // 18   // prev. ch [new]

      Up = 20,                  // [new]
      Down,                     // [new]
      Left,                     // [new]
      Right,                    // [new]
      Videos,                   // [new]
      Music,                    // [new]
      Pictures,                 // [new]
      Epg,                      // guide [new]
      Tv, // 28                 // [new]

      SkipForward = 30,
      BackExit,
      ChannelUp,
      ChannelDown,
      Asterix,                  // * [new, untested credit card remote]
      Hash,                     // # [new, untested credit card remote]
      SkipBack,                 // replay
      Okay, // 37

      Enter = 39,               // [new, DSR-0101]

      Blue = 41,
      Green = 46,
      Pause = 48,

      Rewind = 50,
      PlayPause,                // [new, untested credit card remote]
      FastForward,
      Play,
      Stop,
      Record,
      Yellow, // 56

      Go = 59,
      FullScreen,               // [untested credit card remote]
      Power
    }

    /// <remarks>
    /// Image:
    ///   black = http://www.hauppauge.com/site/press/presspictures/Remote_MCE-black.jpg
    ///   grey 1 = http://www.hauppauge.com/pics/remote_mce.gif
    ///   grey 2 = http://www.hauppauge.com/site/press/presspictures/Remote_MCE-2_angle.jpg
    /// Testing: untested, based on default c:\Windows\irremote.ini.
    /// Comments are labels above the buttons.
    /// </remarks>
    private enum HcwRemoteCodeMce
    {
      Zero = 0,                 // space
      One,
      Two,                      // ABC
      Three,                    // DEF
      Four,                     // GHI
      Five,                     // JKL
      Six,                      // MNO
      Seven,                    // PQRS
      Eight,                    // TUV
      Nine,                     // WXYZ
      Clear,  // 10
      Enter,
      Power,
      Logo,                     // the button with the Windows logo on it ("green button")
      Mute,
      Info,                     // more
      VolumeUp,
      VolumeDown,
      ChannelUp,
      ChannelDown,
      FastForward,  // 20
      Rewind,
      Play,
      Record,
      Pause,
      Stop,
      SkipForward,              // skip
      SkipBack,                 // replay
      Hash,                     // #
      Asterix,                  // *
      Up,   // 30
      Down,
      Left,
      Right,
      Okay,
      BackExit,
      DvdMenu,
      Tv,
      Epg,  // 38               // guide

      TvNew = 70,
      Music,
      RecordedTv,
      Pictures,
      Videos,
      Print,
      Radio,

      Teletext = 90,
      Red,
      Green,
      Yellow,
      Blue,

      Mce128 = 128,
      Mce129
    }

    #endregion

    #region DLL imports

    /*
     * Available with version 2.66.28078:
     * - IR_Open
     * - IR_GetKeyCode [obsolete]
     * - IR_Close
     * - IR_Power
     * - IR_GetVersion
     * - IR_GetKeyCodeEx
     * - IR_GetSystemKeyCode
     */

    /// <summary>
    /// Open the receiver interface.
    /// </summary>
    /// <remarks>
    /// The optional window handle has two uses:
    /// 1. As the parent for the result message box.
    /// 2. As the target for a timer which sends WM_TIMER messages to the
    /// window at a poll rate which is suitable for the hardware/driver.
    /// 
    /// The first use is unnecessary. The second can be avoided if you know the
    /// poll rate.
    /// 
    /// As it turns out, it is actually desirable to avoid passing a window
    /// handle. The WM_TIMER messages seem to trigger a variety of exceptions
    /// (access violation, SEH, null reference) when processed by
    /// DispatchMessage() in a managed window pump. I spent hours trying to
    /// figure out the problem but failed. Ultimately using a window handle
    /// is much more effort and complexity when you can simply start a managed
    /// thread.
    /// 
    /// 
    /// The last parameter can be used to control which type of receiver - eg.
    /// Conexant CX23885, eMPIA 28** - is opened, but this is essentially
    /// useless because:
    /// 1. The parameter values are not documented.
    /// 2. The parameter values are subject to change.
    /// 3. Each value selects a set of products based on the given driver, not
    ///    a specific product.
    /// 4. There is no documentation of the driver used for each product.
    /// 5. Products often have many revisions with different drivers.
    /// 6. We have no control over which product instance is selected.
    ///
    /// People who are desperate to control the value of the third parameter in
    /// order to [for example] ensure the correct receiver is opened (because
    /// they have multiple Hauppauge products) can use the registry to get the
    /// same result (at their own risk!!!):
    /// HKEY_LOCAL_MACHINE\SOFTWARE\Hauppauge\IR\IRPort
    ///
    /// Note: the I2Ctype key (also undocumented etc.) may also be required.
    /// </remarks>
    /// <param name="msg">Not used.</param>
    [DllImport("irremote.dll")]
    private static extern bool IR_Open(int windowHandle, uint msg, [MarshalAs(UnmanagedType.Bool)] bool showResultMessageBox, ushort irPort);

    /// <summary>
    /// Get the firmware version of the receiver hardware.
    /// </summary>
    [DllImport("irremote.dll")]
    private static extern int IR_GetVersion();

    /// <summary>
    /// Poll for key-press event details.
    /// </summary>
    /// <returns><c>true</c> if a key-press has occurred, otherwise <c>false</c></returns>
    [DllImport("irremote.dll")]
    private static extern bool IR_GetSystemKeyCode(ref int repeatCount, out HcwRemoteType remoteType, out int code);

    /// <summary>
    /// Close the receiver interface.
    /// </summary>
    /// <param name="windowHandle">Not used.</param>
    /// <param name="msg">Not used.</param>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    [DllImport("irremote.dll")]
    private static extern bool IR_Close(int windowHandle, uint msg);

    #endregion

    #region constants

    private const string IR32_PROCESS_NAME = "Ir";
    private const string IR32_EXE_NAME = "Ir.exe";
    private const string IR32_DLL_NAME = "irremote.DLL";
    private const string IR32_MINIMUM_COMPATIBLE_VERSION = "2.49.23332";
    private const int DEFAULT_REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 85;    // ms

    #endregion

    #region variables

    // This plugin is not tuner-specific. We use this variable to restrict to
    // one instance.
    private static bool _isLoaded = false;
    private static object _instanceLock = new object();

    private bool _isHauppaugeRemote = false;
    private bool _restartIrExe = false;
    private string _ir32Path = null;
    private int _repeatCount = 0;

    private volatile bool _isRemoteControlInterfaceOpen = false;
    private Thread _remoteControlListenerThread = null;
    private ManualResetEvent _remoteControlListenerThreadStopEvent = null;
    private int _remoteControlListenerThreadWaitTime = DEFAULT_REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME;

    #endregion

    /// <summary>
    /// Get the Hauppauge IR32 application installation path from the windows registry.
    /// </summary>
    /// <returns>the IR32 installation path if successful, otherwise <c>null</c></returns>
    private string GetIr32InstallPath()
    {
      List<RegistryView> views = new List<RegistryView>() { RegistryView.Default };
      if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
      {
        views.Add(RegistryView.Registry64);
      }
      foreach (RegistryView view in views)
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hauppauge WinTV Infrared Remote"))
        {
          if (key == null)
          {
            continue;
          }

          try
          {
            object pathValue = key.GetValue("InstallLocation");
            string path = null;
            if (pathValue != null)
            {
              path = pathValue.ToString();
            }
            else
            {
              pathValue = key.GetValue("UninstallString");
              if (pathValue == null)
              {
                continue;
              }
              path = pathValue.ToString();
              int index = path.ToLowerInvariant().IndexOf("unir32");
              if (index < 0)
              {
                continue;
              }
              path = path.Substring(0, index);
            }
            if (File.Exists(Path.Combine(path, IR32_EXE_NAME)) && File.Exists(Path.Combine(path, IR32_DLL_NAME)))
            {
              return path;
            }
          }
          finally
          {
            key.Close();
          }
        }
      }
      return null;
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
        this.LogDebug("Hauppauge remote: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new ManualResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Hauppauge remote control listener";
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
          this.LogWarn("Hauppauge remote: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(_remoteControlListenerThreadWaitTime * 2))
          {
            this.LogWarn("Hauppauge remote: failed to join remote control listener thread, aborting thread");
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
      this.LogDebug("Hauppauge remote: remote control listener thread start polling");

      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(_remoteControlListenerThreadWaitTime))
        {
          HcwRemoteType remoteType = 0;
          int code = 0;
          if (IR_GetSystemKeyCode(ref _repeatCount, out remoteType, out code))
          {
            string codeName;
            if (
              remoteType == HcwRemoteType.Pvr2_35Button ||
              remoteType == HcwRemoteType.Pvr2_45Button ||
              remoteType == HcwRemoteType.Pvr2_Unknown ||
              remoteType == HcwRemoteType.Pvr2_Unknown2
            )
            {
              codeName = ((HcwRemoteCodePvr2)code).ToString();
            }
            else if (remoteType == HcwRemoteType.Pvr_34Button)
            {
              codeName = ((HcwRemoteCodePvr1)code).ToString();
            }
            else if (remoteType == HcwRemoteType.Classic21Button)
            {
              codeName = ((HcwRemoteCodeClassic)code).ToString();
            }
            else if (remoteType == HcwRemoteType.Pctv)
            {
              codeName = ((HcwRemoteCodePctv)code).ToString();
            }
            else if (remoteType == HcwRemoteType.Mce)
            {
              codeName = ((HcwRemoteCodeMce)code).ToString();
            }
            else
            {
              codeName = code.ToString();
            }
            this.LogDebug("Hauppauge remote: remote control key press, remote type = {0}, code = {1}, repeat = {2}", remoteType, codeName, _repeatCount);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Hauppauge remote: remote control listener thread exception");
        return;
      }
      this.LogDebug("Hauppauge remote: remote control listener thread stop polling");
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
        return "Hauppauge remote";
      }
    }

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
      this.LogDebug("Hauppauge remote: initialising");

      lock (_instanceLock)
      {
        if (_isLoaded)
        {
          this.LogDebug("Hauppauge remote: already loaded");
          return false;
        }

        _ir32Path = GetIr32InstallPath();
        if (_ir32Path == null)
        {
          this.LogDebug("Hauppauge remote: IR32 not installed or incompatible");
          return false;
        }

        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(_ir32Path, IR32_EXE_NAME));
        if (versionInfo.FileVersion.CompareTo(IR32_MINIMUM_COMPATIBLE_VERSION) < 0)
        {
          this.LogDebug("Hauppauge remote: {0} {1} installed, please install IR32 {2} or later", IR32_EXE_NAME, versionInfo.FileVersion, IR32_MINIMUM_COMPATIBLE_VERSION);
          return false;
        }
        versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(_ir32Path, IR32_DLL_NAME));
        if (versionInfo.FileVersion.CompareTo(IR32_MINIMUM_COMPATIBLE_VERSION) < 0)
        {
          this.LogDebug("Hauppauge remote: {0} {1} installed, please install IR32 {2} or later", IR32_DLL_NAME, versionInfo.FileVersion, IR32_MINIMUM_COMPATIBLE_VERSION);
          return false;
        }

        _isLoaded = true;
      }

      this.LogInfo("Hauppauge remote: extension supported");
      _isHauppaugeRemote = true;
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
      this.LogDebug("Hauppauge remote: open remote control interface");

      if (!_isHauppaugeRemote)
      {
        this.LogWarn("Hauppauge remote: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Hauppauge remote: remote control interface is already open");
        return true;
      }

      // We must add the IR32 directory to our path, otherwise any calls to
      // functions imported from irremote.dll will fail. This also must be
      // done in the thread from which the functions are invoked.
      if (!NativeMethods.SetDllDirectory(_ir32Path))
      {
        this.LogError("Hauppauge remote: failed to add Hauppauge IR32 directory {0} to path", _ir32Path);
        return false;
      }

      _restartIrExe = false;
      if (Process.GetProcessesByName(IR32_PROCESS_NAME).Length != 0)
      {
        // IR32 is running. We have to stop it to receive remote key presses.
        // We'll restart it when we're done.
        int i = 0;
        int processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
        this.LogDebug("Hauppauge remote: stop {0} IR32 process(es)", processCount);
        string irExePath = Path.Combine(_ir32Path, IR32_EXE_NAME);
        while ((Process.GetProcessesByName(IR32_PROCESS_NAME).Length != 0) && (i < processCount))
        {
          i++;
          try
          {
            Process.Start(irExePath, "/QUIT"); // upper case important
          }
          catch (Exception ex)
          {
            this.LogWarn(ex, "Hauppauge remote: failed to stop IR32, is the install path {0} correct?", irExePath);
          }
          Thread.Sleep(400);
        }
        processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
        if (processCount != 0)
        {
          this.LogWarn("Hauppauge remote: still {0} IR32 process(es), attempt terminate", processCount);
          foreach (Process proc in Process.GetProcessesByName(IR32_PROCESS_NAME))
          {
            proc.Kill();
          }
          Thread.Sleep(400);
          processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
          if (processCount != 0)
          {
            this.LogError("Hauppauge remote: failed to stop or terminate IR32, still {0} process(es)", processCount);
            return false;
          }
        }
        _restartIrExe = true;
      }

      try
      {
        // Open the default receiver.
        if (!IR_Open(0, 0, false, 0))
        {
          this.LogError("Hauppauge remote: failed to open IR interface, is other software active?");
          return false;
        }
        this.LogDebug("Hauppauge remote: receiver firmware version = {0}", IR_GetVersion());
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Hauppauge remote: failed to open IR interface, is other software active?");
        return false;
      }

      // Read the correct polling rate from the registry.
      bool foundRegKey = false;
      _remoteControlListenerThreadWaitTime = DEFAULT_REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME;
      List<RegistryView> views = new List<RegistryView>() { RegistryView.Default };
      if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
      {
        views.Add(RegistryView.Registry64);
      }
      foreach (RegistryView view in views)
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Hauppauge\IR"))
        {
          if (key == null)
          {
            continue;
          }
          try
          {
            object pollRateValue = key.GetValue("PollRate");
            if (pollRateValue != null)
            {
              int pollRate = (int)pollRateValue;
              if (pollRate > 0)
              {
                this.LogDebug("Hauppauge remote: poll rate = {0} ms", pollRate);
                _remoteControlListenerThreadWaitTime = pollRate;
              }
              else
              {
                this.LogDebug("Hauppauge remote: poll rate = {0} ms [default]", _remoteControlListenerThreadWaitTime);
              }
              foundRegKey = true;
              break;
            }
          }
          finally
          {
            key.Close();
          }
        }
      }
      if (!foundRegKey)
      {
        this.LogWarn("Hauppauge remote: failed to read poll rate, will use default");
      }

      _isRemoteControlInterfaceOpen = true;
      StartRemoteControlListenerThread();

      this.LogDebug("Hauppauge remote: result = success");
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
      this.LogDebug("Hauppauge remote: close remote control interface");

      if (_isRemoteControlInterfaceOpen)
      {
        if (isDisposing)
        {
          StopRemoteControlListenerThread();
        }
        try
        {
          if (!IR_Close(0, 0))
          {
            this.LogWarn("Hauppauge remote: failed to close IR interface");
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "Hauppauge remote: failed to close IR interface");
        }
      }

      if (_restartIrExe)
      {
        this.LogDebug("Hauppauge remote: restart IR32 process");
        try
        {
          int processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
          if (processCount == 0)
          {
            Process.Start(Path.Combine(_ir32Path, IR32_EXE_NAME), "/QUIET");
          }
          else
          {
            this.LogWarn("Hauppauge remote: {0} IR32 process(es) already running", processCount);
          }
          _restartIrExe = false;
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "Hauppauge remote: failed to restart IR32 process");
        }
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Hauppauge remote: result = success");
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

    ~HauppaugeRemote()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isHauppaugeRemote)
      {
        CloseRemoteControlListenerInterface(isDisposing);
        if (isDisposing)
        {
          lock (_instanceLock)
          {
            _isLoaded = false;
          }
        }
      }
      _isHauppaugeRemote = false;
    }

    #endregion
  }
}