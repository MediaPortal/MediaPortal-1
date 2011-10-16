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
using System.IO;
using MediaPortal.Utilities.CommandLine;

namespace DeployVersionGIT
{
  internal class Program
  {
    private static void Main(string[] args)
    {

      string version;
      int versionInt;
      string fullVersion;

      ICommandLineOptions argsOptions = new CommandLineOptions();

      try
      {
        CommandLine.Parse(args, ref argsOptions);
      }
      catch (ArgumentException)
      {
        argsOptions.DisplayOptions();
        Environment.Exit(0);
      }

      CommandLineOptions options = argsOptions as CommandLineOptions;

      if (!options.IsOption(CommandLineOptions.Option.path))
      {
        argsOptions.DisplayOptions();
        Environment.Exit(0);
      }
      string directory = options.GetOption(CommandLineOptions.Option.path);

      if (options.IsOption(CommandLineOptions.Option.UpdateCopyright))
      {
        string copyrightText = options.GetOption(CommandLineOptions.Option.UpdateCopyright);
        Console.WriteLine("Writing Copyright text: " + copyrightText);

        AssemblyUpdate copyrightUpdate = new AssemblyUpdate(copyrightText, AssemblyUpdate.UpdateMode.Copyright);
        copyrightUpdate.UpdateAll(directory);

        Environment.Exit(0);
      }

      if (options.IsOption(CommandLineOptions.Option.GetVersion))
      {
        string gitDir = options.GetOption(CommandLineOptions.Option.git);
        if (string.IsNullOrEmpty(gitDir))
        {
          gitDir = directory;
        }
        VersionGIT git = new VersionGIT();
        bool versionExists = git.ReadVersion(gitDir);

        if (File.Exists("version.txt"))
        {
          File.Delete("version.txt");
        }
        if (!versionExists)
        {
          Console.WriteLine("Local GIT not up to date");
          Environment.Exit(0);
        }

        version = git.GetVersion();
        fullVersion = git.GetFullVersion();

        TextWriter write = new StreamWriter("version.txt");
        write.Write(fullVersion);
        write.Close();
        Int32.TryParse(version, out versionInt);
        Environment.Exit(versionInt);
      }

      if (options.IsOption(CommandLineOptions.Option.revert))
      {
        version = "0";
        fullVersion = "";

        Console.WriteLine("Reverting to build 0");
      }
      else
      {
        string gitDir = options.GetOption(CommandLineOptions.Option.git);
        if (string.IsNullOrEmpty(gitDir))
        {
          gitDir = directory;
        }
        VersionGIT git = new VersionGIT();
        bool versionExists = git.ReadVersion(gitDir);

        if (!versionExists)
        {
          Console.WriteLine("Local GIT not up to date");
          Environment.Exit(0);
        }

        version = git.GetVersion();
        fullVersion = git.GetFullVersion();

        Console.WriteLine("GIT Version: " + fullVersion);
      }

      AssemblyUpdate update = new AssemblyUpdate(version, fullVersion);
      update.UpdateAll(directory);
    }
  }
}