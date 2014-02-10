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

#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Media.Animation;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using TvControl;
using TvDatabase;
using Action = MediaPortal.GUI.Library.Action;
using TvLibrary.Epg;

#endregion

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvGuideBase : GuideBase, IMDB.IProgress
  {
    #region constants

    private int _loopDelay = 100; // wait at the last item this amount of msec until loop to the first item

    private const string _skinPropertyPrefix = "#TV";

    #endregion

    protected override string SkinPropertyPrefix
    {
      get { return _skinPropertyPrefix; }
    }

    #region variables

    private Channel _recordingExpected = null;
    private DateTime _updateTimerRecExpected = DateTime.Now;
    private IList<Schedule> _recordingList = new List<Schedule>();
    private Dictionary<int, GUIButton3PartControl> _controls = new Dictionary<int, GUIButton3PartControl>();

    private string _currentTitle = String.Empty;
    private string _currentTime = String.Empty;
    private bool _currentRecOrNotify = false;
    private long _currentStartTime = 0;
    private long _currentEndTime = 0;
    private Program _currentProgram = null;
    private bool _needUpdate = false;
    private DateTime m_dtStartTime = DateTime.Now;
    private int _programOffset = 0;
    private int _totalProgramCount = 0;
    private TvServer _server = null;
    private IList<Program> _programs = null;

    private int _backupCursorX = 0;
    private int _backupCursorY = 0;
    private int _backupChannelOffset = 0;

    private DateTime _keyPressedTimer = DateTime.Now;
    private string _lineInput = String.Empty;

    private bool _byIndex = false;
    private bool _showChannelNumber = false;
    private int _channelNumberMaxLength = 3;
    private bool _useNewRecordingButtonColor = false;
    private bool _useNewPartialRecordingButtonColor = false;
    private bool _useNewNotifyButtonColor = false;
    private bool _recalculateProgramOffset;
    private bool _useHdProgramIcon = false;
    private string _hdtvProgramText = String.Empty;
    private bool _guideContinuousScroll = false;

    // current minimum/maximum indexes
    //private int MaxXIndex; // means rows here (channels)
    private int MinYIndex; // means cols here (programs/time)

    protected double _lastCommandTime = 0;

    /// <summary>
    /// Logic to decide if channel group button is available and visible
    /// </summary>
    protected bool GroupButtonAvail
    {
      get
      {
        // show/hide channel group button
        GUIControl btnChannelGroup = GetControl((int)Controls.CHANNEL_GROUP_BUTTON) as GUIControl;

        // visible only if more than one group? and not in single channel, and button exists in skin!
        return (TVHome.Navigator.Groups.Count > 1 && !_singleChannelView && btnChannelGroup != null);
      }
    }

    #endregion

    #region ctor

    public TvGuideBase()
    {
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        String channelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
        TvBusinessLayer layer = new TvBusinessLayer();
        IList<Channel> channels = layer.GetChannelsByName(channelName);
        if (channels != null && channels.Count > 0)
        {
          _currentChannel = channels[0];
        }
        PositionGuideCursorToCurrentChannel();

        _byIndex = xmlreader.GetValueAsBool("mytv", "byindex", true);
        _showChannelNumber = xmlreader.GetValueAsBool("mytv", "showchannelnumber", false);
        _channelNumberMaxLength = xmlreader.GetValueAsInt("mytv", "channelnumbermaxlength", 3);
        _timePerBlock = xmlreader.GetValueAsInt("tvguide", "timeperblock", 30);
        _hdtvProgramText = xmlreader.GetValueAsString("mytv", "hdtvProgramText", "(HDTV)");
        _guideContinuousScroll = xmlreader.GetValueAsBool("mytv", "continuousScrollGuide", false);
        _loopDelay = xmlreader.GetValueAsInt("gui", "listLoopDelay", 0);

        // Load the genre map.
        if (_mpGenres == null)
        {
          _mpGenres = RemoteControl.Instance.GetMpGenres();
        }
      }

      // Load settings defined by the skin.
      LoadSkinSettings();

      // Load genre colors.
      // If guide colors have not been loaded then attempt to load guide colors.
      if (!_guideColorsLoaded)
      {
        using (Settings xmlreader = new SKSettings())
        {
          _guideColorsLoaded = LoadGuideColors(xmlreader);
        }
      }

      _useNewRecordingButtonColor =
        Utils.FileExistsInCache(GUIGraphicsContext.GetThemedSkinFile(@"\media\tvguide_recButton_Focus_middle.png"));
      _useNewPartialRecordingButtonColor =
        Utils.FileExistsInCache(GUIGraphicsContext.GetThemedSkinFile(@"\media\tvguide_partRecButton_Focus_middle.png"));
      _useNewNotifyButtonColor =
        Utils.FileExistsInCache(GUIGraphicsContext.GetThemedSkinFile(@"\media\tvguide_notifyButton_Focus_middle.png"));
      _useHdProgramIcon =
        Utils.FileExistsInCache(GUIGraphicsContext.GetThemedSkinFile(@"\media\tvguide_hd_program.png"));
    }

    protected override void LoadSkinSettings()
    {
      String temp;

      // Guide coloring options are defined by each skin (and may not be present).  Read the settings from skin properties.
      _useColorsForButtons = false;
      temp = GUIPropertyManager.GetProperty("#skin.tvguide.usecolorsforbuttons");
      if (temp != null && temp.Length != 0)
      {
        _useColorsForButtons = bool.Parse(temp);
      }

      _useBorderHighlight = false;
      temp = GUIPropertyManager.GetProperty("#skin.tvguide.useborderhighlight");
      if (temp != null && temp.Length != 0)
      {
        _useBorderHighlight = bool.Parse(temp);
      }

      _useColorsForGenres = false;
      temp = GUIPropertyManager.GetProperty("#skin.tvguide.usecolorsforgenre");
      if (temp != null && temp.Length != 0)
      {
        _useColorsForGenres = bool.Parse(temp);
      }

      _showGenreKey = false;
      temp = GUIPropertyManager.GetProperty("#skin.tvguide.showgenrekey");
      if (temp != null && temp.Length != 0)
      {
        _showGenreKey = bool.Parse(temp);
      }
    }

    private bool LoadGuideColors(Settings xmlreader)
    {
      List<string> temp;

      // Load supporting guide colors.
      _guideColorChannelButton = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorchannelbutton", "ff0e517b"));
      _guideColorChannelButtonSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorchannelbuttonselected", "Green"));
      _guideColorGroupButton = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorgroupbutton", "ff0e517b"));
      _guideColorGroupButtonSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorgroupbuttonselected", "Green"));
      _guideColorProgramSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorprogramselected", "Green"));
      _guideColorProgramEnded = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorprogramended", "Gray"));
      _guideColorBorderHighlight = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorborderhighlight", "99ffffff"));

      // Load the default genre colors.
      temp = new List<string>((xmlreader.GetValueAsString("tvguidecolors", "defaultgenre", String.Empty)).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
      if (temp.Count == 2)
      {
        _defaultGenreColorOnNow = GetColorFromString(temp[0]);
        _defaultGenreColorOnLater = GetColorFromString(temp[1]);
      }
      else if (temp.Count == 1)
      {
        _defaultGenreColorOnNow = GetColorFromString(temp[0]);
        _defaultGenreColorOnLater = _defaultGenreColorOnNow;
      }
      else
      {
        _defaultGenreColorOnNow = 0xff1d355b; // Dark blue
        _defaultGenreColorOnLater = 0xff0e517b; // Light blue
      }

      // Each genre color entry is a csv list.  The first value is the color for program "on now", the second value is for program "on later".
      // If only one value is provided then that value is used for both.
      long color0;
      long color1;
      MpGenre genreObj;

      for (int i = 0; i < _mpGenres.Count; i++)
      {
        // If the genre is disabled then set the program colors to the default colors.
        genreObj = _mpGenres.Find(x => x.Id == i);
        if (!genreObj.Enabled)
        {
          _genreColorsOnNow.Add(_mpGenres[i].Name, _defaultGenreColorOnNow);
          _genreColorsOnLater.Add(_mpGenres[i].Name, _defaultGenreColorOnLater);
          continue;
        }

        temp = new List<string>((xmlreader.GetValueAsString("tvguidecolors", "genre" + i.ToString(), String.Empty)).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

        if (temp.Count > 0)
        {
          color0 = GetColorFromString(temp[0]);
          color1 = color0;
          if (temp.Count == 2)
          {
            color1 = GetColorFromString(temp[1]);
            _genreColorsOnNow.Add(_mpGenres[i].Name, color0);
            _genreColorsOnLater.Add(_mpGenres[i].Name, color1);
          }
          else if (temp.Count == 1)
          {
            _genreColorsOnNow.Add(_mpGenres[i].Name, color0);
            _genreColorsOnLater.Add(_mpGenres[i].Name, color1);
          }
        }
      }

      return true;
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        if (_currentChannel != null)
        {
          xmlwriter.SetValue("mytv", "channel", _currentChannel.DisplayName);
        }
        xmlwriter.SetValue("tvguide", "timeperblock", _timePerBlock);
      }
    }

    #endregion

    #region overrides

    public override int GetFocusControlId()
    {
      int focusedId = base.GetFocusControlId();
      if (_cursorX >= 0 ||
          focusedId == (int)Controls.SPINCONTROL_DAY ||
          focusedId == (int)Controls.SPINCONTROL_TIME_INTERVAL ||
          focusedId == (int)Controls.CHANNEL_GROUP_BUTTON
         )
      {
        return focusedId;
      }
      else
      {
        return -1;
      }
    }

    protected void Initialize()
    {
      _server = new TvServer();
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          if (_singleChannelView)
          {
            OnSwitchMode();
            return; // base.OnAction would close the EPG as well
          }
          else
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }

        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
          {
            OnKeyCode((char)action.m_key.KeyChar);
          }
          break;

        case Action.ActionType.ACTION_RECORD:
          if ((GetFocusControlId() != -1) && (_cursorY > 0) && (_cursorX >= 0))
          {
            OnRecord();
          }
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int x = (int)action.fAmount1;
            int y = (int)action.fAmount2;
            foreach (GUIControl control in controlList)
            {
              if (control.GetID >= (int)Controls.IMG_CHAN1 + 0 &&
                  control.GetID <= (int)Controls.IMG_CHAN1 + _channelCount)
              {
                if (x >= control.XPosition && x < control.XPosition + control.Width)
                {
                  if (y >= control.YPosition && y < control.YPosition + control.Height)
                  {
                    UnFocus();
                    _cursorX = control.GetID - (int)Controls.IMG_CHAN1;
                    _cursorY = 0;

                    if (_singleChannelNumber != _cursorX + ChannelOffset)
                    {
                      Update(false);
                    }
                    UpdateCurrentProgram();
                    UpdateHorizontalScrollbar();
                    UpdateVerticalScrollbar();
                    updateSingleChannelNumber();
                    return;
                  }
                }
              }
              if (control.GetID >= GUIDE_COMPONENTID_START)
              {
                if (x >= control.XPosition && x < control.XPosition + control.Width)
                {
                  if (y >= control.YPosition && y < control.YPosition + control.Height)
                  {
                    int iControlId = control.GetID;
                    if (iControlId >= GUIDE_COMPONENTID_START)
                    {
                      iControlId -= GUIDE_COMPONENTID_START;
                      int iCursorY = (iControlId / RowID);
                      iControlId -= iCursorY * RowID;
                      if (iControlId % ColID == 0)
                      {
                        int iCursorX = (iControlId / ColID) + 1;
                        if (iCursorY != _cursorX || iCursorX != _cursorY)
                        {
                          UnFocus();
                          _cursorX = iCursorY;
                          _cursorY = iCursorX;
                          UpdateCurrentProgram();
                          SetFocus();
                          UpdateHorizontalScrollbar();
                          UpdateVerticalScrollbar();
                          updateSingleChannelNumber();
                          return;
                        }
                        return;
                      }
                    }
                  }
                }
              }
            }
            UnFocus();
            _cursorY = -1;
            _cursorX = -1;
            base.OnAction(action);
          }
          break;

        case Action.ActionType.ACTION_TVGUIDE_RESET:
          _cursorY = 0;
          _viewingTime = DateTime.Now;
          Update(false);
          break;


        case Action.ActionType.ACTION_CONTEXT_MENU:
          {
            if (_cursorY >= 0 && _cursorX >= 0)
            {
              if (_cursorY == 0)
              {
                OnSwitchMode();
                return;
              }
              else
              {
                ShowContextMenu();
              }
            }
            else
            {
              action.wID = Action.ActionType.ACTION_SELECT_ITEM;
              GUIWindowManager.OnAction(action);
            }
          }
          break;

        case Action.ActionType.ACTION_PAGE_UP:
          OnPageUp();
          updateSingleChannelNumber();
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          updateSingleChannelNumber();
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (_cursorX >= 0)
            {
              OnLeft();
              updateSingleChannelNumber();
              UpdateHorizontalScrollbar();
              return;
            }
          }
          break;
        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            if (_cursorX >= 0)
            {
              OnRight();
              UpdateHorizontalScrollbar();
              return;
            }
          }
          break;
        case Action.ActionType.ACTION_MOVE_UP:
          {
            if (_cursorX >= 0)
            {
              OnUp(true, false);
              updateSingleChannelNumber();
              UpdateVerticalScrollbar();
              return;
            }
          }
          break;
        case Action.ActionType.ACTION_MOVE_DOWN:
          {
            if (_cursorX >= 0)
            {
              OnDown(true);
              updateSingleChannelNumber();
              UpdateVerticalScrollbar();
            }
            else
            {
              _cursorX = 0;
              SetFocus();
              updateSingleChannelNumber();
              UpdateVerticalScrollbar();
            }
            return;
          }
          //break;
        case Action.ActionType.ACTION_SHOW_INFO:
          {
            ShowContextMenu();
          }
          break;
        case Action.ActionType.ACTION_INCREASE_TIMEBLOCK:
          {
            _timePerBlock += 15;
            if (_timePerBlock > 60)
            {
              _timePerBlock = 60;
            }
            GUISpinControl cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            cntlTimeInterval.Value = (_timePerBlock / 15) - 1;
            Update(false);
            SetFocus();
          }
          break;

        case Action.ActionType.ACTION_REWIND:
        case Action.ActionType.ACTION_MUSIC_REWIND:
          _viewingTime = _viewingTime.AddHours(-3);
          Update(false);
          SetFocus();
          break;

        case Action.ActionType.ACTION_FORWARD:
        case Action.ActionType.ACTION_MUSIC_FORWARD:
          _viewingTime = _viewingTime.AddHours(3);
          Update(false);
          SetFocus();
          break;

        case Action.ActionType.ACTION_DECREASE_TIMEBLOCK:
          {
            if (_timePerBlock > 15)
            {
              _timePerBlock -= 15;
            }
            GUISpinControl cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            cntlTimeInterval.Value = (_timePerBlock / 15) - 1;
            Update(false);
            SetFocus();
          }
          break;
        case Action.ActionType.ACTION_DEFAULT_TIMEBLOCK:
          {
            _timePerBlock = 30;
            GUISpinControl cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            cntlTimeInterval.Value = (_timePerBlock / 15) - 1;
            Update(false);
            SetFocus();
          }
          break;
        case Action.ActionType.ACTION_TVGUIDE_INCREASE_DAY:
          OnNextDay();
          break;

        case Action.ActionType.ACTION_TVGUIDE_DECREASE_DAY:
          OnPreviousDay();
          break;
          // TV group changing actions
        case Action.ActionType.ACTION_TVGUIDE_NEXT_GROUP:
          OnChangeChannelGroup(1);
          break;

        case Action.ActionType.ACTION_TVGUIDE_PREV_GROUP:
          OnChangeChannelGroup(-1);
          break;
      }
      base.OnAction(action);
    }

    protected void OnNotify()
    {
      if (_currentProgram == null)
      {
        return;
      }

      _currentProgram.Notify = !_currentProgram.Notify;

      // get the right db instance of current prog before we store it
      // currentProgram is not a ref to the real entity
      Program modifiedProg = Program.RetrieveByTitleTimesAndChannel(_currentProgram.Title, _currentProgram.StartTime,
                                                                    _currentProgram.EndTime, _currentProgram.IdChannel);
      modifiedProg.Notify = _currentProgram.Notify;
      modifiedProg.Persist();
      TvNotifyManager.OnNotifiesChanged();
      Update(false);
      SetFocus();
    }

    /// <summary>
    /// changes the current channel group and refreshes guide display
    /// </summary>
    /// <param name="Direction"></param>
    protected virtual void OnChangeChannelGroup(int Direction)
    {
      // in single channel view there would be errors when changing group
      if (_singleChannelView) return;
      int newIndex, oldIndex;
      int countGroups = TVHome.Navigator.Groups.Count; // all

      newIndex = oldIndex = TVHome.Navigator.CurrentGroupIndex;
      if (
        (newIndex >= 1 && Direction < 0) ||
        (newIndex < countGroups - 1 && Direction > 0)
       )
      {
        newIndex += Direction; // change group
      }
      else // Cycle handling
        if ((newIndex == countGroups - 1) && Direction > 0)
        {
          newIndex = 0;
        }
        else if (newIndex == 0 && Direction < 0)
        {
          newIndex = countGroups - 1;
        }

      if (oldIndex != newIndex)
      {
        // update list
        GUIWaitCursor.Show();
        TVHome.Navigator.SetCurrentGroup(newIndex);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Group", TVHome.Navigator.CurrentGroup.GroupName);

        GetChannels(true);
        PositionGuideCursorToCurrentChannel();
        Update(false);
        SetFocus();
        GUIWaitCursor.Hide();
      }
    }

    private void UpdateOverlayAllowed()
    {
      if (_isOverlayAllowedCondition == 0)
      {
        return;
      }
      bool bWasAllowed = GUIGraphicsContext.Overlay;
      _isOverlayAllowed = GUIInfoManager.GetBool(_isOverlayAllowedCondition, GetID);
      if (bWasAllowed != _isOverlayAllowed)
      {
        GUIGraphicsContext.Overlay = _isOverlayAllowed;
      }
    }


    public override bool OnMessage(GUIMessage message)
    {
      try
      {
        switch (message.Message)
        {
          case GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED:
            if (message.SenderControlId == (int)Controls.HORZ_SCROLLBAR)
            {
              _needUpdate = true;
              float fPercentage = (float)message.Param1;
              fPercentage /= 100.0f;
              fPercentage *= 24.0f;
              fPercentage *= 60.0f;
              _viewingTime = new DateTime(_viewingTime.Year, _viewingTime.Month, _viewingTime.Day, 0, 0, 0, 0);
              _viewingTime = _viewingTime.AddMinutes((int)fPercentage);
            }

            if (message.SenderControlId == (int)Controls.VERT_SCROLLBAR)
            {
              _needUpdate = true;
              float fPercentage = (float)message.Param1;
              fPercentage /= 100.0f;
              if (_singleChannelView)
              {
                fPercentage *= (float)_totalProgramCount;
                int iChan = (int)fPercentage;
                ChannelOffset = 0;
                _cursorX = 0;
                while (iChan >= _channelCount)
                {
                  iChan -= _channelCount;
                  ChannelOffset += _channelCount;
                }
                _cursorX = iChan;
              }
              else
              {
                fPercentage *= (float)_channelList.Count;
                int iChan = (int)fPercentage;
                ChannelOffset = 0;
                _cursorX = 0;
                while (iChan >= _channelCount)
                {
                  iChan -= _channelCount;
                  ChannelOffset += _channelCount;
                }
                _cursorX = iChan;
              }
            }
            break;

          case GUIMessage.MessageType.GUI_MSG_SKIN_CHANGED:
            {
              base.OnMessage(message);
              _guideColorsLoaded = false;
              return true;
            }

          case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
            {
              base.OnMessage(message);
              SaveSettings();
              if (_recordingList != null && TVHome.Connected)
              {
                _recordingList.Clear();
              }

              _controls = new Dictionary<int, GUIButton3PartControl>();
              _channelList = null;
              _recordingList = null;
              _currentProgram = null;

              return true;
            }

          case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
            {

              if (!TVHome.Connected)
              {
                RemoteControl.Clear();
                GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
                return false;
              }

              TVHome.WaitForGentleConnection();

              if (TVHome.Navigator == null)
              {
                TVHome.OnLoaded();
              }
              else if (TVHome.Navigator.Channel == null)
              {
                TVHome.m_navigator.ReLoad();
                TVHome.LoadSettings(false);
              }

              // Create the channel navigator (it will load groups and channels)
              if (TVHome.m_navigator == null)
              {
                TVHome.m_navigator = new ChannelNavigator();
              }

              GUIPropertyManager.SetProperty("#itemcount", string.Empty);
              GUIPropertyManager.SetProperty("#selecteditem", string.Empty);
              GUIPropertyManager.SetProperty("#selecteditem2", string.Empty);
              GUIPropertyManager.SetProperty("#selectedthumb", string.Empty);

              if (_shouldRestore)
              {
                DoRestoreSkin();
              }
              else
              {
                LoadSkin();
                AllocResources();
              }

              InitControls();

              base.OnMessage(message);

              UpdateOverlayAllowed();
              GUIGraphicsContext.Overlay = _isOverlayAllowed;

              // set topbar autohide
              switch (_autoHideTopbarType)
              {
                case AutoHideTopBar.No:
                  _autoHideTopbar = false;
                  break;
                case AutoHideTopBar.Yes:
                  _autoHideTopbar = true;
                  break;
                default:
                  _autoHideTopbar = GUIGraphicsContext.DefaultTopBarHide;
                  break;
              }
              GUIGraphicsContext.AutoHideTopBar = _autoHideTopbar;
              GUIGraphicsContext.TopBarHidden = _autoHideTopbar;
              GUIGraphicsContext.DisableTopBar = _disableTopBar;
              UpdateChannelCount();

              // Loading tvguide settings will overwrite the guide cursor position.  If we are coming back from the program info window (where
              // recording selections are made) we would like to reposition the cursor to the program from which we invoked the
              // program info window; the user comes back to where they started.  To do this we need to save and restore the cursor
              // position after loading tvguide settings.
              _backupCursorX = _cursorX;
              _backupCursorY = _cursorY;
              _backupChannelOffset = ChannelOffset;

              LoadSettings();

              if (message.Param1 == (int)Window.WINDOW_TV_PROGRAM_INFO)
              {
                _cursorX = _backupCursorX;
                _cursorY = _backupCursorY;
                ChannelOffset = _backupChannelOffset;
              }

              LoadSchedules(true);
              _currentProgram = null;
              if (message.Param1 != (int)Window.WINDOW_TV_PROGRAM_INFO)
              {
                _viewingTime = DateTime.Now;
                _singleChannelView = false;
                _showChannelLogos = false;
                if (TVHome.Card.IsTimeShifting)
                {
                  _currentChannel = TVHome.Navigator.Channel;
                  PositionGuideCursorToCurrentChannel();
                }
              }
              // Mantis 3579: the above lines can lead to too large channeloffset. 
              // Now we check if the offset is too large, and if it is, we reduce it and increase the cursor position accordingly
              if (!_guideContinuousScroll && (ChannelOffset > _channelList.Count - _channelCount))
              {
                _cursorX += ChannelOffset - (_channelList.Count - _channelCount);
                ChannelOffset = _channelList.Count - _channelCount;
              }
              GUISpinControl cntlDay = GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;
              if (cntlDay != null)
              {
                DateTime dtNow = DateTime.Now;
                cntlDay.Reset();
                cntlDay.SetRange(0, MaxDaysInGuide - 1);
                for (int iDay = 0; iDay < MaxDaysInGuide; iDay++)
                {
                  DateTime dtTemp = dtNow.AddDays(iDay);
                  string day;
                  switch (dtTemp.DayOfWeek)
                  {
                    case DayOfWeek.Monday:
                      day = GUILocalizeStrings.Get(657);
                      break;
                    case DayOfWeek.Tuesday:
                      day = GUILocalizeStrings.Get(658);
                      break;
                    case DayOfWeek.Wednesday:
                      day = GUILocalizeStrings.Get(659);
                      break;
                    case DayOfWeek.Thursday:
                      day = GUILocalizeStrings.Get(660);
                      break;
                    case DayOfWeek.Friday:
                      day = GUILocalizeStrings.Get(661);
                      break;
                    case DayOfWeek.Saturday:
                      day = GUILocalizeStrings.Get(662);
                      break;
                    default:
                      day = GUILocalizeStrings.Get(663);
                      break;
                  }
                  day = String.Format("{0} {1}-{2}", day, dtTemp.Day, dtTemp.Month);
                  cntlDay.AddLabel(day, iDay);
                }
              }
              else
              {
                Log.Debug("TvGuideBase: SpinControl cntlDay is null!");
              }

              GUISpinControl cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
              if (cntlTimeInterval != null)
              {
                cntlTimeInterval.Reset();
                for (int i = 1; i <= 4; i++)
                {
                  cntlTimeInterval.AddLabel(String.Empty, i);
                }
                cntlTimeInterval.Value = (_timePerBlock / 15) - 1;
              }
              else
              {
                Log.Debug("TvGuideBase: SpinControl cntlTimeInterval is null!");
              }

              InitGenreKey();

              if (message.Param1 != (int)Window.WINDOW_TV_PROGRAM_INFO)
              {
                Update(true);
              }
              else
              {
                Update(false);
              }

              SetFocus();

              if (_currentProgram != null)
              {
                m_dtStartTime = _currentProgram.StartTime;
              }
              UpdateCurrentProgram();
              return true;
            }
            //break;

          case GUIMessage.MessageType.GUI_MSG_CLICKED:
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.SPINCONTROL_DAY)
            {
              GUISpinControl cntlDay = GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;
              int iDay = cntlDay.Value;

              _viewingTime = DateTime.Now;
              _viewingTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _viewingTime.Hour,
                                          _viewingTime.Minute, 0, 0);
              _viewingTime = _viewingTime.AddDays(iDay);
              _recalculateProgramOffset = true;
              Update(false);
              SetFocus();
              return true;
            }
            if (iControl == (int)Controls.SPINCONTROL_TIME_INTERVAL)
            {
              GUISpinControl cntlTimeInt = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
              int iInterval = (cntlTimeInt.Value) + 1;
              if (iInterval > 4)
              {
                iInterval = 4;
              }
              _timePerBlock = iInterval * 15;
              Update(false);
              SetFocus();
              return true;
            }
            if (iControl == (int)Controls.CHANNEL_GROUP_BUTTON)
            {
              OnSelectChannelGroup();
              return true;
            }
            if (iControl >= GUIDE_COMPONENTID_START)
            {
              if (OnSelectItem(true))
              {
                // Tuning a channel was attempted.  Save the channel setting to ensure that the guide channel selection is in sync
                // with the desired (the channel may not be playing) channel selection.
                SaveSettings();
                Update(false);
                SetFocus();
              }
            }
            else if (_cursorY == 0)
            {
              OnSwitchMode();
            }
            break;
        }
      }
      catch (Exception ex)
      {
        Log.Debug("TvGuideBase: {0}", ex);
      }
      return base.OnMessage(message);
      ;
    }

    /// <summary>
    /// Shows channel group selection dialog
    /// </summary>
    protected virtual void OnSelectChannelGroup()
    {
      // only if more groups present and not in singleChannelView
      if (TVHome.Navigator.Groups.Count > 1 && !_singleChannelView)
      {
        int prevGroup = TVHome.Navigator.CurrentGroup.IdGroup;

        TVHome.OnSelectGroup();

        if (prevGroup != TVHome.Navigator.CurrentGroup.IdGroup)
        {
          GUIWaitCursor.Show();

          // group has been changed
          GetChannels(true);
          PositionGuideCursorToCurrentChannel();
          Update(false);

          SetFocus();
          GUIWaitCursor.Hide();
        }
      }
    }

    private void PositionGuideCursorToCurrentChannel()
    {
      _cursorX = 0;
      _cursorY = 1; // cursor should be on the program guide item
      ChannelOffset = 0;

      if (_channelList == null || _currentChannel == null)
      {
         //Log.Error("PositionGuideCursorToCurrentChannel _channelList = {0} _currentChannel = {1}", _channelList, _currentChannel);
         return;
      }

      // Attempt to position to the current channel in the new list of channels.  If the channel is not in
      // the group then the first channel in the group is selected.
      bool channelInGroup = false;
      for (int i = 0; i < _channelList.Count; i++)
      {
        Channel chan = ((GuideChannel)_channelList[i]).channel;
        if (chan.IdChannel == _currentChannel.IdChannel)
        {
          _cursorX = i;
          channelInGroup = true;
          break;
        }
      }
      if (channelInGroup)
      {
        while (_cursorX >= _channelCount)
        {
          _cursorX -= _channelCount;
          ChannelOffset += _channelCount;
        }
      }
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();

      OnKeyTimeout();

      //if we did a manual rec. on the tvguide directly, then we have to wait for it to start and the update the GUI.
      if (_recordingExpected != null)
      {
        TimeSpan ts = DateTime.Now - _updateTimerRecExpected;
        if (ts.TotalMilliseconds > 1000)
        {
          _updateTimerRecExpected = DateTime.Now;
          VirtualCard card;
          if (_server.IsRecording(_recordingExpected.IdChannel, out card))
          {
            _recordingExpected = null;
            GetChannels(true);
            LoadSchedules(true);
            _needUpdate = true;
          }
        }
      }

      if (_needUpdate)
      {
        _needUpdate = false;
        Update(false);
        SetFocus();
      }

      GUIImage vertLine = GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
      if (vertLine != null)
      {
        if (_singleChannelView)
        {
          vertLine.IsVisible = false;
        }
        else
        {
          vertLine.IsVisible = true;

          DateTime dateNow = DateTime.Now.Date;
          DateTime datePrev = _viewingTime.Date;
          TimeSpan ts = dateNow - datePrev;
          if (ts.TotalDays == 1)
          {
            _viewingTime = DateTime.Now;
          }


          if (_viewingTime.Date.Equals(DateTime.Now.Date) && _viewingTime < DateTime.Now)
          {
            int iStartX = GetControl((int)Controls.LABEL_TIME1).XPosition;
            int iWidth = GetControl((int)Controls.LABEL_TIME1 + 1).XPosition - iStartX;
            iWidth *= 4;

            int iMin = _viewingTime.Minute;
            int iStartTime = _viewingTime.Hour * 60 + iMin;
            int iCurTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (iCurTime >= iStartTime)
            {
              iCurTime -= iStartTime;
            }
            else
            {
              iCurTime = 24 * 60 + iCurTime - iStartTime;
            }

            int iTimeWidth = (_numberOfBlocks * _timePerBlock);
            float fpos = ((float)iCurTime) / ((float)(iTimeWidth));
            fpos *= (float)iWidth;
            fpos += (float)iStartX;
            int width = vertLine.Width / 2;
            vertLine.IsVisible = true;
            vertLine.SetPosition((int)fpos - width, vertLine.YPosition);
            vertLine.Select(0);
            ts = DateTime.Now - _updateTimer;
            if (ts.TotalMinutes >= 1)
            {
              if ((DateTime.Now - _viewingTime).TotalMinutes >= iTimeWidth / 2)
              {
                _cursorY = 0;
                _viewingTime = DateTime.Now;
              }
              Update(false);
            }
          }
          else
          {
            vertLine.IsVisible = false;
          }
        }
      }
    }

    public override void Render(float timePassed)
    {
      lock (this)
      {
        GUIImage vertLine = GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
        base.Render(timePassed);
        if (vertLine != null)
        {
          vertLine.Render(timePassed);
        }
      }
    }

    #endregion

    #region private members

    private string GetChannelLogo(string strChannel)
    {
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, strChannel);
      if (string.IsNullOrEmpty(strLogo))
      {
        // Check for a default TV channel logo.
        strLogo = Utils.GetCoverArt(Thumbs.TVChannel, "default");
        if (string.IsNullOrEmpty(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
      }
      return strLogo;
    }

    private void SetProperties()
    {
      bool bRecording = false;

      if (_channelList == null)
      {
        return;
      }
      if (_channelList.Count == 0)
      {
        return;
      }
      int channel = _cursorX + ChannelOffset;
      while (channel >= _channelList.Count)
      {
        channel -= _channelList.Count;
      }
      if (channel < 0)
      {
        channel = 0;
      }
      Channel chan = (Channel)_channelList[channel].channel;
      if (chan == null)
      {
        return;
      }
      string strChannel = chan.DisplayName;

      if (!_singleChannelView)
      {
        string strLogo = GetChannelLogo(strChannel);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.thumb", strLogo);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.ChannelName", strChannel);
        if (_showChannelNumber)
        {
          int channelNum = chan.ChannelNumber;
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.ChannelNumber", channelNum + "");
        }
        else
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.ChannelNumber", String.Empty);
        }
      }

      if (_cursorY == 0 || _currentProgram == null)
      {
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Title", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.CompositeTitle", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Time", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Description", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Genre", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.MpGenre", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.SubTitle", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Episode", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.EpisodeDetail", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Date", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.StarRating", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Classification", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Duration", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.DurationMins", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.TimeFromNow", String.Empty);

        _currentStartTime = 0;
        _currentEndTime = 0;
        _currentTitle = String.Empty;
        _currentTime = String.Empty;
        _currentChannel = chan;
        GUIControl.HideControl(GetID, (int)Controls.IMG_REC_PIN);
      }
      else if (_currentProgram != null)
      {
        string strTime = String.Format("{0}-{1}",
                                       _currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       _currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        // Lookup the MediaPortal genre for this program.  If found use it, if not found then use the program genre.
        string mpg = "";
        MpGenre mpGenre = _mpGenres.Find(x => x.MappedProgramGenres.Contains(_currentProgram.Genre));
        if (mpGenre != null && mpGenre.Enabled)
        {
          mpg = mpGenre.Name;
        }
        // Try to apply the "movie" genre.
        else if (IsMPAA(_currentProgram.Classification))
        {
          mpGenre = _mpGenres.Find(x => x.IsMovie == true);
          if (mpGenre != null && mpGenre.Enabled)
          {
            mpg = mpGenre.Name;
          }
        }

        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Title", _currentProgram.Title);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.CompositeTitle", TVUtil.GetDisplayTitle(_currentProgram));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Time", strTime);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Description", _currentProgram.Description);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Genre", _currentProgram.Genre);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.MpGenre", mpg);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Duration", GetDuration(_currentProgram));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.DurationMins", GetDurationAsMinutes(_currentProgram));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.TimeFromNow", GetStartTimeFromNow(_currentProgram));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Episode", _currentProgram.EpisodeNumber);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.SubTitle", _currentProgram.EpisodeName);

        if (_currentProgram.Classification == "")
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Classification", "No Rating");
        }
        else
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Classification", _currentProgram.Classification);
        }

        _currentStartTime = Utils.datetolong(_currentProgram.StartTime);
        _currentEndTime = Utils.datetolong(_currentProgram.EndTime);
        _currentTitle = _currentProgram.Title;
        _currentTime = strTime;
        _currentChannel = chan;

        bool bSeries = _currentProgram.IsRecordingSeries || _currentProgram.IsRecordingSeriesPending || _currentProgram.IsPartialRecordingSeriesPending;
        bool bConflict = _currentProgram.HasConflict;
        bRecording = bSeries || (_currentProgram.IsRecording || _currentProgram.IsRecordingOncePending);

        if (bRecording)
        {
          GUIImage img = (GUIImage)GetControl((int)Controls.IMG_REC_PIN);

          bool bPartialRecording = _currentProgram.IsPartialRecordingSeriesPending;

          if (bConflict)
          {
            if (bSeries)
            {
              img.SetFileName(bPartialRecording ?
                              Thumbs.TvConflictPartialRecordingSeriesIcon : Thumbs.TvConflictRecordingSeriesIcon);
            }
            else
            {
              img.SetFileName(bPartialRecording ?
                              Thumbs.TvConflictPartialRecordingIcon : Thumbs.TvConflictRecordingIcon);
            }
          }
          else if (bSeries)
          {
            if (bPartialRecording)
            {
              img.SetFileName(Thumbs.TvPartialRecordingSeriesIcon);
            }
            else
            {
              img.SetFileName(Thumbs.TvRecordingSeriesIcon);
            }
          }
          else
          {
            if (bPartialRecording)
            {
              img.SetFileName(Thumbs.TvPartialRecordingIcon);
            }
            else
            {
              img.SetFileName(Thumbs.TvRecordingIcon);
            }
          }
          GUIControl.ShowControl(GetID, (int)Controls.IMG_REC_PIN);
        }
        else
        {
          GUIControl.HideControl(GetID, (int)Controls.IMG_REC_PIN);
        }
      }

      _currentRecOrNotify = bRecording;

      if (!_currentRecOrNotify && _currentProgram != null)
      {
        if (_currentProgram.Notify)
        {
          _currentRecOrNotify = true;
        }
      }
    }

    //void SetProperties()

    protected override void RenderSingleChannel(Channel channel)
    {
      string strLogo;
      int chan = ChannelOffset;
      for (int iChannel = 0; iChannel < _channelCount; iChannel++)
      {
        if (chan < _channelList.Count)
        {
          Channel tvChan = _channelList[chan].channel;

          strLogo = GetChannelLogo(tvChan.DisplayName);
          GUIButton3PartControl img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
          if (img != null)
          {
            if (_showChannelLogos)
            {
              img.TexutureIcon = strLogo;
            }
            img.Label1 = tvChan.DisplayName;
            img.Data = tvChan;
            img.IsVisible = true;

            if (_useColorsForButtons)
            {
              img.ColourDiffuse = _guideColorChannelButton;
            }
          }
        }
        chan++;
      }

      GUILabelControl channelLabel = GetControl((int)Controls.SINGLE_CHANNEL_LABEL) as GUILabelControl;
      GUIImage channelImage = GetControl((int)Controls.SINGLE_CHANNEL_IMAGE) as GUIImage;

      strLogo = GetChannelLogo(channel.DisplayName);
      if (channelImage == null)
      {
        if (strLogo.Length > 0)
        {
          channelImage = new GUIImage(GetID, (int)Controls.SINGLE_CHANNEL_IMAGE,
                                      GetControl((int)Controls.LABEL_TIME1).XPosition,
                                      GetControl((int)Controls.LABEL_TIME1).YPosition - 15,
                                      40, 40, strLogo, Color.White);
          channelImage.AllocResources();
          GUIControl temp = (GUIControl)channelImage;
          Add(ref temp);
        }
      }
      else
      {
        channelImage.SetFileName(strLogo);
      }

      if (channelLabel == null)
      {
        channelLabel = new GUILabelControl(GetID, (int)Controls.SINGLE_CHANNEL_LABEL,
                                           channelImage.XPosition + 44,
                                           channelImage.YPosition + 10,
                                           300, 40, "font16", channel.DisplayName, 4294967295, GUIControl.Alignment.Left,
                                           GUIControl.VAlignment.Top,
                                           true, 0, 0, 0xFF000000);
        channelLabel.AllocResources();
        GUIControl temp = channelLabel;
        Add(ref temp);
      }

      setSingleChannelLabelVisibility(true);

      channelLabel.Label = channel.DisplayName;
      if (strLogo.Length > 0)
      {
        channelImage.SetFileName(strLogo);
      }

      if (channelLabel != null)
      {
        channelLabel.Label = channel.DisplayName;
      }
      if (_recalculateProgramOffset)
      {
        _programs = new List<Program>();

        DateTime dtStart = DateTime.Now;
        dtStart = dtStart.AddDays(-1);

        DateTime dtEnd = dtStart.AddDays(30);

        TvBusinessLayer layer = new TvBusinessLayer();
        _programs = layer.GetPrograms(channel, dtStart, dtEnd);

        _totalProgramCount = _programs.Count;
        if (_totalProgramCount == 0)
        {
          _totalProgramCount = _channelCount;
        }

        _recalculateProgramOffset = false;
        bool found = false;
        for (int i = 0; i < _programs.Count; i++)
        {
          Program program = (Program)_programs[i];
          if (program.StartTime <= _viewingTime && program.EndTime >= _viewingTime)
          {
            _programOffset = i;
            found = true;
            break;
          }
        }
        if (!found)
        {
          _programOffset = 0;
        }
      }
      else if (_programOffset < _programs.Count)
      {
        int day = ((Program)_programs[_programOffset]).StartTime.DayOfYear;
        bool changed = false;
        while (day > _viewingTime.DayOfYear)
        {
          _viewingTime = _viewingTime.AddDays(1.0);
          changed = true;
        }
        while (day < _viewingTime.DayOfYear)
        {
          _viewingTime = _viewingTime.AddDays(-1.0);
          changed = true;
        }
        if (changed)
        {
          GUISpinControl cntlDay = GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;

          // Find first day in TVGuide and set spincontrol position
          int iDay = CalcDays();
          for (; iDay < 0; ++iDay)
          {
            _viewingTime = _viewingTime.AddDays(1.0);
          }
          for (; iDay >= MaxDaysInGuide; --iDay)
          {
            _viewingTime = _viewingTime.AddDays(-1.0);
          }
          cntlDay.Value = iDay;
        }
      }
      // ichan = number of rows
      for (int ichan = 0; ichan < _channelCount; ++ichan)
      {
        GUIButton3PartControl imgCh = GetControl(ichan + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
        imgCh.TexutureIcon = "";

        int iStartXPos = GetControl(0 + (int)Controls.LABEL_TIME1).XPosition;
        int height = GetControl((int)Controls.IMG_CHAN1 + 1).YPosition;
        height -= GetControl((int)Controls.IMG_CHAN1).YPosition;
        int width = GetControl((int)Controls.LABEL_TIME1 + 1).XPosition;
        width -= GetControl((int)Controls.LABEL_TIME1).XPosition;

        int iTotalWidth = width * _numberOfBlocks;

        Program program;
        int offset = _programOffset;
        if (offset + ichan < _programs.Count)
        {
          program = (Program)_programs[offset + ichan];
        }
        else
        {
          // bugfix for 0 items
          if (_programs.Count == 0)
          {
            program = new Program(channel.IdChannel, _viewingTime, _viewingTime, "-", string.Empty, string.Empty,
                                  Program.ProgramState.None,
                                  DateTime.MinValue, string.Empty, string.Empty, string.Empty, string.Empty, -1,
                                  string.Empty, -1);
          }
          else
          {
            program = (Program)_programs[_programs.Count - 1];
            if (program.EndTime.DayOfYear == _viewingTime.DayOfYear)
            {
              program = new Program(channel.IdChannel, program.EndTime, program.EndTime, "-", "-", "-",
                                    Program.ProgramState.None,
                                    DateTime.MinValue, string.Empty, string.Empty, string.Empty, string.Empty, -1,
                                    string.Empty, -1);
            }
            else
            {
              program = new Program(channel.IdChannel, _viewingTime, _viewingTime, "-", "-", "-",
                                    Program.ProgramState.None,
                                    DateTime.MinValue, string.Empty, string.Empty, string.Empty, string.Empty, -1,
                                    string.Empty, -1);
            }
          }
        }

        int ypos = GetControl(ichan + (int)Controls.IMG_CHAN1).YPosition;
        int iControlId = GUIDE_COMPONENTID_START + ichan * RowID + 0 * ColID;
        GUIButton3PartControl img = GetControl(iControlId) as GUIButton3PartControl;
        GUIButton3PartControl buttonTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOT_RUNNING) as GUIButton3PartControl;

        if (img == null)
        {
          if (buttonTemplate != null)
          {
            buttonTemplate.IsVisible = false;
            img = new GUIButton3PartControl(GetID, iControlId, iStartXPos, ypos, iTotalWidth, height - 2,
                                            buttonTemplate.TexutureFocusLeftName,
                                            buttonTemplate.TexutureFocusMidName,
                                            buttonTemplate.TexutureFocusRightName,
                                            buttonTemplate.TexutureNoFocusLeftName,
                                            buttonTemplate.TexutureNoFocusMidName,
                                            buttonTemplate.TexutureNoFocusRightName,
                                            String.Empty);

            img.TileFillTFL = buttonTemplate.TileFillTFL;
            img.TileFillTNFL = buttonTemplate.TileFillTNFL;
            img.TileFillTFM = buttonTemplate.TileFillTFM;
            img.TileFillTNFM = buttonTemplate.TileFillTNFM;
            img.TileFillTFR = buttonTemplate.TileFillTFR;
            img.TileFillTNFR = buttonTemplate.TileFillTNFR;

            img.OverlayFileNameTFL = buttonTemplate.OverlayFileNameTFL;
            img.OverlayFileNameTNFL = buttonTemplate.OverlayFileNameTNFL;
            img.OverlayFileNameTFM = buttonTemplate.OverlayFileNameTFM;
            img.OverlayFileNameTNFM = buttonTemplate.OverlayFileNameTNFM;
            img.OverlayFileNameTFR = buttonTemplate.OverlayFileNameTFR;
            img.OverlayFileNameTNFR = buttonTemplate.OverlayFileNameTNFR;
          }
          else
          {
            img = new GUIButton3PartControl(GetID, iControlId, iStartXPos, ypos, iTotalWidth, height - 2,
                                            "tvguide_button_selected_left.png",
                                            "tvguide_button_selected_middle.png",
                                            "tvguide_button_selected_right.png",
                                            "tvguide_button_light_left.png",
                                            "tvguide_button_light_middle.png",
                                            "tvguide_button_light_right.png",
                                            String.Empty);
          }
          img.AllocResources();
          GUIControl cntl = (GUIControl)img;
          Add(ref cntl);
        }
        else
        {
          if (buttonTemplate != null)
          {
            buttonTemplate.IsVisible = false;
            img.TexutureFocusLeftName = buttonTemplate.TexutureFocusLeftName;
            img.TexutureFocusMidName = buttonTemplate.TexutureFocusMidName;
            img.TexutureFocusRightName = buttonTemplate.TexutureFocusRightName;
            img.TexutureNoFocusLeftName = buttonTemplate.TexutureNoFocusLeftName;
            img.TexutureNoFocusMidName = buttonTemplate.TexutureNoFocusMidName;
            img.TexutureNoFocusRightName = buttonTemplate.TexutureNoFocusRightName;

            img.TileFillTFL = buttonTemplate.TileFillTFL;
            img.TileFillTNFL = buttonTemplate.TileFillTNFL;
            img.TileFillTFM = buttonTemplate.TileFillTFM;
            img.TileFillTNFM = buttonTemplate.TileFillTNFM;
            img.TileFillTFR = buttonTemplate.TileFillTFR;
            img.TileFillTNFR = buttonTemplate.TileFillTNFR;

            img.OverlayFileNameTFL = buttonTemplate.OverlayFileNameTFL;
            img.OverlayFileNameTNFL = buttonTemplate.OverlayFileNameTNFL;
            img.OverlayFileNameTFM = buttonTemplate.OverlayFileNameTFM;
            img.OverlayFileNameTNFM = buttonTemplate.OverlayFileNameTNFM;
            img.OverlayFileNameTFR = buttonTemplate.OverlayFileNameTFR;
            img.OverlayFileNameTNFR = buttonTemplate.OverlayFileNameTNFR;
          }
          else
          {
            img.TexutureFocusLeftName = "tvguide_button_selected_left.png";
            img.TexutureFocusMidName = "tvguide_button_selected_middle.png";
            img.TexutureFocusRightName = "tvguide_button_selected_right.png";
            img.TexutureNoFocusLeftName = "tvguide_button_light_left.png";
            img.TexutureNoFocusMidName = "tvguide_button_light_middle.png";
            img.TexutureNoFocusRightName = "tvguide_button_light_right.png";
          }
          img.Focus = false;
          img.SetPosition(iStartXPos, ypos);
          img.Width = iTotalWidth;
          img.IsVisible = true;
          img.DoUpdate();
        }
        img.RenderLeft = false;
        img.RenderRight = false;
        img.StretchIfNotRendered = true;

        bool bSeries = (program.IsRecordingSeries || program.IsRecordingSeriesPending || program.IsPartialRecordingSeriesPending);
        bool bConflict = program.HasConflict;
        bool bRecording = bSeries || (program.IsRecording || program.IsRecordingOncePending);

        img.Data = program;
        height = height - 10;
        height /= 2;
        int iWidth = iTotalWidth;
        if (iWidth > 10)
        {
          iWidth -= 10;
        }
        else
        {
          iWidth = 1;
        }

        DateTime dt = DateTime.Now;

        img.TextOffsetX1 = 5;
        img.TextOffsetY1 = 5;
        img.FontName1 = "font13";
        img.TextColor1 = 0xffffffff;

        img.Label1 = TVUtil.GetDisplayTitle(program);

        string strTimeSingle = String.Format("{0}",
                                             program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        if (program.StartTime.DayOfYear != _viewingTime.DayOfYear)
        {
          img.Label1 = String.Format("{0} {1}", Utils.GetShortDayString(program.StartTime),
                                     TVUtil.GetDisplayTitle(program));
        }

        GUILabelControl labelTemplate;
        if (program.IsRunningAt(dt))
        {
          labelTemplate = _titleDarkTemplate;
        }
        else
        {
          labelTemplate = _titleTemplate;
        }

        if (labelTemplate != null)
        {
          img.FontName1 = labelTemplate.FontName;
          img.TextColor1 = labelTemplate.TextColor;
          img.TextOffsetX1 = labelTemplate.XPosition;
          img.TextOffsetY1 = labelTemplate.YPosition;
          img.SetShadow1(labelTemplate.ShadowAngle, labelTemplate.ShadowDistance, labelTemplate.ShadowColor);
        }
        img.TextOffsetX2 = 5;
        img.TextOffsetY2 = img.Height / 2;
        img.FontName2 = "font13";
        img.TextColor2 = 0xffffffff;
        img.Label2 = "";
        if (program.IsRunningAt(dt))
        {
          img.TextColor2 = 0xff101010;
          labelTemplate = _genreDarkTemplate;
        }
        else
        {
          labelTemplate = _genreTemplate;
        }

        if (labelTemplate != null)
        {
          img.FontName2 = labelTemplate.FontName;
          img.TextColor2 = labelTemplate.TextColor;
          img.Label2 = program.Genre;
          img.TextOffsetX2 = labelTemplate.XPosition;
          img.TextOffsetY2 = labelTemplate.YPosition;
          img.SetShadow2(labelTemplate.ShadowAngle, labelTemplate.ShadowDistance, labelTemplate.ShadowColor);
        }
        imgCh.Label1 = strTimeSingle;
        imgCh.TexutureIcon = "";

        if (program.IsRunningAt(dt))
        {
          GUIButton3PartControl buttonRunningTemplate = _programRunningTemplate;
          if (buttonRunningTemplate != null)
          {
            buttonRunningTemplate.IsVisible = false;

            img.TexutureFocusLeftName = buttonRunningTemplate.TexutureFocusLeftName;
            img.TexutureFocusMidName = buttonRunningTemplate.TexutureFocusMidName;
            img.TexutureFocusRightName = buttonRunningTemplate.TexutureFocusRightName;
            img.TexutureNoFocusLeftName = buttonRunningTemplate.TexutureNoFocusLeftName;
            img.TexutureNoFocusMidName = buttonRunningTemplate.TexutureNoFocusMidName;
            img.TexutureNoFocusRightName = buttonRunningTemplate.TexutureNoFocusRightName;

            img.TileFillTFL = buttonRunningTemplate.TileFillTFL;
            img.TileFillTNFL = buttonRunningTemplate.TileFillTNFL;
            img.TileFillTFM = buttonRunningTemplate.TileFillTFM;
            img.TileFillTNFM = buttonRunningTemplate.TileFillTNFM;
            img.TileFillTFR = buttonRunningTemplate.TileFillTFR;
            img.TileFillTNFR = buttonRunningTemplate.TileFillTNFR;

            img.OverlayFileNameTFL = buttonTemplate.OverlayFileNameTFL;
            img.OverlayFileNameTNFL = buttonTemplate.OverlayFileNameTNFL;
            img.OverlayFileNameTFM = buttonTemplate.OverlayFileNameTFM;
            img.OverlayFileNameTNFM = buttonTemplate.OverlayFileNameTNFM;
            img.OverlayFileNameTFR = buttonTemplate.OverlayFileNameTFR;
            img.OverlayFileNameTNFR = buttonTemplate.OverlayFileNameTNFR;
          }
          else
          {
            img.TexutureFocusLeftName = "tvguide_button_selected_left.png";
            img.TexutureFocusMidName = "tvguide_button_selected_middle.png";
            img.TexutureFocusRightName = "tvguide_button_selected_right.png";
            img.TexutureNoFocusLeftName = "tvguide_button_left.png";
            img.TexutureNoFocusMidName = "tvguide_button_middle.png";
            img.TexutureNoFocusRightName = "tvguide_button_right.png";
          }
        }

        img.SetPosition(img.XPosition, img.YPosition);

        img.TexutureIcon = String.Empty;
        if (program.Notify)
        {
          GUIButton3PartControl buttonNotifyTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOTIFY) as GUIButton3PartControl;
          if (buttonNotifyTemplate != null)
          {
            buttonNotifyTemplate.IsVisible = false;

            img.TexutureFocusLeftName = buttonNotifyTemplate.TexutureFocusLeftName;
            img.TexutureFocusMidName = buttonNotifyTemplate.TexutureFocusMidName;
            img.TexutureFocusRightName = buttonNotifyTemplate.TexutureFocusRightName;
            img.TexutureNoFocusLeftName = buttonNotifyTemplate.TexutureNoFocusLeftName;
            img.TexutureNoFocusMidName = buttonNotifyTemplate.TexutureNoFocusMidName;
            img.TexutureNoFocusRightName = buttonNotifyTemplate.TexutureNoFocusRightName;

            img.TileFillTFL = buttonNotifyTemplate.TileFillTFL;
            img.TileFillTNFL = buttonNotifyTemplate.TileFillTNFL;
            img.TileFillTFM = buttonNotifyTemplate.TileFillTFM;
            img.TileFillTNFM = buttonNotifyTemplate.TileFillTNFM;
            img.TileFillTFR = buttonNotifyTemplate.TileFillTFR;
            img.TileFillTNFR = buttonNotifyTemplate.TileFillTNFR;

            img.OverlayFileNameTFL = buttonTemplate.OverlayFileNameTFL;
            img.OverlayFileNameTNFL = buttonTemplate.OverlayFileNameTNFL;
            img.OverlayFileNameTFM = buttonTemplate.OverlayFileNameTFM;
            img.OverlayFileNameTNFM = buttonTemplate.OverlayFileNameTNFM;
            img.OverlayFileNameTFR = buttonTemplate.OverlayFileNameTFR;
            img.OverlayFileNameTNFR = buttonTemplate.OverlayFileNameTNFR;

            // Use of the button template control implies use of the icon.  Use a blank image if the icon is not desired.
            img.TexutureIcon = Thumbs.TvNotifyIcon;
            img.IconOffsetX = buttonNotifyTemplate.IconOffsetX;
            img.IconOffsetY = buttonNotifyTemplate.IconOffsetY;
            img.IconAlign = buttonNotifyTemplate.IconAlign;
            img.IconVAlign = buttonNotifyTemplate.IconVAlign;
            img.IconInlineLabel1 = buttonNotifyTemplate.IconInlineLabel1;
          }
          else
          {
            if (_useNewNotifyButtonColor)
            {
              img.TexutureFocusLeftName = "tvguide_notifyButton_Focus_left.png";
              img.TexutureFocusMidName = "tvguide_notifyButton_Focus_middle.png";
              img.TexutureFocusRightName = "tvguide_notifyButton_Focus_right.png";
              img.TexutureNoFocusLeftName = "tvguide_notifyButton_noFocus_left.png";
              img.TexutureNoFocusMidName = "tvguide_notifyButton_noFocus_middle.png";
              img.TexutureNoFocusRightName = "tvguide_notifyButton_noFocus_right.png";
            }
            else
            {
              img.TexutureIcon = Thumbs.TvNotifyIcon;
            }
          }
        }
        if (bRecording)
        {
          bool bPartialRecording = program.IsPartialRecordingSeriesPending;
          GUIButton3PartControl buttonRecordTemplate = GetControl((int)Controls.BUTTON_PROGRAM_RECORD) as GUIButton3PartControl;

          // Select the partial recording template if needed.
          if (bPartialRecording)
          {
            buttonRecordTemplate = GetControl((int)Controls.BUTTON_PROGRAM_PARTIAL_RECORD) as GUIButton3PartControl;
          }

          if (buttonRecordTemplate != null)
          {
            buttonRecordTemplate.IsVisible = false;

            img.TexutureFocusLeftName = buttonRecordTemplate.TexutureFocusLeftName;
            img.TexutureFocusMidName = buttonRecordTemplate.TexutureFocusMidName;
            img.TexutureFocusRightName = buttonRecordTemplate.TexutureFocusRightName;
            img.TexutureNoFocusLeftName = buttonRecordTemplate.TexutureNoFocusLeftName;
            img.TexutureNoFocusMidName = buttonRecordTemplate.TexutureNoFocusMidName;
            img.TexutureNoFocusRightName = buttonRecordTemplate.TexutureNoFocusRightName;

            img.TileFillTFL = buttonRecordTemplate.TileFillTFL;
            img.TileFillTNFL = buttonRecordTemplate.TileFillTNFL;
            img.TileFillTFM = buttonRecordTemplate.TileFillTFM;
            img.TileFillTNFM = buttonRecordTemplate.TileFillTNFM;
            img.TileFillTFR = buttonRecordTemplate.TileFillTFR;
            img.TileFillTNFR = buttonRecordTemplate.TileFillTNFR;

            img.OverlayFileNameTFL = buttonTemplate.OverlayFileNameTFL;
            img.OverlayFileNameTNFL = buttonTemplate.OverlayFileNameTNFL;
            img.OverlayFileNameTFM = buttonTemplate.OverlayFileNameTFM;
            img.OverlayFileNameTNFM = buttonTemplate.OverlayFileNameTNFM;
            img.OverlayFileNameTFR = buttonTemplate.OverlayFileNameTFR;
            img.OverlayFileNameTNFR = buttonTemplate.OverlayFileNameTNFR;

            // Use of the button template control implies use of the icon.  Use a blank image if the icon is not desired.
            if (bConflict)
            {
              img.TexutureIcon = Thumbs.TvConflictRecordingIcon;
            }
            else if (bSeries)
            {
              img.TexutureIcon = Thumbs.TvRecordingSeriesIcon;
            }
            else
            {
              img.TexutureIcon = Thumbs.TvRecordingIcon;
            }
            img.IconOffsetX = buttonRecordTemplate.IconOffsetX;
            img.IconOffsetY = buttonRecordTemplate.IconOffsetY;
            img.IconAlign = buttonRecordTemplate.IconAlign;
            img.IconVAlign = buttonRecordTemplate.IconVAlign;
            img.IconInlineLabel1 = buttonRecordTemplate.IconInlineLabel1;
          }
          else
          {
            if (bPartialRecording && _useNewPartialRecordingButtonColor)
            {
              img.TexutureFocusLeftName = "tvguide_partRecButton_Focus_left.png";
              img.TexutureFocusMidName = "tvguide_partRecButton_Focus_middle.png";
              img.TexutureFocusRightName = "tvguide_partRecButton_Focus_right.png";
              img.TexutureNoFocusLeftName = "tvguide_partRecButton_noFocus_left.png";
              img.TexutureNoFocusMidName = "tvguide_partRecButton_noFocus_middle.png";
              img.TexutureNoFocusRightName = "tvguide_partRecButton_noFocus_right.png";
            }
            else
            {
              if (_useNewRecordingButtonColor)
              {
                img.TexutureFocusLeftName = "tvguide_recButton_Focus_left.png";
                img.TexutureFocusMidName = "tvguide_recButton_Focus_middle.png";
                img.TexutureFocusRightName = "tvguide_recButton_Focus_right.png";
                img.TexutureNoFocusLeftName = "tvguide_recButton_noFocus_left.png";
                img.TexutureNoFocusMidName = "tvguide_recButton_noFocus_middle.png";
                img.TexutureNoFocusRightName = "tvguide_recButton_noFocus_right.png";
              }
              else
              {
                if (bConflict)
                {
                  img.TexutureIcon = Thumbs.TvConflictRecordingIcon;
                }
                else if (bSeries)
                {
                  img.TexutureIcon = Thumbs.TvRecordingSeriesIcon;
                }
                else
                {
                  img.TexutureIcon = Thumbs.TvRecordingIcon;
                }
              }
            }
          }
        }

        if (_useColorsForButtons)
        {
          if (program.IsRunningAt(DateTime.Now))
          {
            img.ColourDiffuse = GetColorForProgram(program, true);
          }
          else if (program.EndedBefore(DateTime.Now))
          {
            img.ColourDiffuse = _guideColorProgramEnded;
          }
          else
          {
            img.ColourDiffuse = GetColorForProgram(program, false);
          }
        }
      }
    }

    private bool IsRecordingNoEPG(Channel channel)
    {
      VirtualCard vc = null;
      _server.IsRecording(channel.IdChannel, out vc);

      if (vc != null)
      {
        return vc.IsRecording;
      }
      return false;
    }

    protected override void RenderChannel(ref Dictionary<int, List<Program>> mapPrograms, int iChannel, GuideChannel tvGuideChannel,
                               long iStart, long iEnd, bool selectCurrentShow)
    {
      int channelNum = 0;
      Channel channel = tvGuideChannel.channel;

      if (!_byIndex)
      {
        channelNum = channel.ChannelNumber;
      }
      else
      {
        channelNum = _channelList.IndexOf(tvGuideChannel) + 1;
      }

      GUIButton3PartControl img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
      if (img != null)
      {
        if (_showChannelLogos)
        {
          img.TexutureIcon = tvGuideChannel.strLogo;
        }
        if (channelNum > 0 && _showChannelNumber)
        {
          img.Label1 = channelNum + " " + channel.DisplayName;
        }
        else
        {
          img.Label1 = channel.DisplayName;
        }
        img.Data = channel;
        img.IsVisible = true;

        if (_useColorsForButtons)
        {
          img.ColourDiffuse = _guideColorChannelButton;
        }
      }


      List<Program> programs = null;
      if (mapPrograms.ContainsKey(channel.IdChannel))
        programs = mapPrograms[channel.IdChannel];

      bool noEPG = (programs == null || programs.Count == 0);
      if (noEPG)
      {
        DateTime dt = Utils.longtodate(iEnd);
        long iProgEnd = Utils.datetolong(dt);
        Program prog = new Program(channel.IdChannel, Utils.longtodate(iStart), Utils.longtodate(iProgEnd),
                                   GUILocalizeStrings.Get(736), "", "", Program.ProgramState.None, DateTime.MinValue,
                                   string.Empty,
                                   string.Empty, string.Empty, string.Empty, -1, string.Empty, -1);
        if (programs == null)
        {
          programs = new List<Program>();
        }
        programs.Add(prog);
      }

      int iProgram = 0;
      int iPreviousEndXPos = 0;

      int width = GetControl((int)Controls.LABEL_TIME1 + 1).XPosition;
      width -= GetControl((int)Controls.LABEL_TIME1).XPosition;

      int height = 0;
      GUIControl guiControl = GetControl((int)Controls.IMG_CHAN1 + 1);
      if (guiControl != null)
      {
        height = guiControl.YPosition;
        height -= GetControl((int)Controls.IMG_CHAN1).YPosition;
      }
      else
      {
        height = GetControl((int)Controls.IMG_CHAN1).Height;
      }


      foreach (Program program in programs)
      {
        if (Utils.datetolong(program.EndTime) <= iStart)
          continue;

        string strTitle = TVUtil.GetDisplayTitle(program);
        bool bStartsBefore = false;
        bool bEndsAfter = false;

        if (Utils.datetolong(program.StartTime) < iStart)
          bStartsBefore = true;

        if (Utils.datetolong(program.EndTime) > iEnd)
          bEndsAfter = true;

        DateTime dtBlokStart = new DateTime();
        dtBlokStart = _viewingTime;
        dtBlokStart = dtBlokStart.AddMilliseconds(-dtBlokStart.Millisecond);
        dtBlokStart = dtBlokStart.AddSeconds(-dtBlokStart.Second);

        bool bSeries = false;
        bool bRecording = false;
        bool bConflict = false;
        bool bPartialRecording = false;

        bConflict = program.HasConflict;
        bSeries = (program.IsRecordingSeries || program.IsRecordingSeriesPending);
        bRecording = bSeries || (program.IsRecording || program.IsRecordingOncePending);
        bPartialRecording = program.IsPartialRecordingSeriesPending;
        bool bManual = program.IsRecordingManual;

        if (noEPG && !bRecording)
        {
          bRecording = IsRecordingNoEPG(channel);
        }

        bool bProgramIsHD = program.Description.Contains(_hdtvProgramText);

        int iStartXPos = 0;
        int iEndXPos = 0;
        for (int iBlok = 0; iBlok < _numberOfBlocks; iBlok++)
        {
          float fWidthEnd = (float)width;
          DateTime dtBlokEnd = dtBlokStart.AddMinutes(_timePerBlock - 1);
          if (program.RunningAt(dtBlokStart, dtBlokEnd))
          {
            //dtBlokEnd = dtBlokStart.AddSeconds(_timePerBlock * 60);
            if (program.EndTime <= dtBlokEnd)
            {
              TimeSpan dtSpan = dtBlokEnd - program.EndTime;
              int iEndMin = _timePerBlock - (dtSpan.Minutes);

              fWidthEnd = (((float)iEndMin) / ((float)_timePerBlock)) * ((float)(width));
              if (bEndsAfter)
              {
                fWidthEnd = (float)width;
              }
            }

            if (iStartXPos == 0)
            {
              TimeSpan ts = program.StartTime - dtBlokStart;
              int iStartMin = ts.Hours * 60;
              iStartMin += ts.Minutes;
              if (ts.Seconds == 59)
              {
                iStartMin += 1;
              }
              float fWidth = (((float)iStartMin) / ((float)_timePerBlock)) * ((float)(width));

              if (bStartsBefore)
              {
                fWidth = 0;
              }

              iStartXPos = GetControl(iBlok + (int)Controls.LABEL_TIME1).XPosition;
              iStartXPos += (int)fWidth;
              iEndXPos = GetControl(iBlok + (int)Controls.LABEL_TIME1).XPosition + (int)fWidthEnd;
            }
            else
            {
              iEndXPos = GetControl(iBlok + (int)Controls.LABEL_TIME1).XPosition + (int)fWidthEnd;
            }
          }
          dtBlokStart = dtBlokStart.AddMinutes(_timePerBlock);
        }

        if (iStartXPos >= 0)
        {
          if (iPreviousEndXPos > iStartXPos)
          {
            iStartXPos = iPreviousEndXPos;
          }
          if (iEndXPos <= iStartXPos + 5)
          {
            iEndXPos = iStartXPos + 6; // at least 1 pixel width
          }

          int ypos = GetControl(iChannel + (int)Controls.IMG_CHAN1).YPosition;
          int iControlId = GUIDE_COMPONENTID_START + iChannel * RowID + iProgram * ColID;
          int iWidth = iEndXPos - iStartXPos;
          if (iWidth > 3)
          {
            iWidth -= 3;
          }
          else
          {
            iWidth = 1;
          }

          string TexutureFocusLeftName = "tvguide_button_selected_left.png";
          string TexutureFocusMidName = "tvguide_button_selected_middle.png";
          string TexutureFocusRightName = "tvguide_button_selected_right.png";
          string TexutureNoFocusLeftName = "tvguide_button_light_left.png";
          string TexutureNoFocusMidName = "tvguide_button_light_middle.png";
          string TexutureNoFocusRightName = "tvguide_button_light_right.png";

          bool TileFillTFL = false;
          bool TileFillTNFL = false;
          bool TileFillTFM = false;
          bool TileFillTNFM = false;
          bool TileFillTFR = false;
          bool TileFillTNFR = false;

          string OverlayFileNameTFL = "";
          string OverlayFileNameTNFL = "";
          string OverlayFileNameTFM = "";
          string OverlayFileNameTNFM = "";
          string OverlayFileNameTFR = "";
          string OverlayFileNameTNFR = "";

          if (_programNotRunningTemplate != null)
          {
            _programNotRunningTemplate.IsVisible = false;

            TexutureFocusLeftName = _programNotRunningTemplate.TexutureFocusLeftName;
            TexutureFocusMidName = _programNotRunningTemplate.TexutureFocusMidName;
            TexutureFocusRightName = _programNotRunningTemplate.TexutureFocusRightName;
            TexutureNoFocusLeftName = _programNotRunningTemplate.TexutureNoFocusLeftName;
            TexutureNoFocusMidName = _programNotRunningTemplate.TexutureNoFocusMidName;
            TexutureNoFocusRightName = _programNotRunningTemplate.TexutureNoFocusRightName;

            TileFillTFL = _programNotRunningTemplate.TileFillTFL;
            TileFillTNFL = _programNotRunningTemplate.TileFillTNFL;
            TileFillTFM = _programNotRunningTemplate.TileFillTFM;
            TileFillTNFM = _programNotRunningTemplate.TileFillTNFM;
            TileFillTFR = _programNotRunningTemplate.TileFillTFR;
            TileFillTNFR = _programNotRunningTemplate.TileFillTNFR;

            OverlayFileNameTFL = _programNotRunningTemplate.OverlayFileNameTFL;
            OverlayFileNameTNFL = _programNotRunningTemplate.OverlayFileNameTNFL;
            OverlayFileNameTFM = _programNotRunningTemplate.OverlayFileNameTFM;
            OverlayFileNameTNFM = _programNotRunningTemplate.OverlayFileNameTNFM;
            OverlayFileNameTFR = _programNotRunningTemplate.OverlayFileNameTFR;
            OverlayFileNameTNFR = _programNotRunningTemplate.OverlayFileNameTNFR;
          }

          bool isNew = false;

          if (!_controls.TryGetValue((int)iControlId, out img))
          {
            img = new GUIButton3PartControl(GetID, iControlId, iStartXPos, ypos, iWidth, height - 2,
                                            TexutureFocusLeftName,
                                            TexutureFocusMidName,
                                            TexutureFocusRightName,
                                            TexutureNoFocusLeftName,
                                            TexutureNoFocusMidName,
                                            TexutureNoFocusRightName,
                                            String.Empty);

            isNew = true;
          }
          else
          {
            img.Focus = false;
            img.SetPosition(iStartXPos, ypos);
            img.Width = iWidth;
            img.IsVisible = true;
            img.DoUpdate();
          }

          img.RenderLeft = false;
          img.RenderRight = false;
          img.StretchIfNotRendered = true;

          img.TexutureIcon = String.Empty;
          if (program.Notify)
          {
            if (_programNotifyTemplate != null)
            {
              _programNotifyTemplate.IsVisible = false;

              TexutureFocusLeftName = _programNotifyTemplate.TexutureFocusLeftName;
              TexutureFocusMidName = _programNotifyTemplate.TexutureFocusMidName;
              TexutureFocusRightName = _programNotifyTemplate.TexutureFocusRightName;
              TexutureNoFocusLeftName = _programNotifyTemplate.TexutureNoFocusLeftName;
              TexutureNoFocusMidName = _programNotifyTemplate.TexutureNoFocusMidName;
              TexutureNoFocusRightName = _programNotifyTemplate.TexutureNoFocusRightName;

              TileFillTFL = _programNotifyTemplate.TileFillTFL;
              TileFillTNFL = _programNotifyTemplate.TileFillTNFL;
              TileFillTFM = _programNotifyTemplate.TileFillTFM;
              TileFillTNFM = _programNotifyTemplate.TileFillTNFM;
              TileFillTFR = _programNotifyTemplate.TileFillTFR;
              TileFillTNFR = _programNotifyTemplate.TileFillTNFR;

              OverlayFileNameTFL = _programNotifyTemplate.OverlayFileNameTFL;
              OverlayFileNameTNFL = _programNotifyTemplate.OverlayFileNameTNFL;
              OverlayFileNameTFM = _programNotifyTemplate.OverlayFileNameTFM;
              OverlayFileNameTNFM = _programNotifyTemplate.OverlayFileNameTNFM;
              OverlayFileNameTFR = _programNotifyTemplate.OverlayFileNameTFR;
              OverlayFileNameTNFR = _programNotifyTemplate.OverlayFileNameTNFR;

              // Use of the button template control implies use of the icon.  Use a blank image if the icon is not desired.
              img.TexutureIcon = Thumbs.TvNotifyIcon;
              img.IconOffsetX = _programNotifyTemplate.IconOffsetX;
              img.IconOffsetY = _programNotifyTemplate.IconOffsetY;
              img.IconAlign = _programNotifyTemplate.IconAlign;
              img.IconVAlign = _programNotifyTemplate.IconVAlign;
              img.IconInlineLabel1 = _programNotifyTemplate.IconInlineLabel1;
            }
            else
            {
              if (_useNewNotifyButtonColor)
              {
                TexutureFocusLeftName = "tvguide_notifyButton_Focus_left.png";
                TexutureFocusMidName = "tvguide_notifyButton_Focus_middle.png";
                TexutureFocusRightName = "tvguide_notifyButton_Focus_right.png";
                TexutureNoFocusLeftName = "tvguide_notifyButton_noFocus_left.png";
                TexutureNoFocusMidName = "tvguide_notifyButton_noFocus_middle.png";
                TexutureNoFocusRightName = "tvguide_notifyButton_noFocus_right.png";
              }
              else
              {
                img.TexutureIcon = Thumbs.TvNotifyIcon;
              }
            }
          }
          if (bRecording)
          {
            GUIButton3PartControl buttonRecordTemplate = _programRecordTemplate;

            // Select the partial recording template if needed.
            if (bPartialRecording)
            {
              buttonRecordTemplate = _programPartialRecordTemplate;
            }

            if (buttonRecordTemplate != null)
            {
              buttonRecordTemplate.IsVisible = false;

              TexutureFocusLeftName = buttonRecordTemplate.TexutureFocusLeftName;
              TexutureFocusMidName = buttonRecordTemplate.TexutureFocusMidName;
              TexutureFocusRightName = buttonRecordTemplate.TexutureFocusRightName;
              TexutureNoFocusLeftName = buttonRecordTemplate.TexutureNoFocusLeftName;
              TexutureNoFocusMidName = buttonRecordTemplate.TexutureNoFocusMidName;
              TexutureNoFocusRightName = buttonRecordTemplate.TexutureNoFocusRightName;

              TileFillTFL = buttonRecordTemplate.TileFillTFL;
              TileFillTNFL = buttonRecordTemplate.TileFillTNFL;
              TileFillTFM = buttonRecordTemplate.TileFillTFM;
              TileFillTNFM = buttonRecordTemplate.TileFillTNFM;
              TileFillTFR = buttonRecordTemplate.TileFillTFR;
              TileFillTNFR = buttonRecordTemplate.TileFillTNFR;

              OverlayFileNameTFL = buttonRecordTemplate.OverlayFileNameTFL;
              OverlayFileNameTNFL = buttonRecordTemplate.OverlayFileNameTNFL;
              OverlayFileNameTFM = buttonRecordTemplate.OverlayFileNameTFM;
              OverlayFileNameTNFM = buttonRecordTemplate.OverlayFileNameTNFM;
              OverlayFileNameTFR = buttonRecordTemplate.OverlayFileNameTFR;
              OverlayFileNameTNFR = buttonRecordTemplate.OverlayFileNameTNFR;

              // Use of the button template control implies use of the icon.  Use a blank image if the icon is not desired.
              if (bConflict)
              {
                img.TexutureIcon = Thumbs.TvConflictRecordingIcon;
              }
              else if (bSeries)
              {
                img.TexutureIcon = Thumbs.TvRecordingSeriesIcon;
              }
              else
              {
                img.TexutureIcon = Thumbs.TvRecordingIcon;
              }
              img.IconOffsetX = buttonRecordTemplate.IconOffsetX;
              img.IconOffsetY = buttonRecordTemplate.IconOffsetY;
              img.IconAlign = buttonRecordTemplate.IconAlign;
              img.IconVAlign = buttonRecordTemplate.IconVAlign;
              img.IconInlineLabel1 = buttonRecordTemplate.IconInlineLabel1;
            }
            else
            {
              if (bPartialRecording && _useNewPartialRecordingButtonColor)
              {
                TexutureFocusLeftName = "tvguide_partRecButton_Focus_left.png";
                TexutureFocusMidName = "tvguide_partRecButton_Focus_middle.png";
                TexutureFocusRightName = "tvguide_partRecButton_Focus_right.png";
                TexutureNoFocusLeftName = "tvguide_partRecButton_noFocus_left.png";
                TexutureNoFocusMidName = "tvguide_partRecButton_noFocus_middle.png";
                TexutureNoFocusRightName = "tvguide_partRecButton_noFocus_right.png";
              }
              else
              {
                if (_useNewRecordingButtonColor)
                {
                  TexutureFocusLeftName = "tvguide_recButton_Focus_left.png";
                  TexutureFocusMidName = "tvguide_recButton_Focus_middle.png";
                  TexutureFocusRightName = "tvguide_recButton_Focus_right.png";
                  TexutureNoFocusLeftName = "tvguide_recButton_noFocus_left.png";
                  TexutureNoFocusMidName = "tvguide_recButton_noFocus_middle.png";
                  TexutureNoFocusRightName = "tvguide_recButton_noFocus_right.png";
                }
                else
                {
                  if (bConflict)
                  {
                    img.TexutureIcon = Thumbs.TvConflictRecordingIcon;
                  }
                  else if (bSeries)
                  {
                    img.TexutureIcon = Thumbs.TvRecordingSeriesIcon;
                  }
                  else
                  {
                    img.TexutureIcon = Thumbs.TvRecordingIcon;
                  }
                }
              }
            }
          }

          DateTime dt = DateTime.Now;

          img.TexutureIcon2 = String.Empty;
          if (bProgramIsHD)
          {
            if (program.IsRunningAt(dt) && _programRunningTemplate != null)
            {
              img.TexutureIcon2 = _programRunningTemplate.TexutureIcon2;
              img.Icon2Align = _programRunningTemplate.Icon2Align;
              img.Icon2VAlign = _programRunningTemplate.Icon2VAlign;
              img.Icon2OffsetX = _programRunningTemplate.Icon2OffsetX;
              img.Icon2OffsetY = _programRunningTemplate.Icon2OffsetY;
              img.Icon2InlineLabel1 = _programRunningTemplate.Icon2InlineLabel1;
            }
            else if (!program.IsRunningAt(dt) && _programNotRunningTemplate != null)
            {
              img.TexutureIcon2 = _programNotRunningTemplate.TexutureIcon2;
              img.Icon2Align = _programNotRunningTemplate.Icon2Align;
              img.Icon2VAlign = _programNotRunningTemplate.Icon2VAlign;
              img.Icon2OffsetX = _programNotRunningTemplate.Icon2OffsetX;
              img.Icon2OffsetY = _programNotRunningTemplate.Icon2OffsetY;
              img.Icon2InlineLabel1 = _programNotRunningTemplate.Icon2InlineLabel1;
            }
            else
            {
              if (_useHdProgramIcon)
              {
                img.TexutureIcon2 = "tvguide_hd_program.png";
                img.Icon2Align = GUIControl.Alignment.ALIGN_LEFT;
                img.Icon2VAlign = GUIControl.VAlignment.ALIGN_MIDDLE;
                img.Icon2OffsetX = 5;
                img.Icon2OffsetY = 0;
                img.Icon2InlineLabel1 = true;
              }
            }
          }
          img.Data = program.Clone();
          iWidth = iEndXPos - iStartXPos;
          if (iWidth > 10)
          {
            iWidth -= 10;
          }
          else
          {
            iWidth = 1;
          }

          img.TextOffsetX1 = 5;
          img.TextOffsetY1 = 5;
          img.FontName1 = "font13";
          img.TextColor1 = 0xffffffff;
          img.Label1 = strTitle;
          GUILabelControl labelTemplate;
          if (program.IsRunningAt(dt))
          {
            labelTemplate = _titleDarkTemplate;
          }
          else
          {
            labelTemplate = _titleTemplate;
          }

          if (labelTemplate != null)
          {
            img.FontName1 = labelTemplate.FontName;
            img.TextColor1 = labelTemplate.TextColor;
            img.TextColor2 = labelTemplate.TextColor;
            img.TextOffsetX1 = labelTemplate.XPosition;
            img.TextOffsetY1 = labelTemplate.YPosition;
            img.SetShadow1(labelTemplate.ShadowAngle, labelTemplate.ShadowDistance, labelTemplate.ShadowColor);

            // This is a legacy behavior check.  Adding labelTemplate.XPosition and labelTemplate.YPosition requires
            // skinners to add these values to the skin xml unless this check exists.  Perform a sanity check on the
            // x,y position to ensure it falls into the bounds of the button.  If it does not then fall back to use the
            // legacy values.  This check is necessary because the x,y position (without skin file changes) will be taken
            // from either the references.xml control template or the controls coded defaults.
            if (img.TextOffsetY1 > img.Height)
            {
              // Set legacy values.
              img.TextOffsetX1 = 5;
              img.TextOffsetY1 = 5;
            }
          }
          img.TextOffsetX2 = 5;
          img.TextOffsetY2 = img.Height / 2;
          img.FontName2 = "font13";
          img.TextColor2 = 0xffffffff;

          if (program.IsRunningAt(dt))
          {
            labelTemplate = _genreDarkTemplate;
          }
          else
          {
            labelTemplate = _genreTemplate;
          }

          if (labelTemplate != null)
          {
            img.FontName2 = labelTemplate.FontName;
            img.TextColor2 = labelTemplate.TextColor;
            img.Label2 = program.Genre;
            img.TextOffsetX2 = labelTemplate.XPosition;
            img.TextOffsetY2 = labelTemplate.YPosition;
            img.SetShadow2(labelTemplate.ShadowAngle, labelTemplate.ShadowDistance, labelTemplate.ShadowColor);

            // This is a legacy behavior check.  Adding labelTemplate.XPosition and labelTemplate.YPosition requires
            // skinners to add these values to the skin xml unless this check exists.  Perform a sanity check on the
            // x,y position to ensure it falls into the bounds of the button.  If it does not then fall back to use the
            // legacy values.  This check is necessary because the x,y position (without skin file changes) will be taken
            // from either the references.xml control template or the controls coded defaults.
            if (img.TextOffsetY2 > img.Height)
            {
              // Set legacy values.
              img.TextOffsetX2 = 5;
              img.TextOffsetY2 = 5;
            }
          }

          if (program.IsRunningAt(dt))
          {
            GUIButton3PartControl buttonRunningTemplate = _programRunningTemplate;
            if (!bRecording && !bPartialRecording && buttonRunningTemplate != null)
            {
              buttonRunningTemplate.IsVisible = false;

              TexutureFocusLeftName = buttonRunningTemplate.TexutureFocusLeftName;
              TexutureFocusMidName = buttonRunningTemplate.TexutureFocusMidName;
              TexutureFocusRightName = buttonRunningTemplate.TexutureFocusRightName;
              TexutureNoFocusLeftName = buttonRunningTemplate.TexutureNoFocusLeftName;
              TexutureNoFocusMidName = buttonRunningTemplate.TexutureNoFocusMidName;
              TexutureNoFocusRightName = buttonRunningTemplate.TexutureNoFocusRightName;

              TileFillTFL = buttonRunningTemplate.TileFillTFL;
              TileFillTNFL = buttonRunningTemplate.TileFillTNFL;
              TileFillTFM = buttonRunningTemplate.TileFillTFM;
              TileFillTNFM = buttonRunningTemplate.TileFillTNFM;
              TileFillTFR = buttonRunningTemplate.TileFillTFR;
              TileFillTNFR = buttonRunningTemplate.TileFillTNFR;

              OverlayFileNameTFL = buttonRunningTemplate.OverlayFileNameTFL;
              OverlayFileNameTNFL = buttonRunningTemplate.OverlayFileNameTNFL;
              OverlayFileNameTFM = buttonRunningTemplate.OverlayFileNameTFM;
              OverlayFileNameTNFM = buttonRunningTemplate.OverlayFileNameTNFM;
              OverlayFileNameTFR = buttonRunningTemplate.OverlayFileNameTFR;
              OverlayFileNameTNFR = buttonRunningTemplate.OverlayFileNameTNFR;
            }
            else if (bRecording && _useNewRecordingButtonColor)
            {
              TexutureFocusLeftName = "tvguide_recButton_Focus_left.png";
              TexutureFocusMidName = "tvguide_recButton_Focus_middle.png";
              TexutureFocusRightName = "tvguide_recButton_Focus_right.png";
              TexutureNoFocusLeftName = "tvguide_recButton_noFocus_left.png";
              TexutureNoFocusMidName = "tvguide_recButton_noFocus_middle.png";
              TexutureNoFocusRightName = "tvguide_recButton_noFocus_right.png";
            }
            else if (!(bRecording && bPartialRecording && _useNewRecordingButtonColor))
            {
              TexutureFocusLeftName = "tvguide_button_selected_left.png";
              TexutureFocusMidName = "tvguide_button_selected_middle.png";
              TexutureFocusRightName = "tvguide_button_selected_right.png";
              TexutureNoFocusLeftName = "tvguide_button_left.png";
              TexutureNoFocusMidName = "tvguide_button_middle.png";
              TexutureNoFocusRightName = "tvguide_button_right.png";
            }
            if (selectCurrentShow && iChannel == _cursorX)
            {
              _cursorY = iProgram + 1;
              _currentProgram = program;
              m_dtStartTime = program.StartTime;
              SetProperties();
            }
          }

          if (bEndsAfter)
          {
            img.RenderRight = true;

            // If no template found then use default texture names.
            // Texture names already set if using template.
            if (_programNotRunningTemplate == null)
            {
              TexutureFocusRightName = "tvguide_arrow_selected_right.png";
              TexutureNoFocusRightName = "tvguide_arrow_light_right.png";
            }

            if (program.IsRunningAt(dt) && _programRunningTemplate == null)
            {
              TexutureNoFocusRightName = "tvguide_arrow_right.png";
            }
          }
          if (bStartsBefore)
          {
            img.RenderLeft = true;

            if (_programNotRunningTemplate == null)
            {
              TexutureFocusLeftName = "tvguide_arrow_selected_left.png";
              TexutureNoFocusLeftName = "tvguide_arrow_light_left.png";
            }

            if (program.IsRunningAt(dt) && _programRunningTemplate == null)
            {
              TexutureNoFocusLeftName = "tvguide_arrow_left.png";
            }
          }

          if (_useColorsForButtons)
          {
            if (program.IsRunningAt(DateTime.Now))
            {
              img.ColourDiffuse = GetColorForProgram(program, true);
            }
            else if (program.EndedBefore(DateTime.Now))
            {
              img.ColourDiffuse = _guideColorProgramEnded;
            }
            else
            {
              img.ColourDiffuse = GetColorForProgram(program, false);
            }
          }

          img.TexutureFocusLeftName = TexutureFocusLeftName;
          img.TexutureFocusMidName = TexutureFocusMidName;
          img.TexutureFocusRightName = TexutureFocusRightName;
          img.TexutureNoFocusLeftName = TexutureNoFocusLeftName;
          img.TexutureNoFocusMidName = TexutureNoFocusMidName;
          img.TexutureNoFocusRightName = TexutureNoFocusRightName;

          img.TileFillTFL = TileFillTFL;
          img.TileFillTNFL = TileFillTNFL;
          img.TileFillTFM = TileFillTFM;
          img.TileFillTNFM = TileFillTNFM;
          img.TileFillTFR = TileFillTFR;
          img.TileFillTNFR = TileFillTNFR;

          img.OverlayFileNameTFL = OverlayFileNameTFL;
          img.OverlayFileNameTNFL = OverlayFileNameTNFL;
          img.OverlayFileNameTFM = OverlayFileNameTFM;
          img.OverlayFileNameTNFM = OverlayFileNameTNFM;
          img.OverlayFileNameTFR = OverlayFileNameTFR;
          img.OverlayFileNameTNFR = OverlayFileNameTNFR;

          if (isNew)
          {
            img.AllocResources();
            GUIControl cntl = (GUIControl)img;
            _controls.Add((int)iControlId, img);
            Add(ref cntl);
          }
          else
            img.DoUpdate();
          iProgram++;
        }
        iPreviousEndXPos = iEndXPos;
      }
    }

    //void RenderChannel(int iChannel,Channel channel, long iStart, long iEnd, bool selectCurrentShow)

    private int ProgramCount(int iChannel)
    {
      int iProgramCount = 0;
      for (int iProgram = 0; iProgram < _numberOfBlocks * 5; ++iProgram)
      {
        int iControlId = GUIDE_COMPONENTID_START + iChannel * RowID + iProgram * ColID;
        GUIControl cntl = GetControl(iControlId);
        if (cntl != null && cntl.IsVisible)
        {
          iProgramCount++;
        }
        else
        {
          return iProgramCount;
        }
      }
      return iProgramCount;
    }

    private void OnDown(bool updateScreen)
    {
      if (updateScreen)
      {
        UnFocus();
      }
      if (_cursorX < 0)
      {
        _cursorY = 0;
        _cursorX = 0;
        if (updateScreen)
        {
          SetFocus();
          GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL).Focus = false;
        }
        return;
      }

      if (_singleChannelView)
      {
        if (_cursorX + 1 < _channelCount)
        {
          _cursorX++;
        }
        else
        {
          if (_cursorX + _programOffset + 1 < _totalProgramCount)
          {
            _programOffset++;
          }
        }
        if (updateScreen)
        {
          Update(false);
          SetFocus();
          UpdateCurrentProgram();
          SetProperties();
        }
        return;
      }

      if (_cursorY == 0)
      {
        MoveDown();

        if (updateScreen)
        {
          Update(false);
          SetFocus();
          SetProperties();
        }
        return;
      }

      // not on tvguide button
      if (_cursorY > 0)
      {
        // if cursor is on a program in guide, try to find the "best time matching" program in new channel
        SetBestMatchingProgram(updateScreen, true);
      }
    }

    private void MoveDown()
    {
      // Move the cursor only if there are more channels in the view.
      if (_cursorX + 1 < Math.Min(_channelList.Count, _channelCount))
      {
        _cursorX++;
        _lastCommandTime = AnimationTimer.TickCount;
      }
      else
      {
        // reached end of screen
        // more channels than rows?
        if (_channelList.Count > _channelCount)
        {
          // Guide may be allowed to loop continuously bottom to top.
          if (_guideContinuousScroll)
          {
            // We're at the bottom of the last page of channels.
            if (ChannelOffset >= _channelList.Count)
            {
              // Position to first channel in guide without moving the cursor (implements continuous loops of channels).
              ChannelOffset = 0;
            }
            else
            {
              // Advance to next channel, wrap around if at end of list.
              ChannelOffset++;
              if (ChannelOffset >= _channelList.Count)
              {
                ChannelOffset = 0;
              }
            }
          }
          else
          {
            // Are we at the bottom of the lst page of channels?
            if (ChannelOffset > 0 && ChannelOffset >= (_channelList.Count - 1) - _cursorX)
            {
              // We're at the bottom of the last page of channels.
              // Reposition the guide to the top only after the key/button has been released and pressed again.
              if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
              {
                ChannelOffset = 0;
                _cursorX = 0;
                _lastCommandTime = AnimationTimer.TickCount;
              }
            }
            else
            {
              // Advance to next channel.
              ChannelOffset++;
              _lastCommandTime = AnimationTimer.TickCount;
            }
          }
        }
        else if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
        {
          // Move the highlight back to the top of the list only after the key/button has been released and pressed again.
          _cursorX = 0;
          _lastCommandTime = AnimationTimer.TickCount;
        }
      }
    }

    private void OnUp(bool updateScreen, bool isPaging)
    {
      if (updateScreen)
      {
        UnFocus();
      }

      if (_singleChannelView)
      {
        if (_cursorX == 0 && _cursorY == 0 && !isPaging)
        {
          // Don't focus the control when it is not visible.
          if (GetControl((int)Controls.SPINCONTROL_DAY).IsVisible)
          {
            _cursorX = -1;
            GetControl((int)Controls.SPINCONTROL_DAY).Focus = true;
          }
          return;
        }

        if (_cursorX > 0)
        {
          _cursorX--;
        }
        else if (_programOffset > 0)
        {
          _programOffset--;
        }

        if (updateScreen)
        {
          Update(false);
          SetFocus();
          UpdateCurrentProgram();
          SetProperties();
        }
        return;
      }
      else
      {
        if (_cursorY == -1)
        {
          _cursorX = -1;
          _cursorY = 0;
          GetControl((int)Controls.CHANNEL_GROUP_BUTTON).Focus = false;
          GetControl((int)Controls.SPINCONTROL_DAY).Focus = true;
          return;
        }

        if (_cursorY == 0 && _cursorX == 0 && !isPaging)
        {
          // Only focus the control if it is visible.
          if (GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL).Visible)
          {
            _cursorX = -1;
            GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL).Focus = true;
            return;
          }
        }
      }

      if (_cursorY == 0)
      {
        if (_cursorX == 0)
        {
          if (ChannelOffset > 0)
          {
            // Somewhere in the middle of the guide; just scroll up.
            ChannelOffset--;
          }
          else if (ChannelOffset == 0)
          {
            // We're at the top of the first page of channels.
            // Reposition the guide to the bottom only after the key/button has been released and pressed again.
            if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
            {
              ChannelOffset = _channelList.Count - _channelCount;
              _cursorX = _channelCount - 1;
            }
          }
        }
        else if (_cursorX > 0)
        {
          _cursorX--;
        }

        if (updateScreen)
        {
          Update(false);
          SetFocus();
          SetProperties();
        }
      }

      // not on tvguide button
      if (_cursorY > 0)
      {
        // if cursor is on a program in guide, try to find the "best time matching" program in new channel
        SetBestMatchingProgram(updateScreen, false);
      }
    }

    private void MoveUp()
    {
      if (_cursorX == 0)
      {
        if (_guideContinuousScroll)
        {
          if (ChannelOffset == 0 && _channelList.Count > _channelCount)
          {
            // We're at the top of the first page of channels.  Position to last channel in guide.
            ChannelOffset = _channelList.Count - 1;
          }
          else if (ChannelOffset > 0)
          {
            // Somewhere in the middle of the guide; just scroll up.
            ChannelOffset--;
          }
        }
        else
        {
          if (ChannelOffset > 0)
          {
            // Somewhere in the middle of the guide; just scroll up.
            ChannelOffset--;
            _lastCommandTime = AnimationTimer.TickCount;
          }
          // Are we at the top of the first page of channels?
          else if (ChannelOffset == 0 && _cursorX == 0)
          {
            // We're at the top of the first page of channels.
            // Reposition the guide to the bottom only after the key/button has been released and pressed again.
            if ((AnimationTimer.TickCount - _lastCommandTime) > _loopDelay)
            {
              ChannelOffset = _channelList.Count - _channelCount;
              _cursorX = _channelCount - 1;
              _lastCommandTime = AnimationTimer.TickCount;
            }
          }
        }
      }
      else
      {
        _cursorX--;
        _lastCommandTime = AnimationTimer.TickCount;
      }
    }

    /// <summary>
    /// Sets the best matching program in new guide row
    /// </summary>
    /// <param name="updateScreen"></param>
    private void SetBestMatchingProgram(bool updateScreen, bool DirectionIsDown)
    {
      // if cursor is on a program in guide, try to find the "best time matching" program in new channel
      int iCurY = _cursorX;
      int iCurOff = ChannelOffset;
      int iX1, iX2;
      int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
      GUIControl control = GetControl(iControlId);
      if (control == null)
      {
        return;
      }
      iX1 = control.XPosition;
      iX2 = control.XPosition + control.Width;

      bool bOK = false;
      int iMaxSearch = _channelList.Count;

      // TODO rewrite the while loop, the code is a little awkward.
      while (!bOK && (iMaxSearch > 0))
      {
        iMaxSearch--;
        if (DirectionIsDown == true)
        {
          MoveDown();
        }
        else // Direction "Up"
        {
          MoveUp();
        }
        if (updateScreen)
        {
          Update(false);
        }

        for (int x = 1; x < ColID; x++)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (x - 1) * ColID;
          control = GetControl(iControlId);
          if (control != null)
          {
            Program prog = (Program)control.Data;

            if (_singleChannelView)
            {
              _cursorY = x;
              bOK = true;
              break;
            }

            bool isvalid = false;
            DateTime time = DateTime.Now;
            if (time < prog.EndTime) // present & future
            {
              if (m_dtStartTime <= prog.StartTime)
              {
                isvalid = true;
              }
              else if (m_dtStartTime >= prog.StartTime && m_dtStartTime < prog.EndTime)
              {
                isvalid = true;
              }
              else if (m_dtStartTime < time)
              {
                isvalid = true;
              }
            }
            // this one will skip past programs
            else if (time > _currentProgram.EndTime) // history
            {
              if (prog.EndTime > m_dtStartTime)
              {
                isvalid = true;
              }
            }

            if (isvalid)
            {
              _cursorY = x;
              bOK = true;
              break;
            }
          }
        }
      }
      if (!bOK)
      {
        _cursorX = iCurY;
        ChannelOffset = iCurOff;
      }
      if (updateScreen)
      {
        Correct();
        if (iCurOff == ChannelOffset)
        {
          UpdateCurrentProgram();
          return;
        }
        SetFocus();
      }
    }

    private void OnLeft()
    {
      if (_cursorX < 0)
      {
        return;
      }
      UnFocus();
      if (_cursorY <= 0)
      {
        // custom focus handling only if button available
        if (MinYIndex == -1)
        {
          _cursorY--; // decrease by 1,
          if (_cursorY == -1) // means tvgroup entered (-1) or moved left (-2)
          {
            SetFocus();
            return;
          }
        }
        _viewingTime = _viewingTime.AddMinutes(-_timePerBlock);
        // Check new day
        int iDay = CalcDays();
        if (iDay < 0)
        {
          _viewingTime = _viewingTime.AddMinutes(+_timePerBlock);
        }
      }
      else
      {
        if (_cursorY == 1)
        {
          _cursorY = 0;

          SetFocus();
          SetProperties();
          return;
        }
        _cursorY--;
        Correct();
        UpdateCurrentProgram();
        if (_currentProgram != null)
        {
          m_dtStartTime = _currentProgram.StartTime;
        }
        return;
      }
      Correct();
      Update(false);
      SetFocus();
      if (_currentProgram != null)
      {
        m_dtStartTime = _currentProgram.StartTime;
      }
    }

    private void UpdateCurrentProgram()
    {
      if (_cursorX < 0)
      {
        return;
      }
      if (_cursorY < 0)
      {
        return;
      }
      if (_cursorY == 0)
      {
        SetProperties();
        SetFocus();
        return;
      }
      int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
      GUIButton3PartControl img = GetControl(iControlId) as GUIButton3PartControl;
      ;
      if (null != img)
      {
        SetFocus();
        _currentProgram = (Program)img.Data;
        SetProperties();
      }
    }

    /// <summary>
    /// Show or hide group button
    /// </summary>
    protected override void UpdateGroupButton()
    {
      // text for button
      String GroupButtonText = " ";

      // show/hide tvgroup button
      GUIControl btnTvGroup = GetControl((int)Controls.CHANNEL_GROUP_BUTTON) as GUIControl;

      if (btnTvGroup != null)
        btnTvGroup.Visible = GroupButtonAvail;

      // set min index for focus handling
      if (GroupButtonAvail)
      {
        MinYIndex = -1; // allow focus of button
        GroupButtonText = String.Format("{0}: {1}", GUILocalizeStrings.Get(971), TVHome.Navigator.CurrentGroup.GroupName);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Group", TVHome.Navigator.CurrentGroup.GroupName);
      }
      else
      {
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Group", TVHome.Navigator.CurrentGroup.GroupName);
        MinYIndex = 0;
      }

      // Set proper text for group change button; Empty string to hide text if only 1 group
      // (split between button and rotated label due to focusing issue of rotated buttons)
      GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.ChangeGroup", GroupButtonText); // existing string "group"
    }

    private void OnRight()
    {
      if (_cursorX < 0)
      {
        return;
      }
      UnFocus();
      if (_cursorY < ProgramCount(_cursorX))
      {
        _cursorY++;
        Correct();
        UpdateCurrentProgram();
        if (_currentProgram != null)
        {
          m_dtStartTime = _currentProgram.StartTime;
        }
        return;
      }
      else
      {
        _viewingTime = _viewingTime.AddMinutes(_timePerBlock);
        // Check new day
        int iDay = CalcDays();
        if (iDay >= MaxDaysInGuide)
        {
          _viewingTime = _viewingTime.AddMinutes(-_timePerBlock);
        }
      }
      Correct();
      Update(false);
      SetFocus();
      if (_currentProgram != null)
      {
        m_dtStartTime = _currentProgram.StartTime;
      }
    }

    private void updateSingleChannelNumber()
    {
      // update selected channel
      if (!_singleChannelView)
      {
        _singleChannelNumber = _cursorX + ChannelOffset;
        if (_singleChannelNumber < 0)
        {
          _singleChannelNumber = 0;
        }
        if (_singleChannelNumber >= _channelList.Count)
        {
          _singleChannelNumber -= _channelList.Count;
        }
        // instead of direct casting us "as"; else it fails for other controls!
        GUIButton3PartControl img = GetControl(_cursorX + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
        ;
        if (null != img)
        {
          _currentChannel = (Channel)img.Data;
        }
      }
    }

    private void UnFocus()
    {
      if (_cursorX < 0)
      {
        return;
      }
      if (_cursorY == 0 || _cursorY == MinYIndex) // either channel or group button
      {
        // Handle the tv channel buttons (the left column of buttons).
        int controlid;
        if (_cursorY == -1)
          controlid = (int)Controls.CHANNEL_GROUP_BUTTON;
        else
          controlid = (int)Controls.IMG_CHAN1 + _cursorX;

        GUIButton3PartControl img = GetControl(controlid) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          if (_useColorsForButtons)
          {
            if (_cursorY == 0)
            {
              img.ColourDiffuse = _guideColorChannelButton;
            }
            else
            {
              img.ColourDiffuse = _guideColorGroupButton;
            }
          }
        }

        GUIControl.UnfocusControl(GetID, controlid);
      }
      else
      {
        // Handle the main guide buttons (all but the left column of buttons).
        Correct();
        int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
        GUIButton3PartControl img = GetControl(iControlId) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          if (_useColorsForButtons && _currentProgram != null)
          {
            if (_currentProgram.IsRunningAt(DateTime.Now))
            {
              img.ColourDiffuse = GetColorForProgram(_currentProgram, true);
            }
            else if (_currentProgram.EndedBefore(DateTime.Now))
            {
              img.ColourDiffuse = _guideColorProgramEnded;
            }
            else
            {
              img.ColourDiffuse = GetColorForProgram(_currentProgram, false);
            }
          }
        }
        GUIControl.UnfocusControl(GetID, iControlId);
      }
    }

    private void SetFocus()
    {
      if (_cursorX < 0)
      {
        return;
      }
      if (_cursorY == 0 || _cursorY == MinYIndex) // either channel or group button
      {
        // Handle the tv channel buttons (the left column of buttons).
        int controlid;
        GUIControl.UnfocusControl(GetID, (int)Controls.SPINCONTROL_DAY);
        GUIControl.UnfocusControl(GetID, (int)Controls.SPINCONTROL_TIME_INTERVAL);

        if (_cursorY == -1)
          controlid = (int)Controls.CHANNEL_GROUP_BUTTON;
        else
          controlid = (int)Controls.IMG_CHAN1 + _cursorX;

        GUIButton3PartControl img = GetControl(controlid) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          if (_useBorderHighlight)
          {
            SetFocusBorder(ref img);
          }
          else if (_useColorsForButtons)
          {
            if (_cursorY == -1)
            {
              img.ColourDiffuse = _guideColorGroupButtonSelected;
            }
            else
            {
              img.ColourDiffuse = _guideColorChannelButtonSelected;
            }
          }
        }

        GUIControl.FocusControl(GetID, controlid);
      }
      else
      {
        // Handle the main guide buttons (all but the left column of buttons).
        Correct();
        int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
        GUIButton3PartControl img = GetControl(iControlId) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          if (_useBorderHighlight)
          {
            SetFocusBorder(ref img);
          }
          else if (_useColorsForButtons)
          {
            img.ColourDiffuse = _guideColorProgramSelected;
          }

          _currentProgram = img.Data as Program;
          SetProperties();
        }
        GUIControl.FocusControl(GetID, iControlId);
      }
    }

    private void SetFocusBorder(ref GUIButton3PartControl button)
    {
      // Setup the highlight border for the specified button control.
      // The focus overlay needs to have an outline border to highlight the selected button.
      // To get the border to render on top of everything else we need to move the selected
      // control to the end of the windows list of children controls.
      GUIControl ctrl = (GUIControl)button;
      SendToFront(ref ctrl);

      if (button.RenderLeft && button.RenderRight)
      {
        button.SetBorderTFM("0,0,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFL("6,0,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFR("0,6,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
      }
      else if (button.RenderLeft && !button.RenderRight)
      {
        button.SetBorderTFM("0,6,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFL("6,0,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFR("0", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
      }
      else if (!button.RenderLeft && button.RenderRight)
      {
        button.SetBorderTFM("6,0,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFL("0", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFR("0,6,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
      }
      else
      {
        button.SetBorderTFM("6,6,6,6", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFL("0", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
        button.SetBorderTFR("0", GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE, false, false, "tvguide_highlight_border.png", _guideColorBorderHighlight, true, true);
      }
    }

    private void Correct()
    {
      int iControlId;
      if (_cursorY < MinYIndex) // either channel or group button
      {
        _cursorY = MinYIndex;
      }
      if (_cursorY > 0)
      {
        while (_cursorY > 0)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
          GUIControl cntl = GetControl(iControlId);
          if (cntl == null)
          {
            _cursorY--;
          }
          else if (!cntl.IsVisible)
          {
            _cursorY--;
          }
          else
          {
            break;
          }
        }
      }
      if (_cursorX < 0)
      {
        _cursorX = 0;
      }
      if (!_singleChannelView)
      {
        while (_cursorX > 0)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (0) * ColID;
          GUIControl cntl = GetControl(iControlId);
          if (cntl == null)
          {
            _cursorX--;
          }
          else if (!cntl.IsVisible)
          {
            _cursorX--;
          }
          else
          {
            break;
          }
        }
      }
    }

    private void ShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(924)); //Menu

        if (_currentChannel != null)
        {
          dlg.AddLocalizedString(938); // View this channel
        }

        if (_currentProgram.IdProgram != 0)
        {
          dlg.AddLocalizedString(1041); //Upcoming episodes
        }

        if (_currentProgram != null && _currentProgram.StartTime > DateTime.Now)
        {
          if (_currentProgram.Notify)
          {
            dlg.AddLocalizedString(1212); // cancel reminder
          }
          else
          {
            dlg.AddLocalizedString(1040); // set reminder
          }
        }

        dlg.AddLocalizedString(939); // Switch mode

        bool isRecordingNoEPG = false;

        if (_currentProgram != null && _currentChannel != null && _currentTitle.Length > 0)
        {
          if (_currentProgram.IdProgram == 0) // no EPG program recording., only allow to stop it.
          {
            isRecordingNoEPG = IsRecordingNoEPG(_currentProgram.ReferencedChannel());
            if (isRecordingNoEPG)
            {
              dlg.AddLocalizedString(629); // stop non EPG Recording
            }
            else
            {
              dlg.AddLocalizedString(264); // start non EPG Recording
            }
          }
          else if (!_currentRecOrNotify)
          {
            dlg.AddLocalizedString(264); // Record
          }

          else
          {
            dlg.AddLocalizedString(637); // Edit Recording
          }
        }
        //dlg.AddLocalizedString(937);// Reload tvguide

        if (TVHome.Navigator.Groups.Count > 1)
        {
          dlg.AddLocalizedString(971); // Group
        }

        dlg.AddLocalizedString(368); // IMDB

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedId)
        {

          case 1041:
            ShowProgramInfo();
            Log.Debug("TVGuide: show episodes or repeatings for current show");
            break;
          case 368: // IMDB
            OnGetIMDBInfo();
            break;
          case 971: //group
            OnSelectChannelGroup();
            break;
          case 1040: // set reminder
          case 1212: // cancel reminder
            OnNotify();
            break;

          case 938: // view channel

            Log.Debug("viewch channel:{0}", _currentChannel);
            TVHome.ViewChannelAndCheck(_currentProgram.ReferencedChannel());
            if (TVHome.Card.IsTimeShifting && TVHome.Card.IdChannel == _currentProgram.ReferencedChannel().IdChannel)
            {
              g_Player.ShowFullScreenWindow();
            }
            return;


          case 939: // switch mode
            OnSwitchMode();
            break;
          case 629: //stop recording
            Schedule schedule = Schedule.FindNoEPGSchedule(_currentProgram.ReferencedChannel());
            TVUtil.DeleteRecAndEntireSchedWithPrompt(schedule);
            Update(true); //remove RED marker
            break;

          case 637: // edit recording
          case 264: // record
            if (_currentProgram.IdProgram == 0)
            {
              TVHome.StartRecordingSchedule(_currentProgram.ReferencedChannel(), true);
              _currentProgram.IsRecordingOncePending = true;
              Update(true); //remove RED marker
            }
            else
            {
              OnRecordContext();
            }
            break;
        }
      }
    }

    private void OnSwitchMode()
    {
      UnFocus();
      _singleChannelView = !_singleChannelView;
      if (_singleChannelView)
      {
        _backupCursorX = _cursorY;
        _backupCursorY = _cursorX;
        _backupChannelOffset = ChannelOffset;

        _programOffset = _cursorY = _cursorX = 0;
        _recalculateProgramOffset = true;
      }
      else
      {
        //focus current channel
        _cursorY = 0;
        _cursorX = _backupCursorY;
        ChannelOffset = _backupChannelOffset;
      }
      Update(true);
      SetFocus();
    }

    private void ShowProgramInfo()
    {
      if (_currentProgram == null)
      {
        return;
      }
      TVProgramInfo.CurrentProgram = _currentProgram;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_PROGRAM_INFO);
    }

    private void OnGetIMDBInfo()
    {
      IMDBMovie movieDetails = new IMDBMovie();
      movieDetails.SearchString = _currentProgram.Title;
      if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, false))
      {
        TvBusinessLayer dbLayer = new TvBusinessLayer();

        IList<Program> progs = dbLayer.GetProgramExists(Channel.Retrieve(_currentProgram.IdChannel),
                                                        _currentProgram.StartTime, _currentProgram.EndTime);
        if (progs != null && progs.Count > 0)
        {
          Program prog = (Program)progs[0];
          prog.Description = movieDetails.Plot;
          // prog.Genre = movieDetails.Genre;
          prog.StarRating = (int)movieDetails.Rating;
          prog.Persist();
        }
        GUIVideoInfo videoInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_INFO);
        videoInfo.AllocResources();
        videoInfo.Movie = movieDetails;
        GUIButtonControl btnPlay = (GUIButtonControl)videoInfo.GetControl(2);
        if (btnPlay != null)
        {
          btnPlay.Visible = false;
        }
        GUICheckButton btnCast = (GUICheckButton)videoInfo.GetControl(4);
        if (btnCast != null)
        {
          btnCast.Visible = false;
        }
        GUICheckButton btnWatched = (GUICheckButton)videoInfo.GetControl(6);
        if (btnWatched != null)
        {
          btnWatched.Visible = false;
        }
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_VIDEO_INFO);
      }
      else
      {
        Log.Info("IMDB Fetcher: Nothing found");
      }
    }

    /// <summary>
    /// Handle the selection of a guide entry.
    /// </summary>
    /// <param name="isItemSelected"></param>
    /// <returns>true if a channel was attempted to be tuned; that the channel is or is not playing is not indicated</returns>
    private bool OnSelectItem(bool isItemSelected)
    {
      bool tuneAttempted = false;
      TVHome.Navigator.UpdateCurrentChannel();
      if (_currentProgram == null)
      {
        return tuneAttempted;
      }
      if (isItemSelected)
      {
        if (_currentProgram.IsRunningAt(DateTime.Now) || _currentProgram.EndTime <= DateTime.Now)
        {
          //view this channel
          if (g_Player.Playing && g_Player.IsTVRecording)
          {
            g_Player.Stop(true);
          }
          try
          {
            string fileName = "";
            bool isRec = _currentProgram.IsRecording;

            Recording rec = null;
            if (isRec)
            {
              rec = Recording.ActiveRecording(_currentProgram.Title, _currentProgram.IdChannel);
            }


            if (rec != null)
            {
              fileName = rec.FileName;
            }

            if (!string.IsNullOrEmpty(fileName)) //are we really recording ?
            {
              Log.Info("TVGuide: clicked on a currently running recording");
              GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
              if (dlg == null)
              {
                return tuneAttempted;
              }

              dlg.Reset();
              dlg.SetHeading(_currentProgram.Title);
              dlg.AddLocalizedString(979); //Play recording from beginning
              dlg.AddLocalizedString(938); //View this channel
              dlg.DoModal(GetID);

              if (dlg.SelectedLabel == -1)
              {
                return tuneAttempted;
              }
              if (_recordingList != null)
              {
                Log.Debug("TVGuide: Found current program {0} in recording list", _currentTitle);
                switch (dlg.SelectedId)
                {
                  case 979: // Play recording from beginning
                    {
                      Recording recDB = Recording.Retrieve(fileName);
                      if (recDB != null)
                      {
                        TVUtil.PlayRecording(recDB);
                      }
                    }
                    return tuneAttempted;

                  case 938: // View this channel
                    {
                      TVHome.ViewChannelAndCheck(_currentProgram.ReferencedChannel());
                      if (g_Player.Playing)
                      {
                        g_Player.ShowFullScreenWindow();
                      }
                    }
                    tuneAttempted = true;
                    return tuneAttempted;
                }
              }
              else
              {
                Log.Info("EPG: _recordingList was not available");
              }


              if (string.IsNullOrEmpty(fileName))
              {
                TVHome.ViewChannelAndCheck(_currentProgram.ReferencedChannel());
                if (g_Player.Playing)
                {
                  g_Player.ShowFullScreenWindow();
                }
                tuneAttempted = true;
              }
            }
            else //not recording
            {
              // clicked the show we're currently watching
              if (TVHome.Navigator.Channel != null && TVHome.Navigator.Channel.IdChannel == _currentChannel.IdChannel && g_Player.Playing && g_Player.IsTV)
              {
                Log.Debug("TVGuide: clicked on a currently running show");
                GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
                if (dlg == null)
                {
                  return tuneAttempted;
                }

                dlg.Reset();
                dlg.SetHeading(_currentProgram.Title);
                dlg.AddLocalizedString(938); //View this channel
                dlg.AddLocalizedString(1041); //Upcoming episodes
                dlg.DoModal(GetID);

                if (dlg.SelectedLabel == -1)
                {
                  return tuneAttempted;
                }

                switch (dlg.SelectedId)
                {
                  case 1041:
                    ShowProgramInfo();
                    Log.Debug("TVGuide: show episodes or repeatings for current show");
                    break;
                  case 938:
                    Log.Debug("TVGuide: switch currently running show to fullscreen");
                    GUIWaitCursor.Show();
                    try
                    {
                      TVHome.ViewChannelAndCheck(_currentProgram.ReferencedChannel());
                    }
                    finally
                    {
                      GUIWaitCursor.Hide();
                    }
                    if (g_Player.Playing)
                    {
                      g_Player.ShowFullScreenWindow();
                    }
                    else
                    {
                      Log.Debug("TVGuide: no show currently running to switch to fullscreen");
                    }
                    tuneAttempted = true;
                    break;
                }
              }
              else
              {
                bool isPlayingTV = (g_Player.FullScreen && g_Player.IsTV);
                // zap to selected show's channel
                TVHome.UserChannelChanged = true;
                // fixing mantis 1874: TV doesn't start when from other playing media to TVGuide & select program
                GUIWaitCursor.Show();
                try
                {
                  TVHome.ViewChannelAndCheck(_currentProgram.ReferencedChannel());
                }
                finally
                {
                  GUIWaitCursor.Hide();
                }
                if (g_Player.Playing)
                {
                  if (isPlayingTV) GUIWindowManager.CloseCurrentWindow();
                  g_Player.ShowFullScreenWindow();
                }
                tuneAttempted = true;
              }
            } //end of not recording
          }
          finally
          {
            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.Enable(true);
            }
          }

          return tuneAttempted;
        }
        ShowProgramInfo();
        return tuneAttempted;
      }
      else
      {
        ShowProgramInfo();
      }
      return tuneAttempted;
    }

    /// <summary>
    /// "Record" via REC button
    /// </summary>
    private void OnRecord()
    {
      if (_currentProgram == null)
      {
        return;
      }
      if ((_currentProgram.IsRunningAt(DateTime.Now) ||
           (_currentProgram.EndTime <= DateTime.Now)))
      {
        //record current programme
        GUIWindow tvHome = GUIWindowManager.GetWindow((int)Window.WINDOW_TV);
        if ((tvHome != null) && (tvHome.GetID != GUIWindowManager.ActiveWindow))
        {
          //tvHome.OnAction(new Action(Action.ActionType.ACTION_RECORD, 0, 0));
          bool didRecStart = TVHome.ManualRecord(_currentProgram.ReferencedChannel(), GetID);
          //refresh view.
          if (didRecStart)
          {
            _recordingExpected = _currentProgram.ReferencedChannel();
          }
        }
      }
      else
      {
        ShowProgramInfo();
      }
    }

    /// <summary>
    /// "Record" entry in context menu
    /// </summary>
    private void OnRecordContext()
    {
      if (_currentProgram == null)
      {
        return;
      }
      ShowProgramInfo();
    }

    private void CheckRecordingConflicts() { }

    private void OnPageUp()
    {
      int Steps;
      if (_singleChannelView)
      {
        Steps = _channelCount; // all available rows
      }
      else
      {
        if (_guideContinuousScroll)
        {
          Steps = _channelCount; // all available rows
        }
        else
        {
          // If we're on the first channel in the guide then allow one step to get back to the end of the guide.
          if (ChannelOffset == 0 && _cursorX == 0)
          {
            Steps = 1;
          }
          else
          {
            // only number of additional avail channels
            Steps = Math.Min(ChannelOffset + _cursorX, _channelCount);
          }
        }
      }
      UnFocus();
      for (int i = 0; i < Steps; ++i)
      {
        OnUp(false, true);
      }
      Correct();
      Update(false);
      SetFocus();
    }

    private void OnPageDown()
    {
      int Steps;
      if (_singleChannelView)
        Steps = _channelCount; // all available rows
      else
      {
        if (_guideContinuousScroll)
        {
          Steps = _channelCount; // all available rows
        }
        else
        {
          // If we're on the last channel in the guide then allow one step to get back to top of guide.
          if (ChannelOffset + (_cursorX + 1) == _channelList.Count)
          {
            Steps = 1;
          }
          else
          {
            // only number of additional avail channels
            Steps = Math.Min(_channelList.Count - ChannelOffset - _cursorX - 1, _channelCount);
          }
        }
      }

      UnFocus();
      for (int i = 0; i < Steps; ++i)
      {
        OnDown(false);
      }
      Correct();
      Update(false);
      SetFocus();
    }

    private void OnNextDay()
    {
      _viewingTime = _viewingTime.AddDays(1.0);
      _recalculateProgramOffset = true;
      Update(false);
      SetFocus();
    }

    private void OnPreviousDay()
    {
      _viewingTime = _viewingTime.AddDays(-1.0);
      _recalculateProgramOffset = true;
      Update(false);
      SetFocus();
    }

    private long GetColorForProgram(Program program, bool onNow)
    {
      // Set the default color in case the genre color is not defined.
      long defaultColor = _defaultGenreColorOnLater;
      if (onNow)
      {
        defaultColor = _defaultGenreColorOnNow;
      }

      if (!_useColorsForGenres)
      {
        return defaultColor;
      }

      // If the program has a movie rating then choose the user specified "movie" genre if it exists.
      MpGenre mpGenre = null;
      if (IsMPAA(program.Classification))
      {
        mpGenre = _mpGenres.Find(x => x.IsMovie == true);
      }
      else
      {
        mpGenre = _mpGenres.Find(x => x.MappedProgramGenres.Contains(program.Genre));
      }

      // If no mapped mp genre could be found or the found genre is disabled then return the default genre color.
      if (mpGenre == null || !mpGenre.Enabled)
      {
        return defaultColor;
      }

      // Return a valid default color if the specified genre does not have a color association.
      long color = defaultColor;
      bool found = false;
      if (onNow)
      {
        found = _genreColorsOnNow.TryGetValue(mpGenre.Name, out color);
      }
      else
      {
        found = _genreColorsOnLater.TryGetValue(mpGenre.Name, out color);
      }

      if (!found)
      {
        color = defaultColor;
      }

      return color;
    }

    private long GetColorFromString(string strColor)
    {
      long result = 0xFFFFFFFF;

      if (long.TryParse(strColor, System.Globalization.NumberStyles.HexNumber, null, out result))
      {
        // Result set in out param
      }
      else if (Color.FromName(strColor).IsKnownColor)
      {
        result = Color.FromName(strColor).ToArgb();
      }

      return result;
    }

    private bool IsMPAA(string classification)
    {
      return ",G,PG,PG-13,R,NC-17,AO,NR,".Contains("," + classification.Trim() + ",");
    }

    private void OnKeyTimeout()
    {
      if (_lineInput.Length == 0)
      {
        // Hide label if no keyed channel number to display.
        GUILabelControl label = GetControl((int)Controls.LABEL_KEYED_CHANNEL) as GUILabelControl;
        if (label != null)
        {
          label.IsVisible = false;
        }
        return;
      }
      TimeSpan ts = DateTime.Now - _keyPressedTimer;
      if (ts.TotalMilliseconds >= 1000)
      {
        // change channel
        int iChannel = Int32.Parse(_lineInput);
        ChangeChannelNr(iChannel);
        _lineInput = String.Empty;
      }
    }

    private void OnKeyCode(char chKey)
    {
      // Don't accept keys when in single channel mode.
      if (_singleChannelView)
      {
        return;
      }

      if (chKey >= '0' && chKey <= '9') //Make sure it's only for the remote
      {
        TimeSpan ts = DateTime.Now - _keyPressedTimer;
        if (_lineInput.Length >= _channelNumberMaxLength || ts.TotalMilliseconds >= 1000)
        {
          _lineInput = String.Empty;
        }
        _keyPressedTimer = DateTime.Now;
        _lineInput += chKey;

        // give feedback to user that numbers are being entered
        // Check for new standalone label control for keyed in channel numbers.
        GUILabelControl label;
        label = GetControl((int)Controls.LABEL_KEYED_CHANNEL) as GUILabelControl;
        if (label != null)
        {
          // Show the keyed channel number.
          label.IsVisible = true;
        }
        else
        {
          label = GetControl((int)Controls.LABEL_TIME1) as GUILabelControl;
        }
        label.Label = _lineInput;

        // Add an underscore "cursor" to visually indicate that more numbers may be entered.
        if (_lineInput.Length < _channelNumberMaxLength)
        {
          label.Label += "_";
        }

        if (_lineInput.Length == _channelNumberMaxLength)
        {
          // change channel
          int iChannel = Int32.Parse(_lineInput);
          ChangeChannelNr(iChannel);

          // Hide the keyed channel number label.
          GUILabelControl labelKeyed = GetControl((int)Controls.LABEL_KEYED_CHANNEL) as GUILabelControl;
          if (labelKeyed != null)
          {
            labelKeyed.IsVisible = false;
          }
        }
      }
    }

    private void ChangeChannelNr(int iChannelNr)
    {
      int iCounter = 0;
      bool found = false;
      int searchChannel = iChannelNr;

      Channel chan;
      int channelDistance = 99999;

      if (_byIndex == false)
      {
        while (iCounter < _channelList.Count && found == false)
        {
          chan = (Channel)_channelList[iCounter].channel;

          if (chan.ChannelNumber == searchChannel)
          {
            iChannelNr = iCounter;
            found = true;
          } //find closest channel number
          else if ((int)Math.Abs(chan.ChannelNumber - searchChannel) < channelDistance)
          {
            channelDistance = (int)Math.Abs(chan.ChannelNumber - searchChannel);
            iChannelNr = iCounter;
          }

          iCounter++;
        }
      }
      else
      {
        iChannelNr--; // offset for indexed channel number
      }
      if (iChannelNr >= 0 && iChannelNr < _channelList.Count)
      {
        UnFocus();
        ChannelOffset = 0;
        _cursorX = 0;

        // Last page adjust (To get a full page channel listing)
        if (iChannelNr > _channelList.Count - Math.Min(_channelList.Count, _channelCount) + 1)
        {
          // minimum of available channel/max visible channels
          ChannelOffset = _channelList.Count - _channelCount;
          iChannelNr = iChannelNr - ChannelOffset;
        }

        while (iChannelNr >= Math.Min(_channelList.Count, _channelCount))
        {
          iChannelNr -= Math.Min(_channelList.Count, _channelCount);
          ChannelOffset += Math.Min(_channelList.Count, _channelCount);
        }
        _cursorX = iChannelNr;
      }
      Update(false);
      SetFocus();
    }

    protected override void LoadSchedules(bool refresh)
    {
      if (refresh)
      {
        _recordingList = Schedule.ListAll();
        return;
      }
    }



    protected override void GetChannels(bool refresh)
    {
      if (refresh || _channelList == null)
      {
        if (_channelList != null)
        {
          if (_channelList.Count < _channelCount)
          {
            _previousChannelCount = _channelList.Count;
          }
          else
          {
            _previousChannelCount = _channelCount;
          }
        }
        _channelList = new List<GuideChannel>();
      }

      if (_channelList.Count == 0)
      {
        try
        {
          if (TVHome.Navigator.CurrentGroup != null)
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            IList<Channel> channels = layer.GetTVGuideChannelsForGroup(TVHome.Navigator.CurrentGroup.IdGroup);
            foreach (Channel chan in channels)
            {
              GuideChannel tvGuidChannel = new GuideChannel();
              tvGuidChannel.channel = chan;

              if (tvGuidChannel.channel.VisibleInGuide && tvGuidChannel.channel.IsTv)
              {
                if (_showChannelNumber)
                {

                  if (_byIndex)
                  {
                    tvGuidChannel.channelNum = _channelList.Count + 1;
                  }
                  else
                  {
                    tvGuidChannel.channelNum = chan.ChannelNumber;
                  }
                }
                tvGuidChannel.strLogo = GetChannelLogo(tvGuidChannel.channel.DisplayName);
                _channelList.Add(tvGuidChannel);
              }
            }
          }
        }
        catch { }

        if (_channelList.Count == 0)
        {
          GuideChannel tvGuidChannel = new GuideChannel();
          tvGuidChannel.channel = new Channel(false, true, 0, DateTime.MinValue, false,
                                              DateTime.MinValue, 0, true, "", GUILocalizeStrings.Get(911), 10000);
          for (int i = 0; i < 10; ++i)
          {
            _channelList.Add(tvGuidChannel);
          }
        }
      }
    }

    protected override void UpdateVerticalScrollbar()
    {
      if (_channelList == null || _channelList.Count <= 0)
      {
        return;
      }
      int channel = _cursorX + ChannelOffset;
      while (channel > 0 && channel >= _channelList.Count)
      {
        channel -= _channelList.Count;
      }
      float current = (float)(_cursorX + ChannelOffset);
      float total = (float)_channelList.Count - 1;

      if (_singleChannelView)
      {
        current = (float)(_cursorX + _programOffset);
        total = (float)_totalProgramCount - 1;
      }
      if (total == 0)
      {
        total = _channelCount;
      }

      float percentage = (current / total) * 100.0f;
      if (percentage < 0)
      {
        percentage = 0;
      }
      if (percentage > 100)
      {
        percentage = 100;
      }
      GUIVerticalScrollbar scrollbar = GetControl((int)Controls.VERT_SCROLLBAR) as GUIVerticalScrollbar;
      if (scrollbar != null)
      {
        scrollbar.Percentage = percentage;
      }
    }

    protected override void UpdateHorizontalScrollbar()
    {
      if (_channelList == null)
      {
        return;
      }
      GUIHorizontalScrollbar scrollbar = GetControl((int)Controls.HORZ_SCROLLBAR) as GUIHorizontalScrollbar;
      if (scrollbar != null)
      {
        float percentage = (float)_viewingTime.Hour * 60 + _viewingTime.Minute +
          (float)_timePerBlock * ((float)_viewingTime.Hour / 24.0f);
        percentage /= (24.0f * 60.0f);
        percentage *= 100.0f;
        if (percentage < 0)
        {
          percentage = 0;
        }
        if (percentage > 100)
        {
          percentage = 100;
        }
        if (_singleChannelView)
        {
          percentage = 0;
        }

        if ((int)percentage != (int)scrollbar.Percentage)
        {
          scrollbar.Percentage = percentage;
        }
      }
    }

    protected override int CalcDays()
    {
      int iDay = _viewingTime.DayOfYear - DateTime.Now.DayOfYear;
      if (_viewingTime.Year > DateTime.Now.Year)
      {
        iDay += (new DateTime(DateTime.Now.Year, 12, 31)).DayOfYear;
      }
      return iDay;
    }

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
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

    #region TV Database callbacks

    protected void TVDatabase_On_notifyListChanged()
    {
      /// @
      /*
      if (_notifyList != null)
      {
        _notifyList.Clear();
        TVDatabase.GetNotifies(_notifyList, false);
        _needUpdate = true;
      }
       */
    }

    protected void ConflictManager_OnConflictsUpdated()
    {
      _needUpdate = true;
    }

    protected void TVDatabase_OnProgramsChanged()
    {
      _needUpdate = true;
    }

    #endregion

    /// <summary>
    /// Calculates the duration of a program and sets the Duration property
    /// </summary>
    private string GetDuration(Program program)
    {
      if (program.Title == "No TVGuide data available")
      {
        return "";
      }
      string space = " ";
      DateTime progStart = program.StartTime;
      DateTime progEnd = program.EndTime;
      TimeSpan progDuration = progEnd.Subtract(progStart);
      string duration = "";
      switch (progDuration.Hours)
      {
        case 0:
          duration = progDuration.Minutes + space + GUILocalizeStrings.Get(3004);
          break;
        case 1:
          if (progDuration.Minutes == 1)
          {
            duration = progDuration.Hours + space + GUILocalizeStrings.Get(3001) + ", " + progDuration.Minutes + space +
              GUILocalizeStrings.Get(3003);
          }
          else if (progDuration.Minutes > 1)
          {
            duration = progDuration.Hours + space + GUILocalizeStrings.Get(3001) + ", " + progDuration.Minutes + space +
              GUILocalizeStrings.Get(3004);
          }
          else
          {
            duration = progDuration.Hours + space + GUILocalizeStrings.Get(3001);
          }
          break;
        default:
          if (progDuration.Minutes == 1)
          {
            duration = progDuration.Hours + " Hours" + ", " + progDuration.Minutes + space +
              GUILocalizeStrings.Get(3003);
          }
          else if (progDuration.Minutes > 0)
          {
            duration = progDuration.Hours + " Hours" + ", " + progDuration.Minutes + space +
              GUILocalizeStrings.Get(3004);
          }
          else
          {
            duration = progDuration.Hours + space + GUILocalizeStrings.Get(3002);
          }
          break;
      }
      return duration;
    }

    private string GetDurationAsMinutes(Program program)
    {
      if (program.Title == "No TVGuide data available")
      {
        return "";
      }
      DateTime progStart = program.StartTime;
      DateTime progEnd = program.EndTime;
      TimeSpan progDuration = progEnd.Subtract(progStart);
      return progDuration.TotalMinutes + " " + GUILocalizeStrings.Get(2998);
    }

    /// <summary>
    /// Calculates how long from current time a program starts or started, set the TimeFromNow property
    /// </summary>
    private string GetStartTimeFromNow(Program program)
    {
      string timeFromNow = String.Empty;
      if (program.Title == "No TVGuide data available")
      {
        return timeFromNow;
      }
      string space = " ";
      string strRemaining = String.Empty;
      DateTime progStart = program.StartTime;
      TimeSpan timeRelative = progStart.Subtract(DateTime.Now);
      if (timeRelative.Days == 0)
      {
        if (timeRelative.Hours >= 0 && timeRelative.Minutes >= 0)
        {
          switch (timeRelative.Hours)
          {
            case 0:
              if (timeRelative.Minutes == 1)
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3003); // starts in 1 minute
              }
              else if (timeRelative.Minutes > 1)
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3004); //starts in x minutes
              }
              else
              {
                timeFromNow = GUILocalizeStrings.Get(3013);
              }
              break;
            case 1:
              if (timeRelative.Minutes == 1)
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                  GUILocalizeStrings.Get(3001) + ", " + timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3003); //starts in 1 hour, 1 minute
              }
              else if (timeRelative.Minutes > 1)
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                  GUILocalizeStrings.Get(3001) + ", " + timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3004); //starts in 1 hour, x minutes
              }
              else
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + GUILocalizeStrings.Get(3001);
                //starts in 1 hour
              }
              break;
            default:
              if (timeRelative.Minutes == 1)
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                  GUILocalizeStrings.Get(3002) + ", " + timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3003); //starts in x hours, 1 minute
              }
              else if (timeRelative.Minutes > 1)
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                  GUILocalizeStrings.Get(3002) + ", " + timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3004); //starts in x hours, x minutes
              }
              else
              {
                timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                  GUILocalizeStrings.Get(3002); //starts in x hours
              }
              break;
          }
        }
        else //already started
        {
          DateTime progEnd = program.EndTime;
          TimeSpan tsRemaining = DateTime.Now.Subtract(progEnd);
          if (tsRemaining.Minutes > 0)
          {
            timeFromNow = GUILocalizeStrings.Get(3016);
            return timeFromNow;
          }
          switch (tsRemaining.Hours)
          {
            case 0:
              if (timeRelative.Minutes == 1)
              {
                strRemaining = "(" + -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3018) + ")";
                //(1 Minute Remaining)
              }
              else
              {
                strRemaining = "(" + -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3010) + ")";
                //(x Minutes Remaining)
              }
              break;
            case -1:
              if (timeRelative.Minutes == 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3001) + ", " +
                  -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3018) + ")";
                //(1 Hour,1 Minute Remaining)
              }
              else if (timeRelative.Minutes > 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3001) + ", " +
                  -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3010) + ")";
                //(1 Hour,x Minutes Remaining)
              }
              else
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3012) + ")";
                //(1 Hour Remaining)
              }
              break;
            default:
              if (timeRelative.Minutes == 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3002) + ", " +
                  -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3018) + ")";
                //(x Hours,1 Minute Remaining)
              }
              else if (timeRelative.Minutes > 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3002) + ", " +
                  -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3010) + ")";
                //(x Hours,x Minutes Remaining)
              }
              else
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3012) + ")";
                //(x Hours Remaining)
              }
              break;
          }
          switch (timeRelative.Hours)
          {
            case 0:
              if (timeRelative.Minutes == -1)
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3007) + space + strRemaining; //Started 1 Minute ago
              }
              else if (timeRelative.Minutes < -1)
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Minutes + space +
                  GUILocalizeStrings.Get(3008) + space + strRemaining; //Started x Minutes ago
              }
              else
              {
                timeFromNow = GUILocalizeStrings.Get(3013); //Starting Now
              }
              break;
            case -1:
              if (timeRelative.Minutes == -1)
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3001) +
                  ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3007) + " " + strRemaining;
                //Started 1 Hour,1 Minute ago
              }
              else if (timeRelative.Minutes < -1)
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3001) +
                  ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3008) + " " + strRemaining;
                //Started 1 Hour,x Minutes ago
              }
              else
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3005) +
                  space + strRemaining; //Started 1 Hour ago
              }
              break;
            default:
              if (timeRelative.Minutes == -1)
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3006) +
                  ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3008) + " " + strRemaining;
                //Started x Hours,1 Minute ago
              }
              else if (timeRelative.Minutes < -1)
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3006) +
                  ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3008) + " " + strRemaining;
                //Started x Hours,x Minutes ago
              }
              else
              {
                timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3006) +
                  space + strRemaining; //Started x Hours ago
              }
              break;
          }
        }
      }
      else
      {
        if (timeRelative.Days == 1)
        {
          timeFromNow = GUILocalizeStrings.Get(3009) + space + timeRelative.Days + space + GUILocalizeStrings.Get(3014);
          //Starts in 1 Day
        }
        else
        {
          timeFromNow = GUILocalizeStrings.Get(3009) + space + timeRelative.Days + space + GUILocalizeStrings.Get(3015);
          //Starts in x Days
        }
      }
      return timeFromNow;
    }

    protected override void setGuideHeadingVisibility(bool visible)
    {
      // can't rely on the heading text control having a unique id, so locate it using the localised heading string.
      // todo: update all skins to have a unique id for this control...?
      foreach (GUIControl control in controlList)
      {
        if (control is GUILabelControl)
        {
          if (((GUILabelControl)control).Label == GUILocalizeStrings.Get(4)) // TV Guide heading
          {
            control.Visible = visible;
          }
        }
      }
    }

    protected override void setSingleChannelLabelVisibility(bool visible)
    {
      GUILabelControl channelLabel = GetControl((int)Controls.SINGLE_CHANNEL_LABEL) as GUILabelControl;
      GUIImage channelImage = GetControl((int)Controls.SINGLE_CHANNEL_IMAGE) as GUIImage;
      GUISpinControl timeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;

      if (channelLabel != null)
      {
        channelLabel.Visible = visible;
      }

      if (channelImage != null)
      {
        channelImage.Visible = visible;
      }

      if (timeInterval != null)
      {
        // If the x position of the control is negative then we assume that the control is not in the viewable area
        // and so it should not be made visible.  Skinners can set the x position negative to effectively remove the control
        // from the window.
        if (timeInterval.XPosition < 0)
        {
          timeInterval.Visible = false;
        }
        else
        {
          timeInterval.Visible = !visible;
        }
      }
    }

    #endregion

    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      pDlgOK.SetHeading(195);
      pDlgOK.SetLine(1, fetcher.MovieName);
      pDlgOK.SetLine(2, string.Empty);
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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

    public bool OnActorInfoStarting(IMDBFetcher fetcher)
    {
      // won't occure
      return true;
    }

    public bool OnSelectActor(IMDBFetcher fetcher, out int selected)
    {
      // won't occure
      selected = 0;
      return true;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      Log.Info("IMDB Fetcher: OnDetailsNotFound");
      // show dialog...
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      // show dialog...
      pDlgOK.SetHeading(195);
      pDlgOK.SetLine(1, fetcher.MovieName);
      pDlgOK.SetLine(2, string.Empty);
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      movieName = fetcher.MovieName;
      if (GetKeyboard(ref movieName))
      {
        if (movieName == string.Empty)
        {
          return false;
        }
        return true;
      }
      movieName = string.Empty;
      return false;
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      // more then 1 movie found
      // ask user to select 1
      pDlgSelect.Reset();
      pDlgSelect.SetHeading(196); //select movie
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
      pDlgSelect.EnableButton(true);
      pDlgSelect.SetButtonLabel(413); // manual
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

      // and wait till user selects one
      selectedMovie = pDlgSelect.SelectedLabel;
      if (pDlgSelect.IsButtonPressed)
      {
        return true;
      }
      if (selectedMovie == -1)
      {
        return false;
      }
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
  }
}
