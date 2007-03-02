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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using TvControl;
using TvDatabase;
using TvEngine;
using TvEngine.Interfaces;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Log;
using TvLibrary.Interfaces;
#endregion

namespace TvEngine.PowerScheduler
{
  #region Enums
  public enum ShutdownMode
  {
    Suspend = 0,
    Hibernate = 1,
    StayOn = 2
  }
  #endregion

  /// <summary>
  /// PowerScheduler: tvservice plugin which controls power management
  /// </summary>
  public class PowerScheduler : MarshalByRefObject, IPowerScheduler, IPowerController
  {
    #region Variables
    /// <summary>
    /// Reference to tvservice's TVController
    /// </summary>
    IController _controller;
    /// <summary>
    /// Factory for creating various IStandbyHandlers/IWakeupHandlers
    /// </summary>
    PowerSchedulerFactory _factory;
    /// <summary>
    /// Manages setting the according thread execution state
    /// </summary>
    PowerManager _powerManager;
    /// <summary>
    /// List of registered standby handlers ("disable standby" plugins)
    /// </summary>
    List<IStandbyHandler> _standbyHandlers;
    /// <summary>
    /// List of registered wakeup handlers ("enable wakeup" plugins)
    /// </summary>
    List<IWakeupHandler> _wakeupHandlers;
    /// <summary>
    /// IStandbyHandler for the client in singleseat setups
    /// </summary>
    GenericStandbyHandler _clientStandbyHandler;
    /// <summary>
    /// IWakeupHandler for the client in singleseat setups
    /// </summary>
    GenericWakeupHandler _clientWakeupHandler;
    /// <summary>
    /// Timer for executing periodic checks (should we enter standby..)
    /// </summary>
    System.Timers.Timer _timer;
    /// <summary>
    /// Timer with support for waking up the system
    /// </summary>
    WaitableTimer _wakeupTimer;
    /// <summary>
    /// Last time the system changed from busy to idle state
    /// </summary>
    DateTime _lastIdleTime;
    /// <summary>
    /// Last name of the IStandbyHandler which prevented the system from entering standby
    /// </summary>
    string _lastStandbyPreventer = String.Empty;
    /// <summary>
    /// Global indicator if the PowerScheduler thinks the system is idle
    /// </summary>
    bool _idle = false;
    /// <summary>
    /// Indicates if the PowerScheduler is configured to actively put the system into standby after idletimeout
    /// </summary>
    bool _standbyEnabled = false;
    /// <summary>
    /// Global indicator if system is allowed to enter standby
    /// </summary>
    bool _standbyAllowed = true;
    /// <summary>
    /// IdleTimeout in minutes; if system is idle this long (and PowerScheduler is configured to actively put
    /// the system into standby) the system will go into the configured standby mode (suspend/hibernate)
    /// </summary>
    int _idleTimeout = 5;
    /// <summary>
    /// Time in seconds to wakeup the system before the eariest wakeup time is due
    /// </summary>
    int _preWakeupTime = 60;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new PowerScheduler plugin and performs the one-time initialization
    /// </summary>
    public PowerScheduler()
    {
      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();
      _clientStandbyHandler = new GenericStandbyHandler(_idleTimeout);
      _clientWakeupHandler = new GenericWakeupHandler();
      _lastIdleTime = DateTime.Now;
      _idle = false;
    }
    #endregion

    #region Public methods

    #region Start/Stop methods
    /// <summary>
    /// Called by the PowerSchedulerPlugin to start the PowerScheduler
    /// </summary>
    /// <param name="controller">TVController from the tvservice</param>
    public void Start(IController controller)
    {
      _controller = controller;

      // register to power events generated by the system
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().AddPowerEventHandler(new PowerEventHandler(OnPowerEvent));
        Log.Debug("PowerScheduler: Registered PowerScheduler as IPowerEventHandler for tvservice");
      }
      else
      {
        Log.Error("PowerScheduler: Unable to register power event handler!");
      }

      // Add ourselves to the GlobalServiceProvider
      GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
      Log.Debug("PowerScheduler: Registered PowerScheduler service to GlobalServiceProvider");

      // Create the default set of standby/resume handlers
      _factory = new PowerSchedulerFactory(controller);
      _factory.CreateDefaultSet();
      Register(_clientStandbyHandler);
      Register(_clientWakeupHandler);
      Log.Debug("PowerScheduler: Registered default set of standby/resume handlers to PowerScheduler");

      // Create the PowerManager that helps setting the correct thread executation state
      _powerManager = new PowerManager();

      // Create the timer that will wakeup the system after a specific amount of time after the
      // system has been put into standby
      _wakeupTimer = new WaitableTimer();
      _wakeupTimer.OnTimerExpired += new WaitableTimer.TimerExpiredHandler(OnWakeupTimerExpired);

