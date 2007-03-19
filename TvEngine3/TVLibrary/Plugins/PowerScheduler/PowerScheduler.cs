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
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using TvControl;
using TvDatabase;
using TvEngine;
using TvEngine.Interfaces;
using TvEngine.PowerScheduler.Handlers;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Log;
using TvLibrary.Interfaces;
#endregion

namespace TvEngine.PowerScheduler
{
  /// <summary>
  /// PowerScheduler: tvservice plugin which controls power management
  /// </summary>
  public class PowerScheduler : MarshalByRefObject, IPowerScheduler, IPowerController
  {
    #region Variables
    public event PowerSchedulerEventHandler OnPowerSchedulerEvent;
    /// <summary>
    /// PowerScheduler single instance
    /// </summary>
    static PowerScheduler _powerScheduler;
    /// <summary>
    /// mutex lock object to ensure only one instance of the PowerScheduler object
    /// is created.
    /// </summary>
    static readonly object _mutex = new object();
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
    /// All PowerScheduler related settings are stored here
    /// </summary>
    PowerSettings _settings;
    /// <summary>
    /// Global indicator if system is allowed to enter standby
    /// </summary>
    bool _standbyAllowed = true;
    /// <summary>
    /// Indicator if remoting has been setup
    /// </summary>
    bool _remotingStarted = false;
    /// <summary>
    /// Indicator if the TVController should be reinitialized
    /// (or if this has already been done)
    /// </summary>
    bool _reinitializeController = false;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new PowerScheduler plugin and performs the one-time initialization
    /// </summary>
    PowerScheduler()
    {
      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();
      _clientStandbyHandler = new GenericStandbyHandler();
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
      if (_factory == null)
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

      // Configure remoting if not already done
      StartRemoting();

      LoadSettings();

      SendPowerSchedulerEvent(PowerSchedulerEventType.Started);

      Log.Info("Powerscheduler: started");
    }
    /// <summary>
    /// Called by the PowerSchedulerPlugin to stop the PowerScheduler
    /// </summary>
    public void Stop()
    {
      // Unconfigure remoting for power control
      // RemotingServices.Disconnect(this);
      // Log.Debug("PowerScheduler: Removed PowerScheduler from remoting service");

      // stop the global timer responsible for standby checking and refreshing settings
      _timer.Enabled = false;
      _timer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimerElapsed);
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

      SendPowerSchedulerEvent(PowerSchedulerEventType.Stopped);

      Log.Info("Powerscheduler: stopped");

    }
    /// <summary>
    /// Configure remoting for power control from MP
    /// </summary>
    private void StartRemoting()
    {
      if (_remotingStarted)
        return;
      try
      {
        ChannelServices.RegisterChannel(new HttpChannel(31457), false);
      }
      catch (RemotingException) { }
      catch (System.Net.Sockets.SocketException) { }
      // RemotingConfiguration.RegisterWellKnownServiceType(typeof(PowerScheduler), "PowerControl", WellKnownObjectMode.Singleton);
      ObjRef objref = RemotingServices.Marshal(this, "PowerControl", typeof(IPowerController));
      RemotePowerControl.Clear();
      Log.Debug("PowerScheduler: Registered PowerScheduler as \"PowerControl\" remoting service");
      _remotingStarted = true;
    }
    #endregion

