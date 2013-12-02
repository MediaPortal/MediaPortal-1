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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream
{
  /// <summary>
  /// The MediaPortal IPTV/DVB-IP/URL stream source class.
  /// </summary>
  [ComImport, Guid("d3dd4c59-d3a7-4b82-9727-7b9203eb67c0")]
  public class MediaPortalStreamSource
  {
  }

  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which supports the MediaPortal
  /// stream source filter.
  /// </summary>
  public class TunerStream : TunerDirectShowBase
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
    /// The class ID (CLSID) for the source filter main class.
    /// </summary>
    protected Guid _sourceFilterClsid = Guid.Empty;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStream"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="sequenceNumber">A sequence number or index for this instance.</param>
    public TunerStream(DsDevice device, int sequenceNumber)
      : base(device)
    {
      _tunerType = CardType.DvbIP;
      _defaultUrl = "udp://@0.0.0.0:1234";
      _sourceFilterClsid = typeof(MediaPortalStreamSource).GUID;

      _sequenceNumber = sequenceNumber;
      _name += " " + (_sequenceNumber + 1);

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
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("DirectShow stream: perform loading");
      InitialiseGraph();

      // Start with the source filter.
      if (_sourceFilterClsid == typeof(MediaPortalStreamSource).GUID)
      {
        _filterStreamSource = FilterGraphTools.AddFilterFromFile(_graph, "MPIPTVSource.ax", _sourceFilterClsid, "MediaPortal Stream Source");
      }
      else
      {
        _filterStreamSource = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, _sourceFilterClsid, "Stream Source");
      }

      // Check for and load plugins, adding any additional device filters to the graph.
      IBaseFilter lastFilter = _filterStreamSource;
      LoadPlugins(_filterStreamSource, _graph, ref lastFilter);

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    protected override void PerformUnloading()
    {
      this.LogDebug("DirectShow stream: perform unloading");
      if (_graph != null && _filterStreamSource != null)
      {
        _graph.RemoveFilter(_filterStreamSource);
      }
      Release.ComObject("DirectShow stream source filter", ref _filterStreamSource);
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
      DVBIPChannel streamChannel = channel as DVBIPChannel;
      if (streamChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }
      int hr = (_filterStreamSource as IFileSourceFilter).Load(streamChannel.Url, _sourceMediaType);
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
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        return new ScannerStream(this, _filterTsWriter as ITsChannelScan);
      }
    }

    #endregion

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      _isSignalPresent = true;
      _isSignalLocked = true;
      _signalLevel = 100;
      _signalQuality = 100;
    }

    /// <summary>
    /// return the DevicePath
    /// </summary>
    public override string DevicePath
    {
      get
      {
        return base.DevicePath + " (" + _sequenceNumber + ")";
      }
    }
  }
}