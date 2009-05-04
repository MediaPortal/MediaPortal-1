using System;
using System.Collections.Generic;
using System.Text;

namespace Windows
{
  class cmdArgs
  {
    List<string> _Names = new List<string>();
    List<string> _Values = new List<string>();

    public List<string> Name
    {
      get { return _Names; }
      set { _Names = value; }
    }

    public List<string> Values
    {
      get { return _Values; }
      set { _Values = value; }
    }

    public cmdArgs(string[] cmdArgs)
    {
      string[] stringSeparators = new string[] {"="};
      //string[] cmdArgs = Environment.GetCommandLineArgs();
      for (int i = 0; i < cmdArgs.Length; i++)
      {
        string[] tmpSplittedArg = cmdArgs[i].Split(stringSeparators, StringSplitOptions.None);
        _Names.Add(tmpSplittedArg[0]);
        _Values.Add(tmpSplittedArg[1]);
      }

    }

    public int FindArgPos(string argName)
    {
      for (int i = 0; i < _Names.Count; i++)
      {
        string tmpArgName = _Names[i];
        if (tmpArgName.ToLower() == argName.ToLower())
        {
          return i;
        }
      }
      return -1;
    }

    public bool ArgExists(string argName)
    {
      if (FindArgPos(argName) != -1) return true;
      else return false;
    }

  }
}