    #region IPowerScheduler implementation
    /// <summary>
    /// Registers a new IStandbyHandler plugin which can prevent entering standby
    /// </summary>
    /// <param name="handler">handler to register</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Register(IStandbyHandler handler)
    {
      if (!_standbyHandlers.Contains(handler))
        _standbyHandlers.Add(handler);
    }
    /// <summary>
    /// Registers a new IWakeupHandler plugin which can wakeup the system at a desired time
    /// </summary>
    /// <param name="handler">handler to register</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Register(IWakeupHandler handler)
    {
      if (!_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Add(handler);
    }
    /// <summary>
    /// Unregisters a IStandbyHandler plugin
    /// </summary>
    /// <param name="handler">handler to unregister</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Unregister(IStandbyHandler handler)
    {
      if (_standbyHandlers.Contains(handler))
        _standbyHandlers.Remove(handler);
    }
    /// <summary>
    /// Unregisters a IWakeupHandler plugin
    /// </summary>
    /// <param name="handler">handler to register</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Unregister(IWakeupHandler handler)
    {
      if (_wakeupHandlers.Contains(handler))
        _wakeupHandlers.Remove(handler);
    }
    /// <summary>
    /// Checks if the given IStandbyHandler is registered
    /// </summary>
    /// <param name="handler">IStandbyHandler to check</param>
    /// <returns>is the given handler registered?</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool IsRegistered(IStandbyHandler handler)
    {
      return _standbyHandlers.Contains(handler);
    }
    /// <summary>
    /// Checks if the given IWakeupHandler is registered
    /// </summary>
    /// <param name="handler">IWakeupHandler to check</param>
    /// <returns>is the given handler registered?</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool IsRegistered(IWakeupHandler handler)
    {
      return _wakeupHandlers.Contains(handler);
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
      LogVerbose("PowerScheduler.SetStandbyAllowed: {0} {1}", standbyAllowed, handlerName);
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
      LogVerbose("PowerScheduler.SetNextWakeupTime: {0} {1}", nextWakeupTime, handlerName);
      _clientWakeupHandler.Update(nextWakeupTime, handlerName);
    }
    /// <summary>
    /// Indicates whether or not the client is connected to the server (or not)
    /// </summary>
    public bool IsConnected
    {
      get { return true; }
    }
    public IPowerSettings PowerSettings
    {
      get { return _settings; }
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
      try
      {
        LoadSettings();
        CheckForStandby();
        SendPowerSchedulerEvent(PowerSchedulerEventType.Elapsed);
      }
      // explicitly catch exceptions and log them otherwise they are ignored by the Timer object
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    /// <summary>
    /// Refreshes the standby configuration
    /// </summary>
    private void LoadSettings()
    {
      int setting;
      bool changed = false;

      if (_settings == null)
        _settings = new PowerSettings();

      TvBusinessLayer layer = new TvBusinessLayer();

      // Check if PowerScheduler should log verbose debug messages
      if (_settings.ExtensiveLogging != Convert.ToBoolean(layer.GetSetting("PowerSchedulerExtensiveLogging", "false").Value))
      {
        _settings.ExtensiveLogging = !_settings.ExtensiveLogging;
        Log.Debug("PowerScheduler: extensive logging enabled: {0}", _settings.ExtensiveLogging);
        changed = true;
      }
      // Check if PowerScheduler should actively put the system into standby
      if (_settings.ShutdownEnabled != Convert.ToBoolean(layer.GetSetting("PowerSchedulerShutdownActive", "false").Value))
      {
        _settings.ShutdownEnabled = !_settings.ShutdownEnabled;
        LogVerbose("PowerScheduler: entering standby is enabled: {0}", _settings.ShutdownEnabled);
        changed = true;
      }
      // Check if PowerScheduler should wakeup the system automatically
      if (_settings.WakeupEnabled != Convert.ToBoolean(layer.GetSetting("PowerSchedulerWakeupActive", "false").Value))
      {
        _settings.WakeupEnabled = !_settings.WakeupEnabled;
        LogVerbose("PowerScheduler: automatic wakeup is enabled: {0}", _settings.WakeupEnabled);
        changed = true;
      }
      // Check if PowerScheduler should force the system into suspend/hibernate
      if (_settings.ForceShutdown != Convert.ToBoolean(layer.GetSetting("PowerSchedulerForceShutdown", "false").Value))
      {
        _settings.ForceShutdown = !_settings.ForceShutdown;
        LogVerbose("PowerScheduler: force shutdown enabled: {0}", _settings.ForceShutdown);
        changed = true;
      }
      // Check if PowerScheduler should reinitialize the TVController after wakeup
      PowerSetting pSetting = _settings.GetSetting("ReinitializeController");
      bool bSetting = Convert.ToBoolean(layer.GetSetting("PowerSchedulerReinitializeController", "false").Value);
      if (pSetting.Get<bool>() != bSetting)
      {
        pSetting.Set<bool>(bSetting);
        LogVerbose("PowerScheduler: Reinitialize tvservice controller on wakeup: {0}", bSetting);
        changed = true;
      }

      // Check configured PowerScheduler idle timeout
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerIdleTimeout", "5").Value);
      if (_settings.IdleTimeout != setting)
      {
        _settings.IdleTimeout = setting;
        LogVerbose("PowerScheduler: idle timeout set to: {0} minutes", _settings.IdleTimeout);
        changed = true;
      }
      // Check configured pre-wakeup time
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerPreWakeupTime", "60").Value);
      if (_settings.PreWakeupTime != setting)
      {
        _settings.PreWakeupTime = setting;
        LogVerbose("PowerScheduler: pre-wakeup time set to: {0} seconds", _settings.PreWakeupTime);
        changed = true;
      }
      // Check if check interval needs to be updated
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerCheckInterval", "60").Value);
      if (_settings.CheckInterval != setting)
      {
        _settings.CheckInterval = setting;
        LogVerbose("PowerScheduler: Check interval set to {0} seconds", _settings.CheckInterval);
        setting *= 1000;
        _timer.Interval = setting;
        changed = true;
      }
      // Check configured shutdown mode
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerShutdownMode", "2").Value);
      if ((int)_settings.ShutdownMode != setting)
      {
        _settings.ShutdownMode = (ShutdownMode)setting;
        LogVerbose("PowerScheduler: Shutdown mode set to {0}", _settings.ShutdownMode);
        changed = true;
      }

      // Send message in case any setting has changed
      if (changed)
      {
        PowerSchedulerEventArgs args = new PowerSchedulerEventArgs(PowerSchedulerEventType.SettingsChanged);
        args.SetData<PowerSettings>(_settings.Clone());
        SendPowerSchedulerEvent(args);
      }
    }

    /// <summary>
    /// Checks if the system should enter standby
    /// </summary>
    private void CheckForStandby()
    {
      if (!_settings.ShutdownEnabled)
        return;
      if (SystemIdle)
      {
        if (!_idle)
        {
          Log.Info("PowerScheduler: System changed from busy state to idle state");
          _lastIdleTime = DateTime.Now;
          _idle = true;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemIdle);
        }
        else
        {
          if (_lastIdleTime <= DateTime.Now.AddMinutes(-_settings.IdleTimeout))
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
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemBusy);
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
          _reinitializeController = true;
          bool idle = SystemIdle;
          Log.Debug("PowerScheduler: System idle: {0}", idle);
          SetWakeupTimer();
          if (idle)
          {
            SendPowerSchedulerEvent(PowerSchedulerEventType.EnteringStandby, false);
            _timer.Enabled = false;
          }
          return idle;
        case System.ServiceProcess.PowerBroadcastStatus.QuerySuspendFailed:
          Log.Debug("PowerScheduler: Entering standby was disallowed (blocked)");
          _reinitializeController = false;
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
          lock (this)
          {
            // reinitialize TVController if system is configured to do so and not already done
            ReinitializeController();
          }
          ResetAndEnableTimer();
          SendPowerSchedulerEvent(PowerSchedulerEventType.ResumedFromStandby);
          return true;
      }
      return true;
    }

    /// <summary>
    /// Resets the last time the system changed from busy to idle state
    /// and re-enables the timer which periodically checks for config changes/power management
    /// </summary>
    private void ResetAndEnableTimer()
    {
      _lastIdleTime = DateTime.Now;
      _idle = false;
      _timer.Enabled = true;
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
      switch (_settings.ShutdownMode)
      {
        case ShutdownMode.Suspend:
          state = PowerState.Suspend;
          break;
        case ShutdownMode.Hibernate:
          state = PowerState.Hibernate;
          break;
        case ShutdownMode.StayOn:
          Log.Debug("PowerScheduler: Standby requested but system is configured to stay on");
          return false;
        default:
          Log.Error("PowerScheduler: unknown shutdown mode: {0}", _settings.ShutdownMode);
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
      if (_settings.WakeupEnabled)
      {
        // determine next wakeup time from IWakeupHandlers
        DateTime nextWakeup = NextWakeupTime;
        if (nextWakeup < DateTime.MaxValue.AddSeconds(-_settings.PreWakeupTime) && nextWakeup > DateTime.Now)
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

    #region Logging wrapper methods
    private void LogVerbose(string msg)
    {
      LogVerbose(msg, null);
    }
    private void LogVerbose(string format, params object[] args)
    {
      if (_settings.ExtensiveLogging)
        Log.Debug(format, args);
    }
    #endregion

    private void ReinitializeController()
    {
      // only reinitialize controller if enabled in settings
      if (_settings.GetSetting("ReinitializeController").Get<bool>())
      {
        TvService.TVController controller = _controller as TvService.TVController;
        if (controller != null && _reinitializeController)
        {
          Log.Debug("PowerScheduler: reinitializing the tvservice TVController");
          controller.Restart();
          _reinitializeController = false;
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
        {
          LogVerbose("PowerScheduler.SystemIdle: inspecting handler {0}", handler.HandlerName);
          if (handler.DisAllowShutdown)
          {
            LogVerbose("PowerScheduler.SystemIdle: handler {0} wants to prevent standby", handler.HandlerName);
            if (!_idle && !_lastStandbyPreventer.Equals(handler.HandlerName))
            {
              _lastStandbyPreventer = handler.HandlerName;
              Log.Debug("PowerScheduler: System declared busy by {0}", handler.HandlerName);
            }
            _standbyAllowed = !_powerManager.PreventStandby();
            return false;
          }
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
        DateTime earliestWakeupTime = _lastIdleTime.AddMinutes(_settings.IdleTimeout);
        Log.Debug("PowerScheduler: earliest wakeup time: {0}", earliestWakeupTime);
        foreach (IWakeupHandler handler in _wakeupHandlers)
        {
          DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
          LogVerbose("PowerScheduler.NextWakeupTime: inspecting handler:{0} time:{1}", handler.HandlerName, nextTime);
          if (nextTime < nextWakeupTime && nextTime >= earliestWakeupTime)
          {
            Log.Debug("PowerScheduler: found next wakeup time {0} by {1}", nextTime, handler.HandlerName);
            nextWakeupTime = nextTime;
          }
        }
        nextWakeupTime = nextWakeupTime.AddSeconds(-_settings.PreWakeupTime);
        Log.Debug("PowerScheduler: next wakeup time: {0}", nextWakeupTime);
        return nextWakeupTime;
      }
    }

    #endregion

    #region Public properties
    public static PowerScheduler Instance
    {
      get
      {
        if (_powerScheduler == null)
        {
          lock (_mutex)
          {
            if (_powerScheduler == null)
            {
              _powerScheduler = new PowerScheduler();
            }
          }
        }
        return _powerScheduler;
      }
    }
    public PowerSettings Settings
    {
      get { return _settings; }
    }
    #endregion
  }
}
