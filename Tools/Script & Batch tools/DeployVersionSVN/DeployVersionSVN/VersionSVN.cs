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
      FileInfo file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\TortoiseSVN\bin\SubWCRev.exe");

      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = "\"" + directory + "\"";
      procInfo.FileName = file.FullName;

      Console.WriteLine("Running : {0}", file.FullName);

      if (file.Exists)
      {

        // Start process
        Process proc;
        proc = Process.Start(procInfo);

        // Get process output
        string svn = proc.StandardOutput.ReadToEnd();

        Regex tortoiseRegex = new Regex("Update.+ (?<version>[0-9]+)");

        string ver = tortoiseRegex.Match(svn).Groups["version"].Value;
        if (String.IsNullOrEmpty(ver))
        {
          Console.WriteLine("Unable to determine SVN version. Try with a SVN cleanup!");
          return string.Empty;
        }
        return ver;
      }

      Console.WriteLine("Not found!");
      return string.Empty;
    }
  }
}
