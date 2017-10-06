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

using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  /// <summary>
  /// A base implementation of <see cref="ITuner"/> for tuners that receive
  /// MPEG 2 transport streams and are supported using a DirectShow graph.
  /// </summary>
  internal abstract class TunerDirectShowMpeg2TsBase : TunerDirectShowBase
  {
    #region variables

    /// <summary>
    /// The MediaPortal TS writer/analyser filter.
    /// </summary>
    private IBaseFilter _filterTsWriter = null;

    /// <summary>
    /// The tuner's sub-channel manager.
    /// </summary>
    private ISubChannelManager _subChannelManager = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    private IChannelScannerInternal _channelScanner = null;

    /// <summary>
    /// The tuner's EPG grabbing interface.
    /// </summary>
    private IEpgGrabberInternal _epgGrabber = null;

    // TsWriter configuration.
    private int _tsWriterInputDumpMask = 0;
    private bool _tsWriterDisableCrcChecking = false;

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDirectShowMpeg2TsBase"/> class.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalId">The external identifier for the tuner.</param>
    /// <param name="tunerInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single tuner.</param>
    /// <param name="productInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single product.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the tuner.</param>
    protected TunerDirectShowMpeg2TsBase(string name, string externalId, string tunerInstanceId, string productInstanceId, BroadcastStandard supportedBroadcastStandards)
      : base(name, externalId, tunerInstanceId, productInstanceId, supportedBroadcastStandards)
    {
    }

    ~TunerDirectShowMpeg2TsBase()
    {
      Dispose(false);
    }

    #endregion

    #region properties

    /// <summary>
    /// Get the tuner's TS writer/analyser filter instance.
    /// </summary>
    protected IBaseFilter TsWriter
    {
      get
      {
        return _filterTsWriter;
      }
    }

    #endregion

    #region TS writer/analyser management

    /// <summary>
    /// Add and connect the TS writer/analyser filter into the DirectShow graph.
    /// ...[upstream filter]->[TS writer/analyser]
    /// </summary>
    /// <param name="upstreamFilter">The filter to connect to the TS writer/analyser filter.</param>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    protected void AddAndConnectTsWriterIntoGraph(IBaseFilter upstreamFilter, StreamFormat streamFormat)
    {
      this.LogDebug("DirectShow MPEG 2 TS base: add TS writer/analyser filter, stream format = [{0}]", streamFormat);

      _filterTsWriter = ComHelper.LoadComObjectFromFile("TsWriter.ax", typeof(MediaPortalTsWriter).GUID, typeof(IBaseFilter).GUID, true) as IBaseFilter;
      FilterGraphTools.AddAndConnectFilterIntoGraph(Graph, _filterTsWriter, "MediaPortal TS Analyser", upstreamFilter);
      ApplyTsWriterConfig();

      // If the stream format has not been set then set it based on location
      // and supported broadcast standards.
      if (streamFormat == StreamFormat.Default)
      {
        string countryName = System.Globalization.RegionInfo.CurrentRegion.EnglishName;
        if (countryName != null && (countryName.Equals("Canada") || countryName.Equals("United States")))
        {
          streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Atsc | StreamFormat.Scte;
        }
        else
        {
          streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Dvb;
          if ((SupportedBroadcastStandards & BroadcastStandard.MaskSatellite) != 0)
          {
            streamFormat |= StreamFormat.Freesat;
          }
        }
      }

      // Instanciate a sub-channel manager, channel scanner, and EPG grabber
      // [optional] based on the specified stream format.
      IGrabberSiMpeg mpegSiGrabber = null;
      if (streamFormat.HasFlag(StreamFormat.Mpeg2Ts))
      {
        mpegSiGrabber = _filterTsWriter as IGrabberSiMpeg;
      }
      if (streamFormat.HasFlag(StreamFormat.Analog))
      {
        _subChannelManager = new SubChannelManagerAnalog(_filterTsWriter as ITsWriter);
        _channelScanner = new ChannelScannerAnalog(this, mpegSiGrabber, _filterTsWriter as IGrabberSiDvb);
        // RDS grabbing and teletext EPG scraping currently not supported, so
        // no EPG grabber interface is available.
        return;
      }
      if (streamFormat.HasFlag(StreamFormat.Dvb))
      {
        _subChannelManager = new SubChannelManagerDvb(_filterTsWriter as ITsWriter);

        IGrabberSiFreesat freesatSiGrabber = null;
        if (streamFormat.HasFlag(StreamFormat.Freesat))
        {
          freesatSiGrabber = new GrabberSiFreesatWrapper(_filterTsWriter as IGrabberSiFreesat);
        }
        _channelScanner = new ChannelScannerDvb(this, mpegSiGrabber, _filterTsWriter as IGrabberSiDvb, freesatSiGrabber);

        _epgGrabber = new EpgGrabberDvb(new EpgGrabberController(SubChannelManager), _filterTsWriter as IGrabberEpgDvb, _filterTsWriter as IGrabberEpgMhw, _filterTsWriter as IGrabberEpgOpenTv);
        return;
      }

      _subChannelManager = new SubChannelManagerMpeg2Ts(_filterTsWriter as ITsWriter);
      if (!streamFormat.HasFlag(StreamFormat.Atsc) && !streamFormat.HasFlag(StreamFormat.Scte))
      {
        _channelScanner = new ChannelScannerDvb(this, mpegSiGrabber, null, null);
        return;
      }

      IGrabberSiAtsc grabberSiAtsc = null;
      IGrabberSiScte grabberSiScte = null;
      IGrabberEpgAtsc grabberEpgAtsc = null;
      IGrabberEpgScte grabberEpgScte = null;
      if (streamFormat.HasFlag(StreamFormat.Atsc))
      {
        grabberSiAtsc = _filterTsWriter as IGrabberSiAtsc;
        grabberEpgAtsc = _filterTsWriter as IGrabberEpgAtsc;
      }
      if (streamFormat.HasFlag(StreamFormat.Scte))
      {
        grabberSiScte = new GrabberSiScteWrapper(_filterTsWriter as IGrabberSiScte);
        grabberEpgScte = new GrabberEpgScteWrapper(_filterTsWriter as IGrabberEpgScte);
      }
      _channelScanner = new ChannelScannerAtsc(this, mpegSiGrabber, grabberSiAtsc, grabberSiScte);
      _epgGrabber = new EpgGrabberAtsc(new EpgGrabberController(SubChannelManager), grabberEpgAtsc, grabberEpgScte);
    }

    private void ApplyTsWriterConfig()
    {
      ITsWriter tsWriter = _filterTsWriter as ITsWriter;
      if (tsWriter != null)
      {
        tsWriter.DumpInput((_tsWriterInputDumpMask & 1) != 0, (_tsWriterInputDumpMask & 2) != 0);
        tsWriter.CheckSectionCrcs(!_tsWriterDisableCrcChecking);
      }
    }

    /// <summary>
    /// Remove and release the TS writer/analyser filter from the DirectShow graph.
    /// </summary>
    protected void RemoveTsWriterFromGraph()
    {
      this.LogDebug("DirectShow MPEG 2 TS base: remove TS writer/analyser filter");

      _subChannelManager = null;
      _channelScanner = null;
      _epgGrabber = null;
      if (Graph != null)
      {
        Graph.RemoveFilter(_filterTsWriter);
      }
      Release.ComObject("TS writer/analyser filter", ref _filterTsWriter);
    }

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(Tuner configuration)
    {
      this.LogDebug("DirectShow MPEG 2 base: reload configuration");

      if (configuration == null)
      {
        _tsWriterInputDumpMask = 0;
        _tsWriterDisableCrcChecking = false;
      }
      else
      {
        _tsWriterInputDumpMask = configuration.TsWriterInputDumpMask;
        _tsWriterDisableCrcChecking = configuration.DisableTsWriterCrcChecking;
      }
      this.LogDebug("  TsWriter input dump mask = 0x{0:x}", _tsWriterInputDumpMask);
      this.LogDebug("  TsWriter CRC check?      = {0}", !_tsWriterDisableCrcChecking);

      ApplyTsWriterConfig();
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's sub-channel manager.
    /// </summary>
    public override ISubChannelManager SubChannelManager
    {
      get
      {
        return _subChannelManager;
      }
    }

    /// <summary>
    /// Get the tuner's channel linkage scanning interface.
    /// </summary>
    public override IChannelLinkageScanner InternalChannelLinkageScanningInterface
    {
      get
      {
        return null;  // not required => no longer supported by TsWriter
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override IChannelScannerInternal InternalChannelScanningInterface
    {
      get
      {
        return _channelScanner;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public override IEpgGrabberInternal InternalEpgGrabberInterface
    {
      get
      {
        return _epgGrabber;
      }
    }

    #endregion

    #endregion
  }
}