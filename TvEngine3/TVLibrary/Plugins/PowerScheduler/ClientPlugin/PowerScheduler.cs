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

#endregion

namespace MediaPortal.Plugins.Process
{
  public class PowerScheduler : IPowerScheduler, IStandbyHandler
  {

    #region WndProc message constants
    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMQUERYSUSPEND = 0x0000;
    private const int PBT_APMQUERYSTANDBY = 0x0001;
    private const int PBT_APMSUSPEND = 0x0004;
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
    private WaitableTimer _wakeupTimer;
    private bool _refreshSettings = false;
    private DateTime _lastBusyTime = DateTime.Now;
    private PowerSettings _settings;
    private PowerManager _powerManager;
    private List<IStandbyHandler> _standbyHandlers;
    private List<IWakeupHandler> _wakeupHandlers;
    private bool _systemIdle = false;
    private bool _standbyAllowed = true;
    private Action _lastAction;
    #endregion

    #region Constructor
    public PowerScheduler()
    {
      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();

      _timer = new System.Timers.Timer();
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
    }
    #endregion

    #region Start/Stop methods
    public void Start()
    {
      Log.Info("Starting PowerScheduler client plugin...");
      if (!LoadSettings())
        return;
      _powerManager = new PowerManager();
      Register(this);
      GUIWindowManager.OnNewAction += new OnActionHandler(this.OnAction);
      _timer.Enabled = true;
      // Create the timer that will wakeup the system after a specific amount of time after the
      // system has been put into standby
      _wakeupTimer = new WaitableTimer();
      _wakeupTimer.OnTimerExpired += new WaitableTimer.TimerExpiredHandler(OnWakeupTimerExpired);
      if (!GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
      SendPowerSchedulerEvent(PowerSchedulerEventType.Started);
      Log.Info("PowerScheduler client plugin started");
    }
    public void Stop()
    {
      Log.Info("Stopping PowerScheduler client plugin...");
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
      _timer.Enabled = false;
      // disable the wakeup timer
      _wakeupTimer.SecondsToWait = -1;
      _wakeupTimer.Close();
      GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnAction);
      Unregister(this);
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
    #endregion

    #region IStandbyHandler implementation
    public bool DisAllowShutdown
    {
      get { return !_systemIdle; }
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
        if (_settings.GetSetting("SingleSeat").Get<bool>())
        {
          // fetch setting from tvservice
          intSetting = RemotePowerControl.Instance.PowerSettings.IdleTimeout;
          if (_settings.IdleTimeout != intSetting)
          {
            _settings.IdleTimeout = intSetting;
            LogVerbose("idle timeout set to: {0} minutes by tvserver", intSetting);
            changed = true;
          }
        }
        else
        {
          // fetch local settings
          intSetting = reader.GetValueAsInt("psclientplugin", "idletimeout", 5);
          if (_settings.IdleTimeout != intSetting)
          {
            _settings.IdleTimeout = intSetting;
            LogVerbose("idle timeout locally set to: {0} minutes", intSetting);
            changed = true;
          }
        }
        // Check configured pre-wakeup time
        intSetting = reader.GetValueAsInt("psclientplugin", "prewakeup", 60);
        if (_settings.PreWakeupTime != intSetting)
        {
          _settings.PreWakeupTime = intSetting;
          LogVerbose("pre-wakeup time set to: {0} seconds", intSetting);
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
      _lastBusyTime = DateTime.Now;
      _lastAction = action;
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
        UpdateStandbyHandler();
        CheckStandbyHandlers();
        SendPowerSchedulerEvent(PowerSchedulerEventType.Elapsed);
      }
      // explicitly catch exceptions and log them otherwise they are ignored by the Timer object
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    /// <summary>
    /// Updates the status of the PowerScheduler internal IStandbyHandler implementation
    /// This implementation only does the following basic checks:
    /// - Checks if any actions are being received by the system. If so, resets the last busy time
    /// - Checks if the global player is playing. If not, then:
    ///   - (if configured) checks whether or not the system is at the home screen
    /// </summary>
    private void UpdateStandbyHandler()
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
            _systemIdle = true;
          }
          else
          {
            _systemIdle = false;
          }
        }
        else
        {
          _systemIdle = true;
        }
      }
      else
      {
        LogVerbose("player is playing currently");
        _systemIdle = false;
      }
    }

    /// <summary>
    /// Checks if any of the registered IStandbyHandlers wants to prevent the system from entering standby.
    /// Also takes care of resetting the last busy time on standby prevention status changes and setting the
    /// appropriate thread execution state. Depending on the type of setup, the status is either forwarded to
    /// the powerscheduler service on the tvserver, or (if idletimeout is expired) putting the system into standby.
    /// Before the status changed is affected, a wakeup timer is fired in case any plugin wants to wakeup the
    /// system at a particular time.
    /// </summary>
    private void CheckStandbyHandlers()
    {
      bool standbyAllowed = true;
      string handlerName = String.Empty;
      // Check all registered ISTandbyHandlers
      foreach (IStandbyHandler handler in _standbyHandlers)
      {
        if (handler.DisAllowShutdown)
        {
          LogVerbose("System declared busy by {0}", handler.HandlerName);
          handlerName += String.Format("{0} ", handler.HandlerName);
          standbyAllowed = false;
        }
      }
      // Check all found IWakeable plugins
      ArrayList wakeables = PluginManager.WakeablePlugins;
      foreach (IWakeable wakeable in wakeables)
      {
        if (wakeable.DisallowShutdown())
        {
          LogVerbose("System declared busy by {0}", wakeable.PluginName());
          handlerName += String.Format("{0} ", wakeable.PluginName());
          standbyAllowed = false;
        }
      }
      // set thread executionstate accordingly
      if (standbyAllowed)
      {
        // set last busy time if client was busy last time we checked
        if (_standbyAllowed)
          _lastBusyTime = DateTime.Now;
        LogVerbose("System is allowed to enter standby by client");
        _powerManager.AllowStandby();
        SendPowerSchedulerEvent(PowerSchedulerEventType.SystemIdle);
      }
      else
      {
        LogVerbose("System is prevented to enter standby by client");
        _powerManager.PreventStandby();
        SendPowerSchedulerEvent(PowerSchedulerEventType.SystemBusy);
      }
      // check for singleseat or multiseat setup
      if (_settings.GetSetting("SingleSeat").Get<bool>())
      {
        // directly update status via RemotePowerControl (tvserver)
        LogVerbose("updating client standby status on tvserver; standby allowed: {0}", standbyAllowed);
        RemotePowerControl.Instance.SetStandbyAllowed(standbyAllowed, handlerName);
        // if client is idle, fire off the wakeup timer
        if (standbyAllowed)
        {
          CheckWakeupHandlers();
        }
      }
      else
      {
        // multi-seat situation, so powerscheduler client plugin should initiate standby by itself
        // so, check first if system indeed is idle
        if (standbyAllowed)
        {
          // then check if idle timeout is expired
          if (_settings.IdleTimeout > 0 && _lastBusyTime.AddMinutes(_settings.IdleTimeout) < DateTime.Now)
          {
            // it has expired, so activate wakeup timer & put the system into standby
            LogVerbose("IdleTimeout is expired: timeout:{0}, last activity: {1}", _settings.IdleTimeout, _lastBusyTime);
            CheckWakeupHandlers();
            EnterSuspendOrHibernate(_settings.ForceShutdown);
          }
          else
          {
            LogVerbose("IdleTimeout not yet expired: timeout:{0}, last activity: {1}", _settings.IdleTimeout, _lastBusyTime);
            if (_lastAction != null)
              LogVerbose("Last action: ID:{0} {1}", _lastAction.wID, _lastAction.ToString());
          }
        }
      }
      _standbyAllowed = standbyAllowed;
    }

    /// <summary>
    /// Checks if there are any IWakeuphandlers or IWakeable plugins who might want to wakeup the
    /// system at a given time.
    /// </summary>
    private void CheckWakeupHandlers()
    {
      string handlerName = String.Empty;
      DateTime nextWakeupTime = DateTime.MaxValue;
      DateTime earliestWakeupTime = _lastBusyTime.AddMinutes(_settings.IdleTimeout);
      Log.Debug("PSClientPlugin: earliest wakeup time: {0}", earliestWakeupTime);
      // Inspect all registered IWakeupHandlers
      foreach (IWakeupHandler handler in _wakeupHandlers)
      {
        DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
        if (nextTime < nextWakeupTime)
        {
          Log.Debug("PSClientPlugin: found next wakeup time {0} by {1}", nextTime, handler.HandlerName);
          nextWakeupTime = nextTime;
          handlerName = handler.HandlerName;
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

      Log.Debug("PSClientPlugin: next wakeup time: {0}", nextWakeupTime);

      if (_settings.GetSetting("SingleSeat").Get<bool>())
      {
        // Pass earliest desired wakeup time to powerscheduler on tvserver
        RemotePowerControl.Instance.SetNextWakeupTime(nextWakeupTime, HandlerName);
      }
      else
      {
        // Use a local WaitableTimer to wakeup the system
        nextWakeupTime = nextWakeupTime.AddSeconds(-_settings.PreWakeupTime);
        if (nextWakeupTime < DateTime.MaxValue.AddSeconds(-_settings.PreWakeupTime) && nextWakeupTime > DateTime.Now)
        {
          TimeSpan delta = nextWakeupTime.Subtract(DateTime.Now);
          _wakeupTimer.SecondsToWait = delta.TotalSeconds;
          Log.Debug("PSClientPlugin: Set wakeup timer to wakeup system in {0} minutes", delta.TotalMinutes);
        }
        else
        {
          Log.Debug("PSClientPlugin: No pending events found in the future which should wakeup the system");
          _wakeupTimer.SecondsToWait = -1;
        }
      }
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

    private void Reset()
    {
      // unreference remoting singletons as they'll be reinitialized after suspend
      LogVerbose("resetting PowerScheduler RemotePowerControl interface");
      RemotePowerControl.Clear();
      LogVerbose("resetting TVServer RemoteControl interface");
      RemoteControl.Clear();
      LogVerbose("resetting last busy time to {0}", DateTime.Now.ToString());
      _lastBusyTime = DateTime.Now;
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
      if (msg.Msg == WM_POWERBROADCAST)
      {
        switch (msg.WParam.ToInt32())
        {
          case PBT_APMRESUMEAUTOMATIC:
          case PBT_APMRESUMECRITICAL:
          case PBT_APMRESUMESTANDBY:
          case PBT_APMRESUMESUSPEND:
            Reset();
            _timer.Enabled = true;
            SendPowerSchedulerEvent(PowerSchedulerEventType.ResumedFromStandby);
            break;
          case PBT_APMSUSPEND:
            CheckWakeupHandlers();
            SendPowerSchedulerEvent(PowerSchedulerEventType.EnteringStandby, false);
            _timer.Enabled = false;
            Reset();
            break;
        }
      }
      return false;
    }
    #endregion

    #region Logging wrapper methods
    private void LogVerbose(string msg)
    {
      LogVerbose(msg, null);
    }
    private void LogVerbose(string format, params object[] args)
    {
      if (_settings.ExtensiveLogging)
        Log.Debug("PSClientPlugin: " + format, args);
    }
    #endregion
  }
}
