#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

using TvDatabase;
using TvControl;
using Gentle.Common;
using Gentle.Framework;

namespace TvPlugin
{
  /// <summary>
  /// Summary description for GUITVProgramInfo.
  /// </summary>
  public class TVProgramInfo : GUIWindow
  {
    [SkinControlAttribute(17)]     protected GUILabelControl lblProgramGenre = null;
    [SkinControlAttribute(15)]     protected GUITextScrollUpControl lblProgramDescription = null;
    [SkinControlAttribute(14)]     protected GUILabelControl lblProgramTime = null;
    [SkinControlAttribute(13)]     protected GUIFadeLabel lblProgramTitle = null;
    [SkinControlAttribute(16)]     protected GUIFadeLabel lblProgramChannel = null;
    [SkinControlAttribute(2)]      protected GUIButtonControl btnRecord = null;
    [SkinControlAttribute(3)]      protected GUIButtonControl btnAdvancedRecord = null;
    [SkinControlAttribute(4)]      protected GUIButtonControl btnKeep = null;
    [SkinControlAttribute(5)]      protected GUIToggleButtonControl btnNotify = null;
    [SkinControlAttribute(10)]     protected GUIListControl lstUpcomingEpsiodes = null;
    [SkinControlAttribute(6)]      protected GUIButtonControl btnQuality = null;
    [SkinControlAttribute(7)]      protected GUIButtonControl btnEpisodes = null;
    [SkinControlAttribute(8)]      protected GUIButtonControl btnPreRecord = null;
    [SkinControlAttribute(9)]      protected GUIButtonControl btnPostRecord = null;
    static Program currentProgram = null;

    public TVProgramInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO;//748
    }

    public override void OnAdded()
    {
      Log.Debug("TVProgramInfo:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO, this);
      Restore();
      PreInit();
      ResetAllControls();
    }

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvprogram.xml");
      return bResult;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      Update();
    }

    static public Program CurrentProgram
    {
      get { return currentProgram; }
      set { currentProgram = value; }
    }

    static public Schedule CurrentRecording
    {
      set
      {
        CurrentProgram = null;
        IList programs = new ArrayList();
        TvBusinessLayer layer = new TvBusinessLayer();
        programs = layer.GetPrograms(DateTime.Now, DateTime.Now.AddDays(10));
        foreach (Program prog in programs)
        {
          if (value.IsRecordingProgram(prog, false))
          {
            CurrentProgram = prog;
            return;
          }
        }
      }
    }

    void UpdateProgramDescription(Schedule rec, Program program)
    {
      if (program == null) return;
      /*
      if (rec == null) return;
      IList progs = new ArrayList();
      TvBusinessLayer layer = new TvBusinessLayer();
      progs = layer.GetPrograms(rec.ReferencedChannel(), rec.StartTime, rec.EndTime);
      if (progs.Count > 0)
      {
        foreach (Program prog in progs)
        {
          if (prog.StartTime == rec.StartTime && prog.EndTime == rec.EndTime && prog.ReferencedChannel() == rec.ReferencedChannel())
          {
            currentProgram = prog;
            break;
          }
        }
      }
      if (currentProgram != null)
      {
        string strTime = String.Format("{0} {1} - {2}",
          Utils.GetShortDayString(currentProgram.StartTime),
          currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
          currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        lblProgramGenre.Label = currentProgram.Genre;
        lblProgramTime.Label = strTime;
        lblProgramDescription.Label = currentProgram.Description;
        lblProgramTitle.Label = currentProgram.Title;
      }*/
      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(program.StartTime),
        program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = program.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = program.Description;
      lblProgramTitle.Label = program.Title;
    }

