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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Gentle.Framework;
using MediaPortal;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using TvLibrary.Implementations.DVB;
using TvLibrary.Interfaces;

#endregion

namespace TvPlugin
{
  /// <summary>
  /// TV Home screen.
  /// </summary>
  [PluginIcons("TvPlugin.TVPlugin.gif", "TvPlugin.TVPluginDisabled.gif")]
  public class TVHome : GUIInternalWindow, ISetupForm, IShowPlugin, IPluginReceiver
  {
    #region constants

    private const int HEARTBEAT_INTERVAL = 5; //seconds
    private const int MAX_WAIT_FOR_SERVER_CONNECTION = 10; //seconds
    private const int WM_POWERBROADCAST = 0x0218;
    private const int WM_QUERYENDSESSION = 0x0011;
    private const int PBT_APMQUERYSUSPEND = 0x0000;
    private const int PBT_APMQUERYSTANDBY = 0x0001;
    private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
    private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
    private const int PBT_APMSUSPEND = 0x0004;
    private const int PBT_APMSTANDBY = 0x0005;
    private const int PBT_APMRESUMECRITICAL = 0x0006;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

    #endregion

    #region variables

    private enum Controls
    {
      IMG_REC_CHANNEL = 21,
      LABEL_REC_INFO = 22,
      IMG_REC_RECTANGLE = 23,
    } ;

    //heartbeat related stuff
    private Thread heartBeatTransmitterThread = null;
    //private static string _newTimeshiftFileName = "";
    private static DateTime _updateProgressTimer = DateTime.MinValue;
    private static ChannelNavigator m_navigator;
    private static TVUtil _util;
    private static VirtualCard _card = null;
    private DateTime _updateTimer = DateTime.Now;
    private static bool _autoTurnOnTv = false;
    private static int _waitonresume = 0;
    //int _lagtolerance = 10; //Added by joboehl
    public static bool settingsLoaded = false;
    private DateTime _dtlastTime = DateTime.Now;
    private TvCropManager _cropManager = new TvCropManager();
    private TvNotifyManager _notifyManager = new TvNotifyManager();
    //static string[] _preferredLanguages;
    private static List<string> _preferredLanguages;
    private static bool _usertsp = true;
    private static string _recordingpath = "";
    private static string _timeshiftingpath = "";
    private static bool _preferAC3 = false;
    private static bool _preferAudioTypeOverLang = false;
    private static bool _autoFullScreen = false;
    private static bool _autoFullScreenOnly = false;
    private static bool _resumed = false;
    private static bool _suspended = false;
    private static bool _showlastactivemodule = false;
    private static bool _showlastactivemoduleFullscreen = false;
    private static bool _playbackStopped = false;
    private static bool _onPageLoadDone = false;
    private static bool _userChannelChanged = false;
    private static bool _showChannelStateIcons = true;
    private static bool _doingHandleServerNotConnected = false;
    private static bool _doingChannelChange = false;
    private static bool _ServerNotConnectedHandled = false;
    private static bool _ServerLastStatusOK = true;

    private static ManualResetEvent _waitForBlackScreen = null;
    private static ManualResetEvent _waitForVideoReceived = null;

    private static int FramesBeforeStopRenderBlackImage = 0;

    // this var is used to block the user from hitting "record now" button multiple times
    // the sideeffect is that the user is able to record the same show twice.
    private static int lastActiveRecChannelId = 0;

    private static DateTime lastRecordTime = DateTime.MinValue;
    // we need to reset the lastActiveRecChannelId based on how long since a rec. was initiated.

    //Stopwatch benchClock = null;

    [SkinControl(2)]
    protected GUIButtonControl btnTvGuide = null;
    [SkinControl(3)]
    protected GUIButtonControl btnRecord = null;
    // [SkinControlAttribute(6)]     protected GUIButtonControl btnGroup = null;
    [SkinControl(7)]
    protected GUIButtonControl btnChannel = null;
    [SkinControl(8)]
    protected GUIToggleButtonControl btnTvOnOff = null;
    [SkinControl(13)]
    protected GUIButtonControl btnTeletext = null;
    //    [SkinControlAttribute(14)]    protected GUIButtonControl btnTuningDetails = null;
    [SkinControl(24)]
    protected GUIImage imgRecordingIcon = null;
    [SkinControl(99)]
    protected GUIVideoControl videoWindow = null;
    [SkinControl(9)]
    protected GUIButtonControl btnActiveStreams = null;

    [SkinControl(14)]
    protected GUIButtonControl btnActiveRecordings = null;

    private static bool _connected = false;
    protected static TvServer _server;

    #endregion

    #region Events

    private static event ShowDlgSuccessful OnShowDlgCompleted;

    #endregion

    #region delegates

    private delegate void ShowDlgSuccessful(object Dialogue);

    protected delegate void ShowDlgInteractGUI(object Dialogue);

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "TV";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return (int)Window.WINDOW_TV;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      // TODO:  Add TVHome.GetHome implementation
      strButtonText = GUILocalizeStrings.Get(605);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = @"hover_my tv.png";
      return true;
    }

    public string Author()
    {
      return "Frodo, gemx";
    }

    public string Description()
    {
      return "Connect to TV service to watch, record and timeshift analog and digital TV";
    }

    public bool HasSetup()
    {
      return false;
    }

    public void ShowPlugin()
    {
      /*
      TvSetupForm setup = new TvSetupForm();
      setup.ShowDialog();
       */
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion

    static TVHome()
    {
      GUIGraphicsContext.OnBlackImageRendered += new BlackImageRenderedHandler(OnBlackImageRendered);
      GUIGraphicsContext.OnVideoReceived += new VideoReceivedHandler(OnVideoReceived);

      _waitForBlackScreen = new ManualResetEvent(false);
      _waitForVideoReceived = new ManualResetEvent(false);

      try
      {
        NameValueCollection appSettings = ConfigurationManager.AppSettings;
        appSettings.Set("GentleConfigFile", Config.GetFile(Config.Dir.Config, "gentle.config"));
        m_navigator = new ChannelNavigator();
        LoadSettings();
        string pluginVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        string tvServerVersion = RemoteControl.Instance.GetAssemblyVersion;
        if (pluginVersion != tvServerVersion)
        {
          string strLine = "TvPlugin and TvServer don't have the same version.\r\n";
          strLine += "Please update the older component to the same version as the newer one.\r\n";
          strLine += "TvServer Version: " + tvServerVersion + "\r\n";
          strLine += "TvPlugin Version: " + pluginVersion;
          throw new Exception(strLine);
        }
      }
      catch (Exception ex)
      {
        Log.Error("TVHome: Error occured in constructor: {0}", ex.Message);
      }
    }

    public TVHome()
    {
      GUIGraphicsContext.OnBlackImageRendered += new BlackImageRenderedHandler(OnBlackImageRendered);
      _waitForBlackScreen = new ManualResetEvent(false);
      _waitForVideoReceived = new ManualResetEvent(false);

      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
      Log.Info("TVHome V" + versionInfo.FileVersion + ":ctor");
      GetID = (int)Window.WINDOW_TV;
      Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

      startHeartBeatThread();
    }

    #region Private methods

    private void Application_ApplicationExit(object sender, EventArgs e)
    {
      try
      {
        if (Card.IsTimeShifting)
        {
          Card.User.Name = new User().Name;
          Card.StopTimeShifting();
        }
        stopHeartBeatThread();
      }
      catch (Exception)
      {
      }
    }

    private void HeartBeatTransmitter()
    {
      while (true)
      {
        if (Connected && _resumed && !_suspended)
        {
          bool isTS = (Card != null && Card.IsTimeShifting);
          if (Connected && isTS)
          {
            // send heartbeat to tv server each 5 sec.
            // this way we signal to the server that we are alive thus avoid being kicked.
            // Log.Debug("TVHome: sending HeartBeat signal to server.");

            // when debugging we want to disable heartbeats
#if !DEBUG
            try
            {
              RemoteControl.Instance.HeartBeat(Card.User);
            }
            catch (Exception e)
            {
              Log.Error("TVHome: failed sending HeartBeat signal to server. ({0})", e.Message);
            }
#endif
          }
          else if (Connected && !isTS && !_playbackStopped && _onPageLoadDone &&
                   (!g_Player.IsMusic && !g_Player.IsDVD && !g_Player.IsRadio && !g_Player.IsVideo))
          {
            // check the possible reason why timeshifting has suddenly stopped
            // maybe the server kicked the client b/c a recording on another transponder was due.

            TvStoppedReason result = Card.GetTimeshiftStoppedReason;
            if (result != TvStoppedReason.UnknownReason)
            {
              Log.Debug("TVHome: Timeshifting seems to have stopped - TvStoppedReason:{0}", result);
              string errMsg = "";

              switch (result)
              {
                case TvStoppedReason.HeartBeatTimeOut:
                  errMsg = GUILocalizeStrings.Get(1515);
                  break;
                case TvStoppedReason.KickedByAdmin:
                  errMsg = GUILocalizeStrings.Get(1514);
                  break;
                case TvStoppedReason.RecordingStarted:
                  errMsg = GUILocalizeStrings.Get(1513);
                  break;
                case TvStoppedReason.OwnerChangedTS:
                  errMsg = GUILocalizeStrings.Get(1517);
                  break;
                default:
                  errMsg = GUILocalizeStrings.Get(1516);
                  break;
              }

              GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

              if (pDlgOK != null)
              {
                if (GUIWindowManager.ActiveWindow == (int)(int)Window.WINDOW_TVFULLSCREEN)
                {
                  GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV, true);
                }

                pDlgOK.SetHeading(GUILocalizeStrings.Get(605) + " - " + Navigator.CurrentChannel); //my tv
                errMsg = errMsg.Replace("\\r", "\r");
                string[] lines = errMsg.Split('\r');
                //pDlgOK.SetLine(1, TVHome.Navigator.CurrentChannel);

                for (int i = 0; i < lines.Length; i++)
                {
                  string line = lines[i];
                  pDlgOK.SetLine(1 + i, line);
                }
                pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
              }
              Action keyAction = new Action(Action.ActionType.ACTION_STOP, 0, 0);
              GUIGraphicsContext.OnAction(keyAction);
              _playbackStopped = true;
            }
          }
        }
        Thread.Sleep(HEARTBEAT_INTERVAL * 1000); //sleep for 5 secs. before sending heartbeat again
      }
    }

    private void startHeartBeatThread()
    {
      // setup heartbeat transmitter thread.						
      // thread already running, then leave it.
      if (heartBeatTransmitterThread != null)
      {
        if (heartBeatTransmitterThread.IsAlive)
        {
          return;
        }
      }
      Log.Debug("TVHome: HeartBeat Transmitter started.");
      heartBeatTransmitterThread = new Thread(HeartBeatTransmitter);
      heartBeatTransmitterThread.IsBackground = true;
      heartBeatTransmitterThread.Name = "TvClient-TvHome: HeartBeat transmitter thread";
      heartBeatTransmitterThread.Start();
    }

    private void stopHeartBeatThread()
    {
      if (heartBeatTransmitterThread != null)
      {
        if (heartBeatTransmitterThread.IsAlive)
        {
          Log.Debug("TVHome: HeartBeat Transmitter stopped.");
          heartBeatTransmitterThread.Abort();
        }
      }
    }

    #endregion

    #region Overrides

    public override void OnAdded()
    {
      Log.Info("TVHome:OnAdded");

      // replace g_player's ShowFullScreenWindowTV
      g_Player.ShowFullScreenWindowTV = ShowFullScreenWindowTVHandler;

      Connected = RemoteControl.IsConnected;
    }

    public override bool IsTv
    {
      get { return true; }
    }

    #endregion

    #region Public static methods

