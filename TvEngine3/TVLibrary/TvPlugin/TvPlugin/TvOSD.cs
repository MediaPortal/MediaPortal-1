#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using Gentle.Framework;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Player.PostProcessing;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using Action = MediaPortal.GUI.Library.Action;

namespace TvPlugin
{
  public class TvOsd : GUIInternalWindow
  {
    private enum Controls
    {
      OSD_VIDEOPROGRESS = 1,

      OSD_TIMEINFO = 100,
      Panel1 = 101,
      Panel2 = 150,

      OSD_PAUSE = 209,
      OSD_SKIPBWD = 210,
      OSD_REWIND = 211,
      OSD_STOP = 212,
      OSD_PLAY = 213,
      OSD_FFWD = 214,
      OSD_SKIPFWD = 215,

      OSD_MUTE = 216,
      // OSD_SYNC =217 - not used
      OSD_SUBTITLES = 218,
      OSD_BOOKMARKS = 219,
      OSD_VIDEO = 220,
      OSD_AUDIO = 221,

      OSD_SUBMENU_BG_VOL = 300,
      OSD_VOLUMESLIDER = 400,
      OSD_VOLUMESLIDER_LABEL = 450,

      // OSD_SUBMENU_BG_SYNC 301 - not used

      OSD_SUBMENU_BG_SUBTITLES = 302,
      OSD_SUBTITLE_DELAY = 800,
      OSD_SUBTITLE_DELAY_LABEL = 850,
      OSD_SUBTITLE_ONOFF = 801,
      OSD_SUBTITLE_LIST = 802,

      OSD_SUBMENU_BG_BOOKMARKS = 303,

      OSD_SUBMENU_BG_VIDEO = 304,
      OSD_BRIGHTNESS = 704,
      OSD_BRIGHTNESSLABEL = 752,
      OSD_CONTRAST = 705,
      OSD_CONTRASTLABEL = 753,
      OSD_GAMMA = 706,
      OSD_GAMMALABEL = 754,
      OSD_SHARPNESS = 716,
      OSD_SHARPNESSLABEL = 755,
      OSD_SATURATION = 717,
      OSD_SATURATIONLABEL = 756,
      OSD_NONINTERLEAVED = 701,
      OSD_NOCACHE = 702,
      OSD_ADJFRAMERATE = 703,
      OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF = 707,
      OSD_VIDEO_POSTPROC_RESIZE_ONOFF = 708,
      OSD_VIDEO_POSTPROC_CROP_ONOFF = 709,
      OSD_VIDEO_POSTPROC_CROP_VERTICAL = 711,
      OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL = 710,
      OSD_VIDEO_POSTPROC_CROP_HORIZONTAL = 713,
      OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL = 712,
      OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF = 714,

      OSD_SUBMENU_NIB = 350,
      OSD_AVDELAY = 500,
      OSD_AVDELAY_LABEL = 550,

      OSD_SUBMENU_BG_AUDIO = 305,
      OSD_AUDIOSTREAM_LIST = 501,
      OSD_AUDIOVOLUMESLIDER = 511,
      OSD_AUDIOVOLUMESLIDER_LABEL = 512,
      OSD_CREATEBOOKMARK = 600,
      OSD_BOOKMARKS_LIST = 601,
      OSD_CLEARBOOKMARKS = 602,
      OSD_BOOKMARKS_LIST_LABEL = 650,
      OSD_VIDEOPOS = 700,
      OSD_VIDEOPOS_LABEL = 750
    } ;

    [SkinControl(10)] protected GUIImage imgTvChannelLogo = null;
    [SkinControl(31)] protected GUIButtonControl btnChannelUp = null;
    [SkinControl(32)] protected GUIButtonControl btnChannelDown = null;
    [SkinControl(33)] protected GUIButtonControl btnPreviousProgram = null;
    [SkinControl(34)] protected GUIButtonControl btnNextProgram = null;
    [SkinControl(35)] protected GUILabelControl lblCurrentChannel = null;
    [SkinControl(36)] protected GUITextControl tbOnTvNow = null;
    [SkinControl(37)] protected GUITextControl tbOnTvNext = null;
    [SkinControl(38)] protected GUITextScrollUpControl tbProgramDescription = null;
    [SkinControl(39)] protected GUIImage imgRecIcon = null;
    [SkinControl(100)] protected GUILabelControl lblCurrentTime = null;

    private bool isSubMenuVisible = false;
    private int m_iActiveMenu = 0;
    private int m_iActiveMenuButtonID = 0;
    private int m_subtitleDelay = 0;
    private int m_delayInterval = 0;
    private int m_audioDelay = 0;
    private int m_delayIntervalAudio = 0;
    private bool m_bNeedRefresh = false;
    private DateTime m_dateTime = DateTime.Now;
    private DateTime _RecIconLastCheck = DateTime.Now;
    private Program previousProgram = null;
    private bool _immediateSeekIsRelative = true;
    private int _immediateSeekValue = 10;
    private bool _confirmTimeshiftStop = true;

    private IList listTvChannels;

    private int UpdateInt = 0;

    public TvOsd()
    {
      GetID = (int)Window.WINDOW_TVOSD;
    }

    public override bool IsTv
    {
      get { return true; }
    }

