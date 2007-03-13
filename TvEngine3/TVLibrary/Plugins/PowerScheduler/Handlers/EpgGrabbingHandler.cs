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
  public class EpgGrabbingHandler : IStandbyHandler, IWakeupHandler
  {
    #region Variables
    IController _controller;
    #endregion

    #region Constructor
    public EpgGrabbingHandler(IController controller)
    {
      _controller = controller;
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent += new PowerSchedulerEventHandler(EpgGrabbingHandler_OnPowerSchedulerEvent);
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
                  config.LastRun = DateTime.Now;
                  Setting s = layer.GetSetting("EPGWakeupConfig", String.Empty);
                  s.Value = config.SerializeAsString();
                  s.Persist();
                }
              }
            }
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
    #endregion

    #region IStandbyHandler/IWakeupHandler implementation
    public bool DisAllowShutdown
    {
      get
      {
        return _controller.EpgGrabberEnabled;
      }
    }
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
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
    public string HandlerName
    {
      get { return "EpgGrabbingHandler"; }
    }
    #endregion
  }
}
