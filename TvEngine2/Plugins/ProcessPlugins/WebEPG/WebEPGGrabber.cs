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

#region usings

using System;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;

#endregion

namespace MediaPortal.ProcessPlugins.WebEPG
{
  public class WebEPGGrabber : IPlugin, IWakeable, ISetupForm
  {
    #region vars

    private bool _grabberRunning;
    private bool _run;
    private int _runHours = 0;
    private int _runMinutes = 0;
    private bool _runMondays = false;
    private bool _runTuesdays = false;
    private bool _runWednesdays = false;
    private bool _runThursdays = false;
    private bool _runFridays = false;
    private bool _runSaturdays = false;
    private bool _runSundays = false;
    private Thread _thread;
    private ILog _epgLog;

    #endregion

    #region Ctor

    public WebEPGGrabber()
    {
      // setup logging
      ServiceProvider services = GlobalServiceProvider.Instance;
      _epgLog = services.Get<ILog>();

      // load settings
      using (Settings reader = new MPSettings())
      {
        _runMondays = reader.GetValueAsBool("webepggrabber", "monday", true);
        _runTuesdays = reader.GetValueAsBool("webepggrabber", "tuesday", true);
        _runWednesdays = reader.GetValueAsBool("webepggrabber", "wednesday", true);
        _runThursdays = reader.GetValueAsBool("webepggrabber", "thursday", true);
        _runFridays = reader.GetValueAsBool("webepggrabber", "friday", true);
        _runSaturdays = reader.GetValueAsBool("webepggrabber", "saturday", true);
        _runSundays = reader.GetValueAsBool("webepggrabber", "sunday", true);
        _runHours = reader.GetValueAsInt("webepggrabber", "hours", 0);
        _runMinutes = reader.GetValueAsInt("webepggrabber", "minutes", 0);
      }
      Log.Info("WebEPGGrabber: schedule: {0}:{1}", _runHours, _runMinutes);
      Log.Info(
        "WebEPGGrabber: run on: monday:{0}, tuesday:{1}, wednesday:{2}, thursday:{3}, friday:{4}, saturday:{5}, sunday:{6}",
        _runMondays, _runTuesdays, _runWednesdays, _runThursdays, _runFridays, _runSaturdays, _runSundays);
    }

    #endregion

    #region IPlugin members

    /// <summary>
    /// Starts the WebEPGGrabber
    /// </summary>
    public void Start()
    {
      _run = true;
      _thread = new Thread(new ThreadStart(Run));
      _thread.IsBackground = true;
      _thread.Name = "WebEPGGrabber thread";
      _thread.Priority = ThreadPriority.Lowest;
      _thread.Start();
      Log.Info("WebEPGGrabber: started");
    }

    /// <summary>
    /// Stops the WebEPGGrabber
    /// </summary>
    public void Stop()
    {
      _run = false;
      Log.Info("WebEPGGrabber: stopped");
    }

    #endregion

    #region Implementation

