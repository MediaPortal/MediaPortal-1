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
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP.Description;
using RtspClient = Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp.RtspClient;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for SAT>IP tuners.
  /// </summary>
  public abstract class TunerSatIpBase : TunerStream, IMpeg2PidFilter
  {
    #region constants

    private static readonly Regex REGEX_RESPONSE_DESCRIBE_SIGNAL_INFO = new Regex(@";tuner=\d+,(\d+),(\d+),(\d+),", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    #endregion

    #region variables

    /// <summary>
    /// The SAT>IP server's UPnP device descriptor.
    /// </summary>
    protected DeviceDescriptor _serverDescriptor = null;

    /// <summary>
    /// The IP address of the local NIC which is connected to the SAT>IP server.
    /// </summary>
    protected IPAddress _localIpAddress = null;

    /// <summary>
    /// The SAT>IP server's IP address.
    /// </summary>
    protected string _serverIpAddress = string.Empty;

    /// <summary>
    /// The tuner's sequence number or index.
    /// </summary>
    /// <remarks>
    /// This number uniquely identifies this tuner instance among the SAT>IP server's tuners of the
    /// same type. Note that this is not the SAT>IP front end identifier.
    /// </remarks>
    protected int _sequenceNumber = -1;

    /// <summary>
    /// A string describing the pending PID filter changes.
    /// </summary>
    private string _pidFilterChangeString = string.Empty;

    /// <summary>
    /// An RTSP client, used to communicate with the SAT>IP server.
    /// </summary>
    private RtspClient _rtspClient = null;

    private string _rtspSessionId = string.Empty;
    private int _satIpStreamId = -1;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpBase"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="localIpAddress">The IP address of the local NIC which is connected to the server.</param>
    /// <param name="serverIpAddress">The server's current IP address.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="tunerType">A character describing the tuner type.</param>
    public TunerSatIpBase(DeviceDescriptor serverDescriptor, IPAddress localIpAddress, string serverIpAddress, int sequenceNumber, char tunerType)
      : base(serverDescriptor.FriendlyName + " " + tunerType + " Tuner " + sequenceNumber, serverDescriptor.DeviceUUID + tunerType + sequenceNumber)
    {
      _serverDescriptor = serverDescriptor;
      _localIpAddress = localIpAddress;
      _serverIpAddress = serverIpAddress;
      _sequenceNumber = sequenceNumber;
    }

    #endregion

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected void PerformTuning(string url)
    {
      if (_currentTuningDetail == null)
      {
        // Need to SETUP a session.
      }
      else
      {
        new IPEndPoint(
        new UdpClient(
      }
    }

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
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      _customDeviceInterfaces.Add(this);
      base.PerformLoading();
    }

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      bool isSignalLocked = false;
      int signalLevel = 0;
      int signalQuality = 0;

      try
      {
        RtspResponse response = null;
        try
        {
          RtspRequest request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/stream={1}", _serverIpAddress, _satIpStreamId));
          request.Headers.Add("Accept", "application/sdp");
          request.Headers.Add("Connection", "close");
          request.Headers.Add("Session", _rtspSessionId);
          _rtspClient.SendRequest(request, out response);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "SAT>IP base: exception updating signal status");
          return;
        }

        if (response.StatusCode != RtspStatusCode.Ok)
        {
          this.LogError("SAT>IP base: failed to retrieve signal status, non-OK RTSP DESCRIBE status code {0} {1}", response.StatusCode, response.ReasonPhrase);
          return;
        }

        // Find the first stream information. We assume that all signal statistics apply equally as
        // we're only meant to be using one front end.
        Match m = REGEX_RESPONSE_DESCRIBE_SIGNAL_INFO.Match(response.Body);
        if (m.Success)
        {
          isSignalLocked = m.Groups[2].Captures[0].Value.Equals("1");
          signalLevel = int.Parse(m.Groups[1].Captures[0].Value) * 100 / 255;    // level: 0..255 => 0..100
          signalQuality = int.Parse(m.Groups[3].Captures[0].Value) * 100 / 15;   // quality: 0..15 => 0..100
          return;
        }

        this.LogError("SAT>IP base: failed to locate signal status information in RTSP DESCRIBE response");
      }
      finally
      {
        _isSignalPresent = isSignalLocked;
        _isSignalLocked = isSignalLocked;
        _signalLevel = signalLevel;
        _signalQuality = signalQuality;
      }
    }

    private RtspStatusCode SendRtspPlayRequest(string additionalParameters, out string responseString)
    {
      responseString = null;
      try
      {
        StringBuilder rtspPlayRequest = new StringBuilder();
        rtspPlayRequest.AppendFormat("DESCRIBE rtsp://{0}/stream={1}", _ipAddress, _satIpStreamId);
        if (!string.IsNullOrEmpty(additionalParameters))
        {
          rtspPlayRequest.AppendFormat("?{0}", additionalParameters);
        }
        rtspPlayRequest.Append(" RTSP/1.0\r\n");
        rtspPlayRequest.AppendFormat("CSeq: {0}\r\n", _rtspCseqNumber);
        rtspPlayRequest.AppendFormat("Session: {0}\r\n", _rtspSessionId);
        rtspPlayRequest.Append("Accept: application/sdp\r\n");
        rtspPlayRequest.Append("Connection: close\r\n\r\n");
        NetworkStream stream = _tcpClient.GetStream();
        try
        {
          byte[] requestBytes = System.Text.Encoding.ASCII.GetBytes(rtspPlayRequest.ToString());
          stream.Write(requestBytes, 0, requestBytes.Length);
          byte[] responseBytes = new byte[_tcpClient.ReceiveBufferSize];
          while (stream.DataAvailable)
          {
            int byteCount = stream.Read(responseBytes, 0, responseBytes.Length);
            responseString = System.Text.Encoding.ASCII.GetString(responseBytes, 0, byteCount);
          }
        }
        finally
        {
          stream.Close();
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "SAT>IP base: exception sending RTSP PLAY request");
        return RtspStatusCode.Unknown;
      }

      return ReadRtspResponseStatusCode(responseString);
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public byte Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalIdentifier">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalIdentifier, CardType tunerType, object context)
    {
      // This is a "special" implementation. We do initialisation in other functions.
      return true;
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITVCard tuner, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the tuner is started but
    /// before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that TV Server wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITVCard tuner, ref TunerAction action)
    {
    }

    #endregion

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for the current multiplex.
    /// </summary>
    /// <param name="tuningDetail">The current multiplex/transponder tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    public bool ShouldEnableFilter(IChannel tuningDetail)
    {
      // SAT>IP tuners are networked tuners. It is desirable to enable PID filtering in order to
      // reduce the network bandwidth used.
      return true;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    public bool DisableFilter()
    {
      _pidFilterChangeString = "pids=all";
      return true;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    public int MaximumPidCount
    {
      get
      {
        return -1;  // maximum not known
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool AllowStreams(ICollection<ushort> pids)
    {
      if (!string.IsNullOrEmpty(_pidFilterChangeString))
      {
        _pidFilterChangeString += "&";
      }
      _pidFilterChangeString += "addpids=" + string.Join(",", pids);
      return true;
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool BlockStreams(ICollection<ushort> pids)
    {
      if (!string.IsNullOrEmpty(_pidFilterChangeString))
      {
        _pidFilterChangeString += "&";
      }
      _pidFilterChangeString += "delpids=" + string.Join(",", pids);
      return true;
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    public bool ApplyFilter()
    {
      this.LogDebug("SAT>IP base: apply PID filter");
      string rtspPlayResponse = null;
      RtspStatusCode code = SendRtspPlayRequest(_pidFilterChangeString, out rtspPlayResponse);
      _pidFilterChangeString = string.Empty;

      if (code == RtspStatusCode.Ok)
      {
        this.LogDebug("SAT>IP base: result = success");
        return true;
      }

      if (code != RtspStatusCode.Unknown)
      {
        this.LogWarn("SAT>IP base: failed to apply PID filter, non-OK RTSP DESCRIBE status code {0}", code);
      }
      return false;
    }

    #endregion
  }
}