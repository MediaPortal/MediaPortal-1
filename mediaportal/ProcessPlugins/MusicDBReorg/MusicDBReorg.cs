#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Threading;
#endregion

namespace MediaPortal.ProcessPlugins.MusicDBReorg
{
  public class MusicDBReorg : IPlugin, IWakeable, ISetupForm
  {

    #region vars

    bool _reorgRunning;
    bool _run;
    int _runHours = 0;
    int _runMinutes = 0;
    bool _runMondays = false;
    bool _runTuesdays = false;
    bool _runWednesdays = false;
    bool _runThursdays = false;
    bool _runFridays = false;
    bool _runSaturdays = false;
    bool _runSundays = false;
    MusicDatabase mDB = null;

    #endregion

    #region Ctor

    public MusicDBReorg()
    {
      // load settings
      using (Settings reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
      {
        _runMondays = reader.GetValueAsBool("musicdbreorg", "monday", false);
        _runTuesdays = reader.GetValueAsBool("musicdbreorg", "tuesday", false);
        _runWednesdays = reader.GetValueAsBool("musicdbreorg", "wednesday", false);
        _runThursdays = reader.GetValueAsBool("musicdbreorg", "thursday", false);
        _runFridays = reader.GetValueAsBool("musicdbreorg", "friday", false);
        _runSaturdays = reader.GetValueAsBool("musicdbreorg", "saturday", false);
        _runSundays = reader.GetValueAsBool("musicdbreorg", "sunday", false);
        _runHours = reader.GetValueAsInt("musicdbreorg", "hours", 0);
        _runMinutes = reader.GetValueAsInt("musicdbreorg", "minutes", 0);
      }

      mDB = MusicDatabase.Instance;
    }

    #endregion

    #region IPlugin members

    /// <summary>
    /// Starts MusicDBreorg
    /// </summary>
    public void Start()
    {
      Log.Info("MusicDBReorg: schedule: {0}:{1}", _runHours, _runMinutes);
      Log.Info("MusicDBReorg: run on: monday:{0}, tuesday:{1}, wednesday:{2}, thursday:{3}, friday:{4}, saturday:{5}, sunday:{6}", _runMondays, _runTuesdays, _runWednesdays, _runThursdays, _runFridays, _runSaturdays, _runSundays);

      // Establish Handler to catch reorg events, when the reorg is from within the Settings GUI
      MusicDatabase.DatabaseReorgChanged += new MusicDBReorgEventHandler(ReorgStatusChange);

      _run = true;
      Work work = new Work(new DoWorkHandler(this.Run));
      work.ThreadPriority = ThreadPriority.Lowest;
      work.Description = "MusicDBReorg Thread";
      GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
      Log.Info("MusicDBReorg: started");
    }

    /// <summary>
    /// Stops MusicDBReorg
    /// </summary>
    public void Stop()
    {
      _run = false;
      Log.Info("MusicDBReorg: stopped");
    }

    #endregion

    #region Implementation


    /// <summary>
    /// When the Reorg is run from inside the GUI Settings, we are notified here and prevent shutdown, while the import is running 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ReorgStatusChange(object sender, DatabaseReorgEventArgs e)
    {
      switch (e.progress)
      {
        case 2:
          _reorgRunning = true;
          break;
        case 100:
          _reorgRunning = false;
          break;
        default:
          break;
      }
    }

    /// <summary>
    /// Run the MusicDBReorg, when it's schedule is due.
    /// </summary>
    private void Run()
    {
      Log.Debug("MusicDBReorg.Run: thread started");
      while (_run)
      {
        if (ShouldRunSchedule())
        {
          // Start the Music DB Reorganization
          _reorgRunning = true;
          Log.Info("MusicDBReorg.Run: schedule is due:{0}", DateTime.Now.ToString());

          try
          {
            mDB.MusicDatabaseReorg(null);
          }
          catch (Exception ex)
          {
            Log.Error("MusicDBReorg.Run: Reorg failed:{0}", ex.Message);
          }

          // store last run
          using (Settings writer = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
          {
            writer.SetValue("musicdbreorg", "lastrun", DateTime.Now.Day);
          }
          Log.Info("MusicDBReorg.Run: Reorg finished:{0}", DateTime.Now.ToString());
          _reorgRunning = false;
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
      Log.Debug("MusicDBReorg.Run: thread stopped");
    }

    /// <summary>
    /// Determines if the configured scheduled is due
    /// </summary>
    /// <returns>bool indicating whether or not to start the Reorg</returns>
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
      if (DateTime.Now.Hour == _runHours)
      {
        if (DateTime.Now.Minute == _runMinutes)
        {
          Log.Info("MusicDBReorg.ShouldRunSchedule: schedule {0}:{1} is due: {2}:{3}", _runHours, _runMinutes, DateTime.Now.Hour, DateTime.Now.Minute);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Determines if the reorg has already run today
    /// </summary>
    /// <returns></returns>
    private bool HasRunToday()
    {
      int lastRunDay;
      using (Settings reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
      {
        lastRunDay = reader.GetValueAsInt("musicdbreorg", "lastrun", 0);
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
          if (!_runMondays) return false;
          break;
        case DayOfWeek.Tuesday:
          if (!_runTuesdays) return false;
          break;
        case DayOfWeek.Wednesday:
          if (!_runWednesdays) return false;
          break;
        case DayOfWeek.Thursday:
          if (!_runThursdays) return false;
          break;
        case DayOfWeek.Friday:
          if (!_runFridays) return false;
          break;
        case DayOfWeek.Saturday:
          if (!_runSaturdays) return false;
          break;
        case DayOfWeek.Sunday:
          if (!_runSundays) return false;
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
          MediaPortal.GUI.Library.Log.Error("MusicDBReorg.GetNextEvent: no valid next run day found!");
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
      return _reorgRunning;
    }

    #endregion

    #region ISetupForm members

    public string PluginName()
    {
      return "MusicDB Reorganisation";
    }

    public string Author()
    {
      return "hwahrmann";
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
      return "Run MusicDB Reorg inside MediaPortal, preventing standby when active and resuming from standby when schedule is due";
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
      System.Windows.Forms.Form f = new MusicDBReorgSettings();
      System.Windows.Forms.DialogResult result = f.ShowDialog();
    }

    #endregion

  }
}