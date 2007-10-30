using System;
using System.Collections.Generic;
using ProjectInfinity.Utilities.CommandLine;

namespace ProjectInfinity
{
  public class CommandLineOptions : ICommandLineOptions
  {
    public enum Option
    {
      svn,
      revert
    }

    private Dictionary<Option, string> _options;

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