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
        version = svn.GetVerion(directory);

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
