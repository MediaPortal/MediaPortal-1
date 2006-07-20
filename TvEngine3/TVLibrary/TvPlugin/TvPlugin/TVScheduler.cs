/*	Copyright (C) 2005-2006 Team MediaPortal
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
  public class TvScheduler : GUIWindow, IComparer<GUIListItem>
  {
    #region variables, ctor/dtor
    enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
      Type = 3,
      Status = 4,
    }
    [SkinControlAttribute(2)]
    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnNew = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnCleanup = null;
    [SkinControlAttribute(10)]
    protected GUIListControl listSchedules = null;

    SortMethod currentSortMethod = SortMethod.Date;
    bool m_bSortAscending = true;
    int m_iSelectedItem = 0;
    bool needUpdate = false;
    string currentShow = String.Empty;

    public TvScheduler()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SCHEDULER;
    }
    ~TvScheduler()
    {
    }
    public override void OnAdded()
    {
      Log.Write("TvRecorded:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_SCHEDULER, this);
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }


    #endregion

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string strTmp = String.Empty;
        strTmp = (string)xmlreader.GetValue("tvscheduler", "sort");
        if (strTmp != null)
        {
          if (strTmp == "channel") currentSortMethod = SortMethod.Channel;
          else if (strTmp == "date") currentSortMethod = SortMethod.Date;
          else if (strTmp == "name") currentSortMethod = SortMethod.Name;
          else if (strTmp == "type") currentSortMethod = SortMethod.Type;
          else if (strTmp == "status") currentSortMethod = SortMethod.Status;
        }
        m_bSortAscending = xmlreader.GetValueAsBool("tvscheduler", "sortascending", true);
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvscheduler", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvscheduler", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvscheduler", "sort", "name");
            break;
          case SortMethod.Type:
            xmlwriter.SetValue("tvscheduler", "sort", "type");
            break;
          case SortMethod.Status:
            xmlwriter.SetValue("tvscheduler", "sort", "status");
            break;
        }
        xmlwriter.SetValueAsBool("tvscheduler", "sortascending", m_bSortAscending);
      }
    }
    #endregion

    #region overrides
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvscheduler.xml");
      LoadSettings();
      return bResult;
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (listSchedules.Focus)
        {
          GUIListItem item = listSchedules[0];
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              currentShow = String.Empty;
              LoadDirectory();
              return;
            }
          }
        }
      }

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
      //@      ConflictManager.OnConflictsUpdated += new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);

      LoadSettings();
      needUpdate = false;
      LoadDirectory();

      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listSchedules.GetID, m_iSelectedItem);

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }
    protected override void OnPageDestroy(int newWindowId)
    {
      //@ConflictManager.OnConflictsUpdated -= new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);
      base.OnPageDestroy(newWindowId);
      m_iSelectedItem = GetSelectedItemNo();
      SaveSettings();

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

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnSortBy) // sort by
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            currentSortMethod = SortMethod.Date;
            break;
          case SortMethod.Date:
            currentSortMethod = SortMethod.Name;
            break;
          case SortMethod.Name:
            currentSortMethod = SortMethod.Type;
            break;
          case SortMethod.Type:
            currentSortMethod = SortMethod.Status;
            break;
          case SortMethod.Status:
            currentSortMethod = SortMethod.Channel;
            break;
        }
        OnSort();
      }
      if (control == listSchedules)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
      }
      if (control == btnCleanup)
      {
        OnCleanup();
      }
      if (control == btnNew)
      {
        OnNewShedule();
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
      return listSchedules.SelectedListItem;
    }

    GUIListItem GetItem(int index)
    {
      if (index < 0 || index >= listSchedules.Count) return null;
      return listSchedules[index];
    }

    int GetSelectedItemNo()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, listSchedules.GetID, 0, 0, null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      return listSchedules.Count;
    }
    #endregion

    #region Sort Members
    void OnSort()
    {
      SetLabels();
      listSchedules.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2) return 0;
      if (item1 == null) return -1;
      if (item2 == null) return -1;
      if (item1.IsFolder && item1.Label == "..") return -1;
      if (item2.IsFolder && item2.Label == "..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;

      int iComp = 0;
      TimeSpan ts;
      Schedule rec1 = (Schedule)item1.TVTag;
      Schedule rec2 = (Schedule)item2.TVTag;

      //0=Recording->1=Finished->2=Waiting->3=Canceled
      int type1 = 2, type2 = 2;
      if (item1.Label3 == GUILocalizeStrings.Get(682)) type1 = 0;
      else if (item1.Label3 == GUILocalizeStrings.Get(683)) type1 = 1;
      else if (item1.Label3 == GUILocalizeStrings.Get(681)) type1 = 2;
      else if (item1.Label3 == GUILocalizeStrings.Get(684)) type1 = 3;


      if (item2.Label3 == GUILocalizeStrings.Get(682)) type2 = 0;
      else if (item2.Label3 == GUILocalizeStrings.Get(683)) type2 = 1;
      else if (item2.Label3 == GUILocalizeStrings.Get(681)) type2 = 2;
      else if (item2.Label3 == GUILocalizeStrings.Get(684)) type2 = 3;
      if (type1 == 0 && type2 != 0) return -1;
      if (type2 == 0 && type1 != 0) return 1;

      switch (currentSortMethod)
      {
        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.ProgramName, rec2.ProgramName, true);
            if (iComp == 0) goto case SortMethod.Channel;
            else return iComp;
          }
          else
          {
            iComp = String.Compare(rec2.ProgramName, rec1.ProgramName, true);
            if (iComp == 0) goto case SortMethod.Channel;
            else return iComp;
          }

        case SortMethod.Status:
          // sort by: 0=Recording->1=Finished->2=Waiting->3=Canceled
          if (m_bSortAscending)
          {
            if (type1 < type2) return -1;
            if (type1 > type2) return 1;
          }
          else
          {
            if (type1 < type2) return 1;
            if (type1 > type2) return -1;
          }
          goto case SortMethod.Channel;

        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Channel.Name, rec2.Channel.Name, true);
            if (iComp == 0) goto case SortMethod.Date;
            else return iComp;
          }
          else
          {
            iComp = String.Compare(rec2.Channel.Name, rec1.Channel.Name, true);
            if (iComp == 0) goto case SortMethod.Date;
            else return iComp;
          }

        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime == rec2.StartTime) return 0;
            if (rec1.StartTime > rec2.StartTime) return 1;
            return -1;
          }
          else
          {
            if (rec2.StartTime == rec1.StartTime) return 0;
            if (rec2.StartTime > rec1.StartTime) return 1;
            return -1;
          }

        case SortMethod.Type:
          item1.Label2 = GetScheduleType(rec1.ScheduleType);
          item2.Label2 = GetScheduleType(rec2.ScheduleType);
          if (rec1.ScheduleType != rec2.ScheduleType)
          {
            if (m_bSortAscending)
              return (int)rec1.ScheduleType - (int)rec2.ScheduleType;
            else
              return (int)rec2.ScheduleType - (int)rec1.ScheduleType;
          }
          if (rec1.StartTime != rec2.StartTime)
          {
            if (m_bSortAscending)
            {
              ts = rec1.StartTime - rec2.StartTime;
              return (int)(ts.Minutes);
            }
            else
            {
              ts = rec2.StartTime - rec1.StartTime;
              return (int)(ts.Minutes);
            }
          }
          if (rec1.Channel.Name != rec2.Channel.Name)
            if (m_bSortAscending)
              return String.Compare(rec1.Channel.Name, rec2.Channel.Name);
            else
              return String.Compare(rec2.Channel.Name, rec1.Channel.Name);
          if (rec1.ProgramName != rec2.ProgramName)
            if (m_bSortAscending)
              return String.Compare(rec1.ProgramName, rec2.ProgramName);
            else
              return String.Compare(rec2.ProgramName, rec1.ProgramName);
          return 0;
      }
      return 0;
    }
    #endregion

    #region scheduled tv methods
    void LoadDirectory()
    {
      GUIControl.ClearControl(GetID, listSchedules.GetID);

      EntityList<Schedule> itemlist = DatabaseManager.Instance.GetEntities<Schedule>();
      int total = 0;
      if (currentShow == String.Empty)
      {
        foreach (Schedule rec in itemlist)
        {
          List<Schedule> recs = TVHome.Util.GetRecordingTimes(rec);

          GUIListItem item = new GUIListItem();
          item.Label = rec.ProgramName;
          item.TVTag = rec;
          item.MusicTag = rec;
          if (recs.Count > 1)
          {
            item.IsFolder = true;
            Utils.SetDefaultIcons(item);
          }
          else
          {
            string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel.Name);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            item.ThumbnailImage = strLogo;
            item.IconImageBig = strLogo;
            item.IconImage = strLogo;
          }
          VirtualCard card;
          if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card))
          {
            if (rec.ScheduleType != (int)ScheduleRecordingType.Once)
              item.PinImage = Thumbs.TvRecordingSeriesIcon;
            else
              item.PinImage = Thumbs.TvRecordingIcon;
          }
          else
          {
            //@
            /*if (ConflictManager.IsConflict(rec))
            {
              item.PinImage = Thumbs.TvConflictRecordingIcon;
            }*/
          }
          listSchedules.Add(item);
          total++;
        }
      }
      else
      {

        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        Utils.SetDefaultIcons(item);
        listSchedules.Add(item);
        total++;

        foreach (Schedule rec in itemlist)
        {
          if (!rec.ProgramName.Equals(currentShow)) continue;
          //@List<Schedule> recs = ConflictManager.Util.GetRecordingTimes(rec);
          List<Schedule> recs = new List<Schedule>();
          if (recs.Count >= 1)
          {
            for (int x = 0; x < recs.Count; ++x)
            {
              Schedule recSeries = (Schedule)recs[x];
              if (DateTime.Now > recSeries.EndTime) continue;
              if (recSeries.Canceled != Schedule.MinSchedule) continue;

              item = new GUIListItem();
              item.Label = recSeries.ProgramName;
              item.TVTag = recSeries;
              item.MusicTag = rec;
              string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, recSeries.Channel.Name);
              if (!System.IO.File.Exists(strLogo))
              {
                strLogo = "defaultVideoBig.png";
              }
              VirtualCard card;
              if (RemoteControl.Instance.IsRecordingSchedule(recSeries.IdSchedule, out card))
              {
                if (rec.StartTime <= DateTime.Now && rec.EndTime >= DateTime.Now)
                {
                  if (rec.ScheduleType != (int)ScheduleRecordingType.Once)
                    item.PinImage = Thumbs.TvRecordingSeriesIcon;
                  else
                    item.PinImage = Thumbs.TvRecordingIcon;
                }
              }
              else
              {
                //@
                /*if (ConflictManager.IsConflict(rec))
                {
                  item.PinImage = Thumbs.TvConflictRecordingIcon;
                }*/
              }
              item.ThumbnailImage = strLogo;
              item.IconImageBig = strLogo;
              item.IconImage = strLogo;
              listSchedules.Add(item);
              total++;
            }
          }
        }
      }

      string strObjects = String.Format("{0} {1}", total, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", strObjects);
      GUIControl cntlLabel = GetControl(12);
      if (cntlLabel != null)
        cntlLabel.YPosition = listSchedules.SpinY;

      OnSort();
      UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
      string strLine = String.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Channel:
          strLine = GUILocalizeStrings.Get(620);// Sort by: Channel
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(621);// Sort by: Date
          break;
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(268);// Sort by: Title
          break;
        case SortMethod.Type:
          strLine = GUILocalizeStrings.Get(623);// Sort by: Type
          break;
        case SortMethod.Status:
          strLine = GUILocalizeStrings.Get(685);// Sort by: Status
          break;
      }

      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, strLine);
      btnSortBy.IsAscending = m_bSortAscending;
    }

    void SetLabels()
    {
      SortMethod method = currentSortMethod;
      bool bAscending = m_bSortAscending;

      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label.Equals("..")) continue;
        Schedule rec = (Schedule)item.TVTag;

        //@
        /*switch (rec.Status)
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
        //@
        /*
      else if (ConflictManager.IsConflict(rec))
      {
        item.PinImage = Thumbs.TvConflictRecordingIcon;
      }*/
        else
        {
          item.PinImage = String.Empty;
        }

        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            goto case SortMethod.Name;
          case SortMethod.Date:
            goto case SortMethod.Name;
          case SortMethod.Name:
            goto case SortMethod.Type;
          case SortMethod.Type:
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
                  GUILocalizeStrings.Get(663),//663=Sun
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
            break;
        }
      }
    }

    void OnClick(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem item = GetItem(iItem);
      if (item == null) return;
      Schedule rec = item.TVTag as Schedule;
      if (item.IsFolder)
      {
        if (item.Label.Equals(".."))
        {
          currentShow = String.Empty;
          LoadDirectory();
          return;
        }
        currentShow = rec.ProgramName;
        LoadDirectory();
        return;
      }

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
      if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card))
      {
        dlg.AddLocalizedString(979); //Play recording from beginning
        dlg.AddLocalizedString(980); //Play recording from live point
      }
      else
      {
        dlg.AddLocalizedString(882);//Quality settings
      }
      dlg.AddLocalizedString(1048); // settings

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      switch (dlg.SelectedId)
      {
        case 888:////Episodes management
          TvPriorities.OnSetEpisodesToKeep(rec);
          break;

        case 1048:////settings
          TVProgramInfo.CurrentRecording = item.MusicTag as Schedule;
          if (TVProgramInfo.CurrentProgram != null)
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO);
          return;
        case 882:////Quality settings
          //GUITVPriorities.OnSetQuality(rec);
          break;

        case 981: //Delete this recording only
          {
            if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card))
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              if (null != dlgYesNo)
              {
                dlgYesNo.SetDefaultToYes(false);
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
                dlgYesNo.SetDefaultToYes(false);
                dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
                dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
                dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                if (dlgYesNo.IsConfirmed)
                {
                  RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
                }
              }
            }
            else
            {
              rec.Delete();
              RemoteControl.Instance.OnNewSchedule();
            }
            LoadDirectory();
          }
          break;

        case 979: // Play recording from beginning
          
          string filename = TVHome.Card.RecordingFileName;
          if (filename != String.Empty)
          {
            g_Player.Play(filename);
            if (g_Player.Playing)
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
              return;
            }
          }
          break;

        case 980: // Play recording from live point

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
      GUIControl.SelectItemControl(GetID, listSchedules.GetID, m_iSelectedItem);
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
        DatabaseManager.SaveChanges();
        RemoteControl.Instance.OnNewSchedule();
        LoadDirectory();

      }
    }

    void OnNewShedule()
    {
      bool isQuickRecord = false;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(780));  //Select scheduling method
      dlg.Add(GUILocalizeStrings.Get(781));         //Quick record 
      dlg.Add(GUILocalizeStrings.Get(782));         //Advanced record
      dlg.DoModal(GetID);

      if (dlg.SelectedLabel < 0) return;
      if (dlg.SelectedLabel == 0)
        isQuickRecord = true;

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(891));  //Select TV Channel
      ReadOnlyEntityList<GroupMap> channels = TVHome.Navigator.CurrentGroup.GroupMaps;
      foreach (GroupMap chan in channels)
      {
        GUIListItem item = new GUIListItem(chan.Channel.Name);
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.Channel.Name);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        dlg.Add(item);
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0) return;

      Channel selectedChannel = channels[dlg.SelectedLabel].Channel as Channel;
      dlg.Reset();
      dlg.SetHeading(616);//select recording type
      for (int i = 611; i <= 615; ++i)
      {
        dlg.Add(GUILocalizeStrings.Get(i));
      }
      dlg.Add(GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
      dlg.Add(GUILocalizeStrings.Get(1051));// 1051=Record Sat-Sun

      Schedule rec = Schedule.Create();
      rec.Channel = selectedChannel;

      if (!isQuickRecord)
      {
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1) return;

        switch (dlg.SelectedLabel)
        {
          case 0://once
            rec.ScheduleType = (int)ScheduleRecordingType.Once;
            break;
          case 1://everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            break;
          case 2://everytime, all channels
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            break;
          case 3://weekly
            rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
            break;
          case 4://daily
            rec.ScheduleType = (int)ScheduleRecordingType.Daily;
            break;
          case 5://Mo-Fi
            rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
            break;
          case 6://Sat-Sun
            rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
            break;
        }
      }
      else
        rec.ScheduleType = (int)ScheduleRecordingType.Once;

      DateTime dtNow = DateTime.Now;
      int day;
      if (!isQuickRecord)
      {
        dlg.Reset();
        dlg.SetHeading(636);//select day
        dlg.ShowQuickNumbers = false;

        for (day = 0; day < 30; day++)
        {
          if (day > 0)
            dtNow = DateTime.Now.AddDays(day);
          dlg.Add(dtNow.ToLongDateString());
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
          return;
        day = dlg.SelectedLabel;
      }
      else
        day = 0;

      dlg.Reset();
      dlg.SetHeading(142);//select time
      dlg.ShowQuickNumbers = false;
      //time
      //int no = 0;
      int hour, minute, steps;
      if (isQuickRecord) steps = 15;
      else steps = 5;
      dlg.Add("00:00");
      for (hour = 0; hour <= 23; hour++)
      {
        for (minute = 0; minute < 60; minute += steps)
        {
          if (hour == 0 && minute == 0) continue;
          string time = "";
          if (hour < 10) time = "0" + hour.ToString();
          else time = hour.ToString();
          time += ":";
          if (minute < 10) time = time + "0" + minute.ToString();
          else time += minute.ToString();

          //if (hour < 1) time = String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004));
          dlg.Add(time);
        }
      }
      // pre-select the current time
      dlg.SelectedLabel = (DateTime.Now.Hour * (60 / steps)) + (Convert.ToInt16(DateTime.Now.Minute / steps));
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;

      int mins = (dlg.SelectedLabel) * steps;
      hour = (mins) / 60;
      minute = ((mins) % 60);


      dlg.Reset();
      dlg.SetHeading(180);//select time
      dlg.ShowQuickNumbers = false;
      //duration
      for (float hours = 0.5f; hours <= 24f; hours += 0.5f)
      {
        dlg.Add(String.Format("{0} {1}", hours.ToString("f2"), GUILocalizeStrings.Get(3002)));
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      int duration = (dlg.SelectedLabel + 1) * 30;

      if (!isQuickRecord)
      {
        TvPriorities.OnSetQuality(rec);
      }

      dtNow = DateTime.Now.AddDays(day);
      rec.StartTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, hour, minute, 0, 0);
      rec.EndTime = rec.StartTime.AddMinutes(duration);
      rec.ProgramName = GUILocalizeStrings.Get(413) + " (" + rec.Channel.Name + ")";
      DatabaseManager.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      LoadDirectory();
    }

    void OnEdit(Schedule rec)
    {
      GUIDialogDateTime dlg = (GUIDialogDateTime)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_DATETIME);
      if (dlg != null)
      {
        VirtualCard card;
        EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>();
        dlg.SetHeading(637);
        dlg.Items.Clear();
        dlg.EnableChannel = true;
        dlg.EnableStartTime = true;
        if (RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule,out card))
        {
          dlg.EnableChannel = false;
          dlg.EnableStartTime = false;
        }
        foreach (Channel chan in channels)
        {
          dlg.Items.Add(chan.Name);
        }
        dlg.Channel = rec.Channel.Name;
        dlg.StartDateTime = rec.StartTime;
        dlg.EndDateTime = rec.EndTime;
        dlg.DoModal(GetID);
        if (dlg.IsConfirmed)
        {
          //@rec.Channel = dlg.Channel;
          rec.EndTime = dlg.EndDateTime;
          rec.Canceled = Schedule.MinSchedule;
          DatabaseManager.SaveChanges();
          RemoteControl.Instance.OnNewSchedule();
          LoadDirectory();
        }
      }
    }

    string GetScheduleType(int type)
    {
      ScheduleRecordingType ScheduleType = (ScheduleRecordingType)type;
      string strType = String.Empty;
      switch (ScheduleType)
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

    void OnCleanup()
    {
      int iCleaned = 0;
      EntityList<Schedule> itemlist = DatabaseManager.Instance.GetEntities<Schedule>();
      foreach (Schedule rec in itemlist)
      {
        //@
        /*
        if (rec.IsDone() || rec.Canceled != Schedule.MinSchedule)
        {
          iCleaned++;
          TVDatabase.RemoveRecording(rec);
        }*/
      }
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      LoadDirectory();
      if (pDlgOK != null)
      {
        pDlgOK.SetHeading(624);
        pDlgOK.SetLine(1, String.Format("{0}{1}", GUILocalizeStrings.Get(625), iCleaned));
        pDlgOK.SetLine(2, String.Empty);
        pDlgOK.DoModal(GetID);
      }
    }

    void SetProperties(Schedule rec)
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
    public void SetProperties(Schedule schedule, Program prog)
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

      Program prog = rec.Channel.GetProgramAt(rec.StartTime.AddMinutes(1));
      SetProperties(rec, prog);
    }
    #endregion

    private void ConflictManager_OnConflictsUpdated()
    {
      needUpdate = true;
    }
    public override void Process()
    {
      if (needUpdate)
      {
        needUpdate = false;
        LoadDirectory();
      }
    }

    void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;

      OnSort();
    }
  }
}
