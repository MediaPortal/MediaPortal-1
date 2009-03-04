using System;
using System.IO;
using ProjectInfinity;
using ProjectInfinity.Utilities.CommandLine;

namespace DeployVersionSVN
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
        return;
      }

      CommandLineOptions options = argsOptions as CommandLineOptions;

      if (!options.IsOption(CommandLineOptions.Option.svn))
      {
        argsOptions.DisplayOptions();
        return;
      }
      string directory = options.GetOption(CommandLineOptions.Option.svn);

      string version;
      if (options.IsOption(CommandLineOptions.Option.GetVersion))
      {
        VersionSVN svn = new VersionSVN();
        version = svn.GetVerion(directory);

        if (File.Exists("version.txt"))
        {
          File.Delete("version.txt");
        }
        if (version == string.Empty)
        {
          Console.WriteLine("Local SVN not up to date");
          return;
        }

        TextWriter write = new StreamWriter("version.txt");
        write.Write(version);
        write.Close();
        return;
      }

      if (options.IsOption(CommandLineOptions.Option.revert))
      {
        version = "0";

        Console.WriteLine("Reverting to build 0");
      }
      else
      {
        VersionSVN svn = new VersionSVN();
        version = svn.GetVerion(directory);

        if (version == string.Empty)
        {
          Console.WriteLine("Local SVN not up to date");
          return;
        }

        Console.WriteLine("SVN Version: " + version);
      }

      AssemblyUpdate update = new AssemblyUpdate(version);
      update.UpdateAll(directory);
    }
  }
}