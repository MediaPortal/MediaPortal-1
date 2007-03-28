using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Logging;
using ProjectInfinity.Menu;
using ProjectInfinity.Messaging;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.Themes;
using ProjectInfinity.Utilities.CommandLine;
using ProjectInfinity.Windows;

namespace ProjectInfinity
{
  public class Launcher
  {
    [STAThread]
    public static void Main(params string[] args)
    {
      ILogger logger = new FileLogger("ProjectInfinity.log", LogLevel.Debug);
      ServiceScope.Add(logger);
      logger.Critical("ProjectInfinity is starting...");
      //register service implementations
      ServiceScope.Add<IMessageBroker>(new MessageBroker()); //Our messagebroker
      ServiceScope.Add<IPluginManager>(new ReflectionPluginManager());
      ServiceScope.Add<IThemeManager>(new ThemeManager());
      ServiceScope.Add<IMenuManager>(new MenuManager());
      ServiceScope.Add<INavigationService>(new NavigationService());
      //A pluginmanager that uses reflection to enumerate available plugins

      ICommandLineOptions piArgs = new ProjectInfinityCommandLine();

      try
      {
        CommandLine.Parse(args, ref piArgs);
      }
      catch (ArgumentException)
      {
        piArgs.DisplayOptions();
        return;
      }
      ProjectInfinityCore.Start();

    }
  }
}
