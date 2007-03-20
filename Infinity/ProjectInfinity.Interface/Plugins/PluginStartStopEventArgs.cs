using System;

namespace ProjectInfinity.Plugins
{
  public class PluginStartStopEventArgs : EventArgs
  {
    private string _pluginName;

    public PluginStartStopEventArgs(string pluginName)
    {
      _pluginName = pluginName;
    }

    public string PluginName
    {
      get { return _pluginName; }
    }

    public override string ToString()
    {
      return string.Format("PluginName={0}", _pluginName);
    }
  }
}