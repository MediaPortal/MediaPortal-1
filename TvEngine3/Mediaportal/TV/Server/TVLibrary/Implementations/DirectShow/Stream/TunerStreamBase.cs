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
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaType = DirectShowLib.MediaType;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream
{
  /// <summary>
  /// A base implementation of <see cref="ITuner"/> for DirectShow stream tuners.
  /// </summary>
  internal abstract class TunerStreamBase : TunerDirectShowMpeg2TsBase
  {
    #region variables

    /// <summary>
    /// The source filter.
    /// </summary>
    private IBaseFilter _filterStreamSource = null;

    /// <summary>
    /// A media type describing the source stream format. 
    /// </summary>
    private AMMediaType _sourceMediaType = null;

    /// <summary>
    /// The tuner's current state.
    /// </summary>
    private TunerState _state = TunerState.NotLoaded;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStreamBase"/> class.
    /// </summary>
    /// <param name="name">A short name or description for the tuner.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerStreamBase(string name, int sequenceNumber)
      : this(name + " " + sequenceNumber, name + " " + sequenceNumber)
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerStreamBase"/> class.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalId">The external identifier for the tuner.</param>
    protected TunerStreamBase(string name, string externalId)
      : base(name, externalId, null, null, BroadcastStandard.DvbIp)
    {
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

    #endregion

    protected abstract IBaseFilter AddSourceFilter();

    #region ITunerInternal members

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      this.LogDebug("DirectShow stream base: perform loading");
      InitialiseGraph();

      // Start with the source filter.
      _filterStreamSource = AddSourceFilter();

      // Check for and load extensions, adding any additional filters to the graph.
      IBaseFilter lastFilter = _filterStreamSource;
      IList<ITunerExtension> extensions = LoadExtensions(_filterStreamSource, ref lastFilter);

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter, streamFormat);
      CompleteGraph();
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformSetTunerState(TunerState state, bool isFinalising = false)
    {
      base.PerformSetTunerState(state, isFinalising);
      _state = state;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      this.LogDebug("DirectShow stream base: perform unloading");

      if (!isFinalising)
      {
        if (Graph != null)
        {
          Graph.RemoveFilter(_filterStreamSource);
        }
        Release.ComObject("DirectShow stream source filter", ref _filterStreamSource);
        Release.AmMediaType(ref _sourceMediaType);

        RemoveTsWriterFromGraph();
      }

      CleanUpGraph(isFinalising);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("DirectShow stream base: perform tuning");
      ChannelStream streamChannel = channel as ChannelStream;
      if (streamChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      bool restart = false;
      if (_state == TunerState.Started)
      {
        // We have to replace the source filter. This is a workaround for the
        // filter's inability to change URLs.
        this.LogDebug("DirectShow stream base: replacing source filter");
        restart = true;
        base.PerformSetTunerState(TunerState.Stopped);
        IPin pinOutput = DsFindPin.ByDirection(_filterStreamSource, PinDirection.Output, 0);
        try
        {
          IPin pinDownstream;
          TvExceptionDirectShowError.Throw(pinOutput.ConnectedTo(out pinDownstream), "Failed to get stream source filter downstream pin.");
          try
          {
            Graph.RemoveFilter(_filterStreamSource);
            _filterStreamSource = AddSourceFilter();
            Release.ComObject("DirectShow stream source output pin", ref pinOutput);
            pinOutput = DsFindPin.ByDirection(_filterStreamSource, PinDirection.Output, 0);
            TvExceptionDirectShowError.Throw(Graph.ConnectDirect(pinOutput, pinDownstream, null), "Failed to connect new stream source filter.");
          }
          finally
          {
            Release.ComObject("DirectShow stream source downstream pin", ref pinDownstream);
          }
        }
        finally
        {
          Release.ComObject("DirectShow stream source output pin", ref pinOutput);
        }
      }

      int hr = (_filterStreamSource as IFileSourceFilter).Load(streamChannel.Url, _sourceMediaType);
      TvExceptionDirectShowError.Throw(hr, "Failed to tune channel.");

      if (restart)
      {
        base.PerformSetTunerState(TunerState.Started);
      }
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      isLocked = true;
      isPresent = true;
      strength = 100;
      quality = 100;
    }

    #endregion

    #endregion
  }
}