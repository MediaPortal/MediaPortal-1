using System.Linq;
using TVDatabaseEntities;

namespace RuleBasedScheduler
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
