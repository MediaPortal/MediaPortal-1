using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IChannelScanning
  {
    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>
    bool IsScanning { get; }

    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="channel">IChannel containing the transponder tuning details.</param>
    /// <param name="settings">Scan settings</param>
    /// <returns>list of channels found</returns>
    IChannel[] Scan(IChannel channel, ScanParameters settings);

    IChannel[] ScanNIT(IChannel channel, ScanParameters settings);
  }
}