    void Update()
    {
      lstUpcomingEpsiodes.Clear();
      if (currentProgram == null) return;

      //set program description
      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(currentProgram.StartTime),
        currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = currentProgram.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = currentProgram.Description;
      lblProgramTitle.Label = currentProgram.Title;

      //check if we are recording this program
      IList schedules = Schedule.ListAll();
      bool isRecording = false;
      bool isSeries = false;
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.IsRecordingProgram(currentProgram, true))
        {
          if (!schedule.IsSerieIsCanceled(currentProgram.StartTime))
          {
            if ((ScheduleRecordingType)schedule.ScheduleType != ScheduleRecordingType.Once)
              isSeries = true;
            isRecording = true;
            break;
          }
        }
      }

      if (isRecording)
      {
        btnRecord.Label = GUILocalizeStrings.Get(1039);//dont record
        btnAdvancedRecord.Disabled = true;
        btnKeep.Disabled = false;
        btnQuality.Disabled = false;
        btnEpisodes.Disabled = !isSeries;
        btnPreRecord.Disabled = false;
        btnPostRecord.Disabled = false;
      }
      else
      {
        btnRecord.Label = GUILocalizeStrings.Get(264);//record
        btnAdvancedRecord.Disabled = false;
        btnKeep.Disabled = true;
        btnQuality.Disabled = true;
        btnEpisodes.Disabled = true;
        btnPreRecord.Disabled = true;
        btnPostRecord.Disabled = true;
      }
      btnNotify.Selected = currentProgram.Notify;

      //find upcoming episodes
      lstUpcomingEpsiodes.Clear();
      TvBusinessLayer layer = new TvBusinessLayer();
      DateTime dtDay = DateTime.Now;
      IList episodes = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(14), currentProgram.Title, null);

      foreach (Program episode in episodes)
      {
        GUIListItem item = new GUIListItem();
        item.Label = episode.Title;
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, episode.ReferencedChannel().Name);
        if (!System.IO.File.Exists(logo))
        {
          item.Label = String.Format("{0} {1}", episode.ReferencedChannel().Name,episode.Title);
          logo = "defaultVideoBig.png";
        }
        Schedule recordingSchedule;
        if (IsRecordingProgram(episode, out recordingSchedule, false))
        {
          if (false == recordingSchedule.IsSerieIsCanceled(episode.StartTime))
          {
            if (recordingSchedule.ReferringConflicts().Count > 0)
            {
              item.PinImage = Thumbs.TvConflictRecordingIcon;
            }
            else
            {
              item.PinImage = Thumbs.TvRecordingIcon;
            }
          }
          item.TVTag = recordingSchedule;
        }
        item.MusicTag = episode;
        item.ThumbnailImage = logo;
        item.IconImageBig = logo;
        item.IconImage = logo;
        item.Label2 = String.Format("{0} {1} - {2}",
                                  Utils.GetShortDayString(episode.StartTime),
                                  episode.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  episode.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)); ;

        lstUpcomingEpsiodes.Add(item);
      }
    }
    bool IsRecordingProgram(Program program, out Schedule recordingSchedule, bool filterCanceledRecordings)
    {
      recordingSchedule = null;
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.IsRecordingProgram(program, filterCanceledRecordings))
        {
          recordingSchedule = schedule;
          return true;
        }
      }
      return false;
    }


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnPreRecord)
        OnPreRecordInterval();
      if (control == btnPostRecord)
        OnPostRecordInterval();
      if (control == btnEpisodes)
        OnSetEpisodes();
      if (control == btnQuality)
        OnSetQuality();
      if (control == btnKeep)
        OnKeep();
      if (control == btnRecord)
        OnRecordProgram(currentProgram);
      if (control == btnAdvancedRecord)
        OnAdvancedRecord();
      if (control == btnNotify)
        OnNotify();
      if (control == lstUpcomingEpsiodes)
      {
        GUIListItem item = lstUpcomingEpsiodes.SelectedListItem;
        if (item != null)
        {
          Schedule schedule = item.TVTag as Schedule;
          Program episode = item.MusicTag as Program;
          OnEpisodeClicked(episode, schedule);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }
    void OnPreRecordInterval()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1444));//pre-record
        dlg.Add(GUILocalizeStrings.Get(886));//default
        for (int minute = 0; minute < 20; minute++)
        {
          dlg.Add(String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004)));
        }
        if (rec.PreRecordInterval < 0) dlg.SelectedLabel = 0;
        else dlg.SelectedLabel = rec.PreRecordInterval + 1;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0) return;
        rec.PreRecordInterval = dlg.SelectedLabel - 1;
        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
      }
      Update();
    }

    void OnPostRecordInterval()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1445));//pre-record
        dlg.Add(GUILocalizeStrings.Get(886));//default
        for (int minute = 0; minute < 20; minute++)
        {
          dlg.Add(String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004)));
        }
        if (rec.PostRecordInterval < 0) dlg.SelectedLabel = 0;
        else dlg.SelectedLabel = rec.PostRecordInterval + 1;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0) return;
        rec.PostRecordInterval = dlg.SelectedLabel - 1;
        rec.Persist();
      }
      Update();
    }

    void OnSetQuality()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;
      ///@
      ///GUITVPriorities.OnSetQuality(rec);
      Update();
    }
    void OnSetEpisodes()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;

      TvPriorities.OnSetEpisodesToKeep(rec);
      Update();
    }

    void OnEpisodeClicked(Program episode, Schedule schedule)
    {
      if (schedule != null)
      {
        //no schedule yet for this epsiode
        if (schedule.IsSerieIsCanceled(episode.StartTime))
        {
          schedule.UnCancelSerie(episode.StartTime);
          schedule.Persist();
          TvServer server = new TvServer();
          server.OnNewSchedule();
          Update();
        }
        else
        {
          CanceledSchedule canceled = new CanceledSchedule(schedule.IdSchedule, episode.StartTime);
          canceled.Persist();
          TvServer server = new TvServer();
          server.OnNewSchedule();
          Update();
        }
      }
      else
      {
        OnRecordProgram(episode);
      }
    }

    void OnRecordProgram(Program program)
    {
      Schedule recordingSchedule;
      if (IsRecordingProgram(program, out  recordingSchedule, false))
      {
        //already recording this program
        if (recordingSchedule.ScheduleType != (int)ScheduleRecordingType.Once)
        {
          GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg == null)
            return;
          dlg.Reset();
          dlg.SetHeading(program.Title);
          dlg.AddLocalizedString(981);//Delete this recording
          dlg.AddLocalizedString(982);//Delete series recording
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel == -1)
            return;
          switch (dlg.SelectedId)
          {
            case 981: //Delete this recording only
              {
                if (CheckIfRecording(recordingSchedule))
                {
                  //delete specific series
                  CanceledSchedule canceledSchedule = new CanceledSchedule(recordingSchedule.IdSchedule, program.StartTime);
                  canceledSchedule.Persist();
                  TvServer server = new TvServer();
                  server.StopRecordingSchedule(recordingSchedule.IdSchedule);
                  server.OnNewSchedule();
                }
              }
              break;
            case 982: //Delete entire recording
              {
                if (CheckIfRecording(recordingSchedule))
                {
                  //cancel recording
                  TvServer server = new TvServer();
                  server.StopRecordingSchedule(recordingSchedule.IdSchedule);
                  recordingSchedule.Delete();
                  server.OnNewSchedule();
                }
              }
              break;
          }
        }
        else
        {
          if (CheckIfRecording(recordingSchedule))
          {
            TvServer server = new TvServer();
            server.StopRecordingSchedule(recordingSchedule.IdSchedule);
            recordingSchedule.Delete();
            server.OnNewSchedule();
          }
        }
      }
      else
      {
        //not recording this program
        // check if this program is conflicting with any other already scheduled recording
        TvBusinessLayer layer = new TvBusinessLayer();
        Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
        rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        if (SkipForConflictingRecording(rec)) return;

        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
      }
      Update();
    }

    void OnAdvancedRecord()
    {
      if (currentProgram == null)
        return;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Schedule type
        //610=None
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
        for (int i = 611; i <= 615; ++i)
        {
          dlg.AddLocalizedString(i);
        }
        dlg.AddLocalizedString(672);// 672=Record Mon-Fri
        dlg.AddLocalizedString(1051);// 1051=Record Sat-Sun

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1) return;

        Schedule rec = new Schedule(currentProgram.IdChannel, currentProgram.Title, currentProgram.StartTime, currentProgram.EndTime);
        switch (dlg.SelectedId)
        {
          case 611://once
            rec.ScheduleType = (int)ScheduleRecordingType.Once;
            break;
          case 612://everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            break;
          case 613://everytime, all channels
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            break;
          case 614://weekly
            rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
            break;
          case 615://daily
            rec.ScheduleType = (int)ScheduleRecordingType.Daily;
            break;
          case 672://Mo-Fi
            rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
            break;
          case 1051://Record Sat-Sun
            rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
            break;
        }
        if (SkipForConflictingRecording(rec)) return;

        TvBusinessLayer layer = new TvBusinessLayer();
        rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();

        //check if this program is interrupted (for example by a news bulletin)
        //ifso ask the user if he wants to record the 2nd part also
        IList programs = new ArrayList();
        DateTime dtStart = rec.EndTime.AddMinutes(1);
        DateTime dtEnd = dtStart.AddHours(3);
        long iStart = Utils.datetolong(dtStart);
        long iEnd = Utils.datetolong(dtEnd);
        programs = layer.GetPrograms(rec.ReferencedChannel(), dtStart, dtEnd);
        if (programs.Count >= 2)
        {
          Program next = programs[0] as Program;
          Program nextNext = programs[1] as Program;
          if (nextNext.Title == rec.ProgramName)
          {
            TimeSpan ts = next.EndTime - next.StartTime;
            if (ts.TotalMinutes <= 40)
            {
              //
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              dlgYesNo.SetHeading(1012);//This program will be interrupted by
              dlgYesNo.SetLine(1, next.Title);
              dlgYesNo.SetLine(2, 1013);//Would you like to record the second part also?
              dlgYesNo.DoModal(GetID);
              if (dlgYesNo.IsConfirmed)
              {
                rec = new Schedule(currentProgram.IdChannel, currentProgram.Title, nextNext.StartTime, nextNext.EndTime);

                rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                rec.Persist();
                server.OnNewSchedule();
              }
            }
          }
        }

      }
      Update();
    }

    bool CheckIfRecording(Schedule rec)
    {

      VirtualCard card;
      TvServer server = new TvServer();
      if (!server.IsRecordingSchedule(rec.IdSchedule, out card)) return true;
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo) return true;
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
      dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
      dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

      if (dlgYesNo.IsConfirmed)
      {
        return true;
      }
      return false;
    }

    void OnNotify()
    {
      currentProgram.Notify = !currentProgram.Notify;
      currentProgram.Persist();
      Update();
      TvNotifyManager.OnNotifiesChanged();
    }

    void OnKeep()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;


      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(1042);
      dlg.AddLocalizedString(1043);//Until watched
      dlg.AddLocalizedString(1044);//Until space needed
      dlg.AddLocalizedString(1045);//Until date
      dlg.AddLocalizedString(1046);//Always
      switch (rec.KeepMethod)
      {
        case (int)KeepMethodType.UntilWatched:
          dlg.SelectedLabel = 0;
          break;
        case (int)KeepMethodType.UntilSpaceNeeded:
          dlg.SelectedLabel = 1;
          break;
        case (int)KeepMethodType.TillDate:
          dlg.SelectedLabel = 2;
          break;
        case (int)KeepMethodType.Always:
          dlg.SelectedLabel = 3;
          break;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      switch (dlg.SelectedId)
      {
        case 1043:
          rec.KeepMethod = (int)KeepMethodType.UntilWatched;
          break;
        case 1044:
          rec.KeepMethod = (int)KeepMethodType.UntilSpaceNeeded;

          break;
        case 1045:
          rec.KeepMethod = (int)KeepMethodType.TillDate;
          dlg.Reset();
          dlg.ShowQuickNumbers = false;
          dlg.SetHeading(1045);
          for (int iDay = 1; iDay <= 100; iDay++)
          {
            DateTime dt = currentProgram.StartTime.AddDays(iDay);
            dlg.Add(dt.ToLongDateString());
          }
          TimeSpan ts = (rec.KeepDate - currentProgram.StartTime);
          int days = (int)ts.TotalDays;
          if (days >= 100) days = 30;
          dlg.SelectedLabel = days - 1;
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel < 0) return;
          rec.KeepDate = currentProgram.StartTime.AddDays(dlg.SelectedLabel + 1);
          break;
        case 1046:
          rec.KeepMethod = (int)KeepMethodType.Always;
          break;
      }
      rec.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();

    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      UpdateProgramDescription(item.TVTag as Schedule, item.MusicTag as Program);
    }

    private bool SkipForConflictingRecording(Schedule rec)
    {
      Log.Info("SkipForConflictingRecording: Schedule = " + rec.ToString());

      TvBusinessLayer layer = new TvBusinessLayer();
			
/*			Setting setting = layer.GetSetting("CMLastUpdateTime", DateTime.Now.ToString());
			string lastUpdate = setting.Value;
			Log.Info("SkipForConflictingRecording: LastUpDateTime = " + setting.Value);
			
			rec.Persist();            // save it for the ConflictManager
			TvServer server = new TvServer();
			server.OnNewSchedule();   // inform ConflictManger

			int counter = 0;
			while ((lastUpdate.Equals(setting.Value)) && (counter++ < 20)) // wait until Conflict Manager has done his job
			{
				Thread.Sleep(500);
				setting = layer.GetSetting("CMLastUpdateTime", DateTime.Now.ToString());
				Log.Info("SkipForConflictingRecording: LastUpDateTime = " + setting.Value);
			}
			Log.Info("SkipForConflictingRecording: rec.IdSchedule = " + rec.IdSchedule.ToString());
			IList conflicts = rec.ConflictingSchedules();
			Log.Info("SkipForConflictingRecording: 1.Conflicts.Count = " + conflicts.Count.ToString());

			rec.Delete();           // for testing -> toDo: add Schedule handling in the functions below
			server.OnNewSchedule(); // inform Conflict Manager

			if (conflicts.Count < 1)
			{
				Log.Info("SkipForConflictingRecording: Start 2nd try");
				conflicts = layer.GetConflictingSchedules(rec);
				Log.Info("SkipForConflictingRecording: 2.Conflicts.Count = " + conflicts.Count.ToString());
			}
*/
			IList conflicts = layer.GetConflictingSchedules(rec);
      if (conflicts.Count > 0)
      {
        GUIDialogTVConflict dlg = (GUIDialogTVConflict)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TVCONFLICT);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(GUILocalizeStrings.Get(879));   // "recording conflict"
          foreach (Schedule conflict in conflicts)
          {
            Log.Info("SkipForConflictingRecording: Conflicts = " + conflict.ToString());

            GUIListItem item = new GUIListItem(conflict.ProgramName);
            item.Label2 = GetRecordingDateTime(conflict);
            item.Label3 = conflict.IdChannel.ToString();
            item.TVTag = conflict;
            dlg.AddConflictRecording(item);
          }
          dlg.DoModal(GetID);
          switch (dlg.SelectedLabel)
          {
            case 0: return true;   // Skip new Recording
            case 1:                // Don't record the already scheduled one(s)
              {
                foreach (Schedule conflict in conflicts)
                {
                  Program prog = new Program(conflict.IdChannel, conflict.StartTime, conflict.EndTime, conflict.ProgramName, "-", "-", false, DateTime.MinValue, string.Empty, string.Empty, -1, string.Empty);
                  OnRecordProgram(prog); 
                }
                break;
              }
            case 2: return false;   // No Skipping new Recording
            default: return true;   // Skipping new Recording
          }
        }
      }
      return false;
    }

    private string GetRecordingDateTime(Schedule rec)
    {
      return String.Format("{0} {1} - {2}",
                MediaPortal.Util.Utils.GetShortDayString(rec.StartTime),
                rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
    }
  }
}
