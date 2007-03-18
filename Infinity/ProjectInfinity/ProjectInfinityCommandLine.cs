using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Utilities.CommandLine;

namespace ProjectInfinity
{
  public class ProjectInfinityCommandLine : ICommandLineOptions
  {
    public enum Option
    {
      Config,
      Wizard
    }

    private Dictionary<Option, string> _options;

    public ProjectInfinityCommandLine()
    {
      _options = new Dictionary<Option, string>();
    }

    public void SetOption(string option, string argument)
    {
      _options.Add((Option) Enum.Parse(typeof(Option), option, true), argument);
    }

    public void DisplayOptions()
    {
      Console.WriteLine("Vaid Command Line options:");
      Console.WriteLine("/config    Start normal configuration");
      Console.WriteLine("/wizard    Start configuration wizard");
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