      // start the timer responsible for standby checking and refreshing settings
      _timer = new System.Timers.Timer();
      _timer.Interval = 60000;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
      _timer.Enabled = true;

      // Configure remoting for power control
      ChannelServices.RegisterChannel(new HttpChannel(31457), false);
      // RemotingConfiguration.RegisterWellKnownServiceType(typeof(PowerScheduler), "PowerControl", WellKnownObjectMode.Singleton);
      ObjRef objref = RemotingServices.Marshal(this, "PowerControl", typeof(IPowerController));
      RemotePowerControl.Clear();
      Log.Debug("PowerScheduler: Registered PowerScheduler as \"PowerControl\" remoting service");

      Log.Info("Powerscheduler: started");
    }
    /// <summary>
    /// Called by the PowerSchedulerPlugin to stop the PowerScheduler
    /// </summary>
    public void Stop()
    {
      // Unconfigure remoting for power control
      RemotingServices.Disconnect(this);
      Log.Debug("PowerScheduler: Removed PowerScheduler from remoting service");

      // stop the global timer responsible for standby checking and refreshing settings
      _timer.Enabled = false;
      _timer.Dispose();
      Log.Debug("PowerScheduler: Disabled standby timer");

      // disable the wakeup timer
      _wakeupTimer.SecondsToWait = -1;
      _wakeupTimer.Close();

      // dereference the PowerManager instance
      _powerManager = null;

      // Remove the default set of standby/resume handlers
      _factory.RemoveDefaultSet();
      Unregister(_clientStandbyHandler);
      Unregister(_clientWakeupHandler);
      Log.Debug("PowerScheduler: Removed default set of standby/resume handlers to PowerScheduler");

      // Remove ourselves from the GlobalServiceProvider
      GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
      Log.Debug("PowerScheduler: Removed PowerScheduler service from GlobalServiceProvider");

      // unregister from power events generated by the system
      GlobalServiceProvider.Instance.Get<IPowerEventHandler>().RemovePowerEventHandler(new PowerEventHandler(OnPowerEvent));
      Log.Debug("PowerScheduler: Removed PowerScheduler as IPowerEventHandler from tvservice");

      Log.Info("Powerscheduler: stopped");

    }
    #endregion

    #region IPowerScheduler implementation
    /// <summary>
    /// Registers a new IStandbyHandler plugin which can prevent entering standby
    /// </summary>
    /// <param name="handler">handler to register</param>
    public void Register(IStandbyHandler handler)
    {
      if (!_standbyHandlers.Contains(handler))
        _standbyHandlers.Add(handler);
    }
    /// <summary>
    /// Registers a new IWakeupHandler plugin which can wakeup the system at a desired time
    /// </summary>
    /// <param name="handler">handler to register</param>
    public void Register(IWakeupHandler handler)
    {
      if (!_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Add(handler);
    }
    /// <summary>
    /// Unregisters a IStandbyHandler plugin
    /// </summary>
    /// <param name="handler">handler to unregister</param>
    public void Unregister(IStandbyHandler handler)
    {
      if (_standbyHandlers.Contains(handler))
        _standbyHandlers.Remove(handler);
    }
    /// <summary>
    /// Unregisters a IWakeupHandler plugin
    /// </summary>
    /// <param name="handler">handler to register</param>
    public void Unregister(IWakeupHandler handler)
    {
      if (_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Remove(handler);
    }
    /// <summary>
    /// Manually puts the system in Standby (Suspend/Hibernate depending on what is configured)
    /// </summary>
    /// <param name="source">description of the source who puts the system into standby</param>
    /// <param name="force">should we ignore PowerScheduler's current state (true) or not? (false)</param>
    /// <returns></returns>
    public bool SuspendSystem(string source, bool force)
    {
      Log.Info("PowerScheduler: Manual system suspend requested by {0}", source);
      return EnterSuspendOrHibernate(force);
    }
    #endregion

    #region IPowerController implementation
    /// <summary>
    /// Allows the PowerScheduler client plugin to register its powerstate with the tvserver PowerScheduler
    /// </summary>
    /// <param name="standbyAllowed">Is standby allowed by the client (true) or not? (false)</param>
    /// <param name="handlerName">Description of the handler preventing standby</param>
    public void SetStandbyAllowed(bool standbyAllowed, string handlerName)
    {
      //Log.Debug("PowerScheduler.SetStandbyAllowed: {0} {1}", standbyAllowed, handlerName);
      _clientStandbyHandler.DisAllowShutdown = !standbyAllowed;
      _clientStandbyHandler.HandlerName = handlerName;
    }
    /// <summary>
    /// Allows the PowerScheduler client plugin to set its desired wakeup time
    /// </summary>
    /// <param name="nextWakeupTime">desired (earliest) wakeup time</param>
    /// <param name="handlerName">Description of the handler causing the system to wakeup</param>
    public void SetNextWakeupTime(DateTime nextWakeupTime, string handlerName)
    {
      Log.Debug("PowerScheduler.SetNextWakeupTime: {0} {1}", nextWakeupTime, handlerName);
      _clientWakeupHandler.Update(nextWakeupTime, handlerName);
    }
    /// <summary>
    /// Indicates whether or not the client is connected to the server (or not)
    /// </summary>
    public bool IsConnected
    {
      get { return true; }
    }
    #endregion

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

    #endregion

    #region Private methods
    /// <summary>
    /// Called when the wakeup timer is due (when system resumes from standby)
    /// </summary>
    private void OnWakeupTimerExpired()
    {
      Log.Debug("PowerScheduler: OnResume");
    }

    /// <summary>
    /// Periodically refreshes the standby configuration and checks if the system should enter standby
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      UpdateStandbySettings();
      CheckForStandby();
    }

    /// <summary>
    /// Refreshes the standby configuration
    /// </summary>
    private void UpdateStandbySettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      // Check if PowerScheduler should actively put the system into standby
      if (Convert.ToBoolean(layer.GetSetting("PowerSchedulerShutdownActive", "false").Value))
      {
        if (!_standbyEnabled)
          Log.Debug("PowerScheduler: standby is activated");
        _standbyEnabled = true;
      }
      else
      {
        if (_standbyEnabled)
          Log.Debug("PowerScheduler: standby timer is deactivated");
        _standbyEnabled = false;
      }
      // Check if check interval needs to be updated
      int checkInterval = Int32.Parse(layer.GetSetting("PowerSchedulerCheckInterval", "60").Value);
      checkInterval *= 1000;
      if (_timer.Interval != checkInterval)
        _timer.Interval = checkInterval;

      // Update idleTimeout
      _idleTimeout = Int32.Parse(layer.GetSetting("PowerSchedulerIdleTimeout", "5").Value);

      // Update preWakeupTime
      _preWakeupTime = Int32.Parse(layer.GetSetting("PowerSchedulerPreWakeupTime", "60").Value);
    }

