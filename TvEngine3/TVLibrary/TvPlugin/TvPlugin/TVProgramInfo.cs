using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

using TvDatabase;
using TvControl;
using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
namespace TvPlugin
{
  /// <summary>
  /// Summary description for GUITVProgramInfo.
  /// </summary>
  public class TVProgramInfo : GUIWindow
  {
    [SkinControlAttribute(17)]
    protected GUILabelControl lblProgramGenre = null;
    [SkinControlAttribute(15)]
    protected GUITextScrollUpControl lblProgramDescription = null;
    [SkinControlAttribute(14)]
    protected GUILabelControl lblProgramTime = null;
    [SkinControlAttribute(13)]
    protected GUIFadeLabel lblProgramTitle = null;
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnRecord = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnAdvancedRecord = null;
    [SkinControlAttribute(4)]
    protected GUIButtonControl btnKeep = null;
    [SkinControlAttribute(5)]
    protected GUIToggleButtonControl btnNotify = null;
    [SkinControlAttribute(10)]
    protected GUIListControl lstUpcomingEpsiodes = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnQuality = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnEpisodes = null;
    [SkinControlAttribute(8)]
    protected GUIButtonControl btnPreRecord = null;
    [SkinControlAttribute(9)]
    protected GUIButtonControl btnPostRecord = null;

    static Program currentProgram = null;

    public TVProgramInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO;//748
    }

