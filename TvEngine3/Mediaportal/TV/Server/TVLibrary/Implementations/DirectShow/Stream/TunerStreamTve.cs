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
using System.IO;
using System.Text;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for receiving MPEG 2 transport
  /// streams with the MediaPortal stream source filter.
  /// </summary>
  internal class TunerStreamTve : TunerStreamBase
  {
    public static readonly Guid CLSID = new Guid(0xd3dd4c59, 0xd3a7, 0x4b82, 0x97, 0x27, 0x7b, 0x92, 0x03, 0xeb, 0x67, 0xc0);

    #region variables

    /// <summary>
    /// All filter instances read the same configuration file. In order to
    /// enable instance-specific configuration, we have to prevent multiple
    /// instances from loading simultaneously. That's what this lock is used
    /// for.
    /// </summary>
    private static object _configLock = new object();
    private StringBuilder _config = null;
    private string _httpRtpUdpnetworkInterface = null;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStreamTve"/> class.
    /// </summary>
    /// <param name="sequenceNumber">A sequence number or index for this instance.</param>
    public TunerStreamTve(int sequenceNumber)
      : base("MediaPortal Stream Source", sequenceNumber)
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStreamTve"/> class.
    /// </summary>
    /// <param name="name">A short name or description for the tuner.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerStreamTve(string name, int sequenceNumber)
      : base(name + " " + sequenceNumber, name + " " + sequenceNumber)
    {
    }

    #endregion

    protected override IBaseFilter AddSourceFilter()
    {
      // Write this filter's config to file so it picks it up during loading.
      string configFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MediaPortal TV Server", "MPIPTVSource.ini");
      lock (_configLock)
      {
        using (StreamWriter sw = new StreamWriter(File.Open(configFileName, FileMode.Create), Encoding.ASCII))
        {
          sw.Write(_config.ToString());
          sw.Close();
        }
        return FilterGraphTools.AddFilterFromFile(Graph, "MPIPTVSource.ax", CLSID, Name);
      }
    }

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(TVDatabase.Entities.Tuner configuration)
    {
      this.LogDebug("DirectShow stream TVE: reload configuration");
      base.ReloadConfiguration(configuration);

      StreamTunerSettings settings;
      if (configuration == null)
      {
        settings = CreateDefaultConfiguration(false);
      }
      else
      {
        if (configuration.StreamTunerSettings == null)
        {
          configuration.StreamTunerSettings = CreateDefaultConfiguration(true);
        }
        settings = configuration.StreamTunerSettings;
      }
      _httpRtpUdpnetworkInterface = settings.NetworkInterface;

      // Settings that are shared by all tuners...
      int maximumLogFileSize = SettingsManagement.GetValue("streamTunersMaximumLogFileSize", 1024 * 1024);      // unit = bytes
      int caWaitTime = SettingsManagement.GetValue("streamTunersConditionalAccessWaitTime", 0);                 // unit = ms
      int maximumPluginCount = SettingsManagement.GetValue("streamTunersMaximumPluginCount", 256);              // plugins are RTP, RTSP, UDP etc.
      int bufferChunkByteCount = SettingsManagement.GetValue("streamTunersBufferChunkByteCount", 32768);        // unit = bytes
      int transferBufferChunkCount = SettingsManagement.GetValue("streamTunersTransferBufferChunkCount", 16);
      bool analyseDiscontinuities = SettingsManagement.GetValue("streamTunersAnalyseDiscontinuities", false);   // log continuity counter jumps on all PIDs
      bool dumpOutput = SettingsManagement.GetValue("streamTunersDumpOutput", false);
      int portMinimum = SettingsManagement.GetValue("streamTunersPortMinimum", 49152);
      int portMaximum = SettingsManagement.GetValue("streamTunersPortMaximum", 65535);
      int rtspUdpMaximumPayloadByteCount = SettingsManagement.GetValue("streamTunersRtspUdpMaximumPayloadByteCount", 12288);  // unit = bytes

      int logVerbosity = 0;   // none
      LogLevel levelsEnabled = Log.Level;
      if (levelsEnabled.HasFlag(LogLevel.Debug))
      {
        logVerbosity = 4;
        if (dumpOutput || settings.DumpInput)
        {
          logVerbosity = 5;
        }
      }
      else if (levelsEnabled.HasFlag(LogLevel.Info))
      {
        logVerbosity = 3;
      }
      else if (levelsEnabled.HasFlag(LogLevel.Warn))
      {
        logVerbosity = 2;
      }
      else if (levelsEnabled.HasFlag(LogLevel.Error) || levelsEnabled.HasFlag(LogLevel.Critical))
      {
        logVerbosity = 1;
      }

      // Divide the port range between RTP and UDP. Allocate at least two
      // thirds for RTP because two ports are required (RTP + RTCP) for every
      // one UDP port.
      int rtpPortMaximum = portMinimum + (int)Math.Ceiling((double)(portMaximum - portMinimum + 1) * 2 / 3) - 1;

      // Convert buffer sizes to chunk counts. If the buffer size isn't an
      // integral of the chunk size, use the next highest integral.
      int bufferChunkCount;
      int bufferChunkCountMaximum;
      int bufferSizeBytes = settings.BufferSize * 1024;
      int bufferSizeRemainder = bufferSizeBytes % bufferChunkByteCount;
      if (bufferSizeRemainder != 0)
      {
        bufferChunkCount = (bufferSizeBytes - bufferSizeRemainder + bufferChunkByteCount) / bufferChunkByteCount;
      }
      else
      {
        bufferChunkCount = bufferSizeBytes / bufferChunkByteCount;
      }
      bufferSizeBytes = settings.BufferSizeMaximum * 1024;
      bufferSizeRemainder = bufferSizeBytes % bufferChunkByteCount;
      if (bufferSizeRemainder != 0)
      {
        bufferChunkCountMaximum = (bufferSizeBytes - bufferSizeRemainder + bufferChunkByteCount) / bufferChunkByteCount;
      }
      else
      {
        bufferChunkCountMaximum = bufferSizeBytes / bufferChunkByteCount;
      }

      StringBuilder config = new StringBuilder(5000);
      config.AppendLine("[MPIPTVSource]");
      config.AppendLine(string.Format("MaxLogSize = {0}", maximumLogFileSize));
      config.AppendLine(string.Format("LogVerbosity = {0}", logVerbosity));
      config.AppendLine(string.Format("ConditionalAccessWaitingTimeout = {0}", caWaitTime));
      config.AppendLine(string.Format("MaxPlugins = {0}", maximumPluginCount));
      config.AppendLine(string.Format("IptvBufferCount = {0}", transferBufferChunkCount));
      config.AppendLine(string.Format("IptvBufferSize = {0}", bufferChunkByteCount));
      config.AppendLine(string.Format("DumpRawTS = {0}", dumpOutput));
      config.AppendLine(string.Format("AnalyzeDiscontinuity = {0}", analyseDiscontinuities));
      config.AppendLine(string.Format("DumpInputPackets = {0}", settings.DumpInput));
      config.AppendLine("[UDP]");
      config.AppendLine(string.Format("UdpReceiveDataTimeout = {0}", settings.ReceiveDataTimeLimit));
      config.AppendLine(string.Format("UdpInternalBufferMultiplier = {0}", bufferChunkCount));
      config.AppendLine(string.Format("UdpInternalBufferMaxMultiplier = {0}", bufferChunkCountMaximum));
      config.AppendLine(string.Format("UdpOpenConnectionMaximumAttempts = {0}", settings.OpenConnectionAttemptLimit));
      config.AppendLine("[RTP]");
      config.AppendLine(string.Format("RtpReceiveDataTimeout = {0}", settings.ReceiveDataTimeLimit));
      config.AppendLine(string.Format("RtpMaxFailedPackets = {0}", settings.RtpSwitchToUdpPacketCount));
      config.AppendLine(string.Format("RtpOpenConnectionMaximumAttempts = {0}", settings.OpenConnectionAttemptLimit));
      config.AppendLine("[HTTP]");
      config.AppendLine(string.Format("HttpReceiveDataTimeout = {0}", settings.ReceiveDataTimeLimit));
      config.AppendLine(string.Format("HttpInternalBufferMultiplier = {0}", bufferChunkCount));
      config.AppendLine(string.Format("HttpInternalBufferMaxMultiplier = {0}", bufferChunkCountMaximum));
      config.AppendLine(string.Format("HttpOpenConnectionMaximumAttempts = {0}", settings.OpenConnectionAttemptLimit));
      config.AppendLine("[File]");
      config.AppendLine(string.Format("FileReceiveDataTimeout = {0}", settings.ReceiveDataTimeLimit));
      config.AppendLine(string.Format("FileRepeatLimit = {0}", settings.FileRepeatCount));
      config.AppendLine(string.Format("FileInternalBufferMultiplier = {0}", bufferChunkCount));
      config.AppendLine(string.Format("FileOpenConnectionMaximumAttempts = {0}", settings.OpenConnectionAttemptLimit));
      config.AppendLine("[RTSP]");
      config.AppendLine(string.Format("RtspReceiveDataTimeout = {0}", settings.ReceiveDataTimeLimit));
      config.AppendLine(string.Format("RtspRtpClientPortRangeStart = {0}", portMinimum + (portMinimum % 2)));
      config.AppendLine(string.Format("RtspRtpClientPortRangeEnd = {0}", rtpPortMaximum));
      config.AppendLine(string.Format("RtspUdpSinkMaxPayloadSize = {0}", rtspUdpMaximumPayloadByteCount));
      config.AppendLine(string.Format("RtspUdpPortRangeStart = {0}", rtpPortMaximum + 1));
      config.AppendLine(string.Format("RtspUdpPortRangeEnd = {0}", portMaximum));
      config.AppendLine(string.Format("RtspOpenConnectionTimeout = {0}", settings.RtspOpenConnectionTimeLimit));
      config.AppendLine(string.Format("RtspOpenConnectionMaximumAttempts = {0}", settings.OpenConnectionAttemptLimit));
      config.AppendLine(string.Format("RtspSendCommandOptions = {0}", settings.RtspSendCommandOptions ? 1 : 0));
      config.AppendLine(string.Format("RtspSendCommandDescribe = {0}", settings.RtspSendCommandDescribe ? 1 : 0));
      config.AppendLine(string.Format("RtspKeepAliveWithOptions = {0}", settings.RtspKeepAliveWithOptions ? 1 : 0));

      this.LogDebug("  shared/global...");
      this.LogDebug("    logging...");
      this.LogDebug("      verbosity               = {0}", logVerbosity);
      this.LogDebug("      maximum file size       = {0} bytes", maximumLogFileSize);
      this.LogDebug("    CA wait time              = {0} ms", caWaitTime);
      this.LogDebug("    maximum plugin count      = {0}", maximumPluginCount);
      this.LogDebug("    buffer chunk size         = {0} bytes", bufferChunkByteCount);
      this.LogDebug("    transfer chunk count      = {0}", transferBufferChunkCount);
      this.LogDebug("    check discontinuities     = {0}", analyseDiscontinuities);
      this.LogDebug("    dump output               = {0}", dumpOutput);
      this.LogDebug("    RTP port range            = {0} - {1}", portMinimum + (portMinimum % 2), rtpPortMaximum);
      this.LogDebug("    UDP port range            = {0} - {1}", rtpPortMaximum + 1, portMaximum);
      this.LogDebug("    RTSP UDP max payload      = {0} bytes", rtspUdpMaximumPayloadByteCount);
      this.LogDebug("  tuner-specific...");
      this.LogDebug("    receive data time limit   = {0} ms", settings.ReceiveDataTimeLimit);
      this.LogDebug("    buffer chunk count        = {0} ({1} kB)", bufferChunkCount, settings.BufferSize);
      this.LogDebug("    buffer chunk count max    = {0} ({1} kB)", bufferChunkCountMaximum, settings.BufferSizeMaximum);
      this.LogDebug("    connect attempt limit     = {0}", settings.OpenConnectionAttemptLimit);
      this.LogDebug("    dump input                = {0}", settings.DumpInput);
      this.LogDebug("    RTSP...");
      this.LogDebug("      open conn. time limit   = {0} ms", settings.RtspOpenConnectionTimeLimit);
      this.LogDebug("      send OPTIONS command    = {0}", settings.RtspSendCommandOptions);
      this.LogDebug("      send DESCRIBE command   = {0}", settings.RtspSendCommandDescribe);
      this.LogDebug("      keep-alive with OPTIONS = {0}", settings.RtspKeepAliveWithOptions);
      this.LogDebug("    network interface         = {0}", settings.NetworkInterface);
      this.LogDebug("    file repeat count         = {0}", settings.FileRepeatCount);
      this.LogDebug("    RTP to UDP packet count   = {0}", settings.RtpSwitchToUdpPacketCount);

      lock (_configLock)
      {
        _config = config;
      }
    }

    /// <summary>
    /// Create sensible default configuration.
    /// </summary>
    private StreamTunerSettings CreateDefaultConfiguration(bool save)
    {
      this.LogDebug("DirectShow stream TVE: first detection, create default configuration");

      StreamTunerSettings settings = new StreamTunerSettings();
      settings.IdStreamTunerSettings = TunerId;
      settings.BufferSize = 256;                    // kB
      settings.BufferSizeMaximum = 32768;           // kB
      settings.DumpInput = false;
      settings.FileRepeatCount = 0;
      settings.NetworkInterface = string.Empty;
      settings.OpenConnectionAttemptLimit = 3;
      settings.ReceiveDataTimeLimit = 2000;         // ms
      settings.RtpSwitchToUdpPacketCount = 5;
      settings.RtspOpenConnectionTimeLimit = 1000;  // ms
      settings.RtspSendCommandDescribe = true;
      settings.RtspSendCommandOptions = true;
      settings.RtspKeepAliveWithOptions = false;
      if (save)
      {
        return StreamTunerSettingsManagement.SaveStreamTunerSettings(settings);
      }
      return settings;
    }

    #endregion

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      if (!string.IsNullOrEmpty(_httpRtpUdpnetworkInterface))
      {
        ChannelStream streamChannel = channel as ChannelStream;
        if (
          streamChannel != null &&
          streamChannel.Url != null &&
          (
            streamChannel.Url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ||
            streamChannel.Url.StartsWith("rtp://", StringComparison.InvariantCultureIgnoreCase) ||
            streamChannel.Url.StartsWith("udp://", StringComparison.InvariantCultureIgnoreCase)
          )
        )
        {
          streamChannel.Url = string.Format("c:|interface={0}|url={1}", _httpRtpUdpnetworkInterface, streamChannel.Url);
        }
      }
      base.PerformTuning(channel);
    }

    #endregion
  }
}