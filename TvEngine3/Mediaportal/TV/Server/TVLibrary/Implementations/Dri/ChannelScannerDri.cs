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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
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
    private bool _isCetonDevice = false;
    private string _tunerIpAddress = null;
    private RequestFdcTablesDelegate _requestFdcTables = null;
    private volatile bool _cancelScan = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerDri"/> class.
    /// </summary>
    /// <param name="scanner">The channel scanner instance to use for scanning.</param>
    /// <param name="isCetonDevice"><c>True</c> if the tuner is a Ceton device.</param>
    /// <param name="tunerIpAddress">The tuner's IP address.</param>
    /// <param name="requestFdcTables">A delegate </param>
    public ChannelScannerDri(IChannelScannerInternal scanner, bool isCetonDevice, string tunerIpAddress, RequestFdcTablesDelegate requestFdcTables)
    {
      _scanner = scanner;
      _scannerAtsc = scanner as ChannelScannerAtsc;
      _isCetonDevice = isCetonDevice;
      _tunerIpAddress = tunerIpAddress;
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
        _scanner.Scan(channel, isFastNetworkScan, out channels, out groupNames);
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
        if (_isCetonDevice)
        {
          return;
        }

        UpdateChannels(channels, groupNames);
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

    #region private functions

    private void UpdateChannels(IList<ScannedChannel> channels, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      // Get the SiliconDust proprietary channel list.
      // We use this for three purposes:
      // 1. Marking inaccessible channels as not visible in the guide.
      // 2. Updating names of channels where real names were previously not
      //    available.
      // 3. Adding channels delivered using switched digital video (SDV).
      //
      // We can determine whether a given channel is accessible or not by
      // trying to tune it directly... but this is incredibly time consuming
      // and error prone for hundreds of channels. Therefore we use the
      // proprietary list instead.
      //
      // Channels delivered via SDV are not generally included in the
      // CableCARD's channel list. Sometimes they are, but apparently that is a
      // mistake which we can't rely on. The only certain way to get a complete
      // channel list when SDV is active is to request it from the tuning
      // adaptor/resolver. Only the tuner can communicate directly with the
      // TA/TR, and we have no way to ask the tuner to ask the TA/TR to give us
      // the information. Therefore we have to use the proprietary list.
      IDictionary<string, string> channelsSubscribedCopyFreely;
      IDictionary<string, string> channelsNotSubscribed;
      IDictionary<string, string> channelsCopyOnce;
      if (!GetSiliconDustChannelSets(out channelsSubscribedCopyFreely, out channelsNotSubscribed, out channelsCopyOnce))
      {
        return;
      }

      // Build/filter the final channel list.
      this.LogInfo("scan DRI: update channel details...");
      foreach (ScannedChannel channel in channels)
      {
        string lcn = channel.Channel.LogicalChannelNumber;
        string name;
        if (channelsNotSubscribed.TryGetValue(lcn, out name) || channelsCopyOnce.TryGetValue(lcn, out name))
        {
          channelsNotSubscribed.Remove(lcn);
          channelsCopyOnce.Remove(lcn);
          if (channel.Channel.Name.StartsWith("Unknown ") && !string.IsNullOrEmpty(name))
          {
            this.LogDebug("  setting name, name = {0}, number = {1}", name, lcn);
            channel.Channel.Name = name;
          }
          if (channel.IsVisibleInGuide)
          {
            this.LogDebug("  clearing visible-in-guide flag, name = {0}, number = {1}", channel.Channel.Name, lcn);
            channel.IsVisibleInGuide = false;
          }
          continue;
        }
        if (channelsSubscribedCopyFreely.TryGetValue(lcn, out name))
        {
          channelsSubscribedCopyFreely.Remove(lcn);
          if (channel.Channel.Name.StartsWith("Unknown ") && !string.IsNullOrEmpty(name))
          {
            this.LogDebug("  setting name, name = {0}, number = {1}", name, lcn);
            channel.Channel.Name = name;
          }
          continue;
        }

        this.LogWarn("  unknown channel accessibility, name = {0}, number = {1}", channel.Channel.Name, lcn);
      }

      // Any channels remaining in the SiliconDust channel lists are assumed to
      // be switched digital video channels that weren't in the CableCARD's
      // channel list. Add them. Note these channels won't be tunable by clear
      // QAM tuners even if they're actually not encrypted, because we can't
      // get the physical tuning details.
      int sdvChannelCount = channelsSubscribedCopyFreely.Count + channelsNotSubscribed.Count + channelsCopyOnce.Count;
      if (sdvChannelCount == 0)
      {
        return;
      }
      this.LogInfo("scan DRI: add {0} switched digital video channel(s)...", sdvChannelCount);
      foreach (KeyValuePair<string, string> channel in channelsSubscribedCopyFreely)
      {
        channels.Add(ChannelScannerAtsc.CreateSdvChannel(channel.Value, channel.Key, true, groupNames));
      }
      foreach (KeyValuePair<string, string> channel in channelsNotSubscribed)
      {
        channels.Add(ChannelScannerAtsc.CreateSdvChannel(channel.Value, channel.Key, false, groupNames));
      }
      foreach (KeyValuePair<string, string> channel in channelsCopyOnce)
      {
        channels.Add(ChannelScannerAtsc.CreateSdvChannel(channel.Value, channel.Key, false, groupNames));
      }
    }

    private bool GetSiliconDustChannelSets(out IDictionary<string, string> channelsSubscribedCopyFreely, out IDictionary<string, string> channelsNotSubscribed, out IDictionary<string, string> channelsCopyOnce)
    {
      channelsSubscribedCopyFreely = new Dictionary<string, string>();  // channel number => name
      channelsNotSubscribed = new SortedDictionary<string, string>();   // channel number => name
      channelsCopyOnce = new Dictionary<string, string>();              // channel number => name

      IDictionary<string, string> channelsSubscribedCopyOnce;
      if (!GetSiliconDustChannelLineUp(new Uri(new Uri(_tunerIpAddress), "lineup.xml"), out channelsSubscribedCopyFreely, out channelsSubscribedCopyOnce))
      {
        return false;
      }

      int subscribedCount = channelsSubscribedCopyFreely.Count + channelsCopyOnce.Count;
      this.LogInfo("scan DRI: SiliconDust lineup info...");
      this.LogInfo("  subscribed         = {0}", subscribedCount);
      this.LogInfo("  subscribed DRM     = {0}", channelsSubscribedCopyOnce.Count);
      foreach (KeyValuePair<string, string> channel in channelsSubscribedCopyOnce)
      {
        this.LogDebug("  {0, -5} = {1}", channel.Key, channel.Value);
      }

      IDictionary<string, string> channelsCopyFreely;   // includes channels that aren't part of the user's subscribed package(s)
      if (!GetSiliconDustChannelLineUp(new Uri(new Uri(_tunerIpAddress), "lineup.xml?show=all"), out channelsCopyFreely, out channelsCopyOnce))
      {
        channelsCopyOnce = channelsSubscribedCopyOnce;
        return true;
      }

      HashSet<string> temp = new HashSet<string>(channelsCopyFreely.Keys);
      temp.ExceptWith(channelsSubscribedCopyFreely.Keys);
      this.LogInfo("  not subscribed     = {0}", temp.Count);
      foreach (string channelNumber in temp)
      {
        string name = channelsCopyFreely[channelNumber];
        channelsNotSubscribed.Add(channelNumber, name);
        this.LogDebug("  {0, -5} = {1}", channelNumber, name);
      }

      temp = new HashSet<string>(channelsCopyOnce.Keys);
      temp.ExceptWith(channelsSubscribedCopyOnce.Keys);
      this.LogInfo("  not subscribed DRM = {0}", temp.Count);
      foreach (string channelNumber in temp)
      {
        this.LogDebug("  {0, -5} = {1}", channelNumber, channelsCopyOnce[channelNumber]);
      }
      return true;
    }

    private bool GetSiliconDustChannelLineUp(Uri uri, out IDictionary<string, string> channelsCopyFreely, out IDictionary<string, string> channelsCopyOnce)
    {
      channelsCopyFreely = new SortedDictionary<string, string>();  // channel number => name
      channelsCopyOnce = new SortedDictionary<string, string>();    // channel number => name

      // Request.
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
      request.Timeout = 5000;
      HttpWebResponse response = null;
      try
      {
        response = (HttpWebResponse)request.GetResponse();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "scan DRI: failed to get SiliconDust XML lineup from tuner, URI = {0}", uri);
        request.Abort();
        return false;
      }

      // Response.
      string content = string.Empty;
      try
      {
        using (Stream responseStream = response.GetResponseStream())
        using (TextReader textReader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
        using (XmlReader xmlReader = XmlReader.Create(textReader))
        {
          string number = string.Empty;
          string name = string.Empty;
          bool isCopyFreely = true;
          while (!xmlReader.EOF)
          {
            if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name.Equals("Program"))
            {
              if (string.IsNullOrEmpty(number))
              {
                if (!isCopyFreely)
                {
                  channelsCopyOnce.Add(number, name);
                }
                else
                {
                  channelsCopyFreely.Add(number, name);
                }
              }
              number = string.Empty;
              name = string.Empty;
              isCopyFreely = true;
            }
            else if (xmlReader.NodeType == XmlNodeType.Element)
            {
              if (xmlReader.Name.Equals("GuideNumber"))
              {
                // I don't know whether SiliconDust would have two-part
                // channel numbers in this list, or what format they would
                // have (eg. X.Y or X-Y) if they did. For now assume they
                // either don't have them or use our format (X.Y).
                number = xmlReader.ReadElementContentAsString();
              }
              else if (xmlReader.Name.Equals("GuideName"))
              {
                name = xmlReader.ReadElementContentAsString();
              }
              else if (
                (xmlReader.Name.Equals("DRM") && xmlReader.ReadElementContentAsInt() == 1) ||               // new format
                (xmlReader.Name.Equals("Tags") && xmlReader.ReadElementContentAsString().Contains("drm"))   // old format
              )
              {
                isCopyFreely = false;
              }
              else
              {
                xmlReader.Read();
              }
              continue;
            }
            xmlReader.Read();
          }
          xmlReader.Close();
          textReader.Close();
          responseStream.Close();
        }
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "scan DRI: failed to handle SiliconDust XML lineup response from tuner, URI = {0}", uri);
        return false;
      }
      finally
      {
        if (response != null)
        {
          response.Close();
        }
      }
    }

    #endregion
  }
}