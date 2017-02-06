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
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using RtspClient = Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp.RtspClient;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorUpnp"/> which detects SAT>IP tuners.
  /// </summary>
  internal class TunerDetectorSatIp : ITunerDetectorUpnp
  {
    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "SAT>IP";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a UPnP device.
    /// </summary>
    /// <param name="descriptor">The UPnP device's root descriptor.</param>
    /// <param name="controlPoint">The control point that the device is attached to.</param>
    /// <returns>the compatible tuners exposed by the device</returns>
    public ICollection<ITuner> DetectTuners(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
    {
      List<ITuner> tuners = new List<ITuner>();

      // Is the UPnP device a SAT>IP server?
      if (!descriptor.TypeVersion_URN.Equals("urn:ses-com:device:SatIPServer:1"))
      {
        return tuners;
      }
      this.LogInfo("SAT>IP detector: tuner added");

      int feCountC = 0;
      int feCountC2 = 0;
      int feCountS2 = 0;
      int feCountT = 0;
      int feCountT2 = 0;
      string remoteHost = new Uri(descriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;

      // SAT>IP servers may have more than one tuner, but their descriptors
      // only ever have one device node. We have to find out how many tuners
      // are available and what type they are.
      XmlNamespaceManager nm = new XmlNamespaceManager(descriptor.DeviceNavigator.NameTable);
      nm.AddNamespace("s", "urn:ses-com:satip");
      XPathNodeIterator it = descriptor.DeviceNavigator.Select("s:X_SATIPCAP/text()", nm);
      if (it.MoveNext())
      {
        // The easiest way to get the information we need is from X_SATIPCAP,
        // but unfortunately that element is optional.
        this.LogDebug("SAT>IP detector: get capabilites from X_SATIPCAP");
        string[] sections = it.Current.Value.Split(',');
        foreach (string section in sections)
        {
          Match m = Regex.Match(section, @"^([^-]+)-(\d+)$", RegexOptions.IgnoreCase);
          if (m.Success)
          {
            string msys = m.Groups[1].Captures[0].Value;
            int count = int.Parse(m.Groups[2].Captures[0].Value);
            switch (msys)
            {
              case "DVBC":
                feCountC += count;
                break;
              case "DVBC2":
                feCountC2 += count;
                break;
              case "DVBS2":
                feCountS2 += count;
                break;
              case "DVBT":
                feCountT += count;
                break;
              case "DVBT2":
                feCountT2 += count;
                break;
              default:
                this.LogWarn("SAT>IP detector: unsupported msys {0} found in X_SATIPCAP {1}, section {2}", msys, it.Current.Value, section);
                break;
            }
          }
          else
          {
            this.LogError("SAT>IP detector: failed to interpret X_SATIPCAP {0}, section {1}", it.Current.Value, section);
          }
        }
      }
      else
      {
        // X_SATIPCAP is not available. Try an RTSP DESCRIBE.
        this.LogDebug("SAT>IP detector: attempt to get capabilities using RTSP DESCRIBE");
        RtspResponse response = null;
        try
        {
          RtspRequest request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/", remoteHost));
          request.Headers.Add("Accept", "application/sdp");
          request.Headers.Add("Connection", "close");
          using (RtspClient client = new RtspClient(remoteHost, 554))
          {
            client.SendRequest(request, out response);
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "SAT>IP detector: RTSP DESCRIBE request and/or response failed");
        }
        if (response != null)
        {
          if (response.StatusCode == RtspStatusCode.Ok)
          {
            Match m = Regex.Match(response.Body, @"s=SatIPServer:1\s+([^\s]+)\s+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (m.Success)
            {
              string frontEndInfo = m.Groups[1].Captures[0].Value;
              try
              {
                string[] frontEndCounts = frontEndInfo.Split(',');
                feCountS2 = int.Parse(frontEndCounts[0]);
                if (frontEndCounts.Length >= 2)
                {
                  // Assume all DVB-X frontends also support DVB-X2. The user
                  // can correct this with configuration.
                  feCountT2 = int.Parse(frontEndCounts[1]);
                  if (frontEndCounts.Length > 2)
                  {
                    feCountC2 = int.Parse(frontEndCounts[2]);
                    if (frontEndCounts.Length > 3)
                    {
                      this.LogWarn("SAT>IP detector: RTSP DESCRIBE response contains more than 3 front end counts, not supported");
                    }
                  }
                }
              }
              catch (Exception ex)
              {
                this.LogError(ex, "SAT>IP detector: failed to interpret RTSP DESCRIBE response SatIPServer section {0}", frontEndInfo);
              }
            }
            else
            {
              this.LogDebug("SAT>IP detector: RTSP DESCRIBE response does not contain SatIPServer section");
            }
          }
          else if (response.StatusCode == RtspStatusCode.NotFound)
          {
            this.LogDebug("SAT>IP detector: server does not have any active streams");
          }
          else
          {
            this.LogError("SAT>IP detector: RTSP DESCRIBE response status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          }
        }
      }

      if (feCountC == 0 && feCountC2 == 0 && feCountS2 == 0 && feCountT == 0 && feCountT2 == 0)
      {
        this.LogWarn("SAT>IP detector: failed to gather front end information, assuming 2 DVB-S/S2 front ends");
        feCountS2 = 2;
      }

      this.LogInfo("  C/C2 = {0}/{1}", feCountC, feCountC2);
      this.LogInfo("  S2   = {0}", feCountS2);
      this.LogInfo("  T/T2 = {0}/{1}", feCountT, feCountT2);

      int i = 1;
      int j = 0;
      for (; i <= feCountC; i++)
      {
        tuners.Add(new TunerSatIpCable(descriptor, i, new TunerStreamTve(string.Format("MediaPortal SAT>IP {0} DVB-C Stream Source", descriptor.DeviceUUID), i)));
      }
      j += feCountC;
      for (; i <= feCountC2 + j; i++)
      {
        // Note: DVB-C2 not supported for now.
        tuners.Add(new TunerSatIpCable(descriptor, i, new TunerStreamTve(string.Format("MediaPortal SAT>IP {0} DVB-C/C2 Stream Source", descriptor.DeviceUUID), i)));
      }
      j += feCountC2;

      // Currently the Digital Devices Octopus Net is the only SAT>IP product
      // to support DVB-T/T2. The DVB-T/T2 tuners also support DVB-C/C2. In
      // general we'll assume that if the DVB-C/C2 and DVB-T/T2 counts are
      // equal the tuners are hybrid.
      if (feCountC + feCountC2 > 0 && (feCountC + feCountC2) == (feCountT + feCountT2))
      {
        i = 1;
        j = 0;
      }

      for (; i <= feCountT + j; i++)
      {
        tuners.Add(new TunerSatIpTerrestrial(descriptor, i, BroadcastStandard.DvbT, new TunerStreamTve(string.Format("MediaPortal SAT>IP {0} DVB-T Stream Source", descriptor.DeviceUUID), i)));
      }
      j += feCountT;
      for (; i <= feCountT2 + j; i++)
      {
        tuners.Add(new TunerSatIpTerrestrial(descriptor, i, BroadcastStandard.DvbT | BroadcastStandard.DvbT2, new TunerStreamTve(string.Format("MediaPortal SAT>IP {0} DVB-T/T2 Stream Source", descriptor.DeviceUUID), i)));
      }
      j += feCountT2;

      for (; i <= feCountS2 + j; i++)
      {
        tuners.Add(new TunerSatIpSatellite(descriptor, i, new TunerStreamTve(string.Format("MediaPortal SAT>IP {0} DVB-S/S2 Stream Source", descriptor.DeviceUUID), i)));
      }

      return tuners;
    }
  }
}