    public override bool Init()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _immediateSeekIsRelative = xmlreader.GetValueAsBool("movieplayer", "immediateskipstepsisrelative", true);
        _immediateSeekValue = xmlreader.GetValueAsInt("movieplayer", "immediateskipstepsize", 10);
        _confirmTimeshiftStop = xmlreader.GetValueAsBool("mytv", "confirmTimeshiftStop", true);
      }
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\tvOSD.xml"));
      return bResult;
    }

    public bool SubMenuVisible
    {
      get { return isSubMenuVisible; }
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void Render(float timePassed)
    {
      // update information all 50 frames to avoid stuttering
      if (UpdateInt == 50 || UpdateInt == 0)
      {
        UpdateProgressBar();
        //SetVideoProgress();
        Get_TimeInfo(); // show the time elapsed/total playing time
        if (UpdateInt == 50)
        {
          UpdateInt = 0;
        }
      }
      UpdateInt++;
      SetRecorderStatus(); // BAV: fixing bug 1429: OSD is not updated with recording status 
      base.Render(timePassed); // render our controls to the screen
    }

    private void HideControl(int dwSenderId, int dwControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, GetID, dwSenderId, dwControlID, 0, 0, null);
      OnMessage(msg);
    }

    private void ShowControl(int dwSenderId, int dwControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID, dwSenderId, dwControlID, 0, 0, null);
      OnMessage(msg);
    }

    private void FocusControl(int dwSenderId, int dwControlID, int dwParam)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, dwSenderId, dwControlID, dwParam,
                                      0, null);
      OnMessage(msg);
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        // translate movements (up, down, left right) back
        case Action.ActionType.ACTION_STEP_BACK:
          action.wID = Action.ActionType.ACTION_MOVE_LEFT;
          break;

        case Action.ActionType.ACTION_STEP_FORWARD:
          action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
          break;

        case Action.ActionType.ACTION_BIG_STEP_BACK:
          action.wID = Action.ActionType.ACTION_MOVE_DOWN;
          break;

        case Action.ActionType.ACTION_BIG_STEP_FORWARD:
          action.wID = Action.ActionType.ACTION_MOVE_UP;
          break;

        case Action.ActionType.ACTION_OSD_SHOW_LEFT:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_RIGHT:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_UP:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_DOWN:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_SELECT:
          break;

        case Action.ActionType.ACTION_OSD_HIDESUBMENU:
          break;

        case Action.ActionType.ACTION_CONTEXT_MENU:
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        case Action.ActionType.ACTION_SHOW_OSD:
          {
            if (isSubMenuVisible) // is sub menu on?
            {
              FocusControl(GetID, m_iActiveMenuButtonID, 0); // set focus to last menu button
              ToggleSubMenu(0, m_iActiveMenu); // hide the currently active sub-menu
            }
            if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
            {
              TvFullScreen tvWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)Window.WINDOW_TVFULLSCREEN);
              tvWindow.OnAction(new Action(Action.ActionType.ACTION_SHOW_OSD, 0, 0));
              tvWindow.OnAction(action);
            }
            return;
          }

        case Action.ActionType.ACTION_PAUSE:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_PAUSE,
                                               (int)Controls.OSD_PAUSE, 0, 0, null);
            OnMessage(msgSet);

            if (g_Player.Paused)
            {
              GUIWindowManager.IsPauseOsdVisible = true;
            }
            else
            {
              GUIWindowManager.IsPauseOsdVisible = false;
            }
            return;
          }

        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          {
            g_Player.Speed = 1; // Single speed
            ToggleButton((int)Controls.OSD_REWIND, false); // pop all the relevant
            ToggleButton((int)Controls.OSD_FFWD, false); // buttons back to
            ToggleButton((int)Controls.OSD_PLAY, false); // their up state
            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_STOP:
          {
            if (g_Player.IsTVRecording)
            {
              Log.Debug("TvOSD: stop from recorded TV");
              g_Player.Stop();
            }
            if (g_Player.IsTimeShifting && CanStopTimeshifting())
            {
              Log.Debug("TvOSD: stop confirmed");
              g_Player.Stop();
            }
            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_FORWARD:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_FFWD,
                                               (int)Controls.OSD_FFWD, 0, 0, null);
            OnMessage(msgSet);

            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_REWIND:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_REWIND,
                                               (int)Controls.OSD_REWIND, 0, 0, null);
            OnMessage(msgSet);

            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_OSD_SHOW_VALUE_PLUS:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_SKIPFWD,
                                               (int)Controls.OSD_SKIPFWD, 0, 0, null);
            OnMessage(msgSet);
            return;
          }

        case Action.ActionType.ACTION_OSD_SHOW_VALUE_MIN:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_SKIPBWD,
                                               (int)Controls.OSD_SKIPBWD, 0, 0, null);
            OnMessage(msgSet);
            return;
          }

        case Action.ActionType.ACTION_NEXT_CHANNEL:
          {
            GUIWindowManager.IsPauseOsdVisible = false;
            OnNextChannel();
            return;
          }

        case Action.ActionType.ACTION_PREV_CHANNEL:
          {
            GUIWindowManager.IsPauseOsdVisible = false;
            OnPreviousChannel();
            return;
          }
      }

      if (/*((action.wID >= Action.ActionType.REMOTE_0) && (action.wID <= Action.ActionType.REMOTE_9)) ||*/
          (action.wID == Action.ActionType.ACTION_KEY_PRESSED && action.m_key != null &&
          (char)action.m_key.KeyChar >= '0' && (char)action.m_key.KeyChar <= '9'))
      {
        TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)Window.WINDOW_TVFULLSCREEN);
        if (TVWindow != null)
        {
          TVWindow.OnKeyCode((char)action.m_key.KeyChar);
        }
      }
      base.OnAction(action);
    }

    private bool CanStopTimeshifting()
    {
      if (!_confirmTimeshiftStop)
      {
        // Can always stop timeshift when confirmation is not required
        return true;
      }

      // Get dialog to ask the user for confirmation
      Log.Debug("TvOSD: user request to stop");
      GUIDialogPlayStop dlgPlayStop =
        (GUIDialogPlayStop)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PLAY_STOP);
      if (dlgPlayStop == null)
      {
        // Return true to avoid dead end on missing dialog
        return true;
      }

      dlgPlayStop.SetHeading(GUILocalizeStrings.Get(605));
      dlgPlayStop.SetLine(1, GUILocalizeStrings.Get(2550));
      dlgPlayStop.SetLine(2, GUILocalizeStrings.Get(2551));
      dlgPlayStop.SetDefaultToStop(false);
      dlgPlayStop.DoModal(GetID);
      if (dlgPlayStop.IsStopConfirmed)
      {
        Log.Debug("TvOSD: stop confirmed");
        return true;
      }

      return false;
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT: // fired when OSD is hidden
          {
            //if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(true);
            // following line should stay. Problems with OSD not
            // appearing are already fixed elsewhere
            //for (int i = (int)Controls.Panel1; i < (int)Controls.Panel2; i++)
            //{
            //  HideControl(GetID, i);
            //}
            Dispose();
            GUIPropertyManager.SetProperty("#currentmodule", GUIWindowManager.GetWindow(message.Param1).GetModuleName());
            return true;
          }


        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT: // fired when OSD is shown
          {
            // following line should stay. Problems with OSD not
            // appearing are already fixed elsewhere
            SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
            sb.AddConstraint(Operator.Equals, "istv", 1);
            sb.AddOrderByField(true, "sortOrder");
            SqlStatement stmt = sb.GetStatement(true);
            listTvChannels = ObjectFactory.GetCollection(typeof (Channel), stmt.Execute());
            GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());
            previousProgram = null;
            AllocResources();
            // if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(false);
            ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
            isSubMenuVisible = false;
            m_iActiveMenuButtonID = 0;
            m_iActiveMenu = 0;
            m_bNeedRefresh = false;
            m_dateTime = DateTime.Now;
            Reset();
            FocusControl(GetID, (int)Controls.OSD_PLAY, 0); // set focus to play button by default when window is shown
            ShowPrograms();
            QueueAnimation(AnimationType.WindowOpen);
            for (int i = (int)Controls.Panel1; i < (int)Controls.Panel2; i++)
            {
              ShowControl(GetID, i);
            }
            if (g_Player.Paused)
            {
              ToggleButton((int)Controls.OSD_PLAY, true);
              // make sure play button is down (so it shows the pause symbol)
            }
            else
            {
              ToggleButton((int)Controls.OSD_PLAY, false); // make sure play button is up (so it shows the play symbol)
            }
            m_delayInterval = MediaPortal.Player.Subtitles.SubEngine.GetInstance().DelayInterval;
            if (m_delayInterval > 0)
              m_subtitleDelay = MediaPortal.Player.Subtitles.SubEngine.GetInstance().Delay / m_delayInterval;
            m_delayIntervalAudio = PostProcessingEngine.GetInstance().AudioDelayInterval;
            if (m_delayIntervalAudio > 0)
              m_audioDelay = PostProcessingEngine.GetInstance().AudioDelay / m_delayIntervalAudio;

            g_Player.UpdateMediaInfoProperties();
            GUIPropertyManager.SetProperty("#TV.View.HasTeletext", TVHome.Card.HasTeletext.ToString());

            MediaPortal.Player.VideoStreamFormat videoFormat = g_Player.GetVideoFormat();

            GUIPropertyManager.SetProperty("#Play.Current.TSBitRate",
             ((float)MediaPortal.Player.g_Player.GetVideoFormat().bitrate / 1024 / 1024).ToString("0.00", CultureInfo.InvariantCulture));

            GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalLevel", TVHome.Card.SignalLevel.ToString());
            GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalQuality", TVHome.Card.SignalQuality.ToString());

            GUIPropertyManager.SetProperty("#Play.Current.VideoFormat.RawResolution",
              videoFormat.width.ToString() + "x" + videoFormat.height.ToString());

            GUIPropertyManager.SetProperty("#TV.TuningDetails.FreeToAir", string.Empty);

            Channel chan = TVHome.Navigator.Channel;
            if (chan != null)
            {
              IList<TuningDetail> details = chan.ReferringTuningDetail();
              if (details.Count > 0)
              {
                TuningDetail detail = null;
                switch (TVHome.Card.Type)
                {
                  case TvLibrary.Interfaces.CardType.Analog:
                    foreach (TuningDetail t in details)
                    {
                      if (t.ChannelType == 0)
                        detail = t;
                    }
                    break;
                  case TvLibrary.Interfaces.CardType.Atsc:
                    foreach (TuningDetail t in details)
                    {
                      if (t.ChannelType == 1)
                        detail = t;
                    }
                    break;
                  case TvLibrary.Interfaces.CardType.DvbC:
                    foreach (TuningDetail t in details)
                    {
                      if (t.ChannelType == 2)
                        detail = t;
                    }
                    break;
                  case TvLibrary.Interfaces.CardType.DvbS:
                    foreach (TuningDetail t in details)
                    {
                      if (t.ChannelType == 3)
                        detail = t;
                    }
                    break;
                  case TvLibrary.Interfaces.CardType.DvbT:
                    foreach (TuningDetail t in details)
                    {
                      if (t.ChannelType == 4)
                        detail = t;
                    }
                    break;
                  default:
                    detail = details[0];
                    break;
                }
                GUIPropertyManager.SetProperty("#TV.TuningDetails.FreeToAir", detail.FreeToAir.ToString());
              }
            }
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;

        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
          {
            if (message.SenderControlId == 13)
            {
              return true;
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId; // get the ID of the control sending us a message

            if (btnChannelUp != null && iControl == btnChannelUp.GetID)
            {
              OnNextChannel();
            }

            if (btnChannelDown != null && iControl == btnChannelDown.GetID)
            {
              OnPreviousChannel();
            }

            if (!g_Player.IsTVRecording)
            {
              if (btnPreviousProgram != null && iControl == btnPreviousProgram.GetID)
              {
                Program prog = GetChannel().GetProgramAt(m_dateTime);
                if (prog != null)
                {
                  prog =
                    GetChannel().GetProgramAt(
                      prog.StartTime.Subtract(new TimeSpan(0, 1, 0)));
                  if (prog != null)
                  {
                    m_dateTime = prog.StartTime.AddMinutes(1);
                  }
                }
                ShowPrograms();
              }
              if (iControl == btnNextProgram.GetID)
              {
                Program prog = GetChannel().GetProgramAt(m_dateTime);
                if (prog != null)
                {
                  prog = GetChannel().GetProgramAt(prog.EndTime.AddMinutes(+1));
                  if (prog != null)
                  {
                    m_dateTime = prog.StartTime.AddMinutes(1);
                  }
                }
                ShowPrograms();
              }
            }

            if (iControl >= (int)Controls.OSD_VOLUMESLIDER)
              // one of the settings (sub menu) controls is sending us a message
            {
              Handle_ControlSetting(iControl, message.Param1);
            }

            if (iControl == (int)Controls.OSD_PAUSE)
            {
              if (g_Player.Paused)
              {
                ToggleButton((int)Controls.OSD_PLAY, true);
                // make sure play button is down (so it shows the pause symbol)                
                ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
              }
              else
              {
                ToggleButton((int)Controls.OSD_PLAY, false);
                // make sure play button is up (so it shows the play symbol)
                if (g_Player.Speed < 1) // are we not playing back at normal speed
                {
                  ToggleButton((int)Controls.OSD_REWIND, true); // make sure out button is in the down position
                  ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                }
                else
                {
                  ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
                  if (g_Player.Speed == 1)
                  {
                    ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                  }
                }
              }
            }

            if (iControl == (int)Controls.OSD_PLAY)
            {
              //TODO
              int iSpeed = g_Player.Speed;
              if (iSpeed != 1) // we're in ffwd or rewind mode
              {
                g_Player.Speed = 1; // drop back to single speed
                ToggleButton((int)Controls.OSD_REWIND, false); // pop all the relevant
                ToggleButton((int)Controls.OSD_FFWD, false); // buttons back to
                ToggleButton((int)Controls.OSD_PLAY, false); // their up state
              }
              else
              {
                g_Player.Pause(); // Pause/Un-Pause playback
                if (g_Player.Paused)
                {
                  ToggleButton((int)Controls.OSD_PLAY, true);
                  // make sure play button is down (so it shows the pause symbol)
                }
                else
                {
                  ToggleButton((int)Controls.OSD_PLAY, false);
                  // make sure play button is up (so it shows the play symbol)
                }
              }
            }

            if (iControl == (int)Controls.OSD_STOP)
            {
              if (isSubMenuVisible) // sub menu currently active ?
              {
                FocusControl(GetID, m_iActiveMenuButtonID, 0); // set focus to last menu button
                ToggleSubMenu(0, m_iActiveMenu); // hide the currently active sub-menu
              }
              //g_application.m_guiWindowFullScreen.m_bOSDVisible = false;	// toggle the OSD off so parent window can de-init
              Log.Debug("TVOSD:stop");
              if (TVHome.Card.IsRecording)
              {
                int id = TVHome.Card.RecordingScheduleId;
                if (id > 0)
                {
                  TVHome.TvServer.StopRecordingSchedule(id);
                }
              }
              //GUIWindowManager.ShowPreviousWindow();							// go back to the previous window
            }

            if (iControl == (int)Controls.OSD_REWIND)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause(); // Unpause playback
              }

              if (g_Player.Speed < 1) // are we not playing back at normal speed
              {
                ToggleButton((int)Controls.OSD_REWIND, true); // make sure out button is in the down position
                ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
              }
              else
              {
                ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
                if (g_Player.Speed == 1)
                {
                  ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                }
              }
            }

            if (iControl == (int)Controls.OSD_FFWD)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause(); // Unpause playback
              }

              if (g_Player.Speed > 1) // are we not playing back at normal speed
              {
                ToggleButton((int)Controls.OSD_FFWD, true); // make sure out button is in the down position
                ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
              }
              else
              {
                ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                if (g_Player.Speed == 1)
                {
                  ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
                }
              }
            }

            if (iControl == (int)Controls.OSD_SKIPBWD)
            {
              if (_immediateSeekIsRelative)
              {
                g_Player.SeekRelativePercentage(-_immediateSeekValue);
              }
              else
              {
                g_Player.SeekRelative(-_immediateSeekValue);
              }
              ToggleButton((int)Controls.OSD_SKIPBWD, false); // pop the button back to it's up state
            }

            if (iControl == (int)Controls.OSD_SKIPFWD)
            {
              if (_immediateSeekIsRelative)
              {
                g_Player.SeekRelativePercentage(_immediateSeekValue);
              }
              else
              {
                g_Player.SeekRelative(_immediateSeekValue);
              }
              ToggleButton((int)Controls.OSD_SKIPFWD, false); // pop the button back to it's up state
            }

            if (iControl == (int)Controls.OSD_MUTE)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_VOL); // hide or show the sub-menu
              if (isSubMenuVisible) // is sub menu on?
              {
                int iValue = g_Player.Volume;
                GUISliderControl pSlider = GetControl((int)Controls.OSD_VOLUMESLIDER) as GUISliderControl;
                if (null != pSlider)
                {
                  pSlider.Percentage = iValue; // Update our volume slider accordingly ...
                }
                ShowControl(GetID, (int)Controls.OSD_VOLUMESLIDER); // show the volume control
                ShowControl(GetID, (int)Controls.OSD_VOLUMESLIDER_LABEL);
                FocusControl(GetID, (int)Controls.OSD_VOLUMESLIDER, 0); // set focus to it
              }
              else // sub menu is off
              {
                FocusControl(GetID, (int)Controls.OSD_MUTE, 0); // set focus to the mute button
              }
            }

            if (iControl == (int)Controls.OSD_SUBTITLES)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_SUBTITLES); // hide or show the sub-menu
              if (isSubMenuVisible)
              {
                // set the controls values
                GUISliderControl pControl = (GUISliderControl)GetControl((int)Controls.OSD_SUBTITLE_DELAY);
                pControl.SpinType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT;
                pControl.FloatInterval = 1;
                pControl.SetRange(-10, 10);
                SetSliderValue(-10, 10, m_subtitleDelay, (int)Controls.OSD_SUBTITLE_DELAY);
                SetCheckmarkValue(g_Player.EnableSubtitle, (int)Controls.OSD_SUBTITLE_ONOFF);
                // show the controls on this sub menu
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

                FocusControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY, 0);
                // set focus to the first control in our group
                PopulateSubTitles(); // populate the list control with subtitles for this video
              }
            }

            if (iControl == (int)Controls.OSD_BOOKMARKS)
            {
              //not used
            }

            if (iControl == (int)Controls.OSD_VIDEO)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_VIDEO); // hide or show the sub-menu
              if (isSubMenuVisible) // is sub menu on?
              {
                // set the controls values
                float fPercent = (float)(100 * (g_Player.CurrentPosition / g_Player.Duration));
                SetSliderValue(0.0f, 100.0f, (float)fPercent, (int)Controls.OSD_VIDEOPOS);

                bool hasPostProc = g_Player.HasPostprocessing;
                if (hasPostProc)
                {
                  IPostProcessingEngine engine = PostProcessingEngine.GetInstance();
                  SetCheckmarkValue(engine.EnablePostProcess, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
                  SetCheckmarkValue(engine.EnableResize, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
                  SetCheckmarkValue(engine.EnableCrop, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
                  SetCheckmarkValue(engine.EnableDeinterlace, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
                  UpdatePostProcessing();
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL);
                }

                //SetCheckmarkValue(g_stSettings.m_bNonInterleaved, Controls.OSD_NONINTERLEAVED);
                //SetCheckmarkValue(g_stSettings.m_bNoCache, Controls.OSD_NOCACHE);
                //SetCheckmarkValue(g_stSettings.m_bFrameRateConversions, Controls.OSD_ADJFRAMERATE);

                UpdateGammaContrastBrightness();
                // show the controls on this sub menu
                ShowControl(GetID, (int)Controls.OSD_VIDEOPOS);
                ShowControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
                ShowControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
                ShowControl(GetID, (int)Controls.OSD_NOCACHE);
                ShowControl(GetID, (int)Controls.OSD_ADJFRAMERATE);
                ShowControl(GetID, (int)Controls.OSD_SATURATIONLABEL);
                ShowControl(GetID, (int)Controls.OSD_SATURATION);
                ShowControl(GetID, (int)Controls.OSD_SHARPNESSLABEL);
                ShowControl(GetID, (int)Controls.OSD_SHARPNESS);
                ShowControl(GetID, (int)Controls.OSD_BRIGHTNESS);
                ShowControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);
                ShowControl(GetID, (int)Controls.OSD_CONTRAST);
                ShowControl(GetID, (int)Controls.OSD_CONTRASTLABEL);
                ShowControl(GetID, (int)Controls.OSD_GAMMA);
                ShowControl(GetID, (int)Controls.OSD_GAMMALABEL);
                FocusControl(GetID, (int)Controls.OSD_VIDEOPOS, 0); // set focus to the first control in our group
              }
            }

            if (iControl == (int)Controls.OSD_AUDIO)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_AUDIO); // hide or show the sub-menu
              if (isSubMenuVisible) // is sub menu on?
              {
                int iValue = g_Player.Volume;
                GUISliderControl pSlider = GetControl((int)Controls.OSD_AUDIOVOLUMESLIDER) as GUISliderControl;
                if (null != pSlider)
                {
                  pSlider.Percentage = iValue; // Update our volume slider accordingly ...
                }

                // set the controls values
                GUISliderControl pControl = (GUISliderControl)GetControl((int)Controls.OSD_AVDELAY);
                pControl.SpinType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT;
                pControl.SetRange(-20, 20);
                SetSliderValue(-20, 20, m_audioDelay, (int)Controls.OSD_AVDELAY);

                bool hasPostProc = g_Player.HasPostprocessing;
                if (hasPostProc)
                {
                  GUIPropertyManager.SetProperty("#TvOSD.AudioVideoDelayPossible", "true");
                  pControl.FloatInterval = 1;
                }
                else
                { 
                  GUIPropertyManager.SetProperty("#TvOSD.AudioVideoDelayPossible", "false");
                  pControl.FloatValue = 0;
                  m_audioDelay = 0;
                  pControl.FloatInterval = 0;
                }

                // show the controls on this sub menu
                ShowControl(GetID, (int)Controls.OSD_AVDELAY);
                ShowControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);
                ShowControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
                ShowControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER);
                ShowControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER_LABEL);

                FocusControl(GetID, (int)Controls.OSD_AVDELAY, 0); // set focus to the first control in our group
                PopulateAudioStreams(); // populate the list control with audio streams for this video
              }
            }

            return true;
          }
      }
      return base.OnMessage(message);
    }

    private void UpdateGammaContrastBrightness()
    {
      float fBrightNess = (float)GUIGraphicsContext.Brightness;
      float fContrast = (float)GUIGraphicsContext.Contrast;
      float fGamma = (float)GUIGraphicsContext.Gamma;
      float fSaturation = (float)GUIGraphicsContext.Saturation;
      float fSharpness = (float)GUIGraphicsContext.Sharpness;

      SetSliderValue(0.0f, 100.0f, (float)fBrightNess, (int)Controls.OSD_BRIGHTNESS);
      SetSliderValue(0.0f, 100.0f, (float)fContrast, (int)Controls.OSD_CONTRAST);
      SetSliderValue(0.0f, 100.0f, (float)fGamma, (int)Controls.OSD_GAMMA);
      SetSliderValue(0.0f, 100.0f, (float)fSaturation, (int)Controls.OSD_SATURATION);
      SetSliderValue(0.0f, 100.0f, (float)fSharpness, (int)Controls.OSD_SHARPNESS);
    }

    private void UpdatePostProcessing()
    {
      IPostProcessingEngine engine = PostProcessingEngine.GetInstance();
      SetSliderValue(0.0f, 100.0f, (float)engine.CropVertical, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
      SetSliderValue(0.0f, 100.0f, (float)engine.CropHorizontal, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
    }

    private void SetVideoProgress()
    {
      if (g_Player.Playing)
      {
        //  double fPercentage=g_Player.CurrentPosition / g_Player.Duration;
        //      GUIProgressControl pControl = (GUIProgressControl)GetControl((int)Controls.OSD_VIDEOPROGRESS);
        //    if (null!=pControl) pControl.Percentage=(int)(100*fPercentage);			// Update our progress bar accordingly ...

        //float fPercent = (float)(100 * (g_Player.CurrentPosition / g_Player.Duration));
        //SetSliderValue(0.0f, 100.0f, (float)fPercent, (int)Controls.OSD_VIDEOPOS); // Update our progress slider accordingly ...

        int iValue = g_Player.Volume;
        GUISliderControl pSlider = GetControl((int)Controls.OSD_VOLUMESLIDER) as GUISliderControl;
        if (null != pSlider)
        {
          pSlider.Percentage = iValue; // Update our volume slider accordingly ...
        }
        pSlider = GetControl((int)Controls.OSD_AUDIOVOLUMESLIDER) as GUISliderControl;
        if (null != pSlider)
        {
          pSlider.Percentage = iValue; // Update our volume slider accordingly ...
        }
      }
    }

    private void ToggleButton(int iButtonID, bool bSelected)
    {
      GUIControl pControl = GetControl(iButtonID) as GUIControl;

      if (pControl != null)
      {
        if (bSelected) // do we want the button to appear down?
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, GetID, 0, iButtonID, 0, 0, null);
          OnMessage(msg);
        }
        else // or appear up?
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, GetID, 0, iButtonID, 0, 0, null);
          OnMessage(msg);
        }
      }
    }

    private void ToggleSubMenu(int iButtonID, int iBackID)
    {
      int iX, iY;

      GUIImage pImgNib = GetControl((int)Controls.OSD_SUBMENU_NIB) as GUIImage; // pointer to the nib graphic
      GUIImage pImgBG = GetControl(iBackID) as GUIImage; // pointer to the background graphic
      GUIControl pButton = GetControl(iButtonID);
      // pointer to the OSD menu button

      // check to see if we are currently showing a sub-menu and it's position is different
      if (isSubMenuVisible && iBackID != m_iActiveMenu)
      {
        m_bNeedRefresh = true;
        isSubMenuVisible = false; // toggle it ready for the new menu requested
      }

      // Get button position
      if (pButton != null)
      {
        iX = (pButton.XPosition + (pButton.Width / 2)); // center of button
        iY = pButton.YPosition;
      }
      else
      {
        iX = 0;
        iY = 0;
      }

      // Set nib position
      if (pImgNib != null && pImgBG != null)
      {
        pImgNib.SetPosition(iX - (pImgNib.TextureWidth / 2), iY - pImgNib.TextureHeight);

        if (!isSubMenuVisible) // sub menu not currently showing?
        {
          pImgNib.Visible = true; // make it show
          pImgBG.Visible = true; // make it show
        }
        else
        {
          pImgNib.Visible = false; // hide it
          pImgBG.Visible = false; // hide it
        }
      }

      isSubMenuVisible = !isSubMenuVisible; // toggle sub menu visible status
      if (!isSubMenuVisible)
      {
        m_bNeedRefresh = true;
      }

      GUIPropertyManager.SetProperty("#TvOSD.AudioVideoDelayPossible", " ");

      // Set all sub menu controls to hidden
      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
      HideControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER_LABEL);

      HideControl(GetID, (int)Controls.OSD_AVDELAY);
      HideControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);

      HideControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
      HideControl(GetID, (int)Controls.OSD_NOCACHE);
      HideControl(GetID, (int)Controls.OSD_ADJFRAMERATE);

      HideControl(GetID, (int)Controls.OSD_SHARPNESSLABEL);
      HideControl(GetID, (int)Controls.OSD_SHARPNESS);

      HideControl(GetID, (int)Controls.OSD_SATURATIONLABEL);
      HideControl(GetID, (int)Controls.OSD_SATURATION);

      HideControl(GetID, (int)Controls.OSD_BRIGHTNESS);
      HideControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);

      HideControl(GetID, (int)Controls.OSD_GAMMA);
      HideControl(GetID, (int)Controls.OSD_GAMMALABEL);

      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL);

      HideControl(GetID, (int)Controls.OSD_CONTRAST);
      HideControl(GetID, (int)Controls.OSD_CONTRASTLABEL);

      HideControl(GetID, (int)Controls.OSD_CREATEBOOKMARK);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST_LABEL);
      HideControl(GetID, (int)Controls.OSD_CLEARBOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

      // Reset the other buttons back to up except the one that's active
      if (iButtonID != (int)Controls.OSD_MUTE)
      {
        ToggleButton((int)Controls.OSD_MUTE, false);
      }
      //if (iButtonID != (int)Controls.OSD_SYNC) ToggleButton((int)Controls.OSD_SYNC, false); - not used
      if (iButtonID != (int)Controls.OSD_SUBTITLES)
      {
        ToggleButton((int)Controls.OSD_SUBTITLES, false);
      }
      if (iButtonID != (int)Controls.OSD_BOOKMARKS)
      {
        ToggleButton((int)Controls.OSD_BOOKMARKS, false);
      }
      if (iButtonID != (int)Controls.OSD_VIDEO)
      {
        ToggleButton((int)Controls.OSD_VIDEO, false);
      }
      if (iButtonID != (int)Controls.OSD_AUDIO)
      {
        ToggleButton((int)Controls.OSD_AUDIO, false);
      }

      if (m_iActiveMenu > 1 && m_iActiveMenu != iBackID)
        GUIControl.HideControl(GetID, m_iActiveMenu);

      m_iActiveMenu = iBackID;
      m_iActiveMenuButtonID = iButtonID;
    }

    private float GetSliderValue(int iControlID)
    {
      GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);

      if (null != pControl)
      {
        switch (pControl.SpinType)
        {
          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
            return pControl.FloatValue;

          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
            return pControl.IntValue;

          default:
            return pControl.Percentage;
        }
      }

      return 0.0f;
    }

    private void SetSliderValue(float fMin, float fMax, float fValue, int iControlID)
    {
      GUISliderControl pControl = GetControl(iControlID) as GUISliderControl;

      if (null != pControl)
      {
        switch (pControl.SpinType)
        {
          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
            pControl.SetFloatRange(fMin, fMax);
            pControl.FloatValue = fValue;
            break;

          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
            pControl.SetRange((int)fMin, (int)fMax);
            pControl.IntValue = (int)fValue;
            break;

          default:
            pControl.Percentage = (int)fValue;
            break;
        }
      }
    }

    private void SetCheckmarkValue(bool bValue, int iControlID)
    {
      if (bValue)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, GetID, 0, iControlID, 0, 0, null);
        OnMessage(msg);
      }
      else
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, GetID, 0, iControlID, 0, 0, null);
        OnMessage(msg);
      }
    }

    private void Handle_ControlSetting(int iControlID, long wID)
    {
      string strMovie = g_Player.CurrentFile;

      switch (iControlID)
      {
        case (int)Controls.OSD_VOLUMESLIDER:
        case (int)Controls.OSD_AUDIOVOLUMESLIDER:
          {
            GUISliderControl pControl = GetControl(iControlID) as GUISliderControl;
            if (null != pControl)
            {
              // no volume control yet so no code here at the moment
              if (g_Player.Playing)
              {
                int iPercentage = pControl.Percentage;
                g_Player.Volume = iPercentage;
              }
            }
          }
          break;

        case (int)Controls.OSD_VIDEOPOS:
          {
            g_Player.SeekAsolutePercentage((int)GetSliderValue(iControlID));
          }
          break;

        case (int)Controls.OSD_SATURATION:
          {
            GUIGraphicsContext.Saturation = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_SHARPNESS:
          {
            GUIGraphicsContext.Sharpness = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_BRIGHTNESS:
          {
            GUIGraphicsContext.Brightness = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_CONTRAST:
          {
            GUIGraphicsContext.Contrast = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_GAMMA:
          {
            GUIGraphicsContext.Gamma = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        /* not used
        case (int)Controls.OSD_AUDIOSTREAM_LIST:
          {

          }
          break;

        case (int)Controls.OSD_CREATEBOOKMARK:
          {

          }
          break;

        case (int)Controls.OSD_BOOKMARKS_LIST:
          {

          }
          break;

        case (int)Controls.OSD_CLEARBOOKMARKS:
          {

          }
          break;
        */

        case (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF:
          {
            PostProcessingEngine.GetInstance().EnablePostProcess = !PostProcessingEngine.GetInstance().EnablePostProcess;
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF:
          {
            PostProcessingEngine.GetInstance().EnableResize = !PostProcessingEngine.GetInstance().EnableResize;
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF:
          {
            PostProcessingEngine.GetInstance().EnableDeinterlace = !PostProcessingEngine.GetInstance().EnableDeinterlace;
          }
          break;

        case (int)Controls.OSD_SUBTITLE_ONOFF:
          {
            g_Player.EnableSubtitle = !g_Player.EnableSubtitle;
          }
          break;

        case (int)Controls.OSD_SUBTITLE_LIST:
          {
            if (wID != 0) // check to see if list control has an action ID, remote can cause 0 based events
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0,
                                              (int)Controls.OSD_SUBTITLE_LIST, 0, 0, null);
              OnMessage(msg); // retrieve the selected list item
              if (g_Player.SupportsCC) // Subtitle CC
              {
                if (g_Player.SupportsCC && msg.Param1 == 0)
                {
                  msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM, GetID, 0,
                                              (int)Controls.OSD_SUBTITLE_LIST, msg.Param1, 0, null);
                  g_Player.EnableSubtitle = false;
                  g_Player.CurrentSubtitleStream = -1;
                  Log.Info("Subtitle CC selected ");
                }
                else
                {
                  Log.Info("Subtitle stream selected " + msg.Label);
                  g_Player.CurrentSubtitleStream = msg.Param1 - 1; // set the current subtitle
                  g_Player.EnableSubtitle = true;
                }
              }
              else
              {
                Log.Info("Subtitle stream selected " + msg.Label);
                g_Player.CurrentSubtitleStream = msg.Param1; // set the current subtitle
              }
              PopulateSubTitles();
            }
          }
          break;

        case (int)Controls.OSD_SUBTITLE_DELAY:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            if (null != pControl && g_Player.EnableSubtitle)
            {
              if (pControl.FloatValue < m_subtitleDelay)
              {
                MediaPortal.Player.Subtitles.SubEngine.GetInstance().DelayMinus();
              }
              else if (pControl.FloatValue > m_subtitleDelay)
              {
                MediaPortal.Player.Subtitles.SubEngine.GetInstance().DelayPlus();
              }
              m_subtitleDelay = (int)pControl.FloatValue;
            }
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            if (null != pControl)
            {
              PostProcessingEngine.GetInstance().CropVertical = pControl.Percentage;
              UpdatePostProcessing();
            }
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            if (null != pControl)
            {
              PostProcessingEngine.GetInstance().CropHorizontal = pControl.Percentage;
              UpdatePostProcessing();
            }
          }
          break;

        case (int)Controls.OSD_AVDELAY:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            IPostProcessingEngine engine = PostProcessingEngine.GetInstance();

            if (null != pControl && g_Player.HasPostprocessing)
            {
              if (pControl.FloatValue < m_audioDelay)
              { 
                  PostProcessingEngine.GetInstance().AudioDelayMinus();
              }
              else if (pControl.FloatValue > m_audioDelay)
              { 
                  PostProcessingEngine.GetInstance().AudioDelayPlus();
              }
              m_audioDelay = (int)pControl.FloatValue;
            }
          }
          break;
      }
    }

    private void PopulateAudioStreams()
    {
      // tell the list control not to show the page x/y spin control
      GUIListControl pControl = (GUIListControl)GetControl((int)Controls.OSD_AUDIOSTREAM_LIST);
      if (null != pControl)
      {
          pControl.SetPageControlVisible(false);
      }

      // empty the list ready for population
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID, 0,
                                      (int)Controls.OSD_AUDIOSTREAM_LIST, 0, 0, null);
      OnMessage(msg);

      string strLabel = GUILocalizeStrings.Get(460); // "Audio Stream"
      string strActiveLabel = GUILocalizeStrings.Get(461); // "[active]"

      // Add DVB audio streams
      /*
      ArrayList audioPidList = RemoteControl.Instance.GetAudioLanguageList();
      if (audioPidList != null && audioPidList.Count > 0)
      {
        DVBSections.AudioLanguage al;
        DVBSections sections = new DVBSections();
        int ActiveIndex = 0;
        for (int i = 0; i < audioPidList.Count; i++)
        {
          al = (DVBSections.AudioLanguage)audioPidList[i];
          string strItem;
          string strLang = DVBSections.GetLanguageFromCode(al.AudioLanguageCode);

          if (RemoteControl.Instance.GetAudioLanguage() == al.AudioPid)
          {
            // formats to 'Audio Stream X [active]'
            strItem = String.Format(strLang + "  " + strActiveLabel);	// this audio stream is active, show as such
            ActiveIndex = i;
          }
          else
          {
            // formats to 'Audio Stream X'
            strItem = String.Format(strLang);
          }

          // create a list item object to add to the list
          GUIListItem pItem = new GUIListItem();
          pItem.Label = strItem;
          pItem.ItemId = al.AudioPid;

          // add it ...
          lstAudioStreamList.Add(pItem);
        }

        // set the current active audio stream as the selected item in the list control
        lstAudioStreamList.SelectedListItemIndex = ActiveIndex;
      }*/
    }

    private void PopulateSubTitles()
    {
      // get the number of subtitles in the current movie
      int subStreamsCount = g_Player.SubtitleStreams;

      // tell the list control not to show the page x/y spin control
      GUIListControl pControl = GetControl((int)Controls.OSD_SUBTITLE_LIST) as GUIListControl;
      if (null != pControl)
      {
        pControl.SetPageControlVisible(false);
      }

      // empty the list ready for population
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID, 0,
                                      (int)Controls.OSD_SUBTITLE_LIST, 0, 0, null);
      OnMessage(msg);

      string strLabel = GUILocalizeStrings.Get(462); // "Subtitle"
      string strActiveLabel = GUILocalizeStrings.Get(461); // "[active]"

      // cycle through each subtitle and add it to our list control
      int currentSubtitleStream = g_Player.CurrentSubtitleStream;
      if (g_Player.SupportsCC) // The current subtitle CC is not in the list, add it on top of it
      {
        string strActive = (g_Player.SupportsCC) ? strActiveLabel : null;
        string CC1 = "CC1";

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem();
        pItem.Label = CC1;

        if (currentSubtitleStream == -1)
          if (strActive != null) pItem.Label = "CC1" + " " + strActiveLabel;

        // add it ...
        GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID, 0,
                                         (int)Controls.OSD_SUBTITLE_LIST, 0, 0, pItem);
        OnMessage(msg2);
      }
      for (int i = 0; i < subStreamsCount; ++i)
      {
        string strItem;
        string strLang = g_Player.SubtitleLanguage(i);
        // formats right label2 to '[active]'
        string strActive = (currentSubtitleStream == i) ? strActiveLabel : null;
        int ipos = strLang.IndexOf("[");
        if (ipos > 0)
        {
          strLang = strLang.Substring(0, ipos);
        }
        // formats to 'Language'
        strItem = String.Format(strLang);

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem(strItem);

        if (strActive != null) pItem.Label = strItem + " " + strActiveLabel;

        // add it ...
        GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID, 0,
                                         (int)Controls.OSD_SUBTITLE_LIST, 0, 0, pItem);
        OnMessage(msg2);
      }

      // set the current active subtitle as the selected item in the list control
      if (g_Player.SupportsCC)
      {
        if (currentSubtitleStream == -1)
        {
          GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
            (int)Controls.OSD_SUBTITLE_LIST, 0, 0, null);
          OnMessage(msgSet);
        }
        else
        {
          GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
            (int)Controls.OSD_SUBTITLE_LIST, currentSubtitleStream + 1, 0, null);
          OnMessage(msgSet);
        }
      }
      else
      {
        GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
          (int)Controls.OSD_SUBTITLE_LIST, g_Player.CurrentSubtitleStream, 0, null);
        OnMessage(msgSet);
      }
    }

    public override void ResetAllControls()
    {
      //reset all
      bool bOffScreen = false;
      int iCalibrationY = GUIGraphicsContext.OSDOffset;
      int iTop = GUIGraphicsContext.OverScanTop;
      int iMin = 0;

      foreach (CPosition pos in _listPositions)
      {
        pos.control.SetPosition((int)pos.XPos, (int)pos.YPos + iCalibrationY);
      }
      foreach (CPosition pos in _listPositions)
      {
        GUIControl pControl = pos.control;

        int dwPosY = pControl.YPosition;
        if (pControl.IsVisible)
        {
          if (dwPosY < iTop)
          {
            int iSize = iTop - dwPosY;
            if (iSize > iMin)
            {
              iMin = iSize;
            }
            bOffScreen = true;
          }
        }
      }
      if (bOffScreen)
      {
        foreach (CPosition pos in _listPositions)
        {
          GUIControl pControl = pos.control;
          int dwPosX = pControl.XPosition;
          int dwPosY = pControl.YPosition;
          if (dwPosY < (int)100)
          {
            dwPosY += Math.Abs(iMin);
            pControl.SetPosition(dwPosX, dwPosY);
          }
        }
      }
      base.ResetAllControls();
    }

    private void Reset()
    {
      GUIPropertyManager.SetProperty("#TvOSD.AudioVideoDelayPossible", " ");

      // Set all sub menu controls to hidden
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_AUDIO);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_VIDEO);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_BOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_SUBTITLES);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_VOL);

      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
      HideControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
      HideControl(GetID, (int)Controls.OSD_AVDELAY);
      HideControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER_LABEL);

      HideControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
      HideControl(GetID, (int)Controls.OSD_NOCACHE);
      HideControl(GetID, (int)Controls.OSD_ADJFRAMERATE);

      HideControl(GetID, (int)Controls.OSD_SATURATION);
      HideControl(GetID, (int)Controls.OSD_SATURATIONLABEL);

      HideControl(GetID, (int)Controls.OSD_SHARPNESS);
      HideControl(GetID, (int)Controls.OSD_SHARPNESSLABEL);

      HideControl(GetID, (int)Controls.OSD_BRIGHTNESS);
      HideControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);

      HideControl(GetID, (int)Controls.OSD_GAMMA);
      HideControl(GetID, (int)Controls.OSD_GAMMALABEL);

      HideControl(GetID, (int)Controls.OSD_CONTRAST);
      HideControl(GetID, (int)Controls.OSD_CONTRASTLABEL);

      HideControl(GetID, (int)Controls.OSD_CREATEBOOKMARK);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST_LABEL);
      HideControl(GetID, (int)Controls.OSD_CLEARBOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL);

      ToggleButton((int)Controls.OSD_MUTE, false);
      ToggleButton((int)Controls.OSD_SUBTITLES, false);
      ToggleButton((int)Controls.OSD_BOOKMARKS, false);
      ToggleButton((int)Controls.OSD_VIDEO, false);
      ToggleButton((int)Controls.OSD_AUDIO, false);

      ToggleButton((int)Controls.OSD_REWIND, false); // pop all the relevant
      ToggleButton((int)Controls.OSD_FFWD, false); // buttons back to
      ToggleButton((int)Controls.OSD_PLAY, false); // their up state

      ToggleButton((int)Controls.OSD_SKIPBWD, false); // pop all the relevant
      ToggleButton((int)Controls.OSD_STOP, false); // buttons back to
      ToggleButton((int)Controls.OSD_SKIPFWD, false); // their up state
      ToggleButton((int)Controls.OSD_MUTE, false); // their up state

      //ShowControl(GetID, (int)Controls.OSD_VIDEOPROGRESS);
    }

    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh)
      {
        m_bNeedRefresh = false;
        return true;
      }
      return false;
    }

    private void Get_TimeInfo()
    {
      string strTime = "";
      if (!g_Player.IsTVRecording)
      {
        string strChannel = GetChannelName();
        strTime = strChannel;
        Program prog = TVHome.Navigator.Channel.CurrentProgram;
        if (prog != null)
        {
          strTime = String.Format("{0}-{1}",
                                  prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        }
      }
      else
      {
        long currentPosition = (long)(g_Player.CurrentPosition);
        int hh = (int)(currentPosition / 3600) % 100;
        int mm = (int)((currentPosition / 60) % 60);
        int ss = (int)((currentPosition / 1) % 60);
        DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hh, mm, ss);

        long duration = (long)(g_Player.Duration);
        hh = (int)(duration / 3600) % 100;
        mm = (int)((duration / 60) % 60);
        ss = (int)((duration / 1) % 60);
        DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hh, mm, ss);

        strTime = String.Format("{0}-{1}",
                                startTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                endTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Controls.OSD_TIMEINFO, 0,
                                      0, null);
      msg.Label = strTime;
      OnMessage(msg); // ask our label to update it's caption
    }

    private void OnPreviousChannel()
    {
      Log.Debug("GUITV OSD: OnPreviousChannel");
      if (!TVHome.Card.IsTimeShifting && !g_Player.IsTVRecording)
      {
        return;
      }
      TVHome.Navigator.ZapToPreviousChannel(false);

      ShowPrograms();
      m_dateTime = DateTime.Now;
    }

    private void OnNextChannel()
    {
      Log.Debug("GUITV OSD: OnNextChannel");
      if (!TVHome.Card.IsTimeShifting && !g_Player.IsTVRecording)
      {
        return;
      }

      TVHome.Navigator.ZapToNextChannel(false);

      ShowPrograms();
      m_dateTime = DateTime.Now;
    }

    public void UpdateChannelInfo()
    {
      ShowPrograms();
    }

    private void SetCurrentChannelLogo()
    {
      string strChannel = GetChannelName();
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, strChannel);

      if (imgTvChannelLogo != null)
      {
        if (!string.IsNullOrEmpty(strLogo))                      
        {
          imgTvChannelLogo.SetFileName(strLogo);
          m_bNeedRefresh = true;
          imgTvChannelLogo.Visible = true;
        }
        else
        {
          imgTvChannelLogo.Visible = false;
        }
      }
    }

    private Channel GetChannel()
    {
      if (g_Player.IsTVRecording)
      {
        Recording rec = TvRecorded.ActiveRecording();
        return rec.ReferencedChannel();
      }
      else
      {
        return TVHome.Navigator.ZapChannel;
      }
    }
    private string GetChannelName()
    {
      if (g_Player.IsTVRecording)
      {
        Recording rec = TvRecorded.ActiveRecording();
        return TvRecorded.GetRecordingDisplayName(rec);
      }
      else
      {
        string tmpDisplayName = TVHome.Navigator.ZapChannel.DisplayName;
        Channel tmpChannel = TVHome.Navigator.ZapChannel;

        if (tmpChannel != null)
        {
          return TVHome.Navigator.ZapChannel.DisplayName;
        }
        else
        {
          TVHome.Navigator.ReLoad();
          // Let TvHome reload all channel information from the database. This makes sure that recently renamed linked subchannels handled the right way.
          return TVHome.Navigator.ZapChannel.DisplayName;
        }
      }
    }

    private void ShowPrograms()
    {
      if (tbProgramDescription != null)
      {
        tbProgramDescription.Clear();
      }
      if (tbOnTvNow != null)
      {
        tbOnTvNow.EnableUpDown = false;
        tbOnTvNow.Clear();
      }
      if (tbOnTvNext != null)
      {
        tbOnTvNext.EnableUpDown = false;
        tbOnTvNext.Clear();
      }

      SetRecorderStatus(true);

      // Channel icon
      if (imgTvChannelLogo != null)
      {
        SetCurrentChannelLogo();
      }

      if (lblCurrentChannel != null)
      {
        lblCurrentChannel.Label = GetChannelName();
      }

      if (TVHome.Navigator.Channel != null)
      {
        Program prog = TVHome.Navigator.Channel.CurrentProgram;
        //TVHome.Navigator.GetChannel(GetChannelName()).GetProgramAt(m_dateTime);      

        if (prog != null && !g_Player.IsTVRecording)
        {
          string strTime = String.Format("{0}-{1}",
                                         prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                         prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

          if (lblCurrentTime != null)
          {
            lblCurrentTime.Label = strTime;
          }
          // On TV Now
          if (tbOnTvNow != null)
          {
            strTime = String.Format("{0} ", prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            tbOnTvNow.Label = strTime + TVUtil.GetDisplayTitle(prog);
            GUIPropertyManager.SetProperty("#TV.View.start", strTime);

            strTime = String.Format("{0} ", prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.View.stop", strTime);
            GUIPropertyManager.SetProperty("#TV.View.remaining", Utils.SecondsToHMSString(prog.CalculateTimeRemaining()));
            GUIPropertyManager.SetProperty("#TV.View.remainingSeconds", Utils.SecondsToHMSStringSeconds(prog.CalculateTimeRemaining()));
            GUIPropertyManager.SetProperty("#TV.View.remainingMinutes", Utils.SecondsToHMSStringMinutes(prog.CalculateTimeRemaining()));
          }
          if (tbProgramDescription != null)
          {
            tbProgramDescription.Label = prog.Description;
          }

          // next program
          Channel chan = GetChannel();
          if (chan != null)
          {
            prog = chan.GetProgramAt(prog.EndTime.AddMinutes(1));

            if (prog != null)
            {
              if (tbOnTvNext != null)
              {
                tbOnTvNext.Label = strTime + "  " + TVUtil.GetDisplayTitle(prog);
              }
            }
          }
        }
      }
      else if (g_Player.IsTVRecording)
      {
        Recording rec = null;
        string description = "";
        string title = "";
        string startTime = ""; // DateTime.MinValue;
        string endTime = ""; //  DateTime.MaxValue;
        //string remaining = "";

        rec = TvRecorded.ActiveRecording();
        if (rec != null)
        {
          description = rec.Description;
          title = rec.Title;
          Channel ch = Channel.Retrieve(rec.IdChannel);
          if (ch != null)
          {
            string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, ch.DisplayName);
            if (string.IsNullOrEmpty(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);
          }

          long currentPosition = (long)(g_Player.CurrentPosition);
          startTime = Utils.SecondsToHMSString((int)currentPosition);

          long duration = (long)(g_Player.Duration);
          endTime = Utils.SecondsToHMSString((int)duration);

          //remaining = "0";                    
          if (tbOnTvNow != null)
          {
            tbOnTvNow.Label = title;
          }
          GUIPropertyManager.SetProperty("#TV.View.compositetitle", title);
          GUIPropertyManager.SetProperty("#TV.View.start", startTime);
          GUIPropertyManager.SetProperty("#TV.View.stop", endTime);

          if (tbProgramDescription != null)
          {
            tbProgramDescription.Label = description;
          }
        }
      }

      else
      {
        if (tbOnTvNow != null)
        {
          tbOnTvNow.Label = GUILocalizeStrings.Get(736); // no epg for this channel
        }
        if (tbOnTvNext != null)
        {
          tbOnTvNext.Label = GUILocalizeStrings.Get(736); // no epg for this channel
        }

        GUIPropertyManager.SetProperty("#TV.View.start", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.stop", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.remaining", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.remainingSeconds", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.remainingMinutes", string.Empty);
        if (lblCurrentTime != null)
        {
          lblCurrentTime.Label = String.Empty;
        }
      }


      UpdateProgressBar();
    }

    private void SetRecorderStatus()
    {
      SetRecorderStatus(false);
    }

    private void SetRecorderStatus(bool forced)
    {
      if (imgRecIcon != null)
      {
        TimeSpan ts = DateTime.Now - _RecIconLastCheck;
        if (ts.TotalSeconds > 15 || forced)
        {
          bool isRecording = false;
          VirtualCard card;
          TvServer server = new TvServer();

          if (GetChannel() != null)
          {
            if (server.IsRecording(GetChannel().IdChannel, out card))
            {
              if (g_Player.IsTVRecording)
              {
                Recording rec = TvRecorded.ActiveRecording();
                if (rec != null)
                {
                  isRecording = TvRecorded.IsLiveRecording();
                }
              }
              else
              {
                isRecording = true;
              }
            }
          }

          imgRecIcon.Visible = isRecording;
          _RecIconLastCheck = DateTime.Now;
          Log.Info("OSD.SetRecorderStatus = {0}", imgRecIcon.Visible);
        }
      }
    }

    private void UpdateProgressBar()
    {
      if (g_Player.IsTimeShifting)
      {
        GUIPropertyManager.SetProperty("#Play.Current.TSBitRate",
         ((float)MediaPortal.Player.g_Player.GetVideoFormat().bitrate / 1024 / 1024).ToString("0.00", CultureInfo.InvariantCulture));
        GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalLevel", TVHome.Card.SignalLevel.ToString());
        GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalQuality", TVHome.Card.SignalQuality.ToString());
      }
      
      if (!g_Player.IsTVRecording)
      {
        double fPercent;
        Program prog = GetChannel().CurrentProgram;

        if (prog == null)
        {
          GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
          return;
        }
        string strTime = String.Format("{0}-{1}",
                                       prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        TimeSpan ts = prog.EndTime - prog.StartTime;
        double iTotalSecs = ts.TotalSeconds;
        ts = DateTime.Now - prog.StartTime;
        double iCurSecs = ts.TotalSeconds;
        fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
        fPercent *= 100.0d;
        GUIPropertyManager.SetProperty("#TV.View.Percentage", fPercent.ToString());
        Get_TimeInfo();

        bool updateProperties = false;
        if (previousProgram == null)
        {
          m_dateTime = DateTime.Now;
          previousProgram = prog.Clone();
          ShowPrograms();
          updateProperties = true;
        }
        else if (previousProgram.StartTime != prog.StartTime || previousProgram.IdChannel != prog.IdChannel)
        {
          m_dateTime = DateTime.Now;
          previousProgram = prog.Clone();
          ShowPrograms();
          updateProperties = true;
        }
        if (updateProperties)
        {
          GUIPropertyManager.SetProperty("#TV.View.channel", prog.ReferencedChannel().DisplayName);
          GUIPropertyManager.SetProperty("#TV.View.start",
                                         prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.View.stop",
                                         prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.View.remaining", Utils.SecondsToHMSString(prog.CalculateTimeRemaining()));
          GUIPropertyManager.SetProperty("#TV.View.remainingSeconds", Utils.SecondsToHMSStringSeconds(prog.CalculateTimeRemaining()));
          GUIPropertyManager.SetProperty("#TV.View.remainingMinutes", Utils.SecondsToHMSStringMinutes(prog.CalculateTimeRemaining()));
          GUIPropertyManager.SetProperty("#TV.View.genre", prog.Genre);
          GUIPropertyManager.SetProperty("#TV.View.title", prog.Title);
          GUIPropertyManager.SetProperty("#TV.View.compositetitle", TVUtil.GetDisplayTitle(prog));
          GUIPropertyManager.SetProperty("#TV.View.subtitle", prog.EpisodeName);
          GUIPropertyManager.SetProperty("#TV.View.description", prog.Description);
          GUIPropertyManager.SetProperty("#TV.View.episode", prog.EpisodeNumber);
        }
      }

      else
      {
        Recording rec = null;
        string startTime = "";
        string endTime = "";
        string channelDisplayName = "";

        rec = TvRecorded.ActiveRecording();
        if (rec != null)
        {
          long currentPosition = (long)(g_Player.CurrentPosition);
          startTime = Utils.SecondsToHMSString((int)currentPosition);

          long duration = (long)(g_Player.Duration);
          endTime = Utils.SecondsToHMSString((int)duration);


          channelDisplayName = TvRecorded.GetRecordingDisplayName(rec) + " (" + GUILocalizeStrings.Get(604) + ")";

          double fPercent;
          if (rec == null)
          {
            GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
            return;
          }

          fPercent = ((double)currentPosition) / ((double)duration);
          fPercent *= 100.0d;

          GUIPropertyManager.SetProperty("#TV.View.Percentage", fPercent.ToString());
          Get_TimeInfo();

          GUIPropertyManager.SetProperty("#TV.View.channel", channelDisplayName);
          GUIPropertyManager.SetProperty("#TV.View.start", startTime);
          GUIPropertyManager.SetProperty("#TV.View.stop", endTime);
          GUIPropertyManager.SetProperty("#TV.View.genre", rec.Genre);
          GUIPropertyManager.SetProperty("#TV.View.title", rec.Title);
          GUIPropertyManager.SetProperty("#TV.View.compositetitle", TVUtil.GetDisplayTitle(rec));
          GUIPropertyManager.SetProperty("#TV.View.description", rec.Description);
          GUIPropertyManager.SetProperty("#TV.View.subtitle", rec.EpisodeName);
          GUIPropertyManager.SetProperty("#TV.View.episode", rec.EpisodeNumber);
        }
      }
    }
  }
}