using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MediaPortal.Utils.CommandLine;

namespace MediaPortal.EPG.WebEPGxmltv
{
  public class CommandLineOptions : ICommandLineOptions
  {
    #region Enums
    public enum Option
    {
      webepg,
      xmltv,
      config
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
      MessageBox.Show("Vaild Command Line options:\n/webepg=[path]  Path to webepg directory\n/xmltv=[path]   Path to xmltv directory", "Command Line Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    #endregion
  }
}