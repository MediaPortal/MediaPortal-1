using ProjectInfinity.Plugins;

namespace ProjectInfinity.Tests.Plugins.Mocks
{
  internal class TestReflectionPluginManager : ReflectionPluginManager
  {
    public IPlugin this[string _name]
    {
      get { return runningPlugins[_name]; }
    }
  }
}