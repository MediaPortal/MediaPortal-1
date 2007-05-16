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
using ProjectInfinity.Localisation;
using ProjectInfinity.Players;
using ProjectInfinity.Settings;
using ProjectInfinity.TaskBar;
using ProjectInfinity.Thumbnails;
using MediaLibrary;

namespace ProjectInfinity
{
  public class Launcher
  {
    [STAThread]
    public static void Main(params string[] args)
    {
      using (new ServiceScope(true))
      {
        ILogger logger = new FileLogger("ProjectInfinity.log", LogLevel.Debug);
        ServiceScope.Add(logger);
        logger.Critical("ProjectInfinity is starting...");
        //register service implementations
        ServiceScope.Add<IMessageBroker>(new MessageBroker()); //Our messagebroker
        //A pluginmanager that uses reflection to enumerate available plugins
        //ServiceScope.Add<IPluginManager>(new ReflectionPluginManager());
        // A plugin manager that uses a plugin tree
        ServiceScope.Add<IPluginManager>(new PluginManager());
        ServiceScope.Add<IThemeManager>(new ThemeManager());
        ServiceScope.Add<INavigationService>(new NavigationService());
        ServiceScope.Add<IPlayerCollectionService>(new PlayerCollectionService());
        ServiceScope.Add<ILocalisation>(new StringManager("Language", "en"));
        ServiceScope.Add<ISettingsManager>(new SettingsManager());
        ServiceScope.Add<IWindowsTaskBar>(new WindowsTaskBar());
        ServiceScope.Add<IMediaLibrary>(new MediaLibraryClass());
        ServiceScope.Add<IThumbnailBuilder>(new ThumbnailBuilder());

        // Parse Command Line options
        ICommandLineOptions piArgs = new CommandLineOptions();

        try
        {
          CommandLine.Parse(args, ref piArgs);
        }
        catch (ArgumentException)
        {
          piArgs.DisplayOptions();
          return;
        }

        // Start Core
        Core.Start();
      }
    }
  }
}
