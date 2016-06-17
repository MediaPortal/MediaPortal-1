using System;
using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IScheduleService
  {    
    [OperationContract]
    IList<Schedule> ListAllSchedules();
    [OperationContract(Name = "ListAllSchedulesWithSpecificRelations")]
    IList<Schedule> ListAllSchedules(ScheduleRelation includeRelations);
    [OperationContract]
    Schedule SaveSchedule(Schedule schedule);
    [OperationContract]
    Schedule GetSchedule(int idSchedule);
    [OperationContract(Name = "GetScheduleWithSpecificRelations")]
    Schedule GetSchedule(int idSchedule, ScheduleRelation includeRelations);
    [OperationContract]
    bool IsScheduleRecording(int idSchedule);

    [OperationContract]
    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    Schedule RetrieveSeriesByStartEndTimes(int idChannel, string programName, DateTime startTime, DateTime endTime);

    [OperationContract]
    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>
    /// <param name="programName">Title we wanna look for</param>    
    /// <returns>schedule instance or null</returns>
    Schedule RetrieveSeriesByProgramName(int idChannel, string programName);

    [OperationContract]
    /// <summary>
    /// Retreives the first found instance of a 'Series' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="idChannel">Channel id to look for</param>    
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    Schedule RetrieveSeries(int idChannel, DateTime startTime, DateTime endTime);

    [OperationContract]
    void DeleteSchedule(int idSchedule);

    [OperationContract]
    Schedule RetrieveSpawnedSchedule(int parentScheduleId, DateTime startTime);

    [OperationContract]
    bool IsScheduleRecordingProgram(int idSchedule, int idProgram);
    [OperationContract]
    Schedule GetScheduleWithNoEPG(int idChannel);

    [OperationContract]
    void UnCancelSerie(Schedule schedule, DateTime startTime, int idChannel);

    [OperationContract]
    IList<Schedule> GetConflictingSchedules(Schedule schedule, out List<Schedule> notViewabledSchedules);

    [OperationContract]
    IList<Schedule> GetRecordingTimes(Schedule schedule, int days);
    [OperationContract]
    IList<ScheduleRulesTemplate> ListAllScheduleRules();
    [OperationContract]
    IList<ScheduleRulesTemplate> ListAllScheduleRulesTemplates();
    [OperationContract]
    RuleBasedSchedule SaveRuleBasedSchedule(RuleBasedSchedule schedule);
  }
}
