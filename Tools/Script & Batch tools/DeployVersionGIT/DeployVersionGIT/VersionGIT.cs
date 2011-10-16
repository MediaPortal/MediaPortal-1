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
    private const string AnyReleaseTagPattern = "Release_1.*";
    private const string ReleaseTagPattern = "Release_1.*.0*";
    private const string ServiceReleaseTagPattern = "Release_1.{0}.*";
    //private const string ReleaseTagRegEx = @"^Release_1\.(?<minver>[0-9]+)\.0";
    //private const string ServiceReleaseTagRegEx = @"^Release_1\.(?<minver>[0-9]+)\.(?<revision>[0-9]+)";
    private const string ServiceReleaseBranchRegEx = @"^Release_1\.(?<minver>[0-9]+)\.[xX]";

    private const string DescribeOutputRegEx =
      @"^Release_1\.(?<minver>[0-9]+)\.(?<revision>[0-9]+)(-(?<build>[0-9]+)\-[0-9a-z]{8}\s*$";

    private string _version;
    private string _fullVersion;

    private Process RunGitCommand(string arguments)
    {
      FileInfo file = new FileInfo(Environment.GetEnvironmentVariable("ProgramFiles") + @"\Git\bin\git.exe");

      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = arguments;
      procInfo.FileName = file.FullName;

      Console.WriteLine("Running : {0}", file.FullName);

      if (file.Exists)
      {
        return Process.Start(procInfo);
      }

      Console.WriteLine("git.exe not found!");
      return null;
    }

    private string GetGitDir(string directory)
    {
      while (!Directory.Exists(directory + @"\.git"))
      {
        var parent = Directory.GetParent(directory);
        if (parent == null)
        {
          return ".git";
        }
        directory = parent.FullName;
      }
      return directory + @"\.git";
    }

    public string GetCurrentBranch(string gitDir, string committish)
    {
      using (
        var proc = RunGitCommand(string.Format("--git-dir=\"{0}\" --no-pager symbolic-ref {1} --no-color", gitDir, committish)))
      {
        if (proc != null)
        {
          string gitOut = proc.StandardOutput.ReadToEnd();
          Regex regex = new Regex(@"^refs/heads/(?<branch>.+)", RegexOptions.Multiline);
          return regex.Match(gitOut).Groups["branch"].Value.Trim(' ', '\n', '\r', '\t');
        }
      }
      return null;
    }

    public string GetCurrentBranch(string gitDir)
    {
      return GetCurrentBranch(gitDir, "HEAD");
    }

    public string GitDescribe(string gitDir, string pattern)
    {
      using (
        var proc = RunGitCommand(String.Format("--git-dir=\"{0}\" --no-pager describe --tags --match {1}", gitDir, pattern)))
      {
        if (proc != null)
        {
          return proc.StandardOutput.ReadToEnd();
        }
      }
      return null;
    }

    public bool ReadVersion(string directory)
    {
      string gitDir = GetGitDir(directory);
      string branch = GetCurrentBranch(gitDir);
      string pattern = AnyReleaseTagPattern;

      Regex regEx;
      Match match;

      if (branch.Equals("master", StringComparison.InvariantCultureIgnoreCase))
      {
        // on master branch so only consider normal releases (1.x.0[alpha/beta/rc]) not service releases (1.x.1, 1.x.2 etc)
        pattern = ReleaseTagPattern;
      }
      else
      {
        regEx = new Regex(ServiceReleaseBranchRegEx);
        match = regEx.Match(branch);
        if (match.Success)
        {
          // on a service release branch so only consider service releases on the same branch
          pattern = string.Format(ServiceReleaseTagPattern, match.Groups["minver"].Value);
        }
        // Otherwise we are on a feature branch, use default pattern (any release)
      }

      regEx = new Regex(DescribeOutputRegEx);
      string gitOut = GitDescribe(gitDir, pattern);
      match = regEx.Match(gitOut);
      if (!match.Success && pattern != AnyReleaseTagPattern)
      {
        pattern = AnyReleaseTagPattern;
        gitOut = GitDescribe(gitDir, pattern);
        match = regEx.Match(gitOut);
      }
      if (match.Success)
      {
        string build = match.Groups["build"].Value;
        _version = (String.IsNullOrEmpty(build)) ? "0" : build;
        _fullVersion = gitOut.Trim(' ', '\n', '\r', '\t');
      }
      else
      {
        Console.WriteLine("Unable to determine GIT version.");
        return false;
      }

      if (String.IsNullOrEmpty(branch))
      {
        Console.WriteLine("Unable to determine GIT branch.");
      }
      else
      {
        _fullVersion = _fullVersion + "-" + branch;
      }
      return true;
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