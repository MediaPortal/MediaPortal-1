using System;

namespace Core.Util
{
	/// <summary>
	/// Summary description for ConditionChecker.
	/// </summary>
	public class ConditionChecker
	{
		public ConditionChecker()
		{
			//
			// TODO: Add constructor logic here
			//
		}

    string strProblems = "";
    string strSeparator = "";

    public void Clear()
    {
      strProblems = "";
      strSeparator = "";
    }

    public bool DoCheck(bool bSuccessCondition, string strErrorMsg)
    {
      if (!bSuccessCondition)
      {
        strProblems = strProblems + strSeparator + strErrorMsg;
        strSeparator = "\n";
      }
      return bSuccessCondition;
    }

    public bool IsOk
    {
      get
      {
        return (strProblems == "");
      }
    }

    public string Problems
    {
      get
      {
        return strProblems;
      }
    }

	}
}
