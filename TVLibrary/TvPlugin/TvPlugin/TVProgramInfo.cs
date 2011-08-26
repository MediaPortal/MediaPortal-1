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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using Action = MediaPortal.GUI.Library.Action;


namespace TvPlugin
{

  #region ScheduleInfo class

  public class ScheduleInfo
  {
    public ScheduleInfo(int aIdChannel, string aTitle, string aDescription, string aGenre, DateTime aStartTime,
                        DateTime aEndTime)
    {
      idChannel = aIdChannel;
      title = aTitle;
      description = aDescription;
      genre = aGenre;
      startTime = aStartTime;
      endTime = aEndTime;
    }

    private int idChannel;

    public int IdChannel
    {
      get { return idChannel; }
    }

    private string title;

    public string Title
    {
      get { return title; }
    }

    private string description;

    public string Description
    {
      get { return description; }
    }

    private string genre;

    public string Genre
    {
      get { return genre; }
    }

    private DateTime startTime;

    public DateTime StartTime
    {
      get { return startTime; }
    }

    private DateTime endTime;

    public DateTime EndTime
    {
      get { return endTime; }
    }
  }

  #endregion

  /// <summary>
  /// Summary description for GUITVProgramInfo.
  /// </summary>
  public class TVProgramInfo : GUIInternalWindow
  {
    #region Invoke delegates

    protected delegate void UpdateCurrentItem(ScheduleInfo aInfo);

    #endregion

    #region Variables

    [SkinControl(17)] protected GUILabelControl lblProgramGenre = null;
    [SkinControl(15)] protected GUITextScrollUpControl lblProgramDescription = null;
    [SkinControl(14)] protected GUILabelControl lblProgramTime = null;
    [SkinControl(13)] protected GUIFadeLabel lblProgramTitle = null;
    [SkinControl(16)] protected GUIFadeLabel lblProgramChannel = null;
    [SkinControl(2)] protected GUIButtonControl btnRecord = null;
    [SkinControl(3)] protected GUIButtonControl btnAdvancedRecord = null;
    [SkinControl(4)] protected GUIButtonControl btnKeep = null;
    [SkinControl(11)]
    protected GUILabelControl lblUpcomingEpsiodes = null;
    [SkinControl(10)] protected GUIListControl lstUpcomingEpsiodes = null;
    [SkinControl(6)] protected GUIButtonControl btnQuality = null;
    [SkinControl(7)] protected GUIButtonControl btnEpisodes = null;
    [SkinControl(8)] protected GUIButtonControl btnPreRecord = null;
    [SkinControl(9)] protected GUIButtonControl btnPostRecord = null;

    private static Program currentProgram;
    private static Program initialProgram;
    private static Schedule currentSchedule;
    private static bool anyUpcomingEpisodesRecording = true;

    private readonly List<int> RecordingIntervalValues = new List<int>();
    private int _preRec;
    private int _postRec;
    private object updateLock = null;
    private static object fieldLock = new object();

    #endregion

    #region Ctor

    public TVProgramInfo()
    {
      updateLock = new object();
      GetID = (int)Window.WINDOW_TV_PROGRAM_INFO; //748

      LoadSettings();
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
    }

    #endregion

    #region Overrides

    public override bool IsTv
    {
      get { return true; }
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvprogram.xml");
      return bResult;
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT)
      {
        ResetCurrentScheduleAndProgram();
        initialProgram = null;
      }
      else if (message.Message == GUIMessage.MessageType.GUI_MSG_WINDOW_INIT)
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

        int.TryParse(layer.GetSetting("preRecordInterval", "7").Value, out _preRec);
        int.TryParse(layer.GetSetting("postRecordInterval", "10").Value, out _postRec);

        if (!RecordingIntervalValues.Contains(_preRec))
        {
          RecordingIntervalValues.Add(_preRec);
        }

