/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;

using TvDatabase;
using TvControl;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvPriorities : GUIWindow
  {

    #region variables, ctor/dtor
    [SkinControlAttribute(10)]
    protected GUIListControl listPriorities = null;

    int m_iSelectedItem = 0;
    TVUtil util = null;

    public TvPriorities()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES;
    }
    ~TvPriorities()
    {
    }

    #endregion

    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES, this);
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    #region overrides
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvpriorities.xml");
      return bResult;
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_GUI:
          if (!g_Player.Playing && TVHome.Card.IsTimeShifting)
          {
            //if we're watching tv
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
          else if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
          {
            //if we're watching a tv recording
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
          else if (g_Player.Playing && g_Player.HasVideo)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
          break;
      }
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

      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
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

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == listPriorities)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
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
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        OnMoveUp(iItem);
      }
    }
    protected override void OnClickedDown(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listPriorities)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
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
    GUIListItem GetSelectedItem()
    {
      return listPriorities.SelectedListItem;
    }

    GUIListItem GetItem(int index)
    {
      if (index < 0 || index >= listPriorities.Count) return null;
      return listPriorities[index];
    }

    int GetSelectedItemNo()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, listPriorities.GetID, 0, 0, null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      return listPriorities.Count;
    }
    #endregion


    #region scheduled tv methods
    void LoadDirectory()
    {
      GUIControl.ClearControl(GetID, listPriorities.GetID);
      EntityQuery query = new EntityQuery(typeof(Schedule));
      query.AddOrderBy(Schedule.PriorityEntityColumn);

      EntityList<Schedule> itemlist = DatabaseManager.Instance.GetEntities<Schedule>(query);
      int total = 0;
      foreach (Schedule rec in itemlist)
      {
        GUIListItem item = new GUIListItem();
        item.Label = String.Format("{0}.{1}", total, rec.ProgramName);
        item.TVTag = rec;
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel.Name);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        VirtualCard card;
        if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card))
        {
          if (rec.ScheduleType != (int)ScheduleRecordingType.Once)
            item.PinImage = Thumbs.TvRecordingSeriesIcon;
          else
            item.PinImage = Thumbs.TvRecordingIcon;
        }
        //@ else if (ConflictManager.IsConflict(rec))
        //{
        //  item.PinImage = Thumbs.TvConflictRecordingIcon;
        //}
        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        listPriorities.Add(item);
        total++;
      }

      string strObjects = String.Format("{0} {1}", total, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", strObjects);
      GUIControl.SelectItemControl(GetID, listPriorities.GetID, m_iSelectedItem);
      GUIControl cntlLabel = GetControl(12);
      if (cntlLabel != null)
        cntlLabel.YPosition = listPriorities.SpinY;

    }
    void SetLabels()
    {

      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        Schedule rec = (Schedule)item.TVTag;
        //@
        /*
        switch (rec.Status)
        {
          case Schedule.RecordingStatus.Waiting:
            item.Label3 = GUILocalizeStrings.Get(681);//waiting
            break;
          case Schedule.RecordingStatus.Finished:
            item.Label3 = GUILocalizeStrings.Get(683);//Finished
            break;
          case Schedule.RecordingStatus.Canceled:
            item.Label3 = GUILocalizeStrings.Get(684);//Canceled
            break;
        }*/

        // check with recorder.
        VirtualCard card;
        if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card))
        {
          item.Label3 = GUILocalizeStrings.Get(682);//Recording
          if (rec.ScheduleType != (int)ScheduleRecordingType.Once)
            item.PinImage = Thumbs.TvRecordingSeriesIcon;
          else
            item.PinImage = Thumbs.TvRecordingIcon;
        }
        //@else if (ConflictManager.IsConflict(rec))
        //{
        //  item.PinImage = Thumbs.TvConflictRecordingIcon;
        //}
        else
        {
          item.PinImage = String.Empty;
        }

        string strType = String.Empty;
        item.Label = rec.ProgramName;
        string strTime = String.Format("{0} {1} - {2}",
          rec.StartTime.ToShortDateString(),
          rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
          rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        switch ((ScheduleRecordingType)rec.ScheduleType)
        {
          case ScheduleRecordingType.Once:
            item.Label2 = String.Format("{0} {1} - {2}",
              Utils.GetShortDayString(rec.StartTime),
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)); ;
            break;
          case ScheduleRecordingType.Daily:
            strTime = String.Format("{0}-{1}",
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(648);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case ScheduleRecordingType.WorkingDays:
            strTime = String.Format("{0}-{1} {2}-{3}",
              GUILocalizeStrings.Get(657),//657=Mon
              GUILocalizeStrings.Get(661),//661=Fri
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(648);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case ScheduleRecordingType.Weekends:
            strTime = String.Format("{0}-{1} {2}-{3}",
                GUILocalizeStrings.Get(662),//662=Sat
                GUILocalizeStrings.Get(663),//6613Sun
                rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(649);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case ScheduleRecordingType.Weekly:
            string day;
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

            strTime = String.Format("{0}-{1}",
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(649);
            item.Label2 = String.Format("{0} {1} {2}", strType, day, strTime);
            break;
          case ScheduleRecordingType.EveryTimeOnThisChannel:
            item.Label = rec.ProgramName;
            item.Label2 = GUILocalizeStrings.Get(650);
            break;
          case ScheduleRecordingType.EveryTimeOnEveryChannel:
            item.Label = rec.ProgramName;
            item.Label2 = GUILocalizeStrings.Get(651);
            break;
        }
      }
    }

    void OnClick(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem item = GetItem(iItem);
      if (item == null) return;
      Schedule rec = (Schedule)item.TVTag;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;

      dlg.Reset();
      dlg.SetHeading(rec.ProgramName);

      if (rec.Series == false)
      {
        dlg.AddLocalizedString(618);//delete
      }
      else
      {
        dlg.AddLocalizedString(981);//Delete this recording
        dlg.AddLocalizedString(982);//Delete series recording
        dlg.AddLocalizedString(888);//Episodes management
      }
      VirtualCard card;
      if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule,out card))
      {
        dlg.AddLocalizedString(979); //Play recording from beginning
        dlg.AddLocalizedString(980); //Play recording from live point
      }
      else
      {
        dlg.AddLocalizedString(882);//Quality settings
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      switch (dlg.SelectedId)
      {

        case 888:////Episodes management
          OnSetEpisodesToKeep(rec);
          break;
        case 882:
          OnSetQuality(rec);
          break;

        case 981: //Delete this recording only
          {
            if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card))
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              if (null != dlgYesNo)
              {
                dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
                dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
                dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                if (dlgYesNo.IsConfirmed)
                {
                  RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
                  CanceledSchedule schedule = CanceledSchedule.Create();
                  schedule.CancelDateTime = rec.StartTime;
                  schedule.Schedule = rec;
                  DatabaseManager.Instance.SaveChanges();
                  RemoteControl.Instance.OnNewSchedule();
                }
              }
            }
            else
            {
              RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
              CanceledSchedule schedule = CanceledSchedule.Create();
              schedule.CancelDateTime = rec.StartTime;
              schedule.Schedule = rec;
              DatabaseManager.Instance.SaveChanges();
              RemoteControl.Instance.OnNewSchedule();
            }
            LoadDirectory();
          }
          break;

        case 982: //Delete series recording
          goto case 618;

        case 618: // delete entire recording
          {
            if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card))
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              if (null != dlgYesNo)
              {
                dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
                dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
                dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                if (dlgYesNo.IsConfirmed)
                {
                  RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
                  rec.DeleteAll();
                  RemoteControl.Instance.OnNewSchedule();
                }
              }
            }
            else
            {
              rec.DeleteAll();
              RemoteControl.Instance.OnNewSchedule();
            }
            LoadDirectory();
          }
          break;

        case 979: // Play recording from beginning
          if (g_Player.Playing && g_Player.IsTVRecording)
          {
            g_Player.Stop();
          }
          //TVHome.IsTVOn = true;
          TVHome.ViewChannel(rec.Channel.Name);
          g_Player.SeekAbsolute(0);
          if (TVHome.Card.IsTimeShifting)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
            return;
          }
          break;

        case 980: // Play recording from live point
          //TVHome.IsTVOn = true;
          TVHome.ViewChannel(rec.Channel.Name);
          if (TVHome.Card.IsTimeShifting)
          {
            if (g_Player.Playing)
            {
              g_Player.SeekAsolutePercentage(99);
            }
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
            return;
          }
          break;
      }
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listPriorities.GetID, m_iSelectedItem);
    }

    void ChangeType(Schedule rec)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
        for (int i = 611; i <= 615; ++i)
        {
          dlg.Add(GUILocalizeStrings.Get(i));
        }
        dlg.Add(GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
        dlg.Add(GUILocalizeStrings.Get(1051));// 1051=Record Sat-Sun 
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
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1) return;
        switch (dlg.SelectedLabel)
        {
          case 0://once
            rec.ScheduleType = (int)ScheduleRecordingType.Once;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 1://everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 2://everytime, all channels
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 3://weekly
            rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 4://daily
            rec.ScheduleType = (int)ScheduleRecordingType.Daily;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 5://Mo-Fi
            rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 6://Sat-Sun
            rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
            rec.Canceled = Schedule.MinSchedule;
            break;
        }
        DatabaseManager.Instance.SaveChanges();
        RemoteControl.Instance.OnNewSchedule();
        LoadDirectory();

      }
    }


    string GetRecType(ScheduleRecordingType recType)
    {
      string strType = String.Empty;
      switch (recType)
      {
        case ScheduleRecordingType.Daily:
          strType = GUILocalizeStrings.Get(648);//daily
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          strType = GUILocalizeStrings.Get(651);//Everytime on any channel
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          strType = GUILocalizeStrings.Get(650);//Everytime on this channel
          break;
        case ScheduleRecordingType.Once:
          strType = GUILocalizeStrings.Get(647);//Once
          break;
        case ScheduleRecordingType.WorkingDays:
          strType = GUILocalizeStrings.Get(680);//Mon-Fri
          break;
        case ScheduleRecordingType.Weekends:
          strType = GUILocalizeStrings.Get(1050);//Sat-Sun
          break;
        case ScheduleRecordingType.Weekly:
          strType = GUILocalizeStrings.Get(679);//Weekly
          break;
      }
      return strType;
    }

    void OnMoveDown(int item)
    {

      if (item == GetItemCount() - 1) return;
      m_iSelectedItem = item + 1;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        return;
      }
      Schedule rec = pItem.TVTag as Schedule;
      if (rec == null) return;
      GUIListItem tmpItem;
      Schedule tmprec;
      //0
      //1
      //2 ---->3
      //3 ----
      //4
      //5

      for (int i = 0; i < item; ++i)
      {
        tmpItem = GetItem(i);
        tmprec = tmpItem.TVTag as Schedule;
        tmprec.Priority = Schedule.HighestPriority - i;
      }
      tmpItem = GetItem(item + 1);
      tmprec = tmpItem.TVTag as Schedule;
      tmprec.Priority = Schedule.HighestPriority - item;
      
      for (int i = item + 2; i < GetItemCount(); ++i)
      {
        tmpItem = GetItem(i);
        tmprec = tmpItem.TVTag as Schedule;
        tmprec.Priority = Schedule.HighestPriority - i;
        
      }

      rec.Priority = Schedule.HighestPriority - item - 1;
      DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      LoadDirectory();
    }

    void OnMoveUp(int item)
    {
      if (item == 0) return;
      m_iSelectedItem = item - 1;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        return;
      }
      Schedule rec = pItem.TVTag as Schedule;
      if (rec == null) return;
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
        if (item == i) continue;
        tmpItem = GetItem(i);
        tmprec = tmpItem.TVTag as Schedule;
        tmprec.Priority = Schedule.HighestPriority - i - 1;
      }

      rec.Priority = Schedule.HighestPriority - item + 1;

      DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      LoadDirectory();
    }

    void SetProperties(Schedule  rec)
    {
      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(rec.StartTime),
        rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.ProgramName);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", "");
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", "");
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel.Name);
      if (System.IO.File.Exists(strLogo))
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", strLogo);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
    }
    public void SetProperties(Schedule schedule,Program  prog)
    {
      GUIPropertyManager.SetProperty("#TV.Scheduled.Title", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", String.Empty);

      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(schedule.StartTime),
        schedule.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        schedule.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.Scheduled.Title", prog.Title);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", strTime);
      if (prog != null)
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Description", prog.Description);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", prog.Genre);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
      }


      string logo = Utils.GetCoverArt(Thumbs.TVChannel, schedule.Channel.Name);
      if (System.IO.File.Exists(logo))
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", logo);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", "defaultVideoBig.png");
      }
    }


    void UpdateDescription()
    {
      Schedule rec = Schedule.New();
      SetProperties(rec);
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        return;
      }
      rec = pItem.TVTag as Schedule;
      if (rec == null) return;
      Program prog = rec.Channel.GetProgramAt( rec.StartTime.AddMinutes(1));
      SetProperties(rec,prog);
    }

    static public void OnSetEpisodesToKeep(Schedule rec)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(887);//quality settings
      dlg.ShowQuickNumbers = false;
      dlg.AddLocalizedString(889);//All episodes
      for (int i = 1; i < 40; ++i)
        dlg.Add(i.ToString() + " " + GUILocalizeStrings.Get(874));
      if (rec.MaxAirings == Int32.MaxValue)
        dlg.SelectedLabel = 0;
      else
        dlg.SelectedLabel = rec.MaxAirings;

      dlg.DoModal(GUIWindowManager.ActiveWindow);
      if (dlg.SelectedLabel == -1) return;

      if (dlg.SelectedLabel == 0) rec.MaxAirings = Int32.MaxValue;
      else rec.MaxAirings = dlg.SelectedLabel;
      DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
    }

    static public void OnSetQuality(Schedule rec)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;

      dlg.Reset();
      dlg.SetHeading(882);//quality settings
      dlg.AddLocalizedString(886);//Default
      dlg.AddLocalizedString(893);//Portable
      dlg.AddLocalizedString(883);//Low
      dlg.AddLocalizedString(884);//Medium
      dlg.AddLocalizedString(885);//High
      switch ((Schedule.QualityType)rec.Quality)
      {
        case Schedule.QualityType.NotSet: dlg.SelectedLabel = 0; break;
        case Schedule.QualityType.Portable: dlg.SelectedLabel = 1; break;
        case Schedule.QualityType.Low: dlg.SelectedLabel = 2; break;
        case Schedule.QualityType.Medium: dlg.SelectedLabel = 3; break;
        case Schedule.QualityType.High: dlg.SelectedLabel = 4; break;

      }
      dlg.DoModal(GUIWindowManager.ActiveWindow);
      if (dlg.SelectedLabel == -1) return;
      switch (dlg.SelectedId)
      {
        case 886: rec.Quality = (int)Schedule.QualityType.NotSet; break;
        case 893: rec.Quality = (int)Schedule.QualityType.Portable; break;
        case 883: rec.Quality = (int)Schedule.QualityType.Low; break;
        case 884: rec.Quality = (int)Schedule.QualityType.Medium; break;
        case 885: rec.Quality = (int)Schedule.QualityType.High; break;
      }
      DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
    }
    #endregion
  }
}
