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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using RtspClient = Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp.RtspClient;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorUpnp"/> which detects SAT>IP tuners.
  /// </summary>
  internal class TunerDetectorSatIp : ITunerDetectorUpnp
  {
    private string ignoreUUID = SettingsManagement.GetValue("SATIP_UDN", System.Guid.NewGuid().ToString("D"));
    private bool detectMPserver = SettingsManagement.GetValue("SATIP_detectMPServer", true);
    
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
    public ICollection<ITVCard> DetectTuners(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
    {
      List<ITVCard> tuners = new List<ITVCard>();

      // Is the UPnP device a SAT>IP server?
      if (!descriptor.TypeVersion_URN.Equals("urn:ses-com:device:SatIPServer:1"))
      {
        return tuners;
      }
      // Is the SAT>IP server our own server?
      if (descriptor.DeviceUDN.Equals("uuid:" + ignoreUUID))
      {
        return tuners;
      }
      // Do we want to detect the MP SAT>IP server at all?
      if (descriptor.FriendlyName.Contains("MediaPortal") && !detectMPserver)
      {
        return tuners;
      }

      this.LogInfo("SAT>IP detector: tuner added");

      int satelliteFrontEndCount = 0;
      int terrestrialFrontEndCount = 0;
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
            if (msys.Equals("DVBS2"))
            {
              satelliteFrontEndCount += int.Parse(m.Groups[2].Captures[0].Value);
            }
            else if (msys.Equals("DVBT") || msys.Equals("DVBT2"))
            {
              terrestrialFrontEndCount += int.Parse(m.Groups[2].Captures[0].Value);
            }
            else
            {
              this.LogWarn("SAT>IP detector: unsupported msys {0} found in X_SATIPCAP {1}, section {2}", msys, it.Current.Value, section);
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
          RtspClient client = new RtspClient(remoteHost, 554);
          client.SendRequest(request, out response);
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
                if (frontEndCounts.Length >= 2)
                {
                  satelliteFrontEndCount = int.Parse(frontEndCounts[0]);
                  terrestrialFrontEndCount = int.Parse(frontEndCounts[1]);
                  if (frontEndCounts.Length > 2)
                  {
                    this.LogWarn("SAT>IP detector: RTSP DESCRIBE response contains more than 2 front end counts, not supported");
                  }
                }
                else
                {
                  satelliteFrontEndCount = int.Parse(frontEndCounts[0]);
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

      if (satelliteFrontEndCount == 0 && terrestrialFrontEndCount == 0)
      {
        this.LogWarn("SAT>IP detector: failed to gather front end information, assuming 2 satellite front ends");
        satelliteFrontEndCount = 2;
      }

      this.LogInfo("  sat FE count  = {0}", satelliteFrontEndCount);
      this.LogInfo("  terr FE count = {0}", terrestrialFrontEndCount);

      for (int i = 1; i <= satelliteFrontEndCount; i++)
      {
        tuners.Add(new TunerSatIpSatellite(descriptor, i, new TunerStream("MediaPortal SAT>IP Stream Source", i)));
      }
      for (int i = satelliteFrontEndCount + 1; i <= satelliteFrontEndCount + terrestrialFrontEndCount; i++)
      {
        // Currently the Digital Devices Octopus Net is the only SAT>IP product
        // to support DVB-T/T2. The DVB-T tuners also support DVB-C.
        tuners.Add(new TunerSatIpTerrestrial(descriptor, i, new TunerStream("MediaPortal SAT>IP Stream Source", i)));
        tuners.Add(new TunerSatIpCable(descriptor, i, new TunerStream("MediaPortal SAT>IP Stream Source", i)));
      }
      return tuners;
    }
  }
}