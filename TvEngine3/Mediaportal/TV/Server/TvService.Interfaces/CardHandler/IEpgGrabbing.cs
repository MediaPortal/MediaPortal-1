using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IEpgGrabbing
  {
    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <returns></returns>
    bool Start(BaseEpgGrabber grabber);

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    void Abort();

    /// <summary>
    /// Gets the epg.
    /// </summary>
    /// <value>The epg.</value>
    List<EpgChannel> Epg { get; }

    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    bool IsGrabbing { get; }

    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="user">User</param>
    void Stop(IUser user);
  }
}