#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Text;
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
using Action = MediaPortal.GUI.Library.Action;

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

    private const int HEARTBEAT_INTERVAL = 1; //seconds
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
    private const int PROGRESS_PERCENTAGE_UPDATE_INTERVAL = 1000;
    private const int PROCESS_UPDATE_INTERVAL = 1000;

    #endregion

    #region variables

    private enum Controls
    {
      IMG_REC_CHANNEL = 21,
      LABEL_REC_INFO = 22,
      IMG_REC_RECTANGLE = 23,
    }

    [Flags]
    public enum LiveTvStatus
    {
      WasPlaying = 1,
      CardChange = 2,
      SeekToEnd = 4,
      SeekToEndAfterPlayback = 8
    }

    private static readonly SynchronizationContext _mainThreadContext = SynchronizationContext.Current;
    private Channel _resumeChannel = null;
    private Thread heartBeatTransmitterThread = null;
    private static DateTime _updateProgressTimer = DateTime.MinValue;
    public static ChannelNavigator m_navigator;
    private static TVUtil _util;
    private static VirtualCard _card = null;
    private static DateTime _updateTimer = DateTime.Now;
    private static bool _autoTurnOnTv = false;
    private static int _waitonresume = 0;
    public static bool settingsLoaded = false;
    private TvCropManager _cropManager = new TvCropManager();
    private static TvNotifyManager _notifyManager = new TvNotifyManager();
    private static List<string> _preferredLanguages;
    private static bool _usertsp;
    private static string _recordingpath = "";
    private static string _timeshiftingpath = "";
    private static bool _preferAC3 = false;
    private static bool _preferAudioTypeOverLang = false;
    private static bool _autoFullScreen = false;
    private static bool _suspended = false;
    private static bool _showlastactivemodule = false;
    private static bool _showlastactivemoduleFullscreen = false;
    private static bool _playbackStopped = false;
    public static bool _onPageLoadDone = false;
    private static bool _userChannelChanged = false;
    private static bool _showChannelStateIcons = true;
    private static bool _doingHandleServerNotConnected = false;
    private static bool _doingChannelChange = false;
    private static bool _ServerNotConnectedHandled = false;
    private static bool _recoverTV = false;
    private static bool _connected = false;
    private static bool _isAnyCardRecording = false;
    protected static TvServer _server;
    public static bool firstNotLoaded = true;

    private static ManualResetEvent _waitForBlackScreen = null;
    private static ManualResetEvent _waitForVideoReceived = null;

    private static int FramesBeforeStopRenderBlackImage = 0;
    private static BitHelper<LiveTvStatus> _status = new BitHelper<LiveTvStatus>();

    [SkinControl(2)]
    protected GUIButtonControl btnTvGuide = null;
    [SkinControl(3)]
    protected GUIButtonControl btnRecord = null;
    [SkinControl(7)]
    protected GUIButtonControl btnChannel = null;
    [SkinControl(8)]
    protected GUICheckButton btnTvOnOff = null;
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

    // error handling
    public class ChannelErrorInfo
    {
      public Channel FailingChannel;
      public TvResult Result;
      public List<String> Messages = new List<string>();
    }

    public static ChannelErrorInfo _lastError = new ChannelErrorInfo();

    // CI Menu
    private static CiMenuHandler ciMenuHandler;
    public static GUIDialogCIMenu dlgCiMenu;
    public static GUIDialogNotify _dialogNotify = null;

    private static CiMenu currentCiMenu = null;
    private static object CiMenuLock = new object();
    private static bool CiMenuActive = false;

    private static List<CiMenu> CiMenuList = new List<CiMenu>();

    // EPG Channel
    private static Channel _lastTvChannel = null;

    // notification
    protected static int _notifyTVTimeout = 15;
    protected static bool _playNotifyBeep = true;
    protected static int _preNotifyConfig = 60;

    #endregion

    #region events & delegates

    private static event OnChannelChangedDelegate OnChannelChanged;
    private delegate void OnChannelChangedDelegate();

    #endregion

    #region delegates

    private delegate void StopPlayerMainThreadDelegate();

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

    public void ShowPlugin() { }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion

    public TVHome()
    {
      TVUtil.SetGentleConfigFile();
      GetID = (int)Window.WINDOW_TV;

    }

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\mytvhomeServer.xml");
    }

    public static void OnLoaded()
    {
      Log.Info("TVHome:OnLoaded");

      try
      {
        if (Connected && !firstNotLoaded)
        {
          m_navigator = new ChannelNavigator();
          m_navigator.OnZapChannel -= new ChannelNavigator.OnZapChannelDelegate(ForceUpdates);
          m_navigator.OnZapChannel += new ChannelNavigator.OnZapChannelDelegate(ForceUpdates);
          LoadSettings(true);

          string pluginVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
          string tvServerVersion = Connected ? RemoteControl.Instance.GetAssemblyVersion : "Unknown";

          if (Connected && pluginVersion != tvServerVersion)
          {
            string strLine = "TvPlugin and TvServer don't have the same version.\r\n";
            strLine += "TvServer Version: " + tvServerVersion + "\r\n";
            strLine += "TvPlugin Version: " + pluginVersion;
            Log.Error(strLine);
          }
          else
            Log.Info("TVHome V" + pluginVersion + ":ctor");
        }
      }
      catch (Exception ex)
      {
        Log.Error("TVHome: Error occured in on loading : {0}, st {1}", ex.Message, Environment.StackTrace);
      }

      if (!firstNotLoaded)
      {
        _notifyManager.Start();
      }
    }

    public override void OnAdded()
    {
      Log.Info("TVHome:OnAdded");
      RemoteControl.OnRemotingDisconnected +=
        new RemoteControl.RemotingDisconnectedDelegate(RemoteControl_OnRemotingDisconnected);
      RemoteControl.OnRemotingConnected += new RemoteControl.RemotingConnectedDelegate(RemoteControl_OnRemotingConnected);

      GUIGraphicsContext.OnBlackImageRendered += new BlackImageRenderedHandler(OnBlackImageRendered);
      GUIGraphicsContext.OnVideoReceived += new VideoReceivedHandler(OnVideoReceived);

      _waitForBlackScreen = new ManualResetEvent(false);
      _waitForVideoReceived = new ManualResetEvent(false);

      Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.AudioTracksReady += new g_Player.AudioTracksReadyHandler(OnAudioTracksReady);

      GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);

      // Delete tv thumbs from local thumbs folder and file existence cache
      Log.Debug("TVHome.OnAdded: Delete thumb files in {0}", Thumbs.TVRecorded);
      try
      {
        string[] strFiles = Directory.GetFiles(Thumbs.TVRecorded, @"*" + MediaPortal.Util.Utils.GetThumbExtension());
        foreach (string strFile in strFiles)
        {
          try
          {
            File.Delete(strFile);
            Utils.DoInsertNonExistingFileIntoCache(strFile);
          }
          catch (Exception delex)
          {
            Log.Error("TVHome.OnAdded: Cannot delete file {0} - {1}", strFile, delex.Message);
          }
        }
      }
      catch (Exception direx)
      {
        Log.Error("TVHome.OnAdded: Cannot get files from directory {0} - {1}", Thumbs.TVRecorded, direx.Message);
      }

      try
      {
        // Make sure that we have valid hostname for the TV server
        SetRemoteControlHostName();

        // Wake up the TV server, if required
        HandleWakeUpTvServer();
        startHeartBeatThread();

        TVHome.OnChannelChanged -= new OnChannelChangedDelegate(ForceUpdates);
        TVHome.OnChannelChanged += new OnChannelChangedDelegate(ForceUpdates);
      }
      catch (Exception ex)
      {
        Log.Error("TVHome: Error occured in Init(): {0}, st {1}", ex.Message, Environment.StackTrace);
      }

      OnLoaded();
    }



    /// <summary>
    /// Gets called by the runtime when a  window will be destroyed
    /// Every window window should override this method and cleanup any resources
    /// </summary>
    /// <returns></returns>
    public override void DeInit()
    {
      OnPageDestroy(-1);

      RemoteControl.OnRemotingDisconnected -=
       new RemoteControl.RemotingDisconnectedDelegate(RemoteControl_OnRemotingDisconnected);
      RemoteControl.OnRemotingConnected -= new RemoteControl.RemotingConnectedDelegate(RemoteControl_OnRemotingConnected);

      GUIGraphicsContext.OnBlackImageRendered -= new BlackImageRenderedHandler(OnBlackImageRendered);
      GUIGraphicsContext.OnVideoReceived -= new VideoReceivedHandler(OnVideoReceived);

      Application.ApplicationExit -= new EventHandler(Application_ApplicationExit);

      g_Player.PlayBackStarted -= new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackStopped -= new g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.AudioTracksReady -= new g_Player.AudioTracksReadyHandler(OnAudioTracksReady);

      GUIWindowManager.Receivers -= new SendMessageHandler(OnGlobalMessage);
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_RECORD:
          // record current program on current channel
          // are we watching tv?                    
          ManualRecord(Navigator.Channel, GetID);
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
        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL:
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

    protected override void OnPageLoad()
    {
      Log.Info("TVHome:OnPageLoad");

      if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).PreviousWindowId != (int)Window.WINDOW_TVFULLSCREEN)
      {
        _playbackStopped = false;
      }

      btnActiveStreams.Label = GUILocalizeStrings.Get(692);

      if (!Connected)
      {
        RemoteControl.Clear();
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
        UpdateStateOfRecButton();
        UpdateProgressPercentageBar();
        UpdateRecordingIndicator();
        return;
      }

      try
      {
        int cards = RemoteControl.Instance.Cards;
      }
      catch (Exception)
      {
        RemoteControl.Clear();
      }

      // stop the old recorder.
      // DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.DataSourceOnly;
      GUIMessage msgStopRecorder = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgStopRecorder);

      if (!_onPageLoadDone && m_navigator != null)
      {
        m_navigator.ReLoad();
        LoadSettings(false);
      }

      if (m_navigator == null)
      {
        m_navigator = new ChannelNavigator(); // Create the channel navigator (it will load groups and channels)
      }

      base.OnPageLoad();

      // set video window position
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
        if (!_suspended)
        {
          AutoTurnOnTv(channel);
        }
        else
        {
          _resumeChannel = channel;
        }
        GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.CurrentGroup.GroupName);
        Log.Info("tv home init:{0} done", channel.DisplayName);
      }

      if (!_suspended)
      {
        AutoFullScreenTv();
      }

      _onPageLoadDone = true;
      _suspended = false;

      UpdateGUIonPlaybackStateChange();
      UpdateCurrentChannel();
    }

    private void AutoTurnOnTv(Channel channel)
    {
      if (_autoTurnOnTv && !_playbackStopped && !wasPrevWinTVplugin())
      {
        if (!wasPrevWinTVplugin())
        {
          _userChannelChanged = false;
        }
        ViewChannelAndCheck(channel);
      }
    }

    private void AutoFullScreenTv()
    {
      if (_autoFullScreen)
      {
        // if using showlastactivemodule feature and last module is fullscreen while returning from powerstate, then do not set fullscreen here (since this is done by the resume last active module feature)
        // we depend on the onresume method, thats why tvplugin now impl. the IPluginReceiver interface.      
        if (!_suspended)
        {
          bool isTvOrRec = (g_Player.IsTV || g_Player.IsTVRecording);
          if (isTvOrRec)
          {
            Log.Debug("GUIGraphicsContext.IsFullScreenVideo {0}", GUIGraphicsContext.IsFullScreenVideo);
            bool wasFullScreenTV = (PreviousWindowId == (int)Window.WINDOW_TVFULLSCREEN);

            if (!wasFullScreenTV)
            {
              if (!wasPrevWinTVplugin())
              {
                Log.Debug("TVHome.AutoFullScreenTv(): setting autoFullScreen");
                bool showlastActModFS = (_showlastactivemodule && _showlastactivemoduleFullscreen && !_suspended &&
                                         _autoTurnOnTv);
                if (!showlastActModFS)
                {
                  //if we are resuming from standby with tvhome, we want this in fullscreen, but we need a delay for it to work.
                  Thread tvDelayThread = new Thread(TvDelayThread);
                  tvDelayThread.Start();
                }
                else
                {
                  g_Player.ShowFullScreenWindow();
                }
              }
              else
              {
                g_Player.ShowFullScreenWindow();
              }
            }
          }
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      // if we're switching to another plugin
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

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();

      RefreshConnectionState();

      if (!Connected)
      {
        UpdateStateOfRecButton();
        UpdateRecordingIndicator();
        UpdateGUIonPlaybackStateChange();
        ShowDlgAsynch();
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
          // tv off
          g_Player.Stop();
          Log.Warn("TVHome.OnClicked(): EndTvOff {0} ms", benchClock.ElapsedMilliseconds.ToString());
          benchClock.Stop();
          return;
        }
        else
        {
          // tv on
          Log.Info("TVHome:turn tv on {0}", Navigator.CurrentChannel);

          // stop playing anything
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
        if (Navigator.Channel != null && Navigator.Channel.IsTv)
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
            // we only want to resume TV if previous window is NOT a tvplugin based one. (ex. tvguide.)
            if (_autoTurnOnTv && !wasPrevWinTVplugin())
            {
              //restart viewing...  
              Log.Info("tv home msg resume tv:{0}", Navigator.CurrentChannel);
              ViewChannel(Navigator.Channel);
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    private static void ForceUpdates()
    {
      _updateTimer = DateTime.Now.AddMilliseconds(-1 * (PROCESS_UPDATE_INTERVAL+1));
      _updateProgressTimer = DateTime.Now.AddMilliseconds(-1 * (PROGRESS_PERCENTAGE_UPDATE_INTERVAL+1));
    }

    public override void Process()
    {
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (!Connected || _suspended || ts.TotalMilliseconds < PROCESS_UPDATE_INTERVAL)
      {
        return;
      }

      try
      {
        UpdateRecordingIndicator();
        UpdateStateOfRecButton();

        if (!Card.IsTimeShifting)
        {
          UpdateProgressPercentageBar();
          // mantis #2218 : TV guide information in TV home screen does not update when program changes if TV is not playing           
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
        ShowCiMenu();
        UpdateCurrentChannel();
      }
      finally
      {
        _updateTimer = DateTime.Now;
      }
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
      if (manual) // until manual stop
      {
        Schedule newSchedule = new Schedule(channel.IdChannel,
                                            GUILocalizeStrings.Get(413) + " (" + channel.DisplayName + ")",
                                            DateTime.Now, DateTime.Now.AddDays(1));
        newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        newSchedule.Persist();
        server.OnNewSchedule();
      }
      else // current program
      {
        // lets find any canceled episodes that match this one we want to create, if found uncancel it.
        Schedule existingParentSchedule = Schedule.RetrieveSeries(channel.IdChannel, channel.CurrentProgram.Title,
                                                                  channel.CurrentProgram.StartTime,
                                                                  channel.CurrentProgram.EndTime);
        if (existingParentSchedule != null)
        {
          foreach (CanceledSchedule cancelSched in existingParentSchedule.ReferringCanceledSchedule())
          {
            if (cancelSched.CancelDateTime == channel.CurrentProgram.StartTime)
            {
              existingParentSchedule.UnCancelSerie(channel.CurrentProgram.StartTime, channel.CurrentProgram.IdChannel);
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
        newSchedule.Persist();
        server.OnNewSchedule();
      }
      GUIMessage msgManualRecord = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MANUAL_RECORDING_STARTED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgManualRecord);
    }

    public static bool UseRTSP()
    {
      if (!settingsLoaded)
      {
        LoadSettings(false);
      }
      return _usertsp;
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

    private static void StopPlayerMainThread()
    {
      //call g_player.stop only on main thread.
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        StopPlayerMainThreadDelegate d = new StopPlayerMainThreadDelegate(StopPlayerMainThread);
        GUIGraphicsContext.form.Invoke(d);
        return;
      }

      g_Player.Stop();
    }

    private delegate void ShowDlgAsynchDelegate();

    private delegate void ShowDlgMessageAsynchDelegate(String Message);

    private static void ShowDlgAsynch()
    {
      //show dialogue only on main thread.
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        ShowDlgAsynchDelegate d = new ShowDlgAsynchDelegate(ShowDlgAsynch);
        GUIGraphicsContext.form.Invoke(d);
        return;
      }

      _ServerNotConnectedHandled = true;
      GUIDialogNotify pDlgOK = (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);

      if (pDlgOK == null)
      {
        return;
      }

      try
      {
        pDlgOK.Reset();
        pDlgOK.SetHeading(257); //error
        if (Navigator != null && Navigator.CurrentChannel != null && g_Player.IsTV)
        {
          pDlgOK.SetText(Navigator.CurrentChannel);
        }
        else
        {
          pDlgOK.SetText("");
        }
        pDlgOK.SetText(GUILocalizeStrings.Get(1510)); //Connection to TV server lost
        pDlgOK.TimeOut = 5;
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      }
      catch (Exception)
      {
        Log.Debug("TVHome: GUIDialogNotify null value catched");
      }
    }

    public static void ShowDlgThread()
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
        ShowDlgAsynch();
      }
    }

    private static void RefreshConnectionState()
    {
      IController iController = RemoteControl.Instance; //calling instance will make sure the state is refreshed.
    }

    public static bool HandleServerNotConnected()
    {
      // _doingHandleServerNotConnected is used to avoid multiple calls to this method.
      // the result could be that the dialogue is not shown.

      if (_ServerNotConnectedHandled)
      {
        return true; //still not connected
      }

      if (_doingHandleServerNotConnected)
      {
        return false; //we assume we are still not connected
      }

      _doingHandleServerNotConnected = true;

      try
      {
        if (!Connected)
        {
          //Card.User.Name = new User().Name;
          if (g_Player.Playing)
          {
            if (g_Player.IsTimeShifting) // live TV or radio must be stopped
              TVHome.StopPlayerMainThread();
            else // playing something else so do not disturb
              return true;
          }

          if (g_Player.FullScreen)
          {
            GUIMessage initMsgTV = null;
            initMsgTV = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, (int)Window.WINDOW_TV, 0, 0, 0, 0,
                                       null);
            GUIWindowManager.SendThreadMessage(initMsgTV);
            return true;
          }
          Thread showDlgThread = new Thread(ShowDlgThread);
          showDlgThread.IsBackground = true;
          // show the dialog asynch.
          // this fixes a hang situation that would happen when resuming TV with showlastactivemodule
          showDlgThread.Start();
          return true;
        }
        else
        {
          bool gentleConnected = WaitForGentleConnection();

          if (!gentleConnected)
          {
            return true;
          }
        }
      }
      catch (Exception e)
      {
        //we assume that server is disconnected.
        Log.Error("TVHome.HandleServerNotConnected caused an error {0},{1}", e.Message, e.StackTrace);
        return true;
      }
      finally
      {
        _doingHandleServerNotConnected = false;
      }
      return false;
    }

    public static bool WaitForGentleConnection()
    {
      // lets try one more time - seems like the gentle framework is not properly initialized when coming out of standby/hibernation.                    
      // lets wait 10 secs before giving up.
      bool success = false;

      Stopwatch timer = Stopwatch.StartNew();
      while (!success && timer.ElapsedMilliseconds < 10000) //10sec max
      {
        try
        {
          IList<Card> cards = TvDatabase.Card.ListAll();
          success = true;
        }
        catch (Exception)
        {
          success = false;
          Log.Debug("TVHome: waiting for gentle.net DB connection {0} msec", timer.ElapsedMilliseconds);
          Thread.Sleep(100);
        }
      }

      if (!success)
      {
        RemoteControl.Clear();
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
        //GUIWaitCursor.Hide();          
      }

      return success;
    }

    public static List<string> PreferredLanguages
    {
      set { _preferredLanguages = value; }
    }

    public static bool PreferAC3
    {
      set { _preferAC3 = value; }
    }

    public static bool PreferAudioTypeOverLang
    {
      set { _preferAudioTypeOverLang = value; }
    }

    public static bool UserChannelChanged
    {
      set { _userChannelChanged = value; }
      get { return _userChannelChanged; }
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

    public static bool IsAnyCardRecording
    {
      get { return _isAnyCardRecording; }
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
          IUser user = new User();
          _card = TvServer.CardByIndex(user, 0);
        }
        return _card;
      }
      set
      {
        if (_card != null)
        {
          _card = value;
        }
      }
    }

    #endregion

    #region Serialisation

    public static void LoadSettings(bool force)
    {
      if (settingsLoaded && !force)
      {
        return;
      }

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
        _usertsp = xmlreader.GetValueAsBool("tvservice", "usertsp", !Network.IsSingleSeat());
        _recordingpath = xmlreader.GetValueAsString("tvservice", "recordingpath", "");
        _timeshiftingpath = xmlreader.GetValueAsString("tvservice", "timeshiftingpath", "");

        _preferAC3 = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        _preferAudioTypeOverLang = xmlreader.GetValueAsBool("tvservice", "preferAudioTypeOverLang", true);
        _autoFullScreen = xmlreader.GetValueAsBool("mytv", "autofullscreen", false);
        _showChannelStateIcons = xmlreader.GetValueAsBool("mytv", "showChannelStateIcons", true);

        _notifyTVTimeout = xmlreader.GetValueAsInt("mytv", "notifyTVTimeout", 15);
        _playNotifyBeep = xmlreader.GetValueAsBool("mytv", "notifybeep", true);
        _preNotifyConfig = xmlreader.GetValueAsInt("mytv", "notifyTVBefore", 300);
      }
      settingsLoaded = true;
    }

    private static void SaveSettings()
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

    #region Private methods

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
      int intTimeOut;
      String macAddress;
      byte[] hwAddress;

      using (Settings xmlreader = new MPSettings())
      {
        isWakeOnLanEnabled = xmlreader.GetValueAsBool("tvservice", "isWakeOnLanEnabled", false);
        isAutoMacAddressEnabled = xmlreader.GetValueAsBool("tvservice", "isAutoMacAddressEnabled", false);
        intTimeOut = xmlreader.GetValueAsInt("tvservice", "WOLTimeOut", 10);
      }

      if (isWakeOnLanEnabled)
      {
        if (!Network.IsSingleSeat())
        {
          WakeOnLanManager wakeOnLanManager = new WakeOnLanManager();

          if (isAutoMacAddressEnabled)
          {
            IPAddress ipAddress = null;

            // Check if we already have a valid IP address stored in RemoteControl.HostName,
            // otherwise try to resolve the IP address
            if (!IPAddress.TryParse(RemoteControl.HostName, out ipAddress))
            {
              // Get IP address of the TV server
              try
              {
                IPAddress[] ips;

                ips = Dns.GetHostAddresses(RemoteControl.HostName);

                Log.Debug("TVHome: WOL - GetHostAddresses({0}) returns:", RemoteControl.HostName);

                foreach (IPAddress ip in ips)
                {
                  Log.Debug("    {0}", ip);
                }

                // Use first valid IP address
                ipAddress = ips[0];
              }
              catch (Exception ex)
              {
                Log.Error("TVHome: WOL - Failed GetHostAddress - {0}", ex.Message);
              }
            }

            // Check for valid IP address
            if (ipAddress != null)
            {
              // Update the MAC address if possible
              hwAddress = wakeOnLanManager.GetHardwareAddress(ipAddress);

              if (wakeOnLanManager.IsValidEthernetAddress(hwAddress))
              {
                Log.Debug("TVHome: WOL - Valid auto MAC address: {0:x}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}"
                          , hwAddress[0], hwAddress[1], hwAddress[2], hwAddress[3], hwAddress[4], hwAddress[5]);

                // Store MAC address
                macAddress = BitConverter.ToString(hwAddress).Replace("-", ":");

                Log.Debug("TVHome: WOL - Store MAC address: {0}", macAddress);

                using (
                  MediaPortal.Profile.Settings xmlwriter =
                    new MediaPortal.Profile.MPSettings())
                {
                  xmlwriter.SetValue("tvservice", "macAddress", macAddress);
                }
              }
            }
          }

          // Use stored MAC address
          using (Settings xmlreader = new MPSettings())
          {
            macAddress = xmlreader.GetValueAsString("tvservice", "macAddress", null);
          }

          Log.Debug("TVHome: WOL - Use stored MAC address: {0}", macAddress);

          try
          {
            hwAddress = wakeOnLanManager.GetHwAddrBytes(macAddress);

            // Finally, start up the TV server
            Log.Info("TVHome: WOL - Start the TV server");

            if (wakeOnLanManager.WakeupSystem(hwAddress, RemoteControl.HostName, intTimeOut))
            {
              Log.Info("TVHome: WOL - The TV server started successfully!");
            }
            else
            {
              Log.Error("TVHome: WOL - Failed to start the TV server");
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

    private static void RemoteControl_OnRemotingConnected()
    {
      if (!Connected)
        Log.Info("TVHome: OnRemotingConnected, recovered from a disconnection");
      Connected = true;
      _ServerNotConnectedHandled = false;
      if (_recoverTV)
      {
        _recoverTV = false;
        GUIMessage initMsg = null;
        initMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, (int) Window.WINDOW_TV_OVERLAY, 0, 0, 0, 0,
                                 null);
        GUIWindowManager.SendThreadMessage(initMsg);
      }
      if (firstNotLoaded)
      {
        firstNotLoaded = false;
        OnLoaded();
      }
    }

    private static void RemoteControl_OnRemotingDisconnected()
    {
      if (Connected)
        Log.Info("TVHome: OnRemotingDisconnected");
      Connected = false;
      HandleServerNotConnected();
    }

    private void Application_ApplicationExit(object sender, EventArgs e)
    {
      try
      {
        if (Card.IsTimeShifting)
        {
          Card.User.Name = new User().Name;
          Card.StopTimeShifting();
        }
        _notifyManager.Stop();
        stopHeartBeatThread();
      }
      catch (Exception) { }
    }

    private void HeartBeatTransmitter()
    {
      RemoteControl.Clear();
      int countToHBLoop = 5;

      while (true)
      {
        // 1 second loop
        if (Connected)
        {
          _isAnyCardRecording = TvServer.IsAnyCardRecording();
        }

        // HeartBeat loop (5 seconds)
        if (countToHBLoop >= 5)
        {
          countToHBLoop = 0;
          if (!Connected) // is this needed to update connection status
            RefreshConnectionState();
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
                NotifyUser(errMsg);
              }
            }
          }
        }
        Thread.Sleep(HEARTBEAT_INTERVAL * 1000); //sleep for 1 sec. before sending heartbeat again
        countToHBLoop++;
      }
    }

    /// <summary>
    /// Notify the user about the reason of stopped live tv. 
    /// Ensures that the dialog is run in main thread.
    /// </summary>
    /// <param name="errMsg">The error messages</param>
    private static void NotifyUser(string errMsg)
    {
      // show dialogue only on main thread.
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        ShowDlgMessageAsynchDelegate d = new ShowDlgMessageAsynchDelegate(NotifyUser);
        GUIGraphicsContext.form.Invoke(d, errMsg);
        return;
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
        _mainThreadContext.Send(delegate
        {
          g_Player.ShowFullScreenWindow();
        }, null);
      }
    }

    private void OnSuspend()
    {
      Log.Debug("TVHome.OnSuspend()");

      RemoteControl.OnRemotingDisconnected -=
        new RemoteControl.RemotingDisconnectedDelegate(RemoteControl_OnRemotingDisconnected);
      RemoteControl.OnRemotingConnected -= new RemoteControl.RemotingConnectedDelegate(RemoteControl_OnRemotingConnected);

      try
      {
        if (Card.IsTimeShifting)
        {
          Card.User.Name = new User().Name;
          Card.StopTimeShifting();
        }
        _notifyManager.Stop();
        stopHeartBeatThread();
        //Connected = false;
        _ServerNotConnectedHandled = false;
      }
      catch (Exception) { }
      finally
      {
        _ServerNotConnectedHandled = false;
        _suspended = true;
      }
    }

    private void OnResume()
    {
      Log.Debug("TVHome.OnResume()");
      try
      {
        Connected = false;
        RemoteControl.OnRemotingDisconnected +=
          new RemoteControl.RemotingDisconnectedDelegate(RemoteControl_OnRemotingDisconnected);
        RemoteControl.OnRemotingConnected += new RemoteControl.RemotingConnectedDelegate(RemoteControl_OnRemotingConnected);
        HandleWakeUpTvServer();
        startHeartBeatThread();
        _notifyManager.Start();
        if (_resumeChannel != null)
        {
          Log.Debug("TVHome.OnResume() - automatically turning on TV: {0}", _resumeChannel.DisplayName);
          AutoTurnOnTv(_resumeChannel);
          AutoFullScreenTv();
          _resumeChannel = null;
        }
      }
      finally
      {
        _suspended = false;
      }
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

      //plz add any newly added ID's to this list.

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
                 prev == (int)Window.WINDOW_TV_TUNING_DETAILS ||
                 prev == (int)Window.WINDOW_TV
               );
      if (!result && prev == (int)Window.WINDOW_FULLSCREEN_VIDEO && g_Player.IsTVRecording)
      {
        result = true;
      }
      return result;
    }

    public static void OnGlobalMessage(GUIMessage message)
    {
      switch (message.Message)
      {
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
        case GUIMessage.MessageType.GUI_MSG_GET_PROGRAM_INFO:
          {
            if (!Radio.CurrentChannel.IsWebstream() && Navigator.Channel != null && Navigator.Channel.CurrentProgram.Title != null 
              && Navigator.Channel.NextProgram.Title != null)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEND_PROGRAM_INFO, 0, 0, 0, 0, 0, null);
              msg.Label = Navigator.Channel.CurrentProgram.Title;
              msg.Label2 = Navigator.Channel.NextProgram.Title;
              GUIWindowManager.SendMessage(msg);
            }
            break;
          }
        case GUIMessage.MessageType.GUI_MSG_NOTIFY_REC:
          string heading = message.Label;
          string text = message.Label2;
          Channel ch = message.Object as Channel;
          //Log.Debug("Received rec notify message: {0}, {1}, {2}", heading, text, (ch != null).ToString()); //remove later
          string logo = TVUtil.GetChannelLogo(ch);
          GUIDialogNotify pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (pDlgNotify != null)
          {
            pDlgNotify.Reset();
            pDlgNotify.ClearAll();
            pDlgNotify.SetHeading(heading);
            if (!string.IsNullOrEmpty(text))
            {
              pDlgNotify.SetText(text);
            }
            pDlgNotify.SetImage(logo);
            pDlgNotify.TimeOut = 5;

            pDlgNotify.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM:
          {
            TVNotifyYesNoDialog tvNotifyDlg = (TVNotifyYesNoDialog)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TVNOTIFYYESNO);

            TVProgramDescription notify = message.Object as TVProgramDescription;
            if (tvNotifyDlg == null || notify == null)
            {
              return;
            }
            int minUntilStart = _preNotifyConfig / 60;
            if (notify.StartTime > DateTime.Now)
            {
              if (minUntilStart > 1)
              {
                tvNotifyDlg.SetHeading(String.Format(GUILocalizeStrings.Get(1018), minUntilStart));
              }
              else
              {
                tvNotifyDlg.SetHeading(1019); // Program is about to begin
              }
            }
            else
            {
              tvNotifyDlg.SetHeading(String.Format(GUILocalizeStrings.Get(1206), (DateTime.Now - notify.StartTime).Minutes.ToString()));
            }
            tvNotifyDlg.SetLine(1, notify.Title);
            tvNotifyDlg.SetLine(2, notify.Description);
            tvNotifyDlg.SetLine(4, String.Format(GUILocalizeStrings.Get(1207), notify.Channel.DisplayName));
            Channel c = notify.Channel;
            string strLogo = string.Empty;
            if (c.IsTv)
            {
              strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, c.DisplayName);
            }
            else if (c.IsRadio)
            {
              strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.Radio, c.DisplayName);
            }

            tvNotifyDlg.SetImage(strLogo);
            tvNotifyDlg.TimeOut = _notifyTVTimeout;
            if (_playNotifyBeep)
            {
              MediaPortal.Util.Utils.PlaySound("notify.wav", false, true);
            }
            tvNotifyDlg.SetDefaultToYes(false);
            tvNotifyDlg.DoModal(GUIWindowManager.ActiveWindow);

            if (tvNotifyDlg.IsConfirmed)
            {
              try
              {
                MediaPortal.Player.g_Player.Stop();

                if (c.IsTv)
                {
                  MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_TV);
                  TVHome.ViewChannelAndCheck(c);
                  if (TVHome.Card.IsTimeShifting && TVHome.Card.IdChannel == c.IdChannel)
                  {
                    g_Player.ShowFullScreenWindow();
                  }
                }
                else if (c.IsRadio)
                {
                  MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_RADIO);
                  Radio.CurrentChannel = c;
                  Radio.Play();
                }
              }
              catch (Exception e)
              {
                Log.Error("TVHome: TVNotification: Error on starting channel {0} after notification: {1} {2} {3}", notify.Channel.DisplayName, e.Message, e.Source, e.StackTrace);
              }

            }
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
      else if (g_Player.GetAudioDualMonoMode() != eAudioDualMonoMode.UNSUPPORTED)
      {
        g_Player.SetAudioDualMonoMode(eAudioDualMonoMode.STEREO);
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

      if (currentWindow.IsTv && type == g_Player.MediaType.TV)
      {
        return;
      }
      if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_RADIO || GUIWindowManager.ActiveWindow == (int)Window.WINDOW_RADIO_GUIDE)
      {
        return;
      }

      //gemx: fix for 0001181: Videoplayback does not work if tvservice.exe is not running
      bool isTS = (Card != null && Card.IsTimeShifting);
      if (Connected && isTS)
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

      StopPlayback();
      UpdateGUIonPlaybackStateChange(false);
    }

    private static void StopPlayback()
    {

      //gemx: fix for 0001181: Videoplayback does not work if tvservice.exe is not running 
      if (!Connected)
      {
        _recoverTV = true;
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

      _recoverTV = false;
      _playbackStopped = true;
    }

    public static bool ManualRecord(Channel channel, int dialogId)
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
      var server = new TvServer();

      VirtualCard card = null;
      Program prog = channel.CurrentProgram;
      bool isRecording;
      bool hasProgram = (prog != null);
      if (hasProgram)
      {
        prog.Refresh();//refresh the states from db
        isRecording = (prog.IsRecording || prog.IsRecordingOncePending);
      }
      else
      {
        isRecording = server.IsRecording(channel.IdChannel, out card);
      }

      if (!isRecording)
      {
        if (hasProgram)
        {
          GUIDialogMenuBottomRight pDlgOK =
            (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
          if (pDlgOK != null)
          {
            pDlgOK.Reset();
            pDlgOK.SetHeading(605); //my tv
            pDlgOK.AddLocalizedString(875); //current program

            bool doesManuelScheduleAlreadyExist = DoesManualScheduleAlreadyExist(channel);
            if (!doesManuelScheduleAlreadyExist)
            {
              pDlgOK.AddLocalizedString(876); //till manual stop
            }
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            switch (pDlgOK.SelectedId)
            {
              case 875:
                //record current program                  
                TVProgramInfo.CreateProgram(prog, (int)ScheduleRecordingType.Once, dialogId);
                GUIMessage msgManualRecord = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MANUAL_RECORDING_STARTED, 0, 0, 0, 0, 0, null);
                GUIWindowManager.SendMessage(msgManualRecord);
                return true;

              case 876:
                //manual
                StartRecordingSchedule(channel, true);
                return true;
             }
          }
        }
        else
        {
          //manual record
          StartRecordingSchedule(channel, true);
          return true;
        }
      }
      else
      {
        Schedule s = null;
        int idChannel = 0;
        if (hasProgram)
        {
          TVProgramInfo.IsRecordingProgram(prog, out s, false);
          if (s != null)
          {
            idChannel = s.ReferencedChannel().IdChannel;
          }
        }
        else
        {
          s = Schedule.Retrieve(card.RecordingScheduleId);
          idChannel = card.IdChannel;
        }

        if (s != null && idChannel > 0)
        {
          TVUtil.StopRecAndSchedWithPrompt(s, idChannel);
        }
      }
      return false;
    }

    private static bool DoesManualScheduleAlreadyExist(Channel channel)
    {
      Schedule existingSchedule = Schedule.FindNoEPGSchedule(channel);
      return (existingSchedule != null);
    }

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

    private void UpdateCurrentChannel()
    {
      if (!g_Player.Playing)
      {
        return;
      }
      Navigator.UpdateCurrentChannel();
      UpdateProgressPercentageBar();

      GUIControl.HideControl(GetID, (int)Controls.LABEL_REC_INFO);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_RECTANGLE);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_CHANNEL);
    }

    public static void UpdateTimeShift() { }

    private void OnActiveRecordings()
    {
      IList<Recording> ignoreActiveRecordings = new List<Recording>();
      OnActiveRecordings(ignoreActiveRecordings);
    }

    private void OnActiveRecordings(IList<Recording> ignoreActiveRecordings)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(200052); // Active Recordings      

      IList<Recording> activeRecordings = Recording.ListAllActive();
      if (activeRecordings != null && activeRecordings.Count > 0)
      {
        foreach (Recording activeRecording in activeRecordings)
        {
          if (!ignoreActiveRecordings.Contains(activeRecording))
          {
            GUIListItem item = new GUIListItem();
            string channelName = activeRecording.ReferencedChannel().DisplayName;
            string programTitle = activeRecording.Title.Trim(); // default is current EPG info

            item.Label = channelName;
            item.Label2 = programTitle;

            string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, channelName);
            if (string.IsNullOrEmpty(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }

            item.IconImage = strLogo;
            item.IconImageBig = strLogo;
            item.PinImage = "";
            dlg.Add(item);
          }
        }

        dlg.SelectedLabel = activeRecordings.Count;

        dlg.DoModal(this.GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }

        if (dlg.SelectedLabel < 0 || (dlg.SelectedLabel - 1 > activeRecordings.Count))
        {
          return;
        }

        Recording selectedRecording = activeRecordings[dlg.SelectedLabel];
        Schedule parentSchedule = selectedRecording.ReferencedSchedule();
        if (parentSchedule == null || parentSchedule.IdSchedule < 1)
        {
          return;
        }
        bool deleted = TVUtil.StopRecAndSchedWithPrompt(parentSchedule, selectedRecording.IdChannel);
        if (deleted && !ignoreActiveRecordings.Contains(selectedRecording))
        {
          ignoreActiveRecordings.Add(selectedRecording);
        }
        OnActiveRecordings(ignoreActiveRecordings); //keep on showing the list until --> 1) user leaves menu, 2) no more active recordings
      }
      else
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        if (pDlgOK != null)
        {
          pDlgOK.SetHeading(200052); //my tv
          pDlgOK.SetLine(1, GUILocalizeStrings.Get(200053)); // No Active recordings
          pDlgOK.SetLine(2, "");
          pDlgOK.DoModal(this.GetID);
        }
      }
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
      List<IUser> _users = new List<IUser>();
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
        IUser[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        if (users == null)
        {
          return;
        }
        for (int i = 0; i < users.Length; ++i)
        {
          IUser user = users[i];
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
            if (string.IsNullOrEmpty(strLogo))
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
      ManualRecord(Navigator.Channel, GetID);
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
        if (server.IsRecording(Navigator.Channel.IdChannel, out vc))
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
      //Log.Debug("updaterec: conn:{0}, rec:{1}", Connected, Card.IsRecording);
      // if we're recording tv, update gui with info
      if (Connected && Card.IsRecording)
      {
        int scheduleId = Card.RecordingScheduleId;
        if (scheduleId > 0)
        {
          Schedule schedule = Schedule.Retrieve(scheduleId);
          if (schedule != null)
          {
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
      }
      else
      {
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
      if (ts.TotalMilliseconds < PROGRESS_PERCENTAGE_UPDATE_INTERVAL)
      {
        return;
      }

      try
      {
        if (!Connected)
        {
          return;
        }
        
        //set audio video related media info properties.
        int currAudio = g_Player.CurrentAudioStream;
        if (currAudio > -1)
        {
          UpdateAudioProperties(currAudio);
        }

        // Check for recordings vs liveTv/Radio or Idle
        if (g_Player.IsTVRecording)
        {
          UpdateRecordingProperties();
        }
        else
        {
          UpdateTvProperties();
        }
      }
      finally
      {
        _updateProgressTimer = DateTime.Now;
      }
    }

    private static void UpdateTvProperties()
    {
      // No channel -> no EPG
      if (Navigator.Channel != null && !g_Player.IsRadio)
      {
        Channel infoChannel;
        if (Navigator.Channel.IsTv)
        {
          infoChannel = Navigator.Channel;
        }
        else
        {
          infoChannel = _lastTvChannel;
        }
        UpdateCurrentEpgProperties(infoChannel);
        UpdateNextEpgProperties(infoChannel);
        //Update lastTvChannel with current
        _lastTvChannel = infoChannel;
      }
    }

    private static void UpdateRecordingProperties()
    {
      double currentPosition = g_Player.CurrentPosition;
      double duration = g_Player.Duration;

      string startTime = Utils.SecondsToHMSString((int)currentPosition);
      string endTime = Utils.SecondsToHMSString((int)duration);

      double percentLivePoint = currentPosition / duration;
      percentLivePoint *= 100.0d;

      GUIPropertyManager.SetProperty("#TV.Record.percent1", percentLivePoint.ToString());
      GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
      GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");

      Recording rec = TvRecorded.ActiveRecording();

      if (rec != null)
      {
        GUIPropertyManager.SetProperty("#TV.View.title", rec.Title);
        GUIPropertyManager.SetProperty("#TV.View.compositetitle", TVUtil.GetDisplayTitle(rec));
        GUIPropertyManager.SetProperty("#TV.View.subtitle", rec.EpisodeName);
        GUIPropertyManager.SetProperty("#TV.View.episode", rec.EpisodeNumber);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.View.title", g_Player.currentTitle);
        GUIPropertyManager.SetProperty("#TV.View.compositetitle", g_Player.currentTitle);
      }
      string displayName = TvRecorded.GetRecordingDisplayName(rec);
      GUIPropertyManager.SetProperty("#TV.View.channel", displayName + " (" + GUILocalizeStrings.Get(604) + ")");
      GUIPropertyManager.SetProperty("#TV.View.description", g_Player.currentDescription);
      GUIPropertyManager.SetProperty("#TV.View.start", startTime);
      GUIPropertyManager.SetProperty("#TV.View.stop", endTime);

      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, displayName);
      GUIPropertyManager.SetProperty("#TV.View.thumb",
                                     string.IsNullOrEmpty(strLogo) ? "defaultVideoBig.png" : strLogo);
    }

    private static void UpdateAudioProperties(int currAudio)
    {
      string streamType = g_Player.AudioType(currAudio);

      GUIPropertyManager.SetProperty("#TV.View.IsAC3", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.IsMP1A", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.IsMP2A", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.IsAAC", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC", string.Empty);

      switch (streamType)
      {
        case "AC3":
        case "AC3plus": // just for the time being use the same icon for AC3 & AC3plus
          GUIPropertyManager.SetProperty("#TV.View.IsAC3",
                                         string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                       "ac3.png"));
          break;

        case "Mpeg1":
          GUIPropertyManager.SetProperty("#TV.View.IsMP1A",
                                         string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                       "mp1a.png"));
          break;

        case "Mpeg2":
          GUIPropertyManager.SetProperty("#TV.View.IsMP2A",
                                         string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                       "mp2a.png"));
          break;

        case "AAC":
          GUIPropertyManager.SetProperty("#TV.View.IsAAC",
                                         string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                       "aac.png"));
          break;

        case "LATMAAC":
          GUIPropertyManager.SetProperty("#TV.View.IsLATMAAC",
                                         string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\",
                                                       "latmaac3.png"));
          break;
      }
    }

    private static void UpdateCurrentEpgProperties(Channel ch)
    {

      bool hasChannel = (ch != null);
      Program current = null;
      if (hasChannel)
      {
        current = ch.CurrentProgram;
      }
      bool hasCurrentEPG = hasChannel && current != null;

      if (!hasChannel || !hasCurrentEPG)
      {
        ResetTvProperties();
        if (!hasChannel)
        {
          GUIPropertyManager.SetProperty("#TV.View.channel", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.thumb", String.Empty);
          Log.Debug("UpdateCurrentEpgProperties: no channel, returning");
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.View.channel", ch.DisplayName);
          SetTvThumbProperty(ch);
        }            
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.View.channel", ch.DisplayName);
        GUIPropertyManager.SetProperty("#TV.View.title", current.Title);
        GUIPropertyManager.SetProperty("#TV.View.compositetitle", TVUtil.GetDisplayTitle(current));
        GUIPropertyManager.SetProperty("#TV.View.start",
                                       current.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.View.stop",
                                       current.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.View.description", current.Description);
        GUIPropertyManager.SetProperty("#TV.View.subtitle", current.EpisodeName);
        GUIPropertyManager.SetProperty("#TV.View.episode", current.EpisodeNumber);
        GUIPropertyManager.SetProperty("#TV.View.genre", current.Genre);
        GUIPropertyManager.SetProperty("#TV.View.remaining",Utils.SecondsToHMSString(current.CalculateTimeRemaining()));
        GUIPropertyManager.SetProperty("#TV.View.remainingSeconds", Utils.SecondsToHMSStringSeconds(current.CalculateTimeRemaining()));
        GUIPropertyManager.SetProperty("#TV.View.remainingMinutes", Utils.SecondsToHMSStringMinutes(current.CalculateTimeRemaining()));
        SetTvThumbProperty(ch);

        TimeSpan ts = current.EndTime - current.StartTime;

        if (ts.TotalSeconds > 0)
        {
          // calculate total duration of the current program
          double programDuration = ts.TotalSeconds;

          //calculate where the program is at this time
          ts = (DateTime.Now - current.StartTime);
          double livePoint = ts.TotalSeconds;

          //calculate when timeshifting was started
          double timeShiftStartPoint = livePoint - g_Player.Duration;
          double playingPoint = timeShiftStartPoint + g_Player.CurrentPosition;
          if (timeShiftStartPoint < 0)
          {
            timeShiftStartPoint = 0;
          }

          double timeShiftStartPointPercent = timeShiftStartPoint / programDuration;
          timeShiftStartPointPercent *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.Record.percent1", timeShiftStartPointPercent.ToString());

          double playingPointPercent = playingPoint / programDuration;
          playingPointPercent *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.Record.percent2", playingPointPercent.ToString());

          double percentLivePoint = livePoint / programDuration;
          percentLivePoint *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.View.Percentage", percentLivePoint.ToString());
          GUIPropertyManager.SetProperty("#TV.Record.percent3", percentLivePoint.ToString());
        }
      }


    }

    private static void SetTvThumbProperty(Channel ch)
    {
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, ch.DisplayName);
      if (string.IsNullOrEmpty(strLogo))
      {
        strLogo = "defaultVideoBig.png";
      }
      GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);
    }

    private static void ResetTvProperties()
    {
      GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(736)); // no epg for this channel
      GUIPropertyManager.SetProperty("#TV.View.compositetitle", GUILocalizeStrings.Get(736)); // no epg for this channel
      GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.subtitle", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.episode", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.genre", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
      GUIPropertyManager.SetProperty("#TV.Record.percent1", "0");
      GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
      GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");
      GUIPropertyManager.SetProperty("#TV.View.remaining", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.remainingSeconds", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.remainingMinutes", string.Empty);
    }

    private static void UpdateNextEpgProperties(Channel ch)
    {
      Program next = null;
      if (ch == null)
      {
        Log.Debug("UpdateNextEpgProperties: no channel, returning");
      }
      else
      {
        next = ch.NextProgram;
        if (next == null)
        {
          Log.Debug("UpdateNextEpgProperties: no EPG data, returning");
        }
      }

      if (next != null)
      {
        GUIPropertyManager.SetProperty("#TV.Next.title", next.Title);
        GUIPropertyManager.SetProperty("#TV.Next.compositetitle", TVUtil.GetDisplayTitle(next));
        GUIPropertyManager.SetProperty("#TV.Next.start",
                                               next.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.Next.stop",
                                       next.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.Next.description", next.Description);
        GUIPropertyManager.SetProperty("#TV.Next.subtitle", next.EpisodeName);
        GUIPropertyManager.SetProperty("#TV.Next.episode", next.EpisodeNumber);
        GUIPropertyManager.SetProperty("#TV.Next.genre", next.Genre);
        GUIPropertyManager.SetProperty("#TV.Next.remaining", Utils.SecondsToHMSString(next.EndTime - next.StartTime));
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Next.title", GUILocalizeStrings.Get(736));          // no epg for this channel
        GUIPropertyManager.SetProperty("#TV.Next.compositetitle", GUILocalizeStrings.Get(736)); // no epg for this channel
        GUIPropertyManager.SetProperty("#TV.Next.start", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.stop", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.description", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.subtitle", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.episode", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.genre", String.Empty);
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

    #region audio selection section

    /// <summary>
    /// unit test enabled method. please respect this.
    /// run and/or modify the unit tests accordingly.
    /// </summary>
    public static int GetPreferedAudioStreamIndex(out eAudioDualMonoMode dualMonoMode)
    // also used from tvrecorded class
    {
      int idxFirstAc3 = -1; // the index of the first avail. ac3 found
      int idxFirstmpeg = -1; // the index of the first avail. mpg found
      int idxStreamIndexAc3 = -1; // the streamindex of ac3 found based on lang. pref
      int idxStreamIndexmpeg = -1; // the streamindex of mpg found based on lang. pref   
      int idx = -1; // the chosen audio index we return
      int idxLangPriAc3 = -1; // the lang priority of ac3 found based on lang. pref
      int idxLangPrimpeg = -1; // the lang priority of mpg found based on lang. pref         
      string ac3BasedOnLang = ""; // for debugging, what lang. in prefs. where used to choose the ac3 audio track ?
      string mpegBasedOnLang = "";
      // for debugging, what lang. in prefs. where used to choose the mpeg audio track ?      

      dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      IAudioStream[] streams = GetStreamsList();

      if (IsPreferredAudioLanguageAvailable())
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
      bool dualMonoModeEnabled = (g_Player.GetAudioDualMonoMode() != eAudioDualMonoMode.UNSUPPORTED);

      if (streams.Length == 1 && !ShouldApplyDualMonoMode(streams[0].Language))
      {
        Log.Info("Audio stream: switching to preferred AC3/MPEG audio stream 0 (only 1 track avail.)");
        return 0;
      }

      int priority = int.MaxValue;
      idxFirstAc3 = GetFirstAC3Index(streams);
      idxFirstmpeg = GetFirstMpegIndex(streams);

      UpdateAudioStreamIndexesAndPrioritiesBasedOnLanguage(streams, priority, ref idxStreamIndexmpeg,
                                                           ref mpegBasedOnLang, ref idxStreamIndexAc3, idxLangPriAc3,
                                                           idxLangPrimpeg, ref ac3BasedOnLang, out dualMonoMode);
      idx = GetAC3AudioStreamIndex(idxStreamIndexmpeg, idxStreamIndexAc3, ac3BasedOnLang, idx, idxFirstAc3);

      if (idx == -1 && _preferAC3)
      {
        Log.Info("Audio stream: no preferred AC3 audio stream found, trying mpeg instead.");
      }

      if (idx == -1 || !_preferAC3)
      // we end up here if ac3 selection didnt happen (no ac3 avail.) or if preferac3 is disabled.
      {
        if (IsPreferredAudioLanguageAvailable())
        {
          //did we find a mpeg track that matches our LANG prefs ?
          idx = GetMpegAudioStreamIndexBasedOnLanguage(idxStreamIndexmpeg, mpegBasedOnLang, idxStreamIndexAc3, idx,
                                                       idxFirstmpeg);
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
        Log.Info("Audio stream: no preferred stream found - switching to audio stream 0");
      }

      return idx;
    }

    private static int GetAC3AudioStreamIndex(int idxStreamIndexmpeg, int idxStreamIndexAc3, string ac3BasedOnLang,
                                              int idx, int idxFirstAc3)
    {
      if (_preferAC3)
      {
        if (IsPreferredAudioLanguageAvailable())
        {
          //did we find an ac3 track that matches our LANG prefs ?
          idx = GetAC3AudioStreamIndexBasedOnLanguage(idxStreamIndexmpeg, idxStreamIndexAc3, ac3BasedOnLang, idx,
                                                      idxFirstAc3);
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
      return idx;
    }

    private static void UpdateAudioStreamIndexesAndPrioritiesBasedOnLanguage(IAudioStream[] streams, int priority,
                                                                             ref int idxStreamIndexmpeg,
                                                                             ref string mpegBasedOnLang,
                                                                             ref int idxStreamIndexAc3,
                                                                             int idxLangPriAc3, int idxLangPrimpeg,
                                                                             ref string ac3BasedOnLang,
                                                                             out eAudioDualMonoMode dualMonoMode)
    {
      dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      if (IsPreferredAudioLanguageAvailable())
      {
        for (int i = 0; i < streams.Length; i++)
        {
          //now find the ones based on LANG prefs.        
          if (ShouldApplyDualMonoMode(streams[i].Language))
          {
            dualMonoMode = GetDualMonoMode(streams, i, ref priority, ref idxStreamIndexmpeg, ref mpegBasedOnLang);
            if (dualMonoMode != eAudioDualMonoMode.UNSUPPORTED)
            {
              break;
            }
          }
          else
          {
            // lower value means higher priority
            UpdateAudioStreamIndexesBasedOnLang(streams, i, ref idxStreamIndexmpeg, ref idxStreamIndexAc3,
                                                ref mpegBasedOnLang, ref idxLangPriAc3, ref idxLangPrimpeg, ref ac3BasedOnLang);
          }
        }
      }
    }

    private static int GetMpegAudioStreamIndexBasedOnLanguage(int idxStreamIndexmpeg, string mpegBasedOnLang,
                                                              int idxStreamIndexAc3, int idx, int idxFirstmpeg)
    {
      if (idxStreamIndexmpeg > -1)
      {
        idx = idxStreamIndexmpeg;
        Log.Info("Audio stream: switching to preferred MPEG audio stream {0}, based on LANG {1}", idx,
                 mpegBasedOnLang);
      }
      //if not, did we even find a mpeg track ?
      else if (idxFirstmpeg > -1)
      {
        //we did find a AC3 track, but not based on LANG - should we choose this or the mpeg track which is based on LANG.
        if (_preferAudioTypeOverLang || (idxStreamIndexAc3 == -1 && _preferAudioTypeOverLang))
        {
          idx = idxFirstmpeg;
          Log.Info(
            "Audio stream: switching to preferred MPEG audio stream {0}, NOT based on LANG (none avail. matching {1})",
            idx, mpegBasedOnLang);
        }
        else if (idxStreamIndexAc3 > -1)
        {
          idx = idxStreamIndexAc3;
          Log.Info("Audio stream: ignoring MPEG audio stream {0}", idx);
        }
      }
      return idx;
    }

    private static int GetAC3AudioStreamIndexBasedOnLanguage(int idxStreamIndexmpeg, int idxStreamIndexAc3,
                                                             string ac3BasedOnLang, int idx, int idxFirstAc3)
    {
      if (idxStreamIndexAc3 > -1)
      {
        idx = idxStreamIndexAc3;
        Log.Info("Audio stream: switching to preferred AC3 audio stream {0}, based on LANG {1}", idx, ac3BasedOnLang);
      }
      //if not, did we even find an ac3 track ?
      else if (idxFirstAc3 > -1)
      {
        //we did find an AC3 track, but not based on LANG - should we choose this or the mpeg track which is based on LANG.
        if (_preferAudioTypeOverLang || idxStreamIndexmpeg == -1)
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
      return idx;
    }

    private static void UpdateAudioStreamIndexesBasedOnLang(IAudioStream[] streams, int i, ref int idxStreamIndexmpeg,
                                                            ref int idxStreamIndexAc3, ref string mpegBasedOnLang,
                                                            ref int idxLangPriAc3, ref int idxLangPrimpeg,
                                                            ref string ac3BasedOnLang)
    {
      int langPriority = _preferredLanguages.IndexOf(streams[i].Language);
      string langSel = streams[i].Language;
      Log.Debug("Stream {0} lang {1}, lang priority index {2}", i, langSel, langPriority);

      // is the stream language preferred?
      if (langPriority >= 0)
      {
        // has the stream a higher priority than an old one or is this the first AC3 stream with lang pri (idxLangPriAc3 == -1) (AC3)
        bool isAC3 = IsStreamAC3(streams[i]);
        if (isAC3)
        {
          if (idxLangPriAc3 == -1 || langPriority < idxLangPriAc3)
          {
            Log.Debug("Setting AC3 pref");
            idxStreamIndexAc3 = i;
            idxLangPriAc3 = langPriority;
            ac3BasedOnLang = langSel;
          }
        }
        else //not AC3
        {
          // has the stream a higher priority than an old one or is this the first mpeg stream with lang pri (idxLangPrimpeg == -1) (mpeg)
          if (idxLangPrimpeg == -1 || langPriority < idxLangPrimpeg)
          {
            Log.Debug("Setting mpeg pref");
            idxStreamIndexmpeg = i;
            idxLangPrimpeg = langPriority;
            mpegBasedOnLang = langSel;
          }
        }
      }
    }

    private static bool IsStreamAC3(IAudioStream stream)
    {
      return (stream.StreamType == AudioStreamType.AC3 ||
              stream.StreamType == AudioStreamType.EAC3);
    }

    private static bool ShouldApplyDualMonoMode(string language)
    {
      bool dualMonoModeEnabled = (g_Player.GetAudioDualMonoMode() != eAudioDualMonoMode.UNSUPPORTED);
      return (dualMonoModeEnabled && language.Length == 6);
    }

    private static int GetFirstAC3Index(IAudioStream[] streams)
    {
      int idxFirstAc3 = -1;

      for (int i = 0; i < streams.Length; i++)
      {
        if (IsStreamAC3(streams[i]))
        {
          idxFirstAc3 = i;
          break;
        }
      }
      return idxFirstAc3;
    }

    private static int GetFirstMpegIndex(IAudioStream[] streams)
    {
      int idxFirstMpeg = -1;

      for (int i = 0; i < streams.Length; i++)
      {
        if (!IsStreamAC3(streams[i]))
        {
          idxFirstMpeg = i;
          break;
        }
      }
      return idxFirstMpeg;
    }

    private static eAudioDualMonoMode GetDualMonoMode(IAudioStream[] streams, int currentIndex, ref int priority,
                                                      ref int idxStreamIndexmpeg, ref string mpegBasedOnLang)
    {
      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      string leftAudioLang = streams[currentIndex].Language.Substring(0, 3);
      string rightAudioLang = streams[currentIndex].Language.Substring(3, 3);

      int indexLeft = _preferredLanguages.IndexOf(leftAudioLang);
      if (indexLeft >= 0 && indexLeft < priority)
      {
        dualMonoMode = eAudioDualMonoMode.LEFT_MONO;
        mpegBasedOnLang = leftAudioLang;
        idxStreamIndexmpeg = currentIndex;
        priority = indexLeft;
      }

      int indexRight = _preferredLanguages.IndexOf(rightAudioLang);
      if (indexRight >= 0 && indexRight < priority)
      {
        dualMonoMode = eAudioDualMonoMode.RIGHT_MONO;
        mpegBasedOnLang = rightAudioLang;
        idxStreamIndexmpeg = currentIndex;
        priority = indexRight;
      }
      return dualMonoMode;
    }

    private static bool IsPreferredAudioLanguageAvailable()
    {
      return (_preferredLanguages != null && _preferredLanguages.Count > 0);
    }

    private static IAudioStream[] GetStreamsList()
    {
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
        string[] lang = stream.Language.Split('(');
        if (lang.Length > 1)
        {
          stream.Language = lang[1].Substring(0, lang[1].Length - 1);
        }
        streamsList.Add(stream);
      }
      return streamsList.ToArray();
    }

    #endregion

    private static void ChannelTuneFailedNotifyUser(TvResult succeeded, bool wasPlaying, Channel channel)
    {
      GUIGraphicsContext.RenderBlackImage = false;

      _lastError.Result = succeeded;
      _lastError.FailingChannel = channel;
      _lastError.Messages.Clear();

      int TextID = 0;
      _lastError.Messages.Add(GUILocalizeStrings.Get(1500));
      switch (succeeded)
      {
        case TvResult.NoPmtFound:
          TextID = 1498;
          break;
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
        case TvResult.TuneCancelled:
          TextID = 1524;
          break;
        case TvResult.UnknownError:
          // this error can also happen if we have no connection to the server.
          if (!Connected) // || !IsRemotingConnected())
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
          if (!Connected) // || !IsRemotingConnected())
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
      string caption = GUILocalizeStrings.Get(605) + " - " + _lastError.FailingChannel.DisplayName;
      pDlgNotify.SetHeading(caption); //my tv
      pDlgNotify.SetImage(TVUtil.GetChannelLogo(_lastError.FailingChannel));
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
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_TV_ERROR_NOTIFY, GUIWindowManager.ActiveWindow, 0,
                                        0, 0, 0,
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
        Log.Info("TVHome.ViewChannelAndCheck(): system just woke up...waiting {0} ms., suspended {2}", _waitonresume,
                 _suspended);
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
      var userCopy = Card.User.Clone() as IUser;
      if (userCopy != null) 
      {
        userCopy.Name = Dns.GetHostName();
        ChannelState CurrentChanState = TvServer.GetChannelState(channel.IdChannel, userCopy);
        if (CurrentChanState == ChannelState.nottunable)
        {
          ChannelTuneFailedNotifyUser(TvResult.AllCardsBusy, false, channel);
          return false;
        }
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

      if (!Connected)
      {
        return false;
      }

      _status.Clear();

      _doingChannelChange = false;

      try
      {
        checkResult = PreTuneChecks(channel, out doContinue);
        if (doContinue == false)
          return checkResult;

        _doingChannelChange = true;
        TvResult succeeded;


        IUser user = new User();
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
        if (_status.AllSet(LiveTvStatus.WasPlaying | LiveTvStatus.CardChange))
        {
          Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. CardId:{0}/{1}, RTSP:{2}", Card.Id, newCardId,
                    Card.RTSPUrl);
          Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. Timeshifting:{0}", Card.TimeShiftFileName);
          Log.Debug("TVHome.ViewChannelAndCheck(): rebuilding graph (card changed) - timeshifting continueing.");
        }
        if (_status.IsSet(LiveTvStatus.WasPlaying))
        {
          RenderBlackImage();
          g_Player.PauseGraph();
        }
        else
        {
          // if CI menu is not attached due to card change, do it if graph was not playing 
          // (some handlers use polling threads that get stopped on graph stop)
          if (_status.IsNotSet(LiveTvStatus.CardChange))
            RegisterCiMenu(newCardId);
        }

        // if card was not changed
        if (_status.IsNotSet(LiveTvStatus.CardChange))
        {
          g_Player.OnZapping(0x80); // Setup Zapping for TsReader, requesting new PAT from stream
        }
        bool cardChanged = false;
        succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card, out cardChanged);

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

        if (card != null && card.NrOfOtherUsersTimeshiftingOnCard > 0)
        {
          _status.Set(LiveTvStatus.SeekToEndAfterPlayback);
        }

        if (cardChanged)
        {
          _status.Set(LiveTvStatus.CardChange);
          if (card != null)
          {
            RegisterCiMenu(card.Id);
          }
          _status.Reset(LiveTvStatus.WasPlaying);
        }
        else
        {
          _status.Reset(LiveTvStatus.CardChange);
          _status.Set(LiveTvStatus.SeekToEnd);
        }

        // Update channel navigator
        if (Navigator.Channel != null &&
            (channel.IdChannel != Navigator.Channel.IdChannel || (Navigator.LastViewedChannel == null)))
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
        if (!g_Player.Playing || _status.IsSet(LiveTvStatus.CardChange) || (g_Player.Playing && !(g_Player.IsTV || g_Player.IsRadio)))
        {
          StartPlay();

          // if needed seek to end
          if (_status.IsSet(LiveTvStatus.SeekToEndAfterPlayback))
          {
            double dTime = g_Player.Duration - 5;
            g_Player.SeekAbsolute(dTime);
          }
        }
        try
        {

          TvTimeShiftPositionWatcher.SetNewChannel(channel.IdChannel);
        }
        catch
        {
          //ignore, error already logged
        }

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
        g_Player.Stop();
        Card.StopTimeShifting();
        return false;
      }
      finally
      {
        StopRenderBlackImage();        
        _userChannelChanged = false;
        FireOnChannelChangedEvent();
        Navigator.UpdateCurrentChannel();
      }
    }

    private static void FireOnChannelChangedEvent()
    {
      if (OnChannelChanged != null)
      {
        OnChannelChanged();
      }
    }    

    public static void ViewChannel(Channel channel)
    {
      ViewChannelAndCheck(channel);      
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

      TvLibrary.Interfaces.IChannel channel = Card.Channel;
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

      benchClock.Stop();
      Log.Warn("tvhome:startplay.  Phase 1 - {0} ms - Done method initialization",
               benchClock.ElapsedMilliseconds.ToString());
      benchClock.Reset();
      benchClock.Start();

      timeshiftFileName = TVUtil.GetFileNameForTimeshifting();
      bool useRTSP = UseRTSP();

      Log.Info("tvhome:startplay:{0} - using rtsp mode:{1}", timeshiftFileName, useRTSP);

      if (!useRTSP)
      {
        bool tsFileExists = false;
        int timeout = 0;
        while (!tsFileExists && timeout < 50)
        {
          tsFileExists = File.Exists(timeshiftFileName);
          if (!tsFileExists)
          {
            Log.Info("tvhome:startplay: waiting for TS file {0}", timeshiftFileName);
            timeout++;
            Thread.Sleep(10);
          }
        }
      }

      // Force use TsReader
      if (!g_Player.Play(timeshiftFileName, mediaType, null, false))
      {
        StopPlayback();
      }

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
      //keyboard.Title = title;
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
    /// Pass the CiMenu to TvHome so that Process can handle it in own thread
    /// </summary>
    /// <param name="Menu"></param>
    public static void ProcessCiMenu(CiMenu Menu)
    {
      lock (CiMenuLock)
      {
        CiMenuList.Add(Menu);
        if (CiMenuActive)       // Just suppose if a new menu is coming from CAM, last one can be trashed.
          dlgCiMenu.Reset();
        Log.Debug("ProcessCiMenu {0} {1} ", Menu, CiMenuList.Count);
      }
    }

    /// <summary>
    /// Handles all CiMenu actions from callback
    /// </summary>
    public static void ShowCiMenu()
    {
      lock (CiMenuLock)
      {
        if (CiMenuActive || CiMenuList.Count == 0) return;
        currentCiMenu = CiMenuList[0];
        CiMenuList.RemoveAt(0);
        CiMenuActive = true; // avoid re-entrance from process()
      }

      if (dlgCiMenu == null)
      {
        dlgCiMenu =
          (GUIDialogCIMenu)
          GUIWindowManager.GetWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_DIALOG_CIMENU);
      }

      switch (currentCiMenu.State)
      {
        // choices available, so show them
        case TvLibrary.Interfaces.CiMenuState.Ready:
          dlgCiMenu.Reset();
          dlgCiMenu.SetHeading(currentCiMenu.Title, currentCiMenu.Subtitle, currentCiMenu.BottomText); // CI Menu

          for (int i = 0; i < currentCiMenu.NumChoices; i++) // CI Menu Entries
            dlgCiMenu.Add(currentCiMenu.MenuEntries[i].Message); // take only message, numbers come from dialog

          // show dialog and wait for result       
          dlgCiMenu.DoModal(GUIWindowManager.ActiveWindow);
          if (currentCiMenu.State != TvLibrary.Interfaces.CiMenuState.Error)
          {
            if (dlgCiMenu.SelectedId != -1)
            {
              TVHome.Card.SelectCiMenu(Convert.ToByte(dlgCiMenu.SelectedId));
            }
            else
            {
              if (CiMenuList.Count == 0)      // Another menu is pending, do not answer...
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
            _dialogNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
          }
          if (null != _dialogNotify)
          {
            _dialogNotify.Reset();
            _dialogNotify.ClearAll();
            _dialogNotify.SetHeading(currentCiMenu.Title);
            _dialogNotify.SetText(String.Format("{0}\r\n{1}", currentCiMenu.Subtitle, currentCiMenu.BottomText));
            _dialogNotify.TimeOut = 2; // seconds
            _dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;

        // requests require users input so open keyboard
        case TvLibrary.Interfaces.CiMenuState.Request:
          String result = "";
          if (
            GetKeyboard(currentCiMenu.RequestText, currentCiMenu.AnswerLength, currentCiMenu.Password, ref result) ==
            true)
          {
            TVHome.Card.SendMenuAnswer(false, result); // send answer, cancel=false
          }
          else
          {
            TVHome.Card.SendMenuAnswer(true, null); // cancel request 
          }
          break;
        case CiMenuState.Close:
          if (_dialogNotify != null)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _dialogNotify.GetID, 0, 0, 0, 0, null);
            _dialogNotify.OnMessage(msg);	// Send a de-init msg to hide the current notify dialog
          }
          if (dlgCiMenu != null)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, dlgCiMenu.GetID, 0, 0, 0, 0, null);
            dlgCiMenu.OnMessage(msg);	// Send a de-init msg to hide the current CI menu dialog
          }
          break;
      }

      CiMenuActive = false; // finished
      currentCiMenu = null; // reset menu
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
      TvPlugin.TVHome.ProcessCiMenu(Menu);
    }
    catch
    {
      Menu = new CiMenu("Remoting Exception", "Communication with server failed", null,
                        TvLibrary.Interfaces.CiMenuState.Error);
      // pass menu to calling dialog
      TvPlugin.TVHome.ProcessCiMenu(Menu);
    }
  }
}

#endregion