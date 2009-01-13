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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace MediaPortal.PowerScheduler
{
  [PluginIcons("ProcessPlugins.PowerScheduler.PowerScheduler.gif",
    "ProcessPlugins.PowerScheduler.PowerScheduler_disabled.gif")]
  public class PowerScheduler : IPluginReceiver, IWakeable, ISetupForm
  {
    #region Variables

    #region Private Variables

    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMQUERYSUSPEND = 0x0000;
    private const int PBT_APMQUERYSTANDBY = 0x0001;
    private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
    private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
    private const int PBT_APMSUSPEND = 0x0004;
    private const int PBT_APMSTANDBY = 0x0005;
    private const int PBT_APMRESUMECRITICAL = 0x0006;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    private const int PBT_APMBATTERYLOW = 0x0009;
    private const int PBT_APMPOWERSTATUSCHANGE = 0x000A;
    private const int PBT_APMOEMEVENT = 0x000B;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    private const string _version = "v0.1.1";

    #endregion

    #region Protected Variables

    // Shut Down Variables
    protected int _shutDownInterval = 2; // idle minutes before shutdown(hibernate/standby)
    protected DateTime _shutDownTime = DateTime.MaxValue; // DateTime for the next shutDown
    protected string _shutDownMode = "None"; // mode to use for shutdown (Hibernate, Suspend, Shutdown, None)
    protected bool _forceShutDown = false; // force shutdown

    protected bool _lastShutDownDisabled = false;
                   // ShutDown was last time disabled by a plugin -> recheck to start ShutDown once again

    // Timer Variables
    protected Timer _Timer = new Timer();
    protected int _timerInterval = 30; // intervall after the timer procedure gets called calls    
    protected bool _rescanTVDatabase = false; // true if the TVDatabase got some changes
    protected bool _extensiveLogging = false; // show more details on each OnTimer() call
    // Wake Up Variables
    private WaitableTimer _wakeupTimer = new WaitableTimer();
    protected DateTime _wakeupTime = DateTime.MaxValue; // Date/Time for the next wakeup
    protected int _preRecordInterval = 2; // Interval to start recording before entered starttime
    protected int _wakeupInterval = 1; // 1 minute to give computer time to wakeup
    protected bool _reinitRecorder = false; // Re-init the Recorder when resuming
    protected bool _onResumeRunning = false; // avoid multible OnResume calls
    // TV Guide Variables
    protected DateTime _nextRecordingTime = DateTime.MaxValue; // Date/Time when the next recording takes place
    // Power Saving Options
    protected bool _preventMonitorPowerOff = true;

    #endregion

    // Public Variables

    #endregion

    #region Properties

    // Public Properties
    public DateTime EarliestStartTime
    {
      get { return DateTime.Now; }
    }

    #endregion

    #region events

    /// <summary>
    /// Recordings has been changed, flag for a rescan
    /// </summary>
    private void OnRecordingsChanged(TVDatabase.RecordingChange change)
    {
      _rescanTVDatabase = true;
    }

    /// <summary>
    /// Programs has been changed, flag for a rescan
    /// </summary>
    private void OnProgramsChanged()
    {
      _rescanTVDatabase = true;
    }

    private void OnTvRecordingStarted(string recordingFilename, TVRecording recording, TVProgram program)
    {
      _rescanTVDatabase = true;
    }

    #endregion

    #region Constructors/Destructors

    public PowerScheduler()
    {
      LogExtensive("Init");
      LoadSettings();
      PowerManager.OnPowerUp += new PowerManager.ResumeHandler(this.OnWakeupTimer);
      _wakeupTimer.OnTimerExpired += new WaitableTimer.TimerExpiredHandler(PowerManager.OnResume);
      GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(OnActivateWindow);
      TVDatabase.OnRecordingsChanged += new TVDatabase.OnRecordingChangedHandler(this.OnRecordingsChanged);
      TVDatabase.OnProgramsChanged += new TVDatabase.OnChangedHandler(this.OnProgramsChanged);
      Recorder.OnTvRecordingStarted += new Recorder.OnTvRecordingHandler(this.OnTvRecordingStarted);
    }

    #endregion

    #region Private Methods

    #region Settings

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _shutDownInterval = xmlreader.GetValueAsInt("powerscheduler", "shutdowninterval", 0);
        _shutDownMode = xmlreader.GetValueAsString("powerscheduler", "shutdownmode", "Suspend").ToLower();
        _forceShutDown = xmlreader.GetValueAsBool("powerscheduler", "forcedshutdown", false);
        _extensiveLogging = xmlreader.GetValueAsBool("powerscheduler", "extensivelogging", false);

        _timerInterval = xmlreader.GetValueAsInt("powerscheduler", "timerinterval", 30);
        _wakeupInterval = xmlreader.GetValueAsInt("powerscheduler", "wakeupinterval", 1);
        _reinitRecorder = xmlreader.GetValueAsBool("powerscheduler", "reinitonresume", false);

        _preventMonitorPowerOff = xmlreader.GetValueAsBool("powerscheduler", "preventmonitorpowerdow", false);

        if (_shutDownInterval < 1)
        {
          ResetShutDown(); // be sure that we do no shutdown
        }

        if (_shutDownInterval > 0)
        {
          if (_wakeupInterval >= _shutDownInterval)
          {
            _shutDownInterval = _wakeupInterval + 1;
              // ensure that shutDownIntervall is longer then wakeUpIntervall -> to avoid shutdown before recordings starts        
          }
        }

        if (_extensiveLogging)
        {
          Log.Info("Powerscheduler: LoadSettings");
          Log.Info("   - ShutDownInterval = {0}", _shutDownInterval);
          Log.Info("   - ShutDownMode     = {0}", _shutDownMode);
          Log.Info("   - ForceShutDown    = {0}", _forceShutDown);
          Log.Info("   - TimerInterval    = {0}", _timerInterval);
          Log.Info("   - WakeUpInterval   = {0}", _wakeupInterval);
          Log.Info("   - ReinitRecorder   = {0}", _reinitRecorder);
        }
      }
    }

    #endregion

    #region ShutDown Timer routines

    /// <summary>
    /// This callback gets called by the window manager when user switches to another window (plugin)
    /// When the users enters the Home plugin we want to enable the shut down counter.
    /// When the user leaves the Home plugin then the shut down counter should be disabled
    /// </summary>
    /// <param name="windowId">id of the window which is about to be activated</param
    private void OnActivateWindow(int windowId)
    {
      if ((windowId == (int) GUIWindow.Window.WINDOW_HOME) ||
          (windowId == (int) GUIWindow.Window.WINDOW_SECOND_HOME))
      {
        if (_shutDownInterval > 0)
        {
          SetShutDown(); // we are switching to home
        }

        if (_preventMonitorPowerOff)
        {
          Win32API.AllowMonitorPowerdown(); // Turn on Energy Saving Options
        }
      }
      else
      {
        if (_shutDownInterval > 0)
        {
          ResetShutDown();
        }

        Win32API.PreventMonitorPowerdown(); // Turn off Energy Saving Options
      }


      /*
       *  Hwahrmann: leave in, until the above changes are confirmed 
       *
      if (_shutDownInterval > 0)
      {
        if ((windowId == (int)GUIWindow.Window.WINDOW_HOME) ||
                (windowId == (int)GUIWindow.Window.WINDOW_SECOND_HOME))
        {
          // we are switching to home
          SetShutDown();
        }
        else
        {
          ResetShutDown();
        }
      }
      */
    }

    /// <summary>
    /// This routine sets the ShutDown Time. 
    /// !!!! DO NOT call it directly !!!!
    /// Always call  ->  OnActivateWindow(GUIWindowManager.ActiveWindow); 
    /// </summary>
    private void SetShutDown()
    {
      _shutDownTime = DateTime.Now.AddMinutes(_shutDownInterval);
      LogExtensive("Next ShutDown in {0} minutes -> " + _shutDownTime.ToLongTimeString(), _shutDownInterval);
    }

    private void ResetShutDown()
    {
      if (_shutDownTime == DateTime.MaxValue)
      {
        return; // no display message
      }
      LogExtensive("ShutDownTimer disabled");
      _shutDownTime = DateTime.MaxValue;
    }

    private void StartTimer()
    {
      if (!_Timer.Enabled)
      {
        _Timer.Interval = _timerInterval*1000; // interval in milliseconds
        _Timer.Tick += new EventHandler(OnTimer);
        _Timer.Start();
        //LogExtensive("Timer started");
      }
    }

    private void StopTimer()
    {
      if (_Timer.Enabled)
      {
        _Timer.Stop();
        _Timer.Tick -= new EventHandler(OnTimer);
        //LogExtensive("Timer stopped");
      }
    }

    private void OnTimer(Object sender, EventArgs e)
    {
      PowerManager.SystemBusy(); // must be called periodically to tell windows that the system is required

      StopTimer();
      if (_extensiveLogging)
      {
        Log.Info("-- OnTimerCall -- ");
        Log.Info("   - Recorder.IsAnyCardRecording() = " + Recorder.IsAnyCardRecording().ToString());
        Log.Info("   - Recorder.IsRadio()     = " + Recorder.IsRadio().ToString());
        Log.Info("   - g_Player.Playing       = " + g_Player.Playing.ToString());
        Log.Info("   - Database Update        = " + TVDatabase.SupressEvents.ToString());
        Log.Info("   - ShutDown Time          = " + _shutDownTime.ToString());
        Log.Info("   - WakeUp Time            = " + _wakeupTime.ToString());
      }

      if (_lastShutDownDisabled) // last Time ShutDown was disabled by a plugin -> recheck
      {
        if (!WakeAbleDisallowShutdown())
        {
          _lastShutDownDisabled = false;
          OnActivateWindow(GUIWindowManager.ActiveWindow); // SetShutDown is called when needed
        }
      }

      if ((_wakeupTime < DateTime.Now) ||
          ((_rescanTVDatabase) && (!TVDatabase.SupressEvents)))
      {
        ResetShutDown(); // ensure that the assumptions about EarliestStartTime are true.
        CheckNextRecoring(); // checks when the next recording takes place 
        //       SetWakeUpTime();                                  // set the WakeUp Timer
        _rescanTVDatabase = false;
        OnActivateWindow(GUIWindowManager.ActiveWindow); // SetShutDown is called when needed 
      }

      SetWakeUpTime();
      //OnActivateWindow(GUIWindowManager.ActiveWindow);  // SetShutDown is called when needed 

      if (_shutDownTime > DateTime.Now)
      {
        StartTimer();
        return;
      }

      if (WakeAbleDisallowShutdown())
      {
        //Log.Info("Shutdown process aborted by plugin - {0}", wakeable.PluginName());
        //SetShutDown();  // next try
        _lastShutDownDisabled = true;
        StartTimer();
        return;
      }
      ShutDown();
    }

    protected bool WakeAbleDisallowShutdown()
    {
      ArrayList wakeables = PluginManager.WakeablePlugins;
      foreach (IWakeable wakeable in wakeables)
      {
        if (wakeable.DisallowShutdown())
        {
          Log.Info("Shutdown process aborted by plugin - {0}", wakeable.PluginName());
          return true;
        }
      }
      return false;
    }


    protected void ShutDown()
    {
      switch (_shutDownMode.ToLower())
      {
        case "suspend":
          Log.Info("PowerScheduler: Suspend system -> WakeUp at {0}", _wakeupTime);
          Util.Utils.SuspendSystem(_forceShutDown);
          break;
        case "hibernate":
          Log.Info("PowerScheduler: Hibernate system -> WakeUp at {0}", _wakeupTime);
          Util.Utils.HibernateSystem(_forceShutDown);
          break;
        case "shutdown (no wakeup)":
          Log.Info("PowerScheduler: ShutDown system");
          WindowsController.ExitWindows(RestartOptions.ShutDown, _forceShutDown);
          break;
      }
    }

    #endregion

    #region Logging

    private void LogExtensive(string format, params object[] arg)
    {
      if (_extensiveLogging)
      {
        StackTrace st = new StackTrace();
        StackFrame sf = st.GetFrame(1);
        Log.Info(PluginName() + "." + sf.GetMethod().Name + ": " + format, arg);
      }
    }

    #endregion

    #region CheckNextRecording

    private void CheckNextRecoring()
    {
      DateTime ealiestStartTime = EarliestStartTime;
      if (_shutDownTime < DateTime.MaxValue)
      {
        ealiestStartTime = _shutDownTime;
      }
      _nextRecordingTime = DateTime.MaxValue;
      DateTime nextRecTime = DateTime.MaxValue;
      TVRecording nextRec = null;
      ArrayList recList = new ArrayList();

      if (TVDatabase.GetRecordings(ref recList))
      {
        foreach (TVRecording rec in recList)
        {
          if ((rec.Canceled > 0) || (rec.IsDone()))
          {
            continue;
          }
          LogExtensive("Recording: " + rec.ToString());
          List<TVRecording> recs = ConflictManager.Util.GetRecordingTimes(rec);
          foreach (TVRecording foundRec in recs)
          {
            DateTime startTime = foundRec.StartTime.AddMinutes(-foundRec.PreRecord);
            if ((startTime >= ealiestStartTime) && (startTime < nextRecTime))
            {
              nextRecTime = startTime;
              nextRec = new TVRecording(foundRec);
            }
          }
        }
      }
      if ((nextRecTime < DateTime.MaxValue) && (nextRecTime > ealiestStartTime))
      {
        _nextRecordingTime = nextRecTime;
        LogExtensive("Next Recording found at StartTime = {0}, {1} - {2}", nextRecTime, nextRec.Channel, nextRec.Title);
      }
    }

    #endregion

    #region WakeUp/resume routines

    private void SetWakeUpTime()
    {
      DateTime nextWakeUpTime = DateTime.MaxValue;
      DateTime ealiestStartTime = EarliestStartTime;
      ArrayList wakeables = PluginManager.WakeablePlugins;
      foreach (IWakeable wakeable in wakeables)
      {
        DateTime pluginTime = wakeable.GetNextEvent(ealiestStartTime);
        if ((pluginTime >= ealiestStartTime) && (pluginTime < nextWakeUpTime))
        {
          nextWakeUpTime = pluginTime;
        }
        LogExtensive("Plugin: {0},  Time: {1}", wakeable.PluginName(), pluginTime);
      }

      if ((nextWakeUpTime > DateTime.Now) && (nextWakeUpTime < DateTime.Now.AddMonths(3)))
      {
        if (nextWakeUpTime != _wakeupTime) // only set it when it is different
        {
          LogExtensive("WakeUp Timer set to {0}", nextWakeUpTime);
          _wakeupTime = nextWakeUpTime;
          TimeSpan tDelta = _wakeupTime.Subtract(DateTime.Now);
          _wakeupTimer.SecondsToWait = tDelta.TotalSeconds;
          LogExtensive("    ==> Seconds to wakeup = {0}", tDelta.TotalSeconds);
        }
      }
      else
      {
        LogExtensive("WakeUp Timer deactivated (no valid Time found)");
        _wakeupTime = DateTime.MaxValue;
        _wakeupTimer.SecondsToWait = -1;
      }
    }

    /// <summary>
    /// The wakeup timer has expired
    /// </summary>
    private void OnWakeupTimer()
    {
      LogExtensive("Wakeup timer expired");
      /* undo because of OnWakeUpTimer calls when user is not in home for the recording
       * if (_Timer.Enabled)  // did we shut down correctly?
       {
         Log.Info("PowerScheduler: did not shut down correctly -> resync");
         OnSuspend();
       }
       
       */
    }

    private void OnSuspend()
    {
      LogExtensive("OnSuspend->StopTimer");
      StopTimer();
      _onResumeRunning = false;
    }

    private void OnResume()
    {
      LogExtensive("OnResume");
      if (_reinitRecorder)
      {
        if (Recorder.IsAnyCardRecording())
        {
          LogExtensive("Reinit Recorder cancled due to running Recording");
        }
        else
        {
          LogExtensive("Reinit Recorder -> Start");
          Recorder.Stop();
          Recorder.Start();
          LogExtensive("Reinit Recorder -> Done");
        }
      }
    }

    #endregion

    #endregion

    #region <Interface> Implementations

    #region IPluginReceiver Interface

    /// <summary>
    /// This method will be called by mediaportal to start your process plugin
    /// </summary>
    public void Start()
    {
      Log.Info(PluginName() + ".Start() - Version: " + _version);
      OnActivateWindow(GUIWindowManager.ActiveWindow); // SetShutDown is called when needed
      CheckNextRecoring(); // checks when the next recording takes place 
      SetWakeUpTime(); // sets the next wakeup time 
      StartTimer();
    }

    /// <summary>
    /// This method will be called by mediaportal to stop your process plugin
    /// </summary>
    public void Stop()
    {
      LogExtensive("Stopping");
      StopTimer();
      _wakeupTime = DateTime.MaxValue;
      _wakeupTimer.SecondsToWait = -1;
    }

    /// <summary>
    /// This method will be called by mediaportal to send system messages to your process plugin,
    /// if the plugin implements WndProc (optional) / added by mPod
    /// </summary>
    public bool WndProc(ref Message msg)
    {
      if (msg.Msg == WM_POWERBROADCAST)
      {
        LogExtensive("WM_POWERBROADCAST: {0}", msg.WParam.ToInt32());
        switch (msg.WParam.ToInt32())
        {
            //The PBT_APMQUERYSUSPEND message is sent to request permission to suspend the computer.
            //An application that grants permission should carry out preparations for the suspension before returning.
            //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSUSPEND:
            //The PBT_APMQUERYSTANDBY message is sent to request permission to suspend the computer.
            //An application that grants permission should carry out preparations for the suspension before returning.
            //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSTANDBY:
          case PBT_APMSUSPEND:
            OnSuspend();
            break;

            //The PBT_APMRESUMECRITICAL event is broadcast as a notification that the system has resumed operation. 
            //this event can indicate that some or all applications did not receive a PBT_APMSUSPEND event. 
            //For example, this event can be broadcast after a critical suspension caused by a failing battery.
          case PBT_APMRESUMECRITICAL:
            //The PBT_APMRESUMESUSPEND event is broadcast as a notification that the system has resumed operation after being suspended.
          case PBT_APMRESUMESUSPEND:
            //The PBT_APMRESUMESTANDBY event is broadcast as a notification that the system has resumed operation after being standbye.
          case PBT_APMRESUMESTANDBY:
            //The PBT_APMRESUMEAUTOMATIC event is broadcast when the computer wakes up automatically to
            //handle an event. An application will not generally respond unless it is handling the event, because the user is not present.
          case PBT_APMRESUMEAUTOMATIC:
            if (!_onResumeRunning)
            {
              _onResumeRunning = true;
              LogExtensive("OnResume->Start");
              OnResume();
              Start();
            }
            break;
        }
      }
      return false; // false = all other processes will handle the msg
    }

    #endregion

    #region IWakeable Interface

    public DateTime GetNextEvent(DateTime earliestWakeuptime)
    {
      if (_wakeupInterval < 1)
      {
        return DateTime.MaxValue; // function disabled
      }

      DateTime recordingTime = _nextRecordingTime.AddMinutes(-_wakeupInterval);
      if (recordingTime < earliestWakeuptime)
      {
        recordingTime = DateTime.MaxValue; // no recording planed
      }
      return recordingTime;
    }

    public bool DisallowShutdown()
    {
      if ((g_Player.Playing) || // are we playing something ? 
          (Recorder.IsRadio()) || // are we playing analog or digital radio?    
          (Recorder.IsAnyCardRecording()) || // are we recording something? 
          (TVDatabase.SupressEvents) || // is there a DataBase update running?
          (_shutDownTime > _wakeupTime)) // is shutdown killing the start of the recording
      {
        Log.Info(" PowerScheduler.DisallowShutdown() = TRUE");
        Log.Info("   - Recorder.IsAnyCardRecording() = " + Recorder.IsAnyCardRecording().ToString());
        Log.Info("   - Recorder.IsRadio()            = " + Recorder.IsRadio().ToString());
        Log.Info("   - g_Player.Playing              = " + g_Player.Playing.ToString());
        Log.Info("   - Database Update               = " + TVDatabase.SupressEvents.ToString());
        Log.Info("   - ShutDown Time                 = " + _shutDownTime.ToString());
        Log.Info("   - WakeUp Time                   = " + _wakeupTime.ToString());
        return true;
      }
      return false;
    }

    #endregion

    #region ISetupForm Interface

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "PowerScheduler";
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
      return 6039;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = "Power Scheduler";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public string Author()
    {
      return "Bavarian";
    }

    public string Description()
    {
      return "Power Scheduler for standby, hibernate, etc.";
    }

    public void ShowPlugin() // show the setup dialog
    {
      Form setup = new PowerSchedulerSetupForm();
      setup.ShowDialog();
    }

    #endregion

    #endregion
  }
}