        if (!RecordingIntervalValues.Contains(_postRec))
        {
          RecordingIntervalValues.Add(_postRec);
        }

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
      //Quality control is currently not implemented, so we don't want to confuse the user
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
        OnRecordProgram(CurrentProgram);
      }
      if (control == btnAdvancedRecord)
      {
        OnAdvancedRecord();
      }
      if (control == lstUpcomingEpsiodes)
      {
        GUIListItem item = lstUpcomingEpsiodes.SelectedListItem;
        if ((item != null) && (item.MusicTag != null))
        {
          OnRecordProgram(item.MusicTag as Program);
        }
        else
        {
          Log.Warn("TVProgrammInfo.OnClicked: item {0} was NULL!", lstUpcomingEpsiodes.SelectedItem.ToString());
        }
      }

      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    #region Fields

    public static Program CurrentProgram
    {
      get
      {
        lock (fieldLock)
        {
          return currentProgram;
        }
      }
      set
      {
        lock (fieldLock)
        {
          currentProgram = value;
          if (initialProgram == null)
          {
            initialProgram = currentProgram;
            if (currentProgram != null)
            {


              currentSchedule = Schedule.RetrieveSeries(currentProgram.ReferencedChannel().IdChannel,
                                                        currentProgram.StartTime,
                                                        currentProgram.EndTime);
            }
          }
        }
      }
    }

    public static Schedule CurrentRecording
    {
      set
      {
        currentSchedule = value;
        currentProgram = null;
        TvBusinessLayer layer = new TvBusinessLayer();
        // comment: this is not performant, i.e. query 15.000 programs and then start comparing each of them
        IList<Program> programs = layer.GetPrograms(DateTime.Now, DateTime.Now.AddDays(10));
        foreach (Program prog in programs)
        {
          if (value.IsRecordingProgram(prog, false))
          {
            CurrentProgram = prog;
            return;
          }
        }
        // in case of no forthcoming programs, we need to create a dummy program taken from schedule information
        // to keep the dialog working at all
        if (CurrentProgram == null)
        {
          CurrentProgram = new Program(value.IdChannel, value.StartTime, value.EndTime, value.ProgramName, "",
                                       "", Program.ProgramState.None, DateTime.MinValue, "", "", "", "", 0, "", 0);
        }        
      }
    }

    #endregion

    #region Static methods

