using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

namespace Mediaportal.TV.Server.Plugins.Base.Interfaces
{
  /// <summary>
  /// base class for tv-server plugins
  /// </summary>
  public interface ITvServerPlugin
  {
    #region properties

    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    string Version { get; }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    string Author { get; }

    #endregion

    #region  methods

    /// <summary>
    /// Starts the plugin
    /// </summary>
    void Start(IInternalControllerService controllerService);

    /// <summary>
    /// Stops the plugin
    /// </summary>
    void Stop();

    /// <summary>
    /// returns the setup sections for display in SetupTv
    /// </summary>
    SectionSettings Setup { get; }

    #endregion
  }
}