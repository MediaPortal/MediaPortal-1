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

      if (!options.IsOption(CommandLineOptions.Option.dir)
        || !options.IsOption(CommandLineOptions.Option.output)
        )
      {
        argsOptions.DisplayOptions();
        Environment.Exit(0);
      }
      string directory = options.GetOption(CommandLineOptions.Option.dir);
      string output = options.GetOption(CommandLineOptions.Option.output);

      string ignore = string.Empty;
      if (options.IsOption(CommandLineOptions.Option.ignore))
      {
        TextReader reader = new StreamReader(options.GetOption(CommandLineOptions.Option.ignore));
        ignore = reader.ReadToEnd();
      }


      FileLister lister = new FileLister(directory, ignore);
      lister.UpdateAll();

      if (File.Exists(output))
      {
        File.Delete(output);
      }

      TextWriter write = new StreamWriter(output);
      write.Write(lister.FileList);
      write.Close();
    }
  }
}
