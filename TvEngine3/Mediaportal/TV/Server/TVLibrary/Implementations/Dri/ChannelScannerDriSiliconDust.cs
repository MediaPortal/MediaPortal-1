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
using System.Xml;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An implementation of <see cref="IChannelScanner"/> for SiliconDust
  /// CableCARD tuners. Hauppauge CableCARD tuners (designed by SiliconDust)
  /// are assumed to also be supported. 
  /// </summary>
  internal class ChannelScannerDriSiliconDust : ChannelScannerDri
  {
    private string _tunerIpAddress = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerDriSiliconDust"/> class.
    /// </summary>
    /// <param name="scanner">The channel scanner instance to use for scanning.</param>
    /// <param name="isCetonDevice"><c>True</c> if the tuner is a Ceton device.</param>
    /// <param name="tunerIpAddress">The tuner's IP address.</param>
    /// <param name="requestFdcTables">A delegate for requesting table data from the tuner's forward data channel.</param>
    public ChannelScannerDriSiliconDust(IChannelScannerInternal scanner, RequestFdcTablesDelegate requestFdcTables, string tunerIpAddress)
      : base(scanner, requestFdcTables)
    {
      _tunerIpAddress = tunerIpAddress;
    }

    protected override void UpdateChannels(IList<ScannedChannel> channels, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, ISet<string> ignoredChannelNumbers)
    {
      IDictionary<string, string> channelsSubscribedCopyFreely;
      IDictionary<string, string> channelsNotSubscribed;
      IDictionary<string, string> channelsCopyOnce;
      if (!GetSiliconDustChannelSets(out channelsSubscribedCopyFreely, out channelsNotSubscribed, out channelsCopyOnce))
      {
        return;
      }

      // Build/filter the final channel list.
      this.LogInfo("scan DRI SiliconDust: update channel details using Silicondust lineup info...");
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

        this.LogWarn("  scanned channel not in SiliconDust lineup(s), name = {0}, number = {1}", channel.Channel.Name, lcn);
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
      this.LogInfo("scan DRI SiliconDust: add {0} switched digital video channel(s)...", sdvChannelCount);
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
      this.LogInfo("scan DRI SiliconDust: SiliconDust lineup info...");
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
        Log.Warn(ex, "scan DRI SiliconDust: failed to get SiliconDust XML lineup from tuner, URI = {0}", uri);
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
        Log.Error(ex, "scan DRI SiliconDust: failed to handle SiliconDust XML lineup response from tuner, URI = {0}", uri);
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
  }
}