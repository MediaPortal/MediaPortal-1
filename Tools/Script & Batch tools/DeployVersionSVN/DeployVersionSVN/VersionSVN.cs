using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace DeployVersionSVN
{
  public class VersionSVN
  {
    public string GetVerion(string directory)
    {
      FileInfo file = new FileInfo(Environment.GetEnvironmentVariable("ProgramFiles") + @"\TortoiseSVN\bin\SubWCRev.exe");

      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = "\"" + directory + "\"";
      procInfo.FileName = file.FullName;

      Console.WriteLine("Running : {0}", file.FullName);

      if (file.Exists)
      {
        // Start process
        Process proc = Process.Start(procInfo);

        // Get process output
        if (proc != null)
        {
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
      }

      Console.WriteLine("Not found!");
      return string.Empty;
    }
  }
}