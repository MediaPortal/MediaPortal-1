#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Ripper;
namespace MediaPortal.PowerScheduler
{
  /// <summary>
  /// Summary description for NVPPowerScheduler.
  /// </summary>
  public class NVPPowerScheduler : IPlugin, ISetupForm, IWakeable
  {
    public static int WINDOW_POWERSCHEDULER = 6039;	// a window ID shouldn't be needed when a non visual plugin ?!

    static int m_iPreRecordInterval = 0;		// Interval to start recording before entered starttime
    static int m_iStartupInterval = 1;			// 1 minute to give computer time to startup
    static int m_iShutdownInterval = 3;		// idle minutes before shutdown(hibernate/standby)
    static long m_iCurrentStart = 0;			// store current wakeup time (ticks) for comparesing
    static int m_iActiveWindow = -1;			// current active window, used to check when WINDOW_HOME is activated
    static string m_shutdownMode = "None";		// mode to use for shutdown (Hibernate, Suspend, Shutdown, None)
    static bool m_bShutdownEnabled = false;		// shutdown enabled/disabled
    static DateTime m_dtShutdownTime = new DateTime();	// Next time system will automaticly shutdown
    static bool m_bExtensiveLog = false;		// Write a lot to Mediaportal.log
    static bool m_bProgramsChanged = false;		// flag - TVGuide has changed, reset wake up time
    static bool m_bRecordingsChanged = false;	// flag - recordings has changed, reset wake up time	
    static bool m_bResetWakeuptime = true;		// flag - reset wake up time
    static bool m_bForceShutdown = false;		// Force shutdown
    static bool m_bDisabled = false;			// if plugin is enabled or disabled
    static bool m_settingsread = false;			// keep track if settings are read
    static DateTime m_dtLastTimercheck = DateTime.Now;	// store last time active to be able to calculate if a power up has ocurrued 
    static bool m_bFirstLogRec = true;			// when aborting shutdown due to recording in progress, write to log only once

    // Instanceate a waitabletimer
    static private WaitableTimer m_Timer = new WaitableTimer();

    // Instanceate a ShutdownTimer
    // Shutdown will only be active on HOME-window but is deactivated by ongoing and
    // near in time pending recordings
    static private System.Windows.Forms.Timer m_SDTimer = new System.Windows.Forms.Timer();

    public NVPPowerScheduler()
    {
    }

    # region Main program

    public void Start()
    {
      if (!m_settingsread)
        LoadSettings();

      if (m_bExtensiveLog) Log.Write(" PowerScheduler: Start() ");

      if (m_bDisabled)
      {
        if (m_bExtensiveLog) Log.Write(" PowerScheduler: disabled ");
        return;
      }

      // start recorder if needed
      Recorder.Start();

      // if StartupInterval is larger than 0 register startup timer events
      if (m_iStartupInterval > 0)
      {	// Register with PowerManager
        PowerManager.OnPowerUp += new PowerManager.ResumeHandler(this.OnWakeupTimer);

        // Register with WaitableTimer - make sure that processing starts on a different thread
        m_Timer.OnTimerExpired += new WaitableTimer.TimerExpiredHandler(PowerManager.OnResume);
      }

      // Register with TVDatabase
      // listen to OnRecordingsChanged and OnProgramsChanged
      TVDatabase.OnRecordingsChanged += new MediaPortal.TV.Database.TVDatabase.OnRecordingChangedHandler(this.OnRecordingsChanged);
      TVDatabase.OnProgramsChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(this.OnProgramsChanged);

      m_bResetWakeuptime = true;
      m_iCurrentStart = -1;

      // Initialize timer
      m_SDTimer.Tick += new EventHandler(OnTimer);
      m_SDTimer.Interval = 10000; // 10 secs between every check 
      m_SDTimer.Start();
      ResetShutdownTimer(m_iShutdownInterval);
    }

    public void Stop()
    {
      // turn off m_timer to allow system to exit
      m_Timer.SecondsToWait = -1;
      Log.Write("PowerScheduler: Stop() ");
    }

