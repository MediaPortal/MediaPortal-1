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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An implementation of <see cref="IChannelScanner"/> for Ceton CableCARD
  /// tuners.
  /// </summary>
  internal class ChannelScannerDriCeton : ChannelScannerDri
  {
    private string _tunerIpAddress = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerDriCeton"/> class.
    /// </summary>
    /// <param name="scanner">The channel scanner instance to use for scanning.</param>
    /// <param name="requestFdcTables">A delegate for requesting table data from the tuner's forward data channel.</param>
    /// <param name="tunerIpAddress">The tuner's IP address.</param>
    public ChannelScannerDriCeton(IChannelScannerInternal scanner, RequestFdcTablesDelegate requestFdcTables, string tunerIpAddress)
      : base(scanner, requestFdcTables)
    {
      _tunerIpAddress = tunerIpAddress;
    }

    protected override void UpdateChannels(IList<ScannedChannel> channels, IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames, ISet<string> ignoredChannelNumbers)
    {
      IDictionary<string, IChannel> cetonChannels;
      if (!GetCetonChannelLineUp(out cetonChannels))
      {
        return;
      }
      this.LogInfo("scan DRI Ceton: Ceton channel count = {0}", cetonChannels.Count);

      foreach (ScannedChannel channel in channels)
      {
        string lcn = channel.Channel.LogicalChannelNumber;
        IChannel tuningChannel;
        if (cetonChannels.TryGetValue(lcn, out tuningChannel))
        {
          if (channel.Channel.Name.StartsWith("Unknown ") && !string.IsNullOrEmpty(tuningChannel.Name))
          {
            this.LogDebug("  setting name, name = {0}, number = {1}", tuningChannel.Name, lcn);
            channel.Channel.Name = tuningChannel.Name;
          }
 
        }

        this.LogWarn("  scanned channel not in Ceton lineup, name = {0}, number = {1}", channel.Channel.Name, lcn);
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
      this.LogInfo("scan DRI Ceton: add {0} extra channel(s) from the Ceton list...", cetonChannels.Count);
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
    private bool GetCetonChannelLineUp(out IDictionary<string, IChannel> channels)
    {
      channels = new Dictionary<string, IChannel>();

      // Request.
      Uri uri = new Uri(new Uri(_tunerIpAddress), "view_channel_map.cgi?page=0&xml=1");
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
      request.Timeout = 5000;
      HttpWebResponse response = null;
      try
      {
        response = (HttpWebResponse)request.GetResponse();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "scan DRI Ceton: failed to get Ceton XML lineup from tuner, URI = {0}", uri);
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
                  this.LogWarn("scan DRI Ceton: unrecognised Ceton modulation scheme {0}, falling back to automatic", modulation);
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
        this.LogError(ex, "scan DRI Ceton: failed to handle SiliconDust XML lineup response from tuner, URI = {0}", uri);
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