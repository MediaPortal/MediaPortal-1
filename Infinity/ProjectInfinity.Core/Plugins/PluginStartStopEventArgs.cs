using ProjectInfinity.Messaging;

namespace ProjectInfinity.Plugins
{
  public class PluginStarted : Message
  {
    private readonly string _pluginName;

    public PluginStarted(string pluginName)
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


  public class PluginStopped : Message
  {
    private readonly string _pluginName;

    public PluginStopped(string pluginName)
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