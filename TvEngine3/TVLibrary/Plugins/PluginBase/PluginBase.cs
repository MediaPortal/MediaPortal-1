using System;
using System.Collections.Generic;
using System.Text;
using TvControl;

namespace TvEngine
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
    string Name { get;}

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    string Version { get;}

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    string Author { get;}

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    bool MasterOnly { get;}
    #endregion

    #region  methods
    /// <summary>
    /// Starts the plugin
    /// </summary>
    void Start(IController controller);

    /// <summary>
    /// Stops the plugin
    /// </summary>
    void Stop();

    /// <summary>
    /// returns the setup sections for display in SetupTv
    /// </summary>
    SetupTv.SectionSettings Setup { get;}
    #endregion
  }
}
