#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using TvControl;
using TvDatabase;
using TvEngine.Interfaces;
using TvEngine.PowerScheduler.Handlers;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using System.Runtime.InteropServices;
using System.Threading;

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
    /// Factory for creating various IStandbyHandlers/IWakeupHandlers
    /// </summary>
    private PowerSchedulerFactory _factory;

    /// <summary>
    /// Manages setting the according thread execution state
    /// </summary>
    private PowerManager _powerManager;

    /// <summary>
    /// List of registered standby handlers ("disable standby" plugins)
    /// </summary>
    private List<IStandbyHandler> _standbyHandlers;

    /// <summary>
    /// List of registered wakeup handlers ("enable wakeup" plugins)
    /// </summary>
    private List<IWakeupHandler> _wakeupHandlers;

    /// <summary>
    /// IStandbyHandler for the client in singleseat setups
    /// </summary>
    private GenericStandbyHandler _clientStandbyHandler;

    /// <summary>
    /// IWakeupHandler for the client in singleseat setups
    /// </summary>
    private GenericWakeupHandler _clientWakeupHandler;

    /// <summary>
    /// Timer for executing periodic checks (should we enter standby..)
    /// </summary>
    private System.Timers.Timer _timer;

    /// <summary>
    /// Timer with support for waking up the system
    /// </summary>
    private WaitableTimer _wakeupTimer;

    /// <summary>
    /// Last time any activity by the user was detected.
    /// </summary>
    private DateTime _lastUserTime;

    /// <summary>
    /// Global indicator if the PowerScheduler thinks the system is idle
    /// </summary>
    private bool _idle = false;

    /// <summary>
    /// Indicating whether the PowerScheduler is in standby-mode.
    /// </summary>
    private bool _standby = false;

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
      _clientStandbyHandler = new GenericStandbyHandler();
      _clientWakeupHandler = new GenericWakeupHandler();
      _lastUserTime = DateTime.Now;
      _idle = false;

      // Add ourselves to the GlobalServiceProvider
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
      {
        GlobalServiceProvider.Instance.Remove<IPowerScheduler>();
      }
      GlobalServiceProvider.Instance.Add<IPowerScheduler>(this);
      Log.Debug("PowerScheduler: Registered PowerScheduler service to GlobalServiceProvider");
    }

    ~PowerScheduler()
    {
      try
      {
        // disable the wakeup timer
        _wakeupTimer.SecondsToWait = -1;
        _wakeupTimer.Close();
      }
      catch (ObjectDisposedException) {}
      catch (AppDomainUnloadedException appex)
      {
        Log.Info("PowerScheduler: Error on dispose - {0}", appex.Message);
      }
    }

    #endregion

    #region Public methods

    #region Start/Stop methods

    /// <summary>
    /// Called by the PowerSchedulerPlugin to start the PowerScheduler
    /// </summary>
    /// <param name="controller">TVController from the tvservice</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Start(IController controller)
    {
      try
      {
        string threadname = Thread.CurrentThread.Name;
        if (string.IsNullOrEmpty(threadname))
          Thread.CurrentThread.Name = "Powerscheduler";
      }
      catch (Exception ex)
      {
        Log.Error("Powerscheduler: Error naming thread - {0}", ex.Message);
      }

      _controller = controller;

      Register(_clientStandbyHandler);
      Register(_clientWakeupHandler);
      RegisterPowerEventHandler();

      Log.Debug("PowerScheduler: Registered default set of standby/resume handlers to PowerScheduler");

      // Create the PowerManager that helps setting the correct thread executation state
      _powerManager = new PowerManager();

      // Create the timer that will wakeup the system after a specific amount of time after the
      // system has been put into standby
      if (_wakeupTimer == null)
      {
        _wakeupTimer = new WaitableTimer();
        _wakeupTimer.OnTimerExpired += new WaitableTimer.TimerExpiredHandler(OnWakeupTimerExpired);
      }

      // start the timer responsible for standby checking and refreshing settings
      _timer = new System.Timers.Timer();
      _timer.Interval = 60000;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
      _timer.Enabled = true;

      // Configure remoting if not already done
      StartRemoting();

      LoadSettings();

      // Create the default set of standby/resume handlers
      if (_factory == null)
        _factory = new PowerSchedulerFactory(controller);
      _factory.CreateDefaultSet();

      SendPowerSchedulerEvent(PowerSchedulerEventType.Started);

      Log.Info("Powerscheduler: started");
    }

    /// <summary>
    /// Called by the PowerSchedulerPlugin to stop the PowerScheduler
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Stop()
    {
      // stop the global timer responsible for standby checking and refreshing settings
      _timer.Enabled = false;
      _timer.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimerElapsed);
      _timer.Dispose();
      _timer = null;

      UnRegisterPowerEventHandler();

      // dereference the PowerManager instance
      _powerManager = null;

      // Remove the default set of standby/resume handlers
      _factory.RemoveDefaultSet();
      Unregister(_clientStandbyHandler);
      Unregister(_clientWakeupHandler);
      Log.Debug("PowerScheduler: Removed default set of standby/resume handlers to PowerScheduler");

      SendPowerSchedulerEvent(PowerSchedulerEventType.Stopped);

      Log.Info("Powerscheduler: stopped");
    }

    private void RegisterPowerEventHandler()
    {
      // register to power events generated by the system
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().AddPowerEventHandler(new PowerEventHandler(OnPowerEvent));
        Log.Debug("PowerScheduler: Registered PowerScheduler as PowerEventHandler to tvservice");
      }
      else
      {
        Log.Error("PowerScheduler: Unable to register power event handler!");
      }
    }

    private void UnRegisterPowerEventHandler()
    {
      // unregister to power events generated by the system
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().RemovePowerEventHandler(
          new PowerEventHandler(OnPowerEvent));
        Log.Debug("PowerScheduler: UnRegistered PowerScheduler as PowerEventHandler to tvservice");
      }
      else
      {
        Log.Error("PowerScheduler: Unable to unregister power event handler!");
      }
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
      catch (RemotingException) {}
      catch (System.Net.Sockets.SocketException) {}
      // RemotingConfiguration.RegisterWellKnownServiceType(typeof(PowerScheduler), "PowerControl", WellKnownObjectMode.Singleton);
      ObjRef objref = RemotingServices.Marshal(this, "PowerControl", typeof (IPowerController));
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
    /// Checks if a suspend request is in progress
    /// </summary>
    /// <returns>is the system currently trying to suspend?</returns>
    public bool IsSuspendInProgress()
    {
      return _isSuspendInProgress;
    }


    /// <summary>
    /// Used to avoid concurrent suspend requests which could result in a suspend - user resumes - immediately suspends.
    /// </summary>
    private DateTime _ignoreSuspendUntil = DateTime.MinValue;

    /// <summary>
    /// Manually puts the system in Standby (Suspend/Hibernate depending on what is configured)
    /// </summary>
    /// <param name="source">description of the source who puts the system into standby</param>
    /// <param name="force">should we ignore PowerScheduler's current state (true) or not? (false)</param>
    /// <returns></returns>
    public void SuspendSystem(string source, bool force)
    {
      Log.Info("PowerScheduler: Manual system suspend requested by {0}", source);

      // determine standby mode
      switch (_settings.ShutdownMode)
      {
        case ShutdownMode.Suspend:
          SuspendSystem(source, (int)RestartOptions.Suspend, force);
          break;
        case ShutdownMode.Hibernate:
          SuspendSystem(source, (int)RestartOptions.Hibernate, force);
          break;
        case ShutdownMode.StayOn:
          Log.Debug("PowerScheduler: Standby requested but system is configured to stay on");
          break;
        default:
          Log.Error("PowerScheduler: unknown shutdown mode: {0}", _settings.ShutdownMode);
          break;
      }
    }

    /// <summary>
    /// Puts the system into the configured standby mode (Suspend/Hibernate)
    /// </summary>
    /// <returns>bool indicating whether or not the request was honoured</returns>
    private void SuspendSystem()
    {
      SuspendSystem("", _settings.ForceShutdown);
    }

    protected class SuspendSystemThreadEnv
    {
      public PowerScheduler that;
      public RestartOptions how;
      public bool force;
      public string source;
    }

    /// <summary>
    /// Puts the system into the configured standby mode (Suspend/Hibernate)
    /// </summary>
    /// <param name="force">should the system be forced to enter standby?</param>
    /// <returns>bool indicating whether or not the request was honoured</returns>
    public void SuspendSystem(string source, int how, bool force)
    {
      lock (this)
      {
        DateTime now = DateTime.Now;

        // block concurrent request?
        if (_ignoreSuspendUntil > now)
        {
          Log.Info("PowerScheduler: Concurrent shutdown was ignored: {0} ; force: {1}", (RestartOptions)how, force);
          return;
        }

        // block any other request forever (for now)
        _ignoreSuspendUntil = DateTime.MaxValue;
      }

      Log.Info("PowerScheduler: Entering shutdown {0} ; forced: {1} -- kick off shutdown thread", (RestartOptions)how,
               force);
      SuspendSystemThreadEnv data = new SuspendSystemThreadEnv();
      data.that = this;
      data.how = (RestartOptions)how;
      data.force = force;
      data.source = source;

      Thread suspendThread = new Thread(SuspendSystemThread);
      suspendThread.Name = "Powerscheduler Suspender";
      suspendThread.Start(data);
    }

    protected static void SuspendSystemThread(object _data)
    {
      SuspendSystemThreadEnv data = (SuspendSystemThreadEnv)_data;
      data.that.SuspendSystemThread(data.source, data.how, data.force);
    }

    protected void SuspendSystemThread(string source, RestartOptions how, bool force)
    {
      Log.Debug("PowerScheduler: Shutdown thread is running: {0}, force: {1}", how, force);
      _isSuspendInProgress = true;
      Log.Debug("PowerScheduler: Informing handlers about UserShutdownNow");
      UserShutdownNow();

      // user is away, so we set _lastUserTime long time in the past to pretend that he didn't access system a long time
      _lastUserTime = DateTime.MinValue;

      // test if shutdown is allowed
      bool disallow = DisAllowShutdown;

      Log.Info("PowerScheduler: Source: {0}; shutdown is allowed {1} ; forced: {2}", source, !disallow, force);

      if (disallow && !force)
      {
        lock (this)
        {
          // allow further requests
          _ignoreSuspendUntil = DateTime.MinValue;
        }
        _isSuspendInProgress = false;
        return;
      }

      SetWakeupTimer();
      if (source == "System")
      {
        // Here we should wait for QuerySuspendFailed / QueryStandByFailed since we have rejected
        // the suspend request
        _querySuspendFailed++;
        Log.Info("PowerScheduler: _querySuspendFailed {0}", _querySuspendFailed);
        do
        {
          System.Threading.Thread.Sleep(1000);
        } while (_querySuspendFailed > 0);
      }
      // activate standby
      _denySuspendQuery = false;
      Log.Info("PowerScheduler: Entering shutdown {0} ; forced: {1}", (RestartOptions)how, force);
      WindowsController.ExitWindows((RestartOptions)how, force, SuspendSystemThreadAfter);
    }

    protected void SuspendSystemThreadAfter(RestartOptions how, bool force, bool result)
    {
      _isSuspendInProgress = false;

      lock (this)
      {
        if (!result)
        {
          // allow further requests
          _ignoreSuspendUntil = DateTime.MinValue;
          return;
        }
        switch (how)
        {
          case RestartOptions.LogOff:
          case RestartOptions.Suspend:
          case RestartOptions.Hibernate:
            {
              // allow not before 5 seconds
              // *** this will block any about-to-suspend requests that have been pending before the shutdown was issued
              // *** (resolves the system-immediately-suspends-after-resume issue)
              _ignoreSuspendUntil = DateTime.Now.AddSeconds(5);
              break;
            }
          case RestartOptions.PowerOff:
          case RestartOptions.Reboot:
          case RestartOptions.ShutDown:
            {
              // allow not before 120 seconds, i.e. give enough time to shutdown the system (anyway this value is reset on reboot)
              _ignoreSuspendUntil = DateTime.Now.AddSeconds(120);
              break;
            }
        }
      }
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
    /// Resets the idle timer of the PowerScheduler. When enough time has passed (IdleTimeout), the system
    /// is suspended as soon as possible (no handler disallows shutdown).
    /// Note that the idle timer is automatically reset when the user moves the mouse or touchs the keyboard.
    /// </summary>
    public void UserActivityDetected(DateTime when)
    {
      if (when > _lastUserTime)
      {
        _lastUserTime = when;
        LogVerbose("PowerScheduler: User input detected at {0}", _lastUserTime);
      }
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

    private int _remoteTags = 0;
    private Hashtable _remoteStandbyHandlers = new Hashtable();
    private Hashtable _remoteWakeupHandlers = new Hashtable();
    private Dictionary<string, int> _remoteStandbyHandlerURIs = new Dictionary<string, int>();
    private Dictionary<string, int> _remoteWakeupHandlerURIs = new Dictionary<string, int>();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int RegisterRemote(string standbyHandlerURI, string wakeupHandlerURI)
    {
      int oldStandbyTag = 0;
      int oldWakeupTag = 0;

      // Find existing registrations
      if (!string.IsNullOrEmpty(standbyHandlerURI) &&
          !_remoteStandbyHandlerURIs.TryGetValue(standbyHandlerURI, out oldStandbyTag))
      {
        oldStandbyTag = _remoteTags + 1;
      }
      if (!string.IsNullOrEmpty(wakeupHandlerURI) &&
          !_remoteWakeupHandlerURIs.TryGetValue(wakeupHandlerURI, out oldWakeupTag))
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
      LogVerbose("PowerScheduler: RegisterRemote tag: {0}, uris: {1}, {2}", newTag, standbyHandlerURI, wakeupHandlerURI);
      if (standbyHandlerURI != null && standbyHandlerURI.Length > 0)
      {
        RemoteStandbyHandler hdl;
        if (newTag <= _remoteTags)
        {
          hdl = (RemoteStandbyHandler)_remoteStandbyHandlers[oldStandbyTag];
          _remoteStandbyHandlers.Remove(oldStandbyTag);
        }
        else
        {
          hdl = new RemoteStandbyHandler(standbyHandlerURI, newTag);
          Register(hdl);
        }

        _remoteStandbyHandlers[newTag] = hdl;
        _remoteStandbyHandlerURIs[standbyHandlerURI] = newTag;
      }
      if (wakeupHandlerURI != null && wakeupHandlerURI.Length > 0)
      {
        RemoteWakeupHandler hdl;
        if (newTag <= _remoteTags)
        {
          hdl = (RemoteWakeupHandler)_remoteWakeupHandlers[oldWakeupTag];
          _remoteWakeupHandlers.Remove(oldWakeupTag);
        }
        else
        {
          hdl = new RemoteWakeupHandler(wakeupHandlerURI, newTag);
          Register(hdl);
        }
        _remoteWakeupHandlers[newTag] = hdl;
        _remoteWakeupHandlerURIs[wakeupHandlerURI] = newTag;
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
      return newTag;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void UnregisterRemote(int tag)
    {
      {
        RemoteStandbyHandler hdl = (RemoteStandbyHandler)_remoteStandbyHandlers[tag];
        if (hdl != null)
        {
          _remoteStandbyHandlers.Remove(tag);
          _remoteStandbyHandlerURIs.Remove(hdl.Url);
          hdl.Close();
          LogVerbose("PowerScheduler: UnregisterRemote StandbyHandler {0}", tag);
          Unregister(hdl);
        }
      }
      {
        RemoteWakeupHandler hdl = (RemoteWakeupHandler)_remoteWakeupHandlers[tag];
        if (hdl != null)
        {
          _remoteWakeupHandlers.Remove(tag);
          _remoteWakeupHandlerURIs.Remove(hdl.Url);
          hdl.Close();
          LogVerbose("PowerScheduler: UnregisterRemote WakeupHandler {0}", tag);
          Unregister(hdl);
        }
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

    #endregion

    #region Private methods

    /// <summary>
    /// Called when the wakeup timer is due (when system resumes from standby)
    /// </summary>
    private void OnWakeupTimerExpired()
    {
      Log.Debug("PowerScheduler: OnResume");
    }

    private bool _onTimerElapsedInside = false;

    /// <summary>
    /// Periodically refreshes the standby configuration and checks if the system should enter standby
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        string threadname = Thread.CurrentThread.Name;
        if (string.IsNullOrEmpty(threadname))
          Thread.CurrentThread.Name = "Powerscheduler Timer";
      }
      catch (Exception ex)
      {
        Log.Error("Powerscheduler: Error naming thread - {0}", ex.Message);
      }

      if (_onTimerElapsedInside) return;
      _onTimerElapsedInside = true;
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
      _onTimerElapsedInside = false;
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
      if (_settings.ExtensiveLogging !=
          Convert.ToBoolean(layer.GetSetting("PowerSchedulerExtensiveLogging", "false").Value))
      {
        _settings.ExtensiveLogging = !_settings.ExtensiveLogging;
        Log.Debug("PowerScheduler: extensive logging enabled: {0}", _settings.ExtensiveLogging);
        changed = true;
      }
      // Check if PowerScheduler should actively put the system into standby
      if (_settings.ShutdownEnabled !=
          Convert.ToBoolean(layer.GetSetting("PowerSchedulerShutdownActive", "false").Value))
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
      if (_settings.ReinitializeController !=
          Convert.ToBoolean(layer.GetSetting("PowerSchedulerReinitializeController", "false").Value))
      {
        _settings.ReinitializeController = !_settings.ReinitializeController;
        LogVerbose("PowerScheduler: Reinitialize controller on wakeup: {0}", _settings.ReinitializeController);
        changed = true;
      }

      PowerSetting pSetting = _settings.GetSetting("ExternalCommand");
      string sSetting = layer.GetSetting("PowerSchedulerCommand", String.Empty).Value;
      if (!sSetting.Equals(pSetting.Get<string>()))
      {
        pSetting.Set<string>(sSetting);
        LogVerbose("PowerScheduler: Run external command before standby / after resume: {0}", sSetting);
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

      // Check configured pre-no-shutdown time
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerPreNoShutdownTime", "120").Value);
      if (_settings.PreNoShutdownTime != setting)
      {
        _settings.PreNoShutdownTime = setting;
        LogVerbose("PowerScheduler: pre-no-shutdown time set to: {0} seconds", _settings.PreNoShutdownTime);
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

      // Check allowed stop time
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerStandbyAllowedEnd", "24").Value);
      if (_settings.AllowedSleepStopTime != setting)
      {
        _settings.AllowedSleepStopTime = setting;
        LogVerbose("PowerScheduler: Standby allowed until {0} o' clock", _settings.AllowedSleepStopTime);
        changed = true;
      }

      // Check configured allowed start time
      setting = Int32.Parse(layer.GetSetting("PowerSchedulerStandbyAllowedStart", "0").Value);
      if (_settings.AllowedSleepStartTime != setting)
      {
        _settings.AllowedSleepStartTime = setting;
        LogVerbose("PowerScheduler: Standby allowed starting at {0} o' clock", _settings.AllowedSleepStartTime);
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
      long systemUptime = Environment.TickCount;

      LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
      lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
      lastInputInfo.dwTime = 0;

      if (!GetLastInputInfo(ref lastInputInfo))
      {
        Log.Error("PowerScheduler: Unable to GetLastInputInfo!");
        return DateTime.MinValue;
      }

      long lastKick = lastInputInfo.dwTime;
      long delta = lastKick - systemUptime;

      if (delta > 0)
      {
        // there was an overflow (restart at 0) in the tick-counter!
        delta = delta - uint.MaxValue - 1;
      }

      return DateTime.Now.AddMilliseconds(delta);
    }

    /// <summary>
    /// Checks if the system should enter standby
    /// </summary>
    private void CheckForStandby()
    {
      lock (this) // to avoid clash with OnPowerEvent
      {
        if (!_settings.ShutdownEnabled)
          return;

        // scenario: CheckForStandby is called right after resume, but before Resume is handled by OnPowerEvent
        // then we could mis-send the PC to hibernation again (Unattended not reset yet)
        // so, we just check for _standby
        if (_standby)
          return;

        // unattended? (check regualary to have log entries)
        bool unattended = Unattended;

        // is anybody disallowing shutdown?
        if (!DisAllowShutdown)
        {
          if (!_idle)
          {
            Log.Info("PowerScheduler: System changed from busy state to idle state");
            _idle = true;
            SendPowerSchedulerEvent(PowerSchedulerEventType.SystemIdle);
          }

          // Bav fixing mantis bug 1183: TV Server kick comp to hib after long time "editing" in TV Setup
          // DisAllowShutdown takes some seconds to run => check once again Unattended 
          if (Unattended)
          {
            Log.Info("PowerScheduler: System is unattended and idle - initiate suspend/hibernate");
            SuspendSystem();
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
        case PowerEventType.QuerySuspend:
        case PowerEventType.QueryStandBy:
          Log.Debug("PowerScheduler: System wants to enter standby (query)");
          // First request for suspend, this we will reject by returning false.
          // Instead we will start a shutdown thread that will de-init and last will 
          // issue a new suspend query that will accept.
          if (_denySuspendQuery)
          {
            Log.Debug("PowerScheduler: Suspend queried, starting suspend sequence");
            // Always try to Hibernate (S4). If system is set to S3, then Hibernate will fail and result will be S3
            SuspendSystem("System", (int)RestartOptions.Hibernate, false);
            return false;
          }
          return true;
        case PowerEventType.Suspend:
        case PowerEventType.StandBy:
          Suspend(powerStatus);
          return true;
        case PowerEventType.QuerySuspendFailed:
        case PowerEventType.QueryStandByFailed:
          _querySuspendFailed--;
          Log.Debug("PowerScheduler: Entering standby was disallowed (blocked)");
          return true;
        case PowerEventType.ResumeAutomatic:
        case PowerEventType.ResumeCritical:
        case PowerEventType.ResumeSuspend:
          // note: this event may not arrive unless the user has moved the mouse or hit a key
          // so, we should also handle ResumeAutomatic and ResumeCritical (as done above)
          Resume(powerStatus);
          return true;
      }
      return true;
    }

    private void Suspend(PowerEventType powerStatus)
    {
      if (powerStatus == PowerEventType.Suspend)
        Log.Debug("PowerScheduler: System is going to suspend");
      else if (powerStatus == PowerEventType.StandBy)
        Log.Debug("PowerScheduler: System is going to standby");
      _denySuspendQuery = true; // reset the flag
      _standby = true;
      _timer.Enabled = false;
      _controller.EpgGrabberEnabled = false;
      SetWakeupTimer();
      DeInitController();
      RunExternalCommand("suspend");
      SendPowerSchedulerEvent(PowerSchedulerEventType.EnteringStandby, false);
    }

    private void Resume(PowerEventType powerStatus)
    {
      if (powerStatus == PowerEventType.ResumeAutomatic)
        Log.Debug("PowerScheduler: System has resumed automatically from standby");
      else if (powerStatus == PowerEventType.ResumeCritical)
        Log.Debug("PowerScheduler: System has resumed from standby after a critical suspend");
      else if (powerStatus == PowerEventType.ResumeSuspend)
        Log.Debug("PowerScheduler: System has resumed from standby");

      if (!_standby)
        return;

      lock (this)
      {
        // INSIDE lock!!! to avoid clash with CheckForStandBy --->
        _lastUserTime = DateTime.Now;
        if (_idle)
        {
          Log.Info("PowerScheduler: System changed from idle state to busy state");
          _idle = false;
          SendPowerSchedulerEvent(PowerSchedulerEventType.SystemBusy);
        }
        _standby = false;
        // <--- INSIDE lock!!!

        // if real resume, run command
        RunExternalCommand("wakeup");

        // reinitialize TVController if system is configured to do so and not already done
        ReInitController();
      }
      // enable timer
      if (_timer != null)
        _timer.Enabled = true;

      if (!_controller.EpgGrabberEnabled)
        _controller.EpgGrabberEnabled = true;
      SendPowerSchedulerEvent(PowerSchedulerEventType.ResumedFromStandby);
    }

    /// <summary>
    /// Sets the wakeup timer to the earliest desirable wakeup time
    /// </summary>
    private void SetWakeupTimer()
    {
      Log.Debug("PowerScheduler: SetWakeupTimer");
      if (_settings.WakeupEnabled)
      {
        // determine next wakeup time from IWakeupHandlers
        DateTime nextWakeup = NextWakeupTime;
        bool disallow = DisAllowShutdown;
        if (disallow && System.Environment.OSVersion.Version.Major >= 6)
        {
          // fixing mantis 1487: If suspend it's triggered by remote on vista PSClient tells TV Server Power scheduler to wakeup after 1 min 
          Log.Debug("PowerScheduler: Vista detected => DisAllowShutdown ignored in SetWakeupTimer");
          disallow = false;
        }
        if (nextWakeup < DateTime.MaxValue || disallow)
        {
          double delta;
          if (disallow) delta = 0; // should instantly restart
          else
          {
            nextWakeup = nextWakeup.AddSeconds(-_settings.PreWakeupTime);
            delta = nextWakeup.Subtract(DateTime.Now).TotalSeconds;
          }

          if (delta < 60)
          {
            // the wake up event is too near, when we set the timer and the suspend process takes to long, i.e. the timer gets fired
            // while suspending, the system would NOT wake up!

            // so, we will in any case set the wait time to 60 seconds
            delta = 60;
          }
          _wakeupTimer.SecondsToWait = delta;
          Log.Debug("PowerScheduler: Set wakeup timer to wakeup system in {0} minutes", delta / 60);
        }
        else
        {
          Log.Debug("PowerScheduler: No pending events found in the future which should wakeup the system");
          _wakeupTimer.SecondsToWait = -1;
        }
      }
      else
        Log.Debug("PowerScheduler: Warning WakeupEnabled is not set.");
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

    /// <summary>
    /// action: standby, wakeup, epg
    /// </summary>
    /// <param name="action"></param>
    public void RunExternalCommand(String action)
    {
      PowerSetting setting = _settings.GetSetting("ExternalCommand");
      if (setting.Get<string>().Equals(String.Empty))
        return;
      using (Process p = new Process())
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = setting.Get<string>();
        psi.UseShellExecute = true;
        psi.WindowStyle = ProcessWindowStyle.Minimized;
        psi.Arguments = action;
        p.StartInfo = psi;
        LogVerbose("Starting external command: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
        try
        {
          p.Start();
          p.WaitForExit();
        }
        catch (Exception e)
        {
          Log.Write(e);
        }
        LogVerbose("External command finished");
      }
    }

    #region Logging wrapper methods

    private void LogVerbose(string msg)
    {
      //don't just do this: LogVerbose(msg, null);!!
      if (_settings.ExtensiveLogging)
        Log.Debug(msg);
    }

    private void LogVerbose(string format, params object[] args)
    {
      if (_settings.ExtensiveLogging)
        Log.Debug(format, args);
    }

    #endregion

    /// <summary>
    /// Frees the tv tuners before entering standby
    /// </summary>
    private void DeInitController()
    {
      if (_cardsStopped)
        return;
      // only free tuner cards if reinitialization is enabled in settings
      if (!_settings.ReinitializeController)
        return;

      TvService.TVController controller = _controller as TvService.TVController;
      if (controller != null)
      {
        Log.Debug("PowerScheduler: DeInit controller");
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
      // only reinitialize controller if enabled in settings
      if (!_settings.ReinitializeController)
        return;

      TvService.TVController controller = _controller as TvService.TVController;
      if (controller != null && _reinitializeController)
      {
        Log.Debug("PowerScheduler: ReInit Controller");
        controller.Restart();
        _reinitializeController = false;
        _cardsStopped = false;
      }
    }

    #endregion

    private bool _currentUnattended = false;
    private DateTime _currentNextWakeupTime = DateTime.MaxValue;
    private String _currentNextWakeupHandler = "";
    private bool _currentDisAllowShutdown = false;
    private String _currentDisAllowShutdownHandler = "";
    private bool _denySuspendQuery = true;
    private int _querySuspendFailed = 0;
    private bool _isSuspendInProgress = false;

    public void GetCurrentState(bool refresh, out bool unattended, out bool disAllowShutdown,
                                out String disAllowShutdownHandler, out DateTime nextWakeupTime,
                                out String nextWakeupHandler)
    {
      if (refresh)
      {
        bool dummy = DisAllowShutdown;
        DateTime dummy2 = NextWakeupTime;
        dummy = Unattended;
      }

      // give state
      unattended = _currentUnattended;
      disAllowShutdown = _currentDisAllowShutdown;
      disAllowShutdownHandler = _currentDisAllowShutdownHandler;
      nextWakeupTime = _currentNextWakeupTime;
      nextWakeupHandler = _currentNextWakeupHandler;
    }

    #region Private properties

    /// <summary>
    /// Checks whether the system is unattended, i.e. the user was idle some time.
    /// </summary>
    private bool Unattended
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        // adjust _lastUserTime by user activity
        DateTime userInput = GetLastInputTime();

        if (userInput > _lastUserTime)
        {
          _lastUserTime = userInput;
          LogVerbose("PowerScheduler: User input detected at {0}", _lastUserTime);
        }

        LogVerbose("PowerScheduler: lastUserTime: {0:HH:mm:ss.ffff} , {1}", _lastUserTime, _currentUnattended);

        bool val = _lastUserTime <= DateTime.Now.AddMinutes(-_settings.IdleTimeout);
        if (val != _currentUnattended)
        {
          _currentUnattended = val;
          LogVerbose("PowerScheduler: System is now unattended: {0}", val);
        }
        return val;
      }
    }

    /// <summary>
    /// Checks all IStandbyHandlers if one of them wants to prevent standby;
    /// returns false if one of them does; returns true of none of them does.
    /// </summary>
    private bool DisAllowShutdown
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        // at first ask the handlers

        foreach (IStandbyHandler handler in _standbyHandlers)
        {
          bool handlerDisAllowsShutdown = handler.DisAllowShutdown;
          LogVerbose("PowerScheduler.DisAllowShutdown: inspecting handler:{0} DisAllowShutdown:{1}", handler.HandlerName,
                     handlerDisAllowsShutdown);
          if (handlerDisAllowsShutdown)
          {
            _currentDisAllowShutdownHandler = handler.HandlerName;
            _currentDisAllowShutdown = true;
            _powerManager.PreventStandby();
            _lastUserTime = DateTime.Now;
              // remember this time; avoid immediate shutdown after preventing handler is finished
            return true;
          }
        }

        // then, check whether the next event is almost due, i.e. within PreNoShutdownTime seconds
        DateTime nextWakeupTime = NextWakeupTime;
        if (DateTime.Now >= nextWakeupTime.AddSeconds(-_settings.PreNoShutdownTime))
        {
          LogVerbose("PowerScheduler.DisAllowShutdown: some event is almost due");
          _currentDisAllowShutdownHandler = "EVENT-DUE";
          _currentDisAllowShutdown = true;
          return true;
        }

        // get a save 24h hour regardless of the regional settings.
        int Current24hHour = Convert.ToInt32(DateTime.Now.ToString("HH"));

        //check if is allowed to sleep at this time. 
        // e.g. 23:00 -> 07:00 or 01:00 -> 17:00
        if ( // Stop time one day after start time (23:00 -> 07:00)
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
          ))
        {
          _currentDisAllowShutdownHandler = "NOT-ALLOWED-TIME";
          LogVerbose("PowerScheduler.DisAllowShutdown: not allowed hour for standby {0}", Current24hHour);
          _currentDisAllowShutdown = true;
          _powerManager.PreventStandby();
          return true;
        }

        _currentDisAllowShutdown = false;
        _currentDisAllowShutdownHandler = "";
        _powerManager.AllowStandby();
        return false;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void UserShutdownNow()
    {
      // trigger the handlers
      foreach (IStandbyHandler handler in _standbyHandlers)
      {
        handler.UserShutdownNow();
      }
    }

    /// <summary>
    /// Returns the earliest desirable wakeup time from all IWakeupHandlers
    /// </summary>
    private DateTime NextWakeupTime
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        // earliestWakeupTime is set to "now" in order to not miss wakeups that are almost due.
        // preWakupTime is not substracted here.
        String handlerName = "";

        DateTime nextWakeupTime = DateTime.MaxValue;
        DateTime earliestWakeupTime = DateTime.Now;

        //too much logging Log.Debug("PowerScheduler: earliest wakeup time: {0}", earliestWakeupTime); 
        foreach (IWakeupHandler handler in _wakeupHandlers)
        {
          DateTime nextTime = handler.GetNextWakeupTime(earliestWakeupTime);
          if (nextTime < earliestWakeupTime) nextTime = DateTime.MaxValue;
          LogVerbose("PowerScheduler.NextWakeupTime: inspecting IWakeupHandler:{0} time:{1}", handler.HandlerName,
                     nextTime);
          if (nextTime < nextWakeupTime && nextTime >= earliestWakeupTime)
          {
            //too much logging Log.Debug("PowerScheduler: found next wakeup time {0} by {1}", nextTime, handler.HandlerName);
            handlerName = handler.HandlerName;
            nextWakeupTime = nextTime;
          }
        }

        _currentNextWakeupHandler = handlerName;

        // next wake-up time changed?
        if (nextWakeupTime != _currentNextWakeupTime)
        {
          _currentNextWakeupTime = nextWakeupTime;

          Log.Debug("PowerScheduler: new next wakeup time {0} found by {1}", nextWakeupTime, handlerName);
        }

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