using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public class ScheduleServiceAgent : ServiceAgent<IScheduleService>, IScheduleService
  {
    public ScheduleServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<Schedule> ListAllSchedules()
    {
      return _channel.ListAllSchedules();
    }

    public Schedule SaveSchedule(Schedule schedule)
    {
      schedule.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveSchedule(schedule);
    }

    public Schedule GetSchedule(int idSchedule)
    {
      return _channel.GetSchedule(idSchedule);
    }

    public bool IsScheduleRecording(int idSchedule)
    {
      return _channel.IsScheduleRecording(idSchedule);
    }

    public Schedule RetrieveSeriesByStartEndTimes(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      return _channel.RetrieveSeriesByStartEndTimes(idChannel, programName, startTime, endTime);
    }

    public Schedule RetrieveSeriesByProgramName(int idChannel, string programName)
    {
      return _channel.RetrieveSeriesByProgramName(idChannel, programName);
    }

    public Schedule RetrieveSeries(int idChannel, DateTime startTime, DateTime endTime)
    {
      return _channel.RetrieveSeries(idChannel, startTime, endTime);
    }

    public void DeleteSchedule(int idSchedule)
    {
      _channel.DeleteSchedule(idSchedule);
    }

    public Schedule RetrieveSpawnedSchedule(int parentScheduleId, DateTime startTime)
    {
      return _channel.RetrieveSpawnedSchedule(parentScheduleId, startTime);
    }

    public bool IsScheduleRecordingProgram(int idSchedule, int idProgram)
    {
      return _channel.IsScheduleRecordingProgram(idSchedule, idProgram);
    }

    public Schedule GetScheduleWithNoEPG(int idChannel)
    {
      return _channel.GetScheduleWithNoEPG(idChannel);
    }

    public void UnCancelSerie(Schedule schedule, DateTime startTime, int idChannel)
    {
      _channel.UnCancelSerie(schedule, startTime, idChannel);
    }

    public IList<Schedule> GetConflictingSchedules(Schedule schedule)
    {
      return _channel.GetConflictingSchedules(schedule);
    }

    public IList<Schedule> GetRecordingTimes(Schedule schedule, int days)
    {
      return _channel.GetRecordingTimes(schedule, days);
    }

    public IList<ScheduleRulesTemplate> ListAllScheduleRules()
    {
      return _channel.ListAllScheduleRules();
    }

    public IList<ScheduleRulesTemplate> ListAllScheduleRulesTemplates()
    {
      return _channel.ListAllScheduleRulesTemplates();
    }

    public RuleBasedSchedule SaveRuleBasedSchedule(RuleBasedSchedule schedule)
    {
      schedule.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveRuleBasedSchedule(schedule);
    }
  }
}
