#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// Summary description for GUITVProgramInfo.
  /// </summary>
  public class GUITVProgramInfo : GUIWindow
  {
    [SkinControl(17)] protected GUILabelControl lblProgramGenre = null;
    [SkinControl(15)] protected GUITextScrollUpControl lblProgramDescription = null;
    [SkinControl(14)] protected GUILabelControl lblProgramTime = null;
    [SkinControl(13)] protected GUIFadeLabel lblProgramTitle = null;
    [SkinControl(16)] protected GUIFadeLabel lblProgramChannel = null;
    [SkinControl(2)] protected GUIButtonControl btnRecord = null;
    [SkinControl(3)] protected GUIButtonControl btnAdvancedRecord = null;
    [SkinControl(4)] protected GUIButtonControl btnKeep = null;
    [SkinControl(5)] protected GUIToggleButtonControl btnNotify = null;
    [SkinControl(10)] protected GUIListControl lstUpcomingEpsiodes = null;
    [SkinControl(6)] protected GUIButtonControl btnQuality = null;
    [SkinControl(7)] protected GUIButtonControl btnEpisodes = null;
    [SkinControl(8)] protected GUIButtonControl btnPreRecord = null;
    [SkinControl(9)] protected GUIButtonControl btnPostRecord = null;

    private bool _notificationEnabled = false;
    private static TVProgram currentProgram = null;

    public GUITVProgramInfo()
    {
      GetID = (int) Window.WINDOW_TV_PROGRAM_INFO; //748
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

    public static TVProgram CurrentProgram
    {
      get { return currentProgram; }
      set { currentProgram = value; }
    }

    public static TVRecording CurrentRecording
    {
      set
      {
        CurrentProgram = null;
        List<TVProgram> programs = new List<TVProgram>();
        TVDatabase.GetPrograms(Util.Utils.datetolong(DateTime.Now), Util.Utils.datetolong(DateTime.Now.AddDays(10)),
                               ref programs);
        foreach (TVProgram prog in programs)
        {
          if (value.IsRecordingProgram(prog, false))
          {
            CurrentProgram = prog;
            return;
          }
        }
      }
    }

    private void UpdateProgramDescription(TVRecording rec)
    {
      if (rec == null)
      {
        return;
      }
      List<TVProgram> progs = new List<TVProgram>();

      TVDatabase.GetProgramsPerChannel(rec.Channel, rec.Start, rec.End, ref progs);
      if (progs.Count > 0)
      {
        foreach (TVProgram prog in progs)
        {
          if (prog.Start == rec.Start && prog.End == rec.End && prog.Channel == rec.Channel)
          {
            currentProgram = prog;
            break;
          }
        }
      }
      if (currentProgram != null)
      {
        string strTime = String.Format("{0} {1} - {2}",
                                       Util.Utils.GetShortDayString(currentProgram.StartTime),
                                       currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        lblProgramGenre.Label = currentProgram.Genre;
        lblProgramTime.Label = strTime;
        lblProgramDescription.Label = currentProgram.Description;
        lblProgramTitle.Label = currentProgram.Title;
        lblProgramChannel.Label = currentProgram.Channel;
      }
    }

    private void Update()
    {
      lstUpcomingEpsiodes.Clear();
      if (currentProgram == null)
      {
        return;
      }

      string strTime = String.Format("{0} {1} - {2}",
                                     Util.Utils.GetShortDayString(currentProgram.StartTime),
                                     currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                     currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = currentProgram.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = currentProgram.Description;
      lblProgramTitle.Label = currentProgram.Title;
      lblProgramChannel.Label = currentProgram.Channel;

      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings);
      bool bRecording = false;
      bool bSeries = false;
      bool bNoPaddingEnd = false;
      bool bNoPaddingFront = false;

      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
        if (record.IsRecordingProgram(currentProgram, true))
        {
          if (!record.IsSerieIsCanceled(currentProgram.StartTime))
          {
            if (record.RecType != TVRecording.RecordingType.Once)
            {
              bSeries = true;
            }
            if (record.PaddingFront == -2)
            {
              bNoPaddingFront = true;
            }
            if (record.PaddingEnd == -2)
            {
              bNoPaddingEnd = true;
            }
            bRecording = true;
            break;
          }
        }
      }

      if (bRecording)
      {
        btnRecord.Label = GUILocalizeStrings.Get(1039); //dont record
        btnAdvancedRecord.Disabled = true;
        btnKeep.Disabled = false;
        btnQuality.Disabled = false;
        btnEpisodes.Disabled = !bSeries;
        btnPreRecord.Disabled = false;
        btnPostRecord.Disabled = false;
      }
      else
      {
        btnRecord.Label = GUILocalizeStrings.Get(264); //record
        btnAdvancedRecord.Disabled = false;
        btnKeep.Disabled = true;
        btnQuality.Disabled = true;
        btnEpisodes.Disabled = true;
        btnPreRecord.Disabled = true;
        btnPostRecord.Disabled = true;
      }

      if (bNoPaddingFront)
      {
        btnPreRecord.Disabled = true;
      }
      if (bNoPaddingEnd)
      {
        btnPostRecord.Disabled = true;
      }

      List<TVNotify> notifies = new List<TVNotify>();
      TVDatabase.GetNotifies(notifies, false);
      bool showNotify = false;
      foreach (TVNotify notify in notifies)
      {
        if (notify.Program.ID == currentProgram.ID)
        {
          showNotify = true;
          break;
        }
      }
      if (_notificationEnabled)
      {
        btnNotify.Disabled = false;
        if (showNotify)
        {
          btnNotify.Selected = true;
        }
        else
        {
          btnNotify.Selected = false;
        }
      }
      else
      {
        btnNotify.Selected = false;
        btnNotify.Disabled = true;
      }

      lstUpcomingEpsiodes.Clear();
      TVRecording recTmp = new TVRecording();
      recTmp.Channel = currentProgram.Channel;
      recTmp.Title = currentProgram.Title;
      recTmp.Start = currentProgram.Start;
      recTmp.End = currentProgram.End;
      recTmp.RecType = TVRecording.RecordingType.EveryTimeOnEveryChannel;
      List<TVRecording> recs = ConflictManager.Util.GetRecordingTimes(recTmp);
      foreach (TVRecording recSeries in recs)
      {
        GUIListItem item = new GUIListItem();
        item.Label = recSeries.Title;
        item.TVTag = recSeries;
        item.MusicTag = null;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, recSeries.Channel);
        if (!File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        TVRecording recOrg;
        if (IsRecordingSchedule(recSeries, out recOrg, true))
        {
          item.PinImage = Thumbs.TvRecordingIcon;
          item.MusicTag = recOrg;
        }

        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        item.Label2 = String.Format("{0} {1} - {2}",
                                    Util.Utils.GetShortDayString(recSeries.StartTime),
                                    recSeries.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                    recSeries.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        ;
        lstUpcomingEpsiodes.Add(item);
      }
    }

    private bool IsRecordingSchedule(TVRecording rec, out TVRecording recOrg, bool filterOutCanceled)
    {
      recOrg = null;
      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings, rec);
      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
        List<TVRecording> recs = ConflictManager.Util.GetRecordingTimes(record);
        foreach (TVRecording recSeries in recs)
        {
          if (!record.IsSerieIsCanceled(recSeries.StartTime) || (filterOutCanceled == false))
          {
            if (rec.Channel == recSeries.Channel &&
                rec.Title == recSeries.Title &&
                rec.Start == recSeries.Start)
            {
              recOrg = record;
              return true;
            }
          }
        }
      }
      return false;
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnPreRecord)
      {
        OnPreRecordInterval();
      }
      if (control == btnPostRecord)
      {
        OnPostRecordInterval();
      }
      if (control == btnEpisodes)
      {
        OnSetEpisodes();
      }
      if (control == btnQuality)
      {
        OnSetQuality();
      }
      if (control == btnKeep)
      {
        OnKeep();
      }
      if (control == btnRecord)
      {
        OnRecordProgram(currentProgram);
      }
      if (control == btnAdvancedRecord)
      {
        OnAdvancedRecord();
      }
      if (control == btnNotify)
      {
        OnNotify();
      }
      if (control == lstUpcomingEpsiodes)
      {
        GUIListItem item = lstUpcomingEpsiodes.SelectedListItem;
        if (item != null)
        {
          TVRecording recSeries = item.TVTag as TVRecording;
          TVRecording recOrg = item.MusicTag as TVRecording;
          OnRecordRecording(recSeries, recOrg);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnPreRecordInterval()
    {
      bool bRecording = false;
      TVRecording rec = null;
      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings);

      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
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
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1444)); //pre-record
        dlg.Add(GUILocalizeStrings.Get(886)); //default
        for (int minute = 0; minute < 20; minute++)
        {
          dlg.Add(String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004)));
        }
        if (rec.PaddingFront < 0)
        {
          dlg.SelectedLabel = 0;
        }
        else
        {
          dlg.SelectedLabel = rec.PaddingFront + 1;
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }
        rec.PaddingFront = dlg.SelectedLabel - 1;
        TVDatabase.UpdateRecording(rec, TVDatabase.RecordingChange.Modified);
      }
      Update();
    }

    private void OnPostRecordInterval()
    {
      bool bRecording = false;
      TVRecording rec = null;
      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings);

      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
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
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1445)); //pre-record
        dlg.Add(GUILocalizeStrings.Get(886)); //default
        for (int minute = 0; minute < 20; minute++)
        {
          dlg.Add(String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004)));
        }
        if (rec.PaddingEnd < 0)
        {
          dlg.SelectedLabel = 0;
        }
        else
        {
          dlg.SelectedLabel = rec.PaddingEnd + 1;
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }
        rec.PaddingEnd = dlg.SelectedLabel - 1;
        TVDatabase.UpdateRecording(rec, TVDatabase.RecordingChange.Modified);
      }
      Update();
    }

    private void OnSetQuality()
    {
      bool bRecording = false;
      TVRecording rec = null;
      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings);

      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
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
      GUITVPriorities.OnSetQuality(rec);
      Update();
    }

    private void OnSetEpisodes()
    {
      bool bRecording = false;
      TVRecording rec = null;
      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings);

      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
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
      GUITVPriorities.OnSetEpisodesToKeep(rec);
      Update();
    }

    private void OnRecordRecording(TVRecording recSeries, TVRecording rec)
    {
      if (rec == null)
      {
        //not recording yet.
        TVRecording recOrg;
        if (IsRecordingSchedule(recSeries, out recOrg, false))
        {
          recOrg.UnCancelSerie(recSeries.StartTime);
          TVDatabase.UpdateRecording(recOrg, TVDatabase.RecordingChange.Modified);
        }
        else
        {
          recSeries.RecType = TVRecording.RecordingType.Once;
          Recorder.AddRecording(ref recSeries);
        }
        Update();
        return;
      }
      else
      {
        if (rec.RecType != TVRecording.RecordingType.Once)
        {
          GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
          if (dlg == null)
          {
            return;
          }
          dlg.Reset();
          dlg.SetHeading(rec.Title);
          dlg.AddLocalizedString(981); //Delete this recording
          dlg.AddLocalizedString(982); //Delete series recording
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel == -1)
          {
            return;
          }
          switch (dlg.SelectedId)
          {
            case 981: //Delete this recording only
              {
                if (CheckIfRecording(rec))
                {
                  //delete specific series
                  rec.CanceledSeries.Add(recSeries.Start);
                  TVDatabase.AddCanceledSerie(rec, recSeries.Start);
                  Recorder.StopRecording(rec);
                }
              }
              break;
            case 982: //Delete entire recording
              {
                if (CheckIfRecording(rec))
                {
                  //cancel recording
                  TVDatabase.RemoveRecording(rec);
                  Recorder.StopRecording(rec);
                }
              }
              break;
          }
        }
        else
        {
          if (CheckIfRecording(rec))
          {
            //cancel recording2
            TVDatabase.RemoveRecording(rec);
            Recorder.StopRecording(rec);
          }
        }
      }
      Update();
    }


    private void OnRecordProgram(TVProgram program)
    {
      bool bRecording = false;
      TVRecording rec = null;
      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings);

      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
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
      if (!bRecording) // Not recording this program, add it.
      {
        // check if this program is conflicting with any other already scheduled recording
        rec = new TVRecording();
        rec.Title = program.Title;
        rec.Channel = program.Channel;
        rec.Start = program.Start;
        rec.End = program.End;
        rec.RecType = TVRecording.RecordingType.Once;

        if (SkipForConflictingRecording(rec))
        {
          return;
        }

        foreach (TVRecording record in recordings)
        {
          if (record.IsRecordingProgram(program, false))
          {
            if (record.Canceled > 0)
            {
              record.RecType = TVRecording.RecordingType.Once;
              record.Canceled = 0;
              TVDatabase.UpdateRecording(record, TVDatabase.RecordingChange.Modified);
            }
            else if (record.IsSerieIsCanceled(program.StartTime))
            {
              record.UnCancelSerie(program.StartTime);
              TVDatabase.UpdateRecording(record, TVDatabase.RecordingChange.Modified);
            }
            Update();
            return;
          }
        }
        Recorder.AddRecording(ref rec);
      }
      else
      {
        if (rec.IsRecordingProgram(program, true))
        {
          if (rec.RecType != TVRecording.RecordingType.Once)
          {
            GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
            {
              return;
            }
            dlg.Reset();
            dlg.SetHeading(rec.Title);
            dlg.AddLocalizedString(981); //Delete this recording
            dlg.AddLocalizedString(982); //Delete series recording
            dlg.DoModal(GetID);
            if (dlg.SelectedLabel == -1)
            {
              return;
            }
            switch (dlg.SelectedId)
            {
              case 981: //Delete this recording only
                {
                  if (CheckIfRecording(rec))
                  {
                    //delete specific series
                    rec.CanceledSeries.Add(program.Start);
                    TVDatabase.AddCanceledSerie(rec, program.Start);
                    Recorder.StopRecording(rec);
                  }
                }
                break;
              case 982: //Delete entire recording
                {
                  if (CheckIfRecording(rec))
                  {
                    //cancel recording
                    TVDatabase.RemoveRecording(rec);
                    Recorder.StopRecording(rec);
                  }
                }
                break;
            }
          }
          else
          {
            if (CheckIfRecording(rec))
            {
              //cancel recording2
              TVDatabase.RemoveRecording(rec);
              Recorder.StopRecording(rec);
            }
          }
        }
      }
      Update();
    }

    private void OnAdvancedRecord()
    {
      if (currentProgram == null)
      {
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616)); //616=Select Recording type
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
        dlg.AddLocalizedString(672); // 672=Record Mon-Fri
        dlg.AddLocalizedString(1051); // 1051=Record Sat-Sun

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }

        TVRecording rec = new TVRecording();
        rec.Title = currentProgram.Title;
        rec.Channel = currentProgram.Channel;
        rec.Start = currentProgram.Start;
        rec.End = currentProgram.End;
        switch (dlg.SelectedId)
        {
          case 611: //once
            rec.RecType = TVRecording.RecordingType.Once;
            break;
          case 612: //everytime, this channel
            rec.RecType = TVRecording.RecordingType.EveryTimeOnThisChannel;
            break;
          case 613: //everytime, all channels
            rec.RecType = TVRecording.RecordingType.EveryTimeOnEveryChannel;
            break;
          case 614: //weekly
            rec.RecType = TVRecording.RecordingType.Weekly;
            break;
          case 615: //daily
            rec.RecType = TVRecording.RecordingType.Daily;
            break;
          case 672: //Mo-Fi
            rec.RecType = TVRecording.RecordingType.WeekDays;
            break;
          case 1051: //Record Sat-Sun
            rec.RecType = TVRecording.RecordingType.WeekEnds;
            break;
        }
        // check if we get any conflicts
        if (SkipForConflictingRecording(rec))
        {
          return;
        }
        Recorder.AddRecording(ref rec);

        //check if this program is interrupted (for example by a news bulletin)
        //ifso ask the user if he wants to record the 2nd part also
        List<TVProgram> programs = new List<TVProgram>();
        DateTime dtStart = rec.EndTime.AddMinutes(1);
        DateTime dtEnd = dtStart.AddHours(3);
        long iStart = Util.Utils.datetolong(dtStart);
        long iEnd = Util.Utils.datetolong(dtEnd);
        TVDatabase.GetProgramsPerChannel(rec.Channel, iStart, iEnd, ref programs);
        if (programs.Count >= 2)
        {
          TVProgram next = programs[0] as TVProgram;
          TVProgram nextNext = programs[1] as TVProgram;
          if (nextNext.Title == rec.Title)
          {
            TimeSpan ts = next.EndTime - next.StartTime;
            if (ts.TotalMinutes <= 40)
            {
              //
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
              dlgYesNo.SetHeading(1012); //This program will be interrupted by
              dlgYesNo.SetLine(1, next.Title);
              dlgYesNo.SetLine(2, 1013); //Would you like to record the second part also?
              dlgYesNo.DoModal(GetID);
              if (dlgYesNo.IsConfirmed)
              {
                rec.Start = nextNext.Start;
                rec.End = nextNext.End;
                rec.ID = -1;
                Recorder.AddRecording(ref rec);
              }
            }
          }
        }
      }
      Update();
    }


    private bool SkipForConflictingRecording(TVRecording rec)
    {
      List<TVRecording> conflicts = new List<TVRecording>();
      ConflictManager.GetConflictingSeries2(rec, conflicts);

      if (conflicts.Count > 0)
      {
        GUIDialogTVConflict dlg =
          (GUIDialogTVConflict) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_TVCONFLICT);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(GUILocalizeStrings.Get(879)); // "recording conflict"
          dlg.AddConflictRecordings(conflicts);
          dlg.DoModal(GetID);
          switch (dlg.SelectedLabel)
          {
            case 0:
              return true; // Skip new Recording
            case 1: // Don't record the already scheduled one(s)
              {
                foreach (TVRecording conflict in conflicts)
                {
                  TVProgram prog = new TVProgram(conflict.Channel, conflict.StartTime, conflict.EndTime, conflict.Title);
                  OnRecordProgram(prog);
                  //OnRecordRecording(null, conflict);
                }
                break;
              }
            case 2:
              return false; // No Skipping new Recording
              //case 3: ;   // ToDo: Skip only conflicting episodes
            default:
              return true; // Skipping new Recording
          }
        }
      }
      return false;
    }


    private bool CheckIfRecording(TVRecording rec)
    {
      int card;
      if (!Recorder.IsRecordingSchedule(rec, out card))
      {
        return true;
      }
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return true;
      }

      dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
      dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
      dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

      if (dlgYesNo.IsConfirmed)
      {
        return true;
      }
      return false;
    }

    private void OnNotify()
    {
      TVNotify notification = null;
      List<TVNotify> notifies = new List<TVNotify>();
      TVDatabase.GetNotifies(notifies, false);
      bool showNotify = false;
      foreach (TVNotify notify in notifies)
      {
        if (notify.Program.ID == currentProgram.ID)
        {
          showNotify = true;
          notification = notify;
          break;
        }
      }
      if (showNotify)
      {
        TVDatabase.DeleteNotify(notification);
      }
      else
      {
        TVNotify notify = new TVNotify();
        notify.Program = currentProgram;
        TVDatabase.AddNotify(notify);
      }
      Update();
    }

    private void OnKeep()
    {
      bool bRecording = false;
      TVRecording rec = null;
      List<TVRecording> recordings = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recordings);

      foreach (TVRecording record in recordings)
      {
        if (record.Canceled > 0)
        {
          continue;
        }
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

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(1042);
      dlg.AddLocalizedString(1043); //Until watched
      dlg.AddLocalizedString(1044); //Until space needed
      dlg.AddLocalizedString(1045); //Until date
      dlg.AddLocalizedString(1046); //Always
      switch (rec.KeepRecordingMethod)
      {
        case TVRecorded.KeepMethod.UntilWatched:
          dlg.SelectedLabel = 0;
          break;
        case TVRecorded.KeepMethod.UntilSpaceNeeded:
          dlg.SelectedLabel = 1;
          break;
        case TVRecorded.KeepMethod.TillDate:
          dlg.SelectedLabel = 2;
          break;
        case TVRecorded.KeepMethod.Always:
          dlg.SelectedLabel = 3;
          break;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 1043:
          rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilWatched;
          break;
        case 1044:
          rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;

          break;
        case 1045:
          rec.KeepRecordingMethod = TVRecorded.KeepMethod.TillDate;
          dlg.Reset();
          dlg.ShowQuickNumbers = false;
          dlg.SetHeading(1045);
          for (int iDay = 1; iDay <= 100; iDay++)
          {
            DateTime dt = currentProgram.StartTime.AddDays(iDay);
            dlg.Add(dt.ToLongDateString());
          }
          TimeSpan ts = (rec.KeepRecordingTill - currentProgram.StartTime);
          int days = (int) ts.TotalDays;
          if (days >= 100)
          {
            days = 30;
          }
          dlg.SelectedLabel = days - 1;
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel < 0)
          {
            return;
          }
          rec.KeepRecordingTill = currentProgram.StartTime.AddDays(dlg.SelectedLabel + 1);
          break;
        case 1046:
          rec.KeepRecordingMethod = TVRecorded.KeepMethod.Always;
          break;
      }
      TVDatabase.UpdateRecording(rec, TVDatabase.RecordingChange.Modified);
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      UpdateProgramDescription(item.TVTag as TVRecording);
    }

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _notificationEnabled = xmlreader.GetValueAsBool("plugins", "TV Notifier", false);
      }
    }
  }
}