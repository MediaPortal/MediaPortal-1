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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Threading;
using Gentle.Common;
using TvControl;
using TvDatabase;
using TvEngine.Interfaces;
using TvEngine.PowerScheduler.Handlers;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvEngine.PowerScheduler
{
  /// <summary>
  /// PowerScheduler Server Plugin: tvservice plugin which controls power management
  /// </summary>
  public class PowerScheduler : MarshalByRefObject, IPowerScheduler, IPowerController
  {
    #region Variables

    /// <summary>
    /// PowerScheduler single instance
    /// </summary>
    private static PowerScheduler _powerScheduler;

    /// <summary>
    /// mutex lock object to ensure only one instance of the PowerScheduler object
    /// is created.
    /// </summary>
    private static readonly object _mutex = new object();

    /// <summary>
    /// Reference to tvservice's TVController
    /// </summary>
    private IController _controller;

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
    /// StandbyHandler for the IPowerController interface
    /// </summary>
    private PowerControllerStandbyHandler _powerControllerStandbyHandler;

    /// <summary>
    /// WakeupHandler for the IPowerController interface
    /// </summary>
    private PowerControllerWakeupHandler _powerControllerWakeupHandler;

    /// <summary>
    /// IStandbyHandler for remote clients in client/server setups
    /// </summary>
    private RemoteClientStandbyHandler _remoteClientStandbyHandler;

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
    /// Indicator if remoting has been setup
    /// </summary>
    private bool _remotingStarted = false;

    /// <summary>
    /// Indicator if the TVController should be reinitialized
    /// (or if this has already been done)
    /// </summary>
    private bool _reinitializeController = false;

    /// <summary>
    /// Indicator if the cards have been stopped
    /// </summary>
    private bool _cardsStopped = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new PowerScheduler plugin and performs the one-time initialization
    /// </summary>
    private PowerScheduler()
    {
      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();
      _lastUserTime = DateTime.Now;
      _idle = false;

      // Register as global service provider instance
      if (!GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
      {
        GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
      }
      Log.Debug("PS: Registered PowerScheduler as IPowerScheduler service to GlobalServiceProvider");
    }

    ~PowerScheduler()
    {
      try
      {
        // Disable the wakeup timer, if not done already
        if (_wakeupTimer != null)
        {
          _wakeupTimer.TimeToWakeup = DateTime.MaxValue;
          _wakeupTimer.Close();
          _wakeupTimer = null;
        }

        // Unregister as global service provider instance
        if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        {
          GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
          Log.Debug("PS: Unregistered IPowerScheduler service from GlobalServiceProvider");
        }
      }
      catch (Exception ex)
      {
        Log.Error("PS: Exception in Destructor: {0}", ex);
        Log.Info("PS: Exception in Destructor: {0}", ex);
      }
    }

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
          Log.Info("PS: Unknown shutdown mode: {0}", _settings.ShutdownMode);
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
      UserShutdownNow();

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

      // Stop PowerScheduler plugin before shutdown or reboot
      if ((how == RestartOptions.PowerOff) || (how == RestartOptions.Reboot))
        Stop();

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
    /// User activity was detected on the local client or a remote client is active
    /// </summary>
    /// <param name="when">Time of last user activity (local client) or DateTime.MinValue (remote client)</param>
    public void UserActivityDetected(DateTime when)
    {
      if (when == DateTime.MinValue)
      {
        Log.Debug("PS: Remote client activity detected");
        _remoteClientStandbyHandler.DisAllowShutdown = true;
        return;
      }

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

    #region IStandbyHandler(Ex)/IwakeupHandler methods

    /// <summary>
    /// Indicator which standby mode is requested by the handler
    /// </summary>
    private StandbyMode StandbyMode
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
            Log.Debug("PS: Inspecting {0}: {1}", handler.HandlerName, handlerStandbyMode.ToString());
            if (standbyMode != StandbyMode.AwayModeRequested)
              standbyMode = handlerStandbyMode;
            if (standbyHandler == "")
              standbyHandler = handler.HandlerName;
            else
              standbyHandler += ", " + handler.HandlerName;
          }
        }
        if (standbyMode != StandbyMode.StandbyAllowed)
        {
          _currentStandbyHandler = standbyHandler;
          _currentStandbyMode = standbyMode;
          return standbyMode;
        }

        // Then check whether the next event is almost due (within pre-no-standby time)
        if (DateTime.Now >= _currentNextWakeupTime.AddSeconds(-_settings.PreNoShutdownTime))
        {
          Log.Debug("PS: Event is almost due ({0}): StandbyPrevented", _currentNextWakeupHandler);
          _currentStandbyHandler = "Event due";
          _currentStandbyMode = StandbyMode.StandbyPrevented;
          return StandbyMode.StandbyPrevented;
        }

        // Then check if standby is allowed at this moment
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

        // Nothing prevents standby
        _currentStandbyHandler = "";
        _currentStandbyMode = StandbyMode.StandbyAllowed;
        return StandbyMode.StandbyAllowed;
      }
    }

    /// <summary>
    /// Indicator whether to prevent suspension/hibernation of the system
    /// </summary>
    private bool DisAllowShutdown
    {
      get
      {
        return (StandbyMode != StandbyMode.StandbyAllowed);
      }
    }

    /// <summary>
    /// Description of the source that allows / disallows shutdown
    /// </summary>
    private string HandlerName
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
    private void UserShutdownNow()
    {
      // Trigger the handlers
      foreach (IStandbyHandler handler in _standbyHandlers)
      {
        handler.UserShutdownNow();
      }
    }

    /// <summary>
    /// Retrieves the earliest wakeup time from all IWakeupHandlers
    /// </summary>
    /// <param name="earliestWakeupTime">indicates the earliest valid wake up time that is considered valid by the PowerScheduler</param>
    /// <returns>the next wakeup time</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      string handlerName = String.Empty;
      DateTime nextWakeupTime = DateTime.MaxValue;

      // Inspect all registered IWakeupHandlers
      foreach (IWakeupHandler handler in _wakeupHandlers)
      {
        DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
        if (nextTime < earliestWakeupTime)
          nextTime = DateTime.MaxValue;
        if (nextTime < DateTime.MaxValue)
          Log.Debug("PS: Inspecting {0}: {1}", handler.HandlerName, nextTime.ToString());

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

    #endregion

    #region IPowerController implementation

    /// <summary>
    /// Enables clients on singleseat setups to indicate whether or not the system
    /// is allowed to enter standby
    /// </summary>
    /// <param name="standbyAllowed">is standby allowed?</param>
    /// <param name="handlerName">client handlername which prevents standby</param>
    public void SetStandbyAllowed(bool standbyAllowed, string handlerName)
    {
      Log.Debug("PS: SetStandbyAllowed: {0}, {1}", standbyAllowed, handlerName);
      _powerControllerStandbyHandler.DisAllowShutdown = !standbyAllowed;
      _powerControllerStandbyHandler.HandlerName = "PowerController (" + handlerName + ")";
    }

    /// <summary>
    /// Enables clients on singleseat setups to indicate when the next
    /// earliest wakeup time is due
    /// </summary>
    /// <param name="nextWakeupTime">DateTime when to wakeup the system</param>
    /// <param name="handlerName">client handlername which is responsible for this wakeup time</param>
    public void SetNextWakeupTime(DateTime nextWakeupTime, string handlerName)
    {
      Log.Debug("PS: SetNextWakeupTime: {0:G}, {1}", nextWakeupTime, handlerName);
      _powerControllerWakeupHandler.Update(nextWakeupTime, handlerName);
    }

    private int _remoteTags = 0;
    private Hashtable _localClientStandbyHandlers = new Hashtable();
    private Hashtable _localClientWakeupHandlers = new Hashtable();
    private Dictionary<string, int> _localClientStandbyHandlerURIs = new Dictionary<string, int>();
    private Dictionary<string, int> _localClientWakeupHandlerURIs = new Dictionary<string, int>();

    /// <summary>
    /// Register remote handlers. If an empty string or null is passed, no handler is registered for that type.
    /// </summary>
    /// <param name="standbyHandlerURI"></param>
    /// <param name="wakeupHandlerURI"></param>
    /// <returns>Returns a tag used to unregister the handler(s)</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public int RegisterRemote(string standbyHandlerURI, string wakeupHandlerURI)
    {
      int oldStandbyTag = 0;
      int oldWakeupTag = 0;
      bool registeredHandler = false;

      // Find existing tags
      if (!string.IsNullOrEmpty(standbyHandlerURI) &&
          !_localClientStandbyHandlerURIs.TryGetValue(standbyHandlerURI, out oldStandbyTag))
      {
        oldStandbyTag = _remoteTags + 1;
      }
      if (!string.IsNullOrEmpty(wakeupHandlerURI) &&
          !_localClientWakeupHandlerURIs.TryGetValue(wakeupHandlerURI, out oldWakeupTag))
      {
        oldWakeupTag = _remoteTags + 1;
      }

      if (oldStandbyTag == 0 && oldWakeupTag == 0)
        return 0; // No URIs supplied!

      // Determine new registration tag
      int newTag = 0;
      if (oldStandbyTag == oldWakeupTag && oldStandbyTag != 0 && oldStandbyTag <= _remoteTags)
      {
        newTag = oldStandbyTag;
      }
      else if (oldStandbyTag == 0)
      {
        newTag = oldWakeupTag;
      }
      else if (oldWakeupTag == 0)
      {
        newTag = oldStandbyTag;
      }
      else
      {
        newTag = _remoteTags + 1;
      }

      // Register handlers
      Log.Debug("PS: RegisterRemote tag: {0}, uris: {1}, {2}", newTag, standbyHandlerURI, wakeupHandlerURI);
      if (standbyHandlerURI != null && standbyHandlerURI.Length > 0)
      {
        LocalClientStandbyHandler hdl;
        if (newTag <= _remoteTags)
        {
          hdl = (LocalClientStandbyHandler)_localClientStandbyHandlers[oldStandbyTag];
          _localClientStandbyHandlers.Remove(oldStandbyTag);
        }
        else
        {
          hdl = new LocalClientStandbyHandler(standbyHandlerURI, newTag);
          Register(hdl);
          registeredHandler = true;
        }

        _localClientStandbyHandlers[newTag] = hdl;
        _localClientStandbyHandlerURIs[standbyHandlerURI] = newTag;
      }
      if (wakeupHandlerURI != null && wakeupHandlerURI.Length > 0)
      {
        LocalClientWakeupHandler hdl;
        if (newTag <= _remoteTags)
        {
          hdl = (LocalClientWakeupHandler)_localClientWakeupHandlers[oldWakeupTag];
          _localClientWakeupHandlers.Remove(oldWakeupTag);
        }
        else
        {
          hdl = new LocalClientWakeupHandler(wakeupHandlerURI, newTag);
          Register(hdl);
          registeredHandler = true;
        }
        _localClientWakeupHandlers[newTag] = hdl;
        _localClientWakeupHandlerURIs[wakeupHandlerURI] = newTag;
      }
      if (newTag > _remoteTags)
        _remoteTags = newTag;

      //Unregister old handlers
      if (oldStandbyTag != 0 && oldStandbyTag != newTag)
      {
        UnregisterRemote(oldStandbyTag);
      }
      if (oldWakeupTag != 0 && oldWakeupTag != newTag)
      {
        UnregisterRemote(oldWakeupTag);
      }

      // Trigger StandbWakeupThread
      if (registeredHandler)
      {
        _standbyWakeupTriggered.Set();
        Log.Debug("PS: RegisterRemote - Triggered StandbyWakeupThread");
      }

      return newTag;
    }

    /// <summary>
    /// Unregister remote handlers.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void UnregisterRemote(int tag)
    {
      {
        LocalClientStandbyHandler hdl = (LocalClientStandbyHandler)_localClientStandbyHandlers[tag];
        if (hdl != null)
        {
          _localClientStandbyHandlers.Remove(tag);
          _localClientStandbyHandlerURIs.Remove(hdl._url);
          hdl.Close();
          Log.Debug("PS: UnregisterRemote StandbyHandler {0}", tag);
          Unregister(hdl);
        }
      }
      {
        LocalClientWakeupHandler hdl = (LocalClientWakeupHandler)_localClientWakeupHandlers[tag];
        if (hdl != null)
        {
          _localClientWakeupHandlers.Remove(tag);
          _localClientWakeupHandlerURIs.Remove(hdl._url);
          hdl.Close();
          Log.Debug("PS: UnregisterRemote WakeupHandler {0}", tag);
          Unregister(hdl);
        }
      }
    }

    /// <summary>
    /// Indicates whether or not the client is connected to the server (or not)
    /// </summary>
    public bool IsConnected
    {
      get { return true; }
    }

    /// <summary>
    /// Provides access to PowerScheduler's settings
    /// </summary>
    public IPowerSettings PowerSettings
    {
      get { return _settings; }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Start the PowerScheduler plugin
    /// </summary>
    /// <param name="controller">TVController from the tvservice</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Start(IController controller)
    {
      Log.Info("PS: Starting PowerScheduler server plugin...");

      try
      {
        // Save controller
        _controller = controller;

        // Create the timer that will wakeup the system after a specific amount of time after the
        // system has been put into standby
        _wakeupTimer = new WaitableTimer();

        // Load settings
        LoadSettings();

        // Register standby/wakeup handlers
        _powerControllerStandbyHandler = new PowerControllerStandbyHandler();
        Register(_powerControllerStandbyHandler);
        _powerControllerWakeupHandler = new PowerControllerWakeupHandler();
        Register(_powerControllerWakeupHandler);
        _remoteClientStandbyHandler = new RemoteClientStandbyHandler();
        Register(_remoteClientStandbyHandler);
        _factory = new PowerSchedulerFactory(controller);
        _factory.CreateDefaultSet();
        Log.Debug("PS: Registered standby/wakeup handlers to PowerScheduler server plugin");

        // Register power event handler to TVServer
        RegisterPowerEventHandler();

        // Register PowerScheduler as IPowerControl remoting service
        StartRemoting();

        // Create the EventWaitHandles for the StandbyWakeupThread and GetCurrentState
        _standbyWakeupTriggered = new EventWaitHandle(false, EventResetMode.AutoReset, "TvEngine.PowerScheduler.StandbyWakeupTriggered");
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
        Log.Info("PS: PowerScheduler server plugin started");
      }
      catch (Exception ex)
      {
        Log.Error("PS: Exception in Start: {0}", ex);
        Log.Info("PS: Exception in Start: {0}", ex);
        Stop();
      }
    }

    /// <summary>
    /// Stop the PowerScheduler plugin
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Stop()
    {
      Log.Info("PS: Stopping PowerScheduler server plugin...");

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

        // Unregister power event handler
        UnRegisterPowerEventHandler();

        // Remove the standby/resume handlers
        if (_factory != null)
        {
          _factory.RemoveDefaultSet();
          _factory = null;
        }
        Unregister(_remoteClientStandbyHandler);
        Unregister(_powerControllerWakeupHandler);
        Unregister(_powerControllerStandbyHandler);
        Log.Debug("PS: Removed standby/wakeup handlers");

        // Disable the wakeup timer
        if (_wakeupTimer != null)
        {
          _wakeupTimer.TimeToWakeup = DateTime.MaxValue;
          _wakeupTimer.Close();
          _wakeupTimer = null;
        }

        SendPowerSchedulerEvent(PowerSchedulerEventType.Stopped);
        Log.Info("PS: PowerScheduler server plugin stopped");
      }
      catch (Exception ex)
      {
        Log.Error("PS: Exception in Stop: {0}", ex);
        Log.Info("PS: Exception in Stop: {0}", ex);
      }
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
        // Load settings and trigger registered handlers
        CacheManager.Clear();
        GC.Collect();
        LoadSettings();
        SendPowerSchedulerEvent(PowerSchedulerEventType.Elapsed);

        // Check handlers for next wakeup time and standby allowed
        SetWakeupTimer();
        CheckForStandby();

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

      TvBusinessLayer layer = new TvBusinessLayer();

      // Load initial settings only once
      if (_settings == null)
      {
        // Check if update of old PS settings is necessary
        Setting setting = layer.GetSetting("PowerSchedulerExpertMode");
        if (setting.Value == "")
        {
          setting.Remove();

          // Initialise list of old and new settings to update
          List<String[]> settingNames = new List<String[]>();
          settingNames.Add(new String[] { "PreventStandybyWhenSpecificSharesInUse", "PowerSchedulerActiveShares" });
          settingNames.Add(new String[] { "PreventStandybyWhenSharesInUse", "PowerSchedulerActiveSharesEnabled" });
          settingNames.Add(new String[] { "PowerSchedulerEpgCommand", "PowerSchedulerEPGCommand" });
          settingNames.Add(new String[] { "PreventStandbyWhenGrabbingEPG", "PowerSchedulerEPGPreventStandby" });
          settingNames.Add(new String[] { "WakeupSystemForEPGGrabbing", "PowerSchedulerEPGWakeup" });
          settingNames.Add(new String[] { "EPGWakeupConfig", "PowerSchedulerEPGWakeupConfig" });
          settingNames.Add(new String[] { "NetworkMonitorEnabled", "PowerSchedulerNetworkMonitorEnabled" });
          settingNames.Add(new String[] { "NetworkMonitorIdleLimit", "PowerSchedulerNetworkMonitorIdleLimit" });
          settingNames.Add(new String[] { "PowerSchedulerPreNoShutdownTime", "PowerSchedulerPreNoStandbyTime" });
          settingNames.Add(new String[] { "PowerSchedulerShutdownActive", "PowerSchedulerShutdownEnabled" });
          settingNames.Add(new String[] { "PowerSchedulerStandbyAllowedStart", "PowerSchedulerStandbyHoursFrom" });
          settingNames.Add(new String[] { "PowerSchedulerStandbyAllowedEnd", "PowerSchedulerStandbyHoursTo" });

          // Update settings names
          foreach (String[] settingName in settingNames)
          {
            setting = layer.GetSetting(settingName[0], "---");
            if (setting.Value != "---")
            {
              setting.Tag = settingName[1];
              setting.Persist();
            }
            else
            {
              setting.Remove();
            }
          }
        }

        _settings = new PowerSettings();
        changed = true;

        // Set constant values (needed for backward compatibility)
        _settings.ForceShutdown = false;
        _settings.ExtensiveLogging = false;
        _settings.CheckInterval = 15;
      }

      // Check if PowerScheduler should actively put the system into standby
      boolSetting = Convert.ToBoolean(layer.GetSetting("PowerSchedulerShutdownEnabled", "false").Value);
      if (_settings.ShutdownEnabled != boolSetting)
      {
        _settings.ShutdownEnabled = boolSetting;
        Log.Debug("PS: PowerScheduler forces system to go to standby when idle: {0}", boolSetting);
        changed = true;
      }

      if (_settings.ShutdownEnabled)
      {
        // Check configured shutdown mode
        intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerShutdownMode", "0").Value);
        if ((int)_settings.ShutdownMode != intSetting)
        {
          _settings.ShutdownMode = (ShutdownMode)intSetting;
          Log.Debug("PS: Shutdown mode: {0}", _settings.ShutdownMode.ToString());
          changed = true;
        }
      }

      // Get idle timeout
      if (_settings.ShutdownEnabled)
        intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerIdleTimeout", "30").Value);
      else
        intSetting = (int)PowerManager.GetActivePowerSetting(PowerManager.SystemPowerSettingType.STANDBYIDLE) / 60;
      if (_settings.IdleTimeout != intSetting)
      {
        _settings.IdleTimeout = intSetting;
        Log.Debug("PS: {0}: {1} minutes", (_settings.ShutdownEnabled ? "Standby after" : "System idle timeout"), intSetting);
        changed = true;
      }

      // Check configured pre-wakeup time
      intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerPreWakeupTime", "60").Value);
      if (_settings.PreWakeupTime != intSetting)
      {
        _settings.PreWakeupTime = intSetting;
        Log.Debug("PS: Pre-wakeup time: {0} seconds", intSetting);
        changed = true;
      }

      // Check configured pre-no-standby time
      intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerPreNoStandbyTime", "300").Value);
      if (_settings.PreNoShutdownTime != intSetting)
      {
        _settings.PreNoShutdownTime = intSetting;
        Log.Debug("PS: Pre-no-standby time: {0} seconds", intSetting);
        changed = true;
      }

      // Check allowed start time
      intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerStandbyHoursFrom", "0").Value);
      if (_settings.AllowedSleepStartTime != intSetting)
      {
        _settings.AllowedSleepStartTime = intSetting;
        Log.Debug("PS: Standby allowed from {0} o' clock", _settings.AllowedSleepStartTime);
        changed = true;
      }

      // Check allowed stop time
      intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerStandbyHoursTo", "24").Value);
      if (_settings.AllowedSleepStopTime != intSetting)
      {
        _settings.AllowedSleepStopTime = intSetting;
        Log.Debug("PS: Standby allowed until {0} o' clock", _settings.AllowedSleepStopTime);
        changed = true;
      }

      // Check allowed start time on weekend
      intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerStandbyHoursOnWeekendFrom", "0").Value);
      if (_settings.AllowedSleepStartTimeOnWeekend != intSetting)
      {
        _settings.AllowedSleepStartTimeOnWeekend = intSetting;
        Log.Debug("PS: Standby allowed from {0} o' clock on weekend", _settings.AllowedSleepStartTimeOnWeekend);
        changed = true;
      }

      // Check allowed stop time on weekend
      intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerStandbyHoursOnWeekendTo", "24").Value);
      if (_settings.AllowedSleepStopTimeOnWeekend != intSetting)
      {
        _settings.AllowedSleepStopTimeOnWeekend = intSetting;
        Log.Debug("PS: Standby allowed until {0} o' clock on weekend", _settings.AllowedSleepStopTimeOnWeekend);
        changed = true;
      }

      // Check if PowerScheduler should wakeup the system automatically
      intSetting = Int32.Parse(layer.GetSetting("PowerSchedulerProfile", "0").Value);
      if (intSetting == 2)
        boolSetting = false;  // Notebook
      else
        boolSetting = true;   // HTPC, Desktop, Server
      if (_settings.WakeupEnabled != boolSetting)
      {
        _settings.WakeupEnabled = boolSetting;
        Log.Debug("PS: Wakeup system for varios events: {0}", boolSetting);
        changed = true;
      }

      // Check if PowerScheduler should reinitialize the TVService after wakeup
      boolSetting = Convert.ToBoolean(layer.GetSetting("PowerSchedulerReinitializeController", "false").Value);
      if (_settings.ReinitializeController != boolSetting)
      {
        _settings.ReinitializeController = boolSetting;
        Log.Debug("PS: Reinitialize TVService on wakeup: {0}", boolSetting);
        changed = true;
      }

      // Get external command
      powerSetting = _settings.GetSetting("Command");
      stringSetting = layer.GetSetting("PowerSchedulerCommand", String.Empty).Value;
      if (!stringSetting.Equals(powerSetting.Get<string>()))
      {
        powerSetting.Set<string>(stringSetting);
        Log.Debug("PS: Run command on power state change: {0}", stringSetting);
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
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void CheckForStandby()
    {
      Log.Debug("PS: CheckForStandby()");

      // Check handlers for allowing standby
      StandbyMode standbyMode = StandbyMode;

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
        {
          Log.Debug("PS: Active standby is disabled - standby is handled by Windows");
          string requests = PowerManager.GetPowerCfgRequests(true);
          if (!string.IsNullOrEmpty(requests))
            Log.Debug("PS: Requests preventing Windows standby: " + requests.Replace(Environment.NewLine, ", "));
        }
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

      Log.Debug("PS: Stop EPG grabbing and TV controller");
      _controller.EpgGrabberEnabled = false;
      DeInitController();

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

      // Reinitialize TVController if system is configured to do so and not already done
      Log.Info("PS: ReInitController");
      ReInitController();
      if (!_controller.EpgGrabberEnabled)
        _controller.EpgGrabberEnabled = true;

      // Resume the StandbyWakeupThread
      Log.Debug("PS: Signal \"Resume\" to the StandbyWakeupThread");
      _standbyWakeupResume.Set();

      Log.Debug("PS: Send \"ResumedFromStandby\" event");
      _standby = false;
      SendPowerSchedulerEvent(PowerSchedulerEventType.ResumedFromStandby);
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
        DateTime nextWakeup = GetNextWakeupTime(DateTime.Now);
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
          Log.Info("PS: Exception in RunExternalCommand: {0}", ex);
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

    private void RegisterPowerEventHandler()
    {
      // Register to power events generated by the system
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().AddPowerEventHandler(new PowerEventHandler(OnPowerEvent));
        Log.Debug("PS: Registered PowerScheduler as IPowerEventHandler service to GlobalServiceProvider");
      }
      else
      {
        Log.Info("PS: Unable to register PowerScheduler as IPowerEventHandler service to GlobalServiceProvider");
      }
    }

    private void UnRegisterPowerEventHandler()
    {
      // Unregister to power events generated by the system
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().RemovePowerEventHandler(
          new PowerEventHandler(OnPowerEvent));
        Log.Debug("PS: UnRegistered IPowerEventHandler from GlobalServiceProvider");
      }
      else
      {
        Log.Info("PS: Unable to unregister IPowerEventHandler from GlobalServiceProvider");
      }
    }

    /// <summary>
    /// Windows PowerEvent handler
    /// </summary>
    /// <param name="powerStatus">PowerBroadcastStatus the system is changing to</param>
    /// <returns>bool indicating if the broadcast was honoured</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private bool OnPowerEvent(PowerEventType powerStatus)
    {
      switch (powerStatus)
      {
        // This event is triggered only on Windows XP
        case PowerEventType.QuerySuspend:
          Log.Debug("PS: QUERYSUSPEND");
          if (_currentStandbyMode == StandbyMode.AwayModeRequested)
          {
            // We reject all requests for suspend when a suspend should not be allowed
            Log.Debug("PS: Suspend queried while away mode is required - deny suspend");
            return false;
          }
          if (_denyQuerySuspend)
          {
            // We reject all requests for suspend not coming from PowerScheduler by returning false.
            // Instead we start our own shutdown thread that issues a new QUERYSUSPEND that we will accept.
            // Always try to Hibernate (S4). If system is set to S3, then Hibernate will fail and result will be S3
            Log.Debug("PS: Suspend queried by another application - deny suspend and start own suspend sequence");
            SuspendSystem("System", (int)RestartOptions.Hibernate, false);
            return false;
          }
          return true;

        case PowerEventType.Suspend:
          Log.Debug("PS: SUSPEND");
          OnSuspend();
          return true;

        // This event is triggered only on Windows XP
        case PowerEventType.QuerySuspendFailed:
          Log.Debug("PS: QUERYSUSPENDFAILED");
          // Another application prevents our suspend
          _QuerySuspendFailedCount--;
          return true;

        case PowerEventType.ResumeAutomatic:
          Log.Debug("PS: RESUMEAUTOMATIC");
          OnResume();
          return true;

        case PowerEventType.ResumeCritical:
          Log.Debug("PS: RESUMECRITICAL");
          OnResume();
          return true;

        case PowerEventType.ResumeSuspend:
          // Note: This event is triggered when the user has moved the mouse or hit a key
          // ResumeAutomatic or ResumeCritical have triggered before, so no need to call OnResume()
          Log.Debug("PS: RESUMESUSPEND");
          _lastUserTime = DateTime.Now;
          return true;
      }
      return true;
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
        ListDictionary channelProperties = new ListDictionary();
        channelProperties.Add("port", 31457);
        channelProperties.Add("exclusiveAddressUse", false);
        HttpChannel channel = new HttpChannel(channelProperties,
                                              new SoapClientFormatterSinkProvider(),
                                              new SoapServerFormatterSinkProvider());
        ChannelServices.RegisterChannel(channel, false);
      }
      catch (RemotingException) { }
      catch (System.Net.Sockets.SocketException) { }
      // RemotingConfiguration.RegisterWellKnownServiceType(typeof(PowerScheduler), "PowerControl", WellKnownObjectMode.Singleton);
      ObjRef objref = RemotingServices.Marshal(this, "PowerControl", typeof(IPowerController));
      RemotePowerControl.Clear();
      Log.Debug("PS: Registered PowerScheduler as IPowerControl remoting service");
      _remotingStarted = true;
    }

    /// <summary>
    /// Frees the tv tuners before entering standby
    /// </summary>
    private void DeInitController()
    {
      if (_cardsStopped)
        return;
      // Only free tuner cards if reinitialization is enabled in settings
      if (!_settings.ReinitializeController)
        return;

      TvService.TVController controller = _controller as TvService.TVController;
      if (controller != null)
      {
        Log.Debug("PS: DeInit controller");
        controller.DeInit();
        _cardsStopped = true;
        _reinitializeController = true;
      }
    }

    /// <summary>
    /// Restarts the TVController when resumed from standby
    /// </summary>
    private void ReInitController()
    {
      if (!_reinitializeController)
        return;
      // Only reinitialize controller if enabled in settings
      if (!_settings.ReinitializeController)
        return;

      TvService.TVController controller = _controller as TvService.TVController;
      if (controller != null && _reinitializeController)
      {
        Log.Debug("PS: ReInit Controller");
        Thread.Sleep(5000); // Give it a few seconds.
        controller.Init();
        _reinitializeController = false;
        _cardsStopped = false;
      }
    }

    #endregion
  }
}