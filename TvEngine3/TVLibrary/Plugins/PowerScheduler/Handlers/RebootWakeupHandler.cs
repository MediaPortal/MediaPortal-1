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
using TvDatabase;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Handles wakeup for system reboot
  /// </summary>
  public class RebootWakeupHandler : IWakeupHandler
  {
    #region Constructor

    /// <summary>
    /// Constructor for RebootHandler
    /// </summary>
    public RebootWakeupHandler()
    {
      GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent +=
        new PowerSchedulerEventHandler(OnPowerSchedulerEvent);
    }

    #endregion

    #region IWakeupHandler implementation

    [MethodImpl(MethodImplOptions.Synchronized)]
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      EPGWakeupConfig cfg = ps.Settings.GetSetting("RebootConfig").Get<EPGWakeupConfig>();
      DateTime now = DateTime.Now;

      // The earliest wakeup time cannot be in the past
      if (earliestWakeupTime < now)
        earliestWakeupTime = now;

      // Start with the earliest possible day
      DateTime nextRun = new DateTime(earliestWakeupTime.Year, earliestWakeupTime.Month, earliestWakeupTime.Day, cfg.Hour, cfg.Minutes, 0);

      // If the wakeup time is before the earliest wakeup time or if there already was a reboot on this day then take the next day
      if (nextRun < earliestWakeupTime || cfg.LastRun.Date >= nextRun.Date)
        nextRun = nextRun.AddDays(1);

      // Try the next 7 days
      for (int i = 0; i < 7; i++)
      {
        // Check if this day is configured for reboot
        if (ShouldRun(cfg.Days, nextRun.DayOfWeek))
          return nextRun;

        nextRun = nextRun.AddDays(1);
      }

      // Found no day configured for reboot
      return DateTime.MaxValue;
    }

    public string HandlerName
    {
      get { return "Reboot"; }
    }

    #endregion

    #region Private methods

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:

          IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
          if (ps == null)
            return;

          TvBusinessLayer layer = new TvBusinessLayer();
          PowerSetting setting;
          bool enabled;

          EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("PowerSchedulerRebootConfig", String.Empty).Value));

          if (args.EventType == PowerSchedulerEventType.Started)
          {
            // Get time of last reboot
            if (config.LastRun == DateTime.MinValue)
              config.LastRun = DateTime.Now;
            else
              config.LastRun = DateTime.Now.AddMilliseconds(-Environment.TickCount);

            // Save last reboot status
            Setting s = layer.GetSetting("PowerSchedulerRebootConfig", String.Empty);
            s.Value = config.SerializeAsString();
            s.Persist();
            Log.Debug("RebootHandler: Set time of last reboot: {0}", config.LastRun);
          }

          // Check if system should wakeup for reboot
          setting = ps.Settings.GetSetting("RebootWakeup");
          enabled = Convert.ToBoolean(layer.GetSetting("PowerSchedulerRebootWakeup", "false").Value);
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
            Log.Debug("RebootHandler: Wakeup system for reboot: {0}", enabled ? "enabled" : "disabled");
          }

          // Check if a reboot time is set
          setting = ps.Settings.GetSetting("RebootConfig");
          if (!config.Equals(setting.Get<EPGWakeupConfig>()))
          {
            setting.Set<EPGWakeupConfig>(config);
            Log.Debug("RebootHandler: Reboot system at {0:00}:{1:00}", config.Hour, config.Minutes);
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
              Log.Debug("RebootHandler: Reboot system on: {0}", days);
            }
          }

          if (args.EventType == PowerSchedulerEventType.Elapsed)
          {
            // Check if reboot is due
            if (ShouldRunNow())
            {
              // See if system is idle
              bool unattended, disAllowShutdown;
              String disAllowShutdownHandler, nextWakeupHandler;
              DateTime nextWakeupTime;

              // Reboot only if all other handlers allow standby
              ps.GetCurrentState(false, out unattended, out disAllowShutdown, out disAllowShutdownHandler,
                out nextWakeupTime, out nextWakeupHandler);
              if (!disAllowShutdown)
              {
                // Kick off reboot thread
                Log.Debug("RebootHandler: Reboot is due - reboot now");
                Thread workerThread = new Thread(new ThreadStart(RebootThread));
                workerThread.Name = "RebootHandler";
                workerThread.IsBackground = true;
                workerThread.Priority = ThreadPriority.Lowest;
                workerThread.Start();
              }
              else
                Log.Debug("RebootHandler: Reboot is due - reboot when standby is allowed");
            }
          }
          break;
      }
    }

    /// <summary>
    /// Run an external command with parameter "action" 
    /// </summary>
    /// <param name="action">standby, wakeup, epg, reboot</param>
    public void RunExternalCommand(String action)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      String cmd = layer.GetSetting("PowerSchedulerRebootCommand", String.Empty).Value;
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
        psi.Verb = "runas";

        p.StartInfo = psi;
        Log.Debug("RebootHandler: Starting external command: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
        try
        {
          p.Start();
          p.WaitForExit();
        }
        catch (Exception ex)
        {
          Log.Error("RebootHandler: Exception in RunExternalCommand: {0}", ex.Message);
          Log.Info("RebootHandler: Exception in RunExternalCommand: {0}", ex.Message);
        }
        Log.Debug("RebootHandler: External command finished");
      }
    }

    /// <summary>
    /// Thread to perform reboot
    /// </summary>
    private void RebootThread()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("PowerSchedulerRebootConfig", String.Empty).Value));

      Log.Debug("RebootHandler: Reboot schedule {0:00}:{1:00} is due", config.Hour, config.Minutes);

      // Start external command
      RunExternalCommand("reboot");

      // Trigger reboot
      Log.Info("RebootHandler: Reboot system");
      IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      ps.SuspendSystem("RebootHandler", (int)RestartOptions.Reboot, false);
    }

    /// <summary>
    /// Check if a reboot is due
    /// </summary>
    /// <returns>Returns whether the system should reboot now</returns>
    private bool ShouldRunNow()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      EPGWakeupConfig config = new EPGWakeupConfig((layer.GetSetting("PowerSchedulerRebootConfig", String.Empty).Value));

      // Check if this day is configured for reboot and there was no reboot yet
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

    #endregion
  }
}