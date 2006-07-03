using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MediaPortal.Support
{
  public class DxDiagLog : ILogCreator
  {
    private ProcessRunner runner;

    public DxDiagLog(ProcessRunner runner)
    {
      this.runner = runner;
    }

    public void CreateLogs(string destinationFolder)
    {
      string tmpFile = Environment.GetEnvironmentVariable("SystemDrive") + "\\_dxdiag.txt";
      string dstFile = destinationFolder + "\\dxdiag.txt";
      CreateDxDiagFile(tmpFile);
      if (File.Exists(dstFile)) 
        File.Delete(dstFile);
      File.Move(tmpFile, dstFile);
    }

    private void CreateDxDiagFile(string tmpFile)
    {
      string executable = Environment.GetEnvironmentVariable("windir") + @"\system32\dxdiag.exe";
      string arguments = "/whql:off /t " + tmpFile;
      runner.Arguments = arguments;
      runner.Executable = executable;
      runner.Run();
    }

    public string ActionMessage
    {
      get { return "Gathering DirectX diagnostics text file..."; }
    }
  }
}
