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
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for SAT>IP tuners.
  /// </summary>
  public class TunerSatIpBase : TunerStream, IMpeg2PidFilter
  {
    #region variables

    /// <summary>
    /// The SAT>IP server's UPnP UUID.
    /// </summary>
    protected string _uuid = string.Empty;

    /// <summary>
    /// The SAT>IP server's IP address.
    /// </summary>
    protected string _ipAddress = string.Empty;

    /// <summary>
    /// A sequence number or index.
    /// </summary>
    /// <remarks>
    /// This number uniquely identifies this tuner instance among the SAT>IP server's tuners of the
    /// same type.
    /// </remarks>
    protected int _sequenceNumber = -1;

    /// <summary>
    /// A string containing tuning details, used to identify one of this tuner's streams in an RTSP
    /// DESCRIBE response.
    /// </summary>
    protected string _streamMatchString = string.Empty;

    /// <summary>
    /// A string describing the pending PID filter changes.
    /// </summary>
    private string _pidFilterChangeString = string.Empty;

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
      Match m = Regex.Match(rtspDescribeResponse.ToString(), @";tuner=\d+,(\d+),(\d+),(\d+)," + _streamMatchString, RegexOptions.Singleline | RegexOptions.IgnoreCase);
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
      // TODO here we need to send a PLAY request, but for that we require the RTSP session ID and
      // SAT>IP stream ID. We might be able to get these from a DESCRIBE response... but it isn't
      // looking good at present as the Digital Devices DESCRIBE response doesn't seem to be
      // conformant.
      return false;
    }

    #endregion
  }
}