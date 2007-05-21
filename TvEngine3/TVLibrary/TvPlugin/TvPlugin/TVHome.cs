#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using AMS.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;


using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  /// <summary>v
  /// Summary description for Class1.
  /// </summary>
  public class TVHome : GUIWindow, ISetupForm, IShowPlugin
  {

    #region variables
    enum Controls
    {
      IMG_REC_CHANNEL = 21,
      LABEL_REC_INFO = 22,
      IMG_REC_RECTANGLE = 23,

    };

    static ChannelNavigator m_navigator;
    static TVUtil _util;
    static VirtualCard _card = null;
    DateTime _updateTimer = DateTime.Now;
    static DateTime _updateProgressTimer = DateTime.MinValue;
    bool _autoTurnOnTv = false;
    static bool _autoswitchTVon = false;
    int _lagtolerance = 10; //Added by joboehl
    bool _settingsLoaded = false;
    DateTime _dtlastTime = DateTime.Now;
    TvCropManager _cropManager = new TvCropManager();
    TvNotifyManager _notifyManager = new TvNotifyManager();
    static string _preferredLanguages = "";
    static bool _preferAC3 = false;
    Stopwatch benchClock = null;

    [SkinControlAttribute(2)]     protected GUIButtonControl btnTvGuide = null;
    [SkinControlAttribute(3)]     protected GUIButtonControl btnRecord = null;
    // [SkinControlAttribute(6)]     protected GUIButtonControl btnGroup = null;
    [SkinControlAttribute(7)]     protected GUIButtonControl btnChannel = null;
    [SkinControlAttribute(8)]     protected GUIToggleButtonControl btnTvOnOff = null;
    [SkinControlAttribute(13)]    protected GUIButtonControl btnTeletext = null;
//    [SkinControlAttribute(14)]    protected GUIButtonControl btnTuningDetails = null;
    [SkinControlAttribute(24)]    protected GUIImage imgRecordingIcon = null;
    [SkinControlAttribute(99)]    protected GUIVideoControl videoWindow = null;
    [SkinControlAttribute(9)]     protected GUIButtonControl btnActiveStreams = null;
    static bool _connected = false;

    static protected TvServer _server;
    #endregion

    public TVHome()
    {
      //ServiceProvider services = GlobalServiceProvider.Instance;


      MediaPortal.GUI.Library.Log.Info("TVHome:ctor");
      try
      {
        m_navigator = new ChannelNavigator();
      }
      catch (Exception ex)
      {
        MediaPortal.GUI.Library.Log.Error(ex);
      }
      GetID = (int)GUIWindow.Window.WINDOW_TV;
      Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
    }

    void Application_ApplicationExit(object sender, EventArgs e)
    {
      try
      {
        if (TVHome.Card.IsTimeShifting)
        {
          if (!TVHome.Card.IsRecording)
          {
            TVHome.Card.User.Name = new User().Name;
            TVHome.Card.StopTimeShifting();
          }
        }
      }
      catch (Exception)
      {
      }
    }

    public override void OnAdded()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:OnAdded");

      // replace g_player's ShowFullScreenWindowTV
      g_Player.ShowFullScreenWindowTV = ShowFullScreenWindowTVHandler;

      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV, this);
      Restore();
      PreInit();
      ResetAllControls();
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }
    static public TVUtil Util
    {
      get
      {
        if (_util == null) _util = new TVUtil();
        return _util;
      }
    }
    static public TvServer TvServer
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
    static public bool Connected
    {
      get
      {
        return _connected;
      }
      set
      {
        _connected = value;
      }
    }
    static public VirtualCard Card
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
            if (value.Id == _card.Id || value.Id == -1) stop = false;
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

    #region Serialisation
    void LoadSettings()
    {
      if (_settingsLoaded) return;
      _settingsLoaded = true;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        m_navigator.LoadSettings(xmlreader);
        _autoswitchTVon = xmlreader.GetValueAsBool("mytv", "autoswitchTVon", false); //Added by joboehl
        _autoTurnOnTv = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);

        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
        _preferredLanguages = xmlreader.GetValueAsString("tvservice", "preferredlanguages", "");
        _preferAC3 = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
      }
    }

    void SaveSettings()
    {
      if (m_navigator != null)
      {
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          m_navigator.SaveSettings(xmlwriter);
        }
      }
    }
    #endregion

    #region overrides
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
      get
      {
        return false;
      }
    }
    public override bool Init()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:Init");
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvhomeServer.xml");
      GetID = (int)GUIWindow.Window.WINDOW_TV;

      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);
      GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
      return bResult;
    }

    static public void OnGlobalMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO:
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel channel = layer.GetChannelByName(message.Label);
            if (channel != null)
              TVHome.ViewChannelAndCheck(channel);
            break;
          }
        case GUIMessage.MessageType.GUI_MSG_STOP_SERVER_TIMESHIFTING:
          {
            TVHome.Card.StopTimeShifting();
            break;
          }
      }
    }

    void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.TV) return;
      GUIWindow currentWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (currentWindow.IsTv) return;
      if (TVHome.Card.IsTimeShifting == false) return;
      if (TVHome.Card.IsRecording == true) return;
      TVHome.Card.User.Name = new User().Name;
      TVHome.Card.StopTimeShifting();
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_RECORD:
          //record current program on current channel
          //are we watching tv?
          if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            MediaPortal.GUI.Library.Log.Info("send message to fullscreen tv");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORD, GUIWindowManager.ActiveWindow, 0, 0, 0, 0, null);
            msg.SendToTargetWindow = true;
            msg.TargetWindowId = (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN;
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

          MediaPortal.GUI.Library.Log.Info("TVHome:Record action");
          TvServer server = new TvServer();
          string channel = TVHome.Card.ChannelName;
          VirtualCard card;
          if (false == server.IsRecording(channel, out card))
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Program prog = null;
            if (Navigator.Channel != null)
              prog = Navigator.Channel.CurrentProgram;
            if (prog != null)
            {
              GUIDialogMenuBottomRight pDlgOK = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
              if (pDlgOK != null)
              {
                pDlgOK.Reset();
                pDlgOK.SetHeading(605);//my tv
                pDlgOK.AddLocalizedString(875); //current program
                pDlgOK.AddLocalizedString(876); //till manual stop
                pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
                switch (pDlgOK.SelectedId)
                {
                  case 875:
                    {
                      //record current program
                      Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, Navigator.Channel.CurrentProgram.Title,
                                                  Navigator.Channel.CurrentProgram.StartTime, Navigator.Channel.CurrentProgram.EndTime);
                      newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                      newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);

                      newSchedule.RecommendedCard = TVHome.Card.Id;

                      newSchedule.Persist();
                      server.OnNewSchedule();
                    }
                    break;

                  case 876:
                    {
                      Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")",
                                                  DateTime.Now, DateTime.Now.AddDays(1));
                      newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                      newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                      newSchedule.RecommendedCard = TVHome.Card.Id;

                      newSchedule.Persist();
                      server.OnNewSchedule();
                    }
                    break;
                }
              }
            }//if (prog != null)
            else
            {
              Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")",
                                                  DateTime.Now, DateTime.Now.AddDays(1));
              newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
              newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
              newSchedule.RecommendedCard = TVHome.Card.Id;

              newSchedule.Persist();
              server.OnNewSchedule();

            }
          }//if (false == server.IsRecording(channel, out Card))
          else
          {
            server.StopRecordingSchedule(card.RecordingScheduleId);
          }
          break;

        case Action.ActionType.ACTION_PREV_CHANNEL:
          TVHome.OnPreviousChannel();
          break;
        case Action.ActionType.ACTION_PAGE_DOWN:
          TVHome.OnPreviousChannel();
          break;

        case Action.ActionType.ACTION_NEXT_CHANNEL:
          TVHome.OnNextChannel();
          break;
        case Action.ActionType.ACTION_PAGE_UP:
          TVHome.OnNextChannel();
          break;

        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL:  // mPod
          TVHome.OnLastViewedChannel();
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
              OnLastViewedChannel();
          }
          break;
      }
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      btnActiveStreams.Label = GUILocalizeStrings.Get(692);
      try
      {
        int cards = RemoteControl.Instance.Cards;
      }
      catch (Exception)
      {
        RemoteControl.Clear();
      }

      if (!RemoteControl.IsConnected)
      {
        RemoteControl.Clear();
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SETTINGS_TVENGINE);
        return;
      }

      try
      {
        IList cards = TvDatabase.Card.ListAll(); ;
      }
      catch (Exception)
      {
        RemoteControl.Clear();
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SETTINGS_TVENGINE);
        return;
      }
      LoadSettings();
      //stop the old recorder.
      //DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.DataSourceOnly;
      GUIMessage msgStopRecorder = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgStopRecorder);
      if (m_navigator == null)
      {
        m_navigator = new ChannelNavigator();			// Create the channel navigator (it will load groups and channels)
      }
      base.OnPageLoad();

      //set video window position
      if (videoWindow != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width, videoWindow.Height);
      }

      // start viewing tv... 
      GUIGraphicsContext.IsFullScreenVideo = false;
      Channel channel = Navigator.Channel;
      if (channel == null)
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
        if (TVHome.Card.IsTimeShifting)
        {
          int id = TVHome.Card.IdChannel;
          if (id >= 0)
          {
            channel = Channel.Retrieve(id);
          }
        }
        MediaPortal.GUI.Library.Log.Info("tv home init:{0}", channel.Name);
        if (_autoTurnOnTv)
        {
          ViewChannelAndCheck(channel);
        }
        GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.Groups[0].GroupName);
        MediaPortal.GUI.Library.Log.Info("tv home init:{0} done", channel.Name);
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

    static public void OnSelectGroup()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
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
        return;

      Navigator.SetCurrentGroup(dlg.SelectedLabelText);
      GUIPropertyManager.SetProperty("#TV.Guide.Group", dlg.SelectedLabelText);

      if (Navigator.CurrentGroup != null)
      {
        if (Navigator.CurrentGroup.ReferringGroupMap().Count > 0)
        {
          GroupMap gm = (GroupMap)Navigator.CurrentGroup.ReferringGroupMap()[0];
        }
      }
    }

    void OnSelectChannel()
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();

      TvMiniGuide miniGuide = (TvMiniGuide)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MINI_GUIDE);
      miniGuide.AutoZap = false;
      miniGuide.SelectedChannel = Navigator.Channel;
      miniGuide.DoModal(GetID);

      //Only change the channel if the channel selectd is actually different. 
      //Without this, a ChannelChange might occur even when MiniGuide is canceled. 
      if (!miniGuide.Canceled)
      {
        ViewChannelAndCheck(miniGuide.SelectedChannel);
      }

      benchClock.Stop();
      Log.Debug("TVHome.OnSelecChannel(): Total Time {0} ms", benchClock.ElapsedMilliseconds.ToString());


    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();

      if (control == btnActiveStreams)
      {
        OnActiveStreams();
      }
      if (control == btnTvOnOff)
      {
        if (TVHome.Card.IsTimeShifting && g_Player.IsTV && g_Player.Playing)
        {
          //tv off
          Log.Info("TVHome:turn tv off");
          SaveSettings();
          g_Player.Stop();
          TVHome.Card.User.Name = new User().Name;
          TVHome.Card.StopTimeShifting();
          benchClock.Stop();
          Log.Debug("TVHome.OnClicked(): EndTvOff {0} ms", benchClock.ElapsedMilliseconds.ToString());

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

              Log.Debug("TVHome.OnClicked: Stop Called - {0} ms", benchClock.ElapsedMilliseconds.ToString());

              g_Player.Stop();
            }
          }
          SaveSettings();
        }

        // turn tv on/off
        ViewChannelAndCheck(Navigator.Channel);
        UpdateStateOfButtons();
        UpdateProgressPercentageBar();
        benchClock.Stop();
        Log.Debug("TVHome.OnClicked(): Total Time - {0} ms", benchClock.ElapsedMilliseconds.ToString());

      }
      /*
      if (control == btnGroup)
      {
        OnSelectGroup();
      }*/
      if (control == btnTeletext)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
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
        case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
          {
            LoadSettings();
            if (_autoTurnOnTv)
            {
              //restart viewing...  
              MediaPortal.GUI.Library.Log.Info("tv home msg resume tv:{0}", Navigator.CurrentChannel);
              ViewChannel(Navigator.Channel);
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
          MediaPortal.GUI.Library.Log.Info("tv home msg view chan:{0}", message.Label);
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel ch = layer.GetChannelByName(message.Label);
            ViewChannel(ch);
            Navigator.UpdateCurrentChannel();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
          {
            MediaPortal.GUI.Library.Log.Info("tv home msg stop chan:{0}", message.Label);
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel ch = layer.GetChannelByName(message.Label);
            ViewChannel(ch);
            Navigator.UpdateCurrentChannel();
          }
          break;
      }
      return base.OnMessage(message);
    }

    public override void Process()
    {
      if (!TVHome.Connected) return;
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (GUIGraphicsContext.InVmr9Render)
        return;
      if (ts.TotalMilliseconds < 1000)
        return;
      UpdateRecordingIndicator();
      if (btnChannel.Disabled != false)
        btnChannel.Disabled = false;
      //if (btnGroup.Disabled != false)
      //btnGroup.Disabled = false;
      if (btnRecord.Disabled != false)
        btnRecord.Disabled = false;
      //btnTeletext.Visible = false;
      bool isTimeShifting = TVHome.Card.IsTimeShifting;
      if (btnTvOnOff.Selected != ( g_Player.Playing && g_Player.IsTV && isTimeShifting ))
      {
          btnTvOnOff.Selected = (g_Player.Playing && g_Player.IsTV && isTimeShifting);
      }
      if (g_Player.Playing == false)
      {
        UpdateProgressPercentageBar();
        //if (btnTuningDetails!=null)
        //  btnTuningDetails.Visible = false;
        if (btnTeletext.Visible)
          btnTeletext.Visible = false;
        return;
      }
      //else
      //  if (btnTuningDetails!=null)
      //    btnTuningDetails.Visible = true;
      if (btnChannel.Disabled != false)
        btnChannel.Disabled = false;
      //if (btnGroup.Disabled != false)
      // btnGroup.Disabled = false;
      if (btnRecord.Disabled != false)
        btnRecord.Disabled = false;

      bool hasTeletext = TVHome.Card.HasTeletext;
      if (btnTeletext.IsVisible != hasTeletext)
      {
        btnTeletext.IsVisible = hasTeletext;
      }
      // Let the navigator zap channel if needed
      Navigator.CheckChannelChange();
      // Update navigator with information from the Recorder
      // TODO: This should ideally be handled using events. Recorder should fire an event
      // when the current channel changes. This is a temporary workaround //Vic
      string currchan = Navigator.CurrentChannel;		// Remember current channel
      Navigator.UpdateCurrentChannel();
      bool channelchanged = currchan != Navigator.CurrentChannel;

      UpdateStateOfButtons();
      UpdateProgressPercentageBar();
      UpdateRecordingIndicator();
      GUIControl.HideControl(GetID, (int)Controls.LABEL_REC_INFO);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_RECTANGLE);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_CHANNEL);
      _updateTimer = DateTime.Now;
    }

    #endregion

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowTV
    /// </summary>
    /// <returns></returns>
    private static bool ShowFullScreenWindowTVHandler()
    {
      if (!g_Player.Playing && Card.IsTimeShifting)
      {
        // watching TV
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          return true;
        Log.Info("TVHome: ShowFullScreenWindow switching to fullscreen tv");
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
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
      Log.Debug("TvFullScreen: IsSingleSeat - RemoteControl.HostName = {0} / Environment.MachineName = {1}", RemoteControl.HostName, Environment.MachineName);
      return (RemoteControl.HostName.ToLowerInvariant() == Environment.MachineName.ToLowerInvariant());
    }

    public static void UpdateTimeShift()
    {
    }

    void OnActiveStreams()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(692); // Active Tv Streams
      int selected = 0;

      IList cards = TvDatabase.Card.ListAll();
      List<Channel> channels = new List<Channel>();
      int count = 0;
      TvServer server = new TvServer();
      List<User> _users = new List<User>();
      foreach (Card card in cards)
      {
        if (card.Enabled == false) continue;
        User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
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
            item.Label = ch.Name;
            item.Label2 = user.Name;
            string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, ch.Name);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            item.IconImage = strLogo;
            if (isRecording)
              item.PinImage = Thumbs.TvRecordingIcon;
            else
              item.PinImage = "";
            dlg.Add(item);
            _users.Add(user);
            if (TVHome.Card != null && TVHome.Card.IdChannel == idChannel)
            {
              selected = count;
            }
            count++;
          }
        }
      }
      if (channels.Count == 0)
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (pDlgOK != null)
        {
          pDlgOK.SetHeading(692);//my tv
          pDlgOK.SetLine(1, "No active streams");
          pDlgOK.SetLine(2, "");
          pDlgOK.DoModal(this.GetID);
        }
        return;
      }
      dlg.SelectedLabel = selected;
      dlg.DoModal(this.GetID);
      if (dlg.SelectedLabel < 0) return;
      TVHome.Card = new VirtualCard(_users[dlg.SelectedLabel], RemoteControl.HostName);
      if (TVHome.Card.IsRecording && !TVHome.Card.IsTimeShifting)
      {
        string fileName = TVHome.Card.RecordingFileName;
        g_Player.Stop();
        if (System.IO.File.Exists(fileName))
        {
          g_Player.Play(fileName, g_Player.MediaType.Recording);
          g_Player.SeekAbsolute(g_Player.Duration);
          g_Player.ShowFullScreenWindow();
        }
        else
        {
          string url = server.GetRtspUrlForFile(fileName);
          Log.Info("recording url:{0}", url);
          if (url.Length > 0)
          {
            g_Player.Play(url, g_Player.MediaType.Recording);

            if (g_Player.Playing)
            {
              g_Player.SeekAbsolute(g_Player.Duration);
              g_Player.SeekAbsolute(g_Player.Duration);
              g_Player.ShowFullScreenWindow();
            }
          }
        }
      }
      else
      {
        g_Player.Stop();
        StartPlay();
      }
      TVHome.Card.User.Name = new User().Name;
    }

    void OnRecord()
    {
      //record now.
      //Are we recording this channel already?
      TvBusinessLayer layer = new TvBusinessLayer();
      TvServer server = new TvServer();
      VirtualCard card;
      if (false == server.IsRecording(Navigator.Channel.Name, out card))
      {
        //no then start recording
        Program prog = Navigator.Channel.CurrentProgram;
        if (prog != null)
        {
          GUIDialogMenuBottomRight pDlgOK = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
          if (pDlgOK != null)
          {
            pDlgOK.SetHeading(605);//my tv
            pDlgOK.AddLocalizedString(875); //current program
            pDlgOK.AddLocalizedString(876); //till manual stop
            pDlgOK.DoModal(this.GetID);
            switch (pDlgOK.SelectedId)
            {
              case 875:
                {
                  Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, Navigator.Channel.CurrentProgram.Title,
                            Navigator.Channel.CurrentProgram.StartTime, Navigator.Channel.CurrentProgram.EndTime);
                  newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                  newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                  newSchedule.RecommendedCard = TVHome.Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

                  newSchedule.Persist();
                  server.OnNewSchedule();
                }
                break;

              case 876:
                {
                  Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")",
                                              DateTime.Now, DateTime.Now.AddDays(1));
                  newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                  newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                  newSchedule.RecommendedCard = TVHome.Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

                  newSchedule.Persist();
                  server.OnNewSchedule();
                }
                break;
            }
          }
        }
        else
        {
          //manual record
          Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")",
                                      DateTime.Now, DateTime.Now.AddDays(1));
          newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
          newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
          newSchedule.RecommendedCard = TVHome.Card.Id;

          newSchedule.Persist();
          server.OnNewSchedule();
        }
      }
      else
      {
        server.StopRecordingSchedule(card.RecordingScheduleId);

      }
      UpdateStateOfButtons();
    }

    /// <summary>
    /// Update the state of the following buttons
    /// - tv on/off
    /// - timeshifting on/off
    /// - record now
    /// </summary>
    void UpdateStateOfButtons()
    {
      bool isTimeShifting = TVHome.Card.IsTimeShifting;
      if (btnTvOnOff.Selected != (g_Player.Playing && g_Player.IsTV && isTimeShifting ))
      {
          btnTvOnOff.Selected = (g_Player.Playing && g_Player.IsTV && isTimeShifting );
      }
      bool hasTeletext = TVHome.Card.HasTeletext;
      if (btnTeletext.IsVisible != hasTeletext)
      {
        btnTeletext.IsVisible = hasTeletext;
      }
      //are we recording a tv program?
      VirtualCard card;
      if (Navigator.Channel != null && TVHome.Card != null)
      {
        TvServer server = new TvServer();
        string label;
        if (server.IsRecording(Navigator.Channel.Name, out card))
        {
          //yes then disable the timeshifting on/off buttons
          //and change the Record Now button into Stop Record
          label = GUILocalizeStrings.Get(629);//stop record
        }
        else
        {
          //nop. then change the Record Now button
          //to Record Now
          label = GUILocalizeStrings.Get(601);// record
        }
        if (label != btnRecord.Label)
        {
          btnRecord.Label = label;
        }
      }
    }

    void UpdateRecordingIndicator()
    {
      // if we're recording tv, update gui with info
      if (TVHome.Card.IsRecording)
      {
        //int card;
        int scheduleId = TVHome.Card.RecordingScheduleId;
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
        imgRecordingIcon.IsVisible = false;
      }
    }

    /// <summary>
    /// Update the the progressbar in the GUI which shows
    /// how much of the current tv program has elapsed
    /// </summary>
    static public void UpdateProgressPercentageBar()
    {

      TimeSpan ts = DateTime.Now - _updateProgressTimer;
      if (ts.TotalMilliseconds < 1000) return;
      _updateProgressTimer = DateTime.MinValue;

      //make sure no garbage is left in the tvhome area. Ideally, every stop method should do it. 
      GUIPropertyManager.SetProperty("#TV.View.channel", "");
      GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.title", String.Empty);
      GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);

      if (g_Player.Playing && g_Player.IsTimeShifting)
      {
        if (TVHome.Card != null)
        {
          if (TVHome.Card.IsTimeShifting == false)
          {
            g_Player.Stop();
          }
        }
      }
      if (g_Player.IsTVRecording)
      {
        GUIPropertyManager.SetProperty("#TV.View.channel", "Recorded");
        GUIPropertyManager.SetProperty("#TV.View.title", g_Player.currentTitle);
        GUIPropertyManager.SetProperty("#TV.View.description", g_Player.currentDescription);
        return;
      }

      if (Navigator.Channel == null) return;
      try
      {
        if (Navigator.CurrentChannel == null)
        {
          GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
          GUIPropertyManager.SetProperty("#TV.View.channel", "");

          GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.remaining", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.genre", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.title", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Next.start", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Next.stop", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Next.genre", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Next.title", String.Empty);
          GUIPropertyManager.SetProperty("#TV.Next.description", String.Empty);
          return;
        }
        GUIPropertyManager.SetProperty("#TV.View.channel", Navigator.CurrentChannel);
        GUIPropertyManager.SetProperty("#TV.View.title", Navigator.CurrentChannel);
        Program current = Navigator.Channel.CurrentProgram;
        if (current != null)
        {
          GUIPropertyManager.SetProperty("#TV.View.start", current.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.View.stop", current.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.View.remaining", Utils.SecondsToHMSString(current.EndTime - current.StartTime));
          GUIPropertyManager.SetProperty("#TV.View.genre", current.Genre);
          GUIPropertyManager.SetProperty("#TV.View.title", current.Title);
          GUIPropertyManager.SetProperty("#TV.View.description", current.Description);
        }
        Program next = Navigator.Channel.NextProgram;
        if (next != null)
        {
          GUIPropertyManager.SetProperty("#TV.Next.start", next.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.Next.stop", next.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.Next.remaining", Utils.SecondsToHMSString(next.EndTime - next.StartTime));
          GUIPropertyManager.SetProperty("#TV.Next.genre", next.Genre);
          GUIPropertyManager.SetProperty("#TV.Next.title", next.Title);
          GUIPropertyManager.SetProperty("#TV.Next.description", next.Description);
        }
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, Navigator.CurrentChannel);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);

        //get current tv program
        Program prog = Navigator.Channel.CurrentProgram;
        if (prog == null)
        {
          GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
          GUIPropertyManager.SetProperty("#TV.Record.percent1", "0");
          GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
          GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");
          return;
        }
        ts = prog.EndTime - prog.StartTime;
        if (ts.TotalSeconds <= 0)
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
        if (timeShiftStartPoint < 0) timeShiftStartPoint = 0;

        //calculate where we the current playing point is
        double playingPoint = g_Player.Duration - g_Player.CurrentPosition;
        playingPoint = (livePoint - playingPoint);

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
        MediaPortal.GUI.Library.Log.Info("grrrr:{0}", ex.Source, ex.StackTrace);
      }
    }

    /// <summary>
    /// When called this method will switch to the previous TV channel
    /// </summary>
    static public void OnPreviousChannel()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:OnPreviousChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
        if (TVWindow != null) TVWindow.ZapPreviousChannel();
        return;
      }

      // Zap to previous channel immediately
      Navigator.ZapToPreviousChannel(false);
    }

    static private int GetPreferedAudioStreamIndex(string langCodes, bool preferAC3)
    {
      int idx = -1;
      int idxNonAC3 = -1;
      int idxLastAC3 = -1;
      IAudioStream[] streams = TVHome.Card.AvailableAudioStreams;
      for (int i = 0; i < streams.Length; i++)
      {
        if ((preferAC3) && (streams[i].StreamType == AudioStreamType.AC3))
          idxLastAC3 = i;
        if (langCodes.Contains(streams[i].Language))
        {
          if (!preferAC3)
          {
            idx = i;
            break;
          }
          else
          {
            if ((streams[i].StreamType == AudioStreamType.AC3))
            {
              idx = i;
              break;
            }
            else
              idxNonAC3 = i;
          }
        }
      }
      Log.Info("Preferred audio stream: idx={0}, idxNonAC3={1}, idxLastAC3={2}", idx, idxNonAC3, idxLastAC3);
      if ((idx == -1) && (idxNonAC3 != -1))
        idx = idxNonAC3;
      else
        if (idxLastAC3 != -1)
          idx = idxLastAC3;
      if (idx == -1)
        idx = 0;
      Log.Info("Preferred audio stream: switching to audio stream {0}", idx);
      return idx;
    }

    static public bool ViewChannelAndCheck(Channel channel)
    {
      try
      {
        Stopwatch benchClock = null;
        benchClock = Stopwatch.StartNew();
        if (channel == null)
        {
          MediaPortal.GUI.Library.Log.Info("TVHome.ViewChannelAndCheck(): channel==null");
          return false;
        }
        MediaPortal.GUI.Library.Log.Info("TVHome.ViewChannelAndCheck(): View channel={0}", channel.Name);
        if (g_Player.Playing && !_autoswitchTVon) //- Changed by joboehl - Enable TV to autoturnon on channel selection. 
        {
          if (g_Player.IsTVRecording) return true;
          if (g_Player.IsVideo) return true;
          //if (g_Player.IsTV) return true;
          if (g_Player.IsDVD) return true;
          if ((g_Player.IsMusic && g_Player.HasVideo)) return true;
        }
        if (Navigator.Channel != null)
        {
          if (channel.IdChannel != Navigator.Channel.IdChannel)
            Navigator.LastViewedChannel = Navigator.Channel;
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("Navigator.Channel==null");
        }
        string errorMessage;
        TvResult succeeded;
        if (TVHome.Card != null)
        {
          //if (g_Player.Playing )
          if (g_Player.Playing && g_Player.IsTV) //modified by joboehl. Avoids other video being played instead of TV. 
            //if we're already watching this channel, then simply return
            if (TVHome.Card.IsTimeShifting == true && TVHome.Card.IdChannel == channel.IdChannel)
            {
              return true;
            }
        }
        /*
        //check if we are currently watching a tv channel with AC3
        IAudioStream[] audioStreams;
        bool hadAc3 = false;
        if (g_Player.Playing && TVHome.Card != null)
        {
            audioStreams = TVHome.Card.AvailableAudioStreams;
            foreach (IAudioStream stream in audioStreams)
            {
              if (stream.StreamType == AudioStreamType.AC3)
                  {
                      hadAc3 = true;
                  }
            }
        }  
        */
        benchClock.Stop();
        Log.Debug("TVHome.ViewChannelAndCheck(): Fase 1 - {0} ms", benchClock.ElapsedMilliseconds.ToString());
        benchClock.Reset();
        benchClock.Start();

        GUIWaitCursor.Show();
        bool wasPlaying = g_Player.Playing && g_Player.IsTimeShifting && g_Player.IsTV;

        //Start timeshifting the new tv channel
        TvServer server = new TvServer();
        VirtualCard card;
        bool _return = false;

        User user = new User();
        /*
        TvBusinessLayer layer = new TvBusinessLayer();
        int newCardId = -1;
        foreach (Card c in layer.Cards)
        {
            foreach (ChannelMap map in c.ReferringChannelMap())
            {
                if (map.IdChannel == channel.IdChannel)
                {
                   newCardId = c.IdCard;
                    break;
                }
            }
            if (newCardId > -1) break;
        }

        // by gibman - comment from rtv: if there's a timing issue which causes no AvailableAudioStreams then please
        //                               add a callback and wait for that (suggested by tourettes)
        if (newCardId != _card.Id && newCardId > -1)
        {
            Log.Debug("TvPlugin: Stop player. newCard {0} <> _CardID {1} and newCard is Valid" , newCardId, _card.Id);
            g_Player.Stop();
        } 
        Log.Debug("TvPlugin: Continuing player. NewCard is {0} and oldCard was {1}", newCardId, _card.Id); */

        benchClock.Stop();
        MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Fase 2 - {0} ms", benchClock.ElapsedMilliseconds.ToString());
        benchClock.Reset();
        benchClock.Start();

        succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);

        benchClock.Stop();
        MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Fase 3 - {0} ms", benchClock.ElapsedMilliseconds.ToString());
        benchClock.Reset();
        benchClock.Start();

        if (succeeded == TvResult.Succeeded)
        {
          //timeshifting succeeded
          /*
          //check if we the new tvchannel has AC3
          audioStreams = TVHome.Card.AvailableAudioStreams;
          bool hasAc3 = false;
          foreach (IAudioStream stream in audioStreams)
          {
            if (stream.StreamType == AudioStreamType.AC3)
            {
              hasAc3 = true;
            }
          }
          // when we switch from mpeg-2 <-> ac3, then recreate the playing graph
          if (hadAc3 != hasAc3)
          {
            Log.Info("stop player: ac3 changed:{0}-{1}", hadAc3, hasAc3);
            g_Player.Stop();
          }
        
          //Removed code since it's functions were replaced by previous check
          bool useRtsp = System.IO.File.Exists("usertsp.txt");
          if (g_Player.Playing)
          {
            if (System.IO.File.Exists(TVHome.Card.TimeShiftFileName) && !useRtsp)
            {
              if (g_Player.CurrentFile != TVHome.Card.TimeShiftFileName)
              {
                Log.Info("stop player: file changed:{0}-{1}", g_Player.CurrentFile, TVHome.Card.TimeShiftFileName);
                g_Player.Stop();
              }
              Log.Debug("TvPlugin: Continuing player. File not changed :{0}-{1}", g_Player.CurrentFile, TVHome.Card.TimeShiftFileName);
            }
            else
            {
              if (g_Player.CurrentFile != TVHome.Card.RTSPUrl)
              {
                Log.Info("stop player: url changed:{0}-{1}", g_Player.CurrentFile, TVHome.Card.RTSPUrl);
                g_Player.Stop();
              }
              Log.Debug("TvPlugin: Continuing player. URL  not changed :{0}-{1}", g_Player.CurrentFile, TVHome.Card.RTSPUrl);
            }
          }*/

          //Added by joboehl - If any major related to the timeshifting changed during the start, restart the player. 
            if (TVHome.Card.Id != card.Id || TVHome.Card.RTSPUrl != card.RTSPUrl || TVHome.Card.TimeShiftFileName != Card.TimeShiftFileName)
            {
                if (wasPlaying)
                {
                    MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. CardId:{0}/{1}, RTSP:{2}/{3}", TVHome.Card.Id ,card.Id ,TVHome.Card.RTSPUrl, card.RTSPUrl);
                    MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. Timeshifting:{0}/{1}", TVHome.Card.TimeShiftFileName,Card.TimeShiftFileName);
                    g_Player.Stop();
                }
            }

          TVHome.Card = card; //Moved by joboehl - Only touch the card if starttimeshifting succeeded. 

          MediaPortal.GUI.Library.Log.Info("succeeded:{0} ", succeeded);


          benchClock.Stop();
          MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Fase 4 - {0} ms", benchClock.ElapsedMilliseconds.ToString());
          benchClock.Reset();
          benchClock.Start();



          if (!g_Player.Playing)
            StartPlay();

          else if (wasPlaying)
            SeekToEnd(true);

          benchClock.Stop();
          MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Fase 5- {0} ms", benchClock.ElapsedMilliseconds.ToString());
          benchClock.Reset();
          benchClock.Start();

          int prefLangId = GetPreferedAudioStreamIndex(_preferredLanguages, _preferAC3);
          if (IsSingleSeat())
            g_Player.CurrentAudioStream = prefLangId;
          else
          {
            IAudioStream[] streams = TVHome.Card.AvailableAudioStreams;
            TVHome.Card.AudioStream = streams[prefLangId];
          }

          GUIWaitCursor.Hide();

          benchClock.Stop();
          MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Fase 6 - {0} ms", benchClock.ElapsedMilliseconds.ToString());
          benchClock.Reset();
          benchClock.Start();

          return true;

        }
        else
        {
          //timeshifting new channel failed. 
          benchClock.Stop();
          MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Fase 4 failed - {0} ms", benchClock.ElapsedMilliseconds.ToString());
          benchClock.Reset();
          benchClock.Start();

          if (g_Player.Duration == 0) //Added by joboehl - Only stops if stream is empty. Otherwise, a message is displaying but everything stays as before.  
            g_Player.Stop();
        }

        GUIWaitCursor.Hide();
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (pDlgOK != null)
        {
          errorMessage = GUILocalizeStrings.Get(1500);
          switch (succeeded)
          {
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
              errorMessage += "\r" + GUILocalizeStrings.Get(1506) + "\r";
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
            default:
              errorMessage += "\r" + GUILocalizeStrings.Get(1506) + "\r";
              break;
          }
          string[] lines = errorMessage.Split('\r');
          pDlgOK.SetHeading(605);//my tv
          pDlgOK.SetLine(1, lines[0]);
          if (lines.Length > 1)
            pDlgOK.SetLine(2, lines[1]);
          else
            pDlgOK.SetLine(2, "");

          if (lines.Length > 2)
            pDlgOK.SetLine(3, lines[2]);
          else
            pDlgOK.SetLine(3, "");
          pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Debug("TvPlugin:ViewChannelandCheck Exception {0}", ex.ToString());
        GUIWaitCursor.Hide();
        return false;
      }
    }

    static public void ViewChannel(Channel channel)
    {
      ViewChannelAndCheck(channel);
      return;
    }

    /// <summary>
    /// When called this method will switch to the next TV channel
    /// </summary>
    static public void OnNextChannel()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:OnNextChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
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
    static public void OnLastViewedChannel()
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
    static public bool IsTVWindow(int windowId)
    {
      if (windowId == (int)GUIWindow.Window.WINDOW_TV) return true;

      return false;
    }


    /// <summary>
    /// Gets the channel navigator that can be used for channel zapping.
    /// </summary>
    static public ChannelNavigator Navigator
    {
      get { return m_navigator; }
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "My TV";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return (int)GUIWindow.Window.WINDOW_TV;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      // TODO:  Add TVHome.GetHome implementation
      strButtonText = GUILocalizeStrings.Get(605);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Watch, record and timeshift analog and digital TV with MediaPortal";
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      TvSetupForm setup = new TvSetupForm();
      setup.ShowDialog();
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    static void StartPlay()
    {
      if (TVHome.Card == null)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay card=null");
        return;
      }
      if (TVHome.Card.IsScrambled)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay scrambled");
        return;
      }
      MediaPortal.GUI.Library.Log.Info("tvhome:startplay");
      string timeshiftFileName = TVHome.Card.TimeShiftFileName;
      MediaPortal.GUI.Library.Log.Info("tvhome:file:{0}", timeshiftFileName);

      IChannel channel = TVHome.Card.Channel;
      if (channel == null)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay channel=null");
        return;
      }
      g_Player.MediaType mediaType = g_Player.MediaType.TV;
      if (channel.IsRadio)
        mediaType = g_Player.MediaType.Radio;

      bool useRtsp = System.IO.File.Exists("usertsp.txt");
      if (System.IO.File.Exists(timeshiftFileName) && !useRtsp)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay:{0}", timeshiftFileName);
        g_Player.Play(timeshiftFileName, mediaType);
        SeekToEnd(false);
      }
      else
      {
        timeshiftFileName = TVHome.Card.RTSPUrl;
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay:{0}", timeshiftFileName);
        g_Player.Play(timeshiftFileName, mediaType);
        SeekToEnd(true);
      }
    }
    static void SeekToEnd(bool zapping)
    {
      string timeshiftFileName = TVHome.Card.TimeShiftFileName;
      bool useRtsp = System.IO.File.Exists("usertsp.txt");
      if (System.IO.File.Exists(timeshiftFileName) && !useRtsp)
      {
        if (g_Player.IsRadio == false)
        {
          double duration = g_Player.Duration;
          double position = g_Player.CurrentPosition;
          /*
          if (duration > 0 && position > 0)
          {
            if (Math.Abs(duration - position) >= 10)
            {
              MediaPortal.GUI.Library.Log.Info("tvhome:seektoend dur:{0} pos:{1}", g_Player.Duration, g_Player.CurrentPosition);
              g_Player.SeekAbsolute(duration);
              MediaPortal.GUI.Library.Log.Info("tvhome:seektoend  done dur:{0} pos:{1}", g_Player.Duration, g_Player.CurrentPosition);
            }
          }*/
          g_Player.SeekAbsolute(duration + 10);
        }
      }
      else
      {
        //streaming....
        if (zapping)
        {
          System.Threading.Thread.Sleep(100);
          double duration = g_Player.Duration;
          double position = g_Player.CurrentPosition;
          if (duration > 0 || position > 0)
            g_Player.SeekAbsolute(duration - 10);
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
    const string ConfigFileXml = @"<?xml version=|1.0| encoding=|utf-8|?> 
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

    private List<ChannelGroup> m_groups = new List<ChannelGroup>(); // Contains all channel groups (including an "all channels" group)
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

      MediaPortal.GUI.Library.Log.Info("ChannelNavigator::ctor()");
      string ipadres = Dns.GetHostName();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        ipadres = xmlreader.GetValueAsString("tvservice", "hostname", "");
        if (ipadres == "" || ipadres == "localhost")
        {
          ipadres = Dns.GetHostName();
          MediaPortal.GUI.Library.Log.Info("Remote control: hostname not specified on mediaportal.xml!");
          xmlreader.SetValue("tvservice", "hostname", ipadres);
          ipadres = "localhost";
          MediaPortal.Profile.Settings.SaveCache();
        }
      }
      RemoteControl.HostName = ipadres;
      MediaPortal.GUI.Library.Log.Info("Remote control:master server :{0}", RemoteControl.HostName);

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
          doc.Load("gentle.config");
          XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
          XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
          XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
          node.InnerText = connectionString;
          nodeProvider.InnerText = provider;
          doc.Save("gentle.config");
        }
        catch (Exception ex)
        {
          Log.Error("Unable to create/modify gentle.config {0},{1}", ex.Message, ex.StackTrace);
        }

        MediaPortal.GUI.Library.Log.Info("ChannelNavigator::Reload()");
        Gentle.Framework.ProviderFactory.ResetGentle(true);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);
        MediaPortal.GUI.Library.Log.Info("get channels from database");
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddConstraint(Operator.Equals, "isTv", 1);
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        MediaPortal.GUI.Library.Log.Info("found:{0} tv channels", channels.Count);
        TvNotifyManager.OnNotifiesChanged();
        m_groups.Clear();

        MediaPortal.GUI.Library.Log.Info("get all groups from database");
        sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
        sb.AddOrderByField(true, "groupName");
        stmt = sb.GetStatement(true);
        IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
        IList allgroupMaps = GroupMap.ListAll();
        bool found = false;
        foreach (ChannelGroup group in groups)
        {
          if (group.GroupName == GUILocalizeStrings.Get(972))
          {
            found = true;
            TvBusinessLayer layer = new TvBusinessLayer();
            foreach (Channel channel in channels)
            {
              if (channel.IsTv == false) continue;
              bool groupContainsChannel = false;
              foreach (GroupMap map in allgroupMaps)
              {
                if (map.IdGroup != group.IdGroup) continue;
                if (map.IdChannel == channel.IdChannel)
                {
                  groupContainsChannel = true;
                  break;
                }
              }
              if (!groupContainsChannel)
              {
                layer.AddChannelToGroup(channel, GUILocalizeStrings.Get(972));

              }
            }
            break;
          }
        }

        if (!found)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          MediaPortal.GUI.Library.Log.Info(" group:{0} not found. create it", GUILocalizeStrings.Get(972));
          foreach (Channel channel in channels)
          {
            layer.AddChannelToGroup(channel, GUILocalizeStrings.Get(972));
          }
          MediaPortal.GUI.Library.Log.Info(" group:{0} created", GUILocalizeStrings.Get(972));
        }

        groups = ChannelGroup.ListAll();
        foreach (ChannelGroup group in groups)
        {
          //group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);
          m_groups.Add(group);
        }

        MediaPortal.GUI.Library.Log.Info("loaded {0} groups", m_groups.Count);
        TVHome.Connected = true;
      }
      catch (Exception ex)
      {
        MediaPortal.GUI.Library.Log.Error(ex);
        TVHome.Connected = false;
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
        if (m_currentChannel == null) return null;
        return m_currentChannel.Name;
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
    /// Gets the currently active channel group.
    /// </summary>
    public ChannelGroup CurrentGroup
    {
      get { return (ChannelGroup)m_groups[m_currentgroup]; }
    }

    /// <summary>
    /// Gets the list of channel groups.
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
          return m_currentChannel;
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
          return CurrentGroup.GroupName;
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
      if (reentrant) return false;
      if (GUIGraphicsContext.InVmr9Render) return false;
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
            if (CurrentGroup.ReferringGroupMap().Count > 0)
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
          m_zapchannel = null;
          MediaPortal.GUI.Library.Log.Info("Channel change:{0}", zappingTo.Name);
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
      if (TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording)
      {
        id = TVHome.Card.IdChannel;
        if (id >= 0)
          newChannel = Channel.Retrieve(id);
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
        newChannel = m_currentChannel;
      if (m_currentChannel != newChannel && newChannel != null)
      {
        m_currentChannel = newChannel;
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelName">The channel to switch to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(Channel channel, bool useZapDelay)
    {
      m_zapchannel = channel;

      if (useZapDelay)
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      else
        m_zaptime = DateTime.Now;
    }

    /// <summary>
    /// Changes the current channel (based on channel number) after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannelNumber(int channelNr, bool useZapDelay)
    {
      IList channels = CurrentGroup.ReferringGroupMap();
      if (channelNr >= 0)
      {
        bool found = false;
        int ChannelCnt = 0;
        Channel chan;
        while (found == false && ChannelCnt < channels.Count)
        {
          GroupMap gm = (GroupMap)channels[channelNr];
          chan = (Channel)gm.ReferencedChannel();
          if (chan.SortOrder == channelNr)
          {
            ZapToChannel(chan, useZapDelay);
            found = true;
          }
          else
            ChannelCnt++;
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
      IList channels = CurrentGroup.ReferringGroupMap();
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        GroupMap gm = (GroupMap)channels[channelNr];
        Channel chan = gm.ReferencedChannel();
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
      // Step to next channel
      currindex++;
      if (currindex >= CurrentGroup.ReferringGroupMap().Count)
        currindex = 0;
      GroupMap gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
      Channel chan = (Channel)gm.ReferencedChannel();
      m_zapchannel = chan;

      MediaPortal.GUI.Library.Log.Info("Navigator:ZapNext {0}->{1}", currentChan.Name, m_zapchannel.Name);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        else
          m_zaptime = DateTime.Now;
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
      // Step to previous channel
      currindex--;
      if (currindex < 0)
        currindex = CurrentGroup.ReferringGroupMap().Count - 1;


      GroupMap gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
      Channel chan = (Channel)gm.ReferencedChannel();
      m_zapchannel = chan;

      MediaPortal.GUI.Library.Log.Info("Navigator:ZapPrevious {0}->{1}", currentChan.Name, m_zapchannel.Name);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        else
          m_zaptime = DateTime.Now;
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
        m_zapgroup = m_currentgroup + 1;
      else
        m_zapgroup = m_zapgroup + 1;			// Zap from last zap group

      if (m_zapgroup >= m_groups.Count)
        m_zapgroup = 0;

      if (useZapDelay)
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      else
        m_zaptime = DateTime.Now;
    }

    /// <summary>
    /// Changes to the previous channel group.
    /// </summary>
    public void ZapToPreviousGroup(bool useZapDelay)
    {
      if (m_zapgroup == -1)
        m_zapgroup = m_currentgroup - 1;
      else
        m_zapgroup = m_zapgroup - 1;

      if (m_zapgroup < 0)
        m_zapgroup = m_groups.Count - 1;

      if (useZapDelay)
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      else
        m_zaptime = DateTime.Now;
    }

    /// <summary>
    /// Zaps to the last viewed Channel (without ZapDelay).  // mPod
    /// </summary>
    public void ZapToLastViewedChannel()
    {
      if (_lastViewedChannel != null)
      {
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
      IList groupMaps = CurrentGroup.ReferringGroupMap();
      for (int i = 0; i < groupMaps.Count; i++)
      {
        GroupMap gm = (GroupMap)groupMaps[i];
        Channel chan = (Channel)gm.ReferencedChannel();
        if (chan.IdChannel == ch.IdChannel)
          return i;
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
          return i;
      }
      return -1;
    }

    public Channel GetChannel(string channelName)
    {
      foreach (Channel chan in channels)
      {
        if (chan.Name == channelName) return chan;
      }
      return null;
    }

    #endregion

    #region Serialization

    public void LoadSettings(MediaPortal.Profile.Settings xmlreader)
    {
      MediaPortal.GUI.Library.Log.Info("ChannelNavigator::LoadSettings()");
      string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
      m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
      string groupname = xmlreader.GetValueAsString("mytv", "group", GUILocalizeStrings.Get(972));
      m_currentgroup = GetGroupIndex(groupname);
      if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count)		// Group no longer exists?
        m_currentgroup = 0;

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
    }

    public void SaveSettings(MediaPortal.Profile.Settings xmlwriter)
    {
      if (m_currentChannel != null)
      {
        xmlwriter.SetValue("mytv", "channel", m_currentChannel.Name);
      }
      if (CurrentGroup != null)
      {
        if (CurrentGroup.GroupName.Trim() != String.Empty)
          xmlwriter.SetValue("mytv", "group", CurrentGroup.GroupName);
      }
    }

    #endregion
  }

  #endregion
}