    /// <summary>
    /// Main entrypoint for the scheduler thread. Ideally should be replaced by a general scheduler/threadpool
    /// which can run timed events.
    /// </summary>
    private void Run()
    {
      Log.Debug("WebEPGGrabber.Run: thread started");
      while (_run)
      {
        if (ShouldRunSchedule())
        {
          // Start the WebEPG grabber
          _grabberRunning = true;
          Log.Info("WebEPGGrabber.Run: schedule is due:{0}", DateTime.Now.ToString());

          string configFile = Config.GetFile(Config.Dir.Config, "WebEPG", "WebEPG.xml");
          string xmltvDirectory = Config.GetSubFolder(Config.Dir.Config, @"xmltv\");
          EPG.WebEPG grabber = new EPG.WebEPG(configFile, xmltvDirectory, Config.GetFolder(Config.Dir.Config));
          try
          {
            Log.Info("WebEPGGrabber.Run: run grabber");
            grabber.Import();
          }
          catch (Exception ex)
          {
            Log.Error("WebEPGGrabber.Run: grabber exception:{0} {1}", ex.Message, ex.StackTrace);
            _epgLog.Error(LogType.WebEPG, "WebEPG: Fatal Error");
            _epgLog.Error(LogType.WebEPG, "WebEPG: {0}", ex.Message);
          }
          _epgLog.Info(LogType.WebEPG, "WebEPG: Finished");

          // store last run
          using (Settings writer = new MPSettings())
          {
            writer.SetValue("webepggrabber", "lastrun", DateTime.Now.Day);
          }
          Log.Info("WebEPGGrabber.Run: grabber finished:{0}", DateTime.Now.ToString());
          _grabberRunning = false;
        }
        else
        {
          // stay Idle for a minute checking if we have to stop every second
          int timeout = 60000;
          while (_run && timeout > 0)
          {
            Thread.Sleep(1000);
            timeout -= 1000;
          }
        }
      }
      Log.Debug("WebEPGGrabber.Run: thread stopped");
    }

    /// <summary>
    /// Determines if the configured scheduled is due
    /// </summary>
    /// <returns>bool indicating whether or not to start the grabber</returns>
    private bool ShouldRunSchedule()
    {
      // if we've already run today then don't run
      if (HasRunToday())
      {
        return false;
      }

      // check if we have to run this day
      if (!ShouldRun(DateTime.Now.DayOfWeek))
      {
        return false;
      }

      // check if the schedule is due
      if (DateTime.Now.Hour >= _runHours)
      {
        if (DateTime.Now.Minute >= _runMinutes)
        {
          Log.Info("WebEPGGrabber.ShouldRunSchedule: schedule {0}:{1} is due: {2}:{3}", _runHours, _runMinutes,
                   DateTime.Now.Hour, DateTime.Now.Minute);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Determines if the grabber has already run today
    /// </summary>
    /// <returns></returns>
    private bool HasRunToday()
    {
      int lastRunDay;
      using (Settings reader = new MPSettings())
      {
        lastRunDay = reader.GetValueAsInt("webepggrabber", "lastrun", 0);
      }
      if (lastRunDay == DateTime.Now.Day)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determines if the schedule should run on the specified DateTime.DayOfWeek
    /// </summary>
    /// <param name="dow">DayOfWeek to check for</param>
    /// <returns>bool indicating whether or not the schedule should run on the specified day</returns>
    private bool ShouldRun(DayOfWeek dow)
    {
      switch (dow)
      {
        case DayOfWeek.Monday:
          if (!_runMondays)
          {
            return false;
          }
          break;
        case DayOfWeek.Tuesday:
          if (!_runTuesdays)
          {
            return false;
          }
          break;
        case DayOfWeek.Wednesday:
          if (!_runWednesdays)
          {
            return false;
          }
          break;
        case DayOfWeek.Thursday:
          if (!_runThursdays)
          {
            return false;
          }
          break;
        case DayOfWeek.Friday:
          if (!_runFridays)
          {
            return false;
          }
          break;
        case DayOfWeek.Saturday:
          if (!_runSaturdays)
          {
            return false;
          }
          break;
        case DayOfWeek.Sunday:
          if (!_runSundays)
          {
            return false;
          }
          break;
        default:
          return false;
      }
      return true;
    }

    #endregion

    #region IWakeable members

    /// <summary>
    /// Determines on what DateTime the next schedule will run
    /// </summary>
    /// <param name="time">earliestWakeupDateTime</param>
    /// <returns>DateTime indicating the next schedule</returns>
    public DateTime GetNextEvent(DateTime time)
    {
      DateTime nextRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _runHours, _runMinutes, 0);
      if (HasRunToday())
      {
        // find out next scheduled run datetime
        int i = 0;
        while (++i < 8)
        {
          if (ShouldRun(nextRun.AddDays(i).DayOfWeek))
          {
            nextRun = nextRun.AddDays(i);
            break;
          }
        }
        if (DateTime.Now.DayOfWeek == nextRun.DayOfWeek)
        {
          Log.Error("WebEPGGrabber.GetNextEvent: no valid next run day found!");
          nextRun = nextRun.AddYears(1);
        }
      }
      return nextRun;
    }

    /// <summary>
    /// Is PowerScheduler allowed to put the system into standby?
    /// </summary>
    /// <returns>bool</returns>
    public bool DisallowShutdown()
    {
      return _grabberRunning;
    }

    #endregion

    #region ISetupForm members

    public string PluginName()
    {
      return "WebEPG grabber";
    }

    public string Author()
    {
      return "micheloe";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public string Description()
    {
      return
        "Run WebEPG inside MediaPortal, preventing standby when active and resuming from standby when schedule is due";
    }

    public bool GetHome(out string buttonText, out string buttonImage, out string imageFocus, out string pictureImage)
    {
      buttonText = string.Empty;
      buttonImage = string.Empty;
      imageFocus = string.Empty;
      pictureImage = string.Empty;
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      Form f = new WebEPGGrabberSettings();
      DialogResult result = f.ShowDialog();
    }

    #endregion
  }
}