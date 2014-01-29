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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Video.Database;
using MediaPortal.Player.Subtitles;
using Action = MediaPortal.GUI.Library.Action;
using MediaPortal.Player.PostProcessing;
using FFDShow;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for GUIVideoFullscreen.
  /// </summary>
  public class GUIVideoFullscreen : GUIInternalWindow, IRenderLayer
  {
    private class FullScreenState
    {
      public int SeekStep = 1;
      public int Speed = 1;
      public bool OsdVisible = false;
      public bool PauseOsdVisible = false;
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
    private bool _isPauseOsdVisible = false;
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
    private bool _IsClosingDialog = false;
    private bool _needToClearScreen = false;
    private bool _isVolumeVisible = false;
    private bool _isForbiddenVisible = false;
    private GUIDialogMenu dlg;
    private GUIVideoOSD _osdWindow = null;
    private bool NotifyDialogVisible = false;
    private DateTime _volumeTimer = DateTime.MinValue;
    private DateTime _forbiddenTimer = DateTime.MinValue;
    private PlayListPlayer playlistPlayer;
    private const int SKIPBAR_PADDING = 10;
    private List<Geometry.Type> _allowedArModes = new List<Geometry.Type>();
    private FullScreenState screenState = new FullScreenState();
    private bool _immediateSeekIsRelative = true;
    private int _immediateSeekValue = 10;
    private bool _settingsLoaded;
    private bool _usemoviecodects = false;

    public GUIVideoFullscreen()
    {
      GetID = (int)Window.WINDOW_FULLSCREEN_VIDEO;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\videoFullScreen.xml"));
      GetID = (int)Window.WINDOW_FULLSCREEN_VIDEO;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        _immediateSeekIsRelative = xmlreader.GetValueAsBool("movieplayer", "immediateskipstepsisrelative", true);
        _immediateSeekValue = xmlreader.GetValueAsInt("movieplayer", "immediateskipstepsize", 10);
        _usemoviecodects = xmlreader.GetValueAsBool("movieplayer", "usemoviecodects", false);
      }

      SettingsLoaded = false;

      g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
      g_Player.PlayBackChanged += new g_Player.ChangedHandler(g_Player_PlayBackChanged);

      return bResult;
    }

    #region settings serialisation

    private void LoadSettings()
    {
      string key1 = "movieplayer";
      string key2 = "movies";

      if (g_Player.IsDVD)
      {
        key1 = "dvdplayer";
        key2 = "dvdplayer";
      }
      if (g_Player.IsTVRecording || g_Player.CurrentFile.EndsWith(".ts"))
      {
        key1 = "mytv";
        key2 = "mytv";
      }
      if (_usemoviecodects && g_Player.CurrentFile.EndsWith(".ts"))
      {
        key1 = "movieplayer";
        key2 = "movies";
      }

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        m_iMaxTimeOSDOnscreen = 1000 * xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 5);
        bool BDInternalMenu = xmlreader.GetValueAsBool("bdplayer", "useInternalBDPlayer", true);

        if (BDInternalMenu && g_Player.CurrentFile.EndsWith(".bdmv"))
        {
          key1 = "bdplayerAR";
          key2 = "bdplayer";
        }

        string aspectRatioText = xmlreader.GetValueAsString(key1, "defaultar", "Normal");
        GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(aspectRatioText);

        //Log.Debug("isTV: {0}, isTimeShifting:{1}, isTVRecording:{2}, CurrentFile:<{3}>", g_Player.IsTV, g_Player.IsTimeShifting, g_Player.IsTVRecording, g_Player.CurrentFile);
        if (!String.IsNullOrEmpty(key2))
        {
          Log.Debug("Loading AR modes from \"{0}\" section...", key2);
          if (xmlreader.GetValueAsBool(key2, "allowarzoom", true))
          {
            _allowedArModes.Add(Geometry.Type.Zoom);
          }
          if (xmlreader.GetValueAsBool(key2, "allowarstretch", true))
          {
            _allowedArModes.Add(Geometry.Type.Stretch);
          }
          if (xmlreader.GetValueAsBool(key2, "allowarnormal", true))
          {
            _allowedArModes.Add(Geometry.Type.Normal);
          }
          if (xmlreader.GetValueAsBool(key2, "allowaroriginal", true))
          {
            _allowedArModes.Add(Geometry.Type.Original);
          }
          if (xmlreader.GetValueAsBool(key2, "allowarletterbox", true))
          {
            _allowedArModes.Add(Geometry.Type.LetterBox43);
          }
          if (xmlreader.GetValueAsBool(key2, "allowarnonlinear", true))
          {
            _allowedArModes.Add(Geometry.Type.NonLinearStretch);
          }
          if (xmlreader.GetValueAsBool(key2, "allowarzoom149", true))
          {
            _allowedArModes.Add(Geometry.Type.Zoom14to9);
          }
        }
      }

      SettingsLoaded = true;
    }

    #endregion

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
          int x = (int)action.fAmount1;
          int y = (int)action.fAmount2;
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
        if (action.wID != Action.ActionType.ACTION_KEY_PRESSED && action.wID != Action.ActionType.ACTION_PAUSE &&
            ActionTranslator.GetAction((int)Window.WINDOW_OSD, action.m_key, ref newAction))
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
          GUIControl cntl = base.GetControl((int)Control.OSD_VIDEOPROGRESS);
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
                g_Player.SeekAbsolute(g_Player.Duration * percentage);
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
        int y = (int)action.fAmount2;
        if (y > GUIGraphicsContext.Height - 100)
        {
          m_dwOSDTimeOut = DateTime.Now;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _osdWindow.OnMessage(msg); // Send an init msg to the OSD
          _isOsdVisible = true;
          _showSkipBar = false;
          GUIWindowManager.VisibleOsd = Window.WINDOW_OSD;
          GUIWindowManager.IsOsdVisible = true;
        }
        else if (y < 50)
        {
          _showSkipBar = true;
          _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
        }
        else
        {
          _showSkipBar = false;
        }
      }

      if (g_Player.IsDVD)
      {
        Action newAction = new Action();
        if (ActionTranslator.GetAction((int)Window.WINDOW_DVD, action.m_key, ref newAction))
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
        if (g_Player.OnAction(action))
          return;
      }

      switch (action.wID)
      {
          // previous : play previous song from playlist or previous item from MyPictures
        case Action.ActionType.ACTION_PREV_CHAPTER:
        case Action.ActionType.ACTION_PREV_ITEM:
          {
            if (g_Player.IsPicture)
            {
              GUISlideShow._slideDirection = -1;
              g_Player.Stop();
            }
            else
            {
              //g_playlistPlayer.PlayNext();
            }
          }
          break;

          // next : play next song from playlist or next item from MyPictures
        case Action.ActionType.ACTION_NEXT_CHAPTER:
        case Action.ActionType.ACTION_NEXT_ITEM:
          {
            if (g_Player.IsPicture)
            {
              GUISlideShow._slideDirection = 1;
              g_Player.Stop();
            }
            else
            {
              //g_playlistPlayer.PlayNext();
            }
          }
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
        case Action.ActionType.ACTION_SHOW_GUI:
          {
            // Stop Video for MyPictures when going to home
            if (g_Player.IsPicture)
            {
              GUISlideShow._slideDirection = 0;
              g_Player.Stop();
            }
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
            return;
          }
        case Action.ActionType.ACTION_AUTOCROP:
          {
            Log.Debug("ACTION_AUTOCROP");
            _showStatus = true;
            _timeStatusShowTime = (DateTime.Now.Ticks / 10000);

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1,
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
            _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
            IAutoCrop cropper = GUIGraphicsContext.autoCropper;

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1,
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
            _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
            string status = "";

            Geometry.Type arMode = GUIGraphicsContext.ARType;

            bool foundMode = false;

            for (int i = 0; i < _allowedArModes.Count; i++)
            {
              if (_allowedArModes[i] == arMode)
              {
                arMode = _allowedArModes[(i + 1) % _allowedArModes.Count]; // select next allowed mode
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
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1,
                                            0, 0, null);
            msg.Label = status;
            OnMessage(msg);
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
        case Action.ActionType.ACTION_STEP_BACK:
          {
            if (g_Player.CanSeek)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
                _isPauseOsdVisible = false;
                GUIWindowManager.IsPauseOsdVisible = false;
                ScreenStateChanged();
                UpdateGUI();
              }

              _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
              _showStep = true;
              g_Player.SeekStep(false);
              string statusLine = g_Player.GetStepDescription();
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int)Control.LABEL_ROW1, 0, 0, null);
              msg.Label = statusLine;
              OnMessage(msg);
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
        case Action.ActionType.ACTION_STEP_FORWARD:
          {
            if (g_Player.CanSeek)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
                _isPauseOsdVisible = false;
                GUIWindowManager.IsPauseOsdVisible = false;
                ScreenStateChanged();
                UpdateGUI();
              }
              _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
              _showStep = true;
              g_Player.SeekStep(true);
              string statusLine = g_Player.GetStepDescription();
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int)Control.LABEL_ROW1, 0, 0, null);
              msg.Label = statusLine;
              OnMessage(msg);
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
                  _isPauseOsdVisible = false;
                  GUIWindowManager.IsPauseOsdVisible = false;
                  ScreenStateChanged();
                  UpdateGUI();
                }

                _showStatus = true;
                _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                                (int)Control.LABEL_ROW1, 0, 0, null);
                msg.Label = "";
                OnMessage(msg);
                if (_immediateSeekIsRelative)
                {
                  double currentpos = g_Player.CurrentPosition;
                  double duration = g_Player.Duration;
                  double percent = (currentpos / duration) * 100d;
                  percent -= _immediateSeekValue;
                  if (percent < 0)
                  {
                    percent = 0;
                  }
                  g_Player.SeekAsolutePercentage((int)percent);
                }
                else
                {
                  double dTime = (int)g_Player.CurrentPosition - _immediateSeekValue;
                  if (dTime > g_Player.Duration) dTime = g_Player.Duration - 5;
                  if (dTime < 0) dTime = 0d;
                  Log.Debug("BIG_STEP_BACK - Preparing to seek to {0}:{1}:{2}", (int)(dTime / 3600d),
                            (int)((dTime % 3600d) / 60d), (int)(dTime % 60d));
                  g_Player.SeekAbsolute(dTime);
                }
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
                  _isPauseOsdVisible = false;
                  GUIWindowManager.IsPauseOsdVisible = false;
                  ScreenStateChanged();
                  UpdateGUI();
                }

                _showStatus = true;
                _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                                (int)Control.LABEL_ROW1, 0, 0, null);
                msg.Label = "";
                OnMessage(msg);
                if (_immediateSeekIsRelative)
                {
                  double currentpos = g_Player.CurrentPosition;
                  double duration = g_Player.Duration;
                  double percent = (currentpos / duration) * 100d;
                  percent += _immediateSeekValue;
                  if (percent > 99)
                  {
                    percent = 99;
                  }
                  g_Player.SeekAsolutePercentage((int)percent);
                }
                else
                {
                  double dTime = (int)g_Player.CurrentPosition + _immediateSeekValue;
                  if (dTime > g_Player.Duration) dTime = g_Player.Duration - 5;
                  if (dTime < 0) dTime = 0d;
                  Log.Debug("BIG_STEP_FORWARD - Preparing to seek to {0}:{1}:{2}", (int)(dTime / 3600d),
                            (int)((dTime % 3600d) / 60d), (int)(dTime % 60d));
                  g_Player.SeekAbsolute(dTime);
                }
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
            GUIWindowManager.IsOsdVisible = true;
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
              _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int)Control.LABEL_ROW1, 0, 0, null);
              g_Player.SwitchToNextAudio();

              String language = g_Player.AudioLanguage(g_Player.CurrentAudioStream);
              String languageType = g_Player.AudioType(g_Player.CurrentAudioStream);
              if (languageType == Strings.Unknown || string.IsNullOrEmpty(languageType))
              {
                msg.Label = string.Format("{0} ({1}/{2})", language,
                                          g_Player.CurrentAudioStream + 1, g_Player.AudioStreams);
              }
              else
              {
                msg.Label = string.Format("{0} [{1}] ({2}/{3})", language, languageType.TrimEnd(),
                                          g_Player.CurrentAudioStream + 1, g_Player.AudioStreams);
              }

              OnMessage(msg);
              Log.Info("GUIVideoFullscreen: switched audio to {0}", msg.Label);
            }
          }
          break;

        case Action.ActionType.ACTION_NEXT_EDITION:
          {
            if (g_Player.EditionStreams > 1)
            {
              _showStatus = true;
              _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int)Control.LABEL_ROW1, 0, 0, null);
              g_Player.SwitchToNextEdition();

              String language = g_Player.EditionLanguage(g_Player.CurrentEditionStream);
              if (String.Equals(language, "Edition") || String.Equals(language, ""))
              {
                msg.Label = string.Format("{0} ({1}/{2})", g_Player.EditionType(g_Player.CurrentEditionStream),
                                          g_Player.CurrentEditionStream + 1, g_Player.EditionStreams);
              }
              else
              {
                msg.Label = string.Format("{0} {1} ({2}/{3})", language,
                                          g_Player.EditionType(g_Player.CurrentEditionStream),
                                          g_Player.CurrentEditionStream + 1, g_Player.EditionStreams);
              }

              OnMessage(msg);
              Log.Info("GUIVideoFullscreen: switched edition to {0}", msg.Label);
            }
          }
          break;

        case Action.ActionType.ACTION_NEXT_VIDEO:
          {
            if (g_Player.VideoStreams > 1)
            {
              _showStatus = true;
              _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int)Control.LABEL_ROW1, 0, 0, null);
              g_Player.SwitchToNextVideo();

              String language = g_Player.VideoLanguage(g_Player.CurrentVideoStream);
              String languagetype = g_Player.VideoType(g_Player.CurrentVideoStream);
              if (String.Equals(language, "Video") || String.Equals(language, "") || String.Equals(language, languagetype))
              {
                msg.Label = string.Format("{0} ({1}/{2})", languagetype,
                                          g_Player.CurrentVideoStream + 1, g_Player.VideoStreams);
              }
              else
              {
                msg.Label = string.Format("{0} {1} ({2}/{3})", language,
                                          languagetype,
                                          g_Player.CurrentVideoStream + 1, g_Player.VideoStreams);
              }

              OnMessage(msg);
              Log.Info("GUIVideoFullscreen: switched Video to {0}", msg.Label);
            }
          }
          break;

        case Action.ActionType.ACTION_NEXT_SUBTITLE:
          {
            int subStreamsCount = g_Player.SubtitleStreams;
            if (subStreamsCount > 0 || g_Player.SupportsCC)
            {
              _showStatus = true;
              _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int)Control.LABEL_ROW1, 0, 0, null);
              g_Player.SwitchToNextSubtitle();
              if (g_Player.EnableSubtitle)
              {
                if (g_Player.CurrentSubtitleStream == -1 && g_Player.SupportsCC)
                {
                  msg.Label = "CC1 Analog";
                }
                else
                {
                  int streamId = g_Player.CurrentSubtitleStream;
                  string strName = g_Player.SubtitleName(streamId);
                  string langName = g_Player.SubtitleLanguage(streamId);
                  if (!string.IsNullOrEmpty(strName))
                    msg.Label = string.Format("{0} [{1}] ({2}/{3})", langName, strName.TrimStart(),
                                              streamId + 1, subStreamsCount);
                  else
                    msg.Label = string.Format("{0} ({1}/{2})", langName,
                                              streamId + 1, subStreamsCount);
                }
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
            if (g_Player.IsPicture)
            {
              GUISlideShow._slideDirection = 0;
            }
            Log.Info("GUIVideoFullscreen:stop");
            g_Player.Stop();
            GUIWindowManager.ShowPreviousWindow();
          }
          break;

        case Action.ActionType.ACTION_PAUSE:
          if (g_Player.Paused)
          {
            m_dwOSDTimeOut = DateTime.Now;
            _isPauseOsdVisible = true;
            GUIWindowManager.IsPauseOsdVisible = true;
          }
          else
          {
            _isPauseOsdVisible = false;
            GUIWindowManager.IsPauseOsdVisible = false;
          }
          break;

        case Action.ActionType.ACTION_SUBTITLE_DELAY_MIN:
          if (g_Player.EnableSubtitle)
          {
            SubEngine.GetInstance().DelayMinus();
            ShowSubtitleDelayStatus();
          }
          break;
        case Action.ActionType.ACTION_SUBTITLE_DELAY_PLUS:
          if (g_Player.EnableSubtitle)
          {
            SubEngine.GetInstance().DelayPlus();
            ShowSubtitleDelayStatus();
          }
          break;
        case Action.ActionType.ACTION_AUDIO_DELAY_MIN:
          //g_application.m_pPlayer.AudioOffset(false);
          break;
        case Action.ActionType.ACTION_AUDIO_DELAY_PLUS:
          //g_application.m_pPlayer.AudioOffset(true);
          break;

        case Action.ActionType.ACTION_REWIND:
          {
            _isPauseOsdVisible = false;
            GUIWindowManager.IsPauseOsdVisible = false;

            if (g_Player.CanSeek && !g_Player.IsDVD)
            {
              ScreenStateChanged();
              UpdateGUI();
              double dPos = g_Player.CurrentPosition;
              if (dPos > 1)
              {
                Log.Debug("GUIVideoFullscreen.Rewind - skipping");
                g_Player.SeekAbsolute(dPos - 0.25d);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_FORWARD:
          {
            _isPauseOsdVisible = false;
            GUIWindowManager.IsPauseOsdVisible = false;

            if (g_Player.CanSeek && !g_Player.IsDVD)
            {
              ScreenStateChanged();
              UpdateGUI();
              double dPos = g_Player.CurrentPosition;
              if (g_Player.Duration - dPos > 1)
              {
                Log.Debug("GUIVideoFullscreen.Forward - skipping");
                g_Player.SeekAbsolute(dPos + 0.25d);
              }
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

        case Action.ActionType.ACTION_SMALL_STEP_FORWARD:
          {
            if (g_Player.CanSeek)
            {
              // seek forward 5 sec
              double dPos = g_Player.Duration - g_Player.CurrentPosition;

              if (dPos > 5)
              {
                g_Player.SeekAbsolute(g_Player.CurrentPosition + 5.0d);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          {
            GUIWindowManager.IsPauseOsdVisible = false;
            break;
          }


        case Action.ActionType.ACTION_CONTEXT_MENU:
          ShowContextMenu();
          break;
        case Action.ActionType.ACTION_PREV_BOOKMARK:
          {
            ArrayList bookmarks = new ArrayList();
            VideoDatabase.GetBookMarksForMovie(g_Player.CurrentFile, ref bookmarks);
            if (bookmarks.Count <= 0)
            {
              break; // no bookmarks? leave if so ...
            }
            List<double> bookmarkList = new List<double>();
            for (int i = 0; i < bookmarks.Count; i++)
            {
              bookmarkList.Add((double)bookmarks[i]);
            }
            bookmarkList.Sort();
            double dCurTime = g_Player.CurrentPosition;
            int bookmarkIndex = bookmarkList.Count - 1;
            for (int i = 0; i < bookmarkList.Count; i++)
            {
              double pos = bookmarkList[i];
              if (pos + 0.5 < dCurTime)
              {
                bookmarkIndex = i;
              }
              else
                break;
            }
            g_Player.SeekAbsolute(bookmarkList[bookmarkIndex]);
            break;
          }
        case Action.ActionType.ACTION_NEXT_BOOKMARK:
          {
            ArrayList bookmarks = new ArrayList();
            VideoDatabase.GetBookMarksForMovie(g_Player.CurrentFile, ref bookmarks);
            if (bookmarks.Count <= 0)
            {
              break; // no bookmarks? leave if so ...
            }
            List<double> bookmarkList = new List<double>();
            for (int i = 0; i < bookmarks.Count; i++)
            {
              bookmarkList.Add((double)bookmarks[i]);
            }
            bookmarkList.Sort();
            double dCurTime = g_Player.CurrentPosition;
            int bookmarkIndex = 0;
            for (int i = 0; i < bookmarkList.Count; i++)
            {
              double pos = bookmarkList[i];
              if (pos >= dCurTime)
              {
                bookmarkIndex = i;
                break;
              }
            }
            g_Player.SeekAbsolute(bookmarkList[bookmarkIndex]);
            break;
          }
      }

      base.OnAction(action);
    }

    private void ShowSubtitleDelayStatus()
    {
      _showStatus = true;
      _timeStatusShowTime = (DateTime.Now.Ticks / 10000);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                      (int)Control.LABEL_ROW1, 0, 0, null);
      msg.Label = string.Format("{0} ms", SubEngine.GetInstance().Delay);
      OnMessage(msg);
      Log.Info("GUIVideoFullscreen subtitles delay: {0}", msg.Label);
    }

    private bool OnOsdMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
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

      if (_isOsdVisible)
      {
        return OnOsdMessage(message);
      }

      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);

            _osdWindow = (GUIVideoOSD)GUIWindowManager.GetWindow((int)Window.WINDOW_OSD);

            HideControl(GetID, (int)Control.LABEL_ROW1);
            HideControl(GetID, (int)Control.LABEL_ROW2);
            HideControl(GetID, (int)Control.LABEL_ROW3);
            HideControl(GetID, (int)Control.BLUE_BAR);

            if (!SettingsLoaded)
              LoadSettings();

            GUIWindowManager.IsOsdVisible = false;
            GUIWindowManager.IsPauseOsdVisible = g_Player.Paused;
            _isOsdVisible = false;
            _isPauseOsdVisible = g_Player.Paused;
            m_dwOSDTimeOut = DateTime.Now;
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
            _volumeTimer = DateTime.MinValue;
            _forbiddenTimer = DateTime.MinValue;

            screenState = new FullScreenState();
            NotifyDialogVisible = false;

            ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
            ScreenStateChanged();
            _needToClearScreen = true;
            UpdateGUI();
            if (!screenState.Paused)
            {
              for (int i = (int)Control.PANEL1; i < (int)Control.PANEL2; ++i)
              {
                HideControl(GetID, i);
              }
            }

            GUIGraphicsContext.IsFullScreenVideo = true;
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);

            RenderVolume(false);
            RenderForbidden(false);

            //return base.OnMessage(message);
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
              _isPauseOsdVisible = false;
              GUIWindowManager.IsOsdVisible = false;
              GUIWindowManager.IsPauseOsdVisible = false;
              GUIGraphicsContext.IsFullScreenVideo = false;

              GUILayerManager.UnRegisterLayer(this);

              /*imgVolumeMuteIcon.SafeDispose();
              imgVolumeBar.SafeDispose();
              imgActionForbiddenIcon.SafeDispose();
              dlg.SafeDispose();
              _osdWindow.SafeDispose();*/

              base.OnMessage(message);
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
          if (message.SenderControlId != (int)Window.WINDOW_FULLSCREEN_VIDEO)
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
        dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      }
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

      dlg.AddLocalizedString(941); // Change aspect ratio

      // Audio stream selection, show only when more than one streams exists
      if ((g_Player.ShowMenuItems & MenuItems.Audio) == MenuItems.Audio && g_Player.AudioStreams > 1)
      {
        dlg.AddLocalizedString(492);
      }

      // Edition stream selection, show only when more than one streams exists
      if (g_Player.EditionStreams > 1)
      {
        dlg.AddLocalizedString(200090);
      }

      // Video stream selection, show only when more than one streams exists
      if (g_Player.VideoStreams > 1)
      {
        dlg.AddLocalizedString(200095);
      }

      eAudioDualMonoMode dualMonoMode = g_Player.GetAudioDualMonoMode();
      if (dualMonoMode != eAudioDualMonoMode.UNSUPPORTED)
      {
        dlg.AddLocalizedString(200059); // Audio dual mono mode menu
      }

      // SubTitle stream and/or files selection, show only when there exists any streams,
      //    dialog shows then the streams and an item to disable them
      if ((g_Player.ShowMenuItems & MenuItems.Subtitle) == MenuItems.Subtitle && g_Player.SubtitleStreams > 0 || g_Player.SupportsCC)
      {
        dlg.AddLocalizedString(462);
      }

      // If the decoder supports postprocessing features (FFDShow)
      if (g_Player.HasPostprocessing)
      {
        dlg.AddLocalizedString(200073);
      }

      dlg.AddLocalizedString(970); // Previous window
      if (g_Player.IsDVD)
      {
        if ((g_Player.ShowMenuItems & MenuItems.MainMenu) == MenuItems.MainMenu)
          dlg.AddLocalizedString(974); // Root menu
        if ((g_Player.ShowMenuItems & MenuItems.PopUpMenu) == MenuItems.PopUpMenu)
          dlg.AddLocalizedString(1700); // BD popup menu        
        if(!g_Player.HasChapters && (g_Player.ShowMenuItems & MenuItems.Chapter) == MenuItems.Chapter)
        {
          dlg.AddLocalizedString(975); // Previous chapter
          dlg.AddLocalizedString(976); // Next chapter
        }
      }

      if (g_Player.HasChapters && (g_Player.ShowMenuItems & MenuItems.Chapter) == MenuItems.Chapter) // For video files with chapters
      {
        dlg.AddLocalizedString(200091);
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
        case 200073:
          ShowPostProcessingMenu();
          break;
        case 1064:
          ShowBookmarksMenu();
          break;
        case 974: // DVD root menu
          Action actionMenu = new Action(Action.ActionType.ACTION_DVD_MENU, 0, 0);
          GUIGraphicsContext.OnAction(actionMenu);
          break;
        case 1700: // BD popup menu
          Action actionPopupMenu = new Action(Action.ActionType.ACTION_BD_POPUP_MENU, 0, 0);
          GUIGraphicsContext.OnAction(actionPopupMenu);
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

        case 200090:
          ShowEditionStreamsMenu();
          break;

        case 200095:
          ShowVideoStreamsMenu();
          break;

        case 200091:
          ShowChapterStreamsMenu();
          break;
      }
    }

    private void ShowChapterStreamsMenu()
    {
      if (dlg == null)
      {
        dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      }
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(200091); // Chapters Streams

      // Previous chapter
      dlg.Add(String.Format("{0}", GUILocalizeStrings.Get(975)));
      // Next chapter
      dlg.Add(String.Format("{0}", GUILocalizeStrings.Get(976)));

      //List all chapters
      double[] chaptersList = new double[0];
      //List all chapters Name
      string[] chaptersname = new string[0];

      chaptersname = g_Player.ChaptersName;
      chaptersList = g_Player.Chapters;
      for (int i = 0; i < chaptersList.Length; i++)
      {
        GUIListItem item = new GUIListItem();
        if (chaptersname == null)
        {
          item.Label = (String.Format("{0} #{1}", GUILocalizeStrings.Get(200091), (i + 1)));
          item.Label2 = Util.Utils.SecondsToHMSString((int)chaptersList[i]);
          dlg.Add(item);
        }
        else
        {
          if (string.IsNullOrEmpty(chaptersname[i]))
          {
            item.Label = (String.Format("{0} #{1}", GUILocalizeStrings.Get(200091), (i + 1)));
            item.Label2 = Util.Utils.SecondsToHMSString((int)chaptersList[i]);
            dlg.Add(item);
          }
          else
          {
            item.Label = (String.Format("{0} #{1}: {2}", GUILocalizeStrings.Get(200091), (i + 1), chaptersname[i]));
            item.Label2 = Util.Utils.SecondsToHMSString((int)chaptersList[i]);
            dlg.Add(item);
          }
        }
      }

      // show dialog and wait for result
      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      else if (dlg.SelectedLabel == 0)
      {
        Action actionPrevChapter = new Action(Action.ActionType.ACTION_PREV_CHAPTER, 0, 0);
        GUIGraphicsContext.OnAction(actionPrevChapter);
      }
      else if (dlg.SelectedLabel == 1)
      {
        Action actionNextChapter = new Action(Action.ActionType.ACTION_NEXT_CHAPTER, 0, 0);
        GUIGraphicsContext.OnAction(actionNextChapter);
      }
      else
      {
        // get selected Chapters
        int selectedChapterIndex = dlg.SelectedLabel - 2;

        // set mplayers play position
        g_Player.SeekAbsolute(chaptersList[selectedChapterIndex]);
      }
    }

    // Add edition stream selection to be able to switch edition streams in .ts recordings
    private void ShowEditionStreamsMenu()
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(200090); // Edition Streams

      // get the number of editionstreams in the current movie
      int count = g_Player.EditionStreams;
      // cycle through each editionstream and add it to our list control
      for (int i = 0; i < count; i++)
      {
        string editionType = g_Player.EditionType(i);
        if (editionType == Strings.Unknown || String.Equals(editionType, "") ||
            editionType.Equals(g_Player.EditionType(i)))
        {
          dlg.Add(g_Player.EditionLanguage(i));
        }
        else
        {
          dlg.Add(String.Format("{0} {1}", g_Player.EditionLanguage(i), editionType));
        }
      }

      // select/focus the editionstream, which is active atm
      dlg.SelectedLabel = g_Player.CurrentEditionStream;

      // show dialog and wait for result
      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      if (dlg.SelectedLabel != g_Player.CurrentEditionStream)
      {
        g_Player.CurrentEditionStream = dlg.SelectedLabel;
      }
    }

    // Add video stream selection to be able to switch video streams
    private void ShowVideoStreamsMenu()
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(200095); // Video Streams

      // get the number of videostreams in the current movie
      int count = g_Player.VideoStreams;
      // cycle through each videostream and add it to our list control
      for (int i = 0; i < count; i++)
      {
        string videoType = g_Player.VideoType(i);
        string videoLang = g_Player.VideoLanguage(i);
        if (videoType == Strings.Unknown || String.Equals(videoType, "") ||
            videoType.Equals(videoLang))
        {
          dlg.Add(videoLang);
        }
        else
        {
          dlg.Add(String.Format("{0} {1}", videoLang, videoType));
        }
      }

      // select/focus the videostream, which is active atm
      dlg.SelectedLabel = g_Player.CurrentVideoStream;

      // show dialog and wait for result
      _IsDialogVisible = true;
      dlg.DoModal(GetID);
      _IsDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      if (dlg.SelectedLabel != g_Player.CurrentVideoStream)
      {
        g_Player.CurrentVideoStream = dlg.SelectedLabel;
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
        if (audioType == Strings.Unknown || string.IsNullOrEmpty(audioType))
        {
          dlg.Add(g_Player.AudioLanguage(i));
        }
        else
        {
          dlg.Add(String.Format("{0} [{1}]", g_Player.AudioLanguage(i), audioType.TrimEnd()));
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

      dlg.SelectedLabel = (int)dualMonoMode;

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      g_Player.SetAudioDualMonoMode((eAudioDualMonoMode)dlg.SelectedLabel);
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

      if (g_Player.SupportsCC)
      {
        dlg.Add("CC1 Analog");
      }

      // get the number of subtitles in the current movie
      int nbSubStreams = g_Player.SubtitleStreams;
      // cycle through each subtitle and add it to our list control
      for (int i = 0; i < nbSubStreams; ++i)
      {
        // remove (English) in: "English (English)", should be done by gplayer
        string strLang = g_Player.SubtitleLanguage(i);
        int ipos = strLang.IndexOf("(");
        if (ipos > 0)
        {
          strLang = strLang.Substring(0, ipos);
        }
        string strName = g_Player.SubtitleName(i);
        if (!string.IsNullOrEmpty(strName) && !strLang.Equals(strName))
        {
          dlg.Add(String.Format("{0} [{1}]", strLang.TrimEnd(), strName.TrimStart()));
        }
        else
        {
          dlg.Add(strLang);
        }
      }

      // select/focus the subtitle, which is active atm.
      // There may be no subtitle streams selected at all (-1), which happens when a subtitle file is used instead
      if (g_Player.EnableSubtitle)
      {
        if (g_Player.SupportsCC)
        {
          dlg.SelectedLabel = g_Player.CurrentSubtitleStream + 2;
        }
        else
        {
          dlg.SelectedLabel = g_Player.CurrentSubtitleStream + 1;
        }
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
      else if (g_Player.SupportsCC && dlg.SelectedLabel == 1 && g_Player.CurrentSubtitleStream != -1)
      {
        g_Player.CurrentSubtitleStream = -1;
        g_Player.EnableSubtitle = true;
      }
      else
      {
        int i = 1;
        if (g_Player.SupportsCC)
        {
          i = 2;
        }
        if (dlg.SelectedLabel != g_Player.CurrentSubtitleStream + i)
        {
          Log.Info("Subtitle stream selected : " + (dlg.SelectedLabel - i));
          g_Player.CurrentSubtitleStream = dlg.SelectedLabel - i;
        }
        g_Player.EnableSubtitle = true;
      }
    }

    private void ShowPostProcessingMenu()
    {
      if (dlg == null)
      {
        return;
      }

      do
      {
        dlg.Reset();
        dlg.SetHeading(200073); // Postprocessing
        IPostProcessingEngine engine = PostProcessingEngine.GetInstance();
        // Deblocking
        dlg.Add(String.Format("{0} {1}", GUILocalizeStrings.Get(200074),
                              (engine.EnablePostProcess) ? GUILocalizeStrings.Get(461) : ""));
        // Resize
        dlg.Add(String.Format("{0} {1}", GUILocalizeStrings.Get(200075),
                              (engine.EnableResize) ? GUILocalizeStrings.Get(461) : ""));
        // Crop
        dlg.Add(String.Format("{0} {1}", GUILocalizeStrings.Get(200078),
                              (engine.EnableCrop) ? GUILocalizeStrings.Get(461) : ""));
        // Deinterlace
        dlg.Add(String.Format("{0} {1}", GUILocalizeStrings.Get(200077),
                              (engine.EnableDeinterlace) ? GUILocalizeStrings.Get(461) : ""));
        dlg.AddLocalizedString(970); // Previous window
        dlg.SelectedLabel = 0;

        // show dialog and wait for result
        _IsDialogVisible = true;
        dlg.DoModal(GetID);
        if (dlg.SelectedId == 970)
        {
          // switch back to previous window
          _IsDialogVisible = false;
          GUIWindowManager.ShowPreviousWindow();
          return;
        }

        switch (dlg.SelectedLabel)
        {
          case 0:
            engine.EnablePostProcess = !engine.EnablePostProcess;
            break;
          case 1:
            engine.EnableResize = !engine.EnableResize;
            break;
          case 2:
            engine.EnableCrop = !engine.EnableCrop;
            break;
          case 3:
            engine.EnableDeinterlace = !engine.EnableDeinterlace;
            break;
        }
      } while (dlg.SelectedId != -1);
      _IsDialogVisible = false;
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
      _timeStatusShowTime = (DateTime.Now.Ticks / 10000);

      string strStatus = "";

      GUIGraphicsContext.ARType = Util.Utils.GetAspectRatioByLangID(dlg.SelectedId);
      strStatus = GUILocalizeStrings.Get(dlg.SelectedId);

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0,
                                      null);
      msg.Label = strStatus;
      OnMessage(msg);
    }

    public void ShowBookmarksMenu()
    {
      if (dlg == null)
      {
        dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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
        bookmarkList.Add((double)bookmarks[i]);
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
        item.Label2 = Util.Utils.SecondsToHMSString((int)bookmarkList[i]);

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
        VideoDatabase.AddBookMarkToMovie(g_Player.CurrentFile, (float)dCurTime);
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
      if (_isPauseOsdVisible != screenState.PauseOsdVisible)
      {
        screenState.PauseOsdVisible = _isPauseOsdVisible;
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
      int iSpeed = g_Player.Speed;
      HideControl(GetID, (int)Control.IMG_2X);
      HideControl(GetID, (int)Control.IMG_4X);
      HideControl(GetID, (int)Control.IMG_8X);
      HideControl(GetID, (int)Control.IMG_16X);
      HideControl(GetID, (int)Control.IMG_32X);
      HideControl(GetID, (int)Control.IMG_MIN2X);
      HideControl(GetID, (int)Control.IMG_MIN4X);
      HideControl(GetID, (int)Control.IMG_MIN8X);
      HideControl(GetID, (int)Control.IMG_MIN16X);
      HideControl(GetID, (int)Control.IMG_MIN32X);

      if (!_showStep)
      {
        switch (iSpeed)
        {
          case 2:
            ShowControl(GetID, (int)Control.IMG_2X);
            break;
          case 4:
            ShowControl(GetID, (int)Control.IMG_4X);
            break;
          case 8:
            ShowControl(GetID, (int)Control.IMG_8X);
            break;
          case 16:
            ShowControl(GetID, (int)Control.IMG_16X);
            break;
          case 32:
            ShowControl(GetID, (int)Control.IMG_32X);
            break;
          case -2:
            ShowControl(GetID, (int)Control.IMG_MIN2X);
            break;
          case -4:
            ShowControl(GetID, (int)Control.IMG_MIN4X);
            break;
          case -8:
            ShowControl(GetID, (int)Control.IMG_MIN8X);
            break;
          case -16:
            ShowControl(GetID, (int)Control.IMG_MIN16X);
            break;
          case -32:
            ShowControl(GetID, (int)Control.IMG_MIN32X);
            break;
        }
      }

      HideControl(GetID, (int)Control.LABEL_ROW1);
      HideControl(GetID, (int)Control.LABEL_ROW2);
      HideControl(GetID, (int)Control.LABEL_ROW3);
      HideControl(GetID, (int)Control.BLUE_BAR);
      if (screenState.SeekStep != 0)
      {
        ShowControl(GetID, (int)Control.BLUE_BAR);
        ShowControl(GetID, (int)Control.LABEL_ROW1);
      }
      if (_showStatus)
      {
        ShowControl(GetID, (int)Control.BLUE_BAR);
        ShowControl(GetID, (int)Control.LABEL_ROW1);
      }
      if (_showSkipBar && !g_Player.Paused) // If paused, this will already be shown, including LABEL_ROW1
      {
        ShowControl(GetID, (int)Control.BLUE_BAR);
      }
      if (_showTime)
      {
        ShowControl(GetID, (int)Control.BLUE_BAR);
        ShowControl(GetID, (int)Control.LABEL_ROW1);
      }

      RenderVolume(_isVolumeVisible);
      RenderForbidden(_isForbiddenVisible);
    }

    private void CheckTimeOuts()
    {
      if (_isVolumeVisible)
      {
        TimeSpan ts = DateTime.Now - _volumeTimer;
        // mantis 0002467: Keep Mute Icon on screen if muting is ON 
        if (ts.TotalSeconds >= 3 && !VolumeHandler.Instance.IsMuted)
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
        long lTimeSpan = ((DateTime.Now.Ticks / 10000) - _timeStatusShowTime);
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
      if (g_Player.Paused && m_iMaxTimeOSDOnscreen > 0)
      {
        TimeSpan ts = DateTime.Now - m_dwOSDTimeOut;
        if (ts.TotalMilliseconds > m_iMaxTimeOSDOnscreen)
        {
          _isPauseOsdVisible = false;
          GUIWindowManager.IsPauseOsdVisible = false;
        }
        else
        {
          _isPauseOsdVisible = true;
          GUIWindowManager.IsPauseOsdVisible = true;
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
        _isPauseOsdVisible = false;
        GUIWindowManager.IsPauseOsdVisible = false;
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
    }

    public override void Render(float timePassed)
    {
      if (GUIWindowManager.IsSwitchingToNewWindow)
      {
        return;
      }
      if (GUIGraphicsContext.Vmr9Active)
      {
        base.Render(timePassed);
      }
      if (_isOsdVisible)
      {
        _osdWindow.Render(timePassed);
      }
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
        ih = (_timeStamp[0] - (char)'0') * 10;
        ih += (_timeStamp[1] - (char)'0');
        im = (_timeStamp[3] - (char)'0') * 10;
        im += (_timeStamp[4] - (char)'0');
        im *= 60;
        ih *= 3600;
        itotal = ih + im + lis;
        if (itotal < g_Player.Duration)
        {
          Log.Debug("GUIVideoFullscreen.ChangetheTimeCode - skipping");
          g_Player.SeekAbsolute((double)itotal);
        }
        _timeStamp = "";
        _timeCodePosition = 0;
        _showTime = false;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0,
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
          imgVolumeBar.Maximum = VolumeHandler.Instance.StepMax;
          imgVolumeBar.Current = 0;
          imgVolumeMuteIcon.Visible = true;
          imgVolumeBar.Image1 = 1;
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
          return (x - cntl.XPosition - SKIPBAR_PADDING) / (cntl.Width - 2 * SKIPBAR_PADDING);
        }
      }
      return -1;
    }

    private void g_Player_PlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
    {
      SettingsLoaded = false; // we should reload
    }

    private void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      SettingsLoaded = false; // we should reload
      // playback was stopped, if we are the current window, close our context menu, so we also get closed
      if (type != g_Player.MediaType.Video || GUIWindowManager.ActiveWindow != GetID) return;
      if (!_IsClosingDialog)
      {
        _IsClosingDialog = true;
        GUIDialogWindow.CloseRoutedWindow();
        _IsClosingDialog = false;
      }
    }

    private void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
    {
      SettingsLoaded = false; // we should reload
      // playback ended, if we are the current window, close our context menu, so we also get closed
      if (type != g_Player.MediaType.Video || GUIWindowManager.ActiveWindow != GetID) return;
      if (!_IsClosingDialog)
      {
        _IsClosingDialog = true;
        GUIDialogWindow.CloseRoutedWindow();
        _IsClosingDialog = false;
      }
    }

    #endregion

    #region Properties

    private bool SettingsLoaded
    {
      get { return _settingsLoaded; }
      set
      {
        _settingsLoaded = value;
        //maybe additional logic?
      }
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
      //base.Render(timePassed);
    }

    #endregion
  }
}