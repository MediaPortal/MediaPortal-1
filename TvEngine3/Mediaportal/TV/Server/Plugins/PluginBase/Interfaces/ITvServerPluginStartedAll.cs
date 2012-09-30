namespace Mediaportal.TV.Server.Plugins.Base.Interfaces
{
  /// <summary>
  /// Interface for plugins that implement the StartedAll method.
  /// </summary>
  public interface ITvServerPluginStartedAll
  {
    /// <summary>
    /// Called when all plugins where started.
    /// </summary>
    void StartedAll();
  }
}