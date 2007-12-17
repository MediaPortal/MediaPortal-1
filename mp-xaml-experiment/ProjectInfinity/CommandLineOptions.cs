using System;
using System.Collections.Generic;
using ProjectInfinity.Utilities.CommandLine;

namespace ProjectInfinity
{
  public class CommandLineOptions : ICommandLineOptions
  {
    #region Enums
    public enum Option
    {
      Wizard
    }
    #endregion

    #region Variables
    private Dictionary<Option, string> _options;
    #endregion

    #region Constructors/Destructors
    public CommandLineOptions()
    {
      _options = new Dictionary<Option, string>();
    }
    #endregion

    #region Public Methods
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
    #endregion

    #region ICommandLineOptiosn Implementations
    public void SetOption(string option, string argument)
    {
      _options.Add((Option)Enum.Parse(typeof(Option), option, true), argument);
    }

    public void DisplayOptions()
    {
      Console.WriteLine("Vaild Command Line options:");
      Console.WriteLine("/wizard    Start configuration wizard");
    }
    #endregion
  }
}