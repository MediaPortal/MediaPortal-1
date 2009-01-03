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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Gentle.Common;
using Gentle.Framework;

using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;


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
    [SkinControlAttribute(16)]
    protected GUIFadeLabel lblProgramChannel = null;
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

    protected bool _notificationEnabled = false;

    static Program currentProgram = null;

    List<int> RecordingIntervalValues = new List<int>();
    private int _preRec;
    private int _postRec;

    public TVProgramInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO;//748

      LoadSettings();
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

    public override bool OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_INIT)
      {
        RecordingIntervalValues.Clear();
        //Fill the list with all available pre & post intervals
        RecordingIntervalValues.Add(0);
        RecordingIntervalValues.Add(1);
        RecordingIntervalValues.Add(3);
        RecordingIntervalValues.Add(5);
        RecordingIntervalValues.Add(10);
        RecordingIntervalValues.Add(15);
        RecordingIntervalValues.Add(30);
        RecordingIntervalValues.Add(45);
        RecordingIntervalValues.Add(60);
        RecordingIntervalValues.Add(90);

        TvBusinessLayer layer = new TvBusinessLayer();
        _preRec = 0;
        _postRec = 0;

        int.TryParse(layer.GetSetting("preRecordInterval", "5").Value, out _preRec);
        int.TryParse(layer.GetSetting("postRecordInterval", "5").Value, out _postRec);

        if (!RecordingIntervalValues.Contains(_preRec))
          RecordingIntervalValues.Add(_preRec);

        if (!RecordingIntervalValues.Contains(_postRec))
          RecordingIntervalValues.Add(_postRec);

        // sort the list to get the values in correct order if _preRec and/or _postRec were added
        RecordingIntervalValues.Sort();

      }
      return base.OnMessage(message);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      Update();
    }

    public static Program CurrentProgram
    {
      get { return currentProgram; }
      set { currentProgram = value; }
    }

    public static Schedule CurrentRecording
    {
      set
      {
        CurrentProgram = null;
        IList<Program> programs = new List<Program>();
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
      if (program == null)
        return;

      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(program.StartTime),
        program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = program.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = program.Description;
      lblProgramTitle.Label = program.Title;
      lblProgramChannel.Label = Channel.Retrieve(program.IdChannel).DisplayName;
    }

    void Update()
    {
      GUIListItem lastSelectedItem = lstUpcomingEpsiodes.SelectedListItem;
      int itemToSelect = -1;
      lstUpcomingEpsiodes.Clear();
      if (currentProgram == null)
        return;

      //set program description
      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(currentProgram.StartTime),
        currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = currentProgram.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = currentProgram.Description;
      lblProgramTitle.Label = currentProgram.Title;
      lblProgramChannel.Label = Channel.Retrieve(currentProgram.IdChannel).DisplayName;

      //check if we are recording this program
      IList<Schedule> schedules = Schedule.ListAll();
      bool isRecording = false;
      bool isSeries = false;
      foreach (Schedule schedule in schedules)
      {
        Schedule recSched;
        isRecording = IsRecordingProgram(currentProgram, out recSched, true);

        if (isRecording)
        {
          if ((ScheduleRecordingType)schedule.ScheduleType != ScheduleRecordingType.Once)
          {
            isSeries = true;
          }
          break;
        }

      }
      if (isRecording)
      {
        btnRecord.Label = GUILocalizeStrings.Get(1039);//dont record
        btnAdvancedRecord.Disabled = true;
        btnKeep.Disabled = false;
        btnQuality.Disabled = true;
        IList<TuningDetail> details = Channel.Retrieve(currentProgram.IdChannel).ReferringTuningDetail();
        foreach (TuningDetail detail in details)
        {
          if (detail.ChannelType == 0)
          {
            btnQuality.Disabled = false;
            break;
          }
        }
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
      if (_notificationEnabled)
      {
        btnNotify.Disabled = false;
        btnNotify.Selected = currentProgram.Notify;
      }
      else
      {
        btnNotify.Selected = false;
        btnNotify.Disabled = true;
      }


      //find upcoming episodes
      lstUpcomingEpsiodes.Clear();
      TvBusinessLayer layer = new TvBusinessLayer();
      DateTime dtDay = DateTime.Now;
      IList<Program> episodes = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(14), currentProgram.Title, null);

      foreach (Program episode in episodes)
      {
        GUIListItem item = new GUIListItem();
        item.Label = episode.Title;
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, episode.ReferencedChannel().DisplayName);
        if (!System.IO.File.Exists(logo))
        {
          item.Label = String.Format("{0} {1}", episode.ReferencedChannel().DisplayName, episode.Title);
          logo = "defaultVideoBig.png";
        }
        Schedule recordingSchedule;
        bool isRecPrg = IsRecordingProgram(episode, out recordingSchedule, true);
        /*
        bool isRecPrgNow = false;
        if (!isRecPrg)
        {
          isRecPrgNow = TVHome.IsRecordingSchedule();
        }*/

        if (isRecPrg)
        {
          if (!recordingSchedule.IsSerieIsCanceled(episode.StartTime))
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
        else
        {
          if (episode.Notify && _notificationEnabled)
          {
            item.PinImage = Thumbs.TvNotifyIcon;
          }
        }

        item.MusicTag = episode;
        item.ThumbnailImage = logo;
        item.IconImageBig = logo;
        item.IconImage = logo;
        item.Label2 = String.Format("{0} {1} - {2}",
                                  Utils.GetShortDayString(episode.StartTime),
                                  episode.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  episode.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        ;
        if (lastSelectedItem != null)
        {
          if ((item.Label == lastSelectedItem.Label) && (item.Label2 == lastSelectedItem.Label2))
            itemToSelect = lstUpcomingEpsiodes.Count;
        }
        lstUpcomingEpsiodes.Add(item);
      }
      if (itemToSelect != -1)
        lstUpcomingEpsiodes.SelectedListItemIndex = itemToSelect;

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", MediaPortal.Util.Utils.GetObjectCountLabel(lstUpcomingEpsiodes.ListItems.Count));
    }

    bool IsRecordingProgram(Program program, out Schedule recordingSchedule, bool filterCanceledRecordings)
    {
      recordingSchedule = null;
      IList<Schedule> schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule)
          continue;
        if (schedule.IsManual && schedule.IdChannel == program.IdChannel && schedule.EndTime >= program.EndTime)
        {
          Schedule manual = schedule.Clone();
          manual.ProgramName = program.Title;
          manual.EndTime = program.EndTime;
          manual.StartTime = program.StartTime;
          if (manual.IsRecordingProgram(program, filterCanceledRecordings))
          {
            recordingSchedule = schedule;
            return true;
          }
        }
        else
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
      //Quality control is currently not implemented, so we don't want to confuse the user
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
        if ((item != null) && (item.MusicTag != null))
        {
          OnRecordProgram(item.MusicTag as Program);
        }
        else
          Log.Warn("TVProgrammInfo.OnClicked: item {0} was NULL!", lstUpcomingEpsiodes.SelectedItem.ToString());
      }

      base.OnClicked(controlId, control, actionType);
    }

    void OnPreRecordInterval()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false))
        return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1444));//pre-record

        foreach (int interval in RecordingIntervalValues)
        {
          if (interval == 1)
          {
            if (interval == _preRec)
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3003) + " (" + GUILocalizeStrings.Get(886) + ")")); // minute (default)
            }
            else
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3003))); // minute
            }
          }
          else
          {
            if (interval == _preRec)
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004) + " (" + GUILocalizeStrings.Get(886) + ")")); // minutes (default)
            }
            else
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004))); // minutes
            }
          }

        }

        if (rec.PreRecordInterval < 0)
          dlg.SelectedLabel = 0;
        else if (RecordingIntervalValues.IndexOf(rec.PreRecordInterval) == -1)
          RecordingIntervalValues.IndexOf(_preRec); // select default if the value is not part of the list
        else
          dlg.SelectedLabel = RecordingIntervalValues.IndexOf(rec.PreRecordInterval);

        dlg.DoModal(GetID);

        if (dlg.SelectedLabel < 0)
          return;

        rec.PreRecordInterval = RecordingIntervalValues[dlg.SelectedLabel];
        rec.Persist();

        Schedule assocSchedule = Schedule.RetrieveOnce(rec.IdChannel, currentProgram.Title, currentProgram.StartTime, currentProgram.EndTime);
        if (assocSchedule != null)
        {
          assocSchedule.PreRecordInterval = rec.PreRecordInterval;
          assocSchedule.Persist();
        }

        TvServer server = new TvServer();
        server.OnNewSchedule();
      }
      Update();
    }

    void OnPostRecordInterval()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false))
        return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1445));//pre-record

        foreach (int interval in RecordingIntervalValues)
        {
          if (interval == 1)
          {
            if (interval == _postRec)
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3003) + " (" + GUILocalizeStrings.Get(886) + ")")); // minute (default)
            }
            else
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3003))); // minute
            }
          }
          else
          {
            if (interval == _postRec)
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004) + " (" + GUILocalizeStrings.Get(886) + ")")); // minutes (default)
            }
            else
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004))); // minutes
            }
          }
        }

        if (rec.PostRecordInterval < 0)
          dlg.SelectedLabel = 0;
        else if (RecordingIntervalValues.IndexOf(rec.PostRecordInterval) == -1)
          RecordingIntervalValues.IndexOf(_postRec); // select default if the value is not part of the list
        else
          dlg.SelectedLabel = RecordingIntervalValues.IndexOf(rec.PostRecordInterval);

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
          return;

        rec.PostRecordInterval = RecordingIntervalValues[dlg.SelectedLabel];
        rec.Persist();

        Schedule assocSchedule = Schedule.RetrieveOnce(rec.IdChannel, currentProgram.Title, currentProgram.StartTime, currentProgram.EndTime);
        if (assocSchedule != null)
        {
          assocSchedule.PostRecordInterval = rec.PostRecordInterval;
          assocSchedule.Persist();
        }
      }
      Update();
    }

    void OnSetQuality()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false))
        return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;

        dlg.AddLocalizedString(968);
        dlg.AddLocalizedString(965);
        dlg.AddLocalizedString(966);
        dlg.AddLocalizedString(967);
        VIDEOENCODER_BITRATE_MODE _newBitRate = rec.BitRateMode;
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

        dlg.DoModal(GetID);

        if (dlg.SelectedLabel == -1)
          return;
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

        rec.BitRateMode = _newBitRate;
        rec.Persist();

        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(968);// NotSet
        dlg.AddLocalizedString(886);//Default
        dlg.AddLocalizedString(993); // Custom
        dlg.AddLocalizedString(893);//Portable
        dlg.AddLocalizedString(883);//Low
        dlg.AddLocalizedString(884);//Medium
        dlg.AddLocalizedString(885);//High
        QualityType _newQuality = rec.QualityType;
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

        dlg.DoModal(GetID);

        if (dlg.SelectedLabel == -1)
          return;
        switch (dlg.SelectedLabel)
        {
          case 0: // Default
            _newQuality = QualityType.Default;
            break;

          case 1: // Custom
            _newQuality = QualityType.Custom;
            break;

          case 2: // Protable
            _newQuality = QualityType.Portable;
            break;

          case 3: // Low
            _newQuality = QualityType.Low;
            break;

          case 4: // Medium
            _newQuality = QualityType.Medium;
            break;

          case 5: // High
            _newQuality = QualityType.High;
            break;
        }

        rec.QualityType = _newQuality;
        rec.Persist();

      }
      Update();
    }

    void OnSetEpisodes()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false))
        return;

      TvPriorities.OnSetEpisodesToKeep(rec);
      Update();
    }

    void OnRecordProgram(Program program)
    {
      Log.Debug("TVProgammInfo.OnRecordProgram - programm = {0}", program.ToString());
      Schedule recordingSchedule;
      if (IsRecordingProgram(program, out recordingSchedule, true)) // check if schedule is already existing
      {
        CancelProgram(program, recordingSchedule);
      }
      else
      {
        TvServer server = new TvServer();
        VirtualCard card;
        if (TVHome.Navigator.Channel.IdChannel == program.IdChannel && server.IsRecording(TVHome.Navigator.CurrentChannel, out card))
        {
          Schedule schedFromDB = Schedule.Retrieve(card.RecordingScheduleId);
          if (schedFromDB.IsManual)
          {
            TVHome.PromptAndDeleteRecordingSchedule(card.RecordingScheduleId, null, false, false);
          }
          else
          {
            CreateProgram(program, (int)ScheduleRecordingType.Once);
          }
        }
        else
        {
          CreateProgram(program, (int)ScheduleRecordingType.Once);
        }
      }
      Update();
    }

    private bool DeleteRecordingPrompt(Schedule schedule, Program program)
    {
      if (schedule.IsRecordingProgram(program, true)) //check if we currently recoding this schedule      
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo)
        {
          Log.Error("TVProgramInfo.DeleteRecordingPrompt: ERROR no GUIDialogYesNo found !!!!!!!!!!");
          return false;
        }
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

        if (dlgYesNo.IsConfirmed)
        {
          //server.StopRecordingSchedule(schedule.IdSchedule);
          return true;
        }
        else
        {
          Log.Debug("TVProgramInfo.DeleteRecordingPrompt: not confirmed");
        }
      }
      return false;
    }

    void CancelProgram(Program program, Schedule schedule)
    {
      Log.Debug("TVProgammInfo.CancelProgram - programm = {0}", program.ToString());
      Log.Debug("                            - schedule = {0}", schedule.ToString());
      Log.Debug(" ProgramID = {0}            ScheduleID = {1}", program.IdProgram, schedule.IdSchedule);

      bool deleteEntireSched = false;

      if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        TVHome.PromptAndDeleteRecordingSchedule(schedule.IdSchedule, program, false, false);
        return;
      }

      else if ((schedule.ScheduleType == (int)ScheduleRecordingType.Daily)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.Weekends)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.Weekly)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnEveryChannel)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel))
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          Log.Error("TVProgramInfo.CancelProgram: ERROR no GUIDialogMenu found !!!!!!!!!!");
          return;
        }

        dlg.Reset();
        dlg.SetHeading(program.Title);
        dlg.AddLocalizedString(981); //Cancel this show
        dlg.AddLocalizedString(982); //Delete this entire schedule
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
          return;

        switch (dlg.SelectedId)
        {
          case 981: //delete specific series

            deleteEntireSched = false;
            break;
          case 982: //Delete entire recording
            deleteEntireSched = true;
            break;
        }

        TVHome.PromptAndDeleteRecordingSchedule(schedule.IdSchedule, program, deleteEntireSched, false);
      }
    }

    void CreateProgram(Program program, int scheduleType)
    {
      Log.Debug("TVProgramInfo.CreateProgram: program = {0}", program.ToString());
      Schedule schedule = null;
      Schedule saveSchedule = null;
      TvBusinessLayer layer = new TvBusinessLayer();
      if (IsRecordingProgram(program, out schedule, false)) // check if schedule is already existing
      {
        Log.Debug("TVProgramInfo.CreateProgram - series schedule found ID={0}, Type={1}", schedule.IdSchedule, schedule.ScheduleType);
        Log.Debug("                            - schedule= {0}", schedule.ToString());
        //schedule = Schedule.Retrieve(schedule.IdSchedule); // get the correct informations
        if (schedule.IsSerieIsCanceled(program.StartTime))
        {
          saveSchedule = schedule;
          schedule = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
          schedule.PreRecordInterval = saveSchedule.PreRecordInterval;
          schedule.PostRecordInterval = saveSchedule.PostRecordInterval;
          schedule.ScheduleType = (int)ScheduleRecordingType.Once; // needed for layer.GetConflictingSchedules(...)
        }
      }
      else
      {
        Log.Debug("TVProgramInfo.CreateProgram - no series schedule");
        // no series schedule => create it
        schedule = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
        schedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        schedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        schedule.ScheduleType = scheduleType;
      }

      // check if this program is conflicting with any other already scheduled recording
      IList conflicts = layer.GetConflictingSchedules(schedule);
      Log.Debug("TVProgramInfo.CreateProgram - conflicts.Count = {0}", conflicts.Count);
      TvServer server = new TvServer();
      bool skipConflictingEpisodes = false;
      if (conflicts.Count > 0)
      {
        GUIDialogTVConflict dlg = (GUIDialogTVConflict)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TVCONFLICT);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(GUILocalizeStrings.Get(879)); // "recording conflict"
          foreach (Schedule conflict in conflicts)
          {
            Log.Debug("TVProgramInfo.CreateProgram: Conflicts = " + conflict.ToString());

            GUIListItem item = new GUIListItem(conflict.ProgramName);
            item.Label2 = GetRecordingDateTime(conflict);
            item.Label3 = conflict.IdChannel.ToString();
            item.TVTag = conflict;
            dlg.AddConflictRecording(item);
          }
          dlg.ConflictingEpisodes = (scheduleType != (int)ScheduleRecordingType.Once);
          dlg.DoModal(GetID);
          switch (dlg.SelectedLabel)
          {
            case 0: // Skip new Recording
              {
                Log.Debug("TVProgramInfo.CreateProgram: Skip new recording");
                return;
              }
            case 1: // Don't record the already scheduled one(s)
              {
                Log.Debug("TVProgramInfo.CreateProgram: Skip old recording(s)");
                foreach (Schedule conflict in conflicts)
                {
                  Program prog =
                    new Program(conflict.IdChannel, conflict.StartTime, conflict.EndTime, conflict.ProgramName, "-", "-", false,
                                DateTime.MinValue, string.Empty, string.Empty, -1, string.Empty, -1);
                  CancelProgram(prog, Schedule.Retrieve(conflict.IdSchedule));
                }
                break;
              }
            case 2: // keep conflict
              {
                Log.Debug("TVProgramInfo.CreateProgram: Keep Conflict");
                break;
              }
            case 3: // Skip for conflicting episodes
              {
                Log.Debug("TVProgramInfo.CreateProgram: Skip conflicting episode(s)");
                skipConflictingEpisodes = true;
                break;
              }
            default: // Skipping new Recording
              {
                Log.Debug("TVProgramInfo.CreateProgram: Default => Skip new recording");
                return;
              }
          }
        }
      }

      if (saveSchedule != null)
      {
        Log.Debug("TVProgramInfo.CreateProgram - UnCancleSerie at {0}", program.StartTime);
        //saveSchedule.UnCancelSerie(program.StartTime);
        saveSchedule.UnCancelSerie();

        saveSchedule.Persist();
      }
      else
      {
        Log.Debug("TVProgramInfo.CreateProgram - create schedule = {0}", schedule.ToString());
        schedule.Persist();
      }
      if (skipConflictingEpisodes)
      {
        List<Schedule> episodes = layer.GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.EndTime)
            continue;
          if (episode.IsSerieIsCanceled(episode.StartTime))
            continue;
          foreach (Schedule conflict in conflicts)
          {
            if (episode.IsOverlapping(conflict))
            {
              Log.Debug("TVProgramInfo.CreateProgram - skip episode = {0}", episode.ToString());
              CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, episode.StartTime);
              canceledSchedule.Persist();
            }
          }
        }
      }
      server.OnNewSchedule();
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
        if (dlg.SelectedLabel == -1)
          return;

        int scheduleType = (int)ScheduleRecordingType.Once;
        switch (dlg.SelectedId)
        {
          case 611://once
            scheduleType = (int)ScheduleRecordingType.Once;
            break;
          case 612://everytime, this channel
            scheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            break;
          case 613://everytime, all channels
            scheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            break;
          case 614://weekly
            scheduleType = (int)ScheduleRecordingType.Weekly;
            break;
          case 615://daily
            scheduleType = (int)ScheduleRecordingType.Daily;
            break;
          case 672://Mo-Fi
            scheduleType = (int)ScheduleRecordingType.WorkingDays;
            break;
          case 1051://Record Sat-Sun
            scheduleType = (int)ScheduleRecordingType.Weekends;
            break;
        }
        CreateProgram(currentProgram, scheduleType);

        if (scheduleType == (int)ScheduleRecordingType.Once)
        {
          //check if this program is interrupted (for example by a news bulletin)
          //ifso ask the user if he wants to record the 2nd part also
          IList<Program> programs = new List<Program>();
          DateTime dtStart = currentProgram.EndTime.AddMinutes(1);
          DateTime dtEnd = dtStart.AddHours(3);
          TvBusinessLayer layer = new TvBusinessLayer();
          programs = layer.GetPrograms(currentProgram.ReferencedChannel(), dtStart, dtEnd);
          if (programs.Count >= 2)
          {
            Program next = programs[0] as Program;
            Program nextNext = programs[1] as Program;
            if (nextNext.Title == currentProgram.Title)
            {
              TimeSpan ts = next.EndTime - nextNext.StartTime;
              if (ts.TotalMinutes <= 40)
              {
                //
                GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                dlgYesNo.SetHeading(1012); //This program will be interrupted by
                dlgYesNo.SetLine(1, next.Title);
                dlgYesNo.SetLine(2, 1013); //Would you like to record the second part also?
                dlgYesNo.DoModal(GetID);
                if (dlgYesNo.IsConfirmed)
                {
                  CreateProgram(nextNext, scheduleType);
                  Update();
                }
              }
            }
          }
        }
      }
      Update();
    }

    void OnNotify()
    {
      currentProgram.Notify = !currentProgram.Notify;
      // get the right db instance of current prog before we store it
      // currentProgram is not a ref to the real entity
      Program modifiedProg = Program.RetrieveByTitleAndTimes(currentProgram.Title, currentProgram.StartTime, currentProgram.EndTime);
      modifiedProg.Notify = currentProgram.Notify;
      modifiedProg.Persist();
      Update();
      TvNotifyManager.OnNotifiesChanged();
    }

    void OnKeep()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false))
        return;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
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
      if (dlg.SelectedLabel == -1)
        return;
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
          if (days >= 100)
            days = 30;
          dlg.SelectedLabel = days - 1;
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel < 0)
            return;
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
      if (item != null)
      {
        Program episode = null;
        if (item.MusicTag != null)
          episode = item.MusicTag as Program;

        if (episode != null)
        {
          Log.Info("TVProgrammInfo.item_OnItemSelected: {0}", episode.Title);
          UpdateProgramDescription(null, episode);
        }
        else
          Log.Warn("TVProgrammInfo.item_OnItemSelected: episode was NULL!");
      }
      else
        Log.Warn("TVProgrammInfo.item_OnItemSelected: params where NULL!");
    }

    private string GetRecordingDateTime(Schedule rec)
    {
      return String.Format("{0} {1} - {2}",
                MediaPortal.Util.Utils.GetShortDayString(rec.StartTime),
                rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
    }

    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _notificationEnabled = xmlreader.GetValueAsBool("mytv", "enableTvNotifier", false);
      }
    }

  }
}