    private static string GetRecordingDateTime(Schedule rec)
    {
      return String.Format("{0} {1} - {2}",
                           Utils.GetShortDayString(rec.StartTime),
                           rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                           rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
    }

    public static bool IsRecordingProgram(Program program, out Schedule recordingSchedule,
                                           bool filterCanceledRecordings)
    {
      recordingSchedule = null;
      
      IList<Schedule> schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule || (filterCanceledRecordings && schedule.IsSerieIsCanceled(schedule.GetSchedStartTimeForProg(program), program.IdChannel)))
        {
          continue;
        }
        if (schedule.IsManual && schedule.IdChannel == program.IdChannel && schedule.EndTime >= program.EndTime)
        {
          Schedule manual = schedule.Clone();
          manual.ProgramName = TVUtil.GetDisplayTitle(program);
          manual.EndTime = program.EndTime;
          manual.StartTime = program.StartTime;
          if (manual.IsRecordingProgram(program, filterCanceledRecordings))
          {
            recordingSchedule = schedule;
            return true;
          }
        }
        else if (schedule.IsRecordingProgram(program, filterCanceledRecordings))
        {
          recordingSchedule = schedule;
          return true;
        }
      }
      return false;
    }

    #endregion

    #region Private methods

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      lock (updateLock)
      {
        if (item != null && item.MusicTag != null)
        {
          //CacheManager.ClearQueryResultsByType(typeof(Program));
          Program lstProg = item.MusicTag as Program;
          if (lstProg != null)
          {
            ScheduleInfo refEpisode = new ScheduleInfo(
              lstProg.IdChannel,
              TVUtil.GetDisplayTitle(lstProg),
              lstProg.Description,
              lstProg.Genre,
              lstProg.StartTime,
              lstProg.EndTime
              );
            GUIGraphicsContext.form.Invoke(new UpdateCurrentItem(UpdateProgramDescription), new object[] {refEpisode});
          }
        }
        else
        {
          Log.Warn("TVProgrammInfo.item_OnItemSelected: params where NULL!");
        }
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void UpdateProgramDescription(ScheduleInfo episode)
    {
      if (episode == null)
      {
        episode = new ScheduleInfo(CurrentProgram.IdChannel,
                                   TVUtil.GetDisplayTitle(CurrentProgram),
                                   CurrentProgram.Description,
                                   CurrentProgram.Genre,
                                   CurrentProgram.StartTime,
                                   CurrentProgram.EndTime);
      }

      try
      {
        //Log.Debug("TVProgrammInfo.UpdateProgramDescription: {0} - {1}", episode.Title, episode.Description);

        lblProgramChannel.Label = Channel.Retrieve(episode.IdChannel).DisplayName;
        string strTime = String.Format("{0} {1} - {2}",
                                       Utils.GetShortDayString(episode.StartTime),
                                       episode.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       episode.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        lblProgramGenre.Label = episode.Genre;
        lblProgramTime.Label = strTime;
        lblProgramDescription.Label = episode.Description;
        lblProgramTitle.Label = episode.Title;
      }
      catch (Exception ex)
      {
        Log.Error("TVProgramInfo: Error updating program description - {0}", ex.ToString());
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void Update()
    {
      try
      {
        GUIListItem lastSelectedItem = lstUpcomingEpsiodes.SelectedListItem;
        Program lastSelectedProgram;
        if (lastSelectedItem != null)
        {
          lastSelectedProgram = lastSelectedItem.MusicTag as Program;
        }
        else
        {
          lastSelectedProgram = initialProgram;// CurrentProgram;
        }

        lstUpcomingEpsiodes.Clear();
        if (CurrentProgram == null)
        {
          return;
        }

        //set program description
        UpdateProgramDescription(null);

        //check if we are recording this program
        CheckRecordingStatus();

        PopulateListviewWithUpcomingEpisodes(lastSelectedProgram);
      }
      catch (Exception ex)
      {
        Log.Error("TVProgramInfo: Error in Update() - {0}", ex.ToString());
      }
    }

    private void PopulateListviewWithUpcomingEpisodes(Program lastSelectedProgram)
    {
      int itemToSelect = -1;
      lstUpcomingEpsiodes.Clear();
      TvBusinessLayer layer = new TvBusinessLayer();
      DateTime dtDay = DateTime.Now;

      // build a list of all upcoming instances of program from EPG data based on program name alone
      List<Program> episodes = (List<Program>)layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(28), initialProgram.Title, null);

      // now if schedule is time based then build a second list for that schedule based on start time (see below)
      IList<Program> actualUpcomingEps = new List<Program>();      
      if (currentSchedule != null)
      {
        int scheduletype = currentSchedule.ScheduleType;        

        switch (scheduletype)
        {
          case (int)ScheduleRecordingType.Weekly:
            actualUpcomingEps = Program.RetrieveWeekly(initialProgram.StartTime, initialProgram.EndTime, initialProgram.IdChannel);
            break;

          case (int)ScheduleRecordingType.Weekends:
            actualUpcomingEps = Program.RetrieveWeekends(initialProgram.StartTime, initialProgram.EndTime, initialProgram.IdChannel);
            break;

          case (int)ScheduleRecordingType.WorkingDays:
            actualUpcomingEps = Program.RetrieveWorkingDays(initialProgram.StartTime, initialProgram.EndTime, initialProgram.IdChannel);
            break;

          case (int)ScheduleRecordingType.Daily:
          actualUpcomingEps = Program.RetrieveDaily(initialProgram.StartTime, initialProgram.EndTime, initialProgram.IdChannel);
          break;
        }
       
        // now if we have a time based schedule then loop through that and if entry does not exist
        // in the original list then add it
        // an entry will exist in the second list but not first if the program name is different
        // in reality this will probably be a series that has finished
        // eg. we have set a schedule for channel X for Monday 21:00 to 22:00 which happens to be when 
        // program A is on.   The series for program A finishes and now between 21:00 and 22:00 on 
        // channel X is program B.   A time based schedule does not take the program name into account
        // therefore program B will get recorded.
        if (actualUpcomingEps.Count > 0)
        {
          for (int i = actualUpcomingEps.Count - 1; i >= 0; i--)
          {
            Program ep = actualUpcomingEps[i];

            if (!episodes.Contains(ep))
            {
              episodes.Add(ep);
            }
          }
          episodes.Sort((x, y) => (x.StartTime.CompareTo(y.StartTime))); //resort list locally on starttime
        }

      }
      bool updateCurrentProgram = true;
      anyUpcomingEpisodesRecording = false;
      int activeRecordings = 0;
      for (int i = 0; i < episodes.Count; i++)
      {        
        Program episode = episodes[i];
        GUIListItem item = new GUIListItem();
        item.Label = TVUtil.GetDisplayTitle(episode);
        item.OnItemSelected += item_OnItemSelected;
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, episode.ReferencedChannel().DisplayName);
        if (string.IsNullOrEmpty(logo))                      
        {
          item.Label = String.Format("{0} {1}", episode.ReferencedChannel().DisplayName, TVUtil.GetDisplayTitle(episode));
          logo = "defaultVideoBig.png";
        }

        bool isActualUpcomingEps = actualUpcomingEps.Contains(episode) ;
        bool isRecPrg = isActualUpcomingEps;
        Schedule recordingSchedule = currentSchedule;

        // appears a little odd but seems to work
        // if episode is not in second (time based) list then override isRecPrg by actually
        // checking if episode is due to be recorded (if it is in second (time based) list then
        // it is going to be recorded
        if (!isActualUpcomingEps)
        {
            isRecPrg = (episode.IsRecording || episode.IsRecordingOncePending || episode.IsRecordingSeriesPending || 
                        episode.IsPartialRecordingSeriesPending) && IsRecordingProgram(episode, out recordingSchedule, true);
        }
        if (isRecPrg)
        {          
          if (!recordingSchedule.IsSerieIsCanceled(recordingSchedule.GetSchedStartTimeForProg(episode), episode.IdChannel))
          {
            bool hasConflict = recordingSchedule.ReferringConflicts().Count > 0;
            bool isPartialRecording = false;
            bool isSeries = (recordingSchedule.ScheduleType != (int)ScheduleRecordingType.Once);

            //check for partial recordings.
            if (isActualUpcomingEps && currentSchedule != null)
            {
              isPartialRecording = Schedule.IsPartialRecording(currentSchedule, episode);
            }
            if (isPartialRecording)
            {
              item.PinImage = hasConflict ? 
                (isSeries? Thumbs.TvConflictPartialRecordingSeriesIcon : Thumbs.TvConflictPartialRecordingIcon) : 
                (isSeries? Thumbs.TvPartialRecordingSeriesIcon : Thumbs.TvPartialRecordingIcon);
            }
            else
            {
              item.PinImage = hasConflict ? 
                (isSeries? Thumbs.TvConflictRecordingSeriesIcon : Thumbs.TvConflictRecordingIcon) : 
                (isSeries? Thumbs.TvRecordingSeriesIcon : Thumbs.TvRecordingIcon);
            }

            if (updateCurrentProgram)
            {
              //currentProgram = episode;
            }
            activeRecordings++;
            anyUpcomingEpisodesRecording = true;
            updateCurrentProgram = false;
          }          
          item.TVTag = recordingSchedule;
        }
        else
        {
          if (episode.Notify)
          {
            item.PinImage = Thumbs.TvNotifyIcon;
          }
        }

        item.MusicTag = episode;
        item.ThumbnailImage = item.IconImageBig = item.IconImage = logo;

        item.Label2 = String.Format("{0} {1} - {2}",
                                    Utils.GetShortDayString(episode.StartTime),
                                    episode.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                    episode.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        if (lastSelectedProgram != null)
        {
          if (lastSelectedProgram.IdChannel == episode.IdChannel &&
              lastSelectedProgram.StartTime == episode.StartTime &&
              lastSelectedProgram.EndTime == episode.EndTime && lastSelectedProgram.Title == episode.Title)
          {
            itemToSelect = lstUpcomingEpsiodes.Count;
          }
        }
        lstUpcomingEpsiodes.Add(item);
      }


      if (!anyUpcomingEpisodesRecording)
      {
        currentProgram = initialProgram;
      }

      if (itemToSelect != -1)
      {
        lstUpcomingEpsiodes.SelectedListItemIndex = itemToSelect;
      }

      lblUpcomingEpsiodes.Label = GUILocalizeStrings.Get(1203, new object[] { activeRecordings });

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(lstUpcomingEpsiodes.ListItems.Count));
    }

    private void CheckRecordingStatus()
    {
      IList<Schedule> schedules = Schedule.ListAll();
      bool isRecording = false;
      bool isSeries = false;

      if (currentSchedule != null)
      {
        isRecording = true;
        if ((ScheduleRecordingType)currentSchedule.ScheduleType != ScheduleRecordingType.Once)
        {
          isSeries = true;
        }
      }
      else
      {
      	// Mantis 2927 
      	// Removed loop over all schedules as this was not actually 
      	// needed and was causing performance issues
        Schedule recSched;
        isRecording = IsRecordingProgram(CurrentProgram, out recSched, true);

        if (isRecording && (ScheduleRecordingType)recSched.ScheduleType != ScheduleRecordingType.Once)
        {
          isSeries = true;
        }
      }
      if (isRecording)
      {
        btnRecord.Label = GUILocalizeStrings.Get(1039); //dont record
        btnAdvancedRecord.Disabled = btnQuality.Disabled = true;
        btnKeep.Disabled = false;

        IList<TuningDetail> details = Channel.Retrieve(CurrentProgram.IdChannel).ReferringTuningDetail();
        foreach (TuningDetail detail in details)
        {
          if (detail.ChannelType == 0)
          {
            btnQuality.Disabled = false;
            break;
          }
        }
        btnEpisodes.Disabled = !isSeries;
        btnPreRecord.Disabled = btnPostRecord.Disabled = false;
      }
      else
      {
        btnRecord.Label = GUILocalizeStrings.Get(264); //record
        btnAdvancedRecord.Disabled = false;
        btnKeep.Disabled =
          btnQuality.Disabled = btnEpisodes.Disabled = btnPreRecord.Disabled = btnPostRecord.Disabled = true;
      }
    }

    private void OnPreRecordInterval()
    {
      Schedule rec = currentSchedule;
      if (currentSchedule == null && !IsRecordingProgram(CurrentProgram, out rec, false))
      {
        return;
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1444)); //pre-record

        foreach (int interval in RecordingIntervalValues)
        {
          if (interval == 1)
          {
            if (interval == _preRec)
            {
              dlg.Add(String.Format("{0} {1}", interval,
                                    GUILocalizeStrings.Get(3003) + " (" + GUILocalizeStrings.Get(886) + ")"));
              // minute (default)
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
              dlg.Add(String.Format("{0} {1}", interval,
                                    GUILocalizeStrings.Get(3004) + " (" + GUILocalizeStrings.Get(886) + ")"));
              // minutes (default)
            }
            else
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004))); // minutes
            }
          }
        }

        if (rec.PreRecordInterval < 0)
        {
          dlg.SelectedLabel = 0;
        }
        else if (RecordingIntervalValues.IndexOf(rec.PreRecordInterval) == -1)
        {
          RecordingIntervalValues.IndexOf(_preRec); // select default if the value is not part of the list
        }
        else
        {
          dlg.SelectedLabel = RecordingIntervalValues.IndexOf(rec.PreRecordInterval);
        }

        dlg.DoModal(GetID);

        if (dlg.SelectedLabel < 0)
        {
          return;
        }

        rec.PreRecordInterval = RecordingIntervalValues[dlg.SelectedLabel];
        rec.Persist();
        currentSchedule = rec;
        
        Schedule assocSchedule = Schedule.RetrieveSpawnedSchedule(rec.IdSchedule, rec.StartTime);

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

    private void OnPostRecordInterval()
    {
      Schedule rec = currentSchedule;
      if (currentSchedule == null && !IsRecordingProgram(CurrentProgram, out rec, false))
      {
        return;
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1445)); //pre-record

        foreach (int interval in RecordingIntervalValues)
        {
          if (interval == 1)
          {
            if (interval == _postRec)
            {
              dlg.Add(String.Format("{0} {1}", interval,
                                    GUILocalizeStrings.Get(3003) + " (" + GUILocalizeStrings.Get(886) + ")"));
              // minute (default)
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
              dlg.Add(String.Format("{0} {1}", interval,
                                    GUILocalizeStrings.Get(3004) + " (" + GUILocalizeStrings.Get(886) + ")"));
              // minutes (default)
            }
            else
            {
              dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004))); // minutes
            }
          }
        }

        if (rec.PostRecordInterval < 0)
        {
          dlg.SelectedLabel = 0;
        }
        else if (RecordingIntervalValues.IndexOf(rec.PostRecordInterval) == -1)
        {
          RecordingIntervalValues.IndexOf(_postRec); // select default if the value is not part of the list
        }
        else
        {
          dlg.SelectedLabel = RecordingIntervalValues.IndexOf(rec.PostRecordInterval);
        }

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }

        rec.PostRecordInterval = RecordingIntervalValues[dlg.SelectedLabel];
        rec.Persist();
        currentSchedule = rec;

        Schedule assocSchedule = Schedule.RetrieveSpawnedSchedule(rec.IdSchedule, rec.StartTime);
        
        if (assocSchedule != null)
        {
          assocSchedule.PostRecordInterval = rec.PostRecordInterval;
          assocSchedule.Persist();
        }
      }
      Update();
    }

    private void OnSetQuality()
    {
      Schedule rec = currentSchedule;
      if (currentSchedule == null && !IsRecordingProgram(CurrentProgram, out rec, false))
      {
        return;
      }
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

        rec.BitRateMode = _newBitRate;
        rec.Persist();
        currentSchedule = rec;

        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(968); // NotSet
        dlg.AddLocalizedString(886); //Default
        dlg.AddLocalizedString(993); // Custom
        dlg.AddLocalizedString(893); //Portable
        dlg.AddLocalizedString(883); //Low
        dlg.AddLocalizedString(884); //Medium
        dlg.AddLocalizedString(885); //High
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
        {
          return;
        }
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
        currentSchedule = rec;
      }
      Update();
    }

    private void OnSetEpisodes()
    {
      Schedule rec = currentSchedule;
      if (currentSchedule == null && !IsRecordingProgram(CurrentProgram, out rec, false))
      {
        return;
      }

      TvPriorities.OnSetEpisodesToKeep(rec);
      Update();
    }

    private void OnRecordProgram(Program program)
    {
      if (program == null)
      {
        return;
      }
      Log.Debug("TVProgammInfo.OnRecordProgram - programm = {0}", program.ToString());
      Schedule recordingSchedule;
      if (!anyUpcomingEpisodesRecording && currentSchedule != null)
      {
        CancelProgram(program, currentSchedule, GetID);
      }
      else if (IsRecordingProgram(program, out recordingSchedule, true)) // check if schedule is already existing
      {
        CancelProgram(program, recordingSchedule, GetID);
      }
      else
      {
        TvServer server = new TvServer();
        VirtualCard card;
        if (TVHome.Navigator.Channel.IdChannel == program.IdChannel &&
            server.IsRecording(TVHome.Navigator.Channel.IdChannel, out card))
        {
          Schedule schedFromDB = Schedule.Retrieve(card.RecordingScheduleId);
          if (schedFromDB.IsManual)
          {
            Schedule sched = Schedule.Retrieve(card.RecordingScheduleId);
            TVUtil.DeleteRecAndSchedWithPrompt(sched, program.IdChannel);
          }
          else
          {
            CreateProgram(program, (int)ScheduleRecordingType.Once, GetID);
          }
        }
        else
        {
          CreateProgram(program, (int)ScheduleRecordingType.Once, GetID);
        }
      }
      Update();
    }

    private static void CancelProgram(Program program, Schedule schedule, int dialogId)
    {
      Log.Debug("TVProgammInfo.CancelProgram - programm = {0}", program.ToString());
      Log.Debug("                            - schedule = {0}", schedule.ToString());
      Log.Debug(" ProgramID = {0}            ScheduleID = {1}", program.IdProgram, schedule.IdSchedule);

      bool deleteEntireSched = false;

      if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        TVUtil.DeleteRecAndSchedWithPrompt(schedule, program.IdChannel);        
        ResetCurrentScheduleAndProgram(schedule);
        return;
      }

      if ((schedule.ScheduleType == (int)ScheduleRecordingType.Daily)
          || (schedule.ScheduleType == (int)ScheduleRecordingType.Weekends)
          || (schedule.ScheduleType == (int)ScheduleRecordingType.Weekly)
          || (schedule.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
          || (schedule.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnEveryChannel)
          || (schedule.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel)
          || (schedule.ScheduleType == (int) ScheduleRecordingType.WeeklyEveryTimeOnThisChannel))          
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          Log.Error("TVProgramInfo.CancelProgram: ERROR no GUIDialogMenu found !!!!!!!!!!");
          return;
        }

        dlg.Reset();
        dlg.SetHeading(TVUtil.GetDisplayTitle(program));
        if (anyUpcomingEpisodesRecording)
        {
          dlg.AddLocalizedString(981); //Cancel this show
        }
        dlg.AddLocalizedString(982); //Delete this entire schedule
        dlg.DoModal(dialogId);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }

        switch (dlg.SelectedId)
        {
          case 981: //delete specific series

            break;
          case 982: //Delete entire recording
            deleteEntireSched = true;
            break;
        }
                
        if (deleteEntireSched)
        {
          TVUtil.DeleteRecAndEntireSchedWithPrompt(schedule, program.StartTime);
          ResetCurrentScheduleAndProgram(schedule);
        }
        else
        {
          TVUtil.DeleteRecAndSchedWithPrompt(schedule, program);
        }                
      }
    }

    private static void ResetCurrentScheduleAndProgram()
    {
      currentProgram = initialProgram;
      currentSchedule = null;
    }

    private static void ResetCurrentScheduleAndProgram(Schedule schedule)
    {
      if (
        currentSchedule == null ||
        (
        currentSchedule != null &&
        (currentSchedule.ScheduleType == 0 || (currentSchedule.ScheduleType > 0 && schedule.ScheduleType > 0))
        ))
      {
        ResetCurrentScheduleAndProgram();
      }
    }

    public static void CreateProgram(Program program, int scheduleType, int dialogId)
    {
      Log.Debug("TVProgramInfo.CreateProgram: program = {0}", program.ToString());
      Schedule schedule;
      Schedule saveSchedule = null;
      TvBusinessLayer layer = new TvBusinessLayer();
      if (IsRecordingProgram(program, out schedule, false)) // check if schedule is already existing
      {
        Log.Debug("TVProgramInfo.CreateProgram - series schedule found ID={0}, Type={1}", schedule.IdSchedule,
                  schedule.ScheduleType);
        Log.Debug("                            - schedule= {0}", schedule.ToString());
        //schedule = Schedule.Retrieve(schedule.IdSchedule); // get the correct informations
        if (schedule.IsSerieIsCanceled(schedule.GetSchedStartTimeForProg(program), program.IdChannel))
        {
          //lets delete the cancelled schedule.

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
        TVConflictDialog dlg =
          (TVConflictDialog)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_TVCONFLICT);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(GUILocalizeStrings.Get(879)); // "recording conflict"
          foreach (Schedule conflict in conflicts)
          {
            Log.Debug("TVProgramInfo.CreateProgram: Conflicts = " + conflict);

            GUIListItem item = new GUIListItem(conflict.ProgramName);
            item.Label2 = GetRecordingDateTime(conflict);
            Channel channel = Channel.Retrieve(conflict.IdChannel);
            if (channel != null && !string.IsNullOrEmpty(channel.DisplayName))
            {
              item.Label3 = channel.DisplayName;
            }
            else
            {
              item.Label3 = conflict.IdChannel.ToString();
            }
            item.TVTag = conflict;
            dlg.AddConflictRecording(item);
          }
          dlg.ConflictingEpisodes = (scheduleType != (int)ScheduleRecordingType.Once);
          dlg.DoModal(dialogId);
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
                    new Program(conflict.IdChannel, conflict.StartTime, conflict.EndTime, conflict.ProgramName, "-", "-",
                                Program.ProgramState.None,
                                DateTime.MinValue, string.Empty, string.Empty, string.Empty, string.Empty, -1,
                                string.Empty, -1);
                  CancelProgram(prog, Schedule.Retrieve(conflict.IdSchedule), dialogId);
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
        saveSchedule.UnCancelSerie(program.StartTime, program.IdChannel);
        //saveSchedule.UnCancelSerie();
        saveSchedule.Persist();
        currentSchedule = saveSchedule;
      }
      else
      {
        Log.Debug("TVProgramInfo.CreateProgram - create schedule = {0}", schedule.ToString());
        schedule.Persist();

        if (currentSchedule == null || (currentSchedule.ScheduleType > 0 && schedule.ScheduleType != (int)ScheduleRecordingType.Once))
        {
          currentSchedule = schedule;
        }
      }
      if (skipConflictingEpisodes)
      {
        List<Schedule> episodes = layer.GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.EndTime)
          {
            continue;
          }
          if (episode.IsSerieIsCanceled(episode.StartTime, program.IdChannel))
          {
            continue;
          }
          foreach (Schedule conflict in conflicts)
          {
            if (episode.IsOverlapping(conflict))
            {
              Log.Debug("TVProgramInfo.CreateProgram - skip episode = {0}", episode.ToString());
              CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, program.IdChannel, episode.StartTime);
              canceledSchedule.Persist();
            }
          }
        }
      }
      server.OnNewSchedule();
    }

    private void OnAdvancedRecord()
    {
      if (CurrentProgram == null)
      {
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616)); //616=Select Schedule type
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
        dlg.Add(GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.Record_WorkingDays)));
        dlg.Add(GUILocalizeStrings.Get(WeekEndTool.GetText(DayType.Record_WeekendDays)));
        dlg.AddLocalizedString(990000); // 990000=Weekly everytime on this channel

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }

        int scheduleType = (int)ScheduleRecordingType.Once;
        switch (dlg.SelectedLabel)
        {
          case 0: //once
            scheduleType = (int)ScheduleRecordingType.Once;
            break;
          case 1: //everytime, this channel
            scheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            break;
          case 2: //everytime, all channels
            scheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            break;
          case 3: //weekly
            scheduleType = (int)ScheduleRecordingType.Weekly;
            break;
          case 4: //daily
            scheduleType = (int)ScheduleRecordingType.Daily;
            break;
          case 5: //WorkingDays
            scheduleType = (int)ScheduleRecordingType.WorkingDays;
            break;
          case 6: //Weekends
            scheduleType = (int)ScheduleRecordingType.Weekends;
            break;
          case 7://Weekly everytime, this channel
            scheduleType = (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel;
            break;
        }
        CreateProgram(CurrentProgram, scheduleType, GetID);

        if (scheduleType == (int)ScheduleRecordingType.Once)
        {
          //check if this program is interrupted (for example by a news bulletin)
          //ifso ask the user if he wants to record the 2nd part also
          DateTime dtStart = CurrentProgram.EndTime.AddMinutes(1);
          DateTime dtEnd = dtStart.AddHours(3);
          TvBusinessLayer layer = new TvBusinessLayer();
          IList<Program> programs = layer.GetPrograms(CurrentProgram.ReferencedChannel(), dtStart, dtEnd);
          if (programs.Count >= 2)
          {
            Program next = programs[0];
            Program nextNext = programs[1];
            if (nextNext.Title == CurrentProgram.Title)
            {
              TimeSpan ts = next.EndTime - nextNext.StartTime;
              if (ts.TotalMinutes <= 40)
              {
                //
                GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
                dlgYesNo.SetHeading(1012); //This program will be interrupted by
                dlgYesNo.SetLine(1, next.Title);
                dlgYesNo.SetLine(2, 1013); //Would you like to record the second part also?
                dlgYesNo.DoModal(GetID);
                if (dlgYesNo.IsConfirmed)
                {
                  CreateProgram(nextNext, scheduleType, GetID);
                  Update();
                }
              }
            }
          }
        }
      }
      Update();
    }

    private void OnKeep()
    {
      Schedule rec = currentSchedule;
      if (currentSchedule == null && !IsRecordingProgram(CurrentProgram, out rec, false))
      {
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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
      {
        return;
      }
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
            DateTime dt = CurrentProgram.StartTime.AddDays(iDay);
            dlg.Add(dt.ToLongDateString());
          }
          TimeSpan ts = (rec.KeepDate - CurrentProgram.StartTime);
          int days = (int)ts.TotalDays;
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
          rec.KeepDate = CurrentProgram.StartTime.AddDays(dlg.SelectedLabel + 1);
          break;
        case 1046:
          rec.KeepMethod = (int)KeepMethodType.Always;
          break;
      }
      rec.Persist();
      currentSchedule = rec;
      TvServer server = new TvServer();
      server.OnNewSchedule();
    }

    #endregion
  }
}