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
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.TvPlugin.Helper;

#endregion

namespace Mediaportal.TV.TvPlugin.EPG
{
  /// <summary>
  /// 
  /// </summary>
  public class RadioGuideBase : GuideBase
  {
    #region constants

    private const string SKIN_PROPERTY_PREFIX = "#Radio";
    private const string SETTINGS_GUIDE_SECTION = "radioguide";
    private const string SETTINGS_SECTION = "myradio";

    #endregion

    #region variables

    private Channel _recordingExpected;

    #endregion

    #region property overrides

    protected override string SkinPropertyPrefix
    {
      get { return SKIN_PROPERTY_PREFIX; }
    }

    protected override string SettingsGuideSection
    {
      get { return SETTINGS_GUIDE_SECTION; }
    }

    protected override string SettingsSection
    {
      get { return SETTINGS_SECTION; }
    }

    protected override int ChannelGroupCount
    {
      get { return Radio.Radio.AllRadioGroups.Count; }
    }

    protected override string Thumb
    {
      get { return Thumbs.Radio; }
    }

    protected override string DefaultThumb
    {
      get { return "defaultMyRadioBig.png"; }
    }

    protected override string CurrentGroupName
    {
      get { return Radio.Radio.SelectedGroup.GroupName; }
    }

    #endregion

    #region overrides

    /// <summary>
    /// changes the current channel group and refreshes guide display
    /// </summary>
    /// <param name="direction"></param>
    protected override void OnChangeChannelGroup(int direction)
    {
      // in single channel view there would be errors when changing group
      if (_singleChannelView) return;
      int countGroups = Radio.Radio.AllRadioGroups.Count; // all
      int newIndex = 0;
      int oldIndex = 0;

      for (int i = 0; i < countGroups; ++i)
      {
        if (Radio.Radio.AllRadioGroups[i].IdGroup == Radio.Radio.SelectedGroup.IdGroup)
        {
          newIndex = oldIndex = i;
          break;
        }
      }

      if (
        (newIndex >= 1 && direction < 0) ||
        (newIndex < countGroups - 1 && direction > 0)
        )
      {
        newIndex += direction; // change group
      }
      else // Cycle handling
        if ((newIndex == countGroups - 1) && direction > 0)
        {
          newIndex = 0;
        }
        else if (newIndex == 0 && direction < 0)
        {
          newIndex = countGroups - 1;
        }

      if (oldIndex != newIndex)
      {
        // update list
        GUIWaitCursor.Show();
        Radio.Radio.SelectedGroup = Radio.Radio.AllRadioGroups[newIndex];
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Group", Radio.Radio.SelectedGroup.GroupName);

        _cursorY = 1; // cursor should be on the program guide item
        ChannelOffset = 0;
        // reset to top; otherwise focus could be out of screen if new group has less then old position
        _cursorX = 0; // first channel
        GetChannels(true);
        Update(false);
        SetFocus();
        GUIWaitCursor.Hide();
      }
    }

