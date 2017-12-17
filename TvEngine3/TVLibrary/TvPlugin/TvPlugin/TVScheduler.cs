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
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using Action = MediaPortal.GUI.Library.Action;

namespace TvPlugin
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
    [SkinControl(11)] protected GUICheckButton btnSeries = null;

    private SortMethod currentSortMethod = SortMethod.Date;
    private SortMethod currentSortMethodSeries = SortMethod.Name;
    private bool m_bSortAscending = true;
    private bool m_bSortAscendingSeries = true;
    private int m_iSelectedItem = 0;
    private bool needUpdate = false;
    private Schedule selectedItem = null;


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
        strTmp = (string)xmlreader.GetValue("tvscheduler", "sortseries");
        if (strTmp != null)
        {
          if (strTmp == "channel")
          {
            currentSortMethodSeries = SortMethod.Channel;
          }
          else if (strTmp == "date")
          {
            currentSortMethodSeries = SortMethod.Date;
          }
          else if (strTmp == "name")
          {
            currentSortMethodSeries = SortMethod.Name;
          }
        }
        m_bSortAscending = xmlreader.GetValueAsBool("tvscheduler", "sortascending", true);
        m_bSortAscendingSeries = xmlreader.GetValueAsBool("tvscheduler", "sortascendingseries", true);
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
        switch (currentSortMethodSeries)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvscheduler", "sortseries", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvscheduler", "sortseries", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvscheduler", "sortseries", "name");
            break;
        }
        xmlwriter.SetValueAsBool("tvscheduler", "sortascending", m_bSortAscending);
        xmlwriter.SetValueAsBool("tvscheduler", "sortascendingseries", m_bSortAscendingSeries);
        xmlwriter.SetValueAsBool("tvscheduler", "series", btnSeries.Selected);
      }
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\mytvschedulerserver.xml"));
      LoadSettings();
      return bResult;
    }


    public override void OnAction(Action action)
    {
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      if (!TVHome.Connected)
      {
        RemoteControl.Clear();
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
        return;
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

      if (TVHome.m_navigator == null)
      {
        TVHome.m_navigator = new ChannelNavigator(); // Create the channel navigator (it will load groups and channels)
      }

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
        if (TVHome.Connected)
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
        dlg.SelectedLabel = (int)(btnSeries.Selected ? currentSortMethodSeries : currentSortMethod);

        // show dialog and wait for result
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1)
        {
          return;
        }

        if (btnSeries.Selected)
        {
          currentSortMethodSeries = (SortMethod)dlg.SelectedLabel;
        }
        else
        {
          currentSortMethod = (SortMethod)dlg.SelectedLabel;
        }
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

    private GUIListItem GetSelectedItem()
    {
      return listSchedules.SelectedListItem;
    }

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
        return 1;
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

      SortMethod sortMethod = (btnSeries.Selected ? currentSortMethodSeries : currentSortMethod);
      bool sortAscending = (btnSeries.Selected ? m_bSortAscendingSeries : m_bSortAscending);

      switch (sortMethod)
      {
        case SortMethod.Name:
          if (sortAscending)
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
          if (sortAscending)
          {
            iComp = String.Compare(rec1.ReferencedChannel().DisplayName, rec2.ReferencedChannel().DisplayName, true);
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
            iComp = String.Compare(rec2.ReferencedChannel().DisplayName, rec1.ReferencedChannel().DisplayName, true);
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
          if (sortAscending)
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
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, schedule.ReferencedChannel().DisplayName);
      
      if (string.IsNullOrEmpty(strLogo))                      
      {
        strLogo = "defaultVideoBig.png";
      }
      bool conflicting = (schedule.ReferringConflicts().Count > 0);
      Program program = Program.RetrieveByTitleTimesAndChannel(schedule.ProgramName, schedule.StartTime,
                                                               schedule.EndTime, schedule.IdChannel);
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
      IList<Conflict> conflictsList = Conflict.ListAll();
      btnConflicts.Visible = conflictsList.Count > 0;
      GUIControl.ClearControl(GetID, listSchedules.GetID);
      IList<Schedule> schedulesList = Schedule.ListAll();
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
          if (schedule.Canceled != Schedule.MinSchedule)
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
        foreach (Schedule rec in schedulesList)
        {
          GUIListItem item = new GUIListItem();
          if (rec.ScheduleType != (int)ScheduleRecordingType.Once)
          {
            if (showSeries)
            {
              item = Schedule2ListItem(rec);
              item.TVTag = rec;
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
                if (recSeries.Canceled != Schedule.MinSchedule)
                {
                  continue;
                }

                item = Schedule2ListItem(recSeries);
                item.MusicTag = rec;
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
            if (rec.IsSerieIsCanceled(rec.StartTime, rec.IdChannel))
            {
              continue;
            }

            //Test if this is an instance of a series recording, if so skip it.
            if (rec.ReferencedSchedule() != null)
            {
              continue;
            }
            item = Schedule2ListItem(rec);
            item.TVTag = rec;
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
      SortMethod sortMethod = (btnSeries.Selected ? currentSortMethodSeries : currentSortMethod);
      switch (sortMethod)
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
      btnSortBy.IsAscending = (btnSeries.Selected ? m_bSortAscendingSeries : m_bSortAscending);
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
              item.Label2 = GUILocalizeStrings.Get(650, new object[] {rec.ReferencedChannel().DisplayName});
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
             item.Label2 = GUILocalizeStrings.Get(990001, new object[] { day, rec.ReferencedChannel().DisplayName });
             break;              
        }
        }
        else
        {
          Program program = Program.RetrieveByTitleTimesAndChannel(rec.ProgramName, rec.StartTime,
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
      /*
      GUIListItem item = GetItem(iItem);
      if (item == null) return;
      TVProgramInfo.CurrentRecording = item.MusicTag as Schedule;
      if (TVProgramInfo.CurrentProgram != null)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO);
      return;
      */
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
          if (selectedItem != null)
          {
            selectedItem = null;
          }
          LoadDirectory();
          return;
        }
        if (selectedItem == null)
        {
          IList<Schedule> seriesList = TVHome.Util.GetRecordingTimes(item.TVTag as Schedule);
          if (seriesList.Count < 1)
          {
            noitems = true; // no items existing
          }
          else
          {
            selectedItem = item.TVTag as Schedule;
          }
        }
        if (noitems == false)
        {
          LoadDirectory();
          return;
        }
      }

      bool showSeries = btnSeries.Selected;

      Schedule rec = item.TVTag as Schedule;
      if (rec == null)
      {
        return;
      }

      Log.Info("OnShowContextMenu: Rec = {0}", rec.ToString());
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(rec.ProgramName);

      if (showSeries && item.IsFolder)
      {
        dlg.AddLocalizedString(982); //Cancel this show (618=delete)
        dlg.AddLocalizedString(888); //Episodes management
      }
      else if (rec.Series == false)
      {
        dlg.AddLocalizedString(618); //delete
      }
      else
      {
        dlg.AddLocalizedString(981); //Cancel this show
        dlg.AddLocalizedString(982); //Delete this entire recording
        dlg.AddLocalizedString(888); //Episodes management
      }
      VirtualCard card;
      TvServer server = new TvServer();

      bool isRec = Schedule.IsScheduleRecording(rec.IdSchedule); //TVHome.IsRecordingSchedule(rec, null, out card);

      if (isRec)
      {
        dlg.AddLocalizedString(979); //Play recording from beginning
        dlg.AddLocalizedString(980); //Play recording from live point
      }

      //Schedule schedDB = Schedule.Retrieve(rec.IdSchedule);
      //if (schedDB.ScheduleType != (int)ScheduleRecordingType.Once)
      //{
      dlg.AddLocalizedString(1048); // settings
      //}

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      bool isSchedRec = TvDatabase.Schedule.IsScheduleRecording(rec.IdSchedule);
      //TVHome.IsRecordingSchedule(rec, null, out card);

      string fileName = "";
      if (isSchedRec)
      {
        bool isCardRec = server.IsRecording(rec.ReferencedChannel().IdChannel, out card);
        if (isCardRec && card != null)
        {
          fileName = card.RecordingFileName;
        }
      }
      Log.Info("recording fname:{0}", fileName);
      switch (dlg.SelectedId)
      {
        case 888: ////Episodes management
          TvPriorities.OnSetEpisodesToKeep(rec);
          break;

        case 1048: ////settings
          Schedule schedule = item.MusicTag as Schedule;
          if (schedule == null)
          {
            schedule = item.TVTag as Schedule;
          }
          if (schedule != null)
          {
            TVProgramInfo.CurrentRecording = schedule;           
            GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_PROGRAM_INFO);
          }
          return;
        case 882: ////Quality settings
          TvPriorities.OnSetQuality(rec);
          break;

        case 981: //Cancel this show
          {
            var layer = new TvBusinessLayer();
            // get the program that this episode is for
            var progs = layer.GetPrograms(rec.ReferencedChannel(), rec.StartTime, rec.EndTime);
            // pick up the schedule that is actually used for recording
            // see TVUtil.GetRecordingTimes where schedules are all spawend as one off types
            // and this is what rec is (ie. it does not actually exist in the database)
            var realSchedule = Schedule.Retrieve(rec.IdParentSchedule) ?? rec;
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
            bool res = TVUtil.DeleteRecAndEntireSchedWithPrompt(rec, rec.StartTime);
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
            Recording recDB = Recording.Retrieve(fileName);
            if (recDB != null)
            {
              TVUtil.PlayRecording(recDB);
            }
          }
          return;

        case 980: // Play recording from live point
          {
            TVHome.ViewChannelAndCheck(rec.ReferencedChannel());
            if (g_Player.Playing)
            {
              g_Player.ShowFullScreenWindow();
            }
            /*
            g_Player.Stop();
            if (System.IO.File.Exists(fileName))
            {
              g_Player.Play(fileName, g_Player.MediaType.Recording);
              g_Player.SeekAbsolute(g_Player.Duration);
              g_Player.ShowFullScreenWindow(); 
              return;
            }
            else
            {
              string url = server.GetRtspUrlForFile(fileName);
              Log.Info("recording url:{0}", url);
              if (url.Length > 0)
              {
                g_Player.Play(url, g_Player.MediaType.Recording);

                if (g_Player.Playing)
                {
                  g_Player.SeekAbsolute(g_Player.Duration);
                  g_Player.SeekAbsolute(g_Player.Duration);
                  g_Player.ShowFullScreenWindow();
                  return;
                }
              }
            }*/
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
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 1: //everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 2: //everytime, all channels
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 3: //weekly
            rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 4: //daily
            rec.ScheduleType = (int)ScheduleRecordingType.Daily;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 5: //WorkingDays
            rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 6: //Weekends
            rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
            rec.Canceled = Schedule.MinSchedule;
            break;
          case 7://Weekly everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel;
            rec.Canceled = Schedule.MinSchedule;
            break;
        }
        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
        LoadDirectory();
      }
    }

    private void OnNewShedule()
    {
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_SEARCHTYPE);
    }

    private void OnEdit(Schedule rec)
    {
      GUIDialogDateTime dlg = (GUIDialogDateTime)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_DATETIME);
      if (dlg != null)
      {
        IList<Channel> channels = Channel.ListAll();
        dlg.SetHeading(637);
        dlg.Items.Clear();
        dlg.EnableChannel = true;
        dlg.EnableStartTime = true;
        TvServer server = new TvServer();

        if (rec.IsRecordingProgram(rec.ReferencedChannel().CurrentProgram, true))
        {
          dlg.EnableChannel = false;
          dlg.EnableStartTime = false;
        }
        foreach (Channel chan in channels)
        {
          dlg.Items.Add(chan.DisplayName);
        }
        dlg.Channel = rec.ReferencedChannel().DisplayName;
        dlg.StartDateTime = rec.StartTime;
        dlg.EndDateTime = rec.EndTime;
        dlg.DoModal(GetID);
        if (dlg.IsConfirmed)
        {
          //@rec.Channel = dlg.Channel;
          rec.EndTime = dlg.EndDateTime;
          rec.Canceled = Schedule.MinSchedule;
          rec.Persist();
          server.OnNewSchedule();
          LoadDirectory();
        }
      }
    }

    private string GetScheduleType(Schedule schedule, int type)
    {
      ScheduleRecordingType ScheduleType = (ScheduleRecordingType)type;
      string strType = String.Empty;
      switch (ScheduleType)
      {
        case ScheduleRecordingType.Daily:
          strType = GUILocalizeStrings.Get(648); //daily
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          strType = GUILocalizeStrings.Get(651); //Everytime on any channel
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          strType = String.Format(GUILocalizeStrings.Get(650), schedule.ReferencedChannel().DisplayName);
          ; //Everytime on this channel
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
          strType = String.Format(GUILocalizeStrings.Get(990000), schedule.ReferencedChannel().DisplayName); ;//Weekly Everytime on this channel
          break;
      }
      return strType;
    }

    private void OnCleanup()
    {
      int iCleaned = 0;
      IList<Schedule> itemlist = Schedule.ListAll();
      foreach (Schedule rec in itemlist)
      {
        if (rec.IsDone() || rec.Canceled != Schedule.MinSchedule)
        {
          iCleaned++;
          Schedule r = Schedule.Retrieve(rec.IdSchedule);
          r.Delete();
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
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.ProgramName);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", "");
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", TVUtil.GetRecordingDateStringFull(rec));
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", "");

      if (rec.IdChannel < 0)
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
      else
      {
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.ReferencedChannel().DisplayName);
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
      GUIPropertyManager.SetProperty("#TV.Scheduled.Title", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", String.Empty);

      if (prog != null)
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Title", TVUtil.GetDisplayTitle(prog));
        GUIPropertyManager.SetProperty("#TV.Scheduled.Description", prog.Description);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", prog.Genre);
      }

      if (schedule != null)
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Time", TVUtil.GetRecordingDateStringFull(schedule));

        if (schedule.IdChannel < 0)
        {
          GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", "defaultVideoBig.png");
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", schedule.ReferencedChannel().DisplayName);
          string logo = Utils.GetCoverArt(Thumbs.TVChannel, schedule.ReferencedChannel().DisplayName);
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
      Schedule rec = new Schedule(-1, "", Schedule.MinSchedule, Schedule.MinSchedule);
      TvBusinessLayer layer = new TvBusinessLayer();
      rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
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

      Program prog = rec.ReferencedChannel().GetProgramAt(rec.StartTime.AddMinutes(1));
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
      TVHome.UpdateProgressPercentageBar();
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      if (btnSeries.Selected)
      {
        m_bSortAscendingSeries = e.Order != SortOrder.Descending;
      }
      else
      {
        m_bSortAscending = e.Order != SortOrder.Descending;
      }
      OnSort();
    }
  }
}