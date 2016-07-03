#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System.Collections.Generic;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An implementation of <see cref="IChannelScanner"/> for CableCARD tuners
  /// which receive SCTE-compliant transport streams.
  /// </summary>
  internal class ChannelScannerDri : IChannelScannerInternal
  {
    public delegate void RequestFdcTablesDelegate(List<byte> tableIds);

    #region variables

    private IChannelScannerInternal _scanner = null;
    private ChannelScannerAtsc _scannerAtsc = null;
    private RequestFdcTablesDelegate _requestFdcTables = null;
    private volatile bool _cancelScan = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerDri"/> class.
    /// </summary>
    /// <param name="scanner">The channel scanner instance to use for scanning.</param>
    /// <param name="requestFdcTables">A delegate for requesting table data from the tuner's forward data channel.</param>
    public ChannelScannerDri(IChannelScannerInternal scanner, RequestFdcTablesDelegate requestFdcTables)
    {
      _scanner = scanner;
      _scannerAtsc = scanner as ChannelScannerAtsc;
      _requestFdcTables = requestFdcTables;
    }

    /// <summary>
    /// A delegate to invoke when a program specific information table section
    /// is received from the tuner's out-of-band channel.
    /// </summary>
    /// <param name="section">The PSI table section.</param>
    public void OnOutOfBandSectionReceived(byte[] section)
    {
      if (_scannerAtsc != null)
      {
        _scannerAtsc.OnOutOfBandSectionReceived(section);
      }
    }

    #region IChannelScannerInternal member

    /// <summary>
    /// Set the scanner's tuner.
    /// </summary>
    public ITuner Tuner
    {
      set
      {
        _scanner.Tuner = value;
      }
    }

    #endregion

    #region IChannelScanner members

    /// <summary>
    /// Reload the scanner's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      _scanner.ReloadConfiguration();
    }

    /// <summary>
    /// Get the scanner's current status.
    /// </summary>
    /// <value><c>true</c> if the scanner is scanning, otherwise <c>false</c></value>
    public bool IsScanning
    {
      get
      {
        return _scanner.IsScanning;
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for channel information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="isFastNetworkScan"><c>True</c> to do a fast network scan.</param>
    /// <param name="channels">The channel information found.</param>
    /// <param name="groupNames">The names of the groups referenced in <paramref name="channels"/>.</param>
    public void Scan(IChannel channel, bool isFastNetworkScan, out IList<ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      _cancelScan = false;
      ManualResetEvent requestTablesEvent = new ManualResetEvent(false);
      try
      {
        ThreadPool.QueueUserWorkItem(
          delegate
          {
            do
            {
              _requestFdcTables(new List<byte> { 0xc2, 0xc3, 0xc4, 0xc7, 0xc8, 0xc9 });
            }
            while (!requestTablesEvent.WaitOne(30000));
          }
        );
        ISet<string> ignoredChannelNumbers;
        if (_scannerAtsc != null)
        {
          _scannerAtsc.Scan(channel, isFastNetworkScan, out channels, out groupNames, out ignoredChannelNumbers);
        }
        else
        {
          _scanner.Scan(channel, isFastNetworkScan, out channels, out groupNames);
          ignoredChannelNumbers = new HashSet<string>();
        }
        if (_cancelScan)
        {
          return;
        }

        IDictionary<string, ScannedChannel> channelsByNumber = new Dictionary<string, ScannedChannel>(channels.Count);
        HashSet<string> namelessChannels = new HashSet<string>();
        HashSet<string> hiddenChannels = new HashSet<string>();
        foreach (var c in channels)
        {
          channelsByNumber[c.Channel.LogicalChannelNumber] = c;
          if (c.Channel.Name.StartsWith("Unknown "))
          {
            namelessChannels.Add(c.Channel.LogicalChannelNumber);
          }
          if (!c.IsVisibleInGuide)
          {
            hiddenChannels.Add(c.Channel.LogicalChannelNumber);
          }
        }

        this.LogInfo("scan DRI: stats...");
        this.LogInfo("  scanned = {0}", channels.Count);
        this.LogInfo("  no name = {0} [{1}]", namelessChannels.Count, string.Join(", ", namelessChannels));
        this.LogInfo("  hidden  = {0} [{1}]", hiddenChannels.Count, string.Join(", ", hiddenChannels));
        this.LogInfo("  ignored = {0} [{1}]", ignoredChannelNumbers.Count, string.Join(", ", ignoredChannelNumbers));

        UpdateChannels(channels, groupNames, ignoredChannelNumbers);
      }
      finally
      {
        requestTablesEvent.Set();
        requestTablesEvent.Close();
        requestTablesEvent.Dispose();
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for transmitter tuning details
    /// within the available network information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the tuning details found</returns>
    public IList<TuningDetail> ScanNetworkInformation(IChannel channel)
    {
      return _scanner.ScanNetworkInformation(channel);
    }

    /// <summary>
    /// Abort scanning for channels and/or network information.
    /// </summary>
    public void AbortScanning()
    {
      _cancelScan = true;
      _scanner.AbortScanning();
    }

    #endregion

    /// <remarks>
    /// Use proprietary interfaces to improve the quality and accuracy of the
    /// scan results.
    ///
    /// 1. Mark inaccessible channels as not visible in the guide.
    /// We can determine whether a given channel is accessible or not by trying
    /// to tune it directly... but this is incredibly time consuming (~2
    /// seconds per channel x ~500 channels???) and error prone for hundreds of
    /// channels. SiliconDust already collect this information; it makes sense
    /// to reuse their cache.
    ///
    /// 2. Update names of channels where real names were previously not
    ///    available.
    /// For some reason some channels in the CableCARD's channel list don't
    /// seem to be assigned names. It makes sense to acquire missing names via
    /// proprietary interfaces if they're available.
    ///
    /// 3. Add channels delivered using switched digital video (SDV).
    /// Channels delivered via SDV are not generally included in the
    /// CableCARD's channel list. Sometimes they are, but apparently that is a
    /// mistake which we can't rely on. The only certain way to get a complete
    /// channel list when SDV is active is to request it from the tuning
    /// adaptor/resolver. Only the tuner can communicate directly with the
    /// TA/TR. There is no generic, standardised way to ask the tuner to ask
    /// the TA/TR to give us the information. Therefore we have to fall back on
    /// proprietary interfaces.
    /// </remarks>
    protected virtual void UpdateChannels(IList<ScannedChannel> channels, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, ISet<string> ignoredChannelNumbers)
    {
    }
  }
}