using System;
using System.Threading;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;
using ProjectInfinity.Plugins;
using ProjectInfinity.Windows;
using ProjectInfinity.Utilities.CommandLine;

namespace ProjectInfinity
{
  public class ProjectInfinity
  {
    // Entry point method
    [STAThread]
    public static void Main(string[] args)
    {
      Thread.CurrentThread.Name = "ProjectInfinity";
      //Start new servicecontext
      using (new ServiceScope())
      {
        //Add some services to it
        ILogger logger = new FileLogger("ProjectInfinity.log", LogLevel.Debug);
        ServiceScope.Add(logger);
        logger.Critical("ProjectInfinity is starting...");
        //register service implementations
        ServiceScope.Add<IMessageBroker>(new MessageBroker()); //Our messagebroker
        ServiceScope.Add<IMainWindow>(new MainWindow()); //Our main window
        ServiceScope.Add<IPluginManager>(new ReflectionPluginManager());
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

        //Start the Core
        ProjectInfinityCore.Start();
        //When we return here, the core has quit
        logger.Critical("ProjectInfinity has stopped...");
      }
    }
  }
}