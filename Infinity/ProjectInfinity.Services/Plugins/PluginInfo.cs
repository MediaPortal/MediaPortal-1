namespace ProjectInfinity.Plugins
{
  public sealed class PluginInfo : IPluginInfo
  {
    private string _name;
    private string _description;
    private bool _autoStart = false;
    private bool _listInMenu = false;
    private string _imagePath;

    public PluginInfo(string name, string description)
    {
      _name = name;
      _description = description;
    }

    internal PluginInfo(PluginAttribute attribute)
    {
      _name = attribute.Name;
      _description = attribute.Description;
      _autoStart = attribute.AutoStart;
      _listInMenu = attribute.ListInMenu;
      _imagePath = attribute.ImagePath;
    }

    public string Name
    {
      get { return _name; }
    }

    public string Description
    {
      get { return _description; }
    }

    #region IPluginInfo Members

    public bool AutoStart
    {
      get { return _autoStart; }
    }

    public bool ListInMenu
    {
      get { return _listInMenu; }
    }

    public string ImagePath
    {
      get { return _imagePath; }
    }

    #endregion
  }
}