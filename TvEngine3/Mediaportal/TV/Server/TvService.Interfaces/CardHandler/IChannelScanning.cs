using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IChannelScanning
  {
    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>
    bool IsScanning { get; }

    void Scan(IChannel channel, bool isFastNetworkScan, out IList<ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames);

    ScannedTransmitter[] ScanNIT(IChannel channel);
  }
}