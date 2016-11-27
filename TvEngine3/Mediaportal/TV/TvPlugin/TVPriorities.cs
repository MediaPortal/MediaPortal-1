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
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.TvPlugin.Helper;
using Action = MediaPortal.GUI.Library.Action;

namespace Mediaportal.TV.TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvPriorities : GUIInternalWindow
  {
    #region variables, ctor/dtor

    [SkinControl(10)] protected GUIListControl listPriorities = null;

    private int m_iSelectedItem = 0;
    private TVUtil util = null;

    public TvPriorities()
    {
      GetID = (int)Window.WINDOW_TV_SCHEDULER_PRIORITIES;
    }

    ~TvPriorities() { }

    #endregion

    public override bool IsTv
    {
      get { return true; }
    }

    #region overrides

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvpriorities.xml");
      return bResult;
    }

    public override void OnAction(Action action)
    {
      /*switch (action.wID)
      {
      }*/
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if (util == null)
      {
        util = new TVUtil(31);
      }

      LoadDirectory();

      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, listPriorities.GetID, m_iSelectedItem);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      m_iSelectedItem = GetSelectedItemNo();

      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (TVHome.Card.IsTimeShifting && !(TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            //Recorder.StopViewing();
          }
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == listPriorities)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
      }
    }

    protected override void OnClickedUp(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listPriorities)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        OnMoveUp(iItem);
      }
    }

    protected override void OnClickedDown(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listPriorities)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        OnMoveDown(iItem);
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          UpdateDescription();
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      OnClick(GetSelectedItemNo());
    }

    #endregion

    #region list management

    private GUIListItem GetSelectedItem()
    {
      return listPriorities.SelectedListItem;
    }

    private GUIListItem GetItem(int index)
    {
      if (index < 0 || index >= listPriorities.Count)
      {
        return null;
      }
      return listPriorities[index];
    }

    private int GetSelectedItemNo()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, listPriorities.GetID, 0, 0,
                                      null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }

    private int GetItemCount()
    {
      return listPriorities.Count;
    }

    #endregion

    #region scheduled tv methods

    private void LoadDirectory()
    {
      GUIControl.ClearControl(GetID, listPriorities.GetID);

      IList<ScheduleBLL> itemlist = new List<ScheduleBLL>();
      foreach (var schedule in ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules().OrderBy(s => s.Priority))
      {
        var scheduleBll = new ScheduleBLL(schedule);
        itemlist.Add(scheduleBll);
      }

      int total = 0;
      foreach (ScheduleBLL rec in itemlist)
      {
        if (rec.IsSerieIsCanceled(rec.Entity.StartTime, rec.Entity.IdChannel))
        {
          continue;
        }
        GUIListItem item = new GUIListItem();
        item.Label = String.Format("{0}.{1}", total, rec.Entity.ProgramName);
        item.TVTag = rec;
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Entity.Channel.DisplayName);
        if (string.IsNullOrEmpty(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }

        IVirtualCard card;
        if (ServiceAgents.Instance.ControllerServiceAgent.IsRecordingSchedule(rec.Entity.IdSchedule, out card))
        {
          if (rec.Entity.ScheduleType != (int)ScheduleRecordingType.Once)
          {
            item.PinImage = Thumbs.TvRecordingSeriesIcon;
          }
          else
          {
            item.PinImage = Thumbs.TvRecordingIcon;
          }
        }
        else if (rec.Entity.Conflicts.Count > 0)
        {
          item.PinImage = Thumbs.TvConflictRecordingIcon;
        }
        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        listPriorities.Add(item);
        total++;
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(total));

      GUIControl.SelectItemControl(GetID, listPriorities.GetID, m_iSelectedItem);
    }

    private void OnClick(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem item = GetItem(iItem);
      if (item == null)
      {
        return;
      }
      Schedule rec = (Schedule)item.TVTag;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(rec.ProgramName);

      if (rec.Series == false)
      {
        dlg.AddLocalizedString(618); //delete
      }
      else
      {
        dlg.AddLocalizedString(981); //Delete this recording
        dlg.AddLocalizedString(982); //Delete series recording
        dlg.AddLocalizedString(888); //Episodes management
      }
      IVirtualCard card;

      if (ServiceAgents.Instance.ControllerServiceAgent.IsRecordingSchedule(rec.IdSchedule, out card))
      {
        dlg.AddLocalizedString(979); //Play recording from beginning
        dlg.AddLocalizedString(980); //Play recording from live point
      }
      else
      {
        IList<TuningDetail> details = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(rec.IdChannel).TuningDetails;
        foreach (TuningDetail detail in details)
        {
          if (detail.ChannelType == 0)
          {
            dlg.AddLocalizedString(882); //Quality settings
            break;
          }
        }
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 888: ////Episodes management
          OnSetEpisodesToKeep(rec);
          break;
        case 882:
          OnSetQuality(rec);
          break;

        case 981: //Delete this recording only
          {
            if (ServiceAgents.Instance.ControllerServiceAgent.IsRecordingSchedule(rec.IdSchedule, out card))
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
              if (null != dlgYesNo)
              {
                dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
                dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
                dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                if (dlgYesNo.IsConfirmed)
                {
                  ServiceAgents.Instance.ControllerServiceAgent.StopRecordingSchedule(rec.IdSchedule);
                  ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(rec);
                  ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
                }
              }
            }
            else
            {
              ServiceAgents.Instance.ControllerServiceAgent.StopRecordingSchedule(rec.IdSchedule);
              ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(rec);
              ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
            }
            LoadDirectory();
          }
          break;

        case 982: //Delete series recording
          goto case 618;

        case 618: // delete entire recording
          {
            if (ServiceAgents.Instance.ControllerServiceAgent.IsRecordingSchedule(rec.IdSchedule, out card))
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
              if (null != dlgYesNo)
              {
                dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
                dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
                dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                if (dlgYesNo.IsConfirmed)
                {
                  ServiceAgents.Instance.ControllerServiceAgent.StopRecordingSchedule(rec.IdSchedule);
                  ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(rec.IdSchedule);
                  ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
                }
              }
            }
            else
            {
              ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(rec.IdSchedule);
              ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
            }
            LoadDirectory();
          }
          break;

        case 979: // Play recording from beginning
          if (g_Player.Playing && g_Player.IsTVRecording)
          {
            g_Player.Stop(true);
          }
          //TVHome.IsTVOn = true;
          TVHome.ViewChannel(rec.Channel);
          g_Player.SeekAbsolute(0);
          if (TVHome.Card.IsTimeShifting)
          {
            g_Player.ShowFullScreenWindow();
            return;
          }
          break;

        case 980: // Play recording from live point
          //TVHome.IsTVOn = true;
          TVHome.ViewChannel(rec.Channel);
          if (TVHome.Card.IsTimeShifting)
          {
            if (g_Player.Playing)
            {
              g_Player.SeekAsolutePercentage(99);
            }
            g_Player.ShowFullScreenWindow();
            return;
          }
          break;
      }
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, listPriorities.GetID, m_iSelectedItem);
    }

    private void ChangeType(Schedule rec)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616)); //616=Select Recording type
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
        for (int i = 611; i <= 615; ++i)
        {
          dlg.Add(GUILocalizeStrings.Get(i));
        }
        dlg.Add(GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.Record_WorkingDays)));
        dlg.Add(GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.Record_WeekendDays)));
        dlg.Add(GUILocalizeStrings.Get(990000));// 990000=Weekly everytime on this channel		
        switch ((ScheduleRecordingType)rec.ScheduleType)
        {
          case ScheduleRecordingType.Once:
            dlg.SelectedLabel = 0;
            break;
          case ScheduleRecordingType.EveryTimeOnThisChannel:
            dlg.SelectedLabel = 1;
            break;
          case ScheduleRecordingType.EveryTimeOnEveryChannel:
            dlg.SelectedLabel = 2;
            break;
          case ScheduleRecordingType.Weekly:
            dlg.SelectedLabel = 3;
            break;
          case ScheduleRecordingType.Daily:
            dlg.SelectedLabel = 4;
            break;
          case ScheduleRecordingType.WorkingDays:
            dlg.SelectedLabel = 5;
            break;
          case ScheduleRecordingType.Weekends:
            dlg.SelectedLabel = 6;
            break;
          case ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
            dlg.SelectedLabel = 7;
            break;
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedLabel)
        {
          case 0: //once
            rec.ScheduleType = (int)ScheduleRecordingType.Once;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
          case 1: //everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
          case 2: //everytime, all channels
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
          case 3: //weekly
            rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
          case 4: //daily
            rec.ScheduleType = (int)ScheduleRecordingType.Daily;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
          case 5: //WorkingDays
            rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
          case 6: //Weekends
            rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
          case 7://weekly everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
        }
        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(rec);

        ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
        LoadDirectory();
      }
    }


    private string GetRecType(ScheduleRecordingType recType)
    {
      string strType = String.Empty;
      switch (recType)
      {
        case ScheduleRecordingType.Daily:
          strType = GUILocalizeStrings.Get(648); //daily
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          strType = GUILocalizeStrings.Get(651); //Everytime on any channel
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          strType = GUILocalizeStrings.Get(650); //Everytime on this channel
          break;
        case ScheduleRecordingType.Once:
          strType = GUILocalizeStrings.Get(647); //Once
          break;
        case ScheduleRecordingType.WorkingDays:
          strType = GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.WorkingDays)); //Working Days
          break;
        case ScheduleRecordingType.Weekends:
          strType = GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.WeekendDays)); //Weekend Days
          break;
        case ScheduleRecordingType.Weekly:
          strType = GUILocalizeStrings.Get(679); //Weekly
          break;
        case ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
          strType = GUILocalizeStrings.Get(990000);//Weekly Everytime on this channel
          break;
      }
      return strType;
    }

    private void OnMoveDown(int item)
    {
      if (item == GetItemCount() - 1)
      {
        return;
      }
      m_iSelectedItem = item + 1;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        return;
      }
      Schedule rec = pItem.TVTag as Schedule;
      if (rec == null)
      {
        return;
      }
      GUIListItem tmpItem;
      Schedule tmprec;
      //0
      //1
      //2 ---->3
      //3 ----
      //4
      //5
      int tempPriority;
      for (int i = 0; i < item; ++i)
      {
        tmpItem = GetItem(i);
        tmprec = tmpItem.TVTag as Schedule;
        tempPriority = tmprec.Priority;
        tmprec.Priority = Schedule.HighestPriority - i;
        if (tempPriority != tmprec.Priority)
        {
          ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(tmprec);
        }
      }
      tmpItem = GetItem(item + 1);
      tmprec = tmpItem.TVTag as Schedule;
      tempPriority = tmprec.Priority;
      tmprec.Priority = Schedule.HighestPriority - item;
      if (tempPriority != tmprec.Priority)
      {
        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(tmprec);
      }

      for (int i = item + 2; i < GetItemCount(); ++i)
      {
        tmpItem = GetItem(i);
        tmprec = tmpItem.TVTag as Schedule;
        tempPriority = tmprec.Priority;
        tmprec.Priority = Schedule.HighestPriority - i;
        if (tempPriority != tmprec.Priority)
        {
          ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(tmprec);
        }
      }

      rec.Priority = Schedule.HighestPriority - item - 1;
      ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(rec);

      ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
      LoadDirectory();
    }

    private void OnMoveUp(int item)
    {
      if (item == 0)
      {
        return;
      }
      m_iSelectedItem = item - 1;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        return;
      }
      Schedule rec = pItem.TVTag as Schedule;
      if (rec == null)
      {
        return;
      }
      GUIListItem tmpItem;
      Schedule tmprec;

      for (int i = 0; i < item - 1; ++i)
      {
        tmpItem = GetItem(i);
        tmprec = tmpItem.TVTag as Schedule;
        tmprec.Priority = Schedule.HighestPriority - i;
      }
      for (int i = item - 1; i < GetItemCount(); ++i)
      {
        if (item == i)
        {
          continue;
        }
        tmpItem = GetItem(i);
        tmprec = tmpItem.TVTag as Schedule;
        tmprec.Priority = Schedule.HighestPriority - i - 1;
      }

      rec.Priority = Schedule.HighestPriority - item + 1;

      ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(rec);

      ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
      LoadDirectory();
    }

    private void SetProperties(Schedule rec)
    {
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.ProgramName);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", "");
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", TVUtil.GetRecordingDateStringFull(rec));
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", "");
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel.DisplayName);
      if (string.IsNullOrEmpty(strLogo))
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", strLogo);
      }
    }

    public void SetProperties(Schedule schedule, Program prog)
    {
      GUIPropertyManager.SetProperty("#TV.Scheduled.Title", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", String.Empty);

      GUIPropertyManager.SetProperty("#TV.Scheduled.Title", prog.Title);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", TVUtil.GetRecordingDateStringFull(schedule));
      if (prog != null)
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", prog.Channel.DisplayName);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Description", prog.Description);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", TVUtil.GetCategory(prog.ProgramCategory));
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
      }


      string logo = Utils.GetCoverArt(Thumbs.TVChannel, schedule.Channel.DisplayName);
      if (string.IsNullOrEmpty(logo))
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", "defaultVideoBig.png");
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", logo);
      }
    }


    private void UpdateDescription()
    {
      Schedule rec = ScheduleFactory.CreateSchedule(1, "", ScheduleFactory.MinSchedule, ScheduleFactory.MinSchedule);
      rec.PreRecordInterval = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("preRecordInterval", "5").Value);
      rec.PostRecordInterval = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("postRecordInterval", "5").Value);

      SetProperties(rec);
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        return;
      }
      rec = pItem.TVTag as Schedule;
      if (rec == null)
      {
        return;
      }
      Program prog = ServiceAgents.Instance.ProgramServiceAgent.GetProgramAt(rec.StartTime.AddMinutes(1), rec.IdChannel);
      SetProperties(rec, prog);
    }

    public static void OnSetEpisodesToKeep(Schedule rec)
    {
      Schedule schedule = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(rec.IdSchedule);
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(887); //quality settings
      dlg.ShowQuickNumbers = false;
      dlg.AddLocalizedString(889); //All episodes
      for (int i = 1; i < 40; ++i)
      {
        dlg.Add(i.ToString() + " " + GUILocalizeStrings.Get(874));
      }
      if (schedule.MaxAirings == Int32.MaxValue)
      {
        dlg.SelectedLabel = 0;
      }
      else
      {
        dlg.SelectedLabel = schedule.MaxAirings;
      }

      dlg.DoModal(GUIWindowManager.ActiveWindow);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      if (dlg.SelectedLabel == 0)
      {
        schedule.MaxAirings = Int32.MaxValue;
      }
      else
      {
        schedule.MaxAirings = dlg.SelectedLabel;
      }
      ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(schedule);

      ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
    }

    public static void OnSetQuality(Schedule rec)
    {
      ScheduleBLL recBLL = new ScheduleBLL(rec);

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(968);
        dlg.AddLocalizedString(965);
        dlg.AddLocalizedString(966);
        dlg.AddLocalizedString(967);
        VIDEOENCODER_BITRATE_MODE _newBitRate = recBLL.BitRateMode;
        switch (_newBitRate)
        {
          case VIDEOENCODER_BITRATE_MODE.NotSet:
            dlg.SelectedLabel = 0;
            break;
          case VIDEOENCODER_BITRATE_MODE.ConstantBitRate:
            dlg.SelectedLabel = 1;
            break;
          case VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage:
            dlg.SelectedLabel = 2;
            break;
          case VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak:
            dlg.SelectedLabel = 3;
            break;
        }

        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedLabel)
        {
          case 0: // Not Set
            _newBitRate = VIDEOENCODER_BITRATE_MODE.NotSet;
            break;

          case 1: // CBR
            _newBitRate = VIDEOENCODER_BITRATE_MODE.ConstantBitRate;
            break;

          case 2: // VBR
            _newBitRate = VIDEOENCODER_BITRATE_MODE.VariableBitRateAverage;
            break;

          case 3: // VBR Peak
            _newBitRate = VIDEOENCODER_BITRATE_MODE.VariableBitRatePeak;
            break;
        }

        recBLL.BitRateMode = _newBitRate;
        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(recBLL.Entity);

        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(968);
        dlg.AddLocalizedString(886); //Default
        dlg.AddLocalizedString(993); // Custom
        dlg.AddLocalizedString(893); //Portable
        dlg.AddLocalizedString(883); //Low
        dlg.AddLocalizedString(884); //Medium
        dlg.AddLocalizedString(885); //High
        QualityType _newQuality = recBLL.QualityType;
        switch (_newQuality)
        {
          case QualityType.NotSet:
            dlg.SelectedLabel = 0;
            break;
          case QualityType.Default:
            dlg.SelectedLabel = 1;
            break;
          case QualityType.Custom:
            dlg.SelectedLabel = 2;
            break;
          case QualityType.Portable:
            dlg.SelectedLabel = 3;
            break;
          case QualityType.Low:
            dlg.SelectedLabel = 4;
            break;
          case QualityType.Medium:
            dlg.SelectedLabel = 5;
            break;
          case QualityType.High:
            dlg.SelectedLabel = 6;
            break;
        }

        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedLabel)
        {
          case 0: // Not Set
            _newQuality = QualityType.NotSet;
            break;

          case 1: // Default
            _newQuality = QualityType.Default;
            break;

          case 2: // Custom
            _newQuality = QualityType.Custom;
            break;

          case 3: // Protable
            _newQuality = QualityType.Portable;
            break;

          case 4: // Low
            _newQuality = QualityType.Low;
            break;

          case 5: // Medium
            _newQuality = QualityType.Medium;
            break;

          case 6: // High
            _newQuality = QualityType.High;
            break;
        }

        recBLL.QualityType = _newQuality;
        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(recBLL.Entity);
      }

      ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
    }

    #endregion

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }
  }
}