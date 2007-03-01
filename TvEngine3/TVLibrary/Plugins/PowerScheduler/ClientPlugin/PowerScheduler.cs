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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using TvControl;
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

    #region Variables
    System.Timers.Timer _timer;
    private bool _refreshSettings = false;
    private DateTime _lastBusyTime = DateTime.Now;
    private const int _checkInterval = 1000;
    private int _idleTimeout = 0;
    private int _preWakeupTime = 60;
    private bool _homeOnly = true;
    private bool _extensiveLogging = false;
    private bool _singleSeat = false;
    private string _shutdownMethod = "suspend";
    private bool _forceShutdown = false;
    PowerManager _powerManager;
    List<IStandbyHandler> _standbyHandlers;
    List<IWakeupHandler> _wakeupHandlers;
    private bool _systemIdle = false;
    private bool _standbyAllowed = true;
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
      if (!GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
      Log.Info("PowerScheduler client plugin started");
    }
    public void Stop()
    {
      Log.Info("Stopping PowerScheduler client plugin...");
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
      _timer.Enabled = false;
      GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnAction);
      Unregister(this);
      _powerManager = null;
      Log.Info("PowerScheduler client plugin stopped");
    }
    #endregion

    #region Public methods

    #region IPowerScheduler implementation
    public void Register(IStandbyHandler handler)
    {
      if (!_standbyHandlers.Contains(handler))
        _standbyHandlers.Add(handler);
    }
    public void Register(IWakeupHandler handler)
    {
      if (!_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Add(handler);
    }
    public void Unregister(IStandbyHandler handler)
    {
      if (_standbyHandlers.Contains(handler))
        _standbyHandlers.Remove(handler);
    }
    public void Unregister(IWakeupHandler handler)
    {
      if (_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Remove(handler);
    }
    public bool SuspendSystem(string source, bool force)
    {
      return EnterSuspendOrHibernate(force);
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
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _homeOnly = reader.GetValueAsBool("psclientplugin", "homeonly", true);
        _extensiveLogging = reader.GetValueAsBool("psclientplugin", "extensivelogging", false);
        int checkInterval = reader.GetValueAsInt("psclientplugin", "checkinterval", 25);
        checkInterval *= 1000;
        if (checkInterval != _timer.Interval)
        {
          LogDebug(String.Format("Check interval is set to {0} seconds", checkInterval), false);
          _timer.Interval = checkInterval;
        }
        _shutdownMethod = reader.GetValueAsString("psclientplugin", "shutdownmode", "suspend");
        _forceShutdown = reader.GetValueAsBool("psclientplugin", "forceshutdown", false);
        _idleTimeout = reader.GetValueAsInt("psclientplugin", "idletimeout", 0);
        _preWakeupTime = reader.GetValueAsInt("psclientplugin", "prewakeup", 60);
        if (_refreshSettings)
          return true;
        string tvServerName = reader.GetValueAsString("tvservice", "hostname", String.Empty);
        if (tvServerName == String.Empty)
        {
          Log.Error("This setup is not using the tvservice! PowerScheduler client plugin will not be started!");
          return false;
        }
        if (tvServerName == System.Net.Dns.GetHostName())
        {
          LogDebug("detected a singleseat setup - delegating suspend/hibernate requests to tvserver", false);
          _singleSeat = true;
        }
        else
        {
          LogDebug("detected a multiseat setup - using local methods to suspend/hibernate system", false);
          _singleSeat = false;
        }
        _refreshSettings = true;
        return true;
      }
    }

    /// <summary>
    /// OnAction handler; if any action is received then last busy time is reset (i.e. idletimeout is reset)
    /// </summary>
    /// <param name="action">action message sent by the system</param>
    private void OnAction(Action action)
    {
      _lastBusyTime = DateTime.Now;
    }

    /// <summary>
    /// Periodically refreshes the settings, Updates the status of the internal IStandbyHandler implementation
    /// and checks all standby handlers if standby is allowed or not.
    /// </summary>
    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      LoadSettings();
      UpdateStandbyHandler();
      CheckStandbyHandlers();
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
        LogDebug("player is not playing currently", true);

        if (_homeOnly)
        {
          int activeWindow = GUIWindowManager.ActiveWindow;
          if (activeWindow == (int)GUIWindow.Window.WINDOW_HOME || activeWindow == (int)GUIWindow.Window.WINDOW_SECOND_HOME)
          {
            LogDebug("System is in (basic) home window so basic PSClientplugin says system is idle", true);
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
        LogDebug("player is playing currently", true);
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
          LogDebug(String.Format("System declared busy by {0}", handler.HandlerName), true);
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
          LogDebug(String.Format("System declared busy by {0}", wakeable.PluginName()), true);
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
        LogDebug("System is allowed to enter standby by client", true);
        _powerManager.AllowStandby();
      }
      else
      {
        LogDebug("System is prevented to enter standby by client", true);
        _powerManager.PreventStandby();
      }
      // check for singleseat or multiseat setup
      if (_singleSeat)
      {
        // directly update status via RemotePowerControl (tvserver)
        LogDebug(String.Format("updating client standby status on tvserver; standby allowed: {0}", standbyAllowed), true);
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
          if (_idleTimeout > 0 && _lastBusyTime.AddMinutes(_idleTimeout) < DateTime.Now)
          {
            // it has expired, so activate wakeup timer & put the system into standby
            LogDebug(
              String.Format("IdleTimeout is expired: timeout:{0}, last activity: {1}", _idleTimeout, _lastBusyTime),
              true);
            CheckWakeupHandlers();
            EnterSuspendOrHibernate(_forceShutdown);
          }
          else
          {
            LogDebug(
              String.Format("IdleTimeout not yet expired: timeout:{0}, last activity: {1}", _idleTimeout, _lastBusyTime),
              true);
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
      DateTime nextWakeupTime = DateTime.MaxValue;
      DateTime earliestWakeupTime = _lastBusyTime.AddMinutes(_idleTimeout);
      LogDebug(String.Format("earliest wakeup time: {0}", earliestWakeupTime), false);
      // Inspect all registered IWakeupHandlers
      foreach (IWakeupHandler handler in _wakeupHandlers)
      {
        DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
        if (nextTime < nextWakeupTime)
        {
          LogDebug(String.Format("found next wakeup time {0} by {1}", nextTime, handler.HandlerName), false);
          nextWakeupTime = nextTime;
        }
      }
      // Inspect all found IWakeable plugins from PluginManager
      ArrayList wakeables = PluginManager.WakeablePlugins;
      foreach (IWakeable wakeable in wakeables)
      {
        DateTime nextTime = wakeable.GetNextEvent(earliestWakeupTime);
        if (nextTime < nextWakeupTime)
        {
          LogDebug(String.Format("found next wakeup time {0} by {1}", nextTime, wakeable.PluginName()), false);
          nextWakeupTime = nextTime;
        }
      }

      nextWakeupTime = nextWakeupTime.AddSeconds(-_preWakeupTime);
      LogDebug(String.Format("PowerScheduler: next wakeup time: {0}", nextWakeupTime), false);
    }

    /// <summary>
    /// Puts the system into standby, either by delegating the request to the powerscheduler service in the tvservice,
    /// or by itself depending on the setup.
    /// </summary>
    /// <param name="force">bool which indicates if you want to force the system</param>
    /// <returns>bool indicating whether or not the request was successful</returns>
    private bool EnterSuspendOrHibernate(bool force)
    {
      if (_singleSeat)
      {
        // shutdown method and force mode are ignored by delegated suspend/hibernate requests
        LogDebug("delegating suspend/hibernate request to tvserver", false);
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
        switch (_shutdownMethod.ToLower())
        {
          case "suspend":
            LogDebug(String.Format("locally suspending system (force={0})", force), false);
            return MediaPortal.Util.Utils.SuspendSystem(_forceShutdown);
          case "hibernate":
            LogDebug(String.Format("locally hibernating system (force={0})", force), false);
            return MediaPortal.Util.Utils.HibernateSystem(_forceShutdown);
          default:
            Log.Error("PSClientPlugin: unknown shutdown method: {0}", _shutdownMethod);
            return false;
        }
      }
    }

    private void Reset()
    {
      // unreference remoting singletons as they'll be reinitialized after suspend
      LogDebug("resetting PowerScheduler RemotePowerControl interface", true);
      RemotePowerControl.Clear();
      LogDebug("resetting TVServer RemoteControl interface", true);
      RemoteControl.Clear();
      LogDebug("resetting last busy time to " + DateTime.Now.ToString(), true);
      _lastBusyTime = DateTime.Now;
    }

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
            break;
          case PBT_APMSUSPEND:
            _timer.Enabled = false;
            Reset();
            break;
        }
      }
      return false;
    }
    #endregion

    #region Logging wrapper methods
    private void LogDebug(string msg)
    {
      LogDebug(msg, false);
    }
    private void LogDebug(string msg, bool extensive)
    {
      if (extensive && !_extensiveLogging)
        return;
      Log.Debug(String.Format("PSClientPlugin: {0}", msg));
    }
    #endregion
  }
}
