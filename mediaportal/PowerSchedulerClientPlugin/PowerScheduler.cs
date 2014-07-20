#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Plugins.Process.Handlers;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Util;
using TvEngine.PowerScheduler;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace MediaPortal.Plugins.Process
{
  /// <summary>
  /// PowerScheduler Client Plugin: process plugin which controls power management
  /// </summary>
  public class PowerScheduler : MarshalByRefObject, IPowerScheduler, IStandbyHandlerEx, IWakeupHandler
  {
    #region Variables

    /// <summary>
    /// Thread starting and stopping PowerScheduler
    /// </summary>
    private Thread _parentThread;

    /// <summary>
    /// Factory for creating various IStandbyHandlers/IWakeupHandlers
    /// </summary>
    private PowerSchedulerFactory _factory;

    /// <summary>
    /// List of registered standby handlers ("disable standby" plugins)
    /// </summary>
    private List<IStandbyHandler> _standbyHandlers;

    /// <summary>
    /// List of registered wakeup handlers ("enable wakeup" plugins)
    /// </summary>
    private List<IWakeupHandler> _wakeupHandlers;

    /// <summary>
    /// IStandbyHandler for the IWakeable plugins
    /// </summary>
    private IStandbyHandler _wakeableStandbyHandler;

    /// <summary>
    /// IWakeupHandler for the IWakeable plugins
    /// </summary>
    private IWakeupHandler _wakeableWakeupHandler;

    /// <summary>
    /// EventWaitHandle to trigger the StandbyWakeupThread
    /// </summary>
    private EventWaitHandle _standbyWakeupTriggered;

    /// <summary>
    /// EventWaitHandle to signal "Suspend" to the StandbyWakeupThread
    /// </summary>
    private EventWaitHandle _standbyWakeupSuspend;

    /// <summary>
    /// EventWaitHandle to signal "Resume" to the StandbyWakeupThread
    /// </summary>
    private EventWaitHandle _standbyWakeupResume;

    /// <summary>
    /// EventWaitHandle to signal "Finished"
    /// </summary>
    private EventWaitHandle _standbyWakeupFinished;

    /// <summary>
    /// Thread to check for standby and set wakeup timer
    /// </summary>
    private Thread _standbyWakeupThread;

    /// <summary>
    /// Timer with support for waking up the system
    /// </summary>
    private WaitableTimer _wakeupTimer;

    /// <summary>
    /// Last time any activity by the user was detected.
    /// </summary>
    private DateTime _lastUserTime;

    /// <summary>
    /// Indicator if the PowerScheduler thinks the system is idle
    /// </summary>
    private bool _idle;

    /// <summary>
    /// Indicating whether the PowerScheduler is in standby-mode.
    /// </summary>
    private bool _standby = false;

    /// <summary>
    /// Used to avoid concurrent suspend requests which could result in a suspend - user resumes - immediately suspends.
    /// </summary>
    private DateTime _ignoreSuspendUntil = DateTime.MinValue;

    /// <summary>
    /// All PowerScheduler related settings are stored here
    /// </summary>
    private PowerSettings _settings;

    /// <summary>
    /// Last time a user activity was signaled to the server
    /// </summary>
    private DateTime _lastUserActivitySignaled = DateTime.MinValue;

    /// <summary>
    /// Indicator if the system is in away mode (only vista/win7)
    /// </summary>
    private bool _awayMode = false;


    /// <summary>
    /// Indicator for single-seat configuration
    /// </summary>
    private bool _singleSeat;

    private String _remotingURI = null;
    private int _remotingTag = 0;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new PowerScheduler plugin and performs the one-time initialization
    /// </summary>
    public PowerScheduler()
    {
      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();
      _lastUserTime = DateTime.Now;
      _idle = false;
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

    #region IPowerScheduler implementation

    /// <summary>
    /// Register to this event to receive status changes from the PowerScheduler
    /// </summary>
    public event PowerSchedulerEventHandler OnPowerSchedulerEvent;

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
    /// Requests suspension of the system
    /// </summary>
    /// <param name="source">Description of who wants to suspend the system</param>
    /// <param name="force">Force the system to suspend (XP only)</param>
    public void SuspendSystem(string source, bool force)
    {
      Log.Info("PS: System suspend requested by {0}", string.IsNullOrEmpty(source) ? "PowerScheduler" : source);

      // determine standby mode
      switch (_settings.ShutdownMode)
      {
        case ShutdownMode.Suspend:
          SuspendSystem(source, (int)RestartOptions.Suspend, force);
          break;
        case ShutdownMode.Hibernate:
          SuspendSystem(source, (int)RestartOptions.Hibernate, force);
          break;
        case ShutdownMode.Shutdown:
          SuspendSystem(source, (int)RestartOptions.ShutDown, force);
          break;
        case ShutdownMode.StayOn:
          Log.Debug("PS: Standby requested but system is configured to stay on");
          break;
        default:
          Log.Error("PS: Unknown shutdown mode: {0}", _settings.ShutdownMode);
          break;
      }
    }

    /// <summary>
    /// Puts the system into sleep mode (Suspend/Hibernate)
    /// </summary>
    /// <param name="source">Description of who wants to suspend the system</param>
    /// <param name="how">How to suspend, see MediaPortal.Util.RestartOptions</param>
    /// <param name="force">Force the system to suspend (XP only)</param>
    public void SuspendSystem(string source, int how, bool force)
    {
      Log.Debug("PS: SuspendSystem(source: {0}, how: {1}, force: {2})", source, (RestartOptions)how, force);

      if (_standby)
      {
        Log.Debug("PS: SuspendSystem aborted - suspend request is already in progress");
        return;
      }

      Log.Debug("PS: Kick off shutdown thread (how: {0})", (RestartOptions)how);
      SuspendSystemThreadEnv data = new SuspendSystemThreadEnv();
      data.that = this;
      data.how = (RestartOptions)how;
      data.force = force;
      data.source = source;

      Thread _suspendThread = new Thread(SuspendSystemThread);
      _suspendThread.Name = "PS Suspend";
      _suspendThread.Start(data);
    }

    protected class SuspendSystemThreadEnv
    {
      public PowerScheduler that;
      public RestartOptions how;
      public bool force;
      public string source;
    }

    /// <summary>
    /// Wrapper for SuspendSystemThread
    /// </summary>
    /// <param name="_data">Data object</param>
    protected static void SuspendSystemThread(object _data)
    {
      SuspendSystemThreadEnv data = (SuspendSystemThreadEnv)_data;
      data.that.SuspendSystemThread(data.source, data.how, data.force);
    }

    /// <summary>
    /// Thread that puts the system into sleep mode (Suspend/Hibernate)
    /// </summary>
    /// <param name="how">How to suspend, see MediaPortal.Util.RestartOptions</param>
    protected void SuspendSystemThread(string source, RestartOptions how, bool force)
    {
      Log.Debug("PS: Shutdown thread is running: how: {0}, force: {1}", how, force);

      Log.Debug("PS: Informing handlers about UserShutdownNow");
      ((IStandbyHandler)this).UserShutdownNow();

      if (source == "System")
      {
        // XP only: We just denied a QUERYSUSPEND message and called SuspendSystem() with source "System".
        // Before actually suspending, we have to wait for the outstanding QUERYSUSPENDFAILED message (only Windows XP).
        _QuerySuspendFailedCount++;
        Log.Debug("PS: {0} outstanding QUERYSUSPENDFAILED messages", _QuerySuspendFailedCount);
        do
        {
          System.Threading.Thread.Sleep(1000);
        } while (_QuerySuspendFailedCount > 0);
      }
      _denyQuerySuspend = false;

      // Activate standby
      Log.Info("PS: Entering shutdown: how: {0}, force: {1}", (RestartOptions)how, force);
      WindowsController.ExitWindows((RestartOptions)how, force);
    }

    /// <summary>
    /// Puts the system in sleep mode (Suspend/Hibernate depending on what is configured)
    /// </summary>
    private void SuspendSystem()
    {
      SuspendSystem("", false);
    }

    /// <summary>
    /// User activity was detected (must be implemented but is not used on client).
    /// </summary>
    /// <param name="when">Time of last user activity (local client) or DateTime.MinValue (remote client)</param>
    public void UserActivityDetected(DateTime when)
    {
      if (when > _lastUserTime)
      {
        Log.Debug("PS: Set time of last user activity to {0:T}", when);
        _lastUserTime = when;
      }
    }

    /// <summary>
    /// Indicating current state
    /// </summary>
    private DateTime _currentNextWakeupTime = DateTime.MaxValue;          // next time when the system has to wakeup from suspend
    private String _currentNextWakeupHandler = "";                        // handler that wants to wakeup system
    private StandbyMode _currentStandbyMode = StandbyMode.StandbyAllowed; // indicates requested standby mode
    private String _currentStandbyHandler = "";                           // handlers that want to disallow standby
    private bool _denyQuerySuspend = true;                                // indicates if we should deny QUERYSUSPEND messages (Win XP)
    private int _QuerySuspendFailedCount = 0;                             // how many QUERQSUSPENDFAILED messages are still outstanding (Win XP)

    /// <summary>
    /// Get the current state.
    /// </summary>
    /// <param name="refresh">Indicates if state should be rechecked or stored values should be returned</param>
    /// <param name="unattended"></param>
    /// <param name="disAllowShutdown"></param>
    /// <param name="disAllowShutdownHandler"></param>
    /// <param name="nextWakeupTime"></param>
    /// <param name="nextWakeupHandler"></param>
    public void GetCurrentState(bool refresh, out bool unattended, out bool disAllowShutdown,
      out String disAllowShutdownHandler, out DateTime nextWakeupTime, out String nextWakeupHandler)
    {
      if (refresh)
      {
        // Trigger StandbyWakeupThread to check for standby conditions and next wakeup time and wait until it has done its work
        _standbyWakeupFinished.Reset();
        _standbyWakeupTriggered.Set();
        Log.Debug("PS: GetCurrentState - Reset \"Finished\" and triggered StandbyWakeupThread");

        _standbyWakeupFinished.WaitOne(100);
        Log.Debug("PS: GetCurrentState - StandbyWakeupThread signaled \"Finished\"");
      }
      unattended = _lastUserTime.AddMinutes(_settings.IdleTimeout) <= DateTime.Now;
      disAllowShutdown = _currentStandbyMode != StandbyMode.StandbyAllowed;
      disAllowShutdownHandler = _currentStandbyHandler;
      nextWakeupTime = _currentNextWakeupTime;
      nextWakeupHandler = _currentNextWakeupHandler;
    }

    /// <summary>
    /// Checks if a suspend request is in progress
    /// </summary>
    /// <returns>is the system currently trying to suspend?</returns>
    public bool IsSuspendInProgress()
    {
      return _standby;
    }

    /// <summary>
    /// Provides access to PowerScheduler's settings
    /// </summary>
    public PowerSettings Settings
    {
      get { return _settings; }
    }

    #endregion

    #region IStandbyHandler(Ex) implementation

    /// <summary>
    /// Indicator which standby mode is requested by the handler
    /// </summary>
    StandbyMode IStandbyHandlerEx.StandbyMode
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        StandbyMode standbyMode = StandbyMode.StandbyAllowed;
        string standbyHandler = "";

        // First ask the standby handlers
        foreach (IStandbyHandler handler in _standbyHandlers)
        {
          StandbyMode handlerStandbyMode;

          if (handler is IStandbyHandlerEx)
            handlerStandbyMode = ((IStandbyHandlerEx)handler).StandbyMode;
          else
            handlerStandbyMode = handler.DisAllowShutdown ? StandbyMode.StandbyPrevented : StandbyMode.StandbyAllowed;
          if (handlerStandbyMode != StandbyMode.StandbyAllowed)
          {
            if (standbyMode != StandbyMode.AwayModeRequested)
              standbyMode = handlerStandbyMode;
            if (standbyHandler == "")
              standbyHandler = handler.HandlerName;
            else
              standbyHandler += ", " + handler.HandlerName;
          }
          Log.Debug("PS: Inspecting {0}: {1}", handler.HandlerName,
            handlerStandbyMode == StandbyMode.StandbyAllowed ? "" : handlerStandbyMode.ToString());
        }
        if (standbyMode != StandbyMode.StandbyAllowed)
        {
          _currentStandbyHandler = standbyHandler;
          _currentStandbyMode = standbyMode;
          return standbyMode;
        }

        // Then check if user interface allows suspend
        Log.Debug("PS: Check if user interface is idle");
        if (!UserInterfaceIdle)
        {
          Log.Debug("PS: User interface not idle: StandbyPrevented");
          _currentStandbyMode = StandbyMode.StandbyPrevented;
          return StandbyMode.StandbyPrevented;
        }

        if (!_singleSeat)
        {
          // Then check whether the next event is almost due (within pre-no-standby time)
          Log.Debug("PS: Check whether the next event is almost due");
          if (DateTime.Now >= _currentNextWakeupTime.AddSeconds(-_settings.PreNoShutdownTime))
          {
            Log.Debug("PS: Event is almost due ({0}): StandbyPrevented", _currentNextWakeupHandler);
            _currentStandbyHandler = "Event due";
            _currentStandbyMode = StandbyMode.StandbyPrevented;
            return StandbyMode.StandbyPrevented;
          }

          // Then check if standby is allowed at this moment
          Log.Debug("PS: Check if standby is allowed at this moment");
          int Current24hHour = Convert.ToInt32(DateTime.Now.ToString("HH"));
          if ((( // Stop time one day after start time (23:00 -> 07:00)
            ((_settings.AllowedSleepStartTime > _settings.AllowedSleepStopTime)
            && (Current24hHour < _settings.AllowedSleepStartTime)
            && (Current24hHour >= _settings.AllowedSleepStopTime))
            ||
            // Start time and stop time on the same day (01:00 -> 17:00)
            ((_settings.AllowedSleepStartTime < _settings.AllowedSleepStopTime)
            &&
            // 2 possibilities for the same day: before or after the timespan
            ((Current24hHour < _settings.AllowedSleepStartTime) ||
            (Current24hHour >= _settings.AllowedSleepStopTime))
            )) && ((int)DateTime.Now.DayOfWeek > 0) && (int)DateTime.Now.DayOfWeek < 6)
            ||
            (( // Stop Time one day after start Time (23:00 -> 07:00)
            ((_settings.AllowedSleepStartTimeOnWeekend > _settings.AllowedSleepStopTimeOnWeekend)
            && (Current24hHour < _settings.AllowedSleepStartTimeOnWeekend)
            && (Current24hHour >= _settings.AllowedSleepStopTimeOnWeekend))
            ||
            // Start Time and stop Time on the same day (01:00 -> 17:00)
            ((_settings.AllowedSleepStartTimeOnWeekend < _settings.AllowedSleepStopTimeOnWeekend)
            &&
            // 2 possibilities for the same day: before or after the Timespan
            ((Current24hHour < _settings.AllowedSleepStartTimeOnWeekend) ||
            (Current24hHour >= _settings.AllowedSleepStopTimeOnWeekend))
            )) && (((int)DateTime.Now.DayOfWeek == 0) || (int)DateTime.Now.DayOfWeek == 6)))
          {
            Log.Debug("PS: Standby is not allowed at this hour: StandbyPrevented");
            _currentStandbyHandler = "NOT-ALLOWED-TIME";
            _currentStandbyMode = StandbyMode.StandbyPrevented;
            return StandbyMode.StandbyPrevented;
          }
        }

        // Nothing prevents standby
        _currentStandbyHandler = "";
        _currentStandbyMode = StandbyMode.StandbyAllowed;
        return StandbyMode.StandbyAllowed;
      }
    }

    /// <summary>
    /// Indicator whether to prevent suspension/hibernation of the system
    /// </summary>
    bool IStandbyHandler.DisAllowShutdown
    {
      get
      {
        return (((IStandbyHandlerEx)this).StandbyMode != StandbyMode.StandbyAllowed);
      }
    }

    /// <summary>
    /// Description of the source that allows / disallows shutdown
    /// </summary>
    string IStandbyHandler.HandlerName
    {
      get
      {
        if (string.IsNullOrEmpty(_currentStandbyHandler))
          return "Client Plugin";
        else
          return string.Format("Client Plugin ({0})", _currentStandbyHandler);
      }
    }

    /// <summary>
    /// Called by the SuspendSystemThread to inform handlers
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    void IStandbyHandler.UserShutdownNow()
    {
      Log.Debug("PS: UserShutdownNow()");

      // Stop player
      Log.Debug("PS: UserShutdownNow stops player");
      StopPlayer();

      // Trigger the handlers
      foreach (IStandbyHandler handler in _standbyHandlers)
      {
        handler.UserShutdownNow();
      }

      // Leave away mode
      if (_awayMode)
      {
        Log.Debug("PS: UserShutdownNow exits away mode");

        // Exit away mode by emulation a mouse move
        try
        {
          int x = Cursor.Position.X;
          int y = Cursor.Position.Y;
          mouse_event(MOUSEEVENTF_MOVE|MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
        }
        catch (Exception ex)
        {
          Log.Error("PS: Exception in UserShutdownNow: {0}", ex);
        }
      }
    }

    #endregion

    #region IWakeupHandler implementation

    /// <summary>
    /// Retrieves the earliest wakeup time from all IWakeupHandlers
    /// </summary>
    /// <param name="earliestWakeupTime">indicates the earliest valid wake up time that is considered valid by the PowerScheduler</param>
    /// <returns>the next wakeup time</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    DateTime IWakeupHandler.GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      string handlerName = String.Empty;
      DateTime nextWakeupTime = DateTime.MaxValue;

      // Inspect all registered IWakeupHandlers
      foreach (IWakeupHandler handler in _wakeupHandlers)
      {
        DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
        if (nextTime < earliestWakeupTime)
          nextTime = DateTime.MaxValue;
        Log.Debug("PS: Inspecting {0}: {1}",
          handler.HandlerName, (nextTime < DateTime.MaxValue ? nextTime.ToString() : ""));
        if (nextTime < nextWakeupTime && nextTime >= earliestWakeupTime)
        {
          handlerName = handler.HandlerName;
          nextWakeupTime = nextTime;
        }
      }
      _currentNextWakeupHandler = handlerName;
      if (nextWakeupTime != _currentNextWakeupTime)
      {
        _currentNextWakeupTime = nextWakeupTime;
        Log.Debug("PS: New next wakeup time {0:G} found by {1}", nextWakeupTime, handlerName);
      }
      return nextWakeupTime;
    }

    /// <summary>
    /// Description of the source that retrieves the wakeup time
    /// </summary>
    string IWakeupHandler.HandlerName
    {
      get
      {
        if (string.IsNullOrEmpty(_currentNextWakeupHandler))
          return "Client Plugin";
        else
          return string.Format("Client Plugin ({0})", _currentNextWakeupHandler);
      }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Start the PowerScheduler plugin
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Start()
    {
      Log.Info("PS: Starting PowerScheduler client plugin...");

      try
      {
        // Register for MediaPortal actions to see if user is active
        GUIWindowManager.OnNewAction += new OnActionHandler(this.OnAction);

        // Create the timer that will wakeup the system after a specific amount of time after the
        // system has been put into standby
        _wakeupTimer = new WaitableTimer();

        // Load settings
        LoadSettings();

        // Register as global service provider instance
        if (!GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        {
          GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
        }
        Log.Debug("PS: Registered PowerScheduler as IPowerScheduler service to GlobalServiceProvider");

        // Register standby/wakeup handlers
        _wakeableStandbyHandler = new WakeableStandbyHandler();
        Register(_wakeableStandbyHandler);
        _wakeableWakeupHandler = new WakeableWakeupHandler();
        Register(_wakeableWakeupHandler);
        if (!_singleSeat)
        {
          // Register special standby/wakeup handlers (remote client only)
          _factory = new PowerSchedulerFactory();
          _factory.CreateDefaultSet();
        }
        Log.Debug("PS: Registered standby/wakeup handlers to PowerScheduler client plugin");

        // Register PSClientPlugin as standby / wakeup handler for PowerScheduler server (single-seat only)
        if (_singleSeat)
          RegisterToRemotePowerScheduler();

        // Create the EventWaitHandles for the StandbyWakeupThread and GetCurrentState
        _standbyWakeupTriggered = new EventWaitHandle(false, EventResetMode.AutoReset, "MediaPortal.PowerScheduler.StandbyWakeupTriggered");
        _standbyWakeupFinished = new AutoResetEvent(false);
        _standbyWakeupSuspend = new AutoResetEvent(false);  // initially do not take suspend branch
        _standbyWakeupResume = new ManualResetEvent(true);  // initially releases (no block)

        // Start the StandbyWakeupThread responsible for checking standby conditions and setting the wakeup timer
        _parentThread = Thread.CurrentThread;
        _standbyWakeupThread = new Thread(StandbyWakeupThread);
        _standbyWakeupThread.Name = "PS StandbyWakeup";
        _standbyWakeupThread.Start();
        Log.Debug("PS: StandbyWakeupThread started");

        SendPowerSchedulerEvent(PowerSchedulerEventType.Started);
        Log.Info("PS: PowerScheduler client plugin started");
      }
      catch (Exception ex)
      {
        Log.Error("PS: Exception in Start: {0}", ex);
        Stop();
      }
    }

    /// <summary>
    /// Stop the PowerScheduler plugin
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Stop()
    {
      Log.Info("PS: Stopping PowerScheduler client plugin...");

      try
      {
        // Stop and remove the StandbyWakeupThread
        if (_standbyWakeupThread != null)
        {
          _standbyWakeupThread.Abort();
          _standbyWakeupThread.Join(100);
          _standbyWakeupThread = null;
        }

        // Remove the EventWaitHandles
        if (_standbyWakeupTriggered != null)
        {
          _standbyWakeupTriggered.Close();
          _standbyWakeupTriggered = null;
        }
        if (_standbyWakeupSuspend != null)
        {
          _standbyWakeupSuspend.Close();
          _standbyWakeupSuspend = null;
        }
        if (_standbyWakeupResume != null)
        {
          _standbyWakeupResume.Close();
          _standbyWakeupResume = null;
        }
        if (_standbyWakeupFinished != null)
        {
          _standbyWakeupFinished.Close();
          _standbyWakeupFinished = null;
        }

        // Unregister PSClientPlugin as standby / wakeup handler (single-seat only)
        if (_singleSeat)
          UnregisterFromRemotePowerScheduler();

        // Remove the standby/resume handlers
        if (_factory != null)
        {
          _factory.RemoveDefaultSet();
          _factory = null;
        }
        Unregister(_wakeableStandbyHandler);
        Unregister(_wakeableWakeupHandler);
        Log.Debug("PS: Removed standby/wakeup handlers");

        // Unregister as global service provider instance
        if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        {
          GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
          Log.Debug("PS: Unregistered IPowerScheduler service from GlobalServiceProvider");
        }

        // Disable the wakeup timer
        if (_wakeupTimer != null)
        {
          _wakeupTimer.TimeToWakeup = DateTime.MaxValue;
          _wakeupTimer.Close();
          _wakeupTimer = null;
        }

        // Unregister MediaPortal actions
        GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnAction);

        SendPowerSchedulerEvent(PowerSchedulerEventType.Stopped);
        Log.Info("PS: PowerScheduler client plugin stopped");
      }
      catch (Exception ex)
      {
        Log.Error("PS: Exception in Stop: {0}", ex);
      }
    }

    /// <summary>
    /// Windows PowerEvent handler
    /// </summary>
    /// <param name="msg">The Windows Message to process</param>
    /// <returns>bool indicating if the message should not be handled by other plugins</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool WndProc(ref Message msg)
    {
      // Handle power broadcast message
      if (msg.Msg == PowerManager.WM_POWERBROADCAST)
      {
        switch (msg.WParam.ToInt32())
        {
          // Windows XP only - Requests permission to suspend the computer.
          case PowerManager.PBT_APMQUERYSUSPEND:
            Log.Debug("PS: QUERYSUSPEND");
            if (!_singleSeat && _denyQuerySuspend)
            {
              // We reject all requests for suspend not coming from PowerScheduler by returning false.
              // Instead we start our own shutdown thread that issues a new QUERYSUSPEND that we will accept.
              // Always try to Hibernate (S4). If system is set to S3, then Hibernate will fail and result will be S3
              Log.Debug("PS: Suspend queried by another application - deny suspend and start own suspend sequence");
              ((IPowerScheduler)this).SuspendSystem("System", (int)RestartOptions.Hibernate, false);
              msg.Result = new IntPtr(PowerManager.BROADCAST_QUERY_DENY);
            }
            break;

          // Notifies applications that the computer is about to enter a suspended state.
          case PowerManager.PBT_APMSUSPEND:
            Log.Debug("PS: SUSPEND");
            OnSuspend();
            break;

          // Windows XP only - Notifies applications that permission to suspend the computer was denied.
          case PowerManager.PBT_APMQUERYSUSPENDFAILED:
            Log.Debug("PS: QUERYSUSPENDFAILED");
            // Another application prevents our suspend
            _QuerySuspendFailedCount--;
            break;

          // Notifies applications that the system is resuming from sleep or hibernation.
          case PowerManager.PBT_APMRESUMEAUTOMATIC:
            Log.Debug("PS: RESUMEAUTOMATIC");
            OnResume();
            break;

          // Windows XP only - Notifies applications that the system has resumed operation.
          case PowerManager.PBT_APMRESUMECRITICAL:
            Log.Debug("PS: RESUMECRITICAL");
            OnResume();
            break;

          // Notifies applications that the system has resumed operation due to user activity 
          // Note: ResumeAutomatic has been triggered before
          case PowerManager.PBT_APMRESUMESUSPEND:
            Log.Debug("PS: RESUMESUSPEND");
            OnResumeSuspend();
            break;

          // Power setting change event
          case PowerManager.PBT_POWERSETTINGCHANGE:
            Log.Debug("PS: POWERSETTINGCHANGE");
            PowerManager.POWERBROADCAST_SETTING ps = (PowerManager.POWERBROADCAST_SETTING)Marshal.PtrToStructure(msg.LParam, typeof(PowerManager.POWERBROADCAST_SETTING));
            if (ps.PowerSetting == PowerManager.GUID_SYSTEM_AWAYMODE && ps.DataLength == Marshal.SizeOf(typeof(Int32)))
            {
              if (ps.Data == 1 && !_awayMode)
                OnAwayMode();
              if (ps.Data != 1 && _awayMode)
                OnRunMode();
              _awayMode = (ps.Data == 1);
            }
            break;
        }
      }
      return false;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// StandbyWakeupThread checks for standby required conditions and sets the wakeup timer.
    /// It is activated periodically or by events signaled from the standby and wakeup handlers.
    /// </summary>
    private void StandbyWakeupThread()
    {
      Thread.Sleep(1000);

      while (_parentThread.IsAlive)
      {
        // Reload settings and trigger registered handlers
        LoadSettings();
        SendPowerSchedulerEvent(PowerSchedulerEventType.Elapsed);

        // Check handlers for next wakeup time and standby allowed
        if (!_singleSeat)
        {
          SetWakeupTimer();
          CheckForStandby();
        }

        // Signal "Finished" to waiting threads
        _standbyWakeupFinished.Set();

        // Wait for next trigger event or timeout
        _standbyWakeupTriggered.Reset();
        if (_standbyWakeupTriggered.WaitOne(_settings.CheckInterval * 1000))
        {
          // Trigger event

          // Check, if the Suspend event is set (system is in standby mode)
          // (call wil never block since timeout = 0)
          if (_standbyWakeupSuspend.WaitOne(0))
          {
            // Suspend event is set
            Log.Debug("PS: StandbyWakeupThread suspended");

            // Wait for the resume event
            _standbyWakeupResume.WaitOne();
            Log.Debug("PS: StandbyWakeupThread resumed");
            Thread.Sleep(1000);
          }
          else
          {
            // Suspend event is reset
            Log.Debug("PS: StandbyWakeupThread triggered by event");
          }
        }
        else
        {
          // Check interval timeout
          Log.Debug("PS: StandbyWakeupThread triggered by check interval");
        }

        // Adjust _lastUserTime by user activity
        DateTime userInput = GetLastInputTime();
        if (userInput > _lastUserTime)
        {
          Log.Debug("PS: New user input detected - set time of last user activity to {0:T}", userInput);
          _lastUserTime = userInput;
        }

        if (_singleSeat)
        {
          // (Re-)Register client plugin as IStandby/IWakeup handler to local Tv-Server
          RegisterToRemotePowerScheduler();

          // Signal time of last user activity to the local TvServer
          if (_lastUserActivitySignaled < _lastUserTime)
          {
            Log.Debug("PS: Signal time of last user activity ({0:T}) to the local TvServer", _lastUserTime);
            if (RemotePowerControl.Instance != null && RemotePowerControl.Isconnected)
              RemotePowerControl.Instance.UserActivityDetected(_lastUserTime);
            _lastUserActivitySignaled = _lastUserTime;
          }
        }
        else
        {
          // Signal client activity to the remote TvServer
          try
          {
            if (RemotePowerControl.HostName != String.Empty)
            {
              Log.Debug("PS: Signal client activity to the remote TvServer");
              RemotePowerControl.Instance.UserActivityDetected(DateTime.MinValue);
            }
          }
          catch (Exception ex)
          {
            Log.Debug("PS: Cannot signal client activity to remote TvServer: {0}", ex.Message);
          }
        }
      }
    }

    /// <summary>
    /// Loads the standby configuration
    /// </summary>
    private void LoadSettings()
    {
      bool changed = false;
      bool boolSetting;
      int intSetting;
      string stringSetting;
      PowerSetting powerSetting;

      Log.Debug("PS: LoadSettings()");

      // Load initial settings only once
      if (_settings == null)
      {
        using (Settings reader = new MPSettings())
        {

          // Check if update of old PS settings is necessary
          if (reader.GetValue("psclientplugin", "ExpertMode") == "")
          {
            // Initialise list of old and new settings names to update
            List<String[]> settingNames = new List<String[]>();
            settingNames.Add(new String[] { "homeonly", "HomeOnly" });
            settingNames.Add(new String[] { "idletimeout", "IdleTimeout" });
            settingNames.Add(new String[] { "shutdownenabled", "ShutdownEnabled" });
            settingNames.Add(new String[] { "shutdownmode", "ShutdownMode" });

            // Update settings names
            foreach (String[] settingName in settingNames)
            {
              String settingValue = reader.GetValue("psclientplugin", settingName[0]);
              if (settingValue != "")
              {
                reader.RemoveEntry("psclientplugin", settingName[0]);
                reader.SetValue("psclientplugin", settingName[1], settingValue);
              }
            }
          }

          _settings = new PowerSettings();
          changed = true;

          // Set constant values (needed for backward compatibility)
          _settings.ForceShutdown = false;
          _settings.ExtensiveLogging = false;
          _settings.PreNoShutdownTime = 300;
          _settings.CheckInterval = 15;

          // Check if we only should suspend in MP's home window
          boolSetting = reader.GetValueAsBool("psclientplugin", "HomeOnly", false);
          powerSetting = _settings.GetSetting("HomeOnly");
          powerSetting.Set<bool>(boolSetting);
          Log.Debug("PS: Only allow standby when on home window: {0}", boolSetting);

          // Get external command
          stringSetting = reader.GetValueAsString("psclientplugin", "Command", String.Empty);
          powerSetting = _settings.GetSetting("Command");
          powerSetting.Set<string>(stringSetting);
          Log.Debug("PS: Run command on power state change: {0}", stringSetting);

          // Detect single-seat
          string tvPluginDll = Config.GetSubFolder(Config.Dir.Plugins, "windows") + @"\" + "TvPlugin.dll";
          if (File.Exists(tvPluginDll))
          {
            string hostName = reader.GetValueAsString("tvservice", "hostname", String.Empty);
            if (hostName != String.Empty && PowerManager.IsLocal(hostName))
            {
              _singleSeat = true;
              Log.Info("PS: Detected single-seat setup - TV-Server on local system");
            }
            else if (hostName == String.Empty)
            {
              _singleSeat = false;
              Log.Info("PS: Detected standalone client setup - no TV-Server configured");
            }
            else
            {
              _singleSeat = false;
              Log.Info("PS: Detected remote client setup - TV-Server on \"{0}\"", hostName);

              RemotePowerControl.HostName = hostName;
              Log.Debug("PS: Set RemotePowerControl.HostName: {0}", hostName);
            }
          }
          else
          {
            _singleSeat = false;
            Log.Info("PS: Detected standalone client setup - no TV-Plugin installed");
          }

          // Standalone client has local standby / wakeup settings
          if (!_singleSeat)
          {
            // Check if PowerScheduler should actively put the system into standby
            boolSetting = reader.GetValueAsBool("psclientplugin", "ShutdownEnabled", false);
            _settings.ShutdownEnabled = boolSetting;
            Log.Debug("PS: PowerScheduler forces system to go to standby when idle: {0}", boolSetting);

            if (_settings.ShutdownEnabled)
            {
              // Check configured shutdown mode
              intSetting = reader.GetValueAsInt("psclientplugin", "ShutdownMode", 0);
              _settings.ShutdownMode = (ShutdownMode)intSetting;
              Log.Debug("PS: Shutdown mode: {0}", _settings.ShutdownMode.ToString());
            }

            // Get idle timeout
            if (_settings.ShutdownEnabled)
            {
              intSetting = reader.GetValueAsInt("psclientplugin", "IdleTimeout", 30);
              _settings.IdleTimeout = intSetting;
              Log.Debug("PS: Standby after: {0} minutes", intSetting);
            }

            // Check configured pre-wakeup time
            intSetting = reader.GetValueAsInt("psclientplugin", "PreWakeupTime", 60);
            _settings.PreWakeupTime = intSetting;
            Log.Debug("PS: Pre-wakeup time: {0} seconds", intSetting);

            // Check configured pre-no-standby time
            intSetting = reader.GetValueAsInt("psclientplugin", "PreNoStandbyTime", 300);
            _settings.PreNoShutdownTime = intSetting;
            Log.Debug("PS: Pre-no-standby time: {0} seconds", intSetting);

            // Check allowed start time
            intSetting = reader.GetValueAsInt("psclientplugin", "StandbyHoursFrom", 0);
            _settings.AllowedSleepStartTime = intSetting;
            Log.Debug("PS: Standby allowed from {0} o' clock", _settings.AllowedSleepStartTime);

            // Check allowed stop time
            intSetting = reader.GetValueAsInt("psclientplugin", "StandbyHoursTo", 24);
            _settings.AllowedSleepStopTime = intSetting;
            Log.Debug("PS: Standby allowed until {0} o' clock", _settings.AllowedSleepStopTime);

            // Check allowed start time on weekend
            intSetting = reader.GetValueAsInt("psclientplugin", "StandbyHoursOnWeekendFrom", 0);
            _settings.AllowedSleepStartTimeOnWeekend = intSetting;
            Log.Debug("PS: Standby allowed from {0} o' clock on weekend", _settings.AllowedSleepStartTimeOnWeekend);

            // Check allowed stop time on weekend
            intSetting = reader.GetValueAsInt("psclientplugin", "StandbyHoursOnWeekendTo", 24);
            _settings.AllowedSleepStopTimeOnWeekend = intSetting;
            Log.Debug("PS: Standby allowed until {0} o' clock on weekend", _settings.AllowedSleepStopTimeOnWeekend);

            // Check if PowerScheduler should wakeup the system automatically
            intSetting = reader.GetValueAsInt("psclientplugin", "Profile", 0);
            if (intSetting == 2)
              boolSetting = false;  // Notebook
            else
              boolSetting = true;   // HTPC, Desktop
            _settings.WakeupEnabled = boolSetting;
            Log.Debug("PS: Wakeup system for various events: {0}", boolSetting);
          }
        }
      }

      // (Re-)Load settings every check interval
      if (_singleSeat)
      {
        // Connect to local tvservice (RemotePowerControl)
        if (RemotePowerControl.Instance != null && RemotePowerControl.Isconnected)
        {
          // Check if PowerScheduler should actively put the system into standby
          boolSetting = RemotePowerControl.Instance.PowerSettings.ShutdownEnabled;
          if (_settings.ShutdownEnabled != boolSetting)
          {
            _settings.ShutdownEnabled = boolSetting;
            Log.Debug("PS: Server plugin setting - PowerScheduler forces system to go to standby when idle: {0}", boolSetting);
            changed = true;
          }

          if (_settings.ShutdownEnabled)
          {
            // Get configured shutdown mode from local tvservice
            intSetting = (int)RemotePowerControl.Instance.PowerSettings.ShutdownMode;
            if ((int)_settings.ShutdownMode != intSetting)
            {
              _settings.ShutdownMode = (ShutdownMode)intSetting;
              Log.Debug("PS: Server plugin setting - Shutdown mode: {0}", _settings.ShutdownMode.ToString());
              changed = true;
            }
          }

          // Get idle timeout from local tvservice
          intSetting = RemotePowerControl.Instance.PowerSettings.IdleTimeout;
          if (_settings.IdleTimeout != intSetting)
          {
            _settings.IdleTimeout = intSetting;
            Log.Debug("PS: Server plugin setting - {0}: {1} minutes", (_settings.ShutdownEnabled ? "Standby after" : "System idle timeout"), intSetting);
            changed = true;
          }

          // Get configured pre-wakeup time from local tvservice
          intSetting = RemotePowerControl.Instance.PowerSettings.PreWakeupTime;
          if (_settings.PreWakeupTime != intSetting)
          {
            _settings.PreWakeupTime = intSetting;
            Log.Debug("PS: Pre-wakeup time: {0} seconds", intSetting);
            changed = true;
          }

          // Check if PowerScheduler should wakeup the system automatically
          boolSetting = RemotePowerControl.Instance.PowerSettings.WakeupEnabled;
          if (_settings.WakeupEnabled != boolSetting)
          {
            _settings.WakeupEnabled = boolSetting;
            Log.Debug("PS: Server plugin setting - Wakeup system for various events: {0}", boolSetting);
            changed = true;
          }
        }
        else
        {
          Log.Error("PS: Cannot connect to local tvservice to load settings");
        }
      }
      else
      {
        // Get active idle timeout for standalone client if standby is handled by Windows
        if (!_settings.ShutdownEnabled)
        {
          intSetting = (int)PowerManager.GetActivePowerSetting(PowerManager.SystemPowerSettingType.STANDBYIDLE) / 60;
          if (_settings.IdleTimeout != intSetting)
          {
            _settings.IdleTimeout = intSetting;
            Log.Debug("PS: System idle timeout: {0} minutes", intSetting);
            changed = true;
          }
        }
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
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void CheckForStandby()
    {
      Log.Debug("PS: CheckForStandby()");

      // Check handlers for allowing standby
      StandbyMode standbyMode = ((IStandbyHandlerEx)this).StandbyMode;

      // Allow / prevent system from being suspended
      Log.Debug("PS: SetStandbyMode({0})", standbyMode);
      PowerManager.SetStandbyMode(standbyMode);

      if (standbyMode == StandbyMode.StandbyAllowed)
      {
        if (!_idle)
        {
          Log.Info("PS: System changed from busy state to idle state");

          // Do not suspend for the next two minutes to prevent sudden standby
          _ignoreSuspendUntil = DateTime.Now.AddSeconds(120);

          _idle = true;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemIdle);
        }
        Log.Debug("PS: System is idle and may go to standby");

        // Suspend system if ShutdownEnabled is checked
        if (_settings.ShutdownEnabled)
        {
          DateTime idleTimeout = _lastUserTime.AddMinutes(_settings.IdleTimeout);

          if (idleTimeout <= DateTime.Now && _ignoreSuspendUntil <= DateTime.Now)
          {
            Log.Debug("PS: Active standby is enabled - go to standby now");
            SuspendSystem();
          }
          else
          {
            if (idleTimeout > _ignoreSuspendUntil)
              Log.Debug("PS: Active standby is enabled - go to standby after idle timeout at {0:T}", idleTimeout);
            else
              Log.Debug("PS: SuspendSystem aborted - wait at least until {0:T}", _ignoreSuspendUntil);
          }
        }
        else
          Log.Debug("PS: Active standby is disabled - standby is handled by Windows");
      }
      else
      {
        if (_idle)
        {
          Log.Info("PS: System changed from idle state to busy state");
          _idle = false;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemBusy);
        }
        Log.Debug("PS: System is busy and should not go to standby");
      }
    }    

    /// <summary>
    /// Called on system suspend
    /// </summary>
    private void OnSuspend()
    {
      Log.Debug("PS: System is going to suspend");
      _denyQuerySuspend = true; // reset the flag
      _standby = true;

      // Suspend the StandbyWakeup thread
      Log.Debug("PS: Signal \"Suspend\" to StandbyWakeupThread");
      _standbyWakeupResume.Reset();   // block resume
      _standbyWakeupSuspend.Set();    // set suspend branch
      _standbyWakeupTriggered.Set();  // release wait for trigger

      if (_singleSeat)
      {
        // sync problem => give the TV-Server time to run SetWakeupTimer(); before client disconnects
        System.Threading.Thread.Sleep(200);
        UnregisterFromRemotePowerScheduler();
        Log.Debug("PS: Resetting TVServer RemoteControl interface");
        GUIMessage message = new GUIMessage(GUIMessage.MessageType.PS_ONSTANDBY, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(message);
      }

      // Run external command
      Log.Debug("PS: Run external command");
      RunExternalCommand("Command", "suspend");

      Log.Debug("PS: Send \"EnteringStandby\" event");
      SendPowerSchedulerEvent(PowerSchedulerEventType.EnteringStandby, false);
    }

    /// <summary>
    /// Called on system resume
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnResume()
    {
      Log.Debug("PS: System has resumed from standby");

      // Do not suspend for the next two minutes to prevent sudden standby
      _ignoreSuspendUntil = DateTime.Now.AddSeconds(120);

      // Run external command
      Log.Debug("PS: Run external command");
      RunExternalCommand("Command", "wakeup");

      // Register PSClientPlugin as standby / wakeup handler for PowerScheduler server (single-seat only)
      if (_singleSeat)
        RegisterToRemotePowerScheduler();

      // Resume the StandbyWakeupThread
      Log.Debug("PS: Signal \"Resume\" to the StandbyWakeupThread");
      _standbyWakeupResume.Set();

      Log.Debug("PS: Send \"ResumedFromStandby\" event");
      _standby = false;
      SendPowerSchedulerEvent(PowerSchedulerEventType.ResumedFromStandby);
    }

    /// <summary>
    /// Called on ResumeSuspend
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnResumeSuspend()
    {
      // Reset time of last user activity
      Log.Debug("PS: System has resumed from standby due to user activity - reset time of last user activity");
      _lastUserTime = DateTime.Now;
    }

    /// <summary>
    /// Sets the wakeup timer to the earliest desirable wakeup time
    /// </summary>
    private void SetWakeupTimer()
    {
      Log.Debug("PS: SetWakeupTimer()");
      if (_settings.WakeupEnabled)
      {
        // Determine next wakeup time from IWakeupHandlers
        DateTime nextWakeup = ((IWakeupHandler)this).GetNextWakeupTime(DateTime.Now);
        if (nextWakeup < DateTime.MaxValue)
        {
          nextWakeup = nextWakeup.AddSeconds(-_settings.PreWakeupTime);
          
          double delta = nextWakeup.Subtract(DateTime.Now).TotalSeconds;
          if (delta < 60)
            nextWakeup = nextWakeup.AddSeconds(60 - delta);

          _wakeupTimer.TimeToWakeup = nextWakeup;
          Log.Debug("PS: Set wakeup timer to wakeup system at {0:G}", nextWakeup);
        }
        else
        {
          Log.Debug("PS: No pending events found in the future which should wakeup the system");
          _wakeupTimer.TimeToWakeup = DateTime.MaxValue;
        }
      }
      else
      {
        Log.Debug("PS: Wakeup not enabled");
        _currentNextWakeupHandler = "";
        _currentNextWakeupTime = DateTime.MaxValue;
      }
    }

    /// <summary>
    /// Runs an external command
    /// </summary>
    /// <param name="cmd">Setting to get command string from</param>
    /// <param name="action">Parameter for command</param>
    private void RunExternalCommand(String cmd, String action)
    {
      PowerSetting setting = _settings.GetSetting(cmd);
      if (string.IsNullOrEmpty(setting.Get<string>()))
        return;
      using (System.Diagnostics.Process p = new System.Diagnostics.Process())
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = setting.Get<string>();
        psi.UseShellExecute = true;
        psi.WindowStyle = ProcessWindowStyle.Minimized;
        psi.Arguments = action;
        psi.ErrorDialog = false;
        if (Environment.OSVersion.Version.Major >= 6)
        {
          psi.Verb = "runas";
        }

        p.StartInfo = psi;
        Log.Debug("PS: Starting external command: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
        try
        {
          p.Start();
          p.WaitForExit();
        }
        catch (Exception ex)
        {
          Log.Error("PS: Exception in RunExternalCommand: {0}", ex);
        }
        Log.Debug("PS: External command finished");
      }
    }

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

    private void RegisterToRemotePowerScheduler()
    {
      if (_remotingURI == null)
      {
        // Open a remoting channel
        ChannelServices.RegisterChannel(new HttpChannel(31458), false);

        // Register client's PowerScheduler ("this") as a remoting object
        RemotingServices.Marshal(this);
        // Get the URI for "this"
        _remotingURI = "http://localhost:31458" + RemotingServices.GetObjectUri(this);
        Log.Debug("PS: Marshalled client's PowerScheduler as {0} for remoting", _remotingURI);
      }

      // Run RegisterRemote() on the TvServer to register client's PowerScheduler as RemoteStandbyHandler / RemoteWakeupHandler to server's PowerScheduler
      try
      {
        int newTag = RemotePowerControl.Instance.RegisterRemote(_remotingURI, _remotingURI);
        if (_remotingTag != newTag)
        {
          if (_remotingTag != 0)
          {
            Log.Debug("PS: Reconnected to server's PowerScheduler with tag {0}", newTag);
          }
          Log.Debug("PS: Registered client's PowerScheduler as RemoteStandbyHandler / RemoteWakeupHandler with tag {0}", newTag);
          _remotingTag = newTag;
        }
      }
      catch (Exception ex)
      {
        Log.Info("PS: Cannot register client's PowerScheduler as RemoteStandbyHandler / RemoteWakeupHandler: {0}", ex.Message);
        // Should we also clear the connection?
        //RemotePowerControl.Clear();
      }
    }

    private void UnregisterFromRemotePowerScheduler()
    {
      if (_remotingTag != 0)
      {
        Log.Debug("PS: Unregister handlers with tvservice with tag {0}", _remotingTag);
        if (RemotePowerControl.Instance != null)
        {
          RemotePowerControl.Instance.UnregisterRemote(_remotingTag);
        }
        _remotingTag = 0;
      }
      RemotePowerControl.Clear();
    }

    /// <summary>
    /// Called on entering away mode
    /// </summary>
    private void OnAwayMode()
    {
      Log.Debug("PS: System is entering away mode");

      // Stop player
      StopPlayer();

      // Run external command
      Log.Info("PS: Run external command");
      RunExternalCommand("Command", "awaymode");
    }

    /// <summary>
    /// Called on leaving away mode
    /// </summary>
    private void OnRunMode()
    {
      Log.Debug("PS: System is leaving away mode");

      // Run external command
      Log.Info("PS: Run external command");
      RunExternalCommand("Command", "runmode");
    }

    /// OnAction handler; if any action is received then last busy time is reset (i.e. idletimeout is reset)
    /// </summary>
    /// <param name="action">action message sent by the system</param>
    private void OnAction(MediaPortal.GUI.Library.Action action)
    {
      if (action.IsUserAction())
      {
        // Log.Debug("PS: Action {0} detected - reset time of last user activity and system idle timer", action.wID);
        _lastUserTime = DateTime.Now;
        PowerManager.ResetIdleTimer();
      }
    }

    /// <summary>
    /// Stops player
    /// </summary>
    private void StopPlayer()
    {
      Log.Debug("PS: StopPlayer()");
      if (g_Player.Playing || g_Player.IsTimeShifting)
      {
        Log.Debug("PS: Player is playing, kick off stop player thread");
        GUIWindowManager.SendThreadCallbackAndWait(StopPlayerCallback, 0, 0, null);
      }
    }

    /// <summary>
    /// Stop player callback
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    private int StopPlayerCallback(int p1, int p2, object d)
    {
      Log.Debug("PS: StopPlayerCallback()");
      if (g_Player.Playing || g_Player.IsTimeShifting)
      {
        Log.Debug("PS: StopPlayerCallback - stopping player");
        while (true)
        {
          g_Player.Stop();
          if (g_Player.Playing || g_Player.IsTimeShifting)
          {
            if (!GUIWindowManager.HasPreviousWindow())
            {
              break;
            }
            Log.Debug("PS: StopPlayerCallback - player is still playing, activating previous window");
            GUIWindowManager.ShowPreviousWindow();
          }
          else
          {
            break;
          }
        }
        if (g_Player.Playing || g_Player.IsTimeShifting)
        {
          // could not find any previous window that allows to stop the player, we go home
          Log.Debug("PS: StopPlayerCallback - player is still playing, activating home window");
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
          g_Player.Stop();
        }
        Log.Debug("PS: StopPlayerCallback - stopped player: {0}", !g_Player.Playing);
      }

      // go to home screen if PS allows only homescreen-standby
      if (_settings.GetSetting("HomeOnly").Get<bool>())
      {
        bool basicHome;
        using (Settings xmlreader = new MPSettings())
        {
          basicHome = xmlreader.GetValueAsBool("gui", "startbasichome", true);
        }

        int homeWindow = basicHome ? (int)GUIWindow.Window.WINDOW_SECOND_HOME : (int)GUIWindow.Window.WINDOW_HOME;
        int activeWindow = GUIWindowManager.ActiveWindow;
        if (activeWindow != homeWindow && activeWindow != (int)GUIWindow.Window.WINDOW_PSCLIENTPLUGIN_UNATTENDED)
        {
          Log.Debug("PS: StopPlayerCallback - going to home screen");
          GUIWindowManager.ActivateWindow(homeWindow);
        }
      }
      return 0;
    }

    /// <summary>
    /// The mouse_event function synthesizes mouse motion and button clicks
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

    /// <summary>
    /// dwFlags constants
    /// </summary>
    private const int MOUSEEVENTF_MOVE = 0x0001;      // Movement occurred
    private const int MOUSEEVENTF_ABSOLUTE = 0x8000;  // The dx and dy parameters contain normalized absolute coordinates

    /// <summary>
    /// Checks if the global player is playing or slideshow is active
    /// <returns>User interface is idle</returns>
    /// </summary>
    private bool UserInterfaceIdle
    {
      get
      {
        // See if media is playing
        if (g_Player.Playing || g_Player.IsTimeShifting)
        {
          Log.Debug("PS: User interface is not idle: Media is playing - reset time of last user activity and system idle timer");
          _currentStandbyHandler = "Media playing";
          _lastUserTime = DateTime.Now;
          PowerManager.ResetIdleTimer();
          return false;
        }

        // See if slideshow is active
        int activeWindow = GUIWindowManager.ActiveWindow;
        if (activeWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
        {
          Log.Debug("PS: User interface is not idle: Slideshow is active - reset time of last user activity and system idle timer");
          _currentStandbyHandler = "Slideshow active";
          _lastUserTime = DateTime.Now;
          PowerManager.ResetIdleTimer();
          return false;
        }

        // See if home window is active (if configured)
        if (_settings.GetSetting("HomeOnly").Get<bool>())
        {
          switch (activeWindow)
          {
            case (int)GUIWindow.Window.WINDOW_HOME:
            case (int)GUIWindow.Window.WINDOW_SECOND_HOME:
            case (int)GUIWindow.Window.WINDOW_PSCLIENTPLUGIN_UNATTENDED:
              Log.Debug("PS: User interface is idle: On home window");
              return true;

            default:
              Log.Debug("PS: User interface is not idle: Not on home window");
              _currentStandbyHandler = "Not on home window";
              return false;
          }
        }

        Log.Debug("PS: User interface is idle");
        return true;
      }
    }

    /// <summary>
    /// Struct for GetLastInpoutInfo
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
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    /// <summary>
    /// Returns the current tick as uint (pref. over Environemt.TickCount which only uses int)
    /// </summary>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    private static extern uint GetTickCount();

    /// <summary>
    /// This functions returns the time of the last user input recogniized,
    /// i.e. mouse moves or keyboard inputs.
    /// </summary>
    /// <returns>Last time of user input</returns>
    private DateTime GetLastInputTime()
    {
      LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
      lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

      if (!GetLastInputInfo(ref lastInputInfo))
      {
        Log.Debug("PS: Unable to GetLastInputInfo!");
        return DateTime.MinValue;
      }

      long lastKick = lastInputInfo.dwTime;
      long tick = GetTickCount();

      long delta = lastKick - tick;

      if (delta > 0)
      {
        // There was an overflow (restart at 0) in the tick-counter!
        delta = delta - uint.MaxValue - 1;
      }

      return DateTime.Now.AddMilliseconds(delta);
    }

    #endregion
  }
}