#region Copyright (C) 2007 Team MediaPortal
/* 
 *	Copyright (C) 2007 Team MediaPortal
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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TvControl;
using TvService;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler;
using TvEngine.PowerScheduler.Interfaces;
#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Handles standby/wakeup for EPG grabbing
  /// </summary>
  public class EpgGrabbingHandler : IStandbyHandler, IWakeupHandler, IEpgHandler
  {
    #region Structs
    struct GrabberSource
    {
      string _name;
      bool _standbyAllowed;
      DateTime _lastUpdate;
      DateTime _nextWakeupTime;
      public GrabberSource(string name, bool standbyAllowed)
      {
        _name = name;
        _standbyAllowed = standbyAllowed;
        _lastUpdate = DateTime.Now;
        _nextWakeupTime = DateTime.MaxValue;
      }
      public string Name
      {
        get { return _name; }
      }
      public bool StandbyAllowed
      {
        get { return _standbyAllowed; }
        set
        {
          _standbyAllowed = value;
          _lastUpdate = DateTime.Now;
        }
      }
      public DateTime LastUpdate
      {
        get { return _lastUpdate; }
      }
      public DateTime NextWakeupTime
      {
        get { return _nextWakeupTime; }
        set
        {
          _nextWakeupTime = value;
          _lastUpdate = DateTime.Now;
        }
      }
    }
    #endregion

    #region Events
    private event EPGScheduleHandler _epgScheduleDue;
    #endregion

    #region Variables
    IController _controller;
    Dictionary<object, GrabberSource> _extGrabbers;
    #endregion

    #region Constructor
    public EpgGrabbingHandler(IController controller)
    {
      _controller = controller;
      _extGrabbers = new Dictionary<object, GrabberSource>();
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent += new PowerSchedulerEventHandler(EpgGrabbingHandler_OnPowerSchedulerEvent);
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
        GlobalServiceProvider.Instance.Remove<IEpgHandler>();
      GlobalServiceProvider.Instance.Add<IEpgHandler>(this);
    }
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
          setting = ps.Settings.GetSetting("PreventStandbyWhenGrabbingEPG");
          enabled = Convert.ToBoolean(layer.GetSetting("PreventStandbyWhenGrabbingEPG", "false").Value);
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
            Log.Debug("PowerScheduler: preventing standby when grabbing EPG: {0}", enabled);
          }

          // Check if system should wakeup for EPG grabs
          setting = ps.Settings.GetSetting("WakeupSystemForEPGGrabbing");
          enabled = Convert.ToBoolean(layer.GetSetting("WakeupSystemForEPGGrabbing", "false").Value);
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
            Log.Debug("PowerScheduler: wakeup system for EPG grabbing: {0}", enabled);
          }

          // Check if a wakeup time is set
          setting = ps.Settings.GetSetting("EPGWakeupConfig");
          EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("EPGWakeupConfig", String.Empty).Value));

          if (!config.Equals(setting.Get<EPGWakeupConfig>()))
          {
            setting.Set<EPGWakeupConfig>(config);
            Log.Debug("PowerScheduler: wakeup system for EPG at time: {0}:{1}", config.Hour, config.Minutes);
            if (config.Days != null)
            {
              foreach (EPGGrabDays day in config.Days)
                Log.Debug("PowerScheduler: EPG wakeup on day {0}", day);
            }
            Log.Debug("PowerScheduler: EPG last run: {0}", config.LastRun);
          }

          // check if schedule is due
          // check if we've already run today
          if (config.LastRun.Day != DateTime.Now.Day)
          {
            // check if we should run today
            if (ShouldRun(config.Days, DateTime.Now.DayOfWeek))
            {
              // check if schedule is due
              if (DateTime.Now.Hour >= config.Hour)
              {
                if (DateTime.Now.Minute >= config.Minutes)
                {
                  Log.Info("PowerScheduler: EPG schedule {0}:{1} is due: {2}:{3}",
                    config.Hour, config.Minutes, DateTime.Now.Hour, DateTime.Now.Minute);
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

                  // Update last schedule run status
                  config.LastRun = DateTime.Now;
                  Setting s = layer.GetSetting("EPGWakeupConfig", String.Empty);
                  s.Value = config.SerializeAsString();
                  s.Persist();

                }
              }
            }
          }

          // Cleanup of expired grabber sources
          foreach (object o in _extGrabbers.Keys)
          {
            GrabberSource s = _extGrabbers[o];
            if (s.LastUpdate < DateTime.Now.AddMinutes(-ps.Settings.IdleTimeout))
              _extGrabbers.Remove(o);
          }

          break;
      }
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
      // Start by thinking we should run today
      DateTime nextRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, cfg.Hour, cfg.Minutes, 0);
      // check if we've already run today
      if (cfg.LastRun.Day == DateTime.Now.Day)
      {
        // determine first next day to run EPG grabber
        for (int i = 1; i < 8; i++)
        {
          if (ShouldRun(cfg.Days, nextRun.AddDays(i).DayOfWeek))
          {
            nextRun = nextRun.AddDays(i);
            break;
          }
        }
        if (DateTime.Now.Day == nextRun.Day)
        {
          Log.Error("PowerScheduler: no valid next wakeup date for EPG grabbing found!");
          nextRun = DateTime.MaxValue;
        }
      }
      return nextRun;
    }
    #endregion

    #region IStandbyHandler/IWakeupHandler implementation
    public bool DisAllowShutdown
    {
      get
      {
        // check if DVB EPG grabber is running
        if (_controller.EpgGrabberEnabled)
          return true;
        // check if any external EPG source wants to prevent standby
        foreach (GrabberSource source in _extGrabbers.Values)
          if (!source.StandbyAllowed)
            return true;
        return false;
      }
    }
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      bool isExternal = false;
      string externalName = String.Empty;
      DateTime nextRun = GetNextWakeupSchedule(earliestWakeupTime);
      // check if any external EPG source wants to wakeup the system
      foreach (GrabberSource source in _extGrabbers.Values)
      {
        // check if source has set a valid preferred wakeup time
        if (source.NextWakeupTime != DateTime.MaxValue)
        {
          // check if wakeup time is in the future and past idle timeout
          if (source.NextWakeupTime > earliestWakeupTime)
          {
            if (source.NextWakeupTime < nextRun)
            {
              isExternal = true;
              externalName = source.Name;
              break;
            }
          }
        }
      }
      if (isExternal)
        Log.Debug("PowerScheduler: next EPG wakeup set by external EPG source {0}", externalName);
      return nextRun;
    }
    public string HandlerName
    {
      get { return "EpgGrabbingHandler"; }
    }
    #endregion

    #region IEpgHandler implementation

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetStandbyAllowed(object source, bool allowed)
    {
      if (!_extGrabbers.ContainsKey(source))
      {
        _extGrabbers.Add(source, new GrabberSource(source.ToString(), allowed));
      }
      else
      {
        GrabberSource gSource = _extGrabbers[source];
        gSource.StandbyAllowed = allowed;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetNextEPGWakeupTime(object source, DateTime time)
    {
      if (!_extGrabbers.ContainsKey(source))
      {
        GrabberSource gSource = new GrabberSource(source.ToString(), false);
        gSource.NextWakeupTime = time;
        _extGrabbers.Add(source, gSource);
      }
      else
      {
        GrabberSource gSource = new GrabberSource(source.ToString(), false);
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
          foreach (Delegate del in _epgScheduleDue.GetInvocationList())
          {
            EPGScheduleHandler handler = value;
            Type t = handler.Target.GetType();
            if (del.Target.GetType().Equals(t))
              _epgScheduleDue -= del as EPGScheduleHandler;
            _epgScheduleDue += handler;
          }
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
  }
}