    public override void OnAdded()
    {
      Log.Write("TVProgramInfo:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO, this);
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
        List<Program> programs = new List<Program>();
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

    void UpdateProgramDescription(Schedule rec)
    {
      if (rec == null) return;
      List<Program> progs = new List<Program>();
      TvBusinessLayer layer = new TvBusinessLayer();
      progs = layer.GetPrograms(rec.Channel, rec.StartTime, rec.EndTime);
      if (progs.Count > 0)
      {
        foreach (Program prog in progs)
        {
          if (prog.StartTime == rec.StartTime && prog.EndTime == rec.EndTime && prog.Channel == rec.Channel)
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
      }
    }

    void Update()
    {
      lstUpcomingEpsiodes.Clear();
      if (currentProgram == null) return;

      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(currentProgram.StartTime),
        currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = currentProgram.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = currentProgram.Description;
      lblProgramTitle.Label = currentProgram.Title;


      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();
      bool bRecording = false;
      bool bSeries = false;
      foreach (Schedule record in recordings)
      {
        if (record.Canceled != Schedule.MinSchedule) continue;
        if (record.IsRecordingProgram(currentProgram, true))
        {
          if (!record.IsSerieIsCanceled(currentProgram.StartTime))
          {
            if ((ScheduleRecordingType)record.ScheduleType != ScheduleRecordingType.Once)
              bSeries = true;
            bRecording = true;
            break;
          }
        }
      }

      if (bRecording)
      {
        btnRecord.Label = GUILocalizeStrings.Get(1039);//dont record
        btnAdvancedRecord.Disabled = true;
        btnKeep.Disabled = false;
        btnQuality.Disabled = false;
        btnEpisodes.Disabled = !bSeries;
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

      lstUpcomingEpsiodes.Clear();
      Schedule recTmp = Schedule.New();
      recTmp.Channel = currentProgram.Channel;
      recTmp.ProgramName = currentProgram.Title;
      recTmp.StartTime = currentProgram.StartTime;
      recTmp.EndTime = currentProgram.EndTime;
      recTmp.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
      List<Schedule> recs = TVHome.Util.GetRecordingTimes(recTmp);
      foreach (Schedule recSeries in recs)
      {
        GUIListItem item = new GUIListItem();
        item.Label = recSeries.ProgramName;
        item.TVTag = recSeries;
        item.MusicTag = null;
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, recSeries.Channel.Name);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        Schedule recOrg;
        if (IsRecordingSchedule(recSeries, out recOrg, true))
        {
          item.PinImage = Thumbs.TvRecordingIcon;
          item.MusicTag = recOrg;
        }

        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        item.Label2 = String.Format("{0} {1} - {2}",
                                  Utils.GetShortDayString(recSeries.StartTime),
                                  recSeries.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  recSeries.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)); ;
        lstUpcomingEpsiodes.Add(item);
      }
    }

    bool IsRecordingSchedule(Schedule rec, out Schedule recOrg, bool filterOutCanceled)
    {
      recOrg = null;
      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();

      foreach (Schedule record in recordings)
      {

        if (record.Canceled != Schedule.MinSchedule) continue;
        List<Schedule> recs = TVHome.Util.GetRecordingTimes(record);
        foreach (Schedule recSeries in recs)
        {
          if (!record.IsSerieIsCanceled(recSeries.StartTime) || (filterOutCanceled == false))
          {
            if (rec.Channel == recSeries.Channel &&
              rec.ProgramName == recSeries.ProgramName &&
              rec.StartTime == recSeries.StartTime)
            {
              recOrg = record;
              return true;
            }
          }
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
          Schedule recSeries = item.TVTag as Schedule;
          Schedule recOrg = item.MusicTag as Schedule;
          OnRecordRecording(recSeries, recOrg);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }
    void OnPreRecordInterval()
    {
      bool bRecording = false;
      Schedule rec = null;
      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();

      foreach (Schedule record in recordings)
      {

        if (record.Canceled != Schedule.MinSchedule) continue;
        if (record.IsRecordingProgram(currentProgram, true))
        {
          if (!record.IsSerieIsCanceled(currentProgram.StartTime))
          {
            bRecording = true;
            rec = record;
            break;
          }
        }
      }
      if (!bRecording) return;
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
        DatabaseManager.SaveChanges();
        RemoteControl.Instance.OnNewSchedule();
      }
      Update();
    }

    void OnPostRecordInterval()
    {
      bool bRecording = false;
      Schedule rec = null;
      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();

      foreach (Schedule record in recordings)
      {

        if (record.Canceled != Schedule.MinSchedule) continue;
        if (record.IsRecordingProgram(currentProgram, true))
        {
          if (!record.IsSerieIsCanceled(currentProgram.StartTime))
          {
            bRecording = true;
            rec = record;
            break;
          }
        }
      }
      if (!bRecording) return;
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
        DatabaseManager.SaveChanges();
      }
      Update();
    }

    void OnSetQuality()
    {
      bool bRecording = false;
      Schedule rec = null;
      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();

      foreach (Schedule record in recordings)
      {

        if (record.Canceled != Schedule.MinSchedule) continue;
        if (record.IsRecordingProgram(currentProgram, true))
        {
          if (!record.IsSerieIsCanceled(currentProgram.StartTime))
          {
            bRecording = true;
            rec = record;
            break;
          }
        }
      }
      if (!bRecording) return;
      ///@
      ///GUITVPriorities.OnSetQuality(rec);
      Update();
    }
    void OnSetEpisodes()
    {
      bool bRecording = false;
      Schedule rec = null;
      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();

      foreach (Schedule record in recordings)
      {

        if (record.Canceled != Schedule.MinSchedule) continue;
        if (record.IsRecordingProgram(currentProgram, true))
        {
          if (!record.IsSerieIsCanceled(currentProgram.StartTime))
          {
            bRecording = true;
            rec = record;
            break;
          }
        }
      }
      if (!bRecording) return;
      ///@
      ///GUITVPriorities.OnSetEpisodesToKeep(rec);
      Update();
    }

    void OnRecordRecording(Schedule recSeries, Schedule rec)
    {
      if (rec == null)
      {
        //not recording yet.
        Schedule recOrg;
        if (IsRecordingSchedule(recSeries, out recOrg, false))
        {
          recOrg.UnCancelSerie(recSeries.StartTime);
          DatabaseManager.SaveChanges();
          RemoteControl.Instance.OnNewSchedule();
        }
        else
        {
          recSeries.ScheduleType = (int)ScheduleRecordingType.Once;
          DatabaseManager.SaveChanges();
          RemoteControl.Instance.OnNewSchedule();
        }
        Update();
        return;
      }
      else
      {
        if (rec.ScheduleType != (int)ScheduleRecordingType.Once)
        {
          GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg == null) return;
          dlg.Reset();
          dlg.SetHeading(rec.ProgramName);
          dlg.AddLocalizedString(981);//Delete this recording
          dlg.AddLocalizedString(982);//Delete series recording
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel == -1) return;
          switch (dlg.SelectedId)
          {
            case 981: //Delete this recording only
              {
                if (CheckIfRecording(rec))
                {
                  //delete specific series
                  CanceledSchedule canceledSerie = CanceledSchedule.Create();
                  canceledSerie.Schedule = rec;
                  canceledSerie.CancelDateTime = recSeries.StartTime;
                  DatabaseManager.SaveChanges();
                  RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
                  RemoteControl.Instance.OnNewSchedule();
                }
              }
              break;
            case 982: //Delete entire recording
              {
                if (CheckIfRecording(rec))
                {
                  RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
                  rec.DeleteAll();
                  DatabaseManager.SaveChanges();
                  RemoteControl.Instance.OnNewSchedule();
                }
              }
              break;
          }
        }
        else
        {
          if (CheckIfRecording(rec))
          {
            RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
            rec.DeleteAll();
            DatabaseManager.SaveChanges();
            RemoteControl.Instance.OnNewSchedule();
          }
        }
      }
      Update();
    }

