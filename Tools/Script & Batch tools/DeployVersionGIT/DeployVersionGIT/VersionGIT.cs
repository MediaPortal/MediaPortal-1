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
    public const int DEFAULT_COMMITTISH_LEN = 7;
    private const string AnyReleaseTagPattern = "Release_1.*";
    private const string ReleaseTagPattern = "Release_1.*.0*";
    private const string ServiceReleaseTagPattern = "Release_1.{0}.*";
    //private const string ReleaseTagRegEx = @"^Release_(?<majver>[0-9]+)\.(?<minver>[0-9]+)\.0";
    //private const string ServiceReleaseTagRegEx = @"^Release_(?<majver>[0-9]+)\.(?<minver>[0-9]+)\.(?<revision>[0-9]+)";
    private const string ServiceReleaseBranchRegEx = @"^Release_(?<majver>[0-9]+)\.(?<minver>[0-9]+)\.[xX]";

    private const string DescribeOutputRegEx =
      @"^Release_(?<majver>[0-9]+)\.(?<minver>[0-9]+)\.(?<revision>[0-9]+)(?:[-_](?<reltype>[^0-9][^-]*))?(?:-(?<build>[0-9]+)\-g(?<committish>[0-9a-z]+))?\s*$";

    private const string ReleaseTagLogEntryRegEx = @"^[0-9a-fA-F]+\s+\((.+\s)?tag:\s(?<tag>Release_1\.\d+\.\d+[^),]*)";

    private Version _version;
    private string _releaseType;
    private string _fullVersion;
    private string _branch;
    private string _committish;
    private bool _isReleaseBranch;

    private Process RunGitCommand(string arguments)
    {
      string programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetEnvironmentVariable("ProgramFiles");

      FileInfo file = new FileInfo(programFiles + @"\Git\bin\git.exe");
      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = arguments;
      procInfo.FileName = file.FullName;

      Console.WriteLine("Running : {0} {1}", file.FullName, arguments);

      if (file.Exists)
      {
        return Process.Start(procInfo);
      }

      // GIT V2 is installed to x64 folder
      programFiles = Environment.GetEnvironmentVariable("ProgramW6432");

      file = new FileInfo(programFiles + @"\Git\bin\git.exe");
      procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = arguments;
      procInfo.FileName = file.FullName;

      Console.WriteLine("Running : {0} {1}", file.FullName, arguments);

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
          Console.WriteLine("Git dir not found");
          return "./.git";
        }
        directory = parent.FullName;
      }
      Console.WriteLine("Using git dir: {0}", directory);
      return directory + @"\.git";
    }

    public string GetCurrentBranch(string gitDir, string committish)
    {
      using (
        var proc = RunGitCommand(string.Format("--git-dir=\"{0}\" name-rev --refs=\"refs/heads/*\" --name-only {1} ", gitDir, committish)))
      {
        if (proc != null)
        {
          string gitOut = proc.StandardOutput.ReadToEnd();
          Regex regex = new Regex(@"^(?<branch>[^^~ \t\n\r]+)", RegexOptions.Multiline);
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
      return GitDescribe(gitDir, pattern, true);
    }

    public string GitDescribe(string gitDir, string pattern, bool firstParent)
    {
      if (firstParent)
      {
        // try to find the latest tag using only first-parent links that matches pattern
        using (
          var proc = RunGitCommand(String.Format("--git-dir=\"{0}\" --no-pager log  --oneline --decorate=short --first-parent", gitDir)))
        {
          if (proc != null)
          {
            var output = proc.StandardOutput;
            var logRegEx = new Regex(ReleaseTagLogEntryRegEx);
            var patternRegEx = new Regex("^" + Regex.Escape(pattern).Replace("\\?", ".?").Replace("\\*", ".*") + "$");
            while (!output.EndOfStream)
            {
              var line = output.ReadLine();
              var match = logRegEx.Match(line);
              if (match.Success)
              {
                var tag = match.Groups["tag"].Value;
                if (patternRegEx.IsMatch(tag))
                {
                  pattern = tag;
                  break;
                }
              }
            }
            //proc.WaitForExit();
          }
        }
      }
      using (
        var proc = RunGitCommand(String.Format("--git-dir=\"{0}\" --no-pager describe --tags --match \"{1}\" --abbrev={2}", gitDir, pattern, DEFAULT_COMMITTISH_LEN)))
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
      string pattern = AnyReleaseTagPattern;

      Regex regEx;
      Match match;

      _branch = GetCurrentBranch(gitDir);
      if (_branch.Equals("master", StringComparison.InvariantCultureIgnoreCase))
      {
        // on master branch so only consider normal releases (1.x.0[alpha/beta/rc]) not service releases (1.x.1, 1.x.2 etc)
        pattern = ReleaseTagPattern;
        _isReleaseBranch = true;
      }
      else
      {
        regEx = new Regex(ServiceReleaseBranchRegEx);
        match = regEx.Match(_branch);
        if (match.Success)
        {
          // on a service release branch so only consider service releases on the same branch
          pattern = string.Format(ServiceReleaseTagPattern, match.Groups["minver"].Value);
          _isReleaseBranch = true;
        }
        // Otherwise we are on a feature branch, use default pattern (any release)
      }

      regEx = new Regex(DescribeOutputRegEx, RegexOptions.Multiline);
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
        build = (String.IsNullOrEmpty(build)) ? "0" : build;
        var minver = int.Parse(match.Groups["minver"].Value);
        var revision = int.Parse(match.Groups["revision"].Value);
        _releaseType = match.Groups["reltype"].Value;
        _version = new Version(1, minver, int.Parse(build), revision);
        _committish = match.Groups["committish"].Value;
        _fullVersion = gitOut.Trim(' ', '\n', '\r', '\t').Replace("Release_", "");
      }
      else
      {
        Console.WriteLine("Unable to determine GIT version.");
        return false;
      }

      if (String.IsNullOrEmpty(_branch))
      {
        Console.WriteLine("Unable to determine GIT branch.");
      }
      else
      {
        _fullVersion = _fullVersion + (_isReleaseBranch? "":"-" + _branch);
      }
      return true;
    }

    public Version GetVersion()
    {
      return _version;
    }

    public string GetBuild()
    {
      return _version.Build.ToString();
    }

    public string GetCommittish()
    {
      return _committish;
    }

    public string GetFullVersion()
    {
      return _fullVersion;
    }

    public string GetBranch()
    {
      return _branch;
    }

    public bool IsReleaseBranch()
    {
      return _isReleaseBranch;
    }
  }

}