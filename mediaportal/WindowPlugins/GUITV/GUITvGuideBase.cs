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

#region usings
using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.Configuration;
#endregion


namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  public class GUITvGuideBase : GUIWindow
  {
    #region constants
    const int MaxDaysInGuide = 30;
    const int RowID = 1000;
    const int ColID = 10;

    const int GUIDE_COMPONENTID_START = 50000;// Start for numbering IDs of automaticaly generated TVguide components for channels and programs
    #endregion

    #region enums
    enum Controls
    {
      PANEL_BACKGROUND = 2,
      SPINCONTROL_DAY = 6,
      SPINCONTROL_TIME_INTERVAL = 8,
      CHANNEL_IMAGE_TEMPLATE = 7,
      CHANNEL_LABEL_TEMPLATE = 18,
      LABEL_GENRE_TEMPLATE = 23,
      LABEL_TITLE_TEMPLATE = 24,
      VERTICAL_LINE = 25,
      LABEL_TITLE_DARK_TEMPLATE = 26,
      LABEL_GENRE_DARK_TEMPLATE = 30,
      CHANNEL_TEMPLATE = 20, // Channel rectangle and row height

      HORZ_SCROLLBAR = 28,
      VERT_SCROLLBAR = 29,
      LABEL_TIME1 = 40, // first and template
      IMG_CHAN1 = 50,
      IMG_CHAN1_LABEL = 70,
      IMG_TIME1 = 90, // first and template
      IMG_REC_PIN = 31,
      SINGLE_CHANNEL_LABEL = 32,
      SINGLE_CHANNEL_IMAGE = 33
    };
    #endregion

    #region variables

    DateTime _viewingTime = DateTime.Now;
    int _channelOffset = 0;
    List<TVChannel> _channelList = new List<TVChannel>();
    List<TVRecording> _recordingList = new List<TVRecording>();
    int _timePerBlock = 30; // steps of 30 minutes
    int _channelCount = 5;
    int _numberOfBlocks = 4;
    int _cursorY = 0;
    int _cursorX = 0;
    string _currentTitle = string.Empty;
    string _currentTime = string.Empty;
    string _currentTvChannel = string.Empty;
    long _currentStartTime = 0;
    long _currentEndTime = 0;
    TVProgram _currentProgram = null;
    static string _tvGuideFileName;
    static System.IO.FileSystemWatcher _tvGuideFileWatcher = null;
    bool _needUpdate = false;
    bool _autoTurnOnTv = false;
    bool _disableXMLTVImportOption = false;
    DateTime m_dtStartTime = DateTime.Now;
    bool _useColorsForGenres = false;
    ArrayList _colorList = new ArrayList();
    bool _singleChannelView = false;
    int _programOffset = 0;
    int _totalProgramCount = 0;
    int _singleChannelNumber = 0;
    bool _showChannelLogos = false;
    List<TVNotify> _notifyList = new List<TVNotify>();
    int _backupCursorX = 0;
    int _backupCursorY = 0;
    int _backupChannelOffset = 0;
    DateTime _updateTimer = DateTime.Now;

    DateTime _keyPressedTimer = DateTime.Now;
    string _lineInput = string.Empty;
    static bool _workerThreadRunning = false;

    bool _byIndex = false;

    #endregion

    #region ctor
    public GUITvGuideBase()
    {

      _colorList.Add(Color.Red);
      _colorList.Add(Color.Green);
      _colorList.Add(Color.Blue);
      _colorList.Add(Color.Cyan);
      _colorList.Add(Color.Magenta);
      _colorList.Add(Color.DarkBlue);
      _colorList.Add(Color.Brown);
      _colorList.Add(Color.Fuchsia);
      _colorList.Add(Color.Khaki);
      _colorList.Add(Color.SteelBlue);
      _colorList.Add(Color.SaddleBrown);
      _colorList.Add(Color.Chocolate);
      _colorList.Add(Color.DarkMagenta);
      _colorList.Add(Color.DarkSeaGreen);
      _colorList.Add(Color.Coral);
      _colorList.Add(Color.DarkGray);
      _colorList.Add(Color.DarkOliveGreen);
      _colorList.Add(Color.DarkOrange);
      _colorList.Add(Color.ForestGreen);
      _colorList.Add(Color.Honeydew);
      _colorList.Add(Color.Gray);
      _colorList.Add(Color.Tan);
      _colorList.Add(Color.Silver);
      _colorList.Add(Color.SeaShell);
      _colorList.Add(Color.RosyBrown);
      _colorList.Add(Color.Peru);
      _colorList.Add(Color.OldLace);
      _colorList.Add(Color.PowderBlue);
      _colorList.Add(Color.SpringGreen);
      _colorList.Add(Color.LightSalmon);
    }
    #endregion

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _currentTvChannel = xmlreader.GetValueAsString("tvguide", "channel", string.Empty);
        _cursorX = xmlreader.GetValueAsInt("tvguide", "ypos", 0);
        _channelOffset = xmlreader.GetValueAsInt("tvguide", "yoffset", 0);
        _autoTurnOnTv = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        _disableXMLTVImportOption = xmlreader.GetValueAsBool("plugins", "TV Movie Clickfinder", false);
        _byIndex = xmlreader.GetValueAsBool("mytv", "byindex", true);
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("tvguide", "channel", _currentTvChannel);
        xmlwriter.SetValue("tvguide", "ypos", _cursorX.ToString());
        xmlwriter.SetValue("tvguide", "yoffset", _channelOffset.ToString());
      }
    }
    #endregion

    #region overrides

    public override int GetFocusControlId()
    {
      int focusedId = base.GetFocusControlId();
      if (_cursorX >= 0 || focusedId == (int)Controls.SPINCONTROL_DAY || focusedId == (int)Controls.SPINCONTROL_TIME_INTERVAL)
      {
        return focusedId;
      }
      else
        return -1;
    }

    protected void Initialize()
    {
      Log.Info("TvGuide StartImportXML: Initialize");
      _tvGuideFileName = "xmltv";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _tvGuideFileName = xmlreader.GetValueAsString("xmltv", "folder", "xmltv");
        _tvGuideFileName = Util.Utils.RemoveTrailingSlash(_tvGuideFileName);
        _useColorsForGenres = xmlreader.GetValueAsBool("xmltv", "colors", false);
      }

      // Create a new FileSystemWatcher and set its properties.
      try
      {
        System.IO.Directory.CreateDirectory(_tvGuideFileName);
      }
      catch (Exception) { }
      if (_tvGuideFileWatcher == null)
      {
        _tvGuideFileWatcher = new FileSystemWatcher();
        _tvGuideFileWatcher.Path = _tvGuideFileName;
        _tvGuideFileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        _tvGuideFileWatcher.Filter = "*.xml";
        // Add event handlers.
        _tvGuideFileWatcher.Changed += new FileSystemEventHandler(OnChanged);
        _tvGuideFileWatcher.Created += new FileSystemEventHandler(OnChanged);
        _tvGuideFileWatcher.Renamed += new RenamedEventHandler(OnRenamed);
        _tvGuideFileWatcher.EnableRaisingEvents = true;
      }

      _tvGuideFileName += @"\tvguide.xml";
    }



    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
            OnKeyCode((char)action.m_key.KeyChar);
          break;

        case Action.ActionType.ACTION_SELECT_ITEM:
          if (GetFocusControlId() != -1 && _cursorX >= 0)
          {
            if (_cursorY == 0)
            {
              OnSwitchMode();
            }
            else
            {
              OnSelectItem();
            }
          }
          break;

        case Action.ActionType.ACTION_RECORD:
          if ((GetFocusControlId() != -1) && (_cursorY > 0) && (_cursorX >= 0))
            OnRecord();
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          {
            int x = (int)action.fAmount1;
            int y = (int)action.fAmount2;
            foreach (GUIControl control in controlList)
            {
              if (control.GetID >= (int)Controls.IMG_CHAN1 + 0 && control.GetID <= (int)Controls.IMG_CHAN1 + _channelCount)
              {
                if (x >= control.XPosition && x < control.XPosition + control.Width)
                {
                  if (y >= control.YPosition && y < control.YPosition + control.Height)
                  {
                    UnFocus();
                    _cursorX = control.GetID - (int)Controls.IMG_CHAN1;
                    _cursorY = 0;

                    if (_singleChannelNumber != _cursorX + _channelOffset)
                      Update(false);
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
              OnUp(true);
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
              _timePerBlock = 60;
            Update(false);
            SetFocus();
          }
          break;
        case Action.ActionType.ACTION_REWIND:
          goto case Action.ActionType.ACTION_MUSIC_REWIND;

        case Action.ActionType.ACTION_MUSIC_REWIND:
          _viewingTime = _viewingTime.AddHours(-3);
          Update(false);
          SetFocus();
          break;
        case Action.ActionType.ACTION_FORWARD:
          goto case Action.ActionType.ACTION_MUSIC_FORWARD;

        case Action.ActionType.ACTION_MUSIC_FORWARD:
          _viewingTime = _viewingTime.AddHours(3);
          Update(false);
          SetFocus();
          break;
        case Action.ActionType.ACTION_DECREASE_TIMEBLOCK:
          {
            if (_timePerBlock > 15)
              _timePerBlock -= 15;
            Update(false);
            SetFocus();
          }
          break;
        case Action.ActionType.ACTION_DEFAULT_TIMEBLOCK:
          {
            _timePerBlock = 30;
            Update(false);
            SetFocus();
          }
          break;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
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
              _channelOffset = 0;
              _cursorX = 0;
              while (iChan >= _channelCount)
              {
                iChan -= _channelCount;
                _channelOffset += _channelCount;
              }
              _cursorX = iChan;
            }
            else
            {
              fPercentage *= (float)_channelList.Count;
              int iChan = (int)fPercentage;
              _channelOffset = 0;
              _cursorX = 0;
              while (iChan >= _channelCount)
              {
                iChan -= _channelCount;
                _channelOffset += _channelCount;
              }
              _cursorX = iChan;
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            base.OnMessage(message);
            SaveSettings();
            _recordingList.Clear();
            if (!GUIGraphicsContext.IsTvWindow(message.Param1))
            {
              if (!g_Player.Playing && !Recorder.IsRadio())
              {
                if (GUIGraphicsContext.ShowBackground)
                {
                  // stop timeshifting & viewing... 

                  Recorder.StopViewing();
                }
              }
            }
            _notifyList = null;
            _channelList = null;
            _recordingList = null;
            _currentProgram = null;

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            _notifyList = new List<TVNotify>();
            _channelList = new List<TVChannel>();
            _recordingList = new List<TVRecording>();

            LoadSettings();
            TVDatabase.GetNotifies(_notifyList, false);

            GUIControl cntlPanel = GetControl((int)Controls.PANEL_BACKGROUND);
            GUIImage cntlChannelTemplate = (GUIImage)GetControl((int)Controls.CHANNEL_TEMPLATE);

            int iHeight = cntlPanel.Height + cntlPanel.YPosition - cntlChannelTemplate.YPosition;
            int iItemHeight = cntlChannelTemplate.Height;
            _channelCount = (int)(((float)iHeight) / ((float)iItemHeight));

            UnFocus();
            _currentProgram = null;
            if (message.Param1 != (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO)
            {
              _viewingTime = DateTime.Now;
              _cursorY = 0;
              _cursorX = 0;
              _channelOffset = 0;
              _singleChannelView = false;
              _showChannelLogos = false;
              if (Recorder.IsViewing())
              {
                _currentTvChannel = Recorder.GetTVChannelName();
                GetChannels();
                for (int i = 0; i < _channelList.Count; i++)
                {
                  TVChannel chan = (TVChannel)_channelList[i];
                  if (chan.Name.Equals(_currentTvChannel))
                  {
                    _cursorX = i;
                    break;
                  }
                }
              }
            }
            while (_cursorX >= _channelCount)
            {
              _cursorX -= _channelCount;
              _channelOffset += _channelCount;

            }
            CheckNewTVGuide();

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
                  case DayOfWeek.Monday: day = GUILocalizeStrings.Get(657); break;
                  case DayOfWeek.Tuesday: day = GUILocalizeStrings.Get(658); break;
                  case DayOfWeek.Wednesday: day = GUILocalizeStrings.Get(659); break;
                  case DayOfWeek.Thursday: day = GUILocalizeStrings.Get(660); break;
                  case DayOfWeek.Friday: day = GUILocalizeStrings.Get(661); break;
                  case DayOfWeek.Saturday: day = GUILocalizeStrings.Get(662); break;
                  default: day = GUILocalizeStrings.Get(663); break;
                }
                day = String.Format("{0} {1}-{2}", day, dtTemp.Day, dtTemp.Month);
                cntlDay.AddLabel(day, iDay);
              }
            }
            GUISpinControl cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            if (cntlTimeInterval != null)
            {
              for (int i = 1; i <= 4; i++)
                cntlTimeInterval.AddLabel(string.Empty, i);
              cntlTimeInterval.Value = 1;
            }
            if (message.Param1 != (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO)
              Update(true);
            else
              Update(false);
            SetFocus();
            if (_currentProgram != null)
            {
              m_dtStartTime = _currentProgram.StartTime;
            }
            UpdateCurrentProgram();

            if (_autoTurnOnTv)
            {
              Log.Info("TVGuide: automatically turn on TV");
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV, GetID, 0, 0, 0, null);
              msg.SendToTargetWindow = true;
              GUIWindowManager.SendThreadMessage(msg);
            }
            else
              Log.Info("TVGuide: do not turn tv on automatically");

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
            _viewingTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _viewingTime.Hour, _viewingTime.Minute, 0, 0);
            _viewingTime = _viewingTime.AddDays(iDay);
            Update(false);
            SetFocus();
            return true;
          }
          if (iControl == (int)Controls.SPINCONTROL_TIME_INTERVAL)
          {
            GUISpinControl cntlTimeInt = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            int iInterval = (cntlTimeInt.Value) + 1;
            if (iInterval > 4)
              iInterval = 4;
            _timePerBlock = iInterval * 15;
            Update(false);
            SetFocus();
            return true;
          }
          if (iControl >= GUIDE_COMPONENTID_START)
          {
            OnSelectItem();
            Update(false);
            SetFocus();
          }
          else if (_cursorY == 0)
          {
            OnSwitchMode();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_DISABLEGUIDEREFRESH:
          TVDatabase.OnProgramsChanged -= new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnProgramsChanged);
          TVDatabase.OnNotifiesChanged -= new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_On_notifyListChanged);
          ConflictManager.OnConflictsUpdated -= new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);
          break;

        case GUIMessage.MessageType.GUI_MSG_ENABLEGUIDEREFRESH:
          TVDatabase.OnProgramsChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnProgramsChanged);
          TVDatabase.OnNotifiesChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_On_notifyListChanged);
          ConflictManager.OnConflictsUpdated += new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);
          break;

      }
      return base.OnMessage(message);
      ;
    }


    public override void Process()
    {
      OnKeyTimeout();
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


          if (_viewingTime.Date.Equals(DateTime.Now.Date))
          {
            int iStartX = GetControl((int)Controls.LABEL_TIME1).XPosition;
            int iWidth = GetControl((int)Controls.LABEL_TIME1 + 1).XPosition - iStartX;
            iWidth *= 4;

            int iMin = _viewingTime.Minute;
            int iStartTime = _viewingTime.Hour * 60 + iMin;
            int iCurTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (iCurTime >= iStartTime)
              iCurTime -= iStartTime;
            else
              iCurTime = 24 * 60 + iCurTime - iStartTime;

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
              Update(false);
          }
          else
            vertLine.IsVisible = false;
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
          vertLine.Render(timePassed);
      }
    }

    #endregion

    #region private members

    protected void CheckNewTVGuide()
    {
      bool shouldImportTvGuide = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strTmp = string.Empty;
        strTmp = xmlreader.GetValueAsString("tvguide", "date", string.Empty);
        if (System.IO.File.Exists(_tvGuideFileName))
        {
          string strFileTime = System.IO.File.GetLastWriteTime(_tvGuideFileName).ToString();
          if (strTmp != strFileTime)
          {
            shouldImportTvGuide = true;
          }
        }
      }
      if (shouldImportTvGuide)
      {
        StartImportXML();
      }
    }

    void Update(bool selectCurrentShow)
    {
      lock (this)
      {
        if (GUIWindowManager.ActiveWindowEx != this.GetID)
        {
          return;
        }

        _updateTimer = DateTime.Now;
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

        int xpos, ypos;
        GUIControl cntlPanel = GetControl((int)Controls.PANEL_BACKGROUND);
        GUIImage cntlChannelImg = (GUIImage)GetControl((int)Controls.CHANNEL_IMAGE_TEMPLATE);
        GUILabelControl cntlChannelLabel = (GUILabelControl)GetControl((int)Controls.CHANNEL_LABEL_TEMPLATE);
        GUILabelControl labelTime = (GUILabelControl)GetControl((int)Controls.LABEL_TIME1);
        GUIImage cntlHeaderBkgImg = (GUIImage)GetControl((int)Controls.IMG_TIME1);
        GUIImage cntlChannelTemplate = (GUIImage)GetControl((int)Controls.CHANNEL_TEMPLATE);

        _showChannelLogos = cntlChannelImg != null;
        if (_showChannelLogos)
          cntlChannelImg.IsVisible = false;
        cntlChannelLabel.IsVisible = false;
        cntlHeaderBkgImg.IsVisible = false;
        labelTime.IsVisible = false;
        cntlChannelTemplate.IsVisible = false;
        int iLabelWidth = (cntlPanel.XPosition + cntlPanel.Width - labelTime.XPosition) / 4;

        // add labels for time blocks 1-4
        int iHour, iMin;
        iMin = _viewingTime.Minute;
        _viewingTime = _viewingTime.AddMinutes(-iMin);
        iMin = (iMin / _timePerBlock) * _timePerBlock;
        _viewingTime = _viewingTime.AddMinutes(iMin);

        DateTime dt = new DateTime();
        dt = _viewingTime;

        for (int iLabel = 0; iLabel < 4; iLabel++)
        {
          xpos = iLabel * iLabelWidth + labelTime.XPosition;
          ypos = labelTime.YPosition;

          GUIImage img = GetControl((int)Controls.IMG_TIME1 + iLabel) as GUIImage;
          if (img == null)
          {
            img = new GUIImage(GetID, (int)Controls.IMG_TIME1 + iLabel, xpos, ypos, iLabelWidth - 4, cntlHeaderBkgImg.RenderHeight, cntlHeaderBkgImg.FileName, 0x0);
            img.AllocResources();
            GUIControl cntl2 = (GUIControl)img;
            Add(ref cntl2);
          }

          img.IsVisible = !_singleChannelView;
          img.Width = iLabelWidth - 4;
          img.Height = cntlHeaderBkgImg.RenderHeight;
          img.SetFileName(cntlHeaderBkgImg.FileName);
          img.SetPosition(xpos, ypos);
          img.DoUpdate();

          GUILabelControl label = GetControl((int)Controls.LABEL_TIME1 + iLabel) as GUILabelControl;
          if (label == null)
          {
            label = new GUILabelControl(GetID, (int)Controls.LABEL_TIME1 + iLabel, xpos, ypos, iLabelWidth, cntlHeaderBkgImg.RenderHeight, labelTime.FontName, string.Empty, labelTime.TextColor, GUIControl.Alignment.ALIGN_CENTER, false);
            label.AllocResources();
            GUIControl cntl = (GUIControl)label;
            this.Add(ref cntl);
          }
          iHour = dt.Hour;
          iMin = dt.Minute;
          string strTime = dt.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          label.Label = strTime;
          dt = dt.AddMinutes(_timePerBlock);

          label.TextAlignment = GUIControl.Alignment.ALIGN_CENTER;
          label.IsVisible = !_singleChannelView;
          label.Width = iLabelWidth;
          label.Height = cntlHeaderBkgImg.RenderHeight;
          label.FontName = labelTime.FontName;
          label.TextColor = labelTime.TextColor;
          label.SetPosition(xpos, ypos);
        }

        // add channels...
        int iHeight = cntlPanel.Height + cntlPanel.YPosition - cntlChannelTemplate.YPosition;
        int iItemHeight = cntlChannelTemplate.Height;

        _channelCount = (int)(((float)iHeight) / ((float)iItemHeight));
        for (int iChan = 0; iChan < _channelCount; ++iChan)
        {
          xpos = cntlChannelTemplate.XPosition;
          ypos = cntlChannelTemplate.YPosition + iChan * iItemHeight;

          //this.Remove((int)Controls.IMG_CHAN1+iChan);
          GUIButton3PartControl imgBut = GetControl((int)Controls.IMG_CHAN1 + iChan) as GUIButton3PartControl;
          if (imgBut == null)
          {
            string strChannelImageFileName = string.Empty;
            if (_showChannelLogos)
              strChannelImageFileName = cntlChannelImg.FileName;
            imgBut = new GUIButton3PartControl(GetID, (int)Controls.IMG_CHAN1 + iChan, xpos, ypos,
              cntlChannelTemplate.Width - 2, cntlChannelTemplate.Height - 2,
              "tvguide_button_selected_left.png",
              "tvguide_button_selected_middle.png",
              "tvguide_button_selected_right.png",
              "tvguide_button_light_left.png",
              "tvguide_button_light_middle.png",
              "tvguide_button_light_right.png",
              strChannelImageFileName);
            imgBut.AllocResources();
            GUIControl cntl = (GUIControl)imgBut;
            Add(ref cntl);
          }

          if (_showChannelLogos)
            imgBut.TexutureIcon = cntlChannelImg.FileName;
          imgBut.Width = cntlChannelTemplate.Width - 2;//labelTime.XPosition-cntlChannelImg.XPosition;
          imgBut.Height = cntlChannelTemplate.Height - 2;//iItemHeight-2;
          imgBut.SetPosition(xpos, ypos);
          imgBut.FontName1 = cntlChannelLabel.FontName;
          imgBut.TextColor1 = cntlChannelLabel.TextColor;
          imgBut.Label1 = string.Empty;
          imgBut.RenderLeft = false;
          imgBut.RenderRight = false;

          if (_showChannelLogos)
          {
            imgBut.IconOffsetX = cntlChannelImg.XPosition;
            imgBut.IconOffsetY = cntlChannelImg.YPosition;
            imgBut.IconWidth = cntlChannelImg.RenderWidth;
            imgBut.IconHeight = cntlChannelImg.RenderHeight;
            imgBut.IconKeepAspectRatio = cntlChannelImg.KeepAspectRatio;
            imgBut.IconCentered = cntlChannelImg.Centered;
            imgBut.IconZoom = cntlChannelImg.Zoom;
          }
          imgBut.TextOffsetX1 = cntlChannelLabel.XPosition;
          imgBut.TextOffsetY1 = cntlChannelLabel.YPosition;
          imgBut.ColourDiffuse = 0xffffffff;
          imgBut.DoUpdate();
        }

        UpdateHorizontalScrollbar();
        UpdateVerticalScrollbar();

        GetChannels();


        string day;
        switch (_viewingTime.DayOfWeek)
        {
          case DayOfWeek.Monday: day = GUILocalizeStrings.Get(657); break;
          case DayOfWeek.Tuesday: day = GUILocalizeStrings.Get(658); break;
          case DayOfWeek.Wednesday: day = GUILocalizeStrings.Get(659); break;
          case DayOfWeek.Thursday: day = GUILocalizeStrings.Get(660); break;
          case DayOfWeek.Friday: day = GUILocalizeStrings.Get(661); break;
          case DayOfWeek.Saturday: day = GUILocalizeStrings.Get(662); break;
          default: day = GUILocalizeStrings.Get(663); break;
        }
        day = String.Format("{0} {1}-{2}", day, _viewingTime.Day, _viewingTime.Month);
        GUIPropertyManager.SetProperty("#TV.Guide.Day", day);

        //2004 03 31 22 20 00
        string strStart = String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}",
          _viewingTime.Year, _viewingTime.Month, _viewingTime.Day,
          _viewingTime.Hour, _viewingTime.Minute, 0);
        DateTime dtStop = new DateTime();
        dtStop = _viewingTime;
        dtStop = dtStop.AddMinutes(_numberOfBlocks * _timePerBlock - 1);
        iMin = dtStop.Minute;
        string strEnd = String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}",
          dtStop.Year, dtStop.Month, dtStop.Day,
          dtStop.Hour, iMin, 0);

        long iStart = Int64.Parse(strStart);
        long iEnd = Int64.Parse(strEnd);


        _recordingList.Clear();
        TVDatabase.GetRecordings(ref _recordingList);

        if (_channelOffset > _channelList.Count)
        {
          _channelOffset = 0;
          _cursorX = 0;
        }

        for (int i = 0; i < controlList.Count; ++i)
        {
          GUIControl cntl = (GUIControl)controlList[i];
          if (cntl.GetID >= GUIDE_COMPONENTID_START)
          {
            cntl.IsVisible = false;
          }
        }

        if (_singleChannelView)
        {
          if (_cursorY == 0)
          {
            //_singleChannelNumber=_cursorX + _channelOffset;
            //if (_singleChannelNumber >= _channelList.Count) _singleChannelNumber-=_channelList.Count;
            //GUIButton3PartControl img=(GUIButton3PartControl)GetControl(_cursorX+(int)Controls.IMG_CHAN1);
            //if (null!=img) _currentTvChannel=img.Label1;
          }
          TVChannel channel = (TVChannel)_channelList[_singleChannelNumber];
          setGuideHeadngVisibility(false);
          RenderSingleChannel(channel);
        }
        else
        {
          // make sure the TV Guide heading is visiable and the single channel labels are not.
          setGuideHeadngVisibility(true);
          setSingleChannelLabelVisibility(false);

          int chan = _channelOffset;
          for (int iChannel = 0; iChannel < _channelCount; iChannel++)
          {
            if (chan < _channelList.Count)
            {
              TVChannel channel = (TVChannel)_channelList[chan];
              RenderChannel(iChannel, channel, iStart, iEnd, selectCurrentShow);
            }
            chan++;
            if (chan >= _channelList.Count)
              chan = 0;
          }

          // update selected channel 
          _singleChannelNumber = _cursorX + _channelOffset;
          if (_singleChannelNumber >= _channelList.Count)
            _singleChannelNumber -= _channelList.Count;
          GUIButton3PartControl img = (GUIButton3PartControl)GetControl(_cursorX + (int)Controls.IMG_CHAN1);
          if (null != img)
            _currentTvChannel = img.Label1;
        }
        UpdateVerticalScrollbar();
      }
    }

    void SetProperties()
    {
      if (_channelList == null)
        return;
      if (_channelList.Count == 0)
        return;
      if (_cursorY == 0 || _currentProgram == null)
      {
        int channel = _cursorX + _channelOffset;
        while (channel >= _channelList.Count)
          channel -= _channelList.Count;
        if (channel < 0)
          channel = 0;
        TVChannel chan = (TVChannel)_channelList[channel];
        string strChannel = chan.Name;
        if (strChannel == null)
          return;
        string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, strChannel);
        GUIPropertyManager.SetProperty("#TV.Guide.Title", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.Time", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.Description", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.Genre", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeName", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.SeriesNumber", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeNumber", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodePart", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeDetail", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.Date", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.StarRating", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.Classification", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.Duration", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.TimeFromNow", string.Empty);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        GUIPropertyManager.SetProperty("#TV.Guide.thumb", strLogo);
        _currentStartTime = 0;
        _currentEndTime = 0;
        _currentTitle = string.Empty;
        _currentTime = string.Empty;
        _currentTvChannel = strChannel;
        GUIControl.HideControl(GetID, (int)Controls.IMG_REC_PIN);
      }
      else if (_currentProgram != null)
      {

        string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, _currentProgram.Channel);
        string strTime = String.Format("{0}-{1}",
          _currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
          _currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        GUIPropertyManager.SetProperty("#TV.Guide.Title", _currentProgram.Title);
        GUIPropertyManager.SetProperty("#TV.Guide.Time", strTime);
        GUIPropertyManager.SetProperty("#TV.Guide.Description", _currentProgram.Description);
        GUIPropertyManager.SetProperty("#TV.Guide.Genre", _currentProgram.Genre);
        GUIPropertyManager.SetProperty("#TV.Guide.Duration", _currentProgram.Duration);
        GUIPropertyManager.SetProperty("#TV.Guide.TimeFromNow", _currentProgram.TimeFromNow);
        if (_currentProgram.Episode == "-")
          GUIPropertyManager.SetProperty("#TV.Guide.EpisodeName", string.Empty);
        else
          GUIPropertyManager.SetProperty("#TV.Guide.EpisodeName", _currentProgram.Episode);
        if (_currentProgram.SeriesNum == "-")
          GUIPropertyManager.SetProperty("#TV.Guide.SeriesNumber", string.Empty);
        else
          GUIPropertyManager.SetProperty("#TV.Guide.SeriesNumber", _currentProgram.SeriesNum);
        if (_currentProgram.EpisodeNum == "-")
          GUIPropertyManager.SetProperty("#TV.Guide.EpisodeNumber", string.Empty);
        else
          GUIPropertyManager.SetProperty("#TV.Guide.EpisodeNumber", _currentProgram.EpisodeNum);
        if (_currentProgram.EpisodePart == "-")
          GUIPropertyManager.SetProperty("#TV.Guide.EpisodePart", string.Empty);
        else
          GUIPropertyManager.SetProperty("#TV.Guide.EpisodePart", _currentProgram.EpisodePart);
        if (_currentProgram.Date == "-")
          GUIPropertyManager.SetProperty("#TV.Guide.Date", string.Empty);
        else
          GUIPropertyManager.SetProperty("#TV.Guide.Date", _currentProgram.Date);
        if (_currentProgram.StarRating == "-")
          GUIPropertyManager.SetProperty("#TV.Guide.StarRating", string.Empty);
        else
          GUIPropertyManager.SetProperty("#TV.Guide.StarRating", _currentProgram.StarRating);
        if (_currentProgram.Classification == "-")
          GUIPropertyManager.SetProperty("#TV.Guide.Classification", string.Empty);
        else
          GUIPropertyManager.SetProperty("#TV.Guide.Classification", _currentProgram.Classification);
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeDetail", _currentProgram.EpisodeDetails);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        GUIPropertyManager.SetProperty("#TV.Guide.thumb", strLogo);
        _currentStartTime = _currentProgram.Start;
        _currentEndTime = _currentProgram.End;
        _currentTitle = _currentProgram.Title;
        _currentTime = strTime;
        _currentTvChannel = _currentProgram.Channel;

        bool bRecording = false;
        bool bSeries = false;
        bool bConflict = false;
        if (_recordingList != null)
        {
          foreach (TVRecording record in _recordingList)
          {
            if (record.IsRecordingProgram(_currentProgram, true))
            {
              if (ConflictManager.IsConflict(record))
                bConflict = true;
              if (record.RecType != TVRecording.RecordingType.Once)
                bSeries = true;
              bRecording = true;
              break;
            }
          }
        }
        if (bRecording)
        {
          GUIImage img = (GUIImage)GetControl((int)Controls.IMG_REC_PIN);

          if (bConflict)
            img.SetFileName(Thumbs.TvConflictRecordingIcon);
          else if (bSeries)
            img.SetFileName(Thumbs.TvRecordingSeriesIcon);
          else
            img.SetFileName(Thumbs.TvRecordingIcon);
          GUIControl.ShowControl(GetID, (int)Controls.IMG_REC_PIN);
        }
        else
          GUIControl.HideControl(GetID, (int)Controls.IMG_REC_PIN);
      }
    }//void SetProperties()

    void RenderSingleChannel(TVChannel channel)
    {
      int chan = _channelOffset;
      string strLogo = string.Empty;

      for (int iChannel = 0; iChannel < _channelCount; iChannel++)
      {
        if (chan < _channelList.Count)
        {
          TVChannel tvChan = (TVChannel)_channelList[chan];

          strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, tvChan.Name);
          if (System.IO.File.Exists(strLogo))
          {
            GUIButton3PartControl img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
            if (img != null)
            {
              if (_showChannelLogos)
                img.TexutureIcon = strLogo;
              img.Label1 = tvChan.Name;
              img.IsVisible = true;
            }
          }
          else
          {
            GUIButton3PartControl img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
            if (img != null)
            {
              if (_showChannelLogos)
                img.TexutureIcon = "defaultVideoBig.png";
              img.Label1 = tvChan.Name;
              img.IsVisible = true;
            }
          }
        }
        chan++;
        if (chan >= _channelList.Count)
          chan = 0;
      }

      List<TVProgram> programs = new List<TVProgram>();
      DateTime dtStart = DateTime.Now;
      DateTime dtEnd = dtStart.AddDays(30);
      long iStart = Util.Utils.datetolong(dtStart);
      long iEnd = Util.Utils.datetolong(dtEnd);
      TVDatabase.GetProgramsPerChannel(channel.Name, iStart, iEnd, ref programs);
      strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, channel.Name);

      _totalProgramCount = programs.Count;
      if (_totalProgramCount == 0)
        _totalProgramCount = _channelCount;

      GUILabelControl channelLabel = GetControl((int)Controls.SINGLE_CHANNEL_LABEL) as GUILabelControl;
      GUIImage channelImage = GetControl((int)Controls.SINGLE_CHANNEL_IMAGE) as GUIImage;

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

      if (channelLabel == null)
      {
        channelLabel = new GUILabelControl(GetID, (int)Controls.SINGLE_CHANNEL_LABEL,
          channelImage.XPosition + 44,
          channelImage.YPosition + 10,
          300, 40, "font16", channel.Name, 4294967295, GUIControl.Alignment.Left, true);
        channelLabel.AllocResources();
        GUIControl temp = (GUIControl)channelLabel;
        Add(ref temp);
      }

      setSingleChannelLabelVisibility(true);

      channelLabel.Label = channel.Name;
      if (strLogo.Length > 0)
        channelImage.SetFileName(strLogo);

      if (channelLabel != null)
      {
        channelLabel.Label = channel.Name;
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

        TVProgram program;
        int offset = _programOffset;
        if (offset + ichan < programs.Count)
          program = (TVProgram)programs[offset + ichan];
        else
        {
          program = new TVProgram();
          if (ichan == 0)
          {
            program.Start = Util.Utils.datetolong(DateTime.Now);
            program.End = Util.Utils.datetolong(DateTime.Now);
            program.Title = "-";
            program.Genre = "-";
          }
          program.Channel = channel.Name;
        }

        int ypos = GetControl(ichan + (int)Controls.IMG_CHAN1).YPosition;
        int iControlId = GUIDE_COMPONENTID_START + ichan * RowID + 0 * ColID;
        GUIButton3PartControl img = (GUIButton3PartControl)GetControl(iControlId);

        if (img == null)
        {
          img = new GUIButton3PartControl(GetID, iControlId, iStartXPos, ypos, iTotalWidth, height - 2,
            "tvguide_button_selected_left.png",
            "tvguide_button_selected_middle.png",
            "tvguide_button_selected_right.png",
            "tvguide_button_light_left.png",
            "tvguide_button_light_middle.png",
            "tvguide_button_light_right.png",
            string.Empty);
          img.AllocResources();
          img.ColourDiffuse = GetColorForGenre(program.Genre);
          GUIControl cntl = (GUIControl)img;
          Add(ref cntl);
        }
        else
        {
          img.TexutureFocusLeftName = "tvguide_button_selected_left.png";
          img.TexutureFocusMidName = "tvguide_button_selected_middle.png";
          img.TexutureFocusRightName = "tvguide_button_selected_right.png";
          img.TexutureNoFocusLeftName = "tvguide_button_light_left.png";
          img.TexutureNoFocusMidName = "tvguide_button_light_middle.png";
          img.TexutureNoFocusRightName = "tvguide_button_light_right.png";
          img.Focus = false;
          img.SetPosition(iStartXPos, ypos);
          img.Width = iTotalWidth;
          img.ColourDiffuse = GetColorForGenre(program.Genre);
          img.IsVisible = true;
          img.DoUpdate();
        }
        img.RenderLeft = false;
        img.RenderRight = false;
        bool bRecording = false;
        bool bSeries = false;
        bool bConflict = false;
        foreach (TVRecording record in _recordingList)
        {
          if (record.IsRecordingProgram(program, true))
          {
            if (ConflictManager.IsConflict(record))
              bConflict = true;
            if (record.RecType != TVRecording.RecordingType.Once)
              bSeries = true;
            bRecording = true;
            break;
          }
        }

        img.Data = program.Clone();
        img.ColourDiffuse = GetColorForGenre(program.Genre);
        height = height - 10;
        height /= 2;
        int iWidth = iTotalWidth;
        if (iWidth > 10)
          iWidth -= 10;
        else
          iWidth = 1;

        DateTime dt = DateTime.Now;

        img.TextOffsetX1 = 5;
        img.TextOffsetY1 = 5;
        img.FontName1 = "font13";
        img.TextColor1 = 0xffffffff;

        string strTimeSingle = string.Empty;
        string strTime = string.Empty;

        img.Label1 = program.Title;
        if (program.Start != 0)
        {
          strTimeSingle = String.Format("{0}",
            program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

          strTime = String.Format("{0}-{1}",
            program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
            program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

          if (program.StartTime.Date != DateTime.Now.Date)
          {
            img.Label1 = String.Format("{0} {1}", Util.Utils.GetShortDayString(program.StartTime), program.Title);
          }
        }
        GUILabelControl labelTemplate;
        if (program.IsRunningAt(dt))
        {
          labelTemplate = GetControl((int)Controls.LABEL_TITLE_DARK_TEMPLATE) as GUILabelControl;
        }
        else
          labelTemplate = GetControl((int)Controls.LABEL_TITLE_TEMPLATE) as GUILabelControl;

        if (labelTemplate != null)
        {
          img.FontName1 = labelTemplate.FontName;
          img.TextColor1 = labelTemplate.TextColor;
        }
        img.TextOffsetX2 = 5;
        img.TextOffsetY2 = img.Height / 2;
        img.FontName2 = "font13";
        img.TextColor2 = 0xffffffff;
        img.Label2 = "";
        if (program.IsRunningAt(dt))
        {
          img.TextColor2 = 0xff101010;
          labelTemplate = GetControl((int)Controls.LABEL_GENRE_DARK_TEMPLATE) as GUILabelControl;
        }
        else
          labelTemplate = GetControl((int)Controls.LABEL_GENRE_TEMPLATE) as GUILabelControl;

        if (labelTemplate != null)
        {
          img.FontName2 = labelTemplate.FontName;
          img.TextColor2 = labelTemplate.TextColor;
          img.Label2 = program.Genre;
        }
        imgCh.Label1 = strTimeSingle;
        imgCh.TexutureIcon = "";

        if (program.IsRunningAt(dt))
        {
          img.TexutureNoFocusLeftName = "tvguide_button_left.png";
          img.TexutureNoFocusMidName = "tvguide_button_middle.png";
          img.TexutureNoFocusRightName = "tvguide_button_right.png";
        }

        img.SetPosition(img.XPosition, img.YPosition);

        img.TexutureIcon = string.Empty;
        if (ShouldNotifyProgram(program))
          img.TexutureIcon = Thumbs.TvNotifyIcon;
        if (bRecording)
        {
          if (bConflict)
            img.TexutureIcon = Thumbs.TvConflictRecordingIcon;
          else if (bSeries)
            img.TexutureIcon = Thumbs.TvRecordingSeriesIcon;
          else
            img.TexutureIcon = Thumbs.TvRecordingIcon;
        }
      }
    }//void RenderSingleChannel(TVChannel channel)


    void setGuideHeadngVisibility(bool visible)
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

    void setSingleChannelLabelVisibility(bool visible)
    {
      GUILabelControl channelLabel = GetControl((int)Controls.SINGLE_CHANNEL_LABEL) as GUILabelControl;
      GUIImage channelImage = GetControl((int)Controls.SINGLE_CHANNEL_IMAGE) as GUIImage;

      if (channelLabel != null)
        channelLabel.Visible = visible;

      if (channelImage != null)
        channelImage.Visible = visible;

    }

    void RenderChannel(int iChannel, TVChannel channel, long iStart, long iEnd, bool selectCurrentShow)
    {
      string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, channel.Name);
      if (System.IO.File.Exists(strLogo))
      {
        GUIButton3PartControl img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
        if (img != null)
        {
          if (_showChannelLogos)
            img.TexutureIcon = strLogo;
          img.Label1 = channel.Name;
          img.IsVisible = true;
        }
      }
      else
      {
        GUIButton3PartControl img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
        if (img != null)
        {
          if (_showChannelLogos)
            img.TexutureIcon = "defaultVideoBig.png";
          img.Label1 = channel.Name;
          img.IsVisible = true;
        }
      }


      List<TVProgram> programs = new List<TVProgram>();
      TVDatabase.GetProgramsPerChannel(channel.Name, iStart, iEnd, ref programs);
      if (programs.Count == 0)
      {
        DateTime dt = Util.Utils.longtodate(iEnd);
        //dt=dt.AddMinutes(_timePerBlock);
        long iProgEnd = Util.Utils.datetolong(dt);
        TVProgram prog = new TVProgram();
        prog.Start = iStart;
        prog.End = iProgEnd;
        prog.Channel = channel.Name;
        prog.Title = GUILocalizeStrings.Get(736);//no tvguide data
        programs.Add(prog);
      }
      if (programs.Count > 0)
      {
        int iProgram = 0;
        int iPreviousEndXPos = 0;
        foreach (TVProgram program in programs)
        {
          string strTitle = program.Title;
          bool bStartsBefore = false;
          bool bEndsAfter = false;
          if (program.End <= iStart)
            continue;
          if (program.Start < iStart)
            bStartsBefore = true;
          if (program.End > iEnd)
            bEndsAfter = true;

          if (iProgram == _cursorY - 1 && iChannel == _cursorX)
          {

            _currentProgram = program;
            SetProperties();
          }
          int width = GetControl((int)Controls.LABEL_TIME1 + 1).XPosition;
          width -= GetControl((int)Controls.LABEL_TIME1).XPosition;

          int height = GetControl((int)Controls.IMG_CHAN1 + 1).YPosition;
          height -= GetControl((int)Controls.IMG_CHAN1).YPosition;

          DateTime dtBlokStart = new DateTime();
          dtBlokStart = _viewingTime;
          dtBlokStart = dtBlokStart.AddMilliseconds(-dtBlokStart.Millisecond);
          dtBlokStart = dtBlokStart.AddSeconds(-dtBlokStart.Second);

          bool bSeries = false;
          bool bRecording = false;
          bool bConflict = false;
          foreach (TVRecording record in _recordingList)
          {
            if (record.IsRecordingProgram(program, true))
            {
              if (ConflictManager.IsConflict(record))
                bConflict = true;
              if (record.RecType != TVRecording.RecordingType.Once)
                bSeries = true;
              bRecording = true;
              break;
            }
          }

          int iStartXPos = 0;
          int iEndXPos = 0;
          for (int iBlok = 0; iBlok < _numberOfBlocks; iBlok++)
          {
            float fWidthEnd = (float)width;
            DateTime dtBlokEnd = dtBlokStart.AddMinutes(_timePerBlock - 1); //
            if (program.RunningAt(dtBlokStart, dtBlokEnd))
            {
              //dtBlokEnd = dtBlokStart.AddSeconds(_timePerBlock * 60);
              if (program.EndTime <= dtBlokEnd)
              {
                TimeSpan dtSpan = dtBlokEnd - program.EndTime;
                int iEndMin = _timePerBlock - (dtSpan.Minutes);

                fWidthEnd = (((float)iEndMin) / ((float)_timePerBlock)) * ((float)(width));
                if (bEndsAfter)
                  fWidthEnd = (float)width;
              }

              if (iStartXPos == 0)
              {
                TimeSpan ts = program.StartTime - dtBlokStart;
                int iStartMin = ts.Hours * 60;
                iStartMin += ts.Minutes;
                if (ts.Seconds == 59)
                  iStartMin += 1;
                float fWidth = (((float)iStartMin) / ((float)_timePerBlock)) * ((float)(width));

                if (bStartsBefore)
                  fWidth = 0;

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
              iStartXPos = iPreviousEndXPos;
            if (iEndXPos <= iStartXPos + 5)
              iEndXPos = iStartXPos + 6; // at least 1 pixel width

            int ypos = GetControl(iChannel + (int)Controls.IMG_CHAN1).YPosition;
            int iControlId = GUIDE_COMPONENTID_START + iChannel * RowID + iProgram * ColID;
            GUIButton3PartControl img = (GUIButton3PartControl)GetControl(iControlId);
            int iWidth = iEndXPos - iStartXPos;
            if (iWidth > 3)
              iWidth -= 3;
            else
              iWidth = 1;
            if (img == null)
            {
              img = new GUIButton3PartControl(GetID, iControlId, iStartXPos, ypos, iWidth, height - 2,
                "tvguide_button_selected_left.png",
                "tvguide_button_selected_middle.png",
                "tvguide_button_selected_right.png",
                "tvguide_button_light_left.png",
                "tvguide_button_light_middle.png",
                "tvguide_button_light_right.png",
                string.Empty);
              img.AllocResources();
              GUIControl cntl = (GUIControl)img;
              Add(ref cntl);
            }
            else
            {
              img.TexutureFocusLeftName = "tvguide_button_selected_left.png";
              img.TexutureFocusMidName = "tvguide_button_selected_middle.png";
              img.TexutureFocusRightName = "tvguide_button_selected_right.png";
              img.TexutureNoFocusLeftName = "tvguide_button_light_left.png";
              img.TexutureNoFocusMidName = "tvguide_button_light_middle.png";
              img.TexutureNoFocusRightName = "tvguide_button_light_right.png";
              img.Focus = false;
              img.SetPosition(iStartXPos, ypos);
              img.Width = iWidth;
              img.IsVisible = true;
              img.DoUpdate();
            }
            img.RenderLeft = false;
            img.RenderRight = false;

            img.TexutureIcon = string.Empty;
            if (ShouldNotifyProgram(program))
              img.TexutureIcon = Thumbs.TvNotifyIcon;
            if (bRecording)
            {
              if (bConflict)
                img.TexutureIcon = Thumbs.TvConflictRecordingIcon;
              else if (bSeries)
                img.TexutureIcon = Thumbs.TvRecordingSeriesIcon;
              else
                img.TexutureIcon = Thumbs.TvRecordingIcon;
            }

            img.Data = program.Clone();
            img.ColourDiffuse = GetColorForGenre(program.Genre);
            height = height - 10;
            height /= 2;
            iWidth = iEndXPos - iStartXPos;
            if (iWidth > 10)
              iWidth -= 10;
            else
              iWidth = 1;

            DateTime dt = DateTime.Now;

            img.TextOffsetX1 = 5;
            img.TextOffsetY1 = 5;
            img.FontName1 = "font13";
            img.TextColor1 = 0xffffffff;
            img.Label1 = strTitle;
            GUILabelControl labelTemplate;
            if (program.IsRunningAt(dt))
            {
              labelTemplate = GetControl((int)Controls.LABEL_TITLE_DARK_TEMPLATE) as GUILabelControl;
            }
            else
              labelTemplate = GetControl((int)Controls.LABEL_TITLE_TEMPLATE) as GUILabelControl;

            if (labelTemplate != null)
            {
              img.FontName1 = labelTemplate.FontName;
              img.TextColor1 = labelTemplate.TextColor;
              img.TextColor2 = labelTemplate.TextColor;
            }
            img.TextOffsetX2 = 5;
            img.TextOffsetY2 = img.Height / 2;
            img.FontName2 = "font13";
            img.TextColor2 = 0xffffffff;

            if (program.IsRunningAt(dt))
              labelTemplate = GetControl((int)Controls.LABEL_GENRE_DARK_TEMPLATE) as GUILabelControl;
            else
              labelTemplate = GetControl((int)Controls.LABEL_GENRE_TEMPLATE) as GUILabelControl;
            if (labelTemplate != null)
            {
              img.FontName2 = labelTemplate.FontName;
              img.TextColor2 = labelTemplate.TextColor;
              img.Label2 = program.Genre;
            }

            if (program.IsRunningAt(dt))
            {
              img.TexutureNoFocusLeftName = "tvguide_button_left.png";
              img.TexutureNoFocusMidName = "tvguide_button_middle.png";
              img.TexutureNoFocusRightName = "tvguide_button_right.png";
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

              img.TexutureFocusRightName = "tvguide_arrow_selected_right.png";
              img.TexutureNoFocusRightName = "tvguide_arrow_light_right.png";
              if (program.IsRunningAt(dt))
              {
                img.TexutureNoFocusRightName = "tvguide_arrow_right.png";
              }
            }
            if (bStartsBefore)
            {
              img.RenderLeft = true;
              img.TexutureFocusLeftName = "tvguide_arrow_selected_left.png";
              img.TexutureNoFocusLeftName = "tvguide_arrow_light_left.png";
              if (program.IsRunningAt(dt))
              {
                img.TexutureNoFocusLeftName = "tvguide_arrow_left.png";
              }
            }

            img.SetPosition(img.XPosition, img.YPosition);
            img.DoUpdate();
            iProgram++;
          }
          iPreviousEndXPos = iEndXPos;
        }
      }
    }//void RenderChannel(int iChannel,TVChannel channel, long iStart, long iEnd, bool selectCurrentShow)

    int ProgramCount(int iChannel)
    {
      int iProgramCount = 0;
      for (int iProgram = 0; iProgram < _numberOfBlocks * 5; ++iProgram)
      {
        int iControlId = GUIDE_COMPONENTID_START + iChannel * RowID + iProgram * ColID;
        GUIControl cntl = GetControl(iControlId);
        if (cntl != null && cntl.IsVisible)
          iProgramCount++;
        else
          return iProgramCount;
      }
      return iProgramCount;
    }


    void OnDown(bool updateScreen)
    {
      if (updateScreen)
        UnFocus();
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
          if (updateScreen)
            Update(false);
        }
        else
        {

          if (_cursorX + _programOffset + 1 < _totalProgramCount)
          {
            _programOffset++;
            if (updateScreen)
              Update(false);
          }
        }
        if (updateScreen)
        {
          SetFocus();

          UpdateCurrentProgram();
          SetProperties();
        }
        return;
      }

      if (_cursorY == 0)
      {
        if (_cursorX + 1 < _channelCount)
        {
          _cursorX++;
          if (updateScreen)
            Update(false);
        }
        else
        {
          _channelOffset++;
          if (_channelOffset > 0 && _channelOffset >= _channelList.Count)
            _channelOffset -= _channelList.Count;
          if (updateScreen)
            Update(false);
        }
        if (updateScreen)
        {
          SetFocus();
          SetProperties();
        }
        return;
      }
      int iCurY = _cursorX;
      int iCurOff = _channelOffset;
      int iX1, iX2;
      //      int iNewWidth=0;
      int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
      GUIControl control = GetControl(iControlId);
      if (control == null)
        return;
      iX1 = control.XPosition;
      iX2 = control.XPosition + control.Width;

      bool bOK = false;
      int iMaxSearch = _channelList.Count;
      while (!bOK && (iMaxSearch > 0))
      {
        iMaxSearch--;
        if (_cursorX + 1 < _channelCount)
        {
          _cursorX++;
        }
        else
        {
          _channelOffset++;
          if (_channelOffset > 0 && _channelOffset >= _channelList.Count)
            _channelOffset -= _channelList.Count;
          if (updateScreen)
            Update(false);
        }

        for (int x = 1; x < ColID; x++)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (x - 1) * ColID;
          control = GetControl(iControlId);
          if (control != null)
          {
            TVProgram prog = (TVProgram)control.Data;
            if (x == 1 && m_dtStartTime < prog.StartTime || _singleChannelView)
            {
              _cursorY = x;
              bOK = true;
              break;
            }

            if (m_dtStartTime >= prog.StartTime && m_dtStartTime < prog.EndTime)
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
        _channelOffset = iCurOff;
      }
      if (updateScreen)
      {
        if (iCurOff == _channelOffset)
        {
          Correct();
          UpdateCurrentProgram();
          return;
        }

        Correct();
        Update(false);
        SetFocus();
      }
    }

    void OnUp(bool updateScreen)
    {
      if (updateScreen)
        UnFocus();
      if (!_singleChannelView && _cursorY == 0 && _cursorX == 0 && _channelOffset == 0)
      {
        _cursorX = -1;
        GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL).Focus = true;
        return;
      }
      if (_singleChannelView)
      {
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

      if (_cursorY == 0)
      {
        if (_cursorX == 0)
        {
          if (_channelOffset > 0)
          {
            _channelOffset--;
            if (updateScreen)
              Update(false);
          }
        }
        else
        {
          _cursorX--;
          if (updateScreen)
            Update(false);
        }
        if (updateScreen)
        {
          SetFocus();
          SetProperties();
        }
        return;
      }
      int iCurY = _cursorX;
      int iCurOff = _channelOffset;

      int iX1, iX2;
      int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
      GUIControl control = GetControl(iControlId);
      if (control == null)
        return;
      iX1 = control.XPosition;
      iX2 = control.XPosition + control.Width;

      bool bOK = false;
      int iMaxSearch = _channelList.Count;
      while (!bOK && (iMaxSearch > 0))
      {
        iMaxSearch--;
        if (_cursorX == 0)
        {
          if (_channelOffset > 0)
          {
            _channelOffset--;
            if (updateScreen)
              Update(false);
          }
          else
            break;
        }
        else
        {
          _cursorX--;
        }

        for (int x = 1; x < ColID; x++)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (x - 1) * ColID;
          control = GetControl(iControlId);
          if (control != null)
          {
            TVProgram prog = (TVProgram)control.Data;
            if (x == 1 && m_dtStartTime < prog.StartTime || _singleChannelView)
            {
              _cursorY = x;
              bOK = true;
              break;
            }
            if (m_dtStartTime >= prog.StartTime && m_dtStartTime < prog.EndTime)
            {
              _cursorY = x;
              bOK = true;
              break;
            }
          }
          else
          {
            break;
          }
        }

      }
      if (!bOK)
      {
        _cursorX = iCurY;
        _channelOffset = iCurOff;
      }

      if (updateScreen)
      {
        if (iCurOff == _channelOffset)
        {
          Correct();
          UpdateCurrentProgram();

          return;
        }

        Correct();
        Update(false);
        SetFocus();
      }
    }

    void OnLeft()
    {
      if (_cursorX < 0)
        return;
      UnFocus();
      if (_cursorY == 0)
      {
        _viewingTime = _viewingTime.AddMinutes(-_timePerBlock);
        // Check new day
        int iDay = CalcDays();
        if (iDay < 0)
          _viewingTime = _viewingTime.AddMinutes(+_timePerBlock);
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
          m_dtStartTime = _currentProgram.StartTime;
        return;
      }
      Correct();
      Update(false);
      SetFocus();
      if (_currentProgram != null)
        m_dtStartTime = _currentProgram.StartTime;
    }

    void UpdateCurrentProgram()
    {
      if (_cursorX < 0)
        return;
      if (_cursorY < 0)
        return;
      if (_cursorY == 0)
      {
        SetProperties();
        SetFocus();
        return;
      }
      int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
      GUIButton3PartControl img = (GUIButton3PartControl)GetControl(iControlId);
      if (null != img)
      {
        SetFocus();
        _currentProgram = (TVProgram)img.Data;
        SetProperties();

      }
    }

    void OnRight()
    {
      if (_cursorX < 0)
        return;
      UnFocus();
      if (_cursorY < ProgramCount(_cursorX))
      {
        _cursorY++;
        Correct();
        UpdateCurrentProgram();
        if (_currentProgram != null)
          m_dtStartTime = _currentProgram.StartTime;
        return;
      }
      else
      {
        _viewingTime = _viewingTime.AddMinutes(_timePerBlock);
        // Check new day
        int iDay = CalcDays();
        if (iDay >= MaxDaysInGuide)
          _viewingTime = _viewingTime.AddMinutes(-_timePerBlock);
      }
      Correct();
      Update(false);
      SetFocus();
      if (_currentProgram != null)
        m_dtStartTime = _currentProgram.StartTime;
    }

    void updateSingleChannelNumber()
    {
      // update selected channel 
      if (!_singleChannelView)
      {
        _singleChannelNumber = _cursorX + _channelOffset;
        if (_singleChannelNumber < 0)
          _singleChannelNumber = 0;
        if (_singleChannelNumber >= _channelList.Count)
          _singleChannelNumber -= _channelList.Count;
        GUIButton3PartControl img = (GUIButton3PartControl)GetControl(_cursorX + (int)Controls.IMG_CHAN1);
        if (null != img)
          _currentTvChannel = img.Label1;
      }
    }

    void UnFocus()
    {
      if (_cursorX < 0)
        return;
      if (_cursorY == 0)
      {
        int controlid = (int)Controls.IMG_CHAN1 + _cursorX;
        GUIControl.UnfocusControl(GetID, controlid);
      }
      else
      {
        Correct();
        int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
        GUIButton3PartControl img = GetControl(iControlId) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          if (_currentProgram != null)
            img.ColourDiffuse = GetColorForGenre(_currentProgram.Genre);
        }
        GUIControl.UnfocusControl(GetID, iControlId);
      }
    }
    void SetFocus()
    {
      if (_cursorX < 0)
        return;
      if (_cursorY == 0)
      {
        GUIControl.UnfocusControl(GetID, (int)Controls.SPINCONTROL_DAY);
        GUIControl.UnfocusControl(GetID, (int)Controls.SPINCONTROL_TIME_INTERVAL);

        int controlid = (int)Controls.IMG_CHAN1 + _cursorX;
        GUIControl.FocusControl(GetID, controlid);
      }
      else
      {
        Correct();
        int iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
        GUIButton3PartControl img = GetControl(iControlId) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          img.ColourDiffuse = 0xffffffff;
          _currentProgram = img.Data as TVProgram;
          SetProperties();
        }
        GUIControl.FocusControl(GetID, iControlId);
      }
    }

    void Correct()
    {
      int iControlId;
      if (_cursorY < 0)
        _cursorY = 0;
      if (_cursorY > 0)
      {
        while (_cursorY > 0)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (_cursorY - 1) * ColID;
          GUIControl cntl = GetControl(iControlId);
          if (cntl == null)
            _cursorY--;
          else if (!cntl.IsVisible)
            _cursorY--;
          else
            break;
        }
      }
      if (_cursorX < 0)
        _cursorX = 0;
      if (!_singleChannelView)
      {
        while (_cursorX > 0)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * RowID + (0) * ColID;
          GUIControl cntl = GetControl(iControlId);
          if (cntl == null)
            _cursorX--;
          else if (!cntl.IsVisible)
            _cursorX--;
          else
            break;
        }
      }
    }

    void Import()
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.Reset();
        dlgProgress.SetHeading(606);
        dlgProgress.SetLine(1, string.Empty);
        dlgProgress.SetLine(2, string.Empty);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }

      XMLTVImport import = new XMLTVImport();
      import.ShowProgress += new MediaPortal.TV.Database.XMLTVImport.ShowProgressHandler(import_ShowProgress);
      bool bSucceeded = import.Import(_tvGuideFileName, true);
      if (dlgProgress != null)
        dlgProgress.Close();

      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (pDlgOK != null)
      {
        int iChannels = import.ImportStats.Channels;
        int iPrograms = import.ImportStats.Programs;
        string strChannels = GUILocalizeStrings.Get(627) + iChannels.ToString();
        string strPrograms = GUILocalizeStrings.Get(628) + iPrograms.ToString();
        pDlgOK.SetHeading(606);
        pDlgOK.SetLine(1, strChannels + " " + strPrograms);
        pDlgOK.SetLine(2, import.ImportStats.StartTime.ToShortDateString() + " - " + import.ImportStats.EndTime.ToShortDateString());
        if (!bSucceeded)
        {
          pDlgOK.SetLine(1, 608);
          pDlgOK.SetLine(2, import.ErrorMessage);
        }
        else
        {
          pDlgOK.SetHeading(606);
          pDlgOK.SetLine(1, 609);
        }
        pDlgOK.DoModal(GetID);
      }
      _channelOffset = 0;
      _singleChannelView = false;
      _channelOffset = 0;
      _cursorY = 0;
      _cursorX = 0;
      Update(false);
      SetFocus();
    }

    // Define the event handlers.
    private static void OnChanged(object source, FileSystemEventArgs e)
    {
      if (String.Compare(e.Name, "tvguide.xml", true) == 0)
        StartImportXML();
    }
    private static void OnRenamed(object source, RenamedEventArgs e)
    {
      if (String.Compare(e.Name, "tvguide.xml", true) == 0)
        StartImportXML();
    }
    static protected void StartImportXML()
    {
      Thread.Sleep(500); // give time to the external prog to close file handle
      try
      {
        //check if file can be opened for reading....
        Encoding fileEncoding = Encoding.Default;
        FileStream streamIn = File.Open(_tvGuideFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        StreamReader fileIn = new StreamReader(streamIn, fileEncoding, true);
        fileIn.Close();
        streamIn.Close();
      }
      catch (Exception)
      {
        Log.Info("StartImportXML - Exception " + _tvGuideFileName);
        return;
      }
      _tvGuideFileWatcher.EnableRaisingEvents = false;
      
      if (!_workerThreadRunning)
      {
        _workerThreadRunning = true;
        Thread workerThread = new Thread(new ThreadStart(ThreadFunctionImportTVGuide));
        workerThread.Name = "TvGuideImporter";
        workerThread.Priority = ThreadPriority.Lowest;
        workerThread.Start();
      }
    }

    static void ThreadFunctionImportTVGuide()
    {
      Log.Info(@"detected new tvguide ->import new tvguide");
      Thread.Sleep(500);
      try
      {
        XMLTVImport import = new XMLTVImport(10);  // add 10 msec dely to the background thread
        import.Import(_tvGuideFileName, false);
      }
      catch (Exception)
      {
      }

      try
      {
        //
        // Make sure the file exists before we try to do any processing, thus if the file doesn't
        // exist we we'll save ourselves from getting a file not found exception.
        //
        if (File.Exists(_tvGuideFileName))
        {
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            string strFileTime = System.IO.File.GetLastWriteTime(_tvGuideFileName).ToString();
            xmlreader.SetValue("tvguide", "date", strFileTime);
          }
        }

      }
      catch (Exception)
      {
      }
      _tvGuideFileWatcher.EnableRaisingEvents = true;
      _workerThreadRunning = false;
      Log.Info(@"import done");
    }

    void ShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(498));//Menu

        if (_currentTvChannel.Length > 0)
          dlg.AddLocalizedString(938);// View this channel

        dlg.AddLocalizedString(939);// Switch mode
        if (_currentProgram != null && _currentTvChannel.Length > 0 && _currentTitle.Length > 0)
        {
          dlg.AddLocalizedString(264);// Record
        }
        if (!_disableXMLTVImportOption)
          dlg.AddLocalizedString(937);// Reload tvguide
        dlg.AddLocalizedString(971);// Group
        dlg.AddLocalizedString(724);// TVGuide search
        dlg.AddLocalizedString(603);// Scheduled TV
        dlg.AddLocalizedString(652);// Recorded TV


        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
          return;
        switch (dlg.SelectedId)
        {
          case 652: //Recorded TV
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_RECORDEDTV);
            break;

          case 603: //Scheduled TV
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SCHEDULER);
            break;

          case 724: //TVGuide search
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SEARCHTV);
            break;

          case 971: //group
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(971));//Group
            foreach (TVGroup group in GUITVHome.Navigator.Groups)
            {
              dlg.Add(group.GroupName);
            }
            dlg.DoModal(GetID);
            if (dlg.SelectedLabel == -1)
              return;
            GUITVHome.Navigator.SetCurrentGroup(dlg.SelectedLabelText);
            GetChannels();
            Update(false);
            SetFocus();
            break;

          case 937: //import tvguide
            Import();
            Update(false);
            SetFocus();
            break;

          case 938: // view channel
            GUITVHome.IsTVOn = true;
            GUITVHome.ViewChannelAndCheck(_currentTvChannel);
            if (Recorder.IsViewing() && Recorder.TVChannelName == _currentProgram.Channel)
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
            }
            return;


          case 939: // switch mode
            OnSwitchMode();
            break;

          case 264: // record
            OnRecordContext();
            break;
        }
      }
    }
    void OnSwitchMode()
    {
      UnFocus();
      _singleChannelView = !_singleChannelView;
      if (_singleChannelView)
      {
        _backupCursorX = _cursorY;
        _backupCursorY = _cursorX;
        _backupChannelOffset = _channelOffset;

        _programOffset = _cursorY = _cursorX = 0;
      }
      else
      {
        //focus current channel
        _cursorY = 0;
        _cursorX = _backupCursorY;
        _channelOffset = _backupChannelOffset;
      }
      Update(true);
      SetFocus();
    }
    void ShowProgramInfo()
    {
      if (_currentProgram == null)
        return;
      GUITVProgramInfo.CurrentProgram = _currentProgram;
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO);
    }

    void OnSelectItem()
    {
      if (_currentProgram == null)
        return;
      // Selected show is not 'On'
      if (!(_currentProgram.IsRunningAt(DateTime.Now) || _currentProgram.EndTime <= DateTime.Now))
      {
        ShowProgramInfo();
        return;
      }
      // Stop running player
      if (g_Player.Playing && g_Player.IsTVRecording)
      {
        g_Player.Stop();
      }
      //view this channel
      try
      {
        TVRecording recFound = null;

        Log.Info("TVGuide: IsAnyCardRecording: {0}", Convert.ToString(Recorder.IsAnyCardRecording()));

        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
          return;

        if (Recorder.IsAnyCardRecording())
        {
          if (_recordingList == null)
             Log.Debug("EPG: _recordingList was not available");

          // If you select the program which is currently recording open a dialog to ask if you want to see it from the beginning
          // imagine a sports event where you do not want to see the live point to be spoiled

          // here a check is needed of _currentTitle == Recorder.CurrentTVRecording.Title
          foreach (TVRecording rec in _recordingList)
          {
            // Take into consideration old and comming recordings! Just checking titel is not enough
            // It could be that there is a old recording with the same title
            if ((rec.Title == _currentProgram.Title) && (rec.StartTime <= DateTime.Now) && (rec.EndTime >= DateTime.Now))
            {
              recFound = rec;
              break;
            }
          }
          // We clicked on a show that is recording.
          if (recFound != null)
          {
            Log.Debug("TVGuide: clicked on a currently running recording {0}", _currentProgram.Title);
            dlg.Reset();
            dlg.SetHeading(_currentProgram.Title);
            dlg.AddLocalizedString(979); //Play recording from beginning
            dlg.AddLocalizedString(980); //Play recording from live point
            dlg.AddLocalizedString(1041); //Upcoming episodes
            dlg.DoModal(GetID);

            if (dlg.SelectedLabel == -1)
              return;

            Log.Debug("TVGuide: Found current program {0} in recording list", _currentTitle);
            switch (dlg.SelectedId)
            {
              case 979: // Play recording from beginning                          
                Recorder.StopViewing();
                string filename = Recorder.GetRecordingFileName(recFound);
                if (filename != string.Empty)
                {
                  Log.Info("TVGuide: Play recording {0} from start", _currentTitle);
                  g_Player.Play(filename);
                  if (g_Player.Playing)
                  {
                    g_Player.ShowFullScreenWindow();
                  }
                }
                break;
              case 980: // Play recording from live point
                GUITVHome.IsTVOn = true;
                GUITVHome.ViewChannel(recFound.Channel);
                if (Recorder.IsViewing())
                {
                  if (g_Player.Playing)
                  {
                    Log.Info("TVGuide: Show recording {0} at live point", _currentTitle);
                    g_Player.SeekAsolutePercentage(99);
                  }
                  GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
                }
                break;
              case 1041:
                ShowProgramInfo();
                Log.Debug("TVGuide: show episodes or repeatings for current show");
                break;
            }
            return;
          }
        } // if (Recorder.IsAnyCardRecording())

        // clicked the show we're currently watching
        if (Recorder.TVChannelName == _currentProgram.Channel)
        {
          Log.Debug("TVGuide: clicked on a currently running show");
          dlg.Reset();
          dlg.SetHeading(_currentProgram.Title);
          dlg.AddLocalizedString(938);  //View this channel
          dlg.AddLocalizedString(1041); //Upcoming episodes
          dlg.DoModal(GetID);

          if (dlg.SelectedLabel == -1)
            return;

          switch (dlg.SelectedId)
          {
            case 1041:
              ShowProgramInfo();
              Log.Debug("TVGuide: show episodes or repeatings for current show");
              break;
            case 938:
              Log.Debug("TVGuide: switch currently running show to fullscreen");
              if (Recorder.IsViewing())
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
              break;
          }
        }
        else
        // zap to selected show's channel
        {
          GUITVHome.IsTVOn = true;
          GUITVHome.ViewChannelAndCheck(_currentProgram.Channel);
          if (Recorder.IsViewing() && Recorder.TVChannelName == _currentProgram.Channel)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
        }
      }
      finally
      {
        if (VMR9Util.g_vmr9 != null)
          VMR9Util.g_vmr9.Enable(true);
      }
    }

    /// <summary>
    /// "Record" via REC button
    /// </summary>
    void OnRecord()
    {
      if (_currentProgram == null)
        return;
      if ((_currentProgram.IsRunningAt(DateTime.Now) ||
          (_currentProgram.EndTime <= DateTime.Now)) &&
          (Recorder.IsViewing() || Recorder.IsTimeShifting()))
      {
        //record current programme
        GUIWindow tvHome = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
        if ((tvHome != null) && (tvHome.GetID != GUIWindowManager.ActiveWindow))
          tvHome.OnAction(new Action(Action.ActionType.ACTION_RECORD, 0, 0));
      }
      else
        ShowProgramInfo();
    }

    /// <summary>
    /// "Record" entry in context menu
    /// </summary>
    void OnRecordContext()
    {
      if (_currentProgram == null)
        return;
      ShowProgramInfo();
    }

    void CheckRecordingConflicts()
    {
    }

    void OnPageUp()
    {
      UnFocus();
      for (int i = 0; i < _channelCount; ++i)
        OnUp(false);
      Correct();
      Update(false);
      SetFocus();
    }
    void OnPageDown()
    {
      UnFocus();
      for (int i = 0; i < _channelCount; ++i)
        OnDown(false);
      Correct();
      Update(false);
      SetFocus();

    }
    void OnNextDay()
    {
      _viewingTime = _viewingTime.AddDays(1.0);
      Update(false);
      SetFocus();
    }

    void OnPreviousDay()
    {
      _viewingTime = _viewingTime.AddDays(-1.0);
      Update(false);
      SetFocus();
    }

    long GetColorForGenre(string genre)
    {
      if (!_useColorsForGenres)
        return Color.White.ToArgb();
      List<string> genres = new List<string>();
      TVDatabase.GetGenres(ref genres);

      genre = genre.ToLower();
      for (int i = 0; i < genres.Count; ++i)
      {
        if (String.Compare(genre, (string)genres[i], true) == 0)
        {
          Color col = (Color)_colorList[i % _colorList.Count];
          return col.ToArgb();
        }
      }
      return Color.White.ToArgb();
    }


    void OnKeyTimeout()
    {
      if (_lineInput.Length == 0)
        return;
      TimeSpan ts = DateTime.Now - _keyPressedTimer;
      if (ts.TotalMilliseconds >= 1000)
      {
        // change channel
        int iChannel = Int32.Parse(_lineInput);
        ChangeChannelNr(iChannel);
        _lineInput = string.Empty;
      }
    }

    void OnKeyCode(char chKey)
    {
      if (chKey >= '0' && chKey <= '9') //Make sure it's only for the remote
      {
        TimeSpan ts = DateTime.Now - _keyPressedTimer;
        if (_lineInput.Length >= 2 || ts.TotalMilliseconds >= 800)
        {
          _lineInput = string.Empty;
        }
        _keyPressedTimer = DateTime.Now;
        if (chKey == '0' && _lineInput.Length == 0)
          return;
        _lineInput += chKey;
        if (_lineInput.Length == 2)
        {
          // change channel
          int iChannel = Int32.Parse(_lineInput);
          ChangeChannelNr(iChannel);

        }
      }
    }

    void ChangeChannelNr(int iChannelNr)
    {
      if (_byIndex) // Check if channel changing should be byIndex
      {
        // Channel change by index
        iChannelNr--;
      }
      else
      {
        // Change channel by using actual channel number
        int iCounter = 0;
        bool found = false;
        TVChannel chan;

        // Loop through complete channel list until match is found
        while (iCounter < _channelList.Count && found == false)
        {
          chan = (TVChannel)_channelList[iCounter];

          // Check if channel is an external channel
          if (chan.External == false)
          {
            //Not External Channel
            if (chan.Number == iChannelNr)
            {
              iChannelNr = iCounter;
              found = true;
            }
          }
          else
          {
            //External channel
            if (Int32.Parse(chan.ExternalTunerChannel) == iChannelNr)
            {
              iChannelNr = iCounter;
              found = true;
            }
          }
          iCounter++;  // Increment loop counter
        }
      }


      if (iChannelNr >= 0 && iChannelNr < _channelList.Count)
      {
        UnFocus();
        _channelOffset = 0;
        _cursorX = 0;

        // Last page adjust (To get a full page channel listing)
        if (iChannelNr > _channelList.Count - _channelCount + 1)
        {
          _channelOffset = _channelList.Count - _channelCount;
          iChannelNr = iChannelNr - _channelOffset;
        }

        while (iChannelNr >= _channelCount)
        {
          iChannelNr -= _channelCount;
          _channelOffset += _channelCount;
        }
        _cursorX = iChannelNr;

        Update(false);
        SetFocus();
      }
    }

    void GetChannels()
    {
      _channelList.Clear();

      try
      {
        foreach (TVChannel chan in GUITVHome.Navigator.CurrentGroup.TvChannels)
        {
          if (chan.VisibleInGuide)
          {
            _channelList.Add(chan);
          }
        }
      }
      catch
      {
      }

      if (_channelList.Count == 0)
      {
        TVChannel newChannel = new TVChannel();
        newChannel.Name = GUILocalizeStrings.Get(911);
        for (int i = 0; i < 10; ++i)
          _channelList.Add(newChannel);
      }
    }


    private void import_ShowProgress(MediaPortal.TV.Database.XMLTVImport.Stats stats)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        int iChannels = stats.Channels;
        int iPrograms = stats.Programs;
        string strChannels = GUILocalizeStrings.Get(627) + iChannels.ToString();
        string strPrograms = GUILocalizeStrings.Get(628) + iPrograms.ToString();
        dlgProgress.SetLine(1, stats.Status);
        dlgProgress.SetLine(2, strChannels + " " + strPrograms);
        dlgProgress.Progress();
      }
    }

    private void UpdateVerticalScrollbar()
    {
      if (_channelList == null || _channelList.Count == 0)
        return;
      int channel = _cursorX + _channelOffset;
      while (channel > 0 && channel >= _channelList.Count)
        channel -= _channelList.Count;
      float current = (float)(_cursorX + _channelOffset);
      float total = (float)_channelList.Count;

      if (_singleChannelView)
      {
        current = (float)(_cursorX + _channelOffset);
        total = (float)_totalProgramCount;
      }
      if (total == 0)
        total = _channelCount;

      float percentage = (current / total) * 100.0f;
      if (percentage < 0)
        percentage = 0;
      if (percentage > 100)
        percentage = 100;
      GUIVerticalScrollbar scrollbar = GetControl((int)Controls.VERT_SCROLLBAR) as GUIVerticalScrollbar;
      if (scrollbar != null)
      {
        scrollbar.Percentage = percentage;
      }
    }

    private void UpdateHorizontalScrollbar()
    {
      if (_channelList == null) return;
      GUIHorizontalScrollbar scrollbar = GetControl((int)Controls.HORZ_SCROLLBAR) as GUIHorizontalScrollbar;
      if (scrollbar != null)
      {
        float percentage = (float)_viewingTime.Hour * 60 + _viewingTime.Minute + (float)_timePerBlock * ((float)_viewingTime.Hour / 24.0f);
        percentage /= (24.0f * 60.0f);
        percentage *= 100.0f;
        if (percentage < 0)
          percentage = 0;
        if (percentage > 100)
          percentage = 100;
        if (_singleChannelView)
          percentage = 0;

        if ((int)percentage != (int)scrollbar.Percentage)
        {
          scrollbar.Percentage = percentage;
        }
      }
    }

    /// <summary>
    /// returns true if Mediaportal should send a notification when the program specified is about to start
    /// </summary>
    /// <param name="program">TVProgram</param>
    /// <returns>true : MP shows a notification when program is about to start</returns>
    private bool ShouldNotifyProgram(TVProgram program)
    {
      for (int i = 0; i < _notifyList.Count; ++i)
      {
        TVNotify notify = (TVNotify)_notifyList[i];
        if (notify.Program.ID == program.ID)
          return true;
      }
      return false;
    }

    protected int CalcDays()
    {
      int iDay = _viewingTime.DayOfYear - DateTime.Now.DayOfYear;
      if (_viewingTime.Year > DateTime.Now.Year)
      {
        iDay += (new DateTime(DateTime.Now.Year, 12, 31)).DayOfYear;
      }
      return iDay;
    }


    #region TV Database callbacks
    protected void TVDatabase_On_notifyListChanged()
    {
      if (_notifyList != null)
      {
        _notifyList.Clear();
        TVDatabase.GetNotifies(_notifyList, false);
        _needUpdate = true;
      }
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

    #endregion
  }
}
