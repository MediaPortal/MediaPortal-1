using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MediaPortal.Support
{
  public class PsLogEventsLogger : ILogCreator
  {
    private ProcessRunner runner;
    private string[] logNames = { "Application", "System" };

    public PsLogEventsLogger(ProcessRunner runner)
    {
      this.runner = runner;
    }

    public void CreateLogs(string destinationFolder)
    {
      destinationFolder = Path.GetFullPath(destinationFolder) + "\\";
      runner.Executable = Path.GetFullPath("psloglist.exe");
      foreach (string logName in logNames)
      {
        runner.Arguments = "-g \"" + destinationFolder + logName + ".evt\" " + logName;
        runner.Run();
      }
    }

    public string ActionMessage
    {
      get
      {
        return "Gathering generated eventlogs...";
      }
    }
  }
}
