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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs.DVBIP
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which supports the MediaPortal DVB-IP/IPTV source filter.
  /// </summary>
  public class TvCardDVBIP : DeviceDirectShow
  {
    #region variables

    /// <summary>
    /// The source filter.
    /// </summary>
    private IBaseFilter _filterStreamSource = null;

    /// <summary>
    /// The instance number of this device/tuner.
    /// </summary>
    private int _sequenceNumber = -1;

    /// <summary>
    /// A media type describing the source stream format. 
    /// </summary>
    private AMMediaType _sourceMediaType = null;

    /// <summary>
    /// The source filter's default URL.
    /// </summary>
    protected string _defaultUrl;

    /// <summary>
    /// The CLSID/UUID for the source filter.
    /// </summary>
    protected Guid _sourceFilterGuid = Guid.Empty;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TvCardDVBIP"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface for the instance to use.</param>
    /// <param name="device">The <see cref="DsDevice"/> instance that the instance will encapsulate.</param>
    /// <param name="sequenceNumber">A sequence number or index for this instance.</param>
    public TvCardDVBIP(IEpgEvents epgEvents, DsDevice device, int sequenceNumber)
      : base(device)
    {
      _tunerType = CardType.DvbIP;
      _defaultUrl = "udp://@0.0.0.0:1234";
      _sourceFilterGuid = new Guid(0xd3dd4c59, 0xd3a7, 0x4b82, 0x97, 0x27, 0x7b, 0x92, 0x03, 0xeb, 0x67, 0xc0);

      _sequenceNumber = sequenceNumber;
      if (_sequenceNumber > 0)
      {
        _name += "_" + _sequenceNumber;
      }

      _sourceMediaType = new AMMediaType();
      _sourceMediaType.majorType = MediaType.Stream;
      _sourceMediaType.subType = MediaSubType.Mpeg2Transport;
      _sourceMediaType.unkPtr = IntPtr.Zero;
      _sourceMediaType.sampleSize = 0;
      _sourceMediaType.temporalCompression = false;
      _sourceMediaType.fixedSizeSamples = true;
      _sourceMediaType.formatType = FormatType.None;
      _sourceMediaType.formatSize = 0;
      _sourceMediaType.formatPtr = IntPtr.Zero;
    }

    #region graph building

    /// <summary>
    /// Actually load the device.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("TvCardDvbIp: load device");
      InitialiseGraph();

      // Start with the source filter.
      _filterStreamSource = FilterGraphTools.AddFilterByClsid(_graph, _sourceFilterGuid, "MediaPortal Network Source Filter");

      // Check for and load plugins, adding any additional device filters to the graph.
      IBaseFilter lastFilter = _filterStreamSource;
      LoadPlugins(_filterStreamSource, ref lastFilter);

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();
    }

    /// <summary>
    /// Actually unload the device.
    /// </summary>
    protected override void PerformUnloading()
    {
      if (_graph != null && _filterStreamSource != null)
      {
        _graph.RemoveFilter(_filterStreamSource);
      }
      Release.ComObject("DVB-IP stream source filter", ref _filterStreamSource);
      Release.AmMediaType(ref _sourceMediaType);

      CleanUpGraph();
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      DVBIPChannel dvbipChannel = channel as DVBIPChannel;
      if (dvbipChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }
      int hr = (_filterStreamSource as IFileSourceFilter).Load(dvbipChannel.Url, _sourceMediaType);
      HResult.ThrowException(hr, "Failed to tune channel.");
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBIPChannel;
    }

    /// <summary>
    /// Get the device's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        return new DVBIPScanning(this, _filterTsWriter as ITsChannelScan);
      }
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="id">The identifier for the subchannel.</param>
    /// <returns>the new subchannel instance</returns>
    protected override ITvSubChannel CreateNewSubChannel(int id)
    {
      return new TvDvbChannel(id, this, _filterTsWriter, null);
    }

    #endregion

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    /// <param name="force"><c>True</c> to force the status to be updated (status information may be cached).</param>
    protected override void UpdateSignalStatus(bool force)
    {
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000)
        {
          return;
        }
      }
      if (_currentTuningDetail == null || _state != DeviceState.Started)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
      }
      else
      {
        _tunerLocked = true;
        _signalLevel = 100;
        _signalPresent = true;
        _signalQuality = 100;
      }
    }

    /// <summary>
    /// return the DevicePath
    /// </summary>
    public override string DevicePath
    {
      get
      {
        if (_sequenceNumber == 0)
        {
          return base.DevicePath;
        }
        return base.DevicePath + "(" + _sequenceNumber + ")";
      }
    }
  }
}