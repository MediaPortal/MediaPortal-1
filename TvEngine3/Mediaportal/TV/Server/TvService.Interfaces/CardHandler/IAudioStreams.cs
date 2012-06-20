using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IAudioStreams
  {
    /// <summary>
    /// Gets the available audio streams.
    /// </summary>
    /// <value>The available audio streams.</value>
    IAudioStream[] Streams(IUser user, int idChannel);

    /// <summary>
    /// Gets the current audio stream.
    /// </summary>
    /// <returns></returns>
    IAudioStream GetCurrent(IUser user, int idChannel);

    /// <summary>
    /// Sets the current audio stream.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="stream">The stream.</param>
    /// <param name="idChannel"> </param>
    void Set(IUser user, IAudioStream stream, int idChannel);
  }
}