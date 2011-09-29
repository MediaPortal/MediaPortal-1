using System.Collections.Generic;
using System.Runtime.Serialization;
using RuleBasedScheduler.ScheduleConditions;
using TVDatabaseEntities;

namespace RuleBasedScheduler
{
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [KnownType(typeof(CreditCondition))]  
  [KnownType(typeof(NotOnCategoryCondition))]  
  [KnownType(typeof(NotOnChannelsCondition))]  
  [KnownType(typeof(NotOnDayCondition))]  
  [KnownType(typeof(OnCategoryCondition))]  
  [KnownType(typeof(OnChannelsCondition))]  
  [KnownType(typeof(OnDateCondition))]  
  [KnownType(typeof(OnDayCondition))]  
  [KnownType(typeof(OnlyRecordNewEpisodesCondition))]  
  [KnownType(typeof(OnlyRecordNewTitlesCondition))]  
  [KnownType(typeof(ProgramCondition<string>))]  
  [KnownType(typeof(ProgramCondition<int>))]  
  [KnownType(typeof(SkipRepeatsCondition))]  
  [KnownType(typeof(StartingAroundCondition))]  
  [KnownType(typeof(StartingBetweenCondition))]
  [CollectionDataContract]
  public class ScheduleConditionList : List<IScheduleCondition>
  {

    // would the param in  GetTitle(ProgramDTO program) be needed at all here ? 
    // <<-- yep because GetTitle would get info from program to replace the placeholders in the title template
    public string GetTitle(Program program)
    {
      //todo impl
      return null;
    }

    public string GetDescription(Program program)
    {
      //todo impl
      return null;
    }
  }
}
