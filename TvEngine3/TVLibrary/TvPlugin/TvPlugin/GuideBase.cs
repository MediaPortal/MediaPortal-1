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
using System.Globalization;
using System.Linq;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using TvDatabase;

namespace TvPlugin
{
  public abstract class GuideBase : GUIDialogWindow
  {
    protected const int MaxDaysInGuide = 30;
    protected const int RowID = 1000;
    protected const int ColID = 10;
    protected const int GUIDE_COMPONENTID_START = 50000;
    private int _channelOffset = 0;
    protected DateTime _viewingTime = DateTime.Now;
    protected DateTime _updateTimer = DateTime.Now;
    protected GUILabelControl _titleDarkTemplate;
    protected GUILabelControl _titleTemplate;
    protected GUILabelControl _genreDarkTemplate;
    protected GUILabelControl _genreTemplate;
    protected GUIButton3PartControl _programPartialRecordTemplate;
    protected GUIButton3PartControl _programRecordTemplate;
    protected GUIButton3PartControl _programNotifyTemplate;
    protected GUIButton3PartControl _programNotRunningTemplate;
    protected GUIButton3PartControl _programRunningTemplate;
    protected bool _showChannelLogos = false;
    protected int _timePerBlock = 30; // steps of 30 minutes
    protected bool _singleChannelView = false;
    protected int _channelCount = 5;
    protected List<GuideChannel> _channelList = new List<GuideChannel>();
    protected int _singleChannelNumber = 0;
    protected int _cursorY = 0;
    protected int _cursorX = 0;
    protected Channel _currentChannel = null;
    protected int _numberOfBlocks = 4;

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
        if (GUIWindowManager.ActiveWindowEx != this.GetID)
        {
          return;
        }

