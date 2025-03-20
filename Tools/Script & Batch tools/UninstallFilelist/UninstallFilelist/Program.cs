#region Copyright (C) 2005-2025 Team MediaPortal

// Copyright (C) 2005-2025 Team MediaPortal
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

namespace UninstallFilelist
{
  internal class Program
  {
    private static void Main(string[] args)
    {
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

      if (!options.IsOption(CommandLineOptions.Option.dir) || !options.IsOption(CommandLineOptions.Option.output))
      {
        argsOptions.DisplayOptions();
        Environment.Exit(0);
      }
      string directory = options.GetOption(CommandLineOptions.Option.dir);
      string output = options.GetOption(CommandLineOptions.Option.output);

      string ignore = string.Empty;
      if (options.IsOption(CommandLineOptions.Option.ignore))
      {
        using (TextReader reader = new StreamReader(options.GetOption(CommandLineOptions.Option.ignore)))
        {
          ignore = reader.ReadToEnd();
        }
      }

      FileLister lister = new FileLister(directory, ignore);
      lister.UpdateAll();

      if (File.Exists(output))
      {
        File.Delete(output);
      }

      using (TextWriter write = new StreamWriter(output, false, System.Text.Encoding.UTF8))
      {
        write.Write(lister.FileList);
        write.Close();
      }
    }
  }
}