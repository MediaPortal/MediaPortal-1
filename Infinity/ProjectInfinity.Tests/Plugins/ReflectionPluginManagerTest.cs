using System.Collections.Generic;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;
using ProjectInfinity.Plugins;
using ProjectInfinity.Tests.Mocks;
using ProjectInfinity.Tests.Plugins.Mocks;
using NUnit.Framework;

namespace ProjectInfinity.Tests.Plugins
{
  [TestFixture]
  public class ReflectionPluginManagerTest
  {
    [TestFixtureSetUp]
    public void Setup()
    {
      ServiceScope.Add<IMessageBroker>(new MessageBroker());
      ServiceScope.Add<ILogger>(new NoLogger());
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      ServiceScope.Current.Dispose();
    }

    /// <summary>
    /// Tests whether the plug-in manager is able to find plugins.
    /// </summary>
    /// <remarks>At least our <see cref="DummyPlugin"/> should be found, probably more.</remarks>
    [Test]
    public void TestPluginsFound()
    {
      ReflectionPluginManager pluginMgr = new ReflectionPluginManager();
      IEnumerable<IPluginInfo> plugins = pluginMgr.GetAvailablePlugins();
      Assert.IsNotNull(plugins);
      IEnumerator<IPluginInfo> enumerator = plugins.GetEnumerator();
      Assert.IsNotNull(enumerator);
      enumerator.MoveNext();
      Assert.IsNotNull(enumerator.Current);
    }

    /// <summary>
    /// Tests whether the <see cref="IPlugin.Initialize"/> method is called correctly
    /// when a plugin is started.
    /// </summary>
    [Test]
    public void TestPluginStart()
    {
      TestReflectionPluginManager pluginMgr = new TestReflectionPluginManager();
      pluginMgr.Start("DummyPlugin");
      DummyPlugin plugin = pluginMgr["DummyPlugin"] as DummyPlugin;
      Assert.IsNotNull(plugin);
      Assert.IsTrue(plugin.IsInitialized);
    }

    /// <summary>
    /// Tests whether the <see cref="IPlugin.Dispose"/> method is called correctly
    /// when a plugin is stopped.
    /// </summary    
    /// [Test]
    public void TestPluginStop()
    {
      TestReflectionPluginManager pluginMgr = new TestReflectionPluginManager();
      pluginMgr.Start("DummyPlugin");
      DummyPlugin plugin = pluginMgr["DummyPlugin"] as DummyPlugin;
      Assert.IsNotNull(plugin);
      pluginMgr.Stop("DummyPlugin");
      Assert.IsFalse(plugin.IsInitialized);
      Assert.IsTrue(plugin.IsDisposed);
    }
  }
}