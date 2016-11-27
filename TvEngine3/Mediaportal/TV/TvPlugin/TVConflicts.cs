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
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.TvPlugin.Helper;
using Action = MediaPortal.GUI.Library.Action;

namespace Mediaportal.TV.TvPlugin
{
  public class GUITVConflicts : GUIInternalWindow
  {
    #region variables, ctor/dtor

    [SkinControl(10)] protected GUIListControl listConflicts = null;

    private int m_iSelectedItem = 0;
    private GUIListItem selectedItem = null;

    public GUITVConflicts()
    {
      GetID = (int)Window.WINDOW_TV_CONFLICTS;
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvconflicts.xml");
      return bResult;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      selectedItem = null;
      LoadDirectory();
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, listConflicts.GetID, m_iSelectedItem);
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
            TVHome.Card.StopTimeShifting();
          }
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == listConflicts)
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

    protected override void OnShowContextMenu()
    {
      OnClick(GetSelectedItemNo());
    }

    #endregion

    #region list management

    private GUIListItem GetSelectedItem()
    {
      return listConflicts.SelectedListItem;
    }

    private GUIListItem GetItem(int index)
    {
      if (index < 0 || index >= listConflicts.Count)
      {
        return null;
      }
      return listConflicts[index];
    }

    private int GetSelectedItemNo()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, listConflicts.GetID, 0, 0,
                                      null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }

    private int GetItemCount()
    {
      return listConflicts.Count;
    }

    #endregion

    #region scheduled tv methods

    private GUIListItem Schedule2ListItem(Schedule schedule)
    {
      GUIListItem item = new GUIListItem();
      if (schedule == null)
      {
        return item;
      }
      item.Label = schedule.ProgramName;

      item.TVTag = schedule;
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, schedule.Channel.DisplayName);
      if (string.IsNullOrEmpty(strLogo))
      {
        strLogo = "defaultVideoBig.png";
      }
      item.PinImage = Thumbs.TvConflictRecordingIcon;
      item.ThumbnailImage = strLogo;
      item.IconImageBig = strLogo;
      item.IconImage = strLogo;
      return item;
    }

    private void LoadDirectory()
    {
      int total = 0;
      GUIWaitCursor.Show();
      GUIControl.ClearControl(GetID, listConflicts.GetID);

      if (selectedItem == null)
      {
        IEnumerable<Conflict> conflictsList = ServiceAgents.Instance.ConflictServiceAgent.ListAllConflicts();
        foreach (Conflict conflict in conflictsList)
        {
          Schedule schedule = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(conflict.IdSchedule);
          Schedule conflictingSchedule = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(conflict.IdConflictingSchedule);

          GUIListItem item = Schedule2ListItem(schedule);
          item.MusicTag = conflictingSchedule;
          item.Label3 = conflictingSchedule.ProgramName;
          item.IsFolder = true;
          listConflicts.Add(item);
          total++;
        }
      }
      else
      {
        Schedule schedule = selectedItem.TVTag as Schedule;
        Schedule conflictingSchedule = selectedItem.MusicTag as Schedule;

        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        listConflicts.Add(item);
        total++;

        item = Schedule2ListItem(schedule);
        listConflicts.Add(item);
        total++;

        item = Schedule2ListItem(conflictingSchedule);
        listConflicts.Add(item);
        SetLabels();
        total++;
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(total));

      GUIWaitCursor.Hide();
      if (listConflicts.Count == 0)
      {
        GUIWindowManager.ShowPreviousWindow();
      }
    }

    private void SetLabels()
    {
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label.Equals(".."))
        {
          continue;
        }
        Schedule rec = (Schedule)item.TVTag;
        if (rec == null)
        {
          continue;
        }

        item.Label = rec.ProgramName;
        string strTime = String.Empty;
        string strType = String.Empty;
        switch (rec.ScheduleType)
        {
          case (int)ScheduleRecordingType.Once:
            item.Label2 = TVUtil.GetRecordingDateStringFull(rec);
            break;
          case (int)ScheduleRecordingType.Daily:
            strTime = String.Format("{0}-{1}",
                                    rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                    rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(648);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case (int)ScheduleRecordingType.WorkingDays:
            strTime = String.Format("{0}-{1} {2}-{3}",
                                    GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.FirstWorkingDay)),
                                    GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.LastWorkingDay)),
                                    rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                    rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(648);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case (int)ScheduleRecordingType.Weekends:
            strTime = String.Format("{0}-{1} {2}-{3}",
                                    GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.FirstWeekendDay)),
                                    GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.LastWeekendDay)),
                                    rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                    rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(649);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case (int)ScheduleRecordingType.Weekly:
            string day;
            switch (rec.StartTime.DayOfWeek)
            {
              case DayOfWeek.Monday:
                day = GUILocalizeStrings.Get(11);
                break;
              case DayOfWeek.Tuesday:
                day = GUILocalizeStrings.Get(12);
                break;
              case DayOfWeek.Wednesday:
                day = GUILocalizeStrings.Get(13);
                break;
              case DayOfWeek.Thursday:
                day = GUILocalizeStrings.Get(14);
                break;
              case DayOfWeek.Friday:
                day = GUILocalizeStrings.Get(15);
                break;
              case DayOfWeek.Saturday:
                day = GUILocalizeStrings.Get(16);
                break;
              default:
                day = GUILocalizeStrings.Get(17);
                break;
            }

            strTime = String.Format("{0}-{1}",
                                    rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                    rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(649);
            item.Label2 = String.Format("{0} {1} {2}", strType, day, strTime);
            break;
          case (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
            switch (rec.StartTime.DayOfWeek)
            {
              case DayOfWeek.Monday: day = GUILocalizeStrings.Get(11); break;
              case DayOfWeek.Tuesday: day = GUILocalizeStrings.Get(12); break;
              case DayOfWeek.Wednesday: day = GUILocalizeStrings.Get(13); break;
              case DayOfWeek.Thursday: day = GUILocalizeStrings.Get(14); break;
              case DayOfWeek.Friday: day = GUILocalizeStrings.Get(15); break;
              case DayOfWeek.Saturday: day = GUILocalizeStrings.Get(16); break;
              default: day = GUILocalizeStrings.Get(17); break;
            }

            item.Label = rec.ProgramName;
            item.Label2 = GUILocalizeStrings.Get(990001, new object[] { day, rec.Channel.DisplayName });
            break;
          case (int)ScheduleRecordingType.EveryTimeOnThisChannel:
            item.Label = rec.ProgramName;
            item.Label2 = GUILocalizeStrings.Get(650, new object[] { rec.Channel.DisplayName });
            break;
          case (int)ScheduleRecordingType.EveryTimeOnEveryChannel:
            item.Label = rec.ProgramName;
            item.Label2 = GUILocalizeStrings.Get(651);
            break;
        }
      }
    }

    private void OnClick(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem item = GetItem(iItem);
      if (item == null)
      {
        return;
      }
      if (item.IsFolder)
      {
        if (item.Label == "..")
        {
          if (selectedItem != null)
          {
            selectedItem = null;
          }
          LoadDirectory();
          return;
        }
        if (selectedItem == null)
        {
          selectedItem = item;
        }
        LoadDirectory();
        return;
      }

      Schedule schedule = item.TVTag as Schedule;
      if (schedule == null)
      {
        return;
      }

      if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
        if (null != dlgYesNo)
        {
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
          dlgYesNo.SetLine(1, schedule.Channel.DisplayName);
          dlgYesNo.SetLine(2, schedule.ProgramName);
          dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
          dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

          if (dlgYesNo.IsConfirmed)
          {
            if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
            {
              ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(schedule.IdSchedule);
              selectedItem = null;
            }
          }
        }
      }
      else // advanced recording
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(schedule.ProgramName);
          dlg.AddLocalizedString(981); //Delete this recording
          dlg.AddLocalizedString(982); //Delete series recording
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel == -1)
          {
            return;
          }
          switch (dlg.SelectedId)
          {
            case 981: //delete specific series
              CanceledSchedule canceledSchedule = CanceledScheduleFactory.CreateCanceledSchedule(schedule.IdSchedule, schedule.IdChannel, schedule.StartTime);
              ServiceAgents.Instance.CanceledScheduleServiceAgent.SaveCanceledSchedule(canceledSchedule);
              selectedItem = null;

              ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
              break;
            case 982: //Delete entire recording              
              ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(schedule.IdSchedule);
              selectedItem = null;
              break;
          }
        }
      }
      LoadDirectory();
    }

    #endregion
  }
}