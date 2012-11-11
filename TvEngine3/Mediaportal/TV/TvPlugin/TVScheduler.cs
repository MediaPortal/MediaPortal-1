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
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.TvPlugin.Helper;
using Action = MediaPortal.GUI.Library.Action;

namespace Mediaportal.TV.TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvScheduler : GUIInternalWindow, IComparer<GUIListItem>
  {
 

    #region variables, ctor/dtor

    private enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
    }

    [SkinControl(2)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(6)] protected GUIButtonControl btnNew = null;
    [SkinControl(7)] protected GUIButtonControl btnCleanup = null;
    [SkinControl(8)] protected GUIButtonControl btnPriorities = null;
    [SkinControl(9)] protected GUIButtonControl btnConflicts = null;
    [SkinControl(10)] protected GUIListControl listSchedules = null;
    [SkinControl(11)] protected GUIToggleButtonControl btnSeries = null;

    private SortMethod currentSortMethod = SortMethod.Date;
    private bool m_bSortAscending = true;
    private int m_iSelectedItem = 0;
    private bool needUpdate = false;
    private ScheduleBLL selectedItem = null;


    public TvScheduler()
    {
      GetID = (int)Window.WINDOW_SCHEDULER;
    }

    ~TvScheduler() {}

    public override bool IsTv
    {
      get { return true; }
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        string strTmp = String.Empty;
        strTmp = (string)xmlreader.GetValue("tvscheduler", "sort");
        if (strTmp != null)
        {
          if (strTmp == "channel")
          {
            currentSortMethod = SortMethod.Channel;
          }
          else if (strTmp == "date")
          {
            currentSortMethod = SortMethod.Date;
          }
          else if (strTmp == "name")
          {
            currentSortMethod = SortMethod.Name;
          }
        }
        m_bSortAscending = xmlreader.GetValueAsBool("tvscheduler", "sortascending", true);
        if (btnSeries != null)
        {
          btnSeries.Selected = xmlreader.GetValueAsBool("tvscheduler", "series", false);
        }
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
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
        }
        xmlwriter.SetValueAsBool("tvscheduler", "sortascending", m_bSortAscending);
        xmlwriter.SetValueAsBool("tvscheduler", "series", btnSeries.Selected);
      }
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvschedulerserver.xml");
      LoadSettings();
      return bResult;
    }


    public override void OnAction(Action action)
    {
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      TVHome.ShowTvEngineSettingsUIIfConnectionDown();

      base.OnPageLoad();
      if (btnPriorities != null)
      {
        btnPriorities.Visible = false;
      }
      //@      ConflictManager.OnConflictsUpdated += new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);      

      LoadSettings();
      needUpdate = false;
      selectedItem = null;
      LoadDirectory();

      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
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

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnSeries)
      {
        LoadDirectory();
        return;
      }
      if (control == btnSortBy) // sort by
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(495); //Sort Options
        dlg.AddLocalizedString(620); //channel
        dlg.AddLocalizedString(621); //date
        dlg.AddLocalizedString(268); //title

        // set the focus to currently used sort method
        dlg.SelectedLabel = (int)currentSortMethod;

        // show dialog and wait for result
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1)
        {
          return;
        }

        currentSortMethod = (SortMethod)dlg.SelectedLabel;
        OnSort();
      }
      if (control == listSchedules)
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
      OnShowContextMenu(GetSelectedItemNo(), false);
    }

    #endregion

    #region list management

    private GUIListItem GetItem(int index)
    {
      if (index < 0 || index >= listSchedules.Count)
      {
        return null;
      }
      return listSchedules[index];
    }

    private int GetSelectedItemNo()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, listSchedules.GetID, 0, 0,
                                      null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }

    private int GetItemCount()
    {
      return listSchedules.Count;
    }

    #endregion

    #region Sort Members

    private void OnSort()
    {
      SetLabels();
      listSchedules.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }

      int iComp = 0;
      //TimeSpan ts;
      Schedule rec1 = (Schedule)item1.TVTag;
      Schedule rec2 = (Schedule)item2.TVTag;

      //0=Recording->1=Finished->2=Waiting->3=Canceled
      int type1 = 2, type2 = 2;
      if (item1.Label3 == GUILocalizeStrings.Get(682))
      {
        type1 = 0;
      }
      else if (item1.Label3 == GUILocalizeStrings.Get(683))
      {
        type1 = 1;
      }
      else if (item1.Label3 == GUILocalizeStrings.Get(681))
      {
        type1 = 2;
      }
      else if (item1.Label3 == GUILocalizeStrings.Get(684))
      {
        type1 = 3;
      }


      if (item2.Label3 == GUILocalizeStrings.Get(682))
      {
        type2 = 0;
      }
      else if (item2.Label3 == GUILocalizeStrings.Get(683))
      {
        type2 = 1;
      }
      else if (item2.Label3 == GUILocalizeStrings.Get(681))
      {
        type2 = 2;
      }
      else if (item2.Label3 == GUILocalizeStrings.Get(684))
      {
        type2 = 3;
      }
      if (type1 == 0 && type2 != 0)
      {
        return -1;
      }
      if (type2 == 0 && type1 != 0)
      {
        return 1;
      }

      switch (currentSortMethod)
      {
        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.ProgramName, rec2.ProgramName, true);
            if (iComp == 0)
            {
              goto case SortMethod.Channel;
            }
            else
            {
              return iComp;
            }
          }
          else
          {
            iComp = String.Compare(rec2.ProgramName, rec1.ProgramName, true);
            if (iComp == 0)
            {
              goto case SortMethod.Channel;
            }
            else
            {
              return iComp;
            }
          }

        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Channel.DisplayName, rec2.Channel.DisplayName, true);
            if (iComp == 0)
            {
              goto case SortMethod.Date;
            }
            else
            {
              return iComp;
            }
          }
          else
          {
            iComp = String.Compare(rec2.Channel.DisplayName, rec1.Channel.DisplayName, true);
            if (iComp == 0)
            {
              goto case SortMethod.Date;
            }
            else
            {
              return iComp;
            }
          }

        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime == rec2.StartTime)
            {
              return 0;
            }
            if (rec1.StartTime > rec2.StartTime)
            {
              return 1;
            }
            return -1;
          }
          else
          {
            if (rec2.StartTime == rec1.StartTime)
            {
              return 0;
            }
            if (rec2.StartTime > rec1.StartTime)
            {
              return 1;
            }
            return -1;
          }
      }
      return 0;
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
      bool conflicting = (schedule.Conflicts.Count > 0);
      ProgramBLL program = new ProgramBLL(ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByTitleTimesAndChannel(schedule.ProgramName, schedule.StartTime,
                                                               schedule.EndTime, schedule.IdChannel));
      bool isPartialRecording = (program != null) ? program.IsPartialRecordingSeriesPending : false;

      if (schedule.ScheduleType != (int)(ScheduleRecordingType.Once))
      {
        if (conflicting)
        {
          item.PinImage = isPartialRecording
                            ? Thumbs.TvConflictPartialRecordingSeriesIcon
                            : Thumbs.TvConflictRecordingSeriesIcon;
        }
        else
        {
          item.PinImage = isPartialRecording ? Thumbs.TvPartialRecordingSeriesIcon : Thumbs.TvRecordingSeriesIcon;
        }
      }
      else
      {
        if (conflicting)
        {
          item.PinImage = isPartialRecording ? Thumbs.TvConflictPartialRecordingIcon : Thumbs.TvConflictRecordingIcon;
        }
        else
        {
          item.PinImage = isPartialRecording ? Thumbs.TvPartialRecordingIcon : Thumbs.TvRecordingIcon;
        }
      }
      item.ThumbnailImage = strLogo;
      item.IconImageBig = strLogo;
      item.IconImage = strLogo;
      return item;
    }

    private void LoadDirectory()
    {
      IList<Conflict> conflictsList = ServiceAgents.Instance.ConflictServiceAgent.ListAllConflicts().ToList();
      btnConflicts.Visible = conflictsList.Count > 0;
      GUIControl.ClearControl(GetID, listSchedules.GetID);
      IList<ScheduleBLL> schedulesList = new List<ScheduleBLL>();
                  
      foreach (var schedule in ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules())
      {
        var scheduleBll = new ScheduleBLL(schedule);
        schedulesList.Add(scheduleBll);
      }


      int total = 0;
      bool showSeries = btnSeries.Selected;

      if ((selectedItem != null) && (showSeries))
      {
        IList<Schedule> seriesList = TVHome.Util.GetRecordingTimes(selectedItem);

        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        listSchedules.Add(item);
        //don't increment total for ".."
        //total++;

        foreach (Schedule schedule in seriesList)
        {
          if (DateTime.Now > schedule.EndTime)
          {
            continue;
          }
          if (schedule.Canceled != ScheduleFactory.MinSchedule)
          {
            continue;
          }

          item = Schedule2ListItem(schedule);
          item.TVTag = schedule;
          listSchedules.Add(item);
          total++;
        }
      }
      else
      {
        foreach (ScheduleBLL rec in schedulesList)
        {
          GUIListItem item = new GUIListItem();
          if (rec.Entity.ScheduleType != (int)ScheduleRecordingType.Once)
          {
            if (showSeries)
            {
              item = Schedule2ListItem(rec.Entity);
              item.TVTag = rec.Entity;
              item.IsFolder = true;
              listSchedules.Add(item);
              total++;
            }
            else
            {
              IList<Schedule> seriesList = TVHome.Util.GetRecordingTimes(rec);
              for (int serieNr = 0; serieNr < seriesList.Count; ++serieNr)
              {
                Schedule recSeries = (Schedule)seriesList[serieNr];
                if (DateTime.Now > recSeries.EndTime)
                {
                  continue;
                }
                if (recSeries.Canceled != ScheduleFactory.MinSchedule)
                {
                  continue;
                }

                item = Schedule2ListItem(recSeries);
                item.MusicTag = rec.Entity;
                item.TVTag = recSeries;
                listSchedules.Add(item);
                total++;
              }
            }
          } //if (recs.Count > 1 && currentSortMethod == SortMethod.Date)
          else
          {
            //single recording
            if (showSeries)
            {
              continue; // do not show single recordings if showSeries is enabled
            }
            if (rec.IsSerieIsCanceled(rec.Entity.StartTime, rec.Entity.IdChannel))
            {
              continue;
            }

            //Test if this is an instance of a series recording, if so skip it.
            if (rec.Entity.ParentSchedule != null)
            {
              continue;
            }
            item = Schedule2ListItem(rec.Entity);
            item.TVTag = rec.Entity;
            listSchedules.Add(item);
            total++;
          }
        } //foreach (Schedule rec in itemlist)
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(total));
      if (total == 0)
      {
        SetProperties(null, null);
      }

      OnSort();
      UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
      string strLine = String.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Channel:
          strLine = GUILocalizeStrings.Get(620); // Sort by: Channel
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(621); // Sort by: Date
          break;
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(268); // Sort by: Title
          break;
      }

      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, strLine);
      btnSortBy.IsAscending = m_bSortAscending;
    }

    private void SetLabels()
    {
      bool showSeries = btnSeries.Selected;

      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label.Equals(".."))
        {
          continue;
        }
        Schedule rec = (Schedule)item.TVTag;        
        item.Label = TVUtil.GetDisplayTitle(rec);


        if (showSeries)
        {
          string strTime;
          string strType;
          string day;

          switch (rec.ScheduleType)
          {
            case (int)ScheduleRecordingType.Once:
              item.Label2 = String.Format("{0} {1} - {2}",
                                          Utils.GetShortDayString(rec.StartTime),
                                          rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                          rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
              ;
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
            case (int)ScheduleRecordingType.EveryTimeOnThisChannel:
              item.Label2 = GUILocalizeStrings.Get(650, new object[] {rec.Channel.DisplayName});
              break;
            case (int)ScheduleRecordingType.EveryTimeOnEveryChannel:
              item.Label2 = GUILocalizeStrings.Get(651);
              break;
           case (int) ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
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
             item.Label2 = GUILocalizeStrings.Get(990001, new object[] { day, rec.Channel.DisplayName });
             break;              
        }
        }
        else
        {
          Program program = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByTitleTimesAndChannel(rec.ProgramName, rec.StartTime,
                                                      rec.EndTime, rec.IdChannel);
          if (program != null)
          {
            item.Label2 = Utils.GetNamedDateStartEnd(rec.StartTime, rec.EndTime);
          }
          else
          {
              item.Label2 = String.Empty;
          }
        }
      }
    }

    private void OnClick(int iItem)
    {
      OnShowContextMenu(GetSelectedItemNo(), true);
    }


    private void OnShowContextMenu(int iItem, bool clicked)
    {
      m_iSelectedItem = iItem;
      GUIListItem item = GetItem(iItem);
      if (item == null)
      {
        return;
      }

      if (item.IsFolder && clicked)
      {
        bool noitems = false;
        if (item.Label == "..")
        {
          if (selectedItem.Entity != null)
          {
            selectedItem.Entity = null;
          }
          LoadDirectory();
          return;
        }
        if (selectedItem == null)
        {
          IList<Schedule> seriesList = TVHome.Util.GetRecordingTimes(new ScheduleBLL(item.TVTag as Schedule));
          if (seriesList.Count < 1)
          {
            noitems = true; // no items existing
          }
          else
          {
            selectedItem = new ScheduleBLL(item.TVTag as Schedule);
          }
        }
        if (noitems == false)
        {
          LoadDirectory();
          return;
        }
      }

      bool showSeries = btnSeries.Selected;

      ScheduleBLL rec = new ScheduleBLL(item.TVTag as Schedule);
      if (rec.Entity == null)
      {
        return;
      }

      this.LogInfo("OnShowContextMenu: Rec = {0}", rec.ToString());
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(rec.Entity.ProgramName);

      if (showSeries && item.IsFolder)
      {
        dlg.AddLocalizedString(982); //Cancel this show (618=delete)
        dlg.AddLocalizedString(888); //Episodes management
      }
      else if (rec.Entity.Series == false)
      {
        dlg.AddLocalizedString(618); //delete
      }
      else
      {
        dlg.AddLocalizedString(981); //Cancel this show
        dlg.AddLocalizedString(982); //Delete this entire recording
        dlg.AddLocalizedString(888); //Episodes management
      }


      bool isRec = ServiceAgents.Instance.ScheduleServiceAgent.IsScheduleRecording(rec.Entity.IdSchedule); //TVHome.IsRecordingSchedule(rec, null, out card);

      if (isRec)
      {
        dlg.AddLocalizedString(979); //Play recording from beginning
        dlg.AddLocalizedString(980); //Play recording from live point
      }

      //Schedule schedDB = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(rec.id_Schedule);
      //if (schedDB.scheduleType != (int)ScheduleRecordingType.Once)
      //{
      dlg.AddLocalizedString(1048); // settings
      //}

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      bool isSchedRec = ServiceAgents.Instance.ScheduleServiceAgent.IsScheduleRecording(rec.Entity.IdSchedule);
      //TVHome.IsRecordingSchedule(rec, null, out card);

      string fileName = "";
      if (isSchedRec)
      {
        IVirtualCard card;
        bool isCardRec = ServiceAgents.Instance.ControllerServiceAgent.IsRecording(rec.Entity.IdChannel, out card);
        if (isCardRec && card != null)
        {
          fileName = card.RecordingFileName;
        }
      }
      this.LogInfo("recording fname:{0}", fileName);
      switch (dlg.SelectedId)
      {
        case 888: ////Episodes management
          TvPriorities.OnSetEpisodesToKeep(rec.Entity);
          break;

        case 1048: ////settings
          Schedule schedule = item.MusicTag as Schedule;
          if (schedule == null)
          {
            schedule = item.TVTag as Schedule;
          }
          if (schedule != null)
          {
            TVProgramInfo.CurrentRecording = new ScheduleBLL(schedule);           
            GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_PROGRAM_INFO);
          }
          return;
        case 882: ////Quality settings
          TvPriorities.OnSetQuality(rec.Entity);
          break;

        case 981: //Cancel this show
          {
            // get the program that this episode is for
            IList<Program> progs = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByChannelAndStartEndTimes(rec.Entity.IdChannel, rec.Entity.StartTime, rec.Entity.EndTime).ToList();
            // pick up the schedule that is actually used for recording
            // see TVUtil.GetRecordingTimes where schedules are all spawend as one off types
            // and this is what rec is (ie. it does not actually exist in the database)
            var realSchedule = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(rec.Entity.IdParentSchedule.GetValueOrDefault()) ?? rec.Entity;
            bool res = TVUtil.DeleteRecAndSchedWithPrompt(realSchedule, progs[0]);
            if (res)
            {
              LoadDirectory();
            }
          }
          break;

        case 982: //Delete series recording
          goto case 618;

        case 618: // delete entire recording
          {
            bool res = TVUtil.DeleteRecAndEntireSchedWithPrompt(rec.Entity, rec.Entity.StartTime);
            if (res)
            {
              if (showSeries && !item.IsFolder)
              {
                OnShowContextMenu(0, true);
                return;
              }
              else
              {
                LoadDirectory();
              }
            }
          }
          break;

        case 979: // Play recording from beginning
          {
            Recording recDB = ServiceAgents.Instance.RecordingServiceAgent.GetRecordingByFileName(fileName);
            if (recDB != null)
            {
              TVUtil.PlayRecording(recDB);
            }
          }
          return;

        case 980: // Play recording from live point
          {
            TVHome.ViewChannelAndCheck(rec.Entity.Channel, 0);
            if (g_Player.Playing)
            {
              g_Player.ShowFullScreenWindow();
            }
          }
          break;
      }
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, listSchedules.GetID, m_iSelectedItem);
    }
   
    private void ChangeType(Schedule rec)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616)); //616=Select Recording type
        for (int i = 611; i <= 615; ++i)
        {
          dlg.Add(GUILocalizeStrings.Get(i));
        }
        dlg.Add(GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.Record_WorkingDays)));
        dlg.Add(GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.Record_WeekendDays)));
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
          case 7://Weekly everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel;
            rec.Canceled = ScheduleFactory.MinSchedule;
            break;
        }        
        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(rec);
        
        ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
        LoadDirectory();
      }
    }

    private void OnNewShedule()
    {
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_SEARCHTYPE);
    }

    private void OnCleanup()
    {
      int iCleaned = 0;
      IList<Schedule> itemlist = ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules().ToList();
      foreach (Schedule rec in itemlist)
      {
        ScheduleBLL scheduleBll = new ScheduleBLL(rec);
        if (scheduleBll.IsDone() || rec.Canceled != ScheduleFactory.MinSchedule)
        {
          iCleaned++;
          Schedule r = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(rec.IdSchedule);
          ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(r.IdSchedule);
        }
      }
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      LoadDirectory();
      if (pDlgOK != null)
      {
        pDlgOK.SetHeading(624);
        pDlgOK.SetLine(1, String.Format("{0} {1}", GUILocalizeStrings.Get(625), iCleaned));
        pDlgOK.SetLine(2, String.Empty);
        pDlgOK.DoModal(GetID);
      }
    }

    private void SetProperties(Schedule rec)
    {
      string strTime = String.Format("{0} {1} - {2}",
                                     Utils.GetShortDayString(rec.StartTime),
                                     rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                     rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.ProgramName);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", "");
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", "");

      if (rec.IdChannel < 0)
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
      else
      {
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
    }

    public void SetProperties(Schedule schedule, Program prog)
    {
      GUIPropertyManager.SetProperty("#TV.Scheduled.Ritle", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", String.Empty);

      if (prog != null)
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Title", TVUtil.GetDisplayTitle(prog));
        GUIPropertyManager.SetProperty("#TV.Scheduled.Gescription", prog.Description);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", TVUtil.GetCategory(prog.ProgramCategory));
      }

      if (schedule != null)
      {
        string strTime = String.Format("{0} {1} - {2}",
                                       Utils.GetShortDayString(schedule.StartTime),
                                       schedule.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       schedule.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        GUIPropertyManager.SetProperty("#TV.Scheduled.Time", strTime);

        if (schedule.IdChannel < 0)
        {
          GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", "defaultVideoBig.png");
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", schedule.Channel.DisplayName);
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
      }
    }

    private void UpdateDescription()
    {
      Schedule rec = ScheduleFactory.CreateSchedule(-1, "", ScheduleFactory.MinSchedule, ScheduleFactory.MinSchedule);      
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

    #endregion

    public override void Process()
    {
      if (needUpdate)
      {
        needUpdate = false;
        LoadDirectory();
      }
      TVHome.UpdateProgressPercentageBar();
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != SortOrder.Descending;

      OnSort();
    }
  }
}