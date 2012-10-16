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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Animation;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.TvPlugin.Helper;
using Action = MediaPortal.GUI.Library.Action;

namespace Mediaportal.TV.TvPlugin.EPG
{
  public abstract class GuideBase : GUIDialogWindow
  {    
    #region consts

    protected int _previousChannelCount = 0;
    protected const int MAX_DAYS_IN_GUIDE = 30;
    protected const int ROW_ID = 1000;
    protected const int COL_ID = 10;
    protected const int GUIDE_COMPONENTID_START = 50000;

    #endregion

    #region vars

    #region private

    private string _hdtvProgramText = String.Empty;
    private DateTime _keyPressedTimer = DateTime.Now;
    private double _lastCommandTime;
    private string _lineInput = String.Empty;
    private int _loopDelay = 100; // wait at the last item this amount of msec until loop to the first item
    private GUILabelControl _genreDarkTemplate;
    private GUILabelControl _genreTemplate;
    private GUIButton3PartControl _programPartialRecordTemplate;
    private GUIButton3PartControl _programRecordTemplate;
    private GUIButton3PartControl _programRunningTemplate;
    private GUIButton3PartControl _programNotRunningTemplate;
    private GUIButton3PartControl _programNotifyTemplate;
    private int _programOffset;
    private int _channelNumberMaxLength = 3;
    private int _minYIndex; // means cols here (programs/time)
    private int _backupChannelOffset;    
    private int _backupCursorY;
    private bool _byIndex;
    private bool _recalculateProgramOffset;
    private bool _showChannelNumber;
    private int _singleChannelNumber;
    private GUILabelControl _titleDarkTemplate;
    private GUILabelControl _titleTemplate;
    private int _totalProgramCount;
    private bool _useHdProgramIcon;
    private bool _useNewNotifyButtonColor;
    private bool _useNewPartialRecordingButtonColor;
    private bool _useNewRecordingButtonColor;
    private int _channelOffset;
    private IDictionary<int, GUIButton3PartControl> _controls = new Dictionary<int, GUIButton3PartControl>();
    private IList<Program> _programs;

    #endregion

    #region protected

    protected IList<GuideChannel> _channelList = new List<GuideChannel>();
    protected IList<Schedule> _recordingList = new List<Schedule>();
    protected int _channelCount = 5;
    protected Server.TVDatabase.Entities.Channel _currentChannel;
    protected ProgramBLL _currentProgram;
    protected bool _currentRecOrNotify;
    protected string _currentTitle = String.Empty;
    protected int _cursorX;
    protected int _cursorY;
    protected bool _needUpdate;
    protected int _numberOfBlocks = 4;    
    protected bool _showChannelLogos;
    protected bool _singleChannelView;
    protected int _timePerBlock = 30; // steps of 30 minutes
    protected DateTime _updateTimer = DateTime.Now;
    protected DateTime _updateTimerRecExpected = DateTime.Now;
    protected bool _guideContinuousScroll;
    protected DateTime _viewingTime = DateTime.Now;
    protected DateTime _startTime = DateTime.Now;

    #endregion

    #endregion

    #region protected methods

    protected int ChannelOffset
    {
      get { return _channelOffset; }
      set
      {
        _channelOffset = value;
        if (_channelOffset < 0)
        {
          _channelOffset = 0;
        }
      }
    }

    protected void Update(bool selectCurrentShow)
    {
      lock (this)
      {
        if (GUIWindowManager.ActiveWindowEx != GetID)
        {
          return;
        }

        // sets button visible state
        UpdateGroupButton();

        _updateTimer = DateTime.Now;
        var cntlDay = GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;

        // Find first day in TVGuide and set spincontrol position
        int iDay = CalcDays();
        for (; iDay < 0; ++iDay)
        {
          _viewingTime = _viewingTime.AddDays(1.0);
        }
        for (; iDay >= MAX_DAYS_IN_GUIDE; --iDay)
        {
          _viewingTime = _viewingTime.AddDays(-1.0);
        }
        if (cntlDay != null)
        {
          cntlDay.Value = iDay;
        }

        int xpos, ypos;
        GUIControl cntlPanel = GetControl((int)Controls.PANEL_BACKGROUND);
        var cntlChannelImg = (GUIImage)GetControl((int)Controls.CHANNEL_IMAGE_TEMPLATE);
        var cntlChannelLabel = (GUILabelControl)GetControl((int)Controls.CHANNEL_LABEL_TEMPLATE);
        var labelTime = (GUILabelControl)GetControl((int)Controls.LABEL_TIME1);
        var cntlHeaderBkgImg = (GUIImage)GetControl((int)Controls.IMG_TIME1);
        var cntlChannelTemplate = (GUIImage)GetControl((int)Controls.CHANNEL_TEMPLATE);


        _titleDarkTemplate = GetControl((int)Controls.LABEL_TITLE_DARK_TEMPLATE) as GUILabelControl;
        _titleTemplate = GetControl((int)Controls.LABEL_TITLE_TEMPLATE) as GUILabelControl;
        _genreDarkTemplate = GetControl((int)Controls.LABEL_GENRE_DARK_TEMPLATE) as GUILabelControl;
        _genreTemplate = GetControl((int)Controls.LABEL_GENRE_TEMPLATE) as GUILabelControl;

        _programPartialRecordTemplate = GetControl((int)Controls.BUTTON_PROGRAM_PARTIAL_RECORD) as GUIButton3PartControl;
        _programRecordTemplate = GetControl((int)Controls.BUTTON_PROGRAM_RECORD) as GUIButton3PartControl;
        _programNotifyTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOTIFY) as GUIButton3PartControl;
        _programNotRunningTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOT_RUNNING) as GUIButton3PartControl;
        _programRunningTemplate = GetControl((int)Controls.BUTTON_PROGRAM_RUNNING) as GUIButton3PartControl;

        _showChannelLogos = cntlChannelImg != null;
        if (_showChannelLogos)
        {
          if (cntlChannelImg != null)
          {
            cntlChannelImg.IsVisible = false;
          }
        }
        cntlChannelLabel.IsVisible = false;
        cntlHeaderBkgImg.IsVisible = false;
        labelTime.IsVisible = false;
        cntlChannelTemplate.IsVisible = false;
        int iLabelWidth = (cntlPanel.XPosition + cntlPanel.Width - labelTime.XPosition) / 4;

        // add labels for time blocks 1-4
        int iMin = _viewingTime.Minute;
        _viewingTime = _viewingTime.AddMinutes(-iMin);
        iMin = (iMin / _timePerBlock) * _timePerBlock;
        _viewingTime = _viewingTime.AddMinutes(iMin);

        var dt = new DateTime();
        dt = _viewingTime;

        for (int iLabel = 0; iLabel < 4; iLabel++)
        {
          xpos = iLabel * iLabelWidth + labelTime.XPosition;
          ypos = labelTime.YPosition;

          var img = GetControl((int)Controls.IMG_TIME1 + iLabel) as GUIImage;
          if (img == null)
          {
            img = new GUIImage(GetID, (int)Controls.IMG_TIME1 + iLabel, xpos, ypos, iLabelWidth - 4,
                               cntlHeaderBkgImg.RenderHeight, cntlHeaderBkgImg.FileName, 0x0);
            img.AllocResources();
            GUIControl cntl2 = img;
            Add(ref cntl2);
          }

          img.IsVisible = !_singleChannelView;
          img.Width = iLabelWidth - 4;
          img.Height = cntlHeaderBkgImg.RenderHeight;
          img.SetFileName(cntlHeaderBkgImg.FileName);
          img.SetPosition(xpos, ypos);
          img.DoUpdate();

          var label = GetControl((int)Controls.LABEL_TIME1 + iLabel) as GUILabelControl;
          if (label == null)
          {
            label = new GUILabelControl(GetID, (int)Controls.LABEL_TIME1 + iLabel, xpos, ypos, iLabelWidth,
                                        cntlHeaderBkgImg.RenderHeight, labelTime.FontName, String.Empty,
                                        labelTime.TextColor, labelTime.TextAlignment, labelTime.TextVAlignment, false,
                                        labelTime.ShadowAngle, labelTime.ShadowDistance, labelTime.ShadowColor);
            label.AllocResources();
            GUIControl cntl = label;
            Add(ref cntl);
          }

          string strTime = dt.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          label.Label = " " + strTime;
          dt = dt.AddMinutes(_timePerBlock);

          label.TextAlignment = GUIControl.Alignment.ALIGN_LEFT;
          label.IsVisible = !_singleChannelView;
          label.Width = iLabelWidth;
          label.Height = cntlHeaderBkgImg.RenderHeight;
          label.FontName = labelTime.FontName;
          label.TextColor = labelTime.TextColor;
          label.SetPosition(xpos, ypos);
        }

        // add channels...        
        int iItemHeight = cntlChannelTemplate.Height;
        UpdateChannelCount();

