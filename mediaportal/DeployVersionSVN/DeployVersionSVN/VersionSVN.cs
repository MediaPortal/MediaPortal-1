using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DeployVersionSVN
{
  public class VersionSVN
  {
     // bool _tortoise = true;

    public VersionSVN()
    {
    }

    public string GetVerion(string directory)
    {
      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = directory;
      procInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\TortoiseSVN\bin\SubWCRev.exe";

      Console.WriteLine("Running : {0}", procInfo.FileName);

      // Start process
      Process proc;
      proc = Process.Start(procInfo);

      // Get process output
      string svn = proc.StandardOutput.ReadToEnd();

      Regex tortoiseRegex = new Regex("Update.+ (?<version>[0-9]+)");

      return tortoiseRegex.Match(svn).Groups["version"].Value;
    }
  }
}
