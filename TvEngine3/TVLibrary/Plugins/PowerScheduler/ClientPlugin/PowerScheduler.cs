#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#region Usings
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using TvControl;
using TvEngine.PowerScheduler;
using TvEngine.PowerScheduler.Interfaces;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using TvPlugin;

#endregion

// TODO
// - there are still some race conditions when a timer event is processed while the remote control is dettached (RemotePowerControl.Instance set to null)
//  -> should either sync or build try-catchs around

namespace MediaPortal.Plugins.Process
{
  public class PowerScheduler : MarshalByRefObject, IPowerScheduler, IStandbyHandler, IWakeupHandler
  {
    #region WndProc message constants
    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMQUERYSUSPEND = 0x0000;
    private const int PBT_APMQUERYSTANDBY = 0x0001;
    private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
    private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
    private const int PBT_APMSUSPEND = 0x0004;
    private const int PBT_APMSTANDBY = 0x0005;
    private const int PBT_APMRESUMECRITICAL = 0x0006;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    private const int BROADCAST_QUERY_DENY = 0x424D5144;
    #endregion

    #region Events
    /// <summary>
    /// Register to this event to receive status changes from the PowerScheduler
    /// Not implemented yet
    /// </summary>
    public event PowerSchedulerEventHandler OnPowerSchedulerEvent;
    #endregion

    #region Variables
    private System.Timers.Timer _timer;
    private System.Timers.Timer _fastTimer;
    private WaitableTimer _wakeupTimer;
    private bool _refreshSettings = false;
    private DateTime _lastUserTime = DateTime.Now;  // last time the user was doing sth (action/watching)
    private PowerSettings _settings;
    private PowerManager _powerManager;
    private List<IStandbyHandler> _standbyHandlers;
    private List<IWakeupHandler> _wakeupHandlers;
    private bool _idle;
    private Action _lastAction;
    #endregion

    #region Constructor
    public PowerScheduler()
    {
      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();
      _lastUserTime = DateTime.Now;
      _idle = false;

      _timer = new System.Timers.Timer();
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);

