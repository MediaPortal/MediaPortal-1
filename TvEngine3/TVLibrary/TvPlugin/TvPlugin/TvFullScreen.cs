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

#region usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using Gentle.Common;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using Timer=System.Timers.Timer;
using System.Runtime.Remoting;

#endregion

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvFullScreen : GUIInternalWindow, IRenderLayer, IMDB.IProgress
  {
    #region FullScreenState class

    private class FullScreenState
    {
      public int SeekStep = 1;
      public int Speed = 1;
      public bool OsdVisible = false;
      public bool Paused = false;
      //public bool MsnVisible = false;       // msn related can be removed
      public bool ContextMenuVisible = false;
      public bool ShowStatusLine = false;
      public bool ShowTime = false;
      public bool ZapOsdVisible = false;
      public bool MsgBoxVisible = false;
      public bool ShowGroup = false;
      public bool ShowInput = false;
      public bool _notifyDialogVisible = false;
      public bool _bottomDialogMenuVisible = false;
      public bool wasVMRBitmapVisible = false;
      public bool volumeVisible = false;
      public bool _dialogYesNoVisible = false;
    }

    #endregion

    #region variables

    private bool _stepSeekVisible = false;
    private bool _statusVisible = false;
    private bool _groupVisible = false;
    private bool _byIndex = false;
    private DateTime _statusTimeOutTimer = DateTime.Now;

    ///@
    private TvZapOsd _zapWindow = null;

    private TvOsd _osdWindow = null;

    ///GUITVMSNOSD _msnWindow = null;       // msn related can be removed
    private DateTime _osdTimeoutTimer;

    private DateTime _zapTimeOutTimer;
    private DateTime _groupTimeOutTimer;
    private DateTime _vmr7UpdateTimer = DateTime.Now;
    //		string			m_sZapChannel;
    //		long				m_iZapDelay;
    private bool _isOsdVisible = false;
    private bool _zapOsdVisible = false;
    //bool _msnWindowVisible = false;       // msn related can be removed
    private bool _channelInputVisible = false;

    private long _timeOsdOnscreen;
    private long _zapTimeOutValue;
    private DateTime _updateTimer = DateTime.Now;
    private DateTime _updateTimerProgressbar = DateTime.Now;
    private bool _lastPause = false;
    private int _lastSpeed = 1;
    private DateTime _keyPressedTimer = DateTime.Now;
    private string _channelName = "";
    private bool _isDialogVisible = false;
    //bool _isMsnChatPopup = false;       // msn related can be removed
    private GUIDialogMenu dlg;
    private GUIDialogNotify _dialogNotify = null;
    private GUIDialogMenuBottomRight _dialogBottomMenu = null;
    private GUIDialogYesNo _dlgYesNo = null;
    // Message box
    private bool _dialogYesNoVisible = false;
    private bool _notifyDialogVisible = false;
    private bool _bottomDialogMenuVisible = false;
    private bool _messageBoxVisible = false;
    private DateTime _msgTimer = DateTime.Now;
    private int _msgBoxTimeout = 0;
    private int _notifyTVTimeout = 15;
    private bool _playNotifyBeep = true;
    private bool _needToClearScreen = false;
    private bool _useVMR9Zap = false;
    private bool _immediateSeekIsRelative = true;
    private int _immediateSeekValue = 10;

    // Tv error handling
    private TvPlugin.TVHome.ChannelErrorInfo _gotTvErrorMessage = null;
    ///@
    ///VMR9OSD _vmr9OSD = null;
    private FullScreenState _screenState = new FullScreenState();

    private bool _isVolumeVisible = VolumeHandler.Instance.IsMuted;
    private DateTime _volumeTimer = DateTime.MinValue;
    private bool _isStartingTSForRecording = false;
    private bool _autoZapMode = false;
    private Timer _autoZapTimer = new Timer();
    [SkinControl(500)] protected GUIImage imgVolumeMuteIcon;
    [SkinControl(501)] protected GUIVolumeBar imgVolumeBar;

    private string lastChannelWithNoSignal = string.Empty;
    private VideoRendererStatistics.State videoState = VideoRendererStatistics.State.VideoPresent;
    private List<Geometry.Type> _allowedArModes = new List<Geometry.Type>();

    #endregion

    #region enums

    private enum Control
    {
      BLUE_BAR = 0,
      MSG_BOX = 2,
      MSG_BOX_LABEL1 = 3,
      MSG_BOX_LABEL2 = 4,
      MSG_BOX_LABEL3 = 5,
      MSG_BOX_LABEL4 = 6,
      LABEL_ROW1 = 10,
      LABEL_ROW2 = 11,
      LABEL_ROW3 = 12,
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
      OSD_VIDEOPROGRESS = 100,
      REC_LOGO = 39
    } ;

    #endregion

    public TvFullScreen()
    {
      Log.Debug("TvFullScreen:ctor");
      GetID = (int) Window.WINDOW_TVFULLSCREEN;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override bool IsTv
    {
      get { return true; }
    }

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
      using (Settings xmlreader = new MPSettings())
      {
        _useVMR9Zap = xmlreader.GetValueAsBool("general", "useVMR9ZapOSD", false);
        _notifyTVTimeout = xmlreader.GetValueAsInt("movieplayer", "notifyTVTimeout", 10);
        _playNotifyBeep = xmlreader.GetValueAsBool("movieplayer", "notifybeep", true);
        _immediateSeekIsRelative = xmlreader.GetValueAsBool("movieplayer", "immediateskipstepsisrelative", true);
        _immediateSeekValue = xmlreader.GetValueAsInt("movieplayer", "immediateskipstepsize", 10);
        
      }
      Load(GUIGraphicsContext.Skin + @"\mytvFullScreen.xml");
      GetID = (int) Window.WINDOW_TVFULLSCREEN;

      Log.Debug("TvFullScreen:Init");
      return true;
    }

    #region serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        //_isMsnChatPopup = (xmlreader.GetValueAsInt("MSNmessenger", "popupwindow", 0) == 1);       // msn related can be removed
        _timeOsdOnscreen = 1000*xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 5);
        //				m_iZapDelay = 1000*xmlreader.GetValueAsInt("movieplayer","zapdelay",2);
        _zapTimeOutValue = 1000*xmlreader.GetValueAsInt("movieplayer", "zaptimeout", 5);
        _byIndex = xmlreader.GetValueAsBool("mytv", "byindex", true);
        if (xmlreader.GetValueAsBool("mytv", "allowarzoom", true))
        {
          _allowedArModes.Add(Geometry.Type.Zoom);
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarstretch", true))
        {
          _allowedArModes.Add(Geometry.Type.Stretch);
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarnormal", true))
        {
          _allowedArModes.Add(Geometry.Type.Normal);
        }
        if (xmlreader.GetValueAsBool("mytv", "allowaroriginal", true))
        {
          _allowedArModes.Add(Geometry.Type.Original);
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarletterbox", true))
        {
          _allowedArModes.Add(Geometry.Type.LetterBox43);
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarnonlinear", true))
        {
          _allowedArModes.Add(Geometry.Type.NonLinearStretch);
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarzoom149", true))
        {
          _allowedArModes.Add(Geometry.Type.Zoom14to9);
        }
        if (!TVHome.settingsLoaded)
        {
          string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "Normal");
          GUIGraphicsContext.ARType = Utils.GetAspectRatio(strValue);
        }
      }
    }

    //		public string ZapChannel
    //		{
    //			set
    //			{
    //				m_sZapChannel = value;
    //			}
    //			get
    //			{
    //				return m_sZapChannel;
    //			}
    //		}

   

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




    public override void OnAction(Action action)
    {
      _needToClearScreen = true;

      if (action.wID == Action.ActionType.ACTION_SHOW_VOLUME)
      {
        _volumeTimer = DateTime.Now;
        _isVolumeVisible = true;
        RenderVolume(_isVolumeVisible);
        //				if(_vmr9OSD!=null)
        //					_vmr9OSD.RenderVolumeOSD();
      }
      //ACTION_SHOW_CURRENT_TV_INFO
      if (action.wID == Action.ActionType.ACTION_SHOW_CURRENT_TV_INFO)
      {
        //if(_vmr9OSD!=null)
        //	_vmr9OSD.RenderCurrentShowInfo();
      }

      if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK && action.MouseButton == MouseButtons.Right)
      {
        // switch back to the menu
        _isOsdVisible = false;
        //_msnWindowVisible = false;       // msn related can be removed
        GUIWindowManager.IsOsdVisible = false;
        GUIGraphicsContext.IsFullScreenVideo = false;
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
      if (_isOsdVisible)
      {
        if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI) ||
             (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)) && !_osdWindow.SubMenuVisible) // hide the OSD
        {
          lock (this)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID,
                                            0, null);
            _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            return;
          }
        }
        else
        {
          _osdTimeoutTimer = DateTime.Now;
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

                if (_zapOsdVisible)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0,
                                                  GetID, 0, null);
                  _zapWindow.OnMessage(msg);
                  _zapOsdVisible = false;
                }

                return;
              }
              else
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0,
                                                GetID, 0, null);
                _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
                _isOsdVisible = false;
                GUIWindowManager.IsOsdVisible = false;
                return;
              }
            }
          }
          _osdWindow.OnAction(action);
          return;
        }
      }
        //@
        /*
       * else if (_msnWindowVisible)       // msn related can be removed
      {
        if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI))) // hide the OSD
        {
          lock (this)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
            _msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            _msnWindowVisible = false;
            GUIWindowManager.IsOsdVisible = false;
          }
          return;
        }
        if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
        {
          _msnWindow.OnAction(action);

          return;
        }
      }*/

      else if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE && GUIGraphicsContext.MouseSupport)
      {
        int y = (int) action.fAmount2;
        if (y > GUIGraphicsContext.Height - 100)
        {
          _osdTimeoutTimer = DateTime.Now;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _osdWindow.OnMessage(msg); // Send an init msg to the OSD
          _isOsdVisible = true;
          GUIWindowManager.VisibleOsd = Window.WINDOW_TVOSD;
        }
      }
      else if (_zapOsdVisible)
      {
        if ((action.wID == Action.ActionType.ACTION_SHOW_GUI) || (action.wID == Action.ActionType.ACTION_SHOW_OSD) ||
            (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU))
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _zapWindow.OnMessage(msg);
          _zapOsdVisible = false;
        }
      }
      //Log.DebugFile(Log.LogType.Error, "action:{0}",action.wID);
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOUSE_DOUBLECLICK:

        case Action.ActionType.ACTION_SELECT_ITEM:
          {
            if (_autoZapMode)
            {
              StopAutoZap();
            }
            else if (_zapOsdVisible)
            {
              TVHome.Navigator.ZapNow();
            }
            else
            {
              TvMiniGuide miniGuide = (TvMiniGuide) GUIWindowManager.GetWindow((int) Window.WINDOW_MINI_GUIDE);
              _isDialogVisible = true;
              miniGuide.AutoZap = true;
              miniGuide.DoModal(GetID);
              _isDialogVisible = false;

              // LastChannel has been moved to "0"
              //if (!GUIWindowManager.IsRouted)
              //{
              //  GUITVHome.OnLastViewedChannel();
              //}
            }
          }
          break;

        case Action.ActionType.ACTION_SHOW_INFO:

        case Action.ActionType.ACTION_SHOW_CURRENT_TV_INFO:
          {
            if (action.fAmount1 != 0)
            {
              _zapTimeOutTimer = DateTime.MaxValue;
              _zapTimeOutTimer = DateTime.Now;
            }
            else
            {
              _zapTimeOutTimer = DateTime.Now;
            }

            if (!_zapOsdVisible)
            {
              if (!_useVMR9Zap)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _zapWindow.GetID, 0, 0,
                                                GetID, 0, null);
                if (_gotTvErrorMessage != null)
                {
                  _zapWindow.LastError = _gotTvErrorMessage;
                  _gotTvErrorMessage = null;
                }
                _zapWindow.OnMessage(msg);
                Log.Debug("ZAP OSD:ON");
                _zapTimeOutTimer = DateTime.Now;
                _zapOsdVisible = true;
              }
            }
            else
            {
              _zapWindow.UpdateChannelInfo();
              _zapTimeOutTimer = DateTime.Now;
            }
          }
          break;

          // msn related can be removed
          //case Action.ActionType.ACTION_SHOW_MSN_OSD:
          //  if (_isMsnChatPopup)
          //  {
          //    Log.Debug("MSN CHAT:ON");

          //    _msnWindowVisible = true;
          //    GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVMSNOSD;
          //    ///@
          //    ///_msnWindow.DoModal(GetID, null);
          //    _msnWindowVisible = false;
          //    GUIWindowManager.IsOsdVisible = false;
          //  }
          //  break;

        case Action.ActionType.ACTION_AUTOCROP:
          {
            Log.Debug("ACTION_AUTOCROP");
            _statusVisible = true;
            _statusTimeOutTimer = DateTime.Now;

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
            _statusVisible = true;
            _statusTimeOutTimer = DateTime.Now;
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
            _statusVisible = true;
            _statusTimeOutTimer = DateTime.Now;
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
            status = Utils.GetAspectRatioLocalizedString(arMode);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1,
                                            0, 0, null);
            msg.Label = status;
            OnMessage(msg);


          }
          break;

        case Action.ActionType.ACTION_NEXT_SUBTITLE:
          if (g_Player.SubtitleStreams > 0)
          {
            _statusVisible = true;
            _statusTimeOutTimer = DateTime.Now;

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1,
                                            0, 0, null);
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
            Log.Info("MyTV toggle subtitle: switched subtitle to {0}", msg.Label);
          }
          else
          {
            Log.Info("MyTV toggle subtitle: no subtitle streams available!");
          }
          break;

        case Action.ActionType.ACTION_PAGE_UP:
          OnPageUp();
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            // msn related can be removed
            //if ((action.m_key != null) && (!_msnWindowVisible))
            if (action.m_key != null)
            {
              OnKeyCode((char) action.m_key.KeyChar);
            }

            _messageBoxVisible = false;
          }
          break;

        case Action.ActionType.ACTION_REWIND:
          {
            if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
            {
              g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }

              ScreenStateChanged();
              UpdateGUI();
            }
          }
          break;

        case Action.ActionType.ACTION_FORWARD:
          {
            if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
            {
              g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }

              ScreenStateChanged();
              UpdateGUI();
            }
          }
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:

        case Action.ActionType.ACTION_SHOW_GUI:
          Log.Debug("fullscreentv:show gui");
          //if(_vmr9OSD!=null)
          //	_vmr9OSD.HideBitmap();
          GUIWindowManager.ShowPreviousWindow();
          return;

        case Action.ActionType.ACTION_SHOW_OSD: // Show the OSD
          {
            Log.Debug("OSD:ON");
            _osdTimeoutTimer = DateTime.Now;

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                            null);
            _osdWindow.OnMessage(msg); // Send an init msg to the OSD
            _isOsdVisible = true;
            GUIWindowManager.VisibleOsd = Window.WINDOW_TVOSD;
          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:

        case Action.ActionType.ACTION_STEP_BACK:
          {
            if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
                ScreenStateChanged();
                UpdateGUI();
              }
              _stepSeekVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              g_Player.SeekStep(false);
              string strStatus = g_Player.GetStepDescription();
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int) Control.LABEL_ROW1, 0, 0, null);
              msg.Label = strStatus;
              OnMessage(msg);
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:

        case Action.ActionType.ACTION_STEP_FORWARD:
          {
            if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
                ScreenStateChanged();
                UpdateGUI();
              }
              _stepSeekVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              g_Player.SeekStep(true);
              string strStatus = g_Player.GetStepDescription();
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int) Control.LABEL_ROW1, 0, 0, null);
              msg.Label = strStatus;
              OnMessage(msg);
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:

        case Action.ActionType.ACTION_BIG_STEP_BACK:
          {
            if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
                ScreenStateChanged();
                UpdateGUI();
              }
              _statusVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              if (_immediateSeekIsRelative)
              {
                g_Player.SeekRelativePercentage(-_immediateSeekValue);
              }
              else
              {
                g_Player.SeekRelative(-_immediateSeekValue);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:

        case Action.ActionType.ACTION_BIG_STEP_FORWARD:
          {
            if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
                ScreenStateChanged();
                UpdateGUI();
              }
              _statusVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              if (_immediateSeekIsRelative)
              {
                g_Player.SeekRelativePercentage(_immediateSeekValue);
              }
              else
              {
                g_Player.SeekRelative(_immediateSeekValue);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_PAUSE:
          {
            if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
            {
              g_Player.Pause();
            }

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
          }
          break;

        case Action.ActionType.ACTION_PLAY:

        case Action.ActionType.ACTION_MUSIC_PLAY:
          if (g_Player.IsTimeShifting || g_Player.IsTVRecording)
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

        case Action.ActionType.ACTION_AUTOZAP:
          StartAutoZap();
          break;

        case Action.ActionType.ACTION_AUDIO_NEXT_LANGUAGE:
        case Action.ActionType.ACTION_NEXT_AUDIO:
          {
            //IAudioStream[] streams = TVHome.Card.AvailableAudioStreams;

            if (g_Player.AudioStreams > 1)
            {
              int newIndex = 0;
              int oldIndex = 0;
              string audioLang = g_Player.AudioLanguage(oldIndex);
              oldIndex = g_Player.CurrentAudioStream;
              g_Player.SwitchToNextAudio();

              newIndex = g_Player.CurrentAudioStream;

              if (newIndex + 1 > g_Player.AudioStreams)
              {
                newIndex = 0;
              }

              Log.Debug("Switching from audio stream {0} to {1}", oldIndex, newIndex);

              // Show OSD Label
              _statusVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                              (int) Control.LABEL_ROW1, 0, 0, null);
              //msg.Label = string.Format("{0}:{1} ({2}/{3})", streams[newIndex].StreamType, streams[newIndex].Language, newIndex + 1, streams.Length);
              msg.Label = string.Format("{0}:{1} ({2}/{3})", g_Player.AudioType(newIndex),
                                        g_Player.AudioLanguage(newIndex), newIndex + 1, g_Player.AudioStreams);

              Log.Debug(msg.Label);
              OnMessage(msg);
            }
          }
          break;

        case Action.ActionType.ACTION_STOP:
          if (g_Player.IsTVRecording)
          {
            g_Player.Stop();
          }
          if (g_Player.IsTimeShifting)
          {
            Log.Debug("TVFullscreen: user request to stop");
            GUIDialogPlayStop dlgPlayStop =
              (GUIDialogPlayStop) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PLAY_STOP);
            if (dlgPlayStop != null)
            {
              dlgPlayStop.SetHeading(GUILocalizeStrings.Get(605));
              dlgPlayStop.SetLine(1, GUILocalizeStrings.Get(2550));
              dlgPlayStop.SetLine(2, GUILocalizeStrings.Get(2551));
              dlgPlayStop.SetDefaultToStop(false);
              dlgPlayStop.DoModal(GetID);
              if (dlgPlayStop.IsStopConfirmed)
              {
                Log.Debug("TVFullscreen: stop confirmed");
                g_Player.Stop();
              }
            }
          }
          break;
      }

      base.OnAction(action);
    }

    public override void SetObject(object obj)
    {
      base.SetObject(obj);
      ///@
      /// if (obj.GetType() == typeof(VMR9OSD))
      ///{
      ///  _vmr9OSD = (VMR9OSD)obj;

      ///}
    }

    public override bool OnMessage(GUIMessage message)
    {
      _needToClearScreen = true;

      #region case GUI_MSG_RECORD

      if (message.Message == GUIMessage.MessageType.GUI_MSG_RECORD)
      {
        if (_isDialogVisible)
        {
          return false;
        }

        Channel channel = TVHome.Navigator.Channel;

        if (channel == null)
        {
          return true;
        }

        TvBusinessLayer layer = new TvBusinessLayer();

        Program prog = channel.CurrentProgram;
        VirtualCard card;
        TvServer server = new TvServer();
        if (server.IsRecording(channel.Name, out card))
        {
          _dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
          _dlgYesNo.SetHeading(1449); // stop recording
          _dlgYesNo.SetLine(1, 1450); // are you sure to stop recording?
          if (prog != null)
          {
            _dlgYesNo.SetLine(2, prog.Title);
          }
          _dialogYesNoVisible = true;
          _dlgYesNo.DoModal(GetID);
          _dialogYesNoVisible = false;

          if (!_dlgYesNo.IsConfirmed)
          {
            return true;
          }

          Schedule s = Schedule.Retrieve(card.RecordingScheduleId);
          TVHome.PromptAndDeleteRecordingSchedule(s.IdSchedule, s.ReferencedChannel().CurrentProgram, false, true);

          GUIDialogNotify dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify == null)
          {
            return true;
          }
          string logo = Utils.GetCoverArt(Thumbs.TVChannel, channel.DisplayName);
          dlgNotify.Reset();
          dlgNotify.ClearAll();
          dlgNotify.SetImage(logo);
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1447)); //recording stopped
          if (prog != null)
          {
            dlgNotify.SetText(String.Format("{0} {1}-{2}",
                                            prog.Title,
                                            prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                            prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)));
          }
          else
          {
            dlgNotify.SetText(GUILocalizeStrings.Get(736)); //no tvguide data available
          }
          dlgNotify.TimeOut = 5;

          _notifyDialogVisible = true;
          dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
          TvNotifyManager.ForceUpdate();
          _notifyDialogVisible = false;
          return true;
        }
        else
        {
          if (prog != null)
          {
            _dialogBottomMenu =
              (GUIDialogMenuBottomRight) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
            if (_dialogBottomMenu != null)
            {
              _dialogBottomMenu.Reset();
              _dialogBottomMenu.SetHeading(605); //my tv
              //_dialogBottomMenu.SetHeadingRow2(prog.Title);              

              _dialogBottomMenu.AddLocalizedString(875); //current program
              _dialogBottomMenu.AddLocalizedString(876); //till manual stop
              _bottomDialogMenuVisible = true;

              _dialogBottomMenu.DoModal(GetID);

              _bottomDialogMenuVisible = false;
              switch (_dialogBottomMenu.SelectedId)
              {
                case 875:
                  //record current program
                  _isStartingTSForRecording = !g_Player.IsTimeShifting;

                  TVHome.StartRecordingSchedule(channel, false);
                  break;

                case 876:
                  //manual record
                  _isStartingTSForRecording = !g_Player.IsTimeShifting;

                  TVHome.StartRecordingSchedule(channel, true);
                  break;
                default:
                  return true;
              }
            }
          }
          else
          {
            _isStartingTSForRecording = !g_Player.IsTimeShifting;
            TVHome.StartRecordingSchedule(channel, true);
          }

          // check if recorder has to start timeshifting for this recording
          if (_isStartingTSForRecording)
          {
            Channel ch = Channel.Retrieve(TVHome.Card.IdChannel);
            TVHome.ViewChannel(ch);
            _isStartingTSForRecording = false;
          }

          GUIDialogNotify dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify == null)
          {
            return true;
          }
          string logo = Utils.GetCoverArt(Thumbs.TVChannel, channel.DisplayName);
          dlgNotify.Reset();
          dlgNotify.ClearAll();
          dlgNotify.SetImage(logo);
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1446)); //recording started
          if (prog != null)
          {
            dlgNotify.SetText(String.Format("{0} {1}-{2}",
                                            prog.Title,
                                            prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                            prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)));
          }
          else
          {
            dlgNotify.SetText(GUILocalizeStrings.Get(736)); //no tvguide data available
          }
          dlgNotify.TimeOut = 5;
          _notifyDialogVisible = true;
          dlgNotify.DoModal(GUIWindowManager.ActiveWindow);

          TvNotifyManager.ForceUpdate();


          _notifyDialogVisible = false;
        }
        return true;
      }

      #endregion

      #region case GUI_MSG_NOTIFY_TV_PROGRAM

      if (message.Message == GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM)
      {
        _dialogNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
        ///@
        /*
        Program notify = message.Object as Program;
        if (notify == null) return true;
        _dialogNotify.SetHeading(1016);
        _dialogNotify.SetText(String.Format("{0}\n{1}", notify.Title, notify.Description));
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, notify.Channel);
        _dialogNotify.SetImage(logo);
        _dialogNotify.TimeOut = _notifyTVTimeout;
        _notifyDialogVisible = true;
        if (_playNotifyBeep)
          Utils.PlaySound("notify.wav", false, true);
        _dialogNotify.DoModal(GetID);
        _notifyDialogVisible = false;
         * */
      }

      #endregion

      #region case GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING

      if (message.Message == GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING)
      {
        /*
                TVRecording rec = message.Object as TVRecording;
                if (rec == null) return true;
                if (rec.Channel == Recorder.TVChannelName) return true;
                if (!Recorder.NeedChannelSwitchForRecording(rec)) return true;

                _messageBoxVisible = false;
                _msnWindowVisible = false;     // msn related can be removed
                GUIWindowManager.IsOsdVisible = false;
                if (_zapOsdVisible)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0, GetID, 0, null);
                  _zapWindow.OnMessage(msg);
                  _zapOsdVisible = false;
                  GUIWindowManager.IsOsdVisible = false;
                }
                if (_isOsdVisible)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
                  _osdWindow.OnMessage(msg);
                  _isOsdVisible = false;
                  GUIWindowManager.IsOsdVisible = false;
                }
                if (_msnWindowVisible)     // msn related can be removed
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
                  _msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
                  _msnWindowVisible = false;
                  GUIWindowManager.IsOsdVisible = false;
                }
                if (_isDialogVisible && dlg != null)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, dlg.GetID, 0, 0, GetID, 0, null);
                  dlg.OnMessage(msg);	// Send a de-init msg to the OSD
                }

                _bottomDialogMenuVisible = true;
                _dialogBottomMenu = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
                _dialogBottomMenu.TimeOut = 10;
                _dialogBottomMenu.SetHeading(1004);//About to start recording
                _dialogBottomMenu.SetHeadingRow2(String.Format("{0} {1}", GUILocalizeStrings.Get(1005), rec.Channel));
                _dialogBottomMenu.SetHeadingRow3(rec.Title);
                _dialogBottomMenu.AddLocalizedString(1006); //Allow recording to begin
                _dialogBottomMenu.AddLocalizedString(1007); //Cancel recording and maintain watching tv
                _dialogBottomMenu.DoModal(GetID);
                if (_dialogBottomMenu.SelectedId == 1007) //cancel recording
                {
                  if (rec.RecType == TVRecording.RecordingType.Once)
                  {
                    rec.Canceled = Utils.datetolong(DateTime.Now);
                  }
                  else
                  {
                    Program prog = message.Object2 as Program;
                    if (prog != null)
                      rec.CanceledSeries.Add(prog.Start);
                    else
                      rec.CanceledSeries.Add(Utils.datetolong(DateTime.Now));
                  }
                  TVDatabase.UpdateRecording(rec, TVDatabase.RecordingChange.Canceled);
                }
         */
        _bottomDialogMenuVisible = false;
      }

      #endregion

      #region case GUI_MSG_NOTIFY

      if (message.Message == GUIMessage.MessageType.GUI_MSG_NOTIFY)
      {
        GUIDialogNotify dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
        if (dlgNotify == null)
        {
          return true;
        }
        string channel = GUIPropertyManager.GetProperty("#TV.View.channel");
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, channel);
        dlgNotify.Reset();
        dlgNotify.ClearAll();
        dlgNotify.SetImage(strLogo);
        dlgNotify.SetHeading(channel);
        dlgNotify.SetText(message.Label);
        dlgNotify.TimeOut = message.Param1;
        _notifyDialogVisible = true;
        dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
        _notifyDialogVisible = false;
        Log.Debug("Notify Message:" + channel + ", " + message.Label);
        return true;
      }

      #endregion
      #region case GUI_MSG_NOTIFY_TV_PROGRAM

      // TEST for TV error handling
      if (message.Message == GUIMessage.MessageType.GUI_MSG_TV_ERROR_NOTIFY)
      {
        UpdateOSD((TvPlugin.TVHome.ChannelErrorInfo)message.Object);
        return true;
      }

      #endregion
      #region case GUI_MSG_WINDOW_DEINIT

      if (_isOsdVisible)
      {
        if ((message.Message != GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT))
        {
          _osdTimeoutTimer = DateTime.Now;
          // route messages to OSD window
          if (_osdWindow.OnMessage(message))
          {
            return true;
          }
        }
        else if (message.Param1 == GetID)
        {
          _osdTimeoutTimer = DateTime.Now;
          _osdWindow.OnMessage(message);
        }
      }

      #endregion

      switch (message.Message)
      {
          #region case GUI_MSG_HIDE_MESSAGE

        case GUIMessage.MessageType.GUI_MSG_HIDE_MESSAGE:
          {
            _messageBoxVisible = false;
          }
          break;

          #endregion

          #region case GUI_MSG_SHOW_MESSAGE

        case GUIMessage.MessageType.GUI_MSG_SHOW_MESSAGE:
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,
                                            (int) Control.MSG_BOX_LABEL1, 0, 0, null);
            msg.Label = message.Label;
            OnMessage(msg);

            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.MSG_BOX_LABEL2, 0, 0,
                                 null);
            msg.Label = message.Label2;
            OnMessage(msg);

            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.MSG_BOX_LABEL3, 0, 0,
                                 null);
            msg.Label = message.Label3;
            OnMessage(msg);

            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.MSG_BOX_LABEL4, 0, 0,
                                 null);
            msg.Label = message.Label4;
            OnMessage(msg);

            _messageBoxVisible = true;
            // Set specified timeout
            _msgBoxTimeout = message.Param1;
            _msgTimer = DateTime.Now;
          }
          break;

          #endregion

          // msn related can be removed
          //#region case GUI_MSG_MSN_CLOSECONVERSATION

          //case GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION:
          //  if (_msnWindowVisible)
          //  {
          //    ///@
          //    /// GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
          //    ///_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
          //  }
          //  _msnWindowVisible = false;
          //  GUIWindowManager.IsOsdVisible = false;
          //  break;

          //#endregion

          // msn related can be removed
          //#region case GUI_MSG_MSN_STATUS_MESSAGE

          //case GUIMessage.MessageType.GUI_MSG_MSN_STATUS_MESSAGE:

          //#endregion

          // msn related can be removed
          //#region case GUI_MSG_MSN_MESSAGE
          //case GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE:
          //  if (_isOsdVisible && _isMsnChatPopup)
          //  {
          //    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
          //    _osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
          //    _isOsdVisible = false;
          //    GUIWindowManager.IsOsdVisible = false;

          //  }

          //  if (!_msnWindowVisible && _isMsnChatPopup)
          //  {
          //    Log.Debug("MSN CHAT:ON");
          //    _msnWindowVisible = true;
          //    GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVMSNOSD;
          //    ///@
          //    ///_msnWindow.DoModal(GetID, message);
          //    _msnWindowVisible = false;
          //    GUIWindowManager.IsOsdVisible = false;

          //  }
          //  break;
          //#endregion

          #region case GUI_MSG_WINDOW_DEINIT

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            Log.Debug("TvFullScreen:deinit->OSD:Off");
            if (_isOsdVisible)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0,
                                              GetID, 0, null);
              _osdWindow.OnMessage(msg); // Send a de-init msg to the OSD
            }
            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;

            // msn related can be removed
            //if (_msnWindowVisible)
            //{
            //  ///@
            //  /// GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
            //  ///_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            //}

            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            _channelInputVisible = false;
            _keyPressedTimer = DateTime.Now;
            _channelName = "";
            _updateTimer = DateTime.Now;

            _stepSeekVisible = false;
            _statusVisible = false;
            _groupVisible = false;
            _notifyDialogVisible = false;
            _dialogYesNoVisible = false;
            _bottomDialogMenuVisible = false;
            _statusTimeOutTimer = DateTime.Now;

            _screenState.ContextMenuVisible = false;
            _screenState.MsgBoxVisible = false;
            //_screenState.MsnVisible = false;      // msn related can be removed
            _screenState.OsdVisible = false;
            _screenState.Paused = false;
            _screenState.ShowGroup = false;
            _screenState.ShowInput = false;
            _screenState.ShowStatusLine = false;
            _screenState.ShowTime = false;
            _screenState.ZapOsdVisible = false;
            _needToClearScreen = false;


            base.OnMessage(message);
            GUIGraphicsContext.IsFullScreenVideo = false;
            if (!GUIGraphicsContext.IsTvWindow(message.Param1))
            {
              if (!g_Player.Playing)
              {
                if (GUIGraphicsContext.ShowBackground)
                {
                  // stop timeshifting & viewing... 

                  ///@
                  ///Recorder.StopViewing();
                }
              }
            }
            if (VMR7Util.g_vmr7 != null)
            {
              VMR7Util.g_vmr7.SaveBitmap(null, false, false, 0.8f);
            }
            /*
            if (VMR9Util.g_vmr9!=null)
            {	
              VMR9Util.g_vmr9.SaveBitmap(null,false,false,0.8f);
            }*/
            GUILayerManager.UnRegisterLayer(this);
            return true;
          }

          #endregion

          #region case GUI_MSG_WINDOW_INIT

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            LoadSettings();
            GUIGraphicsContext.IsFullScreenVideo = true;
            ///@
            GUIGraphicsContext.VideoWindow = new Rectangle(GUIGraphicsContext.OverScanLeft,
                                                           GUIGraphicsContext.OverScanTop,
                                                           GUIGraphicsContext.OverScanWidth,
                                                           GUIGraphicsContext.OverScanHeight);
            _osdWindow = (TvOsd) GUIWindowManager.GetWindow((int) Window.WINDOW_TVOSD);
            _zapWindow = (TvZapOsd) GUIWindowManager.GetWindow((int) Window.WINDOW_TVZAPOSD);
            ///_msnWindow = (GUITVMSNOSD)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVMSNOSD);     // msn related can be removed

            _lastPause = g_Player.Paused;
            _lastSpeed = g_Player.Speed;
            ///Log.Debug("start fullscreen channel:{0}", Recorder.TVChannelName);
            Log.Debug("TvFullScreen:init->OSD:Off");
            Log.Debug("TvFullScreen: init, playing {0}, player.CurrentFile {1}, TVHome.Card.TimeShiftFileName {2}",
                      g_Player.Playing, g_Player.CurrentFile, TVHome.Card.TimeShiftFileName);

            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            _channelInputVisible = false;
            _keyPressedTimer = DateTime.Now;
            _channelName = "";
            //					m_sZapChannel="";

            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            _updateTimer = DateTime.Now;
            //					_zapTimeOutTimer=DateTime.Now;

            _stepSeekVisible = false;
            _statusVisible = false;
            _groupVisible = false;
            _notifyDialogVisible = false;
            _dialogYesNoVisible = false;
            _bottomDialogMenuVisible = false;
            _statusTimeOutTimer = DateTime.Now;
            //imgVolumeBar.Current = VolumeHandler.Instance.Step;
						//imgVolumeBar.Maximum = VolumeHandler.Instance.StepMax;
						
						ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
						
						RenderVolume(_isVolumeVisible);
            ScreenStateChanged();
            UpdateGUI();

            ///@
            /// GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            ///try
            ///{
            ///  GUIGraphicsContext.DX9Device.Present();
            ///}
            ///catch (Exception)
            ///{
            ///}
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);


            return true;
          }

          #endregion

          #region case GUI_MSG_SETFOCUS

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;

          #endregion

          #region case GUI_MSG_LOSTFOCUS

        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
          if (_isOsdVisible)
          {
            return true;
          }
          //if (_msnWindowVisible) return true;     // msn related can be removed
          if (message.SenderControlId != (int) Window.WINDOW_TVFULLSCREEN)
          {
            return true;
          }
          break;

          #endregion
      }

      // msn related can be removed
      //if (_msnWindowVisible)
      //{
      //  ///@
      //  ///_msnWindow.OnMessage(message);	// route messages to MSNChat window
      //}
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
      dlg.SetHeading(924); // menu

      if (GUIGraphicsContext.DBLClickAsRightClick)
      {
        dlg.AddLocalizedString(10104); // TV MiniEPG
      }

      //dlg.AddLocalizedString(915); // TV Channels
      dlg.AddLocalizedString(4); // TV Guide

      TvBusinessLayer layer = new TvBusinessLayer();
      IList<ChannelLinkageMap> linkages = layer.GetLinkagesForChannel(TVHome.Navigator.Channel);
      if (linkages != null)
      {
        if (linkages.Count > 0)
        {
          dlg.AddLocalizedString(200042); // Linked Channels
        }
      }

      /*if (TVHome.Navigator.Groups.Count > 1)
        dlg.AddLocalizedString(971); // Group*/
      if (TVHome.Card.HasTeletext && !g_Player.IsTVRecording)
      {
        dlg.AddLocalizedString(1441); // Fullscreen teletext
      }
      dlg.AddLocalizedString(941); // Change aspect ratio

      // msn related can be removed
      //if (PluginManager.IsPluginNameEnabled("MSN Messenger"))
      //{
      //  dlg.AddLocalizedString(12902); // MSN Messenger
      //  dlg.AddLocalizedString(902); // MSN Online contacts
      //}

      //IAudioStream[] streams = TVHome.Card.AvailableAudioStreams;
      //if (streams != null && streams.Length > 0)
      if (g_Player.AudioStreams > 0)
      {
        dlg.AddLocalizedString(492); // Audio language menu
      }

      eAudioDualMonoMode dualMonoMode = g_Player.GetAudioDualMonoMode();
      if (dualMonoMode != eAudioDualMonoMode.UNSUPPORTED)
      {
        dlg.AddLocalizedString(200059); // Audio dual mono mode menu
      }

      dlg.AddLocalizedString(11000); // Crop settings

      if (!g_Player.IsTVRecording)
      {
        dlg.AddLocalizedString(100748); // Program Information
      }
      if (File.Exists(GUIGraphicsContext.Skin + @"\mytvtuningdetails.xml") && !g_Player.IsTVRecording)
      {
        dlg.AddLocalizedString(200041); // tuning details
      }

      VirtualCard vc;
      TvServer server = new TvServer();
      if (server.IsRecording(TVHome.Navigator.Channel.Name, out vc))
      {
        dlg.AddLocalizedString(265); //stop rec.
      }
      else
      {
        dlg.AddLocalizedString(601); //Record Now        
      }

      dlg.AddLocalizedString(970); // Previous window

      if (TVHome.Card.CiMenuSupported())
        dlg.AddLocalizedString(2700); // CI Menu supported

      //dlg.AddLocalizedString(6008); // Sort TvChannel

      if (TVHome.Card.IsOwner() && !TVHome.Card.IsRecording && TVHome.Card.SupportsQualityControl() &&
          !g_Player.IsTVRecording)
      {
        dlg.AddLocalizedString(882);
      }

      dlg.AddLocalizedString(368); // IMDB

      _isDialogVisible = true;

      dlg.DoModal(GetID);
      _isDialogVisible = false;

      Log.Debug("selected id:{0}", dlg.SelectedId);
      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 4: //TVGuide
          {
            TVGuideDialog dlgTvGuide = (TVGuideDialog) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_TVGUIDE);

            dlgTvGuide.GroupChanged = false;
            // do this in loop to reopen guide after change
            do {
              _isDialogVisible = true;
              dlgTvGuide.DoModal(GetID);
              _isDialogVisible = false;
            } while (dlgTvGuide.GroupChanged == true);
            break;
          }

        case 10104: // MiniEPG
          {
            Log.Debug("get miniguide");
            TvMiniGuide miniGuide = (TvMiniGuide) GUIWindowManager.GetWindow((int) Window.WINDOW_MINI_GUIDE);
            _isDialogVisible = true;
            Log.Debug("show miniguide");
            miniGuide.DoModal(GetID);
            Log.Debug("done miniguide");
            _isDialogVisible = false;
            break;
          }

          //TV Channels not needed anymore after MiniEPG
          /*case 915: //TVChannels
          {
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(915));//TV Channels
            int selected = 0;
            int i = 0;
            foreach (TVChannel channel in TVHome.Navigator.CurrentGroup.TvChannels)
            {
              GUIListItem pItem = new GUIListItem(channel.Name);
              string logo = Utils.GetCoverArt(Thumbs.TVChannel, channel.Name);
              if (System.IO.File.Exists(logo))
              {
                pItem.IconImage = logo;
              }
              dlg.Add(pItem);
              if (channel.Name == TVHome.Navigator.CurrentTVChannel.Name)
              {
                selected = i;
              }
              i++;
            }
            dlg.SelectedLabel = selected;
            _isDialogVisible = true;

            dlg.DoModal(GetID);
            _isDialogVisible = false;


            if (dlg.SelectedLabel == -1) return;
            ChangeChannelNr(dlg.SelectedLabel + 1);
          }
          break;*/

          //Groups not used anymore
          /*case 971: //group
          {
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(971));//Group
            foreach (TVGroup group in TVHome.Navigator.Groups)
            {
              dlg.Add(group.GroupName);
            }

            _isDialogVisible = true;

            dlg.DoModal(GetID);
            _isDialogVisible = false;


            if (dlg.SelectedLabel == -1) return;
            int selectedItem = dlg.SelectedLabel;
            if (selectedItem >= 0 && selectedItem < TVHome.Navigator.Groups.Count)
            {
              TVGroup group = (TVGroup)TVHome.Navigator.Groups[selectedItem];
              TVHome.Navigator.SetCurrentGroup(group.GroupName);
            }
          }
          break;*/

        case 941: // Change aspect ratio
          ShowAspectRatioMenu();
          break;

        case 2700: // Open CI Menu
          PrepareCiMenu();
          break;

        //case 6008: // TvChannel sort
        //  SortChannels();
        //  break;

        case 492: // Show audio language menu
          ShowAudioLanguageMenu();
          break;

        case 200059:
          ShowAudioDualMonoModeMenu(dualMonoMode);
          break;

          // msn related can be removed
          //case 12902: // MSN Messenger
          //  Log.Debug("MSN CHAT:ON");
          //  _msnWindowVisible = true;
          //  GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVMSNOSD;
          //  //@_msnWindow.DoModal(GetID, null);
          //  _msnWindowVisible = false;
          //  GUIWindowManager.IsOsdVisible = false;
          //  break;

          // msn related can be removed
          //case 902: // Online contacts
          //  GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
          //  break;

        case 1441: // Fullscreen teletext
          GUIWindowManager.ActivateWindow((int) Window.WINDOW_FULLSCREEN_TELETEXT);
          break;

        case 970:
          // switch back to previous window
          _isOsdVisible = false;
          //_msnWindowVisible = false;     // msn related can be removed
          GUIWindowManager.IsOsdVisible = false;
          GUIGraphicsContext.IsFullScreenVideo = false;
          GUIWindowManager.ShowPreviousWindow();
          break;

        case 11000:
          TvCropSettings cropSettings =
            (TvCropSettings) GUIWindowManager.GetWindow((int) Window.WINDOW_TV_CROP_SETTINGS);
          _isDialogVisible = true;
          cropSettings.DoModal(GetID);
          _isDialogVisible = false;
          break;

        case 100748: // Show Program Info
          ShowProgramInfo();
          break;

        case 601: // RecordNow          
        case 265: // StopRec.          
          TVHome.ManualRecord(TVHome.Navigator.Channel);
          break;

        case 200042: // Linked channels
          CacheManager.Clear();
          linkages = layer.GetLinkagesForChannel(TVHome.Navigator.Channel);
          ShowLinkedChannelsMenu(linkages);
          break;

        case 200041: // tuning details
          GUIWindowManager.ActivateWindow((int) Window.WINDOW_TV_TUNING_DETAILS);
          break;

        case 882: // Quality settings
          ShowQualitySettingsMenu();
          break;

        case 368: //IMDB
          OnGetIMDBInfo();
          break;
      }
    }

    private void OnGetIMDBInfo()
    {
      IMDBMovie movieDetails = new IMDBMovie();
      movieDetails.SearchString = GUIPropertyManager.GetProperty("#TV.View.title");
      if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, true, false))
      {
        GUIVideoInfo videoInfo = (GUIVideoInfo) GUIWindowManager.GetWindow((int) Window.WINDOW_VIDEO_INFO);
        videoInfo.Movie = movieDetails;
        GUIButtonControl btnPlay = (GUIButtonControl) videoInfo.GetControl(2);
        btnPlay.Visible = false;
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_VIDEO_INFO);
      }
      else
      {
        Log.Info("IMDB Fetcher: Nothing found");
      }
    }

    private void ShowQualitySettingsMenu()
    {
      if (TVHome.Card.SupportsBitRateModes() && TVHome.Card.SupportsPeakBitRateMode())
      {
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(965);
        dlg.AddLocalizedString(966);
        dlg.AddLocalizedString(967);
        VIDEOENCODER_BITRATE_MODE _newBitRate = TVHome.Card.BitRateMode;
        switch (_newBitRate)
        {
          case VIDEOENCODER_BITRATE_MODE.ConstantBitRate:
            dlg.SelectedLabel = 0;
            break;
          case VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage:
            dlg.SelectedLabel = 1;
            break;
          case VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak:
            dlg.SelectedLabel = 2;
            break;
        }
        _isDialogVisible = true;

        dlg.DoModal(GetID);
        _isDialogVisible = false;

        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedLabel)
        {
          case 0: // CBR
            _newBitRate = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
            break;

          case 1: // VBR
            _newBitRate = VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage;
            break;

          case 2: // VBR Peak
            _newBitRate = VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak;
            break;
        }
        Log.Info("Setting quality to: {0}", _newBitRate);
        TVHome.Card.BitRateMode = _newBitRate;
      }
      if (TVHome.Card.SupportsBitRate())
      {
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(886); //Default
        dlg.AddLocalizedString(993); // Custom
        dlg.AddLocalizedString(893); //Portable
        dlg.AddLocalizedString(883); //Low
        dlg.AddLocalizedString(884); //Medium
        dlg.AddLocalizedString(885); //High
        QualityType _newQuality = TVHome.Card.QualityType;
        switch (_newQuality)
        {
          case QualityType.Default:
            dlg.SelectedLabel = 0;
            break;
          case QualityType.Custom:
            dlg.SelectedLabel = 1;
            break;
          case QualityType.Portable:
            dlg.SelectedLabel = 2;
            break;
          case QualityType.Low:
            dlg.SelectedLabel = 3;
            break;
          case QualityType.Medium:
            dlg.SelectedLabel = 4;
            break;
          case QualityType.High:
            dlg.SelectedLabel = 5;
            break;
        }
        _isDialogVisible = true;

        dlg.DoModal(GetID);
        _isDialogVisible = false;

        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedLabel)
        {
          case 0: // Default
            _newQuality = QualityType.Default;
            break;

          case 1: // Custom
            _newQuality = QualityType.Custom;
            break;

          case 2: // Protable
            _newQuality = QualityType.Portable;
            break;

          case 3: // Low
            _newQuality = QualityType.Low;
            break;

          case 4: // Medium
            _newQuality = QualityType.Medium;
            break;

          case 5: // High
            _newQuality = QualityType.High;
            break;
        }
        TVHome.Card.QualityType = _newQuality;
      }
    }

    private void ShowProgramInfo()
    {
      if (TVHome.Navigator.Channel == null)
      {
        return;
      }
      Program currentProgram = TVHome.Navigator.GetChannel(TVHome.Navigator.Channel.IdChannel).CurrentProgram;

      if (currentProgram == null)
      {
        return;
      }

      TVProgramInfo.CurrentProgram = currentProgram;
      GUIWindowManager.ActivateWindow((int) Window.WINDOW_TV_PROGRAM_INFO);
    }

    private void ShowAspectRatioMenu()
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio

      // Add the allowed zoom  modes
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
      dlg.SelectedLabel = dlg.IndexOfItem(Utils.GetAspectRatioLocalizedString(GUIGraphicsContext.ARType));
      // show dialog and wait for result       
      _isDialogVisible = true;
      dlg.DoModal(GetID);
      _isDialogVisible = false;

      if (dlg.SelectedId == -1)
      {
        return;
      }
      _statusTimeOutTimer = DateTime.Now;

      string strStatus = "";

      GUIGraphicsContext.ARType = Utils.GetAspectRatioByLangID(dlg.SelectedId);
      strStatus = GUILocalizeStrings.Get(dlg.SelectedId);


      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1, 0, 0,
                                      null);
      msg.Label = strStatus;
      OnMessage(msg);
    }

    #region CI Menu 
    /// <summary>
    /// Sets callbacks and calls EnterCiMenu; Actions are done from callbacks and handled in TVHome globally
    /// </summary>
    private void PrepareCiMenu()
    {
      TVHome.RegisterCiMenu(TVHome.Card.Id); // Ensure listener attached
      TVHome.Card.EnterCiMenu(); // Enter menu. Dialog shows up on callback
    }
    #endregion

    private void SortChannels()
    {
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_SORT_CHANNELS);
      //ChannelSettings channelSettings = (ChannelSettings)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_SORT_CHANNELS);
    }

    private void ShowAudioLanguageMenu()
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(492); // set audio language menu

      dlg.ShowQuickNumbers = true;
      int selected = 0;
      int nrOfstreams = 0;

      int streamCurrent = g_Player.CurrentAudioStream;
      nrOfstreams = g_Player.AudioStreams;

      Log.Debug("TvFullScreen: ShowAudioLanguageMenu - got {0} audio streams", nrOfstreams);

      if (nrOfstreams >= streamCurrent)
      {
        selected = streamCurrent;
      }

      for (int i = 0; i < g_Player.AudioStreams; i++)
      {
        GUIListItem item = new GUIListItem();
        item.Label = String.Format("{0}:{1}", g_Player.AudioLanguage(i), g_Player.AudioType(i));
        dlg.Add(item);
      }

      dlg.SelectedLabel = selected;

      _isDialogVisible = true;

      dlg.DoModal(GetID);
      _isDialogVisible = false;

      if (dlg.SelectedLabel < 0)
      {
        return;
      }

      // Set new language			
      if ((dlg.SelectedLabel >= 0) && (dlg.SelectedLabel < nrOfstreams))
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

    private void ShowLinkedChannelsMenu(IList<ChannelLinkageMap> linkages)
    {
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(200042); // Linked channels menu
      int selected = 0;
      int counter = 0;
      foreach (ChannelLinkageMap map in linkages)
      {
        string channelName = map.ReferringLinkedChannel().DisplayName;
        GUIListItem item = new GUIListItem(channelName);
        if (channelName == TVHome.Navigator.CurrentChannel) //contains the display name
        {
          selected = counter;
        }
        dlg.Add(item);
        counter++;
      }
      dlg.SelectedLabel = selected;
      _isDialogVisible = true;
      dlg.DoModal(GetID);
      _isDialogVisible = false;
      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      ChannelLinkageMap lmap = (ChannelLinkageMap) linkages[dlg.SelectedLabel];
      TVHome.Navigator.ZapToChannel(lmap.ReferringLinkedChannel(), false);
    }

    public override void Process()
    {
      TimeSpan ts = DateTime.Now - _updateTimer;

      if (ts.TotalMilliseconds < 800)
      {
        return;
      }
      _updateTimer = DateTime.Now; // reset timer
      CheckTimeOuts();
      if (ScreenStateChanged())
      {
        UpdateGUI();
      }

      if ((_statusVisible || _stepSeekVisible || (!_isOsdVisible && g_Player.Speed != 1) ||
           (!_isOsdVisible && g_Player.Paused)) || _isOsdVisible)
      {
        TVHome.UpdateProgressPercentageBar();
      }
      else
      {
        // in fullscreen TV we still have to update the properties - since external displays depend on these.
        // one update per. minute should be enough
        TimeSpan tsProgressBar = DateTime.Now - _updateTimerProgressbar;

        if (tsProgressBar.TotalMilliseconds > 60000)
        {
          TVHome.UpdateProgressPercentageBar();
          _updateTimerProgressbar = DateTime.Now;
        }
      }

      if (!VideoRendererStatistics.IsVideoFound)
      {
        if ((lastChannelWithNoSignal != TVHome.Navigator.CurrentChannel) ||
            (videoState != VideoRendererStatistics.VideoState))
        {
          if (!_zapOsdVisible)
          {
            GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY, GetID, GetID, 0, 5, 0, null);
            switch (VideoRendererStatistics.VideoState)
            {
              case VideoRendererStatistics.State.NoSignal:
                message.Label = GUILocalizeStrings.Get(1034);
                break;
              case VideoRendererStatistics.State.Scrambled:
                message.Label = GUILocalizeStrings.Get(1035);
                break;
              case VideoRendererStatistics.State.Signal:
                message.Label = GUILocalizeStrings.Get(1036);
                break;
              default:
                message.Label = GUILocalizeStrings.Get(1036);
                break;
            }
            lastChannelWithNoSignal = TVHome.Navigator.CurrentChannel;
            videoState = VideoRendererStatistics.VideoState;
            OnMessage(message);
          }
        }
      }
      else
      {
        lastChannelWithNoSignal = string.Empty;
        videoState = VideoRendererStatistics.State.VideoPresent;
      }

      //check if the tv recording has reached the end ...if so, stop it.
      if (g_Player.IsTVRecording && !TvRecorded.IsLiveRecording())
      {
        double currentPosition = (double) (g_Player.CurrentPosition);
        double duration = (double) (g_Player.Duration);

        if (currentPosition > duration)
        {
          g_Player.Stop();
        }
      }

      GUIGraphicsContext.IsFullScreenVideo = true;
    }

    public bool ScreenStateChanged()
    {
      bool updateGUI = false;
      if (g_Player.Speed != _screenState.Speed)
      {
        _screenState.Speed = g_Player.Speed;
        updateGUI = true;
      }
      if (g_Player.Paused != _screenState.Paused)
      {
        _screenState.Paused = g_Player.Paused;
        updateGUI = true;
      }
      if (_isOsdVisible != _screenState.OsdVisible)
      {
        _screenState.OsdVisible = _isOsdVisible;
        updateGUI = true;
      }
      if (_zapOsdVisible != _screenState.ZapOsdVisible)
      {
        _screenState.ZapOsdVisible = _zapOsdVisible;
        updateGUI = true;
      }

      // msn related can be removed
      //if (_msnWindowVisible != _screenState.MsnVisible)
      //{
      //  _screenState.MsnVisible = _msnWindowVisible;
      //  updateGUI = true;
      //}
      if (_isDialogVisible != _screenState.ContextMenuVisible)
      {
        _screenState.ContextMenuVisible = _isDialogVisible;
        updateGUI = true;
      }

      bool bStart, bEnd;
      int step = g_Player.GetSeekStep(out bStart, out bEnd);
      if (step != _screenState.SeekStep)
      {
        if (step != 0)
        {
          _stepSeekVisible = true;
        }
        else
        {
          _stepSeekVisible = false;
        }
        _screenState.SeekStep = step;
        updateGUI = true;
      }
      if (_statusVisible != _screenState.ShowStatusLine)
      {
        _screenState.ShowStatusLine = _statusVisible;
        updateGUI = true;
      }
      if (_bottomDialogMenuVisible != _screenState._bottomDialogMenuVisible)
      {
        _screenState._bottomDialogMenuVisible = _bottomDialogMenuVisible;
        updateGUI = true;
      }
      if (_notifyDialogVisible != _screenState._notifyDialogVisible)
      {
        _screenState._notifyDialogVisible = _notifyDialogVisible;
        updateGUI = true;
      }
      if (_messageBoxVisible != _screenState.MsgBoxVisible)
      {
        _screenState.MsgBoxVisible = _messageBoxVisible;
        updateGUI = true;
      }
      if (_groupVisible != _screenState.ShowGroup)
      {
        _screenState.ShowGroup = _groupVisible;
        updateGUI = true;
      }
      if (_channelInputVisible != _screenState.ShowInput)
      {
        _screenState.ShowInput = _channelInputVisible;
        updateGUI = true;
      }
      if (_isVolumeVisible != _screenState.volumeVisible)
      {
        _screenState.volumeVisible = _isVolumeVisible;
        updateGUI = true;
        _volumeTimer = DateTime.Now;
      }
      if (_dialogYesNoVisible != _screenState._dialogYesNoVisible)
      {
        _screenState._dialogYesNoVisible = _dialogYesNoVisible;
        updateGUI = true;
      }

      if (updateGUI)
      {
        _needToClearScreen = true;
      }
      return updateGUI;
    }

    private void UpdateGUI()
    {
      if ((_statusVisible || _stepSeekVisible || (!_isOsdVisible && g_Player.Speed != 1) ||
           (!_isOsdVisible && g_Player.Paused)))
      {
        if (!_isOsdVisible)
        {
          for (int i = (int) Control.OSD_VIDEOPROGRESS; i < (int) Control.OSD_VIDEOPROGRESS + 50; ++i)
          {
            ShowControl(GetID, i);
          }

          // Set recorder status
          VirtualCard card;
          TvServer server = new TvServer();
          if (server.IsRecording(TVHome.Navigator.CurrentChannel, out card))
          {
            ShowControl(GetID, (int) Control.REC_LOGO);
          }
          else
          {
            HideControl(GetID, (int)Control.REC_LOGO);
          }
        }
        else
        {
          for (int i = (int) Control.OSD_VIDEOPROGRESS; i < (int) Control.OSD_VIDEOPROGRESS + 50; ++i)
          {
            HideControl(GetID, i);
          }
          HideControl(GetID, (int) Control.REC_LOGO);
        }
      }
      else
      {
        for (int i = (int) Control.OSD_VIDEOPROGRESS; i < (int) Control.OSD_VIDEOPROGRESS + 50; ++i)
        {
          HideControl(GetID, i);
        }
        HideControl(GetID, (int) Control.REC_LOGO);
      }


      if (g_Player.Paused)
      {
        ShowControl(GetID, (int) Control.IMG_PAUSE);
      }
      else
      {
        HideControl(GetID, (int) Control.IMG_PAUSE);
      }


      int speed = g_Player.Speed;
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

      switch (speed)
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

      HideControl(GetID, (int) Control.LABEL_ROW1);
      HideControl(GetID, (int) Control.LABEL_ROW2);
      HideControl(GetID, (int) Control.LABEL_ROW3);
      HideControl(GetID, (int) Control.BLUE_BAR);
      if (_screenState.SeekStep != 0)
      {
        ShowControl(GetID, (int) Control.BLUE_BAR);
        ShowControl(GetID, (int) Control.LABEL_ROW1);
      }
      if (_statusVisible)
      {
        ShowControl(GetID, (int) Control.BLUE_BAR);
        ShowControl(GetID, (int) Control.LABEL_ROW1);
      }
      if (_groupVisible || _channelInputVisible)
      {
        ShowControl(GetID, (int) Control.BLUE_BAR);
        ShowControl(GetID, (int) Control.LABEL_ROW1);
      }
      HideControl(GetID, (int) Control.MSG_BOX);
      HideControl(GetID, (int) Control.MSG_BOX_LABEL1);
      HideControl(GetID, (int) Control.MSG_BOX_LABEL2);
      HideControl(GetID, (int) Control.MSG_BOX_LABEL3);
      HideControl(GetID, (int) Control.MSG_BOX_LABEL4);

      if (_messageBoxVisible)
      {
        ShowControl(GetID, (int) Control.MSG_BOX);
        ShowControl(GetID, (int) Control.MSG_BOX_LABEL1);
        ShowControl(GetID, (int) Control.MSG_BOX_LABEL2);
        ShowControl(GetID, (int) Control.MSG_BOX_LABEL3);
        ShowControl(GetID, (int) Control.MSG_BOX_LABEL4);
      }

      RenderVolume(_isVolumeVisible);
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
      if (_groupVisible)
      {
        TimeSpan ts = (DateTime.Now - _groupTimeOutTimer);
        if (ts.TotalMilliseconds >= _zapTimeOutValue)
        {
          _groupVisible = false;
        }
      }

      if (_statusVisible || _stepSeekVisible)
      {
        TimeSpan ts = (DateTime.Now - _statusTimeOutTimer);
        if (ts.TotalMilliseconds >= 2000)
        {
          _stepSeekVisible = false;
          _statusVisible = false;
        }
      }

      if (_useVMR9Zap == true)
      {
        TimeSpan ts = DateTime.Now - _zapTimeOutTimer;
        if (ts.TotalMilliseconds > _zapTimeOutValue)
        {
          //if(_vmr9OSD!=null)
          //	_vmr9OSD.HideBitmap();
        }
      }
      //if(_vmr9OSD!=null)
      //	_vmr9OSD.CheckTimeOuts();


      // OSD Timeout?
      if (_isOsdVisible && _timeOsdOnscreen > 0)
      {
        TimeSpan ts = DateTime.Now - _osdTimeoutTimer;
        if (ts.TotalMilliseconds > _timeOsdOnscreen)
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


      OnKeyTimeout();


      if (_messageBoxVisible && _msgBoxTimeout > 0)
      {
        TimeSpan ts = DateTime.Now - _msgTimer;
        if (ts.TotalSeconds > _msgBoxTimeout)
        {
          _messageBoxVisible = false;
        }
      }


      // Let the navigator zap channel if needed
      TVHome.Navigator.CheckChannelChange();
      //Log.Debug("osd visible:{0} timeoutvalue:{1}", _zapOsdVisible ,_zapTimeOutValue);
      if (_zapOsdVisible && _zapTimeOutValue > 0)
      {
        TimeSpan ts = DateTime.Now - _zapTimeOutTimer;
        //Log.Debug("timeout :{0}", ts.TotalMilliseconds);
        if (ts.TotalMilliseconds > _zapTimeOutValue)
        {
          //yes, then remove osd offscreen
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0, GetID, 0,
                                          null);
          _zapWindow.OnMessage(msg); // Send a de-init msg to the OSD
          Log.Debug("ZAP OSD:Off timeout");
          _zapOsdVisible = false;
          msg = null;
        }
      }
    }

    public override void Render(float timePassed)
    {
      if (GUIWindowManager.IsSwitchingToNewWindow)
      {
        return;
      }
      /*
      if (VMR7Util.g_vmr7 != null)
      {
        if (!GUIWindowManager.IsRouted)
        {
          if (_screenState.ContextMenuVisible ||
            _screenState.MsgBoxVisible ||
            //_screenState.MsnVisible ||     // msn related can be removed
            _screenState.OsdVisible ||
            _screenState.Paused ||
            _screenState.ShowGroup ||
            _screenState.ShowInput ||
            _screenState.ShowStatusLine ||
            _screenState.ShowTime ||
            _screenState.ZapOsdVisible ||
            g_Player.Speed != 1 ||
            _needToClearScreen)
          {
            TimeSpan ts = DateTime.Now - _vmr7UpdateTimer;
            if ((ts.TotalMilliseconds >= 5000) || _needToClearScreen)
            {
              _needToClearScreen = false;
              using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width, GUIGraphicsContext.Height))
              {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                  GUIGraphicsContext.graphics = g;
                  base.Render(timePassed);
                  RenderForm(timePassed);
                  GUIGraphicsContext.graphics = null;
                  _screenState.wasVMRBitmapVisible = true;
                  VMR7Util.g_vmr7.SaveBitmap(bmp, true, true, 0.8f);
                }
              }
              _vmr7UpdateTimer = DateTime.Now;
            }
          }
          else
          {
            if (_screenState.wasVMRBitmapVisible)
            {
              _screenState.wasVMRBitmapVisible = false;
              VMR7Util.g_vmr7.SaveBitmap(null, false, false, 0.8f);
            }
          }
        }
      }*/

      if (GUIGraphicsContext.Vmr9Active)
      {
        base.Render(timePassed);
      }
      if (_isOsdVisible)
      {
        _osdWindow.Render(timePassed);
      }
      else if (_zapOsdVisible)
      {
        _zapWindow.Render(timePassed);
      }

      if (g_Player.Playing || TVHome.DoingChannelChange())
      {
        return;
      }
      if (_isStartingTSForRecording)
      {
        return;
      }

      //close window
      GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0,
                                       null);
      _osdWindow.OnMessage(msg2); // Send a de-init msg to the OSD
      msg2 = null;
      Log.Debug("timeout->OSD:Off");
      _isOsdVisible = false;
      GUIWindowManager.IsOsdVisible = false;

      //close window
      ///@
      /// msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
      ///_msnWindow.OnMessage(msg2);	// Send a de-init msg to the OSD
      /// msg2 = null;

      //_msnWindowVisible = false;     // msn related can be removed

      GUIWindowManager.IsOsdVisible = false;
      Log.Debug("Tvfullscreen:not viewing anymore");
      GUIWindowManager.ShowPreviousWindow();
    }

    public void UpdateOSD()
    {
      UpdateOSD(null);
    }
    public void UpdateOSD(TvPlugin.TVHome.ChannelErrorInfo message)
    {
      if (GUIWindowManager.ActiveWindow != GetID)
      {
        return;
      }
      Log.Debug("UpdateOSD()");
      if (_isOsdVisible)
      {
        _osdWindow.UpdateChannelInfo();
        _osdTimeoutTimer = DateTime.Now;
      }
      else
      {
        // FIXME: avoid globals
        _gotTvErrorMessage = message;

        Action myaction = new Action();
        //Show ZAP window indefinetely until channel has been tuned
        myaction.fAmount1 = -1;
        myaction.wID = Action.ActionType.ACTION_SHOW_INFO;
        this.OnAction(myaction);
        myaction = null;
      }
    }

    public void RenderForm(float timePassed)
    {
      if (_needToClearScreen)
      {
        _needToClearScreen = false;
        GUIGraphicsContext.graphics.Clear(Color.Black);
      }
      base.Render(timePassed);
      if (GUIGraphicsContext.graphics != null)
      {
        if (_isDialogVisible)
        {
          dlg.Render(timePassed);
        }

        // msn related can be removed
        ///if (_msnWindowVisible)
        ///_msnWindow.Render(timePassed);
      }
      // do we need 2 render the OSD?

      if (_isOsdVisible)
      {
        _osdWindow.Render(timePassed);
      }
      else if (_zapOsdVisible)
      {
        _zapWindow.Render(timePassed);
      }
    }

    private void HideControl(int idSender, int idControl)
    {
      GUIControl cntl = base.GetControl(idControl);
      if (cntl != null)
      {
        cntl.Visible = false;
      }
      cntl = null;
    }

    private void ShowControl(int idSender, int idControl)
    {
      GUIControl cntl = base.GetControl(idControl);
      if (cntl != null)
      {
        cntl.Visible = true;
      }
      cntl = null;
    }

    private void OnKeyTimeout()
    {
      if (_channelName.Length == 0)
      {
        return;
      }
      TimeSpan ts = DateTime.Now - _keyPressedTimer;
      if (ts.TotalMilliseconds >= 1000)
      {
        // change channel
        int iChannel = -1;
        Int32.TryParse(_channelName, out iChannel);
        if (iChannel > -1)
        {
          ChangeChannelNr(iChannel);
        }
        _channelInputVisible = false;
        _channelName = String.Empty;
      }
    }

    public void OnKeyCode(char chKey)
    {
      if (_isDialogVisible)
      {
        return;
      }
      if (GUIWindowManager.IsRouted)
      {
        return;
      }
      if (chKey == 'o')
      {
        Action showInfo = new Action(Action.ActionType.ACTION_SHOW_CURRENT_TV_INFO, 0, 0);
        OnAction(showInfo);
        return;
      }
      if (chKey == '0' && !_channelInputVisible)
      {
        TVHome.OnLastViewedChannel();
        if (!_zapOsdVisible)
        {
          if (!_useVMR9Zap)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _zapWindow.GetID, 0, 0, GetID, 0,
                                            null);
            _zapWindow.OnMessage(msg);
            Log.Debug("ZAP OSD:ON");
            _zapTimeOutTimer = DateTime.Now;
            _zapOsdVisible = true;
          }
        }
        else
        {
          _zapWindow.UpdateChannelInfo();
          _zapTimeOutTimer = DateTime.Now;
        }
        return;
      }
      if (chKey >= '0' && chKey <= '9') //Make sure it's only for the remote
      {
        _channelInputVisible = true;
        _keyPressedTimer = DateTime.Now;
        _channelName += chKey;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1, 0,
                                        0, null);

        string displayedChannelName = string.Empty;

        if (_byIndex)
        {
          int channelNr;
          if (!Int32.TryParse(_channelName, out channelNr))
          {
            return;
          }
          if (channelNr > TVHome.Navigator.CurrentGroup.ReferringGroupMap().Count)
          {
            return;
          }
          GroupMap map = (GroupMap) TVHome.Navigator.CurrentGroup.ReferringGroupMap()[channelNr - 1];
          displayedChannelName = map.ReferencedChannel().DisplayName;
        }
        /*else
          for (int ChannelCnt = 0; ChannelCnt < TVHome.Navigator.CurrentGroup.ReferringGroupMap().Count; ChannelCnt++)
          {
            GroupMap map = (GroupMap)TVHome.Navigator.CurrentGroup.ReferringGroupMap()[ChannelCnt];
            if (map.ReferencedChannel().SortOrder == Int32.Parse(_channelName))
            {
              displayedChannelName = map.ReferencedChannel().Name;
              break;
            }
          }*/
        if (displayedChannelName != string.Empty)
        {
          //not enough room for label "Channel"
          //msg.Label = String.Format("{0} {1} ({2})", GUILocalizeStrings.Get(602), _channelName, displayedChannelName);  // Channel
          msg.Label = String.Format("{0} {1} ({2})", "", _channelName, displayedChannelName); // Channel
        }
        else
        {
          //not enough room for label "Channel"
          //msg.Label = String.Format("{0} {1}", GUILocalizeStrings.Get(602), _channelName);  // Channel 
          msg.Label = String.Format("{0} {1}", "", _channelName); // Channel
        }

        GUIControl cntTarget = base.GetControl((int) Control.LABEL_ROW1);
        if (cntTarget != null)
        {
          cntTarget.OnMessage(msg);
        }
        cntTarget = null;

        if (_channelName.Length == 3)
        {
          // Change channel immediately          
          int iChannel = -1;
          Int32.TryParse(_channelName, out iChannel);
          if (iChannel > -1)
          {
            ChangeChannelNr(iChannel);
          }
          _channelInputVisible = false;
          _channelName = "";
        }
      }
    }

    /*
      List<TVChannel> channels = CurrentGroup.TvChannels;
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        TVChannel chan = (TVChannel)channels[channelNr];
        ZapToChannel(chan.Name, useZapDelay);
      }
     */


    /*
          List<TVChannel> channels = CurrentGroup.TvChannels;
      if (channelNr >= 0)
      {
        bool found = false;
        int ChannelCnt = 0;
        TVChannel chan;
        while (found == false && ChannelCnt < channels.Count)
        {
          chan = (TVChannel)channels[ChannelCnt];
          if (chan.Number == channelNr)
          {
            ZapToChannel(chan.Name, useZapDelay);
            found = true;
          }
          else
            ChannelCnt++;
        }
      }
*/

    private void OnPageDown()
    {
      // Switch to the next channel group and tune to the first channel in the group
      TVHome.Navigator.ZapToPreviousGroup(true);
      _groupVisible = true;
      _groupTimeOutTimer = DateTime.Now;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1, 0, 0,
                                      null);
      msg.Label = String.Format("{0}:{1}", GUILocalizeStrings.Get(971), TVHome.Navigator.ZapGroupName);
      OnMessage(msg);
    }

    private void OnPageUp()
    {
      // Switch to the next channel group and tune to the first channel in the group
      TVHome.Navigator.ZapToNextGroup(true);
      _groupVisible = true;
      _groupTimeOutTimer = DateTime.Now;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int) Control.LABEL_ROW1, 0, 0,
                                      null);
      msg.Label = String.Format("{0}:{1}", GUILocalizeStrings.Get(971), TVHome.Navigator.ZapGroupName);
      OnMessage(msg);
    }

    private void ChangeChannelNr(int channelNr)
    {
      Log.Debug("ChangeChannelNr()");
      if (_byIndex == true)
      {
        TVHome.Navigator.ZapToChannel(channelNr, false);
      }
      else
      {
        TVHome.Navigator.ZapToChannelNumber(channelNr, false);
      }

      UpdateOSD();
      _zapTimeOutTimer = DateTime.Now;
    }

    public void ZapPreviousChannel()
    {
      Log.Debug("ZapPreviousChannel()");
      TVHome.Navigator.ZapToPreviousChannel(true);
      _zapTimeOutTimer = DateTime.Now;
      UpdateOSD();
      ///@
      ///if (_useVMR9Zap == true && _vmr9OSD != null)
      {
        //_vmr9OSD.RenderChannelList(TVHome.Navigator.CurrentGroup,TVHome.Navigator.ZapChannel);
      }
    }

    public void ZapNextChannel()
    {
      Log.Debug("ZapNextChannel()");
      TVHome.Navigator.ZapToNextChannel(true);
      _zapTimeOutTimer = DateTime.Now;
      UpdateOSD();
      ///@
      ///if (_useVMR9Zap == true && _vmr9OSD != null)
      {
        //_vmr9OSD.RenderChannelList(TVHome.Navigator.CurrentGroup,TVHome.Navigator.ZapChannel);
      }
    }

    public void StartAutoZap()
    {
      Log.Debug("TVFullscreen: Start autozap mode");
      _autoZapMode = true;
      _autoZapTimer.Elapsed += new ElapsedEventHandler(_autoZapTimer_Elapsed);
      using (Settings xmlreader = new MPSettings())
      {
        _autoZapTimer.Interval = xmlreader.GetValueAsInt("capture", "autoZapTimer", 10000);
      }
      _autoZapTimer.Start();
      _autoZapTimer_Elapsed(null, null);
    }

    private void _autoZapTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (_autoZapMode)
      {
        ZapNextChannel();
      }
      else
      {
        _autoZapTimer.Stop();
      }
    }

    public void StopAutoZap()
    {
      Log.Debug("Stop zap mode");
      _autoZapMode = false;
    }

    public override int GetFocusControlId()
    {
      if (_isOsdVisible)
      {
        return _osdWindow.GetFocusControlId();
      }

      // msn related can be removed
      //if (_msnWindowVisible)
      //{
      //  ///@
      //  ///return _msnWindow.GetFocusControlId();
      //}

      return base.GetFocusControlId();
    }

    public override GUIControl GetControl(int iControlId)
    {
      if (_isOsdVisible)
      {
        return _osdWindow.GetControl(iControlId);
      }

      // msn related can be removed
      //if (_msnWindowVisible)
      //{
      //  ///@
      //  ///return _msnWindow.GetControl(iControlId);
      //}

      return base.GetControl(iControlId);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      _autoZapMode = false;
      _autoZapTimer.Dispose();

      ///@
      /*
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }*/
 
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      _autoZapTimer = new Timer();
      base.OnPageLoad();
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

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      //TVHome.SendHeartBeat(); //not needed, now sent from tvoverlay.cs
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion

    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsInstance(fetcher))
      {
        pDlgProgress.DisableCancel(true);
      }
      return true;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (!GUIWindowManager.IsRouted)
      {
        return;
      }
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.SetLine(1, line1);
      pDlgProgress.SetLine(2, line2);
      if (percent > 0)
      {
        pDlgProgress.SetPercentage(percent);
      }
      pDlgProgress.Progress();
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're busy querying www.imdb.com
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(GUILocalizeStrings.Get(197));
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      Log.Info("IMDB Fetcher: OnMovieNotFound");
      // show dialog...
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      pDlgOK.SetHeading(195);
      pDlgOK.SetLine(1, fetcher.MovieName);
      pDlgOK.SetLine(2, string.Empty);
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the movie info
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(GUILocalizeStrings.Get(198));
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      // won't occure
      return true;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      // won't occure
      return true;
    }

    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      // won't occure
      return true;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      Log.Info("IMDB Fetcher: OnDetailsNotFound");
      // show dialog...
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      // show dialog...
      pDlgOK.SetHeading(195);
      pDlgOK.SetLine(1, fetcher.MovieName);
      pDlgOK.SetLine(2, string.Empty);
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      // won't occure
      movieName = "";
      return true;
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      // won't occure
      selectedMovie = 0;
      return true;
    }

    public bool OnScanStart(int total)
    {
      // won't occure
      return true;
    }

    public bool OnScanEnd()
    {
      // won't occure
      return true;
    }

    public bool OnScanIterating(int count)
    {
      // won't occure
      return true;
    }

    public bool OnScanIterated(int count)
    {
      // won't occure
      return true;
    }

    #endregion

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        return true;
      }
      return false;
    }
  }
}