    protected override void WindowInit(GUIMessage message)
    {
      UnFocus();
      GetChannels(true);
      LoadSchedules(true);
      _currentProgram = null;
      if (message.Param1 != (int)Window.WINDOW_TV_PROGRAM_INFO)
      {
        _viewingTime = DateTime.Now;
        _cursorY = 0;
        _cursorX = 0;
        ChannelOffset = 0;
        _singleChannelView = false;
        _showChannelLogos = false;
        if (TVHome.Card.IsTimeShifting)
        {
          _currentChannel = Radio.Radio.CurrentChannel;
          for (int i = 0; i < _channelList.Count; i++)
          {
            Channel chan = (_channelList[i]).Channel;
            if (chan.IdChannel == _currentChannel.IdChannel)
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
        ChannelOffset += _channelCount;
      }
      // Mantis 3579: the above lines can lead to too large channeloffset. 
      // Now we check if the offset is too large, and if it is, we reduce it and increase the cursor position accordingly
      if (!_guideContinuousScroll && (ChannelOffset > _channelList.Count - _channelCount))
      {
        _cursorX += ChannelOffset - (_channelList.Count - _channelCount);
        ChannelOffset = _channelList.Count - _channelCount;
      }
      var cntlDay = GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;
      if (cntlDay != null)
      {
        DateTime dtNow = DateTime.Now;
        cntlDay.Reset();
        cntlDay.SetRange(0, MAX_DAYS_IN_GUIDE - 1);
        for (int iDay = 0; iDay < MAX_DAYS_IN_GUIDE; iDay++)
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
        Log.Debug("RadioGuideBase: SpinControl cntlDay is null!");
      }

      var cntlTimeInterval = GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
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
        Log.Debug("RadioGuideBase: SpinControl cntlTimeInterval is null!");
      }

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
        _startTime = _currentProgram.Entity.StartTime;
      }
      UpdateCurrentProgram();
    }

    /// <summary>
    /// Shows channel group selection dialog
    /// </summary>
    protected override void OnSelectChannelGroup()
    {
      // only if more groups present and not in singleChannelView
      if (Radio.Radio.AllRadioGroups.Count > 1 && !_singleChannelView)
      {
        int prevGroup = Radio.Radio.SelectedGroup.IdGroup;

        var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(971); // group
        int selected = 0;

        for (int i = 0; i < Radio.Radio.AllRadioGroups.Count; ++i)
        {
          dlg.Add(Radio.Radio.AllRadioGroups[i].GroupName);
          if (Radio.Radio.AllRadioGroups[i].GroupName == Radio.Radio.SelectedGroup.GroupName)
          {
            selected = i;
          }
        }

        dlg.SelectedLabel = selected;
        dlg.DoModal(GUIWindowManager.ActiveWindow);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }

        Radio.Radio.SelectedGroup = Radio.Radio.AllRadioGroups[dlg.SelectedId - 1];
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Guide.Group", dlg.SelectedLabelText);

        if (prevGroup != Radio.Radio.SelectedGroup.IdGroup)
        {
          GUIWaitCursor.Show();
          _cursorY = 1; // cursor should be on the program guide item
          ChannelOffset = 0;
          // reset to top; otherwise focus could be out of screen if new group has less then old position
          _cursorX = 0; // set to top, otherwise index could be out of range in new group

          // group has been changed
          GetChannels(true);
          Update(false);

          SetFocus();
          GUIWaitCursor.Hide();
        }
      }
    }