        for (int iChan = 0; iChan < _channelCount; ++iChan)
        {
          xpos = cntlChannelTemplate.XPosition;
          ypos = cntlChannelTemplate.YPosition + iChan * iItemHeight;

          var imgBut = GetControl((int)Controls.IMG_CHAN1 + iChan) as GUIButton3PartControl;
          if (imgBut == null)
          {
            string strChannelImageFileName = String.Empty;
            if (_showChannelLogos)
            {
              if (cntlChannelImg != null)
              {
                strChannelImageFileName = cntlChannelImg.FileName;
              }
            }

            // Use a template control if it exists, otherwise use default values.
            var buttonTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOT_RUNNING) as GUIButton3PartControl;
            if (buttonTemplate != null)
            {
              buttonTemplate.IsVisible = false;
              imgBut = new GUIButton3PartControl(GetID, (int)Controls.IMG_CHAN1 + iChan, xpos, ypos,
                                                 cntlChannelTemplate.Width - 2, cntlChannelTemplate.Height - 2,
                                                 buttonTemplate.TexutureFocusLeftName,
                                                 buttonTemplate.TexutureFocusMidName,
                                                 buttonTemplate.TexutureFocusRightName,
                                                 buttonTemplate.TexutureNoFocusLeftName,
                                                 buttonTemplate.TexutureNoFocusMidName,
                                                 buttonTemplate.TexutureNoFocusRightName,
                                                 strChannelImageFileName)
                         {
                           TileFillTFL = buttonTemplate.TileFillTFL,
                           TileFillTNFL = buttonTemplate.TileFillTNFL,
                           TileFillTFM = buttonTemplate.TileFillTFM,
                           TileFillTNFM = buttonTemplate.TileFillTNFM,
                           TileFillTFR = buttonTemplate.TileFillTFR,
                           TileFillTNFR = buttonTemplate.TileFillTNFR
                         };

            }
            else
            {
              imgBut = new GUIButton3PartControl(GetID, (int)Controls.IMG_CHAN1 + iChan, xpos, ypos,
                                                 cntlChannelTemplate.Width - 2, cntlChannelTemplate.Height - 2,
                                                 "tvguide_button_selected_left.png",
                                                 "tvguide_button_selected_middle.png",
                                                 "tvguide_button_selected_right.png",
                                                 "tvguide_button_light_left.png",
                                                 "tvguide_button_light_middle.png",
                                                 "tvguide_button_light_right.png",
                                                 strChannelImageFileName);
            }
            imgBut.AllocResources();
            GUIControl cntl = imgBut;
            Add(ref cntl);
          }

          imgBut.Width = cntlChannelTemplate.Width - 2;
          imgBut.Height = cntlChannelTemplate.Height - 2; 
          imgBut.SetPosition(xpos, ypos);
          imgBut.FontName1 = cntlChannelLabel.FontName;
          imgBut.TextColor1 = cntlChannelLabel.TextColor;
          imgBut.Label1 = String.Empty;
          imgBut.RenderLeft = false;
          imgBut.RenderRight = false;
          imgBut.SetShadow1(cntlChannelLabel.ShadowAngle, cntlChannelLabel.ShadowDistance, cntlChannelLabel.ShadowColor);

          if (_showChannelLogos)
          {
            if (cntlChannelImg != null) 
            {
              imgBut.TexutureIcon = cntlChannelImg.FileName;
              imgBut.IconOffsetX = cntlChannelImg.XPosition;
              imgBut.IconOffsetY = cntlChannelImg.YPosition;
              imgBut.IconWidth = cntlChannelImg.RenderWidth;
              imgBut.IconHeight = cntlChannelImg.RenderHeight;
              imgBut.IconKeepAspectRatio = cntlChannelImg.KeepAspectRatio;
              imgBut.IconCentered = cntlChannelImg.Centered;
              imgBut.IconZoom = cntlChannelImg.Zoom;
            }
          }
          imgBut.TextOffsetX1 = cntlChannelLabel.XPosition;
          imgBut.TextOffsetY1 = cntlChannelLabel.YPosition;
          imgBut.ColourDiffuse = 0xffffffff;
          imgBut.DoUpdate();
        }

        UpdateHorizontalScrollbar();
        UpdateVerticalScrollbar();

        GetChannels(false);


