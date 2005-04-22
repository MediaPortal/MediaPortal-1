namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for ProgramConditionChecker.
	/// </summary>
	public class ProgramConditionChecker
	{
		string strProblems = "";
		string strSeparator = "";


		public ProgramConditionChecker()
		{
		}

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
			get{ return (strProblems == "");}
		}

		public string Problems
		{
			get{ return strProblems; }
		}

	}
}
