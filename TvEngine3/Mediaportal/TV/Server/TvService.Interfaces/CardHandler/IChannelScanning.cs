using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

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
    /// <returns>list of channels found</returns>
    IChannel[] Scan(IChannel channel);

    TuningDetail[] ScanNIT(IChannel channel);
  }
}