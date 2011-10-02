using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler
{
  /// <summary>
  /// 
  /// </summary>  
  public interface IScheduleCondition
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseQuery"></param>
    /// <returns></returns>
    IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery);
  }
}
