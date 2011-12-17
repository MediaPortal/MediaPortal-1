using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using TvDatabase;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class SerializationTests
  {    
    
    [Test]
    public void SerializationTest()
    {
      var scheduleConditions = new ScheduleConditionList();

      ProgramCreditDTO creditActorLowerCase = new ProgramCreditDTO();
      creditActorLowerCase.Role = "actor";
      creditActorLowerCase.Person = "clint eastWood";
      List<ProgramCreditDTO> programCreditDtos = new List<ProgramCreditDTO>();
      programCreditDtos.Add(creditActorLowerCase);
      var creditCondition = new CreditCondition(programCreditDtos);

      IList<ProgramCategoryDTO> categories = new ObservableCollection<ProgramCategoryDTO>();
      ProgramCategoryDTO cat = new ProgramCategoryDTO();
      cat.Category = "mycategory";
      var notOnCategoryCondition = new NotOnCategoryCondition(categories);

      IList<ChannelDTO> channels = new ObservableCollection<ChannelDTO>();
      ChannelDTO ch = new ChannelDTO();
      ch.IdChannel = 1;
      ch.DisplayName = "BBC";
      channels.Add(ch);
      var notOnChannelsCondition = new NotOnChannelsCondition(channels);

      IList<DayOfWeek> notOndays = new ObservableCollection<DayOfWeek>();
      notOndays.Add(DayOfWeek.Monday);
      notOndays.Add(DayOfWeek.Tuesday);
      var notOnDayCondition = new NotOnDayCondition(notOndays);

      var onCategoryCondition = new OnCategoryCondition(categories);
      var onChannelsCondition = new OnChannelsCondition(channels);

      var onDateCondition = new OnDateCondition(DateTime.Now);
      var onDayCondition = new OnDayCondition(notOndays);
      IList<int> skipEpisodes = new ObservableCollection<int>();
      skipEpisodes.Add(1);
      var onlyRecordNewEpisodesCondition = new OnlyRecordNewEpisodesCondition(skipEpisodes);
      IList<string> skipTitles = new ObservableCollection<string>();
      skipTitles.Add("hallo");
      var onlyRecordNewTitlesCondition = new OnlyRecordNewTitlesCondition(skipTitles);
      var skipRepeatsCondition = new SkipRepeatsCondition();
      var startingAroundCondition = new StartingAroundCondition();
      var startingBetweenCondition = new StartingBetweenCondition(DateTime.Now, DateTime.Now.AddDays(1));

      var programCondition = new ProgramCondition<string>("description", "lookfor", ConditionOperator.Equals);

      scheduleConditions.Add(creditCondition);
      scheduleConditions.Add(notOnCategoryCondition);
      scheduleConditions.Add(notOnChannelsCondition);
      scheduleConditions.Add(notOnDayCondition);
      scheduleConditions.Add(onCategoryCondition);
      scheduleConditions.Add(onChannelsCondition);

      scheduleConditions.Add(onDateCondition);
      scheduleConditions.Add(onDayCondition);
      scheduleConditions.Add(onlyRecordNewEpisodesCondition);
      scheduleConditions.Add(onlyRecordNewTitlesCondition);
      scheduleConditions.Add(programCondition);
      scheduleConditions.Add(skipRepeatsCondition);
      scheduleConditions.Add(startingAroundCondition);
      scheduleConditions.Add(startingBetweenCondition);            

        
      Schedule s = new Schedule(1, -1, 0, "test", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1), 1, 1,"",-1,0, DateTime.Now.AddDays(1), 15, 15, DateTime.MinValue);
      try
      {        
        s.Rules = scheduleConditions;
        s.Persist();

        Schedule sLoad = Schedule.Retrieve(s.IdSchedule);
        IList<IScheduleCondition> scheduleConditionsLoaded = sLoad.Rules;
      }     
      catch (Exception ex)
      {
        Assert.Fail("failure persisting schedule with rule data: " + ex.Message);
      }
      finally
      {
        s.Delete();
      }           

    }


    
  }
}
