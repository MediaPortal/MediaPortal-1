using System;

namespace Mediaportal.TV.Server.Plugins.Base.Interfaces
{
  /// <summary>
  /// Interface for plugins that implement the AddService method.
  /// </summary>
  public interface ITvServerPluginCommunciation
  {
    /// <summary>
    /// Supply a service class implementation for client-server plugin communication
    /// </summary>
    object GetServiceInstance { get; }

    /// <summary>
    /// Supply a service class interface for client-server plugin communication
    /// </summary>
    Type GetServiceInterfaceForContractType { get; }
  }
}