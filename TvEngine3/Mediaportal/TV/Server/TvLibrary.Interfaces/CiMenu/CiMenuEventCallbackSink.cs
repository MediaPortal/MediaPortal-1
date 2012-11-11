using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu
{
  /// <summary>
  /// This class is used by client to provide delegates to the server that will
  /// fire events back through these delegates. Overriding OnServerEvent to capture
  /// the callback from the server
  /// </summary>
  public abstract class CiMenuEventCallbackSink : ICiMenuEventCallback
  {
    /// <summary>
    /// Called by the server to fire the call back to the client
    /// </summary>
    /// <param name="menu">a CiMenu object</param>
    public void FireCiMenuCallback(CiMenu menu)
    {
      //Console.WriteLine("Activating callback");
      CiMenuCallback(menu);
    }

    /// <summary>
    /// Client overrides this method to receive the callback events from the server
    /// </summary>
    /// <param name="menu">a CiMenu object</param>
    public abstract void CiMenuCallback(CiMenu menu);
  }
}