    void OnRecordProgram(Program program)
    {
      Log.Write("OnRecordProgram");
      bool bRecording = false;
      Schedule rec = null;
      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();
      Log.Write("{0} schedules", recordings.Count);
      foreach (Schedule record in recordings)
      {
        Log.Write("record:{0}",record.RowState);
        if (record.Canceled != Schedule.MinSchedule) continue;
        if (record.IsRecordingProgram(program, true))
        {
          if (!record.IsSerieIsCanceled(program.StartTime))
          {
            bRecording = true;
            rec = record;
            break;
          }
        }
      }

      if (!bRecording)
      {
        Log.Write("not recording");
        foreach (Schedule record in recordings)
        {
          Log.Write("2record:{0}", record.RowState);
          if (record.IsRecordingProgram(program, false))
          {
            if (record.Canceled != Schedule.MinSchedule)
            {
              record.ScheduleType = (int)ScheduleRecordingType.Once;
              record.Canceled = Schedule.MinSchedule;
              DatabaseManager.SaveChanges();
              RemoteControl.Instance.OnNewSchedule();
            }
            else if (record.IsSerieIsCanceled(program.StartTime))
            {
              record.UnCancelSerie(program.StartTime);
              DatabaseManager.SaveChanges();
              RemoteControl.Instance.OnNewSchedule();
            }
            Update();
            return;
          }
        }
        Log.Write("new record");
        rec = Schedule.Create();
        rec.ProgramName = program.Title;
        rec.Channel = program.Channel;
        rec.StartTime = program.StartTime;
        rec.EndTime = program.EndTime;
        rec.ScheduleType = (int)ScheduleRecordingType.Once;
        DatabaseManager.SaveChanges();
        RemoteControl.Instance.OnNewSchedule();
      }
      else
      {
        Log.Write("is recording");
        if (rec.IsRecordingProgram(program, true))
        {
          Log.Write("is recording prog");
          if (rec.ScheduleType != (int)ScheduleRecordingType.Once)
          {
            Log.Write("schedule type!=once");
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null) return;
            dlg.Reset();
            dlg.SetHeading(rec.ProgramName);
            dlg.AddLocalizedString(981);//Delete this recording
            dlg.AddLocalizedString(982);//Delete series recording
            dlg.DoModal(GetID);
            if (dlg.SelectedLabel == -1) return;
            switch (dlg.SelectedId)
            {
              case 981: //Delete this recording only
                {
                  if (CheckIfRecording(rec))
                  {
                    //delete specific series
                    CanceledSchedule canceledSchedule = CanceledSchedule.Create();
                    canceledSchedule.Schedule = rec;
                    canceledSchedule.CancelDateTime = program.StartTime;
                    DatabaseManager.SaveChanges();
                    RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
                    RemoteControl.Instance.OnNewSchedule();
                  }
                }
                break;
              case 982: //Delete entire recording
                {
                  if (CheckIfRecording(rec))
                  {
                    //cancel recording
                    RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
                    rec.DeleteAll();
                    DatabaseManager.SaveChanges();
                    RemoteControl.Instance.OnNewSchedule();
                  }
                }
                break;
            }
          }
          else
          {
            Log.Write("schedule type==once");
            if (CheckIfRecording(rec))
            {
              Log.Write("CheckIfRecording done");
              RemoteControl.Instance.StopRecordingSchedule(rec.IdSchedule);
              Log.Write("rec deleted");
              rec.DeleteAll();
              DatabaseManager.SaveChanges();
              RemoteControl.Instance.OnNewSchedule();
            }
          }
        }
      }
      Log.Write("doupdate");
      Update();
      Log.Write("done");
    }

    void OnAdvancedRecord()
    {
      if (currentProgram == null) return;

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

        Schedule rec = Schedule.Create();
        rec.ProgramName = currentProgram.Title;
        rec.Channel = currentProgram.Channel;
        rec.StartTime = currentProgram.StartTime;
        rec.EndTime = currentProgram.EndTime;
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
        DatabaseManager.SaveChanges();
        RemoteControl.Instance.OnNewSchedule();

        //check if this program is interrupted (for example by a news bulletin)
        //ifso ask the user if he wants to record the 2nd part also
        List<Program> programs = new List<Program>();
        DateTime dtStart = rec.EndTime.AddMinutes(1);
        DateTime dtEnd = dtStart.AddHours(3);
        long iStart = Utils.datetolong(dtStart);
        long iEnd = Utils.datetolong(dtEnd);
        TvBusinessLayer layer = new TvBusinessLayer();
        programs = layer.GetPrograms(rec.Channel, dtStart, dtEnd);
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
                rec = Schedule.Create();
                rec.ProgramName = currentProgram.Title;
                rec.Channel = currentProgram.Channel;
                rec.StartTime = nextNext.StartTime;
                rec.EndTime = nextNext.EndTime;
                DatabaseManager.SaveChanges();
                RemoteControl.Instance.OnNewSchedule();
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
      if (!RemoteControl.Instance.IsRecordingSchedule(rec.IdSchedule, out card)) return true;
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
      DatabaseManager.SaveChanges();
      Update();
    }

    void OnKeep()
    {
      bool bRecording = false;
      Schedule rec = null;
      EntityList<Schedule> recordings = DatabaseManager.Instance.GetEntities<Schedule>();

      foreach (Schedule record in recordings)
      {
        if (record.IsRecordingProgram(currentProgram, true))
        {
          if (!record.IsSerieIsCanceled(currentProgram.StartTime))
          {
            bRecording = true;
            rec = record;
            break;
          }
        }
      }

      if (!bRecording)
      {
        return;
      }

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
      DatabaseManager.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();

    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      UpdateProgramDescription(item.TVTag as Schedule);
    }
  }
}
