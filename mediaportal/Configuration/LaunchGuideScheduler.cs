using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.Configuration
{
  class SetupGrabber
  {

    public static bool LaunchGuideScheduler()
    {	//start an TVGuideScheduler process
      string appath = Application.StartupPath + "\\TVGuideScheduler.exe";
      string WorkingDir=Application.StartupPath;
      try
      {
        Process runGuideScheduler = new Process();
        runGuideScheduler.StartInfo.FileName = appath;
        runGuideScheduler.StartInfo.UseShellExecute = false;
        runGuideScheduler.StartInfo.WorkingDirectory = WorkingDir;
        runGuideScheduler.Start();
					
        return true;
      }
      catch(Exception e)
      {
        Console.WriteLine("The following exception was raised: ");
        Console.WriteLine(e.Message);
        return false;
      }
    }
  }
}
