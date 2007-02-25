using System;
using System.Threading;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;
using ProjectInfinity.Plugins;
using ProjectInfinity.Windows;

namespace ProjectInfinity
{
  public class PILauncher
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
        //Start the Core
        ProjectInfinity.Start(args);
        //When we return here, the core has quit
        logger.Critical("ProjectInfinity has stopped...");
      }
    }
  }
}