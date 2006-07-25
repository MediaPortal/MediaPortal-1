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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using AMS.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.Utils.Services;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

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
    bool _autoTurnOnTv = false;
    bool _settingsLoaded = false;
    DateTime _dtlastTime = DateTime.Now;

    [SkinControlAttribute(2)]
    protected GUIButtonControl btnTvGuide = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnRecord = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnGroup = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnChannel = null;
    [SkinControlAttribute(8)]
    protected GUIToggleButtonControl btnTvOnOff = null;
    [SkinControlAttribute(13)]
    protected GUIButtonControl btnTeletext = null;
    [SkinControlAttribute(24)]
    protected GUIImage imgRecordingIcon = null;
    [SkinControlAttribute(99)]
    protected GUIVideoControl videoWindow = null;
    [SkinControlAttribute(9)]
    protected GUIToggleButtonControl btnTimeshiftingOnOff = null;
    static protected ILog _log;
    static protected TvServer _server;
    #endregion

    public TVHome()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      _log.Info("TVHome:ctor");
      try
      {
        m_navigator = new ChannelNavigator();
      }
      catch (Exception ex)
      {
        _log.Error(ex);
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
      _log.Info("TVHome:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV, this);
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
    static public VirtualCard Card
    {
      get
      {
        if (_card == null)
        {
          _card = TvServer.Card(-1);
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
            if (value.Id == _card.Id) stop = false;
          }
          if (stop)
          {
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        m_navigator.LoadSettings(xmlreader);
        _autoTurnOnTv = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);

        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
      }
    }

    void SaveSettings()
    {
      if (m_navigator != null)
      {
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
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
      _log.Info("TVHome:Init");
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvhome.xml");
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
          TVHome.ViewChannelAndCheck(message.Label);
          break;
      }
    }

    void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.TV) return;
      GUIWindow currentWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (currentWindow.IsTv) return;
      if (TVHome.Card.IsTimeShifting == false) return;
      if (TVHome.Card.IsRecording == true) return;
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
            _log.Info("send message to fullscreen tv");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORD, GUIWindowManager.ActiveWindow, 0, 0, 0, 0, null);
            msg.SendToTargetWindow = true;
            msg.TargetWindowId = (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN;
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

          _log.Info("TVHome:Record action");
          if (TVHome.Card.IsTimeShifting)
          {
            string channel = TVHome.Card.ChannelName;
            //yes, are we recording this channel already ?
            Program prog = Navigator.GetChannel(channel).CurrentProgram;
            bool isRecording = false;
            for (int i = 0; i < RemoteControl.Instance.Cards; ++i)
            {
              int id = RemoteControl.Instance.CardId(i);
              if (RemoteControl.Instance.IsRecording(id))
              {
                if (RemoteControl.Instance.CurrentChannel(id).Name == channel)
                {
                  isRecording = true;
                  break;
                }
              }
            }

            if (!isRecording)
            {
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
                        Schedule newSchedule = Schedule.Create();
                        newSchedule.Channel = Navigator.Channel;
                        newSchedule.StartTime = Navigator.Channel.CurrentProgram.StartTime;
                        newSchedule.EndTime = Navigator.Channel.CurrentProgram.EndTime;
                        newSchedule.ProgramName = Navigator.Channel.CurrentProgram.Title;
                        DatabaseManager.SaveChanges();
                        RemoteControl.Instance.OnNewSchedule();
                      }
                      break;

                    case 876:
                      {
                        Schedule newSchedule = Schedule.Create();
                        newSchedule.Channel = Navigator.Channel;
                        newSchedule.StartTime = DateTime.Now;
                        newSchedule.EndTime = DateTime.Now.AddDays(1);
                        newSchedule.ProgramName = GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")";
                        DatabaseManager.SaveChanges();
                        RemoteControl.Instance.OnNewSchedule();
                      }
                      break;
                  }
                }
              }
              else
              {
                Schedule newSchedule = Schedule.Create();
                newSchedule.Channel = Navigator.Channel;
                newSchedule.StartTime = DateTime.Now;
                newSchedule.EndTime = DateTime.Now.AddDays(1);
                newSchedule.ProgramName = GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")";
                DatabaseManager.SaveChanges();
                RemoteControl.Instance.OnNewSchedule();
              }
            }
            else
            {
              int id=TVHome.Card.RecordingScheduleId;
              if (id>0)
                TVHome.TvServer.StopRecordingSchedule(id);
            }
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

        case Action.ActionType.ACTION_SHOW_GUI:
          if (g_Player.Playing && g_Player.CurrentFile == TVHome.Card.TimeShiftFileName)
          {
            //if we're watching a tv recording
            _log.Info("switch to fullscreen tv:{0}", (int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
            GUIWindowManager.ActivateWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
          else if (g_Player.Playing && g_Player.HasVideo)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
          break;

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
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (pDlgOK != null)
        {
          pDlgOK.SetHeading(605);//my tv
          pDlgOK.SetLine(1, "The Tv Service is not running");
          pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
          return;
        }
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

      btnTimeshiftingOnOff.Visible = false;

      // start viewing tv... 
      GUIGraphicsContext.IsFullScreenVideo = false;
      string channelName = Navigator.CurrentChannel;
      if (Navigator.CurrentChannel == String.Empty)
      {
        if (Navigator.CurrentGroup == null && Navigator.Groups.Count > 0)
        {
          Navigator.SetCurrentGroup(Navigator.Groups[0].GroupName);
        }
        if (Navigator.CurrentGroup != null)
        {
          if (Navigator.CurrentGroup.GroupMaps.Count > 0)
          {
            channelName = Navigator.CurrentGroup.GroupMaps[0].Channel.Name;
          }
        }
      }

      if (channelName != String.Empty)
      {
        if (TVHome.Card.IsTimeShifting)
        {
          channelName = TVHome.Card.ChannelName;
        }
        _log.Info("tv home init:{0}", channelName);
        if (_autoTurnOnTv || TVHome.Card.IsTimeShifting)
        {
          ViewChannelAndCheck(channelName);
        }
        _log.Info("tv home init:{0} done", channelName);
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

    void OnSelectGroup()
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
      dlg.DoModal(this.GetID);
      if (dlg.SelectedLabel < 0) return;
      Navigator.SetCurrentGroup(dlg.SelectedLabelText);
      if (Navigator.CurrentGroup != null)
      {
        if (Navigator.CurrentGroup.GroupMaps.Count > 0)
        {
          ViewChannelAndCheck(Navigator.CurrentGroup.GroupMaps[0].Channel.Name);
        }
      }
    }

    void OnSelectChannel()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(891); // Select TV channel
      int selected = 0;
      for (int i = 0; i < Navigator.CurrentGroup.GroupMaps.Count; ++i)
      {
        dlg.Add(Navigator.CurrentGroup.GroupMaps[i].Channel.Name);
        if (Navigator.CurrentChannel != null)
        {
          if (Navigator.CurrentGroup.GroupMaps[i].Channel.Name == Navigator.CurrentChannel)
          {
            selected = i;
          }
        }
      }
      dlg.SelectedLabel = selected;
      dlg.DoModal(this.GetID);
      if (dlg.SelectedLabel < 0) return;
      ViewChannelAndCheck(dlg.SelectedLabelText);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnTvOnOff)
      {
        if (TVHome.Card.IsTimeShifting)
        {
          //tv off
          _log.Info("TVHome:turn tv off");
          SaveSettings();
          g_Player.Stop();
          TVHome.Card.StopTimeShifting();
          return;
        }
        else
        {
          // tv on
          _log.Info("TVHome:turn tv on {0}", Navigator.CurrentChannel);

          //stop playing anything
          if (g_Player.Playing)
          {
            if (g_Player.IsTV && !g_Player.IsTVRecording)
            {
              //already playing tv...
            }
            else
            {
              g_Player.Stop();
            }
          }
          SaveSettings();
        }

        // turn tv on/off
        ViewChannelAndCheck(Navigator.CurrentChannel);
        UpdateStateOfButtons();
        UpdateProgressPercentageBar();
      }

      if (control == btnGroup)
      {
        OnSelectGroup();
      }
      if (control == btnTeletext)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
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
        case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
          {
            LoadSettings();
            if (_autoTurnOnTv)
            {
              //restart viewing...  
              _log.Info("tv home msg resume tv:{0}", Navigator.CurrentChannel);
              ViewChannel(Navigator.CurrentChannel);
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
          _log.Info("tv home msg view chan:{0}", message.Label);
          ViewChannel(message.Label);
          Navigator.UpdateCurrentChannel();
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
          _log.Info("tv home msg stop chan:{0}", message.Label);
          ViewChannel(message.Label);
          Navigator.UpdateCurrentChannel();
          break;
      }
      return base.OnMessage(message);
    }

    public override void Process()
    {
      if (!RemoteControl.IsConnected) return;
      TimeSpan ts = DateTime.Now - _updateTimer;

      if (GUIGraphicsContext.InVmr9Render)
        return;
      if (ts.TotalMilliseconds < 1000)
        return;
      UpdateRecordingIndicator();
      btnChannel.Disabled = false;
      btnGroup.Disabled = false;
      btnRecord.Disabled = true;
      btnTeletext.Visible = false;
      btnTvOnOff.Selected = TVHome.Card.IsTimeShifting;

      if (g_Player.Playing == false)
      {
        if (TVHome.Card.IsTimeShifting)
        {
          if (!TVHome.Card.IsScrambled)
          {
            StartPlay();
          }
        }
        return;
      }
        /*
      else
      {
        if (g_Player.IsTV)
        {

          if (TVHome.Card.IsScrambled)
          {
            g_Player.Stop();
            return;
          }
        }
      }*/


      btnChannel.Disabled = false;
      btnGroup.Disabled = false;
      btnRecord.Disabled = false;
      btnTeletext.Visible = TVHome.Card.HasTeletext;
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

    public static void UpdateTimeShift()
    {
    }

    void OnRecord()
    {
      //record now.
      //Are we recording this channel already?
      if (!TVHome.Card.IsRecording)
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
                  Schedule newSchedule = Schedule.Create();
                  newSchedule.Channel = Navigator.Channel;
                  newSchedule.StartTime = Navigator.Channel.CurrentProgram.StartTime;
                  newSchedule.EndTime = Navigator.Channel.CurrentProgram.EndTime;
                  newSchedule.ProgramName = Navigator.Channel.CurrentProgram.Title;
                  DatabaseManager.SaveChanges();
                  RemoteControl.Instance.OnNewSchedule();
                }
                break;

              case 876:
                {
                  Schedule newSchedule = Schedule.Create();
                  newSchedule.Channel = Navigator.Channel;
                  newSchedule.StartTime = DateTime.Now;
                  newSchedule.EndTime = DateTime.Now.AddDays(1);
                  newSchedule.ProgramName = GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")";
                  DatabaseManager.SaveChanges();
                  RemoteControl.Instance.OnNewSchedule();
                }
                break;
            }
          }
        }
        else
        {
          //manual record
          Schedule newSchedule = Schedule.Create();
          newSchedule.Channel = Navigator.Channel;
          newSchedule.StartTime = DateTime.Now;
          newSchedule.EndTime = DateTime.Now.AddDays(1);
          newSchedule.ProgramName = GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.Name + ")";
          DatabaseManager.SaveChanges();
          RemoteControl.Instance.OnNewSchedule();
        }
      }
      else
      {
        if (TVHome.Card.IsRecording)
        {
          //yes then stop recording
          Navigator.UpdateCurrentChannel();


          int id = TVHome.Card.RecordingScheduleId;
          if (id > 0)
            TVHome.TvServer.StopRecordingSchedule(id);

          // and re-start viewing.... 
          _log.Info("tv home stoprecording chan:{0}", Navigator.CurrentChannel);
          ViewChannel(Navigator.CurrentChannel);
          Navigator.UpdateCurrentChannel();
        }
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
      btnTvOnOff.Selected = TVHome.Card.IsTimeShifting;
      btnTeletext.IsVisible = TVHome.Card.HasTeletext;
      //are we recording a tv program?
      if (TVHome.Card.IsRecording)
      {
        //yes then disable the timeshifting on/off buttons
        //and change the Record Now button into Stop Record
        btnRecord.Label = GUILocalizeStrings.Get(629);//stop record
      }
      else
      {
        //nop. then change the Record Now button
        //to Record Now
        btnRecord.Label = GUILocalizeStrings.Get(601);// record

      }
    }

    void UpdateRecordingIndicator()
    {
      // if we're recording tv, update gui with info
      if (TVHome.Card.IsRecording)
      {
        int card;
        int scheduleId = TVHome.Card.RecordingScheduleId;
        if (scheduleId > 0)
        {
          EntityQuery query = new EntityQuery(typeof(Schedule));
          query.AddClause(Schedule.IdScheduleEntityColumn, EntityQueryOp.EQ, scheduleId);

          EntityList<Schedule> schedules = DatabaseManager.Instance.GetEntities<Schedule>(query);
          if (schedules.Count > 0)
          {
            if (schedules[0].ScheduleType == (int)ScheduleRecordingType.Once)
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
    static public void UpdateProgressPercentageBar()
    {
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
        TimeSpan ts = prog.EndTime - prog.StartTime;
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
        int programDuration = (int)ts.TotalSeconds;

        //calculate where the program is at this time
        ts = (DateTime.Now - prog.StartTime);
        int livePoint = (int)ts.TotalSeconds;

        //calculate when timeshifting was started
        double timeShiftStartPoint = 0d;
        // if timeshifting started after the beginning of the program
        if (prog.StartTime < TVHome.Card.TimeShiftStarted)
        {
          ts = TVHome.Card.TimeShiftStarted - prog.StartTime;
          timeShiftStartPoint = ts.TotalSeconds;
        }

        //calculate where we the current playing point is
        double playingPoint = g_Player.CurrentPosition + timeShiftStartPoint;
        if (TVHome.Card.TimeShiftStarted < prog.StartTime)
        {
          ts = prog.StartTime - TVHome.Card.TimeShiftStarted;
          playingPoint -= ts.TotalSeconds;
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
        _log.Info("grrrr:{0}", ex.Source, ex.StackTrace);
      }
    }

    /// <summary>
    /// When called this method will switch to the previous TV channel
    /// </summary>
    static public void OnPreviousChannel()
    {
      _log.Info("TVHome:OnPreviousChannel()");
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

    static public bool ViewChannelAndCheck(string channel)
    {
      if (g_Player.Playing)
      {
        if (g_Player.IsTVRecording) return true;
        if (g_Player.IsVideo) return true;
        if (g_Player.IsDVD) return true;
        if ((g_Player.IsMusic && g_Player.HasVideo)) return true;
      }
      _log.Info("TVHome.ViewChannel(): View channel={0}", channel);

      if (channel != Navigator.CurrentChannel)
        Navigator.LastViewedChannel = Navigator.CurrentChannel;

      string errorMessage;
      if (TVHome.Card.IsTimeShifting == false ||
          TVHome.Card.ChannelName != channel)
      {

        VirtualCard card;
        bool succeeded = RemoteControl.Instance.StartTimeShifting(channel, out card);
        TVHome.Card = card;
        if (TVHome.Card.IsScrambled)
          succeeded = false;
        _log.Info("succeeded:{0} scrambled:{1}", succeeded, TVHome.Card.IsScrambled);
        if (succeeded)
        {
          if (g_Player.Playing && g_Player.CurrentFile != TVHome.Card.TimeShiftFileName)
          {
            g_Player.Stop();
          }
          if (g_Player.Playing) SeekToEnd();
          else StartPlay();
          return true;
        }
        else
        {
          g_Player.Stop();
        }
      }
      else return true;


      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (pDlgOK != null)
      {
        errorMessage = "Unable to start timeshifting";
        if (TVHome.Card.IsScrambled)
          errorMessage += "\rchannel is scrambled\r";
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

    static public void ViewChannel(string channel)
    {
      ViewChannelAndCheck(channel);
      return;
      if (g_Player.Playing)
      {
        if (g_Player.IsTVRecording) return;
        if (g_Player.IsVideo) return;
        if (g_Player.IsDVD) return;
        if ((g_Player.IsMusic && g_Player.HasVideo)) return;
      }
      _log.Info("TVHome.ViewChannel(): View channel={0}", channel);

      if (channel != Navigator.CurrentChannel)
        Navigator.LastViewedChannel = Navigator.CurrentChannel;

      if (TVHome.Card.IsTimeShifting == false || TVHome.Card.ChannelName != channel)
      {

        VirtualCard card;
        bool succeeded = RemoteControl.Instance.StartTimeShifting(channel, out card);
        if (RemoteControl.Instance.IsScrambled(card.Id))
          succeeded = false;
        if (succeeded)
        {
          TVHome.Card = card;
          if (g_Player.Playing && g_Player.CurrentFile != TVHome.Card.TimeShiftFileName)
          {
            g_Player.Stop();
          }
          if (g_Player.Playing) SeekToEnd();
          else StartPlay();
          return;
        }
        else
        {
          g_Player.Stop();
        }
      }
    }

    /// <summary>
    /// When called this method will switch to the next TV channel
    /// </summary>
    static public void OnNextChannel()
    {
      _log.Info("TVHome:OnNextChannel()");
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
      return false;
    }

    public void ShowPlugin()
    {
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
      if (TVHome.Card.IsScrambled) return;
      _log.Info("tvhome:startplay");
      string timeshiftFileName = TVHome.Card.TimeShiftFileName;
      IChannel channel = TVHome.Card.Channel;
      g_Player.MediaType mediaType = g_Player.MediaType.TV;
      if (channel.IsRadio)
        mediaType = g_Player.MediaType.Radio;
      if (System.IO.File.Exists(timeshiftFileName))
      {
        _log.Info("tvhome:startplay:{0}", timeshiftFileName);
        g_Player.Play(timeshiftFileName, mediaType);
      }
      else
      {
        timeshiftFileName = TVHome.Card.RTSPUrl;
        _log.Info("tvhome:startplay:{0}", timeshiftFileName);
        g_Player.Play(timeshiftFileName, mediaType);
      }
    }
    static void SeekToEnd()
    {
      double pos = g_Player.Duration;
      _log.Info("tvhome:seektoend dur:{0} pos:{1}", g_Player.Duration, g_Player.CurrentPosition);
      if (pos >= 0 && g_Player.CurrentPosition < pos-4)
      {
        g_Player.SeekAbsolute(g_Player.Duration );
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
    <logFile>DebugLog.xml</logFile>
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
    private string m_currentchannel = String.Empty;
    private DateTime m_zaptime;
    private long m_zapdelay;
    private string m_zapchannel = null;
    private int m_zapgroup = -1;
    private string _lastViewedChannel = string.Empty; // saves the last viewed Channel  // mPod
    private Channel m_currentChannel = null;
    private EntityList<Channel> channels = new EntityList<Channel>();
    private bool reentrant = false;
    protected ILog _log;
    #endregion

    #region Constructors

    public ChannelNavigator()
    {
      // Load all groups
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      _log.Info("ChannelNavigator::ctor()");
      string ipadres = Dns.GetHostName();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        ipadres = xmlreader.GetValueAsString("tvservice", "hostname", "");
        if (ipadres == "" || ipadres == "localhost")
        {
          ipadres = Dns.GetHostName();
          _log.Info("Remote control: hostname not specified on mediaportal.xml!");
          xmlreader.SetValue("tvservice", "hostname", ipadres);
          ipadres = "localhost";
          MediaPortal.Profile.Settings.SaveCache();
        }
      }
      RemoteControl.HostName = ipadres;
      _log.Info("Remote control:master server :{0}",RemoteControl.HostName);

      ReLoad();
    }
    public void ReLoad()
    {
      try
      {
        _log.Info("ChannelNavigator::Reload()");
        CreateDatabaseConfigFile(RemoteControl.Instance.DatabaseConnectionString);
        _log.Info("get channels from database");
        EntityQuery query = new EntityQuery(typeof(Channel));
        query.AddClause(Channel.IsTvEntityColumn, EntityQueryOp.EQ, 1);
        query.AddOrderBy(Channel.SortOrderEntityColumn);
        channels = DatabaseManager.Instance.GetEntities<Channel>(query);
        _log.Info("found:{0} tv channels", channels.Count);

        m_groups.Clear();
        DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.Normal;
        _log.Info("get all groups from database");
        EntityList<ChannelGroup> groups = DatabaseManager.Instance.GetEntities<ChannelGroup>();
        bool found = false;
        foreach (ChannelGroup group in groups)
        {
          if (group.GroupName == GUILocalizeStrings.Get(972))
          {
            found = true;
            break;
          }
        }
        if (!found)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          _log.Info(" group:{0} not found. create it", GUILocalizeStrings.Get(972));
          foreach (Channel channel in channels)
          {
            layer.AddChannelToGroup(channel, GUILocalizeStrings.Get(972));
          }
          DatabaseManager.SaveChanges();
          _log.Info(" group:{0} created", GUILocalizeStrings.Get(972));
        }

        DatabaseManager.Instance.ClearQueryCache();
        groups = DatabaseManager.Instance.GetEntities<ChannelGroup>();
        foreach (ChannelGroup group in groups)
        {
          group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);
          m_groups.Add(group);
        }

        _log.Info("loaded {0} groups", m_groups.Count);
      }
      catch (Exception ex)
      {
        _log.Error(ex);
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
      get { return m_currentchannel; }
    }
    public Channel Channel
    {
      get { return m_currentChannel; }
    }

    /// <summary>
    /// Gets and sets the last viewed channel
    /// Returns empty string if no zap occurred before
    /// </summary>
    public string LastViewedChannel
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
    public string ZapChannel
    {
      get
      {
        if (m_zapchannel == null)
          return m_currentchannel;
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
      // _log.Info(Log.LogType.Error, "zapnow group:{0} current group:{0}", m_zapgroup, m_currentgroup);
      //if (m_zapchannel == null)
      //   _log.Info(Log.LogType.Error, "zapchannel==null");
      //else
      //   _log.Info(Log.LogType.Error, "zapchannel=={0}",m_zapchannel);
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
            if (CurrentGroup.GroupMaps.Count > 0)
            {
              Channel chan = (Channel)CurrentGroup.GroupMaps[0].Channel;
              m_zapchannel = chan.Name;
            }
          }
          m_zapgroup = -1;

          //if (m_zapchannel != m_currentchannel)
          //  lastViewedChannel = m_currentchannel;
          // Zap to desired channel
          string zappingTo = m_zapchannel;
          m_zapchannel = null;
          _log.Info("Channel change:{0}", zappingTo);
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
    /// Ensures that the navigator has the correct current channel (retrieved from the Recorder).
    /// </summary>
    public void UpdateCurrentChannel()
    {
      string newChannel = String.Empty;
      //if current card is watching tv then use that channel
      if (TVHome.Card.IsTimeShifting)
      {
        newChannel = TVHome.Card.ChannelName;
      }
      else if (TVHome.Card.IsRecording)
      {
        // else if current card is recording, then use that channel
        newChannel = TVHome.Card.ChannelName;
      }
      else
      {
        // else if any card is recording
        // then get & use that channel
        for (int i = 0; i < RemoteControl.Instance.Cards; ++i)
        {
          int id = RemoteControl.Instance.CardId(i);
          if (RemoteControl.Instance.IsRecording(id))
          {
            newChannel = RemoteControl.Instance.CurrentChannel(id).Name;
          }
        }
      }
      if (newChannel == String.Empty)
        newChannel = m_currentchannel;
      if (m_currentchannel != newChannel && newChannel != String.Empty)
      {
        m_currentchannel = newChannel;
        m_currentChannel = GetChannel(m_currentchannel);
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelName">The channel to switch to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(string channelName, bool useZapDelay)
    {
      m_zapchannel = channelName;

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
      ReadOnlyEntityList<GroupMap> channels = CurrentGroup.GroupMaps;
      if (channelNr >= 0)
      {
        bool found = false;
        int ChannelCnt = 0;
        Channel chan;
        while (found == false && ChannelCnt < channels.Count)
        {
          chan = (Channel)channels[ChannelCnt].Channel;
          if (chan.SortOrder == channelNr)
          {
            ZapToChannel(chan.Name, useZapDelay);
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
      ReadOnlyEntityList<GroupMap> channels = CurrentGroup.GroupMaps;
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        Channel chan = channels[channelNr].Channel;
        ZapToChannel(chan.Name, useZapDelay);
      }
    }

    /// <summary>
    /// Changes to the next channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToNextChannel(bool useZapDelay)
    {
      string currentChan = String.Empty;
      int currindex;
      if (m_zapchannel == null)
      {
        currindex = GetChannelIndex(CurrentChannel);
        currentChan = CurrentChannel;
      }
      else
      {
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel
        currentChan = CurrentChannel;
      }
      // Step to next channel
      currindex++;
      if (currindex >= CurrentGroup.GroupMaps.Count)
        currindex = 0;
      Channel chan = (Channel)CurrentGroup.GroupMaps[currindex].Channel;
      m_zapchannel = chan.Name;

      _log.Info("Navigator:ZapNext {0}->{1}", currentChan, m_zapchannel);
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
      string currentChan = String.Empty;
      int currindex;
      if (m_zapchannel == null)
      {
        currentChan = CurrentChannel;
        currindex = GetChannelIndex(CurrentChannel);
      }
      else
      {
        currentChan = m_zapchannel;
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel
      }
      // Step to previous channel
      currindex--;
      if (currindex < 0)
        currindex = CurrentGroup.GroupMaps.Count - 1;

      Channel chan = (Channel)CurrentGroup.GroupMaps[currindex].Channel;
      m_zapchannel = chan.Name;

      _log.Info("Navigator:ZapPrevious {0}->{1}", currentChan, m_zapchannel);
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
      if (_lastViewedChannel != string.Empty)
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
    private int GetChannelIndex(string channelName)
    {
      for (int i = 0; i < CurrentGroup.GroupMaps.Count; i++)
      {
        Channel chan = (Channel)CurrentGroup.GroupMaps[i].Channel;
        if (chan.Name == channelName)
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
      _log.Info("ChannelNavigator::LoadSettings()");
      m_currentchannel = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
      m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
      string groupname = xmlreader.GetValueAsString("mytv", "group", GUILocalizeStrings.Get(972));
      m_currentgroup = GetGroupIndex(groupname);
      if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count)		// Group no longer exists?
        m_currentgroup = 0;

      m_currentChannel = GetChannel(m_currentchannel);
      if (m_currentChannel == null)
      {
        ChannelGroup group = (ChannelGroup)m_groups[m_currentgroup];
        if (group.GroupMaps.Count > 0)
          m_currentchannel = group.GroupMaps[0].Channel.Name;
        m_currentChannel = GetChannel(m_currentchannel);
      }
    }

    public void SaveSettings(MediaPortal.Profile.Settings xmlwriter)
    {
      if (m_currentchannel != null)
      {
        if (m_currentchannel.Trim() != String.Empty)
          xmlwriter.SetValue("mytv", "channel", m_currentchannel);
      }
      if (CurrentGroup != null)
      {
        if (CurrentGroup.GroupName.Trim() != String.Empty)
          xmlwriter.SetValue("mytv", "group", CurrentGroup.GroupName);
      }
    }

    void CreateDatabaseConfigFile(string connectionString)
    {
      using (FileStream stream = new FileStream("ideablade.ibconfig", FileMode.OpenOrCreate, FileAccess.ReadWrite))
      {
        using (StreamWriter writer = new StreamWriter(stream))
        {
          string configFile = ConfigFileXml;
          configFile = configFile.Replace("[CONNECTION]", connectionString);
          configFile = configFile.Replace("|", "\"");
          writer.WriteLine(configFile);
        }
      }
    }
    #endregion
  }

  #endregion
}
