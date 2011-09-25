using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TvLibrary.Interfaces
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
        
  }
}
