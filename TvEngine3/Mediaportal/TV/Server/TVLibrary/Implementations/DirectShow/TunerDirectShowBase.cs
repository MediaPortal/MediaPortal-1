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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for tuners that expose
  /// DirectShow filters.
  /// </summary>
  public abstract class TunerDirectShowBase : TvCardBase
  {
    #region variables

    /// <summary>
    /// The DirectShow filter graph.
    /// </summary>
    protected IFilterGraph2 _graph = null;

    /// <summary>
    /// The capture graph builder for the graph.
    /// </summary>
    protected ICaptureGraphBuilder2 _captureGraphBuilder = null;

    /// <summary>
    /// The running object table entry for the graph.
    /// </summary>
    private DsROTEntry _runningObjectTableEntry = null;

    /// <summary>
    /// The main tuner component device.
    /// </summary>
    protected DsDevice _deviceMain = null;

    /// <summary>
    /// The main tuner component filter.
    /// </summary>
    protected IBaseFilter _filterMain = null;

    /// <summary>
    /// The MediaPortal TS writer/analyser filter.
    /// </summary>
    protected IBaseFilter _filterTsWriter = null;

    #endregion

    #region constructors

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDirectShowBase"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    protected TunerDirectShowBase(DsDevice device)
      : base(device.Name, device.DevicePath)
    {
      _deviceMain = device;
      if (_deviceMain != null)
      {
        _productInstanceId = device.ProductInstanceIdentifier;
        int tunerInstanceId = device.TunerInstanceIdentifier;
        if (tunerInstanceId >= 0)
        {
          _tunerInstanceId = tunerInstanceId.ToString();
        }
      }
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDirectShowBase"/> class.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalId">The external identifier for the tuner.</param>
    protected TunerDirectShowBase(string name, string externalId)
      : base(name, externalId)
    {
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
      return new Mpeg2SubChannel(id, this, _filterTsWriter as ITsFilter);
    }

    #endregion

    #region graph building & control

    /// <summary>
    /// Create and initialise the DirectShow graph.
    /// </summary>
    protected void InitialiseGraph()
    {
      this.LogDebug("DirectShow base: initialise graph");
      _graph = (IFilterGraph2)new FilterGraph();
      _captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
      int hr = _captureGraphBuilder.SetFiltergraph(_graph);
      HResult.ThrowException(hr, "Failed to set the capture graph builder's graph.");
      _runningObjectTableEntry = new DsROTEntry(_graph);
    }

    /// <summary>
    /// Complete the DirectShow graph.
    /// </summary>
    protected void CompleteGraph()
    {
      FilterGraphTools.SaveGraphFile(_graph, Name + " - " + _tunerType + " Graph.grf");
    }

    /// <summary>
    /// Add the filter for the main tuner component to the DirectShow graph.
    /// </summary>
    protected void AddMainComponentFilterToGraph()
    {
      this.LogDebug("DirectShow base: add main component filter");
      if (!DevicesInUse.Instance.Add(_deviceMain))
      {
        throw new TvException("Main DirectShow tuner component is in use.");
      }
      try
      {
        _filterMain = FilterGraphTools.AddFilterFromDevice(_graph, _deviceMain);
      }
      catch (Exception ex)
      {
        DevicesInUse.Instance.Remove(_deviceMain);
        throw new TvException("Failed to add filter for DirectShow tuner main component to graph.", ex);
      }
    }

    /// <summary>
    /// Add and connect the TS writer/analyser filter into the DirectShow graph.
    /// ...[upstream filter]->[TS writer/analyser]
    /// </summary>
    /// <param name="upstreamFilter">The filter to connect to the TS writer/analyser filter.</param>
    protected void AddAndConnectTsWriterIntoGraph(IBaseFilter upstreamFilter)
    {
      this.LogDebug("DirectShow base: add TS writer/analyser filter");
      _filterTsWriter = ComHelper.LoadComObjectFromFile("TsWriter.ax", typeof(MediaPortalTsWriter).GUID, typeof(IBaseFilter).GUID, true) as IBaseFilter;
      FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterTsWriter, "MediaPortal TS Analyser", upstreamFilter, _captureGraphBuilder);
    }

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    protected override void SetTunerState(TunerState state)
    {
      this.LogDebug("DirectShow base: set tuner state, current state = {0}, requested state = {1}", _state, state);

      if (state == _state)
      {
        this.LogDebug("DirectShow base: tuner already in required state");
        return;
      }
      if (_graph == null)
      {
        throw new TvException("DirectShow graph is null, can't set tuner state.");
      }

      int hr = (int)HResult.Severity.Success;
      if (state == TunerState.Stopped)
      {
        hr = (_graph as IMediaControl).Stop();
      }
      else if (state == TunerState.Paused)
      {
        hr = (_graph as IMediaControl).Pause();
      }
      else if (state == TunerState.Started)
      {
        hr = (_graph as IMediaControl).Run();
      }
      else
      {
        hr = (int)HResult.Severity.Error;
      }
      HResult.ThrowException(hr, string.Format("Failed to change tuner state from {0} to {1}.", _state, state));
      _state = state;
    }

    /// <summary>
    /// Remove and release DirectShow main component and graph components.
    /// </summary>
    protected void CleanUpGraph()
    {
      this.LogDebug("DirectShow base: clean up graph");
      if (_runningObjectTableEntry != null)
      {
        _runningObjectTableEntry.Dispose();
        _runningObjectTableEntry = null;
      }
      if (_graph != null)
      {
        // First remove the filters that we inserted.
        _graph.RemoveFilter(_filterTsWriter);
        if (_filterMain != null)
        {
          _graph.RemoveFilter(_filterMain);
          Release.ComObject("DirectShow tuner main component filter", ref _filterMain);

          DevicesInUse.Instance.Remove(_deviceMain);
          // Do ***NOT*** dispose or release _deviceMain here. This would cause
          // reloading to fail. Deal with this in Dispose().
        }

        // Now check if there are any filters remaining in the graph.
        // Presence of such filters indicate there are badly behaved
        // classes or extensions present.
        IEnumFilters enumFilters = null;
        int hr = _graph.EnumFilters(out enumFilters);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("DirectShow base: failed to enumerate remaining filters in graph");
        }
        else
        {
          try
          {
            IBaseFilter[] filters = new IBaseFilter[1];
            int filterCount;
            while (enumFilters.Next(filters.Length, filters, out filterCount) == (int)HResult.Severity.Success && filterCount == 1)
            {
              IBaseFilter filter = filters[0];
              this.LogWarn("DirectShow base: removing and releasing orphaned filter {0}", FilterGraphTools.GetFilterName(filter));
              _graph.RemoveFilter(filter);
              Release.ComObjectAllRefs("DirectShow tuner orphaned filter", ref filter);
            }
          }
          finally
          {
            Release.ComObject("DirectShow tuner remaining filter enumerator", ref enumFilters);
          }
        }
        Release.ComObject("DirectShow tuner graph", ref _graph);
      }
      Release.ComObject("DirectShow tuner graph builder", ref _captureGraphBuilder);
      Release.ComObject("TS writer/analyser filter", ref _filterTsWriter);
    }

    #endregion

    #region tuning & scanning

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

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      base.Dispose();
      if (_deviceMain != null)
      {
        _deviceMain.Dispose();
        _deviceMain = null;
      }
    }

    #endregion
  }
}