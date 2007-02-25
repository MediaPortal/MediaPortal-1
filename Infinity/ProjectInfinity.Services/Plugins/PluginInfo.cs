namespace ProjectInfinity.Plugins
{
  public sealed class PluginInfo : IPluginInfo
  {
    private string _name;
    private string _description;

    internal PluginInfo(PluginAttribute attribute)
    {
      _name = attribute.Name;
      _description = attribute.Description;
    }

    public string Name
    {
      get { return _name; }
    }

    public string Description
    {
      get { return _description; }
    }
  }
}