    /// <summary>
    /// Checks if the system should enter standby
    /// </summary>
    private void CheckForStandby()
    {
      if (!_standbyEnabled)
        return;
      if (SystemIdle)
      {
        if (!_idle)
        {
          Log.Info("PowerScheduler: System changed from busy state to idle state");
          _lastIdleTime = DateTime.Now;
          _idle = true;
        }
        else
        {
          if (_lastIdleTime <= DateTime.Now.AddMinutes(-_idleTimeout))
          {
            // Idle timeout expired - suspend/hibernate system
            Log.Info("PowerScheduler: System idle since {0} - initiate suspend/hibernate", _lastIdleTime);
            EnterSuspendOrHibernate();
          }
        }
      }
      else
      {
        if (_idle)
        {
          Log.Info("PowerScheduler: System changed from idle state to busy state");
          _idle = false;
        }
      }
    }

    /// <summary>
    /// Windows PowerEvent handler
    /// </summary>
    /// <param name="powerStatus">PowerBroadcastStatus the system is changing to</param>
    /// <returns>bool indicating if the broadcast was honoured</returns>
    private bool OnPowerEvent(System.ServiceProcess.PowerBroadcastStatus powerStatus)
    {
      switch (powerStatus)
      {
        case System.ServiceProcess.PowerBroadcastStatus.QuerySuspend:
          Log.Debug("PowerScheduler: System wants to enter standby");
          bool idle = SystemIdle;
          Log.Debug("PowerScheduler: System idle: {0}", idle);
          if (idle)
          {
            SetWakeupTimer();
            _timer.Enabled = false;
          }
          return idle;
        case System.ServiceProcess.PowerBroadcastStatus.QuerySuspendFailed:
          Log.Debug("PowerScheduler: Entering standby was disallowed (blocked)");
          ResetAndEnableTimer(); 
          return true;
        case System.ServiceProcess.PowerBroadcastStatus.ResumeAutomatic:
          Log.Debug("PowerScheduler: System has resumed automatically from standby");
          ResetAndEnableTimer();
          return true;
        case System.ServiceProcess.PowerBroadcastStatus.ResumeCritical:
          Log.Debug("PowerScheduler: System has resumed from standby after a critical suspend");
          ResetAndEnableTimer();
          return true;
        case System.ServiceProcess.PowerBroadcastStatus.ResumeSuspend:
          Log.Debug("PowerScheduler: System has resumed from standby");
          ResetAndEnableTimer();
          return true;
      }
      return true;
    }