    /// <summary>
    /// The wakeup timer has expired
    /// </summary>
    void OnWakeupTimer()
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: OnWakeupTimer() ");

      Log.Write("PowerScheduler: Wakeup timer expired ");
      m_bResetWakeuptime = true;
      m_iCurrentStart = -1;
      //SetPowerUpTimer();
      WakeupManager();


      // start recorder if needed
      Recorder.Start();
      AutoPlay.StartListening();
    }

    /// <summary>
    /// Recordings has been changed, flag for a rescan
    /// </summary>
    void OnRecordingsChanged(TVDatabase.RecordingChange change)
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: OnRecordingsChanged() ");
      m_bRecordingsChanged = true;
    }

    /// <summary>
    /// TVGuide has been updated, flag for a rescan
    /// </summary>
    void OnProgramsChanged()
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: OnProgramsChanged() ");
      m_bProgramsChanged = true;
    }

    /// <summary>
    /// Check every X seconds if time to do something 
    /// </summary>
    private void OnTimer(Object sender, EventArgs e)
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: OnTimer() ");

      if (m_bExtensiveLog) Log.Write(" PowerScheduler: Active window {0}, {1}, {2}", GUIWindowManager.ActiveWindow, m_iActiveWindow, m_bShutdownEnabled);

      // If it's been more than 25 secs since last time this method was called
      // assume system has been asleep 
      if (DateTime.Now.Subtract(m_dtLastTimercheck).TotalSeconds > 25)
      {
        Log.Write("PowerScheduler: System powerup detected ");

        // start recorder if needed
        Recorder.Start();
        
        LoadSettings();  // Settings may changed by Mediaportal.cs

        m_bShutdownEnabled = false;
        ResetShutdownTimer(m_iShutdownInterval);
      }
      m_dtLastTimercheck = DateTime.Now;

      // Manages when to wake up
      WakeupManager();

      // manages when to shutdown
      ShutdownManager();
    }

    #endregion

    # region WakeupManager
    /// <summary>
    ///  Handles setting the wakeup
    /// </summary>
    private void WakeupManager()
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: WakeupManager() ");

      DateTime earliestStarttime = DateTime.Now.AddMinutes(m_iStartupInterval + 1);
      DateTime nextStarttime = DateTime.MinValue;
      DateTime tmpNextStarttime = new DateTime();
      String pluginname = "";

      ArrayList wakeables = PluginManager.WakeablePlugins;

      foreach (IWakeable wakeable in wakeables)
      {
        pluginname = wakeable.PluginName();
        tmpNextStarttime = wakeable.GetNextEvent(earliestStarttime);
        if (m_bExtensiveLog) Log.Write(" PowerScheduler: tmpNextStarttime {0} earliest {1} - {2}", tmpNextStarttime, earliestStarttime, wakeable.PluginName());

        if (tmpNextStarttime.Ticks > earliestStarttime.Ticks)
        {
          if ((tmpNextStarttime.Ticks < nextStarttime.Ticks) || nextStarttime == DateTime.MinValue)
          {
            pluginname = wakeable.PluginName();
            nextStarttime = new DateTime(tmpNextStarttime.Ticks);
            if (m_bExtensiveLog) Log.Write(" PowerScheduler: found new, next scheduled event {0} triggered by: {1} ", nextStarttime, pluginname);
          }
        }
      }
      SetPowerUpTimer(nextStarttime, pluginname);
    }

    /// <summary>
    /// This function looks at pending recordings and set the wakeup timer
    /// to next recording which starttime is in the future. 
    /// </summary>
    static private void SetPowerUpTimer(DateTime nextStart, String pluginname)
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: SetPowerUpTimer() ");

      if (m_iStartupInterval == 0)
      {
        m_Timer.SecondsToWait = -1;
        return;
      }

      // if Starttime differs set new starttime
      if (nextStart.Ticks != m_iCurrentStart)
      {
        m_iCurrentStart = nextStart.Ticks;

        // make sure the timer is set to a (future) pending recording
        if (nextStart.CompareTo(DateTime.Now) > 0)
        {
          //Log.Write ("PowerScheduler: next scheduled recording starttime: {0} ", nextStart);
          Log.Write("PowerScheduler: next scheduled event {0} triggered by: {1} ", nextStart, pluginname);

          // calculate when to set wakeup timer
          nextStart = nextStart.AddMinutes(-m_iStartupInterval);

          // convert to seconds and set the timer
          TimeSpan tDelta = nextStart.Subtract(DateTime.Now);
          m_Timer.SecondsToWait = tDelta.TotalSeconds;
          Log.Write("PowerScheduler: Set wakeup timer at {0}", nextStart);
        }
        else
        {	// disable timer
          m_Timer.SecondsToWait = -1;
          //Log.Write("PowerScheduler: No pending recordings scheduled, disable wakeup timer (might be pending recordings too near in time to use wakeup)");
          Log.Write("PowerScheduler: No pending events scheduled, disable wakeup timer (might be pending events too near in time to use wakeup)");
        }
      }
      else
      {
        //Log.Write("PowerScheduler: Nothing to change");
      }
    }

    #endregion

    #region ShutdownManager
    /// <summary>
    ///  Handles shutdown, hibernate, stand by 
    /// </summary>
    private void ShutdownManager()
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: ShutdownManager() ");

      // when the active window has changed check if 
      // to enable or disable shutdown
      if (m_iActiveWindow != (int)GUIWindowManager.ActiveWindow)
      {
        m_iActiveWindow = GUIWindowManager.ActiveWindow;

        if (m_iActiveWindow == (int)GUIWindow.Window.WINDOW_HOME)
        {
          if (m_bExtensiveLog) Log.Write(" PowerScheduler: ShutdownManager - in HOME Window ");
          bool enableShutdown = true;
          //are we playing something?
          if ((g_Player.Playing && g_Player.IsRadio) ||  //are we playing internet radio?
              (Recorder.IsRadio())                   ||  //are we playing analog or digital radio?    
              (Recorder.IsRecording()) )                 //are we recording something? 
          {
            //yes -> then disable shutdown
            if (m_bExtensiveLog) Log.Write(" PowerScheduler: shutdown disabled - we are playing something");
            ResetShutdownTimer(0);
            enableShutdown = false;
            m_iActiveWindow = -1; //check again next time
          }

          if (!m_bShutdownEnabled && enableShutdown)
          {
            // Entered HOME - enable shutdown
            ResetShutdownTimer(m_iShutdownInterval);

            // disable viewing and timeshifting when recording and at HOME
            if (Recorder.IsAnyCardRecording())
            {
              Log.Write("PowerScheduler: Turn off timeshifting ");
              Recorder.StopViewing();
            }
          }
        }
        else
        {
          if (m_bShutdownEnabled)
          {
            // Left HOME - disable shutdown
            ResetShutdownTimer(0);
          }
        }
      }
      ShutdownCheck();	// check if time to shutdown
    }

    /// <summary>
    ///  Performs shutdown, hibernate, stand by when it's time
    /// </summary>
    private void ShutdownCheck()
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: ShutdownCheck() ");

      TimeSpan tDelta = m_dtShutdownTime.Subtract(DateTime.Now);
      if (m_bShutdownEnabled && (DateTime.Now.CompareTo(m_dtShutdownTime) > 0))
      {
        if (m_bExtensiveLog) Log.Write("PowerScheduler: Shutdown timer expired");

        bool abortshutdown = false;
        ArrayList wakeables = PluginManager.WakeablePlugins;
        foreach (IWakeable wakeable in wakeables)
        {
          if (wakeable.DisallowShutdown())
          {
            Log.Write("PowerScheduler: Shutdown process aborted by module - {0}", wakeable.PluginName());
            abortshutdown = true;
          }
        }
        if (abortshutdown)
          return;

        //g_Player.Stop();
        //AutoPlay.StopListening();
        //GUIWindowManager.Dispose();
        //GUITextureManager.Dispose();
        //GUIFontManager.Dispose();
        ResetShutdownTimer(0);

        if (m_shutdownMode.StartsWith("None"))
        {
          Log.Write("PowerScheduler: No shutdown");
        }

        if (m_shutdownMode.StartsWith("Suspend"))
        {
          Log.Write("PowerScheduler: Suspend system");
          Utils.SuspendSystem(m_bForceShutdown);
          //WindowsController.ExitWindows(RestartOptions.Suspend, m_bForceShutdown);
        }

        if (m_shutdownMode.StartsWith("Hibernate"))
        {
          Log.Write("PowerScheduler: Hibernate system");
          Utils.HibernateSystem(m_bForceShutdown);
          //WindowsController.ExitWindows(RestartOptions.Hibernate, m_bForceShutdown);
        }

        if (m_shutdownMode.StartsWith("Shutdown"))
        {
          AutoPlay.StopListening();
          WindowsController.ExitWindows(RestartOptions.ShutDown, m_bForceShutdown);
        }
      }
    }

    /// <summary>
    /// Reset the shutdown timer
    /// </summary>
    static void ResetShutdownTimer(int aMinutes)
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: ResetShutdownTimer() ");

      if (aMinutes == 0)
      {	// shutdown disabled (set shutdown time 1 year into the future)
        // - this is probably not the best solution but it works
        m_dtShutdownTime = DateTime.Now.AddYears(1);

        if (m_bShutdownEnabled)
        {
          Log.Write("PowerScheduler: Shutdown timer deactivated");
        }
        m_bShutdownEnabled = false;
      }
      else
      {
        // set shutdown time N minutes from now
        m_dtShutdownTime = DateTime.Now.AddMinutes(aMinutes);
        if (!m_bShutdownEnabled)
        {
          Log.Write("PowerScheduler: Shutdown timer activated, automatic shutdown in {0} minutes", m_iShutdownInterval);
        }
        m_bShutdownEnabled = true;
      }
    }

    #endregion

    #region Settings

    void LoadSettings()
    {
      Log.Write("PowerScheduler: version 0.3");

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        m_iStartupInterval = xmlreader.GetValueAsInt("powerscheduler", "wakeupinterval", 1);
        m_iShutdownInterval = xmlreader.GetValueAsInt("powerscheduler", "shutdowninterval", 3);
        m_shutdownMode = xmlreader.GetValueAsString("powerscheduler", "shutdownmode", "Suspend");
        m_bExtensiveLog = xmlreader.GetValueAsBool("powerscheduler", "extensivelogging", false);
        m_bForceShutdown = xmlreader.GetValueAsBool("powerscheduler", "forcedshutdown", false);

        m_bDisabled = xmlreader.GetValueAsString("plugins", "Power Scheduler", "no") == "no";

        if (m_bDisabled)
        {
          Log.Write("PowerScheduler: Disabled");
        }
        else
        {
          if (m_bForceShutdown)
          {
            Log.Write("PowerScheduler: Settings loaded - wakeup {0}, shutdown {1}, mode {2} - Forced", m_iStartupInterval, m_iShutdownInterval, m_shutdownMode);
          }
          else
          {
            Log.Write("PowerScheduler: Settings loaded - wakeup {0}, shutdown {1}, mode {2}", m_iStartupInterval, m_iShutdownInterval, m_shutdownMode);
          }
          if (m_bExtensiveLog) Log.Write("PowerScheduler: Extensive logging");
          if (m_iStartupInterval == 0) Log.Write("PowerScheduler: Wakeup from hibernate/standby - disabled");
          if (m_iShutdownInterval == 0) Log.Write("PowerScheduler: Shutdown on idle - disabled");
        }

        m_iPreRecordInterval = xmlreader.GetValueAsInt("capture", "prerecord", 0);
      }
    }
    #endregion

    #region Interfacing TVdatabase and TVrecorder

    /// <summary>
    /// This function gets the starttime of the next pending recording
    /// </summary>
    static private DateTime GetNextRecordingStarttime(DateTime earliestStarttime)
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: GetNextRecordingStarttime() ");
      if (m_bExtensiveLog) Log.Write(" PowerScheduler:  Earliest valid starttime {0}", earliestStarttime);

      DateTime nextStarttime = DateTime.MinValue;

      // get starttime of the first recording which is the next to due
      // care not to set it to the current recording (due in a minute or so) 
      ArrayList recordings = new ArrayList();
      ArrayList TVGuideRecordings = new ArrayList();

      recordings.Clear();
      TVGuideRecordings.Clear();

      if (TVDatabase.GetRecordings(ref recordings))
      {
        foreach (TVRecording recording in recordings)
        {
          DateTime tmpNextStarttime = new DateTime();

          //if recording has been canceled, then skip it
          if (recording.Canceled > 0)
          {
            continue;
          }

          //check starttime
          switch (recording.RecType)
          {
            case (TVRecording.RecordingType.Once):
              {
                tmpNextStarttime = recording.StartTime.AddMinutes(-m_iPreRecordInterval);
                if (m_bExtensiveLog)
                  Log.Write(" PowerScheduler:  Next date/time:{0} Type:Once      {1}  {2} ",
                            tmpNextStarttime, recording.Channel, recording.Title);
                break;
              }
            case (TVRecording.RecordingType.Daily):
              {
                tmpNextStarttime = recording.StartTime.AddMinutes(-m_iPreRecordInterval);
                double days = -tmpNextStarttime.Subtract(earliestStarttime).TotalDays;
                tmpNextStarttime = tmpNextStarttime.AddDays(Math.Round(days, 0) + 1);
                if (m_bExtensiveLog)
                  Log.Write(" PowerScheduler:  Next date/time:{0} Type:Daily     {1}  {2} ",
                    tmpNextStarttime, recording.Channel, recording.Title);
                break;
              }
            case (TVRecording.RecordingType.WeekDays):
              {
                tmpNextStarttime = recording.StartTime.AddMinutes(-m_iPreRecordInterval);
                DateTime nextWeekDay = DateTime.Now;

                // Skip Weekend
                while (nextWeekDay.DayOfWeek == System.DayOfWeek.Saturday ||
                       nextWeekDay.DayOfWeek == System.DayOfWeek.Sunday)
                {
                  nextWeekDay = nextWeekDay.AddDays(1);
                }
                tmpNextStarttime = new DateTime(nextWeekDay.Year, nextWeekDay.Month, nextWeekDay.Day,
                                                tmpNextStarttime.Hour, tmpNextStarttime.Minute, tmpNextStarttime.Second, 0);

                if (m_bExtensiveLog)
                  Log.Write(" PowerScheduler:  Next date/time:{0} Type:Weekdays  {1}  {2} ",
                    tmpNextStarttime, recording.Channel, recording.Title);

                break;
              }
            case (TVRecording.RecordingType.WeekEnds):
              {
                tmpNextStarttime = recording.StartTime.AddMinutes(-m_iPreRecordInterval);
                DateTime nextWeekDay = DateTime.Now;

                // Skip Weekend
                while (nextWeekDay.DayOfWeek != System.DayOfWeek.Saturday &&
                             nextWeekDay.DayOfWeek != System.DayOfWeek.Sunday)
                {
                  nextWeekDay = nextWeekDay.AddDays(1);
                }
                tmpNextStarttime = new DateTime(nextWeekDay.Year, nextWeekDay.Month, nextWeekDay.Day,
                                                                              tmpNextStarttime.Hour, tmpNextStarttime.Minute, tmpNextStarttime.Second, 0);

                if (m_bExtensiveLog)
                  Log.Write(" PowerScheduler:  Next date/time:{0} Type:WeekEnds  {1}  {2} ",
                      tmpNextStarttime, recording.Channel, recording.Title);

                break;
              }
            case (TVRecording.RecordingType.Weekly):
              {
                tmpNextStarttime = recording.StartTime.AddMinutes(-m_iPreRecordInterval);
                DateTime nextWeekDay = DateTime.Now;
                while (nextWeekDay.DayOfWeek != tmpNextStarttime.DayOfWeek)
                {
                  nextWeekDay = nextWeekDay.AddDays(1);
                }

                tmpNextStarttime = new DateTime(nextWeekDay.Year, nextWeekDay.Month, nextWeekDay.Day,
                                                tmpNextStarttime.Hour, tmpNextStarttime.Minute, tmpNextStarttime.Second, 0);

                if (m_bExtensiveLog)
                  Log.Write(" PowerScheduler:  Next date/time:{0} Type:Weekly   {1}  {2} ",
                    tmpNextStarttime, recording.Channel, recording.Title);
                break;
              }
            case (TVRecording.RecordingType.EveryTimeOnEveryChannel):
              {
                TVGuideRecordings.Add(recording);
                if (m_bExtensiveLog)
                  Log.Write(" PowerScheduler:  Next date/time:{0} Type:Every1   {1}  {2} ",
                    tmpNextStarttime, recording.Channel, recording.Title);
                break;
              }
            case (TVRecording.RecordingType.EveryTimeOnThisChannel):
              {
                TVGuideRecordings.Add(recording);
                if (m_bExtensiveLog)
                  Log.Write(" PowerScheduler:  Next date/time:{0} Type:Every2   {1}  {2} ",
                    tmpNextStarttime, recording.Channel, recording.Title);
                break;
              }
          }

          if (tmpNextStarttime.Ticks > earliestStarttime.Ticks)
            if ((tmpNextStarttime.Ticks < nextStarttime.Ticks) || nextStarttime == DateTime.MinValue)
              nextStarttime = new DateTime(tmpNextStarttime.Ticks);

        }

        if (TVGuideRecordings.Count > 0)
        {
          if (m_bExtensiveLog)
            Log.Write(" PowerScheduler:  Evaluate TVGuide recordings ");

          ArrayList tvPrograms = new ArrayList();
          bool programfound = false;
          DateTime tmpNextStarttime = DateTime.MinValue;
          DateTime tvguideStartDatetime = new DateTime();
          tvPrograms.Clear();

          // look in the tv guide betweeen earliestStarttime and maximum one month into the future
          // shorter if we already have found a calendric recording
          if (nextStarttime != DateTime.MinValue)
          {
            tvguideStartDatetime = nextStarttime;
          }
          else
          {
            tvguideStartDatetime = DateTime.Now.AddMonths(1);
          }

          if (TVDatabase.GetPrograms(Utils.datetolong(earliestStarttime), Utils.datetolong(tvguideStartDatetime), ref tvPrograms))
          {
            foreach (TVProgram program in tvPrograms)
            {
              // make sure we're only looking at programs from the future
              if (program.StartTime.AddMinutes(-m_iPreRecordInterval).Ticks > earliestStarttime.Ticks)
              {
                foreach (TVRecording rec in TVGuideRecordings)
                {

                  switch (rec.RecType)
                  {
                    case (TVRecording.RecordingType.EveryTimeOnEveryChannel):
                      {
                        //tmpNextStarttime = program.StartTime.AddMinutes(- m_iPreRecordInterval);
                        //if (tmpNextStarttime.Ticks > earliestStarttime.Ticks)
                        if (program.Title == rec.Title)
                        {
                          tmpNextStarttime = program.StartTime.AddMinutes(-m_iPreRecordInterval);
                          programfound = true;
                          if (m_bExtensiveLog) Log.Write(" PowerScheduler:  TVGuide {0} {1} {2} ", program.Title, program.Channel, tmpNextStarttime);
                        }
                        break;
                      }
                    case (TVRecording.RecordingType.EveryTimeOnThisChannel):
                      {
                        if (program.Title == rec.Title && program.Channel == rec.Channel)
                        {
                          tmpNextStarttime = program.StartTime.AddMinutes(-m_iPreRecordInterval);
                          programfound = true;
                          if (m_bExtensiveLog)
                            Log.Write(" PowerScheduler:  TVGuide {0} {1} {2} ", program.Title, program.Channel, tmpNextStarttime);
                        }
                        break;
                      }
                  }
                  if (programfound)
                    break;
                }
              }
              if (programfound)
                break;
            }
          }
          if (tmpNextStarttime.Ticks > earliestStarttime.Ticks)
            if ((tmpNextStarttime.Ticks < nextStarttime.Ticks) || nextStarttime == DateTime.MinValue)
              nextStarttime = new DateTime(tmpNextStarttime.Ticks);
        }
      }
      if (m_bExtensiveLog) Log.Write(" PowerScheduler:  GetNextRecordingStarttime() starttime {0} ", nextStarttime);
      return nextStarttime;
    }

    /// <summary>
    /// Check prior to shutdown, if shutdown is canceled by any 
    /// ongoing or pending recording. 
    /// </summary>
    private bool PreShutdownCheck()
    {
      if (m_bExtensiveLog) Log.Write(" PowerScheduler: PreShutdownCheck() ");

      if (GUIWindowManager.ActiveWindow != 0)
      {
        Log.Write("PowerScheduler: Shutdown process aborted - home is not the active window");
        ResetShutdownTimer(0);
        return false;
      }

      if (Recorder.IsAnyCardRecording())
      {
        if (m_bFirstLogRec)
        {
          Log.Write("PowerScheduler: Shutdown process aborted - TVrecording in progress");
        }
        else
        {
          if (m_bExtensiveLog) Log.Write("PowerScheduler: Shutdown process aborted - TVrecording in progress");
        }
        m_bFirstLogRec = false;

        ResetShutdownTimer(m_iShutdownInterval);
        return false;
      }
      else
        m_bFirstLogRec = true;

      // Find the next pending recording
      // If it's due in less than the "ShutdownInterval" cancel shutdown
      DateTime nextStart = GetNextRecordingStarttime(DateTime.Now);

      if (nextStart.CompareTo(DateTime.Now) > 0)
      {
        TimeSpan tDelta = nextStart.Subtract(DateTime.Now);

        // use same interval as "idle minutes before shutdown" to decide if it's ok
        // to shutdown before the next recording.
        int compvar = (int)Convert.ChangeType(tDelta.TotalMinutes, typeof(int));
        if (compvar <= m_iShutdownInterval)
        {
          Log.Write("PowerScheduler: Shutdown process aborted - pending recording within {0} minutes", tDelta.Minutes);
          ResetShutdownTimer(m_iShutdownInterval);
          return false;
        }
      }
      return true;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "Power Scheduler";
    }

    public bool HasSetup()
    {
      return true;
    }
    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return WINDOW_POWERSCHEDULER;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = "Power Scheduler";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public string Author()
    {
      return "Fred";
    }

    public string Description()
    {
      return "Power manager for standby, hibernate, etc.";
    }

    public void ShowPlugin() // show the setup dialog
    {
      System.Windows.Forms.Form setup = new MediaPortal.PowerScheduler.PowerSchedulerSetupForm();
      setup.ShowDialog();
    }


    #endregion

    #region IWakeable Members

    public bool DisallowShutdown()
    {
      return !PreShutdownCheck();
    }

    public DateTime GetNextEvent(DateTime earliestWakeuptime)
    {	// if something has changed or the wakeup time is past recalculate
      if (m_bProgramsChanged || m_bRecordingsChanged || m_bResetWakeuptime || (earliestWakeuptime.Ticks > m_iCurrentStart))
      {
        // reset the flags
        m_bProgramsChanged = false;
        m_bRecordingsChanged = false;
        m_bResetWakeuptime = false;

        if (m_bRecordingsChanged)
        {
          Log.Write("PowerScheduler: Recordings has changed - rescan recordings ");
        }
        if (m_bProgramsChanged)
        {
          Log.Write("PowerScheduler: TVguide has been updated - recalculate recordings");
        }
        return GetNextRecordingStarttime(earliestWakeuptime.AddMinutes(m_iPreRecordInterval));
      }
      else
      {
        return new DateTime(m_iCurrentStart);
      }
    }

    #endregion
  }
}
