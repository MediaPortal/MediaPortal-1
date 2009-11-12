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
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
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
using System.Text;
using System.Net;

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
    
    [Flags]
    public enum LiveTvStatus
    {
      WasPlaying = 1,
      CardChange = 2,
      SeekToEnd = 4
    }

    //heartbeat related stuff
    private Thread heartBeatTransmitterThread = null;
    private static DateTime _updateProgressTimer = DateTime.MinValue;
    private static ChannelNavigator m_navigator;
    private static TVUtil _util;
    private static VirtualCard _card = null;
    private DateTime _updateTimer = DateTime.Now;
    private static bool _autoTurnOnTv = false;
    private static int _waitonresume = 0;
    public static bool settingsLoaded = false;
    private DateTime _dtlastTime = DateTime.Now;
    private TvCropManager _cropManager = new TvCropManager();
    private TvNotifyManager _notifyManager = new TvNotifyManager();
    private static List<string> _preferredLanguages;
    private static bool _usertsp = true;
    private static string _recordingpath = "";
    private static string _timeshiftingpath = "";
    private static bool _preferAC3 = false;
    private static bool _preferAudioTypeOverLang = false;
    private static bool _autoFullScreen = false;
    private static bool _autoFullScreenOnly = false;    
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

    private static BitHelper<LiveTvStatus> _status = new BitHelper<LiveTvStatus>();

    [SkinControl(2)]
    protected GUIButtonControl btnTvGuide = null;
    [SkinControl(3)]
    protected GUIButtonControl btnRecord = null;
    [SkinControl(7)]
    protected GUIButtonControl btnChannel = null;
    [SkinControl(8)]
    protected GUIToggleButtonControl btnTvOnOff = null;
    [SkinControl(13)]
    protected GUIButtonControl btnTeletext = null;
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

    // error handling
    public class ChannelErrorInfo
    {
      public Channel FailingChannel;
      public TvResult Result;
      public List<String> Messages = new List<string>();
    };
    public static ChannelErrorInfo _lastError = new ChannelErrorInfo();

    // CI Menu
    private static CiMenuHandler ciMenuHandler;
    public static GUIDialogCIMenu dlgCiMenu;
    public static GUIDialogNotify _dialogNotify = null;

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

        //Make sure that we have valid hostname for the TV server
        SetRemoteControlHostName();

        //Wake up the TV server, if required
        HandleWakeUpTvServer();

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

    private static void SetRemoteControlHostName()
    {
      string hostName;

      using (Settings xmlreader = new MPSettings())
      {
        hostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
        if (string.IsNullOrEmpty(hostName) || hostName == "localhost")
        {
          try
          {
            hostName = Dns.GetHostName();

            Log.Info("TVHome: No valid hostname specified in mediaportal.xml!");
            xmlreader.SetValue("tvservice", "hostname", hostName);
            hostName = "localhost";
            Settings.SaveCache();
          }
          catch (Exception ex)
          {
            Log.Info("TVHome: Error resolving hostname - {0}", ex.Message);
            return;
          }
        }
      }
      RemoteControl.HostName = hostName;

      Log.Info("Remote control:master server :{0}", RemoteControl.HostName);
    }

    private static void HandleWakeUpTvServer()
    {

      bool isWakeOnLanEnabled;
      bool isAutoMacAddressEnabled;
      String macAddress;
      byte[] hwAddress;

      //Get settings from MediaPortal.xml
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        isWakeOnLanEnabled = xmlreader.GetValueAsBool("tvservice", "isWakeOnLanEnabled", false);
        isAutoMacAddressEnabled = xmlreader.GetValueAsBool("tvservice", "isAutoMacAddressEnabled", false);
      }

      //isWakeOnlanEnabled
      if (isWakeOnLanEnabled)
      {
        //Check for multi-seat installation
        if (!IsSingleSeat())
        {
          WakeOnLanManager wakeOnLanManager = new WakeOnLanManager();

          //isAutoMacAddressEnabled
          if (isAutoMacAddressEnabled)
          {
            IPAddress ipAddress = null;

            //Check if we already have a valid IP address stored in RemoteControl.HostName,
            //otherwise try to resolve the IP address
            if (!IPAddress.TryParse(RemoteControl.HostName, out ipAddress))
            {
              //Get IP address of the TV server
              try
              {
                IPAddress[] ips;

                ips = Dns.GetHostAddresses(RemoteControl.HostName);

                Log.Debug("TVHome: WOL - GetHostAddresses({0}) returns:", RemoteControl.HostName);

                foreach (IPAddress ip in ips)
                {
                  Log.Debug("    {0}", ip);
                }

                //Use first valid IP address
                ipAddress = ips[0];

              }
              catch (Exception ex)
              {
                Log.Error("TVHome: WOL - Failed GetHostAddress - {0}", ex.Message);
              }
            }

            //Check for valid IP address
            if (ipAddress != null)
            {
              //Update the MAC address if possible
              hwAddress = wakeOnLanManager.GetHardwareAddress(ipAddress);

              if (wakeOnLanManager.IsValidEthernetAddress(hwAddress))
              {
                Log.Debug("TVHome: WOL - Valid auto MAC address: {0:x}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}"
                  , hwAddress[0], hwAddress[1], hwAddress[2], hwAddress[3], hwAddress[4], hwAddress[5]);

                //Store MAC address
                macAddress = BitConverter.ToString(hwAddress).Replace("-", ":");

                Log.Debug("TVHome: WOL - Store MAC address: {0}", macAddress);

                using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
                {
                  xmlwriter.SetValue("tvservice", "macAddress", macAddress);
                }
              }
            }
          }

          //Use stored MAC address
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          {
            macAddress = xmlreader.GetValueAsString("tvservice", "macAddress", null);
          }

          Log.Debug("TVHome: WOL - Use stored MAC address: {0}", macAddress);

          try
          {
            hwAddress = wakeOnLanManager.GetHwAddrBytes(macAddress);

            //Finally, start up the TV server
            Log.Info("TVHome: WOL - Start the TV server");

            if (wakeOnLanManager.WakeupSystem(hwAddress, RemoteControl.HostName, 10))
            {
              Log.Info("TVHome: WOL - The TV server started successfully!");
            }
          }
          catch (Exception ex)
          {
            Log.Error("TVHome: WOL - Failed to start the TV server - {0}", ex.Message);
          }
        }
      }
    }

    ///// <summary>
    ///// Register the remoting service and attaching ciMenuHandler for server events
    ///// </summary>
    //public static void RegisterCiMenu(int newCardId)
    //{
    //  if (ciMenuHandler == null)
    //  {
    //    Log.Debug("CiMenu: PrepareCiMenu");
    //    ciMenuHandler = new CiMenuHandler();
    //    // opens remoting and attach local eventhandler to server event, call only once
    //    RemoteControl.RegisterCiMenuCallbacks(ciMenuHandler);
    //  }
    //  // Check if card supports CI menu
    //  if (newCardId != -1 && RemoteControl.Instance.CiMenuSupported(newCardId))
    //  {
    //    // Enable CI menu handling in card
    //    RemoteControl.Instance.SetCiMenuHandler(newCardId, null);
    //    Log.Debug("TvPlugin: CiMenuHandler attached to new card {0}", newCardId);
    //  }
    //}
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
        Connected = RemoteControl.IsConnected;
        if (Connected && !_suspended)
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
                   (!g_Player.IsTVRecording && (g_Player.IsTV || g_Player.IsRadio)))
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

      //tv off
      Log.Info("TVHome:turn tv off");
      SaveSettings();
      Card.User.Name = new User().Name;
      Card.StopTimeShifting();

      if (type == g_Player.MediaType.Radio || type == g_Player.MediaType.TV)
      {
        UpdateGUIonPlaybackStateChange(false);
      }

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
      }
    }

    private void OnResume()
    {
      Log.Debug("TVHome.OnResume()");

      HandleWakeUpTvServer();
      
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
      }
      return false; // false = all other processes will handle the msg
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
        if (!Connected)
        {
          if (!_onPageLoadDone)
          {
            RemoteControl.Clear();
            GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
            return;
          }
          else if (waits >= MAX_WAIT_FOR_SERVER_CONNECTION)
          {
            HandleServerNotConnected();
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
        if (RemoteControl.IsConnected)
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
      bool showlastActModFS = (_showlastactivemodule && _showlastactivemoduleFullscreen && !_suspended && _autoTurnOnTv);
      bool useDelay = false;

      if (!_suspended && !showlastActModFS)
      {
        useDelay = true;
        showlastActModFS = false;
      }
      else if (!_suspended)
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
            g_Player.ShowFullScreenWindow();
          }
        }
        else if (_autoFullScreenOnly && !g_Player.FullScreen && (PreviousWindowId == (int)Window.WINDOW_TVFULLSCREEN))
        {
          Log.Debug("TVHome.OnPageLoad(): autoFullScreenOnly set, returning to previous window");
          GUIWindowManager.ShowPreviousWindow();
        }
      }

      _onPageLoadDone = true;      
      _suspended = false;

      UpdateGUIonPlaybackStateChange();
      doProcess();
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
      if (Connected)
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
        ViewChannelAndCheck(miniGuide.SelectedChannel);
        UpdateGUIonPlaybackStateChange();
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

      if (control == btnTeletext)
      {
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_TELETEXT);
        return;
      }

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
      if (!Connected)
      {
        if (g_Player.Playing && !g_Player.FullScreen && g_Player.IsTV)
        {
          g_Player.Stop();
        }
        return;
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
      if (Navigator.CheckChannelChange())
      {
        UpdateGUIonPlaybackStateChange();
      }

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
      //TODO: This method does not handle the fact the RemoteControl.Hostname
      //could be an IP address and not a hostname.
      Log.Debug("TvFullScreen: IsSingleSeat - RemoteControl.HostName = {0} / Environment.MachineName = {1}", RemoteControl.HostName, Environment.MachineName);
      
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

      //set audio video related media info properties.
      int currAudio = g_Player.CurrentAudioStream;

      if (currAudio > -1)
      {
        string streamType = g_Player.AudioType(currAudio);

        switch (streamType)
        {
          case "AC3":
          case "AC3plus": // just for the time being use the same icon for AC3 & AC3plus
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
            GUIPropertyManager.SetProperty("#TV.View.title", current.Title);
            GUIPropertyManager.SetProperty("#TV.View.compositetitle", TVUtil.GetDisplayTitle(current));
            GUIPropertyManager.SetProperty("#TV.View.description", current.Description);
            GUIPropertyManager.SetProperty("#TV.View.subtitle", current.EpisodeName);
            GUIPropertyManager.SetProperty("#TV.View.episode", current.EpisodeNumber);
          }
          else
          {
            GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(736)); // no epg for this channel
            GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.subtitle", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.episode", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.genre", String.Empty);
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
            GUIPropertyManager.SetProperty("#TV.Next.title", next.Title);
            GUIPropertyManager.SetProperty("#TV.Next.compositetitle", TVUtil.GetDisplayTitle(next));
            GUIPropertyManager.SetProperty("#TV.Next.description", next.Description);
            GUIPropertyManager.SetProperty("#TV.Next.subtitle", next.EpisodeName);
            GUIPropertyManager.SetProperty("#TV.Next.episode", next.EpisodeNumber);
          }
          else
          {
            GUIPropertyManager.SetProperty("#TV.Next.title", GUILocalizeStrings.Get(736)); // no epg for this channel
            GUIPropertyManager.SetProperty("#TV.Next.start", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.stop", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.description", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.subtitle", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.episode", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.genre", String.Empty);
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

          double playingPointPercent = ((double)playingPoint) / ((double)programDuration);
          playingPointPercent *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.Record.percent2", ((int)playingPointPercent).ToString());

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
          case "AC3plus":
            stream.StreamType = AudioStreamType.EAC3;
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
        if (streams[i].StreamType == AudioStreamType.AC3 ||
            streams[i].StreamType == AudioStreamType.EAC3)
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
            string leftAudioLang = streams[i].Language.Substring(0, 3);
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
              // is the audio track an AC3/AC3plus track?
              if (streams[i].StreamType == AudioStreamType.AC3 ||
                  streams[i].StreamType == AudioStreamType.EAC3) 
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

      _lastError.Result = succeeded;
      _lastError.FailingChannel = channel;
      _lastError.Messages.Clear();

      // reset the last channel, so you can switch back after error
      TVHome.Navigator.SetFailingChannel(_lastError.FailingChannel);

      int TextID = 0;
      _lastError.Messages.Add(GUILocalizeStrings.Get(1500));
      switch (succeeded)
      {
        case TvResult.NoSignalDetected:
          TextID = 1499;
          break;
        case TvResult.CardIsDisabled:
          TextID = 1501;
          break;
        case TvResult.AllCardsBusy:
          TextID = 1502;
          break;
        case TvResult.ChannelIsScrambled:
          TextID = 1503;
          break;
        case TvResult.NoVideoAudioDetected:
          TextID = 1504;
          break;
        case TvResult.UnableToStartGraph:
          TextID = 1505;
          break;
        case TvResult.UnknownError:
          // this error can also happen if we have no connection to the server.
          if (!Connected || !RemoteControl.IsConnected)
          {
            TextID = 1510;
          }
          else
          {
            TextID = 1506;
          }
          break;
        case TvResult.UnknownChannel:
          TextID = 1507;
          break;
        case TvResult.ChannelNotMappedToAnyCard:
          TextID = 1508;
          break;
        case TvResult.NoTuningDetails:
          TextID = 1509;
          break;
        case TvResult.GraphBuildingFailed:
          TextID = 1518;
          break;
        case TvResult.SWEncoderMissing:
          TextID = 1519;
          break;
        case TvResult.NoFreeDiskSpace:
          TextID = 1520;
          break;
        default:
          // this error can also happen if we have no connection to the server.
          if (!Connected || !RemoteControl.IsConnected)
          {
            TextID = 1510;
          }
          else
          {
            TextID = 1506;
          }
          break;
      }

      if (TextID != 0)
      {
        _lastError.Messages.Add(GUILocalizeStrings.Get(TextID));
      }

      GUIDialogNotify pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
      string caption = GUILocalizeStrings.Get(605) + " - " + channel.Name; // +GUILocalizeStrings.Get(1512); ("tune last?")
      pDlgNotify.SetHeading(caption); //my tv
      StringBuilder sbMessage = new StringBuilder();
      // ignore the "unable to start timeshift" line to avoid scrolling, because NotifyDLG has very few space available.
      for (int idx = 1; idx < _lastError.Messages.Count; idx++)
      {
        sbMessage.AppendFormat("\n{0}", _lastError.Messages[idx]);
      }
      pDlgNotify.SetText(sbMessage.ToString());

      // Fullscreen shows the TVZapOSD to handle error messages
      if (GUIWindowManager.ActiveWindow == (int)(int)Window.WINDOW_TVFULLSCREEN)
      {
        // If failed and wasPlaying TV, left screen as it is and show osd with error message 
        Log.Info("send message to fullscreen tv");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_TV_ERROR_NOTIFY, GUIWindowManager.ActiveWindow, 0, 0, 0, 0,
                                        null);
        msg.SendToTargetWindow = true;
        msg.TargetWindowId = (int)(int)Window.WINDOW_TVFULLSCREEN;
        msg.Object = _lastError; // forward error info object
        msg.Param1 = 3; // sec timeout
        GUIGraphicsContext.SendMessage(msg);
        return;
      }
      else
      {
        // show notify dialog 
        pDlgNotify.DoModal((int)GUIWindowManager.ActiveWindowEx);
      }
    }

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
        // Morpheus: number of frames depends on hardware, from 1..5 or higher might be needed! 
        //           Probably the faster the graphics card is, the more frames required???
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

    /// <summary>
    /// Pre-tune checks "outsourced" to reduce code complexity
    /// </summary>
    /// <param name="channel">the channel to tune</param>
    /// <param name="doContinue">indicate to continue</param>
    /// <returns>return value when not continuing</returns>
    private static bool PreTuneChecks(Channel channel, out bool doContinue)
    {
      doContinue = false;
      if (_suspended && _waitonresume > 0)
      {
        Log.Info("TVHome.ViewChannelAndCheck(): system just woke up...waiting {0} ms., suspended {2}", _waitonresume, _suspended);
        Thread.Sleep(_waitonresume);
      }

      _waitForVideoReceived.Reset();

      if (channel == null)
      {
        Log.Info("TVHome.ViewChannelAndCheck(): channel==null");
        return false;
      }
      Log.Info("TVHome.ViewChannelAndCheck(): View channel={0}", channel.DisplayName);

      //if a channel is untunable, then there is no reason to carry on or even stop playback.
      ChannelState CurrentChanState = TvServer.GetChannelState(channel.IdChannel, Card.User);
      if (CurrentChanState == ChannelState.nottunable)
      {
        ChannelTuneFailedNotifyUser(TvResult.AllCardsBusy, false, channel);
        return false;
      }

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
          return true;
        }
        if (!_autoTurnOnTv) //respect the autoturnontv setting.
        {
          if (g_Player.IsVideo || g_Player.IsDVD || g_Player.IsMusic)
          {
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

      if (Card != null)
      {
        if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
        //modified by joboehl. Avoids other video being played instead of TV. 
        {
          //if we're already watching this channel, then simply return
          if (Card.IsTimeShifting == true && Card.IdChannel == channel.IdChannel)
          {
            return true;
          }
        }
      }

      // if all checks passed then we won't return
      doContinue = true;
      return true; // will be ignored
    }

    /// <summary>
    /// Tunes to a new channel
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public static bool ViewChannelAndCheck(Channel channel)
    {
      bool checkResult;
      bool doContinue;

      _status.Clear();

      _doingChannelChange = false;

      try 
      {
        checkResult = PreTuneChecks(channel, out doContinue);
        if (doContinue == false)
          return checkResult;

        _doingChannelChange = true;
        TvResult succeeded;


        User user = new User();
        if (Card != null)
        {
          user.CardId = Card.Id;
        }

        if ((g_Player.Playing && g_Player.IsTimeShifting && !g_Player.Stopped) && (g_Player.IsTV || g_Player.IsRadio))
        {
          _status.Set(LiveTvStatus.WasPlaying);
        }

        //Start timeshifting the new tv channel
        TvServer server = new TvServer();
        VirtualCard card;
        int newCardId = -1;

        // check which card will be used
        newCardId = server.TimeShiftingWouldUseCard(ref user, channel.IdChannel);

        //Added by joboehl - If any major related to the timeshifting changed during the start, restart the player.           
        if (newCardId != -1 && Card.Id != newCardId)
        {
          _status.Set(LiveTvStatus.CardChange);
          RegisterCiMenu(newCardId);
        }

        // we need to stop player HERE if card has changed.        
        if (_status.AllSet(LiveTvStatus.WasPlaying|LiveTvStatus.CardChange))
        {
          Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. CardId:{0}/{1}, RTSP:{2}", Card.Id, newCardId, Card.RTSPUrl);
          Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. Timeshifting:{0}", Card.TimeShiftFileName);
          Log.Debug("TVHome.ViewChannelAndCheck(): rebuilding graph (card changed) - timeshifting continueing.");
        }
        if (_status.IsSet(LiveTvStatus.WasPlaying))
        {
          RenderBlackImage();
          g_Player.PauseGraph();
        }

        // if card was not changed
        if (_status.IsNotSet(LiveTvStatus.CardChange))
        {
          g_Player.OnZapping(0x80);           // Setup Zapping for TsReader, requesting new PAT from stream
        }

        succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);

        if (_status.IsSet(LiveTvStatus.WasPlaying))
        {
          if (card != null)
            g_Player.OnZapping((int)card.Type);
          else
            g_Player.OnZapping(-1);
        }


        if (succeeded != TvResult.Succeeded)
        {
          //timeshifting new channel failed. 
          g_Player.Stop();

          // ensure right channel name, even if not watchable:Navigator.Channel = channel; 
          ChannelTuneFailedNotifyUser(succeeded, _status.IsSet(LiveTvStatus.WasPlaying), channel);

          _doingChannelChange = true; // keep fullscreen false;
          return true; // "success"
        }


        //timeshifting succeeded					                    

        // we might have a situation on the server where card has changed in order to complete a 
        // channel change. - lets check for this.
        // if this has happened, we need to re-create graph.
        if (newCardId != card.Id || Card.Id != card.Id)
        {
          _status.Set(LiveTvStatus.CardChange);
          _status.Reset(LiveTvStatus.WasPlaying);
        }
        else
        {
          // check if after starttimeshift the active card is same as before (tvserver can do "failover" to another card)
          _status.Reset(LiveTvStatus.CardChange);
          _status.Set(LiveTvStatus.SeekToEnd);
        }

        // Update channel navigator
        if (Navigator.Channel != null && (channel.IdChannel != Navigator.Channel.IdChannel || (Navigator.LastViewedChannel == null)))
        {
          Navigator.LastViewedChannel = Navigator.Channel;
        }
        Log.Info("succeeded:{0} {1}", succeeded, card);
        Card = card; //Moved by joboehl - Only touch the card if starttimeshifting succeeded. 

        // if needed seek to end
        if (_status.IsSet(LiveTvStatus.SeekToEnd))
        {
          SeekToEnd(true);
        }

        // continue graph
        g_Player.ContinueGraph();
        if (!g_Player.Playing || _status.IsSet(LiveTvStatus.CardChange))
        {
          StartPlay();
        }
        TvTimeShiftPositionWatcher.CheckOrUpdateTimeShiftPosition(true);
        _playbackStopped = false;
        _doingChannelChange = false;
        _ServerNotConnectedHandled = false;
        return true;
      }
      catch (Exception ex)
      {
        Log.Debug("TvPlugin:ViewChannelandCheckV2 Exception {0}", ex.ToString());
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

      if (useRTSP)
      {
        timeshiftFileName = Card.RTSPUrl;
      }
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

      benchClock.Stop();
    }

    private static void SeekToEnd(bool zapping)
    {
      double duration = g_Player.Duration;
      double position = g_Player.CurrentPosition;

      bool useRtsp = UseRTSP();

      Log.Info("tvhome:SeektoEnd({0}/{1}),{2},rtsp={3}", position, duration, zapping, useRtsp);
      if (duration > 0 || position > 0)
      {
        try
        {
          //singleseat or  multiseat rtsp streaming....
          if (!useRtsp || (useRtsp && zapping))
          {
            g_Player.SeekAbsolute(duration);
          }
        }
        catch (Exception e)
        {
          Log.Error("tvhome:SeektoEnd({0}, rtsp={1} exception: {2}", zapping, useRtsp, e.Message);
          g_Player.Stop();
        }
      }
    }

    #region CI Menu

    /// <summary>
    /// Register the remoting service and attaching ciMenuHandler for server events
    /// </summary>
    public static void RegisterCiMenu(int newCardId)
    {
      if (ciMenuHandler == null)
      {
        Log.Debug("CiMenu: PrepareCiMenu");
        ciMenuHandler = new CiMenuHandler();
      }
      // Check if card supports CI menu
      if (newCardId != -1 && RemoteControl.Instance.CiMenuSupported(newCardId))
      {
        // opens remoting and attach local eventhandler to server event, call only once
        RemoteControl.RegisterCiMenuCallbacks(ciMenuHandler);

        // Enable CI menu handling in card
        RemoteControl.Instance.SetCiMenuHandler(newCardId, null);
        Log.Debug("TvPlugin: CiMenuHandler attached to new card {0}", newCardId);
      }
    }

    /// <summary>
    /// Keyboard input for ci menu
    /// </summary>
    /// <param name="title"></param>
    /// <param name="maxLength"></param>
    /// <param name="bPassword"></param>
    /// <param name="strLine"></param>
    /// <returns></returns>
    protected static bool GetKeyboard(string title, int maxLength, bool bPassword, ref string strLine)
    {
      StandardKeyboard keyboard = (StandardKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.Password = bPassword;
      keyboard.Title = title;
      keyboard.SetMaxLength(maxLength);
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GUIWindowManager.ActiveWindowEx);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Handles all CiMenu actions from callback
    /// </summary>
    /// <param name="Menu">complete CI menu object</param>
    public static void ShowMenu(CiMenu Menu)
    {
      if (dlgCiMenu == null)
      {
        dlgCiMenu = (GUIDialogCIMenu)GUIWindowManager.GetWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_DIALOG_CIMENU);
      }

      switch (Menu.State)
      {
        // choices available, so show them
        case TvLibrary.Interfaces.CiMenuState.Ready:
          dlgCiMenu.Reset();
          dlgCiMenu.SetHeading(Menu.Title, Menu.Subtitle, Menu.BottomText); // CI Menu


          for (int i = 0; i < Menu.NumChoices; i++) // CI Menu Entries
            dlgCiMenu.Add(Menu.MenuEntries[i].Message); // take only message, numbers come from dialog

          // show dialog and wait for result       
          dlgCiMenu.DoModal(GUIWindowManager.ActiveWindow);
          if (Menu.State != TvLibrary.Interfaces.CiMenuState.Error)
          {
            if (dlgCiMenu.SelectedId != -1)
            {
              TVHome.Card.SelectCiMenu(Convert.ToByte(dlgCiMenu.SelectedId));
            }
            else
            {
              TVHome.Card.SelectCiMenu(0); // 0 means "back"
            }
          }
          else
          {
            TVHome.Card.CloseMenu(); // in case of error close the menu
          }
          break;

        // errors and menu options with no choices
        case TvLibrary.Interfaces.CiMenuState.Error:
        case TvLibrary.Interfaces.CiMenuState.NoChoices:

          if (_dialogNotify == null)
          {
            //_dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
            _dialogNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
          }
          if (null != _dialogNotify)
          {
            _dialogNotify.Reset();
            _dialogNotify.ClearAll();
            _dialogNotify.SetHeading(Menu.Title);

            _dialogNotify.SetText(String.Format("{0}\r\n{1}", Menu.Subtitle, Menu.BottomText));
            _dialogNotify.TimeOut = 2; // seconds
            _dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;

        // requests require users input so open keyboard
        case TvLibrary.Interfaces.CiMenuState.Request:
          String result = "";
          if (GetKeyboard(Menu.RequestText, Menu.AnswerLength, Menu.Password, ref result) == true)
          {
            TVHome.Card.SendMenuAnswer(false, result); // send answer, cancel=false
          }
          else
          {
            TVHome.Card.SendMenuAnswer(true, null); // cancel request 
          }
          break;
      }
    }

    #endregion
  }
}


#region CI Menu
/// <summary>
/// Handler class for gui interactions of ci menu
/// </summary>
public class CiMenuHandler : CiMenuCallbackSink
{
  /// <summary>
  /// eventhandler to show CI Menu dialog
  /// </summary>
  /// <param name="Menu"></param>
  protected override void CiMenuCallback(CiMenu Menu)
  {
    try
    {
      Log.Debug("Callback from tvserver {0}", Menu.Title);

      // pass menu to calling dialog
      TvPlugin.TVHome.ShowMenu(Menu);
    }
    catch
    {
      Menu = new CiMenu("Remoting Exception", "Communication with server failed", null, TvLibrary.Interfaces.CiMenuState.Error);
      // pass menu to calling dialog
      TvPlugin.TVHome.ShowMenu(Menu);
    }
  }
}
#endregion