        // sets button visible state
        UpdateGroupButton();

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
          cntlChannelImg.IsVisible = false;
        }
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
            img = new GUIImage(GetID, (int)Controls.IMG_TIME1 + iLabel, xpos, ypos, iLabelWidth - 4,
                               cntlHeaderBkgImg.RenderHeight, cntlHeaderBkgImg.FileName, 0x0);
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
            label = new GUILabelControl(GetID, (int)Controls.LABEL_TIME1 + iLabel, xpos, ypos, iLabelWidth,
                                        cntlHeaderBkgImg.RenderHeight, labelTime.FontName, String.Empty,
                                        labelTime.TextColor, labelTime.TextAlignment, labelTime.TextVAlignment, false,
                                        labelTime.ShadowAngle, labelTime.ShadowDistance, labelTime.ShadowColor);
            label.AllocResources();
            GUIControl cntl = (GUIControl)label;
            this.Add(ref cntl);
          }
          iHour = dt.Hour;
          iMin = dt.Minute;
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

          //this.Remove((int)Controls.IMG_CHAN1+iChan);
          GUIButton3PartControl imgBut = GetControl((int)Controls.IMG_CHAN1 + iChan) as GUIButton3PartControl;
          if (imgBut == null)
          {
            string strChannelImageFileName = String.Empty;
            if (_showChannelLogos)
            {
              strChannelImageFileName = cntlChannelImg.FileName;
            }

            // Use a template control if it exists, otherwise use default values.
            GUIButton3PartControl buttonTemplate = GetControl((int)Controls.BUTTON_PROGRAM_NOT_RUNNING) as GUIButton3PartControl;
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
                                                 strChannelImageFileName);

              imgBut.TileFillTFL = buttonTemplate.TileFillTFL;
              imgBut.TileFillTNFL = buttonTemplate.TileFillTNFL;
              imgBut.TileFillTFM = buttonTemplate.TileFillTFM;
              imgBut.TileFillTNFM = buttonTemplate.TileFillTNFM;
              imgBut.TileFillTFR = buttonTemplate.TileFillTFR;
              imgBut.TileFillTNFR = buttonTemplate.TileFillTNFR;
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
            GUIControl cntl = (GUIControl)imgBut;
            Add(ref cntl);
          }

          imgBut.Width = cntlChannelTemplate.Width - 2; //labelTime.XPosition-cntlChannelImg.XPosition;
          imgBut.Height = cntlChannelTemplate.Height - 2; //iItemHeight-2;
          imgBut.SetPosition(xpos, ypos);
          imgBut.FontName1 = cntlChannelLabel.FontName;
          imgBut.TextColor1 = cntlChannelLabel.TextColor;
          imgBut.Label1 = String.Empty;
          imgBut.RenderLeft = false;
          imgBut.RenderRight = false;
          imgBut.SetShadow1(cntlChannelLabel.ShadowAngle, cntlChannelLabel.ShadowDistance, cntlChannelLabel.ShadowColor);

          if (_showChannelLogos)
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

        //day = String.Format("{0} {1}-{2}", day, _viewingTime.Day, _viewingTime.Month);
        day = Utils.GetShortDayString(_viewingTime);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Day", day);

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


        LoadSchedules(false);

        if (ChannelOffset > _channelList.Count)
        {
          ChannelOffset = 0;
          _cursorX = 0;
        }

        for (int i = 0; i < controlList.Count; ++i)
        {
          GUIControl cntl = (GUIControl)controlList[i];
          if (cntl.GetID >= RadioGuideBase.GUIDE_COMPONENTID_START)
          {
            cntl.IsVisible = false;
          }
        }

        if (_singleChannelView)
        {
          // show all buttons (could be less visible if channels < rows)
          for (int iChannel = 0; iChannel < _channelCount; iChannel++)
          {
            GUIButton3PartControl imgBut = GetControl((int)Controls.IMG_CHAN1 + iChannel) as GUIButton3PartControl;
            if (imgBut != null)
              imgBut.IsVisible = true;
          }

          Channel channel = (Channel)_channelList[_singleChannelNumber].channel;
          setGuideHeadingVisibility(false);
          RenderSingleChannel(channel);
        }
        else
        {
          TvBusinessLayer layer = new TvBusinessLayer();

          List<Channel> visibleChannels = new List<Channel>();

          int chan = ChannelOffset;
          for (int iChannel = 0; iChannel < _channelCount; iChannel++)
          {
            if (chan >= 0 && chan < _channelList.Count)
            {
              visibleChannels.Add(_channelList[chan].channel);
            }
            chan++;
            if (chan >= _channelList.Count && visibleChannels.Count < _channelList.Count)
            {
              chan = 0;
            }
          }
          Dictionary<int, List<Program>> programs = layer.GetProgramsForAllChannels(Utils.longtodate(iStart),
                                                                                    Utils.longtodate(iEnd),
                                                                                    visibleChannels);
          // make sure the TV Guide heading is visiable and the single channel labels are not.
          setGuideHeadingVisibility(true);
          setSingleChannelLabelVisibility(false);
          chan = ChannelOffset;

          int firstButtonYPos = 0;
          int lastButtonYPos = 0;

          for (int iChannel = 0; iChannel < _channelCount; iChannel++)
          {
            if (chan >= 0 && chan < _channelList.Count)
            {
              GuideChannel tvGuideChannel = (GuideChannel)_channelList[chan];
              RenderChannel(ref programs, iChannel, tvGuideChannel, iStart, iEnd, selectCurrentShow);
              // remember bottom y position from last visible button
              GUIButton3PartControl imgBut = GetControl((int)Controls.IMG_CHAN1 + iChannel) as GUIButton3PartControl;
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
              GUIButton3PartControl imgBut = GetControl((int)Controls.IMG_CHAN1 + iChannel) as GUIButton3PartControl;
              if (imgBut != null)
              {
                imgBut.IsVisible = false;
              }
            }
          }

          GUIImage vertLine = GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
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
          GUIButton3PartControl img = GetControl(_cursorX + (int)Controls.IMG_CHAN1) as GUIButton3PartControl;
          if (null != img)
          {
            _currentChannel = (Channel)img.Data;
          }
        }
        UpdateVerticalScrollbar();
      }
    }

    protected abstract void UpdateHorizontalScrollbar();

    protected abstract void GetChannels(bool b);

    protected abstract void LoadSchedules(bool b);

    protected abstract void RenderSingleChannel(Channel channel);

    protected abstract void setSingleChannelLabelVisibility(bool b);

    protected abstract void setGuideHeadingVisibility(bool p0);

    protected abstract void RenderChannel(ref Dictionary<int, List<Program>> programs, int iChannel, GuideChannel tvGuideChannel, long iStart, long iEnd, bool selectCurrentShow);

    protected abstract void UpdateVerticalScrollbar();

    protected abstract int CalcDays();

    protected abstract void UpdateGroupButton();

    protected class GuideChannel
    {
      public Channel channel;
      public int channelNum;
      public string strLogo;
    }

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
    } ;

    #endregion

    protected abstract string SkinPropertyPrefix { get; }

    protected void UpdateChannelCount()
    {
      GetChannels(false);
      GUIControl cntlPanel = GetControl((int)Controls.PANEL_BACKGROUND);
      GUIImage cntlChannelTemplate = (GUIImage)GetControl((int)Controls.CHANNEL_TEMPLATE);

      int iHeight = cntlPanel.Height + cntlPanel.YPosition - cntlChannelTemplate.YPosition;
      int iItemHeight = cntlChannelTemplate.Height;
      _channelCount = (int)(((float)iHeight) / iItemHeight);      

      if (_channelCount > _channelList.Count())
      {
        _channelCount = _channelList.Count();
      }
    }
  }
}
