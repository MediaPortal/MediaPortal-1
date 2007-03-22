using System;
using System.Windows;
using System.Data;
using System.Xml;
using System.Configuration;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;
using ProjectInfinity.Plugins;
using ProjectInfinity.Themes;
using ProjectInfinity.Utilities.CommandLine;
using ProjectInfinity.Windows;

namespace ProjectInfinity
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>

  public partial class App : System.Windows.Application
  {
    private ILogger logger = null;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      logger = new FileLogger("ProjectInfinity.log", LogLevel.Debug);
      ServiceScope.Add(logger);
      logger.Critical("ProjectInfinity is starting...");
      //register service implementations
      ServiceScope.Add<IMessageBroker>(new MessageBroker()); //Our messagebroker
      ServiceScope.Add<IPluginManager>(new ReflectionPluginManager());
      ServiceScope.Add<IThemeManager>(new ThemeManager());
      //A pluginmanager that uses reflection to enumerate available plugins

      ICommandLineOptions piArgs = new ProjectInfinityCommandLine();

      try
      {
        CommandLine.Parse(e.Args, ref piArgs);
      }
      catch (ArgumentException)
      {
        piArgs.DisplayOptions();
        return;
      }
      //Start the plugins
      ServiceScope.Get<IPluginManager>().StartAll();
ServiceScope.Get<IThemeManager>().SetDefaultTheme();
    }

    protected override void OnExit(ExitEventArgs e)
    {
      //When we return here, the core has quit
      logger.Critical("ProjectInfinity has stopped...");
    }

  }
}