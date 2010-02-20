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
using System.IO;
using MediaPortal.Utilities.CommandLine;

namespace DeployVersionSVN
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      string version;
      int versionInt;

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

      if (!options.IsOption(CommandLineOptions.Option.svn))
      {
        argsOptions.DisplayOptions();
        Environment.Exit(0);
      }
      string directory = options.GetOption(CommandLineOptions.Option.svn);

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
        VersionSVN svn = new VersionSVN();
        version = svn.GetVersion(directory);

        if (File.Exists("version.txt"))
        {
          File.Delete("version.txt");
        }
        if (version == string.Empty)
        {
          Console.WriteLine("Local SVN not up to date");
          Environment.Exit(0);
        }

        TextWriter write = new StreamWriter("version.txt");
        write.Write(version);
        write.Close();
        Int32.TryParse(version, out versionInt);
        Environment.Exit(versionInt);
      }

      if (options.IsOption(CommandLineOptions.Option.revert))
      {
        version = "0";

        Console.WriteLine("Reverting to build 0");
      }
      else
      {
        VersionSVN svn = new VersionSVN();
        version = svn.GetVersion(directory);

        if (version == string.Empty)
        {
          Console.WriteLine("Local SVN not up to date");
          Environment.Exit(0);
        }

        Console.WriteLine("SVN Version: " + version);
      }

      AssemblyUpdate update = new AssemblyUpdate(version);
      update.UpdateAll(directory);
    }
  }
}