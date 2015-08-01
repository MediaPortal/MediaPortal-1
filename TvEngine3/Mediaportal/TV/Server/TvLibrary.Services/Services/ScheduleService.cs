using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class ScheduleService : IScheduleService
  {
    public IList<Schedule> ListAllSchedules()
    {
      return ScheduleManagement.ListAllSchedules().ToList();
    }

    public Schedule SaveSchedule(Schedule schedule)
    {
      return ScheduleManagement.SaveSchedule(schedule);      
    }

    public Schedule GetSchedule(int idSchedule)
    {
      return ScheduleManagement.GetSchedule(idSchedule);
    }

    public bool IsScheduleRecording(int idSchedule)
    {
      return ScheduleManagement.IsScheduleRecording(idSchedule);
    }

    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    public Schedule RetrieveSeriesByStartEndTimes(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      return ScheduleManagement.RetrieveSeries(idChannel, programName, startTime, endTime);
    }

    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>    
    /// <returns>schedule instance or null</returns>
    public Schedule RetrieveSeriesByProgramName(int idChannel, string programName)
    {
      return ScheduleManagement.RetrieveSeries(idChannel, programName);
    }

    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>    
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    public Schedule RetrieveSeries(int idChannel, DateTime startTime, DateTime endTime)
    {
      return ScheduleManagement.RetrieveSeries(idChannel, startTime, endTime);
    }

    public void DeleteSchedule(int idSchedule)
    {
      ScheduleManagement.DeleteSchedule(idSchedule);
    }

    public Schedule RetrieveSpawnedSchedule(int parentScheduleId, DateTime startTime)
    {
      return ScheduleManagement.RetrieveSpawnedSchedule(parentScheduleId, startTime);
    }

    public bool IsScheduleRecordingProgram(int idSchedule, int idProgram)
    {
      return ScheduleManagement.IsScheduleRecording(idSchedule, idProgram);
    }

    public Schedule GetScheduleWithNoEPG(int idChannel)
    {
      return ScheduleManagement.GetScheduleWithNoEPG(idChannel);
    }

    public void UnCancelSerie(Schedule schedule, DateTime startTime, int idChannel)
    {
      ScheduleManagement.UnCancelSerie(schedule, startTime, idChannel);
    }

    public IList<Schedule> GetConflictingSchedules(Schedule schedule, out List<Schedule> notViewableSchedules)
    {
      return ScheduleManagement.GetConflictingSchedules(schedule, out notViewableSchedules);
    }

    public IList<Schedule> GetRecordingTimes(Schedule schedule, int days)
    {
      return ScheduleManagement.GetRecordingTimes(schedule, days);
    }

    public IList<ScheduleRulesTemplate> ListAllScheduleRules()
    {
      return ScheduleManagement.ListAllScheduleRules();
    }

    public IList<ScheduleRulesTemplate> ListAllScheduleRulesTemplates()
    {
      return ScheduleManagement.ListAllScheduleRulesTemplates();
    }

    public RuleBasedSchedule SaveRuleBasedSchedule(RuleBasedSchedule schedule)
    {
      return ScheduleManagement.SaveRuleBasedSchedule(schedule);
    }
  }  
}
