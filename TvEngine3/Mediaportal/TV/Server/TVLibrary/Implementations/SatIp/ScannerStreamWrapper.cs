using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// This class wraps an internal scanner, diverting calls to an alternate
  /// tuner as required.
  /// </summary>
  internal sealed class ScannerStreamWrapper : ITVScanning
  {
    private IScannerInternal _scanner = null;

    public ScannerStreamWrapper(IScannerInternal scanner, ITVCard tuner)
    {
      if (scanner == null || tuner == null)
      {
        throw new TvException("Scanner and/or tuner implementations not provided.");
      }
      _scanner = scanner;
      _scanner.Tuner = tuner;
    }

    #region ITVScanning member

    public List<IChannel> Scan(IChannel channel, ScanParameters settings)
    {
      return RenameUnknownChannels(_scanner.Scan(channel, settings));
    }

    public List<IChannel> ScanNIT(IChannel channel, ScanParameters settings)
    {
      return RenameUnknownChannels(_scanner.ScanNIT(channel, settings));
    }

    private List<IChannel> RenameUnknownChannels(List<IChannel> channels)
    {
      if (channels != null)
      {
        foreach (IChannel c in channels)
        {
          // TODO This code is disgusting. We should not have to rely on a specific channel type, DVBBaseChannel should
          // not have a frequency property... ugh! Please make this code better!
          DVBBaseChannel dvbChannel = c as DVBBaseChannel;
          if (dvbChannel != null && dvbChannel.Name != null && dvbChannel.Name.StartsWith("rtp://"))
          {
            dvbChannel.Name = string.Format("Unknown {0}-{1}", dvbChannel.Frequency / 1000, dvbChannel.ServiceId);
          }
        }
      }
      return channels;
    }

    #endregion
  }
}