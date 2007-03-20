using System;
using System.ComponentModel;
using System.Windows;
using ProjectInfinity.Messaging;
using ProjectInfinity.Messaging.SystemMessages;
using ProjectInfinity.Plugins;
using ProjectInfinity.Windows;

namespace ProjectInfinity
{
  /// <summary>
  /// This is the main ProjectInfinity aplication
  /// </summary>
  public class ProjectInfinityCore : Application
  {
    #region Messages

    /// <summary>
    /// Occurs when ProjectInfinity is starting up.
    /// </summary>
    [MessagePublication(typeof (Startup))]
    public new event EventHandler Startup;

    /// <summary>
    /// Occurs when the ProjectInfinity core has finished starting up and is ready to show the
    /// main screen.
    /// </summary>
    [MessagePublication(typeof (StartupComplete))]
    public event EventHandler StartupComplete;

    /// <summary>
    /// <para>Occurs when the operating system is about to end the users' session (and thus shut down ProjectInfinity)</para>
    /// <para>The passed <see cref="SessionEndingCancelEventArgs"/> contain the reason why the session is ending.</para>
    /// </summary>
    /// <remarks>This is a cancelable event.</remarks>
    [MessagePublication(typeof (SessionEnding))]
    public new event SessionEndingCancelEventHandler SessionEnding;

    /// <summary>
    /// Occurs when ProjectInfinity is about to shutdown.
    /// </summary>
    /// <remarks>This is a cancelable event.</remarks>
    [MessagePublication(typeof (BeforeShutdown))]
    public event CancelEventHandler BeforeShutdown;

    [MessagePublication(typeof (Shutdown))]
    public new event EventHandler Shutdown;

    [MessagePublication(typeof (ShutdownComplete))]
    public event EventHandler ShutdownComplete;

    /// <summary>
    /// Occurs when ProjectInfinity becomes the foreground application.
    /// </summary>
    [MessagePublication(typeof (Activated))]
    public new event EventHandler Activated;

    /// <summary>
    /// Occurs when ProjectInfinity stops being the foreground application.
    /// </summary>
    [MessagePublication(typeof (Deactivated))]
    public new event EventHandler Deactivated;

    #endregion

    public ProjectInfinityCore()
    {
      //Get the messagebroker and register ourselfs to it.
      //The messagebroker will inspect all our public events an look for the
      //MessagePublicationAttribute on them.  It will automatically add its eventhandler
      //to all such events it finds.
      IMessageBroker msgBroker = ServiceScope.Get<IMessageBroker>();
      msgBroker.Register(this);
    }

    public static void Start()
    {
      ProjectInfinityCore projectInfinity = new ProjectInfinityCore();
      projectInfinity.Run();
    }

    private void Run()
    {
      //notify our own subscribers (through the message broker)
      OnStartup(new EventArgs());

      //Start the plugins
      ServiceScope.Get<IPluginManager>().StartAll();

      //Get the main window and start it
      IMainWindow window = ServiceScope.Get<IMainWindow>();
      if (window == null)
      {
        throw new ArgumentNullException("Service is not available", "IMainWindow");
      }
      Window mainWindow = window as Window;
      if (mainWindow == null)
      {
        throw new ArgumentException("Window does not inherit from System.Windows.Window", "IMainWindow");
      }
      mainWindow.Closing += new CancelEventHandler(mainWindow_Closing);
      try
      {
        OnStartupComplete(EventArgs.Empty);
        Run(mainWindow);
        OnShutdown(EventArgs.Empty);
      }
      finally
      {
        mainWindow.Closing -= new CancelEventHandler(mainWindow_Closing);
      }
      OnShutdownComplete(EventArgs.Empty);
    }

    protected virtual void OnStartup(EventArgs e)
    {
      if (Startup != null)
      {
        Startup(this, e);
      }
    }

    #region Message Sending

    protected virtual void OnStartupComplete(EventArgs e)
    {
      if (StartupComplete != null)
      {
        StartupComplete(this, e);
      }
    }

    protected virtual void OnBeforeShutdown(CancelEventArgs e)
    {
      if (BeforeShutdown != null)
      {
        BeforeShutdown(this, e);
      }
    }

    protected virtual void OnShutdown(EventArgs e)
    {
      if (Shutdown != null)
      {
        Shutdown(this, e);
      }
    }

    protected virtual void OnShutdownComplete(EventArgs e)
    {
      if (ShutdownComplete != null)
      {
        ShutdownComplete(this, e);
      }
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
      base.OnSessionEnding(e);
      if (SessionEnding != null)
      {
        SessionEnding(this, e);
      }
    }

    protected override void OnActivated(EventArgs e)
    {
      base.OnActivated(e);
      if (Activated != null)
      {
        Activated(this, e);
      }
    }

    protected override void OnDeactivated(EventArgs e)
    {
      base.OnDeactivated(e);
      if (Deactivated != null)
      {
        Deactivated(this, e);
      }
    }

    #endregion

    #region Event Handling

    private void mainWindow_Closing(object sender, CancelEventArgs e)
    {
      OnBeforeShutdown(e);
    }

    #endregion
  }
}