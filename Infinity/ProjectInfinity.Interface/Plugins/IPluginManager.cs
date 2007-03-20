using System;
using System.Collections;
using System.Collections.Generic;
using ProjectInfinity.Messaging;
using ProjectInfinity.Messaging.PluginMessages;

namespace ProjectInfinity.Plugins
{
  /// <summary>
  /// Interface for plug-in managers
  /// </summary>
  /// <remarks>
  /// A plug-in manager is responsible for enumerating, starting and stopping plugins</remarks>
  public interface IPluginManager
  {
    /// <summary>
    /// Gets an enumerable list of available plugins
    /// </summary>
    /// <returns>An <see cref="IEnumerable<IPlugin>"/> list.</returns>
    /// <remarks>A configuration program can use this list to present the user a list of available plugins that he can (de)activate.</remarks>
    IEnumerable<IPluginInfo> GetAvailablePlugins();

    /// <summary>
    /// Stops the given plug-in.
    /// </summary>
    /// <param name="pluginName">name of the plug-in to stop</param>
    void Stop(string pluginName);

    /// <summary>
    /// Stops all plug-ins
    /// </summary>
    void StopAll();

    /// <summary>
    /// Starts the given plug-in.
    /// </summary>
    /// <param name="pluginName">name of the plug-in to start</param>
    void Start(string pluginName);

    /// <summary>
    /// Starts all plug-ins that are activated by the user.
    /// </summary>
    void StartAll();

    [MessagePublication(typeof (PluginStart))]
    event EventHandler<PluginStartStopEventArgs> PluginStarted;

    [MessagePublication(typeof (PluginStop))]
    event EventHandler<PluginStartStopEventArgs> PluginStopped;
  }
}