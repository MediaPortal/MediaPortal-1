using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for Error.
	/// </summary>
	public class Error
	{
    static string errorReason=String.Empty;
    static string errorDescription=String.Empty;
		
    static public string Description
    {
      get { return errorDescription;}
      set 
      { 
        if (value==null) return;
        errorDescription=value;
      }
    }
    
    static public string Reason
    {
      get { return errorReason;}
      set 
      { 
        if (value==null) return;
        errorReason=value;
      }
    }
    static public int ReasonId
    {
      set 
      { 
        Reason=GUILocalizeStrings.Get(value);
      }
    }
    static public int DescriptionId
    {
      set 
      { 
        Description=GUILocalizeStrings.Get(value);
      }
    }

    static public void SetError(string reason, string description)
    {
      Reason=reason;
      Description=description;
    }
    
    static public void SetError(int reasonId, int descriptionId)
    {
      ReasonId=reasonId;
      DescriptionId=descriptionId;
    }

    static public void Clear()
    {
      Reason=String.Empty;
      Description=String.Empty;
    }
	}
}
