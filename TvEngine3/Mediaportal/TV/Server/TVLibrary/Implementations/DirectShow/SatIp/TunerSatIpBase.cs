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
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for SAT>IP tuners.
  /// </summary>
  public class TunerSatIpBase : TunerStream
  {
    #region variables

    /// <summary>
    /// The SAT>IP server UUID.
    /// </summary>
    protected string _uuid = string.Empty;

    /// <summary>
    /// The SAT>IP server IP address.
    /// </summary>
    protected string _ipAddress = string.Empty;

    /// <summary>
    /// A sequence number or index.
    /// </summary>
    /// <remarks>
    /// This number uniquely identifies this tuner instance among the SAT>IP server's tuners of the same
    /// type.
    /// </remarks>
    protected int _sequenceNumber = -1;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpBase"/> class.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalIdentifier">The external identifier for the tuner.</param>
    /// <param name="ipAddress">The SAT>IP server's current IP address.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerSatIpBase(string name, string externalIdentifier, string ipAddress, int sequenceNumber)
      : base(name, externalIdentifier)
    {
      _ipAddress = ipAddress;
      _sequenceNumber = sequenceNumber;
    }

    #endregion

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        return new ScannerMpeg2TsBase(this, _filterTsWriter as ITsChannelScan);
      }
    }

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="streamMatchString">A string to use to match stream tuning details.</param>
    protected void PerformSignalStatusUpdate(string streamMatchString)
    {
      StringBuilder rtspDescribeResponse = new StringBuilder();
      try
      {
        StringBuilder rtspDescribeRequest = new StringBuilder();
        rtspDescribeRequest.Append("DESCRIBE rtsp://").Append(_ipAddress).Append(":554/ RTSP/1.0\r\n");
        rtspDescribeRequest.Append("CSeq: 1\r\n");
        rtspDescribeRequest.Append("Accept: application/sdp\r\n");
        rtspDescribeRequest.Append("Connection:close\r\n\r\n");
        TcpClient client = new TcpClient(_ipAddress, 554);
        NetworkStream stream = client.GetStream();
        byte[] requestBytes = System.Text.Encoding.ASCII.GetBytes(rtspDescribeRequest.ToString());
        stream.Write(requestBytes, 0, requestBytes.Length);
        byte[] responseBytes = new byte[client.ReceiveBufferSize];
        while (stream.DataAvailable)
        {
          int byteCount = stream.Read(responseBytes, 0, responseBytes.Length);
          rtspDescribeResponse.Append(System.Text.Encoding.ASCII.GetString(responseBytes, 0, byteCount));
        }
        stream.Close();
        client.Close();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "SAT>IP base: exception updating signal status");
        _isSignalPresent = false;
        _isSignalLocked = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      // Find the information for the current stream.
      Match m = Regex.Match(rtspDescribeResponse.ToString(), @";tuner=\d+,(\d+),(\d+),(\d+)," + streamMatchString, RegexOptions.Singleline | RegexOptions.IgnoreCase);
      if (m.Success)
      {
        _isSignalLocked = m.Groups[2].Captures[0].Value.Equals("1");
        _isSignalPresent = _isSignalLocked;
        _signalLevel = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;    // level: 0..255 => 0..100
        _signalQuality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;   // quality: 0..15 => 0..100
        return;
      }

      this.LogWarn("SAT>IP base: failed to locate stream in SAT>IP RTSP DESCRIBE response");
      _isSignalPresent = false;
      _isSignalLocked = false;
      _signalLevel = 0;
      _signalQuality = 0;
    }
  }
}