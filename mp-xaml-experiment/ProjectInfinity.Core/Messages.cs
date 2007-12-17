using System.ComponentModel;
using ProjectInfinity.Messaging;

namespace ProjectInfinity
{
  /// <summary>
  /// Notifies listeners that MediaPortal Infinity is starting up.
  /// </summary>
  public class Startup : Message
  {}

  /// <summary>
  /// Notifies listeners that MediaPortal Infinity's startup process is complete.
  /// </summary>
  public class StartupComplete : Message
  {}

  /// <summary>
  /// Notifies listeners that the user is ending his Windows session
  /// </summary>
  public class SessionEnding : CancelMessage
  {
    public SessionEnding()
    {}

    public SessionEnding(bool cancel) : base(cancel)
    {}

    public SessionEnding(CancelEventArgs e) : base(e)
    {}
  }

  /// <summary>
  /// Notifies listeners that Infinity is about to shutdown
  /// </summary>
  public class BeforeShutdown : CancelMessage
  {
    public BeforeShutdown()
    {}

    public BeforeShutdown(bool cancel) : base(cancel)
    {}

    public BeforeShutdown(CancelEventArgs e) : base(e)
    {}
  }

  /// <summary>
  /// Requests a system shutdown
  /// </summary>
  public class Shutdown : Message
  {
    private bool _force;

    public Shutdown() : this(false)
    {}

    public Shutdown(bool force)
    {
      _force = force;
    }

    public bool Force
    {
      get { return _force; }
      set { _force = value; }
    }
  }

  public class ShuttingDown : Message
  {}

  public class ShutdownComplete : Message
  {}

  public class Activated : Message
  {}

  public class Deactivated : Message
  {}
}