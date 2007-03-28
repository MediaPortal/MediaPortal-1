namespace ProjectInfinity.Plugins
{
  public interface IPluginInfo
  {
    string Name { get; }
    string Description { get; }
    bool AutoStart { get; }
    bool ListInMenu { get;}

    //TODO: add Icon and other properties
  }
}