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
      var listAllSchedules = ScheduleManagement.ListAllSchedules().ToList();
      return listAllSchedules;
    }

    public Schedule SaveSchedule(Schedule schedule)
    {
      return ScheduleManagement.SaveSchedule(schedule);      
    }

    public Schedule GetSchedule(int idSchedule)
    {
      var schedule = ScheduleManagement.GetSchedule(idSchedule);
      return schedule;
    }

    public bool IsScheduleRecording(int idSchedule)
    {
      var isScheduleRecording = ScheduleManagement.IsScheduleRecording(idSchedule);
      return isScheduleRecording;
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
      var retrieveSeries = ScheduleManagement.RetrieveSeries(idChannel, programName, startTime, endTime);
      return retrieveSeries;
    }

    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>    
    /// <returns>schedule instance or null</returns>
    public Schedule RetrieveSeriesByProgramName(int idChannel, string programName)
    {
      var retrieveSeries = ScheduleManagement.RetrieveSeries(idChannel, programName);
      return retrieveSeries;
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
      var retrieveSeries = ScheduleManagement.RetrieveSeries(idChannel, startTime, endTime);
      return retrieveSeries;
    }

    public void DeleteSchedule(int idSchedule)
    {
      ScheduleManagement.DeleteSchedule(idSchedule);
    }

    public Schedule RetrieveSpawnedSchedule(int parentScheduleId, DateTime startTime)
    {
      var retrieveSpawnedSchedule = ScheduleManagement.RetrieveSpawnedSchedule(parentScheduleId, startTime);
      return retrieveSpawnedSchedule;
    }

    public bool IsScheduleRecordingProgram(int idSchedule, int idProgram)
    {
      var isScheduleRecordingProgram = ScheduleManagement.IsScheduleRecording(idSchedule, idProgram);
      return isScheduleRecordingProgram;
    }

    public Schedule GetScheduleWithNoEPG(int idChannel)
    {
      var scheduleWithNoEpg = ScheduleManagement.GetScheduleWithNoEPG(idChannel);
      return scheduleWithNoEpg;
    }

    public void UnCancelSerie(Schedule schedule, DateTime startTime, int idChannel)
    {
      ScheduleManagement.UnCancelSerie(schedule, startTime, idChannel);
    }

    public IList<Schedule> GetConflictingSchedules(Schedule schedule)
    {
      var conflictingSchedules = ScheduleManagement.GetConflictingSchedules(schedule);
      return conflictingSchedules;
    }

    public IList<Schedule> GetRecordingTimes(Schedule schedule, int days)
    {
      var recordingTimes = ScheduleManagement.GetRecordingTimes(schedule, days);
      return recordingTimes;
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
