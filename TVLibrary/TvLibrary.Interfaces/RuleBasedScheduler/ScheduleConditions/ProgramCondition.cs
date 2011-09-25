using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
 using System.Linq.Expressions;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class ProgramCondition<T> : IScheduleCondition
  {
    private readonly ConditionOperator _operator; //contains the operator. eg equals.
    private readonly string _programFieldName;        // contains the fieldname
    private readonly T _programFieldValue;             // the value to match. eg "gump"

    public ProgramCondition(string programFieldName, T programFieldValue, ConditionOperator op)
    {      
      _programFieldName = programFieldName;
      _programFieldValue = programFieldValue;
      _operator = op;
    }

    public ProgramCondition()
    {
      
    }
   
    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return DynamicLinqBuilder.ApplyFilter(baseQuery, _programFieldName, _programFieldValue, _operator);      
    }

    public override string ToString()
    {
      return ("[" + _programFieldName + "] " + _operator + " [" + _programFieldValue + "]");
    }

  }


}
