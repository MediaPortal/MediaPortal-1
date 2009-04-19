using System;
using System.Collections.Generic;
using MediaPortal.Utilities.CommandLine;

namespace UninstallFilelist
{
  public class CommandLineOptions : ICommandLineOptions
  {
    public enum Option
    {
      dir,
      ignore,
      output,
    }

    private readonly Dictionary<Option, string> _options;

    public CommandLineOptions()
    {
      _options = new Dictionary<Option, string>();
    }

    public void SetOption(string option, string argument)
    {
      _options.Add((Option) Enum.Parse(typeof (Option), option, true), argument);
    }

    public void DisplayOptions()
    {
      Console.WriteLine("Vaid Command Line options:");
      Console.WriteLine("/svn=<directory>  svn directory");
      Console.WriteLine("/revert           revert to build 0");
      Console.WriteLine("/GetVersion       writes the svn revision in textfile version.txt");
    }

    public bool IsOption(Option option)
    {
      return _options.ContainsKey(option);
    }

    public int Count
    {
      get { return _options.Count; }
    }

    public string GetOption(Option option)
    {
      return _options[option];
    }
  }
}