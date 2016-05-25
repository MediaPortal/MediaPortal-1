using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ScheduleManagement
  {

    public static IList<Schedule> ListAllSchedules(ScheduleIncludeRelationEnum includeRelations)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        IQueryable<Schedule> listAllSchedules = scheduleRepository.GetAll<Schedule>();
        listAllSchedules = scheduleRepository.IncludeAllRelations(listAllSchedules, includeRelations);
        return listAllSchedules.ToList();
      }
    }

    public static IList<Schedule> ListAllSchedules()
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        IQueryable<Schedule> listAllSchedules = scheduleRepository.GetAll<Schedule>();
        listAllSchedules = scheduleRepository.IncludeAllRelations(listAllSchedules);
        return listAllSchedules.ToList();
      }
    }

    public static Schedule SaveSchedule(Schedule schedule)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        scheduleRepository.AttachEntityIfChangeTrackingDisabled(scheduleRepository.ObjectContext.Schedules, schedule);
        scheduleRepository.ApplyChanges(scheduleRepository.ObjectContext.Schedules, schedule);
        scheduleRepository.UnitOfWork.SaveChanges();
        schedule.AcceptChanges();
      }
      ProgramManagement.SynchProgramStates(schedule.IdSchedule);
      return schedule;
    }

    public static Schedule GetSchedule(int idSchedule)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        return scheduleRepository.Single<Schedule>(s => s.IdSchedule == idSchedule);
      }
    }

    public static Schedule GetSchedule(int idSchedule, ScheduleIncludeRelationEnum includeRelations)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        var query = scheduleRepository.GetQuery<Schedule>(s => s.IdSchedule == idSchedule);
        query = scheduleRepository.IncludeAllRelations(query, includeRelations);
        var schedule = query.FirstOrDefault();
        return schedule;
      }
    }

    public static bool IsScheduleRecording(int idSchedule)
    {
      bool isScheduleRecording = false;
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        Schedule schedule = scheduleRepository.First<Schedule>(s => s.IdSchedule == idSchedule);

        if (schedule != null)
        {
          Schedule spawnedSchedule = RetrieveSpawnedSchedule(idSchedule, schedule.StartTime);
          if (spawnedSchedule != null)
          {
            schedule = spawnedSchedule;
          }
          isScheduleRecording = (RecordingManagement.GetActiveRecording(schedule.IdSchedule) != null);
        }
      }
      return isScheduleRecording;
    }

    public static Schedule RetrieveSpawnedSchedule(int parentScheduleId, DateTime startTime)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        Schedule schedule = scheduleRepository.First<Schedule>(s => s.IdParentSchedule == parentScheduleId && s.StartTime == startTime);
        if (schedule == null)
        {
          schedule = scheduleRepository.First<Schedule>(s => s.IdParentSchedule == parentScheduleId);
        }
        return schedule;
      }
    }

    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>    
    /// <returns>schedule instance or null</returns>
    public static Schedule RetrieveSeries(int idChannel, string programName)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        var retrieveSeries = scheduleRepository.First<Schedule>(
          s => s.ScheduleType != (int)ScheduleRecordingType.Once && s.IdChannel == idChannel && s.ProgramName == programName);
        return retrieveSeries;
      }
    }

    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    public static Schedule RetrieveSeries(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        var retrieveSeries = scheduleRepository.First<Schedule>(
          s =>
          s.ScheduleType != (int)ScheduleRecordingType.Once && s.IdChannel == idChannel && s.ProgramName == programName && s.StartTime == startTime &&
          s.EndTime == endTime);
        return retrieveSeries;
      }
    }

    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>    
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    public static Schedule RetrieveSeries(int idChannel, DateTime startTime, DateTime endTime)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        var retrieveSeries = scheduleRepository.First<Schedule>(
          s => s.ScheduleType != (int)ScheduleRecordingType.Once && s.IdChannel == idChannel && s.StartTime == startTime && s.EndTime == endTime);
        return retrieveSeries;
      }
    }

    public static void DeleteSchedule(int idSchedule)
    {
      Schedule scheduleToDelete;
      using (IScheduleRepository scheduleRepository = new ScheduleRepository(true))
      {
        scheduleToDelete = scheduleRepository.First<Schedule>(schedule => schedule.IdSchedule == idSchedule);
        if (scheduleToDelete == null)
          return;

        SetRelatedRecordingsToNull(idSchedule, scheduleRepository);

        scheduleRepository.Delete(scheduleToDelete);
        scheduleRepository.UnitOfWork.SaveChanges();
        scheduleToDelete.MarkAsDeleted();
      }
      ProgramManagement.SynchProgramStates(new ScheduleBLL(scheduleToDelete));
    }

    private static void SetRelatedRecordingsToNull(int idSchedule, IScheduleRepository scheduleRepository)
    {
      // todo : since "on delete: set null" is not currently supported in EF, we have to do this manually - remove this ugly workaround once EF gets mature enough.
      var recordings = scheduleRepository.GetQuery<Recording>(r => r.IdSchedule == idSchedule).ToList();
      if (recordings.Count > 0)
      {
        foreach (Recording r in recordings)
        {
          r.IdSchedule = null;
        }
        scheduleRepository.ApplyChanges<Recording>(scheduleRepository.ObjectContext.Recordings, recordings);
      }
    }

    public static bool IsScheduleRecording(int idSchedule, int idProgram)
    {
      bool isScheduleRecording = false;
      Program prg = ProgramManagement.GetProgram(idProgram);

      if (prg != null)
      {
        var programBll = new ProgramBLL(prg);
        if (programBll.IsRecording)
        {
          using (IScheduleRepository scheduleRepository = new ScheduleRepository())
          {
            Schedule schedule = scheduleRepository.FindOne<Schedule>(s => s.IdSchedule == idSchedule);
            if (schedule != null)
            {
              Schedule spawnedSchedule = RetrieveSpawnedSchedule(idSchedule, prg.StartTime);
              if (spawnedSchedule != null)
              {
                schedule = spawnedSchedule;
              }
            }
            if (schedule != null)
            {
              isScheduleRecording = (RecordingManagement.GetActiveRecording(schedule.IdSchedule) != null);
            }
          }
        }
      }

      return isScheduleRecording;
    }

    public static Schedule GetScheduleWithNoEPG(int idChannel)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        var scheduleWithNoEpg = scheduleRepository.FindOne<Schedule>(
          s => s.ScheduleType == (int)ScheduleRecordingType.Once && s.IdChannel == idChannel && s.IdParentSchedule <= 0 && !s.Series);
        return scheduleWithNoEpg;
      }
    }

    public static void UnCancelSerie(Schedule schedule, DateTime startTime, int idChannel)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository(true))
      {
        foreach (CanceledSchedule canceledSchedule in schedule.CanceledSchedules)
        {
          if (canceledSchedule.CancelDateTime == startTime && canceledSchedule.IdChannel == idChannel)
          {
            scheduleRepository.Delete(canceledSchedule);
            ProgramManagement.SetSingleStateSeriesPending(canceledSchedule.CancelDateTime,
                                                           canceledSchedule.IdChannel,
                                                           canceledSchedule.Schedule.ProgramName);
          }
        }
        scheduleRepository.UnitOfWork.SaveChanges();
      }
    }

    public static IList<Schedule> GetConflictingSchedules(Schedule sched, out List<Schedule> notViewableSchedules)
    {
      var conflicts = new List<Schedule>();
      notViewableSchedules = new List<Schedule>();
      sched.Channel = ChannelManagement.GetChannel(sched.IdChannel);
      Log.Info("GetConflictingSchedules: schedule = {0}", sched);

      IList<Tuner> tuners = TunerManagement.ListAllTuners(TunerIncludeRelationEnum.ChannelMaps);
      Log.Info("GetConflictingSchedules: tuner count = {0}", tuners.Count);
      if (tuners.Count == 0)
      {
        return conflicts;
      }

      IDictionary<int, HashSet<int>> tunerGroups = new Dictionary<int, HashSet<int>>();
      IDictionary<int, IList<Schedule>> tunerSchedules = new Dictionary<int, IList<Schedule>>(tuners.Count);
      foreach (Tuner tuner in tuners)
      {
        if (tuner.IsEnabled)
        {
          tunerSchedules[tuner.IdTuner] = new List<Schedule>();
          if (tuner.IdTunerGroup.HasValue)
          {
            HashSet<int> tunerIds;
            if (!tunerGroups.TryGetValue(tuner.IdTunerGroup.Value, out tunerIds))
            {
              tunerIds = new HashSet<int>();
              tunerGroups[tuner.IdTunerGroup.Value] = tunerIds;
            }
            tunerIds.Add(tuner.IdTuner);
          }
        }
      }

      // GEMX: Assign all already scheduled "episodes" to tuners. Ignore
      // existing conflicts. The user had the opportunity to deal with them
      // when they created the schedule(s). The fact that the schedules are
      // still in the DB means the user decided to keep the conflicts.
      Log.Info("GetConflictingSchedules: assign existing schedules to tuners");
      int defaultPreRecordInterval = SettingsManagement.GetValue("preRecordInterval", 7);
      int defaultPostRecordInterval = SettingsManagement.GetValue("postRecordInterval", 10);
      IEnumerable<Schedule> schedulesList = ListAllSchedules();
      List<Schedule> newEpisodes = GetRecordingTimes(sched);
      foreach (Schedule schedule in schedulesList)
      {
        List<Schedule> episodes = GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.EndTime)
          {
            continue;
          }
          var episodeBLL = new ScheduleBLL(episode);
          if (episodeBLL.IsSerieIsCanceled(episode.StartTime))
          {
            continue;
          }

          // Save effort: only assign existing episodes that overlap with one
          // of the new episodes. These are the only episodes with potential
          // for conflicts.
          foreach (Schedule newEpisode in newEpisodes)
          {
            if (DateTime.Now > newEpisode.EndTime)
            {
              continue;
            }
            var newEpisodeBLL = new ScheduleBLL(newEpisode);
            if (newEpisodeBLL.IsSerieIsCanceled(newEpisode.StartTime))
            {
              continue;
            }
            if (newEpisodeBLL.IsOverlapping(episode, defaultPreRecordInterval, defaultPostRecordInterval))
            {
              IList<Schedule> existingConflicts;
              bool isTunable;
              if (!AssignSchedulesToTuner(episode, tuners, tunerGroups, tunerSchedules, defaultPreRecordInterval, defaultPostRecordInterval, out existingConflicts, out isTunable))
              {
                Log.Warn("GetConflictingSchedules: existing episode cannot be assigned to a tuner, episode = {0}", episode);
              }
            }
          }
        }
      }

      Log.Info("GetConflictingSchedules: try to assign new schedule episodes to tuners");
      foreach (Schedule newEpisode in newEpisodes)
      {
        if (DateTime.Now > newEpisode.EndTime)
        {
          continue;
        }
        var newEpisodeBLL = new ScheduleBLL(newEpisode);
        if (newEpisodeBLL.IsSerieIsCanceled(newEpisode.StartTime))
        {
          continue;
        }
        IList<Schedule> newConflicts;
        bool isTunable;
        if (!AssignSchedulesToTuner(newEpisode, tuners, tunerGroups, tunerSchedules, defaultPreRecordInterval, defaultPostRecordInterval, out newConflicts, out isTunable))
        {
          Log.Warn("GetConflictingSchedules: new episode cannot be assigned to a tuner, episode = {0}, is tunable = {1}", newEpisode, isTunable);
          if (newConflicts.Count > 0)
          {
            Log.Warn("  {0} conflicts(s)...", newConflicts.Count);
            foreach (Schedule s in newConflicts)
            {
              Log.Warn("    {0}", s);
            }
          }
          conflicts.AddRange(newConflicts);
          notViewableSchedules.Add(newEpisode);
        }
      }
      return conflicts;
    }

    private static bool AssignSchedulesToTuner(Schedule schedule, IList<Tuner> tuners, IDictionary<int, HashSet<int>> tunerGroups, IDictionary<int, IList<Schedule>> tunerSchedules,
                                                int defaultPreRecordInterval, int defaultPostRecordInterval,
                                                out IList<Schedule> conflictingSchedules, out bool isTunable)
    {
      conflictingSchedules = new List<Schedule>();
      isTunable = false;

      bool assigned = false;
      foreach (Tuner tuner in tuners)
      {
        ScheduleBLL scheduleBll = new ScheduleBLL(schedule);
        if (TunerManagement.CanTuneChannel(tuner, schedule.IdChannel))
        {
          isTunable = true;

          // Check if any schedule which is already assigned to this tuner
          // overlaps/conflicts with the schedule we've been asked to assign.
          bool free = true;
          foreach (Schedule assignedSchedule in tunerSchedules[tuner.IdTuner])
          {
            if (scheduleBll.IsOverlapping(assignedSchedule, defaultPreRecordInterval, defaultPostRecordInterval) && !scheduleBll.IsSameTransponder(assignedSchedule))
            {
              conflictingSchedules.Add(assignedSchedule);
              free = false;
              // Don't break here because we want to know about all the
              // conflicts, not just the first.
              //break;
            }
          }

          // Apply the same check for any tuners which are in the same tuner
          // group. By definition, only one tuner in each tuner group can be
          // used at any given time. Therefore overlaps with schedules assigned
          // to other tuners in the same group are also conflicts.
          if (tuner.IdTunerGroup.HasValue)
          {
            HashSet<int> tunersInGroup;
            if (tunerGroups.TryGetValue(tuner.IdTunerGroup.Value, out tunersInGroup))
            {
              foreach (int tunerIdOtherTunerInGroup in tunersInGroup)
              {
                if (tunerIdOtherTunerInGroup != tuner.IdTuner)
                {
                  foreach (Schedule assignedSchedule in tunerSchedules[tunerIdOtherTunerInGroup])
                  {
                    if (scheduleBll.IsOverlapping(assignedSchedule, defaultPreRecordInterval, defaultPostRecordInterval) && !scheduleBll.IsSameTransponder(assignedSchedule))
                    {
                      conflictingSchedules.Add(assignedSchedule);
                      free = false;
                      // Don't break here because we want to know about all the
                      // conflicts, not just the first.
                      //break;
                    }
                  }
                }
              }
            }
          }

          if (free)
          {
            Log.Info("AssignSchedulesToTuner: assign schedule, tuner ID = {0}, schedule = {1}", tuner.IdTuner, schedule);
            tunerSchedules[tuner.IdTuner].Add(schedule);
            assigned = true;
            break;
          }
        }
      }

      return (isTunable && assigned);
    }

    public static List<Schedule> GetRecordingTimes(Schedule rec, int days = 10)
    {
      var recordings = new List<Schedule>();
      var recBLL = new ScheduleBLL(rec);

      DateTime dtDay = DateTime.Now;
      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        recordings.Add(recBLL.Entity);
        return recordings;
      }

      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Daily)
      {
        for (int i = 0; i < days; ++i)
        {
          var recNew = ScheduleFactory.Clone(recBLL.Entity);
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.StartTime.Hour, recBLL.Entity.StartTime.Minute,
                                          0);
          if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
          {
            dtDay = dtDay.AddDays(1);
          }
          recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.EndTime.Hour, recBLL.Entity.EndTime.Minute, 0);
          if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
          {
            dtDay = dtDay.AddDays(-1);
          }
          recNew.Series = true;
          if (recNew.StartTime >= DateTime.Now)
          {
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
      {
        for (int i = 0; i < days; ++i)
        {
          if (WeekEndTool.IsWorkingDay(dtDay.DayOfWeek))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.StartTime.Hour, recBLL.Entity.StartTime.Minute,
                                            0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.EndTime.Hour, recBLL.Entity.EndTime.Minute, 0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Weekends)
      {
        IEnumerable<Program> progList = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.IdChannel,
                                                                        recBLL.Entity.ProgramName, dtDay,
                                                                        dtDay.AddDays(days));
        foreach (Program prog in progList)
        {
          if ((recBLL.IsRecordingProgram(prog, false)) &&
              (WeekEndTool.IsWeekend(prog.StartTime.DayOfWeek)))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;

            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
        }
        return recordings;
      }
      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.Weekly)
      {
        for (int i = 0; i < days; ++i)
        {
          if ((dtDay.DayOfWeek == recBLL.Entity.StartTime.DayOfWeek) && (dtDay.Date >= recBLL.Entity.StartTime.Date))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.StartTime.Hour, recBLL.Entity.StartTime.Minute,
                                            0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.EndTime.Hour, recBLL.Entity.EndTime.Minute, 0);
            if (recBLL.Entity.EndTime.Day > recBLL.Entity.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      IEnumerable<Program> programs;
      if (recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel)
      {
        //this.LogDebug("get {0} {1} EveryTimeOnThisChannel", rec.ProgramName, rec.ReferencedChannel().Name);
        programs = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.IdChannel,
                                                                        recBLL.Entity.ProgramName, dtDay,
                                                                        dtDay.AddDays(days));
        foreach (Program prog in programs)
        {
          // dtDay.DayOfWeek == rec.startTime.DayOfWeek
          // this.LogDebug("BusinessLayer.cs Program prog in programs WeeklyEveryTimeOnThisChannel: {0} {1} prog.startTime.DayOfWeek == rec.startTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.startTime.DayOfWeek, rec.startTime.DayOfWeek);
          if (prog.StartTime.DayOfWeek == recBLL.Entity.StartTime.DayOfWeek && recBLL.IsRecordingProgram(prog, false))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.IdChannel = prog.IdChannel;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;
            if (recBLL.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
            //this.LogDebug("BusinessLayer.cs Added Recording WeeklyEveryTimeOnThisChannel: {0} {1} prog.startTime.DayOfWeek == rec.startTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.startTime.DayOfWeek, rec.startTime.DayOfWeek);
          }
        }
        return recordings;
      }

      programs = recBLL.Entity.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel
                   ? ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.IdChannel,
                                          recBLL.Entity.ProgramName, dtDay,
                                          dtDay.AddDays(days))
                   : ProgramManagement.GetProgramsByTitleAndStartEndTimes(recBLL.Entity.ProgramName, dtDay, dtDay.AddDays(days));

      foreach (Program prog in programs)
      {
        if (recBLL.IsRecordingProgram(prog, false))
        {
          Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.IdChannel = prog.IdChannel;
          recNew.StartTime = prog.StartTime;
          recNew.EndTime = prog.EndTime;
          recNew.Series = true;
          if (recBLL.IsSerieIsCanceled(recBLL.GetSchedStartTimeForProg(prog), prog.IdChannel))
          {
            recNew.Canceled = recNew.StartTime;
          }
          recordings.Add(recNew);
        }
      }
      return recordings;
    }

    public static void DeleteOrphanedOnceSchedules()
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository(true))
      {
        IList<Schedule> schedules =
          scheduleRepository.GetQuery<Schedule>(
            s => s.ScheduleType == (int)ScheduleRecordingType.Once && s.EndTime < DateTime.Now).ToList();

        if (schedules.Count > 0)
        {
          Log.Debug("DeleteOrphanedOnceSchedules: Orphaned once schedule(s) found  - removing");

          foreach (var schedule in schedules)
          {
            SetRelatedRecordingsToNull(schedule.IdSchedule, scheduleRepository);
          }

          scheduleRepository.DeleteList(schedules);
          scheduleRepository.UnitOfWork.SaveChanges();
        }
      }
    }

    public static Schedule RetrieveOnce(int idChannel, string title, DateTime startTime, DateTime endTime)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        Schedule onceSchedule =
          scheduleRepository.FindOne<Schedule>(s => s.ScheduleType == (int)ScheduleRecordingType.Once
            && s.IdChannel == idChannel && s.ProgramName == title && s.EndTime == endTime);
        return onceSchedule;
      }
    }

    public static IList<ScheduleRulesTemplate> ListAllScheduleRules()
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        IQueryable<ScheduleRulesTemplate> listAllScheduleRules = scheduleRepository.GetAll<ScheduleRulesTemplate>();
        //listAllScheduleRules = scheduleRepository.IncludeAllRelations(listAllScheduleRules);
        return listAllScheduleRules.ToList();
      }
    }

    public static IList<ScheduleRulesTemplate> ListAllScheduleRulesTemplates()
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        IQueryable<ScheduleRulesTemplate> listAllScheduleRulesTemplates = scheduleRepository.GetAll<ScheduleRulesTemplate>();
        //listAllScheduleRules = scheduleRepository.IncludeAllRelations(listAllScheduleRules);
        return listAllScheduleRulesTemplates.ToList();
      }
    }

    public static RuleBasedSchedule SaveRuleBasedSchedule(RuleBasedSchedule schedule)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        scheduleRepository.AttachEntityIfChangeTrackingDisabled(scheduleRepository.ObjectContext.RuleBasedSchedules, schedule);
        scheduleRepository.ApplyChanges(scheduleRepository.ObjectContext.RuleBasedSchedules, schedule);
        scheduleRepository.UnitOfWork.SaveChanges();
        schedule.AcceptChanges();
        return schedule;
      }
    }
  }
}
