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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Dialogs.Dialogs;
using MediaPortal;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.TvPlugin.EventHandlers;
using Mediaportal.TV.TvPlugin.Helper;
using Action = MediaPortal.GUI.Library.Action;
using Log = Mediaportal.TV.Server.TVLibrary.Interfaces.Logging.Log;

#endregion

namespace Mediaportal.TV.TvPlugin
{
  /// <summary>
  /// TV Home screen.
  /// </summary>
  [PluginIcons("Resources\\TvPlugin.TVPlugin.gif", "Resources\\TvPlugin.TVPluginDisabled.gif")]
  public class TVHome : GUIInternalWindow, ISetupForm, IShowPlugin, IPluginReceiver, ITvServerEventCallbackClient
  {


    #region constants

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

    private const int AUDIO_STREAM_PRIORITY_NOT_DEFINED = 100;

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

    private Channel _resumeChannel = null;
    private static DateTime _updateProgressTimer = DateTime.MinValue;
    private static ChannelNavigator m_navigator;
    private static TVUtil _util;
    private static IVirtualCard _card = null;
    private static DateTime _updateTimer = DateTime.Now;
    private static bool _autoTurnOnTv = false;
    private static int _waitonresume = 0;
    public static bool settingsLoaded = false;
    private TvNotifyManager _notifyManager;
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
    private static bool _onPageLoadDone = false;
    private static bool _userChannelChanged = false;
    private static bool _showChannelStateIcons = true;
    private static bool _doingHandleServerNotConnected = false;
    private static bool _doingChannelChange = false;
    private static bool _ServerNotConnectedHandled = false;
    private static bool _recoverTV = false;
    private static bool _connected = false;
    private static bool _isAnyCardRecording = false;


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

    // error handling
    public class ChannelErrorInfo
    {
      public Channel FailingChannel;
      public TvResult Result;
      public List<String> Messages = new List<string>();
    }

    public static ChannelErrorInfo _lastError = new ChannelErrorInfo();
    private static HeartbeatEventHandler _heartbeatEventHandler;

    // CI Menu
    private static CiMenuEventEventHandler _ciMenuEventEventHandler;
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

