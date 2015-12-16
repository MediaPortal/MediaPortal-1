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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
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

        // Get the Ceton or SiliconDust proprietary channel list.
        // We use the list for three purposes...
        //
        // 1. Marking inaccessible channels as not visible in the guide.
        // We can determine whether a given channel is accessible or not by
        // trying to tune it directly... but this is incredibly time consuming
        // (~2 seconds per channel x ~500 channels???) and error prone for
        // hundreds of channels. SiliconDust already do this so we save time
        // and reuse the information from their list.
        //
        // 2. Updating names of channels where real names were previously not
        //    available.
        // For some reason some channels in the CableCARD's channel list don't
        // seem to be assigned names. It makes sense to acquire missing names
        // from a proprietary list where possible.
        //
        // 3. Adding channels delivered using switched digital video (SDV).
        // Channels delivered via SDV are not generally included in the
        // CableCARD's channel list. Sometimes they are, but apparently that is
        // a mistake which we can't rely on. The only certain way to get a
        // complete channel list when SDV is active is to request it from the
        // tuning adaptor/resolver. Only the tuner can communicate directly
        // with the TA/TR. Since we have no way to ask the tuner to ask the
        // TA/TR to give us the information, we have to use proprietary lists.
        if (_isCetonDevice)
        {
          UpdateChannelsCeton(_tunerIpAddress, channels, groupNames, ignoredChannelNumbers);
        }
        else
        {
          UpdateChannelsSiliconDust(_tunerIpAddress, channels, groupNames, ignoredChannelNumbers);
        }
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

    private static void UpdateChannelsSiliconDust(string tunerIpAddress, IList<ScannedChannel> channels, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, ISet<string> ignoredChannelNumbers)
    {
      IDictionary<string, string> channelsSubscribedCopyFreely;
      IDictionary<string, string> channelsNotSubscribed;
      IDictionary<string, string> channelsCopyOnce;
      if (!GetSiliconDustChannelSets(tunerIpAddress, out channelsSubscribedCopyFreely, out channelsNotSubscribed, out channelsCopyOnce))
      {
        return;
      }

      // Build/filter the final channel list.
      Log.Info("scan DRI: update channel details using Silicondust lineup info...");
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
            Log.Debug("  setting name, name = {0}, number = {1}", name, lcn);
            channel.Channel.Name = name;
          }
          if (channel.IsVisibleInGuide)
          {
            Log.Debug("  clearing visible-in-guide flag, name = {0}, number = {1}", channel.Channel.Name, lcn);
            channel.IsVisibleInGuide = false;
          }
          continue;
        }
        if (channelsSubscribedCopyFreely.TryGetValue(lcn, out name))
        {
          channelsSubscribedCopyFreely.Remove(lcn);
          if (channel.Channel.Name.StartsWith("Unknown ") && !string.IsNullOrEmpty(name))
          {
            Log.Debug("  setting name, name = {0}, number = {1}", name, lcn);
            channel.Channel.Name = name;
          }
          continue;
        }

        Log.Warn("  scanned channel not in SiliconDust lineup(s), name = {0}, number = {1}", channel.Channel.Name, lcn);
      }

      foreach (string channelNumber in ignoredChannelNumbers)
      {
        channelsSubscribedCopyFreely.Remove(channelNumber);
        channelsNotSubscribed.Remove(channelNumber);
        channelsCopyOnce.Remove(channelNumber);
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
      Log.Info("scan DRI: add {0} switched digital video channel(s)...", sdvChannelCount);
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

    private static bool GetSiliconDustChannelSets(string tunerIpAddress, out IDictionary<string, string> channelsSubscribedCopyFreely, out IDictionary<string, string> channelsNotSubscribed, out IDictionary<string, string> channelsCopyOnce)
    {
      channelsSubscribedCopyFreely = new Dictionary<string, string>();  // channel number => name
      channelsNotSubscribed = new SortedDictionary<string, string>();   // channel number => name
      channelsCopyOnce = new Dictionary<string, string>();              // channel number => name

      IDictionary<string, string> channelsSubscribedCopyOnce;
      if (!GetSiliconDustChannelLineUp(new Uri(new Uri(tunerIpAddress), "lineup.xml"), out channelsSubscribedCopyFreely, out channelsSubscribedCopyOnce))
      {
        return false;
      }

      int subscribedCount = channelsSubscribedCopyFreely.Count + channelsCopyOnce.Count;
      Log.Info("scan DRI: SiliconDust lineup info...");
      Log.Info("  subscribed         = {0}", subscribedCount);
      Log.Info("  subscribed DRM     = {0}", channelsSubscribedCopyOnce.Count);
      foreach (KeyValuePair<string, string> channel in channelsSubscribedCopyOnce)
      {
        Log.Debug("  {0, -5} = {1}", channel.Key, channel.Value);
      }

      IDictionary<string, string> channelsCopyFreely;   // includes channels that aren't part of the user's subscribed package(s)
      if (!GetSiliconDustChannelLineUp(new Uri(new Uri(tunerIpAddress), "lineup.xml?show=all"), out channelsCopyFreely, out channelsCopyOnce))
      {
        channelsCopyOnce = channelsSubscribedCopyOnce;
        return true;
      }

      HashSet<string> temp = new HashSet<string>(channelsCopyFreely.Keys);
      temp.ExceptWith(channelsSubscribedCopyFreely.Keys);
      Log.Info("  not subscribed     = {0}", temp.Count);
      foreach (string channelNumber in temp)
      {
        string name = channelsCopyFreely[channelNumber];
        channelsNotSubscribed.Add(channelNumber, name);
        Log.Debug("  {0, -5} = {1}", channelNumber, name);
      }

      temp = new HashSet<string>(channelsCopyOnce.Keys);
      temp.ExceptWith(channelsSubscribedCopyOnce.Keys);
      Log.Info("  not subscribed DRM = {0}", temp.Count);
      foreach (string channelNumber in temp)
      {
        Log.Debug("  {0, -5} = {1}", channelNumber, channelsCopyOnce[channelNumber]);
      }
      return true;
    }

    private static bool GetSiliconDustChannelLineUp(Uri uri, out IDictionary<string, string> channelsCopyFreely, out IDictionary<string, string> channelsCopyOnce)
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
        Log.Warn(ex, "scan DRI: failed to get SiliconDust XML lineup from tuner, URI = {0}", uri);
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
              if (!string.IsNullOrEmpty(number))
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
        Log.Error(ex, "scan DRI: failed to handle SiliconDust XML lineup response from tuner, URI = {0}", uri);
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

    private static void UpdateChannelsCeton(string tunerIpAddress, IList<ScannedChannel> channels, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, ISet<string> ignoredChannelNumbers)
    {
      IDictionary<string, IChannel> cetonChannels;
      if (!GetCetonChannelLineUp(tunerIpAddress, out cetonChannels))
      {
        return;
      }
      Log.Info("scan DRI: Ceton channel count = {0}", cetonChannels.Count);

      foreach (ScannedChannel channel in channels)
      {
        string lcn = channel.Channel.LogicalChannelNumber;
        IChannel tuningChannel;
        if (cetonChannels.TryGetValue(lcn, out tuningChannel))
        {
          if (channel.Channel.Name.StartsWith("Unknown ") && !string.IsNullOrEmpty(tuningChannel.Name))
          {
            Log.Debug("  setting name, name = {0}, number = {1}", tuningChannel.Name, lcn);
            channel.Channel.Name = tuningChannel.Name;
          }
 
        }

        Log.Warn("  scanned channel not in Ceton lineup, name = {0}, number = {1}", channel.Channel.Name, lcn);
      }

      foreach (string channelNumber in ignoredChannelNumbers)
      {
        cetonChannels.Remove(channelNumber);
      }

      // Any channels remaining in the Ceton channel list are assumed to have
      // been missed during scanning (time limit too low) or not be included in
      // the CableCARD's channel list (SDV channels). Add them.
      if (cetonChannels.Count == 0)
      {
        return;
      }
      Log.Info("scan DRI: add {0} extra channel(s) from the Ceton list...", cetonChannels.Count);
      foreach (IChannel channel in cetonChannels.Values)
      {
        channels.Add(ChannelScannerAtsc.CreateScannedChannel(channel, true, BroadcastStandard.Scte, groupNames));
      }
    }

    /// <remarks>
    /// We don't know:
    /// 1. Whether applications are excluded.
    /// 2. Whether SDV channels are included.
    /// Also note that copy once channels are definitely not excluded.
    /// </remarks>
    private static bool GetCetonChannelLineUp(string tunerIpAddress, out IDictionary<string, IChannel> channels)
    {
      channels = new Dictionary<string, IChannel>();

      // Request.
      Uri uri = new Uri(new Uri(tunerIpAddress), "view_channel_map.cgi?page=0&xml=1");
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
      request.Timeout = 5000;
      HttpWebResponse response = null;
      try
      {
        response = (HttpWebResponse)request.GetResponse();
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "scan DRI: failed to get Ceton XML lineup from tuner, URI = {0}", uri);
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
          int number = 0;
          string name = string.Empty;
          string modulation = string.Empty;
          int frequency = 0;
          int programNumber = 0;
          string eia = string.Empty;    // <physical channel number>.<program number>
          int sourceId = 0;
          while (!xmlReader.EOF)
          {
            if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name.Equals("Program"))
            {
              if (number != 0)
              {
                ChannelScte channel = new ChannelScte();
                channel.LogicalChannelNumber = number.ToString();
                channel.Name = name;
                channel.Provider = "Cable";
                channel.MediaType = MediaType.Television;   // assumed
                channel.IsEncrypted = true;                 // assumed
                channel.Frequency = frequency;
                if (modulation.Equals("QAM256"))
                {
                  channel.ModulationScheme = ModulationSchemeQam.Qam256;
                }
                else if (modulation.Equals("QAM64"))
                {
                  channel.ModulationScheme = ModulationSchemeQam.Qam64;
                }
                else
                {
                  Log.Warn("scan DRI: unrecognised Ceton modulation scheme {0}, falling back to automatic", modulation);
                  channel.ModulationScheme = ModulationSchemeQam.Automatic;
                }
                channel.TransportStreamId = 0;              // doesn't really matter
                channel.SourceId = sourceId;
                channel.ProgramNumber = programNumber;
                channel.PmtPid = 0;                         // lookup the correct PID from the PAT when the channel is tuned
                channels.Add(channel.LogicalChannelNumber, channel);
              }
              number = 0;
              name = string.Empty;
              modulation = string.Empty;
              frequency = 0;
              programNumber = 0;
              eia = string.Empty;
              sourceId = 0;
            }
            else if (xmlReader.NodeType == XmlNodeType.Element)
            {
              if (xmlReader.Name.Equals("number"))
              {
                number = xmlReader.ReadElementContentAsInt();
              }
              else if (xmlReader.Name.Equals("name"))
              {
                // Note: for some reason ReadElementContentAsBase64() seems to
                // also read the <modulation> start element, so we avoid using
                // it.
                name = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(xmlReader.ReadElementContentAsString()));
              }
              else if (xmlReader.Name.Equals("modulation"))
              {
                modulation = xmlReader.ReadElementContentAsString();
              }
              else if (xmlReader.Name.Equals("frequency"))
              {
                frequency = xmlReader.ReadElementContentAsInt();
              }
              else if (xmlReader.Name.Equals("program"))
              {
                programNumber = xmlReader.ReadElementContentAsInt();
              }
              else if (xmlReader.Name.Equals("eia"))
              {
                eia = xmlReader.ReadElementContentAsString();
              }
              else if (xmlReader.Name.Equals("sourceid"))
              {
                sourceId = xmlReader.ReadElementContentAsInt();
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
        Log.Error(ex, "scan DRI: failed to handle SiliconDust XML lineup response from tuner, URI = {0}", uri);
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