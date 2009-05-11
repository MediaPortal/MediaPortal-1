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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using AMS.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Configuration;
using System.Threading;
using Config = MediaPortal.Configuration.Config;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// Use locally installed TV cards to watch and record TV
  /// </summary>
  [PluginIcons("WindowPlugins.GUITV.Television.gif", "WindowPlugins.GUITV.TelevisionDisabled.gif")]
  public class GUITVHome : GUIWindow, ISetupForm, IShowPlugin, IPluginReceiver
  {
    #region constants
    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    #endregion

    #region variables
    enum Controls
    {
      IMG_REC_CHANNEL = 21,
      LABEL_REC_INFO = 22,
      IMG_REC_RECTANGLE = 23,
    };

    static bool _isTvOn = true;
    static bool _isTimeShifting = true;
    static bool _autoFullScreen = false;
    static bool _autoFullScreenOnly = false;
    static bool _resumed = false;
    static bool _showlastactivemodule = false;
    static bool _showlastactivemoduleFullscreen = false;

    static ChannelNavigator m_navigator = new ChannelNavigator();
    static GUITVCropManager _cropManager = new GUITVCropManager();

    DateTime _updateTimer = DateTime.Now;
    bool _autoTurnOnTv = false;
    bool _settingsLoaded = false;
    DateTime _dtlastTime = DateTime.Now;

    [SkinControlAttribute(2)]    protected GUIButtonControl btnTvGuide = null;
    [SkinControlAttribute(3)]    protected GUIButtonControl btnRecord = null;
    [SkinControlAttribute(6)]    protected GUIButtonControl btnGroup = null;
    [SkinControlAttribute(7)]    protected GUIButtonControl btnChannel = null;
    [SkinControlAttribute(8)]    protected GUIToggleButtonControl btnTvOnOff = null;
    [SkinControlAttribute(9)]    protected GUIToggleButtonControl btnTimeshiftingOnOff = null;
    [SkinControlAttribute(13)]   protected GUIButtonControl btnTeletext = null;
    [SkinControlAttribute(24)]   protected GUIImage imgRecordingIcon = null;
    [SkinControlAttribute(99)]   protected GUIVideoControl videoWindow = null;

    #endregion

    public GUITVHome()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV;

      // replace g_player's ShowFullScreenWindowTV
      g_Player.ShowFullScreenWindowTV = ShowFullScreenWindowTVHandler;
    }


    #region Serialisation
    void LoadSettings()
    {
      if (_settingsLoaded)
        return;
      _settingsLoaded = true;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml")))
      {
        m_navigator.LoadSettings(xmlreader);
        _isTvOn = xmlreader.GetValueAsBool("mytv", "tvon", false);
        _isTimeShifting = xmlreader.GetValueAsBool("mytv", "autoturnontimeshifting", false);
        _autoTurnOnTv = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        _showlastactivemodule = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
        _showlastactivemoduleFullscreen = xmlreader.GetValueAsBool("general", "lastactivemodulefullscreen", false);
        _autoFullScreen = xmlreader.GetValueAsBool("mytv", "autofullscreen", false);
        _autoFullScreenOnly = xmlreader.GetValueAsBool("mytv", "autofullscreenonly", false);

        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom"))
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch"))
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal"))
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original"))
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox"))
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan"))
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
        if (strValue.Equals("zoom149"))
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom14to9;
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml")))
      {
        m_navigator.SaveSettings(xmlwriter);
        xmlwriter.SetValueAsBool("mytv", "tvon", _isTvOn);
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

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvhome.xml");
      LoadSettings();

      return bResult;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_RECORD:
          //record current program on current channel
          //are we watching tv?
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            Log.Info("send message to fullscreen tv");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORD, GUIWindowManager.ActiveWindow, 0, 0, 0, 0, null);
            msg.SendToTargetWindow = true;
            msg.TargetWindowId = (int)GUIWindow.Window.WINDOW_TVFULLSCREEN;
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

          Log.Info("GUITvHome:Record action");
          if (Recorder.IsViewing() || Recorder.IsTimeShifting())
          {
            string channel = Recorder.GetTVChannelName();
            //yes, are we recording this channel already ?
            TVProgram prog = Navigator.GetTVChannel(channel).CurrentProgram;
            if (!Recorder.IsRecordingChannel(channel))
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
                      //record current program
                      Recorder.RecordNow(channel, false);
                      break;

                    case 876:
                      //manual record
                      Recorder.RecordNow(channel, true);
                      break;
                  }
                }
              }
              else
              {
                Recorder.RecordNow(channel, true);
              }
            }
            else
            {
              Recorder.StopRecording(Recorder.CurrentTVRecording);
            }
          }
          break;

        case Action.ActionType.ACTION_PREV_CHANNEL:
          GUITVHome.OnPreviousChannel();
          break;
        case Action.ActionType.ACTION_PAGE_DOWN:
          GUITVHome.OnPreviousChannel();
          break;

        case Action.ActionType.ACTION_NEXT_CHANNEL:
          GUITVHome.OnNextChannel();
          break;
        case Action.ActionType.ACTION_PAGE_UP:
          GUITVHome.OnNextChannel();
          break;

        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL:  // mPod
          GUITVHome.OnLastViewedChannel();
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            // goto home 
            // are we watching tv & doing timeshifting
            if (!g_Player.Playing && !Recorder.IsRadio())
            {
              //yes, do we want tv as background
              if (GUIGraphicsContext.ShowBackground)
              {
                // No, then stop viewing... 
                Recorder.StopViewing();
              }
            }
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

    private void OnResume()
    {
      Log.Debug("TVHome.OnResume()");
      _resumed = true;
    }

    public void Start()
    {
      Log.Debug("TVHome.Start()");
    }

    public void Stop()
    {
      Log.Debug("TVHome.Stop()");
    }


    public bool WndProc(ref System.Windows.Forms.Message msg)
    {
      if (msg.Msg == WM_POWERBROADCAST)
      {

        switch (msg.WParam.ToInt32())
        {
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

    protected override void OnPageLoad()
    {
      if (m_navigator == null)
      {
        m_navigator = new ChannelNavigator();			// Create the channel navigator (it will load groups and channels)
      }
      base.OnPageLoad();
      /*
      if (g_Player.Playing && !g_Player.IsTV)
      {
        if (!g_Player.IsTVRecording)
        {
          Log.Info("TVHome:stop music/video:{0}",g_Player.CurrentFile);
          g_Player.Stop();
        }
      }
      */

      //set video window position
      if (videoWindow != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width, videoWindow.Height);
      }

      // start viewing tv... 
      GUIGraphicsContext.IsFullScreenVideo = false;
      string channelName = Navigator.CurrentChannel;
      if (Navigator.CurrentChannel == string.Empty)
      {
        if (Navigator.CurrentGroup == null && Navigator.Groups.Count > 0)
        {
          GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.Groups[0].GroupName);
          Navigator.SetCurrentGroup(Navigator.Groups[0].GroupName);
        }
        if (Navigator.CurrentGroup != null)
        {
          if (Navigator.CurrentGroup.TvChannels.Count > 0)
          {
            channelName = Navigator.CurrentGroup.TvChannels[0].Name;
          }
        }
      }

      if (channelName != string.Empty)
      {
        if (Recorder.View)
        {
          channelName = Recorder.TVChannelName;
          _isTimeShifting = Recorder.IsTimeShifting();
          _isTvOn = true;
        }
        Log.Info("tv home init:{0}", channelName);
        if (_autoTurnOnTv) _isTvOn = true;
        ViewChannelAndCheck(channelName);
      }

      UpdateStateOfButtons();
      UpdateProgressPercentageBar();

      // if using showlastactivemodule feature and last module is fullscreen while returning from powerstate, then do not set fullscreen here (since this is done by the resume last active module feature)
      // we depend on the onresume method, thats why tvplugin now impl. the IPluginReceiver interface.      
      bool showlastActModFS = (_showlastactivemodule && _showlastactivemoduleFullscreen && _resumed);
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
      if (!showlastActModFS)
      {
        if (_autoFullScreen && !g_Player.FullScreen && (PreviousWindowId != (int)GUIWindow.Window.WINDOW_TVFULLSCREEN && PreviousWindowId != (int)GUIWindow.Window.WINDOW_TVGUIDE && PreviousWindowId != (int)GUIWindow.Window.WINDOW_SEARCHTV && PreviousWindowId != (int)GUIWindow.Window.WINDOW_RECORDEDTV && PreviousWindowId != (int)GUIWindow.Window.WINDOW_SCHEDULER))
        {
          Log.Debug("TVHome.OnPageLoad(): setting autoFullScreen");
          //if we are resuming from standby with tvhome, we want this in fullscreen, but we need a delay for it to work.
          if (useDelay)
          {
            Thread tvDelayThread = new Thread(TvDelayThread);
            tvDelayThread.IsBackground = true;
            tvDelayThread.Name = "TvDelayThread";
            tvDelayThread.Start();
          }
          else //no delay needed here, since this is when the system is being used normally
          {
            Log.Debug("TVHome.OnPageLoad(): setting autoFullScreen");
            g_Player.ShowFullScreenWindow();
          }
        }
        else if (_autoFullScreenOnly && !g_Player.FullScreen && (PreviousWindowId == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN))
        {
          Log.Debug("TVHome.OnPageLoad(): autoFullScreenOnly set, returning to previous window");
          GUIWindowManager.ShowPreviousWindow();
        }
      }
      _resumed = false;
    }

    private void TvDelayThread()
    {
      //we have to use a small delay before calling tvfullscreen.                                    
      Thread.Sleep(200);
      Log.Debug("TVHome.OnPageLoad(): setting autoFullScreen");
      g_Player.ShowFullScreenWindow();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      //if we're switching to another plugin
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        //and we're not playing which means we dont timeshift tv
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          // and we dont want tv in the background
          if (GUIGraphicsContext.ShowBackground)
          {
            // then stop timeshifting & viewing... 
            Recorder.StopViewing();
          }
        }
      }

      GUIGraphicsContext.VideoWindow = new Rectangle(0, 0, 0, 0);

      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    void OnSelectGroup()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
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
      if (dlg.SelectedLabel < 0)
        return;
      GUIPropertyManager.SetProperty("#TV.Guide.Group", dlg.SelectedLabelText);
      Navigator.SetCurrentGroup(dlg.SelectedLabelText);
      if (Navigator.CurrentGroup != null)
      {
        if (Navigator.CurrentGroup.TvChannels.Count > 0)
        {
          ViewChannelAndCheck(Navigator.CurrentGroup.TvChannels[0].Name);
        }
      }
    }

    void OnSelectChannel()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(891); // Select TV channel
      int selected = 0;
      for (int i = 0; i < Navigator.CurrentGroup.TvChannels.Count; ++i)
      {
        dlg.Add(Navigator.CurrentGroup.TvChannels[i].Name);
        if (Navigator.CurrentTVChannel != null)
        {
          if (Navigator.CurrentGroup.TvChannels[i].Name == Navigator.CurrentTVChannel.Name)
          {
            selected = i;
          }
        }
      }
      dlg.SelectedLabel = selected;
      dlg.DoModal(this.GetID);
      if (dlg.SelectedLabel < 0)
        return;
      ViewChannelAndCheck(dlg.SelectedLabelText);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnTvOnOff)
      {
        if (Recorder.IsViewing())
        {
          //tv off
          Log.Info("TVHome:turn tv off");
          _isTvOn = false;
          SaveSettings();
          g_Player.Stop();
        }
        else
        {
          if (!Recorder.Running)
          {
            Log.Info("TVHome: Recorder.Start()");
            Recorder.Start();
          }
          // tv on
          Log.Info("TVHome:turn tv on {0}", Navigator.CurrentChannel);
          _isTvOn = true;

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

      if (control == btnTimeshiftingOnOff)
      {
        //turn timeshifting off 
        _isTimeShifting = !Recorder.IsTimeShifting();
        Log.Info("tv home timeshift onoff:{0}", _isTimeShifting);
        ViewChannelAndCheck(Navigator.CurrentChannel);

        _isTimeShifting = Recorder.IsTimeShifting();

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
              _isTvOn = true;
              Log.Info("tv home msg resume tv:{0}", Navigator.CurrentChannel);
              ViewChannel(Navigator.CurrentChannel);
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
          Log.Info("tv home msg view chan:{0}", message.Label);
          ViewChannel(message.Label);
          Navigator.UpdateCurrentChannel();
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
          _isTvOn = false;
          Log.Info("tv home msg stop chan:{0}", message.Label);
          ViewChannel(message.Label);
          Navigator.UpdateCurrentChannel();
          break;
      }
      return base.OnMessage(message);
    }

    public override void Process()
    {
      // BAV, 02.03.08: a channel change should not be delayed by rendering.
      //                by moving thisthe 1 min delays in zapping should be fixed
      // Let the navigator zap channel if needed
      Navigator.CheckChannelChange();

      TimeSpan ts = DateTime.Now - _updateTimer;

      if (GUIGraphicsContext.InVmr9Render)
        return;
      if (ts.TotalMilliseconds < 100)
        return;

      // Don't update if recorder is stopped!
      if (Recorder.Running == false)
        return;
      if (Recorder.CommandProcessor == null)
        return;
      // Don't update while busy - Will make it look bad!
      if (Recorder.CommandProcessor.IsBusy)
        return;
      _updateTimer = DateTime.Now;

      // BAV, 02.03.08
      //Navigator.CheckChannelChange();

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

    }

    #endregion

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowTV
    /// </summary>
    /// <returns></returns>
    public static bool ShowFullScreenWindowTVHandler()
    {
      if (!g_Player.Playing && Recorder.IsViewing())
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


    public static void UpdateTimeShift()
    {
      _isTimeShifting = Recorder.IsTimeShifting();
    }

    void OnRecord()
    {
      //record now.
      //Are we recording this channel already?
      if (!Recorder.IsRecordingChannel(Navigator.CurrentChannel))
      {
        //no then start recording
        TVProgram prog = Navigator.CurrentTVChannel.CurrentProgram;
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
                //record current program
                Recorder.RecordNow(Navigator.CurrentChannel, false);
                break;

              case 876:
                //manual record
                Recorder.RecordNow(Navigator.CurrentChannel, true);
                break;
            }
          }
        }
        else
        {
          //manual record
          Recorder.RecordNow(Navigator.CurrentChannel, true);
        }
      }
      else
      {
        if (Recorder.IsRecording())
        {
          //yes then stop recording
          Navigator.UpdateCurrentChannel();
          Recorder.StopRecording();

          // No need to  re-start viewing, because the recorder starts to play TS file
        }
      }
      // UpdateStateOfButtons();
    }

    /// <summary>
    /// Update the state of the following buttons
    /// - tv on/off
    /// - timeshifting on/off
    /// - record now
    /// </summary>
    void UpdateStateOfButtons()
    {
      btnTvOnOff.Selected = Recorder.IsViewing();
      btnTeletext.IsVisible = Recorder.HasTeletext();
      //are we recording a tv program?
      if (Recorder.IsRecording())
      {
        //yes then disable the timeshifting on/off buttons
        //and change the Record Now button into Stop Record
        btnTimeshiftingOnOff.Disabled = true;
        btnTimeshiftingOnOff.Selected = true;
        btnRecord.Label = GUILocalizeStrings.Get(629);//stop record
      }
      else
      {
        //nop. then change the Record Now button
        //to Record Now
        btnRecord.Label = GUILocalizeStrings.Get(601);// record

        //is current card is not supporting timeshifting
        bool supportstimeshifting = Recorder.DoesSupportTimeshifting();
        if (btnTvOnOff.Selected == false || supportstimeshifting == false)
        {
          //then disable the timeshifting button
          btnTimeshiftingOnOff.Disabled = true;
          btnTimeshiftingOnOff.Selected = false;
        }
        else if (supportstimeshifting)
        {
          //enable the timeshifting button
          btnTimeshiftingOnOff.Disabled = false;
          // set state of timeshifting button
          if (Recorder.IsTimeShifting())
          {
            btnTimeshiftingOnOff.Selected = true;
          }
          else
          {
            btnTimeshiftingOnOff.Selected = false;
          }
        }
      }
    }

    void UpdateRecordingIndicator()
    {
      // if we're recording tv, update gui with info
      if (Recorder.IsRecording())
      {
        TVRecording rec = Recorder.GetTVRecording();
        if (rec != null)
        {
          if (rec.RecType != TVRecording.RecordingType.Once)
            imgRecordingIcon.SetFileName(Thumbs.TvRecordingSeriesIcon);
          else
            imgRecordingIcon.SetFileName(Thumbs.TvRecordingIcon);
        }
        imgRecordingIcon.IsVisible = true;
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
    void UpdateProgressPercentageBar()
    {
      try
      {
        if (Navigator.CurrentTVChannel == null)
        {
          GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
          return;
        }
        //get current tv program
        TVProgram prog = Navigator.CurrentTVChannel.CurrentProgram;
        if (prog == null)
        {
          GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
          return;
        }
        TimeSpan ts = prog.EndTime - prog.StartTime;
        if (ts.TotalSeconds <= 0)
        {
          GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
          return;
        }
        double iTotalSecs = ts.TotalSeconds;
        ts = DateTime.Now - prog.StartTime;
        double iCurSecs = ts.TotalSeconds;
        double fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
        fPercent *= 100.0d;
        GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)fPercent).ToString());
      }
      catch (Exception ex)
      {
        Log.Info("grrrr:{0}", ex.Source, ex.StackTrace);
      }
    }

    /// <summary>
    /// When called this method will switch to the previous TV channel
    /// </summary>
    public static void OnPreviousChannel()
    {
      Log.Info("GUITVHome:OnPreviousChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        GUIFullScreenTV TVWindow = (GUIFullScreenTV)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
        if (TVWindow != null)
          TVWindow.ZapPreviousChannel();
        return;
      }

      // Zap to previous channel immediately
      Navigator.ZapToPreviousChannel(false);
    }

    public static bool ViewChannelAndCheck(string channel)
    {
      if (!Recorder.Running)
      {
        Log.Info("GUITVHome.ViewChannelAndCheck(): Recorder.Running = false");
        return false;
      }
      if (g_Player.Playing)
      {
        if (g_Player.IsTVRecording)
          return true;
        if (g_Player.IsVideo)
          return true;
        if (g_Player.IsDVD)
          return true;
        if ((g_Player.IsMusic && g_Player.HasVideo))
          return true;
      }
      if (_isTvOn)
        Log.Info("GUITVHome.ViewChannelAndCheck(): View channel={0} ts:{1}", channel, _isTimeShifting);
      else
        Log.Info("GUITVHome.ViewChannelAndCheck(): _isTvOn = off - autostart doesn't apply");

      if (channel != Navigator.CurrentChannel)
        Navigator.LastViewedChannel = Navigator.CurrentChannel;

      string errorMessage;
      bool succeeded = Recorder.StartViewing(channel, _isTvOn, _isTimeShifting, true, out errorMessage);

      if (succeeded)
        return true;

      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (pDlgOK != null)
      {
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

    public static void ViewChannel(string channel)
    {
      if (g_Player.Playing)
      {
        if (g_Player.IsTVRecording)
          return;
        if (g_Player.IsVideo)
          return;
        if (g_Player.IsDVD)
          return;
        if ((g_Player.IsMusic && g_Player.HasVideo))
          return;
      }
      if (_isTvOn)
        Log.Info("GUITVHome.ViewChannel(): View channel={0} ts:{1}", channel, _isTimeShifting);
      else
        Log.Info("GUITVHome.ViewChannel(): _isTvOn = off");

      if (channel != Navigator.CurrentChannel)
        Navigator.LastViewedChannel = Navigator.CurrentChannel;

      string errorMessage;
      Recorder.StartViewing(channel, _isTvOn, _isTimeShifting, false, out errorMessage);
    }

    /// <summary>
    /// When called this method will switch to the next TV channel
    /// </summary>
    public static void OnNextChannel()
    {
      Log.Info("GUITVHome:OnNextChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        GUIFullScreenTV TVWindow = (GUIFullScreenTV)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
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

    public static bool IsTVOn
    {
      get { return _isTvOn; }
      set { _isTvOn = value; }
    }

    /// <summary>
    /// Gets the channel navigator that can be used for channel zapping.
    /// </summary>
    public static ChannelNavigator Navigator
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
      return "TV";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      // TODO:  Add GUITVHome.GetHome implementation
      strButtonText = GUILocalizeStrings.Get(605);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my tv.png";
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
      return true;
    }

    #endregion

  }

  #region ChannelNavigator class

  /// <summary>
  /// Handles the logic for channel zapping. This is used by the different GUI modules in the TV section.
  /// </summary>
  public class ChannelNavigator
  {
    #region Private members

    private List<TVGroup> m_groups = new List<TVGroup>(); // Contains all channel groups (including an "all channels" group)
    private int m_currentgroup = 0;
    private string m_currentchannel = string.Empty;
    private DateTime m_zaptime;
    private long m_zapdelay;
    private string m_zapchannel = null;
    private int m_zapgroup = -1;
    private string _lastViewedChannel = string.Empty; // saves the last viewed Channel  // mPod
    private TVChannel m_currentTvChannel = null;
    private List<TVChannel> channels = new List<TVChannel>();
    private bool reentrant = false;

    #endregion


    #region Constructors

    public ChannelNavigator()
    {

      // Load all groups
      ReLoad();
      TVDatabase.OnChannelsChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(OnChannelsChanged);
    }
    public void ReLoad()
    {
      m_groups.Clear();
      List<TVGroup> groups = new List<TVGroup>();
      TVDatabase.GetGroups(ref groups); // Put groups in a local variable to ensure the "All" group is first always

      channels.Clear();
      bool hideAllChannelsGroup = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        hideAllChannelsGroup = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
      }

      TVDatabase.GetChannels(ref channels); // Load all channels
      if (!hideAllChannelsGroup || (m_groups.Count == 0))
      {
        // Add a group containing all channels
        TVGroup tvgroup = new TVGroup();
        tvgroup.GroupName = GUILocalizeStrings.Get(972); //all channels
        foreach (TVChannel channel in channels) tvgroup.TvChannels.Add(channel);
        m_groups.Add(tvgroup);
      }

      m_groups.AddRange(groups); // Add rest of the groups to the end of the list
      Log.Debug("ChannelNavigator.Reload: m_groups = {0}, channels = {1}", m_groups.Count, channels.Count);
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
    /// Gets the channel that we currently watch.
    /// Returns empty string if there is no current channel.
    /// </summary>
    public TVChannel CurrentTVChannel
    {
      get { return m_currentTvChannel; }
    }

    /// <summary>
    /// Gets the currently active channel group.
    /// </summary>
    public TVGroup CurrentGroup
    {
      get { return (TVGroup)m_groups[m_currentgroup]; }
    }

    /// <summary>
    /// Gets the list of channel groups.
    /// </summary>
    public List<TVGroup> Groups
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
        return ((TVGroup)m_groups[m_zapgroup]).GroupName;
      }
    }
    #endregion

    #region Public methods


    public void ZapNow()
    {
      m_zaptime = DateTime.Now.AddSeconds(-1);
      //Log.Error("zapnow group:{0} current group:{0}", m_zapgroup, m_currentgroup);
      //if (m_zapchannel == null)
      //  Log.Error("zapchannel==null");
      //else
      //  Log.Error("zapchannel=={0}",m_zapchannel);
    }
    /// <summary>
    /// Checks if it is time to zap to a different channel. This is called during Process().
    /// </summary>
    public bool CheckChannelChange()
    {
      if (reentrant)
        return false;
      // BAV, 02.03.08: a channel change should not be delayed by rendering.
      //                by scipping this => 1 min delays in zapping should be avoided 
      //if (GUIGraphicsContext.InVmr9Render)
      //  return false;
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
            if (CurrentGroup.TvChannels.Count > 0)
            {
              TVChannel chan = (TVChannel)CurrentGroup.TvChannels[0];
              m_zapchannel = chan.Name;
            }
          }
          m_zapgroup = -1;

          //if (m_zapchannel != m_currentchannel)
          //  lastViewedChannel = m_currentchannel;
          // Zap to desired channel
          string zappingTo = m_zapchannel;
          m_zapchannel = null;
          Log.Info("Channel change:{0}", zappingTo);
          GUITVHome.ViewChannel(zappingTo);
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
      string newChannel = string.Empty;
      //if current card is watching tv then use that channel
      if (Recorder.IsViewing() || Recorder.IsTimeShifting())
      {
        newChannel = Recorder.GetTVChannelName();
      }
      else if (Recorder.IsRecording())
      { // else if current card is recording, then use that channel
        newChannel = Recorder.GetTVRecording().Channel;
      }
      else if (Recorder.IsAnyCardRecording())
      { // else if any card is recording
        //then get & use that channel
        for (int i = 0; i < Recorder.Count; ++i)
        {
          if (Recorder.Get(i).IsRecording)
          {
            newChannel = Recorder.Get(i).CurrentTVRecording.Channel;
          }
        }
      }
      if (newChannel == string.Empty)
        newChannel = m_currentchannel;
      if (m_currentchannel != newChannel && newChannel != string.Empty)
      {
        m_currentchannel = newChannel;
        m_currentTvChannel = GetTVChannel(m_currentchannel);
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
      List<TVChannel> channels = CurrentGroup.TvChannels;

      if (channelNr >= 0)
      {
        bool found = false;
        int ChannelCnt = 0;
        TVChannel chan;
        while (found == false && ChannelCnt < channels.Count)
        {
          chan = (TVChannel)channels[ChannelCnt];
          if (chan.External == false)
          {
            if (chan.Number == channelNr)
            {
              ZapToChannel(chan.Name, useZapDelay);
              found = true;
            }
          }
          else
          {
            if (Int32.Parse(chan.ExternalTunerChannel) == channelNr)
            {
              ZapToChannel(chan.Name, useZapDelay);
              found = true;
            }
          }
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
      List<TVChannel> channels = CurrentGroup.TvChannels;
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        TVChannel chan = (TVChannel)channels[channelNr];
        ZapToChannel(chan.Name, useZapDelay);
      }
    }

    /// <summary>
    /// Changes to the next channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToNextChannel(bool useZapDelay)
    {
      string currentChan = string.Empty;
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
      if (currindex >= CurrentGroup.TvChannels.Count)
        currindex = 0;
      TVChannel chan = (TVChannel)CurrentGroup.TvChannels[currindex];
      m_zapchannel = chan.Name;

      Log.Info("Navigator:ZapNext {0}->{1}", currentChan, m_zapchannel);
      if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
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
      string currentChan = string.Empty;
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
        currindex = CurrentGroup.TvChannels.Count - 1;

      TVChannel chan = (TVChannel)CurrentGroup.TvChannels[currindex];
      m_zapchannel = chan.Name;

      Log.Info("Navigator:ZapPrevious {0}->{1}", currentChan, m_zapchannel);
      if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
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

    void OnChannelsChanged()
    {
      // Load all groups
      if (GUIGraphicsContext.DX9Device == null)
        return;
      List<TVGroup> groups = new List<TVGroup>();
      TVDatabase.GetGroups(ref groups); // Put groups in a local variable to ensure the "All" group is first always

      channels.Clear();
      m_groups.Clear();
      // Add a group containing all channels
      TVDatabase.GetChannels(ref channels); // Load all channels
      TVGroup tvgroup = new TVGroup();
      tvgroup.GroupName = GUILocalizeStrings.Get(972); //all channels
      foreach (TVChannel channel in channels)
        tvgroup.TvChannels.Add(channel);
      m_groups.Add(tvgroup);
      m_groups.AddRange(groups); // Add rest of the groups to the end of the list

      if (m_currentchannel.Trim() == string.Empty)
      {
        TVGroup group = (TVGroup)m_groups[m_currentgroup];
        m_currentchannel = ((TVChannel)group.TvChannels[0]).Name;
      }
      m_currentTvChannel = GetTVChannel(m_currentchannel);

    }

    /// <summary>
    /// Retrieves the index of the current channel.
    /// </summary>
    /// <returns></returns>
    private int GetChannelIndex(string channelName)
    {
      for (int i = 0; i < CurrentGroup.TvChannels.Count; i++)
      {
        TVChannel chan = (TVChannel)CurrentGroup.TvChannels[i];
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
        TVGroup group = (TVGroup)m_groups[i];
        if (group.GroupName == groupname)
          return i;
      }
      return -1;
    }
    public TVChannel GetTVChannel(string channelName)
    {
      foreach (TVChannel chan in channels)
      {
        if (chan.Name == channelName)
          return chan;
      }
      return null;
    }

    #endregion

    #region Serialization

    public void LoadSettings(MediaPortal.Profile.Settings xmlreader)
    {
      m_currentchannel = xmlreader.GetValueAsString("mytv", "channel", string.Empty);
      m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
      string groupname = xmlreader.GetValueAsString("mytv", "group", GUILocalizeStrings.Get(972));
      m_currentgroup = GetGroupIndex(groupname);
      if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count)		// Group no longer exists?
        m_currentgroup = 0;

      m_currentTvChannel = GetTVChannel(m_currentchannel);
      Log.Debug("ChannelNavigator.LoadSettings: currentGroup = {0}, currentChannel = {1}, m_groups = {2}", m_currentgroup, m_currentchannel, m_groups.Count);
      if (m_currentTvChannel == null)
      {
        TVGroup group = (TVGroup)m_groups[m_currentgroup];
        if (group.TvChannels.Count > 0)
          m_currentchannel = ((TVChannel)group.TvChannels[0]).Name;
        m_currentTvChannel = GetTVChannel(m_currentchannel);
      }
    }

    public void SaveSettings(MediaPortal.Profile.Settings xmlwriter)
    {
      if (m_currentchannel.Trim() != string.Empty)
        xmlwriter.SetValue("mytv", "channel", m_currentchannel);

      if (CurrentGroup.GroupName.Trim() != string.Empty)
        xmlwriter.SetValue("mytv", "group", CurrentGroup.GroupName);
    }

    #endregion
  }

  #endregion
}