    public override void Process()
    {
      OnKeyTimeout();

      //if we did a manual rec. on the tvguide directly, then we have to wait for it to start and the update the GUI.
      if (_recordingExpected != null)
      {
        TimeSpan ts = DateTime.Now - _updateTimerRecExpected;
        if (ts.TotalMilliseconds > 1000)
        {
          _updateTimerRecExpected = DateTime.Now;
          IVirtualCard card;
          if (ServiceAgents.Instance.ControllerServiceAgent.IsRecording(_recordingExpected.IdChannel, out card))
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

      var vertLine = GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
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
            GUIControl guiControl = GetControl((int)Controls.LABEL_TIME1 + 1);
            int iWidth = 1;
            if (guiControl != null)
            {
              iWidth = guiControl.XPosition - iStartX;
            }

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
            float fpos = (iCurTime) / ((float)(iTimeWidth));
            fpos *= iWidth;
            fpos += iStartX;
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

    protected override void ShowContextMenu()
    {
      var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(924)); //Menu

        if (_currentChannel != null)
        {
          dlg.AddLocalizedString(1213); // Listen to this Station
        }

        if (_currentProgram.Entity.IdProgram != 0)
        {
          dlg.AddLocalizedString(1041); //Upcoming episodes
        }

        if (_currentProgram != null && _currentProgram.Entity.StartTime > DateTime.Now)
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

        if (_currentProgram != null && _currentChannel != null && _currentTitle.Length > 0)
        {
          if (_currentProgram.Entity.IdProgram == 0) // no EPG program recording., only allow to stop it.
          {
            bool isRecordingNoEPG = IsRecordingNoEPG(_currentProgram.Entity.Channel);
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

        if (Radio.Radio.AllRadioGroups.Count > 1)
        {
          dlg.AddLocalizedString(971); // Group
        }

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedId)
        {
          case 1041:
            ShowProgramInfo();
            Log.Debug("RadioGuide: show episodes or repeatings for current show");
            break;
          case 971: //group
            OnSelectChannelGroup();
            break;
          case 1040: // set reminder
          case 1212: // cancel reminder
            OnNotify();
            break;

          case 1213: // listen to station

            Log.Debug("viewch channel:{0}", _currentChannel);
            Radio.Radio.Play();
            if (_currentProgram != null && (TVHome.Card.IsTimeShifting && TVHome.Card.IdChannel == _currentProgram.Entity.IdChannel))
            {
              g_Player.ShowFullScreenWindow();
            }
            return;


          case 939: // switch mode
            OnSwitchMode();
            break;
          case 629: //stop recording
            if (_currentProgram != null)
            {
              Schedule schedule = ServiceAgents.Instance.ScheduleServiceAgent.GetScheduleWithNoEPG(_currentProgram.Entity.IdChannel);              
              TVUtil.DeleteRecAndEntireSchedWithPrompt(schedule);
            }
            Update(true); //remove RED marker
            break;

          case 637: // edit recording
          case 264: // record
            if (_currentProgram != null && _currentProgram.Entity.IdProgram == 0)
            {
              TVHome.StartRecordingSchedule(new ChannelBLL(_currentProgram.Entity.Channel), true);
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

    protected override void OnSelectItem(bool isItemSelected)
    {
      if (_currentProgram == null)
      {
        return;
      }
      Radio.Radio.CurrentChannel = _currentChannel;
      if (isItemSelected)
      {
        if (_currentProgram.IsRunningAt(DateTime.Now) || _currentProgram.Entity.EndTime <= DateTime.Now)
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
              rec = ServiceAgents.Instance.RecordingServiceAgent.GetActiveRecordingByTitleAndChannel(_currentProgram.Entity.Title, _currentProgram.Entity.IdChannel);
            }


            if (rec != null)
            {
              fileName = rec.FileName;
            }

            if (!string.IsNullOrEmpty(fileName)) //are we really recording ?
            {
              Log.Info("RadioGuide: clicked on a currently running recording");
              var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
              if (dlg == null)
              {
                return;
              }

              dlg.Reset();
              dlg.SetHeading(_currentProgram.Entity.Title);
              dlg.AddLocalizedString(979); //Play recording from beginning
              dlg.AddLocalizedString(1213); //Listen to this station
              dlg.DoModal(GetID);

              if (dlg.SelectedLabel == -1)
              {
                return;
              }
              if (_recordingList != null)
              {
                Log.Debug("RadioGuide: Found current program {0} in recording list", _currentTitle);
                switch (dlg.SelectedId)
                {
                  case 979: // Play recording from beginning
                    {                      
                      Recording recDB = ServiceAgents.Instance.RecordingServiceAgent.GetRecordingByFileName(fileName);
                      if (recDB != null)
                      {
                        GUIPropertyManager.RemovePlayerProperties();
                        GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", recDB.Description);
                        GUIPropertyManager.SetProperty("#Play.Current.Album", recDB.Channel.DisplayName);
                        GUIPropertyManager.SetProperty("#Play.Current.Title", recDB.Description);

                        string strLogo = Utils.GetCoverArt(Thumbs.Radio, recDB.Channel.DisplayName);
                        if (string.IsNullOrEmpty(strLogo))
                        {
                          strLogo = "defaultMyRadioBig.png";
                        }

                        GUIPropertyManager.SetProperty("#Play.Current.Thumb", strLogo);
                        TVUtil.PlayRecording(recDB, 0, g_Player.MediaType.Radio);
                      }
                    }
                    return;

                  case 1213: // listen to this station
                    {
                      Radio.Radio.Play();
                      if (g_Player.Playing)
                      {
                        g_Player.ShowFullScreenWindow();
                      }
                    }
                    return;
                }
              }
              else
              {
                Log.Info("EPG: _recordingList was not available");
              }


              if (string.IsNullOrEmpty(fileName))
              {
                Radio.Radio.Play();
                if (g_Player.Playing)
                {
                  g_Player.ShowFullScreenWindow();
                }
              }
            }
            else //not recording
            {
              // clicked the show we're currently watching
              if (Radio.Radio.CurrentChannel != null && Radio.Radio.CurrentChannel.IdChannel == _currentChannel.IdChannel &&
                  g_Player.Playing)
              {
                Log.Debug("RadioGuide: clicked on a currently running show");
                var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
                if (dlg == null)
                {
                  return;
                }

                dlg.Reset();
                dlg.SetHeading(_currentProgram.Entity.Title);
                dlg.AddLocalizedString(1213); //Listen to this Station
                dlg.AddLocalizedString(1041); //Upcoming episodes
                dlg.DoModal(GetID);

                if (dlg.SelectedLabel == -1)
                {
                  return;
                }

                switch (dlg.SelectedId)
                {
                  case 1041:
                    ShowProgramInfo();
                    Log.Debug("RadioGuide: show episodes or repeatings for current show");
                    break;
                  case 1213:
                    Log.Debug("RadioGuide: switch currently running show to fullscreen");
                    GUIWaitCursor.Show();
                    Radio.Radio.Play();
                    GUIWaitCursor.Hide();
                    if (g_Player.Playing)
                    {
                      g_Player.ShowFullScreenWindow();
                    }
                    else
                    {
                      Log.Debug("RadioGuide: no show currently running to switch to fullscreen");
                    }
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
                Radio.Radio.Play();
                GUIWaitCursor.Hide();
                if (g_Player.Playing)
                {
                  if (isPlayingTV) GUIWindowManager.CloseCurrentWindow();
                  g_Player.ShowFullScreenWindow();
                }
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

          return;
        }
        ShowProgramInfo();
        return;
      }
      ShowProgramInfo();
    }

    /// <summary>
    /// "Record" via REC button
    /// </summary>
    protected override void OnRecord()
    {
      if (_currentProgram == null)
      {
        return;
      }
      if ((_currentProgram.IsRunningAt(DateTime.Now) ||
           (_currentProgram.Entity.EndTime <= DateTime.Now)))
      {
        //record current programme
        GUIWindow tvHome = GUIWindowManager.GetWindow((int)Window.WINDOW_TV);
        if ((tvHome != null) && (tvHome.GetID != GUIWindowManager.ActiveWindow))
        {
          bool didRecStart = TVHome.ManualRecord(new ChannelBLL(_currentProgram.Entity.Channel), GetID);
          //refresh view.
          if (didRecStart)
          {
            _recordingExpected = _currentProgram.Entity.Channel;
          }
        }
      }
      else
      {
        ShowProgramInfo();
      }
    }

    protected override IList<Channel> GetGuideChannelsForGroup()
    {      
      return ServiceAgents.Instance.ChannelServiceAgent.GetAllChannelsByGroupIdAndMediaType(Radio.Radio.SelectedGroup.IdGroup, MediaTypeEnum.Radio).ToList();      
    }

    protected override bool HasSelectedGroup()
    {
      return (Radio.Radio.SelectedGroup != null);
    }

    protected override bool IsChannelTypeCorrect(Channel channel)
    {
      return (channel.MediaType == (int)MediaTypeEnum.Radio);
    }

    #endregion
  }
}