        string day;
        switch (_viewingTime.DayOfWeek)
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
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.View.SDOW", day);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.View.Month", _viewingTime.Month.ToString());
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.View.Day", _viewingTime.Day.ToString());

        day = Utils.GetShortDayString(_viewingTime);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Day", day);

        //2004 03 31 22 20 00
        string strStart = String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}",
                                        _viewingTime.Year, _viewingTime.Month, _viewingTime.Day,
                                        _viewingTime.Hour, _viewingTime.Minute, 0);
        var dtStop = new DateTime();
        dtStop = _viewingTime;
        dtStop = dtStop.AddMinutes(_numberOfBlocks * _timePerBlock - 1);
        iMin = dtStop.Minute;
        string strEnd = String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}",
                                      dtStop.Year, dtStop.Month, dtStop.Day,
                                      dtStop.Hour, iMin, 0);

        long iStart = Int64.Parse(strStart);
        long iEnd = Int64.Parse(strEnd);


        LoadSchedules(false);

        if (ChannelOffset > _channelList.Count)
        {
          ChannelOffset = 0;
          _cursorX = 0;
        }

        foreach (GUIControl cntl in controlList.Where(cntl => cntl.GetID >= GUIDE_COMPONENTID_START)) 
        {
          cntl.IsVisible = false;
        }

        if (_singleChannelView)
        {
          // show all buttons (could be less visible if channels < rows)
          for (int iChannel = 0; iChannel < _channelCount; iChannel++)
          {
            var imgBut = GetControl((int)Controls.IMG_CHAN1 + iChannel) as GUIButton3PartControl;
            if (imgBut != null)
              imgBut.IsVisible = true;
          }

          Channel channel = _channelList[_singleChannelNumber].Channel;
          SetGuideHeadingVisibility(false);
          RenderSingleChannel(channel);
        }
        else
        {
          var visibleChannels = new List<Channel>();

          int chan = ChannelOffset;
          for (int iChannel = 0; iChannel < _channelCount; iChannel++)
          {
            if (chan >= 0 && chan < _channelList.Count)
            {
              visibleChannels.Add(_channelList[chan].Channel);
            }
            chan++;
            if (chan >= _channelList.Count && visibleChannels.Count < _channelList.Count)
            {
              chan = 0;
            }
          }

          IDictionary<int, IList<Program>> programEntities = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsForAllChannels(Utils.longtodate(iStart),
                                                                      Utils.longtodate(iEnd),
                                                                      visibleChannels);
          // todo tedious code.. Id like to rethink this
          // as we need to convert the IDictionary<int, IList<Program>> to IDictionary<int, IList<ProgramBLL>>
          IDictionary<int, IList<ProgramBLL>> programs = new Dictionary<int, IList<ProgramBLL>>();
          foreach (IList<Program> pList in programEntities.Values)
          {
            IList<ProgramBLL> programBlls = new List<ProgramBLL>();
            int idchannel = 0;
            foreach (var p in pList)
            {
              idchannel = p.IdChannel;
              var programBll = new ProgramBLL(p);
              programBlls.Add(programBll);              
            }
            programs[idchannel] = programBlls;
          }

         
          // make sure the TV Guide heading is visiable and the single channel labels are not.
          SetGuideHeadingVisibility(true);
          SetSingleChannelLabelVisibility(false);
          chan = ChannelOffset;

          int firstButtonYPos = 0;
          int lastButtonYPos = 0;

          int channelCount = _channelCount;
          if (_previousChannelCount > channelCount)
          {
            channelCount = _previousChannelCount;
          }

          for (int iChannel = 0; iChannel < channelCount; iChannel++)
          {
            if (chan >= 0 && chan < _channelList.Count)
            {
              GuideChannel tvGuideChannel = _channelList[chan];
              RenderChannel(ref programs, iChannel, tvGuideChannel, iStart, iEnd, selectCurrentShow);
              // remember bottom y position from last visible button
              var imgBut = GetControl((int)Controls.IMG_CHAN1 + iChannel) as GUIButton3PartControl;
              if (imgBut != null)
              {
                if (iChannel == 0)
                  firstButtonYPos = imgBut.YPosition;

                lastButtonYPos = imgBut.YPosition + imgBut.Height;
              }
            }
            chan++;
            if (chan >= _channelList.Count && _channelList.Count > _channelCount)
            {
              chan = 0;
            }
            if (chan > _channelList.Count)
            {
              var imgBut = GetControl((int)Controls.IMG_CHAN1 + iChannel) as GUIButton3PartControl;
              if (imgBut != null)
              {
                imgBut.IsVisible = false;
              }
            }
          }

          var vertLine = GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
          if (vertLine != null)
          {
            // height taken from last button (bottom) minus the yposition of slider plus the offset of slider in relation to first button
            vertLine.Height = lastButtonYPos - vertLine.YPosition + (firstButtonYPos - vertLine.YPosition);
          }
          // update selected channel
          _singleChannelNumber = _cursorX + ChannelOffset;
          if (_singleChannelNumber >= _channelList.Count)
          {
            _singleChannelNumber -= _channelList.Count;
          }

          // instead of direct casting us "as"; else it fails for other controls!
          var img = GetControl(_cursorX + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
          if (null != img)
          {
            _currentChannel = (Channel)img.Data;
          }
        }
        UpdateVerticalScrollbar();
      }
    }    

    protected void LoadSchedules(bool refresh)
    {
      if (refresh)
      {
        _recordingList = ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules().ToList();
        return;
      }
    }

    protected void GetChannels(bool refresh)
    {
      if (refresh || _channelList == null)
      {
        _channelList = new List<GuideChannel>();
      }

      if (_channelList.Count == 0)
      {
        try
        {
          bool hasSelectedGroup = HasSelectedGroup();
          if (hasSelectedGroup)
          {
            IList<Channel> channels = GetGuideChannelsForGroup();
            foreach (Channel chan in channels)
            {
              var tvGuidChannel = new GuideChannel {Channel = chan};

              if (tvGuidChannel.Channel.VisibleInGuide && IsChannelTypeCorrect(tvGuidChannel.Channel))
              {
                if (_showChannelNumber)
                {
                  if (_byIndex)
                  {
                    tvGuidChannel.ChannelNum = _channelList.Count + 1;
                  }
                  else
                  {
                    foreach (TuningDetail detail in tvGuidChannel.Channel.TuningDetails)
                    {
                      tvGuidChannel.ChannelNum = detail.ChannelNumber;
                    }
                  }
                }
                tvGuidChannel.StrLogo = GetChannelLogo(tvGuidChannel.Channel.DisplayName);
                _channelList.Add(tvGuidChannel);
              }
            }
          }
        }
        catch {/*ignore*/}

        if (_channelList.Count == 0)
        {
          var tvGuidChannel = new GuideChannel
                                {
                                  Channel = ChannelFactory.CreateChannel(MediaTypeEnum.TV, 0, DateTime.MinValue, false,
                                                        DateTime.MinValue, 0, true, "", GUILocalizeStrings.Get(911))
                                };
          for (int i = 0; i < 10; ++i)
          {
            _channelList.Add(tvGuidChannel);
          }
        }
      }
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
      ProgramBLL modifiedProg =
        new ProgramBLL (ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByTitleTimesAndChannel(_currentProgram.Entity.Title,
                                                              _currentProgram.Entity.StartTime,
                                                              _currentProgram.Entity.EndTime,
                                                              _currentProgram.Entity.IdChannel))
          {Notify = _currentProgram.Notify};

      ServiceAgents.Instance.ProgramServiceAgent.SaveProgram(modifiedProg.Entity);
      TvNotifyManager.OnNotifiesChanged();
      Update(false);
      SetFocus();
    }  

    protected bool IsRecordingNoEPG(Channel channel)
    {
      IVirtualCard vc;
      ServiceAgents.Instance.ControllerServiceAgent.IsRecording(channel.IdChannel, out vc);

      if (vc != null)
      {
        return vc.IsRecording;
      }
      return false;
    }    

   

    protected void UpdateCurrentProgram()
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
      int iControlId = GUIDE_COMPONENTID_START + _cursorX * ROW_ID + (_cursorY - 1) * COL_ID;
      var img = GetControl(iControlId) as GUIButton3PartControl;
      if (null != img)
      {
        SetFocus();
        _currentProgram = new ProgramBLL((Program)img.Data);
        SetProperties();
      }
    }

    protected void OnSwitchMode()
    {
      UnFocus();
      _singleChannelView = !_singleChannelView;
      if (_singleChannelView)
      {        
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

    protected void UnFocus()
    {
      if (_cursorX < 0)
      {
        return;
      }
      if (_cursorY == 0 || _cursorY == _minYIndex) // either channel or group button
      {
        int controlid = (int)Controls.IMG_CHAN1 + _cursorX;
        GUIControl.UnfocusControl(GetID, controlid);
      }
      else
      {
        Correct();
        int iControlId = GUIDE_COMPONENTID_START + _cursorX * ROW_ID + (_cursorY - 1) * COL_ID;
        var img = GetControl(iControlId) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          if (_currentProgram != null)
          {
            img.ColourDiffuse = GetColorForGenre();
          }
        }
        GUIControl.UnfocusControl(GetID, iControlId);
      }
    }

    protected void SetFocus()
    {
      if (_cursorX < 0)
      {
        return;
      }
      if (_cursorY == 0 || _cursorY == _minYIndex) // either channel or group button
      {
        int controlid;
        GUIControl.UnfocusControl(GetID, (int)Controls.SPINCONTROL_DAY);
        GUIControl.UnfocusControl(GetID, (int)Controls.SPINCONTROL_TIME_INTERVAL);

        if (_cursorY == -1)
          controlid = (int)Controls.CHANNEL_GROUP_BUTTON;
        else
          controlid = (int)Controls.IMG_CHAN1 + _cursorX;

        GUIControl.FocusControl(GetID, controlid);
      }
      else
      {
        Correct();
        int iControlId = GUIDE_COMPONENTID_START + _cursorX * ROW_ID + (_cursorY - 1) * COL_ID;
        var img = GetControl(iControlId) as GUIButton3PartControl;
        if (null != img && img.IsVisible)
        {
          img.ColourDiffuse = 0xffffffff;
          _currentProgram = new ProgramBLL(img.Data as Program);
          SetProperties();
        }
        GUIControl.FocusControl(GetID, iControlId);
      }
    }

    /// <summary>
    /// "Record" entry in context menu
    /// </summary>
    protected void OnRecordContext()
    {
      if (_currentProgram == null)
      {
        return;
      }
      ShowProgramInfo();
    }

    protected void ShowProgramInfo()
    {
      if (_currentProgram == null)
      {
        return;
      }

      TVProgramInfo.CurrentProgram = _currentProgram.Entity;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_PROGRAM_INFO);
    }

    protected void OnKeyTimeout()
    {
      if (_lineInput.Length == 0)
      {
        // Hide label if no keyed channel number to display.
        var label = GetControl((int)Controls.LABEL_KEYED_CHANNEL) as GUILabelControl;
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

    #endregion

    #region private methods

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
      Channel chan = _channelList[channel].Channel;
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
          IList<TuningDetail> detail = chan.TuningDetails;
          int channelNum = detail[0].ChannelNumber;
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
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.SubTitle", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Episode", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.EpisodeDetail", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Date", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.StarRating", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Classification", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Duration", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.DurationMins", String.Empty);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.TimeFromNow", String.Empty);

        _currentTitle = String.Empty;
        _currentChannel = chan;
        GUIControl.HideControl(GetID, (int)Controls.IMG_REC_PIN);
      }
      else if (_currentProgram != null)
      {
        string strTime = String.Format("{0}-{1}",
                                       _currentProgram.Entity.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       _currentProgram.Entity.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Title", _currentProgram.Entity.Title);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.CompositeTitle",
                                       TVUtil.GetDisplayTitle(_currentProgram.Entity));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Time", strTime);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Description", _currentProgram.Entity.Description);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Genre", TVUtil.GetCategory(_currentProgram.Entity.ProgramCategory));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Duration", GetDuration(_currentProgram.Entity));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.DurationMins", GetDurationAsMinutes(_currentProgram.Entity));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.TimeFromNow", GetStartTimeFromNow(_currentProgram.Entity));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Episode", _currentProgram.Entity.EpisodeNum);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.SubTitle", _currentProgram.Entity.EpisodeName);

        if (_currentProgram.Entity.Classification == "")
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Classification", "No Rating");
        }
        else
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Classification", _currentProgram.Entity.Classification);
        }

        _currentTitle = _currentProgram.Entity.Title;
        _currentChannel = chan;

        bool bSeries = _currentProgram.IsRecordingSeries || _currentProgram.IsRecordingSeriesPending ||
                       _currentProgram.IsPartialRecordingSeriesPending;
        bool bConflict = _currentProgram.HasConflict;
        bRecording = bSeries || (_currentProgram.IsRecording || _currentProgram.IsRecordingOncePending);

        if (bRecording)
        {
          var img = (GUIImage)GetControl((int)Controls.IMG_REC_PIN);

          bool bPartialRecording = _currentProgram.IsPartialRecordingSeriesPending;

          if (bConflict)
          {
            if (bSeries)
            {
              img.SetFileName(bPartialRecording
                                ? Thumbs.TvConflictPartialRecordingSeriesIcon
                                : Thumbs.TvConflictRecordingSeriesIcon);
            }
            else
            {
              img.SetFileName(bPartialRecording
                                ? Thumbs.TvConflictPartialRecordingIcon
                                : Thumbs.TvConflictRecordingIcon);
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

    /// <summary>
    /// Logic to decide if channel group button is available and visible
    /// </summary>
    private bool GroupButtonAvail
    {
      get
      {
        // show/hide channel group button
        var btnChannelGroup = GetControl((int)Controls.CHANNEL_GROUP_BUTTON) as GUIButtonControl;

        // visible only if more than one group? and not in single channel, and button exists in skin!
        return (ChannelGroupCount > 1 && !_singleChannelView && btnChannelGroup != null);
      }
    }

    private void RenderSingleChannel(Channel channel)
    {
      string strLogo;
      int chan = ChannelOffset;
      for (int iChannel = 0; iChannel < _channelCount; iChannel++)
      {
        if (chan < _channelList.Count)
        {
          Channel tvChan = _channelList[chan].Channel;

          strLogo = GetChannelLogo(tvChan.DisplayName);
          var img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
          if (img != null)
          {
            if (_showChannelLogos)
            {
              img.TexutureIcon = strLogo;
            }
            img.Label1 = tvChan.DisplayName;
            img.Data = tvChan;
            img.IsVisible = true;
          }
        }
        chan++;
      }

      var channelLabel = GetControl((int)Controls.SINGLE_CHANNEL_LABEL) as GUILabelControl;
      var channelImage = GetControl((int)Controls.SINGLE_CHANNEL_IMAGE) as GUIImage;

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
          GUIControl temp = channelImage;
          Add(ref temp);
        }
      }
      else
      {
        channelImage.SetFileName(strLogo);
      }

      if (channelLabel == null)
      {
        if (channelImage != null)
        {
          channelLabel = new GUILabelControl(GetID, (int)Controls.SINGLE_CHANNEL_LABEL,
                                             channelImage.XPosition + 44,
                                             channelImage.YPosition + 10,
                                             300, 40, "font16", channel.DisplayName, 4294967295, GUIControl.Alignment.Left,
                                             GUIControl.VAlignment.Top,
                                             true, 0, 0, 0xFF000000);
        }
        if (channelLabel != null)
        {
          channelLabel.AllocResources();
          GUIControl temp = channelLabel;
          Add(ref temp);
        }
      }

      SetSingleChannelLabelVisibility(true);

      if (channelLabel != null)
      {
        channelLabel.Label = channel.DisplayName;
      }
      if (strLogo.Length > 0)
      {
        if (channelImage != null)
        {
          channelImage.SetFileName(strLogo);
        }
      }

      if (channelLabel != null)
      {
        channelLabel.Label = channel.DisplayName;
      }
      if (_recalculateProgramOffset)
      {        
        DateTime dtStart = DateTime.Now;
        dtStart = dtStart.AddDays(-1);
        DateTime dtEnd = dtStart.AddDays(30);
        _programs = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByChannelAndStartEndTimes(channel.IdChannel, dtStart, dtEnd).ToList();

        _totalProgramCount = _programs.Count;
        if (_totalProgramCount == 0)
        {
          _totalProgramCount = _channelCount;
        }

        _recalculateProgramOffset = false;
        bool found = false;
        for (int i = 0; i < _programs.Count; i++)
        {
          Program program = _programs[i];
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
        int day = (_programs[_programOffset]).StartTime.DayOfYear;
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
          var cntlDay = GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;

          // Find first day in TVGuide and set spincontrol position
          int iDay = CalcDays();
          for (; iDay < 0; ++iDay)
          {
            _viewingTime = _viewingTime.AddDays(1.0);
          }
          for (; iDay >= MAX_DAYS_IN_GUIDE; --iDay)
          {
            _viewingTime = _viewingTime.AddDays(-1.0);
          }
          if (cntlDay != null)
          {
            cntlDay.Value = iDay;
          }
        }
      }
      // ichan = number of rows
      for (int ichan = 0; ichan < _channelCount; ++ichan)
      {
        var imgCh = GetControl(ichan + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
        if (imgCh != null)
        {
          imgCh.TexutureIcon = "";
        }

        int iStartXPos = GetControl(0 + (int)Controls.LABEL_TIME1).XPosition;
        int height = GetControl((int)Controls.IMG_CHAN1 + 1).YPosition;
        height -= GetControl((int)Controls.IMG_CHAN1).YPosition;
        int width = GetControl((int)Controls.LABEL_TIME1 + 1).XPosition;
        width -= GetControl((int)Controls.LABEL_TIME1).XPosition;

        int iTotalWidth = width * _numberOfBlocks;

        ProgramBLL program;
        int offset = _programOffset;
        if (offset + ichan < _programs.Count)
        {
          program = new ProgramBLL(_programs[offset + ichan]);
        }
        else
        {
          // bugfix for 0 items
          if (_programs.Count == 0)
          {
            program = new ProgramBLL(
              ProgramFactory.CreateProgram(channel.IdChannel, _viewingTime, _viewingTime, "-", string.Empty,
                                                   null,
                                                   ProgramState.None,
                                                   DateTime.MinValue, string.Empty, string.Empty, string.Empty,
                                                   string.Empty, -1,
                                                   string.Empty, -1));                       
          }
          else
          {
            program = new ProgramBLL(_programs[_programs.Count - 1]);
            if (program.Entity.EndTime.DayOfYear == _viewingTime.DayOfYear)
            {
              program = new ProgramBLL(ProgramFactory.CreateProgram(channel.IdChannel, program.Entity.EndTime, program.Entity.EndTime, "-", "-", null,
                                    ProgramState.None,
                                    DateTime.MinValue, string.Empty, string.Empty, string.Empty, string.Empty, -1,
                                    string.Empty, -1));
            }
            else
            {
              program = new ProgramBLL(ProgramFactory.CreateProgram(channel.IdChannel, _viewingTime, _viewingTime, "-", "-", null,
                                    ProgramState.None,
                                    DateTime.MinValue, string.Empty, string.Empty, string.Empty, string.Empty, -1,
                                    string.Empty, -1));
            }
          }
        }

        int ypos = GetControl(ichan + (int)Controls.IMG_CHAN1).YPosition;
        int iControlId = GUIDE_COMPONENTID_START + ichan * ROW_ID + 0 * COL_ID;
        var img = GetControl(iControlId) as GUIButton3PartControl;
        var buttonTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOT_RUNNING) as GUIButton3PartControl;

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
                                            String.Empty)
            {
              TileFillTFL = buttonTemplate.TileFillTFL,
              TileFillTNFL = buttonTemplate.TileFillTNFL,
              TileFillTFM = buttonTemplate.TileFillTFM,
              TileFillTNFM = buttonTemplate.TileFillTNFM,
              TileFillTFR = buttonTemplate.TileFillTFR,
              TileFillTNFR = buttonTemplate.TileFillTNFR
            };

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
          img.ColourDiffuse = GetColorForGenre();
          GUIControl cntl = img;
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
          img.ColourDiffuse = GetColorForGenre();
          img.IsVisible = true;
          img.DoUpdate();
        }
        img.RenderLeft = false;
        img.RenderRight = false;

        bool bSeries = (program.IsRecordingSeries || program.IsRecordingSeriesPending ||
                        program.IsPartialRecordingSeriesPending);
        bool bConflict = program.HasConflict;
        bool bRecording = bSeries || (program.IsRecording || program.IsRecordingOncePending);

        img.Data = program.Entity;
        img.ColourDiffuse = GetColorForGenre();
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

        img.Label1 = TVUtil.GetDisplayTitle(program.Entity);

        string strTimeSingle = String.Format("{0}",
                                             program.Entity.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        if (program.Entity.StartTime.DayOfYear != _viewingTime.DayOfYear)
        {
          img.Label1 = String.Format("{0} {1}", Utils.GetShortDayString(program.Entity.StartTime),
                                     TVUtil.GetDisplayTitle(program.Entity));
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
          img.Label2 = TVUtil.GetCategory(program.Entity.ProgramCategory);
          img.TextOffsetX2 = labelTemplate.XPosition;
          img.TextOffsetY2 = labelTemplate.YPosition;
          img.SetShadow2(labelTemplate.ShadowAngle, labelTemplate.ShadowDistance, labelTemplate.ShadowColor);
        }
        if (imgCh != null)
        {
          imgCh.Label1 = strTimeSingle;
          imgCh.TexutureIcon = "";
        }

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
          var buttonNotifyTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOTIFY) as GUIButton3PartControl;
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
          var buttonRecordTemplate = GetControl((int)Controls.BUTTON_PROGRAM_RECORD) as GUIButton3PartControl;

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

            // Use of the button template control implies use of the icon.  Use a blank image if the icon is not desired.
            if (bConflict)
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
              if (bSeries)
              {
                img.TexutureIcon = Thumbs.TvRecordingSeriesIcon;
              }
              else
              {
                img.TexutureIcon = Thumbs.TvRecordingIcon;
              }
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
      }
    }

    private void RenderChannel(ref IDictionary<int, IList<ProgramBLL>> mapPrograms, int iChannel,
                                 GuideChannel tvGuideChannel,
                                 long iStart, long iEnd, bool selectCurrentShow)
    {
      int channelNum = 0;
      Channel channel = tvGuideChannel.Channel;

      if (!_byIndex)
      {
        foreach (TuningDetail detail in channel.TuningDetails)
        {
          channelNum = detail.ChannelNumber;
        }
      }
      else
      {
        channelNum = _channelList.IndexOf(tvGuideChannel) + 1;
      }

      var img = GetControl(iChannel + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
      if (img != null)
      {
        if (_showChannelLogos)
        {
          img.TexutureIcon = tvGuideChannel.StrLogo;
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
      }


      IList<ProgramBLL> programs = null;
      if (mapPrograms.ContainsKey(channel.IdChannel))
      {
        programs = mapPrograms[channel.IdChannel];
      }

      bool noEPG = (programs == null || programs.Count == 0);
      if (noEPG)
      {
        DateTime dt = Utils.longtodate(iEnd);
        long iProgEnd = Utils.datetolong(dt);
        var prog = ProgramFactory.CreateProgram(channel.IdChannel, Utils.longtodate(iStart), Utils.longtodate(iProgEnd),
                               GUILocalizeStrings.Get(736), "", null, ProgramState.None, DateTime.MinValue,
                               string.Empty,
                               string.Empty, string.Empty, string.Empty, -1, string.Empty, -1);
        prog.Channel = channel;
        if (programs == null)
        {
          programs = new List<ProgramBLL>();
        }
        programs.Add(new ProgramBLL(prog));
      }

      int iProgram = 0;
      int iPreviousEndXPos = 0;

      int width = GetControl((int)Controls.LABEL_TIME1 + 1).XPosition;
      width -= GetControl((int)Controls.LABEL_TIME1).XPosition;

      int height = GetControl((int)Controls.IMG_CHAN1 + 1).YPosition;
      height -= GetControl((int)Controls.IMG_CHAN1).YPosition;

      foreach (ProgramBLL program in programs)
      {
        if (Utils.datetolong(program.Entity.EndTime) <= iStart)
          continue;

        string strTitle = TVUtil.GetDisplayTitle(program.Entity);
        bool bStartsBefore = false;
        bool bEndsAfter = false;

        if (Utils.datetolong(program.Entity.StartTime) < iStart)
          bStartsBefore = true;

        if (Utils.datetolong(program.Entity.EndTime) > iEnd)
          bEndsAfter = true;

        DateTime dtBlokStart = _viewingTime;
        dtBlokStart = dtBlokStart.AddMilliseconds(-dtBlokStart.Millisecond);
        dtBlokStart = dtBlokStart.AddSeconds(-dtBlokStart.Second);

        bool bConflict = program.HasConflict;
        bool bSeries = (program.IsRecordingSeries || program.IsRecordingSeriesPending);
        bool bRecording = bSeries ||
                          (program.IsRecording || program.IsRecordingOncePending || program.IsPartialRecordingSeriesPending);
        bool bPartialRecording = program.IsPartialRecordingSeriesPending;
        if (noEPG && !bRecording)
        {
          bRecording = IsRecordingNoEPG(channel);
        }

        bool programIsHd = program.Entity.Description.Contains(_hdtvProgramText);

        int iStartXPos = 0;
        int iEndXPos = 0;
        for (int iBlok = 0; iBlok < _numberOfBlocks; iBlok++)
        {
          float fWidthEnd = width;
          DateTime dtBlokEnd = dtBlokStart.AddMinutes(_timePerBlock - 1);
          if (program.RunningAt(dtBlokStart, dtBlokEnd))
          {
            if (program.Entity.EndTime <= dtBlokEnd)
            {
              TimeSpan dtSpan = dtBlokEnd - program.Entity.EndTime;
              int iEndMin = _timePerBlock - (dtSpan.Minutes);

              fWidthEnd = ((iEndMin) / ((float)_timePerBlock)) * ((width));
              if (bEndsAfter)
              {
                fWidthEnd = width;
              }
            }

            if (iStartXPos == 0)
            {
              TimeSpan ts = program.Entity.StartTime - dtBlokStart;
              int iStartMin = ts.Hours * 60;
              iStartMin += ts.Minutes;
              if (ts.Seconds == 59)
              {
                iStartMin += 1;
              }
              float fWidth = ((iStartMin) / ((float)_timePerBlock)) * ((width));

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
          int iControlId = GUIDE_COMPONENTID_START + iChannel * ROW_ID + iProgram * COL_ID;


          string texutureFocusLeftName = "tvguide_button_selected_left.png";
          string texutureFocusMidName = "tvguide_button_selected_middle.png";
          string texutureFocusRightName = "tvguide_button_selected_right.png";
          string texutureNoFocusLeftName = "tvguide_button_light_left.png";
          string texutureNoFocusMidName = "tvguide_button_light_middle.png";
          string texutureNoFocusRightName = "tvguide_button_light_right.png";

          bool tileFillTfl = false;
          bool tileFillTnfl = false;
          bool tileFillTfm = false;
          bool tileFillTnfm = false;
          bool tileFillTfr = false;
          bool tileFillTnfr = false;

          if (_programNotRunningTemplate != null)
          {
            _programNotRunningTemplate.IsVisible = false;
            texutureFocusLeftName = _programNotRunningTemplate.TexutureFocusLeftName;
            texutureFocusMidName = _programNotRunningTemplate.TexutureFocusMidName;
            texutureFocusRightName = _programNotRunningTemplate.TexutureFocusRightName;
            texutureNoFocusLeftName = _programNotRunningTemplate.TexutureNoFocusLeftName;
            texutureNoFocusMidName = _programNotRunningTemplate.TexutureNoFocusMidName;
            texutureNoFocusRightName = _programNotRunningTemplate.TexutureNoFocusRightName;
            tileFillTfl = _programNotRunningTemplate.TileFillTFL;
            tileFillTnfl = _programNotRunningTemplate.TileFillTNFL;
            tileFillTfm = _programNotRunningTemplate.TileFillTFM;
            tileFillTnfm = _programNotRunningTemplate.TileFillTNFM;
            tileFillTfr = _programNotRunningTemplate.TileFillTFR;
            tileFillTnfr = _programNotRunningTemplate.TileFillTNFR;
          }

          bool isNew = false;
          int iWidth = iEndXPos - iStartXPos;
          if (iWidth > 3)
          {
            iWidth -= 3;
          }
          else
          {
            iWidth = 1;
          }
          if (!_controls.TryGetValue(iControlId, out img))
          {
            img = new GUIButton3PartControl(GetID, iControlId, iStartXPos, ypos, iWidth, height - 2,
                                            texutureFocusLeftName,
                                            texutureFocusMidName,
                                            texutureFocusRightName,
                                            texutureNoFocusLeftName,
                                            texutureNoFocusMidName,
                                            texutureNoFocusRightName,
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

          img.TexutureIcon = String.Empty;
          if (program.Notify)
          {
            if (_programNotifyTemplate != null)
            {
              _programNotifyTemplate.IsVisible = false;
              texutureFocusLeftName = _programNotifyTemplate.TexutureFocusLeftName;
              texutureFocusMidName = _programNotifyTemplate.TexutureFocusMidName;
              texutureFocusRightName = _programNotifyTemplate.TexutureFocusRightName;
              texutureNoFocusLeftName = _programNotifyTemplate.TexutureNoFocusLeftName;
              texutureNoFocusMidName = _programNotifyTemplate.TexutureNoFocusMidName;
              texutureNoFocusRightName = _programNotifyTemplate.TexutureNoFocusRightName;
              tileFillTfl = _programNotifyTemplate.TileFillTFL;
              tileFillTnfl = _programNotifyTemplate.TileFillTNFL;
              tileFillTfm = _programNotifyTemplate.TileFillTFM;
              tileFillTnfm = _programNotifyTemplate.TileFillTNFM;
              tileFillTfr = _programNotifyTemplate.TileFillTFR;
              tileFillTnfr = _programNotifyTemplate.TileFillTNFR;

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
                texutureFocusLeftName = "tvguide_notifyButton_Focus_left.png";
                texutureFocusMidName = "tvguide_notifyButton_Focus_middle.png";
                texutureFocusRightName = "tvguide_notifyButton_Focus_right.png";
                texutureNoFocusLeftName = "tvguide_notifyButton_noFocus_left.png";
                texutureNoFocusMidName = "tvguide_notifyButton_noFocus_middle.png";
                texutureNoFocusRightName = "tvguide_notifyButton_noFocus_right.png";
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
              texutureFocusLeftName = buttonRecordTemplate.TexutureFocusLeftName;
              texutureFocusMidName = buttonRecordTemplate.TexutureFocusMidName;
              texutureFocusRightName = buttonRecordTemplate.TexutureFocusRightName;
              texutureNoFocusLeftName = buttonRecordTemplate.TexutureNoFocusLeftName;
              texutureNoFocusMidName = buttonRecordTemplate.TexutureNoFocusMidName;
              texutureNoFocusRightName = buttonRecordTemplate.TexutureNoFocusRightName;
              tileFillTfl = buttonRecordTemplate.TileFillTFL;
              tileFillTnfl = buttonRecordTemplate.TileFillTNFL;
              tileFillTfm = buttonRecordTemplate.TileFillTFM;
              tileFillTnfm = buttonRecordTemplate.TileFillTNFM;
              tileFillTfr = buttonRecordTemplate.TileFillTFR;
              tileFillTnfr = buttonRecordTemplate.TileFillTNFR;

              // Use of the button template control implies use of the icon.  Use a blank image if the icon is not desired.
              if (bConflict)
              {
                texutureFocusLeftName = "tvguide_recButton_Focus_left.png";
                texutureFocusMidName = "tvguide_recButton_Focus_middle.png";
                texutureFocusRightName = "tvguide_recButton_Focus_right.png";
                texutureNoFocusLeftName = "tvguide_recButton_noFocus_left.png";
                texutureNoFocusMidName = "tvguide_recButton_noFocus_middle.png";
                texutureNoFocusRightName = "tvguide_recButton_noFocus_right.png";
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
                texutureFocusLeftName = "tvguide_partRecButton_Focus_left.png";
                texutureFocusMidName = "tvguide_partRecButton_Focus_middle.png";
                texutureFocusRightName = "tvguide_partRecButton_Focus_right.png";
                texutureNoFocusLeftName = "tvguide_partRecButton_noFocus_left.png";
                texutureNoFocusMidName = "tvguide_partRecButton_noFocus_middle.png";
                texutureNoFocusRightName = "tvguide_partRecButton_noFocus_right.png";
              }
              else
              {
                if (_useNewRecordingButtonColor)
                {
                  texutureFocusLeftName = "tvguide_recButton_Focus_left.png";
                  texutureFocusMidName = "tvguide_recButton_Focus_middle.png";
                  texutureFocusRightName = "tvguide_recButton_Focus_right.png";
                  texutureNoFocusLeftName = "tvguide_recButton_noFocus_left.png";
                  texutureNoFocusMidName = "tvguide_recButton_noFocus_middle.png";
                  texutureNoFocusRightName = "tvguide_recButton_noFocus_right.png";
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

          img.TexutureIcon2 = String.Empty;
          if (programIsHd)
          {
            if (_programNotRunningTemplate != null)
            {
              img.TexutureIcon2 = _programNotRunningTemplate.TexutureIcon2;
            }
            else
            {
              if (_useHdProgramIcon)
              {
                img.TexutureIcon2 = "tvguide_hd_program.png";
              }
            }
            img.Icon2InlineLabel1 = true;
            img.Icon2VAlign = GUIControl.VAlignment.ALIGN_MIDDLE;
            img.Icon2OffsetX = 5;
          }
          img.Data = ProgramFactory.Clone(program.Entity);
          img.ColourDiffuse = GetColorForGenre();

          iWidth = iEndXPos - iStartXPos;
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
            img.Label2 = TVUtil.GetCategory(program.Entity.ProgramCategory);
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
              texutureFocusLeftName = buttonRunningTemplate.TexutureFocusLeftName;
              texutureFocusMidName = buttonRunningTemplate.TexutureFocusMidName;
              texutureFocusRightName = buttonRunningTemplate.TexutureFocusRightName;
              texutureNoFocusLeftName = buttonRunningTemplate.TexutureNoFocusLeftName;
              texutureNoFocusMidName = buttonRunningTemplate.TexutureNoFocusMidName;
              texutureNoFocusRightName = buttonRunningTemplate.TexutureNoFocusRightName;
              tileFillTfl = buttonRunningTemplate.TileFillTFL;
              tileFillTnfl = buttonRunningTemplate.TileFillTNFL;
              tileFillTfm = buttonRunningTemplate.TileFillTFM;
              tileFillTnfm = buttonRunningTemplate.TileFillTNFM;
              tileFillTfr = buttonRunningTemplate.TileFillTFR;
              tileFillTnfr = buttonRunningTemplate.TileFillTNFR;
            }
            else if (bRecording && _useNewRecordingButtonColor)
            {
              texutureFocusLeftName = "tvguide_recButton_Focus_left.png";
              texutureFocusMidName = "tvguide_recButton_Focus_middle.png";
              texutureFocusRightName = "tvguide_recButton_Focus_right.png";
              texutureNoFocusLeftName = "tvguide_recButton_noFocus_left.png";
              texutureNoFocusMidName = "tvguide_recButton_noFocus_middle.png";
              texutureNoFocusRightName = "tvguide_recButton_noFocus_right.png";
            }
            else if (!(bRecording && bPartialRecording && _useNewRecordingButtonColor))
            {
              texutureFocusLeftName = "tvguide_button_selected_left.png";
              texutureFocusMidName = "tvguide_button_selected_middle.png";
              texutureFocusRightName = "tvguide_button_selected_right.png";
              texutureNoFocusLeftName = "tvguide_button_left.png";
              texutureNoFocusMidName = "tvguide_button_middle.png";
              texutureNoFocusRightName = "tvguide_button_right.png";
            }
            if (selectCurrentShow && iChannel == _cursorX)
            {
              _cursorY = iProgram + 1;
              _currentProgram = program;
              _startTime = program.Entity.StartTime;
              SetProperties();
            }
          }

          if (bEndsAfter)
          {
            img.RenderRight = true;

            texutureFocusRightName = "tvguide_arrow_selected_right.png";
            texutureNoFocusRightName = "tvguide_arrow_light_right.png";
            if (program.IsRunningAt(dt))
            {
              texutureNoFocusRightName = "tvguide_arrow_right.png";
            }
          }
          if (bStartsBefore)
          {
            img.RenderLeft = true;
            texutureFocusLeftName = "tvguide_arrow_selected_left.png";
            texutureNoFocusLeftName = "tvguide_arrow_light_left.png";
            if (program.IsRunningAt(dt))
            {
              texutureNoFocusLeftName = "tvguide_arrow_left.png";
            }
          }

          img.TexutureFocusLeftName = texutureFocusLeftName;
          img.TexutureFocusMidName = texutureFocusMidName;
          img.TexutureFocusRightName = texutureFocusRightName;
          img.TexutureNoFocusLeftName = texutureNoFocusLeftName;
          img.TexutureNoFocusMidName = texutureNoFocusMidName;
          img.TexutureNoFocusRightName = texutureNoFocusRightName;

          img.TileFillTFL = tileFillTfl;
          img.TileFillTNFL = tileFillTnfl;
          img.TileFillTFM = tileFillTfm;
          img.TileFillTNFM = tileFillTnfm;
          img.TileFillTFR = tileFillTfr;
          img.TileFillTNFR = tileFillTnfr;

          if (isNew)
          {
            img.AllocResources();
            GUIControl cntl = img;
            _controls.Add(iControlId, img);
            Add(ref cntl);
          }
          else
            img.DoUpdate();
          iProgram++;
        }
        iPreviousEndXPos = iEndXPos;
      }
    }

    private void UpdateChannelCount()
    {
      GetChannels(false);
      GUIControl cntlPanel = GetControl((int)Controls.PANEL_BACKGROUND);
      var cntlChannelTemplate = (GUIImage)GetControl((int)Controls.CHANNEL_TEMPLATE);

      int iHeight = cntlPanel.Height + cntlPanel.YPosition - cntlChannelTemplate.YPosition;
      int iItemHeight = cntlChannelTemplate.Height;
      _channelCount = (int)(((float)iHeight) / iItemHeight);

      if (_channelCount > _channelList.Count)
      {
        _channelCount = _channelList.Count;
      }
    }

    private string GetChannelLogo(string strChannel)
    {
      string strLogo = Utils.GetCoverArt(Thumb, strChannel);
      if (string.IsNullOrEmpty(strLogo))
      {
        // Check for a default channel logo.
        strLogo = Utils.GetCoverArt(Thumb, "default");
        if (string.IsNullOrEmpty(strLogo))
        {
          strLogo = DefaultThumb;
        }
      }
      return strLogo;
    }

    private void UpdateHorizontalScrollbar()
    {
      if (_channelList == null)
      {
        return;
      }
      var scrollbar = GetControl((int)Controls.HORZ_SCROLLBAR) as GUIHorizontalScrollbar;
      if (scrollbar != null)
      {
        float percentage = (float)_viewingTime.Hour * 60 + _viewingTime.Minute +
                           _timePerBlock * (_viewingTime.Hour / 24.0f);
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

    /// <summary>
    /// Show or hide group button
    /// </summary>
    private void UpdateGroupButton()
    {
      // text for button
      String groupButtonText = " ";

      // show/hide tvgroup button
      var btnTvGroup = GetControl((int)Controls.CHANNEL_GROUP_BUTTON) as GUIButtonControl;

      if (btnTvGroup != null)
        btnTvGroup.Visible = GroupButtonAvail;

      // set min index for focus handling
      if (GroupButtonAvail)
      {
        _minYIndex = -1; // allow focus of button
        groupButtonText = String.Format("{0}: {1}", GUILocalizeStrings.Get(971), CurrentGroupName);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Group", CurrentGroupName);
      }
      else
      {
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Group", CurrentGroupName);
        _minYIndex = 0;
      }

      // Set proper text for group change button; Empty string to hide text if only 1 group
      // (split between button and rotated label due to focusing issue of rotated buttons)
      GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.ChangeGroup", groupButtonText);
      // existing string "group"
    }

    private int CalcDays()
    {
      int iDay = _viewingTime.DayOfYear - DateTime.Now.DayOfYear;
      if (_viewingTime.Year > DateTime.Now.Year)
      {
        iDay += (new DateTime(DateTime.Now.Year, 12, 31)).DayOfYear;
      }
      return iDay;
    }

    private void UpdateVerticalScrollbar()
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
      float current = (_cursorX + ChannelOffset);
      float total = (float)_channelList.Count - 1;

      if (_singleChannelView)
      {
        current = (_cursorX + _programOffset);
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
      var scrollbar = GetControl((int)Controls.VERT_SCROLLBAR) as GUIVerticalScrollbar;
      if (scrollbar != null)
      {
        scrollbar.Percentage = percentage;
      }
    }

    private void SetGuideHeadingVisibility(bool visible)
    {
      // can't rely on the heading text control having a unique id, so locate it using the localised heading string.
      // todo: update all skins to have a unique id for this control...?
      foreach (GUILabelControl control in controlList.OfType<GUILabelControl>().Where(control => (control).Label == GUILocalizeStrings.Get(4)))
      {
        control.Visible = visible;
      }
    }

    private void SetSingleChannelLabelVisibility(bool visible)
    {
      var channelLabel = GetControl((int)Controls.SINGLE_CHANNEL_LABEL) as GUILabelControl;
      var channelImage = GetControl((int)Controls.SINGLE_CHANNEL_IMAGE) as GUIImage;
      var timeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;

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

    private long GetColorForGenre()
    {
      return Color.White.ToArgb();
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(SettingsGuideSection, "channel", _currentChannel);
        xmlwriter.SetValue(SettingsGuideSection, "ypos", _cursorX.ToString());
        xmlwriter.SetValue(SettingsGuideSection, "yoffset", ChannelOffset.ToString());
        xmlwriter.SetValue(SettingsGuideSection, "timeperblock", _timePerBlock);
      }
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        String channelName = xmlreader.GetValueAsString(SettingsGuideSection, "channel", String.Empty);
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.GetChannelsByName(channelName);
        if (channels != null && channels.Count > 0)
        {
          _currentChannel = channels[0];
        }
        _cursorX = xmlreader.GetValueAsInt(SettingsGuideSection, "ypos", 0);
        ChannelOffset = xmlreader.GetValueAsInt(SettingsGuideSection, "yoffset", 0);
        _byIndex = xmlreader.GetValueAsBool(SettingsSection, "byindex", true);
        _showChannelNumber = xmlreader.GetValueAsBool(SettingsSection, "showchannelnumber", false);
        _channelNumberMaxLength = xmlreader.GetValueAsInt(SettingsSection, "channelnumbermaxlength", 3);
        _timePerBlock = xmlreader.GetValueAsInt(SettingsGuideSection, "timeperblock", 30);
        _hdtvProgramText = xmlreader.GetValueAsString(SettingsSection, "hdtvProgramText", "(HDTV)");
        _guideContinuousScroll = xmlreader.GetValueAsBool(SettingsSection, "continuousScrollGuide", false);
        _loopDelay = xmlreader.GetValueAsInt("gui", "listLoopDelay", 0);
      }
      _useNewRecordingButtonColor =
        Utils.FileExistsInCache(Path.Combine(GUIGraphicsContext.Skin, @"media\tvguide_recButton_Focus_middle.png"));
      _useNewPartialRecordingButtonColor =
        Utils.FileExistsInCache(Path.Combine(GUIGraphicsContext.Skin, @"media\tvguide_partRecButton_Focus_middle.png"));
      _useNewNotifyButtonColor =
        Utils.FileExistsInCache(Path.Combine(GUIGraphicsContext.Skin, @"media\tvguide_notifyButton_Focus_middle.png"));
      _useHdProgramIcon =
        Utils.FileExistsInCache(Path.Combine(GUIGraphicsContext.Skin, @"media\tvguide_hd_program.png"));
    }

    /// <summary>
    /// Calculates how long from current time a program starts or started, set the TimeFromNow property
    /// </summary>
    private static string GetStartTimeFromNow(Program program)
    {
      string timeFromNow = String.Empty;
      if (program.Title == "No TVGuide data available")
      {
        return timeFromNow;
      }
      const string space = " ";
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

    private static string GetDurationAsMinutes(Program program)
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
    /// Calculates the duration of a program and sets the Duration property
    /// </summary>
    private static string GetDuration(Program program)
    {
      if (program.Title == "No TVGuide data available")
      {
        return "";
      }
      const string space = " ";
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

    private void Correct()
    {
      int iControlId;
      if (_cursorY < _minYIndex) // either channel or group button
      {
        _cursorY = _minYIndex;
      }
      if (_cursorY > 0)
      {
        while (_cursorY > 0)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * ROW_ID + (_cursorY - 1) * COL_ID;
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
          iControlId = GUIDE_COMPONENTID_START + _cursorX * ROW_ID + (0) * COL_ID;
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
        var label = GetControl((int)Controls.LABEL_KEYED_CHANNEL) as GUILabelControl;
        if (label != null)
        {
          // Show the keyed channel number.
          label.IsVisible = true;
        }
        else
        {
          label = GetControl((int)Controls.LABEL_TIME1) as GUILabelControl;
        }
        if (label != null) 
        {
          label.Label = _lineInput;

          // Add an underscore "cursor" to visually indicate that more numbers may be entered.
          if (_lineInput.Length < _channelNumberMaxLength)
          {
            label.Label += "_";
          }
        }

        if (_lineInput.Length == _channelNumberMaxLength)
        {
          // change channel
          int iChannel = Int32.Parse(_lineInput);
          ChangeChannelNr(iChannel);

          // Hide the keyed channel number label.
          var labelKeyed = GetControl((int)Controls.LABEL_KEYED_CHANNEL) as GUILabelControl;
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

      int channelDistance = 99999;

      if (_byIndex == false)
      {
        while (iCounter < _channelList.Count && found == false)
        {
          Channel chan = _channelList[iCounter].Channel;
          foreach (TuningDetail detail in chan.TuningDetails)
          {
            if (detail.ChannelNumber == searchChannel)
            {
              iChannelNr = iCounter;
              found = true;
            } //find closest channel number
            else if (Math.Abs(detail.ChannelNumber - searchChannel) < channelDistance)
            {
              channelDistance = Math.Abs(detail.ChannelNumber - searchChannel);
              iChannelNr = iCounter;
            }
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
          // minimum of available channel/max visible channels
        {
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

    private void OnPageUp()
    {
      int steps;
      if (_singleChannelView)
      {
        steps = _channelCount; // all available rows
      }
      else
      {
        if (_guideContinuousScroll)
        {
          steps = _channelCount; // all available rows
        }
        else
        {
          // If we're on the first channel in the guide then allow one step to get back to the end of the guide.
          if (ChannelOffset == 0 && _cursorX == 0)
          {
            steps = 1;
          }
          else
          {
            // only number of additional avail channels
            steps = Math.Min(ChannelOffset + _cursorX, _channelCount);
          }
        }
      }
      UnFocus();
      for (int i = 0; i < steps; ++i)
      {
        OnUp(false, true);
      }
      Correct();
      Update(false);
      SetFocus();
    }

    private void OnPageDown()
    {
      int steps;
      if (_singleChannelView)
        steps = _channelCount; // all available rows
      else
      {
        if (_guideContinuousScroll)
        {
          steps = _channelCount; // all available rows
        }
        else
        {
          // If we're on the last channel in the guide then allow one step to get back to top of guide.
          if (ChannelOffset + (_cursorX + 1) == _channelList.Count)
          {
            steps = 1;
          }
          else
          {
            // only number of additional avail channels
            steps = Math.Min(_channelList.Count - ChannelOffset - _cursorX - 1, _channelCount);
          }
        }
      }

      UnFocus();
      for (int i = 0; i < steps; ++i)
      {
        OnDown(false);
      }
      Correct();
      Update(false);
      SetFocus();
    }

    /// <summary>
    /// Sets the best matching program in new guide row
    /// </summary>
    /// <param name="updateScreen"></param>
    /// <param name="directionIsDown"></param>
    private void SetBestMatchingProgram(bool updateScreen, bool directionIsDown)
    {
      // if cursor is on a program in guide, try to find the "best time matching" program in new channel
      int iCurY = _cursorX;
      int iCurOff = ChannelOffset;
      int iControlId = GUIDE_COMPONENTID_START + _cursorX * ROW_ID + (_cursorY - 1) * COL_ID;
      GUIControl control = GetControl(iControlId);
      if (control == null)
      {
        return;
      }

      bool ok = false;
      int iMaxSearch = _channelList.Count;

      // TODO rewrite the while loop, the code is a little awkward.
      while (!ok && (iMaxSearch > 0))
      {
        iMaxSearch--;
        if (directionIsDown)
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

        for (int x = 1; x < COL_ID; x++)
        {
          iControlId = GUIDE_COMPONENTID_START + _cursorX * ROW_ID + (x - 1) * COL_ID;
          control = GetControl(iControlId);
          if (control != null)
          {
            var prog = (Program)control.Data;

            if (_singleChannelView)
            {
              _cursorY = x;
              ok = true;
              break;
            }

            bool isvalid = false;
            DateTime time = DateTime.Now;
            if (time < prog.EndTime) // present & future
            {
              if (_startTime <= prog.StartTime)
              {
                isvalid = true;
              }
              else if (_startTime >= prog.StartTime && _startTime < prog.EndTime)
              {
                isvalid = true;
              }
              else if (_startTime < time)
              {
                isvalid = true;
              }
            }
              // this one will skip past programs
            else if (time > _currentProgram.Entity.EndTime) // history
            {
              if (prog.EndTime > _startTime)
              {
                isvalid = true;
              }
            }

            if (isvalid)
            {
              _cursorY = x;
              ok = true;
              break;
            }
          }
        }
      }
      if (!ok)
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

    private int ProgramCount(int iChannel)
    {
      int iProgramCount = 0;
      for (int iProgram = 0; iProgram < _numberOfBlocks * 5; ++iProgram)
      {
        int iControlId = GUIDE_COMPONENTID_START + iChannel * ROW_ID + iProgram * COL_ID;
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
        if (_minYIndex == -1)
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
          _startTime = _currentProgram.Entity.StartTime;
        }
        return;
      }
      Correct();
      Update(false);
      SetFocus();
      if (_currentProgram != null)
      {
        _startTime = _currentProgram.Entity.StartTime;
      }
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
          _startTime = _currentProgram.Entity.StartTime;
        }
        return;
      }
      _viewingTime = _viewingTime.AddMinutes(_timePerBlock);
      // Check new day
      int iDay = CalcDays();
      if (iDay >= MAX_DAYS_IN_GUIDE)
      {
        _viewingTime = _viewingTime.AddMinutes(-_timePerBlock);
      }
      Correct();
      Update(false);
      SetFocus();
      if (_currentProgram != null)
      {
        _startTime = _currentProgram.Entity.StartTime;
      }
    }

    private void UpdateSingleChannelNumber()
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
        var img = GetControl(_cursorX + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
        if (null != img)
        {
          _currentChannel = (Channel)img.Data;
        }
      }
    }

    #endregion

    #region overrides

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
              float fPercentage = message.Param1;
              fPercentage /= 100.0f;
              fPercentage *= 24.0f;
              fPercentage *= 60.0f;
              _viewingTime = new DateTime(_viewingTime.Year, _viewingTime.Month, _viewingTime.Day, 0, 0, 0, 0);
              _viewingTime = _viewingTime.AddMinutes((int)fPercentage);
            }

            if (message.SenderControlId == (int)Controls.VERT_SCROLLBAR)
            {
              _needUpdate = true;
              float fPercentage = message.Param1;
              fPercentage /= 100.0f;
              if (_singleChannelView)
              {
                fPercentage *= _totalProgramCount;
                var iChan = (int)fPercentage;
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
                fPercentage *= _channelList.Count;
                var iChan = (int)fPercentage;
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

          case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
            {
              base.OnMessage(message);
              SaveSettings();              
              _controls = new Dictionary<int, GUIButton3PartControl>();
              _channelList = null;
              _recordingList = null;
              _currentProgram = null;
              return true;
            }

          case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
            {
              TVHome.ShowTvEngineSettingsUIIfConnectionDown();

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
              LoadSettings();
              UpdateChannelCount();
              WindowInit(message);
              return true;
            }

          case GUIMessage.MessageType.GUI_MSG_CLICKED:
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.SPINCONTROL_DAY)
            {
              var cntlDay = GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;
              if (cntlDay != null) 
              {
                int iDay = cntlDay.Value;

                _viewingTime = DateTime.Now;
                _viewingTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _viewingTime.Hour,
                                            _viewingTime.Minute, 0, 0);
                _viewingTime = _viewingTime.AddDays(iDay);
              }
              _recalculateProgramOffset = true;
              Update(false);
              SetFocus();
              return true;
            }
            if (iControl == (int)Controls.SPINCONTROL_TIME_INTERVAL)
            {
              var cntlTimeInt = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
              if (cntlTimeInt != null) 
              {
                int iInterval = (cntlTimeInt.Value) + 1;
                if (iInterval > 4)
                {
                  iInterval = 4;
                }
                _timePerBlock = iInterval * 15;
              }
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
              OnSelectItem(true);
              Update(false);
              SetFocus();
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
        Log.Debug("GuideBase: {0}", ex);
      }
      return base.OnMessage(message);
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
          GUIWindowManager.ShowPreviousWindow();
          return;

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
            var x = (int)action.fAmount1;
            var y = (int)action.fAmount2;
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
                    UpdateSingleChannelNumber();
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
                      int iCursorY = (iControlId / ROW_ID);
                      iControlId -= iCursorY * ROW_ID;
                      if (iControlId % COL_ID == 0)
                      {
                        int iCursorX = (iControlId / COL_ID) + 1;
                        if (iCursorY != _cursorX || iCursorX != _cursorY)
                        {
                          UnFocus();
                          _cursorX = iCursorY;
                          _cursorY = iCursorX;
                          UpdateCurrentProgram();
                          SetFocus();
                          UpdateHorizontalScrollbar();
                          UpdateVerticalScrollbar();
                          UpdateSingleChannelNumber();
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
              ShowContextMenu();
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
          UpdateSingleChannelNumber();
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          UpdateSingleChannelNumber();
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (_cursorX >= 0)
            {
              OnLeft();
              UpdateSingleChannelNumber();
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
              UpdateSingleChannelNumber();
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
              UpdateSingleChannelNumber();
              UpdateVerticalScrollbar();
            }
            else
            {
              _cursorX = 0;
              SetFocus();
              UpdateSingleChannelNumber();
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
            var cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            if (cntlTimeInterval != null)
            {
              cntlTimeInterval.Value = (_timePerBlock / 15) - 1;
            }
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
            var cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            if (cntlTimeInterval != null)
            {
              cntlTimeInterval.Value = (_timePerBlock / 15) - 1;
            }
            Update(false);
            SetFocus();
          }
          break;
        case Action.ActionType.ACTION_DEFAULT_TIMEBLOCK:
          {
            _timePerBlock = 30;
            var cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
            if (cntlTimeInterval != null)
            {
              cntlTimeInterval.Value = (_timePerBlock / 15) - 1;
            }
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

    public override void Render(float timePassed)
    {
      lock (this)
      {
        var vertLine = GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
        base.Render(timePassed);
        if (vertLine != null)
        {
          vertLine.Render(timePassed);
        }
      }
    }

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
      return -1;
    }

    #endregion

    #region abstract methods

    protected abstract string Thumb { get; }
    protected abstract string DefaultThumb { get; }
    protected abstract int ChannelGroupCount { get; }
    protected abstract string CurrentGroupName { get; }
    protected abstract string SkinPropertyPrefix { get; }
    protected abstract string SettingsGuideSection { get; }
    protected abstract string SettingsSection { get; }
    protected abstract bool HasSelectedGroup();
    protected abstract bool IsChannelTypeCorrect(Channel channel);
    protected abstract IList<Channel> GetGuideChannelsForGroup();
    protected abstract void OnRecord();
    protected abstract void OnSelectItem(bool isItemSelected);
    protected abstract void ShowContextMenu();
    protected abstract void WindowInit(GUIMessage message);
    protected abstract void OnSelectChannelGroup();
    protected abstract void OnChangeChannelGroup(int direction);

    #endregion

    #region Nested type: GuideChannel

    protected struct GuideChannel
    {
      public Channel Channel;
      public int ChannelNum;
      public string StrLogo;
    }

    #endregion

    #region enums

    protected enum Controls
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
      SINGLE_CHANNEL_IMAGE = 33,
      LABEL_KEYED_CHANNEL = 34,
      BUTTON_PROGRAM_RUNNING = 35,
      BUTTON_PROGRAM_NOT_RUNNING = 36,
      BUTTON_PROGRAM_NOTIFY = 37,
      BUTTON_PROGRAM_RECORD = 38,
      BUTTON_PROGRAM_PARTIAL_RECORD = 39,

      CHANNEL_GROUP_BUTTON = 100
    };

    #endregion
  }
}
