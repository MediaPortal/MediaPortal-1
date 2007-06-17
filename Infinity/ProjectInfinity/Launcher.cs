using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Logging;
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

        //register core service implementations
        logger.Info("Registering Message Broker");
        ServiceScope.Add<IMessageBroker>(new MessageBroker()); //Our messagebroker

        logger.Info("Registering Plugin Manager");
        ServiceScope.Add<IPluginManager>(new PluginManager());

        logger.Info("Registering Settings Manager");
        ServiceScope.Add<ISettingsManager>(new SettingsManager());

        //Start Localisation
        ServiceScope.Add<ILocalisation>(new StringManager());

        ServiceScope.Add<IThemeManager>(new ThemeManager());
        ServiceScope.Add<INavigationService>(new NavigationService());
        ServiceScope.Add<IPlayerCollectionService>(new PlayerCollectionService());
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
