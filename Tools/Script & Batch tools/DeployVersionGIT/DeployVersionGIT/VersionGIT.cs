#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace DeployVersionGIT
{
  public class VersionGIT
  {
    private string _version;
    private string _fullVersion;

    public bool ReadVersion(string directory)
    {
      FileInfo file = new FileInfo(Environment.GetEnvironmentVariable("ProgramFiles") + @"\Git\bin\git.exe");

      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = String.Format("--git-dir=\"{0}\\.git\" --no-pager describe --match Release_1.*", directory);
      procInfo.FileName = file.FullName;

      Console.WriteLine("Running : {0}", file.FullName);

      if (file.Exists)
      {
        // Start process
        Process proc = Process.Start(procInfo);

        // Get process output
        if (proc != null)
        {
          string git = proc.StandardOutput.ReadToEnd();

          Regex tortoiseRegex = new Regex(@"^.*\-(?<version>[0-9]+)\-[0-9a-z]{8}\s*$", RegexOptions.Multiline);

          string ver = tortoiseRegex.Match(git).Groups["version"].Value;
          if (String.IsNullOrEmpty(ver))
          {
            Console.WriteLine("Unable to determine GIT version.");
            return false;
          }
          _version = ver;
          _fullVersion = git.Trim(' ','\n','\r','\t');
        }
        else
        {
          return false;
        }

        // Get branchname
        procInfo.Arguments = String.Format("--git-dir=\"{0}\\.git\" --no-pager branch --no-color", directory);
        proc = Process.Start(procInfo);
        if (proc != null)
        {
          string git = proc.StandardOutput.ReadToEnd();

          Regex tortoiseRegex = new Regex(@"^[*]\s*(?<branch>.*)\s*$", RegexOptions.Multiline);

          string branch = tortoiseRegex.Match(git).Groups["branch"].Value.Trim(' ', '\n', '\r', '\t');
          if (String.IsNullOrEmpty(branch))
          {
            Console.WriteLine("Unable to determine GIT branch.");
          }
          else
          {
            _fullVersion = _fullVersion + " (" + branch + " branch)";
          }
        }
        return true;
      }

      Console.WriteLine("git.exe not found!");
      return false;
    }

    public string GetVersion()
    {
      return _version;
    }

    public string GetFullVersion()
    {
      return _fullVersion;
    }
  }
}