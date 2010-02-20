#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace DeployVersionSVN
{
  public class VersionSVN
  {
    public string GetVersion(string directory)
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

          Regex tortoiseRegex = new Regex(@"Updated to revision\s?(?<version>[0-9]+)");

          string ver = tortoiseRegex.Match(svn).Groups["version"].Value;
          if (String.IsNullOrEmpty(ver))
          {
            Console.WriteLine("Unable to determine SVN version. Try with a SVN cleanup!");
            return string.Empty;
          }
          return ver;
        }
      }

      Console.WriteLine("SubWCRev.exe not found!");
      return string.Empty;
    }
  }
}