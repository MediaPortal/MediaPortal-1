using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Channel = Mediaportal.TV.Server.TVDatabase.Entities.Channel;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ScheduleManagement
  {

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
      ProgramManagement.SynchProgramStates(new ScheduleBLL(schedule));
      return schedule;
    }

    public static Schedule GetSchedule(int idSchedule)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        return scheduleRepository.Single<Schedule>(s => s.id_Schedule == idSchedule);
      }
    }

    public static bool IsScheduleRecording(int idSchedule)
    {
      bool isScheduleRecording = false;
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        Schedule schedule = scheduleRepository.First<Schedule>(s => s.id_Schedule == idSchedule);

        if (schedule != null)
        {
          Schedule spawnedSchedule = RetrieveSpawnedSchedule(idSchedule, schedule.startTime);
          if (spawnedSchedule != null)
          {
            schedule = spawnedSchedule;
          }
          isScheduleRecording = (RecordingManagement.GetActiveRecording(schedule.id_Schedule) != null);
        }
      }
      return isScheduleRecording;
    }

    public static Schedule RetrieveSpawnedSchedule(int parentScheduleId, DateTime startTime)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository())
      {
        Schedule schedule =
          scheduleRepository.First<Schedule>(s => s.idParentSchedule == parentScheduleId && s.startTime == startTime);
        if (schedule == null)
        {
          schedule = scheduleRepository.First<Schedule>(s => s.idParentSchedule == parentScheduleId);
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
          s => s.scheduleType != 0 && s.idChannel == idChannel && s.programName == programName);
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
          s.scheduleType != 0 && s.idChannel == idChannel && s.programName == programName && s.startTime == startTime &&
          s.endTime == endTime);
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
          s => s.scheduleType != 0 && s.idChannel == idChannel && s.startTime == startTime && s.endTime == endTime);
        return retrieveSeries;
      }
    }

    public static void DeleteSchedule(int idSchedule)
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository(true))
      {        
        SetRelatedRecordingsToNull(idSchedule, scheduleRepository);
        scheduleRepository.Delete<Schedule>(s => s.id_Schedule == idSchedule);
        scheduleRepository.UnitOfWork.SaveChanges();
      }
    }

    private static void SetRelatedRecordingsToNull(int idSchedule, IScheduleRepository scheduleRepository)
    {
      // todo : since "on delete: set null" is not currently supported in EF, we have to do this manually - remove this ugly workaround once EF gets mature enough.
      var schedules = scheduleRepository.GetQuery<Schedule>(s => s.id_Schedule == idSchedule);
      schedules = scheduleRepository.IncludeAllRelations(schedules);
      Schedule schedule = schedules.FirstOrDefault();

      if (schedule != null)
      {
        //scheduleRepository.DeleteList(schedule.Recordings);
        for (int i = schedule.Recordings.Count - 1; i >= 0; i--)
        {
          Recording recording = schedule.Recordings[i];
          recording.idSchedule = null;
        }
        scheduleRepository.ApplyChanges<Schedule>(scheduleRepository.ObjectContext.Schedules, schedule);
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
            Schedule schedule = scheduleRepository.FindOne<Schedule>(s => s.id_Schedule == idSchedule);
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
              isScheduleRecording = (RecordingManagement.GetActiveRecording(schedule.id_Schedule) != null);
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
          s => s.scheduleType == 0 && s.idChannel == idChannel && s.idParentSchedule <= 0 && !s.series);
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
                                                           canceledSchedule.Schedule.programName);
          }
        }
        scheduleRepository.UnitOfWork.SaveChanges();
      }
    }


    public static IList<Schedule> GetConflictingSchedules(Schedule sched)
    {      
      sched.Channel = ChannelManagement.GetChannel(sched.idChannel);
      Log.Info("GetConflictingSchedules: Schedule = " + sched);
      var conflicts = new List<Schedule>();
      IEnumerable<Schedule> schedulesList = ListAllSchedules();

      IList<Card> cards = CardManagement.ListAllCards(CardIncludeRelationEnum.None).ToList(); //SEB
      if (cards.Count == 0)
      {
        return conflicts;
      }
      Log.Info("GetConflictingSchedules: Cards.Count = {0}", cards.Count);

      List<Schedule>[] cardSchedules = new List<Schedule>[cards.Count];
      for (int i = 0; i < cards.Count; i++)
      {
        cardSchedules[i] = new List<Schedule>();
      }

      // GEMX: Assign all already scheduled timers to cards. Assume that even possibly overlapping schedulues are ok to the user,
      // as he decided to keep them before. That's why they are in the db
      foreach (Schedule schedule in schedulesList)
      {
        List<Schedule> episodes = GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.endTime)
          {
            continue;
          }
          var episodeBLL = new ScheduleBLL(episode);
          if (episodeBLL.IsSerieIsCanceled(episode.startTime))
          {
            continue;
          }
          List<Schedule> overlapping;
          AssignSchedulesToCard(episode, cardSchedules, out overlapping);
        }
      }

      List<Schedule> newEpisodes = GetRecordingTimes(sched);
      foreach (Schedule newEpisode in newEpisodes)
      {
        if (DateTime.Now > newEpisode.endTime)
        {
          continue;
        }
        var newEpisodeBLL = new ScheduleBLL(newEpisode);
        if (newEpisodeBLL.IsSerieIsCanceled(newEpisode.startTime))
        {
          continue;
        }
        List<Schedule> overlapping;
        if (!AssignSchedulesToCard(newEpisode, cardSchedules, out overlapping))
        {
          Log.Info("GetConflictingSchedules: newEpisode can not be assigned to a card = " + newEpisode);
          conflicts.AddRange(overlapping);
        }
      }
      return conflicts;
    }

    private static bool AssignSchedulesToCard(Schedule schedule, List<Schedule>[] cardSchedules,
                                             out List<Schedule> overlappingSchedules)
    {
      overlappingSchedules = new List<Schedule>();
      Log.Info("AssignSchedulesToCard: schedule = " + schedule);
      IEnumerable<Card> cards = CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
      bool assigned = false;
      int count = 0;
      foreach (Card card in cards)
      {        
        ScheduleBLL scheduleBll = new ScheduleBLL(schedule);
        if (card.Enabled && CardManagement.CanViewTvChannel(card, schedule.idChannel))
        {
          // checks if any schedule assigned to this cards overlaps current parsed schedule
          bool free = true;
          foreach (Schedule assignedSchedule in cardSchedules[count])
          {
            Log.Info("AssignSchedulesToCard: card {0}, ID = {1} has schedule = " + assignedSchedule, count, card.IdCard);
            bool hasOverlappingSchedule = scheduleBll.IsOverlapping(assignedSchedule);
            if (hasOverlappingSchedule)
            {
              bool isSameTransponder = (scheduleBll.IsSameTransponder(assignedSchedule));
              if (!isSameTransponder)
              {
                overlappingSchedules.Add(assignedSchedule);
                Log.Info("AssignSchedulesToCard: overlapping with " + assignedSchedule + " on card {0}, ID = {1}", count,
                         card.IdCard);
                free = false;
                break;
              }
            }
          }
          if (free)
          {
            Log.Info("AssignSchedulesToCard: free on card {0}, ID = {1}", count, card.IdCard);
            cardSchedules[count].Add(schedule);
            assigned = true;
            break;
          }
        }
        count++;
      }
      if (!assigned)
      {
        return false;
      }

      return true;
    }

    public static List<Schedule> GetRecordingTimes(Schedule rec)
    {
      return GetRecordingTimes(rec, 10);
    }

    public static List<Schedule> GetRecordingTimes(Schedule rec, int days)
    {      
      var recordings = new List<Schedule>();
      var recBLL = new ScheduleBLL(rec);

      DateTime dtDay = DateTime.Now;
      if (recBLL.Entity.scheduleType == (int)ScheduleRecordingType.Once)
      {
        recordings.Add(recBLL.Entity);
        return recordings;
      }

      if (recBLL.Entity.scheduleType == (int)ScheduleRecordingType.Daily)
      {
        for (int i = 0; i < days; ++i)
        {
          var recNew = ScheduleFactory.Clone(recBLL.Entity);
          recNew.scheduleType = (int)ScheduleRecordingType.Once;
          recNew.startTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.startTime.Hour, recBLL.Entity.startTime.Minute,
                                          0);
          if (recBLL.Entity.endTime.Day > recBLL.Entity.startTime.Day)
          {
            dtDay = dtDay.AddDays(1);
          }
          recNew.endTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.endTime.Hour, recBLL.Entity.endTime.Minute, 0);
          if (recBLL.Entity.endTime.Day > recBLL.Entity.startTime.Day)
          {
            dtDay = dtDay.AddDays(-1);
          }
          recNew.series = true;
          if (recNew.startTime >= DateTime.Now)
          {
            if (recBLL.IsSerieIsCanceled(recNew.startTime))
            {
              recNew.canceled = recNew.startTime;
            }
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (recBLL.Entity.scheduleType == (int)ScheduleRecordingType.WorkingDays)
      {
        for (int i = 0; i < days; ++i)
        {
          if (WeekEndTool.IsWorkingDay(dtDay.DayOfWeek))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.scheduleType = (int)ScheduleRecordingType.Once;
            recNew.startTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.startTime.Hour, recBLL.Entity.startTime.Minute,
                                            0);
            if (recBLL.Entity.endTime.Day > recBLL.Entity.startTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.endTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.endTime.Hour, recBLL.Entity.endTime.Minute, 0);
            if (recBLL.Entity.endTime.Day > recBLL.Entity.startTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.series = true;
            if (recBLL.IsSerieIsCanceled(recNew.startTime))
            {
              recNew.canceled = recNew.startTime;
            }
            if (recNew.startTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (recBLL.Entity.scheduleType == (int)ScheduleRecordingType.Weekends)
      {
        IEnumerable<Program> progList = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.idChannel,
                                                                        recBLL.Entity.programName, dtDay,
                                                                        dtDay.AddDays(days));        
        foreach (Program prog in progList)
        {
          if ((recBLL.IsRecordingProgram(prog, false)) &&
              (WeekEndTool.IsWeekend(prog.StartTime.DayOfWeek)))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.scheduleType = (int)ScheduleRecordingType.Once;
            recNew.startTime = prog.StartTime;
            recNew.endTime = prog.EndTime;
            recNew.series = true;

            if (recBLL.IsSerieIsCanceled(recNew.startTime))
            {
              recNew.canceled = recNew.startTime;
            }
            recordings.Add(recNew);
          }
        }
        return recordings;
      }
      if (recBLL.Entity.scheduleType == (int)ScheduleRecordingType.Weekly)
      {
        for (int i = 0; i < days; ++i)
        {
          if ((dtDay.DayOfWeek == recBLL.Entity.startTime.DayOfWeek) && (dtDay.Date >= recBLL.Entity.startTime.Date))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.scheduleType = (int)ScheduleRecordingType.Once;
            recNew.startTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.startTime.Hour, recBLL.Entity.startTime.Minute,
                                            0);
            if (recBLL.Entity.endTime.Day > recBLL.Entity.startTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.endTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, recBLL.Entity.endTime.Hour, recBLL.Entity.endTime.Minute, 0);
            if (recBLL.Entity.endTime.Day > recBLL.Entity.startTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.series = true;
            if (recBLL.IsSerieIsCanceled(recNew.startTime))
            {
              recNew.canceled = recNew.startTime;
            }
            if (recNew.startTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      IEnumerable<Program> programs;
      if (recBLL.Entity.scheduleType == (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel)
      {
        //Log.Debug("get {0} {1} EveryTimeOnThisChannel", rec.ProgramName, rec.ReferencedChannel().Name);
        programs = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.idChannel,
                                                                        recBLL.Entity.programName, dtDay,
                                                                        dtDay.AddDays(days));        
        foreach (Program prog in programs)
        {
          // dtDay.DayOfWeek == rec.startTime.DayOfWeek
          // Log.Debug("BusinessLayer.cs Program prog in programs WeeklyEveryTimeOnThisChannel: {0} {1} prog.startTime.DayOfWeek == rec.startTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.startTime.DayOfWeek, rec.startTime.DayOfWeek);
          if (prog.StartTime.DayOfWeek == recBLL.Entity.startTime.DayOfWeek && recBLL.IsRecordingProgram(prog, false))
          {
            Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
            recNew.scheduleType = (int)ScheduleRecordingType.Once;
            recNew.idChannel = prog.IdChannel;
            recNew.startTime = prog.StartTime;
            recNew.endTime = prog.EndTime;
            recNew.series = true;
            if (recBLL.IsSerieIsCanceled(recNew.startTime))
            {
              recNew.canceled = recNew.startTime;
            }
            recordings.Add(recNew);
            //Log.Debug("BusinessLayer.cs Added Recording WeeklyEveryTimeOnThisChannel: {0} {1} prog.startTime.DayOfWeek == rec.startTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.startTime.DayOfWeek, rec.startTime.DayOfWeek);
          }
        }
        return recordings;
      }

      programs = recBLL.Entity.scheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel
                   ? ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(recBLL.Entity.idChannel,
                                          recBLL.Entity.programName, dtDay,
                                          dtDay.AddDays(days))
                   : ProgramManagement.GetProgramsByTitleAndStartEndTimes(recBLL.Entity.programName, dtDay, dtDay.AddDays(days));

      foreach (Program prog in programs)
      {
        if (recBLL.IsRecordingProgram(prog, false))
        {
          Schedule recNew = ScheduleFactory.Clone(recBLL.Entity);
          recNew.scheduleType = (int)ScheduleRecordingType.Once;
          recNew.idChannel = prog.IdChannel;
          recNew.startTime = prog.StartTime;
          recNew.endTime = prog.EndTime;
          recNew.series = true;
          if (recBLL.IsSerieIsCanceled(recBLL.GetSchedStartTimeForProg(prog), prog.IdChannel))
          {
            recNew.canceled = recNew.startTime;
          }
          recordings.Add(recNew);
        }
      }
      return recordings;
    }


    public static bool DoesScheduleUseEpisodeManagement(Schedule schedule)
    {
      if (schedule.maxAirings == Int32.MaxValue)
      {
        return false;
      }
      if (schedule.maxAirings < 1)
      {
        return false;
      }
      return true;
    }

    public static void DeleteOrphanedOnceSchedules()
    {
      using (IScheduleRepository scheduleRepository = new ScheduleRepository(true))
      {
        IList<Schedule> schedules =
          scheduleRepository.GetQuery<Schedule>(
            s => s.scheduleType == (int) ScheduleRecordingType.Once && s.endTime < DateTime.Now).ToList();        

        if (schedules.Count > 0)
        {
          Log.Debug("DeleteOrphanedOnceSchedules: Orphaned once schedule(s) found  - removing");

          foreach (var schedule in schedules)
          {
            SetRelatedRecordingsToNull(schedule.id_Schedule, scheduleRepository); 
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
          scheduleRepository.FindOne<Schedule>(s=>s.scheduleType == (int)ScheduleRecordingType.Once 
            && s.idChannel == idChannel && s.programName == title && s.endTime == endTime);
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
