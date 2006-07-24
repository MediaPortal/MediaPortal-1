using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MediaPortal.Support
{
  public class ProcessRunner
  {
    protected string arguments = string.Empty;
    protected string executable = null;
    protected int lastExitCode = -1;


    public string Arguments
    {
      get { return arguments; }
      set { arguments = value; }
    }

    public string Executable
    {
      get { return executable; }
      set { executable = value; }
    }

    public int LastExitCode
    {
      get { return lastExitCode; }
    }

    public ProcessRunner()
    {
    }

    public virtual void Run()
    {
      if (executable == null)
        throw new ArgumentNullException("executable");
      
      Process pr = new Process();
      pr.StartInfo.FileName = executable;
      pr.StartInfo.Arguments = arguments;
      pr.StartInfo.CreateNoWindow = true;
      pr.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
      pr.Start();
      pr.WaitForExit();
      lastExitCode = pr.ExitCode;
    }
  }
}