    public static void StartRecordingSchedule(Channel channel, bool manual)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      TvServer server = new TvServer();
      if (manual) //till manual stop
      {
        Schedule newSchedule = new Schedule(channel.IdChannel,
                                            GUILocalizeStrings.Get(413) + " (" + channel.DisplayName + ")",
                                            DateTime.Now, DateTime.Now.AddDays(1));
        newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        newSchedule.RecommendedCard = Card.Id;
        //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

        newSchedule.Persist();
        server.OnNewSchedule();
      }
      else //current program
      {
        //lets find any canceled episodes that match this one we want to create, if found uncancel it.
        Schedule existingParentSchedule = Schedule.RetrieveSeries(channel.IdChannel, channel.CurrentProgram.Title,
                                                                  channel.CurrentProgram.StartTime,
                                                                  channel.CurrentProgram.EndTime);
        if (existingParentSchedule != null)
        {
          foreach (CanceledSchedule cancelSched in existingParentSchedule.ReferringCanceledSchedule())
          {
            if (cancelSched.CancelDateTime == channel.CurrentProgram.StartTime)
            {
              existingParentSchedule.UnCancelSerie(channel.CurrentProgram.StartTime);
              server.OnNewSchedule();
              return;
            }
          }
        }

        // ok, no existing schedule found with matching canceled schedules found. proceeding to add the schedule normally
        Schedule newSchedule = new Schedule(channel.IdChannel, channel.CurrentProgram.Title,
                                            channel.CurrentProgram.StartTime, channel.CurrentProgram.EndTime);
        newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        newSchedule.RecommendedCard = Card.Id;
        //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

        newSchedule.Persist();
        server.OnNewSchedule();
      }
    }

    public static bool IsRecordingSchedule(Schedule rec, Program prg, out VirtualCard card)
    {
      TvServer server = new TvServer();

      if (prg != null)
      {
        if (prg.IdChannel != rec.IdChannel)
        {
          card = null;
          return false;
        }
      }

      bool isRec = false;
      bool isCardRec = server.IsRecording(rec.ReferencedChannel().Name, out card);

      if (!isCardRec)
      {
        return false;
      }
      Schedule schedDB = Schedule.Retrieve(rec.IdSchedule);
      if (prg == null)
      {
        prg = Program.RetrieveByTitleAndTimes(schedDB.ProgramName, schedDB.StartTime, schedDB.EndTime);
      }
      bool typeOnce = (schedDB.ScheduleType == (int)ScheduleRecordingType.Once);
      bool isSchedSetupForRec = false;
      bool isSchedRec = false;

      int schedId2CheckIfRec = schedDB.IdSchedule;
      if (!typeOnce)
      {
        Schedule assocSchedule = Schedule.RetrieveOnce(schedDB.IdChannel, schedDB.ProgramName, schedDB.StartTime,
                                                       schedDB.EndTime);
        if (assocSchedule != null)
        {
          schedId2CheckIfRec = assocSchedule.IdSchedule;
          isSchedSetupForRec = assocSchedule.IsRecordingProgram(prg, false);
          //check if we currently recoding this schedule 
        }
        else
        {
          isSchedSetupForRec = schedDB.IsRecordingProgram(prg, false); //check if we currently recoding this schedule 
        }
      }
      else
      {
        if (prg == null)
        {
          isSchedSetupForRec = (isCardRec && isSchedRec);
        }
        else
        {
          isSchedSetupForRec = schedDB.IsRecordingProgram(prg, false); //check if we currently recoding this schedule 
        }
      }

      isSchedRec = server.IsRecordingSchedule(schedId2CheckIfRec, out card);

      if (typeOnce) //if we have a once rec. that has no EPG, then go ahead
      {
        if (prg == null)
        {
          isRec = (isCardRec && isSchedRec);
        }
        else
        {
          isRec = (isCardRec && isSchedRec && isSchedSetupForRec);
        }
      }
      else
      {
        isRec = (isCardRec && isSchedRec && isSchedSetupForRec);
      }


      return isRec;
    }

    /// <summary>
    /// Deletes a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>
    /// <param name="Program">current program</param>    
    /// <param name="deleteEntireSchedule">true if the complete schedule is to be removed.</param>
    /// <param name="supressPrompt">true if no prompt is needed.</param>
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool PromptAndDeleteRecordingSchedule(int scheduleId, Program program, bool deleteEntireSchedule,
                                                        bool supressPrompt)
    {
      if (scheduleId < 1)
      {
        return false;
      }
      Schedule s = Schedule.Retrieve(scheduleId); //always have the correct version from DB.
      if (s == null)
      {
        return false;
      }
      TvServer server = new TvServer();
      Program prg2Use = program;
      Schedule assocSchedule = null;
      bool typeOnce = (s.ScheduleType == (int)ScheduleRecordingType.Once);

      VirtualCard card;

      int schedId2CheckIfRec = s.IdSchedule;
      if (!typeOnce)
      {
        assocSchedule = Schedule.RetrieveOnce(s.IdChannel, s.ProgramName, s.StartTime, s.EndTime);
        if (assocSchedule != null)
        {
          if (deleteEntireSchedule)
          {
            prg2Use = assocSchedule.ReferencedChannel().CurrentProgram;
          }
        }
      }

      if (s.IsManual)
      {
        prg2Use = null;
      }

      bool isRec = IsRecordingSchedule(s, prg2Use, out card);
      bool confirmed = true;

      if (isRec)
      {
        if (!supressPrompt)
        {
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
          if (null == dlgYesNo)
          {
            Log.Error("TVProgramInfo.DeleteRecordingPrompt: ERROR no GUIDialogYesNo found !!!!!!!!!!");
            return false;
          }
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
          dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
          dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
          dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
          dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
          confirmed = dlgYesNo.IsConfirmed;
        }
      }

      if (confirmed)
      {
        if (deleteEntireSchedule || s.IsManual) //delete the entire schedule
        {
          if (isRec)
          {
            if (assocSchedule != null)
            {
              server.StopRecordingSchedule(assocSchedule.IdSchedule);
              if (s != null)
              {
                s.Delete();
              }
            }
            else
            {
              server.StopRecordingSchedule(s.IdSchedule);
            }
          }
          else if (s != null)
          {
            s.Delete();
          }
          server.OnNewSchedule();
        }
        else //delete only a single show, keep the schedule
        {
          if (isRec)
          {
            if (assocSchedule != null)
            {
              server.StopRecordingSchedule(assocSchedule.IdSchedule);
            }
            else
            {
              server.StopRecordingSchedule(s.IdSchedule);
            }
          }

          if (typeOnce)
          {
            Schedule parentSeriesSchedule = Schedule.RetrieveSeries(s.ReferencedChannel().IdChannel, s.ProgramName);
            if (parentSeriesSchedule != null)
            {
              CanceledSchedule canceledSchedule = new CanceledSchedule(parentSeriesSchedule.IdSchedule,
                                                                       program.StartTime);
              canceledSchedule.Persist();
            }

            if (!isRec && s != null)
            {
              s.Delete();
            }
            server.OnNewSchedule();
          }
          else
          {
            CanceledSchedule canceledSchedule = new CanceledSchedule(s.IdSchedule, program.StartTime);
            canceledSchedule.Persist();
            server.OnNewSchedule();
          }
        }
        return true;
      }
      return false;
    }

    public static bool UseRTSP()
    {
      bool useRtsp = File.Exists("c:\\usertsp.txt");

      if (!useRtsp)
      {
        bool isSingleSeat = IsSingleSeat();
        if (!isSingleSeat)
        {
          if (!settingsLoaded)
          {
            LoadSettings();
          }
          useRtsp = _usertsp;
        }
        else
        {
          useRtsp = false;
        }
      }
      return useRtsp;
    }

    public static bool ShowChannelStateIcons()
    {
      return _showChannelStateIcons;
    }

    public static string RecordingPath()
    {
      return _recordingpath;
    }

    public static string TimeshiftingPath()
    {
      return _timeshiftingpath;
    }

    public static bool DoingChannelChange()
    {
      return _doingChannelChange;
    }

    private static void ShowDlgCompleted(object Dialogue)
    {
      try
      {
        GUIGraphicsContext.form.Invoke(new ShowDlgInteractGUI(ShowDlgGUI), new object[] { Dialogue });
      }
      catch (Exception)
      {
      }
    }

    private static void ShowDlgGUI(object Dialogue)
    {
      GUIDialogOK pDlgOK = null;
      GUIDialogYesNo pDlgYESNO = null;

      if (Dialogue is GUIDialogOK)
      {
        pDlgOK = (GUIDialogOK)Dialogue;
      }
      else if (Dialogue is GUIDialogYesNo)
      {
        pDlgYESNO = (GUIDialogYesNo)Dialogue;
      }
      else
      {
        return;
      }

      if (pDlgOK != null)
      {
        pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
      }
      if (pDlgYESNO != null)
      {
        pDlgYESNO.DoModal(GUIWindowManager.ActiveWindowEx);
        // If failed and wasPlaying TV, fallback to the last viewed channel. 						
        if (pDlgYESNO.IsConfirmed)
        {
          ViewChannelAndCheck(Navigator.Channel);
        }
      }
    }

    public static void ShowDlgThread(object Dialogue)
    {
      GUIWindow guiWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);

      int count = 0;

      while (count < 50)
      {
        if (guiWindow.WindowLoaded)
        {
          break;
        }
        else
        {
          Thread.Sleep(100);
        }
        count++;
      }

      if (guiWindow.WindowLoaded)
      {
        if (OnShowDlgCompleted != null)
        {
          OnShowDlgCompleted(Dialogue);
        }
      }
    }

    public static bool HandleServerNotConnected()
    {
      // _doingHandleServerNotConnected is used to avoid multiple calls to this method.
      // the result could be that the dialogue is not shown.
      try
      {
        if (_ServerNotConnectedHandled)
        {
          return true; //still not connected
        }

        if (_doingHandleServerNotConnected)
        {
          return !Connected;
        }
        _doingHandleServerNotConnected = true;
        bool remConnected = RemoteControl.IsConnected;

        // we just did a successful connect      
        if (remConnected && !Connected)
        {
          _ServerLastStatusOK = true;
          GUIMessage initMsg = null;
          initMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, (int)Window.WINDOW_TV_OVERLAY, 0, 0, 0,
                                   0, null);
          GUIWindowManager.SendThreadMessage(initMsg);
        }

        Connected = remConnected;

        if (!Connected)
        {
          _ServerLastStatusOK = false; // to enable TV connect button again
          //g_Player.Stop(); // moved to Process() for correct threading

          Card.User.Name = new User().Name;

          if (g_Player.FullScreen)
          {
            g_Player.Stop();
            GUIMessage initMsgTV = null;
            initMsgTV = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, (int)Window.WINDOW_TV, 0, 0, 0, 0,
                                       null);
            GUIWindowManager.SendThreadMessage(initMsgTV);

            _doingHandleServerNotConnected = false;
            return true;
          }
          _ServerNotConnectedHandled = true;
          GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

          if (pDlgOK != null)
          {
            pDlgOK.Reset();
            pDlgOK.SetHeading(605); //my tv
            pDlgOK.SetLine(1, Navigator.CurrentChannel);
            pDlgOK.SetLine(2, GUILocalizeStrings.Get(1510)); //Connection to TV server lost

            //pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);

            if (OnShowDlgCompleted == null)
            {
              OnShowDlgCompleted += new ShowDlgSuccessful(ShowDlgCompleted);
            }

            ParameterizedThreadStart pThread = new ParameterizedThreadStart(ShowDlgThread);
            Thread showDlgThread = new Thread(pThread);
            showDlgThread.IsBackground = true;
            // show the dialog asynch.
            // this fixes a hang situation that would happen when resuming TV with showlastactivemodule
            showDlgThread.Start(pDlgOK);
          }
          _doingHandleServerNotConnected = false;
          return true;
        }
      }
      catch (Exception e)
      {
        //we assume that server is disconnected.
        Log.Error("TVHome.HandleServerNotConnected caused an error {0},{1}", e.Message, e.StackTrace);
        _doingHandleServerNotConnected = false;
        return true;
      }
      finally
      {
        _doingHandleServerNotConnected = false;
      }
      return false;
    }


    public static bool UserChannelChanged
    {
      set { _userChannelChanged = value; }
    }

    public static TVUtil Util
    {
      get
      {
        if (_util == null)
        {
          _util = new TVUtil();
        }
        return _util;
      }
    }

    public static TvServer TvServer
    {
      get
      {
        if (_server == null)
        {
          _server = new TvServer();
        }
        return _server;
      }
    }

    public static bool Connected
    {
      get { return _connected; }
      set { _connected = value; }
    }

    public static VirtualCard Card
    {
      get
      {
        if (_card == null)
        {
          User user = new User();
          _card = TvServer.CardByIndex(user, 0);
        }
        return _card;
      }
      set
      {
        if (_card != null)
        {
          bool stop = true;
          if (value != null)
          {
            if (value.Id == _card.Id || value.Id == -1)
            {
              stop = false;
            }
          }
          if (stop)
          {
            _card.User.Name = new User().Name;
            _card.StopTimeShifting();
          }
          _card = value;
        }
      }
    }

    #endregion

    #region Serialisation

    private static void LoadSettings()
    {
      if (settingsLoaded)
      {
        return;
      }
      settingsLoaded = true;
      using (Settings xmlreader = new MPSettings())
      {
        m_navigator.LoadSettings(xmlreader);
        _autoTurnOnTv = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        _showlastactivemodule = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
        _showlastactivemoduleFullscreen = xmlreader.GetValueAsBool("general", "lastactivemodulefullscreen", false);

        _waitonresume = xmlreader.GetValueAsInt("tvservice", "waitonresume", 0);

        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "Normal");
        GUIGraphicsContext.ARType = Utils.GetAspectRatio(strValue);

        string preferredLanguages = xmlreader.GetValueAsString("tvservice", "preferredaudiolanguages", "");
        //_preferredLanguages = preferredLanguages.Split(';');
        _preferredLanguages = new List<string>();
        Log.Debug("TVHome.LoadSettings(): Preferred Audio Languages: " + preferredLanguages);

        StringTokenizer st = new StringTokenizer(preferredLanguages, ";");
        while (st.HasMore)
        {
          string lang = st.NextToken();
          if (lang.Length != 3)
          {
            Log.Warn("Language {0} is not in the correct format!", lang);
          }
          else
          {
            _preferredLanguages.Add(lang);
            Log.Info("Prefered language {0} is {1}", _preferredLanguages.Count, lang);
          }
        }
        _usertsp = xmlreader.GetValueAsBool("tvservice", "usertsp", true);
        _recordingpath = xmlreader.GetValueAsString("tvservice", "recordingpath", "");
        _timeshiftingpath = xmlreader.GetValueAsString("tvservice", "timeshiftingpath", "");

        _preferAC3 = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        _preferAudioTypeOverLang = xmlreader.GetValueAsBool("tvservice", "preferAudioTypeOverLang", true);
        _autoFullScreen = xmlreader.GetValueAsBool("mytv", "autofullscreen", false);
        _autoFullScreenOnly = xmlreader.GetValueAsBool("mytv", "autofullscreenonly", false);
        _showChannelStateIcons = xmlreader.GetValueAsBool("mytv", "showChannelStateIcons", true);
      }
    }

    private void SaveSettings()
    {
      if (m_navigator != null)
      {
        using (Settings xmlwriter = new MPSettings())
        {
          m_navigator.SaveSettings(xmlwriter);
        }
      }
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Gets called by the runtime when a  window will be destroyed
    /// Every window window should override this method and cleanup any resources
    /// </summary>
    /// <returns></returns>
    public override void DeInit()
    {
      OnPageDestroy(-1);
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override bool Init()
    {
      Log.Info("TVHome:Init");
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvhomeServer.xml");
      GetID = (int)Window.WINDOW_TV;

      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.AudioTracksReady += new g_Player.AudioTracksReadyHandler(OnAudioTracksReady);

      GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
      return bResult;
    }

    public static void OnGlobalMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO:
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel channel = layer.GetChannelByName(message.Label);
            if (channel != null)
            {
              ViewChannelAndCheck(channel);
            }
            break;
          }
        case GUIMessage.MessageType.GUI_MSG_STOP_SERVER_TIMESHIFTING:
          {
            User user = new User();
            if (user.Name == Card.User.Name)
            {
              Card.StopTimeShifting();
            }
            ;
            break;
          }
      }
    }

    private void OnAudioTracksReady()
    {
      Log.Debug("TVHome.OnAudioTracksReady()");

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int prefLangIdx = GetPreferedAudioStreamIndex(out dualMonoMode);
      g_Player.CurrentAudioStream = prefLangIdx;

      if (dualMonoMode != eAudioDualMonoMode.UNSUPPORTED)
      {
        g_Player.SetAudioDualMonoMode(dualMonoMode);
      }      
    }

    private void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      // when we are watching TV and suddenly decides to watch a audio/video etc., we want to make sure that the TV is stopped on server.
      GUIWindow currentWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);

      if (type == g_Player.MediaType.Radio || type == g_Player.MediaType.TV)
      {
        UpdateGUIonPlaybackStateChange(true);
      }

      if (currentWindow.IsTv)
      {
        return;
      }
      if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_RADIO)
      {
        return;
      }

      //gemx: fix for 0001181: Videoplayback does not work if tvservice.exe is not running 
      if (Connected)
      {
        Card.StopTimeShifting();
      }
    }

    private void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.TV && type != g_Player.MediaType.Radio)
      {
        return;
      }

      UpdateGUIonPlaybackStateChange(false);

      //gemx: fix for 0001181: Videoplayback does not work if tvservice.exe is not running 
      if (!Connected)
      {
        return;
      }
      if (Card.IsTimeShifting == false)
      {
        return;
      }

      /*if (_newTimeshiftFileName.Length > 0 && !_newTimeshiftFileName.Equals(filename))
      {
        return;
      }
      */

      //tv off
      Log.Info("TVHome:turn tv off");
      SaveSettings();
      Card.User.Name = new User().Name;
      Card.StopTimeShifting();

      if (type == g_Player.MediaType.Radio || type == g_Player.MediaType.TV)
      {
        // doProcess();
        UpdateGUIonPlaybackStateChange(false);
      }

      //_newTimeshiftFileName = "";
      _playbackStopped = true;
    }

    public static bool ManualRecord(Channel channel)
    {
      if (GUIWindowManager.ActiveWindowEx == (int)(int)Window.WINDOW_TVFULLSCREEN)
      {
        Log.Info("send message to fullscreen tv");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORD, GUIWindowManager.ActiveWindow, 0, 0, 0, 0,
                                        null);
        msg.SendToTargetWindow = true;
        msg.TargetWindowId = (int)(int)Window.WINDOW_TVFULLSCREEN;
        GUIGraphicsContext.SendMessage(msg);
        return false;
      }

      Log.Info("TVHome:Record action");
      TvServer server = new TvServer();

      VirtualCard card;
      if (false == server.IsRecording(channel.Name, out card))
      {
        bool alreadyRec = (lastActiveRecChannelId == Navigator.Channel.IdChannel);
        if (!alreadyRec)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          Program prog = channel.CurrentProgram;
          if (prog != null)
          {
            GUIDialogMenuBottomRight pDlgOK =
              (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
            if (pDlgOK != null)
            {
              pDlgOK.Reset();
              pDlgOK.SetHeading(605); //my tv
              pDlgOK.AddLocalizedString(875); //current program
              pDlgOK.AddLocalizedString(876); //till manual stop
              pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
              switch (pDlgOK.SelectedId)
              {
                case 875:
                  {
                    //record current program                  
                    StartRecordingSchedule(channel, false);
                    lastActiveRecChannelId = Navigator.Channel.IdChannel;
                    lastRecordTime = DateTime.Now;
                    return true;
                  }

                case 876:
                  {
                    //manual
                    StartRecordingSchedule(channel, true);
                    lastActiveRecChannelId = Navigator.Channel.IdChannel;
                    lastRecordTime = DateTime.Now;
                    return true;
                  }
              }
            }
          }
          else
          {
            //manual record
            StartRecordingSchedule(channel, true);
            lastActiveRecChannelId = Navigator.Channel.IdChannel;
            lastRecordTime = DateTime.Now;
            return true;
          }
        }
      }
      else
      {
        Schedule s = Schedule.Retrieve(card.RecordingScheduleId);
        PromptAndDeleteRecordingSchedule(s.IdSchedule, s.ReferencedChannel().CurrentProgram, false, true);
        return false;
      }
      return false;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_RECORD:
          //record current program on current channel
          //are we watching tv?                    
          ManualRecord(Navigator.Channel);
          break;

        case Action.ActionType.ACTION_PREV_CHANNEL:
          OnPreviousChannel();
          break;
        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPreviousChannel();
          break;

        case Action.ActionType.ACTION_NEXT_CHANNEL:
          OnNextChannel();
          break;
        case Action.ActionType.ACTION_PAGE_UP:
          OnNextChannel();
          break;

        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL: // mPod
          OnLastViewedChannel();
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            // goto home 
            // are we watching tv & doing timeshifting

            // No, then stop viewing... 
            //g_Player.Stop();
            GUIWindowManager.ShowPreviousWindow();
            return;
          }

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            if ((char)action.m_key.KeyChar == '0')
            {
              OnLastViewedChannel();
            }
          }
          break;
        case Action.ActionType.ACTION_SHOW_GUI:
          {
            // If we are in tvhome and TV is currently off and no fullscreen TV then turn ON TV now!
            if (!g_Player.IsTimeShifting && !g_Player.FullScreen)
            {
              OnClicked(8, btnTvOnOff, Action.ActionType.ACTION_MOUSE_CLICK); //8=togglebutton
            }
            break;
          }
      }
      base.OnAction(action);
    }

    private void OnSuspend()
    {
      Log.Debug("TVHome.OnSuspend()");

      try
      {
        if (Card.IsTimeShifting)
        {
          Card.User.Name = new User().Name;
          Card.StopTimeShifting();
        }
        stopHeartBeatThread();
      }
      catch (Exception)
      {
      }
      finally
      {
        _suspended = true;
        _resumed = false;
      }
    }

    private void OnResume()
    {
      Log.Debug("TVHome.OnResume()");
      _resumed = true;
      _suspended = false;
    }

    public void Start()
    {
      Log.Debug("TVHome.Start()");
    }

    public void Stop()
    {
      Log.Debug("TVHome.Stop()");
    }

    public bool WndProc(ref Message msg)
    {
      if (msg.Msg == WM_POWERBROADCAST)
      {
        switch (msg.WParam.ToInt32())
        {
          case PBT_APMSTANDBY:
            Log.Info("TVHome.WndProc(): Windows is going to standby");
            OnSuspend();
            break;
          case PBT_APMSUSPEND:
            Log.Info("TVHome.WndProc(): Windows is suspending");
            OnSuspend();
            break;
          case PBT_APMQUERYSUSPEND:
          case PBT_APMQUERYSTANDBY:
            Log.Info("TVHome.WndProc(): Windows is going into powerstate (hibernation/standby)");

            break;
          case PBT_APMRESUMESUSPEND:
            Log.Info("TVHome.WndProc(): Windows has resumed from hibernate mode");
            OnResume();
            break;
          case PBT_APMRESUMESTANDBY:
            Log.Info("TVHome.WndProc(): Windows has resumed from standby mode");
            OnResume();
            break;
        }
        return true;
      }
      return false;
    }

    private static bool wasPrevWinTVplugin()
    {
      bool result = false;

      int act = GUIWindowManager.ActiveWindow;
      int prev = GUIWindowManager.GetWindow(act).PreviousWindowId;

      //plz any newly added ID's to this list.

      result = (
                 prev == (int)Window.WINDOW_TV_CROP_SETTINGS ||
                 prev == (int)Window.WINDOW_SETTINGS_SORT_CHANNELS ||
                 prev == (int)Window.WINDOW_SETTINGS_TV_EPG ||
                 prev == (int)Window.WINDOW_TVFULLSCREEN ||
                 prev == (int)Window.WINDOW_TVGUIDE ||
                 prev == (int)Window.WINDOW_MINI_GUIDE ||
                 prev == (int)Window.WINDOW_TV_SEARCH ||
                 prev == (int)Window.WINDOW_TV_SEARCHTYPE ||
                 prev == (int)Window.WINDOW_TV_SCHEDULER_PRIORITIES ||
                 prev == (int)Window.WINDOW_TV_PROGRAM_INFO ||
                 prev == (int)Window.WINDOW_RECORDEDTV ||
                 prev == (int)Window.WINDOW_TV_RECORDED_INFO ||
                 prev == (int)Window.WINDOW_SETTINGS_RECORDINGS ||
                 prev == (int)Window.WINDOW_SCHEDULER ||
                 prev == (int)Window.WINDOW_SEARCHTV ||
                 prev == (int)Window.WINDOW_TV_TUNING_DETAILS
               );
      if (!result && prev == (int)Window.WINDOW_FULLSCREEN_VIDEO && g_Player.IsTVRecording)
      {
        result = true;
      }
      return result;
    }

    protected override void OnPageLoad()
    {
      // when suspending MP while watching fullscreen TV, the player is stopped ok, but it returns to tvhome, which starts timeshifting.
      // this could lead the tv server timeshifting even though client is asleep.
      // although we have to make sure that resuming again activates TV, this is done by checking previous window ID.      

      // disabled currently as pausing the graph stops GUI repainting
      //GUIWaitCursor.Show();
      if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).PreviousWindowId != (int)Window.WINDOW_TVFULLSCREEN)
      {
        _playbackStopped = false;
      }

      btnActiveStreams.Label = GUILocalizeStrings.Get(692);

      int waits = 0;
      while (true)
      {
        if (!RemoteControl.IsConnected)
        {
          if (!_onPageLoadDone)
          {
            RemoteControl.Clear();
            GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
            return;
          }
          else if (waits >= MAX_WAIT_FOR_SERVER_CONNECTION)
          {
            bool res = HandleServerNotConnected();

            UpdateStateOfRecButton();
            UpdateProgressPercentageBar();
            UpdateRecordingIndicator();
            return;
          }

          waits++;
          Log.Info("tv home onpageload: waiting for TVservice {} sec.", waits);
          Thread.Sleep(1000); //wait 1 sec
          //GUIWaitCursor.Hide();          
        }
        else
        {
          Log.Info("tv home onpageload: done waiting for TVservice.");
          Connected = true;
          break;
        }
      }

      try
      {
        int cards = RemoteControl.Instance.Cards;
      }
      catch (Exception)
      {
        RemoteControl.Clear();
      }

      try
      {
        IList<Card> cards = TvDatabase.Card.ListAll();
      }
      catch (Exception)
      {
        // lets try one more time - seems like the gentle framework is not properly initialized when coming out of standby/hibernation.        
        if (Connected && RemoteControl.IsConnected)
        {
          //lets wait 10 secs before giving up.
          DateTime now = DateTime.Now;
          TimeSpan ts = now - DateTime.Now;
          bool success = false;

          while (ts.TotalSeconds > -10 && !success)
          {
            try
            {
              IList<Card> cards = TvDatabase.Card.ListAll();
              success = true;
            }
            catch (Exception)
            {
              success = false;
            }
            ts = now - DateTime.Now;
          }

          if (!success)
          {
            RemoteControl.Clear();
            GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
            //GUIWaitCursor.Hide();
            return;
          }
        }
        else
        {
          RemoteControl.Clear();
          GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
          //GUIWaitCursor.Hide();
          return;
        }
      }

      //stop the old recorder.
      //DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.DataSourceOnly;
      GUIMessage msgStopRecorder = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgStopRecorder);

      if (!_onPageLoadDone && m_navigator != null)
      {
        m_navigator.ReLoad();
      }

      if (m_navigator == null)
      {
        m_navigator = new ChannelNavigator(); // Create the channel navigator (it will load groups and channels)
      }

      base.OnPageLoad();

      //set video window position
      if (videoWindow != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width,
                                                       videoWindow.Height);
      }

      // start viewing tv... 
      GUIGraphicsContext.IsFullScreenVideo = false;
      Channel channel = Navigator.Channel;
      if (channel == null || channel.IsRadio)
      {
        if (Navigator.CurrentGroup != null && Navigator.Groups.Count > 0)
        {
          Navigator.SetCurrentGroup(Navigator.Groups[0].GroupName);
          GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.Groups[0].GroupName);
        }
        if (Navigator.CurrentGroup != null)
        {
          if (Navigator.CurrentGroup.ReferringGroupMap().Count > 0)
          {
            GroupMap gm = (GroupMap)Navigator.CurrentGroup.ReferringGroupMap()[0];
            channel = gm.ReferencedChannel();
          }
        }
      }

      if (channel != null)
      {
        /*
        if (TVHome.Card.IsTimeShifting)
        {
          int id = TVHome.Card.IdChannel;
          if (id >= 0)
          {
            channel = Channel.Retrieve(id);
          }
        }
        */
        Log.Info("tv home init:{0}", channel.DisplayName);
        if (_autoTurnOnTv && !_playbackStopped && !wasPrevWinTVplugin())
        {
          if (!wasPrevWinTVplugin())
          {
            _userChannelChanged = false;
          }

          ViewChannelAndCheck(channel);
        }

        GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.CurrentGroup.GroupName);
        Log.Info("tv home init:{0} done", channel.DisplayName);
      }

      // if using showlastactivemodule feature and last module is fullscreen while returning from powerstate, then do not set fullscreen here (since this is done by the resume last active module feature)
      // we depend on the onresume method, thats why tvplugin now impl. the IPluginReceiver interface.      
      bool showlastActModFS = (_showlastactivemodule && _showlastactivemoduleFullscreen && _resumed && _autoTurnOnTv);
      bool useDelay = false;

      if (_resumed && !showlastActModFS)
      {
        useDelay = true;
        showlastActModFS = false;
      }
      else if (_resumed)
      {
        showlastActModFS = true;
      }
      if (!showlastActModFS && (g_Player.IsTV || g_Player.IsTVRecording))
      {
        if (_autoFullScreen && !g_Player.FullScreen && (!wasPrevWinTVplugin()))
        {
          Log.Debug("TVHome.OnPageLoad(): setting autoFullScreen");
          //if we are resuming from standby with tvhome, we want this in fullscreen, but we need a delay for it to work.
          if (useDelay)
          {
            Thread tvDelayThread = new Thread(TvDelayThread);
            tvDelayThread.Start();
          }
          else //no delay needed here, since this is when the system is being used normally
          {
            // wait for timeshifting to complete
            /*
            int waits = 0;
            while (_playbackStopped && waits < 100)
            {
              //Log.Debug("TVHome.OnPageLoad(): waiting for timeshifting to start");
              Thread.Sleep(100);
              waits++;
            }
            if (!_playbackStopped)
            {
              //Log.Debug("TVHome.OnPageLoad(): timeshifting has started - waits: {0}", waits);
              g_Player.ShowFullScreenWindow();
            }
            */
            g_Player.ShowFullScreenWindow();
          }
        }
        else if (_autoFullScreenOnly && !g_Player.FullScreen && (PreviousWindowId == (int)Window.WINDOW_TVFULLSCREEN))
        {
          Log.Debug("TVHome.OnPageLoad(): autoFullScreenOnly set, returning to previous window");
          GUIWindowManager.ShowPreviousWindow();
        }
      }

      /*
      string currentChannel = TVHome.Navigator.CurrentChannel;
      if (currentChannel != null && currentChannel.Length > 0)
      {
        TvServer server = new TvServer();
        VirtualCard vc;
        server.IsRecording(currentChannel, out vc);
        if (vc != null)
        {
          TVHome.Card = vc;
        }
      }      
      */

      _onPageLoadDone = true;
      _resumed = false;

      UpdateGUIonPlaybackStateChange();
      doProcess();
      //GUIWaitCursor.Hide();
    }

    private void TvDelayThread()
    {
      //we have to use a small delay before calling tvfullscreen.                                    
      Thread.Sleep(200);

      // wait for timeshifting to complete
      int waits = 0;
      while (_playbackStopped && waits < 100)
      {
        //Log.Debug("TVHome.OnPageLoad(): waiting for timeshifting to start");
        Thread.Sleep(100);
        waits++;
      }
      if (!_playbackStopped)
      {
        //Log.Debug("TVHome.OnPageLoad(): timeshifting has started - waits: {0}", waits);
        g_Player.ShowFullScreenWindow();
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      //if we're switching to another plugin
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        //and we're not playing which means we dont timeshift tv
        //g_Player.Stop();
      }
      if (RemoteControl.IsConnected)
      {
        SaveSettings();
      }
      base.OnPageDestroy(newWindowId);
    }

    public static void OnSelectGroup()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(971); // group
      int selected = 0;

      for (int i = 0; i < Navigator.Groups.Count; ++i)
      {
        dlg.Add(Navigator.Groups[i].GroupName);
        if (Navigator.Groups[i].GroupName == Navigator.CurrentGroup.GroupName)
        {
          selected = i;
        }
      }

      dlg.SelectedLabel = selected;
      dlg.DoModal(GUIWindowManager.ActiveWindow);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }

      Navigator.SetCurrentGroup(dlg.SelectedLabelText);
      GUIPropertyManager.SetProperty("#TV.Guide.Group", dlg.SelectedLabelText);

      //if (Navigator.CurrentGroup != null)
      //{
      //  if (Navigator.CurrentGroup.ReferringGroupMap().Count > 0)
      //  {
      //    GroupMap gm = (GroupMap)Navigator.CurrentGroup.ReferringGroupMap()[0];
      //  }
      //}
    }

    private void OnSelectChannel()
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();
      TvMiniGuide miniGuide = (TvMiniGuide)GUIWindowManager.GetWindow((int)Window.WINDOW_MINI_GUIDE);
      miniGuide.AutoZap = false;
      miniGuide.SelectedChannel = Navigator.Channel;
      miniGuide.DoModal(GetID);

      //Only change the channel if the channel selectd is actually different. 
      //Without this, a ChannelChange might occur even when MiniGuide is canceled. 
      if (!miniGuide.Canceled)
      {
        //_userChannelChanged = true;
        ViewChannelAndCheck(miniGuide.SelectedChannel);
      }

      benchClock.Stop();
      Log.Debug("TVHome.OnSelecChannel(): Total Time {0} ms", benchClock.ElapsedMilliseconds.ToString());
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();

      if (HandleServerNotConnected() && _ServerLastStatusOK)
      {
        UpdateStateOfRecButton();
        UpdateProgressPercentageBar();
        UpdateRecordingIndicator();
        return;
      }

      if (control == btnActiveStreams)
      {
        OnActiveStreams();
      }

      if (control == btnActiveRecordings && btnActiveRecordings != null)
      {
        OnActiveRecordings();
      }

      if (control == btnTvOnOff)
      {
        if (Card.IsTimeShifting && g_Player.IsTV && g_Player.Playing)
        {
          //tv off
          g_Player.Stop();
          Log.Warn("TVHome.OnClicked(): EndTvOff {0} ms", benchClock.ElapsedMilliseconds.ToString());
          benchClock.Stop();
          return;
        }
        else
        {
          // tv on
          Log.Info("TVHome:turn tv on {0}", Navigator.CurrentChannel);

          //stop playing anything
          if (g_Player.Playing)
          {
            if (g_Player.IsTV && !g_Player.IsTVRecording)
            {
              //already playing tv...
            }
            else
            {
              Log.Warn("TVHome.OnClicked: Stop Called - {0} ms", benchClock.ElapsedMilliseconds.ToString());
              g_Player.Stop(true);
            }
          }
        }

        // turn tv on/off        
        if (Navigator.Channel.IsTv)
        {
          ViewChannelAndCheck(Navigator.Channel);
        }
        else
        // current channel seems to be non-tv (radio ?), get latest known tv channel from xml config and use this instead
        {
          Settings xmlreader = new MPSettings();
          string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
          Channel currentChannel = Navigator.GetChannel(currentchannelName);
          ViewChannelAndCheck(currentChannel);
        }

        UpdateStateOfRecButton();
        UpdateGUIonPlaybackStateChange();
        //UpdateProgressPercentageBar();
        benchClock.Stop();
        Log.Warn("TVHome.OnClicked(): Total Time - {0} ms", benchClock.ElapsedMilliseconds.ToString());
      }
      /*
      if (control == btnGroup)
      {
        OnSelectGroup();
      }*/
      if (control == btnTeletext)
      {
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_TELETEXT);
        return;
      }
      //if (control == btnTuningDetails)
      //{
      //  GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_TUNING_DETAILS);
      //}

      if (control == btnRecord)
      {
        OnRecord();
      }
      if (control == btnChannel)
      {
        OnSelectChannel();
      }
      base.OnClicked(controlId, control, actionType);
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.PS_ONSTANDBY:
          RemoteControl.Clear();
          break;
        case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
          {
            if (_autoTurnOnTv && !wasPrevWinTVplugin())
            // we only want to resume TV if previous window is NOT a tvplugin based one. (ex. tvguide.)
            {
              //restart viewing...  
              Log.Info("tv home msg resume tv:{0}", Navigator.CurrentChannel);
              ViewChannel(Navigator.Channel);
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
          Log.Info("tv home msg view chan:{0}", message.Label);
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel ch = layer.GetChannelByName(message.Label);
            ViewChannel(ch);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
          {
            Log.Info("tv home msg stop chan:{0}", message.Label);
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel ch = layer.GetChannelByName(message.Label);
            ViewChannel(ch);
          }
          break;
      }
      return base.OnMessage(message);
    }

    public override void Process()
    {
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds < 1000)
      {
        return;
      }
      //stop playing non-fullscreen TV here to overcome thread error
      if (!RemoteControl.IsConnected && !g_Player.FullScreen && g_Player.IsTV)
      {
        g_Player.Stop();
      }

      UpdateRecordingIndicator();
      UpdateStateOfRecButton();

      if (!Card.IsTimeShifting)
      {
        UpdateProgressPercentageBar(); // mantis #2218 : TV guide information in TV home screen does not update when program changes if TV is not playing 
        return;
      }

      // BAV, 02.03.08: a channel change should not be delayed by rendering.
      //                by moving thisthe 1 min delays in zapping should be fixed
      // Let the navigator zap channel if needed
      Navigator.CheckChannelChange();

      if (GUIGraphicsContext.InVmr9Render)
      {
        return;
      }

      doProcess();

      _updateTimer = DateTime.Now;
    }

    #endregion

    private void UpdateGUIonPlaybackStateChange(bool playbackStarted)
    {
      if (btnTvOnOff.Selected != playbackStarted)
      {
        btnTvOnOff.Selected = playbackStarted;
      }

      UpdateProgressPercentageBar();

      bool hasTeletext = (!Connected || Card.HasTeletext) && (playbackStarted);
      btnTeletext.IsVisible = hasTeletext;
    }

    private void UpdateGUIonPlaybackStateChange()
    {
      bool isTimeShiftingTV = (Connected && Card.IsTimeShifting && g_Player.IsTV);

      if (btnTvOnOff.Selected != isTimeShiftingTV)
      {
        btnTvOnOff.Selected = isTimeShiftingTV;
      }

      UpdateProgressPercentageBar();

      bool hasTeletext = (!Connected || Card.HasTeletext) && (isTimeShiftingTV);
      btnTeletext.IsVisible = hasTeletext;
    }

    private void doProcess()
    {
      if (!g_Player.Playing)
      {
        return;
      }

      // BAV, 02.03.08
      //Navigator.CheckChannelChange();

      // Update navigator with information from the Recorder
      // TODO: This should ideally be handled using events. Recorder should fire an event
      // when the current channel changes. This is a temporary workaround //Vic
      string currchan = Navigator.CurrentChannel; // Remember current channel
      Navigator.UpdateCurrentChannel();
      bool channelchanged = currchan != Navigator.CurrentChannel;

      UpdateProgressPercentageBar();

      GUIControl.HideControl(GetID, (int)Controls.LABEL_REC_INFO);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_RECTANGLE);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_CHANNEL);
    }

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowTV
    /// </summary>
    /// <returns></returns>
    private static bool ShowFullScreenWindowTVHandler()
    {
      if (g_Player.IsTV && Card.IsTimeShifting)
      {
        // watching TV
        if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_TVFULLSCREEN)
        {
          return true;
        }
        Log.Info("TVHome: ShowFullScreenWindow switching to fullscreen tv");
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_TVFULLSCREEN);
        GUIGraphicsContext.IsFullScreenVideo = true;
        return true;
      }
      return g_Player.ShowFullScreenWindowTVDefault();
    }

    /// <summary>
    /// check if we have a single seat environment
    /// </summary>
    /// <returns></returns>
    public static bool IsSingleSeat()
    {
      // Log.Debug("TVHome: IsSingleSeat - RemoteControl.HostName = {0} / Environment.MachineName = {1}", RemoteControl.HostName, Environment.MachineName);
      return (RemoteControl.HostName.ToLowerInvariant() == Environment.MachineName.ToLowerInvariant());
    }

    public static void UpdateTimeShift()
    {
    }

    private void OnActiveRecordings()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(200052); // Active Recordings
      int selected = 0;

      IList<Card> cards = TvDatabase.Card.ListAll();
      List<Channel> channels = new List<Channel>();
      int count = 0;
      TvServer server = new TvServer();
      List<User> _users = new List<User>();

      List<Schedule> recordingSchedules = new List<Schedule>();

      foreach (Card card in cards)
      {
        if (card.Enabled == false)
        {
          continue;
        }
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          continue;
        }
        User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        if (users == null)
        {
          return;
        }
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (card.IdCard != user.CardId)
          {
            continue;
          }
          bool isRecording;
          VirtualCard tvcard = new VirtualCard(user, RemoteControl.HostName);
          isRecording = tvcard.IsRecording;

          if (isRecording)
          {
            int idChannel = tvcard.IdChannel;
            user = tvcard.User;
            if (!user.IsAdmin)
            {
              continue; // a scheduler user is always an admin 
            }
            Channel ch = Channel.Retrieve(idChannel);
            channels.Add(ch);
            GUIListItem item = new GUIListItem();
            string channelName = ch.DisplayName;
            string programTitle = "";
            if (ch.CurrentProgram != null)
            {
              programTitle = ch.CurrentProgram.Title.Trim(); // default is current EPG info
            }


            //retrive the EPG info from when the rec. was started.
            IList<Schedule> schedulesList = Schedule.ListAll();
            if (schedulesList != null)
            {
              Schedule rec = Schedule.Retrieve(tvcard.RecordingScheduleId);

              if (rec == null)
              {
                foreach (Schedule s in schedulesList)
                {
                  if (TvServer.IsRecordingSchedule(s.IdSchedule, out tvcard))
                  {
                    rec = Schedule.Retrieve(tvcard.RecordingScheduleId);
                    break;
                  }
                }
              }


              if (rec != null)
              {
                foreach (Schedule s in schedulesList)
                {
                  ScheduleRecordingType scheduleType = (ScheduleRecordingType)s.ScheduleType;
                  bool isManual = (scheduleType == ScheduleRecordingType.Once);

                  if (isManual && s.ReferencedChannel().IdChannel == rec.ReferencedChannel().IdChannel &&
                      s.StartTime == rec.StartTime)
                  {
                    programTitle = s.ProgramName.Trim();
                    recordingSchedules.Add(s);
                    break;
                  }
                }
              }
            }

            int totalLength = channelName.Length + programTitle.Length;
            //scrolling would be better than truncating the string, but scrolling only seems to work on label1, not label2 ???
            if (totalLength > 30)
            {
              programTitle = programTitle.Substring(0, 30 - channelName.Length);
            }

            item.Label = channelName;
            item.Label2 = programTitle;

            string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, ch.DisplayName);
            if (!File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }

            item.IconImage = strLogo;
            item.IconImageBig = strLogo;
            item.PinImage = "";

            dlg.Add(item);
            _users.Add(user);
            if (Card != null && Card.IdChannel == idChannel)
            {
              selected = count;
            }
            count++;
          }
        }
      }
      if (channels.Count == 0)
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        if (pDlgOK != null)
        {
          pDlgOK.SetHeading(200052); //my tv
          pDlgOK.SetLine(1, GUILocalizeStrings.Get(200053)); // No Active recordings
          pDlgOK.SetLine(2, "");
          pDlgOK.DoModal(this.GetID);
        }
        return;
      }
      dlg.SelectedLabel = selected;
      dlg.DoModal(this.GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }

      if (recordingSchedules.Count == 0)
      {
        return;
      }
      Schedule sched = recordingSchedules[dlg.SelectedLabel];
      Program prg2use = Program.RetrieveByTitleAndTimes(sched.ProgramName, sched.StartTime, sched.EndTime);
      PromptAndDeleteRecordingSchedule(sched.IdSchedule, prg2use, false, false);
      OnActiveRecordings(); //keep on showing the list until --> 1) user leaves menu, 2) no more active recordings
    }

    private void OnActiveStreams()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(692); // Active Tv Streams
      int selected = 0;

      IList<Card> cards = TvDatabase.Card.ListAll();
      List<Channel> channels = new List<Channel>();
      int count = 0;
      TvServer server = new TvServer();
      List<User> _users = new List<User>();
      foreach (Card card in cards)
      {
        if (card.Enabled == false)
        {
          continue;
        }
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          continue;
        }
        User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        if (users == null)
        {
          return;
        }
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (card.IdCard != user.CardId)
          {
            continue;
          }
          bool isRecording;
          bool isTimeShifting;
          VirtualCard tvcard = new VirtualCard(user, RemoteControl.HostName);
          isRecording = tvcard.IsRecording;
          isTimeShifting = tvcard.IsTimeShifting;
          if (isTimeShifting || (isRecording && !isTimeShifting))
          {
            int idChannel = tvcard.IdChannel;
            user = tvcard.User;
            Channel ch = Channel.Retrieve(idChannel);
            channels.Add(ch);
            GUIListItem item = new GUIListItem();
            item.Label = ch.DisplayName;
            item.Label2 = user.Name;
            string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, ch.DisplayName);
            if (!File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            item.IconImage = strLogo;
            if (isRecording)
            {
              item.PinImage = Thumbs.TvRecordingIcon;
            }
            else
            {
              item.PinImage = "";
            }
            dlg.Add(item);
            _users.Add(user);
            if (Card != null && Card.IdChannel == idChannel)
            {
              selected = count;
            }
            count++;
          }
        }
      }
      if (channels.Count == 0)
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        if (pDlgOK != null)
        {
          pDlgOK.SetHeading(692); //my tv
          pDlgOK.SetLine(1, GUILocalizeStrings.Get(1511)); // No Active streams
          pDlgOK.SetLine(2, "");
          pDlgOK.DoModal(this.GetID);
        }
        return;
      }
      dlg.SelectedLabel = selected;
      dlg.DoModal(this.GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }

      VirtualCard vCard = new VirtualCard(_users[dlg.SelectedLabel], RemoteControl.HostName);
      Channel channel = Navigator.GetChannel(vCard.IdChannel);
      ViewChannel(channel);
    }

    private void OnRecord()
    {
      ManualRecord(Navigator.Channel);
      UpdateStateOfRecButton();
    }

    /// <summary>
    /// Update the state of the following buttons    
    /// - record now
    /// </summary>
    private void UpdateStateOfRecButton()
    {
      if (!Connected)
      {
        btnTvOnOff.Selected = false;
        return;
      }
      bool isTimeShifting = Card.IsTimeShifting;

      //are we recording a tv program?      
      if (Navigator.Channel != null && Card != null)
      {
        string label;
        TvServer server = new TvServer();
        VirtualCard vc;
        if (server.IsRecording(Navigator.Channel.Name, out vc))
        {
          if (!isTimeShifting)
          {
            Card = vc;
          }
          //yes then disable the timeshifting on/off buttons
          //and change the Record Now button into Stop Record
          label = GUILocalizeStrings.Get(629); //stop record
        }
        else
        {
          //nop. then change the Record Now button
          //to Record Now
          label = GUILocalizeStrings.Get(601); // record
        }
        if (label != btnRecord.Label)
        {
          btnRecord.Label = label;
        }
      }
    }

    private void UpdateRecordingIndicator()
    {
      DateTime now = DateTime.Now;

      // if we're recording tv, update gui with info
      if (Connected && Card.IsRecording)
      {
        /*if (lastRecordTime == DateTime.MinValue)
        {
          lastRecordTime = now;
          
        }
        */

        //int card;
        int scheduleId = Card.RecordingScheduleId;
        if (scheduleId > 0)
        {
          Schedule schedule = Schedule.Retrieve(scheduleId);
          if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
          {
            imgRecordingIcon.SetFileName(Thumbs.TvRecordingIcon);
          }
          else
          {
            imgRecordingIcon.SetFileName(Thumbs.TvRecordingSeriesIcon);
          }
        }
      }
      else
      {
        // if Recording hasn't been active for over 5 sec. then reset the lastActiveRecChannelId var)        
        if (lastRecordTime != DateTime.MinValue && lastActiveRecChannelId > 0)
        {
          TimeSpan ts = now - lastRecordTime;
          if (ts.TotalSeconds > 5)
          {
            lastActiveRecChannelId = 0;
            lastRecordTime = DateTime.MinValue;
          }
        }
        imgRecordingIcon.IsVisible = false;
      }
    }

    /// <summary>
    /// Update the the progressbar in the GUI which shows
    /// how much of the current tv program has elapsed
    /// </summary>
    public static void UpdateProgressPercentageBar()
    {
      TimeSpan ts = DateTime.Now - _updateProgressTimer;
      if (ts.TotalMilliseconds < 1000)
      {
        return;
      }
      _updateProgressTimer = DateTime.Now;

      if (!Connected)
      {
        return;
      }

      if (g_Player.Playing && g_Player.IsTimeShifting)
      {
        /*if (TVHome.Card != null)
        {
          if (TVHome.Card.IsTimeShifting == false)
          {
            g_Player.Stop();
          }
        }*/
        if ((g_Player.currentDescription.Length == 0) &&
            (GUIPropertyManager.GetProperty("#TV.View.description").Length != 0))
        {
          GUIPropertyManager.SetProperty("#TV.View.channel", "");
          GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.title", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.subtitle", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.episode", String.Empty);

        }
      }

      //set audio video related media info properties.

      int currAudio = g_Player.CurrentAudioStream;

      if (currAudio > -1)
      {
        string streamType = g_Player.AudioType(currAudio);

        switch (streamType)
        {
          case "AC3":
            GUIPropertyManager.SetProperty("#TV.View.IsAC3",
                                           string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                         "ac3.png"));
            GUIPropertyManager.SetProperty("#TV.View.IsMP1A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsMP2A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAAC", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC", string.Empty);
            break;
          case "Mpeg1":
            GUIPropertyManager.SetProperty("#TV.View.IsMP1A",
                                           string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                         "mp1a.png"));
            GUIPropertyManager.SetProperty("#TV.View.IsAC3", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsMP2A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAAC", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC", string.Empty);
            break;
          case "Mpeg2":
            GUIPropertyManager.SetProperty("#TV.View.IsMP2A",
                                           string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                         "mp2a.png"));
            GUIPropertyManager.SetProperty("#TV.View.IsMP1A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAC3", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAAC", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC", string.Empty);
            break;
          case "AAC":
            GUIPropertyManager.SetProperty("#TV.View.IsAAC",
                                           string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                         "aac.png"));
            GUIPropertyManager.SetProperty("#TV.View.IsMP1A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsMP2A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAC3", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC", string.Empty);
            break;
          case "LATMAAC":
            GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC",
                                           string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                         "latmaac3.png"));
            GUIPropertyManager.SetProperty("#TV.View.IsMP1A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsMP2A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAAC", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAC3", string.Empty);
            break;
          default:
            GUIPropertyManager.SetProperty("#TV.View.IsAC3", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsMP1A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsMP2A", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsAAC", string.Empty);
            GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC", string.Empty);
            break;
        }
      }


      if (!g_Player.IsTVRecording) //playing live TV
      {
        if (Navigator.Channel == null)
        {
          return;
        }
        try
        {
          if (Navigator.CurrentChannel == null)
          {
            GUIPropertyManager.SetProperty("#TV.View.channel", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.title", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.subtitle", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.episode", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
            GUIPropertyManager.SetProperty("#TV.View.remaining", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.genre", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.start", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.stop", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.genre", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.title", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.subtitle", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.description", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.episode", String.Empty);
            return;
          }

          GUIPropertyManager.SetProperty("#TV.View.channel", Navigator.CurrentChannel);
          GUIPropertyManager.SetProperty("#TV.View.title", Navigator.CurrentChannel);
          Program current = Navigator.Channel.CurrentProgram;

          if (current != null)
          {
            GUIPropertyManager.SetProperty("#TV.View.start",
                                           current.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.View.stop",
                                           current.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.View.remaining",
                                           Utils.SecondsToHMSString(current.EndTime - current.StartTime));
            GUIPropertyManager.SetProperty("#TV.View.genre", current.Genre);
            GUIPropertyManager.SetProperty("#TV.View.title", TVUtil.GetDisplayTitle(current));
            GUIPropertyManager.SetProperty("#TV.View.description", current.Description);
            GUIPropertyManager.SetProperty("#TV.View.subtitle", current.EpisodeName);
            GUIPropertyManager.SetProperty("#TV.View.episode", current.EpisodeNumber);

          }
          else
          {
            GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(736)); // no epg for this channel
          }

          Program next = Navigator.Channel.NextProgram;
          if (next != null)
          {
            GUIPropertyManager.SetProperty("#TV.Next.start",
                                           next.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Next.stop",
                                           next.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Next.remaining", Utils.SecondsToHMSString(next.EndTime - next.StartTime));
            GUIPropertyManager.SetProperty("#TV.Next.genre", next.Genre);
            GUIPropertyManager.SetProperty("#TV.Next.title", TVUtil.GetDisplayTitle(next));
            GUIPropertyManager.SetProperty("#TV.Next.description", next.Description);
            GUIPropertyManager.SetProperty("#TV.Next.subtitle", next.EpisodeName);
            GUIPropertyManager.SetProperty("#TV.Next.episode", next.EpisodeNumber);
          }
          else
          {
            GUIPropertyManager.SetProperty("#TV.Next.title", GUILocalizeStrings.Get(736)); // no epg for this channel
          }

          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, Navigator.CurrentChannel);
          if (!File.Exists(strLogo))
          {
            strLogo = "defaultVideoBig.png";
          }
          GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);

          //get current tv program
          Program prog = Navigator.Channel.CurrentProgram;
          bool clearPrgProperties = false;
          clearPrgProperties = (prog == null);

          if (!clearPrgProperties)
          {
            ts = prog.EndTime - prog.StartTime;
            clearPrgProperties = (ts.TotalSeconds <= 0);
          }

          if (clearPrgProperties)
          {
            GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent1", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");
            return;
          }

          // caclulate total duration of the current program
          ts = (prog.EndTime - prog.StartTime);
          double programDuration = ts.TotalSeconds;

          //calculate where the program is at this time
          ts = (DateTime.Now - prog.StartTime);
          double livePoint = ts.TotalSeconds;

          //calculate when timeshifting was started
          double timeShiftStartPoint = livePoint - g_Player.Duration;
          double playingPoint = timeShiftStartPoint + g_Player.CurrentPosition;
          if (timeShiftStartPoint < 0)
          {
            timeShiftStartPoint = 0;
          }


          double timeShiftStartPointPercent = ((double)timeShiftStartPoint) / ((double)programDuration);
          timeShiftStartPointPercent *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.Record.percent1", ((int)timeShiftStartPointPercent).ToString());

          //if (!g_Player.Paused) // remarked by LKuech too fix the bug that the current position was not shown correctly when paused live tv
          //{
          double playingPointPercent = ((double)playingPoint) / ((double)programDuration);
          playingPointPercent *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.Record.percent2", ((int)playingPointPercent).ToString());
          //}

          double percentLivePoint = ((double)livePoint) / ((double)programDuration);
          percentLivePoint *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)percentLivePoint).ToString());
          GUIPropertyManager.SetProperty("#TV.Record.percent3", ((int)percentLivePoint).ToString());
        }

        catch (Exception ex)
        {
          Log.Info("UpdateProgressPercentageBar:{0}", ex.Source, ex.StackTrace);
        }
      }
      else //recording is playing
      {
        double currentPosition = (double)(g_Player.CurrentPosition);
        double duration = (double)(g_Player.Duration);

        string startTime = Utils.SecondsToHMSString((int)currentPosition);
        string endTime = Utils.SecondsToHMSString((int)duration);

        double percentLivePoint = ((double)currentPosition) / ((double)duration);
        percentLivePoint *= 100.0d;

        GUIPropertyManager.SetProperty("#TV.Record.percent1", ((int)percentLivePoint).ToString());
        GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
        GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");

        Recording rec = TvRecorded.ActiveRecording();
        string displayName = "";
        if (rec != null && rec.ReferencedChannel() != null)
        {
          displayName = rec.ReferencedChannel().DisplayName;
        }

        GUIPropertyManager.SetProperty("#TV.View.channel", displayName + " (" + GUILocalizeStrings.Get(604) + ")");
        GUIPropertyManager.SetProperty("#TV.View.title", g_Player.currentTitle);
        GUIPropertyManager.SetProperty("#TV.View.description", g_Player.currentDescription);

        GUIPropertyManager.SetProperty("#TV.View.start", startTime);
        GUIPropertyManager.SetProperty("#TV.View.stop", endTime);
        if (rec != null)
        {
          GUIPropertyManager.SetProperty("#TV.View.title", TVUtil.GetDisplayTitle(rec));
          GUIPropertyManager.SetProperty("#TV.View.subtitle", rec.EpisodeName);
          GUIPropertyManager.SetProperty("#TV.View.episode", rec.EpisodeNumber);
        }

        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, displayName);
        if (File.Exists(strLogo))
        {
          GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.View.thumb", "defaultVideoBig.png");
        }
        //GUIPropertyManager.SetProperty("#TV.View.remaining", Utils.SecondsToHMSString(prog.EndTime - prog.StartTime));                
      }
    }

    /// <summary>
    /// When called this method will switch to the previous TV channel
    /// </summary>
    public static void OnPreviousChannel()
    {
      Log.Info("TVHome:OnPreviousChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)Window.WINDOW_TVFULLSCREEN);
        if (TVWindow != null)
        {
          TVWindow.ZapPreviousChannel();
        }
        return;
      }

      // Zap to previous channel immediately
      Navigator.ZapToPreviousChannel(false);
    }

    public static int GetPreferedAudioStreamIndex(out eAudioDualMonoMode dualMonoMode) // also used from tvrecorded class
    {
      int idxFirstAc3 = -1; // the index of the first avail. ac3 found
      int idxFirstmpeg = -1; // the index of the first avail. mpg found
      int idxLangAc3 = -1; // the index of ac3 found based on lang. pref
      int idxLangmpeg = -1; // the index of mpg found based on lang. pref   
      int idx = -1; // the chosen audio index we return
      string langSel = ""; // find audio based on this language.
      string ac3BasedOnLang = ""; // for debugging, what lang. in prefs. where used to choose the ac3 audio track ?
      string mpegBasedOnLang = ""; // for debugging, what lang. in prefs. where used to choose the mpeg audio track ?
      dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      IAudioStream[] streams;

      List<IAudioStream> streamsList = new List<IAudioStream>();
      for (int i = 0; i < g_Player.AudioStreams; i++)
      {
        DVBAudioStream stream = new DVBAudioStream();

        string streamType = g_Player.AudioType(i);

        switch (streamType)
        {
          case "AC3":
            stream.StreamType = AudioStreamType.AC3;
            break;
          case "Mpeg1":
            stream.StreamType = AudioStreamType.Mpeg1;
            break;
          case "Mpeg2":
            stream.StreamType = AudioStreamType.Mpeg2;
            break;
          case "AAC":
            stream.StreamType = AudioStreamType.AAC;
            break;
          case "LATMAAC":
            stream.StreamType = AudioStreamType.LATMAAC;
            break;
          default:
            stream.StreamType = AudioStreamType.Unknown;
            break;
        }

        stream.Language = g_Player.AudioLanguage(i);
        streamsList.Add(stream);
      }
      streams = (IAudioStream[])streamsList.ToArray();

      if (_preferredLanguages != null)
      {
        Log.Debug(
          "TVHome.GetPreferedAudioStreamIndex(): preferred LANG(s):{0} preferAC3:{1} preferAudioTypeOverLang:{2}",
          String.Join(";", _preferredLanguages.ToArray()), _preferAC3, _preferAudioTypeOverLang);
      }
      else
      {
        Log.Debug(
          "TVHome.GetPreferedAudioStreamIndex(): preferred LANG(s):{0} preferAC3:{1} _preferAudioTypeOverLang:{2}",
          "n/a", _preferAC3, _preferAudioTypeOverLang);
      }
      Log.Debug("Audio streams avail: {0}", streams.Length);

      if (streams.Length == 1)
      {
        Log.Info("Audio stream: switching to preferred AC3/MPEG audio stream 0 (only 1 track avail.)");
        return 0;
      }

      int priority = int.MaxValue;
      for (int i = 0; i < streams.Length; i++)
      {
        //tag the first found ac3 and mpeg indexes
        if (streams[i].StreamType == AudioStreamType.AC3)
        {
          if (idxFirstAc3 == -1)
          {
            idxFirstAc3 = i;
          }
        }
        else
        {
          if (idxFirstmpeg == -1)
          {
            idxFirstmpeg = i;
          }
        }

        //now find the ones based on LANG prefs.
        if (_preferredLanguages != null)
        {          
          if (g_Player.GetAudioDualMonoMode() != eAudioDualMonoMode.UNSUPPORTED && streams[i].Language.Length == 6)
          {            
            string leftAudioLang = streams[i].Language.Substring(0,3);
            string rightAudioLang = streams[i].Language.Substring(3, 3);

            int indexLeft = _preferredLanguages.IndexOf(leftAudioLang);
            if (indexLeft >= 0 && indexLeft < priority)
            {
              dualMonoMode = eAudioDualMonoMode.LEFT_MONO;
              mpegBasedOnLang = leftAudioLang;
              idxLangmpeg = i;
              priority = indexLeft;           
            }            
                          
            int indexRight = _preferredLanguages.IndexOf(rightAudioLang);
            if (indexRight >= 0 && indexRight < priority)
            {                
              dualMonoMode = eAudioDualMonoMode.RIGHT_MONO;
              mpegBasedOnLang = rightAudioLang;
              idxLangmpeg = i;
              priority = indexRight;
            }                        
          }
          else
          {
            int index = _preferredLanguages.IndexOf(streams[i].Language);
            langSel = streams[i].Language;
            //Log.Debug(streams[i].Language + " Pref index " + index);
            Log.Debug("Stream {0} lang {1}, preffered index {2}", i, langSel, index);

            if (index >= 0 && index < priority)
            {
              if ((streams[i].StreamType == AudioStreamType.AC3)) //is the audio track an AC3 track ?
              {
                idxLangAc3 = i;
                ac3BasedOnLang = langSel;
              }
              else //audiotrack is mpeg
              {
                idxLangmpeg = i;
                mpegBasedOnLang = langSel;
              }
              Log.Debug("Setting as pref");
              priority = index;
            }
          }
        }
        if (idxFirstAc3 > -1 && idxFirstmpeg > -1 && idxLangAc3 > -1 && idxLangmpeg > -1)
        {
          break;
        }
      }

      if (_preferAC3)
      {
        if (_preferredLanguages != null)
        {
          //did we find an ac3 track that matches our LANG prefs ?
          if (idxLangAc3 > -1)
          {
            idx = idxLangAc3;
            Log.Info("Audio stream: switching to preferred AC3 audio stream {0}, based on LANG {1}", idx, ac3BasedOnLang);
          }
          //if not, did we even find an ac3 track ?
          else if (idxFirstAc3 > -1)
          {
            //we did find an AC3 track, but not based on LANG - should we choose this or the mpeg track which is based on LANG.
            if (_preferAudioTypeOverLang || (idxLangmpeg == -1 && _preferAudioTypeOverLang))
            {
              idx = idxFirstAc3;
              Log.Info(
                "Audio stream: switching to preferred AC3 audio stream {0}, NOT based on LANG (none avail. matching {1})",
                idx, ac3BasedOnLang);
            }
            else
            {
              Log.Info("Audio stream: ignoring AC3 audio stream {0}", idxFirstAc3);
            }
          }
          //if not then proceed with mpeg lang. selection below.
        }
        else
        {
          //did we find an ac3 track ?
          if (idxFirstAc3 > -1)
          {
            idx = idxFirstAc3;
            Log.Info("Audio stream: switching to preferred AC3 audio stream {0}, NOT based on LANG", idx);
          }
          //if not then proceed with mpeg lang. selection below.
        }
      }

      if (idx == -1 && _preferAC3)
      {
        Log.Info("Audio stream: no preferred AC3 audio stream found, trying mpeg instead.");
      }

      if (idx == -1 || !_preferAC3)
      // we end up here if ac3 selection didnt happen (no ac3 avail.) or if preferac3 is disabled.
      {
        if (_preferredLanguages != null)
        {
          //did we find a mpeg track that matches our LANG prefs ?
          if (idxLangmpeg > -1)
          {
            idx = idxLangmpeg;
            Log.Info("Audio stream: switching to preferred MPEG audio stream {0}, based on LANG {1}", idx,
                     mpegBasedOnLang);
          }
          //if not, did we even find a mpeg track ?
          else if (idxFirstmpeg > -1)
          {
            //we did find a AC3 track, but not based on LANG - should we choose this or the mpeg track which is based on LANG.
            if (_preferAudioTypeOverLang || (idxLangAc3 == -1 && _preferAudioTypeOverLang))
            {
              idx = idxFirstmpeg;
              Log.Info(
                "Audio stream: switching to preferred MPEG audio stream {0}, NOT based on LANG (none avail. matching {1})",
                idx, mpegBasedOnLang);
            }
            else
            {
              if (idxLangAc3 > -1)
              {
                idx = idxLangAc3;
                Log.Info("Audio stream: ignoring MPEG audio stream {0}", idx);
              }
            }
          }
        }
        else
        {
          idx = idxFirstmpeg;
          Log.Info("Audio stream: switching to preferred MPEG audio stream {0}, NOT based on LANG", idx);
        }
      }

      if (idx == -1)
      {
        idx = 0;
        Log.Info("Audio stream: switching to preferred AC3/MPEG audio stream {0}", idx);
      }

      return idx;
    }

    private static void ChannelTuneFailedNotifyUser(TvResult succeeded, bool wasPlaying, Channel channel)
    {
      GUIGraphicsContext.RenderBlackImage = false;
      string errorMessage = GUILocalizeStrings.Get(1500);
      switch (succeeded)
      {
        case TvResult.NoSignalDetected:
          errorMessage += "\r" + GUILocalizeStrings.Get(1499) + "\r";
          break;
        case TvResult.CardIsDisabled:
          errorMessage += "\r" + GUILocalizeStrings.Get(1501) + "\r";
          break;
        case TvResult.AllCardsBusy:
          errorMessage += "\r" + GUILocalizeStrings.Get(1502) + "\r";
          break;
        case TvResult.ChannelIsScrambled:
          errorMessage += "\r" + GUILocalizeStrings.Get(1503) + "\r";
          break;
        case TvResult.NoVideoAudioDetected:
          errorMessage += "\r" + GUILocalizeStrings.Get(1504) + "\r";
          break;
        case TvResult.UnableToStartGraph:
          errorMessage += "\r" + GUILocalizeStrings.Get(1505) + "\r";
          break;
        case TvResult.UnknownError:
          // this error can also happen if we have no connection to the server.
          if (!Connected || !RemoteControl.IsConnected)
          {
            errorMessage += "\r" + GUILocalizeStrings.Get(1510) + "\r"; // Connection to TV server lost
          }
          else
          {
            errorMessage += "\r" + GUILocalizeStrings.Get(1506) + "\r";
          }
          break;
        case TvResult.UnknownChannel:
          errorMessage += "\r" + GUILocalizeStrings.Get(1507) + "\r";
          break;
        case TvResult.ChannelNotMappedToAnyCard:
          errorMessage += "\r" + GUILocalizeStrings.Get(1508) + "\r";
          break;
        case TvResult.NoTuningDetails:
          errorMessage += "\r" + GUILocalizeStrings.Get(1509) + "\r";
          break;
        case TvResult.GraphBuildingFailed:
          errorMessage += "\r" + GUILocalizeStrings.Get(1518) + "\r";
          break;
        case TvResult.SWEncoderMissing:
          errorMessage += "\r" + GUILocalizeStrings.Get(1519) + "\r";
          break;
        case TvResult.NoFreeDiskSpace:
          errorMessage += "\r" + GUILocalizeStrings.Get(1520) + "\r";
          break;
        default:
          // this error can also happen if we have no connection to the server.
          if (!Connected || !RemoteControl.IsConnected)
          {
            errorMessage += "\r" + GUILocalizeStrings.Get(1510) + "\r"; // Connection to TV server lost
          }
          else
          {
            errorMessage += "\r" + GUILocalizeStrings.Get(1506) + "\r";
          }
          break;
      }
      if (wasPlaying) //show yes no dialogue
      {
        GUIDialogYesNo pDlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
        string[] lines = errorMessage.Split('\r');
        string caption = GUILocalizeStrings.Get(605) + " - " + GUILocalizeStrings.Get(1512);
        pDlgYesNo.SetHeading(caption); //my tv
        pDlgYesNo.SetLine(1, channel.DisplayName);
        pDlgYesNo.SetLine(2, lines[0]);
        if (lines.Length > 1)
        {
          pDlgYesNo.SetLine(3, lines[1]);
        }
        else
        {
          pDlgYesNo.SetLine(3, "");
        }
        if (lines.Length > 2)
        {
          pDlgYesNo.SetLine(4, lines[2]);
        }
        else
        {
          pDlgYesNo.SetLine(4, "");
        }

        if (GUIWindowManager.ActiveWindow == (int)(int)Window.WINDOW_TVFULLSCREEN)
        {
          pDlgYesNo.DoModal((int)GUIWindowManager.ActiveWindowEx);
          // If failed and wasPlaying TV, fallback to the last viewed channel. 

          if (pDlgYesNo.IsConfirmed)
          {
            ViewChannelAndCheck(Navigator.Channel);
            //GUIWaitCursor.Hide();
          }
        }
        else
        {
          if (OnShowDlgCompleted == null)
          {
            OnShowDlgCompleted += new ShowDlgSuccessful(ShowDlgCompleted);
          }
          ParameterizedThreadStart pThread = new ParameterizedThreadStart(ShowDlgThread);
          Thread showDlgThread = new Thread(pThread);
          showDlgThread.IsBackground = true;
          //GUIWaitCursor.Hide();						
          // show the dialog asynch.
          // this fixes a hang situation that would happen when resuming TV with showlastactivemodule
          showDlgThread.Start(pDlgYesNo);
        }
      }
      else //show ok
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        string[] lines = errorMessage.Split('\r');
        pDlgOK.SetHeading(605); //my tv
        pDlgOK.SetLine(1, channel.DisplayName);
        pDlgOK.SetLine(2, lines[0]);
        if (lines.Length > 1)
        {
          pDlgOK.SetLine(3, lines[1]);
        }
        else
        {
          pDlgOK.SetLine(3, "");
        }
        if (lines.Length > 2)
        {
          pDlgOK.SetLine(4, lines[2]);
        }
        else
        {
          pDlgOK.SetLine(4, "");
        }

        if (GUIWindowManager.ActiveWindow == (int)(int)Window.WINDOW_TVFULLSCREEN)
        {
          pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
        }
        else
        {
          if (OnShowDlgCompleted == null)
          {
            OnShowDlgCompleted += new ShowDlgSuccessful(ShowDlgCompleted);
          }

          ParameterizedThreadStart pThread = new ParameterizedThreadStart(ShowDlgThread);
          Thread showDlgThread = new Thread(pThread);
          showDlgThread.IsBackground = true;
          // show the dialog asynch.
          // this fixes a hang situation that would happen when resuming TV with showlastactivemodule
          showDlgThread.Start(pDlgOK);
        }
      }
    }

    /*
    // for joboehl
    private static bool isChannelAnalogue(Channel ch)
    {
      if (ch.ReferringTuningDetail() != null)
      {
        foreach (TuningDetail td in ch.ReferringTuningDetail())
        {
          if (td.ChannelType == 0)
          {
            return true;
          }
        }
      }
      return false;
    }
    */

    private static void OnBlackImageRendered()
    {
      if (GUIGraphicsContext.RenderBlackImage)
      {
        //MediaPortal.GUI.Library.Log.Debug("TvHome.OnBlackImageRendered()");
        _waitForBlackScreen.Set();
      }
    }

    private static void OnVideoReceived()
    {
      if (GUIGraphicsContext.RenderBlackImage)
      {
        Log.Debug("TvHome.OnVideoReceived() {0}", FramesBeforeStopRenderBlackImage);
        if (FramesBeforeStopRenderBlackImage != 0)
        {
          FramesBeforeStopRenderBlackImage--;
          if (FramesBeforeStopRenderBlackImage == 0)
          {
            GUIGraphicsContext.RenderBlackImage = false;
            Log.Debug("TvHome.StopRenderBlackImage()");
          }
        }
      }
    }

    private static void StopRenderBlackImage()
    {
      if (GUIGraphicsContext.RenderBlackImage)
      {
        FramesBeforeStopRenderBlackImage = 3;
        // Ambass : we need to wait the 3rd frame to avoid persistance of previous channel....Why ?????
      }
    }

    private static void RenderBlackImage()
    {
      if (GUIGraphicsContext.RenderBlackImage == false)
      {
        Log.Debug("TvHome.RenderBlackImage()");
        _waitForBlackScreen.Reset();
        GUIGraphicsContext.RenderBlackImage = true;
        _waitForBlackScreen.WaitOne(1000, false);
      }
    }

    public static bool ViewChannelAndCheck(Channel channel)
    {
      //GUIWaitCursor.Show();
      _doingChannelChange = false;
      bool cardChanged = false;

      if (!_resumed && _suspended && _waitonresume > 0)
      {
        Log.Info("TVHome.ViewChannelAndCheck(): system just woke up...waiting {0} ms. resumed {1}, suspended {2}", _waitonresume, _resumed, _suspended);
        Thread.Sleep(_waitonresume);
      }

      _waitForVideoReceived.Reset();
      _waitForVideoReceived.Reset();

      //System.Diagnostics.Debugger.Launch();
      try
      {
        if (channel == null)
        {
          Log.Info("TVHome.ViewChannelAndCheck(): channel==null");
          //GUIWaitCursor.Hide();
          return false;
        }
        Log.Info("TVHome.ViewChannelAndCheck(): View channel={0}", channel.DisplayName);

        //if a channel is untunable, then there is no reason to carry on or even stop playback.


        //if (!isChannelAnalogue(channel))
        //{
        int CurrentChanState = (int)TvServer.GetChannelState(channel.IdChannel, Card.User);
        if (CurrentChanState == (int)ChannelState.nottunable)
        {
          ChannelTuneFailedNotifyUser(TvResult.AllCardsBusy, false, channel);
          //GUIWaitCursor.Hide();
          return false;
        }
        //}

        //BAV: fixing mantis bug 1263: TV starts with no video if Radio is previously ON & channel selected from TV guide
        if ((!channel.IsRadio && g_Player.IsRadio) || (channel.IsRadio && !g_Player.IsRadio))
        {
          Log.Info("TVHome.ViewChannelAndCheck(): Stop g_Player");
          g_Player.Stop(true);
        }
        // do we stop the player when changing channel ?
        // _userChannelChanged is true if user did interactively change the channel, like with mini ch. list. etc.
        if (!_userChannelChanged)
        {
          if (g_Player.IsTVRecording)
          {
            //GUIWaitCursor.Hide();
            return true;
          }
          if (!_autoTurnOnTv) //respect the autoturnontv setting.
          {
            if (g_Player.IsVideo || g_Player.IsDVD || g_Player.IsMusic)
            {
              //GUIWaitCursor.Hide();
              return true;
            }
          }
          else
          {
            if (g_Player.IsVideo || g_Player.IsDVD || g_Player.IsMusic || g_Player.IsCDA) // || g_Player.IsRadio)
            {
              g_Player.Stop(true); // tell that we are zapping so exclusive mode is not going to be disabled
            }
          }
        }
        else if (g_Player.IsTVRecording && _userChannelChanged)
        //we are watching a recording, we have now issued a ch. change..stop the player.
        {
          _userChannelChanged = false;
          g_Player.Stop(true);
        }
        else if ((channel.IsTv && g_Player.IsRadio) || (channel.IsRadio && g_Player.IsTV) || g_Player.IsCDA ||
                 g_Player.IsMusic || g_Player.IsVideo)
        {
          g_Player.Stop(true);
        }

        /* A part of the code to implement IP-TV 
        if (channel.IsWebstream())
        {
          IList details = channel.ReferringTuningDetail();
          TuningDetail detail = (TuningDetail)details[0];
          g_Player.PlayVideoStream(detail.Url, channel.DisplayName);
          return true;
        }
        else
        {
          if (Navigator.LastViewedChannel.IsWebstream())
            g_Player.Stop();
        }*/

        TvResult succeeded;
        if (Card != null)
        {
          if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
          //modified by joboehl. Avoids other video being played instead of TV. 
          {
            //if we're already watching this channel, then simply return
            if (Card.IsTimeShifting == true && Card.IdChannel == channel.IdChannel)
            {
              //GUIWaitCursor.Hide();
              return true;
            }
          }
        }

        _doingChannelChange = true;

        User user = new User();
        if (Card != null)
        {
          user.CardId = Card.Id;
        }

        bool wasPlaying = (g_Player.Playing && g_Player.IsTimeShifting && !g_Player.Stopped) &&
                          (g_Player.IsTV || g_Player.IsRadio);

        //Start timeshifting the new tv channel
        TvServer server = new TvServer();
        VirtualCard card;
        int newCardId = -1;
        if (wasPlaying)
        {
          // we need to stop player HERE if card has changed.        
          newCardId = server.TimeShiftingWouldUseCard(ref user, channel.IdChannel);

          //Added by joboehl - If any major related to the timeshifting changed during the start, restart the player.           
          if (newCardId == -1)
          {
            cardChanged = false;
          }
          else
          {
            cardChanged = (Card.Id != newCardId);
            // || TVHome.Card.RTSPUrl != newCard.RTSPUrl || TVHome.Card.TimeShiftFileName != newCard.TimeShiftFileName);
          }

          if (cardChanged)
          {
            Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. CardId:{0}/{1}, RTSP:{2}", Card.Id, newCardId, Card.RTSPUrl);
            Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. Timeshifting:{0}", Card.TimeShiftFileName);
            //g_Player.StopAndKeepTimeShifting(); // keep timeshifting on server, we only want to recreate the graph on the client
            //server.StopTimeShifting(ref user);              
            Log.Debug("TVHome.ViewChannelAndCheck(): rebuilding graph (card changed) - timeshifting continueing.");

            RenderBlackImage();
            g_Player.PauseGraph();
            succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);
            // check if after starttimeshift the active card is same as before (tvserver can do "failover" to another card)
            bool isSingleSeat = IsSingleSeat();
            if (succeeded == TvResult.Succeeded && card != null && Card.Id == card.Id)
            {
              Log.Debug("TVHome.ViewChannelAndCheck(): card was not changed. seek to end.");
              cardChanged = false;
              //hack - seek2end will cause a huge delay on singleseat systems, but multiseat needs it.
              if (!isSingleSeat)
              {
                SeekToEnd(true);
              }
            }
            //hack - seek2end will cause a huge delay on singleseat systems, but multiseat needs it.
            if (isSingleSeat)
            {
              SeekToEnd(true);
            }
            g_Player.ContinueGraph();
          }
          else //card "probably" not changed.
          {
            // PauseGraph & ContinueGraph does add a bit overhead to channel change times                        
            RenderBlackImage();
            g_Player.PauseGraph();
            g_Player.OnZapping(0x80);           // Setup Zapping for TsReader
            succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);
            if (card != null)
              g_Player.OnZapping((int)card.Type);
            else
              g_Player.OnZapping(-1);            // Send Zapping failed for TsReader.

            if (succeeded == TvResult.Succeeded)
            {
              if (newCardId != card.Id && Card.Id != card.Id)
              {
                // we might have a situation on the server where card has changed in order to complete a 
                // channel change. - lets check for this.
                // if this has happened, we need to re-create graph.
                cardChanged = true;
                wasPlaying = false;
                //g_Player.StopAndKeepTimeShifting();
              }
              else if (succeeded == TvResult.Succeeded) // no card change occured, so carry on.
              {
                cardChanged = false;
                SeekToEnd(true);
                g_Player.ContinueGraph();
              }
            }
            else
            {
              cardChanged = false;
            }
          }
        }
        else //was not playing
        {
          succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);
        }

        if (succeeded == TvResult.Succeeded)
        {
          if (Navigator.Channel != null)
          {
            if (channel.IdChannel != Navigator.Channel.IdChannel || (Navigator.LastViewedChannel == null))
            {
              Navigator.LastViewedChannel = Navigator.Channel;
            }
          }
          else
          {
            Log.Info("Navigator.Channel==null");
          }

          //timeshifting succeeded					                    
          Log.Info("succeeded:{0} {1}", succeeded, card);

          Card = card; //Moved by joboehl - Only touch the card if starttimeshifting succeeded. 

          if (!g_Player.Playing || cardChanged)
          {
            StartPlay();
          }

          _playbackStopped = false;

          //GUIWaitCursor.Hide();
          _doingChannelChange = false;
          _ServerNotConnectedHandled = false;
          //GUIWaitCursor.Hide();
          return true;
        }
        else
        {
          //timeshifting new channel failed. 
          g_Player.Stop();

        }

        //GUIWaitCursor.Hide();

        //GUIWaitCursor.Hide();
        ChannelTuneFailedNotifyUser(succeeded, wasPlaying, channel);

        _doingChannelChange = false;
        return false;
      }
      catch (Exception ex)
      {
        Log.Debug("TvPlugin:ViewChannelandCheck Exception {0}", ex.ToString());
        //GUIWaitCursor.Hide();
        _doingChannelChange = false;
        Card.User.Name = new User().Name;
        Card.StopTimeShifting();
        return false;
      }
      finally
      {
        StopRenderBlackImage();
      }
    }


    public static void ViewChannel(Channel channel)
    {
      ViewChannelAndCheck(channel);
      Navigator.UpdateCurrentChannel();
      UpdateProgressPercentageBar();
      return;
    }

    /// <summary>
    /// When called this method will switch to the next TV channel
    /// </summary>
    public static void OnNextChannel()
    {
      Log.Info("TVHome:OnNextChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)Window.WINDOW_TVFULLSCREEN);
        if (TVWindow != null)
        {
          TVWindow.ZapNextChannel();
        }
        return;
      }

      // Zap to next channel immediately
      Navigator.ZapToNextChannel(false);
    }

    /// <summary>
    /// When called this method will switch to the last viewed TV channel   // mPod
    /// </summary>
    public static void OnLastViewedChannel()
    {
      Navigator.ZapToLastViewedChannel();
    }

    /// <summary>
    /// Returns true if the specified window belongs to the my tv plugin
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: belongs to the my tv plugin
    /// false: does not belong to the my tv plugin</returns>
    public static bool IsTVWindow(int windowId)
    {
      if (windowId == (int)Window.WINDOW_TV)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Gets the channel navigator that can be used for channel zapping.
    /// </summary>
    public static ChannelNavigator Navigator
    {
      get { return m_navigator; }
    }

    private static void StartPlay()
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();
      if (Card == null)
      {
        Log.Info("tvhome:startplay card=null");
        return;
      }
      if (Card.IsScrambled)
      {
        Log.Info("tvhome:startplay scrambled");
        return;
      }
      Log.Info("tvhome:startplay");
      string timeshiftFileName = Card.TimeShiftFileName;
      Log.Info("tvhome:file:{0}", timeshiftFileName);

      IChannel channel = Card.Channel;
      if (channel == null)
      {
        Log.Info("tvhome:startplay channel=null");
        return;
      }
      g_Player.MediaType mediaType = g_Player.MediaType.TV;
      if (channel.IsRadio)
      {
        mediaType = g_Player.MediaType.Radio;
      }

      bool useRTSP = UseRTSP();
      bool tsFileExists = File.Exists(timeshiftFileName);


      benchClock.Stop();
      Log.Warn("tvhome:startplay.  Phase 1 - {0} ms - Done method initialization",
               benchClock.ElapsedMilliseconds.ToString());
      benchClock.Reset();
      benchClock.Start();

      if (!tsFileExists && !useRTSP) //singleseat
      {
        if (_timeshiftingpath.Length > 0)
        {
          string path = Path.GetDirectoryName(timeshiftFileName);
          int index = path.LastIndexOf("\\");

          if (index == -1)
          {
            timeshiftFileName = TimeshiftingPath() + "\\" + Path.GetFileName(timeshiftFileName);
          }
          else
          {
            timeshiftFileName = TimeshiftingPath() + path.Substring(index) + "\\" + Path.GetFileName(timeshiftFileName);
          }
        }
        else
        {
          timeshiftFileName = timeshiftFileName.Replace(":", "");
          timeshiftFileName = "\\\\" + RemoteControl.HostName + "\\" + timeshiftFileName;
        }
      }

      //_newTimeshiftFileName = timeshiftFileName;

      if (!useRTSP)
      {
        Log.Info("tvhome:startplay:{0} - using rtsp mode:{1}", timeshiftFileName, useRTSP);
        g_Player.Play(timeshiftFileName, mediaType);
        benchClock.Stop();
        Log.Warn("tvhome:startplay.  Phase 2 - {0} ms - Done starting g_Player.Play()",
                 benchClock.ElapsedMilliseconds.ToString());
        benchClock.Reset();
        //benchClock.Start();
        //SeekToEnd(false);
        //Log.Warn("tvhome:startplay.  Phase 3 - {0} ms - Done seeking.", benchClock.ElapsedMilliseconds.ToString());
      }
      else //multiseat
      {
        timeshiftFileName = Card.RTSPUrl;
        Log.Info("tvhome:startplay:{0} - using rtsp mode:{1}", timeshiftFileName, useRTSP);
        g_Player.Play(timeshiftFileName, mediaType);
        benchClock.Stop();
        Log.Warn("tvhome:startplay.  Phase 2 - {0} ms - Done starting g_Player.Play()",
                 benchClock.ElapsedMilliseconds.ToString());
        benchClock.Reset();
        //benchClock.Start();
        //SeekToEnd(true);
        //Log.Warn("tvhome:startplay.  Phase 3 - {0} ms - Done seeking.", benchClock.ElapsedMilliseconds.ToString());
        //SeekToEnd(true);
      }
      benchClock.Stop();
    }

    private static void SeekToEnd(bool zapping)
    {
      double duration = g_Player.Duration;
      double position = g_Player.CurrentPosition;

      bool useRtsp = UseRTSP();


      Log.Info("tvhome:SeektoEnd({0}, rtsp={1}", zapping, useRtsp);

      //singleseat
      if (!useRtsp)
      {
        if (duration > 0 || position > 0)
        {
          try
          {
            g_Player.SeekAbsolute(duration);
          }
          catch (Exception e)
          {
            Log.Error("tvhome:SeektoEnd({0}, rtsp={1} exception: {2}", zapping, useRtsp, e.Message);
            g_Player.Stop();
          }
        }
      }
      else
      {
        //multiseat rtsp streaming....
        if (zapping)
        {
          //System.Threading.Thread.Sleep(100);            
          Log.Info("tvhome:SeektoEnd({0}/{1})", position, duration);
          if (duration > 0 || position > 0)
          {
            try
            {
              g_Player.SeekAbsolute(duration); // + 10);
            }
            catch (Exception e)
            {
              Log.Error("tvhome:SeektoEnd({0}, rtsp={1} exception: {2}", zapping, useRtsp, e.Message);
              g_Player.Stop();
            }
          }
        }
      }
    }
  }

  #region ChannelNavigator class

  /// <summary>
  /// Handles the logic for channel zapping. This is used by the different GUI modules in the TV section.
  /// </summary>
  public class ChannelNavigator
  {
    #region config xml file

    private const string ConfigFileXml =
      @"<?xml version=|1.0| encoding=|utf-8|?> 
<ideaBlade xmlns:xsi=|http://www.w3.org/2001/XMLSchema-instance| xmlns:xsd=|http://www.w3.org/2001/XMLSchema| useDeclarativeTransactions=|false| version=|1.03|> 
  <useDTC>false</useDTC>
  <copyLocal>false</copyLocal>
  <logging>
    <archiveLogs>false</archiveLogs>
    <logFile>DebugMediaPortal.GUI.Library.Log.xml</logFile>
    <usesSeparateAppDomain>false</usesSeparateAppDomain>
    <port>0</port>
  </logging>
  <rdbKey name=|default| databaseProduct=|Unknown|>
    <connection>[CONNECTION]</connection>
    <probeAssemblyName>TVDatabase</probeAssemblyName>
  </rdbKey>
  <remoting>
    <remotePersistenceEnabled>false</remotePersistenceEnabled>
    <remoteBaseURL>http://localhost</remoteBaseURL>
    <serverPort>9009</serverPort>
    <serviceName>PersistenceServer</serviceName>
    <serverDetectTimeoutMilliseconds>-1</serverDetectTimeoutMilliseconds>
    <proxyPort>0</proxyPort>
  </remoting>
  <appUpdater/>
</ideaBlade>
";

    #endregion

    #region Private members

    private List<Channel> _channelList = new List<Channel>();

    private List<ChannelGroup> m_groups = new List<ChannelGroup>();
    // Contains all channel groups (including an "all channels" group)

    private int m_currentgroup = 0;
    private DateTime m_zaptime;
    private long m_zapdelay;
    private Channel m_zapchannel = null;
    private int m_zapgroup = -1;
    private Channel _lastViewedChannel = null; // saves the last viewed Channel  // mPod    
    private Channel m_currentChannel = null;
    private IList channels = new ArrayList();
    private bool reentrant = false;

    #endregion

    #region Constructors

    public ChannelNavigator()
    {
      // Load all groups
      //ServiceProvider services = GlobalServiceProvider.Instance;
      Log.Debug("ChannelNavigator: ctor()");
      string IpAddress;

      using (Settings xmlreader = new MPSettings())
      {
        IpAddress = xmlreader.GetValueAsString("tvservice", "hostname", "");
        if (string.IsNullOrEmpty(IpAddress) || IpAddress == "localhost")
        {
          try
          {
            IpAddress = Dns.GetHostName();

            Log.Info("TVHome: No valid hostname specified in mediaportal.xml!");
            xmlreader.SetValue("tvservice", "hostname", IpAddress);
            IpAddress = "localhost";
            Settings.SaveCache();
          }
          catch (Exception ex)
          {
            Log.Info("TVHome: Error resolving hostname - {0}", ex.Message);
            return;
          }
        }
      }
      RemoteControl.HostName = IpAddress;
      Log.Info("Remote control:master server :{0}", RemoteControl.HostName);

      ReLoad();
    }

    public void ReLoad()
    {
      try
      {
        string connectionString, provider;
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);

        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load(Config.GetFile(Config.Dir.Config, "gentle.config"));
          XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
          XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
          XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
          node.InnerText = connectionString;
          nodeProvider.InnerText = provider;
          doc.Save(Config.GetFile(Config.Dir.Config, "gentle.config"));
        }
        catch (Exception ex)
        {
          Log.Error("Unable to create/modify gentle.config {0},{1}", ex.Message, ex.StackTrace);
        }

        Log.Info("ChannelNavigator::Reload()");
        ProviderFactory.ResetGentle(true);
        ProviderFactory.SetDefaultProviderConnectionString(connectionString);
        Log.Info("get channels from database");
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddConstraint(Operator.Equals, "isTv", 1);
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        Log.Info("found:{0} tv channels", channels.Count);
        TvNotifyManager.OnNotifiesChanged();
        m_groups.Clear();

        TvBusinessLayer layer = new TvBusinessLayer();
        RadioChannelGroup allRadioChannelsGroup = layer.GetRadioChannelGroupByName(TvConstants.RadioGroupNames.AllChannels);
        IList<Channel> radioChannels = layer.GetAllRadioChannels();
        if (radioChannels != null)
        {
          if (radioChannels.Count > allRadioChannelsGroup.ReferringRadioGroupMap().Count)
          {
            foreach (Channel radioChannel in radioChannels)
            {
              layer.AddChannelToRadioGroup(radioChannel, allRadioChannelsGroup);
            }
          }
        }
        Log.Info("Done.");

        Log.Info("get all groups from database");
        sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
        sb.AddOrderByField(true, "groupName");
        stmt = sb.GetStatement(true);
        IList<ChannelGroup> groups = ObjectFactory.GetCollection<ChannelGroup>(stmt.Execute());
        IList<GroupMap> allgroupMaps = GroupMap.ListAll();

        bool hideAllChannelsGroup = false;
        using (
          Settings xmlreader =
            new MPSettings())
        {
          hideAllChannelsGroup = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
        }


        foreach (ChannelGroup group in groups)
        {
          if (group.GroupName == TvConstants.TvGroupNames.AllChannels)
          {
            foreach (Channel channel in channels)
            {
              if (channel.IsTv == false)
              {
                continue;
              }
              bool groupContainsChannel = false;
              foreach (GroupMap map in allgroupMaps)
              {
                if (map.IdGroup != group.IdGroup)
                {
                  continue;
                }
                if (map.IdChannel == channel.IdChannel)
                {
                  groupContainsChannel = true;
                  break;
                }
              }
              if (!groupContainsChannel)
              {
                layer.AddChannelToGroup(channel, TvConstants.TvGroupNames.AllChannels);
              }
            }
            break;
          }
        }

        groups = ChannelGroup.ListAll();
        foreach (ChannelGroup group in groups)
        {
          //group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);
          if (hideAllChannelsGroup && group.GroupName.Equals(TvConstants.TvGroupNames.AllChannels) && groups.Count > 1)
          {
            continue;
          }
          m_groups.Add(group);
        }
        Log.Info("loaded {0} tv groups", m_groups.Count);

        //TVHome.Connected = true;
      }
      catch (Exception ex)
      {
        Log.Error("TVHome: Error in Reload");
        Log.Error(ex);
        //TVHome.Connected = false;
      }
    }

    #endregion

    #region Public properties

    /// <summary>
    /// Gets the channel that we currently watch.
    /// Returns empty string if there is no current channel.
    /// </summary>
    public string CurrentChannel
    {
      get
      {
        if (m_currentChannel == null)
        {
          return null;
        }
        return m_currentChannel.DisplayName;
      }
    }

    public Channel Channel
    {
      get { return m_currentChannel; }
    }

    /// <summary>
    /// Gets and sets the last viewed channel
    /// Returns empty string if no zap occurred before
    /// </summary>
    public Channel LastViewedChannel
    {
      get { return _lastViewedChannel; }
      set { _lastViewedChannel = value; }
    }

    /// <summary>
    /// Gets the currently active tv channel group.
    /// </summary>
    public ChannelGroup CurrentGroup
    {
      get
      {
        if (m_groups.Count == 0)
        {
          return null;
        }
        return (ChannelGroup)m_groups[m_currentgroup];
      }
    }
    /// <summary>
    /// Gets the index of currently active tv channel group.
    /// </summary>
    public int CurrentGroupIndex
    {
      get
      {
        return m_currentgroup;
      }
    }
    /// <summary>
    /// Gets the list of tv channel groups.
    /// </summary>
    public List<ChannelGroup> Groups
    {
      get { return m_groups; }
    }

    /// <summary>
    /// Gets the channel that we will zap to. Contains the current channel if not zapping to anything.
    /// </summary>
    public Channel ZapChannel
    {
      get
      {
        if (m_zapchannel == null)
        {
          return m_currentChannel;
        }
        return m_zapchannel;
      }
    }

    /// <summary>
    /// Gets the configured zap delay (in milliseconds).
    /// </summary>
    public long ZapDelay
    {
      get { return m_zapdelay; }
    }

    /// <summary>
    /// Gets the group that we will zap to. Contains the current group name if not zapping to anything.
    /// </summary>
    public string ZapGroupName
    {
      get
      {
        if (m_zapgroup == -1)
        {
          return CurrentGroup.GroupName;
        }
        return ((ChannelGroup)m_groups[m_zapgroup]).GroupName;
      }
    }

    #endregion

    #region Public methods

    public void ZapNow()
    {
      m_zaptime = DateTime.Now.AddSeconds(-1);
      // MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapnow group:{0} current group:{0}", m_zapgroup, m_currentgroup);
      //if (m_zapchannel == null)
      //   MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapchannel==null");
      //else
      //   MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapchannel=={0}",m_zapchannel);
    }

    /// <summary>
    /// Checks if it is time to zap to a different channel. This is called during Process().
    /// </summary>
    public bool CheckChannelChange()
    {
      if (reentrant)
      {
        return false;
      }
      // BAV, 02.03.08: a channel change should not be delayed by rendering.
      //                by scipping this => 1 min delays in zapping should be avoided 
      //if (GUIGraphicsContext.InVmr9Render) return false;
      reentrant = true;
      UpdateCurrentChannel();

      // Zapping to another group or channel?
      if (m_zapgroup != -1 || m_zapchannel != null)
      {
        // Time to zap?
        if (DateTime.Now >= m_zaptime)
        {
          // Zapping to another group?
          if (m_zapgroup != -1 && m_zapgroup != m_currentgroup)
          {
            // Change current group and zap to the first channel of the group
            m_currentgroup = m_zapgroup;
            if (CurrentGroup != null && CurrentGroup.ReferringGroupMap().Count > 0)
            {
              GroupMap gm = (GroupMap)CurrentGroup.ReferringGroupMap()[0];
              Channel chan = (Channel)gm.ReferencedChannel();
              m_zapchannel = chan;
            }
          }
          m_zapgroup = -1;

          //if (m_zapchannel != m_currentchannel)
          //  lastViewedChannel = m_currentchannel;
          // Zap to desired channel
          Channel zappingTo = m_zapchannel;

          //remember to apply the new group also.
          if (m_zapchannel.CurrentGroup != null)
          {
            m_currentgroup = GetGroupIndex(m_zapchannel.CurrentGroup.GroupName);
            Log.Info("Channel change:{0} on group {1}", zappingTo.DisplayName, m_zapchannel.CurrentGroup.GroupName);
          }
          else
          {
            Log.Info("Channel change:{0}", zappingTo.DisplayName);
          }

          m_zapchannel = null;

          TVHome.ViewChannel(zappingTo);
          reentrant = false;

          return true;
        }
      }

      reentrant = false;
      return false;
    }

    /// <summary>
    /// Changes the current channel group.
    /// </summary>
    /// <param name="groupname">The name of the group to change to.</param>
    public void SetCurrentGroup(string groupname)
    {
      m_currentgroup = GetGroupIndex(groupname);
    }

    /// <summary>
    /// Changes the current channel group.
    /// </summary>
    /// <param name="groupIndex">The id of the group to change to.</param>
    public void SetCurrentGroup(int groupIndex)
    {
      m_currentgroup = groupIndex;
    }


    /// <summary>
    /// Ensures that the navigator has the correct current channel (retrieved from the Recorder).
    /// </summary>
    public void UpdateCurrentChannel()
    {
      Channel newChannel = null;
      //if current card is watching tv then use that channel
      int id;


      if (!TVHome.HandleServerNotConnected())
      {
        if (TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording)
        {
          id = TVHome.Card.IdChannel;
          if (id >= 0)
          {
            newChannel = Channel.Retrieve(id);
          }
        }
        else
        {
          // else if any card is recording
          // then get & use that channel
          TvServer server = new TvServer();
          if (server.IsAnyCardRecording())
          {
            for (int i = 0; i < server.Count; ++i)
            {
              User user = new User();
              VirtualCard card = server.CardByIndex(user, i);
              if (card.IsRecording)
              {
                id = card.IdChannel;
                if (id >= 0)
                {
                  newChannel = Channel.Retrieve(id);
                  break;
                }
              }
            }
          }
        }
        if (newChannel == null)
        {
          newChannel = m_currentChannel;
        }
        if (m_currentChannel.IdChannel != newChannel.IdChannel && newChannel != null)
        {
          m_currentChannel = newChannel;
          m_currentChannel.CurrentGroup = CurrentGroup;
        }
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelName">The channel to switch to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(Channel channel, bool useZapDelay)
    {
      Log.Debug("ChannelNavigator.ZapToChannel {0} - zapdelay {1}", channel.DisplayName, useZapDelay);
      TVHome.UserChannelChanged = true;
      m_zapchannel = channel;

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    private void GetChannels(bool refresh)
    {
      if (refresh)
      {
        _channelList = new List<Channel>();
      }
      if (_channelList == null)
      {
        _channelList = new List<Channel>();
      }
      if (_channelList.Count == 0)
      {
        try
        {
          if (TVHome.Navigator.CurrentGroup != null)
          {
            foreach (GroupMap chan in TVHome.Navigator.CurrentGroup.ReferringGroupMap())
            {
              Channel ch = chan.ReferencedChannel();
              if (ch.VisibleInGuide && ch.IsTv)
              {
                _channelList.Add(ch);
              }
            }
          }
        }
        catch
        {
        }

        if (_channelList.Count == 0)
        {
          Channel newChannel = new Channel(GUILocalizeStrings.Get(911), false, true, 0, DateTime.MinValue, false,
                                           DateTime.MinValue, 0, true, "", true, GUILocalizeStrings.Get(911));
          for (int i = 0; i < 10; ++i)
          {
            _channelList.Add(newChannel);
          }
        }
      }
    }

    /// <summary>
    /// Changes the current channel (based on channel number) after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannelNumber(int channelNr, bool useZapDelay)
    {
      IList<GroupMap> channels = CurrentGroup.ReferringGroupMap();
      if (channelNr >= 0)
      {
        Log.Debug("channels.Count {0}", channels.Count);

        bool found = false;
        int iCounter = 0;
        Channel chan;
        GetChannels(true);
        while (iCounter < channels.Count && found == false)
        {
          chan = (Channel)_channelList[iCounter];

          Log.Debug("chan {0}", chan.DisplayName);

          foreach (TuningDetail detail in chan.ReferringTuningDetail())
          {
            Log.Debug("detail nr {0} id{1}", detail.ChannelNumber, detail.IdChannel);

            if (detail.ChannelNumber == channelNr)
            {
              Log.Debug("find channel: iCounter {0}, detail.ChannelNumber {1}, detail.name {2}, channels.Count {3}",
                        iCounter, detail.ChannelNumber, detail.Name, channels.Count);
              found = true;
              ZapToChannel(iCounter + 1, useZapDelay);
            }
          }
          iCounter++;
        }
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(int channelNr, bool useZapDelay)
    {
      IList<GroupMap> channels = CurrentGroup.ReferringGroupMap();
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        GroupMap gm = (GroupMap)channels[channelNr];
        Channel chan = gm.ReferencedChannel();
        TVHome.UserChannelChanged = true;
        ZapToChannel(chan, useZapDelay);
      }
    }

    /// <summary>
    /// Changes to the next channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToNextChannel(bool useZapDelay)
    {
      Channel currentChan = null;
      int currindex;
      if (m_zapchannel == null)
      {
        currindex = GetChannelIndex(Channel);
        currentChan = Channel;
      }
      else
      {
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel 
        currentChan = Channel;
      }
      GroupMap gm;
      Channel chan;
      //check if channel is visible 
      //if not find next visible 
      do
      {
        // Step to next channel 
        currindex++;
        if (currindex >= CurrentGroup.ReferringGroupMap().Count)
        {
          currindex = 0;
        }
        gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
        chan = (Channel)gm.ReferencedChannel();
      } while (!chan.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = chan;
      Log.Info("Navigator:ZapNext {0}->{1}", currentChan.DisplayName, m_zapchannel.DisplayName);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
        {
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        }
        else
        {
          m_zaptime = DateTime.Now;
        }
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    /// <summary>
    /// Changes to the previous channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToPreviousChannel(bool useZapDelay)
    {
      Channel currentChan = null;
      int currindex;
      if (m_zapchannel == null)
      {
        currentChan = Channel;
        currindex = GetChannelIndex(Channel);
      }
      else
      {
        currentChan = m_zapchannel;
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel 
      }
      GroupMap gm;
      Channel chan;
      //check if channel is visible 
      //if not find next visible 
      do
      {
        // Step to prev channel 
        currindex--;
        if (currindex < 0)
        {
          currindex = CurrentGroup.ReferringGroupMap().Count - 1;
        }
        gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
        chan = (Channel)gm.ReferencedChannel();
      } while (!chan.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = chan;
      Log.Info("Navigator:ZapPrevious {0}->{1}",
               currentChan.DisplayName, m_zapchannel.DisplayName);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
        {
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        }
        else
        {
          m_zaptime = DateTime.Now;
        }
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    /// <summary>
    /// Changes to the next channel group.
    /// </summary>
    public void ZapToNextGroup(bool useZapDelay)
    {
      if (m_zapgroup == -1)
      {
        m_zapgroup = m_currentgroup + 1;
      }
      else
      {
        m_zapgroup = m_zapgroup + 1; // Zap from last zap group
      }

      if (m_zapgroup >= m_groups.Count)
      {
        m_zapgroup = 0;
      }

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    /// <summary>
    /// Changes to the previous channel group.
    /// </summary>
    public void ZapToPreviousGroup(bool useZapDelay)
    {
      if (m_zapgroup == -1)
      {
        m_zapgroup = m_currentgroup - 1;
      }
      else
      {
        m_zapgroup = m_zapgroup - 1;
      }

      if (m_zapgroup < 0)
      {
        m_zapgroup = m_groups.Count - 1;
      }

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    /// <summary>
    /// Zaps to the last viewed Channel (without ZapDelay).  // mPod
    /// </summary>
    public void ZapToLastViewedChannel()
    {
      if (_lastViewedChannel != null)
      {
        TVHome.UserChannelChanged = true;
        m_zapchannel = _lastViewedChannel;
        m_zaptime = DateTime.Now;
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Retrieves the index of the current channel.
    /// </summary>
    /// <returns></returns>
    private int GetChannelIndex(Channel ch)
    {
      IList<GroupMap> groupMaps = CurrentGroup.ReferringGroupMap();
      for (int i = 0; i < groupMaps.Count; i++)
      {
        GroupMap gm = (GroupMap)groupMaps[i];
        Channel chan = (Channel)gm.ReferencedChannel();
        if (chan.IdChannel == ch.IdChannel)
        {
          return i;
        }
      }
      return 0; // Not found, return first channel index
    }

    /// <summary>
    /// Retrieves the index of the group with the specified name.
    /// </summary>
    /// <param name="groupname"></param>
    /// <returns></returns>
    private int GetGroupIndex(string groupname)
    {
      for (int i = 0; i < m_groups.Count; i++)
      {
        ChannelGroup group = (ChannelGroup)m_groups[i];
        if (group.GroupName == groupname)
        {
          return i;
        }
      }
      return -1;
    }

    public Channel GetChannel(int channelId)
    {
      foreach (Channel chan in channels)
      {
        if (chan.IdChannel == channelId && chan.VisibleInGuide)
        {
          return chan;
        }
      }
      return null;
    }

    public Channel GetChannel(string channelName)
    {
      foreach (Channel chan in channels)
      {
        if (chan.DisplayName == channelName && chan.VisibleInGuide)
        {
          return chan;
        }
      }
      return null;
    }

    #endregion

    #region Serialization

    public void LoadSettings(Settings xmlreader)
    {
      Log.Info("ChannelNavigator::LoadSettings()");
      string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
      m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
      string groupname = xmlreader.GetValueAsString("mytv", "group", TvConstants.TvGroupNames.AllChannels);
      m_currentgroup = GetGroupIndex(groupname);
      if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count) // Group no longer exists?
      {
        m_currentgroup = 0;
      }

      m_currentChannel = GetChannel(currentchannelName);

      if (m_currentChannel == null)
      {
        if (m_currentgroup < m_groups.Count)
        {
          ChannelGroup group = (ChannelGroup)m_groups[m_currentgroup];
          if (group.ReferringGroupMap().Count > 0)
          {
            GroupMap gm = (GroupMap)group.ReferringGroupMap()[0];
            m_currentChannel = gm.ReferencedChannel();
          }
        }
      }

      //check if the channel does indeed belong to the group read from the XML setup file ?


      bool foundMatchingGroupName = false;

      if (m_currentChannel != null)
      {
        foreach (GroupMap groupMap in m_currentChannel.ReferringGroupMap())
        {
          if (groupMap.ReferencedChannelGroup().GroupName == groupname)
          {
            foundMatchingGroupName = true;
            break;
          }
        }
      }

      //if we still havent found the right group, then iterate through the selected group and find the channelname.      
      if (!foundMatchingGroupName && m_currentChannel != null && m_groups != null)
      {
        foreach (GroupMap groupMap in ((ChannelGroup)m_groups[m_currentgroup]).ReferringGroupMap())
        {
          if (groupMap.ReferencedChannel().DisplayName == currentchannelName)
          {
            foundMatchingGroupName = true;
            m_currentChannel = GetChannel(groupMap.ReferencedChannel().IdChannel);
            break;
          }
        }
      }


      // if the groupname does not match any of the groups assigned to the channel, then find the last group avail. (avoiding the all "channels group") for that channel and set is as the new currentgroup
      if (!foundMatchingGroupName && m_currentChannel != null && m_currentChannel.ReferringGroupMap().Count > 0)
      {
        GroupMap groupMap =
          (GroupMap)m_currentChannel.ReferringGroupMap()[m_currentChannel.ReferringGroupMap().Count - 1];
        m_currentgroup = GetGroupIndex(groupMap.ReferencedChannelGroup().GroupName);
        if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count) // Group no longer exists?
        {
          m_currentgroup = 0;
        }
      }

      if (m_currentChannel != null)
      {
        m_currentChannel.CurrentGroup = CurrentGroup;
      }
    }

    public void SaveSettings(Settings xmlwriter)
    {
      string groupName = "";
      if (CurrentGroup != null)
      {
        groupName = CurrentGroup.GroupName.Trim();
        try
        {
          if (groupName != String.Empty)
          {
            if (m_currentgroup > -1)
            {
              groupName = ((ChannelGroup)m_groups[m_currentgroup]).GroupName;
            }
            else if (m_currentChannel != null)
            {
              groupName = m_currentChannel.CurrentGroup.GroupName;
            }

            if (groupName.Length > 0)
            {
              xmlwriter.SetValue("mytv", "group", groupName);
            }
          }
        }
        catch (Exception)
        {
        }
      }

      if (m_currentChannel != null)
      {
        try
        {
          if (m_currentChannel.IsTv)
          {
            bool foundMatchingGroupName = false;

            foreach (GroupMap groupMap in m_currentChannel.ReferringGroupMap())
            {
              if (groupMap.ReferencedChannelGroup().GroupName == groupName)
              {
                foundMatchingGroupName = true;
                break;
              }
            }
            if (foundMatchingGroupName)
            {
              xmlwriter.SetValue("mytv", "channel", m_currentChannel.DisplayName);
            }
            else
            //the channel did not belong to the group, then pick the first channel avail in the group and set this as the last channel.
            {
              if (m_currentgroup > -1)
              {
                ChannelGroup cg = (ChannelGroup)m_groups[m_currentgroup];
                if (cg.ReferringGroupMap().Count > 0)
                {
                  GroupMap gm = (GroupMap)cg.ReferringGroupMap()[0];
                  xmlwriter.SetValue("mytv", "channel", gm.ReferencedChannel().DisplayName);
                }
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    #endregion
  }

  #endregion
}