      _fastTimer = new System.Timers.Timer();
      _fastTimer.Interval = 1000;
      _fastTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnFastTimerElapsed);
    }
    #endregion

    #region Start/Stop methods
    public void Start()
    {
      Log.Info("Starting PowerScheduler client plugin...");
      if (!LoadSettings())
        return;
      _powerManager = new PowerManager();

      GUIWindowManager.OnNewAction += new OnActionHandler(this.OnAction);
      // Create the timer that will wakeup the system after a specific amount of time after the
      // system has been put into standby
      _wakeupTimer = new WaitableTimer();
      _wakeupTimer.OnTimerExpired += new WaitableTimer.TimerExpiredHandler(OnWakeupTimerExpired);
      if (!GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
      SendPowerSchedulerEvent(PowerSchedulerEventType.Started);
      Log.Info("PowerScheduler client plugin started");

      OnResume();
    }
    public void Stop()
    {
      Log.Info("Stopping PowerScheduler client plugin...");
      
      OnStandBy();

      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
      // disable the wakeup timer
      _wakeupTimer.SecondsToWait = -1;
      _wakeupTimer.Close();
      GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnAction);
      
      _powerManager.AllowStandby();
      _powerManager = null;
      SendPowerSchedulerEvent(PowerSchedulerEventType.Stopped);
      Log.Info("PowerScheduler client plugin stopped");
    }
    #endregion

    #region Public methods

    #region IPowerScheduler implementation
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Register(IStandbyHandler handler)
    {
      if (!_standbyHandlers.Contains(handler))
        _standbyHandlers.Add(handler);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Register(IWakeupHandler handler)
    {
      if (!_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Add(handler);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Unregister(IStandbyHandler handler)
    {
      if (_standbyHandlers.Contains(handler))
        _standbyHandlers.Remove(handler);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Unregister(IWakeupHandler handler)
    {
      if (_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Remove(handler);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool IsRegistered(IStandbyHandler handler)
    {
      return _standbyHandlers.Contains(handler);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool IsRegistered(IWakeupHandler handler)
    {
      return _wakeupHandlers.Contains(handler);
    }
    public bool SuspendSystem(string source, bool force)
    {
      return EnterSuspendOrHibernate(force);
    }
    public PowerSettings Settings
    {
      get { return _settings; }
    }
    /// <summary>
    /// Resets the idle timer of the PowerScheduler. When enough time has passed (IdleTimeout), the system
    /// is suspended as soon as possible (no handler disallows shutdown).
    /// Note that the idle timer is automatically reset when the user moves the mouse or touchs the keyboard.
    /// </summary>
    public void UserActivityDetected(DateTime when)
    {
      if (when > _lastUserTime) _lastUserTime = when;
    }
    #endregion

    private DateTime _currentNextWakeupTime = DateTime.MaxValue;  // used only if multi-seat
    private String _currentNextWakeupHandler = "";  // used only if multi-seat
    private bool _currentDisAllowShutdown = false;  // used only if multi-seat
    private String _currentDisAllowShutdownHandler = "";  // used only if multi-seat

    public void GetCurrentState(bool refresh, out bool disAllowShutdown, out String disAllowShutdownHandler, out DateTime nextWakeupTime, out String nextWakeupHandler)
    {
      // check for singleseat or multiseat setup
      if (_settings.GetSetting("SingleSeat").Get<bool>())
      {
        RemotePowerControl.Instance.GetCurrentState(refresh, out disAllowShutdown, out disAllowShutdownHandler, out nextWakeupTime, out nextWakeupHandler);
        return;
      }

      if (refresh)
      {
        bool dummy= DisAllowShutdown;
        _currentNextWakeupTime= GetNextWakeupTime(DateTime.Now);
      }

      // give state
      disAllowShutdown = _currentDisAllowShutdown;
      disAllowShutdownHandler = _currentDisAllowShutdownHandler;
      nextWakeupTime = _currentNextWakeupTime;
      nextWakeupHandler = _currentNextWakeupHandler;
    }

    #region IStandbyHandler implementation
    public bool DisAllowShutdown
    {
      get {
        if (!UserInterfaceIdle)
        {
          LogVerbose("System declared busy by the user (not idle)" );
          _currentDisAllowShutdownHandler = "USER";
          _currentDisAllowShutdown = true;
          return true;
        }

        // check whether the next event is almost due, i.e. within PreNoShutdownTime seconds
        DateTime nextWakeupTime = GetNextWakeupTime(DateTime.Now);
        if (DateTime.Now >= nextWakeupTime.AddSeconds(-_settings.PreNoShutdownTime))
        {
          LogVerbose("PSClientPlugin.DisAllowShutdown: some event is almost due");
          _currentDisAllowShutdownHandler = "EVENT-DUE";
          _currentDisAllowShutdown = true;
          return true;
        }

        // Check all registered ISTandbyHandlers
        foreach (IStandbyHandler handler in _standbyHandlers)
        {
          if( handler != this )
          {
            if (handler.DisAllowShutdown)
            {
              LogVerbose("System declared busy by {0}", handler.HandlerName);
              _currentDisAllowShutdownHandler = handler.HandlerName;
              _currentDisAllowShutdown = true;
              return true;
            }
          }
        }
        // Check all found IWakeable plugins
        ArrayList wakeables = PluginManager.WakeablePlugins;
        foreach (IWakeable wakeable in wakeables)
        {
          if (wakeable.DisallowShutdown())
          {
            LogVerbose("System declared busy by {0}", wakeable.PluginName());
            _currentDisAllowShutdownHandler = wakeable.PluginName();
            _currentDisAllowShutdown = true;
            return true;
          }
        }

        _currentDisAllowShutdown = false;

        _powerManager.AllowStandby();

        return false;
      }
    }
    public string HandlerName
    {
      get { return "PowerSchedulerClientPlugin"; }
    }
    #endregion
    
    #endregion

    #region Private methods

    private bool LoadSettings()
    {
      bool changed = false;
      bool boolSetting;
      int intSetting;
      string stringSetting;
      PowerSetting setting;
      PowerSchedulerEventArgs args;

      if (_settings == null)
      {
        _settings = new PowerSettings();
        _settings.ExtensiveLogging = true;
        _settings.ShutdownEnabled = true;
        _settings.WakeupEnabled = true;
      }

      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        // Only detect singleseat/multiseat once
        if (!_refreshSettings)
        {
          stringSetting = reader.GetValueAsString("tvservice", "hostname", String.Empty);
          if (stringSetting == String.Empty)
          {
            Log.Error("This setup is not using the tvservice! PowerScheduler client plugin will not be started!");
            return false;
          }
          setting = _settings.GetSetting("SingleSeat");
          if (IsLocal(stringSetting))
          {
            Log.Info("PowerScheduler: detected a singleseat setup - delegating suspend/hibernate requests to tvserver");
            setting.Set<bool>(true);
          }
          else
          {
            Log.Info("detected a multiseat setup - using local methods to suspend/hibernate system");
            setting.Set<bool>(false);
          }
          changed = true;

          // From now on, only refresh the required settings on subsequent LoadSettings() calls
          _refreshSettings = true;
        }

        // Check if logging should be verbose
        boolSetting = reader.GetValueAsBool("psclientplugin", "extensivelogging", false);
        if (_settings.ExtensiveLogging != boolSetting)
        {
          _settings.ExtensiveLogging = boolSetting;
          Log.Debug("Extensive logging enabled: {0}", boolSetting);
          changed = true;
        }

        // Check if we only should suspend in MP's home window
        boolSetting = reader.GetValueAsBool("psclientplugin", "homeonly", true);
        setting = _settings.GetSetting("HomeOnly");
        if (setting.Get<bool>() != boolSetting)
        {
          setting.Set<bool>(boolSetting);
          LogVerbose("Only allow standby when in home screen: {0}", boolSetting);
          changed = true;
        }
        // Check if we should force the system into standby
        boolSetting = reader.GetValueAsBool("psclientplugin", "forceshutdown", false);
        if (_settings.ForceShutdown != boolSetting)
        {
          _settings.ForceShutdown = boolSetting;
          LogVerbose("Force system into standby: {0}", boolSetting);
          changed = true;
        }
        // Check configured PowerScheduler idle timeout
        intSetting = reader.GetValueAsInt("psclientplugin", "idletimeout", 5);
        if (_settings.IdleTimeout != intSetting)
        {
          _settings.IdleTimeout = intSetting;
          LogVerbose("idle timeout locally set to: {0} minutes", intSetting);
          changed = true;
        }

        // Check configured pre-wakeup time
        intSetting = reader.GetValueAsInt("psclientplugin", "prewakeup", 60);
        if (_settings.PreWakeupTime != intSetting)
        {
          _settings.PreWakeupTime = intSetting;
          LogVerbose("pre-wakeup time set to: {0} seconds", intSetting);
          changed = true;
        }

        // Check configured pre-wakeup time
        intSetting = reader.GetValueAsInt("psclientplugin", "prenoshutdown", 120);
        if (_settings.PreNoShutdownTime != intSetting)
        {
          _settings.PreNoShutdownTime = intSetting;
          LogVerbose("pre-shutdown time set to: {0} seconds", intSetting);
          changed = true;
        }

        // Check with what interval the system status should be checked
        intSetting = reader.GetValueAsInt("psclientplugin", "checkinterval", 25);
        if (_settings.CheckInterval != intSetting)
        {
          _settings.CheckInterval = intSetting;
          LogVerbose("Check interval is set to {0} seconds", intSetting);
          _timer.Interval = intSetting * 1000;
          changed = true;
        }
        // Check configured shutdown mode
        intSetting = reader.GetValueAsInt("psclientplugin", "shutdownmode", 2);
        if ((int)_settings.ShutdownMode != intSetting)
        {
          _settings.ShutdownMode = (ShutdownMode)intSetting;
          LogVerbose("Shutdown mode set to {0}", _settings.ShutdownMode);
          changed = true;
        }

        // Send message in case any setting has changed
        if (changed)
        {
          args = new PowerSchedulerEventArgs(PowerSchedulerEventType.SettingsChanged);
          args.SetData<PowerSettings>(_settings.Clone());
          SendPowerSchedulerEvent(args);
        }
      }
      return true;
    }

    /// <summary>
    ///  Checks if the given hostname/IP address is the local host
    /// </summary>
    /// <param name="serverName">hostname/IP address to check</param>
    /// <returns>is this name/address local?</returns>
    private bool IsLocal(string serverName)
    {
      LogVerbose("IsLocal(): checking if {0} is local...", serverName);
      foreach (string name in new string[] { "localhost", "127.0.0.1", System.Net.Dns.GetHostName() })
      {
        LogVerbose("Checking against {0}", name);
        if (serverName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
          return true;
      }

      IPHostEntry hostEntry = Dns.GetHostByName(Dns.GetHostName());
      foreach (IPAddress address in hostEntry.AddressList)
      {
        LogVerbose("Checking against {0}", address);
        if (address.ToString().Equals(serverName, StringComparison.CurrentCultureIgnoreCase))
          return true;
      }

      return false;
    }

    /// <summary>
    /// OnAction handler; if any action is received then last busy time is reset (i.e. idletimeout is reset)
    /// </summary>
    /// <param name="action">action message sent by the system</param>
    private void OnAction(Action action)
    {
      _lastUserTime = DateTime.Now;
      _lastAction = action;
      // TODO should check here if action is power-off and then set the _lastUserTime to Datime.MinValue
      // since the user is going away
    }

    /// <summary>
    /// Periodically refreshes the settings, Updates the status of the internal IStandbyHandler implementation
    /// and checks all standby handlers if standby is allowed or not.
    /// </summary>
    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        LoadSettings();

        // check for singleseat or multiseat setup
        if (_settings.GetSetting("SingleSeat").Get<bool>())
        {
          // tell the tvserver when we detected the real user the last time
          RemotePowerControl.Instance.UserActivityDetected(_lastUserTime);
        }
        else
        {
          // check whether go to standby.
          CheckForStandby();
        }
        SendPowerSchedulerEvent(PowerSchedulerEventType.Elapsed);
        
        RefreshStateDisplay(false);
      }
      // explicitly catch exceptions and log them otherwise they are ignored by the Timer object
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    /// <summary>
    /// This implementation only does the following basic checks:
    /// - Checks if the global player is playing. If not, then:
    ///   - (if configured) checks whether or not the system is at the home screen
    /// </summary>
    public bool UserInterfaceIdle
    {
      get
      {
        if (!g_Player.Playing)
        {
          // No media is playing, see if user is still active then
          LogVerbose("player is not playing currently");

          if (_settings.GetSetting("HomeOnly").Get<bool>())
          {
            int activeWindow = GUIWindowManager.ActiveWindow;
            if (activeWindow == (int)GUIWindow.Window.WINDOW_HOME || activeWindow == (int)GUIWindow.Window.WINDOW_SECOND_HOME)
            {
              LogVerbose("System is in (basic) home window so basic PSClientplugin says system is idle");
              return true;
            }
            else
            {
              _lastUserTime = DateTime.Now;
              return false;
            }
          }
          else
          {
            return true;
          }
        }
        else
        {
          LogVerbose("player is playing currently");
          _lastUserTime = DateTime.Now;
          return false;
        }
      }
    }

    /// <summary>
    /// struct for GetLastInpoutInfo
    /// </summary>
    internal struct LASTINPUTINFO
    {
      public uint cbSize;
      public uint dwTime;
    }

    /// <summary>
    /// The GetLastInputInfo function retrieves the time of the last input event.
    /// </summary>
    /// <param name="plii"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    /// <summary>
    /// Returns the current tick as uint (pref. over Environemt.TickCount which only uses int)
    /// </summary>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    static extern uint GetTickCount();

    /// <summary>
    /// This functions returns the time of the last user input recogniized,
    /// i.e. mouse moves or keyboard inputs.
    /// </summary>
    /// <returns>Last time of user input</returns>
    DateTime GetLastInputTime()
    {
      LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
      lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

      if (!GetLastInputInfo(ref lastInputInfo))
      {
        Log.Error("PowerScheduler: Unable to GetLastInputInfo!");
        return DateTime.MinValue;
      }

      long lastKick = lastInputInfo.dwTime;
      long tick = GetTickCount();

      long delta = lastKick - tick;

      if (delta > 0)
      {
        // there was an overflow (restart at 0) in the tick-counter!
        delta = delta - uint.MaxValue - 1;
      }

      LogVerbose("PowerScheduler: Last user input is {0} s ago", -delta / 1000);

      return DateTime.Now.AddMilliseconds(delta);
    }


    /// <summary>
    /// Checks if the system should enter standby
    /// </summary>
    private void CheckForStandby()
    {
      if (!_settings.ShutdownEnabled)
        return;

      // adjust _lastUserTime by user activity
      DateTime userInput = GetLastInputTime();
      if (userInput > _lastUserTime)
      {
        _lastUserTime = userInput;
        LogVerbose("PowerScheduler: User input detected at {0}", _lastUserTime);
      }

      // is anybody disallowing shutdown?
      if (!DisAllowShutdown)
      {
        if (!_idle)
        {
          Log.Info("PowerScheduler: System changed from busy state to idle state");
          _idle = true;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemIdle);
        }

        // long enough idle to go to sleep?
        if (_lastUserTime <= DateTime.Now.AddMinutes(-_settings.IdleTimeout))
        {
          // Idle timeout expired - suspend/hibernate system
          Log.Info("PowerScheduler: User idle since {0} - initiate suspend/hibernate", _lastUserTime);
          EnterSuspendOrHibernate();
        }
      }
      else
      {
        if (_idle)
        {
          Log.Info("PowerScheduler: System changed from idle state to busy state");
          _idle = false;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemBusy);
        }
      }
    }



    #region IWakeupHandler Members

    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      string handlerName = String.Empty;
      DateTime nextWakeupTime = DateTime.MaxValue;

      // Inspect all registered IWakeupHandlers
      foreach (IWakeupHandler handler in _wakeupHandlers)
      {
        if (handler != this)
        {
          DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
          if (nextTime < nextWakeupTime)
          {
            Log.Debug("PSClientPlugin: found next wakeup time {0} by {1}", nextTime, handler.HandlerName);
            nextWakeupTime = nextTime;
            handlerName = handler.HandlerName;
          }
        }
      }
      // Inspect all found IWakeable plugins from PluginManager
      ArrayList wakeables = PluginManager.WakeablePlugins;
      foreach (IWakeable wakeable in wakeables)
      {
        DateTime nextTime = wakeable.GetNextEvent(earliestWakeupTime);
        if (nextTime < nextWakeupTime)
        {
          Log.Debug("PSClientPlugin: found next wakeup time {0} by {1}", nextTime, wakeable.PluginName());
          nextWakeupTime = nextTime;
          handlerName = wakeable.PluginName();
        }
      }

      _currentNextWakeupTime = nextWakeupTime;
      _currentNextWakeupHandler = handlerName;

      return nextWakeupTime;
    }

    #endregion

    /// <summary>
    /// Sets the wakeup timer to the earliest desirable wakeup time
    /// </summary>
    private void SetWakeupTimer()
    {
      if (_settings.GetSetting("SingleSeat").Get<bool>())
      {
        // no action required since handling is done by the remote scheduler
        return;
      }

      if (_settings.WakeupEnabled)
      {
        // determine next wakeup time from IWakeupHandlers
        DateTime nextWakeup = GetNextWakeupTime(DateTime.Now);
        if (nextWakeup < DateTime.MaxValue )
        {
          nextWakeup = nextWakeup.AddSeconds(-_settings.PreWakeupTime);
          double delta = nextWakeup.Subtract(DateTime.Now).TotalSeconds;
          
          if( delta < 45 )
          {
            // the wake up event is too near, when we set the timer and the suspend process takes to long, i.e. the timer gets fired
            // while suspending, the system will NOT wake up!

            // so, we will in any case set the wait time to 45 seconds
            delta= 45;
          }
          _wakeupTimer.SecondsToWait = delta;
          Log.Debug("PowerScheduler: Set wakeup timer to wakeup system in {0} minutes", delta/60);
        }
        else
        {
          Log.Debug("PowerScheduler: No pending events found in the future which should wakeup the system");
          _wakeupTimer.SecondsToWait = -1;
        }
      }
    }

    /// <summary>
    /// Puts the system into the configured standby mode (Suspend/Hibernate)
    /// </summary>
    /// <returns>bool indicating whether or not the request was honoured</returns>
    private bool EnterSuspendOrHibernate()
    {
      return EnterSuspendOrHibernate(_settings.ForceShutdown);
    }

    /// <summary>
    /// Puts the system into standby, either by delegating the request to the powerscheduler service in the tvservice,
    /// or by itself depending on the setup.
    /// </summary>
    /// <param name="force">bool which indicates if you want to force the system</param>
    /// <returns>bool indicating whether or not the request was successful</returns>
    private bool EnterSuspendOrHibernate(bool force)
    {
      if (_settings.GetSetting("SingleSeat").Get<bool>())
      {
        // shutdown method and force mode are ignored by delegated suspend/hibernate requests
        Log.Debug("delegating suspend/hibernate request to tvserver");
        try
        {
          return RemotePowerControl.Instance.SuspendSystem("PowerSchedulerClientPlugin", force);
        }
        catch (Exception e)
        {
          Log.Error("PSClientPlugin: SuspendSystem failed! {0} {1}", e.Message, e.StackTrace);
          return false;
        }
      }
      else
      {
        // make sure we set the wakeup/resume timer before entering standby
        SetWakeupTimer();

        switch (_settings.ShutdownMode)
        {
          case ShutdownMode.Suspend:
            Log.Debug("locally suspending system (force={0})", force);
            return MediaPortal.Util.Utils.SuspendSystem(force);
          case ShutdownMode.Hibernate:
            Log.Debug("locally hibernating system (force={0})", force);
            return MediaPortal.Util.Utils.HibernateSystem(force);
          case ShutdownMode.StayOn:
            Log.Debug("standby requested but system is configured to stay on");
            return true;
          default:
            Log.Error("PSClientPlugin: unknown shutdown method: {0}", _settings.ShutdownMode);
            return false;
        }
      }
    }

    private DateTime _lastStateUpdate= DateTime.MinValue;

    private void RefreshStateDisplay(bool refresh)
    {
      TvOverlay tvoverlay = TvOverlay.Instance;
      if (tvoverlay != null)
      {
        bool disAllowShutdown;
        String disAllowShutdownHandler;
        DateTime nextWakeupTime;
        String nextWakeupHandler;
        GetCurrentState(refresh, out disAllowShutdown, out disAllowShutdownHandler, out nextWakeupTime, out nextWakeupHandler);

        // todo: descriptions in skin?!
        String text= "";

        int mode;
        if (!disAllowShutdown)
        {
          if (nextWakeupTime == DateTime.MaxValue) mode = 0;
          else
          {
            mode = 3; // text only
            text += "Wake up at " + nextWakeupTime + " for " + nextWakeupHandler;
          }
        }
        else if (disAllowShutdownHandler.Equals("EVENT-DUE"))
        {
          mode = 2; // yellow
          text += "Event is almost due at " + nextWakeupTime + " for " + nextWakeupHandler;
        }
        else
        {
          mode = 2; // yellow
          text += "Busy by " + disAllowShutdownHandler;
          if (nextWakeupTime < DateTime.MaxValue)
          {
            text += "; Wake up at " + nextWakeupTime + " for " + nextWakeupHandler;
          }
        }

        tvoverlay.UpdateState(mode, text);
      }
    }

    public delegate bool IECallBack(IntPtr hwnd, int lParam);

    [DllImport("user32.Dll")]
    public static extern int EnumWindows(IECallBack x, int y);

    private const int GW_HWNDNEXT = 2;
    private const int GW_OWNER = 4;
    private const int GW_CHILD = 5;

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd); 

    [DllImport("user32")]
    private static extern IntPtr GetParent(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count); 

    private bool EnumWindowCallBack(IntPtr hwnd, int lParam)
    {
      // find window:
      // *class  #32770 (Dialog)
      // *has static containing TVService
      // *has no parent win
      // *has no owner win
      // *process is rundll32.exe

      // check owner and parent
      if (GetWindow(hwnd, GW_OWNER).Equals( IntPtr.Zero ) && GetParent(hwnd).Equals( IntPtr.Zero ) )
      {
        // this is a top-level window
        
        // check process
        uint pid;
        GetWindowThreadProcessId(hwnd, out pid);
        if (pid != 0)
        {
          // find the process attached to the pid

          System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById((int)pid);
          if (process != null && process.ProcessName.Equals("rundll32"))
          {
            // this window is owned by a process called rundll32

            // check for static
            IntPtr child = GetWindow(hwnd, GW_CHILD);
            while (!child.Equals(IntPtr.Zero))
            {
              StringBuilder sb = new StringBuilder(1024);
              GetWindowText(child, sb, sb.Capacity);
              string window = sb.ToString();
              if (window.IndexOf("TVService") >= 0)
              {
                // gotcha!
                process.CloseMainWindow();
                Log.Info("PSClientPlugin: Closed nasty messagebox that said that TVService did not allow standby!");
                RefreshStateDisplay(true);
                return false;
              }
              child = GetWindow(child, GW_HWNDNEXT);
            }
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Periodically checks whether this nasty msgbox saying that TVService service denied standby
    /// popped up, closes it and refreshs the gui state.
    /// </summary>
    private void OnFastTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      // find window:
      // *class  #32770 (Dialog)
      // *has static containing TVService
      // *has no parent win
      // *has no owner win
      // *process is rundll32.exe

      // go thru windows
      IECallBack ewp = new IECallBack(EnumWindowCallBack);
      EnumWindows(ewp, 0);
    }

    private String _remotingURI= null;
    private int _remotingTag= 0;

    #region MarshalByRefObject overrides
    /// <summary>
    /// Make sure SAO never expires
    /// </summary>
    /// <returns></returns>
    public override object InitializeLifetimeService()
    {
      return null;
    }
    #endregion

    private void OnResume()
    {
      if( _settings.GetSetting("SingleSeat").Get<bool>() )
      {
        if (_remotingURI == null)
        {
          ChannelServices.RegisterChannel(new HttpChannel(31458), false);

          RemotingServices.Marshal(this);
          _remotingURI= "http://localhost:31458" + RemotingServices.GetObjectUri(this);
          Log.Debug("PSClientPlugin: marshalled handlers as {0}", _remotingURI);
        }
        if (_remotingTag == 0)
        {
          // register with the TVServer
          _remotingTag = RemotePowerControl.Instance.RegisterRemote(_remotingURI, _remotingURI);
          LogVerbose("PSClientPlugin: registered handlers with tvservice with tag {0}", _remotingTag);
        }
      }
      LogVerbose("resetting last user time to {0}", DateTime.Now.ToString());
      _lastUserTime = DateTime.Now;

      _timer.Enabled = true;
      _fastTimer.Enabled = true;
    }

    private void OnStandBy()
    {
      _timer.Enabled = false;
      _fastTimer.Enabled = false;

      if (_settings.GetSetting("SingleSeat").Get<bool>())
      {
        if (_remotingTag != 0)
        {
          LogVerbose("PSClientPlugin: unregister handlers with tvservice with tag {0}", _remotingTag);
          RemotePowerControl.Instance.UnregisterRemote(_remotingTag);
          _remotingTag = 0;
        }
        // unreference remoting singletons as they'll be reinitialized after suspend
        LogVerbose("resetting PowerScheduler RemotePowerControl interface");
        RemotePowerControl.Clear();
        LogVerbose("resetting TVServer RemoteControl interface");
        RemoteControl.Clear();
      }
    }

    /// <summary>
    /// Called when the wakeup timer is due (when system resumes from standby)
    /// </summary>
    private void OnWakeupTimerExpired()
    {
      Log.Debug("PSClientPlugin: OnResume");
    }

    #region Message handling
    /// <summary>
    /// Sends the given PowerScheduler event type to receivers 
    /// </summary>
    /// <param name="eventType">Event type to send</param>
    private void SendPowerSchedulerEvent(PowerSchedulerEventType eventType)
    {
      SendPowerSchedulerEvent(eventType, true);
    }

    private void SendPowerSchedulerEvent(PowerSchedulerEventType eventType, bool sendAsync)
    {
     PowerSchedulerEventArgs args = new PowerSchedulerEventArgs(eventType);
      SendPowerSchedulerEvent(args, sendAsync);
    }

    /// <summary>
    /// Sends the given PowerSchedulerEventArgs to receivers
    /// </summary>
    /// <param name="args">PowerSchedulerEventArgs to send</param>
    private void SendPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      SendPowerSchedulerEvent(args, true);
    }

    /// <summary>
    /// Sends the given PowerSchedulerEventArgs to receivers
    /// </summary>
    /// <param name="args">PowerSchedulerEventArgs to send</param>
    /// <param name="sendAsync">bool indicating whether or not to send it asynchronously</param>
    private void SendPowerSchedulerEvent(PowerSchedulerEventArgs args, bool sendAsync)
    {
      if (OnPowerSchedulerEvent == null)
        return;
      lock (OnPowerSchedulerEvent)
      {
        if (OnPowerSchedulerEvent == null)
          return;
        if (sendAsync)
        {
          OnPowerSchedulerEvent(args);
        }
        else
        {
          foreach (Delegate del in OnPowerSchedulerEvent.GetInvocationList())
          {
            PowerSchedulerEventHandler handler = del as PowerSchedulerEventHandler;
            handler(args);
          }
        }
      }
    }
    #endregion

    #endregion

    #region WndProc messagehandler
    public bool WndProc(ref System.Windows.Forms.Message msg)
    {
      // we only need to handle PowerBroadcast if we're single seat
      if (_settings.GetSetting("SingleSeat").Get<bool>())
      {
        return false;
      }

      if (msg.Msg == WM_POWERBROADCAST)
      {
        switch (msg.WParam.ToInt32())
        {
          case PBT_APMRESUMEAUTOMATIC:
          case PBT_APMRESUMECRITICAL:
          case PBT_APMRESUMESTANDBY:
          case PBT_APMRESUMESUSPEND:
          case PBT_APMQUERYSUSPENDFAILED:
          case PBT_APMQUERYSTANDBYFAILED:
            OnResume();
            SendPowerSchedulerEvent(PowerSchedulerEventType.ResumedFromStandby);
            break;
          case PBT_APMQUERYSUSPEND:
          case PBT_APMQUERYSTANDBY:
            Log.Debug("PSClientPlugin: System wants to enter standby");
            bool idle = !DisAllowShutdown;
            Log.Debug("PSClientPlugin: System idle: {0}", idle);
            if (!idle)
            {
              // scenario: User wants to standby the system, but we disallowed him to do
              // so. We reset the user activity, so the system goes to standby as soon
              // as possible.
              _lastUserTime = DateTime.MinValue;
            }

            if (!idle)
              msg.Result = new IntPtr(BROADCAST_QUERY_DENY);
            break;
          case PBT_APMSUSPEND:
          case PBT_APMSTANDBY:
            OnStandBy();

            SetWakeupTimer();
            SendPowerSchedulerEvent(PowerSchedulerEventType.EnteringStandby, false);
            break;
        }
        return true;
      }
      return false;
    }
    #endregion


    #region Logging wrapper methods
    private void LogVerbose(string msg)
    {
      //don't just do this: LogVerbose(msg, null);!!
      if (_settings.ExtensiveLogging)
        Log.Debug("PSClientPlugin: " + msg);
    }
    private void LogVerbose(string format, params object[] args)
    {
      if (_settings.ExtensiveLogging)
        Log.Debug("PSClientPlugin: " + format, args);
    }
    #endregion

    
  }
}
