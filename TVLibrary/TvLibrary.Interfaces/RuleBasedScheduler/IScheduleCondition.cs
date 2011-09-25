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
  public interface IScheduleCondition
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseQuery"></param>
    /// <returns></returns>
    IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery);
  }
}