    /// <summary>
    /// Resets the last time the system changed from budy to idle state
    /// and re-enables the timer which periodically checks for config changes/power management
    /// </summary>
    private void ResetAndEnableTimer()
    {
      _lastIdleTime = DateTime.Now;
      _idle = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      _timer.Enabled = true;
    }

    /// <summary>
    /// Puts the system into the configured standby mode (Suspend/Hibernate)
    /// </summary>
    /// <returns>bool indicating whether or not the request was honoured</returns>
    private bool EnterSuspendOrHibernate()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      bool force = bool.Parse(layer.GetSetting("PowerSchedulerForceShutdown", "false").Value);
      return EnterSuspendOrHibernate(force);
    }

    /// <summary>
    /// Puts the system into the configured standby mode (Suspend/Hibernate)
    /// </summary>
    /// <param name="force">should the system be forced to enter standby?</param>
    /// <returns>bool indicating whether or not the request was honoured</returns>
    private bool EnterSuspendOrHibernate(bool force)
    {
      if (!_standbyAllowed && !force)
        return false;
      // determine standby mode
      PowerState state = PowerState.Suspend;
      TvBusinessLayer layer = new TvBusinessLayer();
      int standbyMode = Int32.Parse(layer.GetSetting("PowerSchedulerShutdownMode", "2").Value);
      switch (standbyMode)
      {
        case (int)ShutdownMode.Suspend:
          state = PowerState.Suspend;
          break;
        case (int)ShutdownMode.Hibernate:
          state = PowerState.Hibernate;
          break;
        case (int)ShutdownMode.StayOn:
          Log.Debug("PowerScheduler: Standby requested but system is configured to stay on");
          return false;
        default:
          Log.Error("PowerScheduler: unknown shutdown mode: {0}", standbyMode);
          return false;
      }

      // make sure we set the wakeup/resume timer before entering standby
      SetWakeupTimer();

      // activate standby
      Log.Info("PowerScheduler: entering {0} ; forced: {1}", state, force);
      return Application.SetSuspendState(state, force, false);
    }

    /// <summary>
    /// Sets the wakeup timer to the earliest desirable wakeup time
    /// </summary>
    private void SetWakeupTimer()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      if (Convert.ToBoolean(layer.GetSetting("PowerSchedulerWakeupActive", "false").Value))
      {
        // determine next wakeup time from IWakeupHandlers
        DateTime nextWakeup = NextWakeupTime;
        if (nextWakeup < DateTime.MaxValue.AddSeconds(-_preWakeupTime) && nextWakeup > DateTime.Now)
        {
          TimeSpan delta = nextWakeup.Subtract(DateTime.Now);
          _wakeupTimer.SecondsToWait = delta.TotalSeconds;
          Log.Debug("PowerScheduler: Set wakeup timer to wakeup system in {0} minutes", delta.TotalMinutes);
        }
        else
        {
          Log.Debug("PowerScheduler: No pending events found in the future which should wakeup the system");
          _wakeupTimer.SecondsToWait = -1;
        }
      }
    }
    #endregion

    #region Private properties

    /// <summary>
    /// Checks all IStandbyHandlers if one of them wants to prevent standby;
    /// returns false if one of them does; returns true of none of them does.
    /// </summary>
    private bool SystemIdle
    {
      get
      {
        foreach (IStandbyHandler handler in _standbyHandlers)
          if (handler.DisAllowShutdown)
          {
            if (!_idle && !_lastStandbyPreventer.Equals(handler.HandlerName))
            {
              _lastStandbyPreventer = handler.HandlerName;
              Log.Debug("PowerScheduler: System declared busy by {0}", handler.HandlerName);
            }
            _standbyAllowed = !_powerManager.PreventStandby();
            return false;
          }
        _standbyAllowed = _powerManager.AllowStandby();
        return true;
      }
    }

    /// <summary>
    /// Returns the earliest desirable wakeup time from all IWakeupHandlers
    /// </summary>
    private DateTime NextWakeupTime
    {
      get
      {
        DateTime nextWakeupTime = DateTime.MaxValue;
        DateTime earliestWakeupTime = _lastIdleTime.AddMinutes(_idleTimeout);
        Log.Debug("PowerScheduler: earliest wakeup time: {0}", earliestWakeupTime);
        foreach (IWakeupHandler handler in _wakeupHandlers)
        {
          DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
          if (nextTime < nextWakeupTime)
          {
            Log.Debug("PowerScheduler: found next wakeup time {0} by {1}", nextTime, handler.HandlerName);
            nextWakeupTime = nextTime;
          }
        }
        nextWakeupTime = nextWakeupTime.AddSeconds(-_preWakeupTime);
        Log.Debug("PowerScheduler: next wakeup time: {0}", nextWakeupTime);
        return nextWakeupTime;
      }
    }

    #endregion
  }
}