    private static readonly ServerMonitor _serverMonitor = new ServerMonitor();

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
      _notifyManager = new TvNotifyManager();
    }

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\mytvhomeServer.xml");
    }

    public override void OnAdded()
    {
      
#if DEBUG
      if (File.Exists(@"c:\debug_tvplugin.txt"))
      {
        System.Diagnostics.Debugger.Launch();
      }
#endif      
      IntegrationProviderHelper.Register();
      this.LogInfo("TVHome:OnAdded");

      GUIGraphicsContext.OnBlackImageRendered += new BlackImageRenderedHandler(OnBlackImageRendered);
      GUIGraphicsContext.OnVideoReceived += new VideoReceivedHandler(OnVideoReceived);

      _waitForBlackScreen = new ManualResetEvent(false);
      _waitForVideoReceived = new ManualResetEvent(false);

      Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.AudioTracksReady += new g_Player.AudioTracksReadyHandler(OnAudioTracksReady);

      GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);

      // replace g_player's ShowFullScreenWindowTV
      g_Player.ShowFullScreenWindowTV = ShowFullScreenWindowTVHandler;

      try
      {
        TVHome.OnChannelChanged -= new OnChannelChangedDelegate(ForceUpdates);
        TVHome.OnChannelChanged += new OnChannelChangedDelegate(ForceUpdates);

        m_navigator = new ChannelNavigator();
        m_navigator.OnZapChannel -= new ChannelNavigator.OnZapChannelDelegate(ForceUpdates);
        m_navigator.OnZapChannel += new ChannelNavigator.OnZapChannelDelegate(ForceUpdates);

        // Make sure that we have valid hostname for the TV server
        SetRemoteControlHostName();

        // Wake up the TV server, if required
        HandleWakeUpTvServer();
        StartServerMonitor();
        SubscribeToEventService();
        RegisterUserForHeartbeatMonitoring();
        LoadSettings();

        string pluginVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        string tvServerVersion = Connected ? ServiceAgents.Instance.ControllerServiceAgent.GetAssemblyVersion : "Unknown";

        if (Connected && pluginVersion != tvServerVersion)
        {
          string strLine = "TvPlugin and TvServer don't have the same version.\r\n";
          strLine += "TvServer Version: " + tvServerVersion + "\r\n";
          strLine += "TvPlugin Version: " + pluginVersion;
          this.LogError(strLine);
        }
        else
        {
          this.LogInfo("TVHome V" + pluginVersion + ":ctor");
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TVHome: Error occured in Init()");
      }
      finally
      {
        _notifyManager.Start();
      }
    }

    private void StartServerMonitor()
    {
      _serverMonitor.OnServerConnected -= OnServerConnected;
      _serverMonitor.OnServerDisconnected -= OnServerDisconnected;
      _serverMonitor.OnServerConnected += OnServerConnected;
      _serverMonitor.OnServerDisconnected += OnServerDisconnected;
      _serverMonitor.Start();
    }

    private void SubscribeToEventService()
    {
      ServiceAgents.Instance.EventServiceAgent.RegisterTvServerEventCallbacks(this);
    }

    private void UnSubscribeFromEventService()
    {
      ServiceAgents.Instance.EventServiceAgent.UnRegisterTvServerEventCallbacks(this, !Connected);
    }



    private void OnTimeShiftingForcefullyStopped(string username, TvStoppedReason tvStoppedReason)
    {
      if (username.Equals(TVHome.Card.User.Name))
      {
        Action keyAction = new Action(Action.ActionType.ACTION_STOP, 0, 0);
        GUIGraphicsContext.OnAction(keyAction);
        _playbackStopped = true;

        if (tvStoppedReason != TvStoppedReason.UnknownReason)
        {
          this.LogDebug("TVHome: Timeshifting seems to have stopped - TvStoppedReason:{0}", tvStoppedReason);
          string errMsg = "";

          switch (tvStoppedReason)
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

    private void OnRecordingEnded(int idRecording)
    {
      if (Connected)
      {
        _isAnyCardRecording = ServiceAgents.Instance.ControllerServiceAgent.IsAnyCardRecording();
      }
      UpdateRecordingIndicator();
      UpdateStateOfRecButton();
    }

    private void OnRecordingStarted(int idRecording)
    {
      if (Connected)
      {
        _isAnyCardRecording = true;
      }
      UpdateRecordingIndicator();
      UpdateStateOfRecButton();
    }

    private static IDictionary<int, ChannelState> _tvChannelStatesList = new Dictionary<int, ChannelState>();
    private readonly static object _channelStatesLock = new object();


    private void OnChannelStatesChanged(IDictionary<int, ChannelState> channelStates)
    {
      lock (_channelStatesLock)
      {
        _tvChannelStatesList = channelStates;
      }
    }

    private static void RetrieveChannelStatesFromServer()
    {
      lock (_channelStatesLock)
      {
        _tvChannelStatesList = ServiceAgents.Instance.ControllerServiceAgent.GetAllChannelStatesCached(Card.User.Name);
      }
    }


    /// <summary>
    /// Gets called by the runtime when a  window will be destroyed
    /// Every window window should override this method and cleanup any resources
    /// </summary>
    /// <returns></returns>
    public override void DeInit()
    {
      OnPageDestroy(-1);

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
      this.LogInfo("TVHome:OnPageLoad");

      if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).PreviousWindowId != (int)Window.WINDOW_TVFULLSCREEN)
      {
        _playbackStopped = false;
      }

      btnActiveStreams.Label = GUILocalizeStrings.Get(692);
      WaitForServiceConnection();
      if (!Connected)
      {
        if (!_onPageLoadDone)
        {          
          ShowTvEngineSettingsUI();          
          return;
        }
        else
        {
          UpdateStateOfRecButton();
          UpdateProgressPercentageBar();
          UpdateRecordingIndicator();
          return;
        }
      }

      // stop the old recorder.
      // DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.DataSourceOnly;
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

      // set video window position
      if (videoWindow != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width,
                                                       videoWindow.Height);
      }

      // start viewing tv... 
      GUIGraphicsContext.IsFullScreenVideo = false;
      Channel channel = Navigator.Channel.Entity;
      if (channel == null || channel.MediaType == (int)MediaTypeEnum.Radio)
      {
        if (Navigator.CurrentGroup != null && Navigator.Groups.Count > 0)
        {
          Navigator.SetCurrentGroup(Navigator.Groups[0].GroupName);
          GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.Groups[0].GroupName);
        }
        if (Navigator.CurrentGroup != null)
        {
          if (Navigator.CurrentGroup.GroupMaps.Count > 0)
          {
            GroupMap gm = (GroupMap)Navigator.CurrentGroup.GroupMaps[0];
            channel = gm.Channel;
          }
        }
      }

      if (channel != null)
      {
        this.LogInfo("tv home init:{0}", channel.DisplayName);
        if (!_suspended)
        {
          AutoTurnOnTv(channel);
        }
        else
        {
          _resumeChannel = channel;
        }
        GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.CurrentGroup.GroupName);
        this.LogInfo("tv home init:{0} done", channel.DisplayName);
      }

      if (!_suspended)
      {
        AutoFullScreenTv();
      }

      _onPageLoadDone = true;
      _suspended = false;

      UpdateGUIonPlaybackStateChange();
      UpdateCurrentChannel();
      UpdateRecordingIndicator();
      UpdateStateOfRecButton();
    }

    private void AutoTurnOnTv(Channel channel)
    {
      if (_autoTurnOnTv && !_playbackStopped && !wasPrevWinTVplugin())
      {
        if (!wasPrevWinTVplugin())
        {
          _userChannelChanged = false;
        }
        ViewChannelAndCheck(channel, 0);
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
            this.LogDebug("GUIGraphicsContext.IsFullScreenVideo {0}", GUIGraphicsContext.IsFullScreenVideo);
            bool wasFullScreenTV = (PreviousWindowId == (int)Window.WINDOW_TVFULLSCREEN);

            if (!wasFullScreenTV)
            {
              if (!wasPrevWinTVplugin())
              {
                this.LogDebug("TVHome.AutoFullScreenTv(): setting autoFullScreen");
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
          this.LogWarn("TVHome.OnClicked(): EndTvOff {0} ms", benchClock.ElapsedMilliseconds.ToString());
          benchClock.Stop();
          return;
        }
        else
        {
          // tv on
          this.LogInfo("TVHome:turn tv on {0}", Navigator.CurrentChannel);

          // stop playing anything
          if (g_Player.Playing)
          {
            if (g_Player.IsTV && !g_Player.IsTVRecording)
            {
              //already playing tv...
            }
            else
            {
              this.LogWarn("TVHome.OnClicked: Stop Called - {0} ms", benchClock.ElapsedMilliseconds.ToString());
              g_Player.Stop(true);
            }
          }
        }

        // turn tv on/off        
        if (Navigator.Channel.Entity.MediaType == (int)MediaTypeEnum.TV)
        {
          ViewChannelAndCheck(Navigator.Channel.Entity, 0);
        }
        else
        // current channel seems to be non-tv (radio ?), get latest known tv channel from xml config and use this instead
        {
          Settings xmlreader = new MPSettings();
          string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
          Channel currentChannel = Navigator.GetChannel(currentchannelName);
          ViewChannelAndCheck(currentChannel, 0);
        }

        UpdateStateOfRecButton();
        UpdateGUIonPlaybackStateChange();
        //UpdateProgressPercentageBar();
        benchClock.Stop();
        this.LogWarn("TVHome.OnClicked(): Total Time - {0} ms", benchClock.ElapsedMilliseconds.ToString());
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
          break;
        case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
          {
            // we only want to resume TV if previous window is NOT a tvplugin based one. (ex. tvguide.)
            if (_autoTurnOnTv && !wasPrevWinTVplugin())
            {
              //restart viewing...  
              this.LogInfo("tv home msg resume tv:{0}", Navigator.CurrentChannel);
              ViewChannel(Navigator.Channel.Entity);
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    private static void ForceUpdates()
    {
      _updateTimer = DateTime.Now.AddMilliseconds(-1 * (PROCESS_UPDATE_INTERVAL + 1));
      _updateProgressTimer = DateTime.Now.AddMilliseconds(-1 * (PROGRESS_PERCENTAGE_UPDATE_INTERVAL + 1));
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

    public static void StartRecordingSchedule(ChannelBLL channel, bool manual)
    {

      if (manual) // until manual stop
      {
        Schedule newSchedule = ScheduleFactory.CreateSchedule(channel.Entity.IdChannel,
                                                              GUILocalizeStrings.Get(413) + " (" + channel.Entity.DisplayName +
                                                              ")",
                                                              DateTime.Now, DateTime.Now.AddDays(1));
        newSchedule.PreRecordInterval = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("preRecordInterval", "5").Value);
        newSchedule.PostRecordInterval = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("postRecordInterval", "5").Value);
        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(newSchedule);
        ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
      }
      else // current program
      {
        // lets find any canceled episodes that match this one we want to create, if found uncancel it.
        Schedule existingParentSchedule = ServiceAgents.Instance.ScheduleServiceAgent.RetrieveSeriesByStartEndTimes(channel.Entity.IdChannel, channel.CurrentProgram.Title,
                                                          channel.CurrentProgram.StartTime,
                                                          channel.CurrentProgram.EndTime);
        if (existingParentSchedule != null)
        {
          foreach (CanceledSchedule cancelSched in existingParentSchedule.CanceledSchedules)
          {
            if (cancelSched.CancelDateTime == channel.CurrentProgram.StartTime)
            {
              ServiceAgents.Instance.ScheduleServiceAgent.UnCancelSerie(existingParentSchedule, channel.CurrentProgram.StartTime, channel.CurrentProgram.IdChannel);
              ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
              return;
            }
          }
        }

        // ok, no existing schedule found with matching canceled schedules found. proceeding to add the schedule normally
        Schedule newSchedule = ScheduleFactory.CreateSchedule(channel.Entity.IdChannel, channel.CurrentProgram.Title,
                                            channel.CurrentProgram.StartTime, channel.CurrentProgram.EndTime);
        newSchedule.PreRecordInterval = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("preRecordInterval", "5").Value);
        newSchedule.PostRecordInterval = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("postRecordInterval", "5").Value);
        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(newSchedule);
        ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
      }
    }

    public static bool UseRTSP()
    {
      if (!settingsLoaded)
      {
        LoadSettings();
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
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

      pDlgOK.Reset();
      pDlgOK.SetHeading(257); //error
      if (Navigator != null && Navigator.CurrentChannel != null && g_Player.IsTV)
      {
        pDlgOK.SetLine(1, Navigator.CurrentChannel);
      }
      else
      {
        pDlgOK.SetLine(1, "");
      }
      pDlgOK.SetLine(2, GUILocalizeStrings.Get(1510)); //Connection to TV server lost
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
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
      IControllerService iControllerService = ServiceAgents.Instance.ControllerServiceAgent; //calling instance will make sure the state is refreshed.
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
          bool connected = WaitForServiceConnection();
          if (!connected)
          {
            return true;
          }
        }
      }
      catch (Exception e)
      {
        //we assume that server is disconnected.
        Log.Error(e, "TVHome.HandleServerNotConnected caused an error");
        return true;
      }
      finally
      {
        _doingHandleServerNotConnected = false;
      }
      return false;
    }

    private static void ShowTvEngineSettingsUI()
    {
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
    }

    public static void ShowTvEngineSettingsUIIfConnectionDown()
    {
      if (!WaitForServiceConnection())
      {
        ShowTvEngineSettingsUI();
      }
    }

    private static bool WaitForServiceConnection()
    {
      bool success = _serverMonitor.WaitForConnection(10000);      
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



    public static bool IsAnyCardRecording
    {
      get { return _isAnyCardRecording; }
    }

    public static bool Connected
    {
      get { return _connected; }
      set { _connected = value; }
    }

    public static IVirtualCard Card
    {
      get
      {
        if (_card == null)
        {
          IUser user = new User();
          _card = new VirtualCard(user);
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

    private static void LoadSettings()
    {
      if (settingsLoaded)
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
            Log.Error(ex, "TVHome: Error resolving hostname");
            return;
          }
        }
      }
      ServiceAgents.Instance.Hostname = hostName;
      Log.Info("Remote control:master server :{0}", ServiceAgents.Instance.Hostname);
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
            if (!IPAddress.TryParse(ServiceAgents.Instance.Hostname, out ipAddress))
            {
              // Get IP address of the TV server
              try
              {
                IPAddress[] ips = Dns.GetHostAddresses(ServiceAgents.Instance.Hostname);

                Log.Debug("TVHome: WOL - GetHostAddresses({0}) returns:", ServiceAgents.Instance.Hostname);

                foreach (IPAddress ip in ips)
                {
                  Log.Debug("    {0}", ip);
                }

                // Use first valid IP address
                ipAddress = ips[0];
              }
              catch (Exception ex)
              {
                Log.Error(ex, "TVHome: WOL - Failed GetHostAddress");
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

            if (wakeOnLanManager.WakeupSystem(hwAddress, ServiceAgents.Instance.Hostname, intTimeOut))
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
            Log.Error(ex, "TVHome: WOL - Failed to start the TV server");
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
    //    this.LogDebug("CiMenu: PrepareCiMenu");
    //    ciMenuHandler = new CiMenuHandler();
    //    // opens remoting and attach local eventhandler to server event, call only once
    //    RemoteControl.RegisterCiMenuCallbacks(ciMenuHandler);
    //  }
    //  // Check if card supports CI menu
    //  if (newCardId != -1 && ServiceAgents.Instance.ControllerService.CiMenuSupported(newCardId))
    //  {
    //    // Enable CI menu handling in card
    //    ServiceAgents.Instance.ControllerService.SetCiMenuHandler(newCardId, null);
    //    this.LogDebug("TvPlugin: CiMenuHandler attached to new card {0}", newCardId);
    //  }
    //}

    private void OnServerConnected()
    {
      bool recovered = false;
      if (!Connected)
      {
        this.LogInfo("TVHome: OnServerConnected, recovered from a disconnection");
        recovered = true;
      }

      ServiceAgents.Instance.ReConnect();
      ServiceAgents.Instance.EventServiceAgent.ReConnect();

      Connected = true;

      _ServerNotConnectedHandled = false;
      if (_recoverTV)
      {
        _recoverTV = false;
        GUIMessage initMsg = null;
        initMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, (int)Window.WINDOW_TV_OVERLAY, 0, 0, 0, 0,
                                 null);
        GUIWindowManager.SendThreadMessage(initMsg);
      }
      if (recovered)
      {        
        _isAnyCardRecording = ServiceAgents.Instance.ControllerServiceAgent.IsAnyCardRecording();

        RetrieveChannelStatesFromServer();
        UpdateStateOfRecButton();
        UpdateProgressPercentageBar();
        UpdateRecordingIndicator();
      }
    }

    private void OnServerDisconnected()
    {
#if !DEBUG
      if (Connected)
      {
        this.LogInfo("TVHome: OnServerDisconnected");
      }
      Connected = false;

      ServiceAgents.Instance.EventServiceAgent.Disconnect();
      ServiceAgents.Instance.Disconnect();
      

      HandleServerNotConnected();
#endif
    }

    private void Application_ApplicationExit(object sender, EventArgs e)
    {
      try
      {
        _notifyManager.Stop();
        StopServerMonitor();
        UnsubscribeFromEventService();
        UnRegisterUserForHeartbeatMonitoring();
        UnRegisterCiMenu();

        if (Card.IsTimeShifting)
        {
          Card.User.Name = new User().Name;
          Card.StopTimeShifting();
        }
        _notifyManager.Stop();
      }
      catch (Exception) { }
    }

    private void StopServerMonitor()
    {
      _serverMonitor.OnServerConnected -= OnServerConnected;
      _serverMonitor.OnServerDisconnected -= OnServerDisconnected;
      _serverMonitor.Stop();
    }

    private void UnsubscribeFromEventService()
    {
      ServiceAgents.Instance.EventServiceAgent.UnRegisterHeartbeatCallbacks(_heartbeatEventHandler, !Connected);
      //ServiceAgents.Instance.EventServiceAgent.Unsubscribe(Dns.GetHostName());      
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
      miniGuide.SelectedChannel = Navigator.Channel.Entity;
      miniGuide.DoModal(GetID);

      //Only change the channel if the channel selectd is actually different. 
      //Without this, a ChannelChange might occur even when MiniGuide is canceled. 
      if (!miniGuide.Canceled)
      {
        ViewChannelAndCheck(miniGuide.SelectedChannel, 0);
        UpdateGUIonPlaybackStateChange();
      }

      benchClock.Stop();
      this.LogDebug("TVHome.OnSelecChannel(): Total Time {0} ms", benchClock.ElapsedMilliseconds.ToString());
    }

    private void TvDelayThread()
    {
      //we have to use a small delay before calling tvfullscreen.                                    
      Thread.Sleep(200);

      // wait for timeshifting to complete
      int waits = 0;
      while (_playbackStopped && waits < 100)
      {
        //this.LogDebug("TVHome.OnPageLoad(): waiting for timeshifting to start");
        Thread.Sleep(100);
        waits++;
      }

      if (!_playbackStopped)
      {
        g_Player.ShowFullScreenWindow();
      }
    }

    private void OnSuspend()
    {
      this.LogDebug("TVHome.OnSuspend()");

      try
      {
        if (Card.IsTimeShifting)
        {
          Card.User.Name = new User().Name;
          Card.StopTimeShifting();
        }
        _notifyManager.Stop();
        UnsubscribeFromEventService();
        UnRegisterCiMenu();
        StopServerMonitor();
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
      this.LogDebug("TVHome.OnResume()");
      try
      {
        Connected = false;
        HandleWakeUpTvServer();
        StartServerMonitor();
        SubscribeToEventService();
        RegisterUserForHeartbeatMonitoring();
        _notifyManager.Start();
        if (_resumeChannel != null)
        {
          this.LogDebug("TVHome.OnResume() - automatically turning on TV: {0}", _resumeChannel.DisplayName);
          AutoTurnOnTv(_resumeChannel);
          AutoFullScreenTv();
          _resumeChannel = null;
        }
        else
        {
          RetrieveChannelStatesFromServer();
        }
      }
      finally
      {
        _suspended = false;
      }
    }

    private void RegisterUserForHeartbeatMonitoring()
    {
#if !DEBUG
      if (_heartbeatEventHandler == null)
      {
        try
        {
          ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForHeartbeatMonitoring(Dns.GetHostName());
          _heartbeatEventHandler = new HeartbeatEventHandler();
          ServiceAgents.Instance.EventServiceAgent.RegisterHeartbeatCallbacks(_heartbeatEventHandler);        
        }
        catch (Exception ex)
        {
          this.LogError(ex, "TvHome.RegisterUserForHeartbeatMonitoring exception");
          
        }        
      }
#endif
    }

    private void UnRegisterUserForHeartbeatMonitoring()
    {
#if !DEBUG
      if (_heartbeatEventHandler != null)
      {
        try
        {
          ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForHeartbeatMonitoring(Dns.GetHostName());
        }
        catch (Exception)
        {            
          //ignore 
        }    

        try
        {
          ServiceAgents.Instance.EventServiceAgent.UnRegisterHeartbeatCallbacks(_heartbeatEventHandler, !Connected);        
        }
        catch (Exception ex)
        {
          this.LogError(ex, "TvHome.UnRegisterUserForHeartbeatMonitoring exception");          
        }        
      }
#endif
    }

    public void Start()
    {
      this.LogDebug("TVHome.Start()");
    }

    public void Stop()
    {
      this.LogDebug("TVHome.Stop()");
    }

    public bool WndProc(ref Message msg)
    {
      if (msg.Msg == WM_POWERBROADCAST)
      {
        switch (msg.WParam.ToInt32())
        {
          case PBT_APMSTANDBY:
            this.LogInfo("TVHome.WndProc(): Windows is going to standby");
            OnSuspend();
            break;
          case PBT_APMSUSPEND:
            this.LogInfo("TVHome.WndProc(): Windows is suspending");
            OnSuspend();
            break;
          case PBT_APMQUERYSUSPEND:
          case PBT_APMQUERYSTANDBY:
            this.LogInfo("TVHome.WndProc(): Windows is going into powerstate (hibernation/standby)");

            break;
          case PBT_APMRESUMESUSPEND:
            this.LogInfo("TVHome.WndProc(): Windows has resumed from hibernate mode");
            OnResume();
            break;
          case PBT_APMRESUMESTANDBY:
            this.LogInfo("TVHome.WndProc(): Windows has resumed from standby mode");
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
        case GUIMessage.MessageType.GUI_MSG_NOTIFY_REC:
          string heading = message.Label;
          string text = message.Label2;
          Channel ch = message.Object as Channel;
          //this.LogDebug("Received rec notify message: {0}, {1}, {2}", heading, text, (ch != null).ToString()); //remove later
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
            if (c.MediaType == (int)MediaTypeEnum.TV)
            {
              strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, c.DisplayName);
            }
            else if (c.MediaType == (int)MediaTypeEnum.Radio)
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

                if (c.MediaType == (int)MediaTypeEnum.TV)
                {
                  MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_TV);
                  TVHome.ViewChannelAndCheck(c, 0);
                  if (TVHome.Card.IsTimeShifting && TVHome.Card.IdChannel == c.IdChannel)
                  {
                    g_Player.ShowFullScreenWindow();
                  }
                }
                else if (c.MediaType == (int)MediaTypeEnum.Radio)
                {
                  MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_RADIO);
                  Radio.Radio.CurrentChannel = c;
                  Radio.Radio.Play();
                }
              }
              catch (Exception e)
              {
                Log.Error(e, "TVHome: TVNotification: Error on starting channel {0} after notification", notify.Channel.DisplayName);
              }

            }
            break;
          }
      }
    }

    private void OnAudioTracksReady()
    {
      this.LogDebug("TVHome.OnAudioTracksReady()");

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
      // when we are watching TV and suddenly decides to watch a audio/video etc., we want to make sure that the TV is stopped on ServiceAgents.Instance.ControllerService.
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

    public static bool ManualRecord(ChannelBLL channel, int dialogId)
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


      IVirtualCard card = null;
      ProgramBLL prog = new ProgramBLL(channel.CurrentProgram);
      bool isRecording;
      bool hasProgram = (channel.CurrentProgram != null);
      if (hasProgram)
      {
        //refresh the states from db
        prog = new ProgramBLL(ServiceAgents.Instance.ProgramServiceAgent.GetProgram(channel.CurrentProgram.IdProgram));
        isRecording = (prog.IsRecording || prog.IsRecordingOncePending);
      }
      else
      {
        isRecording = ServiceAgents.Instance.ControllerServiceAgent.IsRecording(channel.Entity.IdChannel, out card);
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
            pDlgOK.AddLocalizedString(876); //till manual stop
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            switch (pDlgOK.SelectedId)
            {
              case 875:
                //record current program                  
                TVProgramInfo.CreateProgram(prog.Entity, (int)ScheduleRecordingType.Once, dialogId);
                return true;

              case 876:
                //manual
                bool doesManuelScheduleAlreadyExist = DoesManualScheduleAlreadyExist(channel.Entity);
                if (!doesManuelScheduleAlreadyExist)
                {
                  StartRecordingSchedule(channel, true);
                  return true;
                }
                break;
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
          TVProgramInfo.IsRecordingProgram(prog.Entity, out s, false);
          if (s != null)
          {
            idChannel = s.IdChannel;
          }
        }
        else
        {
          s = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(card.RecordingScheduleId);
          idChannel = Card.IdChannel;
        }

        if (s != null && idChannel > 0)
        {
          TVUtil.DeleteRecAndSchedWithPrompt(s);
        }
      }
      return false;
    }

    private static bool DoesManualScheduleAlreadyExist(Channel channel)
    {
      Schedule existingSchedule = ServiceAgents.Instance.ScheduleServiceAgent.GetScheduleWithNoEPG(channel.IdChannel);
      return (existingSchedule != null);
    }

    private void UpdateGUIonPlaybackStateChange(bool playbackStarted)
    {
      if (btnTvOnOff != null && btnTvOnOff.Selected != playbackStarted)
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

      if (btnTvOnOff != null && btnTvOnOff.Selected != isTimeShiftingTV)
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

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowTV
    /// </summary>
    /// <returns></returns>
    private static bool ShowFullScreenWindowTVHandler()
    {
      if ((g_Player.IsTV && Card.IsTimeShifting) || g_Player.IsTVRecording)
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

      IList<Recording> activeRecordings =
        ServiceAgents.Instance.RecordingServiceAgent.ListAllActiveRecordingsByMediaType(MediaTypeEnum.TV).ToList();

      if (activeRecordings != null && activeRecordings.Count > 0)
      {
        foreach (Recording activeRecording in activeRecordings)
        {
          if (!ignoreActiveRecordings.Contains(activeRecording))
          {
            GUIListItem item = new GUIListItem();
            string channelName = activeRecording.Channel.DisplayName;
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

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }

        if (dlg.SelectedLabel < 0 || (dlg.SelectedLabel - 1 > activeRecordings.Count))
        {
          return;
        }

        Recording selectedRecording = activeRecordings[dlg.SelectedLabel];
        Schedule parentSchedule = selectedRecording.Schedule;
        if (parentSchedule == null || parentSchedule.IdSchedule < 1)
        {
          return;
        }
        bool deleted = TVUtil.StopRecAndSchedWithPrompt(parentSchedule, selectedRecording.IdChannel.GetValueOrDefault());
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
          pDlgOK.DoModal(GetID);
        }
      }
    }


    private void OnActiveStreams()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(692); // Active Tv Streams
        int selected = 0;

        IList<StreamPresentation> streamingChannels = ServiceAgents.Instance.ControllerServiceAgent.ListAllStreamingChannels();
        int count = 0;
        foreach (StreamPresentation streaming in streamingChannels)
        {
          GUIListItem item = new GUIListItem();

          Channel ch = streaming.Channel;
          IUser user = streaming.User;
          bool isParked = streaming.IsParked;
          bool isRecording = streaming.IsRecording;
          int idChannel = ch.IdChannel;
          item.Label = ch.DisplayName;

          if (isParked)
          {
            item.Label += " [parked @ " + streaming.ParkedAt.ToShortTimeString() + "]";
          }

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
          if (Card != null && Card.IdChannel == idChannel)
          {
            selected = count;
          }
          count++;
        }

        if (streamingChannels.Count == 0)
        {
          GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          if (pDlgOK != null)
          {
            pDlgOK.SetHeading(692); //my tv
            pDlgOK.SetLine(1, GUILocalizeStrings.Get(1511)); // No Active streams
            pDlgOK.SetLine(2, "");
            pDlgOK.DoModal(GetID);
          }
        }
        else
        {
          dlg.SelectedLabel = selected;
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel >= 0)
          {
            StreamPresentation streamingChannel = streamingChannels[dlg.SelectedLabel];
            if (streamingChannel.IsParked)
            {
              bool unPark = ShowUnParkDialogue(streamingChannel.User.Name);
              if (unPark)
              {
                ViewParkedChannel(streamingChannel);
                return;
              }
              if (Card != null && streamingChannel.User.Name.Equals(Card.User.Name))
              {
                ViewParkedChannelAndCheck(streamingChannel.Channel, streamingChannel.ParkedDuration, streamingChannel.User.CardId, true);
                return;
              }
            }
            ViewChannel(streamingChannel.Channel);
          }
        }
      }
    }


    private static bool ShowUnParkDialogue(string userName)
    {
      var dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (dlgYesNo != null)
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(2555, new[] { userName }));
        //"Unpark channel by [" + streamingChannel.User.Name + "] ?"
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(2556)); //"Unpark and resume channel?"
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(2557)); //"If not you will watch TV from live point."
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
        return dlgYesNo.IsConfirmed;
      }
      return false;
    }

    private static void ViewParkedChannel(StreamPresentation streamingChannel)
    {
      ViewParkedChannelAndCheck(streamingChannel.Channel, streamingChannel.ParkedDuration, streamingChannel.User.CardId, false);
      UpdateProgressPercentageBar();
    }

    public static bool ViewParkedChannelAndCheck(Channel channel, double parkedDuration, int newCardId, bool watchFromLivePoint)
    {
      if (!Connected)
      {
        return false;
      }

      _status.Clear();

      _doingChannelChange = false;

      try
      {
        bool doContinue;
        bool checkResult = PreTuneChecks(channel, out doContinue);
        if (doContinue == false)
          return checkResult;

        _doingChannelChange = true;
        IUser user = new User();

        if ((g_Player.Playing && g_Player.IsTimeShifting && !g_Player.Stopped) && (g_Player.IsTV || g_Player.IsRadio))
        {
          _status.Set(LiveTvStatus.WasPlaying);
        }

        //Start timeshifting the new tv channel

        IVirtualCard card;

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

        bool succeeded = ServiceAgents.Instance.ControllerServiceAgent.UnParkTimeShifting(user.Name,
                                                                                       parkedDuration,
                                                                                       channel.IdChannel,
                                                                                       out user, out card);

        if (!succeeded)
        {
          //timeshifting new channel failed. 
          g_Player.Stop();

          // ensure right channel name, even if not watchable:Navigator.Channel = channel; 
          ChannelTuneFailedNotifyUser(TvResult.UnknownError, _status.IsSet(LiveTvStatus.WasPlaying), channel);

          _doingChannelChange = true; // keep fullscreen false;
          return true; // "success"
        }

        if (_status.IsSet(LiveTvStatus.WasPlaying))
        {
          if (card != null)
            g_Player.OnZapping((int)card.Type);
          else
            g_Player.OnZapping(-1);
        }

        _status.Set(LiveTvStatus.CardChange);
        if (card != null)
        {
          RegisterCiMenu(card.Id);
        }
        _status.Reset(LiveTvStatus.WasPlaying);

        // Update channel navigator
        if (Navigator.Channel.Entity != null &&
            (channel.IdChannel != Navigator.Channel.Entity.IdChannel || (Navigator.LastViewedChannel == null)))
        {
          Navigator.LastViewedChannel = Navigator.Channel.Entity;
        }
        Log.Info("succeeded:{0} {1}", succeeded, card);
        Card = card; //Moved by joboehl - Only touch the card if starttimeshifting succeeded. 

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

        if (watchFromLivePoint)
        {
          g_Player.SeekAbsolute(g_Player.Duration - 1);
          SeekToEnd(true);
        }
        else
        {
          if (parkedDuration > 0)
          {
            Seek(true, parkedDuration);
          }
          else
          {
            Seek(true, 0);
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
        Log.Error(ex, "TvPlugin:ViewParkedChannelAndCheck Exception");
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
      if (Navigator.Channel != null && Navigator.Channel.Entity != null && Card != null)
      {
        string label;

        IVirtualCard vc;
        if (ServiceAgents.Instance.ControllerServiceAgent.IsRecording(Navigator.Channel.Entity.IdChannel, out vc))
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
      //this.LogDebug("updaterec: conn:{0}, rec:{1}", Connected, Card.IsRecording);
      // if we're recording tv, update gui with info
      if (Connected && Card.IsRecording)
      {
        int scheduleId = Card.RecordingScheduleId;
        if (scheduleId > 0)
        {
          Schedule schedule = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(scheduleId);
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
      if (Navigator.Channel != null && Navigator.Channel.Entity != null && !g_Player.IsRadio)
      {
        ChannelBLL infoChannel;
        if (Navigator.Channel.Entity.MediaType == (int)MediaTypeEnum.TV)
        {
          infoChannel = Navigator.Channel;
        }
        else
        {
          infoChannel = new ChannelBLL(_lastTvChannel);
        }
        UpdateCurrentEpgProperties(infoChannel);
        UpdateNextEpgProperties(infoChannel);
        //Update lastTvChannel with current
        _lastTvChannel = infoChannel.Entity;
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
        GUIPropertyManager.SetProperty("#TV.View.episode", rec.EpisodeNum);
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

    private static void UpdateCurrentEpgProperties(ChannelBLL ch)
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
          GUIPropertyManager.SetProperty("#TV.View.channel", ch.Entity.DisplayName);
          SetTvThumbProperty(ch.Entity);
        }
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.View.channel", ch.Entity.DisplayName);
        GUIPropertyManager.SetProperty("#TV.View.title", current.Title);
        GUIPropertyManager.SetProperty("#TV.View.compositetitle", TVUtil.GetDisplayTitle(current));
        GUIPropertyManager.SetProperty("#TV.View.start",
                                       current.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.View.stop",
                                       current.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.View.description", current.Description);
        GUIPropertyManager.SetProperty("#TV.View.subtitle", current.EpisodeName);
        GUIPropertyManager.SetProperty("#TV.View.episode", current.EpisodeNum);
        GUIPropertyManager.SetProperty("#TV.View.genre", TVUtil.GetCategory(current.ProgramCategory));
        GUIPropertyManager.SetProperty("#TV.View.remaining",
                                       Utils.SecondsToHMSString(current.EndTime - current.StartTime));
        SetTvThumbProperty(ch.Entity);

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
    }

    private static void UpdateNextEpgProperties(ChannelBLL ch)
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
        GUIPropertyManager.SetProperty("#TV.Next.episode", next.EpisodeNum);
        GUIPropertyManager.SetProperty("#TV.Next.genre", TVUtil.GetCategory(next.ProgramCategory));
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
    /// Choose the preferred audio stream based on audio-related preferences.
    /// </summary>
    /// <remarks>
    /// This method has unit tests. Any changes require the unit tests to be re-run and/or updated.
    /// </remarks>
    public static int GetPreferedAudioStreamIndex(out eAudioDualMonoMode dualMonoMode)
    {
      int currentPreferredIndex = -1; // Not set.
      bool isCurrentPreferredAc3 = false;
      int currentPreferredLangPriority = AUDIO_STREAM_PRIORITY_NOT_DEFINED;  // The lower the priority, the more it is preferred.
      string currentPreferredLang = "";
      eAudioDualMonoMode currentPreferredDualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      bool haveLangPreferences = (_preferredLanguages != null && _preferredLanguages.Count > 0);
      bool isDualMonoModeEnabled = (g_Player.GetAudioDualMonoMode() != eAudioDualMonoMode.UNSUPPORTED);

      Log.Debug("TvHome: GetPreferedAudioStreamIndex(), preferred languages = {0}, prefer AC3 = {1}, prefer audio type over language = {2}, dual mono switching enabled = {3}",
                  (haveLangPreferences ? String.Join(";", _preferredLanguages.ToArray()) : "N/A"), _preferAC3, _preferAudioTypeOverLang, isDualMonoModeEnabled);

      for (int i = 0; i < g_Player.AudioStreams; i++)
      {
        bool isAc3 = false;
        string lang = "";
        int langPriority = AUDIO_STREAM_PRIORITY_NOT_DEFINED;
        eAudioDualMonoMode dmMode = eAudioDualMonoMode.UNSUPPORTED;

        // Is the stream an AC3 stream, or not?
        string streamType = g_Player.AudioType(i);
        if (streamType.Equals("AC3") || streamType.Equals("AC3plus"))
        {
          isAc3 = true;
        }
        // else: "Mpeg1", "Mpeg2", "AAC", "LATMAAC"

        // Determine the stream language.
        lang = g_Player.AudioLanguage(i);
        string[] langParts = lang.Split('(');
        if (langParts.Length > 1)
        {
          lang = langParts[1].Substring(0, langParts[1].Length - 1);
        }

        // Handle dual-mono streams by choosing the preferred of the two channels
        // by language (channel encoding format should be the same).
        if (isDualMonoModeEnabled && lang.Length == 6)
        {
          // Use left channel by default.
          dmMode = eAudioDualMonoMode.LEFT_MONO;
          string rightChannelLang = lang.Substring(3, 3);
          lang = lang.Substring(0, 3);
          langPriority = _preferredLanguages.IndexOf(lang);
          int rightChannelLangPriority = _preferredLanguages.IndexOf(rightChannelLang);

          // Do we actually prefer the right channel?
          if ((langPriority == -1 && rightChannelLangPriority >= 0) || (rightChannelLangPriority >= 0 && rightChannelLangPriority < langPriority))
          {
            dmMode = eAudioDualMonoMode.RIGHT_MONO;
            lang = rightChannelLang;
            langPriority = rightChannelLangPriority;
          }
        }
        else
        {
          langPriority = _preferredLanguages.IndexOf(lang);
        }
        if (langPriority == -1)
        {
          langPriority = AUDIO_STREAM_PRIORITY_NOT_DEFINED;
        }

        // Do we prefer this stream over the previously seen streams?
        bool isPreferred = false;
        if (currentPreferredIndex == -1)
        {
          isPreferred = true;
        }
        // Choice by language...
        else if (langPriority < currentPreferredLangPriority && (!_preferAudioTypeOverLang || !_preferAC3 || isAc3 || !isCurrentPreferredAc3))
        {
          Log.Debug("TvHome: choosing stream {0} (is AC3 {1}, lang = {2}, lang priority = {3}) over {4} (is AC3 = {5}, lang = {6}, lang priority = {7})", i, isAc3, lang, langPriority, currentPreferredIndex, isCurrentPreferredAc3, currentPreferredLang, currentPreferredLangPriority);
          isPreferred = true;
        }
        // Choice by AC3...
        else if (isAc3 && _preferAC3 && !isCurrentPreferredAc3 && (currentPreferredLangPriority == AUDIO_STREAM_PRIORITY_NOT_DEFINED || langPriority <= currentPreferredLangPriority || (_preferAudioTypeOverLang && langPriority != AUDIO_STREAM_PRIORITY_NOT_DEFINED)))
        {
          Log.Debug("TvHome: choosing AC3 stream {0} (lang = {1}, lang priority = {2}) over {3} (is AC3 = {4}, lang = {5}, lang priority = {6})", i, lang, langPriority, currentPreferredIndex, isCurrentPreferredAc3, currentPreferredLang, currentPreferredLangPriority);
          isPreferred = true;
        }
        if (isPreferred)
        {
          currentPreferredIndex = i;
          isCurrentPreferredAc3 = isAc3;
          currentPreferredLangPriority = langPriority;
          currentPreferredLang = lang;
          currentPreferredDualMonoMode = dmMode;
        }
      }

      dualMonoMode = currentPreferredDualMonoMode;
      return currentPreferredIndex;
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
          // this error can also happen if we have no connection to the ServiceAgents.Instance.ControllerService.
          if (!Connected)
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
          // this error can also happen if we have no connection to the ServiceAgents.Instance.ControllerService.
          if (!Connected)
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
        //MediaPortal.GUI.Library.this.LogDebug("TvHome.OnBlackImageRendered()");
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

      //BAV: fixing mantis bug 1263: TV starts with no video if Radio is previously ON & channel selected from TV guide
      if ((channel.MediaType != (int)MediaTypeEnum.Radio && g_Player.IsRadio) || (channel.MediaType == (int)MediaTypeEnum.Radio && !g_Player.IsRadio))
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
      else if ((channel.MediaType == (int)MediaTypeEnum.TV && g_Player.IsRadio) || (channel.MediaType == (int)MediaTypeEnum.Radio && g_Player.IsTV) || g_Player.IsCDA ||
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
    /// <param name="kickCardId"> </param>
    /// <returns></returns>
    public static bool ViewChannelAndCheck(Channel channel, int? kickCardId)
    {
      if (!Connected)
      {
        return false;
      }
      double? parkedDuration = null;
      TvResult succeeded = TvResult.UnknownError;
      Dictionary<int, List<IUser>> kickableCards = null;
      _status.Clear();
      _doingChannelChange = false;

      try
      {
        bool doContinue;
        bool checkResult = PreTuneChecks(channel, out doContinue);
        if (doContinue == false)
          return checkResult;

        _doingChannelChange = true;


        IUser user = new User();

        if ((g_Player.Playing && g_Player.IsTimeShifting && !g_Player.Stopped) && (g_Player.IsTV || g_Player.IsRadio))
        {
          _status.Set(LiveTvStatus.WasPlaying);
        }

        //Start timeshifting the new tv channel

        IVirtualCard card;
        int newCardId = -1;

        // check which card will be used
        newCardId = ServiceAgents.Instance.ControllerServiceAgent.TimeShiftingWouldUseCard(user.Name, channel.IdChannel);

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
        bool cardChanged;
        succeeded = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(user.Name, channel.IdChannel, kickCardId, out card, out kickableCards, out cardChanged, out parkedDuration, out user);

        if (_status.IsSet(LiveTvStatus.WasPlaying))
        {
          if (card != null)
            g_Player.OnZapping((int)card.Type);
          else
            g_Player.OnZapping(-1);
        }


        if (succeeded != TvResult.Succeeded && succeeded != TvResult.UsersBlocking && succeeded != TvResult.AlreadyParked)
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
        if (Navigator.Channel.Entity != null &&
            (channel.IdChannel != Navigator.Channel.Entity.IdChannel || (Navigator.LastViewedChannel == null)))
        {
          Navigator.LastViewedChannel = Navigator.Channel.Entity;
        }
        Log.Info("succeeded:{0} {1}", succeeded, card);
        if (card != null)
        {
          Card = card; //Moved by joboehl - Only touch the card if starttimeshifting succeeded. 
        }

        // if needed seek to end
        if (_status.IsSet(LiveTvStatus.SeekToEnd))
        {
          SeekToEnd(true);
        }


        // CardChange refactor it, so it reacts to changes in timeshifting path / URL.

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
        Log.Error(ex, "TvPlugin:ViewChannelandCheckV2 Exception");
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

        if (succeeded == TvResult.UsersBlocking)
        {
          int cardId = ShowKickUserDialogue(kickableCards);
          if (cardId > 0)
          {
            ViewChannelAndCheck(channel, cardId);
          }
        }
        else if (succeeded == TvResult.AlreadyParked)
        {
          bool dlgYesNo = ShowUnParkDialogue(Card.User.Name);
          bool watchFromLivePoint = !dlgYesNo;
          ViewParkedChannelAndCheck(channel, parkedDuration.GetValueOrDefault(0), Card.User.CardId, watchFromLivePoint);
        }
        else
        {
          FireOnChannelChangedEvent();
          Navigator.UpdateCurrentChannel();
        }
      }
    }

    private static int ShowKickUserDialogue(Dictionary<int, List<IUser>> kickableCards)
    {
      var dlgExt = (GUIDialogMenuExtended)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU_EXTENDED);

      dlgExt.Reset();
      dlgExt.SetHeading(GUILocalizeStrings.Get(2558)); //"Kick user(s) from card?"
      IList<GUIListItem> list = new List<GUIListItem>();

      foreach (KeyValuePair<int, List<IUser>> cardKvp in kickableCards)
      {
        int idcard = cardKvp.Key;
        List<IUser> users = cardKvp.Value;

        string card = GUILocalizeStrings.Get(2559);
        var subitems = new List<GUIListItem>();
        var cardItem = new GUIListItem(card + idcard.ToString());

        foreach (IUser activeUser in users)
        {
          foreach (ISubChannel subchannel in activeUser.SubChannels.Values)
          {
            if (subchannel.TvUsage == TvUsage.Timeshifting && subchannel.Id == Card.IdChannel && activeUser.Name.Equals(Card.User.Name))
            {
              continue;
            }

            Channel channel = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(subchannel.IdChannel);
            var displayName = channel.DisplayName;
            string subItemString = activeUser.Name + " - " + displayName;

            if (subchannel.TvUsage == TvUsage.Parked)
            {
              subItemString += " [PARKED]";
            }

            var subItem = new GUIListItem(subItemString) { IconImage = Utils.GetCoverArt(Thumbs.TVChannel, displayName) };
            subItem.IconImageBig = subItem.IconImage;
            subitems.Add(subItem);
          }
        }
        dlgExt.Add(cardItem, subitems);
      }

      dlgExt.DoModal(GUIWindowManager.ActiveWindowEx);

      int id = dlgExt.SelectedLabel;
      int cardId = -1;
      if (id > -1)
      {
        cardId = kickableCards.Keys.ToList()[id];
      }

      return cardId;
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
      ViewChannelAndCheck(channel, 0);
      UpdateProgressPercentageBar();
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

    public static Dictionary<int, ChannelState> TvChannelStatesList
    {
      get
      {
        lock (_channelStatesLock)
        {
          //return copy, as it is threadsafe.
          if (_tvChannelStatesList != null)
          {
            return new Dictionary<int, ChannelState>(_tvChannelStatesList);
          }

          return new Dictionary<int, ChannelState>();
        }
      }
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

      MediaTypeEnum? mediaTypeCard = Card.MediaType;
      if (!mediaTypeCard.HasValue)
      {
        Log.Info("tvhome:startplay channel=null");
        return;
      }
      g_Player.MediaType mediaType = g_Player.MediaType.TV;
      if (mediaTypeCard.Value == MediaTypeEnum.Radio)
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

      if (!g_Player.Play(timeshiftFileName, mediaType))
      {
        StopPlayback();
      }

      benchClock.Stop();
      Log.Warn("tvhome:startplay.  Phase 2 - {0} ms - Done starting g_Player.Play()",
               benchClock.ElapsedMilliseconds.ToString());
      benchClock.Reset();
      //benchClock.Start();
      //SeekToEnd(true);
      //this.LogWarn("tvhome:startplay.  Phase 3 - {0} ms - Done seeking.", benchClock.ElapsedMilliseconds.ToString());
      //SeekToEnd(true);

      benchClock.Stop();
    }

    private static void SeekToEnd(bool zapping)
    {
      double duration = g_Player.Duration;
      Seek(zapping, duration);
    }

    private static void Seek(bool zapping, double duration)
    {
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
          Log.Error(e, "tvhome:SeektoEnd({0}, rtsp={1}", zapping, useRtsp);
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
      if (_ciMenuEventEventHandler == null)
      {
        Log.Debug("CiMenu: PrepareCiMenu");
        _ciMenuEventEventHandler = new CiMenuEventEventHandler();
      }
      // Check if card supports CI menu
      if (newCardId != -1 && ServiceAgents.Instance.ControllerServiceAgent.CiMenuSupported(newCardId))
      {
        // opens remoting and attach local eventhandler to server event, call only once        
        ServiceAgents.Instance.EventServiceAgent.RegisterCiMenuCallbacks(_ciMenuEventEventHandler);

        // Enable CI menu handling in card
        ServiceAgents.Instance.ControllerServiceAgent.SetCiMenuHandler(newCardId, null);
        Log.Debug("TvPlugin: CiMenuHandler attached to new card {0}", newCardId);
      }
    }

    public static void UnRegisterCiMenu()
    {
      if (_ciMenuEventEventHandler != null)
      {
        ServiceAgents.Instance.EventServiceAgent.UnRegisterCiMenuCallbacks(_ciMenuEventEventHandler, !Connected);
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
      //keyboard.title = title;
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
        case CiMenuState.Ready:
          dlgCiMenu.Reset();
          dlgCiMenu.SetHeading(currentCiMenu.Title, currentCiMenu.Subtitle, currentCiMenu.BottomText); // CI Menu

          for (int i = 0; i < currentCiMenu.NumChoices; i++) // CI Menu Entries
            dlgCiMenu.Add(currentCiMenu.MenuEntries[i].Message); // take only message, numbers come from dialog

          // show dialog and wait for result       
          dlgCiMenu.DoModal(GUIWindowManager.ActiveWindow);
          if (currentCiMenu.State != CiMenuState.Error)
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
        case CiMenuState.Error:
        case CiMenuState.NoChoices:

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
        case CiMenuState.Request:
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

    #region Implementation of ITvServerEvent



    #endregion

    #region Implementation of ITvServerEventEventCallback

    public void CallbackTvServerEvent(TvServerEventArgs eventArgs)
    {
      switch (eventArgs.EventType)
      {
        case TvServerEventType.ChannelStatesChanged:
          OnChannelStatesChanged(eventArgs.User.ChannelStates);
          break;

        case TvServerEventType.ForcefullyStoppedTimeShifting:
          OnTimeShiftingForcefullyStopped(eventArgs.User.Name, eventArgs.User.TvStoppedReason);
          break;

        case TvServerEventType.RecordingEnded:
          if (eventArgs.Recording == Card.IdChannel)
          {
            Card.IsRecording = false;
          }
          OnRecordingEnded(eventArgs.Recording);
          break;

        case TvServerEventType.RecordingStarted:
          if (eventArgs.Recording == Card.IdChannel)
          {
            Card.IsRecording = true;
          }
          OnRecordingStarted(eventArgs.Recording);
          break;

        case TvServerEventType.EpgGrabbingStarted:
          if (eventArgs.Card.Id == Card.Id)
          {
            Card.IsGrabbingEpg = true;
          }
          break;
        case TvServerEventType.EpgGrabbingStopped:
          if (eventArgs.Card.Id == Card.Id)
          {
            Card.IsGrabbingEpg = false;
          }
          break;

        case TvServerEventType.ScanningStarted:
          if (eventArgs.Card.Id == Card.Id)
          {
            Card.IsScanning = true;
          }
          break;
        case TvServerEventType.ScanningStopped:
          if (eventArgs.Card.Id == Card.Id)
          {
            Card.IsScanning = false;
          }
          break;
      }

      //todo : gibman, maybe handle scrambled state as well ?
      //TVHome.Card.IsScrambled

    }

    #endregion
  }
}