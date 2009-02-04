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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TV.Recording;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for GUIVideoFullscreen.
  /// </summary>
  public class GUIVideoFullscreen : GUIWindow, IRenderLayer
  {
    private class FullScreenState
    {
      public int SeekStep = 1;
      public int Speed = 1;
      public bool OsdVisible = false;
      public bool Paused = false;
      public bool ContextMenuVisible = false;
      public bool ShowStatusLine = false;
      public bool ShowTime = false;
      public bool ShowSkipBar = false;
      public bool wasVMRBitmapVisible = false;
      public bool NotifyDialogVisible = false;
      public bool volumeVisible = false;
      public bool forbiddenVisible = false;
    }

    private enum Control
    {
      BLUE_BAR = 0,
      OSD_VIDEOPROGRESS = 1,
      LABEL_ROW1 = 10,
      LABEL_ROW2 = 11, // never used, and should be used, because some skins won't support it
      LABEL_ROW3 = 12, // never used, and should be used, because some skins won't support it
      IMG_PAUSE = 16,
      IMG_2X = 17,
      IMG_4X = 18,
      IMG_8X = 19,
      IMG_16X = 20,
      IMG_32X = 21,
      IMG_MIN2X = 23,
      IMG_MIN4X = 24,
      IMG_MIN8X = 25,
      IMG_MIN16X = 26,
      IMG_MIN32X = 27,
      OSD_TIMEINFO = 100,
      PANEL1 = 101,
      PANEL2 = 150
    } ;

    [SkinControl(500)] protected GUIImage imgVolumeMuteIcon;
    [SkinControl(501)] protected GUIVolumeBar imgVolumeBar;
    [SkinControl(502)] protected GUIImage imgActionForbiddenIcon;

    private bool _isOsdVisible = false;
    private bool _showStep = false;
    private bool _showStatus = false;
    private bool _showTime = false;
    private bool _showSkipBar = false;

    private DateTime m_dwTimeCodeTimeout;
    private string _timeStamp = "";
    private int _timeCodePosition = 0;
    private long _timeStatusShowTime = 0;
    private DateTime m_dwOSDTimeOut;
    private long m_iMaxTimeOSDOnscreen;
    //FormOSD     m_form=null;
    private DateTime _updateTimer = DateTime.Now;
    private DateTime _vmr7UpdateTimer = DateTime.Now;
    private bool _IsDialogVisible = false;
    private bool _needToClearScreen = false;
    private bool _isVolumeVisible = false;
    private bool _isForbiddenVisible = false;
    private GUIDialogMenu dlg;
    private GUIVideoOSD _osdWindow = null;
    private bool NotifyDialogVisible = false;
    private int _notifyTVTimeout = 15;
    private bool _playNotifyBeep = true;
    private DateTime _volumeTimer = DateTime.MinValue;
    private DateTime _forbiddenTimer = DateTime.MinValue;
    private VMR9OSD _vmr9OSD = new VMR9OSD();
    private PlayListPlayer playlistPlayer;
    private const int SKIPBAR_PADDING = 10;
    private List<Geometry.Type> _allowedArModes = new List<Geometry.Type>();
    private FullScreenState screenState = new FullScreenState();

    public GUIVideoFullscreen()
    {
      GetID = (int) Window.WINDOW_FULLSCREEN_VIDEO;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\videoFullScreen.xml");
      GetID = (int) Window.WINDOW_FULLSCREEN_VIDEO;
      return bResult;
    }

    #region settings serialisation

    private void LoadSettings()
    {
      string key = "movieplayer";
      if (g_Player.IsDVD)
      {
        key = "dvdplayer";
      }
      if (g_Player.IsTVRecording)
      {
        key = "mytv";
      }

      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        m_iMaxTimeOSDOnscreen = 1000*xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 5);
        _notifyTVTimeout = xmlreader.GetValueAsInt("movieplayer", "notifyTVTimeout", 10);
        _playNotifyBeep = xmlreader.GetValueAsBool("movieplayer", "notifybeep", true);

        string aspectRatioText = xmlreader.GetValueAsString(key, "defaultar", "Normal");
        GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(aspectRatioText);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strKey = "movieplayer";
        if (g_Player.IsDVD)
        {
          strKey = "dvdplayer";
        }
        if (g_Player.IsTVRecording)
        {
          strKey = "mytv";
        }

        string aspectRatioText = Util.Utils.GetAspectRatio(GUIGraphicsContext.ARType);
        xmlwriter.SetValue(strKey, "defaultar", aspectRatioText);
      }
    }

    #endregion

    private void OnOsdAction(Action action)
    {
      if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI) ||
           (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)) && !_osdWindow.SubMenuVisible) // hide the OSD
      {
        lock (this)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
          _isOsdVisible = false;
          GUIWindowManager.IsOsdVisible = false;
        }
      }
      else
      {
        m_dwOSDTimeOut = DateTime.Now;
        if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE || action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
        {
          int x = (int) action.fAmount1;
          int y = (int) action.fAmount2;
          if (!GUIGraphicsContext.MouseSupport)
          {
            _osdWindow.OnAction(action); // route keys to OSD window

            return;
          }
          else
          {
            if (_osdWindow.InWindow(x, y))
            {
              _osdWindow.OnAction(action); // route keys to OSD window

              return;
            }
            else
            {
              if (!_osdWindow.SubMenuVisible)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0,
                                                GetID, 0, null);
                _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
                _isOsdVisible = false;
                GUIWindowManager.IsOsdVisible = false;
              }
            }
          }
        }
        Action newAction = new Action();
        if (action.wID != Action.ActionType.ACTION_KEY_PRESSED &&
            ActionTranslator.GetAction((int) Window.WINDOW_OSD, action.m_key, ref newAction))
        {
          _osdWindow.OnAction(newAction); // route keys to OSD window
        }
        else
        {
          // route unhandled actions to OSD window
          _osdWindow.OnAction(action);
        }
      }
      return;
    }

    public override void OnAction(Action action)
    {
      _needToClearScreen = true;
      //switch back to menu on right-click
      if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
      {
        if (action.MouseButton == MouseButtons.Right)
        {
          _isOsdVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          GUIGraphicsContext.IsFullScreenVideo = false;
          GUIWindowManager.ShowPreviousWindow();
          return;
        }
        else if (_showSkipBar && action.MouseButton == MouseButtons.Left)
        {
          GUIControl cntl = base.GetControl((int) Control.OSD_VIDEOPROGRESS);
          if (cntl != null && cntl.Visible)
          {
            double percentage = GetPercentage(action.fAmount1, action.fAmount2, cntl);
            if (percentage > 0 && g_Player.CanSeek)
            {
              if (g_Player.IsDVD && g_Player.Paused)
              {
                _forbiddenTimer = DateTime.Now;
                RenderForbidden(true);
                return; // Skipping during pause doesn't work so well for DVD's...
              }
              else
              {
                Log.Debug("GUIVideoFullscreen.Mouse_Click - skipping");
                g_Player.SeekAbsolute(g_Player.Duration*percentage);
              }
            }
          }
          return;
        }
      }
      if (action.wID == Action.ActionType.ACTION_SHOW_VOLUME)
      {
        _volumeTimer = DateTime.Now;
        _isVolumeVisible = true;
        RenderVolume(_isVolumeVisible);

        //				if(m_vmr9OSD!=null)
        //					m_vmr9OSD.RenderVolumeOSD();
      }
      if (_isOsdVisible)
      {
        OnOsdAction(action);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE && GUIGraphicsContext.MouseSupport)
      {
        int y = (int) action.fAmount2;
        if (y > GUIGraphicsContext.Height - 100)
        {
          m_dwOSDTimeOut = DateTime.Now;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _osdWindow.OnMessage(msg); // Send an init msg to the OSD
          _isOsdVisible = true;
          _showSkipBar = false;
          GUIWindowManager.VisibleOsd = Window.WINDOW_OSD;
        }
        else if (y < 50)
        {
          _showSkipBar = true;
          _timeStatusShowTime = (DateTime.Now.Ticks/10000);
        }
        else
        {
          _showSkipBar = false;
        }
      }

      if (g_Player.IsDVD)
      {
        Action newAction = new Action();
        if (ActionTranslator.GetAction((int) Window.WINDOW_DVD, action.m_key, ref newAction))
        {
          if (g_Player.OnAction(newAction))
          {
            if (_osdWindow.NeedRefresh())
            {
              _needToClearScreen = true;
            }
            return;
          }
        }

        // route all unhandled actions to the dvd player
        g_Player.OnAction(action);
      }

      switch (action.wID)
      {
          // previous : play previous song from playlist
        case Action.ActionType.ACTION_PREV_ITEM:
          {
            //g_playlistPlayer.PlayPrevious();
          }
          break;

          // next : play next song from playlist
        case Action.ActionType.ACTION_NEXT_ITEM:
          {
            //g_playlistPlayer.PlayNext();
          }
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
        case Action.ActionType.ACTION_SHOW_GUI:
          {
            // switch back to the menu
            if ((g_Player.IsDVD) && (g_Player.IsDVDMenu))
            {
              Log.Info("GUIVideoFullScreen: Leaving the DVD screen is not permitted while in menu mode.");
              return;
            }
            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            GUIGraphicsContext.IsFullScreenVideo = false;
            GUIWindowManager.ShowPreviousWindow();
            if (_vmr9OSD != null)
            {
              _vmr9OSD.HideBitmap();
            }
            return;
          }
        case Action.ActionType.ACTION_AUTOCROP:
          {
            Log.Debug("ACTION_AUTOCROP");
            _showStatus = true;
            _timeStatusShowTime = (DateTime.Now.Ticks/10000);

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1,
                                            0, 0, null);
            IAutoCrop cropper = GUIGraphicsContext.autoCropper;
            if (cropper != null)
            {
              msg.Label = cropper.Crop();
              if (msg.Label == null)
              {
                msg.Label = "N/A";
              }
            }
            else
            {
              msg.Label = "N/A";
            }

            OnMessage(msg);
            break;
          }
        case Action.ActionType.ACTION_TOGGLE_AUTOCROP:
          {
            Log.Debug("ACTION_TOGGLE_AUTOCROP");
            _showStatus = true;
            _timeStatusShowTime = (DateTime.Now.Ticks/10000);
            IAutoCrop cropper = GUIGraphicsContext.autoCropper;

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1,
                                            0, 0, null);
            msg.Label = "N/A";

            if (cropper != null)
            {
              msg.Label = cropper.ToggleMode();
            }
            OnMessage(msg);
            break;
          }
        case Action.ActionType.ACTION_ASPECT_RATIO:
          {
            _showStatus = true;
            _timeStatusShowTime = (DateTime.Now.Ticks/10000);
            string status = "";

            Geometry.Type arMode = GUIGraphicsContext.ARType;

            bool foundMode = false;
            for (int i = 0; i < _allowedArModes.Count; i++)
            {
              if (_allowedArModes[i] == arMode)
              {
                arMode = _allowedArModes[(i + 1)%_allowedArModes.Count]; // select next allowed mode
                foundMode = true;
                break;
              }
            }
            if (!foundMode && _allowedArModes.Count > 0)
            {
              arMode = _allowedArModes[0];
            }

            GUIGraphicsContext.ARType = arMode;
            status = Util.Utils.GetAspectRatioLocalizedString(arMode);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1,
                                            0, 0, null);
            msg.Label = status;
            OnMessage(msg);

            SaveSettings();
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
        case Action.ActionType.ACTION_STEP_BACK:
          {
            if (g_Player.CanSeek)
            {
              if (g_Player.IsDVD && g_Player.Paused)
              {
                // Don't skip in paused DVD's
                _forbiddenTimer = DateTime.Now;
                RenderForbidden(true);
              }
              else
              {
                if (g_Player.Paused)
                {
                  g_Player.Pause();
                  ScreenStateChanged();
                  UpdateGUI();
                }

                _timeStatusShowTime = (DateTime.Now.Ticks/10000);
                _showStep = true;
                g_Player.SeekStep(false);
                string statusLine = g_Player.GetStepDescription();
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                                (int) Control.LABEL_ROW1, 0, 0, null);
                msg.Label = statusLine;
                OnMessage(msg);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
        case Action.ActionType.ACTION_STEP_FORWARD:
          {
            if (g_Player.CanSeek)
            {
              if (g_Player.IsDVD && g_Player.Paused)
              {
                // Don't skip in paused DVD's
                _forbiddenTimer = DateTime.Now;
                RenderForbidden(true);
              }
              else
              {
                if (g_Player.Paused)
                {
                  g_Player.Pause();
                  ScreenStateChanged();
                  UpdateGUI();
                }
                _timeStatusShowTime = (DateTime.Now.Ticks/10000);
                _showStep = true;
                g_Player.SeekStep(true);
                string statusLine = g_Player.GetStepDescription();
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                                (int) Control.LABEL_ROW1, 0, 0, null);
                msg.Label = statusLine;
                OnMessage(msg);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
        case Action.ActionType.ACTION_BIG_STEP_BACK:
          {
            if (g_Player.CanSeek)
            {
              if (g_Player.IsDVD && g_Player.Paused)
              {
                // Don't skip in paused DVD's
                _forbiddenTimer = DateTime.Now;
                RenderForbidden(true);
              }
              else
              {
                if (g_Player.Paused)
                {
                  g_Player.Pause();
                  ScreenStateChanged();
                  UpdateGUI();
                }

                double currentpos = g_Player.CurrentPosition;
                double duration = g_Player.Duration;
                double percent = (currentpos/duration)*100d;
                percent -= 10d;
                if (percent < 0)
                {
                  percent = 0;
                }
                g_Player.SeekAsolutePercentage((int) percent);
              }
            }
            return;
          }
          //break;

        case Action.ActionType.ACTION_MOVE_UP:
        case Action.ActionType.ACTION_BIG_STEP_FORWARD:
          {
            if (g_Player.CanSeek)
            {
              if (g_Player.IsDVD && g_Player.Paused)
              {
                // Don't skip in paused DVD's
                _forbiddenTimer = DateTime.Now;
                RenderForbidden(true);
              }
              else
              {
                if (g_Player.Paused)
                {
                  g_Player.Pause();
                  ScreenStateChanged();
                  UpdateGUI();
                }

                double currentpos = g_Player.CurrentPosition;
                double duration = g_Player.Duration;
                double percent = (currentpos/duration)*100d;
                percent += 10d;
                if (percent > 100d)
                {
                  percent = 100d;
                }
                g_Player.SeekAsolutePercentage((int) percent);
              }
            }
            return;
          }
          //break;

        case Action.ActionType.ACTION_SHOW_MPLAYER_OSD:
          //g_application.m_pPlayer.ToggleOSD();
          break;

        case Action.ActionType.ACTION_SHOW_OSD: // Show the OSD
          {
            m_dwOSDTimeOut = DateTime.Now;

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                            null);
            _osdWindow.OnMessage(msg); // Send an init msg to the OSD
            _isOsdVisible = true;
            GUIWindowManager.VisibleOsd = Window.WINDOW_OSD;
          }
          break;

        case Action.ActionType.ACTION_SHOW_SUBTITLES:
          {
            g_Player.EnableSubtitle = !g_Player.EnableSubtitle;
          }
          break;

        case Action.ActionType.ACTION_AUDIO_NEXT_LANGUAGE:
        case Action.ActionType.ACTION_NEXT_AUDIO:
          {
            if (g_Player.AudioStreams > 1)
            {
              _showStatus = true;
              _timeStatusShowTime = (DateTime.Now.Ticks/10000);
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int) Control.LABEL_ROW1, 0, 0, null);
              g_Player.SwitchToNextAudio();
              msg.Label = string.Format("{0} ({1}/{2})", g_Player.AudioLanguage(g_Player.CurrentAudioStream),
                                        g_Player.CurrentAudioStream + 1, g_Player.AudioStreams);
              OnMessage(msg);
              Log.Info("GUIVideoFullscreen: switched audio to {0}", msg.Label);
            }
          }
          break;

        case Action.ActionType.ACTION_NEXT_SUBTITLE:
          {
            if (g_Player.SubtitleStreams > 0)
            {
              _showStatus = true;
              _timeStatusShowTime = (DateTime.Now.Ticks/10000);
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int) Control.LABEL_ROW1, 0, 0, null);
              g_Player.SwitchToNextSubtitle();
              if (g_Player.EnableSubtitle)
              {
                msg.Label = string.Format("{0} ({1}/{2})", g_Player.SubtitleLanguage(g_Player.CurrentSubtitleStream),
                                          g_Player.CurrentSubtitleStream + 1, g_Player.SubtitleStreams);
              }
              else
              {
                msg.Label = GUILocalizeStrings.Get(519); // Subtitles off
              }
              OnMessage(msg);
              Log.Info("GUIVideoFullscreen: switched subtitle to {0}", msg.Label);
            }
            else
            {
              Log.Info("GUIVideoFullscreen toggle subtitle: no subtitle streams available!");
            }
          }
          break;

        case Action.ActionType.ACTION_STOP:
          {
            Log.Info("GUIVideoFullscreen:stop");
            g_Player.Stop();
            GUIWindowManager.ShowPreviousWindow();
          }
          break;

          // PAUSE action is handled globally in the Application class
        case Action.ActionType.ACTION_PAUSE:
          g_Player.Pause();
          ScreenStateChanged();
          UpdateGUI();
          if (g_Player.Paused)
          {
            if ((GUIGraphicsContext.Vmr9Active && VMR9Util.g_vmr9 != null))
            {
              VMR9Util.g_vmr9.SetRepaint();
              VMR9Util.g_vmr9.Repaint(); // repaint vmr9
            }
          }

          break;

        case Action.ActionType.ACTION_SUBTITLE_DELAY_MIN:
          //g_application.m_pPlayer.SubtitleOffset(false);
          break;
        case Action.ActionType.ACTION_SUBTITLE_DELAY_PLUS:
          //g_application.m_pPlayer.SubtitleOffset(true);
          break;
        case Action.ActionType.ACTION_AUDIO_DELAY_MIN:
          //g_application.m_pPlayer.AudioOffset(false);
          break;
        case Action.ActionType.ACTION_AUDIO_DELAY_PLUS:
          //g_application.m_pPlayer.AudioOffset(true);
          break;

        case Action.ActionType.ACTION_REWIND:
          {
            if (g_Player.Paused)
            {
              if (g_Player.CanSeek && !g_Player.IsDVD)
              {
                g_Player.Pause();
                ScreenStateChanged();
                UpdateGUI();
                double dPos = g_Player.CurrentPosition;
                if (dPos > 1)
                {
                  Log.Debug("GUIVideoFullscreen.Rewind - skipping");
                  g_Player.SeekAbsolute(dPos - 0.25d);
                }
              }
              else
              {
                // Don't skip in paused DVD's
                _forbiddenTimer = DateTime.Now;
                RenderForbidden(true);
              }
            }
            else
            {
              g_Player.Speed = Util.Utils.GetNextRewindSpeed(g_Player.Speed);
            }
          }
          break;

        case Action.ActionType.ACTION_FORWARD:
          {
            if (g_Player.Paused)
            {
              if (g_Player.CanSeek && !g_Player.IsDVD)
              {
                g_Player.Pause();
                ScreenStateChanged();
                UpdateGUI();
                double dPos = g_Player.CurrentPosition;
                if (g_Player.Duration - dPos > 1)
                {
                  Log.Debug("GUIVideoFullscreen.Forward - skipping");
                  g_Player.SeekAbsolute(dPos + 0.25d);
                }
              }
              else
              {
                // Don't skip in paused DVD's
                _forbiddenTimer = DateTime.Now;
                RenderForbidden(true);
              }
            }
            else
            {
              g_Player.Speed = Util.Utils.GetNextForwardSpeed(g_Player.Speed);
            }
          }
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
          {
            char chKey = (char) action.m_key.KeyChar;
            if (chKey >= '0' && chKey <= '9') //Make sure it's only for the remote
            {
              if (g_Player.CanSeek)
              {
                if (g_Player.IsDVD && g_Player.Paused)
                {
                  // Don't skip in paused DVD's
                  _forbiddenTimer = DateTime.Now;
                  RenderForbidden(true);
                }
                else
                {
                  ChangetheTimeCode(chKey);
                }
              }
            }
          }
          break;

        case Action.ActionType.ACTION_SMALL_STEP_BACK:
          {
            if (g_Player.CanSeek)
            {
              // seek back 5 sec
              double dPos = g_Player.CurrentPosition;
              if (dPos > 5)
              {
                Log.Debug("GUIVideoFullscreen.SMALL_STEP_BACK - skipping");
                g_Player.SeekAbsolute(dPos - 5.0d);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          {
            g_Player.StepNow();
            g_Player.Speed = 1;
            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
          }
          break;

        case Action.ActionType.ACTION_CONTEXT_MENU:
          ShowContextMenu();
          break;
      }

      base.OnAction(action);
    }

    private bool OnOsdMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          m_dwOSDTimeOut = DateTime.Now;
          break;
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
          _isOsdVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          return true;
      }
      bool result = _osdWindow.OnMessage(message); // route messages to OSD window
      if (_osdWindow.NeedRefresh())
      {
        _needToClearScreen = true;
      }
      return result;
    }

    public override bool OnMessage(GUIMessage message)
    {
      _needToClearScreen = true;

      #region case GUI_MSG_NOTIFY_TV_PROGRAM

      //if (message.Message == GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM)
      //{
      //  dialogNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      //  TVProgram notify = message.Object as TVProgram;
      //  if (notify == null) return true;
      //  dialogNotify.SetHeading(1016);
      //  dialogNotify.SetText(String.Format("{0}\n{1}", notify.Title, notify.Description));
      //  string strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, notify.Channel);
      //  dialogNotify.SetImage(strLogo);
      //  dialogNotify.TimeOut = _notifyTVTimeout;
      //  NotifyDialogVisible = true;
      //  if ( _playNotifyBeep )
      //    MediaPortal.Util.Utils.PlaySound("notify.wav", false, true);
      //  dialogNotify.DoModal(GetID);
      //  NotifyDialogVisible = false;
      //}

      #endregion

      if (_isOsdVisible)
      {
        return OnOsdMessage(message);
      }

      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            _osdWindow = (GUIVideoOSD) GUIWindowManager.GetWindow((int) Window.WINDOW_OSD);

            HideControl(GetID, (int) Control.LABEL_ROW1);
            HideControl(GetID, (int) Control.LABEL_ROW2);
            HideControl(GetID, (int) Control.LABEL_ROW3);
            HideControl(GetID, (int) Control.BLUE_BAR);

            LoadSettings();
            GUIWindowManager.IsOsdVisible = false;
            _isOsdVisible = false;
            _showStep = false;
            _showStatus = false;
            _showTime = false;
            _showSkipBar = false;

            _timeStamp = "";
            _timeCodePosition = 0;
            _timeStatusShowTime = 0;
            _updateTimer = DateTime.Now;
            _vmr7UpdateTimer = DateTime.Now;
            _IsDialogVisible = false;
            _needToClearScreen = false;
            _isVolumeVisible = false;
            _isForbiddenVisible = false;
            NotifyDialogVisible = false;
            _notifyTVTimeout = 15;
            _playNotifyBeep = true;
            _volumeTimer = DateTime.MinValue;
            _forbiddenTimer = DateTime.MinValue;
            _vmr9OSD = new VMR9OSD();

            screenState = new FullScreenState();
            NotifyDialogVisible = false;

            GUIGraphicsContext.IsFullScreenVideo = true;
            ScreenStateChanged();
            _needToClearScreen = true;
            UpdateGUI();
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
            RenderVolume(false);
            RenderForbidden(false);
            if (!screenState.Paused)
            {
              for (int i = (int) Control.PANEL1; i < (int) Control.PANEL2; ++i)
              {
                HideControl(GetID, i);
              }
            }


            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            lock (this)
            {
              if (_isOsdVisible)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0,
                                                GetID, 0, null);
                _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
              }
              _isOsdVisible = false;
              GUIWindowManager.IsOsdVisible = false;

              if (VMR7Util.g_vmr7 != null)
              {
                VMR7Util.g_vmr7.SaveBitmap(null, false, false, 0.8f);
              }
              /*if (VMR9Util.g_vmr9!=null)
              {	
                VMR9Util.g_vmr9.SaveBitmap(null,false,false,0.8f);
              }*/
              base.OnMessage(message);

              //            if (m_form!=null) 
              //            {
              //              m_form.Close();
              //              m_form.Dispose();
              //            }
              //            m_form=null;
              GUILayerManager.UnRegisterLayer(this);
            }
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;

        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
          if (_isOsdVisible)
          {
            return true;
          }
          if (message.SenderControlId != (int) Window.WINDOW_FULLSCREEN_VIDEO)
          {
            return true;
          }
          break;
      }

      return base.OnMessage(message);
    }

    private void ShowContextMenu()
    {
      if (dlg == null)
      {
        dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      }
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

      dlg.AddLocalizedString(941); // Change aspect ratio

      // Audio stream selection, show only when more than one streams exists
      if (g_Player.AudioStreams > 1)
      {
        dlg.AddLocalizedString(492);
      }

      eAudioDualMonoMode dualMonoMode = g_Player.GetAudioDualMonoMode();
      if (dualMonoMode != eAudioDualMonoMode.UNSUPPORTED)
      {
        dlg.AddLocalizedString(200059); // Audio dual mono mode menu
      }

      // SubTitle stream selection, show only when there exists any streams,
      //    dialog shows then the streams and an item to disable them
      if (g_Player.SubtitleStreams > 0)
      {
        dlg.AddLocalizedString(462);
      }

      dlg.AddLocalizedString(970); // Previous window
      if (g_Player.IsDVD)
      {
        dlg.AddLocalizedString(974); // Root menu
        dlg.AddLocalizedString(975); // Previous chapter
        dlg.AddLocalizedString(976); // Next chapter
      }
      else if (g_Player.HasChapters) // For video files with chapters
      {
        dlg.AddLocalizedString(976); // Next chapter
        dlg.AddLocalizedString(975); // Previous chapter
      }

      if (g_Player.IsVideo)
      {
        dlg.AddLocalizedString(1064); // Bookmarks
      }

      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
          // Add audio stream selection to be able to switch audio streams in .ts recordings
        case 492:
          ShowAudioStreamsMenu();
          break;
        case 200059:
          ShowAudioDualMonoModeMenu(dualMonoMode);
          break;
        case 462:
          ShowSubtitleStreamsMenu();
          break;
        case 1064:
          ShowBookmarksMenu();
          break;
        case 974: // DVD root menu
          Action actionMenu = new Action(Action.ActionType.ACTION_DVD_MENU, 0, 0);
          GUIGraphicsContext.OnAction(actionMenu);
          break;
        case 975: // DVD previous chapter
          Action actionPrevChapter = new Action(Action.ActionType.ACTION_PREV_CHAPTER, 0, 0);
          GUIGraphicsContext.OnAction(actionPrevChapter);
          break;
        case 976: // DVD next chapter
          Action actionNextChapter = new Action(Action.ActionType.ACTION_NEXT_CHAPTER, 0, 0);
          GUIGraphicsContext.OnAction(actionNextChapter);
          break;
        case 941: // Change aspect ratio
          ShowAspectRatioMenu();
          break;

        case 970:
          // switch back to MyMovies window
          _isOsdVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          GUIGraphicsContext.IsFullScreenVideo = false;
          GUIWindowManager.ShowPreviousWindow();
          break;
      }
    }

    // Add audio stream selection to be able to switch audio streams in .ts recordings
    private void ShowAudioStreamsMenu()
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(492); // Audio Streams

      // get the number of audiostreams in the current movie
      int count = g_Player.AudioStreams;
      // cycle through each audiostream and add it to our list control
      for (int i = 0; i < count; i++)
      {
        string audioType = g_Player.AudioType(i);
        if (audioType == Strings.Unknown)
        {
          dlg.Add(g_Player.AudioLanguage(i));
        }
        else
        {
          dlg.Add(String.Format("{0}:{1}", audioType, g_Player.AudioLanguage(i)));
        }
      }

      // select/focus the audiostream, which is active atm
      dlg.SelectedLabel = g_Player.CurrentAudioStream;

      // show dialog and wait for result
      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      if (dlg.SelectedLabel != g_Player.CurrentAudioStream)
      {
        g_Player.CurrentAudioStream = dlg.SelectedLabel;
      }
    }

    private void ShowAudioDualMonoModeMenu(eAudioDualMonoMode dualMonoMode)
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(200059); // audio dual mono mode 

      dlg.AddLocalizedString(200060); // stereo 
      dlg.AddLocalizedString(200061); //Left channel to the left and right speakers
      dlg.AddLocalizedString(200062); //Right channel to the left and right speakers
      dlg.AddLocalizedString(200063); //Mix both

      dlg.SelectedLabel = (int) dualMonoMode;

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      g_Player.SetAudioDualMonoMode((eAudioDualMonoMode) dlg.SelectedLabel);
    }

    private void ShowSubtitleStreamsMenu()
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(462); // SubTitle Streams

      dlg.AddLocalizedString(519); // disable Subtitles

      // get the number of subtitles in the current movie
      int count = g_Player.SubtitleStreams;
      // cycle through each subtitle and add it to our list control
      for (int i = 0; i < count; ++i)
      {
        // remove (English) in: "English (English)", should be done by gplayer
        string strLang = g_Player.SubtitleLanguage(i);
        int ipos = strLang.IndexOf("(");
        if (ipos > 0)
        {
          strLang = strLang.Substring(0, ipos);
        }

        dlg.Add(strLang);
      }

      // select/focus the subtitle, which is active atm
      if (g_Player.EnableSubtitle)
      {
        dlg.SelectedLabel = g_Player.CurrentSubtitleStream + 1;
      }
      else
      {
        dlg.SelectedLabel = 0;
      }

      // show dialog and wait for result
      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      if (dlg.SelectedLabel == 0)
      {
        g_Player.EnableSubtitle = false;
      }
      else
      {
        if (dlg.SelectedLabel != g_Player.CurrentSubtitleStream + 1)
        {
          g_Player.CurrentSubtitleStream = dlg.SelectedLabel - 1;
        }
        g_Player.EnableSubtitle = true;
      }
    }

    private void ShowAspectRatioMenu()
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio

      if (_allowedArModes.Contains(Geometry.Type.Stretch))
      {
        dlg.AddLocalizedString(942); // Stretch
      }
      if (_allowedArModes.Contains(Geometry.Type.Normal))
      {
        dlg.AddLocalizedString(943); // Normal
      }
      if (_allowedArModes.Contains(Geometry.Type.Original))
      {
        dlg.AddLocalizedString(944); // Original
      }
      if (_allowedArModes.Contains(Geometry.Type.LetterBox43))
      {
        dlg.AddLocalizedString(945); // Letterbox
      }
      if (_allowedArModes.Contains(Geometry.Type.NonLinearStretch))
      {
        dlg.AddLocalizedString(946); // Smart stretch
      }
      if (_allowedArModes.Contains(Geometry.Type.Zoom))
      {
        dlg.AddLocalizedString(947); // Zoom
      }
      if (_allowedArModes.Contains(Geometry.Type.Zoom14to9))
      {
        dlg.AddLocalizedString(1190); //14:9
      }

      // set the focus to currently used mode
      dlg.SelectedLabel = dlg.IndexOfItem(Util.Utils.GetAspectRatioLocalizedString(GUIGraphicsContext.ARType));
      // show dialog and wait for result
      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      _timeStatusShowTime = (DateTime.Now.Ticks/10000);

      string strStatus = "";

      GUIGraphicsContext.ARType = Util.Utils.GetAspectRatioByLangID(dlg.SelectedId);
      strStatus = GUILocalizeStrings.Get(dlg.SelectedId);
      SaveSettings();

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1, 0, 0,
                                      null);
      msg.Label = strStatus;
      OnMessage(msg);
    }

    public void ShowBookmarksMenu()
    {
      if (dlg == null)
      {
        dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      }
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(1064); // Bookmarks

      // load the stored bookmarks
      ArrayList bookmarks = new ArrayList();
      VideoDatabase.GetBookMarksForMovie(g_Player.CurrentFile, ref bookmarks);
      List<double> bookmarkList = new List<double>();
      for (int i = 0; i < bookmarks.Count; i++)
      {
        bookmarkList.Add((double) bookmarks[i]);
      }
      bookmarkList.Sort();

      dlg.AddLocalizedString(294); // create Bookmark
      if (bookmarkList.Count > 0)
      {
        dlg.AddLocalizedString(296); // clear Bookmarks
      }

      // align the time right
      for (int i = 0; i < bookmarkList.Count; ++i)
      {
        GUIListItem item = new GUIListItem();
        item.Label = GUILocalizeStrings.Get(1065); // Jump to
        item.Label2 = Util.Utils.SecondsToHMSString((int) bookmarkList[i]);

        dlg.Add(item);
      }
      // show only the time on the left
      //for (int i = 0; i < bookmarkList.Count; ++i)
      //  dlg.Add(Util.Utils.SecondsToHMSString((int)bookmarkList[i]));

      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }

      if (dlg.SelectedLabel == 0)
      {
        // get the current playing time position
        double dCurTime = g_Player.CurrentPosition;
        // add the current timestamp
        VideoDatabase.AddBookMarkToMovie(g_Player.CurrentFile, (float) dCurTime);
      }
      else if (dlg.SelectedLabel == 1)
      {
        // empty the bookmarks table for this movie
        VideoDatabase.ClearBookMarksOfMovie(g_Player.CurrentFile);
      }
      else
      {
        // get selected bookmark
        // dlg[0] = create, dlg[1] = clearAll --> dlg[2] = bookmark[0]
        int selectedBookmarkIndex = dlg.SelectedLabel - 2;

        // set mplayers play position
        g_Player.SeekAbsolute(bookmarkList[selectedBookmarkIndex]);
      }
    }

    public bool ScreenStateChanged()
    {
      bool updateGUI = false;
      if (NotifyDialogVisible != screenState.NotifyDialogVisible)
      {
        screenState.NotifyDialogVisible = NotifyDialogVisible;
        updateGUI = true;
      }


      if (g_Player.Speed != screenState.Speed)
      {
        screenState.Speed = g_Player.Speed;
        updateGUI = true;
      }
      if (g_Player.Paused != screenState.Paused)
      {
        screenState.Paused = g_Player.Paused;
        updateGUI = true;
      }
      if (_isOsdVisible != screenState.OsdVisible)
      {
        screenState.OsdVisible = _isOsdVisible;
        updateGUI = true;
      }
      if (_isOsdVisible && _osdWindow.NeedRefresh())
      {
        _needToClearScreen = true;
      }

      if (_IsDialogVisible != screenState.ContextMenuVisible)
      {
        screenState.ContextMenuVisible = _IsDialogVisible;
        updateGUI = true;
      }

      bool bStart, bEnd;
      int step = g_Player.GetSeekStep(out bStart, out bEnd);
      if (step != screenState.SeekStep)
      {
        if (step != 0)
        {
          _showStep = true;
        }
        else
        {
          _showStep = false;
        }
        screenState.SeekStep = step;
        updateGUI = true;
      }
      if (_showStatus != screenState.ShowStatusLine)
      {
        screenState.ShowStatusLine = _showStatus;
        updateGUI = true;
      }
      if (_showSkipBar != screenState.ShowSkipBar)
      {
        screenState.ShowSkipBar = _showSkipBar;
        updateGUI = true;
      }
      if (_showTime != screenState.ShowTime)
      {
        screenState.ShowTime = _showTime;
        updateGUI = true;
      }
      if (_isVolumeVisible != screenState.volumeVisible)
      {
        screenState.volumeVisible = _isVolumeVisible;
        updateGUI = true;
        _volumeTimer = DateTime.Now;
      }
      if (_isForbiddenVisible != screenState.forbiddenVisible)
      {
        screenState.forbiddenVisible = _isForbiddenVisible;
        updateGUI = true;
        _forbiddenTimer = DateTime.Now;
      }
      if (updateGUI)
      {
        _needToClearScreen = true;
      }
      return updateGUI;
    }

    private void UpdateGUI()
    {
      if ((_showStep || _showSkipBar || (!_isOsdVisible && g_Player.Speed != 1) || (!_isOsdVisible && g_Player.Paused)))
      {
        if (!_isOsdVisible)
        {
          for (int i = (int) Control.PANEL1; i < (int) Control.PANEL2; ++i)
          {
            ShowControl(GetID, i);
          }
          ShowControl(GetID, (int) Control.OSD_TIMEINFO);
          ShowControl(GetID, (int) Control.OSD_VIDEOPROGRESS);
        }
        else
        {
          for (int i = (int) Control.PANEL1; i < (int) Control.PANEL2; ++i)
          {
            HideControl(GetID, i);
          }
          HideControl(GetID, (int) Control.OSD_TIMEINFO);
          HideControl(GetID, (int) Control.OSD_VIDEOPROGRESS);
        }
      }
      else
      {
        for (int i = (int) Control.PANEL1; i < (int) Control.PANEL2; ++i)
        {
          HideControl(GetID, i);
        }
        HideControl(GetID, (int) Control.OSD_TIMEINFO);
        HideControl(GetID, (int) Control.OSD_VIDEOPROGRESS);
      }


      if (g_Player.Paused && !_showStep && !_showTime && !_showStatus && !_isOsdVisible && g_Player.Speed == 1)
      {
        ShowControl(GetID, (int) Control.IMG_PAUSE);
      }
      else
      {
        HideControl(GetID, (int) Control.IMG_PAUSE);
      }

      int iSpeed = g_Player.Speed;
      HideControl(GetID, (int) Control.IMG_2X);
      HideControl(GetID, (int) Control.IMG_4X);
      HideControl(GetID, (int) Control.IMG_8X);
      HideControl(GetID, (int) Control.IMG_16X);
      HideControl(GetID, (int) Control.IMG_32X);
      HideControl(GetID, (int) Control.IMG_MIN2X);
      HideControl(GetID, (int) Control.IMG_MIN4X);
      HideControl(GetID, (int) Control.IMG_MIN8X);
      HideControl(GetID, (int) Control.IMG_MIN16X);
      HideControl(GetID, (int) Control.IMG_MIN32X);

      if (!_showStep)
      {
        switch (iSpeed)
        {
          case 2:
            ShowControl(GetID, (int) Control.IMG_2X);
            break;
          case 4:
            ShowControl(GetID, (int) Control.IMG_4X);
            break;
          case 8:
            ShowControl(GetID, (int) Control.IMG_8X);
            break;
          case 16:
            ShowControl(GetID, (int) Control.IMG_16X);
            break;
          case 32:
            ShowControl(GetID, (int) Control.IMG_32X);
            break;
          case -2:
            ShowControl(GetID, (int) Control.IMG_MIN2X);
            break;
          case -4:
            ShowControl(GetID, (int) Control.IMG_MIN4X);
            break;
          case -8:
            ShowControl(GetID, (int) Control.IMG_MIN8X);
            break;
          case -16:
            ShowControl(GetID, (int) Control.IMG_MIN16X);
            break;
          case -32:
            ShowControl(GetID, (int) Control.IMG_MIN32X);
            break;
        }
      }

      HideControl(GetID, (int) Control.LABEL_ROW1);
      HideControl(GetID, (int) Control.LABEL_ROW2);
      HideControl(GetID, (int) Control.LABEL_ROW3);
      HideControl(GetID, (int) Control.BLUE_BAR);
      if (screenState.SeekStep != 0)
      {
        ShowControl(GetID, (int) Control.BLUE_BAR);
        ShowControl(GetID, (int) Control.LABEL_ROW1);
      }
      if (_showStatus)
      {
        ShowControl(GetID, (int) Control.BLUE_BAR);
        ShowControl(GetID, (int) Control.LABEL_ROW1);
      }
      if (_showSkipBar && !g_Player.Paused) // If paused, this will already be shown, including LABEL_ROW1
      {
        ShowControl(GetID, (int) Control.BLUE_BAR);
      }
      if (_showTime)
      {
        ShowControl(GetID, (int) Control.BLUE_BAR);
        ShowControl(GetID, (int) Control.LABEL_ROW1);
      }

      RenderVolume(_isVolumeVisible);
      RenderForbidden(_isForbiddenVisible);
    }

    private void CheckTimeOuts()
    {
      if (_vmr9OSD != null)
      {
        _vmr9OSD.CheckTimeOuts();
      }

      if (_isVolumeVisible)
      {
        TimeSpan ts = DateTime.Now - _volumeTimer;
        if (ts.TotalSeconds >= 3)
        {
          RenderVolume(false);
        }
      }
      if (_isForbiddenVisible)
      {
        TimeSpan ts = DateTime.Now - _forbiddenTimer;
        if (ts.TotalSeconds >= 1)
        {
          RenderForbidden(false);
        }
      }
      if (_showStatus || _showStep || _showSkipBar)
      {
        long lTimeSpan = ((DateTime.Now.Ticks/10000) - _timeStatusShowTime);
        if (lTimeSpan >= 3000)
        {
          _showStep = false;
          _showStatus = false;
          _showSkipBar = false;
        }
      }
      if (_showTime)
      {
        TimeSpan lTimeSpan = DateTime.Now - m_dwTimeCodeTimeout;
        if (lTimeSpan.TotalMilliseconds >= 2500)
        {
          _showTime = false;
          _timeCodePosition = 0;
          _timeStamp = "";
          return;
        }
      }

      // OSD Timeout?
      if (_isOsdVisible && m_iMaxTimeOSDOnscreen > 0)
      {
        TimeSpan ts = DateTime.Now - m_dwOSDTimeOut;
        if (ts.TotalMilliseconds > m_iMaxTimeOSDOnscreen)
        {
          //yes, then remove osd offscreen
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
          _isOsdVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          msg = null;
        }
      }
    }

    public override void Process()
    {
      CheckTimeOuts();

      if (ScreenStateChanged())
      {
        UpdateGUI();
        _needToClearScreen = true;
      }
      if (!g_Player.Playing)
      {
        if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC ||
            playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
        {
          // Only stay in fullscreen if there are (still) items to play
          PlayList current = playlistPlayer.GetPlaylist(playlistPlayer.CurrentPlaylistType);
          if (current.Count > 0 && !current.AllPlayed())
          {
            return;
          }
        }
        _isOsdVisible = false;
        GUIWindowManager.IsOsdVisible = false;
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
    }

    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.Vmr9Active || GUIWindowManager.IsRouted)
      {
        base.Render(timePassed);
        if (_isOsdVisible)
        {
          _osdWindow.Render(timePassed);
        }
      }
      else
      {
        if (screenState.ContextMenuVisible ||
            screenState.OsdVisible ||
            screenState.Paused ||
            screenState.ShowStatusLine ||
            screenState.ShowSkipBar ||
            screenState.ShowTime || _needToClearScreen ||
            g_Player.Speed != 1)
        {
          TimeSpan ts = DateTime.Now - _vmr7UpdateTimer;
          if ((ts.TotalMilliseconds >= 5000) || _needToClearScreen)
          {
            _needToClearScreen = false;
            if (VMR7Util.g_vmr7 != null)
            {
              using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width, GUIGraphicsContext.Height))
              {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                  GUIGraphicsContext.graphics = g;
                  base.Render(timePassed);
                  RenderForm(timePassed);
                  GUIGraphicsContext.graphics = null;
                  screenState.wasVMRBitmapVisible = true;
                  VMR7Util.g_vmr7.SaveBitmap(bmp, true, true, 0.8f);
                }
              }
            }
            _vmr7UpdateTimer = DateTime.Now;
          }
        }
        else
        {
          if (screenState.wasVMRBitmapVisible)
          {
            screenState.wasVMRBitmapVisible = false;
            if (VMR7Util.g_vmr7 != null)
            {
              VMR7Util.g_vmr7.SaveBitmap(null, false, false, 0.8f);
            }
          }
        }
      }
    }

    private bool OSDVisible()
    {
      return _isOsdVisible;
    }

    private void ChangetheTimeCode(char chKey)
    {
      _showTime = true;
      m_dwTimeCodeTimeout = DateTime.Now;
      if (_timeCodePosition <= 4)
      {
        //00:12
        _timeStamp += chKey;
        _timeCodePosition++;
        if (_timeCodePosition == 2)
        {
          _timeStamp += ":";
          _timeCodePosition++;
        }
      }
      if (_timeCodePosition > 4)
      {
        int itotal, ih, im, lis = 0;
        ih = (_timeStamp[0] - (char) '0')*10;
        ih += (_timeStamp[1] - (char) '0');
        im = (_timeStamp[3] - (char) '0')*10;
        im += (_timeStamp[4] - (char) '0');
        im *= 60;
        ih *= 3600;
        itotal = ih + im + lis;
        if (itotal < g_Player.Duration)
        {
          Log.Debug("GUIVideoFullscreen.ChangetheTimeCode - skipping");
          g_Player.SeekAbsolute((double) itotal);
        }
        _timeStamp = "";
        _timeCodePosition = 0;
        _showTime = false;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1, 0, 0,
                                      null);
      msg.Label = _timeStamp;
      OnMessage(msg);
    }

    public void RenderForm(float timePassed)
    {
      if (!g_Player.Playing)
      {
        return;
      }
      else
      {
        if (_needToClearScreen)
        {
          _needToClearScreen = false;
          GUIGraphicsContext.graphics.Clear(Color.Black);
        }
        base.Render(timePassed);
        if (_isOsdVisible)
        {
          _osdWindow.Render(timePassed);
        }
      }
    }

    private void RenderVolume(bool show)
    {
      if (imgVolumeBar == null)
      {
        return;
      }

      if (!show)
      {
        _isVolumeVisible = false;
        imgVolumeBar.Visible = false;
        imgVolumeMuteIcon.Visible = false;
        return;
      }
      else
      {
        if (VolumeHandler.Instance.IsMuted)
        {
          imgVolumeMuteIcon.Visible = true;
          imgVolumeBar.Image1 = 1;
          imgVolumeBar.Current = 0;
        }
        else
        {
          imgVolumeBar.Maximum = VolumeHandler.Instance.StepMax;
          imgVolumeBar.Current = VolumeHandler.Instance.Step;
          imgVolumeMuteIcon.Visible = false;
          imgVolumeBar.Image1 = 2;
          imgVolumeBar.Image2 = 1;
        }
        imgVolumeBar.Visible = true;
      }
    }

    private void RenderForbidden(bool show)
    {
      if (imgActionForbiddenIcon == null)
      {
        return;
      }
      _isForbiddenVisible = show;
      imgActionForbiddenIcon.Visible = show;
    }

    #region helper functions

    private void HideControl(int senderId, int controlId)
    {
      GUIControl cntl = base.GetControl(controlId);
      if (cntl != null)
      {
        cntl.Visible = false;
      }
      cntl = null;
    }

    private void ShowControl(int senderId, int controlId)
    {
      GUIControl cntl = base.GetControl(controlId);
      if (cntl != null)
      {
        cntl.Visible = true;
      }
      cntl = null;
    }

    public override int GetFocusControlId()
    {
      if (_isOsdVisible)
      {
        return _osdWindow.GetFocusControlId();
      }

      return base.GetFocusControlId();
    }

    public override GUIControl GetControl(int iControlId)
    {
      if (_isOsdVisible)
      {
        return _osdWindow.GetControl(iControlId);
      }

      return base.GetControl(iControlId);
    }

    private double GetPercentage(float x, float y, GUIControl cntl)
    {
      if (y < (cntl.YPosition + cntl.Height) && y > cntl.YPosition)
      {
        if (x > (cntl.XPosition + SKIPBAR_PADDING) && x < (cntl.XPosition + cntl.Width - SKIPBAR_PADDING))
        {
          return (x - cntl.XPosition - SKIPBAR_PADDING)/(cntl.Width - 2*SKIPBAR_PADDING);
        }
      }
      return -1;
    }

    #endregion

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
      base.Render(timePassed);
    }

    #endregion
  }
}