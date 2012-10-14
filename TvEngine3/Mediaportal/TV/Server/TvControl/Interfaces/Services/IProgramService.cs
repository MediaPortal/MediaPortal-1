using System;
using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  // Define a service contract.
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IProgramService
  {
    [OperationContract]
    IList<Program> GetProgramsForSchedule(Schedule schedule);
    [ServiceKnownType(typeof(NowAndNext))]
    [OperationContract]
    IDictionary<int, NowAndNext> GetNowAndNextForChannelGroup(int idGroup);
    [OperationContract]
    IList<Program> GetProgramsByChannelAndStartEndTimes(int idChannel, DateTime startTime, DateTime endTime);
    [OperationContract]
    Program SaveProgram(Program program);
    [OperationContract]
    IList<Program> GetNowAndNextProgramsForChannel(int idChannel);

    [OperationContract]
    Program GetProgramAt(DateTime date, int idChannel);

    [OperationContract]
    Program GetProgramByTitleAt(DateTime date, string title);

    [OperationContract]
    IList<Program> GetProgramsByTitleAndTimesInterval(string title, DateTime startTime, DateTime endTime);
    [OperationContract]
    Program GetProgramsByTitleTimesAndChannel(string programName, DateTime startTime, DateTime endTime, int idChannel);

    [OperationContract]
    IList<Program> RetrieveWeekly(DateTime startTime, DateTime endTime, int idChannel);

    [OperationContract]
    IList<Program> RetrieveWeekends(DateTime startTime, DateTime endTime, int idChannel);

    [OperationContract]
    IList<Program> RetrieveWorkingDays(DateTime startTime, DateTime endTime, int idChannel);

    [OperationContract]
    IList<Program> RetrieveDaily(DateTime startTime, DateTime endTime, int idChannel);

    [OperationContract]
    IList<Program> GetProgramsByState(ProgramState state);

    [OperationContract]
    Program GetProgramByTitleAndTimes(string programName, DateTime startTime, DateTime endTime);

    [OperationContract]
    IList<Program> GetProgramsByDescriptionAndMediaType(string descriptionCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparison);

    [OperationContract]
    IList<Program> GetProgramsByDescription(string descriptionCriteria, StringComparisonEnum stringComparison);

    [OperationContract]
    IList<Program> GetProgramsByTitle(string titleCriteria, StringComparisonEnum stringComparison);

    [OperationContract]
    IList<Program> GetProgramsByTitleAndMediaType(string titleCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparison);

    [OperationContract]
    Program GetProgram(int idProgram);

    [OperationContract]
    IDictionary<int, IList<Program>> GetProgramsForAllChannels(DateTime startTime, DateTime endTime, IEnumerable<Channel> channels);

    [OperationContract]
    IList<Program> GetProgramsByTitleAndCategoryAndMediaType(string categoryCriteriea, string titleCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparisonCategory, StringComparisonEnum stringComparisonTitle);

    [OperationContract]
    IList<Program> GetProgramsByTimesInterval(DateTime startTime, DateTime endTime);

    [OperationContract]
    IList<Program> GetProgramsByChannelAndTitleAndStartEndTimes(int idChannel, string title, DateTime startTime, DateTime endTime);

    [OperationContract]
    IList<Program> GetProgramsByTitleAndStartEndTimes(string title, DateTime startTime, DateTime endTime);

    [OperationContract]
    void DeleteAllPrograms();

    [OperationContract]
    ProgramCategory GetProgramCategoryByName(string category);

    [OperationContract]
    IList<string> ListAllDistinctCreditRoles();
    [OperationContract]
    IList<ProgramCategory> ListAllCategories();
    [OperationContract]
    IList<ProgramCredit> ListAllCredits();
    [OperationContract]
    IList<Program> RetrieveByChannelAndTimesInterval(int channelId, DateTime startTime, DateTime endTime);
    [OperationContract]
    void InitiateInsertPrograms();
  }
}
