using System;
using System.ComponentModel;
using System.Windows;
using ProjectInfinity.Messaging;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.Themes;

namespace ProjectInfinity
{
  /// <summary>
  /// This is the main ProjectInfinity aplication
  /// </summary>
  public class Core : Application
  {
    #region Messages

    /// <summary>
    /// Occurs when ProjectInfinity is starting up.
    /// </summary>
    [MessagePublication(typeof (Startup))]
    public new event MessageHandler<Startup> Startup;

    /// <summary>
    /// Occurs when the ProjectInfinity core has finished starting up and is ready to show the
    /// main screen.
    /// </summary>
    [MessagePublication(typeof (StartupComplete))]
    public event MessageHandler<StartupComplete>	StartupComplete;

    /// <summary>
    /// <para>Occurs when the operating system is about to end the users' session (and thus shut down ProjectInfinity)</para>
    /// <para>The passed <see cref="SessionEndingCancelEventArgs"/> contain the reason why the session is ending.</para>
    /// </summary>
    /// <remarks>This is a cancelable event.</remarks>
    [MessagePublication(typeof (SessionEnding))]
    public new event CancelMessageHandler<SessionEnding> SessionEnding;

    /// <summary>
    /// Occurs when ProjectInfinity is about to shutdown.
    /// </summary>
    /// <remarks>This is a cancelable event.</remarks>
    [MessagePublication(typeof (BeforeShutdown))]
    public event CancelMessageHandler<BeforeShutdown>	BeforeShutdown;

    [MessagePublication(typeof (ShuttingDown))]
    public event MessageHandler<ShuttingDown> ShuttingDown;

    [MessagePublication(typeof (ShutdownComplete))]
    public event MessageHandler<ShutdownComplete> ShutdownComplete;

    /// <summary>
    /// Occurs when ProjectInfinity becomes the foreground application.
    /// </summary>
    [MessagePublication(typeof (Activated))]
    public new event MessageHandler<Activated> Activated;

    /// <summary>
    /// Occurs when ProjectInfinity stops being the foreground application.
    /// </summary>
    [MessagePublication(typeof (Deactivated))]
    public new event MessageHandler<Deactivated> Deactivated;

    #endregion

    public Core()
    {
      //Get the messagebroker and register ourselfs to it.
      //The messagebroker will inspect all our public events an look for the
      //MessagePublicationAttribute on them.  It will automatically add its eventhandler
      //to all such events it finds.
      IMessageBroker msgBroker = ServiceScope.Get<IMessageBroker>();
      msgBroker.Register(this);
    }


    private void DoStart()
    {
      //notify our own subscribers (through the message broker)
      OnStartup(new Startup());

      //Start the plugins
      ServiceScope.Get<IThemeManager>().SetDefaultTheme();
      ServiceScope.Get<IPluginManager>().Startup();
      INavigationService navigation = ServiceScope.Get<INavigationService>();

      navigation.Closing += mainWindow_Closing;
      try
      {
        OnStartupComplete(new StartupComplete());


        //navigation.Navigate(startupUri);
        Run(navigation.GetWindow());
        OnShuttingDown(new ShuttingDown());
      }
      finally
      {
        navigation.Closing -= mainWindow_Closing;
      }
      OnShutdownComplete(new ShutdownComplete());
    }

    protected virtual void OnStartup(Startup e)
    {
      if (Startup != null)
      {
        Startup(e);
      }
    }

    public static bool IsDesignMode
    {
      get { return !ServiceScope.IsRunning; }
    }

    #region Message Sending

    protected virtual void OnStartupComplete(StartupComplete e)
    {
      if (StartupComplete != null)
      {
        StartupComplete(e);
      }
    }

    protected virtual void OnBeforeShutdown(BeforeShutdown e)
    {
      if (BeforeShutdown != null)
      {
        BeforeShutdown(e);
      }
    }

    protected virtual void OnShuttingDown(ShuttingDown e)
    {
      if (ShuttingDown != null)
      {
        ShuttingDown(e);
      }
    }

    protected virtual void OnShutdownComplete(ShutdownComplete e)
    {
      if (ShutdownComplete != null)
      {
        ShutdownComplete(e);
      }
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
      base.OnSessionEnding(e);
      if (SessionEnding != null)
      {
        SessionEnding(new SessionEnding(e));
      }
    }

    protected override void OnActivated(EventArgs e)
    {
      base.OnActivated(e);
      if (Activated != null)
      {
        Activated(new Activated());
      }
    }

    protected override void OnDeactivated(EventArgs e)
    {
      base.OnDeactivated(e);
      if (Deactivated != null)
      {
        Deactivated(new Deactivated());
      }
    }

    #endregion

    #region Event Handling

    private void mainWindow_Closing(object sender, CancelEventArgs e)
    {
      OnBeforeShutdown(new BeforeShutdown(e));
    }

    [MessageSubscription(typeof(Shutdown))]
    private void Shutdown(Shutdown e)
    {
      if (e.Force)
        Shutdown();
      else
      ServiceScope.Get<INavigationService>().Close();
    }

    #endregion

    public static void Start()
    {
      Core core = new Core();
      core.DoStart();
    }
  }
}