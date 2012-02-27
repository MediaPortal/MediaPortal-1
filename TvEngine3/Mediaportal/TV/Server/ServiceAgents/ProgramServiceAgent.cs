using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public class ProgramServiceAgent : ServiceAgent<IProgramService>, IProgramService
  {
    public ProgramServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<Program> GetProgramsForSchedule(Schedule schedule)
    {
      return _channel.GetProgramsForSchedule(schedule);
    }

    public IDictionary<int, NowAndNext> GetNowAndNext(List<Channel> channels)
    {
      return _channel.GetNowAndNext(channels);
    }

    public IList<Program> GetProgramsByChannelAndStartEndTimes(int idChannel, DateTime startTime, DateTime endTime)
    {
      return _channel.GetProgramsByChannelAndStartEndTimes(idChannel, startTime, endTime);
    }

    public Program SaveProgram(Program program)
    {
      program.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveProgram(program);
    }

    public IList<Program> GetNowAndNextProgramsForChannel(int idChannel)
    {
      return _channel.GetNowAndNextProgramsForChannel(idChannel);
    }

    public Program GetProgramAt(DateTime date, int idChannel)
    {
      return _channel.GetProgramAt(date, idChannel);
    }

    public Program GetProgramByTitleAt(DateTime date, string title)
    {
      return _channel.GetProgramByTitleAt(date, title);
    }

    public IList<Program> GetProgramsByTitleAndTimesInterval(string title, DateTime startTime, DateTime endTime)
    {
      return _channel.GetProgramsByTitleAndTimesInterval(title, startTime, endTime);
    }

    public Program GetProgramsByTitleTimesAndChannel(string programName, DateTime startTime, DateTime endTime, int idChannel)
    {
      return _channel.GetProgramsByTitleTimesAndChannel(programName, startTime, endTime, idChannel);
    }

    public IList<Program> RetrieveWeekly(DateTime startTime, DateTime endTime, int idChannel)
    {
      return _channel.RetrieveWeekly(startTime, endTime, idChannel);
    }

    public IList<Program> RetrieveWeekends(DateTime startTime, DateTime endTime, int idChannel)
    {
      return _channel.RetrieveWeekends(startTime, endTime, idChannel);
    }

    public IList<Program> RetrieveWorkingDays(DateTime startTime, DateTime endTime, int idChannel)
    {
      return _channel.RetrieveWorkingDays(startTime, endTime, idChannel);
    }

    public IList<Program> RetrieveDaily(DateTime startTime, DateTime endTime, int idChannel)
    {
      return _channel.RetrieveDaily(startTime, endTime, idChannel);
    }

    public IList<Program> GetProgramsByState(ProgramState state)
    {
      return _channel.GetProgramsByState(state);
    }

    public Program GetProgramByTitleAndTimes(string programName, DateTime startTime, DateTime endTime)
    {
      return _channel.GetProgramByTitleAndTimes(programName, startTime, endTime);
    }

    public IList<Program> GetProgramsByDescriptionAndMediaType(string descriptionCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparison)
    {
      return _channel.GetProgramsByDescriptionAndMediaType(descriptionCriteria, mediaType, stringComparison);
    }

    public IList<Program> GetProgramsByDescription(string descriptionCriteria, StringComparisonEnum stringComparison)
    {
      return _channel.GetProgramsByDescription(descriptionCriteria, stringComparison);
    }

    public IList<Program> GetProgramsByTitle(string titleCriteria, StringComparisonEnum stringComparison)
    {
      return _channel.GetProgramsByTitle(titleCriteria, stringComparison);
    }

    public IList<Program> GetProgramsByTitleAndMediaType(string titleCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparison)
    {
      return _channel.GetProgramsByTitleAndMediaType(titleCriteria, mediaType, stringComparison);
    }

    public Program GetProgram(int idProgram)
    {
      return _channel.GetProgram(idProgram);
    }

    public IDictionary<int, IList<Program>> GetProgramsForAllChannels(DateTime startTime, DateTime endTime, IEnumerable<Channel> channels)
    {
      return _channel.GetProgramsForAllChannels(startTime, endTime, channels);
    }

    public IList<Program> GetProgramsByTitleAndCategoryAndMediaType(string categoryCriteriea, string titleCriteria, MediaTypeEnum mediaType, StringComparisonEnum stringComparisonCategory, StringComparisonEnum stringComparisonTitle)
    {
      return _channel.GetProgramsByTitleAndCategoryAndMediaType(categoryCriteriea, titleCriteria, mediaType, stringComparisonCategory, stringComparisonTitle);
    }

    public IList<Program> GetProgramsByTimesInterval(DateTime startTime, DateTime endTime)
    {
      return _channel.GetProgramsByTimesInterval(startTime, endTime);
    }

    public IList<Program> GetProgramsByChannelAndTitleAndStartEndTimes(int idChannel, string title, DateTime startTime, DateTime endTime)
    {
      return _channel.GetProgramsByChannelAndTitleAndStartEndTimes(idChannel, title, startTime, endTime);
    }

    public IList<Program> GetProgramsByTitleAndStartEndTimes(string title, DateTime startTime, DateTime endTime)
    {
      return _channel.GetProgramsByTitleAndStartEndTimes(title, startTime, endTime);
    }

    public void DeleteAllPrograms()
    {
      _channel.DeleteAllPrograms();
    }

    public ProgramCategory GetProgramCategoryByName(string category)
    {
      return _channel.GetProgramCategoryByName(category);
    }

    public IList<string> ListAllDistinctCreditRoles()
    {
      return _channel.ListAllDistinctCreditRoles();
    }

    public IList<ProgramCategory> ListAllCategories()
    {
      return _channel.ListAllCategories();
    }

    public IList<ProgramCredit> ListAllCredits()
    {
      return _channel.ListAllCredits();
    }

    public IList<Program> RetrieveByChannelAndTimesInterval(int channelId, DateTime startTime, DateTime endTime)
    {
      return _channel.RetrieveByChannelAndTimesInterval(channelId, startTime, endTime);
    }

    public void InitiateInsertPrograms()
    {
      _channel.InitiateInsertPrograms();
    }
  }
}
