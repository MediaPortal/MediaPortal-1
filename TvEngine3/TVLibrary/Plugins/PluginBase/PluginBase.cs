#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using TvControl;

namespace TvEngine
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

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    bool MasterOnly { get; }

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
    SetupTv.SectionSettings Setup { get; }

    #endregion
  }
}