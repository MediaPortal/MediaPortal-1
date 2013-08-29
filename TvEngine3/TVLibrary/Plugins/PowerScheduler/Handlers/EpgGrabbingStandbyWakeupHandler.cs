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
using System.Runtime.CompilerServices;
using System.Threading;
using TvControl;
using TvDatabase;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Handles standby/wakeup for EPG grabbing
  /// </summary>
  public class EpgGrabbingStandbyWakeupHandler : IStandbyHandler, IStandbyHandlerEx, IWakeupHandler, IEpgHandler
  {
    #region Structs

    private class GrabberSource // don't use struct! they are value types and mess when used in a dictionary!
    {
      private string _name;
      private bool _standbyAllowed;
      private DateTime _timeout;
      private DateTime _nextWakeupTime;

      public GrabberSource(string name, bool standbyAllowed, int timeout)
      {
        _name = name;
        _standbyAllowed = standbyAllowed;
        _timeout = DateTime.Now.AddSeconds(timeout);
        _nextWakeupTime = DateTime.MaxValue;
      }

      public string Name
      {
        get { return _name; }
      }

      public bool StandbyAllowed
      {
        get { return _standbyAllowed; }
      }

      public void SetStandbyAllowed(bool allowed, int timeout)
      {
        _standbyAllowed = allowed;
        _timeout = DateTime.Now.AddSeconds(timeout);
      }

      public DateTime Timeout
      {
        get { return _timeout; }
      }

      public DateTime NextWakeupTime
      {
        get { return _nextWakeupTime; }
        set { _nextWakeupTime = value; }
      }
    }

    #endregion

    #region Events

    private event EPGScheduleHandler _epgScheduleDue;

    #endregion

    #region Variables

    private IController _controller;
    private Dictionary<object, GrabberSource> _extGrabbers;

    /// <summary>
    /// Use away mode setting
    /// </summary>
    private bool _useAwayMode = false;

    #endregion

    #region Constructor

    public EpgGrabbingStandbyWakeupHandler(IController controller)
    {
      _controller = controller;
      _extGrabbers = new Dictionary<object, GrabberSource>();
      GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent +=
        new PowerSchedulerEventHandler(EpgGrabbingHandler_OnPowerSchedulerEvent);
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
        GlobalServiceProvider.Instance.Remove<IEpgHandler>();
      GlobalServiceProvider.Instance.Add<IEpgHandler>(this);
    }

    #endregion

    #region IStandbyHandler(Ex)/IWakeupHandler implementation

    public bool DisAllowShutdown
    {
      get
      {
        return (StandbyMode != StandbyMode.StandbyAllowed);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      bool isExternal = false;
      string externalName = String.Empty;
      DateTime nextRun = GetNextWakeupSchedule(earliestWakeupTime);

      // check if any external EPG source wants to wakeup the system
      foreach (GrabberSource source in _extGrabbers.Values)
      {
        DateTime sourceNext = source.NextWakeupTime;

        // check if source has set a valid preferred wakeup time
        if (sourceNext != DateTime.MaxValue)
        {
          // check if wakeup time is in the future and past earliest
          if (sourceNext >= earliestWakeupTime)
          {
            if (sourceNext < nextRun)
            {
              isExternal = true;
              externalName = source.Name;
              nextRun = sourceNext;
              break;
            }
          }
        }
      }
      if (isExternal)
        Log.Debug("EpgGrabbingHandler: Next EPG wakeup set by external EPG source {0}", externalName);
      return nextRun;
    }

    public void UserShutdownNow() { }

    public string HandlerName
    {
      get { return "EPG Grabbing"; }
    }

    public StandbyMode StandbyMode
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        // Check for "should run, but not running"
        if (ShouldRunNow())
        {
          return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
        }

        // check if any card is grabbing EPG
        for (int i = 0; i < _controller.Cards; i++)
        {
          int cardId = _controller.CardId(i);
          if (_controller.IsGrabbingEpg(cardId))
          {
            return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
          }
        }

        // check if any external EPG source wants to prevent standby
        foreach (GrabberSource source in _extGrabbers.Values)
          if (!source.StandbyAllowed)
          {
            Log.Debug("EpgGrabbingHandler: {0} does not allow standby", source.Name);
            return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
          }

        return StandbyMode.StandbyAllowed;
      }
    }

    #endregion

    #region IEpgHandler implementation

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetStandbyAllowed(object source, bool allowed, int timeout)
    {
      if (!_extGrabbers.ContainsKey(source))
      {
        _extGrabbers.Add(source, new GrabberSource(source.ToString(), allowed, timeout));
      }
      else
      {
        GrabberSource gSource = _extGrabbers[source];
        gSource.SetStandbyAllowed(allowed, timeout);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetNextEPGWakeupTime(object source, DateTime time)
    {
      if (!_extGrabbers.ContainsKey(source))
      {
        GrabberSource gSource = new GrabberSource(source.ToString(), true, 0);
        gSource.NextWakeupTime = time;
        _extGrabbers.Add(source, gSource);
      }
      else
      {
        GrabberSource gSource = _extGrabbers[source];
        gSource.NextWakeupTime = time;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public DateTime GetNextEPGWakeupTime()
    {
      return GetNextWakeupSchedule(DateTime.Now);
    }

    #region Events

    public event EPGScheduleHandler EPGScheduleDue
    {
      add
      {
        lock (this)
        {
          // prevent multiple instances from the same object type to be registered
          // with this event
          EPGScheduleHandler handler = value;
          try
          {
            foreach (Delegate del in _epgScheduleDue.GetInvocationList())
            {
              Type t = handler.Target.GetType();
              if (del.Target.GetType().Equals(t))
                _epgScheduleDue -= del as EPGScheduleHandler;
            }
          }
          catch (Exception) { }
          _epgScheduleDue += handler;
        }
      }
      remove
      {
        lock (this)
        {
          _epgScheduleDue -= value;
        }
      }
    }

    #endregion

    #endregion

    #region Private methods

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void EpgGrabbingHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:

          IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
          TvBusinessLayer layer = new TvBusinessLayer();
          PowerSetting setting;
          bool enabled;

          // Check if standby should be prevented when grabbing EPG
          setting = ps.Settings.GetSetting("EPGPreventStandby");
          enabled = Convert.ToBoolean(layer.GetSetting("PowerSchedulerEPGPreventStandby", "false").Value);
          if (setting.Get<bool>() != enabled)
          {
            setting.Set<bool>(enabled);
            if (enabled)
            {
              if (ps.IsRegistered(this as IStandbyHandler))
                ps.Unregister(this as IStandbyHandler);
              ps.Register(this as IStandbyHandler);
            }
            else
            {
              ps.Unregister(this as IStandbyHandler);
            }
            Log.Debug("EpgGrabbingHandler: Preventing standby when grabbing EPG: {0}", enabled);
          }

          // Check if away mode should be used
          setting = ps.Settings.GetSetting("EPGAwayMode");
          _useAwayMode = Convert.ToBoolean(layer.GetSetting("PowerSchedulerEPGAwayMode", "false").Value);
          if (setting.Get<bool>() != _useAwayMode)
          {
            setting.Set<bool>(_useAwayMode);
            Log.Debug("EpgGrabbingHandler: Use away mode: {0}", _useAwayMode);
          }

          // Check if system should wakeup for EPG grabs
          setting = ps.Settings.GetSetting("EPGWakeup");
          enabled = Convert.ToBoolean(layer.GetSetting("PowerSchedulerEPGWakeup", "false").Value);
          if (setting.Get<bool>() != enabled)
          {
            setting.Set<bool>(enabled);
            if (enabled)
            {
              if (ps.IsRegistered(this as IWakeupHandler))
                ps.Unregister(this as IWakeupHandler);
              ps.Register(this as IWakeupHandler);
            }
            else
            {
              ps.Unregister(this as IWakeupHandler);
            }
            Log.Debug("EpgGrabbingHandler: Wakeup system for EPG grabbing: {0}", enabled ? "enabled" : "disabled");
          }

          // Check if a wakeup time is set
          setting = ps.Settings.GetSetting("EPGWakeupConfig");
          EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("PowerSchedulerEPGWakeupConfig", String.Empty).Value));
          if (!config.Equals(setting.Get<EPGWakeupConfig>()))
          {
            setting.Set<EPGWakeupConfig>(config);
            Log.Debug("EpgGrabbingHandler: EPG grabbing at {0:00}:{1:00}", config.Hour, config.Minutes);
            if (config.Days != null)
            {
              String days = "";
              foreach (EPGGrabDays day in config.Days)
              {
                if (days == "")
                  days = day.ToString();
                else
                  days = days + ", " + day.ToString();
              }
              Log.Debug("EpgGrabbingHandler: EPG grabbing on: {0}", days);
            }
            Log.Debug("EpgGrabbingHandler: EPG last run was at {0}", config.LastRun);
          }

          // check if schedule is due
          // check if we've already run today
          if (ShouldRunNow() && !_epgThreadRunning)
          {
            // kick off EPG thread
            _epgThreadRunning = true;
            Thread workerThread = new Thread(new ThreadStart(EPGThreadFunction));
            workerThread.Name = "EPG Grabbing Handler";
            workerThread.IsBackground = true;
            workerThread.Priority = ThreadPriority.Lowest;
            workerThread.Start();
          }

          // Cleanup of expired grabber sources
          // A grabber is said to be expired, when its timeout has passed and there is no valid wakeup time
          // However, when the timeout has passed, the alow-standby flag is set true
          List<object> expired = new List<object>();
          foreach (object o in _extGrabbers.Keys)
          {
            GrabberSource s = _extGrabbers[o];
            if (s.Timeout < DateTime.Now)
            {
              Log.Debug("EpgGrabbingHandler: EPG source '{0}' timed out, setting allow-standby = true for this source.",
                        s.Name);
              // timeout passed, standby is allowed
              s.SetStandbyAllowed(true, 0);

              // no valid wakeup-time -> expired
              if (s.NextWakeupTime == DateTime.MaxValue)
                expired.Add(o);
            }
          }
          foreach (object o in expired)
            _extGrabbers.Remove(o);
          expired = null;

          break;
      }
    }

    /// <summary>
    /// action: standby, wakeup, epg
    /// </summary>
    /// <param name="action"></param>
    public void RunExternalCommand(String action)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      String cmd = layer.GetSetting("PowerSchedulerEPGCommand", String.Empty).Value;
      if (cmd.Equals(String.Empty))
        return;
      using (Process p = new Process())
      {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = cmd;
        psi.UseShellExecute = true;
        psi.WindowStyle = ProcessWindowStyle.Minimized;
        psi.Arguments = action;
        psi.ErrorDialog = false;
        if (Environment.OSVersion.Version.Major >= 6)
        {
          psi.Verb = "runas";
        }

        p.StartInfo = psi;
        Log.Debug("EpgGrabbingHandler: Starting external command: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
        try
        {
          p.Start();
          p.WaitForExit();
        }
        catch (Exception ex)
        {
          Log.Error("EpgGrabbingHandler: Exception in RunExternalCommand: {0}", ex.Message);
          Log.Info("EpgGrabbingHandler: Exception in RunExternalCommand: {0}", ex.Message);
        }
        Log.Debug("EpgGrabbingHandler: External command finished");
      }
    }

    private bool _epgThreadRunning;

    private void EPGThreadFunction()
    {
      // ShouldRun still returns true until LastRun is updated, 
      // shutdown is disallowed until then

      TvBusinessLayer layer = new TvBusinessLayer();
      EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("PowerSchedulerEPGWakeupConfig", String.Empty).Value));

      Log.Debug("EpgGrabbingHandler: EPG schedule {0:00}:{1:00} is due", config.Hour, config.Minutes);

      // start external command
      RunExternalCommand("epg");

      // try a forced start of EPG grabber if not already started 
      if (!_controller.EpgGrabberEnabled)
        _controller.EpgGrabberEnabled = true;

      // Fire the EPGScheduleDue event for EPG grabber plugins
      if (_epgScheduleDue != null)
      {
        lock (_epgScheduleDue)
        {
          if (_epgScheduleDue != null)
            _epgScheduleDue();
        }
      }

      // sleep 10 Seconds to give the grabbers time to kick off their threads,
      // so that they disallow shutdown
      // without this sleep the PS could be tempted to standby (wakeup/standby race condition)
      Thread.Sleep(10000);

      // Update last schedule run status
      config.LastRun = DateTime.Now;
      Setting s = layer.GetSetting("PowerSchedulerEPGWakeupConfig", String.Empty);
      s.Value = config.SerializeAsString();
      s.Persist();

      _epgThreadRunning = false;
    }

    /// <summary>
    /// Returns whether a schedule is due, and the EPG should run now.
    /// </summary>
    /// <returns></returns>
    private bool ShouldRunNow()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("PowerSchedulerEPGWakeupConfig", String.Empty).Value));

      // Check if this day is configured for EPG and there was no EPG grabbing yet
      DateTime now = DateTime.Now;
      if (ShouldRun(config.Days, now.DayOfWeek) && config.LastRun.Date < now.Date)
      {
        // Check if schedule is due
        if (now >= new DateTime(now.Year, now.Month, now.Day, config.Hour, config.Minutes, 0))
          return true;
      }
      return false;
    }

    private bool ShouldRun(List<EPGGrabDays> days, DayOfWeek dow)
    {
      switch (dow)
      {
        case DayOfWeek.Monday:
          return (days.Contains(EPGGrabDays.Monday));
        case DayOfWeek.Tuesday:
          return (days.Contains(EPGGrabDays.Tuesday));
        case DayOfWeek.Wednesday:
          return (days.Contains(EPGGrabDays.Wednesday));
        case DayOfWeek.Thursday:
          return (days.Contains(EPGGrabDays.Thursday));
        case DayOfWeek.Friday:
          return (days.Contains(EPGGrabDays.Friday));
        case DayOfWeek.Saturday:
          return (days.Contains(EPGGrabDays.Saturday));
        case DayOfWeek.Sunday:
          return (days.Contains(EPGGrabDays.Sunday));
        default:
          return false;
      }
    }

    private DateTime GetNextWakeupSchedule(DateTime earliestWakeupTime)
    {
      IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      EPGWakeupConfig cfg = ps.Settings.GetSetting("EPGWakeupConfig").Get<EPGWakeupConfig>();
      DateTime now = DateTime.Now;

      // The earliest wakeup time cannot be in the past
      if (earliestWakeupTime < now)
        earliestWakeupTime = now;

      // Start with the earliest possible day
      DateTime nextRun = new DateTime(earliestWakeupTime.Year, earliestWakeupTime.Month, earliestWakeupTime.Day, cfg.Hour, cfg.Minutes, 0);

      // If the wakeup time is before the earliest wakeup time or if there already was EPG grabbing on this day then take the next day
      if (nextRun < earliestWakeupTime || cfg.LastRun.Date >= nextRun.Date)
        nextRun = nextRun.AddDays(1);

      // Try the next 7 days
      for (int i = 0; i < 7; i++)
      {
        // Check if this day is configured for EPG
        if (ShouldRun(cfg.Days, nextRun.DayOfWeek))
          return nextRun;

        nextRun = nextRun.AddDays(1);
      }

      // Found no day configured for EPG grabbing
      return DateTime.MaxValue;
    }

    #endregion
  }
}